using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

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
        /// Set an information about application main thread
        /// </summary>
        [JsonProperty(PropertyName = "threads")]
        internal MainThreadInformation ThreadInformations;

        /// <summary>
        /// Get a main thread name
        /// </summary>
        [JsonProperty(PropertyName = "mainThread")]
        public string MainThread = System.Threading.Thread.CurrentThread.Name;

        /// <summary>
        /// Set an information about application main thread
        /// </summary>
        [JsonProperty(PropertyName = "arch")]
        internal Achitecture Architecture = new Achitecture();

        /// <summary>
        /// Exception stack frames
        /// </summary>
        [JsonProperty(PropertyName = "callstack.frames")]
        public List<string> StackFrames;

        /// <summary>
        /// Get a report exepion type 
        /// </summary>
        [JsonProperty(PropertyName = "classifiers")]
        public string Classifier { get; set; }

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

        /// <summary>
        /// Get a Backtrace attributes
        /// </summary>
        private readonly BacktraceAttributes<T> _backtraceAttributes;

        /// <summary>
        /// Merged scoped attributes and report attributes
        /// </summary>
        [JsonProperty(PropertyName = "annotations")]
        public Dictionary<string, T> Annotations;

        /// <summary>
        /// Get an executed application dependencies
        /// </summary>
        [JsonProperty(PropertyName = "dependencies")]
        internal readonly ApplicationDependencies ApplicationDependencies;

        /// <summary>
        /// Get system environment variables
        /// </summary>
        [JsonProperty(PropertyName = "variables")]
        internal readonly EnvironmentVariables EnvironmentVariables;

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
            Annotations = scopedAttributes;
            _report = report;
            EnvironmentVariables = new EnvironmentVariables();
            _backtraceAttributes = new BacktraceAttributes<T>(report);
            ThreadInformations = new MainThreadInformation();
            ApplicationDependencies = new ApplicationDependencies(report.CallingAssembly);
            PrepareData();
        }

        /// <summary>
        /// Prepare all data to JSON file
        /// </summary>
        internal void PrepareData()
        {
            SetAssemblyInformation();
            SetReportInformation();
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
        /// Set information about current report 
        /// </summary>
        internal void SetReportInformation()
        {
            Annotations = BacktraceReport<T>.ConcatAttributes(_report, Annotations);

            //Set exception type properties
            if (_report.ExceptionTypeReport)
            {
                SetExceptionInformation();
            }
        }

        /// <summary>
        /// Set properties based on exception information
        /// </summary>
        internal void SetExceptionInformation()
        {
            Classifier = _report.Classifier;
            var exceptionStack = _report.GetExceptionStack();

            //read exception stack
            ThreadInformations.Stack = exceptionStack;
            StackFrames = exceptionStack?.StackFrames;
        }
    }
}
