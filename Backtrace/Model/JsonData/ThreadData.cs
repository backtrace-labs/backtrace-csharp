#if NET461
using Microsoft.Diagnostics.Runtime;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Diagnostics = System.Diagnostics;
using System.Reflection;
using System.Collections;
using Backtrace.Extensions;

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
        public ThreadData(Assembly callingAssembly, IEnumerable<ExceptionStack> exceptionStack)
        {
            var current = Thread.CurrentThread;
            var managedThreadId = current.ManagedThreadId;
            ProcessThreads(managedThreadId);
            bool mainThreadIncluded = false;
#if NET461
            mainThreadIncluded = !(exceptionStack != null && exceptionStack.Any());
            GetUsedThreads(callingAssembly, mainThreadIncluded);
#endif
            if (mainThreadIncluded)
            {
                return;
            }
            ThreadInformations.Add(current.ManagedThreadId.ToString(), new ThreadInformation(current, exceptionStack));
        }

        private void ProcessThreads(int managedThreadId)
        {
            ProcessThreadCollection currentThreads = Process.GetCurrentProcess().Threads;

            foreach (ProcessThread thread in currentThreads)
            {
                if (thread.Id == managedThreadId)
                {
                    Trace.WriteLine(thread);
                }
                if (thread.ThreadState == Diagnostics.ThreadState.Running)
                {
                    Trace.WriteLine(thread);
                }
            }
        }


#if NET461
        /// <summary>
        /// Get all used threads in calling assembly. Function ignore current thread Id 
        /// </summary>
        /// <param name="callingAssembly">Calling assembly</param>
        /// <param name="mainThreadIncluded">If true, method wont generate stacktrace for main thread</param>
        private void GetUsedThreads(Assembly callingAssembly, bool mainThreadIncluded)
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();

                foreach (ClrThread thread in runtime.Threads)
                {
                    if (!mainThreadIncluded && thread.ManagedThreadId == mainThreadId)
                    {
                        //main thread catched
                        continue;
                    }
                    //ClrThread doesn't have any information about thread name
                    string threadName = thread.ManagedThreadId.ToString();
                    var frames = ExceptionStack.Convert(thread.StackTrace);
                    ThreadInformations.Add(threadName, new ThreadInformation(threadName, false, frames));
                }
            }
        }
#endif
    }
}