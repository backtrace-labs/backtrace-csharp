using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Runtime.CompilerServices;
using System.Linq;
using Backtrace.Base;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

[assembly: InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Class instance to get a built-in attributes from current application
    /// </summary>
    public class BacktraceAttributes<T>
    {
        /// <summary>
        /// Get built-in attributes
        /// </summary>
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        /// <summary>
        /// Create instance of Backtrace Attribute
        /// </summary>
        /// <param name="report">Received report</param>
        /// <param name="scopedAttributes">Client scoped attributes</param>
        public BacktraceAttributes(BacktraceReportBase<T> report, Dictionary<string, T> scopedAttributes)
        {
            //Environment attributes override user attributes
            Attributes = BacktraceReportBase<T>.ConcatAttributes(report, scopedAttributes)
                .ToDictionary(n => n.Key, v => JsonConvert.SerializeObject(v.Value));

            //A unique identifier of a machine
            Attributes["guid"] = GenerateMachineId().ToString();
            //Base name of application generating the report
            Attributes["application"] = report.CallingAssembly.GetName().Name;
            SetMachineAttributes();
            SetProcessAttributes();
            SetExceptionAttributes(report);
        }

        /// <summary>
        /// Generate unique machine identifier. Value should be with guid key in Attributes dictionary. 
        /// Machine id is equal to mac address of first network interface. If network interface in unvailable, random long will be generated.
        /// </summary>
        /// <returns></returns>
        private UInt64 GenerateMachineId()
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault();
            if (networkInterface == null)
            {
                Random random = new Random();
                return (UInt64)(random.NextDouble() * UInt64.MaxValue);
            }
            string macAddress = networkInterface.GetPhysicalAddress().ToString();
            string hex = macAddress.Replace(":", string.Empty);

            return Convert.ToUInt64(hex, 16);
        }

        /// <summary>
        /// Set attributes from exception
        /// </summary>
        internal void SetExceptionAttributes(BacktraceReportBase<T> report)
        {
            //there is no information to analyse
            if (report == null)
            {
                return;
            }
            if (!report.ExceptionTypeReport)
            {
                Attributes["error.message"] = report.Message;
                return;
            }
            var exception = report.Exception;
            Attributes["classifier"] = exception.GetType().FullName;
            Attributes["error.message"] = exception.Message;
        }

        /// <summary>
        /// Set attributes from current process
        /// </summary>
        private void SetProcessAttributes()
        {
            var process = Process.GetCurrentProcess();

            //How long the application has been running] = in millisecounds
            Attributes["process.age"] = Math.Round(process.TotalProcessorTime.TotalMilliseconds).ToString();
            Attributes["gc.heap.used"] = GC.GetTotalMemory(false).ToString();
            try
            {
                Attributes["cpu.process.count"] = Process.GetProcesses().Count().ToString();

                //Resident memory usage.
                Attributes["vm.rss.size"] = process.PagedMemorySize64.ToString();

                //Peak resident memory usage.
                Attributes["vm.rss.peak"] = process.PeakPagedMemorySize64.ToString();

                //Virtual memory usage
                Attributes["vm.vma.size"] = process.VirtualMemorySize64.ToString();

                //Peak virtual memory usage
                Attributes["vm.vma.peak"] = process.PeakVirtualMemorySize64.ToString();
            }
            catch (Exception exception)
            {
                Trace.TraceWarning($"Cannot retrieve information about process memory: ${exception.Message}");
            }
        }

        /// <summary>
        /// Set attributes about current machine
        /// </summary>
        private void SetMachineAttributes()
        {
            //The processor architecture.
            Attributes["uname.machine"] = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            //Operating system name] = such as "windows"
            Attributes["uname.sysname"] = Environment.OSVersion.Platform.ToString();

            //The version of the operating system
            Attributes["uname.version"] = Environment.OSVersion.Version.ToString();

            //The count of processors on the system
            var cpuCount = Environment.ProcessorCount;
            if (cpuCount > 0)
            {
                Attributes["cpu.count"] = cpuCount.ToString();
            }

            //CPU brand string or type.
            string cpuBrand = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            if (!string.IsNullOrEmpty(cpuBrand))
            {
                Attributes["cpu.brand"] = cpuBrand;
            }

            //Time when system was booted
            Attributes["cpu.boottime"] = Environment.TickCount.ToString();

            //The hostname of the crashing system.
            Attributes["hostname"] = Environment.MachineName;
        }
    }
}
