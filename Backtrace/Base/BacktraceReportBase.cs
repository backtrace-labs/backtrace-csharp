﻿using Backtrace.Extensions;
using Backtrace.Model.JsonData;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Backtrace.Base
{
    /// <summary>
    /// Capture application report
    /// </summary>
    public class BacktraceReportBase<T>
    {
        /// <summary>
        /// 16 bytes of randomness in human readable UUID format
        /// server will reject request if uuid is already found
        /// </summary>s
        public readonly Guid Uuid = Guid.NewGuid();

        /// <summary>
        /// UTC timestamp in seconds
        /// </summary>
        public readonly long Timestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

        /// <summary>
        /// Get information aboout report type. If value is true the BacktraceReport has an error information
        /// </summary>
        public bool ExceptionTypeReport = false;

        /// <summary>
        /// Additional information about application state
        /// </summary>
        private Dictionary<string, T> _attributes;

        /// <summary>
        /// Get a report classification 
        /// </summary>
        public string Classifier
        {
            get
            {
                if (ExceptionTypeReport)
                {
                    return Exception.GetType().Name;
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// Get an report attributes
        /// </summary>
        public Dictionary<string, T> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Get a custom client message
        /// </summary>
        internal readonly string Message;

        /// <summary>
        /// Get a report exception
        /// </summary>
        internal Exception Exception;

        /// <summary>
        /// Get an assembly where report was created (or should be created)
        /// </summary>
        internal Assembly CallingAssembly;

        /// <summary>
        /// Get all paths to attachments
        /// </summary>
        internal List<string> _attachmentPaths;

        /// <summary>
        /// Create new instance of Backtrace report to sending a report with custom client message
        /// </summary>
        /// <param name="message">Custom client message</param>
        /// <param name="callingAssembly">Calling assembly instance</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            string message,
            Assembly callingAssembly,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
        {
            CallingAssembly = Assembly.GetCallingAssembly();
            Message = message;
            _attributes = attributes ?? new Dictionary<string, T>();
            _attachmentPaths = attachmentPaths ?? new List<string>();
        }

        /// <summary>
        /// Create new instance of Backtrace report to sending a report with custom client message
        /// </summary>
        /// <param name="message">Custom client message</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            string message,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
            : this(message, Assembly.GetCallingAssembly(), attributes, attachmentPaths)
        {
        }

        /// <summary>
        /// Create new instance of Backtrace report to sending a report with application exception
        /// </summary>
        /// <param name="exception">Current exception</param>
        /// <param name="callingAssembly">Calling assembly</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            Exception exception,
            Assembly callingAssembly,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
        {
            CallingAssembly = callingAssembly;
            _attributes = attributes ?? new Dictionary<string, T>();
            _attachmentPaths = attachmentPaths ?? new List<string>();
            //handle null value in exception parameter
            if (exception == null)
            {
                return;
            }
            Exception = exception;
            var type = Exception?.GetType();
            ExceptionTypeReport = true;
        }

        /// <summary>
        /// Create new instance of Backtrace report to sending a report with application exception
        /// </summary>
        /// <param name="exception">Current exception</param>
        /// <param name="attributes">Additional information about application state</param>
        /// <param name="attachmentPaths">Path to all report attachments</param>
        public BacktraceReportBase(
            Exception exception,
            Dictionary<string, T> attributes = null,
            List<string> attachmentPaths = null)
            : this(exception, Assembly.GetCallingAssembly(), attributes, attachmentPaths)
        {
        }

        /// <summary>
        /// Convert exception to Exception Stack
        /// </summary>
        /// <returns>ExceptionStack based on exception</returns>
        internal IEnumerable<ExceptionStack> GetExceptionStack()
        {
            if (!ExceptionTypeReport)
            {
                return null;
            }
            return ExceptionStack.Convert(Exception);
        }

        /// <summary>
        /// Concat two attributes dictionary 
        /// </summary>
        /// <param name="report">Current report</param>
        /// <param name="attributes">Attributes to concatenate</param>
        /// <returns></returns>
        internal static Dictionary<string, T> ConcatAttributes(
            BacktraceReportBase<T> report, Dictionary<string, T> attributes)
        {
            var reportAttributes = report.Attributes;
            if (attributes == null)
            {
                return reportAttributes;
            };
            return reportAttributes.Merge(attributes);
        }
    }
}
