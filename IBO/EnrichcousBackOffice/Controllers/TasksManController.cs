using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.CustomizeModel.Task;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.Services.Tickets;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class TasksManController : UploadController
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        const string NotificationPage = "NotificationPage";
        const string TaskPage = "TaskPage";
        //const string SUB_TASK = "List sub task";

        // GET: TasksMan
        //public ActionResult Index(string key)
        //{
        //    WebDataModel db = new WebDataModel();
        //    var list_task = new List<Ts_Task>();
        //    ViewBag.Key = key;

        //    if (key == "assignedtask")
        //    {
        //        //add to view history top button
        //        UserContent.TabHistory = "Assigned task" + "|" + Request.Url.PathAndQuery;

        //        var list_task_assigned = db.Ts_Task.Where(t => t.AssignedToMemberNumber.Contains(cMem.MemberNumber) && t.ParentTaskId == null).ToList();
        //        if (list_task_assigned != null && list_task_assigned.Count() > 0)
        //        {

        //            list_task.AddRange(list_task_assigned);

        //        }


        //    }
        //    else if (key == "completetask")
        //    {
        //        //add to view history top button
        //        UserContent.TabHistory = "Completed task" + "|" + Request.Url.PathAndQuery;

        //        var list_task_assigned = db.Ts_Task.Where(t => (t.AssignedToMemberNumber.Contains(cMem.MemberNumber) || t.CreateByMemberNumber == cMem.MemberNumber) && t.ParentTaskId == null && t.Complete == true).ToList();
        //        if (list_task_assigned != null && list_task_assigned.Count() > 0)
        //        {
        //            list_task.AddRange(list_task_assigned);
        //        }

        //        #region Inactive
        //        //var list_subtask_assigned = db.Ts_Task.Where(t => t.AssignedToMemberNumber == cMem.MemberNumber && t.ParentTaskId != null && t.Complete == true).ToList();
        //        //if (list_subtask_assigned != null && list_subtask_assigned.Count() > 0)
        //        //{
        //        //    foreach (var subtask in list_subtask_assigned)
        //        //    {
        //        //        if (list_task.Any(t => t.Id == subtask.ParentTaskId) == false)
        //        //        {
        //        //            var task = db.Ts_Task.Find(subtask.ParentTaskId);
        //        //            list_task.Add(task);
        //        //        }
        //        //    }
        //        //}
        //        #endregion

        //    }
        //    else
        //    {
        //        //add to view history top button
        //        UserContent.TabHistory = "task" + "|" + Request.Url.PathAndQuery;

        //        //my task
        //        list_task = db.Ts_Task.Where(t => t.CreateByMemberNumber == cMem.MemberNumber && t.ParentTaskId == null).ToList();
        //    }



        //    return View(list_task);
        //}
        public ActionResult Index(string Page)
        {
            switch (Page)
            {
                case NotificationPage:
                    ViewBag.Page = NotificationPage;
                    break;
                default:

                    ViewBag.Page = TaskPage;
                    break;
            }
            return View();
        }
        [ChildActionOnly]
        public ActionResult TaskPageContent(string Page)
        {
            if (Page == NotificationPage)
            {
                WebDataModel db = new WebDataModel();
                ViewBag.AllMember = db.P_Member.Where(x => x.Active == true).ToList();
                return PartialView("_NotificationContent");
            }
            else
            {
                WebDataModel db = new WebDataModel();
                var list_task = new List<Ts_Task>();
                string key = "assignedtask";
                ViewBag.Key = key;

                string[] mem_dept = cMem.DepartmentId?.Split(new char[] { ',' }) ?? new string[] { };
                ViewBag.ListMember = db.P_Member.Where(delegate (P_Member m)
                {
                    bool dept = mem_dept.Any(d => !string.IsNullOrEmpty(d) && m.DepartmentId?.Contains(d) == true);
                    return dept && (m.Active ?? false);
                }).Select(m => new MemberSelect_View { MemberNumber = m.MemberNumber, Name = m.FullName }).ToList();
                if (key == "assignedtask")
                {
                    //add to view history top button
                    UserContent.TabHistory = "Assigned task" + "|" + Request.Url.PathAndQuery;

                    var list_task_assigned = db.Ts_Task.Where(t => t.AssignedToMemberNumber.Contains(cMem.MemberNumber) && t.ParentTaskId == null).ToList();
                    if (list_task_assigned != null && list_task_assigned.Count() > 0)
                    {

                        list_task.AddRange(list_task_assigned);

                    }


                }
                else if (key == "completetask")
                {
                    //add to view history top button
                    UserContent.TabHistory = "Completed task" + "|" + Request.Url.PathAndQuery;

                    var list_task_assigned = db.Ts_Task.Where(t => (t.AssignedToMemberNumber.Contains(cMem.MemberNumber) || t.CreateByMemberNumber == cMem.MemberNumber) && t.ParentTaskId == null && t.Complete == true).ToList();
                    if (list_task_assigned != null && list_task_assigned.Count() > 0)
                    {
                        list_task.AddRange(list_task_assigned);
                    }

                    #region Inactive
                    //var list_subtask_assigned = db.Ts_Task.Where(t => t.AssignedToMemberNumber == cMem.MemberNumber && t.ParentTaskId != null && t.Complete == true).ToList();
                    //if (list_subtask_assigned != null && list_subtask_assigned.Count() > 0)
                    //{
                    //    foreach (var subtask in list_subtask_assigned)
                    //    {
                    //        if (list_task.Any(t => t.Id == subtask.ParentTaskId) == false)
                    //        {
                    //            var task = db.Ts_Task.Find(subtask.ParentTaskId);
                    //            list_task.Add(task);
                    //        }
                    //    }
                    //}
                    #endregion

                }
                else
                {
                    //add to view history top button
                    UserContent.TabHistory = "task" + "|" + Request.Url.PathAndQuery;

                    //my task
                    list_task = db.Ts_Task.Where(t => t.CreateByMemberNumber == cMem.MemberNumber && t.ParentTaskId == null).ToList();
                }

                return PartialView("_TaskContent", list_task);
            }
        }

        public async Task<ActionResult> LoadNotification(IDataTablesRequest dataTablesRequest, DateTime? FromDate, DateTime? ToDate, string Category, bool? Read, string MemberNumber, string SearchText)
        {
            var _notificationService = new NotificationService();
            var totalRecord = _notificationService.CountAllNotification(cMem.MemberNumber, FromDate, ToDate, Category, Read, MemberNumber, SearchText, dataTablesRequest.Start, dataTablesRequest.Length);
            var notifications = await _notificationService.GetAll(cMem.MemberNumber, FromDate, ToDate, Category, Read, MemberNumber, SearchText, dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault(), dataTablesRequest.Start, dataTablesRequest.Length);
            var data = notifications.Select(x => new
            {
                x.Id,
                x.Category,
                x.Description,
                x.IsView,
                x.Url,
                ViewTime = x.ViewTime.HasValue ? x.ViewTime.Value.ToString("MMM dd, yyyy hh:mm tt") : "",
                CreateAt = x.CreateAt.ToString("MMM dd, yyyy hh:mm tt"),
                x.CreateBy
            });
            return Json(new
            {
                data = data,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult LoadTask(IDataTablesRequest dataTablesRequest, string Tab, DateTime? FromDate, DateTime? ToDate, string AssignedTo, string SearchText)
        {
            WebDataModel db = new WebDataModel();
            var query = from t in db.Ts_Task select t;
            int totalOpen = query.Where(t => (t.CreateByMemberNumber == cMem.MemberNumber || t.AssignedToMemberNumber.Contains(cMem.MemberNumber)) && t.ParentTaskId == null && t.Complete != true).Count();
            int totalComplete = query.Where(t => (t.CreateByMemberNumber == cMem.MemberNumber || t.AssignedToMemberNumber.Contains(cMem.MemberNumber)) && t.ParentTaskId == null && t.Complete == true).Count();
            if (!string.IsNullOrEmpty(Tab))
            {
                if (Tab == "open")
                {
                    query = query.Where(t => (t.CreateByMemberNumber == cMem.MemberNumber || t.AssignedToMemberNumber.Contains(cMem.MemberNumber)) && t.ParentTaskId == null && t.Complete != true);
                }
                else if (Tab == "completed")
                {
                    query = query.Where(t => (t.CreateByMemberNumber == cMem.MemberNumber || t.AssignedToMemberNumber.Contains(cMem.MemberNumber)) && t.ParentTaskId == null && t.Complete == true);
                }
            }
            if (FromDate != null)
            {
                var from = FromDate.Value.IMSToUTCDateTime();
                query = query.Where(x => x.CreateAt >= from);
            }
            if (ToDate != null)
            {
                var to = ToDate.Value.IMSToUTCDateTime();
                query = query.Where(x => x.CreateAt <= to);
            }
            if (!string.IsNullOrEmpty(AssignedTo))
            {
                query = query.Where(x => x.AssignedToMemberNumber.Contains(AssignedTo));
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.Name.Contains(SearchText) || SqlFunctions.StringConvert((double)x.Id).Contains(SearchText));
            }
            int totalRecord = query.Count();
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch?.Name)
            {
                case "OpenDate":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.CreateAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.CreateAt);
                    }
                    break;
                case "Name":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Name);
                    }
                    break;
                case "AssignedToMemberName":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.AssignedToMemberName);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.AssignedToMemberName);
                    }
                    break;
                case "DueDate":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.DueDate);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.DueDate);
                    }
                    break;
                case "UpdateBy":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.UpdateAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.UpdateAt);
                    }
                    break;

                default:
                    query = query.OrderByDescending(x => x.CreateAt);
                    break;
            };
            var data = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x =>
            {
                var model = new TaskListModel();
                model.Id = x.Id;
                model.ReminderWeeklyAt = x.ReminderWeeklyAt;
                model.CreateAt = x.CreateAt;
                model.OpenDate = x.CreateAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt");
                model.DueDate = string.Format("{0:r}", x.DueDate);
                model.ReminderMonthlyAt = x.ReminderMonthlyAt;
                model.Complete = x.Complete;
                model.CompletedDate = x.CompletedDate;
                model.TicketId = x.TicketId;
                model.TicketName = x.TicketName;
                model.UpdateBy = x.UpdateBy;
                model.Name = x.Name;
                model.AssignedToMemberName = x.AssignedToMemberName;
                var totalSubtasksQuery = db.Ts_Task.Where(t => t.ParentTaskId == x.Id);
                var TotalSubtask = totalSubtasksQuery.Count();
                model.TotalSubtask = TotalSubtask != 0 ? TotalSubtask : 1;
                if (model.Complete == true)
                {
                    model.SubtaskComplete = model.TotalSubtask;
                }
                else
                {
                    model.SubtaskComplete = totalSubtasksQuery.Where(y => y.Complete == true).Count();
                }
                return model;
            });
            return Json(new
            {
                data = data,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                totalOpen,
                totalComplete
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Task Detail
        /// </summary>
        /// <param name="Id">Task Id</param>
        /// <param name="action"></param>
        /// <returns></returns>
        public ActionResult Detail(long? Id, bool? action)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var task = db.Ts_Task.Find(Id);
                if (task != null)
                {
                    var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
                    if (task.AssignedToMemberNumber == cMem.MemberNumber || task.CreateByMemberNumber == cMem.MemberNumber
                        || list_subtask.Any(st => st.AssignedToMemberNumber == cMem.MemberNumber) == true || action == true)
                    {
                        ViewBag.ListSubtask = list_subtask;

                        string subtask_complete = list_subtask.Where(st => st.Complete == true).Count() + "/" + list_subtask.Count() + " done";

                        int percent_complete = 0;
                        if (list_subtask.Count() > 0)
                        {
                            percent_complete = (int)((float)list_subtask.Where(st => st.Complete == true).Count() / list_subtask.Count() * 100);
                        }
                        else
                        {
                            subtask_complete = task.Complete == true ? "1/1 done" : "0/1 done";
                            percent_complete = task.Complete == true ? 100 : 0;
                        }

                        ViewBag.subtask_complete = subtask_complete;
                        ViewBag.percent_complete = percent_complete;

                        return View(task);
                    }
                    else
                    {
                        return Redirect("/home/forbidden");
                    }
                }
                else
                {
                    throw new Exception("Task not found.");
                }
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                return RedirectToAction("index");
            }
        }

        /// <summary>
        /// Delete Task
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Delete(long? Id)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var task = db.Ts_Task.Find(Id);
                if (task != null)
                {
                    var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == Id).ToList();
                    if (list_subtask != null && list_subtask.Count() > 0)
                    {
                        foreach (var item in list_subtask)
                        {
                            db.Ts_Task.Remove(item);
                        }
                    }

                    db.Ts_Task.Remove(task);
                    var FeedbackContent = "<span>delete task: <b>" + task.Name + "</b> success</span>";
                    var ticketFeedback = new T_TicketFeedback()
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                        TicketId = task.TicketId,
                        DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                        FeedbackTitle = "@" + cMem.FullName + " deleted the task",
                        Feedback = FeedbackContent,
                        CreateByName = "System",
                        CreateAt = DateTime.UtcNow,
                        DisableWriteLog = true
                    };
                    db.T_TicketFeedback.Add(ticketFeedback);

                    var ticket = db.T_SupportTicket.Where(x => x.Id == task.TicketId).FirstOrDefault();
                    if (!string.IsNullOrEmpty(ticket.CustomerCode))
                    {
                        var salesLead = db.C_SalesLead.Where(x => x.CustomerCode == ticket.CustomerCode).FirstOrDefault();
                        if (salesLead != null)
                        {
                            var log = new Calendar_Event()
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "Ticket #" + ticket.Id + ": task has been deleted",
                                Description = FeedbackContent,
                                CreateAt = DateTime.UtcNow,
                                CreateBy = cMem.FullName,
                                Type = "Log",
                                GMT = "+00:00",
                                TimeZone = "(UTC) Coordinated Universal Time",
                                StartEvent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "+00:00",
                                Done = 1,
                                SalesLeadId = salesLead.Id,
                                SalesLeadName = salesLead.L_SalonName
                            };
                            db.Calendar_Event.Add(log);
                        }

                    }
                    db.SaveChanges();

                    return Json(new { status = true, message = "delete task success" });
                }
                else
                {
                    return Json(new { status = false, message = "Delete fail! Task not found" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }


        }

        #region Task Popup

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action">action = true: co quyen thao tac voi tat ca cac task</param>
        /// <param name="id">Task Id</param>
        /// <param name="update">[update = true]-la Add New or Edit --- [update = false]-la View Detail</param>
        /// <param name="tk_id">Ticket Id</param>
        /// <returns></returns>
        public ActionResult GetInfoTask(bool? action, long? id, bool? update, long? tk_id, string tk_name)
        {
            try
            {
                //Session.Remove(SUB_TASK);
                WebDataModel db = new WebDataModel();
                var task = new Ts_Task();
                var list_subtask = new List<Ts_Task_session>();

                if (id > 0) //Edit task or detail task
                {
                    task = db.Ts_Task.Find(id);

                    if (task != null)
                    {
                        //chi co nguoi nao duoc assigned hoac la nguoi tao task hoac la VIP moi edit va xem duoc task
                        if (task.AssignedToMemberNumber?.Contains(cMem.MemberNumber) == false && task.CreateByMemberNumber != cMem.MemberNumber && action != true)
                        {
                            //throw new Exception("Access denied");
                            ViewBag.err = "Access denied";
                        }

                        if (db.Ts_Task.Any(t => t.ParentTaskId == id) == true)
                        {

                            list_subtask = (from s_task in db.Ts_Task.Where(t => t.ParentTaskId == id)
                                            join data in db.UploadMoreFiles.Where(u => u.TableName == "Ts_Task")
                                                on s_task.Id equals data.TableId
                                                into task_g
                                            from y_d in task_g.DefaultIfEmpty()
                                            group y_d by s_task into grouped
                                            select new Ts_Task_session
                                            {
                                                Id = grouped.Key.Id,
                                                Name = grouped.Key.Name,
                                                Complete = grouped.Key.Complete,
                                                Files = grouped.Where(g => g.UploadId != null).ToList(),
                                            }).ToList();
                            //Session[SUB_TASK] = list_subtask;
                        }

                        if (task.TicketId > 0 && string.IsNullOrEmpty(task.TicketName) == false)
                        {
                            ViewBag.Ticket = "#" + task.TicketId + " - " + task.TicketName;
                        }
                        ViewBag.TaskFiles = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == task.Id).ToList();
                    }
                    else
                    {
                        //throw new Exception("Task does not exist.");
                        ViewBag.err = "Task not found";
                    }
                }

                if (tk_id > 0)
                {
                    var ticket = db.T_SupportTicket.Where(x => x.Id == tk_id).FirstOrDefault();
                    ViewBag.Ticket = "#" + ticket.Id + " - " + ticket.Name;
                }

                ViewBag.ListSubTask = list_subtask;

                //member co cung department
                string[] mem_dept = cMem.DepartmentId?.Split(new char[] { ',' }) ?? new string[] { };
                ViewBag.ListMember = db.P_Member.Where(x=>x.Active==true && x.SiteId==cMem.SiteId).Select(m => new MemberSelect_View { MemberNumber = m.MemberNumber, Name = m.FullName }).ToList();

                // List<long> listGroup = ViewControler.TicketViewController.GetGroupByMember(db, cMem.MemberNumber);
                if (task.TicketId != null)
                {
                    ViewBag.ListTicket = db.T_SupportTicket.Where(tk => tk.AssignedToMemberNumber.Contains(cMem.MemberNumber) == true
                                              || tk.ReassignedToMemberNumber.Contains(cMem.MemberNumber) == true).ToList();
                }
                else
                {
                    ViewBag.ListTicket = db.T_SupportTicket.Where(tk => tk.AssignedToMemberNumber.Contains(cMem.MemberNumber) == true
                                          || tk.ReassignedToMemberNumber.Contains(cMem.MemberNumber) == true
                                          // || (tk.GroupID > 0 && listGroup.Contains(tk.GroupID ?? 0) == true)
                                          // || (tk.EscaladeToGroupId > 0 && listGroup.Contains(tk.EscaladeToGroupId ?? 0) == true))&&(tk.DateClosed==null)
                                          ).ToList();
                }

                //[update = true]-la Add New or Edit --- [update = false]-la View Detail

                ViewData["ts_update"] = update;
                if (tk_id > 0)
                {
                    task.TicketId = tk_id;
                    var Reminder = new T_RemindersTicket();
                    if (id > 0)
                    {
                        var reminderTicket = db.T_RemindersTicket.Where(x => x.TicketId == tk_id && x.TaskId == id).FirstOrDefault();
                        if (reminderTicket != null)
                        {
                            Reminder = reminderTicket;
                        }
                    }
                    ViewBag.Reminders = Reminder;
                }

                return PartialView("_TaskPopupPartial", task);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Save Task
        /// </summary>
        /// <param name="Task_Model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> Save(Ts_Task Task_Model, List<string> subtask_id, string GMT, T_RemindersTicket reminderModel)
        {
            WebDataModel db = new WebDataModel();
            using (var Trans = db.Database.BeginTransaction())
            {
                try
                {
                    var task_id = long.Parse(Request["ts_id"]);
                    var _id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                    string message = "";
                    if (task_id > 0) //Update task
                    {
                        var task = db.Ts_Task.Find(task_id);
                        var oldTask = task;
                        if (task != null)
                        {
                            var old_task_assign = task.AssignedToMemberNumber ?? "";
                            var old_task_name = task.AssignedToMemberName ?? "";
                            var old_task_note = task.Note ?? "";
                            var old_task_complete = task.Complete;
                            #region Update Task

                            if (string.IsNullOrEmpty(Request["ticket"]) == false)
                            {
                                var ticket = Request["ticket"].Split('#');
                                var ticket_id = ticket[1].Split('-')[0].Trim();
                                var ticket_name = ticket[1].Split('-')[1].Trim();
                                task.TicketId = long.Parse(ticket_id);
                                task.TicketName = db.T_SupportTicket.Find(task.TicketId)?.Name;
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(Request["ticket_id"]) == false)
                                {
                                    var ticket_id = long.Parse(Request["ticket_id"]);
                                    task.TicketId = ticket_id;
                                    task.TicketName = db.T_SupportTicket.Find(ticket_id)?.Name;
                                }
                                else
                                {
                                    task.TicketId = null;
                                    task.TicketName = null;
                                }
                            }

                            if (string.IsNullOrEmpty(Request["task_assign"]) == false)
                            {
                                var memNumbers = Request["task_assign"];
                                task.AssignedToMemberNumber = memNumbers;
                                List<string> name = new List<string>();
                                foreach (var number in memNumbers.Split(','))
                                {
                                    name.Add(db.P_Member.Where(m => m.MemberNumber == number).FirstOrDefault()?.FullName + "(#" + number + ")");
                                }
                                task.AssignedToMemberName = string.Join(",", name);
                            }

                            if (string.IsNullOrEmpty(Request["DueDate"]) == false)
                            {
                                task.DueDate = DateTime.Parse(Request["DueDate"]);
                            }
                            if (string.IsNullOrEmpty(Request["chk_weekly"]) == true)
                            {
                                task.ReminderWeeklyAt = null;
                            }
                            else
                            {
                                task.ReminderWeeklyAt = int.Parse(Request["r_weekly"]);
                            }
                            if (string.IsNullOrEmpty(Request["chk_monthly"]) == true)
                            {
                                task.ReminderMonthlyAt = null;
                            }
                            else
                            {
                                task.ReminderMonthlyAt = int.Parse(Request["r_monthly"]);
                            }

                            task.Name = Task_Model.Name;
                            task.Description = Request.Unvalidated["task_desc"];
                            task.Note = Task_Model.Note;
                            task.Complete = Request["complete_task"] == "1" ? true : false;
                            task.UpdateAt = DateTime.UtcNow;
                            if (task.Complete == true)
                            {
                                if (task.CompletedDate == null)
                                {
                                    task.CompletedDate = DateTime.UtcNow;
                                }
                            }
                            else
                            {
                                task.CompletedDate = null;
                            }
                            task.UpdateBy =  cMem.FullName;

                            db.Entry(task).State = System.Data.Entity.EntityState.Modified;
                            Task_Model.Id = task.Id;
                            //update file
                            var taskfiles = Request["upload_id_" + task.Id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
                            //
                            var task_old_files = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == task.Id).ToList();
                            var remove_files = task_old_files.Where(s => !taskfiles.Contains(s.UploadId.ToString())).ToList();
                            //remove files
                            MoreFileDelete(remove_files);
                            db.UploadMoreFiles.RemoveRange(remove_files);
                            //add new files
                            foreach (var s in taskfiles.Where(i => !task_old_files.Any(of => of.UploadId.ToString() == i)).ToList())
                            {
                                db.UploadMoreFiles.Add(new UploadMoreFile
                                {
                                    TableId = task.Id,
                                    TableName = "Ts_Task",
                                    FileName = UploadAttachFile("/Upload/ts_task/" + task.Id.ToString() + "/", s, null, null, out string n, true),
                                    UploadId = _id++,
                                });
                            }

                            var newSubtasks = new List<Ts_Task>();
                            //update sub task
                            if (subtask_id != null)
                            {
                                var old_subtask = db.Ts_Task.Where(s => s.ParentTaskId == task_id).ToList();
                                var update_subtask = old_subtask.Where(s => subtask_id.Contains(s.Id.ToString())).ToList();
                                var remove_subtask = old_subtask.Where(s => !subtask_id.Contains(s.Id.ToString())).ToList();
                                var remove_subtask_id = remove_subtask.Select(r => r.Id).ToList();
                                //remove tasks
                                db.Ts_Task.RemoveRange(remove_subtask);
                                var _remove_files = db.UploadMoreFiles.Where(u => u.TableName == "Ts_Task" && remove_subtask_id.Contains(u.UploadId)).ToList();
                                MoreFileDelete(_remove_files);
                                db.UploadMoreFiles.RemoveRange(_remove_files);
                                //update tasks
                                foreach (var subtask in update_subtask)
                                {
                                    subtask.Name = Request["sub_name_" + subtask.Id];
                                    subtask.Complete = Request["chk_" + subtask.Id] == "on";
                                    subtask.UpdateAt = DateTime.UtcNow;
                                    subtask.UpdateBy = cMem.FullName;
                                    db.Entry(subtask).State = System.Data.Entity.EntityState.Modified;
                                    var files = Request["upload_id_" + subtask.Id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
                                    //
                                    var old_files = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == subtask.Id).ToList();
                                    var s_remove_files = old_files.Where(s => !files.Contains(s.UploadId.ToString())).ToList();
                                    //remove files
                                    MoreFileDelete(s_remove_files);
                                    db.UploadMoreFiles.RemoveRange(s_remove_files);
                                    //add new files
                                    foreach (var s in files.Where(i => !old_files.Any(of => of.UploadId.ToString() == i)).ToList())
                                    {
                                        db.UploadMoreFiles.Add(new UploadMoreFile
                                        {
                                            TableId = subtask.Id,
                                            TableName = "Ts_Task",
                                            FileName = UploadAttachFile("/Upload/ts_task/sub_task/" + subtask.Id.ToString() + "/", s, null, null, out string n, true),
                                            UploadId = _id++,
                                        });
                                    }
                                }
                                //add new subtasks
                             
                                foreach (var sub_id in subtask_id.Where(i => !update_subtask.Any(of => of.Id.ToString() == i)).ToList())
                                {
                                    var subtask = new Ts_Task
                                    {
                                        Name = Request["sub_name_" + sub_id],
                                        Complete = Request["chk_" + sub_id] == "on",
                                    };
                                    if (string.IsNullOrEmpty(subtask.Name))
                                    {
                                        continue;
                                    }
                                    subtask.Id = long.Parse(sub_id);
                                    subtask.TicketId = task.TicketId;
                                    subtask.TicketName = task.TicketName;
                                    subtask.ParentTaskId = task.Id;
                                    subtask.ParentTaskName = task.Name;
                                    subtask.CreateBy = cMem.FullName;
                                    subtask.CreateByMemberNumber = cMem.MemberNumber;
                                    subtask.CreateAt = DateTime.UtcNow;
                                    subtask.UpdateAt = DateTime.UtcNow;
                                    subtask.UpdateBy = cMem.FullName;
                                    newSubtasks.Add(subtask);
                                    var files = Request["upload_id_" + sub_id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
                                    foreach (var s in files)
                                    {
                                        db.UploadMoreFiles.Add(new UploadMoreFile
                                        {
                                            TableId = subtask.Id,
                                            TableName = "Ts_Task",
                                            FileName = UploadAttachFile("/Upload/ts_task/sub_task/" + subtask.Id.ToString() + "/", s, null, null, out string n, true),
                                            UploadId = _id++,
                                        });
                                    }
                                }
                                if (newSubtasks.Count > 0)
                                {
                                    db.Ts_Task.AddRange(newSubtasks);
                                }
                            }
                            if (task.TicketId != null)
                            {
                                //var lastUpdateIdTicket = (db.T_TicketUpdateLog.Where(x => x.TicketId == task.TicketId).GroupBy(x => x.UpdateId).OrderBy(x => x.Key).FirstOrDefault().Key + 1) ?? 1;
                                //var updateLog = new T_TicketUpdateLog();
                                //updateLog.UpdateId = lastUpdateIdTicket;
                                //updateLog.TicketId = task.TicketId;
                                //updateLog.Name = "TaskInsert";
                                //updateLog.Description = "Have a new task";

                                //updateLog.NewValue = task.Name;
                                //updateLog.CreateAt = DateTime.UtcNow;
                                //updateLog.CreateBy = cMem.FullName;
                                // db.T_TicketUpdateLog.Add(updateLog);

                                var FeedbackContent = "";
                              
                                if (task.Name != old_task_name)
                                {
                                    FeedbackContent += "- Name: " + task.Name + "</br>";
                                }

                                if (task.AssignedToMemberNumber != old_task_assign)
                                {
                                    FeedbackContent += "- Assinged: " + task.AssignedToMemberName + "</br>";
                                }

                                if (oldTask.Note != old_task_note)
                                {
                                    FeedbackContent += "- Note: " + task.Note + "</br>";
                                }

                                if (oldTask.Complete != old_task_complete)
                                {
                                    if (task.Complete == true)
                                    {
                                        FeedbackContent += "- Task Complete: Task has been completed <br/>";
                                    }
                                    else
                                    {
                                        FeedbackContent += "- Task Complete: Task not complete <br/>";
                                    }
                                } 

                                if (newSubtasks.Count > 0)
                                {
                                    
                                        FeedbackContent += "- New SubTask:</br>";
                                        foreach (var sub in newSubtasks)
                                        {
                                            FeedbackContent += "<span style='padding-left:15px'>+ " + sub.Name + "</span></br>";
                                        }
                                    
                                }
                               

                               

                                var ticketFeedback = new T_TicketFeedback()
                                {
                                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                                    TicketId = task.TicketId,
                                    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                                    FeedbackTitle = "@" + cMem.FullName + " updated the task",
                                    Feedback = FeedbackContent,
                                    CreateByName = "System",
                                    CreateAt = DateTime.UtcNow,
                                    DisableWriteLog = true
                                };
                                db.T_TicketFeedback.Add(ticketFeedback);

                                var ticket = db.T_SupportTicket.Where(x => x.Id == task.TicketId).FirstOrDefault();
                                if (!string.IsNullOrEmpty(ticket.CustomerCode))
                                {
                                    var salesLead = db.C_SalesLead.Where(x => x.CustomerCode == ticket.CustomerCode).FirstOrDefault();
                                    if (salesLead != null)
                                    {
                                        var log = new Calendar_Event()
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            Name = "Ticket #" + ticket.Id + ": task has been updated",
                                            Description = FeedbackContent,
                                            CreateAt = DateTime.UtcNow,
                                            CreateBy = cMem.FullName,
                                            Type = "Log",
                                            GMT = "+00:00",
                                            TimeZone = "(UTC) Coordinated Universal Time",
                                            StartEvent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "+00:00",
                                            Done = 1,
                                            SalesLeadId = salesLead.Id,
                                            SalesLeadName = salesLead.L_SalonName
                                        };
                                        db.Calendar_Event.Add(log);
                                    }

                                }
                            }
                            #endregion

                            await db.SaveChangesAsync();
                            Trans.Commit();
                            Trans.Dispose();
                            if (task.Complete == true)
                            {
                                createRecurringTask(task.Id);
                            }
                            //email notice
                            await ViewControler.TaskViewService.SendNoticeAfterTaskUpdate(task, "update", cMem);

                            // TempData["s"] = "Edit success!";
                            message = "Edit success !";

                        }
                        else
                        {
                            throw new Exception("Task does not exist");
                        }
                    }
                    else //Add new task
                    {
                        #region Add New Task

                        int taskCount = db.Ts_Task.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
                        && t.CreateAt.Value.Month == DateTime.Today.Month).Count();

                        var task = new Ts_Task
                        {
                            Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (taskCount + 1).ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString("ff")),
                            Name = Task_Model.Name,
                            Note = Task_Model.Note,
                            Description = Request.Unvalidated["task_desc"],
                            Complete = Request["complete_task"] == "1" ? true : false,
                            CreateBy = cMem.FullName,
                            UpdateBy = cMem.FullName,
                            CreateByMemberNumber = cMem.MemberNumber,
                            CreateAt = DateTime.UtcNow
                        };
                        if (task.Complete == true)
                        {
                            if (task.CompletedDate == null)
                            {
                                task.CompletedDate = DateTime.UtcNow;
                            }
                        }
                        else
                        {
                            task.CompletedDate = null;
                        }
                        if (string.IsNullOrEmpty(Request["ticket"]) == false)
                        {
                            var ticket = Request["ticket"].Split('#');
                            var ticket_id = ticket[1].Split('-')[0].Trim();
                            var ticket_name = ticket[1].Split('-')[1].Trim();
                            task.TicketId = long.Parse(ticket_id);
                            task.TicketName = ticket_name;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(Request["ticket_id"]) == false)
                            {
                                var ticket_id = long.Parse(Request["ticket_id"]);
                                task.TicketId = ticket_id;
                                task.TicketName = db.T_SupportTicket.Find(ticket_id)?.Name;
                            }
                        }

                        if (string.IsNullOrEmpty(Request["task_assign"]) == false)
                        {
                            var memNumber = Request["task_assign"];
                            task.AssignedToMemberNumber = memNumber;
                            List<string> name = new List<string>();
                            foreach (var number in memNumber.Split(','))
                            {
                                name.Add(db.P_Member.Where(m => m.MemberNumber == number).FirstOrDefault()?.FullName + "(#" + number + ")");
                            }
                            task.AssignedToMemberName = string.Join(",", name);
                        }

                        if (string.IsNullOrEmpty(Request["DueDate"]) == false)
                        {
                            task.DueDate = DateTime.Parse(Request["DueDate"]);
                        }

                        if (string.IsNullOrEmpty(Request["chk_weekly"]) == false)
                        {
                            task.ReminderWeeklyAt = int.Parse(Request["r_weekly"]);
                        }

                        if (string.IsNullOrEmpty(Request["chk_monthly"]) == false)
                        {
                            task.ReminderMonthlyAt = int.Parse(Request["r_monthly"]);
                        }

                        db.Ts_Task.Add(task);
                        Task_Model.Id = task.Id;

                        #endregion

                        //add new file 
                        var taskfiles = Request["upload_id_0"].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
                        //add new files
                        foreach (var s in taskfiles)
                        {
                            db.UploadMoreFiles.Add(new UploadMoreFile
                            {
                                TableId = task.Id,
                                TableName = "Ts_Task",
                                FileName = UploadAttachFile("/Upload/ts_task/" + task.Id.ToString() + "/", s, null, null, out string n, true),
                                UploadId = _id++,
                            });
                        }

                        #region Add Sub Task

                        if (subtask_id != null)
                        {
                            //var list_subtask = Session[SUB_TASK] as List<Ts_Task_session>;
                            foreach (var sub_id in subtask_id)
                            {
                                var subtask = new Ts_Task
                                {
                                    Name = Request["sub_name_" + sub_id],
                                    Complete = Request["chk_" + sub_id] == "on",
                                };
                                if (string.IsNullOrEmpty(subtask.Name))
                                {
                                    continue;
                                }
                                subtask.Id = long.Parse(sub_id);
                                subtask.TicketId = task.TicketId;
                                subtask.TicketName = task.TicketName;
                                subtask.ParentTaskId = task.Id;
                                subtask.ParentTaskName = task.Name;
                                subtask.CreateBy = cMem.FullName;
                                subtask.CreateByMemberNumber = cMem.MemberNumber;
                                subtask.CreateAt = DateTime.UtcNow;
                                db.Ts_Task.Add(subtask);
                                var files = Request["upload_id_" + sub_id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
                                foreach (var s in files)
                                {
                                    db.UploadMoreFiles.Add(new UploadMoreFile
                                    {
                                        TableId = subtask.Id,
                                        TableName = "Ts_Task",
                                        FileName = UploadAttachFile("/Upload/ts_task/sub_task/" + subtask.Id.ToString() + "/", s, null, null, out string n, true),
                                        UploadId = _id++,
                                    });
                                }
                            }
                        }

                        #endregion
                        #region update log
                        if (task.TicketId != null)
                        {
                            var lastUpdateIdTicket = 1;
                            var lastUpdate = db.T_TicketUpdateLog.Where(x => x.TicketId == task.TicketId).GroupBy(x => x.UpdateId).OrderBy(x => x.Key).FirstOrDefault();
                            if (lastUpdate != null)
                            {
                                lastUpdateIdTicket = lastUpdate.Key ?? 1;
                            }
                          
                            var updateLog = new T_TicketUpdateLog();
                            updateLog.UpdateId = lastUpdateIdTicket;
                            updateLog.TicketId = task.TicketId;
                            updateLog.Name = "TaskInsert";
                            updateLog.Description = "Have a new task";
                 
                            updateLog.NewValue = task.Name;
                            updateLog.CreateAt = DateTime.UtcNow;
                            updateLog.CreateBy = cMem.FullName;
                            db.T_TicketUpdateLog.Add(updateLog);

                            var FeedbackContent = "- Task Name: " + task.Name +"</br>";
                            FeedbackContent += "- Assigned: " + (task.AssignedToMemberName ?? "Unasigned") + "</br>";

                            if (!string.IsNullOrEmpty(task.Note))
                            {
                                FeedbackContent += "- Note: " + task.Note + "</br>";
                            }

                            if (subtask_id != null) {
                                FeedbackContent += "- Subtask:</br>";
                                foreach (var sub_id in subtask_id)
                                {
                                    FeedbackContent += "<span style='padding-left:15px'>+ " + Request["sub_name_" + sub_id]+"</span></br>";
                                }
                            }
                            if (Request["enableReminder"] == "true")
                            {
                                FeedbackContent += "- Reminder (UTC+0):</br>";
                                double GMTNumber = 7;
                                if (!string.IsNullOrEmpty(GMT))
                                {
                                    GMTNumber = double.Parse(GMT.Substring(0, 3));
                                }
                                var reminderDateUTC = (reminderModel.Date.Value.Date + reminderModel.Time).Value.AddHours(-GMTNumber);
                                //reminder.Note = model.Note;
                            
                                FeedbackContent += "<span style='padding-left:15px'>+ Date: " + reminderModel.Date.Value.ToString("MM/dd/yyyy") + "</span></br>";
                                FeedbackContent += "<span style='padding-left:15px'>+ Time: " + new DateTime(reminderModel.Time.Value.Ticks).ToString("HH:mm") + "</span></br>";
                                FeedbackContent += "<span style='padding-left:15px'>+ Repeat: " + reminderModel.Repeat + "</span></br>";
                                FeedbackContent += "<span style='padding-left:15px'>+ Note: " + reminderModel.Note + "</span></br>";

                            }

                            var ticketFeedback = new T_TicketFeedback()
                            {
                                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                                TicketId = task.TicketId,
                                DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                                FeedbackTitle = "@" + cMem.FullName + " inserted the task",
                                Feedback = FeedbackContent,
                                CreateByName = "System",
                                CreateAt = DateTime.UtcNow,
                                DisableWriteLog = true
                            };
                            db.T_TicketFeedback.Add(ticketFeedback);

                            var ticket = db.T_SupportTicket.Where(x => x.Id == task.TicketId).FirstOrDefault();
                            if (!string.IsNullOrEmpty(ticket.CustomerCode))
                            {
                                var salesLead = db.C_SalesLead.Where(x => x.CustomerCode == ticket.CustomerCode).FirstOrDefault();
                                if (salesLead != null)
                                {
                                    var log = new Calendar_Event()
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        Name = "Ticket #" + ticket.Id + ": task has been inserted",
                                        Description = FeedbackContent,
                                        CreateAt = DateTime.UtcNow,
                                        CreateBy = cMem.FullName,
                                        Type = "Log",
                                        GMT = "+00:00",
                                        TimeZone = "(UTC) Coordinated Universal Time",
                                        StartEvent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "+00:00",
                                        Done = 1,
                                        SalesLeadId = salesLead.Id,
                                        SalesLeadName = salesLead.L_SalonName
                                    };
                                    db.Calendar_Event.Add(log);
                                }
                          
                            }
                        }
                      
                        #endregion
                        db.SaveChanges();
                        Trans.Commit();
                        Trans.Dispose();
                        if (task.Complete == true)
                        {
                            createRecurringTask(task.Id);
                        }
                        //email notice
                        await ViewControler.TaskViewService.SendNoticeAfterTaskUpdate(task, "new", cMem);
                        // TempData["s"] = "Add success!";
                        message = "Add success !";

                    }
                    if (Task_Model.TicketId > 0)
                    {
                        var reminder = db.T_RemindersTicket.FirstOrDefault(x => x.TicketId == Task_Model.TicketId && x.TaskId == Task_Model.Id);
                        double GMTNumber = 7;
                        if (!string.IsNullOrEmpty(GMT))
                        {
                            GMTNumber = double.Parse(GMT.Substring(0, 3));
                        }
                        if (reminder != null)
                        {
                            var reminderTicketService = new ReminderTicketService();

                            if (Request["enableReminder"] == "true")
                            {
                                var reminderDateUTC = (reminderModel.Date.Value.Date + reminderModel.Time).Value.AddHours(-GMTNumber);
                                //reminder.Note = model.Note;
                                reminder.Repeat = reminderModel.Repeat;
                                reminder.Time = reminderDateUTC.TimeOfDay;
                                reminder.Date = reminderDateUTC.Date;
                                reminder.UpdateAt = DateTime.UtcNow;
                                reminder.UpdateBy = cMem.FullName;
                                db.SaveChanges();
                                reminderTicketService.CreateJob(reminder);
                            }
                            else
                            {
                                reminderTicketService.DeleteJob(reminder.HangfireJobId);
                                db.T_RemindersTicket.Remove(reminder);
                                db.SaveChanges();
                            }
                        }
                        else
                        {

                            if (Request["enableReminder"] == "true")
                            {
                                var reminderTicketService = new ReminderTicketService();
                                var reminderDateUTC = (reminderModel.Date.Value.Date + reminderModel.Time).Value.AddHours(-GMTNumber);
                                reminderModel.Time = reminderDateUTC.TimeOfDay;
                                reminderModel.Date = reminderDateUTC.Date;
                                reminderModel.Note = reminderModel.Note;
                                reminderModel.CreateAt = DateTime.UtcNow;
                                reminderModel.CreateBy = cMem.FullName;
                                reminderModel.TicketId = Task_Model.TicketId.Value;
                                reminderModel.TaskId = Task_Model.Id;
                                reminderModel.Active = true;
                                reminderModel.HangfireJobId = Guid.NewGuid().ToString();
                                db.T_RemindersTicket.Add(reminderModel);
                                db.SaveChanges();
                                reminderTicketService.CreateJob(reminderModel);
                            }
                        }
                    }

                    return Json(message);
                }
                catch (Exception ex)
                {
                    Trans.Dispose();
                    // TempData["e"] = ex.Message;
                    // throw ex;
                    //return Json(ex.Message);
                    return new Func.JsonStatusResult("Process Task Error : " + ex.Message, HttpStatusCode.ExpectationFailed);
                }
            }
        }

        private void createRecurringTask(long task_id)
        {
            var db = new WebDataModel();
            var task = db.Ts_Task.Find(task_id);
            var duedate = DateTime.UtcNow;
            if (task.ReminderWeeklyAt != null)
            {
                var date_week = task.ReminderWeeklyAt - (int)duedate.DayOfWeek;
                if (date_week < 0)
                {
                    date_week = date_week.Value + 7;
                }
                duedate = duedate.AddDays(date_week ?? 0);
                if (duedate <= (task.DueDate ?? task.CreateAt.Value.AddDays(7)))
                {
                    duedate = duedate.AddDays(7);
                }
            }
            else if (task.ReminderMonthlyAt != null)
            {
                var date_month = task.ReminderMonthlyAt - duedate.Day;
                duedate = duedate.AddDays(date_month ?? 0);
                if (date_month < 0)
                {
                    duedate = duedate.AddMonths(1);
                }
                if (duedate <= (task.DueDate ?? task.CreateAt.Value.AddMonths(1)))
                {
                    duedate = duedate.AddMonths(1);
                }
            }
            var _id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
            if (task.Reminded != true && (task.ReminderWeeklyAt != null || task.ReminderMonthlyAt != null))
            {
                task.Reminded = true;
                var new_task = new Ts_Task
                {
                    Id = _id++,
                    DueDate = duedate,
                    Name = task.Name,
                    Description = task.Description,
                    TicketId = task.TicketId,
                    TicketName = task.TicketName,
                    Complete = false,
                    ReminderWeeklyAt = task.ReminderWeeklyAt,
                    ReminderMonthlyAt = task.ReminderMonthlyAt,
                    AssignedToMemberNumber = task.AssignedToMemberNumber,
                    AssignedToMemberName = task.AssignedToMemberName,
                    CreateBy = "System",
                    CreateAt = DateTime.Now,
                    CreateByMemberNumber = task.CreateByMemberNumber,
                };
                var task_clone_uploads = db.UploadMoreFiles.Where(u => u.TableId == task_id && u.TableName == "Ts_Task").ToList();
                task_clone_uploads.ForEach(u =>
                {
                    var up = new UploadMoreFile
                    {
                        UploadId = _id++,
                        TableId = new_task.Id,
                        FileName = u.FileName,
                        TableName = u.TableName,
                    };
                    db.UploadMoreFiles.Add(up);
                });
                db.Ts_Task.Add(new_task);
                var subtasks = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
                subtasks.ForEach(t =>
                {
                    var clone_task = new Ts_Task
                    {
                        Id = _id++,
                        Name = t.Name,
                        Complete = false,
                        TicketId = new_task.TicketId,
                        TicketName = new_task.TicketName,
                        ParentTaskId = new_task.Id,
                        ParentTaskName = new_task.Name,
                        CreateBy = cMem.FullName,
                        CreateByMemberNumber = cMem.MemberNumber,
                        CreateAt = DateTime.UtcNow,
                        DueDate = duedate,
                        AssignedToMemberName = new_task.AssignedToMemberName,
                        AssignedToMemberNumber = new_task.AssignedToMemberNumber,
                    };

                    var clone_uploads = db.UploadMoreFiles.Where(u => u.TableId == t.Id && u.TableName == "Ts_Task").ToList();
                    clone_uploads.ForEach(u =>
                    {
                        var up = new UploadMoreFile
                        {
                            UploadId = _id++,
                            TableId = clone_task.Id,
                            FileName = u.FileName,
                            TableName = u.TableName,
                        };
                        db.UploadMoreFiles.Add(up);
                    });
                    db.Ts_Task.Add(clone_task);
                });

                db.SaveChanges();
            }
        }

        #endregion

        #region Sub Task
        public ActionResult NewSubtask()
        {
            return PartialView("_new_subtask", DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
        }
        #endregion

        #region getAjax
        public JsonResult GetPicture(string list_id)
        {
            WebDataModel db = new WebDataModel();
            var list_img = (from m in db.P_Member
                            where list_id.Contains(m.MemberNumber.ToString())
                            select new P_Member_image
                            {
                                Id = m.Id,
                                Picture = m.Picture,
                                FullName = m.FullName
                            }).ToList();
            return Json(new object[] { true, list_img });
        }
        #endregion

        #region template
        public ActionResult Template()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoadTemplate(IDataTablesRequest dataTablesRequest, string Category, string SearchText)
        {
            WebDataModel db = new WebDataModel();
            var query = from t in db.Ts_TaskTemplateCategory where t.IsDeleted != true select t;
            if (!string.IsNullOrEmpty(Category))
            {
                query = query.Where(x => x.TicketGroup == Category);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.Name.Contains(SearchText));
            }
            int totalRecord = query.Count();
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch?.Name)
            {
                case "Date":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.UpdateAt ?? x.CreatedAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.UpdateAt ?? x.CreatedAt);
                    }
                    break;
                case "Category":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.TicketGroup);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.TicketGroup);
                    }
                    break;
                case "Status":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.Status);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Status);
                    }
                    break;
                case "Requirement":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.Requirement);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Requirement);
                    }
                    break;
                case "Name":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Name);
                    }
                    break;

                default:
                    query = query.OrderByDescending(x => x.CreatedAt);
                    break;
            };
            var data = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x => new
            {
                Date = (x.UpdateAt ?? x.CreatedAt).Value.UtcToIMSDateTime().ToString("MMM dd, yyyy"),
                UpdateBy = x.CreateBy ?? x.UpdateBy,
                x.Name,
                x.Description,
                Category = x.TicketGroup,
                x.Requirement,
                x.Status,
                x.Id,
            });
            return Json(new
            {
                data,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
            });
        }
        [HttpPost]
        public ActionResult TaskTemplateCreateOrModel(int? TaskTemplateId)
        {
            var model = new TaskTemplateModel();
            if (TaskTemplateId > 0)
            {
                WebDataModel db = new WebDataModel();
                var taskTemplate = db.Ts_TaskTemplateCategory.Where(x => x.Id == TaskTemplateId).FirstOrDefault();
                if (taskTemplate == null)
                {
                    return Content("Task Template Not Found");
                }
                model.Id = taskTemplate.Id;
                model.Name = taskTemplate.Name;
                model.Description = taskTemplate.Description;
                model.Status = taskTemplate.Status;
                model.Requirement = taskTemplate.Requirement;
                model.TicketGroup = taskTemplate.TicketGroup;
                var listSubTaskTemplate = new List<TaskTemplateFieldModel>();
                listSubTaskTemplate = db.Ts_TaskTemplateField.Where(x => x.CategoryId == taskTemplate.Id).Select(x => new TaskTemplateFieldModel
                {
                    Id = x.Id,
                    Description = x.Description,
                    Name = x.Name,
                    DisplayOrder = x.DisplayOrder,
                    TypeField = x.TypeField,
                }).ToList();
                model.SubTaskTemplateList = listSubTaskTemplate;
            }
            else
            {
                model.Status = true;
            }
            return PartialView("_CreateOrUpdateTaskTemplateModal", model);
        }

        [HttpPost]
        public ActionResult TaskTemplateSave(TaskTemplateModel model)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                if (model.Id > 0)
                {

                    using (var Trans = db.Database.BeginTransaction())
                    {
                        try
                        {
                            var taskTemplate = db.Ts_TaskTemplateCategory.Where(x => x.Id == model.Id).Include(x => x.Ts_TaskTemplateField).FirstOrDefault();
                            if (taskTemplate == null)
                            {
                                return Json(new { status = false, message = "Task template not found" });
                            }
                            taskTemplate.Name = model.Name;
                            taskTemplate.Description = model.Description;
                            taskTemplate.Status = true;
                            taskTemplate.Requirement = model.Requirement;
                            taskTemplate.TicketGroup = model.TicketGroup;
                            taskTemplate.UpdateBy = cMem.FullName;
                            taskTemplate.UpdateAt = DateTime.UtcNow;
                            taskTemplate.Ts_TaskTemplateField.Clear();
                            var ListSubTaskTemplate = new List<Ts_TaskTemplateField>();
                            foreach (var item in model.SubTaskTemplateList)
                            {
                                var subTaskTemplate = new Ts_TaskTemplateField();
                                subTaskTemplate.Name = item.Name;
                                subTaskTemplate.Description = item.Description;
                                subTaskTemplate.CreatedAt = DateTime.UtcNow;
                                subTaskTemplate.CreatedBy = cMem.MemberNumber;
                                subTaskTemplate.DisplayOrder = item.DisplayOrder;
                                ListSubTaskTemplate.Add(subTaskTemplate);
                            }
                            taskTemplate.Ts_TaskTemplateField = ListSubTaskTemplate;
                            db.SaveChanges();
                            Trans.Commit();
                            Trans.Dispose();
                            return Json(new { status = true, message = "update task success" });
                        }
                        catch (Exception ex)
                        {
                            Trans.Dispose();
                            return Json(new { status = false, message = ex.Message });
                        }

                    }

                }
                else
                {
                    var taskTemplate = new Ts_TaskTemplateCategory();
                    taskTemplate.Name = model.Name;
                    taskTemplate.Description = model.Description;
                    taskTemplate.Status = true;
                    taskTemplate.Requirement = model.Requirement;
                    taskTemplate.TicketGroup = model.TicketGroup;
                    taskTemplate.CreateBy = cMem.FullName;
                    taskTemplate.CreatedAt = DateTime.UtcNow;
                    var ListSubTaskTemplate = new List<Ts_TaskTemplateField>();
                    if (model.SubTaskTemplateList != null)
                    {
                        foreach (var item in model.SubTaskTemplateList)
                        {
                            var subTaskTemplate = new Ts_TaskTemplateField();
                            subTaskTemplate.Name = item.Name;
                            subTaskTemplate.Description = item.Description;
                            subTaskTemplate.CreatedAt = DateTime.UtcNow;
                            subTaskTemplate.CreatedBy = cMem.MemberNumber;
                            subTaskTemplate.DisplayOrder = item.DisplayOrder;
                            ListSubTaskTemplate.Add(subTaskTemplate);
                        }
                        taskTemplate.Ts_TaskTemplateField = ListSubTaskTemplate;
                    }

                    db.Ts_TaskTemplateCategory.Add(taskTemplate);
                    db.SaveChanges();
                    return Json(new { status = true, message = "add task success" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }

        }

        [HttpPost]
        public ActionResult DeleteTaskTemplate(int? TaskTemplateId)
        {
            WebDataModel db = new WebDataModel();
            var taskTemplate = db.Ts_TaskTemplateCategory.Where(x => x.Id == TaskTemplateId).FirstOrDefault();
            if (taskTemplate == null)
            {
                return Json(new { status = false, message = "task template not found" });
            }
            taskTemplate.IsDeleted = true;
            db.SaveChanges();
            return Json(new { status = true, message = "delete success" });
        }

        [HttpPost]
        public ActionResult UpdateNote(long? TaskId,string Note)
        {
            WebDataModel db = new WebDataModel();
            var task = db.Ts_Task.Find(TaskId);
            task.Note = Note;
            task.UpdateBy = cMem.FullName;
            task.UpdateAt = DateTime.UtcNow;


            var FeedbackContent = "<span>- Note: " + Note + "</span>";
            var ticketFeedback = new T_TicketFeedback()
            {
                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                TicketId = task.TicketId,
                DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                FeedbackTitle = "@" + cMem.FullName + " updated the task",
                Feedback = FeedbackContent,
                CreateByName = "System",
                CreateAt = DateTime.UtcNow,
                DisableWriteLog = true
            };
            db.T_TicketFeedback.Add(ticketFeedback);

            var ticket = db.T_SupportTicket.Where(x => x.Id == task.TicketId).FirstOrDefault();
            if (!string.IsNullOrEmpty(ticket.CustomerCode))
            {
                var salesLead = db.C_SalesLead.Where(x => x.CustomerCode == ticket.CustomerCode).FirstOrDefault();
                if (salesLead != null)
                {
                    var log = new Calendar_Event()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = "Ticket #" + ticket.Id + ": task has been updated",
                        Description = FeedbackContent,
                        CreateAt = DateTime.UtcNow,
                        CreateBy = cMem.FullName,
                        Type = "Log",
                        GMT = "+00:00",
                        TimeZone = "(UTC) Coordinated Universal Time",
                        StartEvent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "+00:00",
                        Done = 1,
                        SalesLeadId = salesLead.Id,
                        SalesLeadName = salesLead.L_SalonName
                    };
                    db.Calendar_Event.Add(log);
                }

            }
            db.SaveChanges();
            return Json(new { message = "update note success", status = true });
        }

        #endregion

        #region delete Task

        //[HttpPost]
        //public ActionResult DeleteTask(long? TaskId)
        //{
        //    WebDataModel db = new WebDataModel();
        //    var task = db.Ts_Task.Find(TaskId);
        //    if (task == null)
        //    {
        //        return Json(new { status = false, message = "task not found " });
        //    }
    
        //        var allSubtask = db.Ts_Task.Where(x => x.ParentTaskId == task.Id).ToList();
        //        if (allSubtask.Count > 0)
        //        {
        //            db.Ts_Task.RemoveRange(allSubtask);
        //        }
        //        db.Ts_Task.Remove(task);
        //        db.SaveChanges();
        //        return Json(new { status = true, message = "delete task success" });
        //}
        #endregion
    }


}