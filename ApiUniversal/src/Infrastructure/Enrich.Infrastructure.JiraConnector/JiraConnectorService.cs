using Enrich.Common;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.Dto.Base;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.JiraConnector
{
    public class JiraConnectorService : IJiraConnectorService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _baseUrl;
        private readonly string _createIssue;
        private readonly string _updateIssue;
        private readonly EnrichContext _enrichContext;

        public JiraConnectorService(
            IConfiguration appConfig,
            IHttpClientFactory httpClientFactory,
            EnrichContext enrichContext)
        {
            _baseUrl = appConfig["Services:JiraConnector:BaseUrl"];
            _createIssue = appConfig["Services:JiraConnector:IssueCreate"];
            _updateIssue = appConfig["Services:JiraConnector:IssueUpdate"];
            _httpClientFactory = httpClientFactory;
            _enrichContext = enrichContext;
        }

        public async Task CreateIssue(long ticketId)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.Auth.AccessToken}");
            var stringContent = new StringContent(JsonConvert.SerializeObject(new {ticketId = ticketId }), Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync($"{_baseUrl}/{_createIssue}", stringContent);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var payload = JsonConvert.DeserializeObject<Response>(payloadString);
                if (payload.Code == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(payload.Message);

                throw new HttpResponseException(payload.Code, payload.Message);
            }
        }

        public async Task UpdateIssue(long ticketId)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_baseUrl);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_enrichContext.Auth.AccessToken}");
            var stringContent = new StringContent(JsonConvert.SerializeObject(new { ticketId = ticketId }), Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync($"{_baseUrl}/{_updateIssue}", stringContent);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            else
            {
                var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var payload = JsonConvert.DeserializeObject<Response>(payloadString);
                if (payload.Code == HttpStatusCode.Unauthorized)
                    throw new UnauthorizedAccessException(payload.Message);

                throw new HttpResponseException(payload.Code, payload.Message);
            }
        }

    }
}
