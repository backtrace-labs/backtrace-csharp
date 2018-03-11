using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Backtrace API response object
    /// </summary>
    public class BacktraceServerResponse
    {
        /// <summary>
        /// Http status code
        /// </summary>
        [JsonProperty(PropertyName ="response")]
        public string Response { get; set; }

        /// <summary>
        /// Created object id
        /// </summary>
        [JsonProperty(PropertyName = "object")]
        public string Object { get; set; }
    }
}