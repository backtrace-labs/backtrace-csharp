using Backtrace.Model;
using Backtrace.Model.Types;
using System;

namespace Backtrace.Services
{
    public class DeduplicationService
    {
        public DeduplicationStrategy Strategy { get; set; }
        public DeduplicationService(DeduplicationStrategy strategy)
        {
            Strategy = strategy;
        }

        public string GetSha(BacktraceData backtraceData)
        {
            throw new NotImplementedException();
        }
    }
}
