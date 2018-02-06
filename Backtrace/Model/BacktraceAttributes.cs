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
            Attributes.Add("Guid", Guid.NewGuid().ToString());
            //Base name of application generating the report
            Attributes.Add("Application", assembly.GetName().Name);

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
            Attributes.Add("Callstack", exception.StackTrace);
            Attributes.Add("Classifier", exception.GetType().FullName);
            Attributes.Add("Error.Message", exception.Message);
        }

        /// <summary>
        /// Set attributes from current process
        /// </summary>
        private void SetProcessAttributes()
        {
            var process = Process.GetCurrentProcess();

            //How long the application has been running, in seconds.
            TimeSpan processTime = DateTime.Now - process.StartTime;
            Attributes.Add("Process.Age", processTime.TotalSeconds.ToString());

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
            Attributes.Add("Uname.Machine", Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE"));

            //Operating system name, such as "windows"
            Attributes.Add("Uname.Sysname", Environment.OSVersion.Platform.ToString());

            //The version of the operating system
            Attributes.Add("Uname.Version", Environment.OSVersion.Version.ToString());
            
            //The count of processors on the system
            Attributes.Add("Cpu.Count", Environment.ProcessorCount.ToString());
            
            //CPU brand string or type.
            Attributes.Add("Cpu.Brand", Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"));

            //The hostname of the crashing system.
            Attributes.Add("Hostname", Environment.MachineName);
        }
    }
}
