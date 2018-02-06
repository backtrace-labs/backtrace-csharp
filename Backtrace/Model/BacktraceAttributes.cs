using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Model
{
    /// <summary>
    /// Class instance to get a built-in attributes from current application
    /// </summary>
    internal class BacktraceAttributes
    {
        /// <summary>
        /// Get built-in attributes
        /// </summary>
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        private readonly Assembly _assembly;

        /// <summary>
        /// Create instance of Backtrace Attribute
        /// </summary>
        /// <param name="assembly">Executed Assembly</param>
        public BacktraceAttributes(Assembly assembly)
        {
            _assembly = assembly;
            //A unique identifier to a machine
            Attributes.Add("guid", Guid.NewGuid().ToString());
            //Base name of application generating the report
            Attributes.Add("application", assembly.GetName().Name);

            SetProcessAttributes();
            SetMachineAttributes();
        }

        /// <summary>
        /// Create instance of Backtrace Attribute
        /// </summary>
        public BacktraceAttributes()
            : this(Assembly.GetExecutingAssembly())
        {

        }
        /// <summary>
        /// Set attributes from exception
        /// </summary>
        internal void SetExceptionAttributes(Exception exception)
        {
            if (exception == null)
            {
                return;
            }
            Attributes.Add("callstack", exception.StackTrace);
            Attributes.Add("classifier", exception.GetType().FullName);
            Attributes.Add("error.Message", exception.Message);
        }

        /// <summary>
        /// Set attributes from current process
        /// </summary>
        private void SetProcessAttributes()
        {
            var process = Process.GetCurrentProcess();

            //How long the application has been running, in seconds.
            TimeSpan processTime = DateTime.Now - process.StartTime;
            Attributes.Add("process.age", processTime.TotalSeconds.ToString());

            //Resident memory usage.
            Attributes.Add("vm.rss.size", process.PagedMemorySize64.ToString());

            //Peak resident memory usage.
            Attributes.Add("vm.rss.peak", process.PeakPagedMemorySize64.ToString());

            //Virtual memory usage
            Attributes.Add("vm.vma.size", process.VirtualMemorySize64.ToString());

            //Peak virtual memory usage
            Attributes.Add("vm.wma.peak", process.PeakVirtualMemorySize64.ToString());

            //Available physical memory
            //Attributes.Add("vm.rss.available", process.memo)
            
            //Available virtual memory.
            //Attributes.Add("vm.vma.available", )
        }

        /// <summary>
        /// Set attributes about current machine
        /// </summary>
        private void SetMachineAttributes()
        {
            //The processor architecture.
            Attributes.Add("uname.machine", Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));

            //Operating system name, such as "windows"
            Attributes.Add("uname.sysname", Environment.OSVersion.Platform.ToString());

            //The version of the operating system
            Attributes.Add("uname.version", Environment.OSVersion.Version.ToString());
            
            //The count of processors on the system
            Attributes.Add("cpu.count", Environment.ProcessorCount.ToString());
            
            //CPU brand string or type.
            Attributes.Add("cpu.brand", Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));

            //The hostname of the crashing system.
            Attributes.Add("hostname", Environment.MachineName);
        }
    }
}
