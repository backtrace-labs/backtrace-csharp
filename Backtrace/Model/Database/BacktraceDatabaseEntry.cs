using Backtrace.Base;
using Backtrace.Interfaces.Database;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
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
        [JsonIgnore]
        internal virtual BacktraceData<T> Entry { get; set; }

        /// <summary>
        /// Path to database directory
        /// </summary>
        [JsonIgnore]
        private readonly string _path = string.Empty;

        /// <summary>
        /// Entry writer
        /// </summary>
        [JsonIgnore]
        internal IBacktraceDatabaseEntryWriter EntryWriter;

        /// <summary>
        /// Get valid BacktraceData from current entry
        /// </summary>
        [JsonIgnore]
        public virtual BacktraceData<T> BacktraceData
        {
            get
            {
                if (Entry != null)
                {
                    return Entry;
                }
                if (!Valid())
                {
                    return null;
                }
                //read json files stored in BacktraceDatabase
                using (var dataReader = new StreamReader(DiagnosticDataPath))
                using (var reportReader = new StreamReader(ReportPath))
                {
                    //read diagnostic data
                    var diagnosticDataJson = dataReader.ReadToEnd();
                    //read report
                    var reportJson = reportReader.ReadToEnd();

                    //deserialize data - if deserialize fails, we receive invalid entry
                    try
                    {
                        var diagnosticData = JsonConvert.DeserializeObject<BacktraceData<T>>(diagnosticDataJson);
                        var report = JsonConvert.DeserializeObject<BacktraceReportBase<T>>(reportJson);
                        //add report to diagnostic data
                        //we don't store report with diagnostic data in the same json
                        //because we have easier way to serialize and deserialize data
                        //and no problem/condition with serialization when BacktraceApi want to send diagnostic data to API
                        diagnosticData.Report = report;
                        return diagnosticData;
                    }
                    catch (SerializationException)
                    {
                        //catch all exception caused by invalid serialization
                        return null;
                    }
                }
            }
        }
        /// <summary>
        /// Constructor for serialization purpose
        /// </summary>
        [JsonConstructor]
        internal BacktraceDatabaseEntry()
        {
            EntryPath = $"{Id}-entry.json";
        }

        /// <summary>
        /// Create new instance of database entry
        /// </summary>
        /// <param name="data">Diagnostic data</param>
        /// <param name="path">database path</param>
        public BacktraceDatabaseEntry(BacktraceData<T> data, string path)
        {
            Id = data.Uuid;
            Entry = data;
            _path = path;
            EntryWriter = new BacktraceDatabaseEntryWriter(path);
        }

        /// <summary>
        /// Save data to hard drive
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            try
            {
                DiagnosticDataPath = EntryWriter.Write(Entry, $"{Id}-attachment");
                ReportPath = EntryWriter.Write(Entry.Report, $"{Id}-report");
                MiniDumpPath = Entry.Report?.MinidumpFile ?? string.Empty;
                EntryPath = Path.Combine(_path, $"{Id}-entry.json");
                EntryWriter.Write(this, $"{Id}-entry");
                return true;
            }
            catch (IOException)
            {
                Trace.WriteLine($"Received {nameof(IOException)} while saving data to database");
                return false;
            }
            catch (Exception)
            {
                Trace.WriteLine($"Received {nameof(Exception)} while saving data to database");
                return false;
            }
        }

        /// <summary>
        /// Check if all necessary files declared on entry exists
        /// </summary>
        /// <returns>True if entry is valid</returns>
        public bool Valid()
        {
            return File.Exists(DiagnosticDataPath) && File.Exists(ReportPath);
        }

        /// <summary>
        /// Delete all entry files
        /// </summary>
        internal virtual void Delete()
        {
            Delete(MiniDumpPath);
            Delete(ReportPath);
            Delete(DiagnosticDataPath);
            Delete(EntryPath);
        }

        /// <summary>
        /// Delete single file on database entry
        /// </summary>
        /// <param name="path">path to file</param>
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

        /// <summary>
        /// Read single entry from file
        /// </summary>
        /// <param name="file">Current file</param>
        /// <returns>Saved database entry</returns>
        internal static BacktraceDatabaseEntry<T> ReadFromFile(FileInfo file)
        {
            using (StreamReader streamReader = file.OpenText())
            {
                var json = streamReader.ReadToEnd();
                var entry = JsonConvert.DeserializeObject<BacktraceDatabaseEntry<T>>(json);
                return entry;
            }
        }
        #region dispose
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
                Entry = null;
            }
        }
        #endregion
    }
}
