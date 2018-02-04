using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;


[assembly: InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace
{
    //TODO:
    //Change public to internal!

    /// <summary>
    /// Generate environment information about platform, hardware, OS
    /// </summary>
    internal class EnvironmentManager
    {
       
        /// <summary>
        /// Get all threads used in application
        /// </summary>
        //public ProcessThreadCollection UsedThreads => Process.GetCurrentProcess().Threads;
        //public ProcessThreadCollection UseThreads()
        //{
        //    Process.GetCurrentProcess().Threads;
        //}
    }
}
