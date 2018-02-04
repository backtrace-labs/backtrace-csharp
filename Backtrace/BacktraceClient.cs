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
    public class BacktraceClient : IBacktraceClient
    {
        /// <summary>
        /// Backtrace Credentials information
        /// </summary>
        private readonly BacktraceCredentials _backtraceCredentials;

        /// <summary>
        /// Client attributes
        /// </summary>
        private Dictionary<string, string> _attributes;

        /// <summary>
        /// Get scoped attributes from Backtrace client. Every argument stored in dictionary will be send to a Backtrace service
        /// </summary>
        public Dictionary<string, string> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Initialize client with Backtrace host Uri 
        /// </summary>
        /// <param name="backtraceCredentials">Uri to Backtrace host</param>
        /// <param name="attributes">Attributes scoped for every report</param>
        /// <param name="databaseDirectory">Store path for minidump</param>
        /// <param name="reportPerSec">Numbers of records senden per one sec. If number is equal to zero there is senden </param>
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
        /// Send a report to Backtrace
        /// </summary>
        /// <param name="report">Report to send</param>
        public void Send(BacktraceReport report)
        {
            throw new NotImplementedException();
        }
    }
}
