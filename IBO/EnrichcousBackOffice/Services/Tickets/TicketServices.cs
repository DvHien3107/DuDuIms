using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EnrichcousBackOffice.API.DTO;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel.Ticket;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Newtonsoft.Json;

namespace EnrichcousBackOffice.Services
{
    
    public class TicketServices
    {
        public long MakeId()
        {
            using (WebDataModel _db = new WebDataModel())
            {
                int countOfTicket = _db.T_SupportTicket.Count(t => t.CreateAt.Value.Year == DateTime.Today.Year && t.CreateAt.Value.Month == DateTime.Today.Month);
                return long.Parse(DateTime.Now.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("ff"));
            }
        }

        #region API

        /// <summary>
        /// Create new Ticket
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Ticket> CreateTicket(Ticket request)
        {
            using (WebDataModel _db = new WebDataModel())
            {
                T_SupportTicket ticket = new T_SupportTicket();
                // Info
                ticket.Name = request.Title;
                ticket.Description = request.Content;
                // Type
                //ticket.TypeId = (long) UserContent.TICKET_TYPE.Support;
                //ticket.TypeName = "Support";
                // Priority
                ticket.PriorityName = string.IsNullOrEmpty(request.Priority?.Trim() ?? "") ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(request.Priority);
                // Customer
                var cus = _db.C_Customer.FirstOrDefault(c => request.StoreId!=null && c.StoreCode == request.StoreId);
                ticket.CustomerCode = cus?.CustomerCode;
                ticket.CustomerName = cus?.BusinessName;
                ticket.SiteId = cus.SiteId;
                // Group
                switch (request.Type.ToLower() ?? "")
                {
                    case "sales":
                        ticket.GroupID = 19120010;
                        ticket.GroupName = "SALES & MARKETING";
                        break;
                    case "tech":
                        ticket.GroupID = 19120004;
                        ticket.GroupName = "POS SUPPORT - LEVEL 1";
                        break;
                    case "admin":
                        ticket.GroupID = 19120007;
                        ticket.GroupName = "ADMIN SUPPORT - LEVEL 1";
                        break;
                }
                string TypeSupport = BuildInCodeProject.Support_Ticket.ToString();
                var SupportProject = _db.T_Project_Milestone.Where(x => x.BuildInCode == TypeSupport).FirstOrDefault();
                var SupportVersion = _db.T_Project_Milestone.Where(v => v.ParentId == SupportProject.Id && v.Type == "Project_version").FirstOrDefault();
                var SupportDefaultStage = _db.T_Project_Stage.Where(x => x.ProjectId == SupportProject.Id && x.BuildInCode == "default").FirstOrDefault();
                var statusOpen = _db.T_TicketStatus.Where(x => x.Type == "open" & x.ProjectId == SupportProject.Id&&x.Name.Contains("open")).FirstOrDefault();
                if (statusOpen != null)
                {
                    ticket.StatusId = statusOpen.Id.ToString();
                    ticket.StatusName = statusOpen.Name;
                }
                var ticketStatusMapping = new T_TicketStatusMapping();
                ticketStatusMapping.StatusId = statusOpen.Id;
                ticketStatusMapping.StatusName = statusOpen.Name;
                ticket.T_TicketStatusMapping.Add(ticketStatusMapping);
                ticket.ProjectId = SupportProject.Id;
                ticket.ProjectName = SupportProject.Name;
                ticket.VersionId = SupportVersion.Id;
                ticket.VersionName = SupportVersion.Name;
                ticket.StageId = SupportDefaultStage.Id;
                ticket.StageName = SupportDefaultStage.Name;
                ticket.AssignedToMemberName = "";
                ticket.AssignedToMemberNumber = "";

                ticket.CreateAt = DateTime.UtcNow;
                ticket.CreateByName = cus?.BusinessName;
                ticket.CreateByNumber = cus?.CustomerCode;
                ticket.GlobalStatus = Visible.PUBLISH.Text();
                ticket.Visible = true;
                ticket.UpdateTicketHistory = $"{DateTime.UtcNow:dd MMM yyyy hh:mm tt} - by {cus?.BusinessName}|";
                ticket.Id = MakeId();
                //var jsonAssign = new List<JsonAssignedModel>(){new JsonAssignedModel
                //{
                //    StageId = SupportDefaultStage.Id,
                //    StageName = SupportDefaultStage.Name,
                //    AssignMemberNumber = "",
                //    AssignMemberName = ""
                //}};
                //ticket.AssignJson = JsonConvert.SerializeObject(jsonAssign);
                _db.T_SupportTicket.Add(ticket);
                if (request.Attachments?.Count > 0)
                {
                    List<long> idList = new List<long>();
                    request.Attachments.ForEach(attach =>
                    {
                        long newId = DateTime.UtcNow.Ticks;
                        while (idList.Any(id => id == newId)) newId += 1;
                        idList.Add(newId);
                        _db.UploadMoreFiles.Add(new UploadMoreFile
                        {
                            UploadId = newId,
                            FileName = attach,
                            TableId = ticket.Id,
                            TableName = "T_SupportTicket"
                        });
                    });
                }
                await _db.SaveChangesAsync();
                //var ticketStage = new T_TicketStage_Status()
                //{
                //    Id = Guid.NewGuid().ToString("N"),
                //    StageId = SupportDefaultStage.Id,
                //    StageName = SupportDefaultStage.Name,
                //    TicketId = ticket.Id,
                //    Active = true,
                //    ProjectVersionId = SupportVersion.Id,
                //    ProjectVersionName = SupportVersion.Name,
                //    OpenDate = DateTime.UtcNow
                //};
              
            
                //_db.T_TicketStage_Status.Add(ticketStage);
               // _db.SaveChanges();
                return GetTicket(null, ticket.Id).FirstOrDefault();
            }
        }

