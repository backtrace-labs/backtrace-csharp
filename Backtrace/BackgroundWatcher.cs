using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace
{
    /// <summary>
    /// Create a Backtrace backgroud watcher. Use watcher to send a raport to Backtrace api in a period defined in reportPerSec
    /// </summary>
    internal class BackgroundWatcher
    {
        private readonly int _raportPerSec;
        /// <summary>
        /// Create new instance of background watcher
        /// </summary>
        /// <param name="reportPerSec">How many times per secound should watcher send a raport</param>
        public BackgroundWatcher(int reportPerSec)
        {

        }

        /// <summary>
        /// Create work thread that send a environment raport in defined period
        /// </summary>
        public void Initialize()
        {
            throw new NotImplementedException();
        }
    }
}
