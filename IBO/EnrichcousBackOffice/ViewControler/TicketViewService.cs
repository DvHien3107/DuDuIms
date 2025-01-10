using EnrichcousBackOffice.API;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel.Ticket;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using Enrich.Core.Infrastructure;
using Enrich.IServices.Utils.Mailing;
using NPOI.SS.Formula.Functions;
using System.Linq.Dynamic;
using Enrich.IServices.Utils;
using EnrichcousBackOffice.Services;

namespace EnrichcousBackOffice.ViewControler
{
    public class TicketViewService
    {

        /// <summary>
        /// get list members cung group
        /// </summary>
        /// <param name="db"></param>
        /// <param name="groupId"></param>
        /// <param name="membernumber"></param>
        /// <returns></returns>
        public static List<string> GetMembersInTheSameGroup(WebDataModel db, string membernumber)
        {
            //var db = new WebDataModel();
            var listGroupId = new List<long>();
            var group = string.Join(",", db.P_Department.Where(d => d.ParentDepartmentId > 0 && d.GroupMemberNumber.Contains(membernumber) == true).Select(d => d.GroupMemberNumber).ToList());
            group = group.Replace(",,", ",");
            group = Regex.Replace(group, "," + membernumber, "");
            group = Regex.Replace(group, membernumber + ",", "");
            return group.Split(new char[] { ',' }).ToList();
        }

        /// <summary>
        /// get list group id ma member thuoc ve.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="groupId"></param>
        /// <param name="membernumber"></param>
        /// <returns></returns>
        public static List<long> GetGroupByMember(WebDataModel db, string membernumber)
        {
            //var db = new WebDataModel();
            var listGroupIdss = new List<long>();
            // var listGroupId = db.P_Department.Where(d =>d.GroupMemberNumber.Contains(membernumber)||d.LeaderNumber.Contains(membernumber)).Select(x=>x.Id).ToList();
            var listGroupIds = (from d in db.P_Department
                                where d.GroupMemberNumber.Contains(membernumber) || d.LeaderNumber.Contains(membernumber)
                                select new
                                {
                                    d.Id,
                                    child = (from c in db.P_Department
                                             where c.ParentDepartmentId == d.Id
                                             select new
                                             {
                                                 c.Id,
                                             }).ToList()
                                }).ToList();
            foreach (var item in listGroupIds)
            {
                listGroupIdss.Add(item.Id);
                if (item.child.Count() > 0)
                {
                    listGroupIdss.AddRange(item.child.Select(x => x.Id).ToList());
                }
            }
            listGroupIdss = listGroupIdss.GroupBy(x => x).Select(x => x.First()).ToList();
            return listGroupIdss;
        }

        /// <summary>
        /// get thong tin phan tu cuoi cung cua mang
        /// </summary>
        /// <param name="data">chuoi co cac phan tu cach nhau boi dau "|"</param>
        /// <returns></returns>
        public static string GetLastestUpdate(string data, char separator = '|')
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



