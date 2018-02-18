using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Backtrace.Common
{
    internal static class ClrDump
    {
        private static readonly string clrLibrary = "clrdump.dll";

        public static void Dump(string filePath)
        {
            bool clrAvailable = IsClrAvailable();
#if net35
            IntPtr pEP = Marshal.GetExceptionPointers();
            CreateDump(
                Process.GetCurrentProcess().Id,
                filePath,
                (Int32)MINIDUMP_TYPE.MiniDumpNormal,
                Thread.CurrentThread.ManagedThreadId,
                pEP
            );
#endif
        }

        private static bool IsClrAvailable()
        {
            bool result = LoadLibrary(clrLibrary) == IntPtr.Zero;
            if (!result)
            {
                Trace.WriteLine("Clr is not available in your project");
            }
            return result;
        }



        [return: MarshalAs(UnmanagedType.SysInt)]
        [DllImport("clrdump.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern Int32 CreateDump(Int32 ProcessId, string FileName, Int32 DumpType, Int32 ExcThreadId, IntPtr ExtPtrs);

        [DllImport("clrdump.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [Flags]
        public enum FILTER_OPTIONS
        {
            CLRDMP_OPT_CALLDEFAULTHANDLER = 1
        }

        [Flags]
        public enum MINIDUMP_TYPE
        {
            MiniDumpFilterMemory = 8,
            MiniDumpFilterModulePaths = 0x80,
            MiniDumpNormal = 0,
            MiniDumpScanMemory = 0x10,
            MiniDumpWithCodeSegs = 0x2000,
            MiniDumpWithDataSegs = 1,
            MiniDumpWithFullMemory = 2,
            MiniDumpWithFullMemoryInfo = 0x800,
            MiniDumpWithHandleData = 4,
            MiniDumpWithIndirectlyReferencedMemory = 0x40,
            MiniDumpWithoutManagedState = 0x4000,
            MiniDumpWithoutOptionalData = 0x400,
            MiniDumpWithPrivateReadWriteMemory = 0x200,
            MiniDumpWithProcessThreadData = 0x100,
            MiniDumpWithThreadInfo = 0x1000,
            MiniDumpWithUnloadedModules = 0x20
        }
        
    }
}
