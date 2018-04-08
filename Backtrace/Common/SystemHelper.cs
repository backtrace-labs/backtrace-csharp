using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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

        /// <summary>
        /// Detect a system assemblies - assemblies that root namespace is "System" or "Microsoft
        /// If assembly is null, method will return false
        /// </summary>
        /// <param name="assembly">Assembly to check</param>
        /// <returns>True if assembly is from Microsoft of System</returns>
        internal static bool SystemAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return false;
            }
            var assemblyName = assembly.GetName().Name;
            return SystemAssembly(assemblyName);
        }
        /// <summary>
        /// Detect a system assemblies - assemblies that root namespace is "System" or "Microsoft
        /// </summary>
        /// <returns>True if assembly is from Microsoft of System</returns>
        internal static bool SystemAssembly(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                return false;
            }
            return (assemblyName.StartsWith("Microsoft.")
                || assemblyName.StartsWith("mscorlib")
                || assemblyName.Equals("System")
                || assemblyName.StartsWith("System."));
        }
#if !NET35
        internal static bool StateMachineFrame(TypeInfo declaringTypeInfo)
        {
            return typeof(System.Runtime.CompilerServices.IAsyncStateMachine)
                .GetTypeInfo().IsAssignableFrom(declaringTypeInfo);
        }
#endif
    }
}
