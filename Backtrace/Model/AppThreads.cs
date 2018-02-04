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
    internal class AppThreads
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

        public void GetThreadInfo()
        {
            //var task = Task.Run(
            //    () =>
            //    {
            //        Thread.CurrentThread.Name = "Backtrace";
            //        Thread.Sleep(TimeSpan.FromDays(1));
            //    });
#if NETSTANDARD2_0 
            string startOfThisNamespace = this.GetType().Namespace.ToString().Split('.')[0];
            using (DataTarget target = DataTarget.AttachToProcess(System.Diagnostics.Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                ClrRuntime runtime = target.ClrVersions[0].CreateRuntime();
                foreach (ClrThread thread in runtime.Threads)
                {
                    IList<ClrStackFrame> stackFrames = thread.StackTrace;

                    List<ClrStackFrame> stackframesRelatedToUs = stackFrames
                        .Where(o => o.Method != null && o.Method.ToString().StartsWith(startOfThisNamespace)).ToList();

                    if (stackframesRelatedToUs.Count > 0)
                    {
                        Console.Write("ManagedThreadId: {0}, OSThreadId: {1}, Thread: IsAlive: {2}, IsBackground: {3}:\n", thread.ManagedThreadId, thread.OSThreadId, thread.IsAlive, thread.IsBackground);
                        Console.Write("- Stack frames related namespace '{0}':\n", startOfThisNamespace);
                        foreach (var s in stackframesRelatedToUs)
                        {
                            Console.Write("  - StackFrame: {0}\n", s.Method.ToString());
                        }
                    }
                }
            }
#endif
        }
    }
}
