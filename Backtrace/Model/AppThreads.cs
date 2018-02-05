#if NETSTANDARD2_0
using Microsoft.Diagnostics.Runtime;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Backtrace.Model
{
    public class AppThreads
    {
        public readonly string mainThreadName;
        public readonly bool fault;
        public readonly string mainThreadStackTrace;
        public AppThreads()
        {
            var mainThread = Thread.CurrentThread;
            mainThreadName = mainThread.Name;
            fault = (mainThread.ThreadState & ThreadState.Running) == ThreadState.Running;
            mainThreadStackTrace = Environment.StackTrace;
        }
    }
}
