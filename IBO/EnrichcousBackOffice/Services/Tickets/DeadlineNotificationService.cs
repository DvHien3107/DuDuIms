
using Enrich.Core.Infrastructure;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.ViewControler;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.Services.Tickets
{
    public class DeadlineNotificationService
    {
        //public void DeleteJob(string JobId)
        //{
        //    BackgroundJob.Delete(JobId);
        //}
        //public async Task ExecuteJob(long TicketId)
        //{
        //    WebDataModel db = new WebDataModel();
        //    var notificationServices = new NotificationService();
        //    var listMemberNoti = new List<string>();
        //    var tic = db.T_SupportTicket.Find(TicketId);
        //    var allStageTicket = db.T_TicketStage_Status.Where(x => x.TicketId == TicketId && x.Active == true).ToList();
        //    foreach(var item in allStageTicket)
        //    {
        //        if (!string.IsNullOrEmpty(item.AssignedMember_Numbers))
        //        {
        //            listMemberNoti.AddRange(item.AssignedMember_Numbers.Split('|'));
        //        }
        //        listMemberNoti = listMemberNoti.GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();
        //    }
          

         
        //    if (listMemberNoti.Count > 0)
        //    {
        //        notificationServices.TicketNewDeadlineNotification(listMemberNoti, tic.Id.ToString(), tic.Name,"System","");
        //        var subject = "[Enrich IMS] Missed Deadline: " + tic.Name + " | " + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
        //        #region body email
        //        string bodyE = "Dear, <br/>";
        //        string bTag = "";
        //        string url_detail = "/ticket_new/detail/";
        //        string link = ConfigurationManager.AppSettings["IMSUrl"] + url_detail + tic.Id.ToString();
          
        //        var list_tags = db.T_Tags.OrderByDescending(t => t.Id).ToList();
        //        if (!string.IsNullOrEmpty(tic.Tags))
        //        {
        //            foreach (var item in tic.Tags.Split('|'))
        //            {
        //                if (!string.IsNullOrEmpty(item))
        //                {
        //                    var tag = list_tags.Where(t => t.Name == item.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0]).FirstOrDefault();
        //                    bTag += "<span style='background-color: " + tag?.Color + ";'>" + tag?.Name + "</span> ";
        //                }
        //            }
        //        }

        //        bodyE += "Missed Deadline: " + tic.Name + " | Ticket #" + CommonFunc.view_TicketId(tic.Id) + "<br/><br/>" +
        //           "<strong>Ticket name:</strong><br/>" + tic.Name + "<br/><br/>" +
        //           "<strong>Description:</strong><br/>" + tic.Description +
        //           (!string.IsNullOrEmpty(bTag) ? "<strong>Label:</strong><br/>" + bTag + "<br/>" : "") +
        //           "<br/><table  class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
        //              "<tr><td class='w120' style='background-color:#f1eeee'>Stage </td><Td>" + string.Join(", ", allStageTicket.Select(x => x.StageName)) + "</td></tr>" +
        //           "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) ? "Open" : tic.StatusName) + "</td></tr>" +
        //           "<tr><td class='w120' style='background-color:#f1eeee'>Create</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "</td></tr>" +
        //           "<tr><td class='w120' style='background-color:#f1eeee'>Latest update</td><Td>" + GetLastestUpdate(tic.UpdateTicketHistory, '|') + "</td></tr>" +
        //           "</table>";
        //        bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>#" + CommonFunc.view_TicketId(tic.Id) + "</a>";
        //        var members = db.P_Member.Where(m =>m.Active==true && listMemberNoti.Contains(m.MemberNumber) == true);
        //        string to = "";
        //        string toMemNo = "";
        //        string firstname = "";
        //        string cc = "";
        //        foreach (var item in members)
        //        {
        //            to += item.PersonalEmail + ";";
        //            firstname += item.FirstName + ";";
        //            toMemNo += item.MemberNumber + ",";
        //        }
        //        var _mailingService = EngineContext.Current.Resolve<IMailingService>();
        //        string result = await _mailingService.SendBySendGrid(to, firstname, subject, bodyE, cc);
        //        NoticeViewService.SpecifyNotice(subject, tic.Name, url_detail + tic.Id, toMemNo);
        //        NoticeViewService.WebPushNotice(subject, tic.Name, url_detail + tic.Id, toMemNo);
        //        #endregion
        //    }
        //}

        public string GetLastestUpdate(string data, char separator = '|')
        {
            if (!string.IsNullOrWhiteSpace(data))
            {
                var history = data.Split(new char[] { separator }).Reverse();
                if (string.IsNullOrWhiteSpace(history.ElementAt(0)) == true)
                {
                    return history.ElementAt(1);
                }
                else
                {
                    return history.ElementAt(0);
                }

            }
            return string.Empty;
        }

        //public void CreateJob(T_SupportTicket ticket)
        //{
        //    P_Member cMem = AppLB.Authority.GetCurrentMember();
        //    string cronExp = ticket.Deadline.Value.TimeOfDay.Minutes + " " + ticket.Deadline.Value.TimeOfDay.Hours + " " + ticket.Deadline.Value.Date.Day + " " + ticket.Deadline.Value.Month + " *";
        //    RecurringJob.AddOrUpdate(ticket.DeadlineHangfireJobId,() => this.ExecuteJob(ticket.Id), cronExp, TimeZoneInfo.Utc);
        // }
    }
}