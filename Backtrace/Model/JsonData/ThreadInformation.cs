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
    internal class ThreadInformation
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

        //[JsonProperty(PropertyName = "stackTrace")]
        //public readonly string MainThreadStackTrace;

        [JsonProperty(PropertyName = "stack")]
        internal ExceptionStack[] Stack = null;

        /// <summary>
        /// Create new instance of ThreadInformation
        /// </summary>
        /// <param name="thread">Thread to analyse</param>
        public ThreadInformation(Thread thread, ExceptionStack stack)
        {
            Stack = new[] { stack };
            var mainThread = Thread.CurrentThread;
            Name = string.IsNullOrEmpty(mainThread.Name) ? "mainThread" : mainThread.Name;
            Fault = (mainThread.ThreadState & ThreadState.Running) == ThreadState.Running;
            //MainThreadStackTrace = Environment.StackTrace;
        }

    }
}
