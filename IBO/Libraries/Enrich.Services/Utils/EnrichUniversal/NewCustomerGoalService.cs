using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichUniversal.NewCustomerGoal;
using Enrich.IServices;
using Enrich.IServices.Utils.EnrichUniversal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Utils.EnrichUniversal
{
    public class NewCustomerGoalService: INewCustomerGoalService
    {
        private readonly ILogService _logger;
        private readonly EnrichContext _enrichContext;
        private readonly string _apiUrl;
        private readonly string _secretKey;
        private readonly string _newCustomerGoalSearchUrl;
        private readonly string _newCustomerGoalCreateUrl;
        private readonly string _newCustomerGoalUpdateUrl;
        public NewCustomerGoalService(ILogService logger, EnrichContext enrichContext)
        {
            _apiUrl = ConfigurationManager.AppSettings["ApiUniversalUrl"];
            _secretKey = ConfigurationManager.AppSettings["UniversalBasicKey"];
            _newCustomerGoalSearchUrl = ConfigurationManager.AppSettings["ApiUniversalNewCustomerGoalSearch"];
            _newCustomerGoalCreateUrl = ConfigurationManager.AppSettings["ApiUniversalNewCustomerGoalCreate"];
            _newCustomerGoalUpdateUrl = ConfigurationManager.AppSettings["ApiUniversalNewCustomerGoalPatchUpdate"];
            _logger = logger;
            _enrichContext = enrichContext;
        }

        public async Task<NewCustomerGoalResponseDto> GetNewCustomerGoalList(NewCustomerGoalRequestDto request)
        {
            try
            {

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    var data = new StringContent(JsonConvert.SerializeObject(request));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_newCustomerGoalSearchUrl}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                        string contentResponse = await response.Content.ReadAsStringAsync();
                        var reponse = JsonConvert.DeserializeObject<NewCustomerGoalResponseDto>(contentResponse);
                        return reponse;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[FetchNewCustomerGoal] get merchant error");
                throw ex;
            }
        }

       
        
        public async Task Create(NewCustomerGoalDto request)
        {
            try
            {

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    var data = new StringContent(JsonConvert.SerializeObject(request));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // <--
                                                                                             // var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync($"{_apiUrl}/{_newCustomerGoalCreateUrl}", data))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK&& response.StatusCode != HttpStatusCode.Created)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[CreateGoal] create goal error");
                throw ex;
            }
        }
        public async Task PatchUpdate(List<NewCustomerGoalUpdateRequest> request,object id)
        {
            try
            {

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {_secretKey}");
                    var method = "PATCH";
                    var httpVerb = new HttpMethod(method);
                    var data = new StringContent(JsonConvert.SerializeObject(request));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json"); // <--
                    var httpRequestMessage =
                    new HttpRequestMessage(httpVerb, string.Format($"{_apiUrl}/{_newCustomerGoalUpdateUrl}", id))
                    {
                        Content = data
                    };                                                                    // var content = new StringContent(request.ToString(), Encoding.UTF8, "application/json");
                    using (var response = await httpClient.SendAsync(httpRequestMessage))
                    {
                        string responseJson = string.Empty;
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[UpdateGoal] update goal error");
                throw ex;
            }
        }
        
    }
}
