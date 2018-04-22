using Backtrace.Base;
using Backtrace.Common;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
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
        internal IBacktraceDatabaseContext<T> BacktraceDatabaseContext { get; set; }

        /// <summary>
        /// Database settings
        /// </summary>
        private BacktraceDatabaseSettings DatabaseSettings { get; set; }

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

        private readonly Timer _timer = new Timer();

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
            var directory = new DirectoryInfo(DatabasePath);

            BacktraceDatabaseContext = new BacktraceDatabaseContext<T>(DatabasePath, DatabaseSettings.TotalRetry);
            RemoveOrphaned();
            LoadReports();
            SetupTimer();
        }

        private void SetupTimer()
        {
            // timer require time in ms
            _timer.Interval = DatabaseSettings.RetryTime * 1000;
            // don't stop timer work
            _timer.AutoReset = true;
#if NET35
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
#else
            _timer.Elapsed += new ElapsedEventHandler(OnTimedEventAsync);
#endif
            _timer.Enabled = true;

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
            if (miniDumpType != MiniDumpType.None)
            {
                string minidumpPath = GenerateMiniDump(backtraceReport, miniDumpType);
                if (!string.IsNullOrEmpty(minidumpPath))
                {
                    backtraceReport.SetMinidumpPath(minidumpPath);
                }
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
            BacktraceDatabaseContext.Delete(entry);
        }

        /// <summary>
        /// Send and delete all entries from database
        /// </summary>
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
                var result = _backtraceApi.Send(backtraceData);
                Delete(entry);
                entry = BacktraceDatabaseContext.FirstOrDefault();
            }
        }
#if !NET35
        /// <summary>
        /// Send and asynchronous delete all entries from database
        /// </summary>
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

        private async void OnTimedEventAsync(object source, ElapsedEventArgs e)
        {
            if (BacktraceDatabaseContext.Count() == 0)
            {
                return;
            }

            _timer.Stop();
            var entry = BacktraceDatabaseContext.FirstOrDefault();
            while (entry != null)
            {
                var backtraceData = entry.BacktraceData;
                var result = await _backtraceApi.SendAsync(backtraceData);
                if (result.Status == BacktraceResultStatus.Ok)
                {
                    Delete(entry);
                    entry = BacktraceDatabaseContext.FirstOrDefault();
                }
                else
                {
                    BacktraceDatabaseContext.MoveNext();
                    break;
                }
            }
            _timer.Start();
        }
#endif
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (BacktraceDatabaseContext.Count() == 0)
            {
                return;
            }
            _timer.Stop();
            var entry = BacktraceDatabaseContext.FirstOrDefault();
            while (entry != null)
            {
                var backtraceData = entry.BacktraceData;
                var result = _backtraceApi.Send(backtraceData);
                if (result.Status == BacktraceResultStatus.Ok)
                {
                    Delete(entry);
                    entry = BacktraceDatabaseContext.FirstOrDefault();
                }
                else
                {
                    BacktraceDatabaseContext.MoveNext();
                    entry.Dispose();
                    return;
                }
            }
            _timer.Start();
        }

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
            string minidumpDestinationPath = Path.Combine(DatabaseSettings.DatabasePath, $"{backtraceReport.Uuid}.dmp");
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



        internal int Count()
        {
            return BacktraceDatabaseContext.Count();
        }

        /// <summary>
        /// Detect all orp  haned minidump files
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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backtraceApi?.Dispose();
                _timer?.Dispose();
            }
        }
    }
}
