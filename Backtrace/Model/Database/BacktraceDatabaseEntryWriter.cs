using Backtrace.Interfaces.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Backtrace.Model.Database
{
    /// <summary>
    /// Database entry writer
    /// </summary>
    internal class BacktraceDatabaseEntryWriter: IBacktraceDatabaseEntryWriter
    {
        /// <summary>
        /// Path to destination directory
        /// </summary>
        private readonly string _destinationPath;

        /// <summary>
        /// Initialize new database entry writer
        /// </summary>
        /// <param name="path">Path to destination folder</param>
        internal BacktraceDatabaseEntryWriter(string path)
        {
            _destinationPath = path;
        }

        public virtual string Write(object data, string prefix)
        {
            string json = JsonConvert.SerializeObject(data);
            byte[] file = Encoding.UTF8.GetBytes(json);
            return Write(file, prefix);
        }

        public virtual string Write(byte[] data, string prefix)
        {
            string filename = $"{prefix}.json";
            string tempFilePath = Path.Combine(_destinationPath, $"temp_{filename}");
            try
            {
                SaveTemporaryFile(tempFilePath, data);
                string destFilePath = Path.Combine(_destinationPath, filename);
                File.Move(tempFilePath, destFilePath);
                return destFilePath;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Save temporary file to hard drive.
        /// </summary>
        /// <param name="path">Path to temporary file</param>
        /// <param name="file">Current file</param>
        public virtual void SaveTemporaryFile(string path, byte[] file)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                fs.Write(file, 0, file.Length);
            }
        }
    }
}
