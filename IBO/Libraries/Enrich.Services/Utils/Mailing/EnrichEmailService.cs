using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Utils.Mailing
{
    public class EnrichEmailService : IEnrichEmailService
    {
        private readonly string _apiUrl;
        private readonly string _secretKey;
        private readonly string _apiInitialStore;

        private readonly ILogService _logger;

        public EnrichEmailService(ILogService logger)
        {
            _logger = logger;
            _apiUrl = ConfigurationManager.AppSettings["EmailServiceApiUrl"];
            _secretKey = ConfigurationManager.AppSettings["UniversalBasicKey"];
            _apiInitialStore = ConfigurationManager.AppSettings["EmailServiceApiInitialStore"];
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
        //        _logger.Error(ex, $"[InitialStoreDataAsync] initial store data email error, store {storeCode}");
        //    }
        //}
    }
}
