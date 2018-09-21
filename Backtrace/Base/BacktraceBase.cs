using Backtrace.Model;
using Backtrace.Interfaces;
using System;
using System.Collections.Generic;
using Backtrace.Services;
using Backtrace.Types;
using Backtrace.Model.Database;
#if !NET35
using System.Threading.Tasks;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Base
{
    /// <summary>
    /// Base Backtrace .NET Client 
    /// </summary>
    public class BacktraceBase
    {
#if !NET35
        /// <summary>
        /// Ignore AggregateException and only prepare report for inner exceptions
        /// </summary>
        public bool IgnoreAggregateException { get; set; } = false;
#endif

        /// <summary>
        /// Custom request handler for HTTP call to server
        /// </summary>
        public Func<string, string, BacktraceData, BacktraceResult> RequestHandler
        {
            get => BacktraceApi.RequestHandler;
            set => BacktraceApi.RequestHandler = value;
        }
        /// <summary>
        /// Set an event executed when received bad request, unauthorize request or other information from server
        /// </summary>
        public Action<Exception> OnServerError
        {
            get => BacktraceApi.OnServerError;
            set => BacktraceApi.OnServerError = value;
        }

        /// <summary>
        /// Set an event executed when Backtrace API return information about send report
        /// </summary>
        public Action<BacktraceResult> OnServerResponse
        {
            get => BacktraceApi.OnServerResponse;
            set => BacktraceApi.OnServerResponse = value;
        }

        /// <summary>
        /// Get or set minidump type
        /// </summary>
        public MiniDumpType MiniDumpType { get; set; } = MiniDumpType.Normal;

        /// <summary>
        /// Set event executed when client site report limit reached
        /// </summary>
        public Action<BacktraceReport> OnClientReportLimitReached
        {
            set => BacktraceApi.SetClientRateLimitEvent(value);
        }

        /// <summary>
        /// Set event executed before sending data to Backtrace API
        /// </summary>
        public Func<BacktraceData, BacktraceData> BeforeSend = null;

        /// <summary>
        /// Set event executed when unhandled application exception event catch exception
        /// </summary>
        public Action<Exception> OnUnhandledApplicationException = null;

        /// <summary>
        /// Get custom client attributes. Every argument stored in dictionary will be send to Backtrace API
        /// </summary>
        public readonly Dictionary<string, object> Attributes;

        /// <summary>
        /// Backtrace database instance that allows to manage minidump files 
        /// </summary>
        public IBacktraceDatabase Database;

        private IBacktraceApi _backtraceApi;
        /// <summary>
        /// Instance of BacktraceApi that allows to send data to Backtrace API
        /// </summary>
        internal IBacktraceApi BacktraceApi
        {
            get => _backtraceApi;
            set
            {
                _backtraceApi = value;
                Database?.SetApi(_backtraceApi);
            }
        }

        /// <summary>
        /// Initialize new client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Additional information about current application</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        public BacktraceBase(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, object> attributes = null,
            BacktraceDatabaseSettings databaseSettings = null,
            uint reportPerMin = 3)
            : this(backtraceCredentials, attributes, new BacktraceDatabase(databaseSettings),
                  reportPerMin)
        { }

        /// <summary>
        /// Initialize new client instance with BacktraceCredentials
        /// </summary>
        /// <param name="backtraceCredentials">Backtrace credentials to access Backtrace API</param>
        /// <param name="attributes">Additional information about current application</param>
        /// <param name="databaseSettings">Backtrace database settings</param>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        public BacktraceBase(
            BacktraceCredentials backtraceCredentials,
            Dictionary<string, object> attributes = null,
            IBacktraceDatabase database = null,
            uint reportPerMin = 3)
        {
            Attributes = attributes ?? new Dictionary<string, object>();
            BacktraceApi = new BacktraceApi(backtraceCredentials, reportPerMin);
            Database = database ?? new BacktraceDatabase();
            Database.SetApi(BacktraceApi);
            Database.Start();
        }

        /// <summary>
        /// Change maximum number of reportrs sending per one minute
        /// </summary>
        /// <param name="reportPerMin">Number of reports sending per one minute. If value is equal to zero, there is no request sending to API. Value have to be greater than or equal to 0</param>
        public void SetClientReportLimit(uint reportPerMin)
        {
            BacktraceApi.SetClientRateLimit(reportPerMin);
        }

        /// <summary>
        /// Send a report to Backtrace API
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual BacktraceResult Send(BacktraceReport report)
        {
            var record = Database.Add(report, Attributes, MiniDumpType);
            //create a JSON payload instance
            var data = record?.BacktraceData ?? report.ToBacktraceData(Attributes);
            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = BacktraceApi.Send(data);
            record?.Dispose();
            if (result.Status == BacktraceResultStatus.Ok)
            {
                Database.Delete(record);
            }
            //check if there is more errors to send
            //handle inner exception
            result.InnerExceptionResult = HandleInnerException(report);
            return result;
        }

        /// <summary>
        /// Handle inner exception in current backtrace report
        /// if inner exception exists, client should send report twice - one with current exception, one with inner exception
        /// </summary>
        /// <param name="report">current report</param>
        private BacktraceResult HandleInnerException(BacktraceReport report)
        {
            //we have to create a copy of an inner exception report
            //to have the same calling assembly property
            var innerExceptionReport = report.CreateInnerReport();
            if (innerExceptionReport == null)
            {
                return null;
            }
            return Send(innerExceptionReport);
        }
