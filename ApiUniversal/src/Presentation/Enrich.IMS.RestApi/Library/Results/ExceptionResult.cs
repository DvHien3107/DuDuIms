using Microsoft.AspNetCore.Mvc;

namespace Enrich.IMS.RestApi.Library.Results
{
    public class ExceptionResult : ObjectResult
    {
        public ExceptionResult(int httpStatusCode, int businessCode, string message, object extendData = null)
            : base(new { code = businessCode, msg = message, extendData })
        {
            StatusCode = httpStatusCode;
        }
    }
}
