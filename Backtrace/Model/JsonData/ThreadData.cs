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

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Generate information about appliaction threads
    /// </summary>
    public class ThreadData
    {
        /// <summary>
        /// All collected information about application threads
        /// </summary>
        public Dictionary<string, ThreadInformation> ThreadInformations = new Dictionary<string, ThreadInformation>();

        /// <summary>
        /// Create instance of ThreadData class to get more information about threads used in application
        /// </summary>
        internal ThreadData(Assembly callingAssembly, IEnumerable<ExceptionStack> exceptionStack)
        {
            var current = Thread.CurrentThread;
#if NET461
            GetUsedThreads(callingAssembly);
#else
            ProcessThreads();
#endif
            var currentThreadStackTrace = ExceptionStack.FromCurrentThread(callingAssembly.GetName().Name, exceptionStack);
            ThreadInformations.Add(current.ManagedThreadId.ToString(), new ThreadInformation(current,currentThreadStackTrace));
        }

        private void ProcessThreads()
        {
            ProcessThreadCollection currentThreads = null;
            try
            {
                currentThreads = Process.GetCurrentProcess().Threads;
            }
            catch
            {
                //handle UWP
                return;
            }
            foreach (ProcessThread thread in currentThreads)
            {
                //you can't retrieve stack trace for processthread
                string threadId = thread.Id.ToString();
                ThreadInformations.Add(threadId, new ThreadInformation(threadId, false, null));
            }
        }


#if NET461
        /// <summary>
        /// Get all used threads in calling assembly. Function ignore current thread Id 
        /// </summary>
        /// <param name="callingAssembly">Calling assembly</param>
        private void GetUsedThreads(Assembly callingAssembly)
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();

                foreach (ClrThread thread in runtime.Threads)
                {
                    if (thread.ManagedThreadId == mainThreadId)
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