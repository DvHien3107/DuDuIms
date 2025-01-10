using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Core.Container;
using Enrich.IMS.Dto.Authentication;
using Enrich.IMS.Services.Interface.Services;
using Enrich.RestApi.NetCorePlatform;
using Enrich.RestApi.NetCorePlatform.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Middleware
{
    public class AuthenticationMiddleware : BaseMiddleware
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly JwtSettings _jwtSettings;
        private readonly IEnrichContextFactory _enrichContextFactory;
        public AuthenticationMiddleware(RequestDelegate next, IAuthenticationService authenticationService, JwtSettings jwtSettings, IEnrichContextFactory enrichContextFactory)
            : base(next)
        {
            _authenticationService = authenticationService;
            _jwtSettings = jwtSettings;
            _enrichContextFactory = enrichContextFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsRequestOptions(context))
            {
                await Next.Invoke(context);
                return;
            }

            var authInfos = context.Request.GetHeadersForAuth();
            if (authInfos.Authentication != null)
            {
                var container = Resolve<IEnrichContainer>(context);

                var authProvider = container.ResolveByKeyed<IAuthProvider>(authInfos.Authentication.Scheme == "Basic" ? AuthMode.Basic : AuthMode.OAuth);

            }

            await Next.Invoke(context);
        }
    
    }

}
