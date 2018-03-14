﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Backtrace.Model.JsonData
{
    /// <summary>
    /// System environment variables
    /// </summary>
    public class EnvironmentVariables
    {
        /// <summary>
        /// System environment values dictionary
        /// </summary>
        public Dictionary<string, string> Variables = new Dictionary<string, string>();

        /// <summary>
        /// Create instance of EnvironmnetVariables class to get system environment variables
        /// </summary>
        public EnvironmentVariables()
        {
            ReadEnvironmentVariables();
        }

        /// <summary>
        /// Read all environment variables from system
        /// </summary>
        private void ReadEnvironmentVariables()
        {
            foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
            {
                Variables.Add(variable.Key.ToString(), Regex.Escape(variable.Value.ToString()));
            }
        }
    }
}
