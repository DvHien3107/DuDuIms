using Enrich.Common;
using Enrich.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto
{
    public partial class EnrichContext
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public long UserId { get; set; }
        public int SiteId { get; set; }
        public string UserNumber { get; set; }
        public string UserEmail { get; set; }
        public string UserFullName { get; set; } = Constants.SystemName.ToString();
        public Language Language { get; set; }

        /// <summary>
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.http.httpcontext.traceidentifier?view=aspnetcore-5.0
        /// </summary>
        public string TraceIdentifier { get; set; }
        public string ServerRootPath { get; set; }
        public string SessionId { get; set; }
        public string RecurringTime { get; set; }

        public string WebApiVersion { get; set; }

        public EnrichContextFromSource FromSource { get; set; } = EnrichContextFromSource.WebApp;
    }

    public partial class EnrichContext
    {
        public AuthInfo Auth { get; set; } = new AuthInfo();

        public RequestInfo Request { get; set; } = new RequestInfo();

        public class AuthInfo
        {
            public string AccessToken { get; set; }

            public bool HasChecked { get; set; }
        }

        public class RequestInfo
        {
            public string Url { get; set; }

            public string BodyJson { get; set; }

            public string HeaderJson { get; set; }
        }

        public void Release()
        {
        }
    }

    public enum EnrichContextFromSource
    {
        WebApp, 

        Mobile
    }
}
