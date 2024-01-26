﻿using Backtrace.Model;
using Backtrace.Model.Database;
using Backtrace.Types;
using System;
using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Backtrace.Interfaces
{
    internal interface IBacktraceDatabaseContext : IDisposable
    {
        /// <summary>
        /// Add new record to Database
        /// </summary>
        /// <param name="backtraceData">Diagnostic data</param>
        BacktraceDatabaseRecord Add(BacktraceData backtraceData, MiniDumpType miniDumpType = MiniDumpType.None);

        /// <summary>
        /// Add new data to database
        /// </summary>
        /// <param name="backtraceDatabaseRecord">Database record</param>
        BacktraceDatabaseRecord Add(BacktraceDatabaseRecord backtraceDatabaseRecord);

        /// <summary>
        /// Get first record or null
        /// </summary>
        /// <returns>First existing record in database store</returns>
        BacktraceDatabaseRecord FirstOrDefault();

        /// <summary>
        /// Get first record or null
        /// </summary>
        /// <returns>First existing record in database store</returns>
        BacktraceDatabaseRecord FirstOrDefault(Func<BacktraceDatabaseRecord, bool> predicate);

        /// <summary>
        /// Get last record or null
        /// </summary>
        /// <returns>Last existing record in database store</returns>
        BacktraceDatabaseRecord LastOrDefault();

        /// <summary>
        /// Get all records stored in Database
        /// </summary>
        IEnumerable<BacktraceDatabaseRecord> Get();

        /// <summary>
        /// Delete database record by using BacktraceDatabaseRecord
        /// </summary>
        /// <param name="record">Database record</param>
        void Delete(BacktraceDatabaseRecord record);

        /// <summary>
        /// Check if any similar record exists
        /// </summary>
        /// <param name="n">Compared record</param>
        bool Any(BacktraceDatabaseRecord n);

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
        [Obsolete("Please use Count method instead")]
        int GetTotalNumberOfRecords();

        /// <summary>
        /// Remove last record in database. 
        /// </summary>
        /// <returns>If algorithm can remove last record, method return true. Otherwise false</returns>
        bool RemoveLastRecord();

        /// <summary>
        /// Deduplication method. Use this method to override default method to generate hash from deduplication model
        /// </summary>
        Func<DeduplicationStrategy, BacktraceData, string> DeduplicationHash { get; set; }
    }
}
