#if NET45
using Microsoft.Diagnostics.Runtime;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Reflection;
using Backtrace.Extensions;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Generate information about appliaction threads
    /// </summary>
    public class ThreadData
    {
        /// <summary>
        /// All collected application threads information
        /// </summary>
        public Dictionary<string, ThreadInformation> ThreadInformations = new Dictionary<string, ThreadInformation>();

        /// <summary>
        /// Application Id for current thread. This value is used in mainThreadSection in output JSON file
        /// </summary>
        internal string MainThread = string.Empty;

        /// <summary>
        /// Create instance of ThreadData class to collect information about used threads
        /// </summary>
        internal ThreadData(Assembly callingAssembly, IEnumerable<BacktraceStackFrame> exceptionStack)
        {
#if NET45
            //use available in .NET 4.5 api to find stack trace of all available managed threads
            GetUsedThreads(callingAssembly);
#else
            //get all available process threads
            ProcessThreads();
#endif
            //get stack trace and infomrations about current thread
            GenerateCurrentThreadInformation(exceptionStack);
        }

        /// <summary>
        /// Generate information for current thread
        /// </summary>
        /// <param name="exceptionStack">Current BacktraceReport exception stack</param>
        private void GenerateCurrentThreadInformation(IEnumerable<BacktraceStackFrame> exceptionStack)
        {
            var current = Thread.CurrentThread;
            //get current thread id
            string generatedMainThreadId = current.GenerateValidThreadName().ToLower();

            ThreadInformations[generatedMainThreadId] = new ThreadInformation(current, exceptionStack);
            //set currentThreadId
            MainThread = generatedMainThreadId;
        }

        /// <summary>
        /// Generate list of process thread 
        /// </summary>
        private void ProcessThreads()
        {
            ProcessThreadCollection currentThreads = null;
            try
            {
                currentThreads = Process.GetCurrentProcess().Threads;
                if (currentThreads == null)
                {
                    return;
                }
            }
            catch
            {
                //handle UWP
                return;
            }
            foreach (ProcessThread thread in currentThreads)
            {
                if (thread == null)
                {
                    continue;
                }
                //you can't retrieve stack trace from processThread
                //you can't retrieve thread name from processThread 
                string threadId = thread.Id.ToString();
                ThreadInformations.Add(Guid.NewGuid().ToString(), new ThreadInformation(threadId, false, null));
            }
        }

#if NET45
        /// <summary>
        /// Get all used threads in calling assembly. Function ignore current thread Id 
        /// </summary>
        /// <param name="callingAssembly">Calling assembly</param>
        private void GetUsedThreads(Assembly callingAssembly)
        {
            var mainThreadId = Thread.CurrentThread.ManagedThreadId;
            using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                if (target.ClrVersions == null || !target.ClrVersions.Any())
                {
                    return;
                }
                ClrRuntime runtime = null;
                try
                {
                    runtime = target.ClrVersions.First().CreateRuntime();
                }
                catch (ClrDiagnosticsException)
                {
                    //we cannot create runtime for current applications state
                    return;
                }
                foreach (ClrThread thread in runtime.Threads)
                {
                    if (thread.ManagedThreadId == mainThreadId)
                    {
                        //main thread catched
                        continue;
                    }
                    //ClrThread doesn't have any information about thread 
                    string threadName = thread.OSThreadId.ToString();
                    if (string.IsNullOrEmpty(threadName))
                    {
                        threadName = thread.ManagedThreadId.ToString();
                    }
                    var frames = new List<BacktraceStackFrame>();
                    foreach (var frame in thread.StackTrace)
                    {
                        if(frame.Method == null)
                        {
                            continue;
                        }
                        frames.Add(new BacktraceStackFrame(frame));
                    }
                    ThreadInformations[threadName] = new ThreadInformation(threadName, false, frames);
                }
            }
        }
#endif
    }
}