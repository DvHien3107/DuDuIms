using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Base.RestApi
{
    public static class PlatformHelper
    {
        public static Dictionary<string, string> ResponseHeaders = new Dictionary<string, string>
        {
            ["Access-Control-Allow-Origin"] = "*",
            ["Access-Control-Allow-Methods"] = "PATCH, PUT, GET, POST, DELETE, OPTIONS",
            ["Access-Control-Allow-Headers"] = "Content-Type, X-Requested-With, Authorization, Accept, Origin, AuthMode, AuthDomain, X-App-SessionId, X-App-From, X-Master-User",
            ["Access-Control-Expose-Headers"] = "X-PAGINATION-TOTAL, X-PAGINATION-PAGE-COUNT, X-PAGINATION-PAGE-SIZE, X-PAGINATION-PAGE-INDEX, X-AppVersion, X-ApiVersion, X-App-SessionId, X-App-From"
        };
    }
}
