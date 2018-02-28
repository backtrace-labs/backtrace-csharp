using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Backtrace.Extensions
{
    /// <summary>
    /// Extensions method to dictionary data structure
    /// </summary>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Merge two dictionaries
        /// If there is any key conflict value from source dictionary is taken
        /// </summary>
        /// <param name="source">Source dictionary (dictionary from report)</param>
        /// <param name="toMerge">merged dictionary (</param>
        /// <returns>Merged dictionary</returns>
        internal static Dictionary<string, T> Merge<T>(
            this Dictionary<string, T> source, Dictionary<string, T> toMerge)
        {
            if(source == null)
            {
                throw new ArgumentException(nameof(source));
            }
            if(toMerge == null)
            {
                throw new ArgumentException(nameof(toMerge));
            }
            var result = new Dictionary<string, T>(source);
            foreach (var record in toMerge)
            {
                if (!result.ContainsKey(record.Key))
                {
                    result.Add(record.Key, record.Value);
                }
            }

            return result;
        }
    }
}
