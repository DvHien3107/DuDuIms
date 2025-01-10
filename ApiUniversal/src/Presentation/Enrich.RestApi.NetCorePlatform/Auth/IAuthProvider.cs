using Enrich.Common.Enums;
using Enrich.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Auth
{
    public interface IAuthProvider
    {
        string Name { get; }

        EnrichContext Context { get; }

        Task<(AuthStatus Status, EnrichContext Context, ClaimsPrincipal User)> ValidateAsync(params (string Key, string Value)[] values);
    }
}
