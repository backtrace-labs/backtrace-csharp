using Backtrace.Types;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Backtrace.Common
{
    //typedef struct _MINIDUMP_EXCEPTION_INFORMATION {
    //    DWORD ThreadId;
    //    PEXCEPTION_POINTERS ExceptionPointers;
    //    BOOL ClientPointers;
    //} MINIDUMP_EXCEPTION_INFORMATION, *PMINIDUMP_EXCEPTION_INFORMATION;

    /// <summary>
    /// Exception information for current minidump method
    /// Pack=4 is important! So it works also for x64!
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct MiniDumpExceptionInformation
    {
        /// <summary>
        /// current thread id
        /// </summary>
        internal uint ThreadId;
        /// <summary>
        /// pointer to current exception
        /// </summary>
        internal IntPtr ExceptionPointers;

        /// <summary>
        /// Check who generate a pointer
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)]
        internal bool ClientPointers;


        /// <summary>
        /// Create instance of MiniDumpExceptionInformation
        /// </summary>
        /// <param name="exceptionInfo">Type to check if exception exists</param>
        /// <returns>New instance of MiniDumpExceptionInformation</returns>
        internal static MiniDumpExceptionInformation GetInstance(MinidumpException exceptionInfo)
        {
            MiniDumpExceptionInformation exp;
            exp.ThreadId = SystemHelper.GetCurrentThreadId();
            exp.ClientPointers = false;
            exp.ExceptionPointers = IntPtr.Zero;
            //right now GetExceptionPointers method is not available in .NET Standard 
#if !NETSTANDARD2_0
            if (exceptionInfo == MinidumpException.Present)
            {
                exp.ExceptionPointers = Marshal.GetExceptionPointers();
            }
#endif
            return exp;
        }
    }
}