#if !NET35

        /// <summary>
        /// Send asynchronous report to Backtrace API
        /// </summary>
        /// <param name="report">Report to send</param>
        public virtual async Task<BacktraceResult> SendAsync(BacktraceReport report)
        {
            if (IgnoreAggregateException && report.Exception is AggregateException)
            {
                return await HandleAggregateException(report);
            }
            var record = Database.Add(report, Attributes, MiniDumpType);
            //create a JSON payload instance
            var data = record?.BacktraceData ?? report.ToBacktraceData(Attributes);
            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = await BacktraceApi.SendAsync(data);
            record?.Dispose();
            if (result.Status == BacktraceResultStatus.Ok)
            {
                Database.Delete(record);
            }
            //check if there is more errors to send
            //handle inner exception
            result.InnerExceptionResult = await HandleInnerExceptionAsync(report);
            return result;
        }

        private async Task<BacktraceResult> HandleAggregateException(BacktraceReport report)
        {
            AggregateException aggregateException = report.Exception as AggregateException;
            BacktraceResult result = null;

            foreach (var ex in aggregateException.InnerExceptions)
            {
                var innerReport = new BacktraceReport(
                    exception: ex,
                    attributes: report.Attributes,
                    attachmentPaths: report.AttachmentPaths,
                    reflectionMethodName: report._reflectionMethodName)
                {
                    Factor = report.Factor,
                    Fingerprint = report.Fingerprint
                };
                if (result == null)
                {
                    result = await SendAsync(innerReport);
                }
                else
                {
                    var innerResult = Send(innerReport);
                    result.AddInnerResult(innerResult);
                }
            }
            return result;
        }

        /// <summary>
        /// Handle inner exception in current backtrace report
        /// if inner exception exists, client should send report twice - one with current exception, one with inner exception
        /// </summary>
        /// <param name="report">current report</param>
        private async Task<BacktraceResult> HandleInnerExceptionAsync(BacktraceReport report)
        {
            var innerExceptionReport = report.CreateInnerReport();
            if (innerExceptionReport == null)
            {
                return null;
            }
            return await SendAsync(innerExceptionReport);
        }

        /// <summary>
        /// Add automatic exception handling for current application
        /// </summary>
        public virtual void HandleApplicationException()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        /// <summary>
        /// Add automatic thread exception handling for current application
        /// </summary>
        public virtual void HandleApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            SendAsync(new BacktraceReport(e.Exception)).Wait();
            OnUnhandledApplicationException?.Invoke(e.Exception);
        }

        //public virtual void HandleUnobservedTaskExceptions()
        //{
        //    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        //}

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            SendAsync(new BacktraceReport(e.Exception)).Wait();
            OnUnhandledApplicationException?.Invoke(e.Exception);
        }

        /// <summary>
        /// In most situation when application crash, main process wont wait till we prepare report and send it to API. 
        /// Method allows you to get all necessary data required by BacktraceClient and wait till report will be send on server
        /// This method is invoked when application crash, so BacktraceClient override all existing events to make sure 
        /// we can handle request end
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            Task.WaitAll(SendAsync(new BacktraceReport(exception)));
            OnUnhandledApplicationException?.Invoke(exception);
        }
#endif
    }
}
