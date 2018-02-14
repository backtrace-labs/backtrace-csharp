using System;
using Backtrace.Model;
using System.Collections.Generic;
using System.Text;
using Backtrace.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Linq;
using Backtrace.Common;

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
        /// Send a backtrace data to server API. 
        /// </summary>
        /// <param name="data">Collected backtrace data</param>
        /// <returns>False if operation fail or true if API return OK</returns>
        public bool Send(BacktraceData<T> data)
        {
            string json = JsonConvert.SerializeObject(data);
            List<string> attachments = data.Attachments;
            return attachments.Any()
                ? SendAttachments(json, data.Attachments)
                : SendJson(json);
        }

        /// <summary>
        /// Send request to API with file attachments
        /// </summary>
        /// <param name="json">Diagnostics json</param>
        /// <param name="attachmentPaths">Attachments path</param>
        /// <returns>False if operation fail or true if API return OK</returns>
        private bool SendAttachments(string json, List<string> attachmentPaths)
        {
            Guid requestId = Guid.NewGuid();
            var formData = FormDataHelper.GetFormData(json, attachmentPaths, requestId);
            HttpWebRequest request = WebRequest.Create(_serverurl) as HttpWebRequest;

            //Set up the request properties.
            request.Method = "POST";
            request.ContentType = FormDataHelper.GetContentTypeWithBoundary(requestId);
            request.ContentLength = formData.Length;
            //Send the form data to the request.
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(formData, 0, formData.Length);
                requestStream.Close();
            }

            var webResponse = request.GetResponse() as HttpWebResponse;
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();
            return true;
        }

        /// <summary>
        /// Send request to API with single diagnostic JSON
        /// </summary>
        /// <param name="json">Generated diagnostic json</param>
        /// <returns>False if operation fail or true if API return OK</returns>
        private bool SendJson(string json)
        {
            using (var client = new WebClient())
            {
                ServicePointManager.SecurityProtocol = Tls12;
                client.Headers[HttpRequestHeader.ContentType] = "application/json";
                try
                {
                    var response = client.UploadString(address: _serverurl, method: "POST", data: json);
                }
                catch (Exception)
                {
                    //if there is any exception return false because operation fail
                    return false;
                }
            }
            return true;

        }

    }
}
