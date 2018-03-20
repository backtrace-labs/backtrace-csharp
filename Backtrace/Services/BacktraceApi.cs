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
        public bool AsynchronousRequest { get; set; } = false;

        /// <summary>
        /// User custom request method
        /// </summary>
        public Action<string, string, byte[]> RequestHandler { get; set; } = null;

        /// <summary>
        /// Url to server
        /// </summary>
        private readonly string _serverurl;

        /// <summary>
        /// Event triggered when server is unvailable
        /// </summary>
        public Action<Exception> OnServerError { get; set; }

        /// <summary>
        /// Event triggered when server respond to diagnostic data
        /// </summary>
        public Action<BacktraceServerResponse> OnServerResponse { get; set; }

        /// <summary>
        /// Create a new instance of Backtrace API
        /// </summary>
        /// <param name="credentials">API credentials</param>
        public BacktraceApi(BacktraceCredentials credentials)
        {
            _serverurl = $"{credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={credentials.Token}";
            bool isHttps = credentials.BacktraceHostUri.Scheme == "https";
            //prepare web client to send a data to ssl API
            if (isHttps)
            {
                ServicePointManager.SecurityProtocol =
                     SecurityProtocolType.Tls
                    | (SecurityProtocolType)0x00000300
                    | (SecurityProtocolType)0x00000C00;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
            }
        }

        /// <summary>
        /// Get serialization settings
        /// </summary>
        /// <returns></returns>
        private JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
        }

        /// <summary>
        /// Sending a diagnostic report data to server API. 
        /// </summary>
        /// <param name="data">Diagnostic data</param>
        /// <returns>False if operation fail or true if API return OK</returns>
        public void Send(BacktraceData<T> data)
        {
            string json = JsonConvert.SerializeObject(data, GetSerializerSettings());
            List<string> attachments = data.Attachments;
            Send(json, attachments);
        }

        /// <summary>
        /// Sending a diagnostic report data to server API. 
        /// </summary>
        /// <param name="json">Diagnostics json</param>
        /// <param name="attachmentPaths">Attachments path</param>
        private void Send(string json, List<string> attachmentPaths)
        {
            Guid requestId = Guid.NewGuid();
            var formData = FormDataHelper.GetFormData(json, attachmentPaths, requestId);
            string contentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            if (RequestHandler != null)
            {
                RequestHandler.Invoke(_serverurl, contentType, formData);
                return;
            }
            HttpWebRequest request = WebRequest.Create(_serverurl) as HttpWebRequest;

            //Set up the request properties.
            request.Method = "POST";
            request.ContentType = contentType;
            request.ContentLength = formData.Length;

            if (AsynchronousRequest)
            {
                request.BeginGetRequestStream(new AsyncCallback((n) => RequestStreamCallback(n, formData)), request);
                return;
            }
            try
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Close();
                }
                ReadServerResponse(request);
            }
            catch (Exception exception)
            {
                OnServerError?.Invoke(exception);
            }
        }

        /// <summary>
        /// Handle server respond for synchronous request
        /// </summary>
        /// <param name="request">Current HttpWebRequest</param>
        private void ReadServerResponse(HttpWebRequest request)
        {
            using (WebResponse webResponse = request.GetResponse() as HttpWebResponse)
            {
                StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                string fullResponse = responseReader.ReadToEnd();
                if (OnServerResponse != null)
                {
                    var response = JsonConvert.DeserializeObject<BacktraceServerResponse>(fullResponse);
                    OnServerResponse.Invoke(response);
                }
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
    }
}
