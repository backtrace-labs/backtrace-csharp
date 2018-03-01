using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Backtrace.Extensions
{
    /// <summary>
    /// Assembly Extensions used in Backtrace Library
    /// </summary>
    internal static class AssemblyExtensions
    {
        /// <summary>
        /// Get all namesaces from assembly
        /// </summary>
        /// <param name="assembly">Calling assembly</param>
        /// <returns>Available namespaces in assembly</returns>
        internal static IEnumerable<string> GetNamespaces(this Assembly assembly)
        {
            HashSet<string> result = new HashSet<string>();
            if (assembly == null)
            {
                return result;
            }

            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i].Namespace == null)
                {
                    continue;
                }
                result.Add(types[i].Namespace);
            }
            return result;

        }
    }
}
