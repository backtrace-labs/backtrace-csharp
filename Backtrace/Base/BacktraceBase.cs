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
    public class BacktraceBase<T>
    {
        /// <summary>
        /// Custom request handler for HTTP call to server
        /// </summary>
        public Func<string, string, BacktraceData<T>, BacktraceResult> RequestHandler
        {
            get
            {
                return BacktraceApi.RequestHandler;
            }
            set
            {
                BacktraceApi.RequestHandler = value;
            }
        }
        /// <summary>
        /// Set an event executed when received bad request, unauthorize request or other information from server
        /// </summary>
        public Action<Exception> OnServerError
        {
            get
            {
                return BacktraceApi.OnServerError;
            }
            set
            {
                BacktraceApi.OnServerError = value;
            }
        }

        /// <summary>
        /// Set an event executed when Backtrace API return information about send report
        /// </summary>
        public Action<BacktraceResult> OnServerResponse
        {
            get
            {
                return BacktraceApi.OnServerResponse;
            }
            set
            {
                BacktraceApi.OnServerResponse = value;
            }
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
            set
            {
                BacktraceApi.SetClientRateLimitEvent(value);
            }
        }

        /// <summary>
        /// Set event executed before sending data to Backtrace API
        /// </summary>
        public Func<BacktraceData<T>, BacktraceData<T>> BeforeSend = null;

        /// <summary>
        /// Set event executed when unhandled application exception event catch exception
        /// </summary>
        public Action<Exception> OnUnhandledApplicationException = null;

        /// <summary>
        /// Get custom client attributes. Every argument stored in dictionary will be send to Backtrace API
        /// </summary>
        public readonly Dictionary<string, T> Attributes;

        /// <summary>
        /// Backtrace database instance that allows to manage minidump files 
        /// </summary>
        public IBacktraceDatabase<T> Database;

        private IBacktraceApi<T> _backtraceApi;
        /// <summary>
        /// Instance of BacktraceApi that allows to send data to Backtrace API
        /// </summary>
        internal IBacktraceApi<T> BacktraceApi
        {
            get
            {
                return _backtraceApi;
            }
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
            Dictionary<string, T> attributes = null,
            BacktraceDatabaseSettings databaseSettings = null,
            uint reportPerMin = 3)
            : this(backtraceCredentials, attributes, new BacktraceDatabase<T>(databaseSettings),
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
            Dictionary<string, T> attributes = null,
            IBacktraceDatabase<T> database = null,
            uint reportPerMin = 3)
        {
            Attributes = attributes ?? new Dictionary<string, T>();
            BacktraceApi = new BacktraceApi<T>(backtraceCredentials, reportPerMin);
            Database = database ?? new BacktraceDatabase<T>();
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
        [Obsolete("Send is obsolete, please use SendAsync instead if possible.")]
        public virtual BacktraceResult Send(BacktraceReportBase<T> report)
        {
            var entry = Database.Add(report, Attributes, MiniDumpType);
            //create a JSON payload instance
            var data = entry?.BacktraceData ?? report.ToBacktraceData(Attributes);
            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = BacktraceApi.Send(data);
            entry?.Dispose();
            if (result.Status == BacktraceResultStatus.Ok)
            {
                Database.Delete(entry);
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
        private BacktraceResult HandleInnerException(BacktraceReportBase<T> report)
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
        public virtual async Task<BacktraceResult> SendAsync(BacktraceReportBase<T> report)
        {
            var entry = Database.Add(report, Attributes, MiniDumpType);
            //create a JSON payload instance
            var data = entry?.BacktraceData ?? report.ToBacktraceData(Attributes);
            //valid user custom events
            data = BeforeSend?.Invoke(data) ?? data;
            var result = await BacktraceApi.SendAsync(data);
            entry?.Dispose();
            if (result.Status == BacktraceResultStatus.Ok)
            {
                Database.Delete(entry);
            }
            //check if there is more errors to send
            //handle inner exception
            result.InnerExceptionResult = await HandleInnerExceptionAsync(report);
            return result;
        }

        /// <summary>
        /// Handle inner exception in current backtrace report
        /// if inner exception exists, client should send report twice - one with current exception, one with inner exception
        /// </summary>
        /// <param name="report">current report</param>
        private async Task<BacktraceResult> HandleInnerExceptionAsync(BacktraceReportBase<T> report)
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
            SendAsync(new BacktraceReportBase<T>(e.Exception)).Wait();
            OnUnhandledApplicationException?.Invoke(e.Exception);
        }

        //public virtual void HandleUnobservedTaskExceptions()
        //{
        //    TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        //}

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            SendAsync(new BacktraceReportBase<T>(e.Exception)).Wait();
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
            Task.WaitAll(SendAsync(new BacktraceReportBase<T>(exception)));
            OnUnhandledApplicationException?.Invoke(exception);
        }
#endif
    }
}
