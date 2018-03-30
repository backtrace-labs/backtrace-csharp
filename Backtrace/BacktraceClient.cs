using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Reflection;
#if !NET35
using System.Threading.Tasks;
#endif

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
        public Action<BacktraceResult> AfterSend;

#if !NETSTANDARD2_0
        /// <summary>
        /// Initializing Backtrace client instance
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseDirectory">Database path used to store minidumps and temporary reports</param>
        /// <param name="reportPerMin">Numbers of records sending per one min</param>
        /// <param name="tlsSupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
                string sectionName = "BacktraceCredentials",
                Dictionary<string, object> attributes = null,
                string databaseDirectory = "",
                uint reportPerMin = 3,
                bool tlsSupport = false)
                : base(BacktraceCredentials.ReadConfigurationSection(sectionName),
                  attributes, databaseDirectory, reportPerMin,tlsSupport)
        { }
#endif

        /// <summary>
        /// Initializing Backtrace client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseDirectory">Database path used to store minidumps and temporary reports</param>
        /// <param name="reportPerMin">Numbers of records sending per one minute</param>
        /// <param name="tlsSupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, object> attributes = null,
            string databaseDirectory = "",
            uint reportPerMin = 3,
            bool tlsSupport = false)
            : base(backtraceCredentials, attributes, databaseDirectory, reportPerMin, tlsSupport)
        { }

        /// <summary>
        /// Sending an exception to Backtrace API
        /// </summary>
        /// <param name="exception">Current exception</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public virtual BacktraceResult Send(
            Exception exception,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        {
            var report = new BacktraceReport(exception, attributes, attachmentPaths);
            var result = Send(report);
            HandleInnerException(report);
            return result;
        }

        /// <summary>
        /// Sending a message to Backtrace API
        /// </summary>
        /// <param name="message">Custom client message</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public virtual BacktraceResult Send(
            string message,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        {
            return Send(new BacktraceReport(message, attributes, attachmentPaths));
        }

        /// <summary>
        /// Sending a backtrace report to Backtrace API
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        public BacktraceResult Send(BacktraceReport backtraceReport)
        {
            OnReportStart?.Invoke(backtraceReport);
            var result =  base.Send(backtraceReport);
            AfterSend?.Invoke(result);

            //check if there is more errors to send
            //handle inner exception
            HandleInnerException(backtraceReport);
            return result;
        }

#if !NET35
        /// <summary>
        /// Sending asynchronous Backtrace report to Backtrace API
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        /// <returns>Server response</returns>
        public async Task<BacktraceResult> SendAsync(BacktraceReport backtraceReport)
        {
            OnReportStart?.Invoke(backtraceReport);
            var response = await base.SendAsync(backtraceReport);
            AfterSend?.Invoke(response);

            //check if there is more errors to send
            //handle inner exception
            HandleInnerException(backtraceReport);

            return response;
        }

        /// <summary>
        /// Sending a message to Backtrace API
        /// </summary>
        /// <param name="message">Custom client message</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public virtual async Task<BacktraceResult> SendAsync(
            string message,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        {
            return await SendAsync(new BacktraceReport(message, attributes, attachmentPaths));
        }

        /// <summary>
        /// Sending asynchronous exception to Backtrace API
        /// </summary>
        /// <param name="exception">Current exception</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public virtual async Task<BacktraceResult> SendAsync(
            Exception exception,
            Dictionary<string, object> attributes = null,
            List<string> attachmentPaths = null)
        {
            var report = new BacktraceReport(exception, attributes, attachmentPaths);
            var response = await SendAsync(report);
            HandleInnerException(report);
            return response;
        }
#endif

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

       

    }
}
