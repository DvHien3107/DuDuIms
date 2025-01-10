using Enrich.IMS.Entities;
using Microsoft.Identity.Client;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Singleton
{
    public interface ITwilioService
    {
        Task<string> getMessageService(string account_sid, string authToken, string type);
        Task<string> createMessageService(string friendlyname, string account_sid, string authToken);
        Task<string> getPhoneMessageService(string account_sid, string authToken, string messageservice, string PhoneNumber);
        Task<string> createPhoneMessageService(string messageservice, string phonesid, string account_sid, string authToken);
    }
    public class TwilioService : ITwilioService
    {
        public async Task<string> getMessageService(string account_sid, string authToken, string type)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{account_sid}:{authToken}")));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string apiUrl = "https://messaging.twilio.com/v1/Services/";
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    string friendly_name = "Promotion Message Service";
                    if(type == "Operation")
                    {
                        friendly_name = "Operation Message Service";
                    }
                    foreach(var item in responseData.services)
                    {
                        if(item.friendly_name == friendly_name)
                        {
                            return item.sid;
                        }
                    }
                    return "not found";
                    //return responseData.services.FirstOrDefault(x=>x.friendly_name == friendly_name).sid;
               
               
                }
                else
                {
                    return "not found";
                }
            }
            catch
            {
                return "not found";
            }
        }

        public async Task<string> createMessageService(string friendlyname, string account_sid, string authToken)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://messaging.twilio.com/v1/Services");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{account_sid}:{authToken}")));
                var collection = new List<KeyValuePair<string, string>>();
                collection.Add(new("FriendlyName", friendlyname));
                var content = new FormUrlEncodedContent(collection);
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
           
                return responseData.sid;
            }
            catch
            {
                return "not found";
            }
        }

        public async Task<string> getPhoneMessageService(string account_sid, string authToken, string messageservice, string PhoneNumber)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{account_sid}:{authToken}")));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string apiUrl = $"https://messaging.twilio.com/v1/Services/{messageservice}/PhoneNumbers";
                var response = await httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        string phoneNumber = "not found";
                        foreach (var item in responseData.phone_numbers)
                        {
                            if (item.phone_number == PhoneNumber)
                            {
                                phoneNumber = item.phone_number;
                            }
                            else
                            {
                                string ServiceSid = item.service_sid;
                                string Sid = item.sid;
                                await deletePhoneMessageService(account_sid, authToken, ServiceSid, Sid);
                            }
                        }
                        return phoneNumber;
                    }
                    else
                    {
                        return "not found";
                    }
            }
            catch
            {
                return "not found";
            }
        }


        public async Task deletePhoneMessageService(string account_sid, string authToken, string ServiceSid, string Sid)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{account_sid}:{authToken}")));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                string apiUrl = $"https://messaging.twilio.com/v1/Services/{ServiceSid}/PhoneNumbers/{Sid}";
                var response = await httpClient.DeleteAsync(apiUrl);
            }
            catch
            {
            }
        }


        public async Task<string> createPhoneMessageService(string messageservice, string phonesid, string account_sid, string authToken)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"https://messaging.twilio.com/v1/Services/{messageservice}/PhoneNumbers");
                request.Headers.Add("Authorization", "Basic "+Convert.ToBase64String(Encoding.ASCII.GetBytes($"{account_sid}:{authToken}")));
                var collection = new List<KeyValuePair<string, string>>();
                collection.Add(new("PhoneNumberSid", phonesid));
                var content = new FormUrlEncodedContent(collection);
                request.Content = content;
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return responseData.phone_number;
            }
            catch
            {
                return "not found";
            }
        }
    }
}
