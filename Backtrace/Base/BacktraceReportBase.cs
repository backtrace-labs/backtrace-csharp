using Backtrace.Common;
using Backtrace.Extensions;
using Backtrace.Model.JsonData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Backtrace.Base
{
    /// <summary>
    /// Capture application report
    /// </summary>
    public class BacktraceReportBase<T>
    {
        /// <summary>
        /// 16 bytes of randomness in human readable UUID format
        /// server will reject request if uuid is already found
        /// </summary>s
        public readonly Guid Uuid = Guid.NewGuid();

        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        public readonly long Timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        /// <summary>
        /// Get information aboout report type. If value is true the BacktraceReport has an error information
        /// </summary>
        public bool ExceptionTypeReport = false;

        /// <summary>
        /// Get a report classification 
        /// </summary>
        public string Classifier
        {
            get
            {
                if (ExceptionTypeReport)
                {
                    return Exception.GetType().Name;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Get an report attributes
        /// </summary>
        public Dictionary<string, T> Attributes { get; private set; }

        /// <summary>
        /// Get a custom client message
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// Get a report exception
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Get all paths to attachments
        /// </summary>
        public List<string> AttachmentPaths { get; set; }

        /// <summary>
        /// Get or set minidump attachment path
        /// </summary>
        internal string MinidumpFile { get; set; }

        /// <summary>
        /// Get an assembly where report was created (or should be created)
        /// </summary>
        internal Assembly CallingAssembly { get; set; }

        /// <summary>
        /// Current report exception stack
        /// </summary>
        public List<ExceptionStack> ExceptionStack { get; set; } = new List<ExceptionStack>();

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

        private void SetStacktraceInformation(StackFrame[] stackFrames, bool includeCallingAssembly, int startingIndex = 0)
        {
            var executedAssemblyName = Assembly.GetExecutingAssembly().FullName;
            //if includeCallingAssembly is true, we dont need to found calling assembly in current stacktrace
            bool callingAssemblyFound = !includeCallingAssembly;

            foreach (var stackFrame in stackFrames)
            {
                if (stackFrame == null)
                {
                    //received invalid or unvailable stackframe
                    continue;
                }
                Assembly assembly = stackFrame?.GetMethod()?.DeclaringType?.Assembly;
                if (assembly == null)
                {
#if DEBUG
                    Trace.WriteLine(stackFrame);
#endif
                    continue;
                }
                var assemblyName = assembly.FullName;
                if (executedAssemblyName.Equals(assemblyName))
                {
                    // remove all system and microsoft stack frames 
                    //if we add any stackframe to list this is mistake because we receive 
                    //system or microsoft dll (for example async invoke)
                    ExceptionStack.Clear();
                    startingIndex = 0;
                    continue;
                }
                ExceptionStack.Insert(startingIndex, Model.JsonData.ExceptionStack.Convert(stackFrame, assembly.GetName().Name, true));
                startingIndex++;
                if (!callingAssemblyFound && !(SystemHelper.SystemAssembly(assembly)))
                {
                    callingAssemblyFound = true;
                    CallingAssembly = assembly;
                }
            }
        }
        /// <summary>
        /// Set Calling Assembly and Exception Stack property. 
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
            //Add to current stack trace, stackframes from current exception
            //stacktrace from current thread and from current excetpion are diffrent
            var exceptionStackTrace = new StackTrace(Exception, true);
            var exceptionStackFrames = exceptionStackTrace.GetFrames();
            if (exceptionStackFrames[0] != null
                && ExceptionStack[0].ILOffset != exceptionStackFrames[0].GetILOffset()
                && ExceptionStack[0].FunctionName != exceptionStackFrames[0].GetMethod()?.Name)
            {
                SetStacktraceInformation(exceptionStackFrames, false);
            }




            //var executedAssemblyName = Assembly.GetExecutingAssembly().FullName;
            //bool callingAssemblyFound = false;

            //foreach (var stackFrame in stackFrames)
            //{
            //    if (stackFrame == null)
            //    {
            //        continue;
            //    }

            //    Assembly assembly = stackFrame.GetMethod()?.DeclaringType?.Assembly;
            //    if (assembly == null)
            //    {
            //        continue;
            //    }
            //    var assemblyName = assembly?.FullName;
            //    if (executedAssemblyName.Equals(assemblyName))
            //    {
            //        // remove all system and microsoft stack frames 
            //        ExceptionStack.Clear();
            //        continue;
            //    }
            //    ExceptionStack.Add(Model.JsonData.ExceptionStack.Convert(stackFrame, assembly.GetName().Name, true));

            //    if (!callingAssemblyFound && !(SystemHelper.SystemAssembly(assembly)))
            //    {
            //        callingAssemblyFound = true;
            //        CallingAssembly = assembly;
            //    }
            //}

            //List<StackFrame> exceptionStackFrames = null;
            //if (Exception != null)
            //{
            //    var exceptionStackTrace = new StackTrace(Exception, true);
            //    exceptionStackFrames = exceptionStackTrace.GetFrames().ToList();
            //    if (exceptionStackFrames[0] == null
            //        || (ExceptionStack[0].ILOffset == exceptionStackFrames[0].GetILOffset()
            //        && ExceptionStack[0].FunctionName == exceptionStackFrames[0].GetMethod()?.Name))
            //    {
            //        return;
            //    }
            //}
            //foreach (var stackFrame in exceptionStackFrames)
            //{
            //    if (stackFrame == null)
            //    {
            //        continue;
            //    }

            //    Assembly assembly = stackFrame.GetMethod()?.DeclaringType?.Assembly;
            //    if (assembly == null)
            //    {
            //        continue;
            //    }
            //    var assemblyName = assembly?.FullName;
            //    if (executedAssemblyName.Equals(assemblyName))
            //    {
            //        continue;
            //    }
            //    ExceptionStack.Insert(0, Model.JsonData.ExceptionStack.Convert(stackFrame, assembly.GetName().Name, true));

            //    if (!callingAssemblyFound && !(SystemHelper.SystemAssembly(assembly)))
            //    {
            //        callingAssemblyFound = true;
            //        CallingAssembly = assembly;
            //    }
            //}
        }

    }
}
