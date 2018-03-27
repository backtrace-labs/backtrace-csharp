using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Backtrace.Model.JsonData;
using System.Linq;
using System.Reflection;
using static Backtrace.Model.JsonData.SourceCodeData;
using Backtrace.Base;

namespace Backtrace.Model
{
    /// <summary>
    /// Serializable Backtrace API data object
    /// </summary>
    public class BacktraceData<T>
    {
        /// <summary>
        /// 16 bytes of randomness in human readable UUID format
        /// server will reject request if uuid is already found
        /// </summary>
        [JsonProperty(PropertyName = "uuid")]
        public Guid Uuid
        {
            get
            {
                return _report.Uuid;
            }
        }


        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp
        {
            get
            {
                return _report.Timestamp;
            }
        }

        /// <summary>
        /// Name of programming language/environment this error comes from.
        /// </summary>
        [JsonProperty(PropertyName = "lang")]
        public const string Lang = "csharp";

        /// <summary>
        /// Version of programming language/environment this error comes from.
        /// </summary>
        [JsonProperty(PropertyName = "langVersion")]
        public string LangVersion
        {
            get
            {
                return typeof(string).Assembly.ImageRuntimeVersion;
            }
        }

        /// <summary>
        /// Name of the client that is sending this error report.
        /// </summary>
        [JsonProperty(PropertyName = "agent")]
        public string Agent
        {
            get
            {
                return CurrentAssembly.Name;
            }
        }

        /// <summary>
        /// Version of the C# library
        /// </summary>
        [JsonProperty(PropertyName = "agentVersion")]
        public string AgentVersion
        {
            get
            {
                return CurrentAssembly.Version.ToString();
            }
        }


        /// <summary>
        /// Get built-in attributes
        /// </summary>
        [JsonProperty(PropertyName = "attributes")]
        public Dictionary<string, object> Attributes
        {
            get
            {
                return _backtraceAttributes.Attributes;
            }
        }

        /// <summary>
        /// Get current host environment variables and application dependencies
        /// </summary>
        [JsonProperty(PropertyName = "annotations")]
        internal Annotations Annotations
        {
            get
            {
                return new Annotations(_report.CallingAssembly, _backtraceAttributes.ComplexAttributes);
            }
        }

        /// <summary>
        /// Application thread details
        /// </summary>
        [JsonProperty(PropertyName = "threads")]
        internal Dictionary<string, ThreadInformation> ThreadInformations
        {
            get
            {
                return ThreadData.ThreadInformations;
            }
        }

        /// <summary>
        /// Information about available application threads
        /// </summary>
        internal ThreadData ThreadData { get; set; }

        /// <summary>
        /// Get a main thread name
        /// </summary>
        [JsonProperty(PropertyName = "mainThread")]
        public string MainThread
        {
            get
            {
                return ThreadData.MainThread;
            }
        }

        /// <summary>
        /// Get a report classifiers. If user send custom message, then variable should be null
        /// </summary>
        [JsonProperty(PropertyName = "classifiers", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Classifier
        {
            get
            {
                if (_report.ExceptionTypeReport)
                {
                    return new[] { _report.Classifier };
                }
                return null;
            }
        }

        [JsonProperty(PropertyName = "sourceCode", NullValueHandling = NullValueHandling.Ignore)]
        internal Dictionary<string, SourceCode> SourceCode
        {
            get
            {
                var sourceCode = new SourceCodeData(_exceptionStack);
                if (sourceCode.data.Any())
                {
                    return sourceCode.data;
                }
                return null;
            }
        }

        /// <summary>
        /// Get a path to report attachments
        /// </summary>
        [JsonIgnore]
        public List<string> Attachments
        {
            get
            {
                return _report.AttachmentPaths;
            }
        }

        /// <summary>
        /// Get a Backtrace a built-in attributes from current application
        /// </summary>
        private readonly BacktraceAttributes<T> _backtraceAttributes;

        /// <summary>
        /// Current BacktraceReport
        /// </summary>
        private readonly BacktraceReportBase<T> _report;

        private readonly AssemblyName CurrentAssembly = Assembly.GetExecutingAssembly().GetName();

        private readonly IEnumerable<ExceptionStack> _exceptionStack;

        /// <summary>
        /// Create instance of report data
        /// </summary>
        /// <param name="report">Current report</param>
        /// <param name="scopedAttributes">BacktraceClient's attributes</param>
        public BacktraceData(BacktraceReportBase<T> report, Dictionary<string, T> scopedAttributes)
        {
            _report = report;
            _backtraceAttributes = new BacktraceAttributes<T>(_report, scopedAttributes);
            //reading exception stack
            _exceptionStack = _report.GetExceptionStack();
            ThreadData = new ThreadData(report.CallingAssembly, _exceptionStack);
        }
    }
}
