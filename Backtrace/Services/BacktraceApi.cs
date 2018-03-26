using System;
using Backtrace.Model;
using System.Collections.Generic;
using Backtrace.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Security.Authentication;
using Backtrace.Common;
using System.Reflection;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
#if !NET35
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net.Http;
#endif

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
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
        public Action<string, string, byte[]> RequestHandler { get; set; } = null;

        /// <summary>
        /// Event triggered when server is unvailable
        /// </summary>
        [Obsolete]
        public Action<Exception> OnServerError { get; set; }

        /// <summary>
        /// Event triggered when server respond to diagnostic data
        /// </summary>
        [Obsolete]
        public Action<BacktraceServerResponse> OnServerResponse { get; set; }

        /// <summary>
        /// Url to server
        /// </summary>
        private readonly string _serverurl;

#if !NET35
        /// <summary>
        /// The http client.
        /// </summary>
        private readonly HttpClient _http;
#endif
        /// <summary>
        /// Create a new instance of Backtrace API
        /// </summary>
        /// <param name="credentials">API credentials</param>
        public BacktraceApi(BacktraceCredentials credentials)
        {
            _serverurl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
#if !NET35
            _http = new HttpClient
            {
                BaseAddress = credentials.BacktraceHostUri
            };
#endif
        }

        /// <summary>
        /// Setting all necessary security protocols for https requests
        /// </summary>
        public void SetTlsSupport()
        {
            ServicePointManager.SecurityProtocol =
                     SecurityProtocolType.Tls
                    | (SecurityProtocolType)0x00000300
                    | (SecurityProtocolType)0x00000C00;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
        }

        /// <summary>
        /// Get serialization settings
        /// </summary>
        /// <returns></returns>
        private JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

#if !NET35
        public async Task<BacktraceServerResponse> SendAsync(BacktraceData<T> data)
        {
            Guid requestId = Guid.NewGuid();
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);

            var formData = FormDataHelper.GetFormData(json, data.Attachments, requestId);
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);

            using (var request = new HttpRequestMessage(HttpMethod.Post, _serverurl))
            using (var content = new ByteArrayContent(formData))
            {
                // clear and add content type with boundary tag
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                request.Content = content;

                var response = await _http.SendAsync(request);
                var fullResponse = await response.Content.ReadAsStringAsync();
                var serverResponse = JsonConvert.DeserializeObject<BacktraceServerResponse>(fullResponse, JsonSerializerSettings);
                return serverResponse;
            }
        }
#endif

        /// <summary>
        /// Sending a diagnostic report data to server API. 
        /// </summary>
        /// <param name="data">Diagnostic data</param>
        /// <returns>Server response</returns>
        public BacktraceServerResponse Send(BacktraceData<T> data)
        {
            Guid requestId = Guid.NewGuid();
            string json = JsonConvert.SerializeObject(data, JsonSerializerSettings);

            var formData = FormDataHelper.GetFormData(json, data.Attachments, requestId);
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            HttpWebRequest request = WebRequest.Create(_serverurl) as HttpWebRequest;

            //Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = formData.Length;
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }
            return ReadServerResponse(request);
        }

        /// <summary>
        /// Handle server respond for synchronous request
        /// </summary>
        /// <param name="request">Current HttpWebRequest</param>
        private BacktraceServerResponse ReadServerResponse(HttpWebRequest request)
        {
            using (WebResponse webResponse = request.GetResponse() as HttpWebResponse)
            {
                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                string fullResponse = responseReader.ReadToEnd();
                var response = JsonConvert.DeserializeObject<BacktraceServerResponse>(fullResponse);
                return response;
            }
        }

        /// <summary>
        /// Send a diagnostic bytes to server
        /// </summary>
        /// <param name="asyncResult">Asynchronous result</param>
        /// <param name="form">diagnostic data bytes</param>
        private void RequestStreamCallback(IAsyncResult asyncResult, byte[] form)
        {
            var webRequest = (HttpWebRequest)asyncResult.AsyncState;
            Stream postStream = webRequest.EndGetRequestStream(asyncResult);
            postStream.Write(form, 0, form.Length);
            postStream.Close();
            webRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), webRequest);
        }

        /// <summary>
        /// Handle server respond
        /// </summary>
        /// <param name="asyncResult">Asynchronous reuslt</param>
        private void GetResponseCallback(IAsyncResult asyncResult)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)asyncResult.AsyncState;
                ReadServerResponse(webRequest);
            }
            catch (Exception exception)
            {
                OnServerError?.Invoke(exception);
            }
        }

        private bool _disposed = false; // To detect redundant calls

        public void Dispose()
        {
            Dispose(true);

            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!_disposed)
            {
                if (disposing)
                {
#if !NET35
                    _http.Dispose();
#endif
                }
                _disposed = true;
            }
        }

        ~BacktraceApi()
        {
            Dispose(false);
        }
    }
}
