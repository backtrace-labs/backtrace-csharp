using Backtrace.Base;
using Backtrace.Model;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Backtrace.Interfaces
{
    internal interface IBacktraceDatabaseContext : IDisposable
    {
        /// <summary>
        /// Add new report to Database
        /// </summary>
        /// <param name="backtraceData">Diagnostic data</param>
        BacktraceDatabaseEntry Add(BacktraceData backtraceData);

        /// <summary>
        /// Add new data to database
        /// </summary>
        /// <param name="backtraceDatabaseEntry">Database entry</param>
        BacktraceDatabaseEntry Add(BacktraceDatabaseEntry backtraceDatabaseEntry);

        /// <summary>
        /// Get first entry or null
        /// </summary>
        /// <returns>First existing entry in database store</returns>
        BacktraceDatabaseEntry FirstOrDefault();

        /// <summary>
        /// Get last entry or null
        /// </summary>
        /// <returns>Last existing entry in database store</returns>
        BacktraceDatabaseEntry LastOrDefault();

        /// <summary>
        /// Get all repots stored in Database
        /// </summary>
        IEnumerable<BacktraceDatabaseEntry> Get();

        /// <summary>
        /// Delete database entry by using BacktraceDatabaseEntry
        /// </summary>
        /// <param name="entry">Database entry</param>
        void Delete(BacktraceDatabaseEntry entry);

        /// <summary>
        /// Check if any similar entry exists
        /// </summary>
        /// <param name="n">Compared entry</param>
        bool Any(BacktraceDatabaseEntry n);

        /// <summary>
        /// Check if any similar entry exists
        /// </summary>
        bool Any();

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
        /// Increment retry time for all entries
        /// </summary>
        void IncrementBatchRetry();


        /// <summary>
        /// Get all repots stored in Database
        /// </summary>
        //IEnumerable<BacktraceReportBase> Get(Func<BacktraceReportBase,IEnumerable<BacktraceReportBase) filter);

    }
}
