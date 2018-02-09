using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backtrace.Services
{
    /// <summary>
    /// Backtrace Database 
    /// Before start: Be sure that used directory is empty!
    /// </summary>
    internal class BacktraceDatabase
    {
        /// <summary>
        /// Path to a database directory
        /// </summary>
        private readonly string _directoryPath;

        /// <summary>
        /// Create Backtrace database instance
        /// </summary>
        /// <param name="directoryPath">Directory where library store application data</param>
        public BacktraceDatabase(string directoryPath)
        {
            _directoryPath = directoryPath;
            ValidateDatabaseDirectory();
        }

        /// <summary>
        /// Check if used directory database is available 
        /// </summary>
        private void ValidateDatabaseDirectory()
        {
            //there is no database directory
            if (string.IsNullOrEmpty(_directoryPath))
            {
                return;
            }
            //check if directory exists 
            if (!System.IO.Directory.Exists(_directoryPath))
            {
                throw new ArgumentException("databasePath");
            }

            //check if directory is empty
            if (System.IO.Directory.GetFiles(_directoryPath).Any())
            {
                throw new InvalidOperationException("Database directory should be empty before Backtrace library start");
            }

        }
    }
}
