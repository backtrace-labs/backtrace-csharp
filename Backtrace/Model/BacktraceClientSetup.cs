using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// All properties required by BacktraceClient in one place
    /// </summary>
    public class BacktraceClientSetup
    {
        /// <summary>
        /// Client credentials
        /// </summary>
        public readonly BacktraceCredentials Credentials;

        /// <summary>
        /// Database settings 
        /// </summary>
        public BacktraceDatabaseSettings DatabaseSettings { get; set; } = null;

        /// <summary>
        /// Client's attributes
        /// </summary>
        public Dictionary<string, object> ClientAttributes { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Numbers of records sending per one minute
        /// </summary>
        public uint ReportPerMin { get; set; } = 3;

        /// <summary>
        /// Set SSL and TLS flags for https request to Backtrace API
        /// </summary>
        public bool TlsLegacySupport { get; set; } = false;

        /// <summary>
        /// Create new client settings with disabled database
        /// </summary>
        /// <param name="credentials"></param>
        public BacktraceClientSetup(BacktraceCredentials credentials)
            : this(credentials, string.Empty)
        { }

        /// <summary>
        /// Create new client settings with database 
        /// </summary>
        /// <param name="credentials">Backtrace server API credentials</param>
        /// <param name="databasePath">Path to database directory</param>
        public BacktraceClientSetup(BacktraceCredentials credentials, string databasePath)
        {
            Credentials = credentials;
            DatabaseSettings = new BacktraceDatabaseSettings(databasePath);
        }
    }
}
