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
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Check if library is available
        /// </summary>
        /// <param name="libraryName">library name</param>
        internal static bool IsLibraryAvailable(string libraryName)
        {
            try
            {
                return LoadLibrary(libraryName) != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if libraries are available in system
        /// </summary>
        /// <param name="libraries">Library name to check</param>
        internal static bool IsLibraryAvailable(string[] libraries)
        {
            if (libraries == null || libraries.Length == 0)
            {
                return true;
            }
            return !libraries.Any(n => !IsLibraryAvailable(n));
        }

        internal static string Name(string architecture)
        {
#if WINDOWS_UWP
            return "Windows";
#else
            var platform = Environment.OSVersion.Platform;
            switch (platform)
            {
                case PlatformID.Win32NT:
                    return "Windows NT";
#if NETSTANDARD2_0
                case PlatformID.Unix:

                    // for .NET Core Environment.OSVersion returns Unix for MacOS and Linux
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        if (architecture == "arm" || architecture == "arm64")
                        {
                            return "IOS";
                        }
                        return "Mac OS X";
                    }
                    if (architecture == "arm" || architecture == "arm64")
                    {
                        return "Android";
                    }
                    return "Linux";

#else
                case PlatformID.MacOSX:
                    return "Mac OS X";
                case PlatformID.Unix:
                    return "Linux";
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return "Windows";
                case PlatformID.Xbox:
                    return "Xbox";
#endif
                default:
                    return "NaCl";
            }
#endif
        }

        internal static string CpuArchitecture()
        {
#if NETSTANDARD2_0
            Architecture cpuArchitecture = RuntimeInformation.ProcessArchitecture;
            switch (cpuArchitecture)
            {
                case Architecture.X86:
                    return "x86";
                case Architecture.X64:
                    return "amd64";
                case Architecture.Arm:
                    return "arm";
                case Architecture.Arm64:
                    return "arm64";
                default:
                    return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToLower();
            }
#else
            return Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE").ToLower();
#endif
        }
    }
}
