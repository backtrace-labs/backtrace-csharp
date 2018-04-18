using Backtrace.Common;
using Backtrace.Extensions;
using Backtrace.Model;
using Backtrace.Model.JsonData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
#if !NET35
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
#endif
using System.Text;

namespace Backtrace.Base
{
    /// <summary>
    /// Capture application report
    /// </summary>
    [Serializable]
    public class BacktraceReportBase<T>
    {
        /// <summary>
        /// 16 bytes of randomness in human readable UUID format
        /// server will reject request if uuid is already found
        /// </summary>s
        [JsonProperty(PropertyName = "uuid")]
        public Guid Uuid { get; private set; } = new Guid();

        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; private set; } = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        /// <summary>
        /// Get information aboout report type. If value is true the BacktraceReport has an error information
        /// </summary>
        [JsonProperty(PropertyName = "reportType")]
        public bool ExceptionTypeReport { get; private set; } = false;

        /// <summary>
        /// Get a report classification 
        /// </summary>
        [JsonProperty(PropertyName = "classifier")]
        public string Classifier { get; set; } = string.Empty;

        /// <summary>
        /// Get an report attributes
        /// </summary>
        [JsonProperty(PropertyName = "attributes")]
        public Dictionary<string, T> Attributes { get; private set; }

        /// <summary>
        /// Get a custom client message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public readonly string Message;

        /// <summary>
        /// Get a report exception
        /// </summary>
        [JsonProperty(PropertyName = "exception")]
        public Exception Exception { get; set; }

        /// <summary>
        /// Get all paths to attachments
        /// </summary>
        [JsonProperty(PropertyName = "attachmentPaths")]
        public List<string> AttachmentPaths { get; set; }

        /// <summary>
        /// Get or set minidump attachment path
        /// </summary>
        [JsonProperty(PropertyName = "minidumpFile")]
        internal string MinidumpFile { get; set; }

        /// <summary>
        /// Get an assembly where report was created (or should be created)
        /// </summary>
        internal Assembly CallingAssembly { get; private set; }

        /// <summary>
        /// Current report exception stack
        /// </summary>
        [JsonProperty(PropertyName = "diagnosticStack")]
        public List<DiagnosticStack> DiagnosticStack { get; private set; } = new List<DiagnosticStack>();

        /// <summary>
        /// Create new instance of Backtrace report to sending a report with custom client message
        /// </summary>
        /// <param name="message">Custom client message</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            string message,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
        { 
            Message = message;
            Attributes = attributes ?? new Dictionary<string, T>();
            AttachmentPaths = attachmentPaths ?? new List<string>();
            SetCallingAppInformation();
        }

        /// <summary>
        /// Create new instance of Backtrace report to sending a report with application exception
        /// </summary>
        /// <param name="exception">Current exception</param>
        /// <param name="callingAssembly">Calling assembly</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            Exception exception,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
        {
            Attributes = attributes ?? new Dictionary<string, T>();
            AttachmentPaths = attachmentPaths ?? new List<string>();
            Exception = exception;
            ExceptionTypeReport = Exception != null;
            Classifier = ExceptionTypeReport ? Exception.GetType().Name : string.Empty;
            SetCallingAppInformation();
        }

        /// <summary>
        /// Set a path to report minidump
        /// </summary>
        /// <param name="minidumpPath">Path to generated minidump file</param>
        internal void SetMinidumpPath(string minidumpPath)
        {
            if (string.IsNullOrEmpty(minidumpPath))
            {
                return;
            }
            MinidumpFile = minidumpPath;
            AttachmentPaths.Add(minidumpPath);
        }
        
        internal BacktraceData<T> ToBacktraceData(Dictionary<string, T> clientAttributes)
        {
            return new BacktraceData<T>(this, clientAttributes);
        }

        /// <summary>
        /// Concat two attributes dictionary 
        /// </summary>
        /// <param name="report">Current report</param>
        /// <param name="attributes">Attributes to concatenate</param>
        /// <returns></returns>
        internal static Dictionary<string, T> ConcatAttributes(
            BacktraceReportBase<T> report, Dictionary<string, T> attributes)
        {
            var reportAttributes = report.Attributes;
            if (attributes == null)
            {
                return reportAttributes;
            };
            return reportAttributes.Merge(attributes);
        }

        /// <summary>
        /// Set Calling Assembly and current thread stack trace property. 
        /// CallingAssembly and StackTrace are necessary to prepare diagnostic JSON in BacktraceData class
        /// </summary>
        private void SetCallingAppInformation()
        {
            // generate stacktrace with file info
            // if assembly have pbd files, diagnostic JSON will contain information about 
            // line number and column number
            var stackTrace = new StackTrace(true);
            var stackFrames = stackTrace.GetFrames();
            SetStacktraceInformation(stackFrames, true);

            if (Exception == null)
            {
                return;
            }
            // add stack trace from exception
            var head = DiagnosticStack.Any() ? DiagnosticStack[0] : null;
            var generatedStack = Exception.GetExceptionStackFrames(head);
            SetStacktraceInformation(generatedStack, false);
        }

        private void SetStacktraceInformation(StackFrame[] stackFrames, bool includeCallingAssembly, int startingIndex = 0)
        {
            // check if stack frames exists
            if (stackFrames == null)
            {
                return;
            }
            var executedAssemblyName = Assembly.GetExecutingAssembly().FullName;
            //if callingAssemblyFound is true, we dont need to found calling assembly in current stacktrace
            bool callingAssemblyFound = !includeCallingAssembly;

            foreach (var stackFrame in stackFrames)
            {
                var method = stackFrame.GetMethod();
                var declaringType = method?.DeclaringType;
                if (declaringType == null)
                {
                    //received invalid or unvailable stackframe
                    continue;
                }
                Assembly assembly = declaringType.Assembly;
                if (assembly == null)
                {
                    continue;
                }
                var assemblyName = assembly.FullName;
                if (executedAssemblyName.Equals(assemblyName) && CallingAssembly == null)
                {
                    // remove all system and microsoft stack frames 
                    //if we add any stackframe to list this is mistake because we receive 
                    //system or microsoft dll (for example async invoke)
                    DiagnosticStack.Clear();
                    startingIndex = 0;
                    continue;
                }

                if (!callingAssemblyFound && !(SystemHelper.SystemAssembly(assembly)))
                {
                    callingAssemblyFound = true;
                    CallingAssembly = assembly;
                }
#if !NET35
                //test if current stack frame is generated by async state machine
                var declaringTypeInfo = declaringType.GetTypeInfo();
                var stateMachineFrame = SystemHelper.StateMachineFrame(declaringTypeInfo);
                if (stateMachineFrame)
                {
                    continue;
                }
#endif
                if (!callingAssemblyFound)
                {
                    continue;
                }
                var diagnosticStack = Model.JsonData.DiagnosticStack.Convert(stackFrame, assembly.GetName().Name, true);
                DiagnosticStack.Insert(startingIndex, diagnosticStack);
                startingIndex++;
            }
        }

        /// <summary>
        /// create a copy of BacktraceReport for inner exception object inside exception
        /// </summary>
        /// <returns>BacktraceReport for InnerExceptionObject</returns>
        internal BacktraceReport CreateInnerReport()
        {
            var copy = (BacktraceReport)this.MemberwiseClone();
            copy.Exception = this.Exception.InnerException;
            return copy;
        }
    }
}
