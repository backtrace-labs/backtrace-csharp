using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    internal class BacktraceAttributes
    {
        /// <summary>
        /// Get
        /// </summary>
        public Dictionary<string, string> Attributes = new Dictionary<string, string>();

        public BacktraceAttributes(Exception exception)
        {
            //A unique identifier to a machine
            Attributes.Add("Guid", Guid.NewGuid().ToString());
        }

        private void SetProcessAttributes()
        {

        }

        private void SetMachineAttributes()
        {

        }
    }
}
