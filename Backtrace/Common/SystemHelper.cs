using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Backtrace.Common
{
    /// <summary>
    /// Get information about dll's
    /// </summary>
    internal static class SystemHelper
    {
        /// <summary>
        /// Get current thread Id
        /// </summary>
        [DllImport("kernel32.dll", EntryPoint = "GetCurrentThreadId", ExactSpelling = true)]
        internal static extern uint GetCurrentThreadId();

        /// <summary>
        /// Check if library exists
        /// </summary>
        /// <param name="lpFileName">Library name</param>
        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Check if library is available
        /// </summary>
        /// <param name="libraryName">library name</param>
        internal static bool IsLibraryAvailable(string libraryName)
        {
            bool result = LoadLibrary(libraryName) == IntPtr.Zero;
            if (!result)
            {
                Trace.WriteLine($"Library {libraryName} is not available in your project");
            }
            return result;
        }

        /// <summary>
        /// Check if libraries are available in system
        /// </summary>
        /// <param name="libraries">Library name to check</param>
        internal static bool IsLibraryAvailable(string[] libraries)
        {
            if (libraries.Length == 0)
            {
                return true;
            }
            return libraries.Any(n => !IsLibraryAvailable(n));
            
        }


    }
}
