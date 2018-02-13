using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Backtrace.Model.JsonData;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Collections.Specialized;

namespace Backtrace.Model
{
    ///Todo : Add converter to string

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
        public string Lang
        {
            get
            {
                return "csharp";
            }
        }

        /// <summary>
        /// Get a C# language version
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
        /// Version of the client that is sending this error report.
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
        public Dictionary<string, string> Attributes
        {
            get
            {
                return _backtraceAttributes.Attributes;
            }
        }

        [JsonProperty(PropertyName = "annotations")]
        internal Annotations Annotations
        {
            get
            {
                return new Annotations(_report.CallingAssembly);
            }
        }

        [JsonProperty(PropertyName = "threads")]
        internal Dictionary<string, ThreadInformation> ThreadInformations
        {
            get
            {
                return ThreadData.ThreadInformations;
            }
        }

        /// <summary>
        /// Set an information about application main thread
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
                //we can't post to API 'null' value
                string currentThread = Thread.CurrentThread.Name;
                return string.IsNullOrEmpty(currentThread)
                        ? Thread.CurrentThread.ManagedThreadId.ToString()
                        : currentThread;
            }
        }

        /// <summary>
        /// Set an information about application main thread
        /// </summary>
        [JsonProperty(PropertyName = "arch")]
        internal Achitecture Architecture { get { return new Achitecture(); } }

        private readonly List<string> _stackFrames;
        /// <summary>
        /// Exception stack frames
        /// </summary>
        [JsonProperty(PropertyName = "callstack")]
        public Dictionary<string, List<string>> StackFrames
        {
            get
            {
                return new Dictionary<string, List<string>>()
                {
                    { "frames", _stackFrames }
                };
            }
        }

        /// <summary>
        /// Get a report exepion type 
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

        /// <summary>
        /// Get a path to attachments
        /// </summary>
        [JsonIgnore]
        public List<string> Attachments
        {
            get
            {
                return _report._attachmentPaths;
            }
        }

        /// <summary>
        /// Get a Backtrace attributes from client, report and system 
        /// </summary>
        private readonly BacktraceAttributes<T> _backtraceAttributes;

        /// <summary>
        /// Received BacktraceReport
        /// </summary>
        private readonly BacktraceReport<T> _report;

        private readonly AssemblyName CurrentAssembly = Assembly.GetExecutingAssembly().GetName();

        /// <summary>
        /// Create instance of report data class
        /// </summary>
        /// <param name="report">Received report</param>
        /// <param name="scopedAttributes">Scoped Attributes from BacktraceClient</param>
        public BacktraceData(BacktraceReport<T> report, Dictionary<string, T> scopedAttributes)
        {
            _report = report;
            _backtraceAttributes = new BacktraceAttributes<T>(_report, scopedAttributes);
            //reading exception stack
            ExceptionStack exceptionStack = _report.GetExceptionStack();
            _stackFrames = exceptionStack?.StackFrames;
            ThreadData = new ThreadData(exceptionStack);
        }
    }
}
