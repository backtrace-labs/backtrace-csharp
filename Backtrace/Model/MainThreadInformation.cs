using System;
using System.Threading;
using Diagnostics = System.Diagnostics;

namespace Backtrace.Model
{
    /// <summary>
    /// Generate information about appliaction threads
    /// </summary>
    internal class MainThreadInformation
    {
        public readonly string MainThreadName;
        public readonly bool Fault;
        public readonly string MainThreadStackTrace;

        public ExceptionStack Stack = null;

        /// <summary>
        /// Create instance of AppThread class to get more information about thread information while debugging
        /// </summary>
        public MainThreadInformation()
        {
            var mainThread = Thread.CurrentThread;
            MainThreadName = mainThread.Name;
            Fault = (mainThread.ThreadState & ThreadState.Running) == ThreadState.Running;
            MainThreadStackTrace = Environment.StackTrace;

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
