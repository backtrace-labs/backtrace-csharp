using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Backtrace.Extensions
{
    public static class FileInfoExtensions
    {
        public static string ReadAllText(this FileInfo source)
        {
            StringBuilder stringBuilder = new StringBuilder();
            // Open the stream and read it back.
            using (StreamReader sr = source.OpenText())
            {
                string s = string.Empty;
                while ((s = sr.ReadLine()) != null)
                {

                }
            }
        }
    }
}
