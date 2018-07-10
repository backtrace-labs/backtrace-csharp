﻿using Backtrace.Base;
using Backtrace.Common;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Services;
using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Diagnostics;
#if !NET35
using System.Threading.Tasks;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace
{
    /// <summary>
    /// Backtrace Database 
    /// </summary>
    public class BacktraceDatabase : IBacktraceDatabase
    {
        /// <summary>
        /// Backtrace Api instance. Use BacktraceApi to send data to Backtrace server
        /// </summary>
        public IBacktraceApi BacktraceApi { get; set; }

        /// <summary>
        /// Database context - in memory cache and record operations
        /// </summary>
        internal IBacktraceDatabaseContext BacktraceDatabaseContext { get; set; }

        /// <summary>
        /// File context - file collection operations
        /// </summary>
        internal IBacktraceDatabaseFileContext BacktraceDatabaseFileContext { get; set; }

        /// <summary>
        /// Database settings
        /// </summary>
        private BacktraceDatabaseSettings DatabaseSettings { get; set; }

        /// <summary>
        /// Database path
        /// </summary>
        private string DatabasePath => DatabaseSettings.DatabasePath;

        private bool _timerBackgroundWork = false;
        /// <summary>
        /// Determine if BacktraceDatabase is enable and library can store reports
        /// </summary>
        private bool _enable = false;

        private readonly Timer _timer = new Timer();

        /// <summary>
        /// Create disabled instance of BacktraceDatabase
        /// </summary>
        /// <param name="databaseSettings"></param>
        public BacktraceDatabase()
        { }

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
                return;
            }
            if (!Directory.Exists(databaseSettings.DatabasePath))
            {
                throw new ArgumentException("Databse path does not exists");
            }
            DatabaseSettings = databaseSettings;
            BacktraceDatabaseContext = new BacktraceDatabaseContext(DatabasePath, DatabaseSettings.RetryLimit, DatabaseSettings.RetryOrder);
            BacktraceDatabaseFileContext = new BacktraceDatabaseFileContext(DatabasePath, DatabaseSettings.MaxDatabaseSize, DatabaseSettings.MaxRecordCount);
        }

        /// <summary>
        /// Start database tasks
        /// </summary>
        public void Start()
        {
            //database not exists
            if (DatabaseSettings == null)
            {
                return;
            }
            if (BacktraceDatabaseContext?.Any() == true)
            {
                _enable = true;
                return;
            }
            // load reports from hard drive
            LoadReports();
            // remove orphaned files
            RemoveOrphaned();
            // setup database timer events
            if (DatabaseSettings.RetryBehavior == RetryBehavior.ByInterval
                || !DatabaseSettings.AutoSendMode)
            {
                SetupTimer();
            }
            //Enable database
            _enable = true;
        }

        private void SetupTimer()
        {
            // timer require time in ms
            _timer.Interval = DatabaseSettings.RetryInterval * 1000;
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
        public void SetApi(IBacktraceApi backtraceApi)
        {
            BacktraceApi = backtraceApi;
        }

        /// <summary>
        /// Get settings 
        /// </summary>
        /// <returns>Current database settings</returns>
        public BacktraceDatabaseSettings GetSettings()
        {
            return DatabaseSettings;
        }


        /// <summary>
        /// Delete all existing files and directories in current database directory
        /// </summary>
        public void Clear()
        {
            BacktraceDatabaseContext?.Clear();
            BacktraceDatabaseFileContext?.Clear();
        }

        /// <summary>
        /// Add new report to BacktraceDatabase
        /// </summary>
        public BacktraceDatabaseRecord Add(BacktraceReport backtraceReport, Dictionary<string, object> attributes, MiniDumpType miniDumpType = MiniDumpType.Normal)
        {
            if (!_enable)
            {
                return null;
            }
            if (BacktraceDatabaseContext.Count() + 1 > DatabaseSettings.MaxRecordCount && DatabaseSettings.MaxRecordCount != 0)
            {
                throw new ArgumentException("Maximum number of records available in BacktraceDatabase");
            }
            if (BacktraceDatabaseContext.GetSize() > DatabaseSettings.MaxDatabaseSize && DatabaseSettings.MaxDatabaseSize != 0)
            {
                throw new ArgumentException("You don't have enought space in database for more records");
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
        /// Get all stored records in BacktraceDatabase
        /// </summary>
        /// <returns>All stored records in BacktraceDatabase</returns>
        public IEnumerable<BacktraceDatabaseRecord> Get() => BacktraceDatabaseContext?.Get() ?? new List<BacktraceDatabaseRecord>();

        /// <summary>
        /// Delete single record from database
        /// </summary>
        /// <param name="record">Record to delete</param>
        public void Delete(BacktraceDatabaseRecord record) => BacktraceDatabaseContext?.Delete(record);

        /// <summary>
        /// Send and delete all records from database
        /// </summary>
        public void Flush()
        {
            if (BacktraceApi == null)
            {
                throw new ArgumentException("BacktraceApi is required if you want to use Flush method");
            }
            var record = BacktraceDatabaseContext?.FirstOrDefault();
            while (record != null)
            {
                var backtraceData = record.BacktraceData;
                Delete(record);
                record = BacktraceDatabaseContext.FirstOrDefault();
                if (backtraceData != null)
                {
                    BacktraceApi.Send(backtraceData);
                }
            }
        }
#if !NET35
        /// <summary>
        /// Send and asynchronous delete all records from database
        /// </summary>
        public async Task FlushAsync()
        {
            if (BacktraceApi == null)
            {
                throw new ArgumentException("BacktraceApi is required if you want to use Flush method");
            }
            var record = BacktraceDatabaseContext?.FirstOrDefault();
            while (record != null)
            {
                var backtraceData = record.BacktraceData;
                Delete(record);
                record = BacktraceDatabaseContext.FirstOrDefault();
                if (backtraceData != null)
                {
                    await BacktraceApi.SendAsync(backtraceData);
                }
            }
        }

        private async void OnTimedEventAsync(object source, ElapsedEventArgs e)
        {
            if (!BacktraceDatabaseContext.Any() || _timerBackgroundWork) return;
            _timerBackgroundWork = true;
            _timer.Stop();
            //read first record (keep in mind LIFO and FIFO settings) from memory database
            var record = BacktraceDatabaseContext.FirstOrDefault();
            while (record != null)
            {
                var backtraceData = record.BacktraceData;
                //meanwhile someone delete data from a disk
                if (backtraceData == null || backtraceData.Report == null)
                {
                    Delete(record);
                }
                else
                {
                    //send record from database to API
                    var result = await BacktraceApi.SendAsync(backtraceData);
                    if (result.Status == BacktraceResultStatus.Ok)
                    {
                        Delete(record);
                    }
                    else
                    {
                        record.Dispose();
                        BacktraceDatabaseContext.IncrementBatchRetry();
                        break;
                    }

                }
                record = BacktraceDatabaseContext.FirstOrDefault();
            }
            _timer.Start();
            _timerBackgroundWork = false;
        }
#endif
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            if (!BacktraceDatabaseContext.Any() || _timerBackgroundWork) return;
            _timerBackgroundWork = true;
            _timer.Stop();
            //read first record (keep in mind LIFO and FIFO settings) from memory database
            var record = BacktraceDatabaseContext.FirstOrDefault();
            while (record != null)
            {
                var backtraceData = record.BacktraceData;
                //meanwhile someone delete data from a disk
                if (backtraceData == null || backtraceData.Report == null)
                {
                    Delete(record);
                }
                else
                {
                    //send record from database to API
                    var result = BacktraceApi.Send(backtraceData);
                    if (result.Status == BacktraceResultStatus.Ok)
                    {
                        Delete(record);
                    }
                    else
                    {
                        record.Dispose();
                        BacktraceDatabaseContext.IncrementBatchRetry();
                        break;

                    }
                }
                record = BacktraceDatabaseContext.FirstOrDefault();
            }
            _timerBackgroundWork = false;
            _timer.Start();

        }

        /// <summary>
        /// Create new minidump file in database directory path. Minidump file name is a random Guid
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        /// <param name="miniDumpType">Generated minidump type</param>
        /// <returns>Path to minidump file</returns>
        private string GenerateMiniDump(BacktraceReport backtraceReport, MiniDumpType miniDumpType)
        {
            //note that every minidump file generated by app ends with .dmp extension
            //its important information if you want to clear minidump file
            string minidumpDestinationPath = Path.Combine(DatabaseSettings.DatabasePath, $"{backtraceReport.Uuid}-dump.dmp");
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
        /// Get total number of records in database
        /// </summary>
        /// <returns>Total number of records</returns>
        internal int Count() => BacktraceDatabaseContext.Count();

        /// <summary>
        /// Detect all orphaned minidump and files
        /// </summary>
        private void RemoveOrphaned()
        {
            var records = BacktraceDatabaseContext.Get();
            BacktraceDatabaseFileContext.RemoveOrphaned(records);
        }

        /// <summary>
        /// Load all records stored in database path
        /// </summary>
        private void LoadReports()
        {
            var files = BacktraceDatabaseFileContext.GetRecords();
            foreach (var file in files)
            {
                var record = BacktraceDatabaseRecord.ReadFromFile(file);
                if (!record.Valid())
                {
                    record.Delete();
                    continue;
                }
                BacktraceDatabaseContext.Add(record);
                if (!ValidDatabaseSize())
                {
                    throw new ArgumentException("Database directory has too many records or database size is bigger than in option declaration.");
                }
                record.Dispose();
            }
        }
        /// <summary>
        /// Check current size of database
        /// </summary>
        /// <returns>False if BacktraceDatabase doesn't have more free space</returns>
        private bool ValidDatabaseSize()
        {
            return ((BacktraceDatabaseContext.GetSize() < DatabaseSettings.MaxDatabaseSize || DatabaseSettings.MaxDatabaseSize == 0)
                || (BacktraceDatabaseContext.GetTotalNumberOfRecords() < DatabaseSettings.MaxRecordCount || DatabaseSettings.MaxDatabaseSize == 0));
        }

        /// <summary>
        /// Valid database consistency requirements
        /// </summary>
        public bool ValidConsistency()
        {
            return BacktraceDatabaseFileContext.ValidFileConsistency();
        }


        #region dispose
        private bool _disposed = false; // To detect redundant calls

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
#if !NET35
                    BacktraceApi?.Dispose();
                    _timer?.Dispose();
#endif
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Get database size
        /// </summary>
        /// <returns></returns>
        public long GetDatabaseSize()
        {
            return BacktraceDatabaseContext.GetSize();
        }

        ~BacktraceDatabase()
        {
            Dispose(false);
        }
        #endregion
    }
}
