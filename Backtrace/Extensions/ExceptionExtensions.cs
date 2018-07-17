using Backtrace.Model;
using Backtrace.Model.JsonData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Backtrace.Extensions
{
    /// <summary>
    /// Extensions method available for every excepton object
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Convert current exception to BacktraceReport instance
        /// </summary>
        /// <param name="source">Current exception</param>
        /// <returns>Backtrace Report</returns>
        public static BacktraceReport ToBacktraceReport(this Exception source)
        {
            return new BacktraceReport(source);
        }

        public static Assembly GetExceptionSourceAssembly(this Exception source)
        {
            return source?.TargetSite?.DeclaringType?.Assembly;
        }

        /// <summary>
        /// Generate stack traces that not exists in current thread stack trace
        /// </summary>
        /// <returns>Unique exception stack frames</returns>
        //internal static StackFrame[] GetExceptionStackFrames(this Exception source, DiagnosticStack firstFrame)
        //{
        //    if (source == null)
        //    {
        //        return null;
        //    }
        //    var exceptionStackTrace = new StackTrace(source, true);
        //    var exceptionStackFrames = exceptionStackTrace.GetFrames();
        //    if (exceptionStackFrames == null || !exceptionStackFrames.Any())
        //    {
        //        return null;
        //    }
        //    if (firstFrame == null)
        //    {
        //        return exceptionStackFrames;
        //    }
        //    var comparer = exceptionStackFrames[0];
        //    //validate if exception stack frame exists in environment stack trace
        //    if (firstFrame.ILOffset == comparer.GetILOffset()
        //        && firstFrame.FunctionName == comparer.GetMethod()?.Name)
        //    {
        //        return null;
        //    }
        //    return exceptionStackFrames;
        //}
    }
}
