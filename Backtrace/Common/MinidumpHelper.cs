using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Backtrace.Common
{
    /// <summary>
    /// Generate minidump file
    /// </summary>
    internal static class MinidumpHelper
    {
        private static readonly string[] Libraries = new[] { "kernel32.dll", "dbghelp.dll" };

        /// <summary>
        /// Check if dbghelp library is available
        /// </summary>
        /// <returns></returns>
        private static bool IsMemoryDumpAvailable()
        {
            bool result = true;
#if NETSTANDARD2_0
            result = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
            //if we other platform than Windows libraries aren't available
            //check if libraries are availbale in system
            return result && SystemHelper.IsLibraryAvailable(Libraries);
        }

        /// <summary>
        /// Save minidump file with exception informations
        /// </summary>
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, ref MiniDumpExceptionInformation expParam, IntPtr userStreamParam, IntPtr callbackParam);

        /// <summary>
        /// Save minidump file with exception informations. This function supporting MiniDumpExceptionInformation == NULL
        /// </summary>
        [DllImport("dbghelp.dll", EntryPoint = "MiniDumpWriteDump", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType, IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);


        /// <summary>
        /// Save memory dump on disk
        /// </summary>
        /// <param name="filePath">The path where the minidump file will be saved</param>
        /// <param name="options">Minidump save options</param>
        /// <param name="exceptionType">Type to check if exception exists</param>
        public static bool Write(string filePath, MiniDumpOptions options = MiniDumpOptions.WithFullMemory, MinidumpException exceptionType = MinidumpException.None)
        {
            bool miniDumpAvailable = IsMemoryDumpAvailable();
            if (!miniDumpAvailable)
            {
                return false;
            }

            Process currentProcess = Process.GetCurrentProcess();
            IntPtr currentProcessHandle = currentProcess.Handle;
            uint currentProcessId = (uint)currentProcess.Id;
            var exceptionInformation = MiniDumpExceptionInformation.GetInstance(exceptionType);
            using (FileStream fileHandle = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
            {
                return exceptionInformation.ExceptionPointers == IntPtr.Zero
                    ? MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle.SafeFileHandle, (uint)options, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero)
                    : MiniDumpWriteDump(currentProcessHandle, currentProcessId, fileHandle.SafeFileHandle, (uint)options, ref exceptionInformation, IntPtr.Zero, IntPtr.Zero);
            }
        }
    }
}