        /// <summary>
        /// Create new FeedBack
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AppHandleException"></exception>
        public async Task<T_TicketFeedback> CreateFeedBack(Feedback request)
        {
            using (WebDataModel _db = new WebDataModel())
            {
                T_SupportTicket ticket = _db.T_SupportTicket.Find(request.TicketId);
                if (ticket == null) throw new AppHandleException("Ticket not exist!", new Exception("No ticket with id: " + request.TicketId));
                C_Customer customer = _db.C_Customer.FirstOrDefault(c => c.CustomerCode == ticket.CustomerCode);
                T_TicketFeedback feedback = new T_TicketFeedback();
                feedback.TicketId = request.TicketId;
                feedback.FeedbackTitle = request.Title;
                feedback.Feedback = request.Content;
                feedback.CreateByNumber = customer?.CustomerCode;
                feedback.CreateByName = customer?.BusinessName;
                feedback.CreateAt = DateTime.UtcNow;
                feedback.GlobalStatus = Visible.PUBLISH.Text();
                feedback.DateCode = DateTime.UtcNow.ToString("yyyyMMdd");
                feedback.Attachments = request.Attachments?.Count > 0 ? string.Join(";", request.Attachments): null;
                feedback.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                _db.T_TicketFeedback.Add(feedback);
                ticket.UpdateBy = customer?.BusinessName;
                ticket.UpdateAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return feedback;
            }
        }

        /// <summary>
        /// Get Ticket
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="ticketId"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<Ticket> GetTicket(string storeId, long? ticketId = null)
        {
            if (string.IsNullOrEmpty(storeId) && ticketId == null) return null;
            using (WebDataModel _db = new WebDataModel())
            {
                string customer = _db.C_Customer.FirstOrDefault(c => !string.IsNullOrEmpty(storeId) && c.StoreCode == storeId)?.CustomerCode;
                if (string.IsNullOrEmpty(storeId) == false && string.IsNullOrEmpty(customer))
                    throw new AppHandleException("Store not exist!", new Exception($"StoreID : {storeId}"));
                List<Ticket> tickets = _db.T_SupportTicket.Where(t => ((!string.IsNullOrEmpty(storeId) && t.CustomerCode == customer) || (ticketId != null && t.Id == ticketId)) && t.Visible == true).AsEnumerable()
                .Where(t =>  t.GlobalStatus == Visible.PUBLISH.Text()).OrderByDescending(t => t.CreateAt).Select(t => new Ticket
                {
                    Id = t.Id,
                    Title = t.Name,
                    Content = t.Description,
                    StoreId = _db.C_Customer.FirstOrDefault(c => c.CustomerCode == t.CustomerCode)?.StoreCode,
                    Priority = t.PriorityName,
                    Attachments = _db.UploadMoreFiles.Where(up => up.TableId == t.Id && up.TableName == "T_SupportTicket").Select(up => up.FileName).ToList(),
                    Type = TicketType.SUPPORT.Text(),
                    Status = t.StatusName,
                    Feedbacks = _db.T_TicketFeedback.Where(fb => fb.TicketId == t.Id).AsEnumerable()
                    .Where(fb => fb.GlobalStatus == Visible.PUBLISH.Text()).Select(fb => new Feedback
                    {
                        TicketId = fb.TicketId,
                        Id = fb.Id,
                        Title = fb.FeedbackTitle,
                        Content = fb.Feedback,
                        Attachments = fb.Attachments?.Split(';').ToList()
                    }).ToList()
                }).ToList();
                return tickets;
            }
        }
        #endregion

    }
}