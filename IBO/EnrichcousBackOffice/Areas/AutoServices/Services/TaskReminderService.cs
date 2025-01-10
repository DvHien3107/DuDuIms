using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Enrich.Core.Infrastructure;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.Areas.AutoServices.Services
{
    public class TaskReminderService
    {
        public static async void Scan_Task()
        {
            WebDataModel db = new WebDataModel();

            using (var Trans = db.Database.BeginTransaction())
            {
                try
                {
                    var current_date = DateTime.Now;
                    int day_of_week = Convert.ToInt32(current_date.DayOfWeek);
                    var day_of_month = current_date.Day;

                    var list_task_reminder = db.Ts_Task.Where(t => t.ParentTaskId == null &&
                    (t.ReminderWeeklyAt != null || t.ReminderMonthlyAt != null) && t.DueDate <= current_date.Date && t.Reminded != true).ToList();

                    int taskweekly = 0, taskmonthly = 0;

                    long id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                    var db_update = false;
                    foreach (var task in list_task_reminder)
                    {
                        //reminder weekly
                        #region REMINDER WEEKLY

                        if (task.ReminderWeeklyAt != null && task.ReminderWeeklyAt == day_of_week)
                        {



                            if (task.DueDate == null)
                            {
                                task.DueDate = current_date;
                            }
                            task.Reminded = true;
                            db.Entry(task).State = System.Data.Entity.EntityState.Modified;

                            //create new task
                            var new_task = new Ts_Task
                            {
                                Id = id++,
                                DueDate = current_date.AddDays(7),
                                Name = task.Name,
                                Description = task.Description,
                                TicketId = task.TicketId,
                                TicketName = task.TicketName,
                                Complete = task.Complete,
                                ReminderWeeklyAt = task.ReminderWeeklyAt,
                                ReminderMonthlyAt = task.ReminderMonthlyAt,
                                AssignedToMemberNumber = task.AssignedToMemberNumber,
                                AssignedToMemberName = task.AssignedToMemberName,
                                CreateBy = "System",
                                CreateAt = DateTime.Now,
                                CreateByMemberNumber = task.CreateByMemberNumber,
                                //UpdateBy = task.UpdateBy,
                                //UpdateAt = task.UpdateAt,
                                //ParentTaskId = task.ParentTaskId,
                                //ParentTaskName = task.ParentTaskName
                            };

                            db.Ts_Task.Add(new_task);
                            taskweekly++;
                            //create new subtask
                            var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
                            if (list_subtask != null && list_subtask.Count() > 0)
                            {
                                foreach (var subtask in list_subtask)
                                {
                                    var new_subtask = new Ts_Task
                                    {
                                        Id = id++,
                                        //DueDate = subtask.DueDate,
                                        Name = subtask.Name,
                                        //Description = subtask.Description,
                                        //TicketId = subtask.TicketId,
                                        //TicketName = subtask.TicketName,
                                        //Complete = subtask.Complete,
                                        //ReminderWeeklyAt = subtask.ReminderWeeklyAt,
                                        //ReminderMonthlyAt = subtask.ReminderMonthlyAt,
                                        //AssignedToMemberNumber = subtask.AssignedToMemberNumber,
                                        //AssignedToMemberName = subtask.AssignedToMemberName,
                                        CreateBy = "System",
                                        CreateAt = DateTime.Now,
                                        CreateByMemberNumber = subtask.CreateByMemberNumber,
                                        //UpdateBy = subtask.UpdateBy,
                                        //UpdateAt = subtask.UpdateAt,
                                        ParentTaskId = new_task.Id,
                                        ParentTaskName = new_task.Name
                                    };

                                    db.Ts_Task.Add(new_subtask);
                                }
                            }

                            db_update = true;

                            //send email reminder
                         await SendNoticeReminder(task);
                        }
                        #endregion


                        //reminder monthly
                        #region REMINDER MONTHLY
                        if (task.ReminderMonthlyAt != null && (task.ReminderMonthlyAt == day_of_month || (current_date.Month == 2 && task.ReminderMonthlyAt > 28)))
                        {
                            if (task.DueDate == null)
                            {
                                task.DueDate = current_date;
                                db.Entry(task).State = System.Data.Entity.EntityState.Modified;
                            }
                            task.Reminded = true;
                            db.Entry(task).State = System.Data.Entity.EntityState.Modified;
                            //create new task
                            var new_task = new Ts_Task
                            {
                                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                                DueDate = current_date.AddMonths(1),
                                Name = task.Name,
                                Description = task.Description,
                                TicketId = task.TicketId,
                                TicketName = task.TicketName,
                                Complete = task.Complete,
                                ReminderWeeklyAt = task.ReminderWeeklyAt,
                                ReminderMonthlyAt = task.ReminderMonthlyAt,
                                AssignedToMemberNumber = task.AssignedToMemberNumber,
                                AssignedToMemberName = task.AssignedToMemberName,
                                CreateBy = task.CreateBy,
                                CreateAt = task.CreateAt,
                                CreateByMemberNumber = task.CreateByMemberNumber,
                                UpdateBy = task.UpdateBy,
                                UpdateAt = task.UpdateAt,
                                ParentTaskId = task.ParentTaskId,
                                ParentTaskName = task.ParentTaskName
                            };

                            db.Ts_Task.Add(new_task);
                            taskmonthly++;
                            //create new subtask
                            var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
                            if (list_subtask != null && list_subtask.Count() > 0)
                            {
                                var index = 1;
                                foreach (var subtask in list_subtask)
                                {
                                    var new_subtask = new Ts_Task
                                    {
                                        Id = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff")) + index,
                                        DueDate = subtask.DueDate,
                                        Name = subtask.Name,
                                        Description = subtask.Description,
                                        TicketId = subtask.TicketId,
                                        TicketName = subtask.TicketName,
                                        Complete = subtask.Complete,
                                        ReminderWeeklyAt = subtask.ReminderWeeklyAt,
                                        ReminderMonthlyAt = subtask.ReminderMonthlyAt,
                                        AssignedToMemberNumber = subtask.AssignedToMemberNumber,
                                        AssignedToMemberName = subtask.AssignedToMemberName,
                                        CreateBy = subtask.CreateBy,
                                        CreateAt = subtask.CreateAt,
                                        CreateByMemberNumber = subtask.CreateByMemberNumber,
                                        UpdateBy = subtask.UpdateBy,
                                        UpdateAt = subtask.UpdateAt,
                                        ParentTaskId = new_task.Id,
                                        ParentTaskName = new_task.Name
                                    };

                                    db.Ts_Task.Add(new_subtask);
                                    index++;
                                }
                            }

                            db_update = true;
                            //send email reminder
                           await SendNoticeReminder(task);
                        }
                        #endregion
                    }
                    if (db_update)
                    {
                        db.SaveChanges();
                        Trans.Commit();
                    }

                    Trans.Dispose();
                }
                catch (Exception ex)
                {
                    Trans.Dispose();
                    var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                    _mailingService.SendBySendGrid("bau.pahmngoc@enrichco.us", "Bau", "IMS ENRICH AUTO SCAN - Scan_Task function error", ex.Message + "<br/>" + ex.InnerException?.Message).Wait();

                }

            }


        }

        /// <summary>
        /// Send email cho member duoc assign sau khi task duoc create or update
        /// </summary>
        /// <param name="type">new|update</param>
        /// <returns></returns>
        public static string SendNoticeAfterTaskUpdate(Ts_Task task, string type = "update", WebDataModel _db = null)
        {
            try
            {
                string bodyE = "";
                string subject = "";
                string to = "";
                string toMemNo = "";
                string firstname = "";
                string cc = "";
                string link = ConfigurationManager.AppSettings["IMS_Url"] + "/tasksman/detail/" + task.Id;

                var db = _db;
                if (db == null)
                {
                    db = new WebDataModel();
                }
                List<string> memberNumberList = new List<string>();
                memberNumberList.Add(task.CreateByMemberNumber);

                if (string.IsNullOrEmpty(task.AssignedToMemberNumber) == false)
                {
                    foreach (var item in task.AssignedToMemberNumber?.Split(','))
                    {
                        memberNumberList.Add(item);
                    }
                }

                var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
                string subtaskTable = "";
                if (list_subtask != null && list_subtask.Count() > 0)
                {
                    foreach (var item in list_subtask)
                    {
                        if (string.IsNullOrEmpty(item.AssignedToMemberNumber) == false && memberNumberList.Contains(item.AssignedToMemberNumber) == false)
                        {
                            foreach (var _memNumber in item.AssignedToMemberNumber?.Split(','))
                            {
                                memberNumberList.Add(_memNumber);
                            }
                        }
                    }

                    //create string html table subtask
                    subtaskTable = "<br/><strong>Subtask:</strong><br/><table class='table_border' border='1' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                        "<tr style='background-color:#f1eeee'><td>Name</td><td>Status</td></tr>";

                    foreach (var subtask in list_subtask)
                    {
                        subtaskTable += "<tr>" +
                                "<td>" + subtask.Name + " </td>" +
                                "<td>" + (subtask.Complete == true ? "<strong>done</strong>" : "") + "</td>" +
                            "</tr>";
                    }
                    subtaskTable += "</table>";
                }
                string shorttaskname = task.Name.Length > 30 ? task.Name.Substring(0, 30) : task.Name;
                //

                if (type.Equals("new"))
                {
                    //new
                    subject = "[Enrich IMS]You have a new task #" + task.Id;
                    #region body email
                    bodyE = "You have a new task.<br/><br/><b>Task name: </b>" + task.Name + "<br/><br/>" +
                        "<br/><strong>Assigned: </strong><br>" + (task.AssignedToMemberName == null ? "Unassigned" : task.AssignedToMemberName.Replace(",", "<br/>")) +
                        subtaskTable +
                        "<br/><strong>Complete: </strong>" + (task.Complete == true ? "Done" : "Not yet") +
                        "<br/><strong>Create By: </strong>" + task.CreateBy + "<i> at " + task.CreateAt?.ToString("MM/dd/yyyy hh:mm tt") +
                        "<br/><strong>Latest update: </strong>" + task.UpdateBy?.Split('|')[0] + "<br/>";
                    bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>" + link + "</a>";

                    #endregion

                }
                else
                {
                    //update
                    subject = "[Enrich IMS]Your task just had updated.#" + task.Id;
                    #region body email
                    bodyE = "Your task just had updated.#" + task.Id + "<br/><br/><b>Task name: </b>" + task.Name + "<br/>" +
                       "<br/><strong>Assigned: </strong><br>" + (task.AssignedToMemberName == null ? "Unassigned" : task.AssignedToMemberName.Replace(",", "<br/>")) +
                        subtaskTable +
                        "<br/><strong>Complete: </strong>" + (task.Complete == true ? "Done" : "Not yet") +
                        "<br/><strong>Create By: </strong>" + task.CreateBy + "<i> at " + task.CreateAt?.ToString("MM/dd/yyyy hh:mm tt") +
                        "<br/><strong>Latest update: </strong>" + task.UpdateBy?.Split('|')[0] + "<br/>";
                    bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>" + link + "</a>";

                    #endregion

                }
                var members = db.P_Member.Where(m => memberNumberList.Contains(m.MemberNumber) == true).ToList();
                foreach (var item in members)
                {
                    to = string.Join(";", to, item.PersonalEmail);
                    toMemNo = string.Join(";", toMemNo, item.MemberNumber);
                    firstname = string.Join(";", firstname, item.FirstName);

                }

                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                string result = _mailingService.SendBySendGrid(to, firstname, subject, bodyE, cc).Result;
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }

        /// <summary>
        /// Send mail reminder task not complete
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public static async Task<string> SendNoticeReminder(Ts_Task task, WebDataModel _db = null)
        {
            try
            {
                string bodyE = "";
                string subject = "";
                string to = "";
                string toMemNo = "";
                string firstname = "";
                string cc = "";
                string link = ConfigurationManager.AppSettings["IMS_Url"] + "/tasksman/detail/" + task.Id;
                var db = _db;
                if (db == null)
                {
                    db = new WebDataModel();
                }
                List<string> memberNumberList = new List<string>();
                memberNumberList.Add(task.CreateByMemberNumber);

                if (string.IsNullOrEmpty(task.AssignedToMemberNumber) == false)
                {
                    foreach (var item in task.AssignedToMemberNumber?.Split(','))
                    {
                        memberNumberList.Add(item);
                    }
                }
                var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
                if (list_subtask != null && list_subtask.Count() > 0)
                {
                    foreach (var item in list_subtask)
                    {
                        if (string.IsNullOrEmpty(item.AssignedToMemberNumber) == false && memberNumberList.Contains(item.AssignedToMemberNumber) == false)
                        {
                            foreach (var _memNumber in item.AssignedToMemberNumber?.Split(','))
                            {
                                memberNumberList.Add(_memNumber);
                            }
                        }
                    }
                }
                //create string html table subtask
                string subtaskTable = "<br/><strong>Subtask:</strong><br/><table class='table_border' border='1' style='width:100%;border-collapse:collapse;' cellpadding='5'>" +
                    "<tr style='background-color:#f1eeee'><td>Name</td><td>Status</td></tr>";

                foreach (var subtask in list_subtask)
                {
                    subtaskTable += "<tr>" +
                            "<td>" + subtask.Name + " </td>" +
                            "<td>" + (subtask.Complete == true ? "<strong>done</strong>" : "") + "</td>" +
                        "</tr>";
                }
                subtaskTable += "</table>";



                string shorttaskname = task.Name.Length > 30 ? task.Name.Substring(0, 30) : task.Name;

                subject = "[Enrich IMS]You have an unfinished task: " + shorttaskname;

                #region body email
                bodyE =
                    "<strong>Task name: </strong><br/>" + task.Name + "<br/>" +
                    "<br/>" + subtaskTable +
                    "<br/><strong>Assigned: </strong><br>" + (task.AssignedToMemberName == null ? "Unassigned" : task.AssignedToMemberName.Replace(",", "<br/>")) +
                        "<br/><strong>Complete: </strong>" + (task.Complete == true ? "Done" : "Not yet") +
                        "<br/><strong>Create By: </strong>" + task.CreateBy + "<i> at " + task.CreateAt?.ToString("MM/dd/yyyy hh:mm tt") +
                        "<br/><strong>Latest update: </strong>" + task.UpdateBy?.Split('|')[0] + "<br/>";
                bodyE += "<br/>Please click on link to below to open ticket.<br/><a href='" + link + "'>" + link + "</a>";
                #endregion


                var members = db.P_Member.Where(m => memberNumberList.Contains(m.MemberNumber) == true);
                foreach (var item in members)
                {
                    to = string.Join(";", to, item.PersonalEmail);
                    toMemNo = string.Join(";", toMemNo, item.MemberNumber);
                    firstname = string.Join(";", firstname, item.FirstName);

                }
                //string result = SendEmail.SendEmailNotice(bodyE, to, firstname, subject, cc).Result;
                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                string result = await _mailingService.SendBySendGrid(to, firstname, subject, bodyE, cc);
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }


    }
}