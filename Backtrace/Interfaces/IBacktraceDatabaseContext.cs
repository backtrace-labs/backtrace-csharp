using Backtrace.Base;
using Backtrace.Model;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Interfaces
{
    internal interface IBacktraceDatabaseContext<T> : IDisposable
    {
        /// <summary>
        /// Add new report to Database
        /// </summary>
        BacktraceDatabaseEntry<T> Add(BacktraceData<T> backtraceData);

        /// <summary>
        /// Add new data to database
        /// </summary>
        /// <param name="backtraceData">Diagnostic data</param>
        void Add(BacktraceDatabaseEntry<T> backtraceData);

        /// <summary>
        /// Get first entry or null
        /// </summary>
        /// <returns>First existing entry in database store</returns>
        BacktraceDatabaseEntry<T> FirstOrDefault();

        /// <summary>
        /// Get all repots stored in Database
        /// </summary>
        IEnumerable<BacktraceDatabaseEntry<T>> Get();

        /// <summary>
        /// Delete database entry by using BacktraceDatabaseEntry
        /// </summary>
        /// <param name="entry">Database entry</param>
        void Delete(BacktraceDatabaseEntry<T> entry);

        /// <summary>
        /// Check if any similar entry exists
        /// </summary>
        /// <param name="n">Compared entry</param>
        /// <returns>Database entry</returns>
        bool Any(BacktraceDatabaseEntry<T> n);

        /// <summary>
        /// Get total count of entries
        /// </summary>
        /// <returns>Total number of entries</returns>
        int Count();

        /// <summary>
        /// Clear database
        /// </summary>
        void Clear();
       

        /// <summary>
        /// Get all repots stored in Database
        /// </summary>
        //IEnumerable<BacktraceReportBase<T>> Get(Func<BacktraceReportBase<T>,IEnumerable<BacktraceReportBase<T>) filter);

    }
}
