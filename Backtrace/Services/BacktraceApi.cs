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
    internal class BacktraceApi<T> : IBacktraceApi<T>
    {
        /// <summary>
        /// Asynchronous request flag. If value is equal to true, data will be send to server asynchronous
        /// </summary>
        [Obsolete]
        public bool AsynchronousRequest { get; set; } = false;

        /// <summary>
        /// User custom request method
        /// </summary>
        public Func<string, string, BacktraceData<T>, BacktraceResult> RequestHandler { get; set; } = null;

        /// <summary>
        /// Event triggered when server is unvailable
        /// </summary>
        public Action<Exception> OnServerError { get; set; } = null;

        /// <summary>
        /// Event triggered when server respond to diagnostic data
        /// </summary>
        public Action<BacktraceResult> OnServerResponse { get; set; }

        /// <summary>
        /// Url to server
        /// </summary>
        private readonly string _serverurl;

        /// <summary>
        /// Create a new instance of Backtrace API
        /// </summary>
        /// <param name="credentials">API credentials</param>
        public BacktraceApi(BacktraceCredentials credentials, bool tlsLegacySupport)
        {
            _serverurl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
            SetTlsLegacy(tlsLegacySupport);
        }
        #region asyncRequest
#if !NET35

        /// <summary>
        /// The http client.
        /// </summary>
        internal HttpClient HttpClient = new HttpClient();

        public async Task<BacktraceResult> SendAsync(BacktraceData<T> data)
        {
            Guid requestId = Guid.NewGuid();
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            if (RequestHandler != null)
            {
                RequestHandler?.Invoke(_serverurl, FormDataHelper.GetContentTypeWithBoundary(requestId), data);
            }
            return await SendAsync(requestId, json, data);
        }

        private async Task<BacktraceResult> SendAsync(Guid requestId, string json, BacktraceData<T> data)
        {
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            string boundary = FormDataHelper.GetBoundary(requestId);
            var report = data.Report as BacktraceReport;

            using (var request = new HttpRequestMessage(HttpMethod.Post, _serverurl))
            using (var content = new MultipartFormDataContent(boundary))
            {
                content.AddJson("upload_file.json", json);
                content.AddFiles(data.Attachments);

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
                    OnServerError?.Invoke(exception);
                    return BacktraceResult.OnError(report, exception);
                }
            }
        }
#endif
        #endregion


        /// <summary>
        /// Set tls and ssl legacy support for https requests to Backtrace API
        /// </summary>
        internal void SetTlsLegacy(bool legacySupport)
        {
            if (!legacySupport)
            {
                return;
            }
            ServicePointManager.SecurityProtocol =
                     SecurityProtocolType.Tls
                    | (SecurityProtocolType)0x00000300
                    | (SecurityProtocolType)0x00000C00;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        #region synchronousRequest
        /// <summary>
        /// Sending a diagnostic report data to server API. 
        /// </summary>
        /// <param name="data">Diagnostic data</param>
        /// <returns>Server response</returns>
        public BacktraceResult Send(BacktraceData<T> data)
        {
            Guid requestId = Guid.NewGuid();
            string json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            var report = data.Report as BacktraceReport;

            var formData = FormDataHelper.GetFormData(json, data.Attachments, requestId);
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            HttpWebRequest request = WebRequest.Create(_serverurl) as HttpWebRequest;

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
            DefaultValueHandling = DefaultValueHandling.Ignore
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
    }
}
