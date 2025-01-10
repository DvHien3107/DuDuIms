using Enrich.RestApi.NetCorePlatform;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Middleware
{
    public class RequestOptionsMiddleware : BaseMiddleware
    {
        public RequestOptionsMiddleware(RequestDelegate next) : base(next)
        {
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // add accept headers
            foreach (var item in PlatformHelper.ResponseHeaders)
            {
                context.Response.Headers.Add(item.Key, new[] { item.Value });
            }

            if (IsRequestOptions(context))
            {
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(string.Empty);

                return;
            }

            await Next.Invoke(context).ConfigureAwait(false);
        }
    }
}
