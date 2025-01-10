using Notification.SMS.IMS.Service;
using Notification.SMS.IMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Text.RegularExpressions;

namespace Notification.SMS.IMS
{
    public static class NotificationSMS
    {
    
        public static async Task<ResNotification> Create(List<NotificationSMSModel> notis)
        {
            try
            {
                ResNotification resNoti = new ResNotification();
                NotificationService _notificationService = new NotificationService();
                string msg = string.Empty;
                foreach (var noti in notis)
                {
                    try
                    {
                        noti.PhoneNumber = CleanPhone(noti.PhoneNumber);
                        await _notificationService.SendMessageAsync(noti.PhoneNumber, noti.Message, noti.MediaUrl);
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

        private static string CleanPhone(string phone)
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
