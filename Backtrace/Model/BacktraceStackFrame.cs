using Backtrace.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Backtrace.Model
{
    public class BacktraceStackFrame
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
        /// IL Offset
        /// </summary>
        [JsonProperty(PropertyName = "il")]
        public int? Il { get; set; }

        /// <summary>
        /// PBD Unique identifier
        /// </summary>
        [JsonProperty(PropertyName = "metadata_token")]
        public int? MemberInfo { get; set; }


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
        /// Address of the stack frame
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public int? ILOffset { get; set; }

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

        internal Assembly FrameAssembly { get; set; }

#if NET45
        public BacktraceStackFrame(Microsoft.Diagnostics.Runtime.ClrStackFrame frame)
        {
            FunctionName = frame.Method.Name;
            Library = frame.ModuleName;
        }

#endif
        public BacktraceStackFrame(StackFrame frame, bool generatedByException)
        {
            if(frame == null)
            {
                return;
            }
            FunctionName = GetMethodName(frame);
            Line = frame.GetFileLineNumber();
            Il = frame.GetILOffset();
            ILOffset = Il;
            SourceCodeFullPath = frame.GetFileName();
            FrameAssembly = frame.GetMethod().DeclaringType?.Assembly;
            Library = FrameAssembly?.GetName().Name ?? "unknown";
            SourceCode = generatedByException
                    ? Guid.NewGuid().ToString()
                    : string.Empty;
            Column = frame.GetFileColumnNumber();
            try
            {
                MemberInfo = frame.GetMethod().MetadataToken;
            }
            catch (InvalidOperationException)
            {
                //metadata token in some situations can throw Argument Exception. Plase check property definition to leran more about this behaviour
            }
        }

        /// <summary>
        /// Generate valid name for current stack frame.
        /// </summary>
        /// <returns>Valid method name in stack trace</returns>
        private string GetMethodName(StackFrame frame)
        {
            var method = frame.GetMethod();
            string methodName = method.Name;
#if NET35
            return methodName;
#else
            var declaringType = method.DeclaringType?.GetTypeInfo();
            if (declaringType == null)
            {
                return method.Name;
            }
            // generate full method name with included namespace
            string fullName = GetMethodFullName(declaringType);
            // resolve name of async stack frame
            bool isAsync = SystemHelper.StateMachineFrame(declaringType);
            if (isAsync)
            {
                fullName = ResolveAsyncStackFrameName(fullName);
            }
            else
            {
                fullName = fullName + "." + method.Name;
            }
            StringBuilder result = new StringBuilder(fullName);
           
            // add method parameters
            result.Append(AddMethodParameters(method.GetParameters()));
            return result.ToString();
#endif
        }
#if !NET35
        /// <summary>
        /// Generate method full name with included namespace
        /// </summary>
        /// <param name="declaringType">Current type info</param>
        /// <returns>Full method name</returns>
        private string GetMethodFullName(TypeInfo declaringType)
        {
            return declaringType.FullName.Replace("+", ".");
        }

        /// <summary>
        /// Remove invalid method name for async stack frames
        /// </summary>
        /// <param name="fullAsyncName">Full async method name</param>
        /// <returns>Valid async method name</returns>
        private string ResolveAsyncStackFrameName(string fullAsyncName)
        {
            var start = fullAsyncName.LastIndexOf('<');
            var end = fullAsyncName.LastIndexOf('>');
            if (start >= 0 && end >= 0)
            {
                return fullAsyncName.Remove(start, 1).Substring(0, end - 1);
            }
            return fullAsyncName;
        }

        /// <summary>
        /// Generate parameter string
        /// </summary>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method parameters in string</returns>
        private string AddMethodParameters(ParameterInfo[] parameters)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("(");
            var firstParam = true;
            foreach (var param in parameters)
            {
                if (!firstParam)
                {
                    builder.Append(", ");
                }
                else
                {
                    firstParam = false;
                }
                // ReSharper disable once ConstantConditionalAccessQualifier
                // ReSharper disable once ConstantNullCoalescingCondition
                var typeName = param.ParameterType?.Name ?? "<UnknownType>";
                builder.Append(typeName);
                builder.Append(" ");
                builder.Append(param.Name);
            }
            builder.Append(")");
            return builder.ToString();
        }
#endif
    }
}
