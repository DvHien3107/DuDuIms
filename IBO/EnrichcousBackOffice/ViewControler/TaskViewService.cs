using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Enrich.Core.Infrastructure;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services.Notifications;

namespace EnrichcousBackOffice.ViewControler
{
    public class TaskViewService
    {
        /// <summary>
        /// Send email cho member duoc assign sau khi task duoc create or update
        /// </summary>
        /// <param name="type">new|update</param>
        /// <returns></returns>
        public static async Task<string> SendNoticeAfterTaskUpdate(Ts_Task task, string type = "update",P_Member cMem =null)
        {
            try
            {
                string bodyE = "Dear, <br/>";
                string subject = "";    
                string to = "";
                string toMemNo = "";
                string firstname = "";
                string cc = "";
                string link = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/tasksman/detail/" + task.Id;

                var db = new WebDataModel();
                List<string> memberNumberList = new List<string>();
                if (string.IsNullOrEmpty(task.AssignedToMemberNumber) == false)
                {
                    foreach (var item in task.AssignedToMemberNumber?.Split(','))
                    {
                        if(item!=cMem.MemberNumber)
                        {
                            memberNumberList.Add(item);
                        }
                    }
                }
                if (memberNumberList.Count() == 0)
                {
                    return "";
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
                  
                        NotificationService notificationService = new NotificationService();
                        if (task.TicketId != null)
                        {
                            notificationService.TicketNewTaskNotification(memberNumberList, task.Id.ToString(),task.Name, task.CreateBy, task.CreateByMemberNumber);
                        }
                        else
                        {
                            notificationService.NewTaskNotification(memberNumberList, task.Id.ToString(),task.Name, task.CreateBy, task.CreateByMemberNumber);
                        }
                    
                        subject = "[Enrich IMS]You have a new task #" + task.Id;
                        #region body email
                        bodyE += "You have a new task.<br/><br/><b>Task name: </b>" + task.Name + "<br/><br/>" +
                            "<br/><strong>Assigned: </strong><br>" + (task.AssignedToMemberName == null ? "Unassigned" : task.AssignedToMemberName.Replace(",", "<br/>")) +
                            subtaskTable +
                            "<br/><strong>Complete: </strong>" + (task.Complete == true ? "Done" : "Not yet") +
                            "<br/><strong>Create By: </strong>" + task.CreateBy + "<i> at " + task.CreateAt?.ToString("MM/dd/yyyy hh:mm tt") +
                            "<br/><strong>Latest update: </strong>" + task.UpdateBy?.Split('|')[0] + "<br/>";
                        bodyE += "<br/>Please click on link to below to open task.<br/><a href='" + link + "'>" + link + "</a>";
                        #endregion

                }
                else
                {
                 
                        NotificationService notificationService = new NotificationService();
                        if (task.TicketId != null)
                        {
                            notificationService.TicketUpdateTaskNotification(memberNumberList, task.Id.ToString(),task.Name, task.CreateBy, task.CreateByMemberNumber);
                        }
                        else
                        {
                            notificationService.UpdateTaskNotification(memberNumberList, task.Id.ToString(), task.Name, task.CreateBy, task.CreateByMemberNumber);
                        }
                 
                        //update
                        subject = "[Enrich IMS]Your task just had updated.#" + task.Id;
                        #region body email
                        bodyE += "Your task just had updated.#" + task.Id + "<br/><br/><b>Task name: </b>" + task.Name + "<br/>" +
                           "<br/><strong>Assigned: </strong><br>" + (task.AssignedToMemberName == null ? "Unassigned" : task.AssignedToMemberName.Replace(",", "<br/>")) +
                            subtaskTable +
                            "<br/><strong>Complete: </strong>" + (task.Complete == true ? "Done" : "Not yet") +
                            "<br/><strong>Create By: </strong>" + task.CreateBy + "<i> at " + task.CreateAt?.ToString("MM/dd/yyyy hh:mm tt") +
                            "<br/><strong>Latest update: </strong>" + task.UpdateBy?.Split('|')[0] + "<br/>";
                        bodyE += "<br/>Please click on link to below to open task.<br/><a href='" + link + "'>" + link + "</a>";
                        #endregion

                }
                var members = db.P_Member.Where(m => memberNumberList.Contains(m.MemberNumber) == true).ToList();
                foreach (var item in members)
                {
                    to += item.PersonalEmail + ";";
                    toMemNo += item.MemberNumber + ",";
                    firstname += item.FirstName + ";";

                }
                //string result = SendEmail.SendEmailNotice(bodyE, to, firstname, subject, cc).Result;
                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                string result = await _mailingService.SendBySendGrid(to, firstname, subject, bodyE, cc);

                // thong bao theo co che broadcash
                NoticeViewService.SpecifyNotice(subject, "Task name: " + task.Name, "/tasksman/detail/" + task.Id, toMemNo);
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
        public static async Task<string> SendNoticeReminder(Ts_Task task)
        {
            try
            {
                string bodyE = "Dear, <br/>";
                string subject = "";
                string to = "";
                string toMemNo = "";
                string firstname = "";
                string cc = "";
                string link = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/tasksman/detail/" + task.Id;

                var db = new WebDataModel();
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
                bodyE += "Task infomation: <br/><br/>" +
                    "<strong>Task name: </strong><br/>" + task.Name + "<br/>" +
                    "<br/>" + subtaskTable +
                    "<br/><strong>Assigned: </strong><br>" + (task.AssignedToMemberName == null ? "Unassigned" : task.AssignedToMemberName.Replace(",", "<br/>")) +
                        subtaskTable +
                        "<br/><strong>Complete: </strong>" + (task.Complete == true ? "Done" : "Not yet") +
                        "<br/><strong>Create By: </strong>" + task.CreateBy + "<i> at " + task.CreateAt?.ToString("MM/dd/yyyy hh:mm tt") +
                        "<br/><strong>Latest update: </strong>" + task.UpdateBy?.Split('|')[0] + "<br/>";
                bodyE += "<br/>Please click on link to below to open task.<br/><a href='" + link + "'>" + link + "</a>";
                #endregion


                var members = db.P_Member.Where(m => memberNumberList.Contains(m.MemberNumber) == true);
                foreach (var item in members)
                {
                    to += item.PersonalEmail + ";";
                    firstname += item.FirstName + ";";
                    toMemNo += item.MemberNumber + ",";

                }
                //string result = SendEmail.SendEmailNotice(bodyE, to, firstname, subject, cc).Result;
                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                string result = await _mailingService.SendBySendGrid(to, firstname, subject, bodyE, cc);
                NoticeViewService.SpecifyNotice(subject, "Task name: " + task.Name, "/tasksman/detail/" + task.Id, toMemNo);
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }

        }


        public static List<P_Member> GetListMemberNumber(long id)
        {
            WebDataModel db = new WebDataModel();
            List<P_Member> memberList = new List<P_Member>();
            var tic = db.T_SupportTicket.Find(id);
            var memberlist = db.P_Member.ToList();
            memberList.Add(memberlist.Where(m => m.Id == long.Parse(tic.CreateByNumber)).FirstOrDefault());
            if (string.IsNullOrWhiteSpace(tic.AssignedToMemberNumber) == false)
            {
                foreach (var item in tic.AssignedToMemberNumber.Split(new char[] { ',' }))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        memberList.Add(memberlist.Where(m => m.Id == long.Parse(item)).FirstOrDefault());
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(tic.OpenByMemberNumber) == false)
            {
                memberList.Add(
                    memberlist.Where(m => m.Id == long.Parse(tic.OpenByMemberNumber)).FirstOrDefault());
            }
            else if (tic.GroupID > 0)
            {
                var mnIngroup = EmployeesViewService.GetMemberNumberInDept(tic.GroupID ?? 0);
                foreach (string mn in mnIngroup)
                {
                    memberList.Add(
                        memberlist.Where(m => m.Id == long.Parse(mn)).FirstOrDefault());
                }


            }
            if (string.IsNullOrWhiteSpace(tic.ReassignedToMemberNumber) == false)
            {
                memberList.Add(
                    memberlist.Where(m => m.Id == long.Parse(tic.ReassignedToMemberNumber)).FirstOrDefault());
            }
            //if (string.IsNullOrEmpty(tic.EscaladeToStageId))
            //{
            //    var mnInEscaladegroup = EmployeesViewController.GetMemberNumberInDept(tic.EscaladeToStageId);
            //    foreach (string mn in mnInEscaladegroup)
            //    {
            //        memberList.Add(
            //            memberlist.Where(m => m.Id == long.Parse(mn)).FirstOrDefault());
            //    }
            //}
            if (string.IsNullOrWhiteSpace(tic.SubscribeMemberNumber) == false)
            {
                foreach (var item in tic.SubscribeMemberNumber.Split(new char[] { ',' }))
                {
                    if (!string.IsNullOrWhiteSpace(item))
                    {
                        memberList.Add(
                            memberlist.Where(m => m.Id == long.Parse(item)).FirstOrDefault());
                    }
                }
            }
            return memberList;
        }
    }
}