using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// Assembly dependency information
    /// </summary>
    internal class Dependency
    {
        /// <summary>
        /// Requested assembly version
        /// </summary>
        [JsonProperty(PropertyName = "requestedVersion")]
        internal string RequestedVersion { get; set; }

        /// <summary>
        /// Installed assembly version
        /// </summary>
        [JsonProperty(PropertyName = "installedVersion")]
        internal string InstalledVersion { get; set; }
    }
}
