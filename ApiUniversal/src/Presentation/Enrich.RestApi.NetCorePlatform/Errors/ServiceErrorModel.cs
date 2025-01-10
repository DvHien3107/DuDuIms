using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Errors
{
    public class ServiceErrorModel
    {
        public ServiceErrorModel()
        {
        }

        public ServiceErrorModel(ServiceErrorEnum code, string details)
        {
            Code = code;
            Details = details;
        }

        public ServiceErrorEnum Code { set; get; }
        public string Details { get; set; }
    }
}
