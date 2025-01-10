using Enrich.Dto;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Enrich.IMS.RestApi.Library.Filters
{
    public class EnrichResultFilter : IResultFilter //IAsyncResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            // Can't add to headers here because response has already begun.
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            var enrichContext = context.HttpContext.RequestServices.GetService(typeof(EnrichContext)) as EnrichContext;
            if (enrichContext != null)
            {
                string apiVersion;

                switch (enrichContext.FromSource)
                {
                    case EnrichContextFromSource.WebApp:
                    default:
                        apiVersion = enrichContext.WebApiVersion ?? "10.1.1";
                        break;
                }

                if (!string.IsNullOrWhiteSpace(apiVersion))
                    context.HttpContext.Response.Headers.Add("X-ApiVersion", new[] { apiVersion });
            }
        }
    }
}
