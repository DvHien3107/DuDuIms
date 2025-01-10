using Enrich.Dto.Base.Exceptions;
using Enrich.RestApi.NetCorePlatform.Errors;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform
{
    public static class EnrichExceptionHelper
    {
        public static (int StatusCode, object Model, bool LogError, bool PostBug) GetErrorInfo(ref Exception exception)
        {
            if (exception is EnrichException EnrichEx)
            {
                object model = null;
                var httpStatusCode = StatusCodes.Status500InternalServerError;

                if (EnrichEx is EnrichValidationException validateEx)
                {
                    if (EnrichEx.LogError) //change message of exception
                    {
                        var failures = (IEnumerable<EnrichValidationFailure>)validateEx.ExtendData;
                        var errorMessage = $"{validateEx.Message}:{Environment.NewLine}{string.Join(Environment.NewLine, failures.Select(a => $"{a.FullField}: {a.Rules} - {a.Message}"))}";
                        exception = new Exception(errorMessage, validateEx);
                    }

                    httpStatusCode = StatusCodes.Status400BadRequest;
                    model = new { code = EnrichEx.InternalCode, msg = EnrichEx.Message, extendData = EnrichEx.ExtendData };
                }
                else if (EnrichEx is EnrichMultipleException)
                {
                    var exceptions = (EnrichException[])EnrichEx.ExtendData;
                    model = new { code = EnrichEx.InternalCode, msg = EnrichEx.Message, extendData = exceptions.Select(a => new { code = a.InternalCode, msg = a.Message, extendData = a.ExtendData }) };
                }
                else
                {
                    model = new { code = EnrichEx.InternalCode, msg = EnrichEx.Message, extendData = EnrichEx.ExtendData };
                }

                if (EnrichEx.HttpStatusCode != null)
                {
                    httpStatusCode = EnrichEx.HttpStatusCode.Value;
                }

                return (httpStatusCode, model, EnrichEx.LogError, EnrichEx.PostBug);
            }

            return GetGlobalResponseInfo(exception);
        }

        private static (int StatusCode, object Model, bool LogError, bool PostBug) GetGlobalResponseInfo(Exception exception)
        {
            HttpServiceError httpError = new GeneralServiceError();
            bool logError = true, postBug = true;

            if (exception is HttpServiceErrorException serverException)
            {
                httpError = serverException.HttpServiceError;
            }
            else
            {
                httpError.ServiceError.Details = (exception.InnerException ?? exception).Message;
            }

            return ((int)httpError.HttpStatusCode, new { code = httpError.ServiceError.Code, msg = httpError.ServiceError.Details }, logError, postBug);
        }
    }
}
