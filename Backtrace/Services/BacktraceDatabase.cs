using Backtrace.Base;
using Backtrace.Common;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
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
namespace Backtrace.Services
{
    /// <summary>
    /// Backtrace Database 
    /// </summary>
    public class BacktraceDatabase<T> : IBacktraceDatabase<T>
    {
        /// <summary>
        /// In memory database
        /// </summary>
        Dictionary<uint, List<BacktraceDatabaseEntry<T>>> BatchRetry = new Dictionary<uint, List<BacktraceDatabaseEntry<T>>>();
        private long _totalEntries = 0;

        /// <summary>
        /// Database settings
        /// </summary>
        public BacktraceDatabaseSettings DatabaseSettings { get; private set; }

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

        private const string reportPrefix = "attachment_";

        /// <summary>
        /// Create disabled instance of BacktraceDatabase
        /// </summary>
        /// <param name="databaseSettings"></param>
        public BacktraceDatabase()
        {
            _enable = false;
        }

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
            SetupBatch();
            RemoveOrphaned();
            LoadReports();
        }

        private void SetupBatch()
        {
            for (uint i = 0; i < DatabaseSettings.TotalRetry; i++)
            {
                BatchRetry[i] = new List<BacktraceDatabaseEntry<T>>();
            }
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
            var directoryInfo = new DirectoryInfo(DatabasePath);
            IEnumerable<FileInfo> files = directoryInfo.GetFiles();
            IEnumerable<DirectoryInfo> directories = directoryInfo.GetDirectories();

            foreach (FileInfo file in files)
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directories)
            {
                dir.Delete(true);
            }
            BatchRetry.Clear();
        }

        public void Flush()
        {
            if (_backtraceApi == null)
            {
                throw new ArgumentException("BacktraceApi is required if you want to use Flush method");
            }
            foreach (var batch in BatchRetry)
            {
                foreach (var databaseEntry in batch.Value)
                {
                    //_backtraceApi.Send()
                    Delete(databaseEntry);
                }
            }
        }
#if !NET35
        public async Task FlushAsync()
        {
            if (_backtraceApi == null)
            {
                throw new ArgumentException("BacktraceApi is required if you want to use Flush method");
            }
            foreach (var batch in BatchRetry)
            {
                foreach (var databaseEntry in batch.Value)
                {
                    //_backtraceApi.SendAsync()
                    Delete(databaseEntry);
                }
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
            if (!_enable)
            {
                return string.Empty;
            }
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
            if (_totalEntries + 1 > DatabaseSettings.MaxEntryNumber && DatabaseSettings.MaxEntryNumber != 0)
            {
                throw new ArgumentException("Maximum number of entries available in BacktraceDatabase");
            }

            string minidumpPath = GenerateMiniDump(backtraceReport, miniDumpType);
            if (!string.IsNullOrEmpty(minidumpPath))
            {
                backtraceReport.SetMinidumpPath(minidumpPath);
            }

            var data = backtraceReport.ToBacktraceData(attributes);
            var saveResult = new BacktraceDatabaseEntry<T>(data, DatabasePath);
            if (saveResult == null)
            {
                return null;
            }
            BatchRetry[0].Add(saveResult);
            _totalEntries++;
            return saveResult;
        }


        /// <summary>
        /// Get all stored reports in BacktraceDatabase
        /// </summary>
        /// <returns>All stored reports in BacktraceDatabase</returns>
        public IEnumerable<BacktraceDatabaseEntry<T>> Get()
        {
            return BatchRetry.SelectMany(n => n.Value);
        }

        public void Delete(BacktraceDatabaseEntry<T> entry)
        {
            if (entry == null)
            {
                return;
            }
            entry.Delete();
        }

        /// <summary>
        /// Dispose BacktraceDatabase
        /// </summary>
        public void Dispose()
        {
            //dispose database
            throw new NotImplementedException();
            RemoveOrphaned();
        }

        //THIS IS OUT OF SCOPE
        /// <summary>
        /// Get a report by using specific filter
        /// </summary>
        /// <param name="delegate">Report filter</param>
        /// <returns>Stored reports that match filter criteria</returns>
        public IEnumerable<BacktraceReportBase<T>> Get(Func<BacktraceReportBase<T>, IEnumerable<BacktraceReportBase<T>>> @delegate)
        {
            throw new NotImplementedException();
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
            IEnumerable<FileInfo> files = directoryInfo.GetFiles($"_entry*.json", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                using (StreamReader streamReader = file.OpenText())
                {
                    var json = streamReader.ReadToEnd();
                    var entry = JsonConvert.DeserializeObject<BacktraceDatabaseEntry<T>>(json);
                    var diagnosticJson = entry.JsonReport;

                    var backtraceData = JsonConvert.DeserializeObject<BacktraceData<object>>(diagnosticJson);

                    BatchRetry[0].Add(entry);
                    _totalEntries++;
                }
            }
        }
    }
}
