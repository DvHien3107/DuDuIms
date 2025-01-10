using Enrich.DataTransfer.Twilio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Base;
using Twilio.Rest.Api.V2010;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Api.V2010.Account.AvailablePhoneNumberCountry;
using Twilio.Rest.Events.V1;
using Twilio.Rest.Messaging.V1.Service;
using Twilio.Rest.Messaging.V1;
namespace Enrich.IServices.Utils
{
    public interface ITwilioRestApiService
    {
        /// <summary>
        /// Create subaccount
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <returns></returns>
        Task<AccountResource> CreateSubAccount(string friendlyName);

        /// <summary>
        /// Create toll-free verification
        /// </summary>
        /// <param name="request"></param>
        /// <param name="accountSid"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        Task<TollfreeVerificationResponse> RequestTollfreeVerification(TollfreeVerificationRequest request, string accountSid, string authToken);
        Task<TollfreeVerificationResponse> RequestTollfreeVerificationWithCustomerProfile(TollfreeVerificationWithCustomerProfileSIdRequest request, string accountSid, string authToken);
        Task<TollfreeVerificationResponse> RetrieveTollFreeVerification(string tollfreeVerSid, string accountSid, string authToken);
        Task<ResourceSet<TollFreeResource>> AvailablePhoneNumberTollFree(string accountSid, string authToken);
        Task<IncomingPhoneNumberResource> IncomingPhoneNumber(string phoneNumber, string accountSid, string authToken);

        Task SuspendedSubAccountAsync(string subAccountId);
        Task ClosedSubAccountAsync(string subAccountId);

        Task<SinkResource> CreateSinkAsync(string name, string accountSid, string authToken);
        Task<SubscriptionResource> CreateSubscriptionAsync(string sinkSid, string accountSid, string authToken);

        #region Messaging service
        Task<ServiceResource> CreateServiceAsync(string friendly_name, string accountSid, string authToken);
		Task<PhoneNumberResource> AddPhoneServiceAsync(string phoneNumberSid, string pathServiceSid, string accountSid, string authToken);
		Task DeletePhoneServiceAsync(string pathSid, string pathServiceSid, string accountSid, string authToken);
		Task<List<PhoneNumberResource>> ReadPhoneServiceAsync(string pathServiceSid, string accountSid, string authToken);
		#endregion

	}
}
