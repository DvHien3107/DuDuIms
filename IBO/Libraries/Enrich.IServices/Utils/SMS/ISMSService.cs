using Enrich.DataTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio.Base;
using Twilio.Rest.Api.V2010.Account;

namespace Enrich.IServices.Utils.SMS
{
    public interface ISMSService
    {
        Task<ResNotification> Create(List<NotificationSMSModel> notis);

        Task<MessageResource> SendMessageAsync(string to, string body, List<Uri> mediaUrl);

        Task<string> SendSMSTextline(string to_phone, string country, string sms);

        Task<string> NotifyPaymentLink(string phone, string name, string url);

        List<MessageResource> GetAllMessage(DateTime? FromDate = null, DateTime? ToDate = null, string FromPhone = "", string ToPhone = "", string Body = "", int? NumSegments = null);
        Task<Page<MessageResource>> GetMessage(DateTime? FromDate = null, DateTime? ToDate = null, string FromPhone = "", string ToPhone = "", string Body = "", int? NumSegments = null);
        Task<Page<MessageResource>> GetMessage(string Url);

    }
}
