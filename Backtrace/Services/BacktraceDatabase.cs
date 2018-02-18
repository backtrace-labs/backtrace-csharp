using Backtrace.Common;
using Backtrace.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace Backtrace.Services
{
    /// <summary>
    /// Backtrace Database 
    /// Before start: Be sure that used directory is empty!
    /// </summary>
    internal class BacktraceDatabase<T>
    {
        /// <summary>
        /// Path to a database directory
        /// </summary>
        private readonly string _directoryPath;
        private readonly bool _enable;
        /// <summary>
        /// Create Backtrace database instance
        /// </summary>
        /// <param name="directoryPath">Directory where library store application data</param>
        public BacktraceDatabase(string directoryPath)
        {
            _directoryPath = directoryPath;
            ValidateDatabaseDirectory();
            _enable = !string.IsNullOrEmpty(_directoryPath);
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
            if (!Directory.Exists(_directoryPath))
            {
                throw new ArgumentException("databasePath");
            }

            //check if directory is empty
            //if (System.IO.Directory.GetFiles(_directoryPath).Any())
            //{
            //    throw new InvalidOperationException("Database directory should be empty before Backtrace library start");
            //}
        }

        /// <summary>
        /// Create new minidump file in database directory path. Minidump file name is a random Guid
        /// </summary>
        public void GenerateMiniDump()
        {
            string minidumpDestinationPath = Path.Combine(_directoryPath, $"{Guid.NewGuid()}.dmp");
            MinidumpHelper.Write(minidumpDestinationPath);
        }

        /// <summary>
        /// Save diagnostic report on disc
        /// </summary>
        /// <param name="backtraceReport"></param>
        /// <returns></returns>
        public bool SaveReport(BacktraceData<T> backtraceReport)
        {
            if (!_enable)
            {
                return true;
            }

            string json = JsonConvert.SerializeObject(backtraceReport);
            byte[] file = Encoding.UTF8.GetBytes(json);
            string filename = $"Backtrace_{backtraceReport.Timestamp}";
            string filePath = Path.Combine(_directoryPath, filename);
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(file, 0, file.Length);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
