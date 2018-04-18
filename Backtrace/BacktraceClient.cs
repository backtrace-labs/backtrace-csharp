using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Services;
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

        #region Constructor
#if !NETSTANDARD2_0
        /// <summary>
        /// Initializing Backtrace client instance
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Numbers of records sending per one min</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
            string sectionName,
            string databasePath,            
            Dictionary<string, object> attributes = null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
            : this(BacktraceCredentials.ReadConfigurationSection(sectionName),
                attributes, new BacktraceDatabase<object>(new BacktraceDatabaseSettings() { DatabasePath = databasePath }),
                reportPerMin, tlsLegacySupport)
        { }

        /// <summary>
        /// Initializing Backtrace client instance
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Numbers of records sending per one min</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
            BacktraceDatabaseSettings databaseSettings,
            string sectionName = "BacktraceCredentials",
            Dictionary<string, object> attributes = null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
            : base(BacktraceCredentials.ReadConfigurationSection(sectionName),
                attributes, new BacktraceDatabase<object>(databaseSettings), reportPerMin, tlsLegacySupport)
        { }

        /// <summary>
        /// Initializing Backtrace client instance
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Numbers of records sending per one min</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
            string sectionName = "BacktraceCredentials",
            Dictionary<string, object> attributes = null,
            IBacktraceDatabase<object> database = null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
            : base(BacktraceCredentials.ReadConfigurationSection(sectionName),
                attributes, database, reportPerMin, tlsLegacySupport)
        { }
#endif
        /// <summary>
        /// Initializing Backtrace client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Numbers of records sending per one minute</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            BacktraceDatabaseSettings databaseSettings,
            Dictionary<string, object> attributes = null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
            : base(backtraceCredentials, attributes,
                  databaseSettings, reportPerMin, tlsLegacySupport)
        { }

        /// <summary>
        /// Initializing Backtrace client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Numbers of records sending per one minute</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            string databasePath,
            Dictionary<string, object> attributes = null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
            : base(backtraceCredentials, attributes,
                  new BacktraceDatabaseSettings() { DatabasePath = databasePath },
                  reportPerMin, tlsLegacySupport)
        { }

        /// <summary>
        /// Initializing Backtrace client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials</param>
        /// <param name="attributes">Client's attributes</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Numbers of records sending per one minute</param>
        /// <param name="tlsLegacySupport">Set SSL and TLS flags for https request to Backtrace API</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, object> attributes = null,
            IBacktraceDatabase<object> backtraceDatabase = null,
            uint reportPerMin = 3,
            bool tlsLegacySupport = false)
            : base(backtraceCredentials, attributes,
                  backtraceDatabase, reportPerMin, tlsLegacySupport)
        { }
        #endregion

        #region Send synchronous
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
            return Send(new BacktraceReport(exception, attributes, attachmentPaths));
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
            var result = base.Send(backtraceReport);
            AfterSend?.Invoke(result);
            return result;
        }
        #endregion

#if !NET35
        #region Send asynchronous
        /// <summary>
        /// Sending asynchronous Backtrace report to Backtrace API
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        /// <returns>Server response</returns>
        public async Task<BacktraceResult> SendAsync(BacktraceReport backtraceReport)
        {
            OnReportStart?.Invoke(backtraceReport);
            var result = await base.SendAsync(backtraceReport);
            AfterSend?.Invoke(result);

           
            return result;
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
            return await SendAsync(new BacktraceReport(exception, attributes, attachmentPaths));
        }
        #endregion
#endif



       
    }
}
