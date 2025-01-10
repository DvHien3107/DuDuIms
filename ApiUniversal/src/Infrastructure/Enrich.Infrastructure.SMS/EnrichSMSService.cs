using Enrich.Common.Enums;
using Enrich.Core.Utils;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.EnrichSMSService;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Infrastructure.SMS
{
    public partial class EnrichSMSService : IEnrichSMSService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _secretKey;
        private readonly string _apiInsertHistory;
        private readonly string _apiSearchHistory;
        private readonly string _apiUrl;
        public EnrichSMSService(
            IConfiguration appConfig,
            IHttpClientFactory httpClientFactory
            )
        {
            _secretKey = appConfig["Services:SMSConfig:EnrichSMS:SecretKey"];
            _apiInsertHistory = appConfig["Services:SMSConfig:EnrichSMS:ApiInsertHistory"];
            _apiSearchHistory = appConfig["Services:SMSConfig:EnrichSMS:ApiSearchHistory"];
            _apiUrl = appConfig["Services:SMSConfig:EnrichSMS:ApiUrl"];
            _httpClientFactory = httpClientFactory;
        }

        public async Task InsertHistorySMSRemainingAsync(SMSHistoryRemaining request)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(_apiUrl);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");

            using var response = await httpClient.PostAsJsonAsync(string.Format(_apiInsertHistory), request).ConfigureAwait(false);
            if (response.StatusCode != HttpStatusCode.Created)
            {
                var responeJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new Exception(responeJson);
            }
        }

        public async Task<HistorySearchResponse> SearchHistoryAsync(HistorySearchRequest request)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri(_apiUrl);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                using (var response = await httpClient.PostAsJsonAsync(string.Format($"{_apiUrl}/{_apiSearchHistory}"), request).ConfigureAwait(false))
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseData = JsonConvert.DeserializeObject<HistorySearchResponse>(responseJson);
                        if (responseData != null)
                            return responseData;
                    }
                    throw new Exception(responseJson);
                }
            }
        }

        public async Task<HistoryTimeLineResponse> TimeLineHistoryAsync(HistoryTimeLineRequest request)
        {
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.BaseAddress = new Uri(_apiUrl);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                using (var response = await httpClient.PostAsJsonAsync(string.Format($"{_apiUrl}/{_apiSearchHistory}"), request).ConfigureAwait(false))
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseData = JsonConvert.DeserializeObject<HistoryTimeLineResponse>(responseJson);
                        if (responseData != null)
                        {
                            PupulateForLineTime(responseData);
                            return responseData;
                        }
                    }
                    throw new Exception(responseJson);
                }
            }
        }

        private void PupulateForLineTime(HistoryTimeLineResponse response)
        {
            var records = response.Records.
                GroupBy(c => new { c.Type, c.CreatedDate.Value.Month, c.CreatedDate.Value.Year }).
                Select(c => c.First()).ToList();
            records.ToList().ForEach(c =>
            {
                if (records.IndexOf(c) < records.Count - 1)
                    c.TotalSegment = c.RemainingSMS - records[records.IndexOf(c) + 1].RemainingSMS;
            });
            response.Records = records;
        }
    }
}
