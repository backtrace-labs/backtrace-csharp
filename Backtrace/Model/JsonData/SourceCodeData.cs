using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Backtrace.Model.JsonData
{
    internal class SourceCodeData
    {
        internal class SourceCode
        {
            /// <summary>
            /// Function where exception occurs
            /// </summary>
            [JsonProperty(PropertyName = "funcName")]
            public string FunctionName { get; set; }

            /// <summary>
            /// Line number in source code where exception occurs
            /// </summary>
            //[JsonProperty(PropertyName = "startLine")]
            //public int StartLine { get; set; }

            /// <summary>
            /// Column number in source code where exception occurs
            /// </summary>
            //[JsonProperty(PropertyName = "startColumn")]
            //public int StartColumn { get; set; }

            private string _sourceCodeFullPath { get; set; }
            /// <summary>
            /// Full path to source code where exception occurs
            /// </summary>
            [JsonProperty(PropertyName = "path")]
            public string SourceCodeFullPath
            {
                get
                {
                    return Regex.Escape(_sourceCodeFullPath);
                }
                set
                {
                    _sourceCodeFullPath = value;
                }
            }


            /// <summary>
            /// Full path to source code where exception occurs
            /// </summary>
            //[JsonProperty(PropertyName = "text")]
            //public string Text
            //{
            //    get
            //    {
            //        if (!string.IsNullOrEmpty(_sourceCodeFullPath) && File.Exists(_sourceCodeFullPath))
            //        {
            //            return Regex.Escape(File.ReadAllText(_sourceCodeFullPath));
            //        }
            //        return string.Empty;
            //    }
            //}
        }

        internal Dictionary<string, SourceCode> data = new Dictionary<string, SourceCode>();
        public SourceCodeData(ExceptionStack exceptionStack)
        {
            if(exceptionStack == null)
            {
                return;
            }
            SourceCode code = new SourceCode()
            {
                //FunctionName = exceptionStack.FunctionName,
                SourceCodeFullPath = exceptionStack.SourceCodeFullPath,
                //StartColumn = exceptionStack.Column,
                //StartLine = exceptionStack.Line
            };
            data.Add(exceptionStack.FunctionName, code);
        }
    }
}
