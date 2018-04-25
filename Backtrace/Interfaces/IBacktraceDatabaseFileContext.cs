using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Backtrace.Interfaces
{
    internal interface IBacktraceDatabaseFileContext<T>
    {
        /// <summary>
        /// Get all valid physical entries stored in database directory
        /// </summary>
        /// <returns>All existing physical entries</returns>
        IEnumerable<FileInfo> GetEntries();

        /// <summary>
        /// Get all physical files stored in database directory
        /// </summary>
        /// <returns>All existing physical files</returns>
        IEnumerable<FileInfo> GetAll();

        /// <summary>
        /// Remove orphaned files existing in database directory
        /// </summary>
        /// <param name="existingEntries">Existing entries in BacktraceDatabaseContext</param>
        void RemoveOrphaned(IEnumerable<BacktraceDatabaseEntry<T>> existingEntries);
    }
}
