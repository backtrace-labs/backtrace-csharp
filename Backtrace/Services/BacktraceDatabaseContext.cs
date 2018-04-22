using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Linq;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
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
        internal Dictionary<int, List<BacktraceDatabaseEntry<T>>> BatchRetry = new Dictionary<int, List<BacktraceDatabaseEntry<T>>>();

        internal int totalEntries = 0;
        /// <summary>
        /// Path to database directory 
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Maximum number of retries
        /// </summary>
        private readonly int _retryNumber;

        /// <summary>
        /// Initialize new instance of Backtrace Database Context
        /// </summary>
        /// <param name="databasePath">Path to database directory</param>
        /// <param name="retryNumber">Total number of retries</param>
        public BacktraceDatabaseContext(string databasePath, uint retryNumber)
        {
            _path = databasePath;
            _retryNumber = checked((int)retryNumber);
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
                BatchRetry[i] = new List<BacktraceDatabaseEntry<T>>();
            }
        }

        /// <summary>
        /// Add new entry to database
        /// </summary>
        /// <param name="backtraceData">Diagnostic data that should be stored in database</param>
        /// <returns>New instance of DatabaseEntry</returns>
        public virtual BacktraceDatabaseEntry<T> Add(BacktraceData<T> backtraceData)
        {
            var entry = new BacktraceDatabaseEntry<T>(backtraceData, _path);
            BatchRetry[0].Add(entry);
            totalEntries++;
            entry.InUse = true;
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
        public virtual void Delete(BacktraceDatabaseEntry<T> entry)
        {
            if(entry == null)
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
                        totalEntries--;
                        return;
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Increment retry time for current entry
        /// </summary>
        /// <param name="entry">Database entry to move move in memory cache</param>
        public void MoveNext()
        {
            for (int kIndex = BatchRetry.Keys.Count - 1; kIndex != -1 ; kIndex--)
            {
                for (int rIndex = BatchRetry[kIndex].Count - 1; rIndex != -1; rIndex--)
                {
                    var value = BatchRetry[kIndex][rIndex];
                    if (kIndex + 1 < _retryNumber)
                    {
                        BatchRetry[kIndex + 1].Add(value);
                    }
                    else
                    {
                        value.Delete();
                        totalEntries--;
                    }
                    BatchRetry[kIndex].Remove(value);
                }
            }
        }

        /// <summary>
        /// Increment retry time for current entry
        /// </summary>
        /// <param name="entry">Database entry to move move in memory cache</param>
        public virtual void MoveNext(BacktraceDatabaseEntry<T> entry)
        {
            foreach (var key in BatchRetry.Keys)
            {
                foreach (var value in BatchRetry[key])
                {
                    if (value.Id == entry.Id)
                    {
                        if (key + 1 <= _retryNumber)
                        {
                            BatchRetry[key + 1].Add(value);
                        }
                        else
                        {
                            value.Delete();
                            totalEntries--;
                        }
                        BatchRetry[key].Remove(value);
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
            return BatchRetry.SelectMany(n => n.Value).Select(n => { n.InUse = true; return n; });
        }

        /// <summary>
        /// Get total number of entries in database
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            return totalEntries;
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
            totalEntries = 0;
        }

        /// <summary>
        /// Add existing entry to database
        /// </summary>
        /// <param name="backtraceEntry">Database entry</param>
        public void Add(BacktraceDatabaseEntry<T> backtraceEntry)
        {
            BatchRetry[0].Add(backtraceEntry);
            totalEntries++;
        }

        /// <summary>
        /// Get first exising database entry
        /// </summary>
        /// <returns></returns>
        public BacktraceDatabaseEntry<T> FirstOrDefault()
        {
            for (int i = 0; i < BatchRetry.Count; i++)
            {
                if (BatchRetry[i].Any(n => !n.InUse))
                {
                    var entry = BatchRetry[i].First(n => !n.InUse);
                    entry.InUse = true;
                    return entry;
                }
            }
            return null;
        }

    }
}
