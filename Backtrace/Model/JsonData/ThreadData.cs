#if !NET35
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
            ThreadInformations.Add(current.ManagedThreadId.ToString(), new ThreadInformation(current, exceptionStack));
#if !NET35
            GetUsedThreads(callingAssembly, current.ManagedThreadId);
#endif
        }


#if !NET35
        /// <summary>
        /// Get all used threads in calling assembly. Function ignore current thread Id 
        /// </summary>
        /// <param name="callingAssembly">Calling assembly</param>
        /// <param name="ignoreId">Main thread Id</param>
        private void GetUsedThreads(Assembly callingAssembly, int ignoreId)
        {
            using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();

                foreach (ClrThread thread in runtime.Threads)
                {
                    if(thread.ManagedThreadId == ignoreId)
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