using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichOAuth;
using Enrich.IServices;
using Enrich.IServices.Utils.OAuth;
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

namespace Enrich.Services.Utils.OAuth
{
    public partial class EnrichOAuth : IEnrichOAuth
    {
        private readonly string _apiUrl;
        private readonly string _apiOAuthAccessToken;
        private readonly ILogService _logger;
        public EnrichOAuth(ILogService logger)
        {
            _apiUrl = ConfigurationManager.AppSettings["ApiOAuthUrl"];
            _apiOAuthAccessToken = ConfigurationManager.AppSettings["ApiOAuthAccessToken"];
           _logger = logger;
        }

        public async Task<MemberContext> Validation(string email, string password)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(_apiUrl);
                var request = new { Email = email, Password = password };
                var stringRequest = new StringContent(JsonConvert.SerializeObject(request), UnicodeEncoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(string.Format($"{_apiUrl}/{_apiOAuthAccessToken}"), stringRequest))
                {
                    string responseJson = await response.Content.ReadAsStringAsync();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var responseData = JsonConvert.DeserializeObject<MemberContext>(responseJson);
                        if (responseData != null)
                            return responseData;
                    }
                    throw new Exception(responseJson);
                }
            }
        }

        public string GetAccessToken(string email, string password)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_apiUrl);
                    var request = new { Email = email, Password = password };
                    var stringRequest = new StringContent(JsonConvert.SerializeObject(request), UnicodeEncoding.UTF8, "application/json");
                    using (var response = httpClient.PostAsync(string.Format($"{_apiUrl}/{_apiOAuthAccessToken}"), stringRequest).Result)
                    {
                        string responseJson = response.Content.ReadAsStringAsync().Result;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var responseData = JsonConvert.DeserializeObject<MemberContext>(responseJson);
                            if (responseData != null)
                                return responseData.AccessToken;
                        }
                        _logger.Error(responseJson);
                        throw new Exception(responseJson);
                    }
                }
            }
            catch(Exception e)
            {
                _logger.Error(e.ToString());
                return string.Empty;
            }
        }
    }
}