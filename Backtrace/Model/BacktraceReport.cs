using Backtrace.Base;
using Backtrace.Common;
using Backtrace.Extensions;
using Backtrace.Model.JsonData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Capture a report of an application
    /// </summary>
    public class BacktraceReport : BacktraceReportBase<object>
    {
        /// <summary>
        /// Sending a report with custom message
        /// </summary>
        /// <param name="message">message about application state</param>
        /// <param name="attributes">Report additional information</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReport(
            string message,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
            : base(message, attributes, attachmentPaths)
        { }
        /// <summary>
        /// Sending a report with custom exception
        /// </summary>
        /// <param name="exception">Occur exception</param>
        /// <param name="attributes">Report additional information</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReport(
            Exception exception,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
            : base(exception, attributes, attachmentPaths)
        { }

        /// <summary>
        /// create a copy of BacktraceReport for inner exception object inside exception
        /// </summary>
        /// <returns>BacktraceReport for InnerExceptionObject</returns>
        internal BacktraceReport CreateInnerReport()
        {
            var copy = (BacktraceReport)this.MemberwiseClone();
            copy.Exception = this.Exception.InnerException;
            return copy;
        }
    }
}
