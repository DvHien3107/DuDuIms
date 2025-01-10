using Enrich.Core.Infrastructure;
using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichUniversalService.MerchantReport;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.OAuth;
using Enrich.IServices.Utils.SMS;
using Enrich.IServices.Utils.Universal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Web;
using Enrich.Services.Utils.EnrichUniversal;

namespace Enrich.Services.Utils.Universal
{
    public partial class EnrichUniversalService : IEnrichUniversalService
    {
        private readonly string _apiUrl;
        private readonly string _apiNextgenUrl;
        private readonly string _apiUniversalGetRecurringCard;
        private readonly string _customerSearchUniversalAPI;
        private readonly string _apiUniversalGetMerchantSummaries;
        private readonly string _apiUniversalReportCustomerForChart;
        private readonly string _apiUniversalLookupData;
        private readonly string _apiSyncSalesLeadFromMonday;
        private readonly string _secretKey;
        private readonly ILogService _logger;
        private readonly EnrichContext _enrichContext;
        private readonly IEnrichEmailService _enrichEmailService;
        private readonly IEnrichSMSService _enrichSMSService;

        public EnrichUniversalService(
            ILogService logger,
            EnrichContext enrichContext,
            IEnrichEmailService enrichEmailService,
            IEnrichSMSService enrichSMSService)
        {
            _apiUrl = ConfigurationManager.AppSettings["ApiUniversalUrl"];
            _apiNextgenUrl = ConfigurationManager.AppSettings["POSNextGenApi"];
            _apiUniversalGetRecurringCard = ConfigurationManager.AppSettings["ApiUniversalGetRecurringCard"];
            _customerSearchUniversalAPI = ConfigurationManager.AppSettings["UniversalSearhMerchantUrl"];
            _apiUniversalGetMerchantSummaries = ConfigurationManager.AppSettings["ApiUniversalGetMerchantSummaries"];
            _apiUniversalLookupData = ConfigurationManager.AppSettings["ApiUniversalLookupData"];
            _apiUniversalReportCustomerForChart = ConfigurationManager.AppSettings["ApiUniversalReportCustomerForChart"];
            _apiSyncSalesLeadFromMonday = ConfigurationManager.AppSettings["ApiSyncSalesLeadFromMonday"];
            _secretKey = ConfigurationManager.AppSettings["UniversalBasicKey"];
            _logger = logger;
            _enrichContext = enrichContext;
            _enrichEmailService = enrichEmailService;
            _enrichSMSService = enrichSMSService;
        }

        public async Task<string> GetRecurringCardAsync(string customerCode)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    using (var response = await httpClient.GetAsync(string.Format($"{_apiUrl}/{_apiUniversalGetRecurringCard}", customerCode)))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            throw new Exception(responseJson);
                        }
                        responseJson = await response.Content.ReadAsStringAsync();
                        //var responseData = JsonConvert.DeserializeObject(responseJson);
                        return responseJson;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetRecurringCardAsync] get recurring card error, store {customerCode}");
                return "N/A";
            }
        }

        public async Task<CustomerSearchResponse> GetMerchantFromUniversal(CustomerSearchRequest request)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    //httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    var data = new StringContent(JsonConvert.SerializeObject(request));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // <--
                                                                                             // var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_customerSearchUniversalAPI}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<CustomerSearchResponse>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchMerchant] get merchant error");
                throw ex;
            }
        }


        //public async Task<CustomerSearchResponse> GetMerchantFromUniversal(CustomerSearchRequest request)
        //{
        //    try
        //    {
        //        using (var httpClient = new HttpClient())
        //        {
        //            httpClient.BaseAddress = new Uri(_apiNextgenUrl);
        //            //httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");

        //            var stringRequest = new StringContent(JsonConvert.SerializeObject(request), UnicodeEncoding.UTF8, "application/json");


        //            using (var response = await httpClient.PostAsync($"{_apiNextgenUrl}/v1/customer/searchv2", stringRequest))
        //            {
        //                string responseJson = string.Empty;
        //                if (response.StatusCode != HttpStatusCode.OK)
        //                {
        //                    responseJson = await response.Content.ReadAsStringAsync();
        //                    throw new Exception(responseJson);
        //                }
        //                string contentResponse = await response.Content.ReadAsStringAsync();
        //                var reponse = JsonConvert.DeserializeObject<CustomerSearchResponse>(contentResponse);
        //                return reponse;
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex, $"[FetchMerchant] get merchant error");
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// Get Merchant Summaries async
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public async Task<CustomerReportSummary> GetMerchantSummariesAsync()
        {
            try
            {
                var httpSmood = new HttpSmood();
                var httpClient = httpSmood.httpClient();
                httpClient.BaseAddress = new Uri(_apiUrl);
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                var request = new CustomerReportRequest();
                string x = JsonConvert.SerializeObject(request);
                var stringRequest = new StringContent(JsonConvert.SerializeObject(request), UnicodeEncoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(string.Format($"{_apiUrl}/{_apiUniversalGetMerchantSummaries}"), stringRequest))
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseData = JsonConvert.DeserializeObject<CustomerReportResponse>(responseJson);
                        if (responseData != null)
                            return responseData.Summary;
                    }
                    throw new Exception(responseJson);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetMerchantSummariesAsync] get merchant summaries error");
                return new CustomerReportSummary();
            }
        }

        public async Task<object> ReportCustomerForChartAsync(CustomerChartReportRequest request)
        {
            try
            {
                var _auth = string.Empty;
                if (string.IsNullOrEmpty(_enrichContext.AccessToken)) _auth = $"Basic {_secretKey}";
                else _auth = $"Bearer {_enrichContext.AccessToken}";

                //var httpClient = new HttpClient();
                var httpSmood = new HttpSmood();
                var httpClient = httpSmood.httpClient();
                    httpClient.BaseAddress = new Uri(_apiUrl);
                httpClient.DefaultRequestHeaders.Add("Authorization", _auth);
                var stringRequest = new StringContent(JsonConvert.SerializeObject(request), UnicodeEncoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(string.Format($"{_apiUrl}/{_apiUniversalReportCustomerForChart}/{request.Condition.Type}"), stringRequest))
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    string x = JsonConvert.SerializeObject(request);
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return responseJson;
                    }
                    throw new Exception(responseJson);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetMerchantSummariesAsync] get merchant summaries error");
                throw ex;
            }
        }

        public async Task<object> GetLookupDataAsync(string type)
        {
            var lookups = new object { };
            if (string.IsNullOrWhiteSpace(type)) return lookups;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    using (var response = await httpClient.GetAsync(string.Format($"{_apiUrl}/{_apiUniversalLookupData}/{type}")))
                    {
                        string responseJson = string.Empty;
                        responseJson = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode != HttpStatusCode.OK)
                            throw new Exception(responseJson);

                        var responseData = JsonConvert.DeserializeObject(responseJson);
                        return responseData;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[GetLookupDataAsync] get lookup data error, type = {type}");
                return lookups;
            }
        }

        public async Task InitialStoreDataAsync(string storeCode)
        {
           // await _enrichEmailService.InitialStoreDataAsync(storeCode);

          //  await _enrichSMSService.InitialStoreDataAsync(storeCode);
        }

        public async Task<bool> SyncingSalesLeadFromMondayAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    using (var response = await httpClient.GetAsync(string.Format($"{_apiUrl}/{_apiSyncSalesLeadFromMonday}")))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                            return true;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[SyncingSalesLeadFromMondayAsync] syncing sales lead from monday error");
                return false;
            }
        }

        public EnrichContext readContext()
        {
            return _enrichContext;
        }
    }
}