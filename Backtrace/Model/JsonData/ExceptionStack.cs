#if NET461
using Microsoft.Diagnostics.Runtime;
#endif
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Parse exception information to Backtrace API format
    /// missing values - address, guessed_frame, callstack_state
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
        /// Convert stackframe to ExceptionStack used in diagnose JSON
        /// </summary>
        /// <param name="stackFrame">Current Stack frame</param>
        /// <param name="libraryName">Library name</param>
        /// <returns>ExceptionStack instance</returns>
        internal static ExceptionStack Convert(StackFrame stackFrame, string libraryName)
        {
            if (stackFrame == null)
            {
                return null;
            }
            return new ExceptionStack()
            {
                Column = stackFrame.GetFileColumnNumber(),
                Library = libraryName,
                FunctionName = stackFrame.GetMethod().Name,
                Line = stackFrame.GetFileLineNumber(),
                SourceCode = Path.GetFileName(stackFrame.GetFileName()),
                SourceCodeFullPath = stackFrame.GetFileName()
            };
        }

        internal static IEnumerable<ExceptionStack> Convert(Exception exception)
        {
            if (exception == null)
            {
                return null;
            }

            var stackTrace = new StackTrace(exception, true);
            var stackFrames = stackTrace.GetFrames();
            string source = exception.Source;
            if (stackFrames == null || stackFrames.Length == 0)
            {
                return new List<ExceptionStack>();
            }
            return stackFrames.Select(n => Convert(n, source));
        }
#if NET461
        internal static IEnumerable<ExceptionStack> Convert(IEnumerable<ClrStackFrame> clrStackFrames)
        {
            var result = new List<ExceptionStack>();
            if (clrStackFrames == null || clrStackFrames.Count() == 0)
            {
                return result;
            }
            foreach (var clrStackFrame in clrStackFrames)
            {
                if (clrStackFrame.Method == null)
                {
                    continue;
                }
                result.Add(new ExceptionStack()
                {
                    FunctionName = clrStackFrame.Method.Name,
                    Library = clrStackFrame.ModuleName
                });
            }
            return result;
        }
#endif
    }
}
