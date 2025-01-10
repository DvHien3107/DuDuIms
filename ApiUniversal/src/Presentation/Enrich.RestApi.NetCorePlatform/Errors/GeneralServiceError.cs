using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.RestApi.NetCorePlatform.Errors
{
    public class GeneralServiceError : HttpServiceError
    {
        public GeneralServiceError()
        {
            HttpStatusCode = System.Net.HttpStatusCode.InternalServerError;
            ServiceError = new ServiceErrorModel()
            {
                Code = ServiceErrorEnum.InternalServerError,
                Details = "An error occured"
            };
        }
    }
}
