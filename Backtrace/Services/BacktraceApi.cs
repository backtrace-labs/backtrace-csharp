using System;
using Backtrace.Model;
using Backtrace.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Backtrace.Common;
using System.Collections.Generic;
using Backtrace.Extensions;
#if !NET35
using System.Threading.Tasks;
using System.Net.Http;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Backtrace.Services
{
    /// <summary>
    /// Backtrace Api class that allows to send a diagnostic data to server
    /// </summary>
    internal class BacktraceApi : IBacktraceApi
    {
        /// <summary>
        /// User custom request method
        /// </summary>
        public Func<string, string, BacktraceData, BacktraceResult> RequestHandler { get; set; } = null;

        /// <summary>
        /// Event triggered when server is unvailable
        /// </summary>
        public Action<Exception> OnServerError { get; set; } = null;

        /// <summary>
        /// Event triggered when server respond to diagnostic data
        /// </summary>
        public Action<BacktraceResult> OnServerResponse { get; set; }

        internal readonly ReportLimitWatcher reportLimitWatcher;

        /// <summary>
        /// Url to server
        /// </summary>
        private readonly Uri _serverurl;

        /// <summary>
        /// Create a new instance of Backtrace API
        /// </summary>
        /// <param name="credentials">API credentials</param>
        public BacktraceApi(BacktraceCredentials credentials, uint reportPerMin = 3)
        {
            if (credentials == null)
            {
                throw new ArgumentException($"{nameof(BacktraceCredentials)} cannot be null");
            }
            _serverurl = credentials.GetSubmissionUrl();
            reportLimitWatcher = new ReportLimitWatcher(reportPerMin);
#if !NET35
            InitializeHttpClient(credentials.Proxy);
#endif
        }
        #region asyncRequest
#if !NET35

        /// <summary>
        /// The http client.
        /// </summary>
        internal HttpClient HttpClient;
        private void InitializeHttpClient(WebProxy proxy)
        {
            if (proxy != null)
            {
                HttpClient = new HttpClient(new HttpClientHandler() { Proxy = proxy }, true);
            }
            else
            {
                HttpClient = new HttpClient();
            }
        }

        public async Task<BacktraceResult> SendAsync(BacktraceData data)
        {
            //check rate limiting
            bool watcherValidation = reportLimitWatcher.WatchReport(data.Report);
            if (!watcherValidation)
            {
                return BacktraceResult.OnLimitReached(data.Report);
            }
            // execute user custom request handler
            if (RequestHandler != null)
            {
                return RequestHandler?.Invoke(_serverurl.ToString(), FormDataHelper.GetContentTypeWithBoundary(Guid.NewGuid()), data);
            }
            //get a json from diagnostic object
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            return await SendAsync(Guid.NewGuid(), json, data.Attachments, data.Report, data.Deduplication);
        }

        internal async Task<BacktraceResult> SendAsync(Guid requestId, string json, List<string> attachments, BacktraceReport report, int deduplication = 0)
        {
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            string boundary = FormDataHelper.GetBoundary(requestId);

            using (var content = new MultipartFormDataContent(boundary))
            {
                var requestUrl = _serverurl.ToString();
                if (deduplication > 0)
                {
                    requestUrl += $"&_mod_duplicate={deduplication}";
                }

                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
                content.AddJson("upload_file.json", json);
                content.AddFiles(attachments);

                //// clear and add content type with boundary tag
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                request.Content = content;
                try
                {
                    using (var response = await HttpClient.SendAsync(request))
                    {
                        var fullResponse = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            var err = new WebException(response.ReasonPhrase);
                            System.Diagnostics.Trace.WriteLine(fullResponse);
                            OnServerError?.Invoke(err);
                            return BacktraceResult.OnError(report, err);
                        }
                        var result = JsonConvert.DeserializeObject<BacktraceResult>(fullResponse);
                        result.BacktraceReport = report;
                        OnServerResponse?.Invoke(result);
                        return result;
                    }
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Trace.WriteLine($"Backtrace - Server error: {exception.ToString()}");
                    OnServerError?.Invoke(exception);
                    return BacktraceResult.OnError(report, exception);
                }
            }
        }
#endif
        #endregion

        #region synchronousRequest
        /// <summary>
        /// Sending a diagnostic report data to server API. 
        /// </summary>
        /// <param name="data">Diagnostic data</param>
        /// <returns>Server response</returns>
        public BacktraceResult Send(BacktraceData data)
        {
#if !NET35
            return Task.Run(() => SendAsync(data)).Result;
#else
            //check rate limiting
            bool watcherValidation = reportLimitWatcher.WatchReport(data.Report);
            if (!watcherValidation)
            {
                return BacktraceResult.OnLimitReached(data.Report);
            }
            // execute user custom request handler
            if (RequestHandler != null)
            {
                return RequestHandler?.Invoke(_serverurl.ToString(), FormDataHelper.GetContentTypeWithBoundary(Guid.NewGuid()), data);
            }
            //set submission data
            string json = JsonConvert.SerializeObject(data);
            return Send(Guid.NewGuid(), json, data.Report?.AttachmentPaths ?? new List<string>(), data.Report, data.Deduplication);
        }

        private BacktraceResult Send(Guid requestId, string json, List<string> attachments, BacktraceReport report, int deduplication = 0)
        {
            var requestUrl = _serverurl.ToString();
            if (deduplication > 0)
            {
                requestUrl += $"&_mod_duplicate={deduplication}";
            }

            var formData = FormDataHelper.GetFormData(json, attachments, requestId);
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            var request = WebRequest.Create(requestUrl) as HttpWebRequest;

            //Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = formData.Length;
            try
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Close();
                }
                return ReadServerResponse(request, report);
            }
            catch (Exception exception)
            {
                OnServerError?.Invoke(exception);
                return BacktraceResult.OnError(report, exception);
            }
#endif
        }

        /// <summary>
        /// Handle server respond for synchronous request
        /// </summary>
        /// <param name="request">Current HttpWebRequest</param>
        private BacktraceResult ReadServerResponse(HttpWebRequest request, BacktraceReport report)
        {
            using (WebResponse webResponse = request.GetResponse() as HttpWebResponse)
            {
                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                string fullResponse = responseReader.ReadToEnd();
                var response = JsonConvert.DeserializeObject<BacktraceResult>(fullResponse);
                response.BacktraceReport = report;
                OnServerResponse?.Invoke(response);
                return response;
            }
        }
        #endregion
        /// <summary>
        /// Get serialization settings
        /// </summary>
        /// <returns></returns>
        private JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        #region dispose
        private bool _disposed = false; // To detect redundant calls
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
#if !NET35
                    HttpClient.Dispose();
#endif
                }
                _disposed = true;
            }
        }

        ~BacktraceApi()
        {
            Dispose(false);
        }
        #endregion

        public void SetClientRateLimitEvent(Action<BacktraceReport> onClientReportLimitReached)
        {
            reportLimitWatcher.OnClientReportLimitReached = onClientReportLimitReached;
        }

        public void SetClientRateLimit(uint rateLimit)
        {
            reportLimitWatcher.SetClientReportLimit(rateLimit);
        }
    }
}