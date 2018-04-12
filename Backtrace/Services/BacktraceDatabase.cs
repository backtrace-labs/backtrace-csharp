using Backtrace.Base;
using Backtrace.Common;
using Backtrace.Interfaces;
using Backtrace.Model;
using Backtrace.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
#if !NET35
using System.Threading.Tasks;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Services
{
    /// <summary>
    /// Backtrace Database 
    /// </summary>
    public class BacktraceDatabase<T> : IBacktraceDatabase<T>
    {
        /// <summary>
        /// In memory database
        /// </summary>
        Dictionary<uint, List<string>> Database = new Dictionary<uint, List<string>>();


        /// <summary>
        /// Database settings
        /// </summary>
        public BacktraceDatabaseSettings DatabaseSettings { get; private set; }

        /// <summary>
        /// Database path
        /// </summary>
        private string DatabasePath
        {
            get
            {
                return DatabaseSettings.DatabasePath;
            }
        }

        /// <summary>
        /// Backtrace Api instance. Use BacktraceApi to send data to Backtrace server
        /// </summary>
        private IBacktraceApi<T> _backtraceApi;

        /// <summary>
        /// Determine if BacktraceDatabase is enable and library can store reports
        /// </summary>
        private readonly bool _enable = true;

        private const string reportPrefix = "attachment_";

        /// <summary>
        /// Create Backtrace database instance
        /// </summary>
        /// <param name="databaseSettings">Backtrace database settings</param>
        public BacktraceDatabase(BacktraceDatabaseSettings databaseSettings)
        {
            if (databaseSettings == null || string.IsNullOrEmpty(databaseSettings.DatabasePath))
            {
                _enable = false;
                return;
            }
            DatabaseSettings = databaseSettings;
            ValidateDatabaseDirectory();
            LoadReports();
        }

        /// <summary>
        /// Set BacktraceApi instance
        /// </summary>
        /// <param name="backtraceApi">BacktraceApi instance</param>
        public void SetApi(IBacktraceApi<T> backtraceApi)
        {
            _backtraceApi = backtraceApi;
        }

        /// <summary>
        /// Delete all existing files and directories in current database directory
        /// </summary>
        public void Clear()
        {
            var directoryInfo = new DirectoryInfo(DatabasePath);
            IEnumerable<FileInfo> files = directoryInfo.GetFiles();
            IEnumerable<DirectoryInfo> directories = directoryInfo.GetDirectories();

            foreach (FileInfo file in files)
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in directories)
            {
                dir.Delete(true);
            }
            Database.Clear();
        }


        public void Flush()
        {
            //Flush method should USE data structure to store reports!!

            //this code is right now deprecated 
            //we can use this in future
            Database.Clear();

            //var directoryInfo = new DirectoryInfo(DatabasePath);
            //var files = directoryInfo.GetFiles();
            //foreach (var @file in files)
            //{
            //    if (!file.Name.StartsWith("attachment_"))
            //    {
            //        continue;
            //    }

            //}
        }
#if !NET35
        public async Task FlushAsync()
        {
            //Flush method should USE data structure to store reports!!
            //this code is right now deprecated 
            //we can use this in future
            Database.Clear();

            //var directoryInfo = new DirectoryInfo(DatabasePath);
            //var files = directoryInfo.GetFiles();
            //foreach (var @file in files)
            //{
            //    if (!file.Name.StartsWith("attachment_"))
            //    {
            //        continue;
            //    }

            //}

            throw new NotImplementedException();
        }


#endif
        /// <summary>
        /// Check if used directory database is available 
        /// </summary>
        private void ValidateDatabaseDirectory()
        {
            //there is no database directory
            if (string.IsNullOrEmpty(DatabasePath))
            {
                return;
            }
            RemoveOrphaned();
        }

        /// <summary>
        /// Create new minidump file in database directory path. Minidump file name is a random Guid
        /// </summary>
        /// <param name="backtraceReport">Current report</param>
        /// <param name="miniDumpType">Generated minidump type</param>
        /// <returns>Path to minidump file</returns>
        private string GenerateMiniDump(BacktraceReportBase<T> backtraceReport, MiniDumpType miniDumpType)
        {
            if (!_enable)
            {
                return string.Empty;
            }
            //note that every minidump file generated by app ends with .dmp extension
            //its important information if you want to clear minidump file
            string minidumpDestinationPath = Path.Combine(DatabaseSettings.DatabasePath, $"{Guid.NewGuid()}.dmp");
            MinidumpException minidumpExceptionType = backtraceReport.ExceptionTypeReport
                ? MinidumpException.Present
                : MinidumpException.None;

            bool minidumpSaved = MinidumpHelper.Write(
                filePath: minidumpDestinationPath,
                options: miniDumpType,
                exceptionType: minidumpExceptionType);

            return minidumpSaved
                ? minidumpDestinationPath
                : string.Empty;
        }

        /// <summary>
        /// Clear generated minidumps
        /// </summary>
        /// <param name="pathToMinidump">Path to created minidump</param>
        private void ClearMiniDump(string pathToMinidump)
        {
            //if _enable == false then application wont generate any minidump file
            //note that every minidump file generated by app ends with .dmp extension
            //its important information if you want to clear minidump file
            if (!_enable || string.IsNullOrEmpty(pathToMinidump) || Path.GetExtension(pathToMinidump) != ".dmp")
            {
                return;
            }
            File.Delete(pathToMinidump);
        }

        /// <summary>
        /// Save diagnostic report on hard drive
        /// </summary>
        /// <param name="backtraceReport"></param>
        private bool SaveReport(BacktraceData<T> backtraceReport)
        {
            if (!_enable)
            {
                return true;
            }

            string json = JsonConvert.SerializeObject(backtraceReport);
            byte[] file = Encoding.UTF8.GetBytes(json);
            string filename = $"{reportPrefix}{backtraceReport.Timestamp}";
            string filePath = Path.Combine(DatabasePath, filename);
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(file, 0, file.Length);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Add new report to BacktraceDatabase
        /// </summary>
        public void Add()
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Get all stored reports in BacktraceDatabase
        /// </summary>
        /// <returns>All stored reports in BacktraceDatabase</returns>
        public IEnumerable<BacktraceReportBase<T>> Get()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Dispose BacktraceDatabase
        /// </summary>
        public void Dispose()
        {
            //dispose database
            throw new NotImplementedException();
            RemoveOrphaned();
        }

        //THIS IS OUT OF SCOPE
        /// <summary>
        /// Get a report by using specific filter
        /// </summary>
        /// <param name="delegate">Report filter</param>
        /// <returns>Stored reports that match filter criteria</returns>
        public IEnumerable<BacktraceReportBase<T>> Get(Func<BacktraceReportBase<T>, IEnumerable<BacktraceReportBase<T>>> @delegate)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Detect all orphaned minidump files
        /// </summary>
        private void RemoveOrphaned()
        {
            throw new NotImplementedException();
        }

        private void LoadReports()
        {
            var directoryInfo = new DirectoryInfo(DatabasePath);
            IEnumerable<FileInfo> files = directoryInfo.GetFiles($"{reportPrefix}*.json", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                using (var fileStream = file.OpenText())
                {
                    var json = fileStream.ReadToEnd();
                    //todo
                    //check if json is valid
                    var data = JsonConvert.DeserializeObject<BacktraceData<T>>(json);
                    Database[0].Add(file.FullName);
                }
            }
        }
    }
}
