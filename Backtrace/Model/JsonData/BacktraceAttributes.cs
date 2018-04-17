using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Linq;
using Backtrace.Base;
using System.Net.NetworkInformation;
using Backtrace.Common;
using Backtrace.Extensions;
using System.Reflection;

[assembly: InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Class instance to get a built-in attributes from current application
    /// </summary>
    public class BacktraceAttributes<T>
    {
        /// <summary>
        /// Get built-in primitive attributes
        /// </summary>
        public Dictionary<string, object> Attributes = new Dictionary<string, object>();

        /// <summary>
        /// Get built-in complex attributes
        /// </summary>
        public Dictionary<string, object> ComplexAttributes = new Dictionary<string, object>();

        /// <summary>
        /// Create instance of Backtrace Attribute
        /// </summary>
        /// <param name="report">Received report</param>
        /// <param name="scopedAttributes">Client's attributes (report and client)</param>
        public BacktraceAttributes(BacktraceReportBase<T> report, Dictionary<string, T> scopedAttributes)
        {
            //Environment attributes override user attributes
            ConvertAttributes(report, scopedAttributes);
            SetLibraryAttributes(report.CallingAssembly);            
            SetDebuggerAttributes(report.CallingAssembly);
            SetMachineAttributes();
            SetProcessAttributes();
            SetExceptionAttributes(report);
        }

        internal BacktraceAttributes()
        {

        }

        /// <summary>
        /// Set library attributes
        /// </summary>
        /// <param name="callingAssembly">Calling assembly</param>
        private void SetLibraryAttributes(Assembly callingAssembly)
        {
            //A unique identifier of a machine
            Attributes["guid"] = GenerateMachineId().ToString();
            //Base name of application generating the report
            Attributes["application"] = callingAssembly.GetName().Name;
            Attributes["lang.name"] = "C#";

        }

        /// <summary>
        /// Set debugger information
        /// </summary>
        /// <param name="callingAssembly">Calling assembly</param>
        private void SetDebuggerAttributes(Assembly callingAssembly)
        {
            object[] attribs = callingAssembly.GetCustomAttributes(typeof(DebuggableAttribute), false);
            // If the 'DebuggableAttribute' is not found then it is definitely an OPTIMIZED build
            if (attribs == null || !attribs.Any())
            {
                Attributes["build.debug"] = false;
                Attributes["build.jit"] = true;
                Attributes["build.type"] = "Release";
            }
            // Just because the 'DebuggableAttribute' is found doesn't necessarily mean
            // it's a DEBUG build; we have to check the JIT Optimization flag
            // i.e. it could have the "generate PDB" checked but have JIT Optimization enabled
            if (attribs[0] is DebuggableAttribute debuggableAttribute)
            {
                Attributes["build.debug"] = true;
                Attributes["build.jit"] = !debuggableAttribute.IsJITOptimizerDisabled;
                Attributes["build.type"] = debuggableAttribute.IsJITOptimizerDisabled 
                    ? "Debug" : "Release";
                // check for Debug Output "full" or "pdb-only"
                Attributes["build.output"] = (debuggableAttribute.DebuggingFlags &
                                DebuggableAttribute.DebuggingModes.Default) !=
                                DebuggableAttribute.DebuggingModes.None
                                ? "Full" : "pdb-only";
            }
            Attributes["build.debug"] = false;
        }

        /// <summary>
        /// Convert custom user attributes
        /// </summary>
        /// <param name="report">Received report</param>
        /// <param name="clientAttributes">Client's attributes (report and client)</param>
        /// <returns>Dictionary of custom user attributes </returns>
        private void ConvertAttributes(BacktraceReportBase<T> report, Dictionary<string, T> clientAttributes)
        {
            var attributes = BacktraceReportBase<T>.ConcatAttributes(report, clientAttributes);
            foreach (var attribute in attributes)
            {
                var type = attribute.Value.GetType();
                if (type.IsPrimitive || type == typeof(string))
                {
                    Attributes.Add(attribute.Key, attribute.Value);
                }
                else
                {
                    ComplexAttributes.Add(attribute.Key, attribute.Value);
                }
            }
        }

        /// <summary>
        /// Generate unique machine identifier. Value should be with guid key in Attributes dictionary. 
        /// Machine id is equal to mac address of first network interface. If network interface in unvailable, random long will be generated.
        /// </summary>
        /// <returns>Machine uuid</returns>
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
            Attributes["gc.heap.used"] = GC.GetTotalMemory(false);

            var process = Process.GetCurrentProcess();
            if (process.HasExited)
            {
                return;
            }
            //How long the application has been running  in secounds
            Attributes["process.age"] = Math.Round(process.TotalProcessorTime.TotalSeconds);
            try
            {
                Attributes["cpu.process.count"] = Process.GetProcesses().Count();

                //Resident memory usage.
                int pagedMemorySize = unchecked((int)(process.PagedMemorySize64 / 1024));
                pagedMemorySize = pagedMemorySize == -1 ? int.MaxValue : pagedMemorySize;
                if (pagedMemorySize > 0)
                {
                    Attributes["vm.rss.size"] = pagedMemorySize;
                }

                //Peak resident memory usage.
                int peakPagedMemorySize = unchecked((int)(process.PeakPagedMemorySize64 / 1024));
                peakPagedMemorySize = peakPagedMemorySize == -1 ? int.MaxValue : peakPagedMemorySize;
                if (peakPagedMemorySize > 0)
                {
                    Attributes["vm.rss.peak"] = peakPagedMemorySize;
                }

                //Virtual memory usage
                int virtualMemorySize = unchecked((int)(process.VirtualMemorySize64 / 1024));
                virtualMemorySize = virtualMemorySize == -1 ? int.MaxValue : virtualMemorySize;
                if (virtualMemorySize > 0)
                {
                    Attributes["vm.vma.size"] = virtualMemorySize;
                }

                //Peak virtual memory usage
                int peakVirtualMemorySize = unchecked((int)(process.PeakVirtualMemorySize64 / 1024));
                peakVirtualMemorySize = peakVirtualMemorySize == -1 ? int.MaxValue : peakVirtualMemorySize;
                if (peakVirtualMemorySize > 0)
                {
                    Attributes["vm.vma.peak"] = peakVirtualMemorySize;
                }
            }
            catch(PlatformNotSupportedException)
            {
                Trace.TraceWarning($"Cannot retrieve information about process memory - platform not supported");
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
                Attributes["cpu.count"] = cpuCount;
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
            if (boottime <= 0)
            {
                boottime = int.MaxValue;
            }
            Attributes["cpu.boottime"] = boottime;

            //The hostname of the crashing system.
            Attributes["hostname"] = Environment.MachineName;
        }
    }
}
