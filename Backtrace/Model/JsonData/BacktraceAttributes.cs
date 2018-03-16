using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq;
using Backtrace.Base;
using Newtonsoft.Json;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using Backtrace.Common;
using Backtrace.Extensions;

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

        private Guid GenerateMachineId()
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.OperationalStatus == OperationalStatus.Up);

            PhysicalAddress physicalAddr = null;
            string macAddress = null;
            if (networkInterface == null
                || (physicalAddr = networkInterface.GetPhysicalAddress()) == null
                || string.IsNullOrEmpty(macAddress = physicalAddr.ToString()))
            {
                return Guid.NewGuid();
            }

            string hex = macAddress.Replace(":", string.Empty);
            var value = Convert.ToInt64(hex, 16);
            return GuidExtensions.FromLong(value);
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
            Attributes["gc.heap.used"] = GC.GetTotalMemory(false).ToString();

            var process = Process.GetCurrentProcess();
            if (process.HasExited)
            {
                return;
            }
            //How long the application has been running] = in millisecounds
            Attributes["process.age"] = Math.Round(process.TotalProcessorTime.TotalMilliseconds).ToString();
            try
            {
                Attributes["cpu.process.count"] = Process.GetProcesses().Count().ToString();

                //Resident memory usage.
                var pagedMemorySize = process.PagedMemorySize64;
                if (pagedMemorySize > 0)
                {
                    Attributes["vm.rss.size"] = pagedMemorySize.ToString();
                }

                //Peak resident memory usage.
                var peakPagedMemorySize = process.PeakPagedMemorySize64;
                if (peakPagedMemorySize > 0)
                {
                    Attributes["vm.rss.peak"] = peakPagedMemorySize.ToString();
                }

                //Virtual memory usage
                var virtualMemorySize = process.VirtualMemorySize64;
                if (virtualMemorySize > 0)
                {
                    Attributes["vm.vma.size"] = virtualMemorySize.ToString();
                }

                //Peak virtual memory usage
                var peakVirtualMemorySize = process.PeakVirtualMemorySize64;
                if (peakVirtualMemorySize > 0)
                {
                    Attributes["vm.vma.peak"] = peakVirtualMemorySize.ToString();
                }
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
            string cpuArchitecture = SystemHelper.CpuArchitecture();
            Attributes["uname.machine"] = cpuArchitecture;

            //Operating system name = such as "windows"
            Attributes["uname.sysname"] = SystemHelper.Name(cpuArchitecture);

            //The version of the operating system
            Attributes["uname.version"] = Environment.OSVersion.Version.ToString();

            //The count of processors on the system
            var cpuCount = Environment.ProcessorCount;
            if (cpuCount > 0)
            {
                Attributes["cpu.count"] = cpuCount.ToString();
            }

            //CPU brand string or type.
            //Because System.Management is not supported in .NET Core value should be available in Backtrace API in future work (.NET Standard 2.1>)
            string cpuBrand = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");
            if (!string.IsNullOrEmpty(cpuBrand))
            {
                Attributes["cpu.brand"] = cpuBrand;
            }
            //Time when system was booted
            int boottime = Environment.TickCount;
            string bootTimeString = boottime.ToString();
            if (boottime <= 0)
            {
                bootTimeString = "More than 30 days";
            }
            Attributes["cpu.boottime"] = bootTimeString;

            //The hostname of the crashing system.
            Attributes["hostname"] = Environment.MachineName;
        }
    }
}
