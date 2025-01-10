using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio;
using Twilio.Base;
using Twilio.Http;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Notification.SMS.IMS.Service
{
   public class NotificationService
    {
        private string AccountSID = ConfigurationManager.AppSettings["AccountSID"];
        private string AuthToken = ConfigurationManager.AppSettings["AuthToken"];
        private string PhoneNumber = ConfigurationManager.AppSettings["PhoneNumber"];
        public NotificationService()
        {
            if (!string.IsNullOrEmpty(AccountSID) && !string.IsNullOrEmpty(AuthToken))
            {
                TwilioClient.Init(AccountSID, AuthToken);
            }
        }

        public async Task<MessageResource> SendMessageAsync(string to, string body, List<Uri> mediaUrl)
        {
            return await MessageResource.CreateAsync(
                from: new PhoneNumber(PhoneNumber),
                to: new PhoneNumber(to),
                body: body,
                mediaUrl: mediaUrl
                );
        }
        public List<MessageResource> GetAllMessage(DateTime? FromDate = null, DateTime? ToDate = null, string FromPhone = "", string ToPhone = "", string Body = "", int? NumSegments = null)
        {
            try
            {
                var filter = new ReadMessageOptions();
                if (ToDate.Value.Date == FromDate.Value.Date)
                {
                    // request.AddQueryParam("DateSentBefore", ToDate.Value.ToString("yyyy-MM-dd"));
                    filter.DateSent = ToDate.Value.Date;
                }
                else
                {
                    filter.DateSentAfter = FromDate.Value.Date;
                    filter.DateSentBefore = ToDate.Value.Date;
                }
                if (!string.IsNullOrEmpty(FromPhone))
                {
                    filter.From = FromPhone;
                }

                if (!string.IsNullOrEmpty(ToPhone))
                {
                    filter.To = ToPhone;
                }

                var messages =  MessageResource.Read(filter);

                return messages.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public async Task<Page<MessageResource>> GetMessage(DateTime? FromDate=null,DateTime? ToDate=null,string FromPhone = "", string ToPhone = "", string Body="",int? NumSegments=null)
        {
            try
            {
            
                var request = new Request(HttpMethod.Get, $"https://api.twilio.com/2010-04-01/Accounts/{AccountSID}/Messages.json");
                request.AddQueryParam("PageSize", "1000");
                if (ToDate.Value.Date == FromDate.Value.Date)
                {
                    // request.AddQueryParam("DateSentBefore", ToDate.Value.ToString("yyyy-MM-dd"));
                    request.AddQueryParam("DateSent", FromDate.Value.ToString("yyyy-MM-dd"));
                }
                else
                {
                    request.AddQueryParam("DateSent>", FromDate.Value.ToString("yyyy-MM-dd"));
                    request.AddQueryParam("DateSent<",  ToDate.Value.ToString("yyyy-MM-dd"));
                }
             
                
                if (!string.IsNullOrEmpty(FromPhone))
                {
                    request.AddQueryParam("From", FromPhone);
                    
                }

                if (!string.IsNullOrEmpty(ToPhone))
                {
                    request.AddQueryParam("To", ToPhone);
                    
                }
                var response = await TwilioClient.GetRestClient().RequestAsync(request);
                var messages = Page<MessageResource>.FromJson("messages", response.Content);
                return messages;
            }
            catch (Exception ex)
            {
                return null;
            }
           
        }
        public async Task<Page<MessageResource>> GetMessage(string Url)
        {
            try
            {
                var request = new Request(HttpMethod.Get, Url);
                var response = await TwilioClient.GetRestClient().RequestAsync(request);
                var messages = Page<MessageResource>.FromJson("messages", response.Content);
                return messages;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
