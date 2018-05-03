using Backtrace.Interfaces;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// All properties required by BacktraceClient in one place
    /// </summary>
    public class BacktraceClientConfiguration
    {
        /// <summary>
        /// Client credentials
        /// </summary>
        public readonly BacktraceCredentials Credentials;

        public readonly IBacktraceDatabase<object> Database;
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
        /// <param name="credentials">Backtrace server API credentials</param>
        public BacktraceClientConfiguration(BacktraceCredentials credentials)
            : this(credentials, new BacktraceDatabaseSettings(string.Empty))
        { }

        /// <summary>
        /// Create new client settings with database 
        /// </summary>
        /// <param name="credentials">Backtrace server API credentials</param>
        /// <param name="databasePath">Path to database directory</param>
        public BacktraceClientConfiguration(BacktraceCredentials credentials, string databasePath)
            : this(credentials, new BacktraceDatabaseSettings(databasePath))
        { }

        /// <summary>
        /// Create new client settings with database 
        /// </summary>
        /// <param name="credentials">Backtrace server API credentials</param>
        /// <param name="databaseSettings">Database settings</param>
        public BacktraceClientConfiguration(BacktraceCredentials credentials, BacktraceDatabaseSettings databaseSettings)
        {
            Credentials = credentials;
            Database = new BacktraceDatabase<object>(databaseSettings);
            DatabaseSettings = databaseSettings;
        }

        /// <summary>
        /// Create new client settings with database 
        /// </summary>
        /// <param name="credentials">Backtrace server API credentials</param>
        /// <param name="database">Database</param>
        public BacktraceClientConfiguration(BacktraceCredentials credentials, IBacktraceDatabase<object> database)
        {
            Credentials = credentials;
            Database = database;
            DatabaseSettings = database.GetSettings();
        }
    }
}
