using Backtrace.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backtrace.Model
{
    /// <summary>
    /// Send method result
    /// </summary>
    public class BacktraceResult
    {
        private string _message;
        /// <summary>
        /// Message
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                _message = value;
                Result = BacktraceResultType.ServerError;
            }
        }

        /// <summary>
        /// Result
        /// </summary>
        BacktraceResultType Result { get; set; }

        private string _object;
        /// <summary>
        /// Created object id
        /// </summary>
        [JsonProperty(PropertyName = "object")]
        public string Object
        {
            get
            {
                return _object;
            }
            set
            {
                _object = value;
                Result = BacktraceResultType.Ok;
            }
        }

        /// <summary>
        /// Set result when client rate limit reached
        /// </summary>
        /// <returns>BacktraceResult with limit reached information</returns>
        internal static BacktraceResult OnLimitReached()
        {
            return new BacktraceResult()
            {
                Result = BacktraceResultType.LimitReached,
                Message = "Rate limiting reached"
            };
        }

        /// <summary>
        /// Set result when error occurs while sending data to API
        /// </summary>
        /// <param name="exception">Exception</param>
        /// <returns>BacktraceResult with exception information</returns>
        internal static BacktraceResult OnError(Exception exception)
        {
            return new BacktraceResult()
            {
                Message = exception.Message
            };
        }
    }
}