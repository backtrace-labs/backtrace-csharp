using Backtrace.Base;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backtrace.Services
{
    /// <summary>
    /// Backtrace Database Context
    /// </summary>
    internal class BacktraceDatabaseContext<T> : IBacktraceDatabaseContext<T>
    {
        /// <summary>
        /// Database cache
        /// </summary>
        Dictionary<uint, List<BacktraceDatabaseEntry<T>>> BatchRetry = new Dictionary<uint, List<BacktraceDatabaseEntry<T>>>();

        private int _totalEntries = 0;
        /// <summary>
        /// Path to database directory 
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Initialize new instance of Backtrace Database Context
        /// </summary>
        /// <param name="databasePath">Path to database directory</param>
        /// <param name="retryNumber">Total number of retries</param>
        public BacktraceDatabaseContext(string databasePath, uint retryNumber)
        {
            _path = databasePath;
            SetupBatch(retryNumber);
        }

        /// <summary>
        /// SetupBatch
        /// </summary>
        /// <param name="retryNumber">Retry number</param>
        private void SetupBatch(uint retryNumber)
        {
            if (retryNumber == 0)
            {
                throw new ArgumentException($"{nameof(retryNumber)} have to be greater than 0!");
            }
            for (uint i = 0; i < retryNumber; i++)
            {
                BatchRetry[i] = new List<BacktraceDatabaseEntry<T>>();
            }
        }

        /// <summary>
        /// Add new entry to database
        /// </summary>
        /// <param name="backtraceData">Diagnostic data that should be stored in database</param>
        /// <returns>New instance of DatabaseEntry</returns>
        public BacktraceDatabaseEntry<T> Add(BacktraceData<T> backtraceData)
        {
            var entry = new BacktraceDatabaseEntry<T>(backtraceData, _path);
            BatchRetry[0].Add(entry);
            _totalEntries++;
            return entry;
        }

        /// <summary>
        /// Check if any entry exists
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool Any(BacktraceDatabaseEntry<T> entry)
        {
            return BatchRetry.SelectMany(n => n.Value).Any(n => n.Id == entry.Id);
        }

        /// <summary>
        /// Delete existing entry from database
        /// </summary>
        /// <param name="entry">Database entry to delete</param>
        public void Delete(BacktraceDatabaseEntry<T> entry)
        {
            foreach (var key in BatchRetry.Keys)
            {
                foreach (var value in BatchRetry[key])
                {
                    if (value.Id == entry.Id)
                    {
                        BatchRetry[key].Remove(value);
                        _totalEntries--;
                        return;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Get all database entryes
        /// </summary>
        /// <returns>all existing database entries</returns>
        public IEnumerable<BacktraceDatabaseEntry<T>> Get()
        {
            return BatchRetry.SelectMany(n => n.Value);
        }

        /// <summary>
        /// Get total number of entries in database
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return _totalEntries;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
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
            _totalEntries = 0;
        }

        /// <summary>
        /// Add existing entry to database
        /// </summary>
        /// <param name="backtraceEntry">Database entry</param>
        public void Add(BacktraceDatabaseEntry<T> backtraceEntry)
        {
            BatchRetry[0].Add(backtraceEntry);
            _totalEntries++;
        }

        /// <summary>
        /// Get first exising database entry
        /// </summary>
        /// <returns></returns>
        public BacktraceDatabaseEntry<T> FirstOrDefault()
        {
            for (uint i = 0; i < BatchRetry.Count; i++)
            {
                if (BatchRetry[i].Any())
                {
                    return BatchRetry[i].First();
                }
            }
            return null;
        }
    }
}
