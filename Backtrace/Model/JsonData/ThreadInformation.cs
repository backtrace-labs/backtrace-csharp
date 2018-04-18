using Backtrace.Extensions;
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
        public string Name { get; private set; }

        /// <summary>
        /// Denotes whether a thread is a faulting thread 
        /// </summary>
        [JsonProperty(PropertyName = "fault")]
        public bool Fault { get; private set; }


        [JsonProperty(PropertyName = "stack")]
        internal IEnumerable<DiagnosticStack> Stack = new List<DiagnosticStack>();

        /// <summary>
        /// Create new instance of ThreadInformation
        /// </summary>
        /// <param name="threadName">Thread name</param>
        /// <param name="fault">Denotes whether a thread is a faulting thread - in most cases main thread</param>
        /// <param name="stack">Exception stack information</param>
        [JsonConstructor()]
        public ThreadInformation(string threadName, bool fault, IEnumerable<DiagnosticStack> stack)
        {
            if (stack != null)
            {
                Stack = stack;
            }
            Name = threadName;
            Fault = fault;
        }

        /// <summary>
        /// Create new instance of ThreadInformation
        /// </summary>
        /// <param name="thread">Thread to analyse</param>
        /// <param name="stack">Exception stack information</param>
        /// <param name="currentThread">Is current thread flag</param>
        public ThreadInformation(Thread thread, IEnumerable<DiagnosticStack> stack, bool currentThread = false)
            : this(
                 threadName: thread.GenerateValidThreadName(),
                 fault: currentThread, //faulting thread = current thread
                 stack: stack)
        { }
    }
}
