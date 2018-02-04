using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    [Serializable]
    internal class Achitecture
    {
        public string Name = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");
    }
}
