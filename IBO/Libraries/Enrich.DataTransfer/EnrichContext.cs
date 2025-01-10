using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer
{
    public partial class EnrichContext
    {
        public string Id { get; set; }

        public long MemberId { get; set; }
        public string MemberEmail { get; set; }

        public string UserName { get; set; }

        public string SessionId { get; set; }

        public string AccessToken { get; set; }

        public string ApplicationName { get; set; }
        public string MemberFullName { get; set; }

        public string TrackTraceId { get; set; }

        public bool IsProduction { get; set; }

        public string ServerRootPath { get; set; }

        public RequestInfo Request { get; set; } = new RequestInfo();

        public EnrichContext()
        {            
            Id = Guid.NewGuid().ToString();
            IsProduction = "Production" == ConfigurationManager.AppSettings["Environment"];
         }
    }

    public class RequestInfo
    {
        public string Url { get; set; }

        public string BodyJson { get; set; }

        public string HeaderJson { get; set; }
    }

}
