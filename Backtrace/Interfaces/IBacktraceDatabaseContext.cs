using Backtrace.Base;
using Backtrace.Model;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Backtrace.Interfaces
{
    internal interface IBacktraceDatabaseContext<T> : IDisposable
    {
        /// <summary>
        /// Add new record to Database
        /// </summary>
        /// <param name="backtraceData">Diagnostic data</param>
        BacktraceDatabaseRecord<T> Add(BacktraceData<T> backtraceData);

        /// <summary>
        /// Add new data to database
        /// </summary>
        /// <param name="backtraceDatabaseRecord">Database record</param>
        BacktraceDatabaseRecord<T> Add(BacktraceDatabaseRecord<T> backtraceDatabaseRecord);

        /// <summary>
        /// Get first record or null
        /// </summary>
        /// <returns>First existing record in database store</returns>
        BacktraceDatabaseRecord<T> FirstOrDefault();

        /// <summary>
        /// Get last record or null
        /// </summary>
        /// <returns>Last existing record in database store</returns>
        BacktraceDatabaseRecord<T> LastOrDefault();

        /// <summary>
        /// Get all records stored in Database
        /// </summary>
        IEnumerable<BacktraceDatabaseRecord<T>> Get();

        /// <summary>
        /// Delete database record by using BacktraceDatabaseRecord
        /// </summary>
        /// <param name="record">Database record</param>
        void Delete(BacktraceDatabaseRecord<T> record);

        /// <summary>
        /// Check if any similar record exists
        /// </summary>
        /// <param name="n">Compared record</param>
        bool Any(BacktraceDatabaseRecord<T> n);

        /// <summary>
        /// Check if any similar record exists
        /// </summary>
        bool Any();

        /// <summary>
        /// Get total count of records
        /// </summary>
        /// <returns>Total number of records</returns>
        int Count();

        /// <summary>
        /// Clear database
        /// </summary>
        void Clear();

        /// <summary>
        /// Increment record time for all records
        /// </summary>
        void IncrementBatchRetry();

        /// <summary>
        /// Get database size
        /// </summary>
        /// <returns>Database size</returns>
        long GetSize();

        /// <summary>
        /// Get total number of records stored in database
        /// </summary>
        /// <returns>Total number of records</returns>
        int GetTotalNumberOfRecords();

        /// <summary>
        /// Get all repots stored in Database
        /// </summary>
        //IEnumerable<BacktraceReportBase<T>> Get(Func<BacktraceReportBase<T>,IEnumerable<BacktraceReportBase<T>) filter);

    }
}
