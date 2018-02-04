using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace
{
    /// <summary>
    /// Bactrace credentials informations
    /// </summary>
    public class BacktraceCredentials
    {
        private readonly Uri _backtraceHostUri;
        private readonly string _accessToken;

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
        /// Initialize Backtrace credencials
        /// </summary>
        /// <param name="backtraceHostUri">Uri to Backtrace host</param>
        /// <param name="accessToken">Access token to Backtrace services</param>
        public BacktraceCredentials(
            Uri backtraceHostUri,
            string accessToken)
        {
            if (!IsValid(backtraceHostUri, accessToken)) {
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
            string accessToken)
        : this(new Uri(backtraceHostUrl), accessToken)
        {

        }

        /// <summary>
        /// Check if model passed to constructor is valid
        /// </summary>
        /// <param name="uri">Backtrace service uri</param>
        /// <param name="token">Access token to Backtrace services</param>
        /// <returns>validation result</returns>
        internal bool IsValid(Uri uri, string token)
        {
            return string.IsNullOrEmpty(token) && uri.IsWellFormedOriginalString();
        }


        /// <summary>
        /// Check if Backtrace Credential has valid information
        /// </summary>
        internal bool IsValid()
        {
            return string.IsNullOrEmpty(_accessToken) && _backtraceHostUri.IsWellFormedOriginalString();
        }
    }
}
