using Backtrace.Base;
using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Text;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Interfaces
{
    /// <summary>
    /// Backtrace Database Interface
    /// Before start: Be sure that used directory is empty!
    /// </summary>
    internal interface IBacktraceDatabase<T>
    {
        /// <summary>
        /// Create new minidump file in database directory path. Minidump file name is a random Guid
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        /// <param name="miniDumpType">Generated minidump type</param>
        /// <returns>Path to minidump file</returns>
        string GenerateMiniDump(BacktraceReportBase<T> backtraceReport, MiniDumpType miniDumpType);

        /// <summary>
        /// Clear generated minidumps
        /// </summary>
        /// <param name="pathToMinidump">Path to created minidump</param>
        void ClearMiniDump(string pathToMinidump);
    }
}
