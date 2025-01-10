using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Errors
{
    public class HttpServiceErrorException : Exception
    {
        public readonly HttpServiceError HttpServiceError;

        public HttpServiceErrorException(HttpServiceError serviceError)
            : base()
        {
            HttpServiceError = serviceError;
        }

        public HttpServiceErrorException(HttpServiceError serviceError, string message)
            : base(message)
        {
            HttpServiceError = serviceError;
        }

        public HttpServiceErrorException(HttpServiceError serviceError, string message, Exception innerException)
            : base(message, innerException)
        {
            HttpServiceError = serviceError;
        }
    }
}
