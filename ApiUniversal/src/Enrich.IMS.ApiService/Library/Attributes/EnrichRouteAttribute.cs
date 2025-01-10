using Microsoft.AspNetCore.Mvc;

namespace Enrich.IMS.RestApi.Library
{
    public class EnrichRouteAttribute : RouteAttribute
    {
        //public const string Root = "api";
        public const string Root = "";

        public bool IsSub { get; }

        public EnrichRouteAttribute(string module, bool sub = false)
            : base(sub ? $"{Root}/common/{module}" : $"{Root}/{module}")
        {
            IsSub = sub;
        }
    }
}
