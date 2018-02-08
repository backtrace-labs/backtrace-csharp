using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{

    public class EnvironmentVariables
    {
        [JsonProperty(PropertyName = "Environment Variables")]
        public Dictionary<string, string> Variables = new Dictionary<string, string>();
        public EnvironmentVariables()
        {
            ReadEnvironmentVariables();
        }

        private void ReadEnvironmentVariables()
        {
            foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
            {
                Variables.Add(variable.Key.ToString(), variable.Value.ToString());
            }
        }
    }
}
