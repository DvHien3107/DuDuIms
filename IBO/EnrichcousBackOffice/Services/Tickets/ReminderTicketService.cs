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
    public class ReminderTicketService
    {
        public void DeleteJob(string JobId)
        {
            BackgroundJob.Delete(JobId);
        }

        public  string GetLastestUpdate(string data, char separator = '|')
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

        public async Task ExecuteJob(long TicketId,long TaskId,string CreateByName,string MemberNumber,string HangfireJobId)
        {
             WebDataModel db = new WebDataModel();
            var notificationServices = new NotificationService();
            var listMemberNoti = new List<string>();
            var tic = db.T_SupportTicket.Find(TicketId);
         
            var reminderTicket = db.T_RemindersTicket.Where(x=>x.TicketId==TicketId && x.TaskId == TaskId).FirstOrDefault();
            if (reminderTicket == null)
            {
                this.DeleteJob(HangfireJobId);
                return;
            }
            var task = db.Ts_Task.Where(x => x.Id == TaskId).FirstOrDefault();
            if(task == null)
            {
                this.DeleteJob(HangfireJobId);
                return;
            }
            var allStageTicket = db.T_TicketStage_Status.Where(x => x.TicketId == TicketId && x.Active == true).ToList();
            //foreach(var item in allStageTicket)
            //{
                if (!string.IsNullOrEmpty(task.AssignedToMemberNumber))
                {
                    listMemberNoti.AddRange(task.AssignedToMemberNumber.Split(','));
                }
           // }
            listMemberNoti = listMemberNoti.GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();
           
            if  (listMemberNoti.Count > 0)
            {
                notificationServices.TicketReminderNotification(listMemberNoti, TicketId.ToString(), CreateByName, MemberNumber);
                //send email reminder
                var subject = "[Enrich IMS] Reminder: "+ task.Name+ " | " + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
                #region body email
                string bodyE = "Dear, <br/>";
                string bTag = "";
                string url_detail = "/ticket_new/detail/";
                string link = ConfigurationManager.AppSettings["IMSUrl"] + url_detail + tic.Id.ToString();
                //var stages = string.Join("", (from s in db.T_TicketStage_Status
                //                              where s.TicketId == tic.Id
                //                              //join st in db.T_TicketStatus on tic.StatusId equals st.Id into _st
                //                              //from st in _st.DefaultIfEmpty()
                //                              select ("-" + s.StageName + ": " + (tic.T_TicketStatusMapping.Count() > 0 != null ? string.Join(",", tic.T_TicketStatusMapping) : "Pending") + "<br/>")).ToList());
                var list_tags = db.T_Tags.OrderByDescending(t => t.Id).ToList();
                if (!string.IsNullOrEmpty(tic.Tags))
                {
                    foreach (var item in tic.Tags.Split('|'))
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            var tag = list_tags.Where(t => t.Name == item.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries)[0]).FirstOrDefault();
                            bTag += "<span style='background-color: " + tag?.Color + ";'>" + tag?.Name + "</span> ";
                        }
                    }
                }

                bodyE += "Reminder: "+ task .Name+ " | Ticket #" + CommonFunc.view_TicketId(tic.Id) + "<br/><br/>" +
                   "<strong>Ticket name:</strong><br/>" + tic.Name + "<br/><br/>" +
                   "<strong>Description:</strong><br/>" + tic.Description +
                   (!string.IsNullOrEmpty(bTag) ? "<strong>Label:</strong><br/>" + bTag + "<br/>" : "") +
                   "<br/><table  class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                      "<tr><td class='w120' style='background-color:#f1eeee'>Stage </td><Td>" + string.Join(", ", allStageTicket.Select(x => x.StageName)) + "</td></tr>" +
                   "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) ? "Open" : tic.StatusName) + "</td></tr>" +
                   "<tr><td class='w120' style='background-color:#f1eeee'>Create</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "</td></tr>" +
                   "<tr><td class='w120' style='background-color:#f1eeee'>Latest update</td><Td>" + GetLastestUpdate(tic.UpdateTicketHistory, '|') + "</td></tr>" +
                   "</table>" +
                    "<br/><br/><strong>Reminder Note:</strong><br/>" + reminderTicket.Note;

                bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>#" + CommonFunc.view_TicketId(tic.Id) + "</a>";
                var members = db.P_Member.Where(m => m.Active == true && listMemberNoti.Contains(m.MemberNumber) == true);
                string to = "";
                string toMemNo = "";
                string firstname = "";
                string cc = "";
                foreach (var item in members)
                {
                    to += item.PersonalEmail + ";";
                    firstname += item.FirstName + ";";
                    toMemNo += item.MemberNumber + ",";
                }
                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                string result = await _mailingService.SendBySendGrid(to, firstname, subject, bodyE, cc);
                NoticeViewService.SpecifyNotice(subject, tic.Name, url_detail + tic.Id, toMemNo);
                NoticeViewService.WebPushNotice(subject, tic.Name, url_detail + tic.Id, toMemNo);
                #endregion
                db.SaveChanges();
            }

        }
        public void CreateJob(T_RemindersTicket model)
        {
            P_Member cMem = AppLB.Authority.GetCurrentMember();
            if (model.Repeat == RepeatDefine.Never)
            {
                string cronExp = model.Time.Value.Minutes + " " + model.Time.Value.Hours +" " + model.Date.Value.Date.Day + " " + model.Date.Value.Month + " *";
                RecurringJob.AddOrUpdate(model.HangfireJobId, () => this.ExecuteJob(model.TicketId,model.TaskId.Value,model.UpdateBy??model.CreateBy, cMem.MemberNumber, model.HangfireJobId), cronExp,TimeZoneInfo.Utc);
            }
           else if (model.Repeat == RepeatDefine.Daily)
           {
                RecurringJob.AddOrUpdate(model.HangfireJobId, () => this.ExecuteJob(model.TicketId, model.TaskId.Value, model.UpdateBy ?? model.CreateBy, cMem.MemberNumber, model.HangfireJobId), Cron.Daily(model.Time.Value.Hours, model.Time.Value.Minutes), TimeZoneInfo.Utc);
           }
           else if (model.Repeat == RepeatDefine.Weekly)
           {
                RecurringJob.AddOrUpdate(model.HangfireJobId, () => this.ExecuteJob(model.TicketId, model.TaskId.Value, model.UpdateBy ?? model.CreateBy, cMem.MemberNumber, model.HangfireJobId), Cron.Weekly(model.Date.Value.DayOfWeek, model.Time.Value.Hours,model.Time.Value.Minutes), TimeZoneInfo.Utc);
           }
           else  if (model.Repeat == RepeatDefine.Monthly)
            {
                RecurringJob.AddOrUpdate(model.HangfireJobId, () => this.ExecuteJob(model.TicketId, model.TaskId.Value, model.UpdateBy ?? model.CreateBy, cMem.MemberNumber, model.HangfireJobId), Cron.Monthly(model.Date.Value.Day, model.Time.Value.Hours, model.Time.Value.Minutes), TimeZoneInfo.Utc);
            }
        }

    }
}