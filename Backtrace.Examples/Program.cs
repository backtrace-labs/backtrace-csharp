using Backtrace.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backtrace.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            int managedThreadId = 0;
            var task = Task.Run(
                () =>
                {
            // Unfortunately, cant see "Testing" anywhere in result returned
            // from NuGet package ClrMD ...
            Thread.CurrentThread.Name = "Testing";
                    Thread.Sleep(TimeSpan.FromDays(1));
                });


            // ... so we look for our thread by the first word in this namespace.
            string startOfThisNamespace = this.GetType().Namespace.ToString().Split('.')[0]; // Is "MyTest".
            using (DataTarget target = DataTarget.AttachToProcess(Process.GetCurrentProcess().Id, 5000, AttachFlag.Passive))
            {
                ClrRuntime runtime = target.ClrVersions.First().CreateRuntime();

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
                //initialize new BacktraceClient with custom configuration section readed from file App.config
                //Client will be initialized with values stored in default section name "BacktraceCredentials"
                var backtraceClient = new BacktraceClient();


            var credentials = new BacktraceCredentials("backtraceHostUrl", "accessToken");
            var backtraceClientWithCredentials = new BacktraceClient(credentials);
        }
    }
}
