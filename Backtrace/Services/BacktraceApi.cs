using System;
using Backtrace.Model;
using System.Collections.Generic;
using Backtrace.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Security.Authentication;
using Backtrace.Common;
using System.Diagnostics;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Backtrace.Tests")]
namespace Backtrace.Services
{
    /// <summary>
    /// Create requests to Backtrace API
    /// </summary>
    internal class BacktraceApi<T> : IBacktraceApi<T>
    {
        public const SslProtocols _Tls12 = (SslProtocols)0x00000C00;
        public const SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;

        private readonly string _serverurl;
        private readonly BacktraceCredentials _credentials;

        /// <summary>
        /// Create a new instance of Backtrace API request.
        /// </summary>
        /// <param name="credentials">API credentials</param>
        /// <param name="timeout">Request timeout in milliseconds</param>
        public BacktraceApi(BacktraceCredentials credentials, int timeout = 5000)
        {
            _credentials = credentials;
            //_serverurl = $"{_credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={_credentials.Token}";
            _serverurl = $"{_credentials.BacktraceHostUri.AbsoluteUri}post?format=json&token={_credentials.Token}";
            bool isHttps = _credentials.BacktraceHostUri.Scheme == "https";
            //prepare web client to send a data to ssl API
            if (isHttps)
            {
                SslProtocols _Tls12 = (SslProtocols)0x00000C00;
                SecurityProtocolType Tls12 = (SecurityProtocolType)_Tls12;
                ServicePointManager.SecurityProtocol = Tls12;
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
        /// Send a backtrace data to server API. 
        /// </summary>
        /// <param name="data">Collected backtrace data</param>
        /// <returns>False if operation fail or true if API return OK</returns>
        public bool Send(BacktraceData<T> data)
        {
            string json = JsonConvert.SerializeObject(data, GetSerializerSettings());
            List<string> attachments = data.Attachments;
            return Send(json, attachments);
        }

        /// <summary>
        /// Send a backtrace data to server API. 
        /// </summary>
        /// <param name="json">Diagnostics json</param>
        /// <param name="attachmentPaths">Attachments path</param>
        /// <returns>False if operation fail or true if API return OK</returns>
        private bool Send(string json, List<string> attachmentPaths)
        {
            Guid requestId = Guid.NewGuid();
            var formData = FormDataHelper.GetFormData(json, attachmentPaths, requestId);
            HttpWebRequest request = WebRequest.Create(_serverurl) as HttpWebRequest;

            //Set up the request properties.
            request.Method = "POST";
            request.ContentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            request.ContentLength = formData.Length;

            try
            {
                //Send the form data to the request.
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Close();
                }
                using (WebResponse webResponse = request.GetResponse() as HttpWebResponse)
                {
                    StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
                    string fullResponse = responseReader.ReadToEnd();
                }
            }
            catch(Exception exception)
            {
                Trace.TraceWarning($"Backtrace C# Library: {exception.Message}");
                return false;
            }
            return true;
        }
    }
}
