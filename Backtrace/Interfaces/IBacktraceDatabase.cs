using Backtrace.Base;
using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace Database Interface
    /// Before start: Be sure that used directory is empty!
    /// </summary>
    public interface IBacktraceDatabase : IDisposable
    {
        /// <summary>
        /// Start all database tasks - data storage, timers, file loading
        /// </summary>
        void Start();

        /// <summary>
        /// Send all reports stored in BacktraceDatabase and clean database
        /// </summary>
        void Flush();

#if !NET35
        /// <summary>
        /// Send all reports stored in BacktraceDatabase asynchronous and clean database
        /// </summary>
        Task FlushAsync();
#endif
        void SetApi(IBacktraceApi backtraceApi);

        /// <summary>
        /// Remove all existing reports in BacktraceDatabase
        /// </summary>
        void Clear();

        /// <summary>
        /// Add new report to Database
        /// </summary>
        BacktraceDatabaseEntry Add(BacktraceReportBase backtraceReport, Dictionary<string, object> attributes, MiniDumpType miniDumpType = MiniDumpType.Normal);

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
        /// Get database settings
        /// </summary>
        /// <returns></returns>
        BacktraceDatabaseSettings GetSettings();
    }
}
