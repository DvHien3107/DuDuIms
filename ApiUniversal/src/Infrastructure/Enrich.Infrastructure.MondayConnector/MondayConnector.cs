using Enrich.Core.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.MondayConnector
{
    public class MondayConnector : IMondayConnector
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiSyncSalesLead;
        private readonly string _Url;

        public MondayConnector(
            IConfiguration appConfig,
            IHttpClientFactory httpClientFactory)
        {
            _Url = appConfig["Services:MondayConnector:Url"];
            _apiSyncSalesLead = appConfig["Services:MondayConnector:SyncSalesLead"];
            _httpClientFactory = httpClientFactory;
        }

        public async Task<bool> SyncingSalesLeadAsync()
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_Url);
            using var response = await httpClient.GetAsync(string.Format(_apiSyncSalesLead)).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            return false;
        }

    }
}
