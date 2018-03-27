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
        public Dictionary<string, string> Dependencies
        {
            get
            {
                return appDependencies.AvailableDependencies;
            }
        }

        /// <summary>
        /// Get built-in complex attributes
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> ComplexAttributes = new Dictionary<string, object>();

        /// <summary>
        /// System environment variables
        /// </summary>
        private readonly EnvironmentVariables environment;

        /// <summary>
        /// Executed application dependencies
        /// </summary>
        private readonly ApplicationDependencies appDependencies;

        /// <summary>
        /// Create new instance of Annotations class
        /// </summary>
        /// <param name="callingAssembly">Calling assembly</param>
        /// <param name="complexAttributes">Built-in complex attributes</param>
        public Annotations(Assembly callingAssembly, Dictionary<string, object> complexAttributes)
        {
            appDependencies = new ApplicationDependencies(callingAssembly);
            environment = new EnvironmentVariables();
            ComplexAttributes = complexAttributes;
        }
    }
}
