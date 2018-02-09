using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model.JsonData
{
    //TODO: Read register properties from processor

    /// <summary>
    /// Get an information about process architecture
    /// </summary>
    internal class Achitecture
    {
        /// <summary>
        /// Get a processor achitecture
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
    }
}
