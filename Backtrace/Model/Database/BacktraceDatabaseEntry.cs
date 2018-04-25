using Backtrace.Base;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
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
    public class BacktraceDatabaseEntry<T> : IDisposable
    {
        /// <summary>
        /// Entry Id
        /// </summary>
        [JsonProperty]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Check if current entry is in use
        /// </summary>
        [JsonIgnore]
        internal bool Locked { get; set; } = false;

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
        /// Path to Backtrace Report json
        /// </summary>
        [JsonProperty(PropertyName = "reportPath")]
        internal string ReportPath { get; set; }

        /// <summary>
        /// Stored entry
        /// </summary>
        private BacktraceData<T> _entry;

        [JsonIgnore]
        public BacktraceData<T> BacktraceData
        {
            get
            {
                if (_entry != null)
                {
                    return _entry;
                }
                if(!Valid())
                {
                    return null;
                }
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
            EntryPath = $"{Id}-entry.json";
        }

        public BacktraceDatabaseEntry(BacktraceData<T> data, string path)
        {
            Id = data.Uuid;
            _entry = data;
            DiagnosticDataPath = Save(data, "attachment", path);
            ReportPath = Save(data.Report, "report", path);
            MiniDumpPath = data.Report.MinidumpFile;
            EntryPath = Path.Combine(path, $"{Id}-entry.json");
            Save(this, "entry", path);
        }

        /// <summary>
        /// Check if all necessary files declared on entry exists
        /// </summary>
        /// <returns>True if entry is valid</returns>
        public bool Valid()
        {
            return File.Exists(DiagnosticDataPath) && File.Exists(ReportPath);
        }

        internal virtual void Delete()
        {
            Delete(MiniDumpPath);
            Delete(ReportPath);
            Delete(DiagnosticDataPath);
            Delete(EntryPath);
            
        }
        private void Delete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (IOException e)
            {
                Trace.WriteLine($"File {path} is in use. Message: {e.Message}");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Cannot delete file: {path}. Message: {e.Message}");
            }
        }

        internal virtual string Save(object o, string jsonPrefix, string path)
        {
            string json = JsonConvert.SerializeObject(o);
            return Save(json, jsonPrefix, path);
        }

        internal static BacktraceDatabaseEntry<T> ReadFromFile(FileInfo file)
        {
            using (StreamReader streamReader = file.OpenText())
            {
                var json = streamReader.ReadToEnd();
                var entry = JsonConvert.DeserializeObject<BacktraceDatabaseEntry<T>>(json);
                return entry;
            }
        }

        private string Save(string json, string jsonPrefix, string path)
        {
            byte[] file = Encoding.UTF8.GetBytes(json);
            string filename = $"{Id}-{jsonPrefix}.json";
            string tempPath = Path.Combine(path, $"temp_{filename}");
            string filePath = Path.Combine(path, filename);
            try
            {
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(file, 0, file.Length);
                }
                //rename temp file - we are sure our report exists
                File.Move(tempPath, filePath);
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return filePath;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Locked = false;
                _entry = null;
            }
        }
    }
}
