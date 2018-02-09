using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Parse exception information to Backtrace API format
    /// </summary>
    internal class ExceptionStack
    {
        /// <summary>
        /// Function where exception occurs
        /// </summary>
        [JsonProperty(PropertyName = "funcName")]
        public string FunctionName { get; set; }

        /// <summary>
        /// Line number in source code where exception occurs
        /// </summary>
        [JsonProperty(PropertyName = "line")]
        public int Line { get; set; }

        /// <summary>
        /// Column number in source code where exception occurs
        /// </summary>
        [JsonProperty(PropertyName = "column")]
        public int Column { get; set; }

        /// <summary>
        /// Full path to source code where exception occurs
        /// </summary>
        public string SourceCodeFullPath { get; set; }

        /// <summary>
        /// Source code file name where exception occurs
        /// </summary>
        [JsonProperty(PropertyName = "sourceCode")]
        public string SourceCode { get; set; }

        /// <summary>
        /// Library name where exception occurs
        /// </summary>
        [JsonProperty(PropertyName = "library")]
        public string Library { get; set; }

        /// <summary>
        /// Check if this frame is known to have initiated an error
        /// </summary>
        //[JsonProperty(PropertyName = "callstack_state")]
        //public bool CallstackState { get; set; }

        /// <summary>
        /// Set exception information to thread
        /// </summary>
        /// <param name="exception"></param>
        public ExceptionStack(Exception exception)
        {
            if (exception == null)
            {
                return;
            }
            //get a current stack frame from an exception
            var stackTrace = new System.Diagnostics.StackTrace(exception, true);
            var stackFrames = stackTrace.GetFrames();
            //handle custom made exceptions 
            if (stackFrames == null || stackFrames.Length == 0)
            {
                return;
            }
            StackFrames = stackFrames
                .Select(n => n.ToString()).ToList();

            var frame = stackFrames[stackTrace.FrameCount - 1];
            if (frame == null)
            {
                return;
            }
            FunctionName = frame.GetMethod().Name;
            Line = frame.GetFileLineNumber();
            Column = frame.GetFileColumnNumber();
            SourceCodeFullPath = frame.GetFileName();
            SourceCode = Path.GetFileName(SourceCodeFullPath);

            Library = exception.Source;
            //CallstackState = exception.InnerException == null;
        }

        /// <summary>
        /// Get all stack frames from current exception
        /// </summary>
        [JsonIgnore]
        internal List<string> StackFrames = new List<string>();
    }
}
