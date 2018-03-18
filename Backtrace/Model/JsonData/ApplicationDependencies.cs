using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Get all application dependencies
    /// </summary>
    public class ApplicationDependencies
    {
        /// <summary>
        /// All listed dependencies
        /// </summary>
        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, Dependency> AvailableDependencies = new Dictionary<string, Dependency>();

        /// <summary>
        /// Create new instance of application dependecies object
        /// </summary>
        /// <param name="assembly">Calling assembly</param>
        public ApplicationDependencies(Assembly assembly)
        {
            ReadDependencies(assembly);
        }

        /// <summary>
        /// Parse all dependencies from assembly to dependency dictionary
        /// </summary>
        /// <param name="assembly">Current assembly</param>
        private void ReadDependencies(Assembly assembly)
        {
            AssemblyName[] referencedAssemblies = null;
            try
            {
                referencedAssemblies = assembly.GetReferencedAssemblies();
            }
            catch(Exception)
            {
                return;
            }
            foreach (var refAssembly in referencedAssemblies)
            {
                var dependency = new Dependency()
                { 
                    InstalledVersion = refAssembly.Version.ToString()
                };
                AvailableDependencies.Add(refAssembly.Name, dependency);
            }
        }

    }
}
