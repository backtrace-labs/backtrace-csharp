using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model.JsonData
{
    [Serializable]
    internal class Achitecture
    {
        [JsonProperty(PropertyName = "name")]
        public string Name = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
    }
}
