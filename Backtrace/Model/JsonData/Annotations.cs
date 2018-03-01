using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Get report annotations - environment variables and application dependencies
    /// </summary>
    public class Annotations
    {

        /// <summary>
        /// Get system environment variables
        /// </summary>
        [JsonProperty(PropertyName = "Environment Variables")]
        public Dictionary<string, string> EnvironmentVariables
        {
            get
            {
                return environment.Variables;
            }
        }

        /// <summary>
        /// Get application dependencies
        /// </summary>
        [JsonProperty(PropertyName = "Dependencies")]
        public Dictionary<string, Dependency> Dependencies
        {
            get
            {
                return appDependencies.AvailableDependencies;
            }
        }

        /// <summary>
        /// System environment variables
        /// </summary>
        private readonly EnvironmentVariables environment;

        /// <summary>
        /// Executed application dependencies
        /// </summary>
        private readonly ApplicationDependencies appDependencies;

        public Annotations(Assembly callingAssembly)
        {
            appDependencies = new ApplicationDependencies(callingAssembly);
            environment = new EnvironmentVariables();
        }
    }
}
