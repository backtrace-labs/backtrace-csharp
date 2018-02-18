#if !NET35
using Microsoft.Diagnostics.Runtime;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Diagnostics = System.Diagnostics;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Generate information about appliaction threads
    /// </summary>
    internal class ThreadData
    {
        internal Dictionary<string, ThreadInformation> ThreadInformations = new Dictionary<string, ThreadInformation>();
        /// <summary>
        /// Create instance of ThreadData class to get more information about threads used in application
        /// </summary>
        public ThreadData(ExceptionStack exceptionStack)
        {
            var current = Thread.CurrentThread;
#if !NET35
            DiagnoseThreads();
#endif
            ThreadInformations.Add(current.ManagedThreadId.ToString(), new ThreadInformation(current, exceptionStack));
        }
        /// <summary>
        /// Get current process thread based on main thread. Function use current process to get a ProcessThread
        /// </summary>
        /// <param name="thread">Main Thread</param>
        /// <returns>Process thread with Id equal to managed thread Id</returns>
        private ProcessThread GetCurrentProcessThread(Thread thread)
        {
            var managedThreadId = thread.ManagedThreadId;

            var processThreads = Diagnostics.Process.GetCurrentProcess().Threads;
            for (int index = 0; index < processThreads.Count; index++)
            {
                var current = processThreads[index];
                if (current.Id == managedThreadId)
                {
                    return current;
                }
            }
            return null;
        }

        private void DiagnoseThreads()
        {

#if !NET35

            var task = System.Threading.Tasks.Task.Run(
               () =>
               {
                   Thread.CurrentThread.Name = "BacktraceTests";
                   Thread.Sleep(TimeSpan.FromDays(1));

               });
            string startOfThisNamespace = this.GetType().Namespace.ToString().Split('.')[0];
            using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                ClrRuntime runtime = target.ClrVersions[0].CreateRuntime();

                foreach (ClrThread thread in runtime.Threads)
                {

                    Console.WriteLine("### Thread {0}", thread.OSThreadId);
                    Console.WriteLine("Thread type: {0}",
                                            thread.IsBackground ? "Background"
                                          : thread.IsGC ? "GC"
                                          : "Foreground");
                    Console.WriteLine("");
                    Console.WriteLine("Stack trace:");
                    foreach (var stackFrame in thread.EnumerateStackTrace())
                    {
                        Console.WriteLine("* {0}", stackFrame.DisplayString);
                    }                    
                }
            }
#endif
        }
    }
}
