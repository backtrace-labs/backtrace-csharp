using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Backtrace.Services
{
    /// <summary>
    /// Backtrace Database Context
    /// </summary>
    internal class BacktraceDatabaseContext : IBacktraceDatabaseContext
    {
        /// <summary>
        /// Database cache
        /// </summary>
        internal Dictionary<int, List<BacktraceDatabaseEntry>> BatchRetry = new Dictionary<int, List<BacktraceDatabaseEntry>>();

        /// <summary>
        /// Total entries in BacktraceDatabase
        /// </summary>
        internal int TotalEntries = 0;

        /// <summary>
        /// Path to database directory 
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Maximum number of retries
        /// </summary>
        private readonly int _retryNumber;

        /// <summary>
        /// Entry order
        /// </summary>
        internal RetryOrder RetryOrder { get; set; }

        /// <summary>
        /// Initialize new instance of Backtrace Database Context
        /// </summary>
        /// <param name="path">Path to database directory</param>
        /// <param name="retryNumber">Total number of retries</param>
        /// <param name="retryOrder">Entry order</param>
        public BacktraceDatabaseContext(string path, uint retryNumber, RetryOrder retryOrder)
        {
            _path = path;
            _retryNumber = checked((int)retryNumber);
            RetryOrder = retryOrder;
            SetupBatch();
        }

        /// <summary>
        /// Setup cache 
        /// </summary>
        private void SetupBatch()
        {
            if (_retryNumber == 0)
            {
                throw new ArgumentException($"{nameof(_retryNumber)} have to be greater than 0!");
            }
            for (int i = 0; i < _retryNumber; i++)
            {
                BatchRetry[i] = new List<BacktraceDatabaseEntry>();
            }
        }

        /// <summary>
        /// Add new entry to database
        /// </summary>
        /// <param name="backtraceData">Diagnostic data that should be stored in database</param>
        /// <returns>New instance of DatabaseEntry</returns>
        public virtual BacktraceDatabaseEntry Add(BacktraceData backtraceData)
        {
            if (backtraceData == null) throw new NullReferenceException(nameof(BacktraceData));
            var entry = new BacktraceDatabaseEntry(backtraceData, _path);
            entry.Save();
            return Add(entry);
        }

        /// <summary>
        /// Add existing entry to database
        /// </summary>
        /// <param name="backtraceEntry">Database entry</param>
        public BacktraceDatabaseEntry Add(BacktraceDatabaseEntry backtraceEntry)
        {
            if (backtraceEntry == null) throw new NullReferenceException(nameof(BacktraceDatabaseEntry));
            backtraceEntry.Locked = true;
            BatchRetry[0].Add(backtraceEntry);
            TotalEntries++;
            return backtraceEntry;
        }

        /// <summary>
        /// Check if any entry exists
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool Any(BacktraceDatabaseEntry entry)
        {
            return BatchRetry.SelectMany(n => n.Value).Any(n => n.Id == entry.Id);
        }

        /// <summary>
        /// Check if any entry exists
        /// </summary>
        public bool Any()
        {
            return TotalEntries != 0;
        }

        /// <summary>
        /// Delete existing entry from database
        /// </summary>
        /// <param name="entry">Database entry to delete</param>
        public virtual void Delete(BacktraceDatabaseEntry entry)
        {
            if (entry == null)
            {
                return;
            }
            foreach (var key in BatchRetry.Keys)
            {
                foreach (var value in BatchRetry[key])
                {
                    if (value.Id == entry.Id)
                    {
                        value.Delete();
                        BatchRetry[key].Remove(value);
                        TotalEntries--;
                        return;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Increment retry time for current entry
        /// </summary>
        public void IncrementBatchRetry()
        {
            RemoveMaxRetries();
            IncrementBatches();
        }

        /// <summary>
        /// Increment each batch
        /// </summary>
        private void IncrementBatches()
        {
            for (int i = _retryNumber - 2; i >= 0; i--)
            {
                var temp = BatchRetry[i];
                BatchRetry[i] = new List<BacktraceDatabaseEntry>();
                BatchRetry[i + 1] = temp;
            }
        }

        /// <summary>
        /// Remove last batch
        /// </summary>
        private void RemoveMaxRetries()
        {
            var currentBatch = BatchRetry[_retryNumber - 1];
            var total = currentBatch.Count;
            for (int i = 0; i < total; i++)
            {
                var value = currentBatch[i];
                value.Delete();
                TotalEntries--;
            }
        }

        /// <summary>
        /// Get all database entryes
        /// </summary>
        /// <returns>all existing database entries</returns>
        public IEnumerable<BacktraceDatabaseEntry> Get()
        {
            return BatchRetry.SelectMany(n => n.Value);
        }

        /// <summary>
        /// Get total number of entries in database
        /// </summary>
        /// <returns></returns>
        public int Count() => TotalEntries;

        /// <summary>
        /// Dispose
        /// </summary>
        public virtual void Dispose()
        {
            TotalEntries = 0;
            BatchRetry.Clear();
        }

        /// <summary>
        /// Delete all entries from database
        /// </summary>
        public void Clear()
        {
            var entries = BatchRetry.SelectMany(n => n.Value);
            foreach (var entry in entries)
            {
                entry.Delete();
            }
            TotalEntries = 0;
            //clear all existing batches
            foreach (var batch in BatchRetry)
            {
                batch.Value.Clear();
            }
        }

        /// <summary>
        /// Get last exising database entry. Method returns entry based on order in Database
        /// </summary>
        /// <returns>First Backtrace database entry</returns>
        public BacktraceDatabaseEntry LastOrDefault()
        {
            return RetryOrder == RetryOrder.Stack
                    ? GetLastEntry()
                    : GetFirstEntry();
        }

        /// <summary>
        /// Get first exising database entry. Method returns entry based on order in Database
        /// </summary>
        /// <returns>First Backtrace database entry</returns>
        public BacktraceDatabaseEntry FirstOrDefault()
        {
            return RetryOrder == RetryOrder.Queue
                    ? GetFirstEntry()
                    : GetLastEntry();
        }

        /// <summary>
        /// Get first entry in in-cache BacktraceDatabase
        /// </summary>
        /// <returns>First database entry</returns>
        private BacktraceDatabaseEntry GetFirstEntry()
        {
            //get all batches (from the beginning)
            for (int i = 0; i < _retryNumber - 1; i++)
            {
                //if batch has any entry that is not used
                //set lock to true 
                //and return file
                if (BatchRetry.ContainsKey(i) && BatchRetry[i].Any(n => !n.Locked))
                {
                    var entry = BatchRetry[i].First(n => !n.Locked);
                    entry.Locked = true;
                    return entry;
                }
            }
            return null;
        }

        /// <summary>
        /// Get last entry in in-cache BacktraceDatabase
        /// </summary>
        /// <returns>Last database entry</returns>
        private BacktraceDatabaseEntry GetLastEntry()
        {
            for (int i = _retryNumber - 1; i >= 0; i--)
            {
                if (BatchRetry[i].Any(n => !n.Locked))
                {
                    var entry = BatchRetry[i].Last(n => !n.Locked);
                    entry.Locked = true;
                    return entry;
                }
            }
            return null;
        }

    }
}
