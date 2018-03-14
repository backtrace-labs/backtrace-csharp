using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Get an information about single thread passed in constructor
    /// </summary>
    public class ThreadInformation
    {
        /// <summary>
        /// Thread Name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public readonly string Name;

        /// <summary>
        /// Denotes whether a thread is a faulting thread 
        /// </summary>
        [JsonProperty(PropertyName = "fault")]
        public readonly bool Fault;


        [JsonProperty(PropertyName = "stack")]
        internal IEnumerable<ExceptionStack> Stack = new List<ExceptionStack>();

        /// <summary>
        /// Create new instance of ThreadInformation
        /// </summary>
        /// <param name="threadName">Thread name</param>
        /// <param name="fault">Denotes whether a thread is a faulting thread - in most cases main thread</param>
        /// <param name="stack">Exception stack information</param>
        public ThreadInformation(string threadName, bool fault, IEnumerable<ExceptionStack> stack)
        {
            if (stack != null)
            {
                Stack = stack;
            }
            Name = threadName;
            Fault = fault;
            //Name = string.IsNullOrEmpty(mainThread.Name) ? mainThread.ManagedThreadId.ToString() : mainThread.Name;
            //Fault = (mainThread.ThreadState & ThreadState.Running) == ThreadState.Running;
        }

        /// <summary>
        /// Create new instance of ThreadInformation
        /// </summary>
        /// <param name="thread">Thread to analyse</param>
        /// <param name="stack">Exception stack information</param>
        /// <param name="currentThread">Is current thread flag</param>
        public ThreadInformation(Thread thread, IEnumerable<ExceptionStack> stack, bool currentThread = false)
            : this(
                 threadName: GenerateValidThreadName(thread),
                 fault: currentThread, //faulting thread = current thread
                 stack: stack)
        {
        }
        /// <summary>
        /// Generate a valid thread name for passed thread
        /// </summary>
        /// <returns>Thread name</returns>
        public static string GenerateValidThreadName(Thread thread)
        {
            //generate temporary thread name
            //thread name cannot be "null" or null or empty string
            //in worst scenario thread name should be managedThreadId 
            var threadName = thread.Name;
            threadName = string.IsNullOrEmpty(threadName)
                        ? thread.ManagedThreadId.ToString()
                        : threadName;

            return threadName;
        }

    }
}
