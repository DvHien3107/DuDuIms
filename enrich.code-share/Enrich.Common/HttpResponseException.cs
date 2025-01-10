using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common
{
    public class HttpResponseException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
    {
        /// <summary>
        /// Gets status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets description.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Construct http status code exception. 
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="message">Message.</param>
        /// <param name="description">Description.</param>
        /// <param name="innerException">Inter exception.</param>
        public HttpResponseException(HttpStatusCode statusCode, string message, string description = null, Exception innerException = null) : base(message, innerException)
        {
            StatusCode = statusCode;
            Description = description;
        }

        /// <inheritdoc />
        /// <summary>
        /// Construct http status code exception. 
        /// </summary>
        /// <param name="statusCode">Status code</param>
        /// <param name="info">Serialize information.</param>
        /// <param name="context">Streaming context.</param>
        /// <exception cref="SerializationException">The class name is null or <see>
        ///     <cref>System.Exception.HResult</cref>
        /// </see> is zero (0).</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="info">info</paramref> parameter is null.</exception>
        protected HttpResponseException(HttpStatusCode statusCode, SerializationInfo info, StreamingContext context) : base(info, context) => StatusCode = statusCode;
    }
}
