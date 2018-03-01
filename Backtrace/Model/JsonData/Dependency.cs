using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Assembly dependency information
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// Requested assembly version
        /// </summary>
        [JsonProperty(PropertyName = "requestedVersion")]
        public string RequestedVersion { get; set; }

        /// <summary>
        /// Installed assembly version
        /// </summary>
        [JsonProperty(PropertyName = "installedVersion")]
        public string InstalledVersion { get; set; }
    }
}
