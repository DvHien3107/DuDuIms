using Enrich.Common;
using Enrich.Core;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.RestApi.NetCorePlatform;
using Enrich.RestApi.NetCorePlatform.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Enrich.IMS.RestApi.Library.Filters
{
    public class EnrichExceptionFilter : IExceptionFilter // IAsyncExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            var info = EnrichExceptionHelper.GetErrorInfo(ref exception);

            if (context.Exception.Message.Contains("existing connection was forcibly closed by the remote host")) // huyngo: re-check SocketException ???
            {
                info.PostBug = false; // ignore
            }

            if (info.LogError || info.PostBug)
            {
                var requestInfo = GetRequestLogInfo(context.HttpContext);
                var extendMessage = requestInfo.FullMessage;

                var log = context.HttpContext.RequestServices.GetService(typeof(IEnrichLog)) as IEnrichLog;
                if (context.HttpContext.RequestAborted.IsCancellationRequested == true)
                {
                    log.Warning($"Request was cancelled. {extendMessage}");
                }
                else
                {
                    if (info.LogError)
                    {

                        CustomErrorHandler.LogError(log, exception, extendMessage);
                    }

                if (info.PostBug)
                {
                    //var postBug = context.HttpContext.RequestServices.GetService(typeof(IEnrichSlack)) as IEnrichSlack;
                    //CustomErrorHandler.PostBugAsync(postBug, exception, extendMessage);
                }

                }


            }

            context.HttpContext.Response.StatusCode = info.StatusCode;
            context.Result = new JsonResult(info.Model);
        }

        private (string Method, string Url, string Body, string Query, string Header, string FullMessage) GetRequestLogInfo(HttpContext context)
        {
            var info = default((string Method, string Url, string Body, string Query, string Header, string FullMessage));

            if (context?.Request != null)
            {
                try
                {
                    var erichcontext = context.RequestServices.GetService(typeof(EnrichContext)) as EnrichContext;

                    info.Method = context.Request.Method;
                    info.Url = erichcontext?.Request.Url ?? context.Request.GetRequestUri();
                    info.Query = CustomErrorHandler.ToJson(context.Request.Query);
                    info.Header = erichcontext?.Request.HeaderJson ?? CustomErrorHandler.ToJson(context.Request.Headers);

                    //info.Body = context.Request.GetBodyAsString();
                    info.Body = erichcontext?.Request.BodyJson;

                    info.FullMessage = $"{info.Method} \"{info.Url}\", Caller({erichcontext?.UserId}), Body({info.Body}), Header({info.Header})";
                }
                catch { }
            }

            if (info.FullMessage == null) info.FullMessage = string.Empty;

            return info;
        }

    }
}
