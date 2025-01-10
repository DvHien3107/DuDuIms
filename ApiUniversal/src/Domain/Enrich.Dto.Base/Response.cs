using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{   
    public class Response
    {
        [JsonProperty("code", Order = 1)]
        public HttpStatusCode Code { get; set; }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore, Order = 2)]
        public string Error { get; set; }

        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore, Order = 3)]
        public string Message { get; set; }

        [JsonProperty("elapse", NullValueHandling = NullValueHandling.Ignore, Order = 4)]
        public TimeSpan? Elapse { get; set; }

        [JsonProperty("traces", NullValueHandling = NullValueHandling.Ignore, Order = 5)]
        public IEnumerable<string> Traces { get; set; }
    }
 
    public class Response<T> : Response
    {
        [JsonProperty("data", Order = 6)]
        public T Data { get; set; }
    }
}
