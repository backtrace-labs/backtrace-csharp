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
        /// <param name="assembly">Current assembly</param>
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
            var referencedAssemblies = assembly.GetReferencedAssemblies();
            foreach (var refAssembly in referencedAssemblies)
            {
                var dependency = new Dependency()
                {
                    RequestedVersion = refAssembly.Version.ToString(),
                    InstalledVersion = refAssembly.Version.ToString()

                };
                AvailableDependencies.Add(refAssembly.Name, dependency);

            }
        }

    }
}
