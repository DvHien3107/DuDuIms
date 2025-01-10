using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform
{
    public static class AuthHelper
    {
        public const string BEARER_DECLARATION = "Bearer ";
        public const string BASIC_DECLARATION = "Basic ";

        public static AuthMode GetAuthMode(string headerAuthMode, string defAuthMode)
        {
            var authMode = !string.IsNullOrWhiteSpace(headerAuthMode) ? headerAuthMode : defAuthMode;
            return EnumHelper.GetEnumNull<AuthMode>(authMode) ?? AuthMode.OAuth;
        }     
    }

}
