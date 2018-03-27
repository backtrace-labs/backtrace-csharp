using System;
using Backtrace.Model;
using Backtrace.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using Backtrace.Common;
using System.Collections.Generic;
#if !NET35
using System.Threading.Tasks;
using System.Net.Http.Headers;
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
        public Func<string, string, BacktraceData<T>, BacktraceServerResponse> RequestHandler { get; set; } = null;

        /// <summary>
        /// Event triggered when server is unvailable
        /// </summary>
        public Action<Exception> OnServerError { get; set; } = null;

        /// <summary>
        /// Event triggered when server respond to diagnostic data
        /// </summary>
        public Action<BacktraceServerResponse> OnServerResponse { get; set; }

        /// <summary>
        /// Url to server
        /// </summary>
        private readonly string _serverurl;


        /// <summary>
        /// Create a new instance of Backtrace API
        /// </summary>
        /// <param name="credentials">API credentials</param>
        public BacktraceApi(BacktraceCredentials credentials)
        {
            _serverurl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
        }

#if !NET35
        /// <summary>
        /// The http client.
        /// </summary>
        private readonly HttpClient _http = new HttpClient();

        public async Task<BacktraceServerResponse> SendAsync(BacktraceData<T> data)
        {
            Guid requestId = Guid.NewGuid();
            var json = JsonConvert.SerializeObject(data, JsonSerializerSettings);
            if (RequestHandler != null)
            {
                RequestHandler?.Invoke(_serverurl, FormDataHelper.GetContentTypeWithBoundary(requestId), data);
            }
            return await SendAsync(requestId, json, data.Attachments);
        }

        private async Task<BacktraceServerResponse> SendAsync(Guid requestId, string json, List<string> attachments)
        {
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            string boundary = FormDataHelper.GetBoundary(requestId);

            using (var request = new HttpRequestMessage(HttpMethod.Post, _serverurl))
            using (var content = new MultipartFormDataContent(boundary))
            {
                var jsonContent = new StringContent(json);
                jsonContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");

                jsonContent.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "\"upload_file\"",
                        FileName = "\"upload_file.json\""
                    };

                content.Add(jsonContent);
                foreach (var file in attachments)
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

                //// clear and add content type with boundary tag
                content.Headers.Remove("Content-Type");
                content.Headers.TryAddWithoutValidation("Content-Type", contentType);
                request.Content = content;

                using (var response = await _http.SendAsync(request))
                {
                    var fullResponse = await response.Content.ReadAsStringAsync();
                    var serverResponse = JsonConvert.DeserializeObject<BacktraceServerResponse>(fullResponse, JsonSerializerSettings);
                    return serverResponse;
                }
            }
        }
#endif


        /// <summary>
        /// Setting all security protocols for https requests via
        /// </summary>
        public void SetTlsSupport()
        {
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
