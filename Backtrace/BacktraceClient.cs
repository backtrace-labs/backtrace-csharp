using Backtrace.Base;
using Backtrace.Interfaces;
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
    public class BacktraceClient : Backtrace<string>, IBacktraceClient
    {
        /// <summary>
        /// Backtrace Credentials information
        /// </summary>
        private readonly BacktraceCredentials _backtraceCredentials;

        /// <summary>
        /// Initialize client with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerSec">Numbers of records senden per one sec.</param>
        public BacktraceClient(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, string> attributes = null,
            string databaseDirectory = "",
            int reportPerSec = 3
            )
        {
            _backtraceCredentials = backtraceCredentials;
            _attributes = attributes ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Initialize Backtrace report client
        /// </summary>
        /// <param name="sectionName">Backtrace configuration section in App.config or Web.config file. Default section is BacktraceCredentials</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Database path</param>
        /// <param name="reportPerSec">Numbers of records senden per one sec.</param>
        public BacktraceClient(
                string sectionName = "BacktraceCredentials",
                Dictionary<string, string> attributes = null,
                string databaseDirectory = "",
                int reportPerSec = 3
            )
            :this(BacktraceCredentials.ReadConfigurationSection(sectionName), attributes,databaseDirectory, reportPerSec)
        {
            
        }

        /// <summary>
        /// Send a report to Backtrace
        /// </summary>
        /// <param name="report">Report to send</param>
        public void Send(BacktraceReport report)
        {
            throw new NotImplementedException();
        }
    }
}
