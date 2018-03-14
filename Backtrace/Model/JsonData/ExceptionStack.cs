﻿#if NET45
using Microsoft.Diagnostics.Runtime;
#endif
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Parse exception information to Backtrace API format
    /// </summary>
    public class ExceptionStack
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
        /// Full path to source code where exception occurs
        /// </summary>
        [JsonIgnore]
        public string SourceCodeFullPath { get; set; }

        /// <summary>
        /// Column number in source code where exception occurs
        /// </summary>
        [JsonProperty(PropertyName = "column")]
        public int Column { get; set; }

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
        /// Convert stackframe to ExceptionStack
        /// </summary>
        /// <param name="stackFrame">Current Stack frame</param>
        /// <param name="libraryName">Library name</param>
        /// <param name="generatedByException">If true, current exception stack is generated by exception</param>
        /// <returns>ExceptionStack instance</returns>
        internal static ExceptionStack Convert(StackFrame stackFrame, string libraryName, bool generatedByException = false)
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
                SourceCode = generatedByException ? Guid.NewGuid().ToString() : string.Empty,
                SourceCodeFullPath = stackFrame.GetFileName()
            };
        }

        internal static IEnumerable<ExceptionStack> FromCurrentThread(string libraryName, IEnumerable<ExceptionStack> exceptionStack)
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var stackTrace = new StackTrace();

            //get all stack created by current thread - excpect stacks created by diagnostic library
            var stackFrames = stackTrace.GetFrames()
                .Where(n => n.GetMethod().DeclaringType.Assembly != currentAssembly);

            //convert current thread stack trace to exception stacks
            var result = stackFrames.Select(n => Convert(n, libraryName)).ToList();

            //if there is no stack trace 
            //return stack trace from exception
            if (result == null || result.Count == 0)
            {
                return exceptionStack;
            }
            //concat two stacks if possible
            if (exceptionStack != null && exceptionStack.Count() > 0)
            {
                var comparer = result[0];
                foreach (var stack in exceptionStack)
                {
                    if (stack.FunctionName == comparer.FunctionName
                        && stack.Library == comparer.Library)
                    {
                        comparer.SourceCode = stack.SourceCode;
                        break;
                    }
                    result.Insert(0, stack);
                }
            }
            return result;
        }

        internal static List<ExceptionStack> Convert(Exception exception)
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
            int stackFramesLength = stackFrames.Length;
            return stackFrames.Select(n => Convert(n, source, true)).ToList();
        }
#if NET45
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
