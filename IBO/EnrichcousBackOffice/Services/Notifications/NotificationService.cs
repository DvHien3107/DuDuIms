using Antlr3.ST;
using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Extensions;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel.Nofitication;
using EnrichcousBackOffice.Models.CustomizeModel.Ticket;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace EnrichcousBackOffice.Services.Notifications
{
    public class NotificationService
    {

        #region Utilities
        private string GetUrlNotification(string Category, string Id)
        {
            using (WebDataModel db = new WebDataModel())
                switch (Category)
                {
                    case NotificationCategoryDefine.Ticket:
                        return "/ticket/detail/" + Id;
                    case NotificationCategoryDefine.TicketReminder:
                        return "/ticket/detail/" + Id;
                    case NotificationCategoryDefine.Order:
                        return "/Order/EstimatesDetail/" + Id;
                    case NotificationCategoryDefine.SalesLead:
                        var salesLead = db.C_SalesLead.Find(Id);
                        return "/SaleLead?tab=sales_lead&searchtext=" + salesLead.L_SalonName;
                    case NotificationCategoryDefine.Task:
                        long TaskId = long.Parse(Id);
                        var task = db.Ts_Task.FirstOrDefault(x => x.Id == TaskId) ?? new Ts_Task() { };
                        if (task.TicketId != null)
                        {
                            return "/ticket/detail/" + task.TicketId;
                        }
                        else
                        {
                            return "/tasksman/";
                        }
                    default:
                        return "";
                };
        }
        private string ConvertDescriptionNotification(P_Member member, string Id, string Content, string Category, int? UpdateId)
        {
            //https://csharp.hotexamples.com/examples/-/StringTemplate/-/php-stringtemplate-class-examples.html
            StringTemplate st = new StringTemplate(Content);
            using (WebDataModel db = new WebDataModel())
                switch (Category)
                {
                    case NotificationCategoryDefine.Ticket:
                        long TicketId = long.Parse(Id);
                        var ticket = db.T_SupportTicket.Find(TicketId);
                        st.SetAttribute("Member", member);
                        st.SetAttribute("Ticket", ticket);
                        if (ticket.UpdateAt != null && UpdateId != null)
                        {
                            var updateDes = new List<TicketUpdateModel>();
                            var updateDetail = db.T_TicketUpdateLog.Where(x => x.TicketId == ticket.Id && x.UpdateId == UpdateId).ToList();
                            if (updateDetail != null)
                            {

                                foreach (var u in updateDetail)
                                {
                                    var itemUpdateDes = new TicketUpdateModel();
                                    if (u.Name != "Label")
                                    {
                                        itemUpdateDes.Name = u.Name;
                                        itemUpdateDes.Value = u.NewValue;
                                    }
                                    else
                                    {

                                        var labelContent = new List<string>();
                                        if (!string.IsNullOrEmpty(u.NewValue))
                                        {
                                            foreach (var i in u.NewValue.Split(','))
                                            {
                                                if (!string.IsNullOrEmpty(i))
                                                {
                                                    var labelName = db.T_Tags.FirstOrDefault(x => x.Id == i);
                                                    if (labelName != null)
                                                    {
                                                        labelContent.Add(labelName.Name);
                                                    }

                                                }
                                            }
                                        }
                                        itemUpdateDes.Name = u.Name;
                                        itemUpdateDes.Value = string.Join(",", labelContent);

                                    }
                                    updateDes.Add(itemUpdateDes);
                                }

                                st.SetAttribute("UpdateDes", updateDes);


                            }
                        }
                        return st.ToString();
                    case NotificationCategoryDefine.TicketReminder:
                        long ticketId = long.Parse(Id);
                        var reminder = db.T_RemindersTicket.Where(x => x.TicketId == ticketId).Include(x => x.T_SupportTicket).FirstOrDefault();
                        st.SetAttribute("Reminder", reminder);
                        return st.ToString();
                    case NotificationCategoryDefine.Order:
                        long orderId = long.Parse(Id);
                        var order = db.O_Orders.Find(orderId);
                        st.SetAttribute("Order", order);
                        return st.ToString();
                    case NotificationCategoryDefine.SalesLead:
                        var salesLead = db.C_SalesLead.Find(Id);
                        st.SetAttribute("SalesLead", salesLead);
                        return st.ToString();
                    case NotificationCategoryDefine.Task:
                        long TaskId = long.Parse(Id);
                        var task = db.Ts_Task.Find(TaskId);
                        st.SetAttribute("Ts_Task", task);
                        return st.ToString();
                    default:
                        return "";
                };
        }
        #endregion

        #region Method
        public void Insert(P_Notification notification, List<string> MemberNumbers)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var ListNotificationMapping = new List<P_NotificationMapping>();
                foreach (var m in MemberNumbers)
                {
                    var notiMapping = new P_NotificationMapping();
                    notiMapping.CreateAt = DateTime.UtcNow;
                    notiMapping.IsSent = false;
                    notiMapping.MemberNumber = m;
                    notiMapping.IsView = false;
                    notiMapping.NotificationId = notification.Id;
                    ListNotificationMapping.Add(notiMapping);
                }
                notification.P_NotificationMapping = (ListNotificationMapping);
                db.P_Notification.Add(notification);
                db.SaveChanges();
                var hubContext = new BroadcastHub();
                hubContext.sendNotificationByMemberNumber(MemberNumbers, notification.Id, notification.EntityId, notification.TemplateId);
            }
        }
        public async Task<List<NotificationModel>> GetAll(string MemberNumber, DateTime? FromDate, DateTime? ToDate, string Category, bool? Read, string CreateBy, string SearchText, IColumn colSearch, int Start = 0, int Length = 10)
        {
            var db = new WebDataModel();
            var Notifications = from noti in db.P_Notification.Include(x => x.P_NotificationTemplate)
                                join map in db.P_NotificationMapping on noti.Id equals map.NotificationId
                                where map.MemberNumber == MemberNumber

                                group new { noti, map } by new { noti.EntityId, noti.TemplateId } into notis

                                select new
                                {
                                    notis.OrderByDescending(x => x.map.CreateAt).FirstOrDefault().noti,
                                    notis.OrderByDescending(x => x.map.CreateAt).FirstOrDefault().map
                                };

            if (FromDate != null)
            {
                var From = FromDate.Value.Date + new TimeSpan(0, 0, 0);
                Notifications = Notifications.Where(x => x.noti.CreateAt > From);
            }
            if (ToDate != null)
            {
                var To = ToDate.Value.AddHours(7).Date + new TimeSpan(23, 59, 59);
                Notifications = Notifications.Where(x => x.noti.CreateAt < To);
            }
            if (!string.IsNullOrEmpty(Category))
            {
                Notifications = Notifications.Where(x => x.noti.Category == Category);
            }
            if (!string.IsNullOrEmpty(CreateBy))
            {
                Notifications = Notifications.Where(x => x.noti.MemberNumber == CreateBy);
            }
            if (Read != null)
            {
                Notifications = Notifications.Where(x => x.map.IsView == Read);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                Notifications = Notifications.Where(x => x.noti.EntityId.Contains(SearchText.Trim()) || x.noti.EntityName.Contains(SearchText.Trim()) || x.noti.EntityCode.Contains(SearchText.Trim()));
            }
            var mNotifications = new List<NotificationModel>();
            switch (colSearch?.Name)
            {


                case "CreateBy":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        Notifications = Notifications.OrderBy(x => x.noti.CreateBy);
                    }
                    else
                    {
                        Notifications = Notifications.OrderByDescending(x => x.noti.CreateBy);
                    }
                    break;
                case "CreateAt":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        Notifications = Notifications.OrderBy(x => x.map.CreateAt);
                    }
                    else
                    {
                        Notifications = Notifications.OrderByDescending(x => x.map.CreateAt);
                    }
                    break;
                case "ViewTime":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        Notifications = Notifications.OrderBy(x => x.map.ViewTime);
                    }
                    else
                    {
                        Notifications = Notifications.OrderByDescending(x => x.map.ViewTime);
                    }
                    break;
                default:
                    Notifications = Notifications.OrderByDescending(x => x.map.CreateAt);
                    break;
            }
            foreach (var item in await Notifications.Skip(Start).Take(Length).ToListWithNoLockAsync())
            {
                var model = new NotificationModel();
                model.Id = item.noti.Id;
                model.Url = GetUrlNotification(item.noti.Category, item.noti.EntityId);
                var member = db.P_Member.Where(x => x.MemberNumber == item.noti.MemberNumber).FirstOrDefault();
                model.Description = ConvertDescriptionNotification(member ?? new P_Member(), item.noti.EntityId, item.noti.P_NotificationTemplate.Content, item.noti.Category, item.noti.UpdateId);
                //model.Image = string.IsNullOrEmpty(member.Picture) ? "/Upload/Img/" + member.Gender + ".png" : member.Picture;
                model.IsView = item.map.IsView ?? false;
                model.CreateAt = item.map.CreateAt.Value;
                model.TemplateId = item.noti.TemplateId;
                model.Category = item.noti.Category;
                model.ViewTime = item.map.ViewTime;
                model.CreateBy = item.noti.CreateBy;
                model.EntityId = item.noti.EntityId;
                mNotifications.Add(model);
            }
            return mNotifications;
        }
        public int CountAllNotification(string MemberNumber, DateTime? FromDate, DateTime? ToDate, string Category, bool? Read, string CreateBy, string SearchText, int Start = 0, int Length = 10)
        {
            var db = new WebDataModel();

            var Notifications = from noti in db.P_Notification.Include(x => x.P_NotificationTemplate)
                                join map in db.P_NotificationMapping on noti.Id equals map.NotificationId
                                where map.MemberNumber == MemberNumber
                                group new { noti, map } by new { noti.EntityId, noti.TemplateId } into notis
                                select new
                                {
                                    notis.OrderByDescending(x => x.map.CreateAt).FirstOrDefault().noti,
                                    notis.OrderByDescending(x => x.map.CreateAt).FirstOrDefault().map
                                };
            if (FromDate != null)
            {
                var From = FromDate.Value.Date + new TimeSpan(0, 0, 0);
                Notifications = Notifications.Where(x => x.noti.CreateAt > From);
            }
            if (ToDate != null)
            {
                var To = ToDate.Value.AddHours(7).Date + new TimeSpan(23, 59, 59);
                Notifications = Notifications.Where(x => x.noti.CreateAt < To);
            }
            if (!string.IsNullOrEmpty(Category))
            {
                Notifications = Notifications.Where(x => x.noti.Category == Category);
            }
            if (!string.IsNullOrEmpty(CreateBy))
            {
                Notifications = Notifications.Where(x => x.noti.MemberNumber == CreateBy);
            }

            if (!string.IsNullOrEmpty(SearchText))
            {
                Notifications = Notifications.Where(x => x.noti.EntityId.Contains(SearchText.Trim()) || x.noti.EntityName.Contains(SearchText.Trim()) || x.noti.EntityCode.Contains(SearchText.Trim()));
            }
            return Notifications.Count();
        }
        public async Task<List<NotificationModel>> GetNotiByMemberNumber(string MemberNumber, bool? Read, int Start = 0, int Length = 10)
        {
            var db = new WebDataModel();
            var query = @"WITH democte AS( select top 99999 t1.Id,t1.TemplateId, t1.EntityId, t1.MemberNumber, t2.CreateAt,t1.CreateBy,isnull(t2.IsView,0) as IsView,t2.ViewTime,t1.Category,t1.UpdateId from P_Notification t1
                join P_NotificationMapping t2 on t1.Id = t2.NotificationId
                and t2.MemberNumber = @MemberNumber
                left join P_NotificationTemplate t3 on t1.TemplateId = t3.Id
                order by t2.CreateAt desc
            ), cte AS
            (
               SELECT*, ROW_NUMBER() OVER (PARTITION BY TemplateId, EntityId ORDER BY CreateAt DESC) AS rn
               FROM democte
            )
            SELECT*
            FROM cte
            WHERE rn = 1";

            var parameters = new List<object>();
            parameters.Add(new SqlParameter("MemberNumber", MemberNumber));
            var Notifications = db.Database.SqlQuery<NotificationQueryModel>(query, parameters.ToArray()).ToList();

            if (Read != null)
            {
                Notifications = Notifications.Where(x => x.IsView == Read).ToList();
            }
            var todays = DateTime.Today;
            var yesterdays = todays.AddDays(-1);
            var today = Notifications.Where(c => c.CreateAt >= todays).OrderByDescending(c => c.CreateAt).FirstOrDefault()?.Id;
            var yesterday = Notifications.Where(c => yesterdays <= c.CreateAt && c.CreateAt < todays).OrderByDescending(c => c.CreateAt).FirstOrDefault()?.Id;
            var before = Notifications.Where(c => c.CreateAt < yesterdays).OrderByDescending(c => c.CreateAt).FirstOrDefault()?.Id;

            var mNotifications = new List<NotificationModel>();
            var members = db.P_Member.ToList();
            var Template = db.P_NotificationTemplate.ToList();
            var ListNotification = Notifications.OrderByDescending(x => x.CreateAt).Skip(Start).Take(Length).ToList();
            foreach (var item in ListNotification)
            {
                var model = new NotificationModel();
                model.Id = item.Id;
                model.Url = GetUrlNotification(item.Category, item.EntityId);
                var member = members.Where(x => x.MemberNumber == item.MemberNumber).FirstOrDefault();
                model.Description = ConvertDescriptionNotification(member ?? new P_Member(), item.EntityId, Template.FirstOrDefault(x => x.Id == item.TemplateId).Content, item.Category, item.UpdateId);
                model.IsView = item.IsView ?? false;
                model.CreateAt = item.CreateAt;
                model.TemplateId = item.TemplateId;
                model.EntityId = item.EntityId;
                model.Category = item.Category;
                model.ViewTime = item.ViewTime;
                model.CreateBy = item.CreateBy;
                model.GroupTitle = item.Id == today ? "Today" : (item.Id == yesterday ? "Yesterday" : (item.Id == before ? "Before" : ""));
                mNotifications.Add(model);
            }
            return mNotifications;
        }
        public NotificationModel GetNotificationById(int Id)
        {
            var db = new WebDataModel();
            var queryNoti = from noti in db.P_Notification.Include(x => x.P_NotificationTemplate)
                            join map in db.P_NotificationMapping on noti.Id equals map.NotificationId
                            where noti.Id == Id
                            orderby map.CreateAt descending

                            select new
                            {
                                noti,
                                map
                            };
            var mNotifications = queryNoti.FirstOrDefault();

            var model = new NotificationModel();
            model.Id = mNotifications.noti.Id;
            model.Url = GetUrlNotification(mNotifications.noti.Category, mNotifications.noti.EntityId);
            var member = db.P_Member.Where(x => x.MemberNumber == mNotifications.noti.MemberNumber).FirstOrDefault();
            model.Description = ConvertDescriptionNotification(member ?? new P_Member(), mNotifications.noti.EntityId, mNotifications.noti.P_NotificationTemplate.Content, mNotifications.noti.Category, mNotifications.noti.UpdateId);
            //model.Image = string.IsNullOrEmpty(member.Picture) ? "/Upload/Img/" + member.Gender + ".png" : member.Picture;
            model.IsView = mNotifications.map.IsView ?? false;
            model.TemplateId = mNotifications.noti.TemplateId;
            model.CreateAt = mNotifications.map.CreateAt.Value;
            model.Category = mNotifications.noti.Category;
            model.CreateBy = mNotifications.noti.CreateBy;
            model.EntityId = mNotifications.noti.EntityId;
            model.ViewTime = mNotifications.map.ViewTime;
            return model;

        }
        public void MarkReadById(int Id, string MemberNumber, bool IsRead)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var mappingNoti = db.P_NotificationMapping.Where(x => x.NotificationId == Id && x.MemberNumber == MemberNumber).FirstOrDefault();
                if (IsRead == true)
                {
                    if (mappingNoti.IsView != true)
                    {
                        mappingNoti.IsView = true;
                        mappingNoti.ViewTime = DateTime.UtcNow;
                    }
                }
                else
                {
                    mappingNoti.IsView = false;
                    mappingNoti.ViewTime = null;
                }
                db.SaveChanges();
            }
        }
        public void MarkAllRead(string MemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var mappingNoti = db.P_NotificationMapping.Where(x => x.MemberNumber == MemberNumber && x.IsView != true).ToList();
                mappingNoti.ForEach(x =>
                {
                    x.IsView = true;
                    x.ViewTime = DateTime.UtcNow;
                });
                db.SaveChanges();
            }
        }
        public int CountNotificationNotRead(string MemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var notificationsQuery = @"EXEC P_CountNotificationNotRead @MemberNumber ";
                var parameters = new List<object>();
                parameters.Add(new SqlParameter("MemberNumber", MemberNumber));
                var total = db.Database.SqlQuery<int>(notificationsQuery, parameters.ToArray()).First();

                return total;
            }
        }
        public int CountNotificationByMember(string MemberNumber, bool? read)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var notificationsQuery = "";
                var parameters = new List<object>();
                parameters.Add(new SqlParameter("MemberNumber", MemberNumber));
                if (read != null)
                {
                    notificationsQuery = @"WITH democte AS( select top 99999 t1.Id,t1.TemplateId,t2.CreateAt, t1.EntityId, t2.MemberNumber,t2.IsView from P_Notification t1
                        join P_NotificationMapping t2 on t1.Id = t2.NotificationId
                        and t2.MemberNumber = @MemberNumber 
                     
                        order by t2.CreateAt desc
                        ), cte AS
                        (
                           SELECT*, ROW_NUMBER() OVER (PARTITION BY TemplateId, EntityId ORDER BY CreateAt DESC) AS rn
                           FROM democte
                        )
                        SELECT Count(1)
                        FROM cte
                        WHERE rn = 1 and isnull(IsView,0)=@IsView";
                    parameters.Add(new SqlParameter("IsView", (read == true ? 1 : 0).ToString()));
                }
                else
                {
                    notificationsQuery = @"WITH democte AS( select top 99999 t1.Id,t1.TemplateId,t2.CreateAt, t1.EntityId, t2.MemberNumber,t2.IsView from P_Notification t1
                        join P_NotificationMapping t2 on t1.Id = t2.NotificationId
                        and t2.MemberNumber = @MemberNumber
                       
                        order by t2.CreateAt desc
                    ), cte AS
                    (
                       SELECT*, ROW_NUMBER() OVER (PARTITION BY TemplateId, EntityId ORDER BY CreateAt DESC) AS rn
                       FROM democte
                    )
                    SELECT Count(1)
                    FROM cte
                    WHERE rn = 1";
                }


                var total = db.Database.SqlQuery<int>(notificationsQuery, parameters.ToArray()).First();
                return total;
            }
        }


        #region Order Notification
        public void OrderAddNotification(string MemberNumber, string OrderId, string OrderCode, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                if (CreateByMemberNumber != MemberNumber)
                {
                    var noti = new P_Notification();
                    noti.Action = NotificationActionDefine.AddNew;
                    noti.Category = NotificationCategoryDefine.Order;
                    noti.EntityId = OrderId;
                    noti.EntityCode = OrderCode;
                    noti.MemberNumber = CreateByMemberNumber;
                    noti.MemberName = CreateByName;
                    noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.OrderAdd).FirstOrDefault().Id;
                    noti.CreateAt = DateTime.UtcNow;
                    noti.CreateBy = CreateByName;
                    this.Insert(noti, new List<string>() { MemberNumber });
                }
            }
        }
        public void OrderUpdateNotification(string MemberNumber, string OrderId, string OrderCode, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                if (MemberNumber != CreateByMemberNumber)
                {
                    var noti = new P_Notification();
                    noti.Action = NotificationActionDefine.Update;
                    noti.Category = NotificationCategoryDefine.Order;
                    noti.EntityId = OrderId;
                    noti.EntityCode = OrderCode;
                    noti.MemberNumber = CreateByMemberNumber;
                    noti.MemberName = CreateByName;
                    noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.OrderUpdate).FirstOrDefault().Id;
                    noti.CreateAt = DateTime.UtcNow;
                    noti.CreateBy = CreateByName;
                    this.Insert(noti, new List<string>() { MemberNumber });
                }
            }
        }
        public void OrderPaidOrCloseNotification(string MemberNumber, string OrderId, string OrderCode, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                if (MemberNumber != CreateByMemberNumber)
                {
                    var noti = new P_Notification();
                    noti.Action = NotificationActionDefine.Update;
                    noti.Category = NotificationCategoryDefine.Order;
                    noti.EntityId = OrderId;
                    noti.EntityCode = OrderCode;
                    noti.MemberNumber = CreateByMemberNumber;
                    noti.MemberName = CreateByName;
                    noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.OrderClose).FirstOrDefault().Id;
                    noti.CreateAt = DateTime.UtcNow;
                    noti.CreateBy = CreateByName;
                    this.Insert(noti, new List<string>() { MemberNumber });
                }
            }
        }
        #endregion
        #region SalesLead
        public void SalesLeadAssignNotification(string MemberNumber, string SalesLeadId, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                if (CreateByMemberNumber != MemberNumber)
                {
                    var noti = new P_Notification();
                    noti.Action = NotificationActionDefine.AddNew;
                    noti.Category = NotificationCategoryDefine.SalesLead;
                    noti.EntityId = SalesLeadId;
                    noti.MemberNumber = CreateByMemberNumber;
                    noti.MemberName = CreateByName;
                    noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.SalesLeadAssign).FirstOrDefault().Id;
                    noti.CreateAt = DateTime.UtcNow;
                    noti.CreateBy = CreateByName;
                    this.Insert(noti, new List<string>() { MemberNumber });
                }
            }

        }
        #endregion
        #region Ticket
        public void TicketMentionsNotification(List<string> MemberNumbers, string TicketId, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var noti = new P_Notification();
                noti.Action = NotificationActionDefine.AddNew;
                noti.Category = NotificationCategoryDefine.Ticket;
                noti.EntityId = TicketId;
                noti.MemberNumber = CreateByMemberNumber;
                noti.MemberName = CreateByName;
                noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketTag).FirstOrDefault().Id;
                noti.CreateAt = DateTime.UtcNow;
                noti.CreateBy = CreateByName;
                this.Insert(noti, MemberNumbers);
            }
        }

        public void TicketReminderNotification(List<string> MemberNumbers, string ReminderTicketId, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var notificationTemplate = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketReminders).FirstOrDefault();
                if (notificationTemplate != null)
                {
                    var noti = new P_Notification();
                    noti.Action = NotificationActionDefine.AddNew;
                    noti.Category = NotificationCategoryDefine.TicketReminder;
                    noti.EntityId = ReminderTicketId;
                    noti.MemberNumber = CreateByMemberNumber;
                    noti.MemberName = CreateByName;


                    noti.TemplateId = notificationTemplate.Id;
                    noti.CreateAt = DateTime.UtcNow;
                    noti.CreateBy = CreateByName;
                    this.Insert(noti, MemberNumbers);
                }
               
            }
        }

        public void TicketNewTaskNotification(List<string> MemberNumbers, string TaskId, string TaskName, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var noti = new P_Notification();
                noti.Action = NotificationActionDefine.AddNew;
                noti.Category = NotificationCategoryDefine.Task;
                noti.EntityId = TaskId;
                noti.EntityName = TaskName;
                noti.MemberNumber = CreateByMemberNumber;
                noti.MemberName = CreateByName;
                noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.Task_NewTaskTicket).FirstOrDefault().Id;
                noti.CreateAt = DateTime.UtcNow;
                noti.CreateBy = CreateByName;
                this.Insert(noti, MemberNumbers);
            }
        }

        public void TicketNewDeadlineNotification(List<string> MemberNumbers, string TaskId, string TaskName, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var noti = new P_Notification();
                noti.Action = NotificationActionDefine.AddNew;
                noti.Category = NotificationCategoryDefine.Ticket;
                noti.EntityId = TaskId;
                noti.EntityName = TaskName;
                noti.MemberNumber = CreateByMemberNumber;
                noti.MemberName = CreateByName;
                noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.TicketDeadline).FirstOrDefault().Id;
                noti.CreateAt = DateTime.UtcNow;
                noti.CreateBy = CreateByName;
                this.Insert(noti, MemberNumbers);
            }
        }

        public void TicketUpdateTaskNotification(List<string> MemberNumbers, string TaskId, string TaskName, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var noti = new P_Notification();
                noti.Action = NotificationActionDefine.Update;
                noti.Category = NotificationCategoryDefine.Task;
                noti.EntityId = TaskId;
                noti.MemberNumber = CreateByMemberNumber;
                noti.MemberName = CreateByName;
                noti.EntityName = TaskName;
                noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.Task_UpdateTaskTicket).FirstOrDefault().Id;
                noti.CreateAt = DateTime.UtcNow;
                noti.CreateBy = CreateByName;
                this.Insert(noti, MemberNumbers);
            }
        }
        public void NewTaskNotification(List<string> MemberNumbers, string TaskId, string TaskName, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var noti = new P_Notification();
                noti.Action = NotificationActionDefine.AddNew;
                noti.Category = NotificationCategoryDefine.Task;
                noti.EntityId = TaskId;
                noti.EntityId = TaskName;
                noti.MemberNumber = CreateByMemberNumber;
                noti.MemberName = CreateByName;
                noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.Task_NewTaskTicket).FirstOrDefault().Id;
                noti.CreateAt = DateTime.UtcNow;
                noti.CreateBy = CreateByName;
                this.Insert(noti, MemberNumbers);
            }
        }
        public void UpdateTaskNotification(List<string> MemberNumbers, string TaskId, string TaskName, string CreateByName, string CreateByMemberNumber)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var noti = new P_Notification();
                noti.Action = NotificationActionDefine.Update;
                noti.Category = NotificationCategoryDefine.Task;
                noti.EntityId = TaskId;
                noti.EntityId = TaskName;
                noti.MemberNumber = CreateByMemberNumber;
                noti.MemberName = CreateByName;
                noti.TemplateId = db.P_NotificationTemplate.Where(x => x.Name == NotificationTemplateDefine.Task_UpdateTaskTicket).FirstOrDefault().Id;
                noti.CreateAt = DateTime.UtcNow;
                noti.CreateBy = CreateByName;
                this.Insert(noti, MemberNumbers);
            }
        }
        #endregion
        #endregion
    }
}