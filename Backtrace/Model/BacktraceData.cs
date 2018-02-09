using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Backtrace.Model.JsonData;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Backtrace.Model
{
    ///Todo : Add converter to string

    /// <summary>
    /// Serializable Backtrace API data object
    /// </summary>
    [Serializable]
    public class BacktraceData<T>
    {
        /// <summary>
        /// 16 bytes of randomness in human readable UUID format
        /// server will reject request if uuid is already found
        /// </summary>
        [JsonProperty(PropertyName = "uuid")]
        public Guid Uuid = Guid.NewGuid();

        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        /// <summary>
        /// Name of programming language/environment this error comes from.
        /// </summary>
        [JsonProperty(PropertyName = "lang")]
        public string Lang = "csharp";

        /// <summary>
        /// Get a C# language version
        /// </summary>
        [JsonProperty(PropertyName = "langVersion")]
        public string LangVersion = typeof(string).Assembly.ImageRuntimeVersion;

        /// <summary>
        /// Name of the client that is sending this error report.
        /// </summary>
        [JsonProperty(PropertyName = "agent")]
        public string Agent;

        /// <summary>
        /// Version of the client that is sending this error report.
        /// </summary>
        [JsonProperty(PropertyName = "agentVersion")]
        public string AgentVersion;

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
        internal readonly Annotations Annotations;

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
        internal ThreadData ThreadData;

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
        internal Achitecture Architecture = new Achitecture();

        private List<string> _stackFrames;
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
        [JsonProperty(PropertyName = "classifiers")]
        public string[] Classifier { get; set; }

        /// <summary>
        /// Get a Backtrace attributes from client, report and system 
        /// </summary>
        private readonly BacktraceAttributes<T> _backtraceAttributes;

        /// <summary>
        /// Received BacktraceReport
        /// </summary>
        private readonly BacktraceReport<T> _report;

        /// <summary>
        /// Create instance of report data class
        /// </summary>
        /// <param name="report">Received report</param>
        /// <param name="scopedAttributes">Scoped Attributes from BacktraceClient</param>
        public BacktraceData(BacktraceReport<T> report, Dictionary<string, T> scopedAttributes)
        {
            _report = report;
            _backtraceAttributes = new BacktraceAttributes<T>(report, scopedAttributes);
            //reading exception stack
            ExceptionStack exceptionStack = _report.GetExceptionStack();
            _stackFrames = exceptionStack?.StackFrames;
            ThreadData = new ThreadData(exceptionStack);
            Annotations = new Annotations(report.CallingAssembly);
            PrepareData();
        }

        /// <summary>
        /// Prepare all data to JSON file
        /// </summary>
        internal void PrepareData()
        {
            SetAssemblyInformation();
            SetExceptionInformation();
        }

        /// <summary>
        /// Set an assembly information about current program
        /// </summary>
        internal void SetAssemblyInformation()
        {
            var assemblyInformation = Assembly.GetExecutingAssembly().GetName();
            Agent = assemblyInformation.Name;
            AgentVersion = assemblyInformation.Version.ToString();
        }

        /// <summary>
        /// Set properties based on exception information
        /// </summary>
        internal void SetExceptionInformation()
        {
            if (!_report.ExceptionTypeReport)
            {
                return;
            }
            Classifier = new[] { _report.Classifier };

        }
    }
}
