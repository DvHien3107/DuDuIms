using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Enrich.RestApi.NetCorePlatform.Errors
{
    public class HttpServiceError
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public ServiceErrorModel ServiceError { get; set; }
    }
}
