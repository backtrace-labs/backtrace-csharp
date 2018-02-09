using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            ThreadInformations.Add(current.ManagedThreadId.ToString(), new ThreadInformation(current, exceptionStack));
        }
        /// <summary>
        /// Get current process thread based on main thread. Function use current process to get a ProcessThread
        /// </summary>
        /// <param name="thread">Main Thread</param>
        /// <returns>Process thread with Id equal to managed thread Id</returns>
        private Diagnostics.ProcessThread GetCurrentProcessThread(Thread thread)
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
    }
}
