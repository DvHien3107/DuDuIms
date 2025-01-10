using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Common
{
    public static class CommonExtensions
    {

        public static string GetRequestUri(this HttpRequest request) => UriHelper.GetDisplayUrl(request);

        public static string GetRequestIp(this HttpContext context) => context.Connection?.RemoteIpAddress?.ToString();

        /// <summary>
        /// current not work -> view ReadRequestBodyMiddleware
        /// </summary>
        public static string GetBodyAsString(this HttpRequest request)
        {
            try
            {
                return request.Body.GetStreamAsString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// current not work -> view ReadRequestBodyMiddleware
        /// </summary>
        public static string GetBodyAsString(this HttpResponse response)
        {
            try
            {
                return response.Body.GetStreamAsString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// current not work -> view ReadRequestBodyMiddleware
        /// </summary>
        public static string GetStreamAsString(this Stream stream)
        {
            if (stream == null)
            {
                return string.Empty;
            }

            try
            {
                using (var ms = new MemoryStream(2048))
                {
                    if (stream.CanSeek && stream.Position != 0)
                    {
                        stream.Position = 0;
                    }

                    stream.CopyTo(ms); // CopyToAsync ??

                    var asString = ms.ToArray();  // returns base64 encoded string JSON result
                    return Encoding.UTF8.GetString(asString); ;
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<string> GetResponseBodyAsString(this MemoryStream response)
        {
            try
            {
                //We need to read the response stream from the beginning...
                response.Seek(0, SeekOrigin.Begin);
                var gZipStream = new GZipStream(response, CompressionMode.Decompress);
                var text = await new StreamReader(gZipStream).ReadToEndAsync();
                //We need to reset the reader for the response so that the client can read it.
                response.Seek(0, SeekOrigin.Begin);
                return text;
            }
            catch
            {
                return string.Empty;
            }

        }

        public static (AuthenticationHeaderValue Authentication, (string Key, string Value)[] Extends) GetHeadersForAuth(this HttpRequest request)
        {
            var authorizationHeader = headerValue("Authorization");
            var accessToken = !string.IsNullOrEmpty(authorizationHeader) ? authorizationHeader.Replace("Bearer", string.Empty) : null;

            // from query-string: notifications
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                accessToken = request.Query["token"].FirstOrDefault();
            }
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return (null, null);
            }

            var authorization = AuthenticationHeaderValue.Parse(authorizationHeader?? $"Bearer {accessToken}");
            var extends = new (string Key, string Value)[]
            {
                ("Parameter", authorization.Parameter),
                ("Accept-Language", headerValue("Accept-Language")),
                ("AuthDomain", headerValue("AuthDomain") ?? headerValue("Origin")),
                ("X-Forwarded-For", headerValue("X-Forwarded-For")),
                ("X-App-SessionId", headerValue("X-App-SessionId")),
                ("RequestUrl", request.GetRequestUri()),
            };

            return (authorization, extends);

            string headerValue(string key) => request.Headers[key].FirstOrDefault();
        }
    }
}
