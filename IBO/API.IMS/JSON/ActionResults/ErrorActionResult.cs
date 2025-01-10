using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace API.IMS.JSON.ActionResults
{
        public class ErrorActionResult : IHttpActionResult
        {
            private readonly string _jsonString;
            private readonly HttpStatusCode _statusCode;

            public ErrorActionResult(string jsonString, HttpStatusCode statusCode)
            {
                _jsonString = jsonString;
                _statusCode = statusCode;
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var content = new StringContent(_jsonString);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = new HttpResponseMessage(_statusCode) { Content = content };
                return Task.FromResult(response);
            }
        
    }
}