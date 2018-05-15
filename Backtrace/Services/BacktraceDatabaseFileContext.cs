﻿using Backtrace.Interfaces;
using Backtrace.Model.Database;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Backtrace.Services
{
    /// <summary>
    /// BacktraceDatabase class for file collection operations
    /// </summary>
    public class BacktraceDatabaseFileContext<T> : IBacktraceDatabaseFileContext<T>
    {
        /// <summary>
        /// Database directory path
        /// </summary>
        private readonly string _databasePath;

        /// <summary>
        /// Database directory info
        /// </summary>
        private readonly DirectoryInfo _databaseDirectoryInfo;

        /// <summary>
        /// Regex for filter physical database records
        /// </summary>
        private const string RecordFilterRegex = "*-record.json";
        /// <summary>
        /// Initialize new BacktraceDatabaseFileContext instance
        /// </summary>
        public BacktraceDatabaseFileContext(string databasePath)
        {
            _databasePath = databasePath;
            _databaseDirectoryInfo = new DirectoryInfo(_databasePath);
        }

        /// <summary>
        /// Get all physical files stored in database directory
        /// </summary>
        /// <returns>All existing physical files</returns>
        public IEnumerable<FileInfo> GetAll()
        {
            return _databaseDirectoryInfo.GetFiles();
        }

        /// <summary>
        /// Get all valid physical records stored in database directory
        /// </summary>
        /// <returns>All existing physical records</returns>
        public IEnumerable<FileInfo> GetRecords()
        {
            return _databaseDirectoryInfo
                .GetFiles(RecordFilterRegex, SearchOption.TopDirectoryOnly)
                .OrderBy(n => n.CreationTime);
        }

        /// <summary>
        /// Remove orphaned files existing in database directory
        /// </summary>
        public void RemoveOrphaned(IEnumerable<BacktraceDatabaseRecord<T>> existingRecords)
        {
            IEnumerable<string> recordStringIds = existingRecords.Select(n => n.Id.ToString());
            var files = GetAll();
            for (int fileIndex = 0; fileIndex < files.Count(); fileIndex++)
            {
                var file = files.ElementAt(fileIndex);
                //check if file should be stored in database
                //database only store data in json and files in dmp extension
                if(file.Extension != ".dmp" && file.Extension != ".json")
                {
                    file.Delete();
                    continue;
                }
                //get id from file name
                //substring from position 0 to position from character '-' contains id
                var name = file.Name.LastIndexOf('-');
                // file can store invalid record because our regex don't match
                // in this case we remove invalid file
                if(name == -1)
                {
                    file.Delete();
                    continue;
                }
                var stringGuid = file.Name.Substring(0, name);
                if (!recordStringIds.Contains(stringGuid))
                {
                    file.Delete();
                }
            }
        }        
    }
}
