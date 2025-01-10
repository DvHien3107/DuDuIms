using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Utils.EnrichUniversal
{
    public class HttpSmood
    {
        public HttpClient httpClient()
        {
            HttpClientHandler _httpHandler = new HttpClientHandler();
            _httpHandler.Proxy = null;
            _httpHandler.UseProxy = false;
            return new HttpClient(_httpHandler);
        }
    }
}
