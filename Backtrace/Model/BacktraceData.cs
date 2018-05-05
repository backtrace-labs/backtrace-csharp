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
        public Guid Uuid { get; private set; }

        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; private set; }

        /// <summary>
        /// Name of programming language/environment this error comes from.
        /// </summary>
        [JsonProperty(PropertyName = "lang")]
        public const string Lang = "csharp";

        /// <summary>
        /// Version of programming language/environment this error comes from.
        /// </summary>
        [JsonProperty(PropertyName = "langVersion")]
        public string LangVersion { get; private set; }

        /// <summary>
        /// Name of the client that is sending this error report.
        /// </summary>
        [JsonProperty(PropertyName = "agent")]
        public string Agent { get; private set; }

        /// <summary>
        /// Version of the C# library
        /// </summary>
        [JsonProperty(PropertyName = "agentVersion")]
        public string AgentVersion { get; set; }

        /// <summary>
        /// Get built-in attributes
        /// </summary>
        [JsonProperty(PropertyName = "attributes")]
        public Dictionary<string, object> Attributes { get; set; }


        /// <summary>
        /// Get current host environment variables and application dependencies
        /// </summary>
        [JsonProperty(PropertyName = "annotations")]
        internal Annotations Annotations { get; set; }

        /// <summary>
        /// Application thread details
        /// </summary>
        [JsonProperty(PropertyName = "threads")]
        internal Dictionary<string, ThreadInformation> ThreadInformations { get; set; }

        /// <summary>
        /// Get a main thread name
        /// </summary>
        [JsonProperty(PropertyName = "mainThread")]
        public string MainThread { get; set; }

        /// <summary>
        /// Get a report classifiers. If user send custom message, then variable should be null
        /// </summary>
        [JsonProperty(PropertyName = "classifiers", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Classifier { get; set; }

        [JsonProperty(PropertyName = "sourceCode", NullValueHandling = NullValueHandling.Ignore)]
        internal Dictionary<string, SourceCode> SourceCode { get; set; }

        /// <summary>
        /// Get a path to report attachments
        /// </summary>
        [JsonIgnore]
        public List<string> Attachments
        {
            get
            {
                return Report.AttachmentPaths.Distinct().ToList();
            }
        }

        /// <summary>
        /// Current BacktraceReport
        /// </summary>
        internal BacktraceReportBase<T> Report { get; set; }

        /// <summary>
        /// Create instance of report data
        /// </summary>
        /// <param name="report">Current report</param>
        /// <param name="clientAttributes">BacktraceClient's attributes</param>
        [JsonConstructor]
        public BacktraceData(BacktraceReportBase<T> report, Dictionary<string, T> clientAttributes)
        {
            if (report == null)
            {
                return;
            }
            Report = report;
            SetReportInformation();
            SetAttributes(clientAttributes);
            SetThreadInformations();

        }

        private void SetThreadInformations()
        {
            var threadData = new ThreadData(Report.CallingAssembly, Report.DiagnosticStack);
            ThreadInformations = threadData.ThreadInformations;
            MainThread = threadData.MainThread;
            var sourceCodeData = new SourceCodeData(Report.DiagnosticStack);
            SourceCode = sourceCodeData.data.Any() ? sourceCodeData.data : null;
        }

        private void SetAttributes(Dictionary<string, T> clientAttributes)
        {
            var backtraceAttributes = new BacktraceAttributes<T>(Report, clientAttributes);
            Attributes = backtraceAttributes.Attributes;
            Annotations = new Annotations(Report.CallingAssembly, backtraceAttributes.ComplexAttributes);
        }

        private void SetReportInformation()
        {
            AssemblyName CurrentAssembly = Assembly.GetExecutingAssembly().GetName();
            Uuid = Report.Uuid;
            Timestamp = Report.Timestamp;
            LangVersion = typeof(string).Assembly.ImageRuntimeVersion;
            Agent = CurrentAssembly.Name;
            AgentVersion = CurrentAssembly.Version.ToString();
            Classifier = Report.ExceptionTypeReport ? new[] { Report.Classifier } : null;
        }
    }
}
