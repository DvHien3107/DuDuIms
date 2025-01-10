using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Enrich.IMS.RestApi.Middleware
{
    public abstract class BaseMiddleware
    {
        protected readonly RequestDelegate Next;

        public BaseMiddleware(RequestDelegate next) => Next = next;
        protected bool IsRequestOptions(HttpContext context) => context.Request.Method == "OPTIONS";

        protected T Resolve<T>(HttpContext context) => context.RequestServices.GetService<T>();

        protected string ToJson(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.None);
            }
            catch
            {
                return $"{obj}";
            }
        }

    }
}
