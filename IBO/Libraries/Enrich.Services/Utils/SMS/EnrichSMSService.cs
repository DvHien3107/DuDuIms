using Enrich.DataTransfer;
using Enrich.IServices.Utils.SMS;
using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using Enrich.IServices;
using Enrich.DataTransfer.EnrichSMSService;
using System.Collections;
using System.Collections.Generic;
using Enrich.DataTransfer.EnrichUniversalService.MerchantReport;
using System.Linq;

namespace Enrich.Services.Utils.SMS
{
    public class EnrichSMSService : IEnrichSMSService
    {
        private readonly string _apiUrl;
        private readonly string _apiUniversalUrl;
        private readonly string _apiInitialStore;
        private readonly string _apiInsertHistory;
        private readonly string _apiGetTimeLine;
        private readonly string _secretKey;
        private readonly ILogService _logger;

        public EnrichSMSService(ILogService logger)
        {
            _apiUrl = ConfigurationManager.AppSettings["SMSServiceApiUrl"];
            _apiUniversalUrl = ConfigurationManager.AppSettings["ApiUniversalUrl"];
            _apiInitialStore = ConfigurationManager.AppSettings["SMSServiceApiInitialStore"];
            _apiInsertHistory = ConfigurationManager.AppSettings["SMSServiceApiInsertHistory"];
            _apiGetTimeLine = ConfigurationManager.AppSettings["SMSServiceApiGetTimeLine"];
            _secretKey = ConfigurationManager.AppSettings["SMSServiceSecretKey"];
            _logger = logger;
        }

        //public async Task InitialStoreDataAsync(string storeCode)
        //{
        //    try
        //    {
        //        using (var httpClient = new HttpClient())
        //        {
        //            httpClient.BaseAddress = new Uri(_apiUrl);
        //            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
        //            using (var response = await httpClient.GetAsync(string.Format($"{_apiUrl}/{_apiInitialStore}", storeCode)))
        //            {
        //                if (response.StatusCode != HttpStatusCode.Created)
        //                {
        //                    var responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        //                    throw new Exception(responseJson);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex, $"[InitialStoreDataAsync] initial store data error, store {storeCode}");
        //    }
        //}

        public async Task InsertHistorySMSRemainingAsync(SMSHistoryRemaining request)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    var stringContent = new StringContent(JsonConvert.SerializeObject(request), UnicodeEncoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(string.Format($"{_apiUrl}/{_apiInsertHistory}"), stringContent).ConfigureAwait(false))
                    {
                        if (response.StatusCode != HttpStatusCode.Created)
                            throw new Exception("Insert history SMS remaining error");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[InsertHistorySMSRemainingAsync] insert history error");
            }
        }

        public async Task<HistoryTimeLineResponse> GetTimeLineAsync(HistoryTimeLineRequest request)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    request.PageIndex = 1;
                    request.PageSize = 10000;
                    request.OrderBy = $"-{nameof(HistoryListItemDto.CreatedDate)}";

                    var stringContent = new StringContent(JsonConvert.SerializeObject(request), UnicodeEncoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(string.Format($"{_apiUniversalUrl}/{_apiGetTimeLine}"), stringContent).ConfigureAwait(false))
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var responseData = JsonConvert.DeserializeObject<HistoryTimeLineResponse>(responseJson);
                            if (responseData != null)
                            {
                                PopulateTimeline(responseData);
                                return responseData;
                            }
                        }
                        throw new Exception(responseJson);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetTimeLineAsync] get timeline error");
                throw ex;
            }
        }

        private void PopulateTimeline(HistoryTimeLineResponse respone)
        {
            respone.Records = respone.Records.Where(c => c.Type != 400 && c.Type != 500 && c.Type != 100);
        }
    }
}
