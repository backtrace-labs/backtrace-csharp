using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Backtrace.Model.JsonData
{
    internal class SourceCodeData
    {
        internal class SourceCode
        {
            /// <summary>
            /// Line number in source code where exception occurs
            /// </summary>
            [JsonProperty(PropertyName = "startLine")]
            public int StartLine { get; set; }

            /// <summary>
            /// Column number in source code where exception occurs
            /// </summary>
            [JsonProperty(PropertyName = "startColumn")]
            public int StartColumn { get; set; }

            private string _sourceCodeFullPath { get; set; }
            /// <summary>
            /// Full path to source code where exception occurs
            /// </summary>
            [JsonProperty(PropertyName = "path")]
            public string SourceCodeFullPath
            {
                get
                {
                    if (!string.IsNullOrEmpty(_sourceCodeFullPath))
                    {
                        return Regex.Escape(_sourceCodeFullPath);
                    }
                    return string.Empty;
                }
                set
                {
                    _sourceCodeFullPath = value;
                }
            }

            public static SourceCode FromExceptionStack(ExceptionStack exceptionStack)
            {
                return new SourceCode()
                {
                    StartColumn = exceptionStack.Column,
                    StartLine = exceptionStack.Line,
                    SourceCodeFullPath = exceptionStack.SourceCodeFullPath
                };
            }
        }

        internal Dictionary<string, SourceCode> data = new Dictionary<string, SourceCode>();
        public SourceCodeData(IEnumerable<ExceptionStack> exceptionStack)
        {
            SetStack(exceptionStack);
        }

        private void SetStack(IEnumerable<ExceptionStack> exceptionStack)
        {
            if (exceptionStack == null || exceptionStack.Count() == 0)
            {
                return;
            }
            foreach (var exception in exceptionStack)
            {
                string key = Guid.NewGuid().ToString();
                var value = SourceCode.FromExceptionStack(exception);
                data.Add(key, value);
            }
        }
    }
}
