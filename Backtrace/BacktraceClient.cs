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
        /// Set an event executed when send function triggers
        /// </summary>
        public Action<BacktraceReport> OnReportStart;

        /// <summary>
        /// Set an event executed after data send to Backtrace API
        /// </summary>
        public Action<BacktraceReport> AfterSend;

        /// <summary>
        /// Initialize Backtrace report client
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerMin">Numbers of records send per one min</param>
        public BacktraceClient(
                string sectionName = "BacktraceCredentials",
                Dictionary<string, object> attributes = null,
                string databaseDirectory = "",
                uint reportPerMin = 3
            )
            : base(BacktraceCredentials.ReadConfigurationSection(sectionName),
                  attributes, databaseDirectory, reportPerMin)
        {
        }

        /// <summary>
        /// Initialize client with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerMin">Numbers of records send per one minute</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, object> attributes = null,
            string databaseDirectory = "",
            uint reportPerMin = 3)
            : base(backtraceCredentials, attributes, databaseDirectory, reportPerMin)
        { }

        /// <summary>
        /// Send a backtrace report to Backtrace API
        /// </summary>
        /// <param name="backtraceReport">report</param>
        public void Send(BacktraceReport backtraceReport)
        {
            OnReportStart?.Invoke(backtraceReport);
            base.Send(backtraceReport);
            AfterSend?.Invoke(backtraceReport);

            //check if there is more errors to send 
            HandleInnerException(backtraceReport);
        }

        /// <summary>
        /// Handle inner exception in current backtrace report
        /// if inner exception exists, we should send report twice
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
        /// Send an exception to Backtrace API
        /// </summary>
        /// <param name="exception">Exception</param>
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
        /// Send a message to Backtrace API
        /// </summary>
        /// <param name="message">Message</param>
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
