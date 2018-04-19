using Backtrace.Base;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Backtrace.Model.Database
{
    /// <summary>
    /// Single entry in BacktraceDatabase
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BacktraceDatabaseEntry<T>
    {
        /// <summary>
        /// Entry Id
        /// </summary>
        [JsonProperty]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Path to json stored all information about current entry
        /// </summary>
        [JsonProperty(PropertyName = "entryName")]
        internal string EntryPath { get; set; }

        /// <summary>
        /// Path to a diagnostic data json
        /// </summary>
        [JsonProperty(PropertyName = "dataPath")]
        internal string DiagnosticDataPath { get; set; }

        /// <summary>
        /// Path to minidump file
        /// </summary>
        [JsonProperty(PropertyName = "minidumpPath")]
        internal string MiniDumpPath { get; set; }

        /// <summary>
        /// Path to minidump file
        /// </summary>
        [JsonProperty(PropertyName = "reportPath")]
        internal string ReportPath { get; set; }

        [JsonIgnore]
        public BacktraceData<T> BacktraceData
        {
            get
            {
                //read json files stored in BacktraceDatabase
                using (var dataReader = new StreamReader(DiagnosticDataPath))
                using (var reportReader = new StreamReader(ReportPath))
                {
                    //read report
                    var reportJson = reportReader.ReadToEnd();
                    var report = JsonConvert.DeserializeObject<BacktraceReportBase<T>>(reportJson);

                    //read diagnostic data
                    var diagnosticDataJson = dataReader.ReadToEnd();
                    var diagnosticData = JsonConvert.DeserializeObject<BacktraceData<T>>(diagnosticDataJson);
                    //add report to diagnostic data
                    //we don't store report with diagnostic data in the same json
                    //because we have easier way to serialize and deserialize data
                    //and no problem/condition with serialization when BacktraceApi want to send diagnostic data to API
                    diagnosticData.Report = report;
                    return diagnosticData;
                }
            }
        }

        [JsonConstructor]
        internal BacktraceDatabaseEntry()
        {
            EntryPath = $"_entry-{Id}.json";
        }

        public BacktraceDatabaseEntry(BacktraceData<T> data, string path)
        {
            Id = data.Uuid;
            DiagnosticDataPath = Save(data, "_attachment", path);
            ReportPath = Save(data.Report, "_report", path);
            MiniDumpPath = data.Report.MinidumpFile;
            EntryPath = Path.Combine(path, $"_entry-{Id}.json");
            Save(this, "_entry", path);
        }

        internal void Delete()
        {
            File.Delete(DiagnosticDataPath);
            File.Delete(EntryPath);
            File.Delete(ReportPath);
            File.Delete(MiniDumpPath);
        }

        internal string Save(object o, string jsonPrefix, string path)
        {
            string json = JsonConvert.SerializeObject(o);
            return Save(json, jsonPrefix, path);
        }

        private string Save(string json, string jsonPrefix, string path)
        {
            byte[] file = Encoding.UTF8.GetBytes(json);
            string filename = $"{jsonPrefix}-{Id}.json";
            string filePath = Path.Combine(path, filename);
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(file, 0, file.Length);
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return filePath;
        }
    }
}
