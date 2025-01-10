using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.ViewControler;

namespace EnrichcousBackOffice.Controllers
{
  
    public class AutoScanController : Controller
    {
        // GET: AutoScan
        public ActionResult Index()
        {
           // WebDataModel db = new WebDataModel();

            //using (var Trans = db.Database.BeginTransaction())
            //{
            //    try
            //    {
            //        var current_date = DateTime.Now;
            //        int day_of_week = Convert.ToInt32(current_date.DayOfWeek);
            //        var day_of_month = current_date.Day;
                    
            //        var list_task_reminder = db.Ts_Task.Where(t => t.ParentTaskId == null && t.Complete != true && (t.ReminderWeeklyAt != null || t.ReminderMonthlyAt != null)).ToList();

            //        foreach (var task in list_task_reminder)
            //        {
            //            //reminder weekly
            //            #region REMINDER WEEKLY

            //            if (task.ReminderWeeklyAt != null && task.ReminderWeeklyAt == day_of_week)
            //            {
            //                if (task.DueDate == null)
            //                {
            //                    task.DueDate = current_date;
            //                    db.Entry(task).State = System.Data.Entity.EntityState.Modified;
            //                }

            //                //create new task
            //                var new_task = new Ts_Task
            //                {
            //                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
            //                    DueDate = current_date.AddDays(7),
            //                    Name = task.Name,
            //                    Description = task.Description,
            //                    TicketId = task.TicketId,
            //                    TicketName = task.TicketName,
            //                    Complete = task.Complete,
            //                    ReminderWeeklyAt = task.ReminderWeeklyAt,
            //                    ReminderMonthlyAt = task.ReminderMonthlyAt,
            //                    AssignedToMemberNumber = task.AssignedToMemberNumber,
            //                    AssignedToMemberName = task.AssignedToMemberName,
            //                    CreateBy = task.CreateBy,
            //                    CreateAt = task.CreateAt,
            //                    CreateByMemberNumber = task.CreateByMemberNumber,
            //                    UpdateBy = task.UpdateBy,
            //                    UpdateAt = task.UpdateAt,
            //                    ParentTaskId = task.ParentTaskId,
            //                    ParentTaskName = task.ParentTaskName
            //                };

            //                db.Ts_Task.Add(new_task);

            //                //create new subtask
            //                var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
            //                if (list_subtask != null && list_subtask.Count() > 0)
            //                {
            //                    int index = 1;
            //                    foreach (var subtask in list_subtask)
            //                    {
            //                        var new_subtask = new Ts_Task
            //                        {
            //                            Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")) + index,
            //                            DueDate = subtask.DueDate,
            //                            Name = subtask.Name,
            //                            Description = subtask.Description,
            //                            TicketId = subtask.TicketId,
            //                            TicketName = subtask.TicketName,
            //                            Complete = subtask.Complete,
            //                            ReminderWeeklyAt = subtask.ReminderWeeklyAt,
            //                            ReminderMonthlyAt = subtask.ReminderMonthlyAt,
            //                            AssignedToMemberNumber = subtask.AssignedToMemberNumber,
            //                            AssignedToMemberName = subtask.AssignedToMemberName,
            //                            CreateBy = subtask.CreateBy,
            //                            CreateAt = subtask.CreateAt,
            //                            CreateByMemberNumber = subtask.CreateByMemberNumber,
            //                            UpdateBy = subtask.UpdateBy,
            //                            UpdateAt = subtask.UpdateAt,
            //                            ParentTaskId = new_task.Id,
            //                            ParentTaskName = new_task.Name
            //                        };

            //                        db.Ts_Task.Add(new_subtask);
            //                        index++;
            //                    }
            //                }
            //                //db.SaveChanges();

            //                //send email reminder
            //                TaskViewController.SendNoticeReminder(task);
            //            }
            //            #endregion


            //            //reminder monthly
            //            #region REMINDER MONTHLY

            //            if (task.ReminderMonthlyAt != null && (task.ReminderMonthlyAt == day_of_month || (current_date.Month == 2 && task.ReminderMonthlyAt > 28)))
            //            {
            //                if (task.DueDate == null)
            //                {
            //                    task.DueDate = current_date;
            //                    db.Entry(task).State = System.Data.Entity.EntityState.Modified;
            //                }

            //                //create new task
            //                var new_task = new Ts_Task
            //                {
            //                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
            //                    DueDate = current_date.AddMonths(1),
            //                    Name = task.Name,
            //                    Description = task.Description,
            //                    TicketId = task.TicketId,
            //                    TicketName = task.TicketName,
            //                    Complete = task.Complete,
            //                    ReminderWeeklyAt = task.ReminderWeeklyAt,
            //                    ReminderMonthlyAt = task.ReminderMonthlyAt,
            //                    AssignedToMemberNumber = task.AssignedToMemberNumber,
            //                    AssignedToMemberName = task.AssignedToMemberName,
            //                    CreateBy = task.CreateBy,
            //                    CreateAt = task.CreateAt,
            //                    CreateByMemberNumber = task.CreateByMemberNumber,
            //                    UpdateBy = task.UpdateBy,
            //                    UpdateAt = task.UpdateAt,
            //                    ParentTaskId = task.ParentTaskId,
            //                    ParentTaskName = task.ParentTaskName
            //                };

            //                db.Ts_Task.Add(new_task);

            //                //create new subtask
            //                var list_subtask = db.Ts_Task.Where(t => t.ParentTaskId == task.Id).ToList();
            //                if (list_subtask != null && list_subtask.Count() > 0)
            //                {
            //                    var index = 1;
            //                    foreach (var subtask in list_subtask)
            //                    {
            //                        var new_subtask = new Ts_Task
            //                        {
            //                            Id = long.Parse(DateTime.Now.ToString("yyMMddHHmmssfff")) + index,
            //                            DueDate = subtask.DueDate,
            //                            Name = subtask.Name,
            //                            Description = subtask.Description,
            //                            TicketId = subtask.TicketId,
            //                            TicketName = subtask.TicketName,
            //                            Complete = subtask.Complete,
            //                            ReminderWeeklyAt = subtask.ReminderWeeklyAt,
            //                            ReminderMonthlyAt = subtask.ReminderMonthlyAt,
            //                            AssignedToMemberNumber = subtask.AssignedToMemberNumber,
            //                            AssignedToMemberName = subtask.AssignedToMemberName,
            //                            CreateBy = subtask.CreateBy,
            //                            CreateAt = subtask.CreateAt,
            //                            CreateByMemberNumber = subtask.CreateByMemberNumber,
            //                            UpdateBy = subtask.UpdateBy,
            //                            UpdateAt = subtask.UpdateAt,
            //                            ParentTaskId = new_task.Id,
            //                            ParentTaskName = new_task.Name
            //                        };

            //                        db.Ts_Task.Add(new_subtask);
            //                        index++;
            //                    }
            //                }
            //                //db.SaveChanges();

            //                //send email reminder
            //                TaskViewController.SendNoticeReminder(task);
            //            }
            //            #endregion
            //        }

            //        db.SaveChanges();
            //        Trans.Commit();
            //        Trans.Dispose();
            //    }
            //    catch (Exception ex)
            //    {
            //        Trans.Dispose();
            //        TempData["e"] = "Reminder failed! " + ex.Message;
            //        AppLB.SendEmail.Send("sonht10@yahoo.com", "IMS ENRICH - AUTO SCAN CHECK TASK REMIDER", ex.Message + "<br/>" + ex.InnerException?.Message);
            //    }

                return Content("N/A");
            //}
        }
    }
}