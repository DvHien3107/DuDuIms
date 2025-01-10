using Enrich.Core.UnitOfWork.Data;
using Enrich.DataTransfer;
using Enrich.Entities;
using Enrich.IServices.Utils.SMS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio;
using Twilio.Base;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.Http;
using Newtonsoft.Json;
using System.Net;

namespace Enrich.Services.Utils.SMS
{
    public class SMSService : ISMSService
    {
        private readonly string _accountSID;
        private readonly string _authToken_;
        private readonly string _phoneNumber;
        private readonly string _token;

        private readonly IUnitOfWork _unitOfWork;
        public SMSService(IUnitOfWork unitOfWork)
        {
            _accountSID = ConfigurationManager.AppSettings["AccountSID"];
            _authToken_ = ConfigurationManager.AppSettings["AuthToken"];
            _phoneNumber = ConfigurationManager.AppSettings["PhoneNumber"];
            _token = ConfigurationManager.AppSettings.Get("Textline_Access_Token");
            _unitOfWork = unitOfWork;
            if (!string.IsNullOrEmpty(_accountSID) && !string.IsNullOrEmpty(_authToken_))
            {
                TwilioClient.Init(_accountSID, _authToken_);
            }
        }
        public async Task<ResNotification> Create(List<NotificationSMSModel> notis)
        {
            try
            {
                ResNotification resNoti = new ResNotification();

                string msg = string.Empty;
                foreach (var noti in notis)
                {
                    try
                    {
                        noti.PhoneNumber = CleanPhone(noti.PhoneNumber);
                        await SendMessageAsync(noti.PhoneNumber, noti.Message, noti.MediaUrl);
                        resNoti.TotalSuccess++;

                    }
                    catch (Exception e)
                    {
                        resNoti.TotalFailed++;
                        resNoti.Message += noti.PhoneNumber + "|" + e.Message + "$";
                    }
                }
                return resNoti;
            }
            catch (Exception ex)
            {
                var resNoti = new ResNotification { Message = ex.Message };
                return resNoti;
            }
        }

        /// <summary>
        /// Send sms by textline
        /// </summary>
        /// <param name="to_phone"></param>
        /// <param name="country"></param>
        /// <param name="sms"></param>
        /// <returns></returns>
        public async Task<string> SendSMSTextline(string to_phone, string country, string sms)
        {
            try
            {
                //var result = await Get_Access_Token_Textline();

                country = country?.Replace(" ", "").ToLower();
                var list_country = _unitOfWork.Repository<Ad_Country>().TableNoTracking.ToList();
                var phone_code = list_country.Where(x => x.Name.ToLower() == country || x.CountryCode.Equals(country, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.PhoneCode;
                if (string.IsNullOrEmpty(phone_code) == true)
                {
                    phone_code = "1";
                }

                to_phone = Regex.Replace(to_phone, "[ ()-]", "");
                if (to_phone.IndexOf(phone_code, 0) != 0 && to_phone.IndexOf("+" + phone_code, 0) != 0)
                {
                    if (to_phone.Substring(0, 1) == "0")
                    {
                        to_phone = to_phone.Remove(0, 1);
                    }
                    to_phone = phone_code + to_phone;
                }


                string token = _token;
                using (var httpClient = new System.Net.Http.HttpClient { BaseAddress = new Uri("https://application.textline.com/") })
                {
                    //httpClient.DefaultRequestHeaders.Add("X-TGP-ACCESS-TOKEN", token);

                    using (var content = new StringContent("{\"phone_number\": \"" + to_phone + "\",  \"group_uuid\": \"<1st Department UUID>\",  \"comment\": {    \"body\": \"" + sms + "\"  },  \"attachments\": []}", System.Text.Encoding.Default, "application/json"))
                    {
                        content.Headers.Add("X-TGP-ACCESS-TOKEN", token);
                        //{?after_uuid,before_uuid,group_uuid,page,page_size,phone_number}
                        using (var response = await httpClient.PostAsync("api/conversations.json", content))
                        {
                            string responseData = await response.Content.ReadAsStringAsync();
                            if (response.StatusCode == System.Net.HttpStatusCode.OK || response.StatusCode == System.Net.HttpStatusCode.Accepted || response.StatusCode == System.Net.HttpStatusCode.Created)
                            {
                                return "";
                            }
                            else
                            {
                                throw new Exception(responseData);
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                return string.IsNullOrWhiteSpace(ex.InnerException?.Message) ? ex.Message : ex.InnerException.Message;
            }

        }

        public async Task<string> NotifyPaymentLink(string phone, string name, string url)
        {
            try
            {
                var phones = phone?.Split(',');
                var names = name?.Split(',');
                var notis = new List<NotificationSMSModel>();
                for (int i = 0; i < phones.Length; i++)
                {
                    var message = $"Hello {names[i]},\n" +
                              $"Below is the link to your invoice for payment. An email was also sent for your convenience.\n" +
                              $"- Simply Pos Team.\n" +
                              $"{url}";

                    notis.Add(new NotificationSMSModel
                    {
                        PhoneNumber = phones[i],
                        Message = message
                    });
                }

                var msg = await Create(notis);
                if (!string.IsNullOrEmpty(msg.Message) && msg.TotalFailed == 0)
                    return msg.Message;
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private string CleanPhone(string phone)
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

        public async Task<MessageResource> SendMessageAsync(string to, string body, List<Uri> mediaUrl)
        {
            return await MessageResource.CreateAsync(
                from: new PhoneNumber(_phoneNumber),
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

                var messages = MessageResource.Read(filter);

                return messages.ToList();
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public async Task<Page<MessageResource>> GetMessage(DateTime? FromDate = null, DateTime? ToDate = null, string FromPhone = "", string ToPhone = "", string Body = "", int? NumSegments = null)
        {
            try
            {

                var request = new Request(Twilio.Http.HttpMethod.Get, "https://api.twilio.com/2010-04-01/Accounts/AC2b45e7a290ed4604d633fcb6d8a5ad04/Messages.json");
                request.AddQueryParam("PageSize", "1000");
                if (ToDate.Value.Date == FromDate.Value.Date)
                {
                    // request.AddQueryParam("DateSentBefore", ToDate.Value.ToString("yyyy-MM-dd"));
                    request.AddQueryParam("DateSent", FromDate.Value.ToString("yyyy-MM-dd"));
                }
                else
                {
                    request.AddQueryParam("DateSent>", FromDate.Value.ToString("yyyy-MM-dd"));
                    request.AddQueryParam("DateSent<", ToDate.Value.ToString("yyyy-MM-dd"));
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
                var request = new Request(Twilio.Http.HttpMethod.Get, Url);
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
