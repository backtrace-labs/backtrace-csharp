using Backtrace.Common;
using Backtrace.Model.JsonData;
using Backtrace.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Backtrace.Model
{
    internal class DeduplicationModel
    {
        private readonly BacktraceData _backtraceData;
        private readonly DeduplicationStrategy _strategy;
        public DeduplicationModel(
            BacktraceData backtraceData,
            DeduplicationStrategy strategy)
        {
            _backtraceData = backtraceData;
            _strategy = strategy;
        }
        public string[] StackTrace
        {
            get
            {
                if (_strategy == DeduplicationStrategy.None)
                {
                    return new string[0];
                }
                if (_backtraceData.Report == null || _backtraceData.Report.DiagnosticStack == null)
                {
                    System.Diagnostics.Debug.WriteLine("Report or diagnostic stack is null");
                }
                var result = _backtraceData.Report.DiagnosticStack
                    .Where(n => !SystemHelper.SystemAssembly(n.FunctionName))
                    .Select(n => n.FunctionName)
                    .OrderByDescending(n => n);

                return new HashSet<string>(result).ToArray();
            }
        }
        public string[] Classifier
        {
            get
            {
                if ((_strategy & DeduplicationStrategy.Classifier) == 0)
                {
                    return new string[0];
                }
                return _backtraceData.Classifier;
            }
        }
        public string ExceptionMessage
        {
            get
            {
                if ((_strategy & DeduplicationStrategy.Message) == 0)
                {
                    return string.Empty;
                }
                return _backtraceData.Report.Message;
            }
        }
        public string Application
        {
            get
            {
                if ((_strategy & DeduplicationStrategy.Application) == 0)
                {
                    return string.Empty;
                }
                string key = BacktraceAttributes.APPLICATION_ATTRIBUTE_NAME;
                return _backtraceData.Attributes[key] as string;
            }
        }

        public string GetSha()
        {
            string json = JsonConvert.SerializeObject(this);
            using (var sha1 = new SHA1Managed())
            {
                var bytes = Encoding.ASCII.GetBytes(json);
                var hash = sha1.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            };
        }
    }
}
