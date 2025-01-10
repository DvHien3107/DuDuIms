using Enrich.DataTransfer.Twilio;
using Enrich.IServices;
using Enrich.IServices.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Services.Utils
{
    public class PosSyncTwilioApiService: IPosSyncTwilioApiService
    {
        private readonly string _posApiBaseUrl = "";
        private readonly string _posUpdateTwilioPhoneApiUrl = "";
        private readonly ILogService _logger;
        public PosSyncTwilioApiService(ILogService logger)
        {
            _posApiBaseUrl = ConfigurationManager.AppSettings["POSApiBaseUrl"];
           
            _posUpdateTwilioPhoneApiUrl = ConfigurationManager.AppSettings["POSApiUpdateTwilioPhoneNumber"];
            _logger = logger;
        }
        public async Task PosUpdateTwilioPhoneNumerAsync(PosUpdateTwilioPhoneNumberRequest request)
        {
            try
            {
                _logger.Info($"[POSApi] start sync phone number to pos, request: {JsonConvert.SerializeObject(request)}");
                using (var httpClient = new HttpClient())
                {
                    var payload = JsonConvert.SerializeObject(request);

                    // Wrap our JSON inside a StringContent object
                    var content = new StringContent(payload, Encoding.UTF8, "application/json");
                    httpClient.BaseAddress = new Uri(_posApiBaseUrl);
                    using (var response = await httpClient.PostAsync($"{_posApiBaseUrl}/{_posUpdateTwilioPhoneApiUrl}", content))
                    {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<dynamic>(responseJson);
                    _logger.Info($"[POSApi] end sync phone number to pos, request: {JsonConvert.SerializeObject(responseJson)}");
                    if (responseData.status != 200)
                            throw new Exception(responseJson);
                    }
                }
                _logger.Info($"[POSApi] end sync phone number to pos, request: {JsonConvert.SerializeObject(request)}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[POSApi] Twilio cannot sync phone number to pos, request: {JsonConvert.SerializeObject(request)}");
                throw new Exception(ex.Message);
            }
        }
    }
}
