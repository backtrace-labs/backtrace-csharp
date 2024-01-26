using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if !NET35
using System.Net.Http;
using System.Net.Http.Headers;
#endif

namespace Backtrace.Extensions
{
#if !NET35
    /// <summary>
    /// Extesions available for Multipart form data used with HttpClient
    /// </summary>
    public static class FormDataExtensions
    {
        public static void AddJson(this MultipartFormDataContent content, string fileName, string json)
        {
            var jsonContent = new StringContent(json);
            jsonContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

            jsonContent.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("form-data")
                {
                    Name = $"{Path.GetFileNameWithoutExtension(fileName)}",
                    FileName = $"\"{fileName}\""
                };

            content.Add(jsonContent);
        }

        public static void AddFiles(this MultipartFormDataContent content, List<string> filePath)
        {
            if (filePath == null || filePath.Count == 0)
            {
                return;
            }
            var files = filePath.Distinct().ToList();
            foreach (var file in files)
            {
                if (!File.Exists(file))
                {
                    continue;
                }
                string fileName = $"attachment_{Path.GetFileName(file)}";
                var fileContent = new StreamContent(File.OpenRead(file));
                fileContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                {
                    Name = $"\"{fileName}\"",
                    FileName = $"\"{fileName}\""
                };
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
                content.Add(fileContent);
            }
        }
    }
#endif
}
