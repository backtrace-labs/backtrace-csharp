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
        /// Application Id for current thread. This value is used in mainThreadSection in output JSON file
        /// </summary>
        internal string mainThread = string.Empty;

        /// <summary>
        /// Create instance of ThreadData class to get more information about threads used in application
        /// </summary>
        internal ThreadData(Assembly callingAssembly, IEnumerable<ExceptionStack> exceptionStack)
        {
#if NET461
            GetUsedThreads(callingAssembly);
#else
            ProcessThreads();
#endif
            GenerateCurrentThreadInformation(callingAssembly, exceptionStack);
        }

       

        /// <summary>
        /// Generate information for current thread
        /// </summary>
        private void GenerateCurrentThreadInformation(Assembly callingAssembly, IEnumerable<ExceptionStack> exceptionStack)
        {
            var current = Thread.CurrentThread;
            //get a current thread stack trace
            //in thread stack trace we concatenate current thread stack trace and stack trace available in exception object
            var currentThreadStackTrace = ExceptionStack.FromCurrentThread(callingAssembly.GetName().Name, exceptionStack);

            //get current thread id
            string generatedMainThreadId = ThreadInformation.GenerateValidThreadName(current);

            ThreadInformations[generatedMainThreadId] = new ThreadInformation(current, currentThreadStackTrace);
            //set currentThreadId
            mainThread = generatedMainThreadId;
        }

        /// <summary>
        /// Generate list of process thread and it to threadInformation dictionary.
        /// </summary>
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
                ThreadInformations.Add(Guid.NewGuid().ToString(), new ThreadInformation(threadId, false, null));
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
                    //ClrThread doesn't have any information about thread 
                    string threadName = thread.ManagedThreadId.ToString();
                    var frames = ExceptionStack.Convert(thread.StackTrace);
                    ThreadInformations.Add(threadName, new ThreadInformation(threadName, false, frames));
                }
            }
        }
#endif
    }
}