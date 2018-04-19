using Backtrace.Base;
using Backtrace.Common;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace
{
    /// <summary>
    /// Backtrace Database 
    /// </summary>
    public class BacktraceDatabase<T> : IBacktraceDatabase<T>
    {
        /// <summary>
        /// Database settings
        /// </summary>
        public BacktraceDatabaseSettings DatabaseSettings { get; private set; }

        internal IBacktraceDatabaseContext<T> BacktraceDatabaseContext { get; set; }
        /// <summary>
        /// Database path
        /// </summary>
        private string DatabasePath
        {
            get
            {
                return DatabaseSettings.DatabasePath;
            }
        }

        /// <summary>
        /// Backtrace Api instance. Use BacktraceApi to send data to Backtrace server
        /// </summary>
        private IBacktraceApi<T> _backtraceApi;

        /// <summary>
        /// Determine if BacktraceDatabase is enable and library can store reports
        /// </summary>
        private readonly bool _enable = true;

        /// <summary>
        /// Create disabled instance of BacktraceDatabase
        /// </summary>
        /// <param name="databaseSettings"></param>
        public BacktraceDatabase()
        {
            _enable = false;
        }

        /// <summary>
        /// Create new Backtrace database instance
        /// </summary>
        /// <param name="path">Path to database directory</param>
        public BacktraceDatabase(string path)
            : this(new BacktraceDatabaseSettings(path))
        { }

        /// <summary>
        /// Create Backtrace database instance
        /// </summary>
        /// <param name="databaseSettings">Backtrace database settings</param>
        public BacktraceDatabase(BacktraceDatabaseSettings databaseSettings)
        {
            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.DatabasePath))
            {
                _enable = false;
                return;
            }
            if (!Directory.Exists(databaseSettings.DatabasePath))
            {
                throw new ArgumentException("Databse path does not exists");
            }

            DatabaseSettings = databaseSettings;
            BacktraceDatabaseContext = new BacktraceDatabaseContext<T>(DatabaseSettings.DatabasePath, DatabaseSettings.TotalRetry);
            RemoveOrphaned();
            LoadReports();
        }

        /// <summary>
        /// Set BacktraceApi instance
        /// </summary>
        /// <param name="backtraceApi">BacktraceApi instance</param>
        public void SetApi(IBacktraceApi<T> backtraceApi)
        {
            _backtraceApi = backtraceApi;
        }

        /// <summary>
        /// Delete all existing files and directories in current database directory
        /// </summary>
        public void Clear()
        {
            if (!_enable)
            {
                return;
            }
            BacktraceDatabaseContext.Clear();
        }

        public void Flush()
        {
            if (!_enable)
            {
                return;
            }
            if (_backtraceApi == null)
            {
                throw new ArgumentException("BacktraceApi is required if you want to use Flush method");
            }
            var entry = BacktraceDatabaseContext.FirstOrDefault();
            while (entry != null)
            {
                var backtraceData = entry.BacktraceData;
                _backtraceApi.Send(backtraceData);
                Delete(entry);
                entry = BacktraceDatabaseContext.FirstOrDefault();
            }
        }
#if !NET35
        public async Task FlushAsync()
        {
            if (!_enable)
            {
                return;
            }
            if (_backtraceApi == null)
            {
                throw new ArgumentException("BacktraceApi is required if you want to use Flush method");
            }
            var entry = BacktraceDatabaseContext.FirstOrDefault();
            while (entry != null)
            {
                var backtraceData = entry.BacktraceData;
                await _backtraceApi.SendAsync(backtraceData);
                Delete(entry);
                entry = BacktraceDatabaseContext.FirstOrDefault();
            }
        }
#endif

        /// <summary>
        /// Create new minidump file in database directory path. Minidump file name is a random Guid
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        /// <param name="miniDumpType">Generated minidump type</param>
        /// <returns>Path to minidump file</returns>
        private string GenerateMiniDump(BacktraceReportBase<T> backtraceReport, MiniDumpType miniDumpType)
        {
            //note that every minidump file generated by app ends with .dmp extension
            //its important information if you want to clear minidump file
            string minidumpDestinationPath = Path.Combine(DatabaseSettings.DatabasePath, $"{Guid.NewGuid()}.dmp");
            MinidumpException minidumpExceptionType = backtraceReport.ExceptionTypeReport
                ? MinidumpException.Present
                : MinidumpException.None;

            bool minidumpSaved = MinidumpHelper.Write(
                filePath: minidumpDestinationPath,
                options: miniDumpType,
                exceptionType: minidumpExceptionType);

            return minidumpSaved
                ? minidumpDestinationPath
                : string.Empty;
        }

        /// <summary>
        /// Add new report to BacktraceDatabase
        /// </summary>
        public BacktraceDatabaseEntry<T> Add(BacktraceReportBase<T> backtraceReport, Dictionary<string, T> attributes, MiniDumpType miniDumpType = MiniDumpType.Normal)
        {
            if (!_enable)
            {
                return null;
            }
            if (BacktraceDatabaseContext.Count() + 1 > DatabaseSettings.MaxEntryNumber && DatabaseSettings.MaxEntryNumber != 0)
            {
                throw new ArgumentException("Maximum number of entries available in BacktraceDatabase");
            }

            string minidumpPath = GenerateMiniDump(backtraceReport, miniDumpType);
            if (!string.IsNullOrEmpty(minidumpPath))
            {
                backtraceReport.SetMinidumpPath(minidumpPath);
            }

            var data = backtraceReport.ToBacktraceData(attributes);
            return BacktraceDatabaseContext.Add(data);
        }


        /// <summary>
        /// Get all stored reports in BacktraceDatabase
        /// </summary>
        /// <returns>All stored reports in BacktraceDatabase</returns>
        public IEnumerable<BacktraceDatabaseEntry<T>> Get()
        {
            return BacktraceDatabaseContext.Get();
        }

        public void Delete(BacktraceDatabaseEntry<T> entry)
        {
            if (entry == null)
            {
                return;
            }
            BacktraceDatabaseContext.Delete(entry);
        }
        /// <summary>
        /// Detect all orphaned minidump files
        /// </summary>
        private void RemoveOrphaned()
        {
            //throw new NotImplementedException();
        }

        private void LoadReports()
        {
            var directoryInfo = new DirectoryInfo(DatabasePath);
            var files = directoryInfo.GetFiles($"_entry*.json", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                using (StreamReader streamReader = file.OpenText())
                {
                    var json = streamReader.ReadToEnd();
                    var entry = JsonConvert.DeserializeObject<BacktraceDatabaseEntry<T>>(json);
                    BacktraceDatabaseContext.Add(entry);
                }
            }
        }
    }
}
