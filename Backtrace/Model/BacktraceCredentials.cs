using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Net;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Bactrace credentials informations
    /// </summary>
    public class BacktraceCredentials
    {
        private const string _configurationHostRecordName = "HostUrl";
        private const string _configurationTokenRecordName = "Token";

        private readonly Uri _backtraceHostUri;
        private readonly byte[] _accessToken;

        /// <summary>
        /// Get a Uri to Backtrace servcie
        /// </summary>
        public Uri BacktraceHostUri
        {
            get
            {
                return _backtraceHostUri;
            }
        }

        /// <summary>
        /// Get an access token
        /// </summary>
        internal string Token
        {
            get
            {
                return Encoding.UTF8.GetString(_accessToken);
            }
        }

#if !NET35
        public WebProxy Proxy { get; set; } = null;
#endif

        /// <summary>
        /// Create submission url to Backtrace API
        /// </summary>
        /// <returns></returns>
        internal Uri GetSubmissionUrl()
        {
            if (_backtraceHostUri == null)
            {
                throw new ArgumentException(nameof(BacktraceHostUri));
            }

            var uriBuilder = new UriBuilder(BacktraceHostUri);
            if (submitUrl)
            {
                return uriBuilder.Uri;
            }
            if (string.IsNullOrEmpty(Token))
            {
                throw new ArgumentException(nameof(Token));
            }

            if (!uriBuilder.Scheme.StartsWith("http"))
            {
                uriBuilder.Scheme = $"https://{uriBuilder.Scheme}";
            }
            if (!uriBuilder.Path.EndsWith("/") && !string.IsNullOrEmpty(uriBuilder.Path))
            {
                uriBuilder.Path += "/";
            }
            uriBuilder.Path = $"{uriBuilder.Path}post";
            uriBuilder.Query = $"format=json&token={Token}";
            return uriBuilder.Uri;
        }
        private readonly bool submitUrl = false;

        /// <summary>
        /// Initialize Backtrace credentials with Backtrace submit url. 
        /// If you pass backtraceSubmitUrl you have to make sure url to API is valid and contains token
        /// </summary>
        /// <param name="backtraceSubmitUrl">Backtrace submit url</param>
        public BacktraceCredentials(
            string backtraceSubmitUrl)
            : this(new Uri(backtraceSubmitUrl))
        { }

        /// <summary>
        /// Initialize Backtrace credentials with Backtrace submit url. 
        /// If you pass backtraceSubmitUrl you have to make sure url to API is valid and contains token
        /// </summary>
        /// <param name="backtraceSubmitUrl">Backtrace submit url</param>
        public BacktraceCredentials(Uri backtraceSubmitUrl)
        {
            var hostToCheck = backtraceSubmitUrl.Host;
            if (!hostToCheck.StartsWith("www."))
            {
                hostToCheck = $"www.{hostToCheck}";
            }
            submitUrl = hostToCheck.StartsWith("www.submit.backtrace.io");
            if (!submitUrl)
            {
                throw new ArgumentException(nameof(backtraceSubmitUrl));
            }
            _backtraceHostUri = backtraceSubmitUrl;
        }

        /// <summary>
        /// Initialize Backtrace credencials
        /// </summary>
        /// <param name="backtraceHostUri">Uri to Backtrace host</param>
        /// <param name="accessToken">Access token to Backtrace services</param>
        /// <exception cref="ArgumentException">Thrown when uri to backtrace is invalid or accessToken is null or empty</exception>
        public BacktraceCredentials(
            Uri backtraceHostUri,
            byte[] accessToken)
        {
            if (!IsValid(backtraceHostUri, accessToken))
            {
                throw new ArgumentException($"{nameof(backtraceHostUri)} or {nameof(accessToken)} is not valid.");
            }
            _backtraceHostUri = backtraceHostUri;
            _accessToken = accessToken;
        }
        /// <summary>
        /// Initialize Backtrace credencials
        /// </summary>
        /// <param name="backtraceHostUrl">Url to Backtrace Url</param>
        /// <param name="accessToken">Access token to Backtrace services</param>
        public BacktraceCredentials(
            string backtraceHostUrl,
            byte[] accessToken)
        : this(new Uri(backtraceHostUrl), accessToken)
        { }

        /// <summary>
        /// Initialize Backtrace credencials
        /// </summary>
        /// <param name="backtraceHostUrl">Url to Backtrace Url</param>
        /// <param name="accessToken">Access token to Backtrace services</param>
        public BacktraceCredentials(
            string backtraceHostUrl,
            string accessToken)
        : this(backtraceHostUrl, Encoding.UTF8.GetBytes(accessToken))
        { }

        /// <summary>
        /// Initialize Backtrace credencials
        /// </summary>
        /// <param name="backtraceHostUri">Url to Backtrace Url</param>
        /// <param name="accessToken">Access token to Backtrace services</param>
        public BacktraceCredentials(
            Uri backtraceHostUri,
            string accessToken)
        : this(backtraceHostUri, Encoding.UTF8.GetBytes(accessToken))
        { }
        /// <summary>
        /// Check if model passed to constructor is valid
        /// </summary>
        /// <param name="uri">Backtrace service uri</param>
        /// <param name="token">Access token to Backtrace services</param>
        /// <returns>validation result</returns>
        internal bool IsValid(Uri uri, byte[] token)
        {
            return token != null && token.Length > 0 && uri.IsWellFormedOriginalString();
        }

#if NET35 || NET48
        /// <summary>
        /// Read Backtrace credentials from application configuration
        /// </summary>
        /// <param name="sectionName">Credentials section name on app.config or web.config</param>
        /// <returns>New BacktraceCredentials instance</returns>
        /// <exception cref="ArgumentException">Thrown when a section is null or empty</exception>
        /// <exception cref="InvalidOperationException">Thrown when a application settings are not defined</exception>
        internal static BacktraceCredentials ReadConfigurationSection(string sectionName)
        {
            if (string.IsNullOrEmpty(sectionName))
            {
                throw new ArgumentException($"Section {nameof(sectionName)} is null or empty");
            }
            if (!(ConfigurationManager.GetSection(sectionName) is NameValueCollection applicationSettings) || applicationSettings.Count == 0)
            {
                throw new InvalidOperationException("Application setting are not defined");
            }
            string backtraceHostUri = applicationSettings[_configurationHostRecordName];
            string accessToken = applicationSettings[_configurationTokenRecordName];
            return new BacktraceCredentials(backtraceHostUri, accessToken);
        }
#endif
    }
}
