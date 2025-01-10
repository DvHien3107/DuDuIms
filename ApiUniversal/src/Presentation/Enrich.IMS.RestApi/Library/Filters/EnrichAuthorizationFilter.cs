using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Core.Container;
using Enrich.RestApi.NetCorePlatform.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Library.Filters
{
    public class EnrichAuthorizationFilter : IAsyncAuthorizationFilter // IAuthorizationFilter
    {
        private readonly IEnrichContainer _container;
        public EnrichAuthorizationFilter(IEnrichContainer container)
        {
            _container = container;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!NeedCheck(context))
            {
                return;
            }

            AuthStatus status;

            // from auth-provider
            var authInfo = context.HttpContext.Request.GetHeadersForAuth();
            if (authInfo.Authentication == null)
            {
                status = AuthStatus.Unauthorized;
                goto Unauthorized;
            }

            var authProvider = _container.ResolveByKeyed<IAuthProvider>(authInfo.Authentication.Scheme == "Basic" ? AuthMode.Basic : AuthMode.OAuth);
            var authResponse = await authProvider.ValidateAsync(authInfo.Extends);

            if (authResponse.Status == AuthStatus.Success) // OK
            {
                context.HttpContext.User = authResponse.User;
                context.HttpContext.User = new System.Security.Claims.ClaimsPrincipal() {};

				return;
            }

            status = authResponse.Status;

        Unauthorized:
            context.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Result = new JsonResult(new { code = 401, msg = status.ToString() });
            return;
        }

        private bool NeedCheck(AuthorizationFilterContext context)
        {
            if (!context.Filters.Any(a => a is EnrichAuthAttribute))
            {
                return false;
            }

            // ignore Action has AllowAnonymousFilter
            return !HasAllowAnonymous(context);
        }

        private static bool HasAllowAnonymous(AuthorizationFilterContext context)
        {
            var filters = context.Filters;
            if (filters.OfType<IAllowAnonymousFilter>().Any())
            {
                return true;
            }

            // When doing endpoint routing, MVC does not add AllowAnonymousFilters for AllowAnonymousAttributes that
            // were discovered on controllers and actions. To maintain compat with 2.x,
            // we'll check for the presence of IAllowAnonymous in endpoint metadata.
            var endpoint = context.HttpContext.GetEndpoint();
            return endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null;
        }

    }
}
