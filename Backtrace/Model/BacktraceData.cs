
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Serializable Backtrace API data object
    /// </summary>
    [Serializable]
    internal class BacktraceData<T>
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

        public string LangVersion = typeof(string).Assembly.ImageRuntimeVersion;

        /// <summary>
        /// Name of the client that is sending this error report.
        /// </summary>
        public string Agent;

        /// <summary>
        /// Version of the client that is sending this error report.
        /// </summary>
        public Version AgentVersion;

        private readonly BacktraceReport<T> _report;
        private Dictionary<string, T> _attributes;

        /// <summary>
        /// Create instance of report data class
        /// </summary>
        /// <param name="report">Received report</param>
        /// <param name="scopedAttributes">Scoped Attributes from BacktraceClient</param>
        public BacktraceData(BacktraceReport<T> report, Dictionary<string, T> scopedAttributes)
        {
            _report = report;
            _attributes = scopedAttributes;
        }

        internal void PrepareData()
        {
            var assembly = Assembly.GetExecutingAssembly().GetName();
            Agent = assembly.Name;
            var Version = assembly.Version;
            var attributes = BacktraceReport<T>.ConcatAttributes(_report, _attributes);
        }
    }
}
