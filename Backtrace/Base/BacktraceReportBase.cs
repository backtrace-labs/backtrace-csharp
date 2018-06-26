using Backtrace.Common;
using Backtrace.Extensions;
using Backtrace.Model;
using Backtrace.Model.JsonData;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if !NET35
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
#endif

namespace Backtrace.Base
{
    /// <summary>
    /// Capture application report
    /// </summary>
    [Serializable]
    public class BacktraceReportBase
    {
        /// <summary>
        /// 16 bytes of randomness in human readable UUID format
        /// server will reject request if uuid is already found
        /// </summary>s
        [JsonProperty(PropertyName = "uuid")]
        public Guid Uuid { get; private set; } = Guid.NewGuid();

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
        public Dictionary<string, object> Attributes { get; private set; }

        /// <summary>
        /// Get a custom client message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; private set; }

        /// <summary>
        /// Get a report exception
        /// </summary>
        [JsonIgnore]
        public Exception Exception { get; private set; }

        /// <summary>
        /// Get all paths to attachments
        /// </summary>
        [JsonProperty(PropertyName = "attachmentPaths")]
        public List<string> AttachmentPaths { get; set; }

        /// <summary>
        /// Get or set minidump attachment path
        /// </summary>
        [JsonProperty(PropertyName = "minidumpFile")]
        internal string MinidumpFile { get; private set; }

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
        [JsonConstructor]
        public BacktraceReportBase(
            string message,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        {
            Message = message;
            Attributes = attributes ?? new Dictionary<string, object>();
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
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        {
            Attributes = attributes ?? new Dictionary<string, object>();
            AttachmentPaths = attachmentPaths ?? new List<string>();
            Exception = exception;
            ExceptionTypeReport = exception != null;
            Classifier = ExceptionTypeReport ? exception.GetType().Name : string.Empty;
            CallingAssembly = exception.GetExceptionSourceAssembly();
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

        internal BacktraceData ToBacktraceData(Dictionary<string, object> clientAttributes)
        {
            return new BacktraceData(this, clientAttributes);
        }

        /// <summary>
        /// Concat two attributes dictionary 
        /// </summary>
        /// <param name="report">Current report</param>
        /// <param name="attributes">Attributes to concatenate</param>
        /// <returns></returns>
        internal static Dictionary<string, object> ConcatAttributes(
            BacktraceReportBase report, Dictionary<string, object> attributes)
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
        internal void SetCallingAppInformation()
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

            //Library didn't found Calling assembly
            //The reason for this behaviour is because we throw exception from TaskScheduler
            //or other method that don't generate valid stack trace
            if (CallingAssembly == null)
            {
                CallingAssembly = Assembly.GetExecutingAssembly();
            }
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
                if (executedAssemblyName.Equals(assemblyName))
                {
                    // remove all system and microsoft stack frames 
                    //if we add any stackframe to list this is mistake because we receive 
                    //system or microsoft dll (for example async invoke)
                    DiagnosticStack.Clear();
                    startingIndex = 0;
                    continue;
                }
                if (!callingAssemblyFound && ((!(SystemHelper.SystemAssembly(assembly)))
                    || (CallingAssembly != null && assembly?.FullName == CallingAssembly.FullName)))
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
        internal BacktraceReportBase CreateInnerReport()
        {
            // there is no additional exception inside current exception
            // or exception does not exists
            if (!ExceptionTypeReport || Exception.InnerException == null)
            {
                return null;
            }
            var copy = (BacktraceReportBase)this.MemberwiseClone();
            copy.Exception = this.Exception.InnerException;
            copy.SetCallingAppInformation();
            copy.Classifier = copy.Exception.GetType().Name;
            return copy;
        }
    }
}
