using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Parse exception information to Backtrace API format
    /// </summary>
    internal class ExceptionStack
    {
        /// <summary>
        /// Function where exception occurs
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// Line number in source code where exception occurs
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// Column number in source code where exception occurs
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// Full path to source code where exception occurs
        /// </summary>
        public string SourceCodeFullPath { get; set; }

        /// <summary>
        /// Source code file name where exception occurs
        /// </summary>
        public string SourceCode { get; set; }

        /// <summary>
        /// Library name where exception occurs
        /// </summary>
        public string Library { get; set; }

        /// <summary>
        /// Check if there is an inner exception
        /// </summary>
        public bool CallstackState { get; set; }

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
            StackFrames = stackTrace.GetFrames();
            var frame = StackFrames[stackTrace.FrameCount - 1];
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
            CallstackState = exception.InnerException == null;
        }

        /// <summary>
        /// Get all stack frames from current exception
        /// </summary>
        public System.Diagnostics.StackFrame[] StackFrames;
    }
}
