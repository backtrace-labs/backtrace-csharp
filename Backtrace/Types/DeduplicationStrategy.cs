using System;

namespace Backtrace.Types
{

    /// <summary>
    /// Determine deduplication strategy
    /// </summary>
    [Flags]
    public enum DeduplicationStrategy
    {
        /// <summary>
        /// Ignore deduplication strategy
        /// </summary>
        None = 0,
        /// <summary>
        /// Only stack trace
        /// </summary>
        Default = 1,
        /// <summary>
        /// Stack trace and exception classifier
        /// </summary>
        Classifier = 2,
        /// <summary>
        /// Stack trace and exception message
        /// </summary>
        Message = 4,
        /// <summary>
        /// Stack trace and library name
        /// </summary>
        Application = 8
    }
}