        /// <summary>
        /// add new feedback
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ticketId"></param>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="statusName"></param>
        /// <returns>Feedback Id</returns>
        public static DateTime InsertFeedback(WebDataModel db, long ticketId, string title, string content, string statusName, long statusId = -1, string updateBy = "")
        {
            try
            {
                if (db == null)
                {
                    db = new WebDataModel();
                }

                var cMem = Authority.GetCurrentMember();
                long id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                DateTime createAt = DateTime.UtcNow;
                var ticketFB = new T_TicketFeedback
                {
                    Id = id,
                    TicketId = ticketId,
                    FeedbackTitle = title,
                    Feedback = content,
                    //TicketStatusChanges = statusName,
                    CreateAt = createAt,
                    CreateByName = (string.IsNullOrWhiteSpace(updateBy) == true ? (cMem?.FullName) : updateBy),
                    CreateByNumber = (string.IsNullOrWhiteSpace(updateBy) == true ? (cMem?.MemberNumber) : ""),
                    DateCode = createAt.ToString("yyyyMMdd")
                };



                var tic = db.T_SupportTicket.Find(ticketId);
                //if (statusId > 0 && tic.StatusId != statusId)
                //{
                //    var status = db.T_TicketStatus.Find(statusId);
                //    if (status != null)
                //    {
                //        //update ticket
                //        ticketFB.TicketStatusChanges = status.Name;
                //        tic.StatusId = status.Id;
                //        tic.StatusName = status?.Name;
                //        if (status.Type == "closed")
                //        {
                //            tic.DateClosed = DateTime.UtcNow;
                //            tic.CloseByMemberNumber = (string.IsNullOrWhiteSpace(updateBy) == true ? (cMem?.MemberNumber) : "");
                //            tic.CloseByName = (string.IsNullOrWhiteSpace(updateBy) == true ? (cMem?.FullName) : updateBy);
                //        }

                //    }

                //}

                //add feedback
                db.T_TicketFeedback.Add(ticketFB);

                //update ticket
                tic.FeedbackTicketHistory += createAt.ToString("dd MMM,yyyy hh:mm tt") + " - by " + (string.IsNullOrWhiteSpace(updateBy) == true ? (cMem?.FullName) : updateBy) + "|";
                tic.UpdateTicketHistory += createAt.ToString("dd MMM,yyyy hh:mm tt") + " - by " + (string.IsNullOrWhiteSpace(updateBy) == true ? (cMem?.FullName) : updateBy) + "|";
                db.Entry(tic).State = System.Data.Entity.EntityState.Modified;


                db.SaveChanges();
                return createAt;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /// <summary>
        /// Auto assigment cho members phu hop, TH xac dinh duoc group
        /// Tam thoi stop tinh nang nay
        /// </summary>
        /// <param name="db"></param>
        /// <param name="tic"></param>
        /// <param name="type"></param>
        public static async Task<bool> AutoAssignment(WebDataModel db, T_SupportTicket tic, string type = "new", string memberNumberSubmit = "")
        {
            await SendNoticeAfterTicketUpdate(tic, type, db, memberNumberSubmit);
            return true;

        }

        /// <summary>
        /// Send email cho merchant if feedback publish
        /// </summary>
        /// <param name="type">new|update|feedback</param>
        /// <returns></returns>
        //public static async Task<string> SendNoticeAfterPublishAction(T_SupportTicket tic, T_TicketFeedback fb, WebDataModel db = null)
        //{
        //    try
        //    {
        //        if (db == null)
        //        {
        //            db = new WebDataModel();
        //        }
        //        var result = await SendNoticeToCustomer(db, tic, fb);
        //        return result;
        //    }
        //    catch (Exception e)
        //    {
        //        return e.Message;
        //    }

        //}


        /// <summary>
        /// Send email cho member lien quan sau khi ticket duoc update or feedback
        /// </summary>
        /// <param name="type">new|update|feedback</param>
        /// <returns></returns>
        public static async Task<string> SendNoticeAfterTicketUpdate(T_SupportTicket tic, string type = "update", WebDataModel db = null, string memberNumberSubmit = "", List<string> shared_leaders = null, List<string> excludeTicketUpdateNotice = null)
        {
            try
            {
                if (db == null)
                {
                    db = new WebDataModel();
                }
                List<string> memberNumberList = new List<string>();

                var fblist = db.T_TicketFeedback.Where(f => f.TicketId == tic.Id).OrderByDescending(f => f.Id);
                var fb = fblist.FirstOrDefault();
                string result = "";

                //Sent mail cho Customer
                if (type != "new" && type != "update" &&
                    tic.GlobalStatus?.Equals("publish", StringComparison.OrdinalIgnoreCase) == true &&
                    fb.GlobalStatus?.Equals("publish", StringComparison.OrdinalIgnoreCase) == true &&
                    string.IsNullOrWhiteSpace(tic.CustomerCode) == false)
                {
                    result = await SendNoticeToCustomer(db, tic, fb);
                }

                //Sent mail cho Member
                result = await SendNoticeToMember(tic, fb, type, db, memberNumberSubmit, shared_leaders,excludeTicketUpdateNotice);

                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        /// <summary>
        /// Thong bao khi ticket duoc reassinged/escalate
        /// </summary>
        /// <param name="type">reassigned/escalate</param>
        /// <param name="tic">ticket system</param>
        /// <returns></returns>
        public static async Task<string> SendNoticeAfterReAssigning(string type, T_SupportTicket tic, string memberNumberSubmit = "")
        {
            try
            {
                var result = await SendNoticeToMember(tic, null, type, new WebDataModel(), memberNumberSubmit);
                return result;

            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public static async Task<string> SendNoticeToCustomer(WebDataModel db, T_SupportTicket tic, T_TicketFeedback fb)
        {
            try
            {
                if (db == null)
                {
                    db = new WebDataModel();
                }
                var cus = db.C_Customer.Where(c => c.CustomerCode == tic.CustomerCode).FirstOrDefault();
                string bodyE = "Dear, <br/>";
                string subject = "";
                //feedback
                subject = "[Enrich IMS]" + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
                #region body email
                bodyE += "Ticket #" + CommonFunc.view_TicketId(tic.Id) + " just has new action" + "<br/><br/>" +
                  "<strong>Ticket Name: </strong><br/>" + tic.Name +
                  "<br/><br/><strong>Ticket action:</strong><br/>";
                bodyE += "<br/>" + fb.Feedback;
                bodyE += "<br/><strong>Ticket description:</strong><br/>" + tic.Description +

                "<br/><table  class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) == true ? "PENDING" : tic.StatusName.ToUpper()) + "</td></tr>";

                bodyE += "<tr><td class='w120'  style='background-color:#f1eeee'>Create</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "</td></tr></table>";
                #endregion
                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                await _mailingService.SendBySendGrid(cus?.Email, cus?.OwnerName, subject, bodyE, "");
                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public static async Task<string> SendNoticeToMember(T_SupportTicket tic, T_TicketFeedback fb, string type, WebDataModel db, string memberNumberSubmit = "", List<string> shared_leaders = null, List<string> excludeTicketUpdateNotice = null)
        {
            try
            {
                string bodyE = "Dear, <br/>";
                string subject = "";
                string to = "";
                string toMemNo = "";
                string firstname = "";
                string cc = "";
                string url_detail = "/ticket_new/detail/";
                //Development ticket co TypeId tu 2000 den 2999
                //if (tic.TypeId >= 2000 && tic.TypeId < 3000)
                //{
                //    url_detail = "/ticket_new/detail/";
                //}
                string link = ConfigurationManager.AppSettings["IMSUrl"] + url_detail + tic.Id.ToString();

                #region xac dinh member can send email thong bao
                List<string> memberNumberList = new List<string>();
                memberNumberList.Add(tic.CreateByNumber);
                //if (tic.TypeId >= 2000 && tic.TypeId < 3000)
                //{
                if (!string.IsNullOrEmpty(tic.AssignedToMemberNumber))
                {
                    memberNumberList.AddRange(tic.AssignedToMemberNumber.Split(','));
                }
                if (type != "new")
                {
                    var listFeedback = db.T_TicketFeedback.Where(x => x.TicketId == tic.Id).ToList();
                    if (listFeedback.Count() > 0)
                    {
                        foreach (var feedback in listFeedback)
                        {
                            if (!string.IsNullOrEmpty(feedback.MentionMemberNumbers))
                            {
                                memberNumberList.AddRange(feedback.MentionMemberNumbers.Split(','));
                            }
                            if (!string.IsNullOrEmpty(feedback.CreateByNumber))
                            {
                                memberNumberList.Add(feedback.CreateByNumber);
                            }

                        }
                    }
                }


                if (string.IsNullOrWhiteSpace(tic.ReassignedToMemberNumber) == false)
                {
                    memberNumberList.Add(tic.ReassignedToMemberNumber);
                }

                if (string.IsNullOrWhiteSpace(tic.SubscribeMemberNumber) == false)
                {
                    foreach (var item in tic.SubscribeMemberNumber.Split(new char[] { ',' }))
                    {
                        if (!string.IsNullOrWhiteSpace(item))
                        {
                            memberNumberList.Add(item);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(tic.TagMemberNumber))
                {
                    memberNumberList.AddRange(tic.TagMemberNumber.Split(',').Where(m => !memberNumberList.Any(y => y == m) && m != memberNumberSubmit));
                }
                #endregion
                memberNumberList = memberNumberList?.Distinct().ToList();
                //bo qua nguoi submit
                memberNumberList.Remove(memberNumberSubmit);
                shared_leaders = (shared_leaders ?? new List<string>()).Distinct().ToList();
                shared_leaders?.Remove(memberNumberSubmit);
                shared_leaders = shared_leaders.Where(s => !memberNumberList.Contains(s)).ToList();

                if (memberNumberList.Count == 0 && shared_leaders.Count == 0)
                {
                    return "";
                }
                List<string> NotiSendMemberNumber = new List<string>();
                NotiSendMemberNumber = memberNumberList;
                

                var list_tags = db.T_Tags.OrderByDescending(t => t.Id).ToList();
                string bTag = "";
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
                var stagesTicket = string.Join(", ", db.T_TicketStage_Status.Where(x => x.TicketId == tic.Id && x.Active == true).Select(x => x.StageName));

                if (type.Equals("new"))
                {
                    //new
                    subject = "[Enrich IMS]" + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
                    #region body email
                    bodyE += "You have a new ticket #" + CommonFunc.view_TicketId(tic.Id) + "<br/><br/>" +
                        "<strong>Ticket name:</strong><br/>" + tic.Name + "<br/><br/>" +
                        "<strong>Description:</strong><br/>" + tic.Description +
                        (!string.IsNullOrEmpty(bTag) ? "<strong>Label:</strong><br/>" + bTag + "<br/>" : "") +
                        "<br/><table  class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                           "<tr><td class='w120' style='background-color:#f1eeee'>Stages </td><Td>" + stagesTicket + "</td></tr>" +
                        "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) ? "Open" : tic.StatusName) + "</td></tr>" +
                        "<tr><td class='w120' style='background-color:#f1eeee'>Create</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "</td></tr>" +
                        "<tr><td class='w120' style='background-color:#f1eeee'>Latest update</td><Td>" + GetLastestUpdate(tic.UpdateTicketHistory, '|') + "</i></td></tr>" +
                        "</table>";

                    bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>#" + CommonFunc.view_TicketId(tic.Id) + "</a>";
                    var notificationService = new NotificationService();
                    P_Member cMem = AppLB.Authority.GetCurrentMember();

                    if (memberNumberList.Count > 0)
                    {
                        var noti = new P_Notification();
                        noti.Action = NotificationActionDefine.AddNew;
                        noti.Category = NotificationCategoryDefine.Ticket;
                        noti.EntityId = tic.Id.ToString();
                        noti.EntityName = tic.Name;
                        noti.MemberNumber = cMem?.MemberNumber ?? null;
                        noti.MemberName = cMem?.FullName ?? null;
                        noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketAdd).FirstOrDefault().Id;
                        noti.CreateAt = DateTime.UtcNow;
                        noti.CreateBy = cMem?.FullName ?? tic.CreateByName;
                        // var listMemberNoti = db.T_Project_Stage_Members.Where(x => forward_stages.Any(s => s.Contains(x.StageId))).Select(x => x.MemberNumber ).Distinct();
                        notificationService.Insert(noti, NotiSendMemberNumber);
                    }
                    #endregion

                }
                else if (type.Equals("update"))
                {
                    //update
                    subject = "[Enrich IMS]" + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
                    #region body email

                    bodyE += "Ticket #" + CommonFunc.view_TicketId(tic.Id) + " just had updated.<br/><br/>" +
                       "<strong>Ticket name:</strong><br/>" + tic.Name + "<br/><br/>" +
                       "<strong>Description:</strong><br/>" + tic.Description +
                       (!string.IsNullOrEmpty(bTag) ? "<strong>Label:</strong><br/>" + bTag + "<br/>" : "") +
                       "<br/><table  class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                       "<tr><td class='w120' style='background-color:#f1eeee'>Stages </td><Td>" + stagesTicket + "</td></tr>" +
                        "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) ? "Open" : tic.StatusName) + "</td></tr>" +
                       "<tr><td class='w120' style='background-color:#f1eeee'>Create</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "</td></tr>" +
                       "<tr><td class='w120' style='background-color:#f1eeee'>Latest update</td><Td>" + GetLastestUpdate(tic.UpdateTicketHistory, '|') + "</td></tr>" +
                       "</table>";
                    bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>#" + CommonFunc.view_TicketId(tic.Id) + "</a>";


                    #endregion
                    var notificationService = new NotificationService();
                    P_Member cMem = AppLB.Authority.GetCurrentMember();
                    if (memberNumberList.Count > 0)
                    {
                        int TemplateId;
                        int? LastUpdateId;
                        LastUpdateId = db.T_TicketUpdateLog.Where(x => x.TicketId == tic.Id).OrderByDescending(x => x.UpdateId).FirstOrDefault()?.UpdateId;
                        if (tic.DateClosed != null)
                        {
                            TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketClose).FirstOrDefault().Id;

                        }
                        else
                        {
                            TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketUpdate).FirstOrDefault().Id;

                        }
                        string EntityId = tic.Id.ToString();
                        var checkUpdateId = db.P_Notification.Any(x => x.TemplateId == TemplateId && x.EntityId == EntityId && x.UpdateId == LastUpdateId);
                   
                        if(excludeTicketUpdateNotice!=null&& excludeTicketUpdateNotice.Count > 0)
                        {
                            NotiSendMemberNumber = NotiSendMemberNumber.Where(x => !excludeTicketUpdateNotice.Any(y => y == x)).ToList();
                        }
                      
                        if (!checkUpdateId&& NotiSendMemberNumber.Count>0)
                        {
                            var noti = new P_Notification();
                            noti.Action = NotificationActionDefine.Update;
                            noti.Category = NotificationCategoryDefine.Ticket;
                            noti.EntityId = tic.Id.ToString();
                            noti.EntityName = tic.Name;
                            noti.MemberNumber = cMem?.MemberNumber ?? null;
                            noti.MemberName = cMem?.FullName ?? null;
                            noti.TemplateId = TemplateId;
                            noti.UpdateId = LastUpdateId;
                            noti.CreateAt = DateTime.UtcNow;
                            noti.CreateBy = cMem?.FullName ?? tic.CreateByName;
                            //var listMemberNoti = db.T_Project_Stage_Members.Where(x => forward_stages.Any(s => s.Contains(x.StageId))).Select(x => x.MemberNumber ).Distinct();
                            notificationService.Insert(noti, NotiSendMemberNumber);
                        }

                    }

                }
                else if (type.Equals("reassigned"))
                {
                    subject = "[Enrich IMS]" + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
                    #region body email
                    bodyE = "Ticket #" + CommonFunc.view_TicketId(tic.Id) + " just had reassigned to " + tic.ReassignedToMemberName + "<br/><br/>" +
                       "<strong>Ticket name:</strong><br/>" + tic.Name + "<br/><br/>" +
                       "<strong>Description:</strong><br/>" + tic.Description +
                       (!string.IsNullOrEmpty(bTag) ? "<strong>Label:</strong><br/>" + bTag + "<br/>" : "") +
                       "<br/><table class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                         "<tr><td class='w120' style='background-color:#f1eeee'>Stages </td><Td>" + stagesTicket + "</td></tr>" +
                        "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) ? "Open" : tic.StatusName) + "</td></tr>" +
                       //"<tr><td class='w120'  style='background-color:#f1eeee'>Group </td><Td>" + tic.GroupName + "</i></td></tr>" +
                       //"<tr><td class='w120'  style='background-color:#f1eeee'>Assigned </td><Td>" + tic.AssignedToMemberName + "</i></td></tr>" +
                       //"<tr><td class='w120'  style='background-color:#f1eeee'>Re-Assigned </td><Td>" + tic.ReassignedToMemberName + "</i></td></tr>" +
                       //"<tr><td class='w120'  style='background-color:#f1eeee'>Escalate </td><Td>" + tic.EscalateToGroupName + "</i></td></tr>" +
                       //"<tr><td class='w120'  style='background-color:#f1eeee'>Customer </td><Td>" + tic.CustomerName + "</i></td></tr>" +
                       //"<tr><td class='w120'  style='background-color:#f1eeee'>Product </td><Td>" + tic.Productname + "</i></td></tr>" +
                       "<tr><td class='w120'  style='background-color:#f1eeee'>Create</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "</td></tr>" +
                       "<tr><td class='w120'  style='background-color:#f1eeee'>Latest update</td><Td>" + GetLastestUpdate(tic.UpdateTicketHistory, '|') + "</td></tr>" +
                       "</table>";
                    bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>#" + CommonFunc.view_TicketId(tic.Id) + "</a>";
                    #endregion
                }
                else if (type.Equals("escalate"))
                {
                    subject = "[Enrich IMS]" + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
                    #region body email
                    bodyE = "Ticket #" + CommonFunc.view_TicketId(tic.Id) + " just had escalated to " + tic.EscalateToStageNote?.Split('|')[0] + "<br/><br/>" +
                      "<strong>Ticket name:</strong><br/>" + tic.Name + "<br/><br/>" +
                      "<strong>Description:</strong><br/>" + tic.Description +
                    (!string.IsNullOrEmpty(bTag) ? "<strong>Label:</strong><br/>" + bTag + "<br/>" : "") +
                      "<br/><table  class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                      "<tr><td class='w120' style='background-color:#f1eeee'>Stages </td><Td>" + stagesTicket + "</td></tr>" +
                      "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) ? "Open" : tic.StatusName) + "</td></tr>" +
                      //"<tr><td class='w120'  style='background-color:#f1eeee'>Group </td><Td>" + tic.GroupName + "</i></td></tr>" +
                      //"<tr><td class='w120'  style='background-color:#f1eeee'>Assigned </td><Td>" + tic.AssignedToMemberName + "</i></td></tr>" +
                      //"<tr><td class='w120'  style='background-color:#f1eeee'>Re-Assigned </td><Td>" + tic.ReassignedToMemberName + "</i></td></tr>" +
                      //"<tr><td class='w120'  style='background-color:#f1eeee'>Escalate </td><Td>" + tic.EscalateToGroupName + "</i></td></tr>" +
                      //"<tr><td class='w120'  style='background-color:#f1eeee'>Customer </td><Td>" + tic.CustomerName + "</i></td></tr>" +
                      //"<tr><td class='w120'  style='background-color:#f1eeee'>Product </td><Td>" + tic.Productname + "</i></td></tr>" +
                      "<tr><td class='w120'  style='background-color:#f1eeee'>Create By</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "</td></tr>" +
                      "<tr><td class='w120'  style='background-color:#f1eeee'>Latest update</td><Td>" + GetLastestUpdate(tic.UpdateTicketHistory, '|') + "</td></tr>" +
                      "</table>";
                    bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>#" + CommonFunc.view_TicketId(tic.Id) + "</a>";
                    #endregion
                }
                else //feedback
                {
                    var notificationService = new NotificationService();
                    P_Member cMem = AppLB.Authority.GetCurrentMember();
                    if (excludeTicketUpdateNotice != null && excludeTicketUpdateNotice.Count > 0)
                    {
                        NotiSendMemberNumber = NotiSendMemberNumber.Where(x => !excludeTicketUpdateNotice.Any(y => y == x)).ToList();
                    }
                    if (NotiSendMemberNumber.Count > 0)
                    {
                        var noti = new P_Notification();
                       
                       
                        noti.Action = NotificationActionDefine.AddNew;
                        noti.Category = NotificationCategoryDefine.Ticket;
                        noti.EntityId = tic.Id.ToString();
                        noti.EntityName = tic.Name;
                        noti.MemberNumber = cMem?.MemberNumber ?? null;
                        noti.MemberName = cMem?.FullName ?? null;
                        noti.UpdateId = db.T_TicketUpdateLog.Where(x => x.TicketId == tic.Id).OrderByDescending(x => x.UpdateId).FirstOrDefault()?.UpdateId;
                        if (tic.DateClosed != null)
                        {
                            noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketClose).FirstOrDefault().Id;
                        }
                        else
                        {
                            noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketUpdate).FirstOrDefault().Id;
                        }

                        noti.CreateAt = DateTime.UtcNow;
                        noti.CreateBy = cMem?.FullName ?? tic.CreateByName;
                        //var listMemberNoti = db.T_Project_Stage_Members.Where(x => forward_stages.Any(s => s.Contains(x.StageId))).Select(x => x.MemberNumber ).Distinct();
                        notificationService.Insert(noti, NotiSendMemberNumber);
                    }
                    subject = "[Enrich IMS]" + tic.Name + " #" + CommonFunc.view_TicketId(tic.Id);
                    #region body email
                    bodyE += "Ticket #" + CommonFunc.view_TicketId(tic.Id) + " just has new action" + "<br/><br/>" +
                        "<strong>Ticket Name: </strong><br/>" + tic.Name +
                        "<br/><br/><strong>Ticket action:</strong><br/>";
                    bodyE += "<u>" + fb.CreateByName + " at " + fb.CreateAt?.ToString("dd MMMM yyyy hh:mm tt") + " </u><br/>" + fb.Feedback + "<br/>";

                    bodyE += "<strong>Ticket description:</strong><br/>" + tic.Description +
                    (!string.IsNullOrEmpty(bTag) ? "<strong>Label:</strong><br/>" + bTag + "<br/>" : "") +
                    "<br/><table  class='table_border' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                      "<tr><td class='w120' style='background-color:#f1eeee'>Stages </td><Td>" + stagesTicket + "</td></tr>" +
                        "<tr><td class='w120' style='background-color:#f1eeee'>Status </td><Td>" + (string.IsNullOrWhiteSpace(tic.StatusName) ? "Open" : tic.StatusName) + "</td></tr>";

                    bodyE += "<tr><td class='w120'  style='background-color:#f1eeee'>Create</td><Td>" + tic.CreateAt?.ToString("dd MMM,yyyy hh:mm tt") + "<br/> by " + tic.CreateByName + "</td></tr>" +
                    "<tr><td class='w120'  style='background-color:#f1eeee'>Latest update</td><Td>" + GetLastestUpdate(tic.UpdateTicketHistory, '|') + "</td></tr>" +
                    "</table>";
                    bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>#" + CommonFunc.view_TicketId(tic.Id) + "</a>";
                    #endregion
                }

                var members = db.P_Member.Where(m => memberNumberList.Contains(m.MemberNumber) == true && m.Active == true);
                foreach (var item in members)
                {
                    to += item.PersonalEmail + ";";
                    firstname += item.FirstName + ";";
                    toMemNo += item.MemberNumber + ",";
                }
                if (!string.IsNullOrEmpty(tic.TagMemberNumber))
                {
                    cc = string.Join(";", db.P_Member.Where(x => tic.TagMemberNumber.Contains(x.MemberNumber) && x.Active == true && x.MemberNumber != memberNumberSubmit).Select(x => x.PersonalEmail));
                }
                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                string result = await _mailingService.SendBySendGrid(to, firstname, subject, bodyE, cc);
                if (shared_leaders != null && shared_leaders.Count > 0)
                {
                    var s_to = "";
                    var s_firstname = "";
                    var ld = db.P_Member.Where(m => shared_leaders.Contains(m.MemberNumber) == true&&m.Active==true);
                    foreach (var item in ld)
                    {
                        s_to += item.PersonalEmail + ";";
                        s_firstname += item.FirstName + ";";
                        toMemNo += item.MemberNumber + ",";
                    }
                    await _mailingService.SendBySendGrid(s_to, s_firstname, subject + " Shared", bodyE, "");
                }
                NoticeViewService.SpecifyNotice(subject, tic.Name, url_detail + tic.Id, toMemNo);
                NoticeViewService.WebPushNotice(subject, tic.Name, url_detail + tic.Id, toMemNo);

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        #region Tags - Loc update 20200407
        /// <summary>
        /// Svae Tags
        /// </summary>
        /// <param name="id">Tag Id</param>
        /// <param name="name">Tag Name</param>
        /// <param name="color">Tag Color</param>
        /// <param name="ErrMsg"></param>
        /// <param name="type">support_ticket|development_ticket|other</param>
        /// <returns></returns>
        public static T_Tags SaveTags(string id, string name, string color, string MemberName, out string ErrMsg, string type)
        {
            ErrMsg = "";
            WebDataModel db = new WebDataModel();
            var cMem = Authority.GetCurrentMember();
            using (var TranS = db.Database.BeginTransaction())
            {
                try
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        throw new Exception("Tag name is required.");
                    }

                    var tag_name = name;
                    color = string.IsNullOrEmpty(color) ? "#808080" : color;
                    var tag = db.T_Tags.Find(id);

                    if (tag == null)
                    {
                        //add new
                        if (db.T_Tags.Any(t => t.Id == name.Trim().Replace(" ", "_") && t.Type == type && t.SiteId== cMem.SiteId))
                        {
                            throw new Exception("Tag was exist.");
                        }

                        tag = new T_Tags()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = tag_name,
                            Type = type,
                            Color = color,
                            UpdateAt = DateTime.UtcNow,
                            UpdateBy = MemberName,
                            SiteId = cMem.SiteId
                        };
                        db.T_Tags.Add(tag);
                    }
                    else
                    {
                        //edit
                        if (db.T_Tags.Any(t => t.Type == type && t.SiteId == cMem.SiteId && t.Id == name.Trim().Replace(" ", "_")) == true && name.Trim().Replace(" ", "_") != tag.Id)
                        {
                            throw new Exception("Tag was exist.");
                        }

                        // thay doi name tags & color trong cac ticket
                        // [tag_name]::tag_color
                        //var newTag = $"{tag_name}::{color}";
                        //var tagNeedUpdate = db.T_SupportTicket.Where(ticket => ticket.Tags.Contains(tag.Name)).ToList();
                        //foreach (var item in tagNeedUpdate)
                        //{
                        //    var ticketNewTag = item.Tags.Split('|').Select(entry => entry.Contains(tag.Name) ? newTag : entry).ToList();
                        //    item.Tags = String.Join("|", ticketNewTag);
                        //}

                        tag.Name = tag_name;
                        tag.Color = color;
                        tag.UpdateAt = DateTime.UtcNow;
                        tag.UpdateBy = MemberName;
                        //tag.Type = type;
                        db.Entry(tag).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.SaveChanges();
                    TranS.Commit();
                    return tag;
                }
                catch (Exception ex)
                {
                    TranS.Dispose();
                    ErrMsg = ex.Message;
                    return null;
                }
            }
        }

        /// <summary>
        /// Get tag info
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public static T_Tags GetTagsInfo(string tagId, out string ErrMsg)
        {
            ErrMsg = "";
            try
            {
                WebDataModel db = new WebDataModel();
                var tag = db.T_Tags.Find(tagId);
                if (tag == null)
                {
                    throw new Exception("Tag not found.");
                }

                return tag;
            }
            catch (Exception ex)
            {
                ErrMsg = ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Delete tag
        /// </summary>
        /// <param name="tagId">Tag Id</param>
        /// <returns></returns>
        public static string DeleteTags(string tagId)
        {
            WebDataModel db = new WebDataModel();
            using (var TranS = db.Database.BeginTransaction())
            {
                try
                {
                    var tag = db.T_Tags.Find(tagId);

                    if (tag != null)
                    {
                        //var tag_value = tag.Name + "-" + tag.Color + "|";

                        //foreach (var item in db.T_SupportTicket.ToList())
                        //{
                        //    if ((item.Tags ?? "").Contains(tag_value))
                        //    {
                        //        item.Tags = item.Tags.Replace(tag_value, "");
                        //        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        //    }
                        //}

                        db.T_Tags.Remove(tag);
                        db.SaveChanges();
                        TranS.Commit();
                    }

                    return "";
                }
                catch (Exception ex)
                {
                    TranS.Dispose();
                    return ex.Message;
                }
            }
        }
        #endregion

        public class AutoTicketScenario
        {




            /// <summary>
            /// update ticket onboarding status khi nuvei reply
            /// </summary>
            /// <param name="orderCode"></param>
            /// <param name="customerCode"></param>
            /// <param name="NuveiResponseStatus">Failed/Completed/Cancelled</param>
            /// <returns></returns>
            //public static async Task UpdateTicketNuveiOnboarding(string customerCode, string NuveiResponseStatus, string updateBy = "Nuvei system")
            //{
            //    using (var db = new WebDataModel())
            //    {
            //        try
            //        {
            //            //System.Diagnostics.Debug.WriteLine("onboarding update: chua xu ly - CustomerCode:" + customerCode);
            //            long onboardingTicketType = (long)AppLB.UserContent.TICKET_TYPE.NuveiOnboarding;
            //            var ticket = db.T_SupportTicket.Where(t => t.CustomerCode == customerCode && t.TypeId == onboardingTicketType).FirstOrDefault();
            //            if (ticket == null)
            //            {
            //                throw new Exception("Ticket is not found");
            //            }
            //            if (NuveiResponseStatus == "Completed" && ticket.StatusId != (long)AppLB.UserContent.DeploymentTicket_Status.Close)
            //            {
            //                //new feedback
            //                DateTime date = InsertFeedback(db, ticket.Id, "Nuvei reply is " + NuveiResponseStatus, "Status: " + NuveiResponseStatus, NuveiResponseStatus, -1, updateBy);
            //                //change status
            //                ticket.StatusId = (long)UserContent.DeploymentTicket_Status.Close;
            //                ticket.StatusName = NuveiResponseStatus;
            //                ticket.UpdateTicketHistory = (ticket.UpdateTicketHistory ?? "") + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - by " + updateBy + "|";
            //                ticket.DateClosed = DateTime.UtcNow;
            //                ticket.CloseByName = updateBy;
            //                db.Entry(ticket).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //                await SendNoticeAfterTicketUpdate(ticket, "feedback");

            //                await UpdateSatellite(ticket.Id, db, updateBy);

            //            }
            //            else if (NuveiResponseStatus == "Failed" && ticket.StatusId != (long)AppLB.UserContent.DeploymentTicket_Status.Cancel)
            //            {
            //                //new feedback
            //                DateTime date = InsertFeedback(db, ticket.Id, "Nuvei reply is " + NuveiResponseStatus, "Status: " + NuveiResponseStatus, NuveiResponseStatus, -1, updateBy);
            //                //change status
            //                ticket.StatusId = (long)UserContent.TICKET_STATUS.NuveiOnboarding_Failed;
            //                ticket.UpdateTicketHistory = (ticket.UpdateTicketHistory ?? "") + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - by " + updateBy + "|";
            //                ticket.StatusName = NuveiResponseStatus;
            //                db.Entry(ticket).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //                await SendNoticeAfterTicketUpdate(ticket, "feedback");
            //            }
            //            else if (NuveiResponseStatus == "Cancelled" && ticket.StatusId != (long)AppLB.UserContent.TICKET_STATUS.NuveiOnboarding_Cancelled)
            //            {
            //                //new feedback
            //                DateTime date = InsertFeedback(db, ticket.Id, "Nuvei reply is " + NuveiResponseStatus, "Status: " + NuveiResponseStatus, NuveiResponseStatus, -1, updateBy);
            //                //change status
            //                ticket.StatusId = (long)UserContent.TICKET_STATUS.NuveiOnboarding_Cancelled;
            //                ticket.StatusName = NuveiResponseStatus;
            //                ticket.UpdateTicketHistory = (ticket.UpdateTicketHistory ?? "") + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - by " + updateBy + "|";

            //                db.Entry(ticket).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //                await SendNoticeAfterTicketUpdate(ticket, "feedback");
            //            }
            //            var stage_Mapping = db.T_TicketStage_Status.FirstOrDefault(x => x.TicketId == ticket.Id);
            //            if (stage_Mapping != null)
            //            {
            //                stage_Mapping.StatusId = ticket.StatusId;
            //                stage_Mapping.CloseDate = DateTime.UtcNow;
            //            }
            //            db.SaveChanges();
            //        }
            //        catch (Exception ex)
            //        {
            //            System.Diagnostics.Debug.WriteLine("[TicketUpdateFromOnboarding]" + ex.Message);

            //        }
            //    }
            //}

            /// <summary>
            /// Sep1: Khi tao moi 1 NEW SALON, ticket nay se duoc tao ra
            /// noi dung:
            /// Gathers info for Nuvei App(nuvei onboarding), Payment for hardware(finance), and sends questionnaire(ims onboarding)
            /// </summary>
            /// <param name="customerCode"></param>
            /// <returns></returns>
            public static async Task<long> NewTicketSalesLead(string customerCode, string orderCode, string description)
            {
                using (var db = new WebDataModel())
                {
                    try
                    {
                        var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode.Equals(customerCode));
                        if (MerchantType.STORE_IN_HOUSE.ToString().Equals(cus?.Type))
                            return 0;

                        //neu ton tai sales ticket cua khach hang nay => update ticket
                        //var list_ticket = db.T_SupportTicket.Where(t => t.CustomerCode == customerCode).ToList();
                        //check ticket chua co order
                        // var ticket = list_ticket.Where(t => (t.OrderCode == null || t.OrderCode == "") && t.TypeId == (long)AppLB.UserContent.TICKET_TYPE.Sales).FirstOrDefault();
                        var order = db.O_Orders.FirstOrDefault(o => o.OrdersCode == orderCode);
                        if (order != null)
                        {
                            //TH: tao luon invoice ma van chua co tic
                            //ket, khong can tao ticket sales nua,=> tao luon ims onboarding va finance ticket.
                            string linkViewInvoiceFull = ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + order.OrdersCode + "&flag=Invoices";
                            string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + order.Id;

                            var checkTerminal = (from ordPrd in db.Order_Products
                                                 join prd in db.O_Product on ordPrd.ProductCode equals prd.Code
                                                 join prdmodel in db.O_Product_Model on ordPrd.ModelCode equals prdmodel.ModelCode
                                                 where ordPrd.OrderCode == orderCode && (prd.ProductLineCode == "terminal" || prdmodel.MerchantOnboarding == true)
                                                 select ordPrd).Any();
                            if (checkTerminal && !db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Any(t => t.CustomerCode == order.CustomerCode && t.OrderCode == order.OrdersCode && t.T_TicketTypeMapping.Any(x => x.TypeId == (long)UserContent.TICKET_TYPE.NuveiOnboarding)))
                            {
                                await NewTicketNuveiOnboarding(order.InvoiceNumber.ToString(), "<iframe  width='600' height='900' src='" + linkViewInvoice + "'></iframe>");
                            }
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }

                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("[NewTicketSalesLead]" + ex.InnerException == null ? ex.Message : ex.InnerException.Message);
                        return -1;
                    }
                }
            }
            /// <summary>
            /// Cap nhat trang thai cac feature, module ve tinh <- sau khi ticket duoc update trang thai.
            /// </summary>
            /// <param name="ticketId"></param>
            /// <returns></returns>
            //public static async Task UpdateSatellite(long ticketId, WebDataModel db = null, string updateBy = "System")
            //{
            //    if (db == null)
            //    {
            //        db = new WebDataModel();
            //    }
            //    using (db)
            //    {
            //        var tic = db.T_SupportTicket.Find(ticketId);
            //        switch (tic.TypeId)
            //        {
            //            case 1:
            //                //sales
            //                //await CheckOrderStatus(db, tic);
            //                break;
            //            case 2:
            //            case 3:
            //            case 10:
            //                //finace/Nuvei/Ims onboarding
            //                await CheckCreateDeploymentTicket(db, tic);
            //                break;
            //            case 4:
            //                //deployment
            //               // CheckDeploymentStatus(db, tic, updateBy);
            //                break;

            //            default:
            //                break;
            //        }
            //    }

            //    return;
            //}

            /// <summary>
            /// Step2: sau khi Estimate -> closed (Invoice -> submited), tao ticket finance
            /// ACH one time required for POS purchase, ACH recurring is for monthly subscription
            ///  Status: Processed
            ///  ** time to validate is 3 business days**
            ///  Marks Invoice as Paid
            ///  Status: Completed = payment was sucessful
            ///  Failed = ACH failed
            /// </summary>
            /// <param name="invoiceNumber"></param>
            /// <returns></returns>
            //public static async Task<long> NewTicketFinance(string invoiceNumber, string description)
            //{
            //    try
            //    {
            //        var tic = new T_SupportTicket();
            //        using (var db = new WebDataModel())
            //        {
            //            //ordercode ~ estimates number ~ invoice number
            //            var invoice = db.O_Orders.Where(o => o.InvoiceNumber.ToString() == invoiceNumber).FirstOrDefault();
            //            if (invoice == null)
            //            {
            //                throw new Exception("Invoice #" + invoiceNumber + " not existed.");
            //            }

            //            var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode.Equals(invoice.CustomerCode));
            //            if (MerchantType.STORE_IN_HOUSE.ToString().Equals(cus?.Type))
            //                return 0;
            //            int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
            //                                               && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
            //            tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("ff"));
            //            tic.Name = "Finance: " + invoice?.CustomerName + ", Invoice#" + invoice.InvoiceNumber;
            //            tic.Description = description;
            //            tic.CreateAt = DateTime.UtcNow;
            //            tic.CreateByName = "System";
            //            tic.Visible = true;
            //            var groupFinance = db.P_Department.Where(d => d.Type == "FINANCE").ToList();
            //            var group = groupFinance.Count() == 1 ? groupFinance.FirstOrDefault() : groupFinance.Where(g => g.ParentDepartmentId > 0).FirstOrDefault();
            //            tic.GroupID = group?.Id;
            //            tic.GroupName = group?.Name;
            //            bool IsSetGroupsAssign = false;
            //            if (!string.IsNullOrEmpty(invoice.SalesMemberNumber))
            //            {
            //                var AllFINANCEGroups = db.P_Department.Where(x => x.Type == "FINANCE").ToList();

            //                foreach (var gr in AllFINANCEGroups.Where(x => x.ParentDepartmentId > 0))
            //                {
            //                    List<string> MemberOfGroups = new List<string>();
            //                    MemberOfGroups.AddRange(gr.GroupMemberNumber.Split(','));
            //                    if (!string.IsNullOrEmpty(gr.SupervisorNumber))
            //                    {
            //                        MemberOfGroups.Add(gr.SupervisorNumber);
            //                    }
            //                    if (MemberOfGroups.Any(x => x.Contains(invoice.SalesMemberNumber)))
            //                    {
            //                        tic.GroupID = gr.Id;
            //                        tic.GroupName = gr.Name;
            //                        tic.AssignedToMemberName = invoice?.SalesName;
            //                        tic.AssignedToMemberNumber = invoice?.SalesMemberNumber;
            //                        tic.DateOpened = DateTime.Now;
            //                        IsSetGroupsAssign = true;
            //                        break;
            //                    }
            //                }
            //                //set assign for seller if seller in groups type : all 
            //                if (IsSetGroupsAssign == false)
            //                {
            //                    var seller = db.P_Member.FirstOrDefault(x => x.MemberNumber == invoice.SalesMemberNumber);
            //                    if (!string.IsNullOrEmpty(seller.DepartmentId))
            //                    {
            //                        var ListDepOfSeller = seller.DepartmentId.Split(',');
            //                        var parentDepFINANCE = AllFINANCEGroups.Where(g => g.ParentDepartmentId == null).FirstOrDefault();
            //                        string Id = parentDepFINANCE.Id.ToString();
            //                        if (ListDepOfSeller.Any(x => x.Contains(Id)))
            //                        {
            //                            tic.AssignedToMemberName = seller?.FullName;
            //                            tic.AssignedToMemberNumber = seller?.MemberNumber;
            //                            tic.GroupID = parentDepFINANCE.Id;
            //                            tic.GroupName = parentDepFINANCE.Name;
            //                            tic.DateOpened = DateTime.Now;
            //                            IsSetGroupsAssign = true;
            //                        }
            //                    }
            //                }
            //            }
            //            //set assign for leader
            //            if (IsSetGroupsAssign == false)
            //            {
            //                var assign_member = groupFinance.Where(g => g.ParentDepartmentId > 0).FirstOrDefault()?.LeaderNumber;
            //                if (!string.IsNullOrEmpty(assign_member))
            //                {
            //                    var assign_mem = db.P_Member.Where(m => m.MemberNumber == assign_member).FirstOrDefault();
            //                    tic.AssignedToMemberName = assign_mem?.FullName;
            //                    tic.AssignedToMemberNumber = assign_mem?.MemberNumber;
            //                    tic.GroupID = group?.Id;
            //                    tic.GroupName = group?.Name;
            //                    tic.DateOpened = DateTime.Now;
            //                    IsSetGroupsAssign = true;
            //                }

            //            }

            //            tic.TypeId = (long)AppLB.UserContent.TICKET_TYPE.Finance;
            //            tic.IdentityType = TicketIdentityType.Finance.Text();
            //            tic.TypeName = "Finance";
            //            tic.CustomerCode = invoice.CustomerCode;
            //            tic.CustomerName = invoice.CustomerName;
            //            if (invoice.GrandTotal == 0)
            //            {
            //                tic.StatusId = (long)AppLB.UserContent.TICKET_STATUS.Finance_Complete;
            //                tic.StatusName = "Completed";
            //                tic.DateClosed = DateTime.UtcNow;
            //            }
            //            else
            //            {
            //                tic.StatusId = (long)AppLB.UserContent.TICKET_STATUS.Finance_Open;
            //                tic.StatusName = "Open";
            //            }

            //            tic.OrderCode = invoice.OrdersCode;
            //            db.T_SupportTicket.Add(tic);
            //            await db.SaveChangesAsync();



            //            //#region feedback thong tin finance len smartsheet.
            //            //bool sh = false;
            //            //string finance_data = string.Empty;
            //            //var customerInfo = db.C_Customer.Where(c => c.CustomerCode == invoice.CustomerCode).FirstOrDefault();






            //            ////feedback
            //            //InsertFeedback(null, tic.Id, (sh ? "ACH has been push up smartsheet" : "Please update the bank infomation of merchant and reupload ACH"), finance_data, "", -1, "System");

            //            //#endregion




            //        }
            //        await AutoAssignment(null, tic);
            //        return tic.Id;
            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Trace.WriteLine("[NewTicketFinance]" + ex.Message);
            //        return -1;
            //    }
            //}

            /// <summary>
            /// Step2: sau khi Estimate -> closed (Invoice -> submited), tao ticket onboarding
            /// ACH one time required for POS purchase, ACH recurring is for monthly subscription
            /// Application is prepped for customer signature along with questionnaire
            /// Signature
            /// sent to Nuvei
            /// Approved
            /// MID issued
            /// </summary>
            /// <param name="invoiceNumber"></param>
            /// <returns></returns>
            public static async Task NewTicketNuveiOnboarding(string invoiceNumber, string description)
            {
                using (var db = new WebDataModel())
                {
                    try
                    {
                        var tic = new T_SupportTicket();
                        //ordercode ~ estimates number ~ invoice number
                        var invoice = db.O_Orders.Where(o => o.InvoiceNumber.ToString() == invoiceNumber).FirstOrDefault();
                        if (invoice == null)
                        {
                            throw new Exception("Invoice #" + invoiceNumber + " not existed.");
                        }
                        var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode.Equals(invoice.CustomerCode));
                        if (MerchantType.STORE_IN_HOUSE.ToString().Equals(cus?.Type))
                            throw new Exception();
                        if (!db.C_MerchantSubscribe.Any(s => s.CustomerCode == invoice.CustomerCode))
                        {
                            int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
                            tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString("ff"));
                            tic.Name = "Priority Onboarding: " + invoice?.CustomerName + ", Invoice#" + invoice.InvoiceNumber;
                            tic.Description = description;
                            tic.CreateAt = DateTime.UtcNow;
                            tic.CreateByName = "System";
                            tic.Visible = true;
                            var group = new P_Department();
                            if (!string.IsNullOrEmpty(invoice.PartnerCode))
                            {
                                group = db.P_Department.Where(x => x.PartnerCode == invoice.PartnerCode && x.Type == "ONBOARDING" && x.Active == true && x.ParentDepartmentId > 0).FirstOrDefault();
                            }
                            else
                            {
                                group = db.P_Department.Where(x => string.IsNullOrEmpty(x.PartnerCode) && x.Type == "ONBOARDING" && x.Active == true && x.ParentDepartmentId > 0).FirstOrDefault();
                            }
                            tic.GroupID = group?.Id;
                            tic.GroupName = group?.Name;

                            if (group != null)
                            {
                                if (!string.IsNullOrEmpty(group.LeaderNumber))
                                {
                                    var assign_mem = db.P_Member.Where(m => m.MemberNumber == group.LeaderNumber && m.Active == true).FirstOrDefault();
                                    if (assign_mem != null)
                                    {
                                        tic.AssignedToMemberName = assign_mem?.FullName;
                                        tic.AssignedToMemberNumber = assign_mem?.MemberNumber;
                                    }
                                }
                                else if (!string.IsNullOrEmpty(group.SupervisorNumber))
                                {
                                    var assign_mem = db.P_Member.Where(m => m.MemberNumber == group.SupervisorNumber && m.Active == true).FirstOrDefault();
                                    if (assign_mem != null)
                                    {
                                        tic.AssignedToMemberName = assign_mem?.FullName;
                                        tic.AssignedToMemberNumber = assign_mem?.MemberNumber;
                                    }
                                }
                            }
                            tic.DateOpened = DateTime.UtcNow;
                            tic.TypeId = AppLB.UserContent.TICKET_TYPE.NuveiOnboarding.ToString();
                            tic.TypeName = "On Boarding";

                            tic.T_TicketTypeMapping.Add(new T_TicketTypeMapping
                            {
                                TypeId = long.Parse(tic.TypeId),
                                TypeName = tic.TypeName,
                                TicketId = tic.Id,
                            });

                            tic.CustomerCode = invoice.CustomerCode;
                            tic.IdentityType = TicketIdentityType.PriorityOnboarding.Text();
                            tic.CustomerName = invoice.CustomerName;

                            tic.OrderCode = invoice.OrdersCode;
                            //project
                            string TypeDeployment = BuildInCodeProject.Deployment_Ticket.ToString();
                            var projectDeployment = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDeployment).FirstOrDefault();
                            tic.ProjectId = projectDeployment.Id;
                            tic.ProjectName = projectDeployment.Name;
                            var version = db.T_Project_Milestone.Where(x => x.ParentId == projectDeployment.Id && x.Type == "Project_version").FirstOrDefault();
                            tic.VersionId = version.Id;
                            tic.VersionName = version.Name;
                            var stageOnDeployment = new T_Project_Stage();
                            stageOnDeployment = db.T_Project_Stage.Where(x => x.ProjectId == projectDeployment.Id && (x.BuildInCode == "default" || string.IsNullOrEmpty(x.BuildInCode))).FirstOrDefault();
                            tic.StageId = stageOnDeployment.Id;
                            tic.StageName = stageOnDeployment.Name;

                           // var ticket_stage_status = new T_TicketStage_Status();
                            //ticket_stage_status.Id = Guid.NewGuid().ToString("N");
                            //ticket_stage_status.TicketId = tic.Id;
                            //ticket_stage_status.OpenDate = tic.DateOpened;
                            //ticket_stage_status.ProjectVersionId = tic.VersionId;
                            //ticket_stage_status.ProjectVersionName = tic.VersionName;
                            //ticket_stage_status.StageId = tic.StageId;
                            //ticket_stage_status.StageName = tic.StageName;
                            //ticket_stage_status.TypeId = tic.TypeId;
                            //ticket_stage_status.StatusId = tic.StatusId;
                            //ticket_stage_status.Active = true;
                            //ticket_stage_status.AssignedMember_Names = !string.IsNullOrEmpty(tic.AssignedToMemberName) ? string.Join("|", tic.AssignedToMemberName.Split(',')) : null;
                            //ticket_stage_status.AssignedMember_Numbers = !string.IsNullOrEmpty(tic.AssignedToMemberNumber) ? string.Join("|", tic.AssignedToMemberNumber.Split(',')) : null;
                            //tic.AssignJson = JsonConvert.SerializeObject(new List<JsonAssignedModel>(){new JsonAssignedModel
                            //{
                            //    StageId = ticket_stage_status.StageId,
                            //    StageName = ticket_stage_status.StageName,
                            //    AssignMemberNumber = ticket_stage_status.AssignedMember_Numbers?.Replace("|", ","),
                            //    AssignMemberName = ticket_stage_status.AssignedMember_Names?.Replace("|", ",")
                            //}});
                          //  db.T_TicketStage_Status.Add(ticket_stage_status);
                            db.SaveChanges();

                            var merchantSubcribe = new C_MerchantSubscribe
                            {
                                Id = DateTime.UtcNow.Ticks,
                                CustomerCode = invoice.CustomerCode,
                                TicketId = tic.Id
                            };
                            db.T_SupportTicket.Add(tic);
                            db.C_MerchantSubscribe.Add(merchantSubcribe);
                            db.SaveChanges();
                            await AutoAssignment(db, tic);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("[NewTicketNuveiOnboarding]" + ex.Message);
                    }
                }
            }
            /// <summary>
            /// Onboarding:
            /// gui questionare cho merchant/ lay thong tin khach hang va cac dieu kien de su dung dich vu...
            /// </summary>
            /// <param name="customer"></param>
            /// <param name="description"></param>
            /// <returns></returns>
            //public static async Task<long> NewTicketNewSalon(C_Customer cus, string description, O_Orders order = null)
            //{
            //    using (var db = new WebDataModel())
            //    {
            //        try
            //        {
            //            if (MerchantType.STORE_IN_HOUSE.ToString().Equals(cus?.Type))
            //                return 0;

            //            var tic = new T_SupportTicket();
            //            //ordercode ~ estimates number ~ invoice number
            //            /*var invoice = db.O_Orders.Where(o => o.InvoiceNumber.ToString() == invoiceNumber).FirstOrDefault();
            //            if (invoice == null)
            //            {
            //                throw new Exception("Invoice #" + invoiceNumber + " not existed.");
            //            }*/
            //            int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
            //            tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("ff"));
            //            // tic.Name = "IMS Onboarding: " + invoice?.CustomerName + ", Invoice#" + invoice.InvoiceNumber;
            //            tic.Name = "New salon: " + (cus?.BusinessName) ?? cus.OwnerName;
            //            tic.Description = description;
            //            tic.CreateAt = DateTime.UtcNow;
            //            tic.CreateByName = "System";
            //            tic.Visible = true;
            //            var groupSale = db.P_Department.Where(d => d.Type == "SALES").ToList();
            //            var group = groupSale.Count() == 1 ? groupSale.FirstOrDefault() : groupSale.Where(g => g.ParentDepartmentId > 0).FirstOrDefault();

            //            //bau update change auto assign for leader to seller 16/03/2021
            //            bool IsSetGroupsAssign = false;
            //            if (order != null)
            //            {
            //                if (!string.IsNullOrEmpty(order.SalesMemberNumber))
            //                {
            //                    tic.AssignedToMemberName = order?.SalesName;
            //                    tic.AssignedToMemberNumber = order?.SalesMemberNumber;
            //                    var AllSalesGroups = db.P_Department.Where(x => x.Type == "SALES").ToList();

            //                    foreach (var gr in AllSalesGroups.Where(x => x.ParentDepartmentId > 0))
            //                    {
            //                        List<string> MemberOfGroups = new List<string>();
            //                        MemberOfGroups.AddRange(gr.GroupMemberNumber.Split(','));
            //                        if (!string.IsNullOrEmpty(gr.SupervisorNumber))
            //                        {
            //                            MemberOfGroups.Add(gr.SupervisorNumber);
            //                        }
            //                        if (MemberOfGroups.Any(x => x.Contains(order.SalesMemberNumber)))
            //                        {
            //                            tic.GroupID = gr.Id;
            //                            tic.GroupName = gr.Name;
            //                            tic.DateOpened = DateTime.Now;
            //                            IsSetGroupsAssign = true;
            //                            break;
            //                        }
            //                    }
            //                    if (IsSetGroupsAssign == false)
            //                    {
            //                        var parentDepamentSales = AllSalesGroups.Where(x => x.ParentDepartmentId == null).FirstOrDefault();
            //                        if (parentDepamentSales != null)
            //                        {
            //                            tic.GroupID = parentDepamentSales.Id;
            //                            tic.GroupName = parentDepamentSales.Name;
            //                            tic.DateOpened = DateTime.Now;
            //                            IsSetGroupsAssign = true;
            //                        }
            //                    }
            //                }
            //            }
            //            if (IsSetGroupsAssign == false)
            //            {
            //                var parentDepamentSales = groupSale.Where(g => g.ParentDepartmentId > 0).FirstOrDefault();
            //                tic.GroupID = parentDepamentSales.Id;
            //                tic.GroupName = parentDepamentSales.Name;
            //                tic.DateOpened = DateTime.Now;
            //            }
            //            tic.TypeId = (long)AppLB.UserContent.TICKET_TYPE.Sales;
            //            tic.IdentityType = TicketIdentityType.NewSalon.Text();
            //            tic.TypeName = "SALES";
            //            tic.CustomerCode = cus.CustomerCode;
            //            tic.CustomerName = cus.ContactName;
            //            tic.StatusId = (long)AppLB.UserContent.TICKET_STATUS.Sales_Lead;
            //            tic.StatusName = "Lead";
            //            db.T_SupportTicket.Add(tic);
            //            db.SaveChanges();

            //            //string qnLink = string.Format("{0}://{1}/{2}", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority, "Page/salon/Questionare/" + cus.Id + "?n=" + AppLB.CommonFunc.ConvertNonUnicodeURL(tic.CustomerName).Replace("'", "").Replace("-", "_"));
            //            string qnLink = ConfigurationManager.AppSettings["IMSUrl"] + "Page/salon/Questionare/" + cus.Id + "?n=" + AppLB.CommonFunc.ConvertNonUnicodeURL(tic.CustomerName).Replace("'", "").Replace("-", "_");
            //            string fbConent = "This is link to questionare form of " + tic.CustomerName?.ToUpper() + "<br/><a href='" + qnLink + "' target='_blank'>" + qnLink + "</a>";
            //            DateTime date = InsertFeedback(db, tic.Id, "Questionare form", fbConent, "", -1, "System");

            //            await AutoAssignment(null, tic);

            //            return tic.Id;
            //        }
            //        catch (Exception ex)
            //        {
            //            System.Diagnostics.Trace.WriteLine("[NewTicketIMSOnboarding]" + ex.Message);
            //            return -1;
            //        }
            //    }
            //}

            /// <summary>
            /// Step3: sau khi ticket finance v onboarding complete
            /// select inv# in package
            /// Hardware integrated
            /// Generates Packing Slip
            /// </summary>
            /// <param name="invoiceNumber"></param>
            /// <returns></returns>
            public static async Task<long> NewTicketDeployment(string invoiceNumber)
            {
                using (var db = new WebDataModel())
                {
                    try
                    {
                        var Order = db.O_Orders.FirstOrDefault(x => x.OrdersCode == invoiceNumber);


                        // check status invoice
                        if (Order == null || (Order?.Status == InvoiceStatus.Open.ToString() || Order?.Status == InvoiceStatus.Canceled.ToString()))
                        {
                            return 0;
                        }
                     
                        var productCodesEnableDeploymentTicket = (from inv in db.Order_Subcription join pro in db.License_Product on inv.Product_Code equals pro.Code where inv.OrderCode == invoiceNumber && pro.DeploymentTiketAuto == true select inv.Product_Code).ToList();

                        if (productCodesEnableDeploymentTicket.Count() == 0)
                        {
                            return 0;
                        }
                        var allInvoiceHaveDeploymentTicket = db.Order_Subcription.Where(x => x.CustomerCode == Order.CustomerCode && productCodesEnableDeploymentTicket.Any(y => y == x.Product_Code)).Select(x=>x.OrderCode).Distinct().ToList();
                        var deploymentTypeId = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                        var checkDeploymentTicketExist = db.T_SupportTicket.Any(x => x.CustomerCode == Order.CustomerCode && x.CreateByName == "System" && allInvoiceHaveDeploymentTicket.Any(y => y == x.OrderCode));
                        if(checkDeploymentTicketExist)
                        {
                            return 0;
                        }
                        var tic = new T_SupportTicket();
                        var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode.Equals(Order.CustomerCode));
                        if (MerchantType.STORE_IN_HOUSE.ToString().Equals(cus?.Type))
                            return 0;

                        int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
                                                           && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
                        tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString("ff"));
                        tic.Name = "Deployment: " + Order?.CustomerName + ", Invoice#" + Order.InvoiceNumber;
                        string description = "<iframe  width='600' height='900' src='" + ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + Order.OrdersCode + "&flag=Estimates'></iframe>";
                        tic.Description = description;
                        tic.CreateAt = DateTime.UtcNow;
                        tic.CreateByName = "System";
                        tic.Visible = true;
                        tic.SiteId = cus.SiteId;
                        var group = new P_Department();
                        if (!string.IsNullOrEmpty(Order.PartnerCode))
                        {
                            group = db.P_Department.Where(x => x.PartnerCode == Order.PartnerCode && x.Type == "DEPLOYMENT" && x.Active == true && x.ParentDepartmentId > 0 && x.SiteId == cus.SiteId).FirstOrDefault();
                        }
                        else
                        {
                            group = db.P_Department.Where(x => string.IsNullOrEmpty(x.PartnerCode) && x.Type == "DEPLOYMENT" && x.Active == true && x.ParentDepartmentId > 0 && x.SiteId == cus.SiteId).FirstOrDefault();
                        }
                        tic.GroupID = group?.Id;
                        tic.GroupName = group?.Name;
                        if (group != null)
                        {
                            if (!string.IsNullOrEmpty(group.LeaderNumber))
                            {
                                var assign_mem = db.P_Member.Where(m => m.MemberNumber == group.LeaderNumber && m.Active == true).FirstOrDefault();
                                if (assign_mem != null)
                                {
                                    tic.AssignedToMemberName = assign_mem?.FullName;
                                    tic.AssignedToMemberNumber = assign_mem?.MemberNumber;
                                }
                            }
                            else if (!string.IsNullOrEmpty(group.SupervisorNumber))
                            {
                                var assign_mem = db.P_Member.Where(m => m.MemberNumber == group.SupervisorNumber && m.Active == true).FirstOrDefault();
                                if (assign_mem != null)
                                {
                                    tic.AssignedToMemberName = assign_mem?.FullName;
                                    tic.AssignedToMemberNumber = assign_mem?.MemberNumber;
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(Order.SalesMemberNumber) && !(tic.AssignedToMemberNumber ?? "").Contains(Order.SalesMemberNumber))
                        {
                            tic.TagMemberNumber = Order.SalesMemberNumber;
                            tic.TagMemberName = Order.SalesName;
                        }

                        tic.DateOpened = DateTime.UtcNow;
                        tic.TypeId = AppLB.UserContent.TICKET_TYPE.Deployment.ToString();
                        tic.TypeName = "Deployment";
                        var typeMapping = new T_TicketTypeMapping();
                        typeMapping.TypeId = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                        typeMapping.TypeName = tic.TypeName;
                        tic.T_TicketTypeMapping.Add(typeMapping);
                        tic.IdentityType = TicketIdentityType.Deployment.Text();
                        tic.CustomerCode = Order.CustomerCode;
                        tic.CustomerName = Order.CustomerName;
                        string TypeDeployment = BuildInCodeProject.Deployment_Ticket.ToString();
                        var projectDeployment = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDeployment).FirstOrDefault();
                        tic.OrderCode = Order.OrdersCode;
                        tic.ProjectId = projectDeployment.Id;
                        tic.ProjectName = projectDeployment.Name;
                        var version = db.T_Project_Milestone.Where(x => x.ParentId == projectDeployment.Id && x.Type == "Project_version").FirstOrDefault();
                        tic.VersionId = version.Id;
                        tic.VersionName = version.Name;
                        var stageOnDeployment = new T_Project_Stage();
                        stageOnDeployment = db.T_Project_Stage.Where(x => x.ProjectId == projectDeployment.Id && x.BuildInCode == "default").FirstOrDefault();
                        tic.StageId = stageOnDeployment?.Id;
                        tic.StageName = stageOnDeployment?.Name;

                        db.T_SupportTicket.Add(tic);
                        var feedbackCreateTicket = new T_TicketFeedback();
                        feedbackCreateTicket.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                        feedbackCreateTicket.TicketId = tic.Id;
                        feedbackCreateTicket.FeedbackTitle = "Invoice #" + Order.OrdersCode + " has been paid";
                        feedbackCreateTicket.Feedback = "- Invoice: <a onclick='show_invoice(" + Order.OrdersCode + ")'><b>#" + Order.OrdersCode + "</b></a><br/>";
                        feedbackCreateTicket.Feedback += "- Sales Person: " + (string.IsNullOrEmpty(Order.SalesMemberNumber) ? "N/A" : "<a href='#Mentions_" + Order.SalesMemberNumber + "'>" + "@" + Order.SalesName + "</a>") + "<br/>";
                        feedbackCreateTicket.Feedback += "- Created By: " + "<a href='#Mentions_" + Order.CreateByMemNumber + "'>" + "@" + Order.CreatedBy + "</a>";

                        feedbackCreateTicket.CreateByName = "System";
                        feedbackCreateTicket.CreateAt = DateTime.UtcNow;
                        feedbackCreateTicket.DateCode = DateTime.UtcNow.ToString("yyyyMMdd");
                        db.T_TicketFeedback.Add(feedbackCreateTicket);
                        Order.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Ready.ToString();
                        db.Entry(Order).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        await AutoAssignment(db, tic);

                        //var _clickUpConnectorService = EngineContext.Current.Resolve<IClickUpConnectorService>();
                        //await _clickUpConnectorService.CreateTaskDeliveryToClickUpAsync(Order.Id.ToString());
                        await new StoreServices(db).SynDeliveryToClickUpAsync(Order.OrdersCode);
                        return tic.Id;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.WriteLine("[NewTicketDeployment]" + ex.Message);
                        return -1;
                    }
                }
            }



            //private static async Task CheckOrderStatus(WebDataModel db, T_SupportTicket currentTicket)
            //{
            //    try
            //    {
            //        if (!string.IsNullOrWhiteSpace(currentTicket.OrderCode))
            //        {
            //            var order = db.O_Orders.Where(o => o.OrdersCode == currentTicket.OrderCode).FirstOrDefault();
            //            string fullname = AppLB.Authority.GetCurrentMember().FullName;
            //            if (order.InvoiceNumber == null && currentTicket.StatusId == (long)UserContent.TICKET_STATUS.Sales_Closed)
            //            {
            //                //update close estimates -> order submitted.
            //                order.Status = InvoiceStatus.Open.ToString();
            //                order.UpdatedAt = DateTime.UtcNow;
            //                order.UpdatedBy = fullname;
            //                order.UpdatedHistory = fullname + "(ticket#" + currentTicket.Id + ")";
            //                order.InvoiceNumber = long.Parse(order.OrdersCode);
            //                order.StatusHistory += "|" + order.Status + "  - Update by: " + fullname + " - At: " + DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt");
            //                //order.BundelStatus = "Packaging";
            //                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();

            //                string linkViewInvoiceFull = ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + order.OrdersCode + "&flag=Invoices";
            //                string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + order.Id;
            //                //  await NewTicketFinance(order.InvoiceNumber.ToString(), "<iframe  width='850' height='900' src='" + linkViewInvoiceFull + "'></iframe>");
            //                var checkTerminal = (from ordPrd in db.Order_Products
            //                                     join prd in db.O_Product on ordPrd.ProductCode equals prd.Code
            //                                     join prdmodel in db.O_Product_Model on ordPrd.ModelCode equals prdmodel.ModelCode
            //                                     where ordPrd.OrderCode == currentTicket.OrderCode && (prd.ProductLineCode == "terminal" || prdmodel.MerchantOnboarding == true)
            //                                     select ordPrd).Any();
            //                if (checkTerminal && !db.T_SupportTicket.Any(t => t.CustomerCode == order.CustomerCode && t.TypeId == (long)UserContent.TICKET_TYPE.NuveiOnboarding))
            //                {
            //                    await NewTicketNuveiOnboarding(order.InvoiceNumber.ToString(), "<iframe  width='850' height='900' src='" + linkViewInvoice + "'></iframe>");
            //                }

            //            }
            //            else if (order.Status != currentTicket.StatusName
            //                && order.InvoiceNumber == null)
            //            {
            //                //update status estimates
            //                order.Status = currentTicket.StatusName;
            //                order.UpdatedAt = DateTime.UtcNow;
            //                order.UpdatedBy = fullname;
            //                order.UpdatedHistory = fullname + "(ticket#" + currentTicket.Id + ")";
            //                order.StatusHistory += "|" + order.Status + "  - Update by: " + fullname + " - At: " + DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt");
            //                //order.BundelStatus = "Packaging";
            //                db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //            }
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine("[CheckOrderStatus]" + ex.Message);
            //    }
            //}

            /// <summary>
            /// kiem tra dieu kien set complete 1 ticket deployment
            /// dieu kien: order phai duoc set complete, nghiep vu deployment dong goi san pham da duoc update complete/shipped thanh cong.
            /// duoc the hien trong order status.
            /// </summary>
            /// <param name="db"></param>
            /// <param name="currentTicket"></param>
            /// <returns></returns>
            //private static void CheckDeploymentStatus(WebDataModel db, T_SupportTicket currentTicket, string updateBy = "System")
            //{
            //    try
            //    {
            //        //statusid: 17 - deployment complete
            //        if (!string.IsNullOrWhiteSpace(currentTicket.OrderCode))
            //        {
            //            var order = db.O_Orders.Where(o => o.OrdersCode == currentTicket.OrderCode).FirstOrDefault();

            //            if (currentTicket.StatusId == (long)UserContent.TICKET_STATUS.Deployment_Complete)
            //            {
            //                if (order.BundelStatus != UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString())
            //                {
            //                    System.Web.Mvc.TempDataDictionary tempData = new System.Web.Mvc.TempDataDictionary();
            //                    tempData.Add("e", "[Ticket#" + currentTicket.Id + "]Packaging and product preparation process is not complete, please check your order again.The request was denied.");
            //                    currentTicket.StatusId = (long)AppLB.UserContent.TICKET_STATUS.Deployment_PrepNotReady;
            //                    currentTicket.StatusName = "Prep/Not Ready";
            //                    currentTicket.DateClosed = null;
            //                    db.Entry(currentTicket).State = System.Data.Entity.EntityState.Modified;
            //                    db.SaveChanges();
            //                }
            //            }
            //            else if (order.BundelStatus == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString())
            //            {
            //                //feedback
            //                DateTime date = InsertFeedback(db, currentTicket.Id, "Deployment complete", "Status: " + order.BundelStatus, order.BundelStatus);
            //                //
            //                currentTicket.StatusId = (long)AppLB.UserContent.TICKET_STATUS.Deployment_Complete;
            //                currentTicket.StatusName = "Complete";
            //                currentTicket.UpdateTicketHistory = (currentTicket.UpdateTicketHistory ?? "") + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - by " + updateBy + "|";
            //                currentTicket.DateClosed = DateTime.UtcNow;
            //                currentTicket.CloseByName = updateBy;
            //                db.Entry(currentTicket).State = System.Data.Entity.EntityState.Modified;
            //                db.SaveChanges();
            //            }
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        System.Diagnostics.Debug.WriteLine("[CheckDeploymentStatus]" + ex.Message);
            //    }
            //}

            /// <summary>
            /// kiem tra du kieu kien de tao deployment ticket
            /// dieu kien la ticket finance va onboarding phai complete
            /// </summary>
            /// <param name="db"></param>
            /// <param name="currentTicket"></param>
            /// <returns></returns>
        }
    }
}