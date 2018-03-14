using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Backtrace
{
    /// <summary>
    /// Backtrace .NET Client 
    /// </summary>
    public class BacktraceClient : BacktraceBase<object>, IBacktraceClient<object>
    {
        /// <summary>
        /// Set an event executed before sending data to Backtrace API
        /// </summary>
        public Action<BacktraceReport> OnReportStart;

        /// <summary>
        /// Set an event executed after sending data to Backtrace API
        /// </summary>
        public Action<BacktraceReport> AfterSend;

#if NET35 || NET45
        /// <summary>
        /// Initializing Backtrace client instance
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseDirectory">Database path used to store minidumps and temporary reports</param>
        /// <param name="reportPerMin">Numbers of records sending per one min</param>
        public BacktraceClient(
                string sectionName = "BacktraceCredentials",
                Dictionary<string, object> attributes = null,
                string databaseDirectory = "",
                uint reportPerMin = 3
            )
            : base(BacktraceCredentials.ReadConfigurationSection(sectionName),
                  attributes, databaseDirectory, reportPerMin)
        { }
#endif

        /// <summary>
        /// Initializing Backtrace client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseDirectory">Database path used to store minidumps and temporary reports</param>
        /// <param name="reportPerMin">Numbers of records sending per one minute</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, object> attributes = null,
            string databaseDirectory = "",
            uint reportPerMin = 3)
            : base(backtraceCredentials, attributes, databaseDirectory, reportPerMin)
        { }

        /// <summary>
        /// Sending a backtrace report to Backtrace API
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        public void Send(BacktraceReport backtraceReport)
        {
            OnReportStart?.Invoke(backtraceReport);
            base.Send(backtraceReport);
            AfterSend?.Invoke(backtraceReport);

            //check if there is more errors to send
            //handle inner exception
            HandleInnerException(backtraceReport);
        }

        /// <summary>
        /// Handle inner exception in current backtrace report
        /// if inner exception exists, client should send report twice - one with current exception, one with inner exception
        /// </summary>
        /// <param name="report">current report</param>
        private void HandleInnerException(BacktraceReport report)
        {
            // there is no additional exception inside current exception
            // or exception does not exists
            if (!report.ExceptionTypeReport || report.Exception.InnerException == null)
            {
                return;
            }
            //we have to create a copy of an inner exception report
            //to have the same calling assembly property
            var innerExceptionReport = report.CreateInnerReport();
            Send(innerExceptionReport);
        }

        /// <summary>
        /// Sending an exception to Backtrace API
        /// </summary>
        /// <param name="exception">Current exception</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public virtual void Send(
            Exception exception,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        { 
            var report = new BacktraceReport(exception, attributes, attachmentPaths)
            {
                CallingAssembly = Assembly.GetCallingAssembly()
            };
            Send(report);
            HandleInnerException(report);
        }
        /// <summary>
        /// Sending a message to Backtrace API
        /// </summary>
        /// <param name="message">Custom client message</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public virtual void Send(
            string message,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        {
            var report = new BacktraceReport(message, attributes, attachmentPaths)
            {
                CallingAssembly = Assembly.GetCallingAssembly()
            };
            Send(report);
        }
    }
}
