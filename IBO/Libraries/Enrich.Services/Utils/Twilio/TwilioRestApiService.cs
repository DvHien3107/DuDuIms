using Enrich.DataTransfer;
using Enrich.DataTransfer.Twilio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Base;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.AvailablePhoneNumberCountry;
using Twilio.Rest.Events.V1;
using Twilio.Rest.Messaging.V1.Service;

namespace Enrich.IServices.Utils
{
    public class TwilioRestApiService : ITwilioRestApiService
    {
        private readonly string accountSid = "";
        private readonly string authToken = "";
        public TwilioRestApiService()
        {
            accountSid = ConfigurationManager.AppSettings["AccountSID"];
            authToken = ConfigurationManager.AppSettings["AuthToken"];
        }
        public async Task<AccountResource> CreateSubAccount(string friendlyName)
        {
            try
            {
                TwilioClient.Init(accountSid, authToken);
                var account = await AccountResource.CreateAsync(friendlyName: friendlyName);
                return account;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TollfreeVerificationResponse> RequestTollfreeVerification(TollfreeVerificationRequest request, string accountSid, string authToken)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}")));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                string apiUrl = "https://messaging.twilio.com/v1/Tollfree/Verifications";


                List<KeyValuePair<string, string>> formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("BusinessStreetAddress", request.BusinessStreetAddress),
                    new KeyValuePair<string, string>("BusinessStreetAddress2", request.BusinessStreetAddress2),
                    new KeyValuePair<string, string>("BusinessCity", request.BusinessCity),
                    new KeyValuePair<string, string>("BusinessStateProvinceRegion", request.BusinessStateProvinceRegion),
                    new KeyValuePair<string, string>("BusinessPostalCode", request.BusinessPostalCode),
                    new KeyValuePair<string, string>("BusinessCountry", request.BusinessCountry),
                    new KeyValuePair<string, string>("BusinessContactFirstName", request.BusinessContactFirstName),
                    new KeyValuePair<string, string>("BusinessContactLastName", request.BusinessContactLastName),
                    new KeyValuePair<string, string>("BusinessContactEmail", request.BusinessContactEmail),
                    new KeyValuePair<string, string>("BusinessContactPhone", request.BusinessContactPhone),
                    new KeyValuePair<string, string>("AdditionalInformation", request.AdditionalInformation),
                    new KeyValuePair<string, string>("ExternalReferenceId", request.ExternalReferenceId),
                    new KeyValuePair<string, string>("BusinessName", request.BusinessName),
                    new KeyValuePair<string, string>("BusinessWebsite", request.BusinessWebsite),
                    new KeyValuePair<string, string>("NotificationEmail", request.NotificationEmail),
                    new KeyValuePair<string, string>("UseCaseSummary", request.UseCaseSummary),
                    new KeyValuePair<string, string>("ProductionMessageSample", request.ProductionMessageSample),
                    new KeyValuePair<string, string>("OptInType", request.OptInType),
                    new KeyValuePair<string, string>("MessageVolume", request.MessageVolume),
                    new KeyValuePair<string, string>("TollfreePhoneNumberSid", request.TollfreePhoneNumberSid)
                };
                
                if(request.UseCaseCategories!=null && request.UseCaseCategories.Count>0)
                foreach(var item in request.UseCaseCategories)
                {
                    formData.Add(new KeyValuePair<string, string>("UseCaseCategories", string.Join(",", item)));
                }

                if (request.OptInImageUrls != null && request.OptInImageUrls.Count > 0)
                foreach (var item in request.OptInImageUrls)
                {
                    formData.Add(new KeyValuePair<string, string>("OptInImageUrls", string.Join(",", item)));
                }

                HttpContent content = new FormUrlEncodedContent(formData);
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<TollfreeVerificationResponse>(responseContent);
                    return responseData;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<ExceptionRespone>(responseContent);
                    throw new Exception(responseData.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TollfreeVerificationResponse> RequestTollfreeVerificationWithCustomerProfile(TollfreeVerificationWithCustomerProfileSIdRequest request, string accountSid, string authToken)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}")));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


                string apiUrl = "https://messaging.twilio.com/v1/Tollfree/Verifications";

                List<KeyValuePair<string, string>> formData = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("CustomerProfileSid", request.CustomerProfileSid),
                    new KeyValuePair<string, string>("AdditionalInformation", request.AdditionalInformation),
                    new KeyValuePair<string, string>("ExternalReferenceId", request.ExternalReferenceId),
                    new KeyValuePair<string, string>("BusinessName", request.BusinessName),
                    new KeyValuePair<string, string>("BusinessWebsite", request.BusinessWebsite),
                    new KeyValuePair<string, string>("NotificationEmail", request.NotificationEmail),
                    new KeyValuePair<string, string>("UseCaseSummary", request.UseCaseSummary),
                    new KeyValuePair<string, string>("ProductionMessageSample", request.ProductionMessageSample),
                    new KeyValuePair<string, string>("OptInType", request.OptInType),
                    new KeyValuePair<string, string>("MessageVolume", request.MessageVolume),
                    new KeyValuePair<string, string>("TollfreePhoneNumberSid", request.TollfreePhoneNumberSid)
                };

                if (request.UseCaseCategories != null && request.UseCaseCategories.Count > 0)
                foreach (var item in request.UseCaseCategories)
                {
                    formData.Add(new KeyValuePair<string, string>("UseCaseCategories", string.Join(",", item)));
                }

