
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Backtrace.Model
{
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
        public Guid Uuid = Guid.NewGuid();

        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        public long Timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        /// <summary>
        /// Name of programming language/environment this error comes from.
        /// </summary>
        public string Lang = "csharp";

        /// <summary>
        /// Get a C# language version
        /// </summary>
        public string LangVersion = typeof(string).Assembly.ImageRuntimeVersion;

        /// <summary>
        /// Get a report exepion type 
        /// </summary>
        public string Classifier { get; set; }

        /// <summary>
        /// Name of the client that is sending this error report.
        /// </summary>
        public string Agent;

        /// <summary>
        /// Version of the client that is sending this error report.
        /// </summary>
        public Version AgentVersion;

        /// <summary>
        /// Set an information about application main thread
        /// </summary>
        internal MainThreadInformation MainThread;

        /// <summary>
        /// Received BacktraceReport
        /// </summary>
        private readonly BacktraceReport<T> _report;

        /// <summary>
        /// Merged scoped attributes and report attributes
        /// </summary>
        private Dictionary<string, T> _attributes;

        private List<string> StackFrames;

        /// <summary>
        /// Create instance of report data class
        /// </summary>
        /// <param name="report">Received report</param>
        /// <param name="scopedAttributes">Scoped Attributes from BacktraceClient</param>
        public BacktraceData(BacktraceReport<T> report, Dictionary<string, T> scopedAttributes)
        {
            _attributes = scopedAttributes;
            _report = report;
            MainThread = new MainThreadInformation();
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
            var assembly = Assembly.GetExecutingAssembly().GetName();
            Agent = assembly.Name;
            AgentVersion = assembly.Version;
        }

        /// <summary>
        /// Set information about current report 
        /// </summary>
        internal void SetReportInformation()
        {
            _attributes = BacktraceReport<T>.ConcatAttributes(_report, _attributes);

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
            MainThread.Stack = exceptionStack;
            StackFrames = exceptionStack?.StackFrames
                .Select(n => n.ToString()).ToList();
        }
    }
}
