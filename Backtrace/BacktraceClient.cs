using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Backtrace
{
    /// <summary>
    /// Backtrace .NET Client 
    /// </summary>
    public class BacktraceClient : Backtrace<object>, IBacktraceClient<object>
    {     
        /// <summary>
        /// Set a event executed before data send to Backtrace API
        /// </summary>
        public Action<BacktraceReport<object>> BeforeSend = null;

        /// <summary>
        /// Set a event executed after data send to Backtrace API
        /// </summary>
        public Action<BacktraceReport<object>> AfterSend;

        /// <summary>
        /// Initialize Backtrace report client
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerSec">Numbers of records senden per one sec.</param>
        public BacktraceClient(
                string sectionName = "BacktraceCredentials",
                Dictionary<string, object> attributes = null,
                string databaseDirectory = "",
                int reportPerSec = 3
            )
            : base(BacktraceCredentials.ReadConfigurationSection(sectionName),
                  attributes, databaseDirectory, reportPerSec)
        {
        }

        /// <summary>
        /// Initialize client with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerSec">Numbers of records senden per one sec.</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, object> attributes = null,
            string databaseDirectory = "",
            int reportPerSec = 3)
            : base(backtraceCredentials, attributes, databaseDirectory, reportPerSec)
        { }

        /// <summary>
        /// Send a backtrace report to Backtrace API
        /// </summary>
        /// <param name="backtraceReport">report</param>
        public new void Send(BacktraceReport<object> backtraceReport)
        {
            BeforeSend?.Invoke(backtraceReport);

            base.Send(backtraceReport);

            AfterSend?.Invoke(backtraceReport);
        }

        /// <summary>
        /// Send an exception to Backtrace API
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <param name="attributes">Additional information about application state</param>
        public virtual void Send(
            Exception exception,
            Dictionary<string, object> attributes = null)
        {
            Send(new BacktraceReport<object>(exception, attributes));
        }
        /// <summary>
        /// Send a message to Backtrace API
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="attributes">Additional information about application state</param>
        public virtual void Send(
            string message,
            Dictionary<string, object> attributes = null)
        {
            Send(new BacktraceReport<object>(message, attributes));
        }
    }
}