                if (request.OptInImageUrls != null && request.OptInImageUrls.Count > 0)
                foreach (var item in request.OptInImageUrls)
                {
                    formData.Add(new KeyValuePair<string, string>("OptInImageUrls", string.Join(",", item)));
                }

                HttpContent content = new FormUrlEncodedContent(formData);

                HttpResponseMessage response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<TollfreeVerificationResponse>(responseContent);
                    return responseData;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<ExceptionRespone>(responseContent);
                    throw new Exception(responseData.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<TollfreeVerificationResponse> RetrieveTollFreeVerification(string tollfreeVerSid, string accountSid, string authToken)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}")));
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


                string apiUrl = "https://messaging.twilio.com/v1/Tollfree/Verifications";

               

                HttpResponseMessage response = await httpClient.GetAsync($"{apiUrl}/{tollfreeVerSid}");

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<TollfreeVerificationResponse>(responseContent);
                    return responseData;
                }
                else
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<ExceptionRespone>(responseContent);
                    throw new Exception(responseData.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<ResourceSet<TollFreeResource>> AvailablePhoneNumberTollFree(string accountSid, string authToken)
        {
            TwilioClient.Init(accountSid, authToken);
            var tollFree = await TollFreeResource.ReadAsync(pathCountryCode: "US", limit: 20);
            return tollFree;
        }

        public async Task<IncomingPhoneNumberResource> IncomingPhoneNumber(string phoneNumber, string accountSid, string authToken)
        {
            TwilioClient.Init(accountSid, authToken);

            var incomingPhoneNumber = IncomingPhoneNumberResource.Create(
                phoneNumber: new Twilio.Types.PhoneNumber(phoneNumber)
            );
            return incomingPhoneNumber;
        }

        public async Task SuspendedSubAccountAsync(string subAccountId)
        {
            try
            {
                TwilioClient.Init(accountSid, authToken);

                var account = AccountResource.Update(
                    status: AccountResource.StatusEnum.Suspended,
                    pathSid: subAccountId
                );
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
           
        }
        public async Task ClosedSubAccountAsync(string subAccountId)
        {
            try
            {
                TwilioClient.Init(accountSid, authToken);

                var account = AccountResource.Update(
                    status: AccountResource.StatusEnum.Closed,
                    pathSid: subAccountId
                );
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region twilio sink
        public async Task<SinkResource> CreateSinkAsync(string name,string accountSid,string authToken)
        {
            try
            {
                TwilioClient.Init(accountSid, authToken);

                var sinkConfiguration = new Dictionary<string, Object>()
                {
                    {"destination", "https://ims.enrichco.us/twiliomanage/HookStatus"},
                    {"method", "POST"},
                };

                var sink = SinkResource.Create(
                    description: name,
                    sinkConfiguration: sinkConfiguration,
                    sinkType: "webhook"
                );
                return sink;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<SubscriptionResource> CreateSubscriptionAsync(string sinkSid, string accountSid, string authToken)
        {
            try
            {
                TwilioClient.Init(accountSid, authToken);
                var types = new List<object>
                {
                    new Dictionary<string, Object>
                    {
                        {"type", "com.twilio.messaging.compliance.toll-free-verification.expired"},
                        {"schema_version", 1},
                    },
                    new Dictionary<string, Object>
                    {
                        {"type", "com.twilio.messaging.compliance.toll-free-verification.pending-review"},
                        {"schema_version", 1},
                    },
                    new Dictionary<string, Object>
                    {
                        {"type", "com.twilio.messaging.compliance.toll-free-verification.request-approved"},
                        {"schema_version", 1},
                    },
                    new Dictionary<string, Object>
                    {
                        {"type", "com.twilio.messaging.compliance.toll-free-verification.request-rejected"},
                        {"schema_version", 1},
                    },
                    new Dictionary<string, Object>
                    {
                        {"type", "com.twilio.messaging.compliance.toll-free-verification.requested"},
                        {"schema_version", 1},
                    }
                };

                var subscription = SubscriptionResource.Create(
                    description: "Toll-Free Verification Subscription",
                    types: types,
                    sinkSid: sinkSid
                );

                return subscription;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }


		#endregion

		#region Messagsing service
		public async Task<Twilio.Rest.Messaging.V1.ServiceResource> CreateServiceAsync(string friendly_name, string accountSid, string authToken)
		{
			TwilioClient.Init(accountSid, authToken);
			var service = Twilio.Rest.Messaging.V1.ServiceResource.Create(friendlyName: friendly_name);
			return service;
		}
		public async Task<PhoneNumberResource> AddPhoneServiceAsync(string phoneNumberSid, string pathServiceSid, string accountSid, string authToken)
        {
			TwilioClient.Init(accountSid, authToken);
			var phoneNumber = PhoneNumberResource.Create(
		        phoneNumberSid: phoneNumberSid,
		        pathServiceSid: pathServiceSid
			);
			return phoneNumber;
		}
		public async Task DeletePhoneServiceAsync(string pathSid, string pathServiceSid, string accountSid, string authToken)
		{
			TwilioClient.Init(accountSid, authToken);
			PhoneNumberResource.Delete(
			pathServiceSid: pathServiceSid,
			pathSid: pathSid
			);
		}

		public async Task<List<PhoneNumberResource>> ReadPhoneServiceAsync(string pathServiceSid, string accountSid, string authToken)
		{
			TwilioClient.Init(accountSid, authToken);
			var phoneNumbers = PhoneNumberResource.Read(
	            pathServiceSid: pathServiceSid,
	            limit: 20
            );
            return phoneNumbers.ToList();
		}
		
		#endregion
	}
}
