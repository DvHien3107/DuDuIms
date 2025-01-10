using Enrich.Core.Utils;
using Enrich.IMS.Dto.Sms;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Enrich.Infrastructure.SMS
{
    public partial class EnrichSms : IEnrichSms
    {
        private readonly string _accountSID;
        private readonly string _authToken;
        private readonly string _phoneNumber;

        public EnrichSms(IConfiguration appConfig)
        {
            _accountSID = appConfig["Services:SMSConfig:Twilio:AccountSID"];
            _authToken = appConfig["Services:SMSConfig:Twilio:AuthToken"];
            _phoneNumber = appConfig["Services:SMSConfig:Twilio:PhoneNumber"];
            if (!string.IsNullOrEmpty(_accountSID) && !string.IsNullOrEmpty(_authToken))
            {
                TwilioClient.Init(_accountSID, _authToken);
            }
        }

        public async Task<MessageResource> SendAsync(SendSmsRequest request)
        {
            return await MessageResource.CreateAsync(
               from: new PhoneNumber(_phoneNumber),
               to: new PhoneNumber(CleanPhone(request.Receiver)),
               body: request.Text,
               mediaUrl: request.MediaUrl );
        }
        private string CleanPhone(string phone = "")
        {
            try
            {
                if (phone[0] == '+') return phone;
                Regex digitsOnly = new Regex(@"[^\d]");
                string phoneText = digitsOnly.Replace(phone, "");
                if (phoneText[0] == '0')
                    return "+84" + phoneText.Substring(1);
                else
                    return "+1" + phoneText;
            }
            catch (Exception)
            {
                return "";
            }

        }
    }
}
