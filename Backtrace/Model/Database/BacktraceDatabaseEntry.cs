using Backtrace.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Backtrace.Model.Database
{
    public class BacktraceDatabaseEntry<T>
    {
        [JsonProperty]
        public Guid Id { get; set; } = Guid.NewGuid();

        [JsonProperty(PropertyName = "entryName")]
        internal string EntryPath { get; set; }

        [JsonProperty(PropertyName = "attachmentPath")]
        internal string FilePath { get; set; }

        [JsonIgnore]
        public string JsonReport
        {
            get
            {
                using (var streamReader = new StreamReader(FilePath))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        [JsonProperty(PropertyName = "minidumpPath")]
        internal string MiniDumpPath { get; set; }

        [JsonProperty(PropertyName = "metadataPath")]
        internal string MetadataPath { get; set; }

        [JsonIgnore]
        public IEnumerable<string> Attachments
        {
            get
            {
                using (var streamReader = new StreamReader(MetadataPath))
                {
                    var json = streamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<IEnumerable<string>>(json);
                }
            }
        }

        [JsonConstructor]
        internal BacktraceDatabaseEntry()
        {
            EntryPath = $"_entry-{Id}.json";
        }

        internal BacktraceDatabaseEntry(BacktraceData<T> data, string path)
        {
            EntryPath = Path.Combine(path, $"_entry-{Id}.json");

            FilePath = Save(data, "_attachment", path);
            MiniDumpPath = data.Report.MinidumpFile;
            MetadataPath = Save(data.Report.AttachmentPaths, "_metadata", path);
            var entry = JsonConvert.SerializeObject(this);
            Save(entry, "_entry", path);
        }

        internal void Delete()
        {
            File.Delete(FilePath);
            File.Delete(MetadataPath);
            File.Delete(EntryPath);
            File.Delete(MiniDumpPath);
        }

        private string Save(object o, string jsonPrefix, string path)
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
