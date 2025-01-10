using Enrich.DataTransfer;
using Enrich.IServices;
using Enrich.IServices.Utils.JiraConnector;
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

namespace Enrich.Services.Utils.JiraConnector
{
    public class JiraConnectorAuthService : IJiraConnectorAuthService
    {
        private readonly string _connectorBaseUrl;
        private readonly string _authGetUrl;
        private readonly string _connectorAuthUrl;
        private readonly string _connectorCheckAuthUrl;
        private readonly ILogService _logger;
        private readonly EnrichContext _enrichContext;
        public JiraConnectorAuthService(ILogService logger, EnrichContext enrichContext)
        {
            _connectorBaseUrl = ConfigurationManager.AppSettings["JiraConnectorBaseUrl"];
            _authGetUrl = ConfigurationManager.AppSettings["ConnectorAuthGetUrl"];
            _connectorAuthUrl = ConfigurationManager.AppSettings["ConnectorAuthUrl"];
            _connectorCheckAuthUrl = ConfigurationManager.AppSettings["ConnectorCheckAuthUrl"];
            _logger = logger;
            _enrichContext = enrichContext;
        }
        public async Task<string> GetAuthorizeUrl()
        {
            try
            {
                var _auth = string.Empty;
                if (string.IsNullOrEmpty(_enrichContext.AccessToken)) _auth = $"Bearer {_enrichContext.AccessToken}";
                else _auth = $"Bearer {_enrichContext.AccessToken}";

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_connectorBaseUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", _auth);

                    using (var response = await httpClient.GetAsync(string.Format($"{_connectorBaseUrl}/{_authGetUrl}")))
                    {
                        string authUrl = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            if (!string.IsNullOrEmpty(authUrl))
                                return authUrl;
                        }
                        throw new Exception("cannot get authorize url jira");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[IMSConnectorService] get connector auth url failed");
                throw ex;
            }
        }


        public async Task<bool> Auth(string code)
        {
            try
            {
                var _auth = string.Empty;
                if (string.IsNullOrEmpty(_enrichContext.AccessToken)) _auth = $"Bearer {_enrichContext.AccessToken}";
                else _auth = $"Bearer {_enrichContext.AccessToken}";

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_connectorBaseUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", _auth);
                    var data = new StringContent(JsonConvert.SerializeObject(new ConnectorJiraAuthRequest { Code = code}));
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    using (var response = await httpClient.PostAsync(string.Format($"{_connectorBaseUrl}/{_connectorAuthUrl}"), data))
                    {
                        string responseJson = await response.Content.ReadAsStringAsync();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                           return true;
                        }
                        throw new Exception(responseJson);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[IMSConnectorService] get connector auth url failed");
                throw ex;
            }
        }

        public async Task<bool> CheckAuth()
        {
            try
            {
                var _auth = string.Empty;
                if (string.IsNullOrEmpty(_enrichContext.AccessToken)) _auth = $"Bearer {_enrichContext.AccessToken}";
                else _auth = $"Bearer {_enrichContext.AccessToken}";

                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(_connectorBaseUrl);
                    httpClient.DefaultRequestHeaders.Add("Authorization", _auth);
                  
                    using (var response = await httpClient.PostAsync($"{_connectorBaseUrl}/{_connectorCheckAuthUrl}",null))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            string responseJson = await response.Content.ReadAsStringAsync();
                            var result = JsonConvert.DeserializeObject<bool>(responseJson);
                            return result;
                        }
                        else
                        {
                            string responseJson = await response.Content.ReadAsStringAsync();
                            throw new Exception(responseJson);
                        }
                   
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[IMSConnectorService] get connector auth url failed");
                throw ex;
            }
        }

    } 
}
