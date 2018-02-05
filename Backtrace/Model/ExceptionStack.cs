using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    internal class ExceptionStack
    {
        public string FunctionName { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string SourceCode { get; set; }
        public string Library { get; set; }
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
            var stackTrace = new System.Diagnostics.StackTrace(exception, true);
            var frame = stackTrace?.GetFrame(stackTrace.FrameCount - 1);
            if (frame == null)
            {
                return;
            }
            FunctionName = frame.GetMethod().Name;
            Line = frame.GetFileLineNumber();
            Column = frame.GetFileColumnNumber();
            SourceCode = frame.GetFileName();
            Library = exception.Source;
            CallstackState = exception.InnerException == null;
        }
    }
}
