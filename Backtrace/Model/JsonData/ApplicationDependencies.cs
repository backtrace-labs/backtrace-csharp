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
        /// All listed dependencies with version
        /// </summary>
        [JsonProperty(PropertyName = "dependencies")]
        public Dictionary<string, string> AvailableDependencies = new Dictionary<string, string>();

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
            if(assembly == null)
            {
                return;
            }
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
                AvailableDependencies.Add(refAssembly.Name, refAssembly.Version.ToString());
            }
        }

    }
}
