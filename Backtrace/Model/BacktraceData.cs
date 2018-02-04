using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Serializable Backtrace API data object
    /// </summary>
    [Serializable]
    internal class BacktraceData<T>
    {
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

        }
    }
}
