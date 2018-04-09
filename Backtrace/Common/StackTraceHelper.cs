using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Backtrace.Common
{
    /// <summary>
    /// All usefull information about current StackTrace
    /// </summary>
    public static class StackTraceHelper
    {
        /// <summary>
        /// Get current thread stack trace
        /// </summary>
        /// <param name="source">Current exception (if exists</param>
        /// <returns>Current thread stack frames</returns>
        public static List<StackFrame> GetStackFrames(Exception source = null)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            // generate stacktrace with file info
            // if assembly have pbd files, diagnostic JSON will contain information about 
            // line number and column number
            var stackTrace = new StackTrace(true);
#if DEBUG
            Trace.WriteLine("CURRENT THREAD STACK TRACE:");
            Trace.WriteLine(stackTrace.ToString());
            Trace.WriteLine("END OF THE STACK TRACE");
#endif
            var stackFrames = stackTrace.GetFrames()
                .Where(n => n?.GetMethod()?.DeclaringType?.Assembly != currentAssembly)
                .ToList();

            if (source != null)
            {
                var exceptionStackTrace = new StackTrace(source, true);
#if DEBUG
                Trace.WriteLine("CURRENT EXCEPTION STACK TRACE:");
                Trace.WriteLine(exceptionStackTrace.ToString());
                Trace.WriteLine("END OF THE EXCEPTION STACK TRACE");
#endif
                var exceptionStackFrames = exceptionStackTrace.GetFrames();
                //information from exception stack frame is already in current stacktrace (example: catching unhandled app exception)
                if (stackFrames[0] != null && exceptionStackFrames[0] != null
                    && stackFrames[0].GetILOffset() == exceptionStackFrames[0].GetILOffset()
                    && stackFrames[0].GetMethod()?.Name == exceptionStackFrames[0].GetMethod()?.Name)
                {
                    return stackFrames;
                }
                stackFrames.InsertRange(0, exceptionStackFrames);
            }
            return stackFrames;
        }
    }
}
