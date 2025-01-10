using DataTables.AspNet.Core;
using Enrich.DataTransfer;
using Enrich.IServices;
using Enrich.IServices.Utils.EnrichUniversal;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Extensions;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.CustomizeModel.Ticket;
using EnrichcousBackOffice.Models.CustomizeModel.Ticket_Task_Template;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.Services.Tickets;
using EnrichcousBackOffice.Utils;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.ViewModel;
using Google.Apis.Calendar.v3.Data;
using Hangfire;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static iTextSharp.tool.xml.html.HTML;

namespace EnrichcousBackOffice.Controllers
{
    [MyAuthorize]
    public class Ticket_NewController : UploadController
    {
        #region Field
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        // private WebDataModel db = new WebDataModel();
        //private string TypeDeployment = BuildInCodeProject.Deployment_Ticket.ToString();
        //private string TypeDevelopment = BuildInCodeProject.Development_Ticket.ToString();
        //private string TypeSupport = BuildInCodeProject.Support_Ticket.ToString();  
        private string TypeDeployment = "19120001";
        private string TypeDevelopment = "19120002";
        private string TypeSupport = "19120003";
        private readonly ILogService _logService;
        private readonly EnrichContext enrichContext;
        private readonly IConnectorJiraService _connectorJiraIssueService;
        private const string PageDeployment = "19120001";
        private const string PageSupport = "19120003";
        private const string PageDevelopmentsTicket = "19120002";

        public Ticket_NewController(ILogService logService, EnrichContext enrichContext, IConnectorJiraService connectorJiraIssueService)
        {
            _logService = logService;
            this.enrichContext = enrichContext;
            _connectorJiraIssueService = connectorJiraIssueService;
        }
        #endregion

        #region Utilities

        public IQueryable<Project_StageVersion> GetCurrentStage(WebDataModel db, string Page, bool getall = false)
        {
            try
            {
                //var projectId = "";
                //var stageId = "";
                //var versionId = "";

                var stg_ver = (from stg in db.T_Project_Stage
                               join ver in db.T_Project_Milestone on stg.ProjectId equals ver.ParentId
                               where ver.Type == "Project_version"
                               join project in db.T_Project_Milestone on ver.ParentId equals project.Id
                               where (stg.BuildInCode == "default" && project.BuildInCode == Page)
                               select new Project_StageVersion { stage = stg, version = ver }).Distinct();

                return stg_ver;

                //if (Page == PageDeployment)
                //{
                //    //projectId = Request.Cookies["projectDeploymentId"].Value;
                //    //versionId = Request.Cookies["versionDeploymentId"].Value;
                //    //stageId = "all";
                //    //if (getall) { projectId = "all"; }
                //    //var vers = db.T_Project_Milestone.Where(v => v.Type == "Project_version");
                //    var stg_ver = (from stg in db.T_Project_Stage
                //                   join ver in db.T_Project_Milestone on stg.ProjectId equals ver.ParentId
                //                   where ver.Type == "Project_version"
                //                   join project in db.T_Project_Milestone on ver.ParentId equals project.Id
                //                   where (stg.BuildInCode == "default" && project.BuildInCode == TypeDeployment)
                //                   select new Project_StageVersion { stage = stg, version = ver }).Distinct();
                //    return stg_ver;
                //}
                //else if (Page == PageSupport)
                //{
                //    //projectId = Request.Cookies["projectSupportId"].Value;
                //    //versionId = Request.Cookies["versionSupportId"].Value;
                //    //stageId = "all";

                //    //var vers = db.T_Project_Milestone.Where(v => v.Type == "Project_version");
                //    var stg_ver = (from stg in db.T_Project_Stage
                //                   join ver in db.T_Project_Milestone on stg.ProjectId equals ver.ParentId
                //                   where ver.Type == "Project_version"
                //                   join project in db.T_Project_Milestone on ver.ParentId equals project.Id
                //                   where (stg.BuildInCode == "default" && project.BuildInCode == TypeSupport)
                //                   orderby stg.BuildInCode descending
                //                   select new Project_StageVersion { stage = stg, version = ver }).Distinct();
                //    return stg_ver;
                //}
                //else
                //{
                //    if (Request.Cookies["projectId"] == null)
                //    {
                //        projectId = "all";
                //        stageId = "all";
                //        versionId = "all";
                //        Response.Cookies["projectId"].Value = projectId;
                //        Response.Cookies["projectId"].Expires = DateTime.Now.AddYears(1);
                //        Response.Cookies["stageId"].Value = stageId;
                //        Response.Cookies["stageId"].Expires = DateTime.Now.AddYears(1);
                //        Response.Cookies["versionId"].Value = versionId;
                //        Response.Cookies["versionId"].Expires = DateTime.Now.AddYears(1);
                //    }
                //    else
                //    {
                //        projectId = Request.Cookies["projectId"].Value;
                //        stageId = Request.Cookies["stageId"].Value;
                //        versionId = Request.Cookies["versionId"].Value;
                //    }

                //    if (getall) { projectId = "all"; }
                //    // var vers = db.T_Project_Milestone.Where(v => v.Type == "Project_version");
                //    IQueryable<Project_StageVersion> stg_ver;
                //    var isAdmin = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                //    stg_ver = (from stg in db.T_Project_Stage
                //               where stg.Active != false
                //               join ver in db.T_Project_Milestone on stg.ProjectId equals ver.ParentId
                //               where ver.Type == "Project_version" && ver.Active != false
                //               join project in db.T_Project_Milestone on ver.ParentId equals project.Id
                //               join mem in db.T_Project_Milestone_Member on project.Id equals mem.ProjectId
                //               where (projectId == "all" || (projectId == stg.ProjectId
                //               && (stageId == "all" || stageId == stg.Id)
                //               && (versionId == "all" || versionId == ver.Id)))
                //               && project.BuildInCode == TypeDevelopment
                //               && (mem.MemberNumber == cMem.MemberNumber || isAdmin)
                //               select new Project_StageVersion { stage = stg, version = ver }).Distinct();
                //    return stg_ver;
                //}
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        public async Task SendEmailUpdate(T_SupportTicket tic, string type, string MemberNumber)
        {
            await TicketViewService.SendNoticeAfterTicketUpdate(tic, type, null, MemberNumber);
        }

        public IQueryable<T_SupportTicket> TicketFilterFactory(WebDataModel db, DeploymentTicket_request request, IQueryable<T_SupportTicket> ticketList)
        {
            // filter from date
            if (request.From != null)
            {
                var From = request.From.Value.IMSToUTCDateTime();
                ticketList = ticketList.Where(x => x.CreateAt >= From);
            }
            // filter to date
            if (request.To != null)
            {
                var To = request.To.Value.IMSToUTCDateTime();
                ticketList = ticketList.Where(x => x.CreateAt <= To);
            }
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                ticketList = ticketList.Where(x => x.Name.Contains(request.SearchText) || x.CustomerName.Contains(request.SearchText) || SqlFunctions.StringConvert((double)x.Id).Contains(request.SearchText));

            }
            // check type
            //if (!string.IsNullOrEmpty(request.TypeDevelop))
            //{
            //    ticketList = ticketList.Where(tk => tk.SupportTicket.TypeName.Equals(request.TypeDevelop, StringComparison.OrdinalIgnoreCase));
            //}
            if (request.TypeDevelop != null && request.TypeDevelop.Count() > 0)
            {
                ticketList = ticketList.Where(x => request.TypeDevelop.Any(y => x.TypeName.Contains(y)));

            }
            // filter by status
            if (request.Status != null && request.Status.Count() > 0)
            {
                if (request.Status.Any(y => y == "OPEN"))
                {

                    ticketList = ticketList.Where(x => string.IsNullOrEmpty(x.StatusId) || request.Status.Any(y => x.StatusName.Contains(y)));
                }
                else
                {
                    ticketList = ticketList.Where(x => request.Status.Any(y => x.StatusName.Contains(y)));
                }
            }
            // filter by department
            if (request.Departments != null && request.Departments.Count() > 0)
            {
                var departmentIds = new List<long>();
                request.Departments.ForEach(item => departmentIds.AddRange(item.Split(',').Select(long.Parse)));
                ticketList = ticketList.Where(x => x.GroupID != null);
                ticketList = ticketList.Where(x => departmentIds.Any(y => x.GroupID == y));
            }

            // filter by priority
            if (request.Priority != null && request.Priority.Count() > 0)
            {
                var priorityIds = request.Priority.Select(int.Parse).ToList();
                if (request.Priority.Any(y => y == "0"))
                {
                    ticketList = ticketList.Where(x => x.PriorityId == null || priorityIds.Any(y => x.PriorityId == y));
                }
                else
                {
                    ticketList = ticketList.Where(x => priorityIds.Any(y => x.PriorityId == y));
                }

            }
            // filter by severity
            if (request.Severity != null && request.Severity.Count() > 0)
            {
                if (request.Severity.Any(y => y == 0))
                {
                    ticketList = ticketList.Where(x => x.SeverityId == null || request.Severity.Any(y => x.SeverityId == y));
                }
                else
                {
                    ticketList = ticketList.Where(x => request.Severity.Any(y => x.SeverityId == y));
                }
            }
            // filter by tag
            if (request.Tags != null && request.Tags.Count() > 0)
            {
                if (request.Tags.Any(y => y == "0"))
                {

                    ticketList = ticketList.Where(x => string.IsNullOrEmpty(x.Tags) || request.Tags.Any(y => x.Tags.Contains(y)));
                }
                else
                {
                    ticketList = ticketList.Where(x => request.Tags.Any(y => x.Tags.Contains(y)));
                }
            }


            // filter by tab
            switch (request.Tab)
            {
                case "open":
                    ticketList = ticketList.Where(tk => tk.Visible == true && tk.DateClosed == null);
                    break;
                case "unassigned":
                    ticketList = ticketList.Where(tk => tk.Visible == true && string.IsNullOrEmpty(tk.AssignedToMemberNumber) && tk.DateClosed == null);
                    break;
                case "closed":
                    ticketList = ticketList.Where(tk => tk.Visible == true && tk.DateClosed != null);
                    if (!string.IsNullOrEmpty(request.CloseAt))
                    {
                        DateTime sdate = DateTime.Parse("1/1/1900 0:0:0");
                        DateTime edate = DateTime.UtcNow;
                        //DateTime edate = DateTime.Parse("1/1/2020 0:0:0");

                        if (request.CloseAt.Equals("today"))
                        {
                            sdate = edate;
                            edate = edate.AddDays(1);
                        }
                        else if (request.CloseAt.Equals("yesterday"))
                        {
                            sdate = edate.AddDays(-1);
                        }
                        else if (request.CloseAt.Equals("thisweek"))
                        {
                            var cul = System.Threading.Thread.CurrentThread.CurrentCulture;
                            var diff = edate.DayOfWeek - cul.DateTimeFormat.FirstDayOfWeek;
                            diff = diff < 0 ? diff + 7 : diff;
                            sdate = edate.AddDays(-diff);
                            edate = edate.AddDays(1);
                        }
                        else if (request.CloseAt.Equals("lastweek"))
                        {
                            var cul = System.Threading.Thread.CurrentThread.CurrentCulture;
                            var diff = edate.DayOfWeek - cul.DateTimeFormat.FirstDayOfWeek;
                            diff = diff < 0 ? diff + 7 : diff;
                            sdate = edate.AddDays(-diff - 7);
                            edate = edate.AddDays(-diff);
                        }
                        else if (request.CloseAt.Equals("thismonth"))
                        {
                            sdate = new DateTime(edate.Year, edate.Month, 1);
                            edate = edate.AddDays(1);
                        }
                        else if (request.CloseAt.Equals("lastmonth"))
                        {
                            edate = new DateTime(edate.Year, edate.Month, 1);
                            sdate = edate.AddMonths(-1);
                        }

                        sdate = Convert.ToDateTime(sdate.ToString("MM/dd/yyyy") + " 0:0:0");
                        edate = Convert.ToDateTime(edate.ToString("MM/dd/yyyy") + " 0:0:0");
                        ticketList = ticketList.Where(tk => tk.DateClosed >= sdate && tk.DateClosed < edate);
                    }
                    break;
                case "all":

                    break;
                case "invisible":
                    ticketList = ticketList.Where(tk => tk.Visible != true);
                    break;
                //open
                default:
                    ticketList = ticketList.Where(tk => tk.Visible == true && tk.DateClosed == null);
                    break;
            }
            //filter by assigned
            if (!string.IsNullOrEmpty(request.FilterBy))
            {
                if (request.FilterBy == "openby")
                {
                    if (request.OpenBy != null && request.OpenBy.Count() > 0)
                    {
                        if (request.OpenBy.Any(x => x.Contains("System")))
                        {
                            ticketList = ticketList.Where(tk => tk.CreateByName.Contains("System") || string.IsNullOrEmpty(tk.CreateByName) || request.OpenBy.Any(x => tk.CreateByNumber.Contains(x)));
                        }
                        else
                        {
                            ticketList = ticketList.Where(tk => request.OpenBy.Any(x => tk.CreateByNumber.Contains(x)));
                        }
                    }
                }
                else if (request.FilterBy == "assigned")
                {
                    if (request.MemberNumber != null && request.MemberNumber.Count() > 0)
                    {
                        ticketList = ticketList.Where(tk => request.MemberNumber.Any(x => tk.SubscribeMemberNumber.Contains(x)
                      || tk.AssignedToMemberNumber.Contains(x)
                      || tk.ReassignedToMemberNumber.Contains(x)));
                    }
                }
                else if (request.FilterBy == "membertag")
                {
                    if (request.MemberTag != null && request.MemberTag.Count() > 0)
                    {

                        ticketList = ticketList.Where(tk => request.MemberTag.Any(x => tk.TagMemberNumber.Contains(x)));
                    }
                }
                else if (request.FilterBy == "salon")
                {
                    if (request.CustomerCode != null && request.CustomerCode.Count() > 0)
                    {
                        ticketList = ticketList.Where(tk => request.CustomerCode.Any(x => tk.CustomerCode.Contains(x)));
                    }
                }
                else if (request.FilterBy == "license")
                {
                    if (request.LicenseCode != null && request.LicenseCode.Count() > 0)
                    {
                        ticketList = from ticket in ticketList join service in db.Store_Services on ticket.CustomerCode equals service.CustomerCode where request.LicenseCode.Any(y => y == service.ProductCode) && service.Active == 1 && service.Type == "license" select ticket;
                    }
                }
            }
            return ticketList;
        }

        #endregion

        #region Index
        //Index

        public ActionResult Index(string Page, string projectId, string stageId, string versionId, string typeid)
        {
            try
            {
                if (Page == "0")
                {
                    Page = PageDeployment;
                }

                ViewBag.Typeid = typeid;


                if (projectId == "0")
                {
                    projectId = "all";
                }
                if (stageId == "0")
                {
                    stageId = "all";
                }
                if (versionId == "0")
                {
                    versionId = "all";
                }



                var TicketModel = new IndexTicketModel();
                //DateTime From = DateTime.UtcNow.AddMonths(-3);
                //TicketModel.FromDate = new DateTime(From.Year, From.Month, 1);
                //TicketModel.ToDate = DateTime.UtcNow;
                var AccessDeployment = false;
                var AccessSupport = true;
                var AccessDevelopments = false;
                var db = new WebDataModel();
                var Project = db.T_Project_Milestone.FirstOrDefault(x => x.BuildInCode == Page);
                var Version = db.T_Project_Milestone.Where(v => v.ParentId == Project.Id && v.Type == "Project_version").FirstOrDefault();
                var IsAdminOrViewAllTicket = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                if (IsAdminOrViewAllTicket)
                {
                    AccessDeployment = true;
                    //AccessSupport = true;
                    AccessDevelopments = true;
                }
                else
                {
                    if (cMem.DepartmentId?.Contains("19120001") == true || cMem.DepartmentId?.Contains("19120002") == true || cMem.DepartmentId?.Contains("19120003") == true)
                    {
                        AccessDeployment = true;
                    }
                    var check_access_devTicket = (access.Any(k => k.Key.Equals("development_ticket_view")) == true && access["development_ticket_view"] == true);
                    if (check_access_devTicket)
                    {
                        AccessDevelopments = true;
                    }
                }
                // hard code disable siteId for Partner
                if (cMem.SiteId != 1)
                {
                    AccessDevelopments = false;
                }
                ViewBag.AccessDeployment = AccessDeployment;
                ViewBag.AccessSupport = AccessSupport;
                ViewBag.AccessDevelopments = AccessDevelopments;
                if (string.IsNullOrEmpty(Page) && Page != PageDeployment && Page != PageSupport && Page != PageDevelopmentsTicket)
                {
                    if (Request.Cookies["TicketPage"] != null)
                    {
                        Page = Request.Cookies["TicketPage"].Value;
                    }
                    else if (AccessDeployment)
                    {
                        Page = PageDeployment;
                    }
                    else if (AccessSupport)
                    {
                        Page = PageSupport;
                    }
                    else
                    {
                        Page = PageDevelopmentsTicket;
                    }
                }
                switch (Page)
                {
                    // tab deployment
                    case PageDeployment:
                    DeploymentTicket:
                        if (!AccessDeployment)
                        {
                            goto T_SupportTicket;
                            return RedirectToAction("forbidden", "home");
                        }
                        var projectDevelopmentId = Project.Id;
                        var versionDevelopmentId = Version.Id;
                        var stageDevelopmentId = "all";
                        if (Request.Cookies["stageDeploymentId"] != null && Request.Cookies["stageDeploymentId"].Value != "all")
                        {
                            stageDevelopmentId = Request.Cookies["stageDeploymentId"].Value;
                            var DeploymentStage = db.T_Project_Stage.Where(t => t.Id == stageDevelopmentId).FirstOrDefault();
                            if (DeploymentStage == null)
                            {
                                stageDevelopmentId = "all";
                            }
                            else
                            {
                                TicketModel.Stage = DeploymentStage;
                            }
                        }
                        Response.Cookies["stageDeploymentId"].Value = stageDevelopmentId;
                        Response.Cookies["stageDeploymentId"].Expires = DateTime.Now.AddYears(1);
                        Response.Cookies["projectDeploymentId"].Value = projectDevelopmentId;
                        Response.Cookies["projectDeploymentId"].Expires = DateTime.Now.AddYears(1);
                        Response.Cookies["versionDeploymentId"].Value = versionDevelopmentId;
                        Response.Cookies["versionDeploymentId"].Expires = DateTime.Now.AddYears(1);
                        TicketModel.Project = Project;
                        TicketModel.Version = Version;
                        TicketModel.AllProject = projectDevelopmentId == "all";
                        TicketModel.AllStage = stageDevelopmentId == "all" || stageId == "all";
                        TicketModel.AllVersion = versionDevelopmentId == "all" || versionId == "all";
                        TicketModel.ListStage = db.T_Project_Stage.Where(x => x.ProjectId == Project.Id).OrderByDescending(x => x.BuildInCode).ToList();
                        TicketModel.Page = PageDeployment;
                        ViewBag.Title = "Enrich IMS - Deployment Ticket";
                        break;
                    //support


                    case PageSupport:
                    T_SupportTicket:
                        if (!AccessSupport)
                        {
                            goto DevelopmentTicket;
                        }

                        var projectSupportId = Project.Id;
                        var versionSupportId = Version.Id;
                        var stageSupportId = "all";
                        if (Request.Cookies["stageSupportId"] != null && Request.Cookies["stageSupportId"].Value != "all")
                        {
                            stageSupportId = Request.Cookies["stageSupportId"].Value;
                            var SupportStage = db.T_Project_Stage.Where(t => t.Id == stageSupportId).FirstOrDefault();
                            if (SupportStage == null)
                            {
                                stageSupportId = "all";
                            }
                            else
                            {
                                TicketModel.Stage = SupportStage;
                            }

                        }

                        Response.Cookies["projectSupportId"].Value = projectSupportId;
                        Response.Cookies["projectSupportId"].Expires = DateTime.Now.AddYears(1);
                        Response.Cookies["stageSupportId"].Value = stageSupportId;
                        Response.Cookies["stageSupportId"].Expires = DateTime.Now.AddYears(1);
                        Response.Cookies["versionSupportId"].Value = versionSupportId;
                        Response.Cookies["versionSupportId"].Expires = DateTime.Now.AddYears(1);
                        TicketModel.Project = Project;
                        TicketModel.Version = Version;
                        TicketModel.AllProject = projectSupportId == "all";
                        TicketModel.AllStage = stageSupportId == "all" || stageId == "all";
                        TicketModel.AllVersion = versionSupportId == "all" || versionId == "all";
                        TicketModel.ListStage = db.T_Project_Stage.Where(x => x.ProjectId == Project.Id).OrderByDescending(x => x.BuildInCode).ToList();

                        TicketModel.Page = PageSupport;
                        ViewBag.Title = "Enrich IMS - Deployment Ticket";

                        break;
                    //tab Develop
                    case PageDevelopmentsTicket:
                    DevelopmentTicket:
                        //check permision
                        if (!AccessDevelopments)
                        {
                            return RedirectToAction("forbidden", "home");
                        }
                        //get and set stage
                        if (string.IsNullOrEmpty(projectId)) projectId = Request.Cookies["projectId"]?.Value.ToString();
                        if (string.IsNullOrEmpty(stageId)) stageId = Request.Cookies["stageId"]?.Value.ToString();
                        if (string.IsNullOrEmpty(versionId)) versionId = Request.Cookies["versionId"]?.Value.ToString();

                        var vers = db.T_Project_Milestone.Where(v => v.Type == "Project_version");
                        IQueryable<Project_StageVersion> stg_ver;
                        stg_ver = (from stg in db.T_Project_Stage
                                   join ver in vers on stg.ProjectId equals ver.ParentId
                                   let mem = db.T_Project_Milestone_Member.Where(m => m.ProjectId == stg.ProjectId && (m.MemberNumber == cMem.MemberNumber || IsAdminOrViewAllTicket)).Count()
                                   where (projectId == "all" || (projectId == stg.ProjectId
                                   && (stageId == "all" || stageId == stg.Id)
                                   && (versionId == "all" || versionId == ver.Id)))
                                   && mem > 0
                                   select new Project_StageVersion { stage = stg, version = ver }).Distinct();
                        if (stg_ver.Count() == 0)
                        {
                            if (Response.Cookies["projectId"] != null)
                            {
                                Response.Cookies["projectId"].Expires = DateTime.Now.AddDays(-1);
                            }
                            if (Response.Cookies["stageId"] != null)
                            {
                                Response.Cookies["stageId"].Expires = DateTime.Now.AddDays(-1);
                            }
                            if (Response.Cookies["versionId"] != null)
                            {
                                Response.Cookies["versionId"].Expires = DateTime.Now.AddDays(-1);
                            }

                            var projects = (from d in db.T_Project_Milestone
                                            where d.Type == "project" && (d.BuildInCode == TypeDevelopment)
                                            join member in db.T_Project_Milestone_Member on d.Id equals member.ProjectId
                                            where member.MemberNumber == cMem.MemberNumber || IsAdminOrViewAllTicket
                                            select d).Distinct().ToList();

                            var stages = db.T_Project_Stage.AsEnumerable().Where(s => projects.Any(x => x.Id == s.ProjectId)).ToList();
                            ViewBag.projects = projects;
                            return View("SelectStage", stages);
                        }
                        else
                        {
                            Response.Cookies["projectId"].Value = projectId;
                            Response.Cookies["projectId"].Expires = DateTime.Now.AddYears(1);
                            Response.Cookies["stageId"].Value = stageId;
                            Response.Cookies["stageId"].Expires = DateTime.Now.AddYears(1);
                            Response.Cookies["versionId"].Value = versionId;
                            Response.Cookies["versionId"].Expires = DateTime.Now.AddYears(1);

                            TicketModel.Project = db.T_Project_Milestone.Find(stg_ver.FirstOrDefault().stage.ProjectId);
                            TicketModel.Stage = stg_ver.FirstOrDefault().stage;
                            TicketModel.Version = stg_ver.FirstOrDefault().version;
                            TicketModel.AllProject = projectId == "all";
                            TicketModel.AllStage = projectId == "all" || stageId == "all";
                            TicketModel.AllVersion = projectId == "all" || versionId == "all";
                            TicketModel.Page = PageDevelopmentsTicket;
                            TicketModel.BuildInCode = TicketModel.Project.BuildInCode;
                            ViewBag.Title = "Enrich IMS - Developments Ticket";
                        }
                        break;
                    //open
                    default:
                        goto DeploymentTicket;
                }
                Response.Cookies["TicketPage"].Value = Page;
                Response.Cookies["TicketPage"].Expires = DateTime.Now.AddYears(1);
                return View(TicketModel);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][Index] error when view ");
                throw ex;
            }
        }
        //LOAD select STAGE
        public ActionResult _Partial_SelectStage()
        {
            try
            {
                _logService.Info($"[Ticket][SelectStage] start select stage");
                var db = new WebDataModel();
                IQueryable<T_Project_Stage> stages;
                var AccessAllTicket = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                var projectsQuery = db.T_Project_Milestone.Where(x => x.Type == "project" && x.Active == true && x.BuildInCode == TypeDevelopment);
                if (!AccessAllTicket)
                {
                    var accessProjectByMembers = db.T_Project_Milestone_Member.Where(x => x.MemberNumber == cMem.MemberNumber).GroupBy(x => x.ProjectId).Select(x => x.Key);
                    projectsQuery = projectsQuery.Where(x => accessProjectByMembers.Any(y => y == x.Id));
                }
                var projects = projectsQuery.ToList();
                var ListStages = db.T_Project_Stage.Where(s => projectsQuery.Any(x => x.Id == s.ProjectId) && s.Active != false).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToList();
                ViewBag.projects = projects.ToList();
                // ViewBag.versions = db.T_Project_Milestone.Where(v => v.ParentId != null && v.Type == "version").ToList();
                ViewBag.reSelect = true;
                _logService.Info($"[Ticket][SelectStage] complete select stage");
                return PartialView("_Partial_SelectStage", ListStages);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][SelectStage] error when select stage");
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex);
                throw;
            }

        }
        [HttpGet]
        //load filter 
        public async Task<JsonResult> LoadAllTicketFilter(string Page)
        {
            try
            {
                var types = new List<SelectListItem>();
                var status = new List<SelectListItem>();
                // var member = new List<SelectListItem>();
                //  var openBy = new List<SelectListItem>();
                var severities = new List<T_TicketSeverity>();
                var priorities = new List<T_Priority>();
                var tags = new List<T_Tags>();
                var version = new List<SelectListItem>();
                var licenses = new List<SelectListItem>();
                var departments = new List<SelectListItem>();
                var db = new WebDataModel();
                string projectId = "";

                //for deployment
                if (Page == PageDeployment)
                {
                    projectId = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDeployment && x.Type == "project").FirstOrDefault().Id;
                    //  var proj_info = GetCurrentStage(db, PageDeployment);
                    types = await db.T_TicketType.Where(t => t.SpecialType == "DEPLOYMENT" && t.IsDeleted != true && t.SiteId == cMem.SiteId).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.TypeName }).ToListWithNoLockAsync();

                    status = db.T_TicketStatus.Where(s => (projectId == s.ProjectId || projectId == "all") && s.IsDeleted != true && s.SiteId == cMem.SiteId).OrderBy(s => s.Order).AsEnumerable().Select(s => new SelectListItem { Group = new SelectListGroup { Name = s.Type }, Value = s.Name.ToUpper(), Text = s.Name.ToUpper() }).GroupBy(p => new { p.Value, p.Text }).Select(g => g.First()).ToList();
                    // var membersSameGroup = TicketViewService.GetMembersInTheSameGroup(db, cMem.MemberNumber);
                    // var viewall = access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true;

                    //  var memberDeployment = await db.P_Member.AsNoTracking().Where(t => t.DepartmentId.Contains("19120001") && t.Active == true).OrderBy(t => t.FullName).Select(t => new { t.MemberNumber, t.FullName }).ToListWithNoLockAsync();
                    //  member.AddRange(memberDeployment.Where(x => !member.Any(m => m.Value == x.MemberNumber)).Select(x => new SelectListItem { Value = x.MemberNumber, Text = x.FullName }));
                    severities = await db.T_TicketSeverity.Where(s => s.SpecialType.Equals("DEPLOYMENT", StringComparison.OrdinalIgnoreCase) && s.Active == true).OrderBy(s => s.SeverityLevel).ToListWithNoLockAsync();
                    tags = db.T_Tags.Where(s => s.Type == PageDeployment && s.SiteId == cMem.SiteId).OrderBy(s => s.Name).ToList();
                }
                else
                {
                    //  var proj_info = GetCurrentStage(db,Page);

                    if (Page == PageSupport)
                    {
                        projectId = db.T_Project_Milestone.AsNoTracking().Where(x => x.BuildInCode == TypeSupport && x.Type == "project").FirstOrDefault().Id;
                    }
                    else
                    {
                        projectId = Request.Cookies["projectId"].Value;
                    }

                    types = db.T_TicketType.Where(s => (projectId == s.ProjectId || projectId == "all") && s.SpecialType == "DEVELOPMENT" && s.SiteId == cMem.SiteId && s.IsDeleted != true).AsEnumerable().Select(s => new SelectListItem { Value = s.TypeName.ToUpper(), Text = s.TypeName.ToUpper() }).GroupBy(p => new { p.Value, p.Text }).Select(g => g.First()).ToList();
                    status = db.T_TicketStatus.Where(s => (projectId == s.ProjectId || projectId == "all") && s.SiteId == cMem.SiteId && s.IsDeleted != true).OrderBy(s => s.Order).AsEnumerable().Select(s => new SelectListItem { Group = new SelectListGroup { Name = s.Type }, Value = s.Name.ToUpper(), Text = s.Name.ToUpper() }).GroupBy(p => new { p.Value, p.Text }).Select(g => g.First()).ToList();
                    if (!status.Any(s => s.Value == "OPEN")) { status.Add(new SelectListItem { Group = new SelectListGroup { Name = "open" }, Value = "OPEN", Text = "OPEN" }); }
                    status = status.OrderByDescending(s => s.Value.Contains("OPEN")).ThenBy(s => s.Value.Contains("CLOSE")).ToList();
                    severities = db.T_TicketSeverity.Where(s => s.SpecialType.Equals("DEVELOPMENT", StringComparison.OrdinalIgnoreCase) && s.Active == true).OrderBy(s => s.SeverityLevel).ToList();

                    if (Page == PageSupport)
                    {
                        //var memberSupport = db.P_Member.Where(t => (t.DepartmentId.Contains("19120002") || t.DepartmentId.Contains("19120003")) && t.Active == true).OrderBy(t => t.FullName).Select(t => new SelectListItem { Value = t.MemberNumber, Text = t.FullName }).ToList();
                        //memberSupport.ForEach(item =>
                        //{
                        //    member.AddRange(memberSupport.Where(x => !member.Any(m => m.Value == x.Value)).Select(x => x));
                        //});
                    }
                    else
                    {
                        //member = (from s_m in db.T_Project_Milestone_Member
                        //          where (projectId == s_m.ProjectId || projectId == "all")
                        //          join m in db.P_Member on s_m.MemberNumber equals m.MemberNumber
                        //          select new SelectListItem { Value = m.MemberNumber, Text = m.FullName }).AsEnumerable().GroupBy(x => x.Value).Select(x => x.First()).ToList();
                        version = (from project in db.T_Project_Milestone join ver in db.T_Project_Milestone on project.Id equals ver.ParentId where (projectId == project.Id || projectId == "all") && project.BuildInCode == TypeDevelopment && ver.Type == "Project_version" && ver.Active != false orderby ver.Order orderby ver.Name select ver).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
                    }

                    tags = db.T_Tags.Where(s => s.Type != PageDeployment && s.SiteId == cMem.SiteId).OrderBy(s => s.Name).ToList();
                }

                // department filter
                if (Page != PageDevelopmentsTicket)
                {
                    var allDepartments = new List<P_Department>();
                    if (Page == PageDeployment)
                    {
                        allDepartments = db.P_Department.AsNoTracking().Where(x => x.Type == "DEPLOYMENT" && x.SiteId == cMem.SiteId).ToList();
                    }
                    else
                    {
                        allDepartments = db.P_Department.AsNoTracking().Where(x => x.Type == "SUPPORT" && x.SiteId == cMem.SiteId).ToList();
                    }

                    foreach (var dep in allDepartments.Where(x => x.ParentDepartmentId == null))
                    {
                        var allDepartmentChildIds = new List<string>();
                        var allDepartmentChild = new List<SelectListItem>();
                        foreach (var depchild in allDepartments.Where(x => x.ParentDepartmentId == dep.Id))
                        {
                            allDepartmentChild.Add(new SelectListItem { Value = depchild.Id.ToString(), Text = "--- " + depchild.Name });
                        }
                        //add select parent
                        var parentIds = allDepartmentChild.Select(x => x.Value).ToList();
                        parentIds.Add(dep.Id.ToString());
                        departments.Add(new SelectListItem
                        {
                            Value = string.Join(",", parentIds),
                            Text = dep.Name
                        });
                        // add all childs
                        departments.AddRange(allDepartmentChild);
                    }
                }
                priorities = await db.T_Priority.Where(x => x.IsDeleted != true).OrderBy(x => x.DisplayOrder).ToListWithNoLockAsync();
                //member = member.Where(x => x.Value != cMem.MemberNumber).ToList();
                //openBy.AddRange(member);
                //openBy.Insert(0, new SelectListItem { Value = "System", Text = "System" });
                //openBy.Insert(0, new SelectListItem { Value = cMem.MemberNumber, Text = "Me" });
                //member.Insert(0, new SelectListItem { Value = cMem.MemberNumber, Text = "Me" });
                string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
                var merchant = (from c in db.C_Customer
                                let order = db.O_Orders.Where(o => c.CustomerCode == o.CustomerCode).Count() > 0
                                where c.Active != 0 && (order || c.Type == STORE_IN_HOUSE) && (cMem.SiteId == 1 || c.SiteId == cMem.SiteId)
                                orderby c.Id descending
                                select c).Distinct().Select(c => new { c.CustomerCode, c.BusinessName, c.BusinessAddressStreet, c.BusinessCity, c.BusinessState, c.BusinessZipCode, c.BusinessCountry }).ToList();
                licenses = db.License_Product.Where(x => x.Active == true && x.Type == "license" && (x.SiteId == cMem.SiteId || cMem.SiteId == 1)).OrderBy(x => x.Level).ThenByDescending(x => x.Price).Select(x => new SelectListItem { Value = x.Code, Text = x.Name }).ToList();
                var ListMember = db.P_Member.Where(x => x.Active != false && x.Delete != true && x.SiteId == cMem.SiteId && x.MemberNumber != cMem.MemberNumber).OrderBy(x => x.MemberNumber).Select(x => new SelectListItem { Value = x.MemberNumber, Text = x.FullName }).ToList();
                ListMember.Insert(0, new SelectListItem { Value = "System", Text = "System" });
                ListMember.Insert(0, new SelectListItem { Value = cMem.MemberNumber, Text = "Me" });

                return Json(new { types, status, priorities, tags, severities, member = ListMember, openBy = ListMember, merchant, version, licenses, departments }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][List] error when load list filter");
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        //LOAD DATA TABLE
        [HttpPost]
        public async Task<JsonResult> LoadTicketTable(DeploymentTicket_request request, IDataTablesRequest dataTablesRequest)
        {
            try
            {
                _logService.Info($"[Ticket][List] start load list ticket");
                var db = new WebDataModel();
                IQueryable<T_SupportTicket> ticketList;
                var stg_ver = GetCurrentStage(db, request.Page);
                if (request.Page == PageDeployment)
                {
                    if (request.StageId != "all")
                    {
                        stg_ver = stg_ver.Where(x => x.stage.Id == request.StageId);
                    }
                    Response.Cookies["stageDeploymentId"].Value = request.StageId;
                    Response.Cookies["stageDeploymentId"].Expires = DateTime.Now.AddYears(1);
                }

                else if (request.Page == PageSupport)
                {
                    if (request.StageId != "all")
                    {
                        stg_ver = stg_ver.Where(x => x.stage.Id == request.StageId);
                    }
                    Response.Cookies["stageSupportId"].Value = request.StageId;
                    Response.Cookies["stageSupportId"].Expires = DateTime.Now.AddYears(1);
                }
                ticketList = from tic in db.T_SupportTicket where tic.SiteId == cMem.SiteId || (tic.SiteId == null && cMem.SiteId == 1) select tic;
                //hard code by siteid
                //  ticketList = ticketList.Where(x => x.SiteId == cMem.SiteId);
                //filter by project

                if (request.Page == PageDevelopmentsTicket)
                {
                    request.ProjectId = Request.Cookies["ProjectId"].Value;
                    if (request.ProjectId != "all")
                    {
                        ticketList = ticketList.Where(x => x.ProjectId == request.ProjectId);
                        if ((request.StageId ?? "all") != "all")
                        {
                            ticketList = ticketList.Where(x => x.StageId.Contains(request.StageId));
                        }

                        if ((request.VersionId ?? "all") != "all")
                        {
                            ticketList = ticketList.Where(x => x.VersionId.Contains(request.VersionId));
                        }
                    }
                    else
                    {
                        var viewAll = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                        IQueryable<T_Project_Milestone> listProjectAvailabel;
                        if (viewAll == true)
                        {
                            listProjectAvailabel = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDevelopment && x.Type == "project" && x.Active == true);
                        }
                        else
                        {
                            listProjectAvailabel = from project in db.T_Project_Milestone
                                                   join memberProject in db.T_Project_Milestone_Member on project.Id equals memberProject.ProjectId
                                                   where project.Active == true && project.Type == "project" && memberProject.MemberNumber == cMem.MemberNumber && project.BuildInCode == TypeDevelopment
                                                   group project by project.Id into project
                                                   select project.FirstOrDefault();
                        }
                        ticketList = ticketList.Where(x => listProjectAvailabel.Any(y => y.Id == x.ProjectId));
                    }
                }
                //filter by project deployment
                else if (request.Page == PageDeployment)
                {
                    var projectDeployment = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDeployment).FirstOrDefault();
                    ticketList = ticketList.Where(x => x.ProjectId == projectDeployment.Id);
                    var aa = ticketList.OrderByDescending(x => x.GroupID).ToList();
                }
                //filter by project support
                else
                {
                    var projectSupport = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeSupport).FirstOrDefault();
                    ticketList = ticketList.Where(x => x.ProjectId == projectSupport.Id);
                }

                //check access permission 
                if (!(cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true)))
                {

                    var checkViewGroup = access.Any(k => k.Key.Equals("ticket_view")) == true && access["ticket_view"] == true;
                    if (request.Page == PageDeployment)
                    {
                        var IsDirectorDelivery = db.P_Department.Any(x => x.LeaderNumber.Contains(cMem.MemberNumber) && x.Type == "DEPLOYMENT" && x.ParentDepartmentId == null);
                        var DeploymentGroupMember = db.P_Department.Where(x => x.Type == "DEPLOYMENT" && x.GroupMemberNumber.Contains(cMem.MemberNumber) && x.ParentDepartmentId != null);

                        // var all = db.P_Department.Any(x => x.LeaderNumber.Contains(cMem.MemberNumber) && x.Type == "DEPLOYMENT" && x.ParentDepartmentId == null);
                        ticketList = ticketList.Where(tk => IsDirectorDelivery || DeploymentGroupMember.Any(x => (x.LeaderNumber.Contains(cMem.MemberNumber) && tk.GroupID == x.Id) || x.GroupMemberNumber.Contains(cMem.MemberNumber) && x.Id == tk.GroupID && checkViewGroup) || tk.AssignedToMemberNumber.Contains(cMem.MemberNumber) || tk.CreateByNumber == cMem.MemberNumber || tk.TagMemberNumber.Contains(cMem.MemberNumber));
                    }
                    else if (request.Page == PageSupport)
                    {
                        var SupportDepartmentIdLeader = from d in db.P_Department
                                                        where d.LeaderNumber.Contains(cMem.MemberNumber) && (d.Type == "SUPPORT")
                                                        select new
                                                        {
                                                            Id = d.Id,
                                                            childGroup = from c in db.P_Department
                                                                         where c.ParentDepartmentId == d.Id
                                                                         select new
                                                                         {
                                                                             Id = c.Id,
                                                                         }
                                                        };
                        var DeploymentGroupMember = db.P_Department.Where(x => x.GroupMemberNumber.Contains(cMem.MemberNumber) && x.Type == "SUPPORT" && x.ParentDepartmentId != null);
                        ticketList = ticketList.Where(tk =>
                        tk.AssignedToMemberNumber.Contains(cMem.MemberNumber)
                       || tk.CreateByNumber == cMem.MemberNumber
                       || DeploymentGroupMember.Any(x => tk.GroupID == x.Id && checkViewGroup)
                       //|| GetListStageAndVersionLeader.Any(y => y.StageId == tk.Stage.StageId && y.ProjectVersionId == tk.Stage.ProjectVersionId)
                       || SupportDepartmentIdLeader.Any(x => x.Id == tk.GroupID || x.childGroup.Any(y => y.Id == tk.GroupID))
                       || tk.TagMemberNumber.Contains(cMem.MemberNumber)
                       );
                    }

                }

                #region filter

                // count number ticket
                int ticketOpenCount = ticketList.Where(tk => tk.Visible == true && tk.DateClosed == null).Count();
                int ticketAllCount = ticketList.Count();
                int ticketUnassignedCount = ticketList.Where(tk => tk.Visible == true && string.IsNullOrEmpty(tk.AssignedToMemberNumber) && tk.DateClosed == null).Count();
                int ticketClosedCount = ticketList.Where(tk => tk.Visible == true && tk.DateClosed != null).Count();
                int ticketinvisibleCount = ticketList.Where(tk => tk.Visible != true).GroupBy(x => x.Id).Count();
                ticketList = this.TicketFilterFactory(db, request, ticketList);
                var filtered_count = ticketList.Count();

                #endregion
                var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
                switch (colSearch?.Name)
                {
                    case "Id":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            ticketList = ticketList.OrderBy(x => x.Id);
                        }
                        else
                        {
                            ticketList = ticketList.OrderByDescending(x => x.Id);
                        }
                        break;
                    case "Name":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            ticketList = ticketList.OrderBy(x => x.Name);
                        }
                        else
                        {
                            ticketList = ticketList.OrderByDescending(x => x.Name);
                        }
                        break;
                    case "StatusName":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            ticketList = ticketList.OrderBy(x => x.StatusId);
                        }
                        else
                        {
                            ticketList = ticketList.OrderByDescending(x => x.StatusId);
                        }
                        break;
                    case "Type":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            ticketList = ticketList.OrderBy(x => x.TypeId);
                        }
                        else
                        {
                            ticketList = ticketList.OrderByDescending(x => x.TypeId);
                        }
                        break;
                    case "CreateAt":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            ticketList = ticketList.OrderBy(x => x.CreateAt);
                        }
                        else
                        {
                            ticketList = ticketList.OrderByDescending(x => x.CreateAt);
                        }
                        break;
                    case "Date":
                        if (request.Tab == "closed")
                        {
                            if (colSearch.Sort.Direction == SortDirection.Ascending)
                            {
                                ticketList = ticketList.OrderBy(x => x.DateClosed);
                            }
                            else
                            {
                                ticketList = ticketList.OrderByDescending(x => x.DateClosed);
                            }
                        }
                        else
                        {
                            if (colSearch.Sort.Direction == SortDirection.Ascending)
                            {
                                ticketList = ticketList.OrderBy(x => x.DateOpened ?? x.CreateAt);
                            }
                            else
                            {
                                ticketList = ticketList.OrderByDescending(x => x.DateOpened ?? x.CreateAt);
                            }
                        }

                        break;
                    case "UpdateTicketHistory":
                        if (colSearch.Sort.Direction == SortDirection.Ascending)
                        {
                            ticketList = ticketList.OrderBy(x => x.UpdateTicketHistory);
                        }
                        else
                        {
                            ticketList = ticketList.OrderByDescending(x => x.UpdateTicketHistory);
                        }
                        break;
                    //case "License":
                    //    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    //    {
                    //        ticketList = ticketList.OrderBy(x => x.storeService.RenewDate);
                    //    }
                    //    else
                    //    {
                    //        ticketList = ticketList.OrderByDescending(x => x.storeService.RenewDate);
                    //    }
                    //    break;
                    default:
                        ticketList = ticketList.OrderByDescending(x => x.DateOpened ?? x.CreateAt);
                        break;
                };
                ticketList = ticketList.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
                var dataresult = await ticketList.ToListWithNoLockAsync();
                var members = db.P_Member.Where(x => x.Delete != true && x.Active == true).ToList();
                var data = dataresult.Select(tic =>
                {
                    var item = new TicketListView();
                    item.Id = tic.Id;
                    item.Name = tic.Name;
                    List<string> AvartarMemberAssign = new List<string>();
                    List<string> NameMemberAssign = new List<string>();
                    if (!string.IsNullOrEmpty(tic.AssignedToMemberNumber))
                    {
                        var ListMemberNumberAssign = tic.AssignedToMemberNumber.Split(',');
                        ListMemberNumberAssign.ForEach(c =>
                        {
                            var memberAssign = members.Where(m => c.Equals(m.MemberNumber)).FirstOrDefault();
                            if (memberAssign != null)
                            {
                                AvartarMemberAssign.Add(string.IsNullOrEmpty(memberAssign.Picture) ? "/Upload/Img/" + memberAssign.Gender + ".png" : memberAssign.Picture);
                                NameMemberAssign.Add(memberAssign.FullName);
                            }
                        });
                    }

                    item.StageName = tic.StageName;
                    item.AssignMemberNumbers = tic.AssignedToMemberNumber;
                    item.AssignMemberNames = string.Join(",", NameMemberAssign); ;
                    item.AssignMemberAvatars = string.Join("|", AvartarMemberAssign);
                    item.OpenByName = tic.OpenByName ?? tic.CreateByName ?? "System";
                    if (!string.IsNullOrEmpty(tic.CustomerCode))
                    {
                        var customer = db.C_Customer.Where(x => x.CustomerCode == tic.CustomerCode).FirstOrDefault();
                        if (customer != null)
                        {
                            item.CustomerId = customer?.Id;
                            item.AccountManager = customer?.FullName;
                            item.SalonPhone = customer?.SalonPhone;
                            item.OwnerPhone = customer?.OwnerMobile;
                            var license = db.Store_Services.Where(x => x.CustomerCode == customer.CustomerCode && x.Active == 1 && x.Type == "license").FirstOrDefault();
                            item.LicenseName = license?.Productname;
                            item.LicenseExpiredDate = license != null ? string.Format("{0:r}", license.RenewDate) : "";
                            if (license != null)
                                item.RemainingDate = CommonFunc.FormatNumberRemainDate((license.RenewDate?.Date - DateTime.UtcNow.Date).Value.Days);
                        }
                    }
                    item.ProjectName = tic.ProjectName;
                    item.VersionName = tic.VersionName;
                    item.CustomerCode = tic.CustomerCode;
                    item.CustomerName = tic.CustomerName;
                    if (tic.PriorityId != null)
                    {
                        var priority = db.T_Priority.Where(x => x.Id == tic.PriorityId).FirstOrDefault();
                        if (priority != null)
                        {
                            item.Priority.Name = priority.Name;
                            item.Priority.Color = priority.Color;
                            item.Priority.Id = priority.Id;
                        }
                    }
                    //item.PriorityName = tic.SupportTicket.PriorityName ?? string.Empty;
                    item.SeverityId = tic.SeverityId;
                    item.SeverityName = tic.SeverityName ?? string.Empty;
                    if (!string.IsNullOrEmpty(tic.Tags))
                    {
                        var ListLabel = new List<string>();
                        var labels = tic.Tags.Split(',');
                        foreach (var label in labels)
                        {
                            var lb = db.T_Tags.Where(x => x.Id == label).FirstOrDefault();
                            if (lb != null)
                            {
                                ListLabel.Add(lb.Name + "::" + lb.Color + "::" + lb.Id);
                            }
                        }
                        item.Tags = string.Join("|", ListLabel);
                    }


                    var types = db.T_TicketTypeMapping.Where(x => x.TicketId == tic.Id);
                    if (types.Count() > 0)
                    {
                        item.TypeName = string.Join(",", types.Select(x => x.TypeName).ToList());
                    }

                    var status = db.T_TicketStatusMapping.Where(x => x.TicketId == item.Id).Include(x => x.T_TicketStatus).ToList();
                    item.Status = status.Count > 0 ? string.Join(",", status.Select(x => x.StatusName)) : "Open";
                    item.StatusType = status.Count > 0 ? (status.Any(x => x.T_TicketStatus.Type == "closed") ? "closed" : "open") : "open";
                    item.GlobalStatus = tic.GlobalStatus;
                    if (tic.DateClosed.HasValue)
                    {
                        item.CloseDate = tic.DateClosed != null ? tic.DateClosed.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy") : null;
                        item.CloseDateAgo = CommonFunc.DateTimeRemain(tic.DateClosed.Value);
                    }
                    item.CloseByName = tic.CloseByName ?? "System";
                    item.CreateAt = tic.CreateAt;
                    var openDate = tic.DateOpened ?? tic.CreateAt;
                    item.OpenDate = openDate.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy");
                    if (openDate.HasValue)
                    {
                        item.OpenDateAgo = CommonFunc.DateTimeRemain(openDate.Value);
                    }
                    item.Updated = tic.UpdateTicketHistory;
                    if (tic.Deadline != null)
                    {
                        item.Deadline = tic.Deadline.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt");
                        item.DeadlineText = CommonFunc.DateTimeNextRemain(tic.Deadline.Value);
                        var DeadlineRemaningDate = tic.Deadline.Value - DateTime.UtcNow;
                        if (DeadlineRemaningDate.TotalDays < 0)
                        {
                            item.DeadlineLevel = (int)TicketDeadlineLevel.DeadlineExpired;
                        }
                        else if (DeadlineRemaningDate.TotalDays < 1)
                        {
                            item.DeadlineLevel = (int)TicketDeadlineLevel.DeadlineNearly;
                        }
                        else
                        {
                            item.DeadlineLevel = (int)TicketDeadlineLevel.DeadlineOpen;
                        }
                    }
                    //item.CreateByNumber = tic.SupportTicket.CreateByNumber;
                    item.Note = tic.Note;
                    if (tic.UpdateAt != null)
                    {
                        var updateDetail = db.T_TicketUpdateLog.Where(x => x.TicketId == tic.Id).AsEnumerable().GroupBy(x => x.UpdateId).LastOrDefault();
                        if (updateDetail != null)
                        {
                            List<string> descriptionUpdate = new List<string>();
                            foreach (var u in updateDetail)
                            {
                                if (u.Name == "EstimatedCompletionTimeTo" || u.Name == "EstimatedCompletionTimeFrom" || u.Name == "Deadline")
                                {
                                    descriptionUpdate.Add(u.Name + ": <span class='time-datatable-log'>" + u.NewValue + "</span>");
                                }
                                else if (u.Name != "Label")
                                {
                                    descriptionUpdate.Add(u.Name + ": " + u.NewValue);
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
                                    descriptionUpdate.Add(u.Name + " :" + string.Join(",", labelContent));
                                }
                            }
                            item.DetailUpdate = string.Join("|", descriptionUpdate);
                        }
                    }
                    return item;
                }).ToList();
                _logService.Info($"[Ticket][List] completed load list ticket");
                return Json(new
                {
                    draw = request.draw,
                    recordsFiltered = filtered_count,
                    recordsTotal = filtered_count,
                    data = data,
                    ticketOpenCount,
                    ticketAllCount,
                    ticketUnassignedCount,
                    ticketClosedCount,
                    ticketinvisibleCount
                });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][List] error when load list ticket in tab ${request.Tab}");
                throw;
            }

        }
        //mark as submit
        [HttpPost]
        public async Task<JsonResult> Markas(List<long> ticketIds, List<string> type, string status, List<string> label, List<string> assign, string severity, string version, bool? isSameType, string Page)
        {
            try
            {
                _logService.Info($"[Ticket][Markas] start Markas submit");
                var db = new WebDataModel();
                var support_ticket = db.T_SupportTicket.Where(t => ticketIds.Contains(t.Id)).Include(x => x.T_TicketTypeMapping).Include(x => x.T_TicketStatusMapping).ToList();
                var ticket_status = db.T_TicketStatus.ToList();
                foreach (var item in support_ticket)
                {
                    //status
                    bool notify = false;
                    //IQueryable<Project_StageVersion> stg_ver;
                    //stg_ver = GetCurrentStage(db,Page);
                    //var stage_tk = db.T_TicketStage_Status.Where(s => s.TicketId == item.Id && stg_ver.Any(p => p.stage.Id == s.StageId && s.ProjectVersionId == p.version.Id)).ToList();
                    //status development 
                    if (!string.IsNullOrEmpty(status) && status != "-1")
                    {
                        db.T_TicketStatusMapping.RemoveRange(item.T_TicketStatusMapping);
                        item.T_TicketStatusMapping.Clear();
                        //  tic.T_TicketStatusMapping.Clear();
                        bool closeTicket = false;
                        //foreach (var statusItem in status)
                        //{                            
                        var st = db.T_TicketStatus.Where(s => s.Name.ToLower().Equals(status) && item.ProjectId == s.ProjectId).FirstOrDefault();

                        if (st != null)
                        {
                            item.T_TicketStatusMapping.Add(new T_TicketStatusMapping
                            {
                                StatusId = st.Id,
                                StatusName = st.Name,
                                TicketId = item.Id,
                            });
                        }
                        if (closeTicket != true && st.Type?.ToLower().Equals("closed") == true)
                        {
                            closeTicket = true;
                        }
                        // }
                        item.StatusId = string.Join(",", item.T_TicketStatusMapping.Select(x => x.StatusId));
                        item.StatusName = string.Join(", ", item.T_TicketStatusMapping.Select(x => x.StatusName));
                        // var tic_status = db.T_TicketStatus.Where(s => s.Name.ToLower().Equals(status.) && item.ProjectId == s.ProjectId).FirstOrDefault();
                        //if (tic_status == null)
                        //{
                        //    tic_status = db.T_TicketStatus.Where(s => (status.ToLower() == "closed" ^ s.Type != "closed") && item.ProjectId == s.ProjectId).FirstOrDefault();
                        //}
                        notify = true;
                        //item.StatusId = tic_status?.Id;
                        //item.StatusName = tic_status?.Name;
                        if (closeTicket == true)
                        {
                            var reminderTicket = db.T_RemindersTicket.Where(x => x.TicketId == item.Id).FirstOrDefault();
                            if (reminderTicket != null)
                            {
                                var reminderTicketService = new ReminderTicketService();
                                reminderTicketService.DeleteJob(reminderTicket.HangfireJobId);
                                db.T_RemindersTicket.Remove(reminderTicket);
                            }
                            //if (!string.IsNullOrEmpty(item.DeadlineHangfireJobId))
                            //{
                            //    var DeadlineNotificationService = new DeadlineNotificationService();
                            //    DeadlineNotificationService.DeleteJob(item.DeadlineHangfireJobId);
                            //    item.DeadlineHangfireJobId = null;
                            //}
                            item.DateClosed = DateTime.UtcNow;
                            item.CloseByMemberNumber = cMem.MemberNumber;
                            item.CloseByName = cMem.FullName;
                        }
                        else
                        {
                            item.DateClosed = null;
                            item.CloseByMemberNumber = null;
                            item.CloseByName = null;
                        }
                    }

                    //assign
                    if (!(assign.Contains("-1") && assign.Count <= 1))
                    {
                        notify = true;
                        var member = db.P_Member.Where(m => assign.Contains(m.MemberNumber));
                        item.AssignedToMemberNumber = string.Join(",", member.Select(m => m.MemberNumber).ToList());
                        item.AssignedToMemberName = string.Join(",", member.Select(m => m.FullName).ToList());


                    }

                    if (type != null && type.Count > 0)
                    {
                        //type
                        db.T_TicketTypeMapping.RemoveRange(item.T_TicketTypeMapping);
                        item.T_TicketTypeMapping.Clear();
                        foreach (var typeName in type)
                        {
                            var newtype = db.T_TicketType.Where(s => s.TypeName.ToLower().Equals(typeName) && item.ProjectId == s.ProjectId).FirstOrDefault();
                            if (newtype != null)
                            {
                                item.T_TicketTypeMapping.Add(new T_TicketTypeMapping
                                {
                                    TypeId = newtype.Id,
                                    TypeName = newtype.TypeName,
                                    TicketId = item.Id,
                                });
                            }

                        }

                        item.TypeId = string.Join(",", item.T_TicketTypeMapping.Select(x => x.TypeId));
                        item.TypeName = string.Join(", ", item.T_TicketTypeMapping.Select(x => x.TypeName));

                    }

                    //version
                    if (!string.IsNullOrEmpty(version))
                    {
                        if (version != "-1")
                        {
                            var ver = db.T_Project_Milestone.Find(version);
                            if (item.ProjectId == ver.ParentId)
                            {
                                item.VersionId = ver.Id;
                                item.VersionName = ver.Name;
                            }

                        }
                    }

                    //label/tags

                    if (label != null && label.Count > 0)
                    {
                        if (label.Any(l => l.Equals("0")))
                        {
                            item.Tags = string.Empty;
                        }
                        else
                        {
                            item.Tags = string.Join(",", label);
                        }
                        if (!string.IsNullOrEmpty(item.Tags))
                        {
                            item.TagName = string.Join(",", db.T_Tags.Where(x => item.Tags.Contains(x.Id)).Select(x => x.Name).ToList());
                        }
                        else
                        {
                            item.TagName = null;
                        }
                    }


                    //severity
                    if (severity != "-1" && long.TryParse(severity, out long _severity))
                    {
                        if (severity == "0")
                        {
                            item.SeverityId = null;
                            item.SeverityName = null;
                        }
                        else
                        {
                            var tic_severity = db.T_TicketSeverity.Find(_severity);
                            if (tic_severity != null)
                            {
                                item.SeverityId = tic_severity.Id;
                                item.SeverityName = tic_severity.SeverityName;
                            }
                        }

                    }

                    item.UpdateTicketHistory += DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName + "|";
                    db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    if (notify)
                    {
                        await TicketViewService.SendNoticeAfterTicketUpdate(item, "update", null, cMem.MemberNumber);
                        //await TicketViewController.AutoTicketScenario.UpdateSatellite(item.Id, null, cMem.FullName);
                    }
                }
                _logService.Info($"[Ticket][Markas] completed Markas submit");
                return Json(true);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][Markas] error when mark as ticket");
                return Json(false);
            }
        }

        #endregion

        #region save ticket
        public ActionResult Update(long? id, string cid, string Page, string urlback)
        {
            try
            {

                bool AccessDeployment = true;
                bool AccessSupport = true;
                bool AccessDevelopments = false;
                var db = new WebDataModel();
                var ticket = new T_SupportTicket();
                var project = new T_Project_Milestone();
                //  var ListMemberAssign = new List<P_Member>();
                var Department = db.P_Department.FirstOrDefault(x => x.Id.ToString() == Page);
                // access
                if (cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true))
                {
                    AccessDevelopments = true;
                }
                else
                {
                    if (access.Any(k => k.Key.Equals("development_ticket_view")) == true && access["development_ticket_view"] == true)
                    {
                        AccessDevelopments = true;
                    }
                }
                // hard code disable development
                if (cMem.SiteId != 1) AccessDevelopments = false;


                if (string.IsNullOrWhiteSpace(urlback))
                {
                    urlback = "/ticket?page=" + Page;
                }
                ViewBag.urlback = urlback;
                ViewBag.AvailablesTags = db.T_Tags.Where(x => x.Type == Page && x.SiteId == cMem.SiteId).OrderBy(x => x.Name).ToList();

                // update 
                if (id > 0)
                {
                    _logService.Info($"[Ticket][Add] start view add ticket");
                    ticket = db.T_SupportTicket.Where(x => x.Id == id).Include(x => x.T_TicketStatusMapping).Include(x => x.T_TicketTypeMapping).FirstOrDefault();
                    if (ticket == null)
                    {
                        _logService.Error($"[Ticket][Detail] Ticket #{id} can not found.");
                        TempData["e"] = "Ticket #" + id + " can not found.";
                        return Redirect("/ticket");
                    }
                    UserContent.TabHistory = "Ticket #" + id + "|" + Request.Url.AbsolutePath;
                    //set status = open
                    if (ticket.T_TicketStatusMapping.Count() == 0)
                    {
                        var statusOpen = db.T_TicketStatus.Where(x => x.ProjectId == ticket.ProjectId && x.Name.ToLower().Contains("open") && x.SiteId == cMem.SiteId).FirstOrDefault();
                        if (statusOpen != null)
                        {
                            var mapping = new T_TicketStatusMapping();
                            mapping.StatusId = statusOpen.Id;
                            ticket.T_TicketStatusMapping.Add(mapping);
                        } // get project 


                    }

                    // set Page
                    project = db.T_Project_Milestone.Find(ticket.ProjectId);
                    Page = project.BuildInCode;
                    //if (project.BuildInCode == TypeDevelopment)
                    //{
                    //    Page = PageDevelopmentsTicket;
                    //}
                    //else if (project.BuildInCode == TypeSupport)
                    //{
                    //    Page = PageSupport;
                    //}
                    //else
                    //{
                    //    Page = PageDeployment;
                    //}
                    Response.Cookies["TicketPage"].Value = Page;
                    Response.Cookies["TicketPage"].Expires = DateTime.Now.AddYears(1);

                    // check access
                    var IsAdminOrViewAllTicket = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                    var checkViewGroup = access.Any(k => k.Key.Equals("ticket_view")) == true && access["ticket_view"] == true;
                    if (!(cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true)))
                    {
                        if ((Page == PageDevelopmentsTicket && !checkViewGroup) || Page != PageDevelopmentsTicket)
                        {
                            List<long> listGroup = TicketViewService.GetGroupByMember(db, cMem?.MemberNumber);
                            //check quyen xem.
                            //var checkView = (access.Any(k => k.Key.Equals("development_ticket_viewall")) == true && access["development_ticket_viewall"] == true)
                            //    || tic.AssignedToMemberNumber == cMem.MemberNumber
                            //    || tic.GlobalStatus == "publish"
                            //    || tic.CreateByNumber == cMem.MemberNumber
                            //    || (tic.SubscribeMemberNumber != null && tic.SubscribeMemberNumber.Contains(cMem.MemberNumber));

                            var checkCanEditTicket = ticket.CreateByNumber == cMem.MemberNumber || cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                            if (!checkCanEditTicket)
                            {
                                _logService.Info($"[Ticket][Update] You are not allowed to edit this ticket.");
                                TempData["e"] = "You are not allowed to edit this ticket.";
                                return Redirect("/ticket");
                            }

                        }

                    }

                    //Attachments
                    ViewBag.Attachments = db.UploadMoreFiles.Where(f => f.TableId == id && f.TableName.Equals("T_SupportTicket")).ToList();
                    //member
                    if (Page != TypeDevelopment)
                    {
                        ViewBag.AvailablesSeverities = db.T_TicketSeverity.Where(s => s.Active == true && s.SpecialType == project.BuildInCode).OrderBy(s => s.SeverityLevel).ToList();

                    }
                    ViewBag.TicketBreadcrumb = ticket.ProjectName + " / " + ticket.StageName + " / " + ticket.VersionName;
                    ViewBag.Title = "UPDATE TICKET #" + id;
                }
                // add new
                else
                {
                    _logService.Info($"[Ticket][Markas] start view update ticket ${id}");
                    if (!string.IsNullOrWhiteSpace(cid))
                    {
                        var _cid = long.Parse(cid);
                        var merchant = db.C_Customer.Where(c => c.Id == _cid).FirstOrDefault();
                        if (merchant != null)
                        {
                            ticket.CustomerCode = merchant.CustomerCode;
                            ticket.CustomerName = merchant?.BusinessName;
                        }
                    }

                    IQueryable<Project_StageVersion> proj_info;
                    proj_info = GetCurrentStage(db, Page);
                    var Default = proj_info.OrderByDescending(x => x.stage.BuildInCode == "default").FirstOrDefault();

                    var projectVersion = new T_Project_Milestone();
                    if (Default != null)
                    {
                        project = db.T_Project_Milestone.FirstOrDefault(x => x.Id == Default.stage.ProjectId);
                    }
                    else
                    {
                        project = db.T_Project_Milestone.FirstOrDefault(x => x.BuildInCode == Page);
                        if (project == null)
                            project = db.T_Project_Milestone_Member.Where(x => x.MemberNumber == cMem.MemberNumber).Include(x => x.T_Project_Milestone).FirstOrDefault().T_Project_Milestone;
                    }

                    ticket.ProjectId = project.Id;
                    ticket.VersionId = db.T_Project_Milestone.Where(x => x.Type == "Project_version" && x.ParentId == ticket.ProjectId).OrderBy(x => x.Order).FirstOrDefault()?.Id;
                    ticket.StageId = db.T_Project_Stage.Where(x => x.ProjectId == ticket.ProjectId).OrderBy(x => x.Name).FirstOrDefault()?.Id;
                    ViewBag.TicketBreadcrumb = ticket.ProjectName + " / " + ticket.StageName + " / " + ticket.VersionName;

                    ViewBag.Title = $"ADD NEW {Department.Name} TICKET";
                }

                //if (Page == PageDevelopmentsTicket)
                //{
                //    var ListProjectMember = (from memProject in db.T_Project_Milestone_Member
                //                             join member in db.P_Member on memProject.MemberNumber equals member.MemberNumber
                //                             where memProject.ProjectId == project.Id && member.Active == true && member.Delete != true
                //                             select member).ToList();

                //    ListMemberAssign.AddRange(ListProjectMember);
                //}

                ViewBag.Page = Page;
                ViewBag.AccessDeployment = AccessDeployment;
                ViewBag.AccessSupport = AccessSupport;
                ViewBag.AccessDevelopments = AccessDevelopments;
                ViewBag.BuildInCode = project.BuildInCode;
                // list viewbag select option
                ViewBag.AvailablesTypes = db.T_TicketType.Where(x => x.ProjectId == ticket.ProjectId && x.IsDeleted != true && x.SiteId == cMem.SiteId).ToList();
                ViewBag.AvailablesPriorities = db.T_Priority.Where(x => x.IsDeleted != true).OrderBy(x => x.DisplayOrder).ToList();
                var ListMemberAssign = db.P_Member.Where(x => x.Active != false && x.Delete != true && x.SiteId == cMem.SiteId).OrderBy(x => x.MemberNumber).ToList();
                ViewBag.AvailablesAsssignMember = ListMemberAssign;
                ViewBag.AvailablesTagsMember = ListMemberAssign;
                var AvailablesProject = db.T_Project_Milestone.Where(p => p.Type == "project" && p.BuildInCode == project.BuildInCode).OrderBy(s => s.Name).ToList();
                ViewBag.AvailablesProject = AvailablesProject;
                var AvailablesMainVersion = db.T_Project_Milestone.Where(x => x.ParentId == ticket.ProjectId && x.Type == "Project_version" && x.Active != false).OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();
                ViewBag.AvailablesMainVersion = AvailablesMainVersion;
                var AvailablesSubversion = db.T_Project_Milestone.Where(s => s.Type == "version" && s.ParentId == ticket.VersionId).OrderBy(s => s.Name).ToList();
                ViewBag.AvailablesSubversion = AvailablesSubversion;
                ViewBag.AvailablesStatus = db.T_TicketStatus.Where(x => x.ProjectId == ticket.ProjectId && x.IsDeleted != true && x.SiteId == cMem.SiteId).OrderBy(s => s.Order).ThenBy(s => s.Order).ToList();
                ViewBag.AvailablesDepartment = db.P_Department.Where(x => (x.Type == "DEPLOYMENT" || x.Type == "SUPPORT") && x.SiteId == cMem.SiteId).ToList();
                ViewBag.access = access;
                ViewBag.AvailablesSeverities = db.T_TicketSeverity.Where(s => s.Active == true && s.SpecialType == project.BuildInCode).OrderBy(s => s.SeverityLevel).ToList();
                ViewBag.AvailablesStages = db.T_Project_Stage.Where(x => x.ProjectId == project.Id && x.Active != false).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToList();
                if (id > 0)
                {
                    _logService.Info($"[Ticket][AddNew] completed add new ticket");
                }
                else
                {
                    _logService.Info($"[Ticket][Update] completed view update ticket ${id}");
                }
                return View(ticket);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Ticket][Update] error when view update {id}");
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);

                TempData["e"] = e.Message;
                return RedirectToAction("index");
            }
        }
        public ActionResult Detail(long? id)
        {
            if (id == null)
            {
                return Redirect("/ticket");
            }
            try
            {

                string Page = "";
                bool AccessDeployment = true;
                bool CanUpdate = false;
                bool AccessSupport = true;
                bool AccessDevelopments = false;
                var db = new WebDataModel();

                //get ticket info
                var tic = db.T_SupportTicket.Where(x => x.Id == id).Include(x => x.T_TicketStatusMapping).Include(x => x.T_TicketTypeMapping).FirstOrDefault();
                if (tic == null)
                {
                    _logService.Error($"[Ticket][Detail] Ticket #{id} can not found.");
                    TempData["e"] = "Ticket #" + id + " can not found.";
                    return Redirect("/ticket");
                }
                _logService.Info($"[Ticket][Detail] start load detail ticket {id}");
                UserContent.TabHistory = "Ticket #" + id + "|" + Request.Url.AbsolutePath;
                //set status = open
                if (tic.T_TicketStatusMapping.Count() == 0)
                {
                    var statusOpen = db.T_TicketStatus.Where(x => x.ProjectId == tic.ProjectId && x.Name.ToLower().Contains("open")).FirstOrDefault();
                    if (statusOpen != null)
                    {
                        var mapping = new T_TicketStatusMapping();
                        mapping.StatusId = statusOpen.Id;
                        tic.T_TicketStatusMapping.Add(mapping);
                    }
                }

                // get project 
                var project = db.T_Project_Milestone.Find(tic.ProjectId);
                if (cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true))
                {
                    AccessDevelopments = true;
                }
                else
                {
                    var check_access_devTicket = (access.Any(k => k.Key.Equals("development_ticket_view")) == true && access["development_ticket_view"] == true);
                    if (check_access_devTicket)
                    {
                        AccessDevelopments = true;
                    }
                }
                // hard code disable development
                if (cMem.SiteId != 1) AccessDevelopments = false;

                if (project.BuildInCode == TypeDevelopment)
                {
                    Page = PageDevelopmentsTicket;
                }
                else if (project.BuildInCode == TypeSupport)
                {
                    Page = PageSupport;
                }
                else
                {
                    Page = PageDeployment;
                }

                // check access
                var IsAdminOrViewAllTicket = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                var checkViewGroup = access.Any(k => k.Key.Equals("ticket_view")) == true && access["ticket_view"] == true;
                if (!(cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true)))
                {
                    if ((Page == PageDevelopmentsTicket && !checkViewGroup) || Page != PageDevelopmentsTicket)
                    {
                        List<long> listGroup = TicketViewService.GetGroupByMember(db, cMem?.MemberNumber);
                        //check quyen xem.
                        //var checkView = (access.Any(k => k.Key.Equals("development_ticket_viewall")) == true && access["development_ticket_viewall"] == true)
                        //    || tic.AssignedToMemberNumber == cMem.MemberNumber
                        //    || tic.GlobalStatus == "publish"
                        //    || tic.CreateByNumber == cMem.MemberNumber
                        //    || (tic.SubscribeMemberNumber != null && tic.SubscribeMemberNumber.Contains(cMem.MemberNumber));

                        var checkCanOpenTicket = (tic.ReassignedToMemberNumber?.Contains(cMem.MemberNumber) == true
                                                 || tic.AssignedToMemberNumber?.Contains(cMem.MemberNumber) == true
                                                 || tic.TagMemberNumber?.Contains(cMem.MemberNumber) == true
                                                 || (tic.GroupID > 0 && listGroup.Contains(tic.GroupID ?? 0) == true)
                                                 || tic.CreateByNumber == cMem.MemberNumber
                                                 || db.T_Project_Milestone_Member.Any(x => x.ProjectId == tic.ProjectId && x.MemberNumber == cMem.MemberNumber)
                                                  );

                        if (!checkCanOpenTicket)
                        {
                            _logService.Info($"[Ticket][Detail] You are not allowed to edit this ticket.");
                            TempData["e"] = "You are not allowed to edit this ticket.";
                            return Redirect("/ticket");
                        }

                    }

                }


                Response.Cookies["TicketPage"].Value = Page;
                Response.Cookies["TicketPage"].Expires = DateTime.Now.AddYears(1);

                //Attachments
                ViewBag.Attachments = db.UploadMoreFiles.Where(f => f.TableId == id && f.TableName.Equals("T_SupportTicket")).ToList();
                // DepartmentsTransfer
                ViewBag.DepartmentsTransfer = db.P_Department.Where(x => (x.Type == "SUPPORT" || x.Type == "DEPLOYMENT") && x.Active == true).ToList();
                //check transfer from
                var transferHistory = db.T_TicketTranferMapping.Where(x => x.TicketId == tic.Id).OrderByDescending(x => x.CreateAt).FirstOrDefault();
                if (transferHistory != null)
                {
                    ViewBag.TransferFrom = db.T_Project_Milestone.Where(x => x.Id == transferHistory.FromProjectId).FirstOrDefault()?.Name;
                }

                // show license ticket 
                if (!string.IsNullOrEmpty(tic.CustomerCode))
                {
                    var licenseActive = db.Store_Services.FirstOrDefault(x => x.CustomerCode == tic.CustomerCode && x.Active == 1 && x.Type == "license");
                    if (licenseActive != null)
                    {
                        ViewBag.LicenseName = licenseActive.Productname;
                        ViewBag.RemainingDate = CommonFunc.FormatNumberRemainDate((int)((licenseActive.RenewDate.Value - DateTime.UtcNow).TotalDays));
                    }
                }
                if (tic.CreateByNumber == cMem.MemberNumber || cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true))
                {
                    CanUpdate = true;
                }
                ViewBag.CanUpdate = CanUpdate;
                // merge ticket and related
                var ticketRelated = db.T_SupportTicket.Where(t => t.CustomerCode == tic.CustomerCode && !string.IsNullOrEmpty(tic.CustomerCode) && t.Id != tic.Id && !string.IsNullOrEmpty(t.ProjectId) && t.ProjectId == tic.ProjectId && !string.IsNullOrEmpty(t.VersionId)).ToList();
                ViewBag.ShowMerge = ticketRelated.Count() > 0;
                ViewBag.TicketRelated = ticketRelated;

                ViewBag.Page = Page;
                ViewBag.AccessDeployment = AccessDeployment;
                ViewBag.AccessSupport = AccessSupport;
                ViewBag.AccessDevelopments = AccessDevelopments;
                ViewBag.BuildInCode = project.BuildInCode;
                //list dropdownlist ticket
                ViewBag.AvailablesTypes = db.T_TicketType.Where(x => x.ProjectId == tic.ProjectId && x.IsDeleted != true).ToList();
                ViewBag.AvailablesPriorities = db.T_Priority.Where(x => x.IsDeleted != true).OrderBy(x => x.DisplayOrder).ToList();
                var ListMemberAssign = db.P_Member.Where(x => x.Active != false && x.Delete != true && x.SiteId == cMem.SiteId).OrderBy(x => x.MemberNumber).ToList();
                ViewBag.AvailablesAsssignMember = ListMemberAssign;
                ViewBag.AvailablesTagsMember = ListMemberAssign;
                ViewBag.AvailablesProject = db.T_Project_Milestone.Where(p => p.Type == "project" && p.BuildInCode == project.BuildInCode).OrderBy(s => s.Name).ToList();
                ViewBag.AvailablesStatus = db.T_TicketStatus.Where(x => x.ProjectId == project.Id && x.IsDeleted != true).OrderBy(s => s.Order).ThenBy(s => s.Order).ToList();
                ViewBag.AvailablesSubversion = db.T_Project_Milestone.Where(s => s.Type == "version" && s.ParentId == tic.VersionId).OrderBy(s => s.Name).ToList();
                ViewBag.AvailablesDepartment = db.P_Department.Where(x => x.Active == true && x.SiteId == cMem.SiteId).ToList();

                ViewBag.AvailablesTags = db.T_Tags.Where(x => x.Type == project.BuildInCode && x.SiteId == cMem.SiteId).OrderBy(x => x.Name).ToList();
                ViewBag.AvailablesSeverities = db.T_TicketSeverity.Where(s => s.Active == true && s.SpecialType == project.BuildInCode).OrderBy(s => s.SeverityLevel).ToList();

                if (project.BuildInCode == TypeDeployment)
                {
                    //ViewBag.AvailablesTags = db.T_Tags.Where(x => x.Type == "19120001" && x.SiteId == cMem.SiteId).OrderBy(x => x.Name).ToList();
                    //ViewBag.AvailablesSeverities = db.T_TicketSeverity.Where(s => s.Active == true && s.SpecialType == "DEPLOYMENT").OrderBy(s => s.SeverityLevel).ToList();
                    bool ShowHardWarePreparing = false;
                    bool HardwareStatus = false;
                    if (tic.T_TicketTypeMapping.Any(x => x.TypeId == 4))
                    {
                        var orderProduct = db.Order_Products.Where(x => x.OrderCode == tic.OrderCode).FirstOrDefault();
                        if (orderProduct != null)
                        {
                            ViewBag.packaging_field = db.I_ProcessSetting.Where(p => p.FieldType == "Packaging").ToList();
                            var order = db.O_Orders.Where(o => o.OrdersCode == tic.OrderCode).FirstOrDefault();
                            ShowHardWarePreparing = true;
                            ViewBag.OrderId = order.Id;
                            HardwareStatus = order.BundelStatus == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString();
                        }
                    }
                    ViewBag.HardwareStatus = HardwareStatus;
                    ViewBag.ShowHardWarePreparing = ShowHardWarePreparing;

                }
                ViewBag.AvailablesMainVersion = db.T_Project_Milestone.Where(x => x.ParentId == tic.ProjectId && x.Type == "Project_version" && x.Active != false).OrderBy(x => x.Order).ThenBy(x => x.Name).ToList();
                ViewBag.AvailablesStages = db.T_Project_Stage.Where(x => x.ProjectId == project.Id && x.Active != false).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToList();
                ViewBag.TicketBreadcrumb = tic.ProjectName + " / " + tic.StageName + " / " + tic.VersionName;
                ViewBag.access = access;
                _logService.Info($"[Ticket][Detail] completed load detail ticket {id}");
                return View(tic);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Ticket][Detail] error when view detail ticket {id}");
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
                TempData["e"] = e.Message;
                return RedirectToAction("index");
            }
        }
        [HttpPost]
        public async Task<JsonResult> SaveTicket(T_TicketStage_Status ts_status = null)
        {
            try
            {

                string _id = Request["hdTicketId"];
                long id = long.Parse(string.IsNullOrWhiteSpace(_id) == true ? "0" : _id.ToString());
                string err = string.Empty;
                long ticketId = id;
                if (id == 0)
                {
                    _logService.Info($"[Ticket][Update] start add ticket");
                    //add new
                    long result = await AddNewTicket();
                    ticketId = result;
                    _logService.Info($"[Ticket][Update] Add Ticket: #{ticketId} success at {DateTime.Now}", new
                    {
                        contextId = Guid.NewGuid(),
                        content = $"Add Ticket: #{ticketId} success at {DateTime.Now}",
                        sessionId = Session.SessionID,

                    });
                    //get task for ticket
                    try
                    {
                        var db = new WebDataModel();

                        var ProjectId = Request["Project_select"];
                        var project = db.T_Project_Milestone.Where(x => x.Id == ProjectId).FirstOrDefault();
                        var BuildInCode = project.BuildInCode;
                        //get list task_template
                        List<Ts_TaskTemplateCategory> task_template = db.Ts_TaskTemplateCategory.Where(x => x.TicketGroup == BuildInCode && x.IsDeleted != true).ToList();
                        for (var i = 0; i < task_template.Count; i++)
                        {
                            using (var Trans = db.Database.BeginTransaction())
                            {
                                int cateid = task_template[i].Id;
                                //get list task_template_field
                                List<Ts_TaskTemplateField> lst_field = db.Ts_TaskTemplateField.Where(a => a.CategoryId == cateid).ToList();
                                var subtaskId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                                //TicketTaskTemplateDetail model = new TicketTaskTemplateDetail();
                                var _idtem = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                                string message = "";
                                long taskId;
                                int taskCount = db.Ts_Task.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
                                && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
                                var task = new Ts_Task
                                {
                                    Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (taskCount + 1).ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString("ff")),
                                    Name = task_template[i].Name,
                                    Complete = false,
                                    Description = task_template[i].Description,
                                    Requirement = task_template[i].Requirement,
                                    TaskTemplateCategoryId = task_template[i].Id,
                                    CreateBy = cMem.FullName,
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
                                var ticket = db.T_SupportTicket.Where(x => x.Id == ticketId).FirstOrDefault();
                                if (ticket != null)
                                {
                                    task.TicketId = ticket.Id;
                                    task.TicketName = ticket.Name;
                                }

                                db.Ts_Task.Add(task);
                                taskId = task.Id;


                                var ListSubTask = new List<SubTaskTemplate>();
                                foreach (var item in lst_field)
                                {
                                    var SubTask = new SubTaskTemplate();
                                    SubTask.Id = subtaskId;
                                    SubTask.Name = item.Name;
                                    SubTask.Required = item.IsRequired;
                                    SubTask.Description = item.Description;
                                    ListSubTask.Add(SubTask);



                                    var subtask = new Ts_Task
                                    {
                                        Name = item.Name,
                                        Complete = false,
                                    };

                                    subtask.Id = subtaskId;
                                    subtask.TicketId = task.TicketId;
                                    subtask.TicketName = task.TicketName;
                                    subtask.ParentTaskId = task.Id;
                                    subtask.ParentTaskName = task.Name;
                                    subtask.Description = task.Description;
                                    subtask.CreateBy = cMem.FullName;
                                    subtask.CreateByMemberNumber = cMem.MemberNumber;
                                    subtask.CreateAt = DateTime.UtcNow;
                                    subtaskId++;
                                    db.Ts_Task.Add(subtask);
                                }
                                db.SaveChanges();
                                Trans.Commit();
                                Trans.Dispose();
                                //email notice
                                await ViewControler.TaskViewService.SendNoticeAfterTaskUpdate(task, "new", cMem);

                            }
                        }

                    }
                    catch (Exception e)
                    {

                    }

                    if (result == -1)
                    {
                        throw new Exception("Add new ticket error. Something went wrong");
                    }
                }
                else
                {
                    //update
                    _logService.Info($"[Ticket][Update] start update ticket {_id}");
                    err = await UpdateTicket(id);
                    _logService.Info($"[Ticket][Update] Update Ticket: #{ticketId} success at {DateTime.Now}", new
                    {
                        contextId = Guid.NewGuid(),
                        content = $"Update Ticket: #{ticketId} success at {DateTime.Now}",
                        sessionId = Session.SessionID,

                    });
                }
                if (string.IsNullOrWhiteSpace(err))
                {
                    return Json(new object[] { true, "Ticket #" + ticketId + " has been saved.", ticketId });
                }
                else
                {
                    throw new Exception(err);
                }
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Ticket][Save] error when process save ticket");
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
                return Json(new object[] { false, e.Message });
            }
        }

        /// <summary>
        /// add new ticket
        /// </summary>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        private async Task<long> AddNewTicket()
        {
            var db = new WebDataModel();
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var tic = new T_SupportTicket();
                    // hard code set site id when add new ticket
                    tic.SiteId = cMem.SiteId;
                    int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
                                                       && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
                    //var BuildInCode = Request["BuildInCode"].ToString();
                    tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("ff"));
                    tic.Name = Request["name"];
                    tic.Description = Request.Unvalidated["comment"];

                    // customer
                    if (!string.IsNullOrEmpty(Request["merchant"]))
                    {
                        tic.CustomerCode = Request["merchant"].ToString();
                        tic.CustomerName = db.C_Customer.Where(c => c.CustomerCode == tic.CustomerCode).FirstOrDefault().BusinessName;
                    }
                    else
                    {
                        tic.CustomerCode = null;
                        tic.CustomerName = null;
                    }

                    // department
                    if (!string.IsNullOrEmpty(Request["department"]))
                    {
                        tic.GroupID = long.Parse(Request["department"].ToString());
                        tic.GroupName = db.P_Department.Where(c => c.Id == tic.GroupID).FirstOrDefault().Name;
                    }
                    else
                    {
                        tic.GroupID = null;
                        tic.GroupName = null;
                    }

                    if (!string.IsNullOrEmpty(Request["Other_Department"]))
                    {
                        tic.OtherGroupId = Request["Other_Department"].ToString();
                        List<long> OtherDepartmentIds = tic.OtherGroupId.Split(',').Select(long.Parse).ToList();
                        tic.OtherGroupName = string.Join(", ", db.P_Department.Where(c => OtherDepartmentIds.Any(y => y == c.Id)).Select(x => x.Name));
                    }
                    else
                    {
                        tic.OtherGroupId = null;
                        tic.OtherGroupName = null;
                    }

                    //label
                    tic.Tags = Request["label"];
                    if (!string.IsNullOrEmpty(tic.Tags))
                    {
                        tic.TagName = string.Join(",", db.T_Tags.Where(x => tic.Tags.Contains(x.Id)).Select(x => x.Name).ToList());
                    }
                    else
                    {
                        tic.TagName = null;
                    }

                    // tag member
                    tic.TagMemberNumber = Request["TagMemberNumber"];
                    if (!string.IsNullOrEmpty(tic.TagMemberNumber))
                    {
                        tic.TagMemberName = string.Join(",", db.P_Member.Where(x => tic.TagMemberNumber.Contains(x.MemberNumber)).Select(x => x.FullName));
                    }
                    else
                    {
                        tic.TagMemberName = null;
                    }

                    // update type
                    if (!string.IsNullOrEmpty(Request["type"]))
                    {
                        foreach (var typeId in Request["type"].Split(','))
                        {
                            var Id = long.Parse(typeId);
                            var type = db.T_TicketType.Find(Id);
                            if (type != null)
                            {
                                tic.T_TicketTypeMapping.Add(new T_TicketTypeMapping
                                {
                                    TypeId = type.Id,
                                    TypeName = type.TypeName,
                                    TicketId = tic.Id,
                                });
                            }
                        }
                        tic.TypeId = string.Join(",", tic.T_TicketTypeMapping.Select(x => x.TypeId));
                        tic.TypeName = string.Join(", ", tic.T_TicketTypeMapping.Select(x => x.TypeName));
                    }


                    // update status
                    bool closeTicket = false;
                    if (!string.IsNullOrEmpty(Request["statusId"]))
                    {
                        foreach (var statusItem in Request["statusId"].Split(','))
                        {
                            var StatusId = long.Parse(statusItem);
                            var st = db.T_TicketStatus.Find(StatusId);
                            if (st != null)
                            {
                                tic.T_TicketStatusMapping.Add(new T_TicketStatusMapping
                                {
                                    StatusId = st.Id,
                                    StatusName = st.Name,
                                    TicketId = tic.Id,
                                });
                            }
                            if (closeTicket != true && st.Type?.ToLower().Equals("closed") == true)
                            {
                                closeTicket = true;
                            }
                        }
                        tic.StatusId = string.Join(",", tic.T_TicketStatusMapping.Select(x => x.StatusId));
                        tic.StatusName = string.Join(", ", tic.T_TicketStatusMapping.Select(x => x.StatusName));
                    }

                    if (closeTicket == true)
                    {
                        var checkTaskCompleted = db.Ts_Task.Where(x => x.TicketId == tic.Id && x.ParentTaskId == null && x.Requirement == true).ToList();
                        if (checkTaskCompleted.Count() > 0 && checkTaskCompleted.Any(x => x.Complete != true))
                        {
                            throw new Exception("Please complete all required tasks before closing this ticket");
                        }

                        tic.CloseByMemberNumber = cMem.MemberNumber;
                        tic.CloseByName = cMem.FullName;
                        tic.DateClosed = DateTime.UtcNow;
                    }
                    else
                    {
                        tic.CloseByMemberNumber = null;
                        tic.CloseByName = null;
                        tic.DateClosed = null;
                    }

                    //priority
                    if (!string.IsNullOrWhiteSpace(Request["Priority"]))
                    {
                        tic.PriorityId = int.Parse(Request["Priority"]?.ToString());
                        tic.PriorityName = db.T_Priority.Where(x => x.Id == tic.PriorityId).FirstOrDefault()?.Name;
                    }
                    else
                    {
                        tic.PriorityId = null;
                        tic.PriorityName = string.Empty;
                    }


                    //severity
                    if (!string.IsNullOrWhiteSpace(Request["Severity"]))
                    {
                        tic.SeverityId = long.Parse(Request["Severity"]);
                        tic.SeverityName = db.T_TicketSeverity.Find(tic.SeverityId).SeverityName;
                    }

                    // deadline and estimated time

                    if (!string.IsNullOrEmpty(Request["Deadline"]))
                    {
                        tic.Deadline = DateTime.Parse(Request["Deadline"]).IMSToUTCDateTime();
                        //  var DeadlineNotificationService = new DeadlineNotificationService();
                        //tic.DeadlineHangfireJobId = Guid.NewGuid().ToString();
                        //DeadlineNotificationService.CreateJob(tic);
                    }
                    else
                    {
                        tic.Deadline = null;
                    }

                    if (!string.IsNullOrEmpty(Request["EstimatedCompletionTimeFrom"]))
                    {
                        // convert to utc
                        tic.EstimatedCompletionTimeFrom = DateTime.Parse(Request["EstimatedCompletionTimeFrom"]).IMSToUTCDateTime();
                    }
                    else
                    {
                        tic.EstimatedCompletionTimeFrom = null;
                    }
                    if (!string.IsNullOrEmpty(Request["EstimatedCompletionTimeTo"]))
                    {
                        // convert to utc
                        tic.EstimatedCompletionTimeTo = DateTime.Parse(Request["EstimatedCompletionTimeTo"]).IMSToUTCDateTime();
                    }
                    else
                    {
                        tic.EstimatedCompletionTimeTo = null;
                    }


                    // sub-version
                    var aff_ver = Request["AffectedVersion"]?.ToString();
                    var fix_ver = Request["FixedVersion"]?.ToString();
                    tic.AffectedVersionId = aff_ver;
                    tic.FixedVersionId = fix_ver;

                    if (!string.IsNullOrEmpty(tic.AffectedVersionId))
                    {
                        tic.AffectedVersionName = string.Join(", ", db.T_Project_Milestone.Where(x => tic.AffectedVersionId.Contains(x.Id)).Select(x => x.Name).ToList());
                    }
                    else
                    {
                        tic.AffectedVersionName = null;
                    }

                    if (!string.IsNullOrEmpty(tic.FixedVersionId))
                    {
                        tic.FixedVersionName = string.Join(", ", db.T_Project_Milestone.Where(x => tic.FixedVersionId.Contains(x.Id)).Select(x => x.Name).ToList());
                    }
                    else
                    {
                        tic.FixedVersionName = null;
                    }
                    //update
                    tic.ProjectId = Request["Project_select"];
                    var project = db.T_Project_Milestone.Where(x => x.Id == tic.ProjectId).FirstOrDefault();
                    tic.ProjectName = project.Name;

                    if (project.BuildInCode == TypeDevelopment)
                    {
                        //update version
                        if (!string.IsNullOrEmpty(Request["ProjectVersion"]))
                        {
                            //update version
                            var project_version = db.T_Project_Milestone.Find(Request["ProjectVersion"] ?? "");
                            tic.VersionId = project_version.Id;
                            tic.VersionName = project_version.Name;
                        }
                        else
                        {
                            tic.VersionId = null;
                            tic.VersionName = null;
                        }

                        // update stages
                        if (!string.IsNullOrEmpty(Request["Stages"]))
                        {
                            // update stages
                            var listStages = Request["Stages"].Split(',').ToList();
                            tic.StageId = Request["Stages"];
                            tic.StageName = string.Join(",", db.T_Project_Stage.Where(x => listStages.Any(y => y == x.Id)).OrderBy(x => x.Name).Select(x => x.Name).ToList());
                        }
                        else
                        {
                            tic.StageId = null;
                            tic.StageName = null;
                        }

                    }

                    // assigned
                    if (!string.IsNullOrEmpty(Request["Assigned"]))
                    {
                        var ListAssign = Request["Assigned"].Split(',');
                        var ListAssignMemberNumber = new List<string>();

                        foreach (var memberNumber in ListAssign)
                        {
                            if (memberNumber == "auto")
                            {
                                if (project.BuildInCode == TypeDevelopment)
                                {
                                    if (project != null && !string.IsNullOrEmpty(project.LeaderNumber))
                                    {
                                        ListAssignMemberNumber.Add(project.LeaderNumber);
                                    }
                                    if (!string.IsNullOrEmpty(project.ManagerNumber))
                                    {
                                        ListAssignMemberNumber.Add(project.ManagerNumber);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(Request["department"]))
                                    {
                                        var dep = db.P_Department.Where(x => x.Id == tic.GroupID).FirstOrDefault();
                                        if (dep != null)
                                        {
                                            if (!string.IsNullOrEmpty(dep.LeaderNumber))
                                            {
                                                ListAssignMemberNumber.AddRange(dep.LeaderNumber.Split(','));
                                            }
                                            else if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                                            {
                                                ListAssignMemberNumber.Add(dep.SupervisorNumber);
                                            }
                                            else if (dep.ParentDepartmentId != null)
                                            {
                                                var parentdep = db.P_Department.Where(x => x.Id == dep.ParentDepartmentId).FirstOrDefault();
                                                if (parentdep != null && !string.IsNullOrEmpty(parentdep.LeaderNumber))
                                                {
                                                    ListAssignMemberNumber.AddRange(parentdep.LeaderNumber.Split(','));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == memberNumber);
                                if (member != null)
                                {
                                    ListAssignMemberNumber.Add(member.MemberNumber);

                                }
                            }

                        }
                        ListAssignMemberNumber = ListAssignMemberNumber.GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();
                        tic.AssignedToMemberNumber = string.Join(",", ListAssignMemberNumber);
                        tic.AssignedToMemberName = string.Join(",", db.P_Member.Where(x => ListAssignMemberNumber.Any(y => y == x.MemberNumber)).Select(x => x.FullName).ToList());
                    }
                    else
                    {
                        tic.AssignedToMemberName = null;
                        tic.AssignedToMemberNumber = null;
                    }


                    // category
                    if (Request["category"] != null)
                    {
                        tic.CategoryId = Request["category"].ToString();
                        tic.CategoryName = db.T_Project_Milestone.Where(x => x.Id == tic.CategoryId).FirstOrDefault()?.Name;
                    }

                    tic.UpdateAt = DateTime.UtcNow;
                    tic.UpdateBy = cMem.FullName;
                    tic.CreateAt = DateTime.UtcNow;
                    tic.CreateByNumber = cMem.MemberNumber;
                    tic.OpenByMemberNumber = cMem.MemberNumber;
                    tic.CreateByName = cMem.FullName;
                    tic.Note = Request["Note"];
                    tic.KB = string.IsNullOrWhiteSpace(Request["kb"]) == false ? true : false;
                    tic.Visible = true;
                    // file attachment
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(tic.Id, "T_SupportTicket", filesTotal);
                    tic.UploadIds = string.Join(",", UploadIds);
                    db.T_SupportTicket.Add(tic);
                    db.SaveChanges();
                    //Debug.WriteLine(errAutoAssigment);
                    tran.Commit();
                    await this.SendEmailUpdate(tic, "new", cMem.MemberNumber);
                    //Task.Run(async () =>
                    //{
                    //    await _connectorJiraIssueService.CreateJiraIssue(tic.Id);
                    //});

                    // await TicketViewController.AutoAssignment(new WebDataModel(), tic, "new");
                    // BackgroundJob.Enqueue(() => this.SendEmailUpdate(tic, "new", cMem.MemberNumber));
                    return tic.Id;
                }
                catch (Exception ex)
                {
                    tran.Dispose();
                    _logService.Error(ex, $"[Ticket][Add] error when add new ticket");
                    WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                    _writeLogErrorService.InsertLogError(ex);
                    return -1;
                }
            }
        }

        /// <summary>
        /// update ticket
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private async Task<string> UpdateTicket(long id)
        {
            _logService.Info($"[Ticket][UpdateTicket] start update ticket {id}");
            var db = new WebDataModel();

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var tic = db.T_SupportTicket.Where(x => x.Id == id).Include(x => x.T_TicketStatusMapping).Include(x => x.T_TicketTypeMapping).FirstOrDefault();
                    var project = db.T_Project_Milestone.Find(tic.ProjectId);
                    if (!string.IsNullOrWhiteSpace(Request["name"]))
                    {
                        //cho phep update name/content ticket
                        tic.Name = Request["name"];
                        tic.Description = Request.Unvalidated["comment"];
                    }
                    // set Mention
                    if (!string.IsNullOrEmpty(Request["NewMention"]))
                    {
                        TempData["NewMention"] = Request["NewMention"];
                    }

                    // tag member
                    tic.TagMemberNumber = Request["TagMemberNumber"];

                    if (!string.IsNullOrEmpty(tic.TagMemberNumber))
                    {
                        tic.TagMemberName = string.Join(",", db.P_Member.Where(x => tic.TagMemberNumber.Contains(x.MemberNumber)).Select(x => x.FullName));
                    }
                    else
                    {
                        tic.TagMemberName = null;
                    }

                    var ticketCustomer = null as C_Customer;
                    // customer
                    if (!string.IsNullOrEmpty(Request["merchant"]))
                    {
                        var customerCode = Request["merchant"].ToString();
                        ticketCustomer = db.C_Customer.Where(x => x.CustomerCode == customerCode).FirstOrDefault();
                        if (ticketCustomer != null)
                        {
                            tic.CustomerCode = ticketCustomer.CustomerCode;
                            tic.CustomerName = ticketCustomer.BusinessName;
                        }
                        else
                        {
                            tic.CustomerCode = null;
                            tic.CustomerName = null;
                        }
                    }
                    else
                    {
                        tic.CustomerCode = null;
                        tic.CustomerName = null;
                    }




                    // status
                    db.T_TicketStatusMapping.RemoveRange(tic.T_TicketStatusMapping);
                    //  tic.T_TicketStatusMapping.Clear();
                    bool closeTicket = false;
                    bool wentlive_donedelivery = false;
                    if (!string.IsNullOrEmpty(Request["statusId"]))
                    {
                        foreach (var statusItem in Request["statusId"].Split(','))
                        {
                            var StatusId = long.Parse(statusItem);
                            var st = db.T_TicketStatus.Find(StatusId);
                            if (st != null)
                            {
                                tic.T_TicketStatusMapping.Add(new T_TicketStatusMapping
                                {
                                    StatusId = st.Id,
                                    StatusName = st.Name,
                                    TicketId = tic.Id,
                                });
                            }
                            if (closeTicket != true && st.Type?.ToLower().Equals("closed") == true)
                            {
                                closeTicket = true;
                            }
                            if (st.Name == "Went Live (Done Delivery)")
                            {
                                wentlive_donedelivery = true;
                            }
                        }
                        if (project.BuildInCode == TypeDeployment)
                        {
                            var goliveStatusId = long.Parse(ConfigurationManager.AppSettings["DeploymentGoLiveStatusId"]);
                            if (tic.T_TicketStatusMapping.Any(x => x.StatusId == goliveStatusId) && ticketCustomer != null)
                            {
                                if (tic.DateClosed != null)
                                    tic.DateClosed = DateTime.UtcNow;
                                if (ticketCustomer.GoLiveDate != null)
                                    ticketCustomer.GoLiveDate = DateTime.UtcNow;
                            }
                        }
                    }


                    //update status
                    tic.StatusId = string.Join(",", tic.T_TicketStatusMapping.Select(x => x.StatusId));
                    tic.StatusName = string.Join(", ", tic.T_TicketStatusMapping.Select(x => x.StatusName));
                    if (closeTicket == true)
                    {
                        var checkTaskCompleted = db.Ts_Task.Where(x => x.TicketId == tic.Id && x.ParentTaskId == null && x.Requirement == true).ToList();
                        if (checkTaskCompleted.Count() > 0 && checkTaskCompleted.Any(x => x.Complete != true))
                        {
                            throw new Exception("Please complete all required tasks before closing this ticket");
                        }
                        var reminderTicket = db.T_RemindersTicket.Where(x => x.TicketId == tic.Id).FirstOrDefault();
                        if (reminderTicket != null)
                        {
                            var reminderTicketService = new ReminderTicketService();
                            reminderTicketService.DeleteJob(reminderTicket.HangfireJobId);
                            db.T_RemindersTicket.Remove(reminderTicket);
                        }
                        tic.CloseByMemberNumber = cMem.MemberNumber;
                        tic.CloseByName = cMem.FullName;
                        tic.DateClosed = DateTime.UtcNow;
                    }
                    else
                    {
                        tic.CloseByMemberNumber = null;
                        tic.CloseByName = null;
                        tic.DateClosed = null;
                    }

                    //type
                    db.T_TicketTypeMapping.RemoveRange(tic.T_TicketTypeMapping);
                    if (!string.IsNullOrEmpty(Request["type"]))
                    {
                        foreach (var typeId in Request["type"].Split(','))
                        {
                            var Id = long.Parse(typeId);
                            var type = db.T_TicketType.Find(Id);
                            if (type != null)
                            {
                                tic.T_TicketTypeMapping.Add(new T_TicketTypeMapping
                                {
                                    TypeId = type.Id,
                                    TypeName = type.TypeName,
                                    TicketId = tic.Id,
                                });
                            }

                        }
                    }
                    tic.TypeId = string.Join(",", tic.T_TicketTypeMapping.Select(x => x.TypeId));
                    tic.TypeName = string.Join(", ", tic.T_TicketTypeMapping.Select(x => x.TypeName));


                    //priority
                    if (!string.IsNullOrWhiteSpace(Request["Priority"]))
                    {
                        tic.PriorityId = int.Parse(Request["Priority"]?.ToString());
                        tic.PriorityName = db.T_Priority.Where(x => x.Id == tic.PriorityId).FirstOrDefault()?.Name;
                    }
                    else
                    {
                        tic.PriorityId = null;
                        tic.PriorityName = string.Empty;
                    }


                    // severity
                    if (!string.IsNullOrWhiteSpace(Request["Severity"]))
                    {
                        tic.SeverityId = long.Parse(Request["Severity"]);
                        tic.SeverityName = db.T_TicketSeverity.Find(tic.SeverityId).SeverityName;
                    }
                    else
                    {
                        tic.SeverityId = null;
                        tic.SeverityName = string.Empty;
                    }

                    // MilestoneServices.UpdateTicketMileStone(tic, Request, db);
                    // deadline and estimated time
                    if (!string.IsNullOrEmpty(Request["Deadline"]))
                    {
                        tic.Deadline = DateTime.Parse(Request["Deadline"]).IMSToUTCDateTime();
                        //if (closeTicket != true)
                        //{
                        //    var DeadlineNotificationService = new DeadlineNotificationService();
                        //    if (string.IsNullOrEmpty(tic.DeadlineHangfireJobId))
                        //    {
                        //        tic.DeadlineHangfireJobId = Guid.NewGuid().ToString();
                        //    }
                        //  //  DeadlineNotificationService.CreateJob(tic);
                        //}

                    }
                    else
                    {
                        tic.Deadline = null;
                        if (!string.IsNullOrEmpty(tic.DeadlineHangfireJobId))
                        {
                            //   var DeadlineNotificationService = new DeadlineNotificationService();
                            //DeadlineNotificationService.DeleteJob(tic.DeadlineHangfireJobId);
                            //tic.DeadlineHangfireJobId = null;
                        }
                    }


                    if (!string.IsNullOrEmpty(Request["EstimatedCompletionTimeFrom"]))
                    {
                        // convert to utc
                        tic.EstimatedCompletionTimeFrom = DateTime.Parse(Request["EstimatedCompletionTimeFrom"]).IMSToUTCDateTime();
                    }
                    else
                    {
                        tic.EstimatedCompletionTimeFrom = null;
                    }

                    if (!string.IsNullOrEmpty(Request["EstimatedCompletionTimeTo"]))
                    {
                        // convert to utc
                        tic.EstimatedCompletionTimeTo = DateTime.Parse(Request["EstimatedCompletionTimeTo"]).IMSToUTCDateTime();
                    }
                    else
                    {
                        tic.EstimatedCompletionTimeTo = null;
                    }

                    //update sub version
                    var aff_ver = Request["AffectedVersion"]?.ToString();
                    var fix_ver = Request["FixedVersion"]?.ToString();
                    tic.AffectedVersionId = aff_ver;
                    tic.FixedVersionId = fix_ver;


                    if (!string.IsNullOrEmpty(tic.AffectedVersionId))
                    {
                        tic.AffectedVersionName = string.Join(", ", db.T_Project_Milestone.Where(x => tic.AffectedVersionId.Contains(x.Id)).Select(x => x.Name).ToList());
                    }
                    else
                    {
                        tic.AffectedVersionName = null;
                    }

                    if (!string.IsNullOrEmpty(tic.FixedVersionId))
                    {
                        tic.FixedVersionName = string.Join(", ", db.T_Project_Milestone.Where(x => tic.FixedVersionId.Contains(x.Id)).Select(x => x.Name).ToList());
                    }
                    else
                    {
                        tic.FixedVersionName = null;
                    }

                    // update category
                    if (Request["category"] != null)
                    {
                        tic.CategoryId = Request["category"].ToString();
                        tic.CategoryName = db.T_Project_Milestone.Where(x => x.Id == tic.CategoryId).FirstOrDefault()?.Name;
                    }
                    else
                    {
                        tic.CategoryId = null;
                        tic.CategoryName = null;
                    }
                    //update project
                    tic.ProjectId = project.Id;
                    tic.ProjectName = project.Name;

                    if (project.BuildInCode == TypeDevelopment)
                    {
                        //update version
                        if (!string.IsNullOrEmpty(Request["ProjectVersion"]))
                        {
                            var project_version = db.T_Project_Milestone.Find(Request["ProjectVersion"] ?? "");
                            tic.VersionId = project_version.Id;
                            tic.VersionName = project_version.Name;
                        }
                        else
                        {
                            tic.VersionId = null;
                            tic.VersionName = null;
                        }

                        // update stages
                        if (!string.IsNullOrEmpty(Request["Stages"]))
                        {
                            var listStages = Request["Stages"].Split(',').ToList();
                            tic.StageId = Request["Stages"];
                            tic.StageName = string.Join(",", db.T_Project_Stage.Where(x => listStages.Any(y => y == x.Id)).OrderBy(x => x.Name).Select(x => x.Name).ToList());
                        }
                        else
                        {
                            tic.StageId = null;
                            tic.StageName = null;
                        }
                    }
                    else
                    {
                        // department and other department
                        if (!string.IsNullOrEmpty(Request["department"]))
                        {
                            tic.GroupID = long.Parse(Request["department"].ToString());
                            tic.GroupName = db.P_Department.Where(c => c.Id == tic.GroupID).FirstOrDefault().Name;
                        }
                        else
                        {
                            tic.GroupID = null;
                            tic.GroupName = null;
                        }

                        if (!string.IsNullOrEmpty(Request["Other_Department"]))
                        {
                            tic.OtherGroupId = Request["Other_Department"].ToString();
                            List<long> OtherDepartmentIds = tic.OtherGroupId.Split(',').Select(long.Parse).ToList();
                            tic.OtherGroupName = string.Join(", ", db.P_Department.Where(c => OtherDepartmentIds.Any(y => y == c.Id)).Select(x => x.Name));
                        }
                        else
                        {
                            tic.OtherGroupId = null;
                            tic.OtherGroupName = null;
                        }


                    }
                    // assigned
                    if (!string.IsNullOrEmpty(Request["Assigned"]))
                    {
                        var ListAssign = Request["Assigned"].Split(',');
                        var ListAssignMemberNumber = new List<string>();

                        foreach (var memberNumber in ListAssign)
                        {
                            if (memberNumber == "auto")
                            {
                                if (project.BuildInCode == TypeDevelopment)
                                {
                                    if (!string.IsNullOrEmpty(project.LeaderNumber))
                                    {
                                        ListAssignMemberNumber.Add(project.LeaderNumber);

                                    }
                                    if (!string.IsNullOrEmpty(project.ManagerNumber))
                                    {
                                        ListAssignMemberNumber.Add(project.ManagerNumber);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(Request["department"]))
                                    {
                                        var dep = db.P_Department.Where(x => x.Id == tic.GroupID).FirstOrDefault();
                                        if (dep != null)
                                        {
                                            if (!string.IsNullOrEmpty(dep.LeaderNumber))
                                            {
                                                ListAssignMemberNumber.AddRange(dep.LeaderNumber.Split(','));
                                            }
                                            else if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                                            {
                                                ListAssignMemberNumber.Add(dep.SupervisorNumber);
                                            }
                                            else if (dep.ParentDepartmentId != null)
                                            {
                                                var parentdep = db.P_Department.Where(x => x.Id == dep.ParentDepartmentId).FirstOrDefault();
                                                if (parentdep != null && !string.IsNullOrEmpty(parentdep.LeaderNumber))
                                                {
                                                    ListAssignMemberNumber.AddRange(parentdep.LeaderNumber.Split(','));
                                                }
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == memberNumber);
                                if (member != null)
                                {
                                    ListAssignMemberNumber.Add(member.MemberNumber);

                                }

                            }

                        }
                        ListAssignMemberNumber = ListAssignMemberNumber.GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();
                        tic.AssignedToMemberNumber = string.Join(",", ListAssignMemberNumber);
                        tic.AssignedToMemberName = string.Join(",", db.P_Member.Where(x => ListAssignMemberNumber.Any(y => y == x.MemberNumber)).Select(x => x.FullName).ToList());
                    }
                    else
                    {
                        tic.AssignedToMemberName = null;
                        tic.AssignedToMemberNumber = null;
                    }



                    //label
                    tic.Tags = Request["label"];
                    if (!string.IsNullOrEmpty(tic.Tags))
                    {
                        tic.TagName = string.Join(",", db.T_Tags.Where(x => tic.Tags.Contains(x.Id)).Select(x => x.Name).ToList());
                    }
                    else
                    {
                        tic.TagName = null;
                    }
                    //ticket info

                    tic.Visible = string.IsNullOrWhiteSpace(Request["visible"]) == false ? true : false;
                    tic.KB = string.IsNullOrWhiteSpace(Request["kb"]) == false ? true : false;
                    tic.Note = Request["Note"];



                    // update date
                    tic.UpdateAt = DateTime.UtcNow;
                    tic.UpdateBy = cMem.FullName;
                    tic.UpdateTicketHistory += DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName + "|";
                    //attach file
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(tic.Id, "T_SupportTicket", filesTotal);
                    tic.UploadIds = string.Join(",", UploadIds);
                    if (wentlive_donedelivery == true && project.BuildInCode == TypeDeployment)
                    {
                        try
                        {
                            var SupportProj = db.T_Project_Milestone.Where(x => x.BuildInCode == PageSupport).FirstOrDefault();
                            tic.ProjectId = SupportProj.Id;
                            var BuildInCode = SupportProj.BuildInCode;
                            //get list task_template
                            List<Ts_TaskTemplateCategory> task_template = db.Ts_TaskTemplateCategory.Where(x => x.TicketGroup == BuildInCode && x.IsDeleted != true).ToList();
                            for (var i = 0; i < task_template.Count; i++)
                            {
                                int cateid = task_template[i].Id;
                                //get list task_template_field
                                List<Ts_TaskTemplateField> lst_field = db.Ts_TaskTemplateField.Where(a => a.CategoryId == cateid).ToList();
                                var subtaskId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                                var _idtem = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                                string message = "";
                                long taskId;
                                int taskCount = db.Ts_Task.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
                                && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
                                var task = new Ts_Task
                                {
                                    Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (taskCount + 1).ToString().PadLeft(4, '0') + DateTime.UtcNow.ToString("ff")),
                                    Name = task_template[i].Name,
                                    Complete = false,
                                    Description = task_template[i].Description,
                                    Requirement = task_template[i].Requirement,
                                    TaskTemplateCategoryId = task_template[i].Id,
                                    CreateBy = cMem.FullName,
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
                                var ticket = db.T_SupportTicket.Where(x => x.Id == tic.Id).FirstOrDefault();
                                if (ticket != null)
                                {
                                    task.TicketId = ticket.Id;
                                    task.TicketName = ticket.Name;
                                }

                                db.Ts_Task.Add(task);
                                taskId = task.Id;


                                var ListSubTask = new List<SubTaskTemplate>();
                                foreach (var item in lst_field)
                                {
                                    var SubTask = new SubTaskTemplate();
                                    SubTask.Id = subtaskId;
                                    SubTask.Name = item.Name;
                                    SubTask.Required = item.IsRequired;
                                    SubTask.Description = item.Description;
                                    ListSubTask.Add(SubTask);



                                    var subtask = new Ts_Task
                                    {
                                        Name = item.Name,
                                        Complete = false,
                                    };

                                    subtask.Id = subtaskId;
                                    subtask.TicketId = task.TicketId;
                                    subtask.TicketName = task.TicketName;
                                    subtask.ParentTaskId = task.Id;
                                    subtask.ParentTaskName = task.Name;
                                    subtask.Description = task.Description;
                                    subtask.CreateBy = cMem.FullName;
                                    subtask.CreateByMemberNumber = cMem.MemberNumber;
                                    subtask.CreateAt = DateTime.UtcNow;
                                    subtaskId++;
                                    db.Ts_Task.Add(subtask);
                                }
                                db.SaveChanges();
                            }

                        }
                        catch (Exception e)
                        {

                        }

                    }

                    db.Entry(tic).State = System.Data.Entity.EntityState.Modified;

                    //add/active forward stage to ticket
                    db.SaveChanges();

                    tran.Commit();
                    tran.Dispose();
                    //chi email thong bao khi update status.
                    //if (current_status != ts_status.StatusId || currentAssigned != ts_status.AssignedMember_Numbers || shared_leaders.Count > 0)
                    //{
                    var ActionFeedBack = bool.Parse(Request["ActionFeedBack"].ToString());
                    if (TempData["feedback_sent"] == null && ActionFeedBack != true)
                    {
                        var listExcludeTicketUpdateNotice = new List<string>();
                        if (!string.IsNullOrEmpty(Request["excludeTicketUpdateNotice"]))
                        {
                            listExcludeTicketUpdateNotice = Request["excludeTicketUpdateNotice"].ToString().Split(',').ToList();
                        }
                        await TicketViewService.SendNoticeAfterTicketUpdate(tic, "update", db, cMem.MemberNumber, null, listExcludeTicketUpdateNotice);
                    }
                    else
                    {
                        TempData["feedback_sent"] = null;
                    }
                    //Task.Run(async () =>
                    //{
                    //    await _connectorJiraIssueService.UpdateJiraIssue(tic.Id);
                    //});
                    //}
                    //await TicketViewController.AutoAssignment(db, tic, "update");
                    //await TicketViewController.AutoTicketScenario.UpdateSatellite(tic.Id, db, cMem.FullName);
                    _logService.Info($"[Ticket][UpdateTicket] complete update ticket {id}");
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    tran.Dispose();
                    _logService.Error(ex, $"[Ticket][Save] error when process save ticket {id}");
                    WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                    _writeLogErrorService.InsertLogError(ex);

                    //tran.Dispose();
                    return ex.Message;
                }
            }
            //nếu ticket delivery chuyển sang went live thì đẩy sang support

        }

        /// <summary>
        /// Save ticket info
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> SaveTicketInfo()
        {
            try
            {
                long id = long.Parse(Request["Id"]);
                var result = await UpdateTicket(id);
                if (string.IsNullOrWhiteSpace(result))
                {
                    var db = new WebDataModel();

                    var tic = db.T_TicketStage_Status.FirstOrDefault(c => c.TicketId == id);
                    var IsClosed = (db.T_TicketStatus.Find(tic?.StatusId)?.Type == "closed");

                    return Json(new object[] { true, "Ticket #" + id + " has been saved", IsClosed });

                    //msg is AssignedToMemberNumber
                }
                else
                {
                    throw new Exception(result);
                }

            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Ticket][SaveTicketInfo] error when save ticket info");
                return Json(new object[] { false, e.Message });
            }
        }

        #endregion

        #region report
        public ActionResult Report()
        {
            try
            {/*---*/
                var db = new WebDataModel();

                ViewBag.project = GetCurrentStage(db, PageDevelopmentsTicket).FirstOrDefault().stage.ProjectId;
                var project_cMem = (from m in db.T_Project_Stage_Members
                                    where m.MemberNumber == cMem.MemberNumber
                                    join stg in db.T_Project_Stage on m.StageId equals stg.Id
                                    select stg.ProjectId).Distinct().ToList();
                var can_view_all = (access.Any(k => k.Key.Equals("ticket_view_report_all")) == true && access["ticket_view_report_all"] == true);
                ViewBag.projects = db.T_Project_Milestone.Where(m => m.Type == "project" && (m.BuildInCode == TypeDevelopment) && (project_cMem.Contains(m.Id) || can_view_all)).ToList();

            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex);
            }
            return View();
        }
        public JsonResult Report_data(string project_id, int month = 0)
        {
            try
            {
                var db = new WebDataModel();
                var project = db.T_Project_Milestone.Find(project_id);
                if (project == null)
                {
                    return Json(new { result = false, message = "Project not found!" });
                }
                var project_cMem = (from m in db.T_Project_Stage_Members
                                    where m.MemberNumber == cMem.MemberNumber
                                    join stg in db.T_Project_Stage on m.StageId equals stg.Id
                                    select stg.ProjectId).Distinct().ToList();
                var can_view_all = (access.Any(k => k.Key.Equals("ticket_view_report_all")) == true && access["ticket_view_report_all"] == true);
                if (!can_view_all && !project_cMem.Contains(project_id))
                {
                    return Json(new { result = false, message = "You cant view report of this project!" });
                }
                var List_data = new List<Chart_data>();
                var stages = db.T_Project_Stage.Where(s => s.ProjectId == project.Id).OrderBy(s => s.Name).ToList();
                var tk_item = db.T_TicketStage_Status.Where(t =>
                ((month == 0 && t.OpenDate.Value.Month == DateTime.Now.Month)
                || (month == 1 && t.OpenDate.Value.Month == DateTime.Now.Month - 1 && t.OpenDate.Value.Year == DateTime.Now.Year)
                || (month == 3 && t.OpenDate.Value.Month > DateTime.Now.Month - 3 && t.OpenDate.Value.Year == DateTime.Now.Year)
                || (month == 6 && t.OpenDate.Value.Month > DateTime.Now.Month - 6 && t.OpenDate.Value.Year == DateTime.Now.Year)
                || (month == 12 && t.OpenDate.Value.Year == DateTime.Now.Year)
                ));
                if (stages.Count() > 0)
                {
                    stages.ForEach(stage =>
                    {
                        var _Count = (from s in tk_item
                                      where s.StageId == stage.Id
                                      join version in db.T_Project_Milestone on s.ProjectVersionId equals version.Id
                                      join status in db.T_TicketStatus on s.StatusId equals status.Id into g
                                      from sta in g.DefaultIfEmpty()
                                      select new
                                      {
                                          ver = version.Id,
                                          sts = (sta == null) ? "open" : (string.IsNullOrEmpty(sta.Type) ? "open" : sta.Type),
                                      } into s
                                      group s by s into s
                                      select new
                                      {
                                          key = s.Key,
                                          count = s.Count()
                                      }).ToList();
                        var versions = db.T_Project_Milestone.Where(m => m.Type == "Project_version" && m.ParentId == project.Id).OrderBy(v => v.Name).ToList();
                        List_data.Add(new Chart_data
                        {
                            open = (from v in versions
                                    join g in _Count.Where(lc => lc.key.sts == "open") on v.Id equals g.key.ver into _g
                                    from g in _g.DefaultIfEmpty()
                                    select (g == null ? 0 : g.count)).ToList(),
                            closed = (from v in versions
                                      join g in _Count.Where(lc => lc.key.sts == "closed") on v.Id equals g.key.ver into _g
                                      from g in _g.DefaultIfEmpty()
                                      select (g == null ? 0 : g.count)).ToList(),
                            all = (from v in versions
                                   join g in _Count.GroupBy(a => a.key.ver) on v.Id equals g.Key into _g
                                   from g in _g.DefaultIfEmpty()
                                   select (g == null ? 0 : g.Sum(a => a.count))).ToList(),
                            label = versions.Select(v => v.Name).ToList(),
                            title = $"{stage.Name}'s statistic",
                            stage = stage.Name,
                        });
                    });
                }
                var Totalticket = new { value = List_data.Sum(s => s.all.Sum()), detail = string.Join(" | ", List_data.Select(s => s.stage + ": " + s.all.Sum())) };
                var TotalOpenticket = new { value = List_data.Sum(s => s.open.Sum()), detail = string.Join(" | ", List_data.Select(s => s.stage + ": " + s.open.Sum())) };
                var TotalClosedticket = new { value = List_data.Sum(s => s.closed.Sum()), detail = string.Join(" | ", List_data.Select(s => s.stage + ": " + s.closed.Sum())) };
                return Json(new { result = true, List_data, y_max = List_data.Select(s => s.all.DefaultIfEmpty(0).Max()).DefaultIfEmpty(0).Max(), Totalticket, TotalOpenticket, TotalClosedticket });
            }
            catch (Exception ex)
            {
                return Json(new { result = false, message = ex.Message, detail = ex.ToString() });
            }
        }
        public class Chart_data
        {
            public List<int> open { get; set; }
            public List<int> closed { get; set; }
            public List<int> all { get; set; }
            public List<string> label { get; set; }
            public string title { get; internal set; }
            public string stage { get; internal set; }
        }
        #endregion

        #region Ajax -Angular
        public JsonResult LoadLabel(string Page)
        {
            var db = new WebDataModel();

            if (Page == PageDeployment)
            {
                return Json(db.T_Tags.Where(x => x.Type == PageDeployment).ToList());
            }
            else
            {
                return Json(db.T_Tags.Where(x => x.Type != PageDeployment).ToList());
            }

        }

        //public JsonResult NewStageVersion(string version_name, string projectVerId)
        //{
        //    try
        //    {
        //        var proj_ver = db.T_Project_Milestone.Find(projectVerId);
        //        if (db.T_Project_Milestone.Any(v => v.Name == version_name && v.ParentId == projectVerId))
        //        {
        //            return Json(new object[] { false, "Version name already exist!" });
        //        }
        //        var version = new T_Project_Milestone
        //        {
        //            Id = Guid.NewGuid().ToString("N"),
        //            Active = true,
        //            Name = version_name,
        //            ParentName = proj_ver.Name,
        //            ParentId = proj_ver.Id,
        //            Type = "version",
        //        };
        //        db.T_Project_Milestone.Add(version);
        //        db.SaveChanges();
        //        return Json(new object[] { true, "Create new version completed!", version });
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, "Create version fail!", e.Message });
        //    }

        //}
        //public JsonResult RenameStageVersion(string Id, string version_name)
        //{
        //    try
        //    {
        //        var version = db.T_Project_Milestone.Find(Id);
        //        if (db.T_Project_Milestone.Any(v => v.Name == version_name && v.ParentId == version.ParentId && v.Id != Id))
        //        {
        //            return Json(new object[] { false, "Version name already exist!" });
        //        }

        //        if (version == null)
        //        {
        //            return Json(new object[] { false, "Version not found!" });
        //        }
        //        version.Name = version_name;
        //        db.Entry(version).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return Json(new object[] { true, "Rename version completed!", version });
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, "Rename version fail!", e.Message });
        //    }

        //}



        #endregion

        [AllowAnonymous]
        public JsonResult getStageMember(string projectId, long? tkId, long? departmentId, List<long> otherDepartmentId)
        {
            try
            {
                var db = new WebDataModel();
                var ListMemberAssign = new List<AssignMemberModel>();
                var project = db.T_Project_Milestone.Where(x => x.Id == projectId).FirstOrDefault();
                var ticket = db.T_SupportTicket.Where(x => x.Id == tkId).FirstOrDefault();
                if (project.BuildInCode != TypeDevelopment)
                {
                    var ListMemberNumber = new List<string>();
                    if (departmentId != null)
                    {
                        var dep = db.P_Department.Where(x => x.Id == departmentId).FirstOrDefault();
                        if (!string.IsNullOrEmpty(dep.GroupMemberNumber))
                        {
                            ListMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                        }
                        if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                        {
                            ListMemberNumber.Add(dep.SupervisorNumber);
                        }
                        if (!string.IsNullOrEmpty(dep.LeaderNumber))
                        {
                            ListMemberNumber.AddRange(dep.LeaderNumber.Split(','));
                        }
                        var depchild = db.P_Department.Where(x => x.ParentDepartmentId == dep.Id).ToList();
                        if (depchild.Count > 0)
                        {
                            foreach (var depitem in depchild)
                            {
                                if (!string.IsNullOrEmpty(depitem.GroupMemberNumber))
                                {
                                    ListMemberNumber.AddRange(depitem.GroupMemberNumber.Split(','));
                                }
                                if (!string.IsNullOrEmpty(depitem.SupervisorNumber))
                                {
                                    ListMemberNumber.Add(depitem.SupervisorNumber);
                                }
                                if (!string.IsNullOrEmpty(depitem.LeaderNumber))
                                {
                                    ListMemberNumber.AddRange(depitem.LeaderNumber.Split(','));
                                }
                            }
                        }
                    }
                    if (otherDepartmentId != null && otherDepartmentId.Count > 0)
                    {
                        var otherDepartments = db.P_Department.Where(x => otherDepartmentId.Any(y => y == x.Id)).ToList();
                        foreach (var otherDepartment in otherDepartments)
                        {
                            if (!string.IsNullOrEmpty(otherDepartment.GroupMemberNumber))
                            {
                                ListMemberNumber.AddRange(otherDepartment.GroupMemberNumber.Split(','));
                            }
                            if (!string.IsNullOrEmpty(otherDepartment.SupervisorNumber))
                            {
                                ListMemberNumber.Add(otherDepartment.SupervisorNumber);
                            }
                            if (!string.IsNullOrEmpty(otherDepartment.LeaderNumber))
                            {
                                ListMemberNumber.AddRange(otherDepartment.LeaderNumber.Split(','));
                            }
                        }
                    }

                    if (ticket != null && !string.IsNullOrEmpty(ticket.TagMemberNumber))
                    {
                        ListMemberNumber.Add(ticket.TagMemberNumber);
                    }
                    ListMemberNumber = ListMemberNumber.GroupBy(x => x).Select(x => x.First()).ToList();
                    var listMemberSelected = db.P_Member.Where(x => ListMemberNumber.Any(y => y.Contains(x.MemberNumber)) && x.Active == true && x.Delete != true).ToList();
                    ListMemberAssign.AddRange(listMemberSelected.Select(x => new AssignMemberModel { Gender = x.Gender, Avatar = x.Picture, MemberNumber = x.MemberNumber, MemberName = x.FullName }));
                }
                else
                {
                    var ListProjectMember = (from memProject in db.T_Project_Milestone_Member
                                             join member in db.P_Member on memProject.MemberNumber equals member.MemberNumber
                                             where memProject.ProjectId == projectId && member.Active == true && member.Delete != true
                                             select new AssignMemberModel { Gender = member.Gender, Avatar = member.Picture, MemberNumber = member.MemberNumber, MemberName = member.FullName }).ToList();
                    ListMemberAssign.AddRange(ListProjectMember);
                }
                return Json(new { status = true, member = ListMemberAssign });

            }
            catch (Exception e)
            {
                return Json(new { status = false, message = e.Message });
            }

        }

        #region category
        [AllowAnonymous]
        public ActionResult LoadCategory()
        {
            var db = new WebDataModel();

            var categories = db.T_Project_Milestone.Where(x => x.Active != false && x.Type == "category").Select(x => new { x.Id, x.Name }).ToList();
            return Json(categories);

        }
        public ActionResult CategorySave(string CategoryId, string CategoryName)
        {
            var db = new WebDataModel();
            if (db.T_Project_Milestone.Count(x => x.Name == CategoryName && x.Type == "category") > 0)
            {
                return Json(new { status = false, message = "category name exist" });
            }
            if (string.IsNullOrEmpty(CategoryId))
            {
                var category = new T_Project_Milestone();
                category.Name = CategoryName;
                category.Type = "category";
                category.Active = true;
                category.Id = Guid.NewGuid().ToString();
                db.T_Project_Milestone.Add(category);
                db.SaveChanges();
                return Json(new { status = true, message = "add category success", category });
            }
            else
            {
                var category = db.T_Project_Milestone.Find(CategoryId);
                if (category == null)
                {
                    return Json(new { status = false, message = "category not found" });
                }
                category.Name = CategoryName;
                db.SaveChanges();
                return Json(new { status = true, message = "update category success", category });
            }
        }
        public ActionResult DeleteCategory(string CategoryId)
        {
            var db = new WebDataModel();
            try
            {
                var category = db.T_Project_Milestone.Find(CategoryId);
                if (category == null)
                {
                    return Json(new { status = false, message = "category not found" });
                }
                db.T_Project_Milestone.Remove(category);
                db.SaveChanges();
                return Json(new { status = true, message = "delete category success !" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "system error !" });
            }

        }
        #endregion

        #region ticket close/reopen/feedback

        [HttpPost]
        public async Task<JsonResult> Feedback(T_RemindersTicket model)
        {
            long ticketId = long.Parse(Request["fb_ticketId"]);
            _logService.Info($"[Ticket][AddFeedback] start add feedback ticket Id:${ticketId}");
            var db = new WebDataModel();

            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //upload attachments
                    string fb_content = Request.Unvalidated["fb_content"];
                    //long fb_status = string.IsNullOrWhiteSpace(Request["fb_status"]) == true ? 0 : long.Parse(Request["fb_status"]);
                    long fbId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                    var tic = db.T_SupportTicket.Find(ticketId);
                    DateTime createFb = DateTime.UtcNow;
                    var fb = new T_TicketFeedback
                    {
                        CreateAt = createFb,
                        CreateByName = cMem.FullName,
                        CreateByNumber = cMem.MemberNumber,
                        Feedback = fb_content,
                        Id = fbId,
                        MentionMemberNumbers = Request["MentionMemberNumbers"],
                        DateCode = createFb.ToString("yyyyMMdd"),
                        GlobalStatus = Request["globalStatus"],
                        TicketId = ticketId
                    };
                    var newStatus = long.Parse(Request["statusTicket"]);
                    //if (newStatus != fb_status)
                    //{
                    //    var status = db.T_TicketStatus.Find(newStatus);
                    //    fb.TicketStatusChanges = status?.Name;
                    //    fb.FeedbackTitle = "Status: " + status?.Name;
                    //    //update ticket
                    //    //tic.StatusId = fb_status;
                    //    //tic.StatusName = status?.Name;
                    //    //if (status.Type == "closed")
                    //    //{
                    //    //    tic.DateClosed = DateTime.UtcNow;
                    //    //    tic.CloseByMemberNumber = cMem.MemberNumber;
                    //    //    tic.CloseByName = cMem.FullName;
                    //    //}
                    //}
                    //update ticket
                    tic.FeedbackTicketHistory += createFb.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName + "|";
                    tic.UpdateTicketHistory += createFb.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName + "|";
                    tic.UpdateAt = DateTime.UtcNow;
                    tic.UpdateBy = cMem.FullName;
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(fb.Id, "T_TicketFeedback", filesTotal);
                    fb.Attachments = string.Join(",", UploadIds);
                    db.Entry(tic).State = System.Data.Entity.EntityState.Modified;
                    db.T_TicketFeedback.Add(fb);



                    //var reminder = db.T_RemindersTicket.FirstOrDefault(x => x.TicketId == ticketId);
                    //if (reminder!=null)
                    //{
                    //    var reminderTicketService = new ReminderTicketService();
                    //    var reminderDateUTC = (model.Date.Value.Date + model.Time).Value.AddHours(-GMTNumber);
                    //    if (Request["enableReminder"] == "true")
                    //    {

                    //        //reminder.Note = model.Note;
                    //        reminder.Repeat = model.Repeat;
                    //        reminder.Time = reminderDateUTC.TimeOfDay;
                    //        reminder.Date = reminderDateUTC.Date;
                    //        reminder.UpdateAt = DateTime.UtcNow;
                    //        reminder.UpdateBy = cMem.FullName;
                    //        db.SaveChanges();
                    //        reminderTicketService.CreateJob(reminder);
                    //    }
                    //    else
                    //    {
                    //        reminderTicketService.DeleteJob(reminder.HangfireJobId);
                    //        db.T_RemindersTicket.Remove(reminder);
                    //    }

                    //}
                    //else
                    //{

                    //    if (Request["enableReminder"] == "true")
                    //    {
                    //        var reminderTicketService = new ReminderTicketService();
                    //        var reminderDateUTC = (model.Date.Value.Date + model.Time).Value.AddHours(-GMTNumber);
                    //        model.Time = reminderDateUTC.TimeOfDay;
                    //        model.Date = reminderDateUTC.Date;
                    //        model.Note = fb.Feedback;
                    //        model.CreateAt = DateTime.UtcNow;
                    //        model.CreateBy = cMem.FullName;
                    //        model.TicketId = tic.Id;
                    //        model.FeedbackId = fb.Id;
                    //        model.Active = true;
                    //        model.HangfireJobId = Guid.NewGuid().ToString();
                    //        db.T_RemindersTicket.Add(model);
                    //        db.SaveChanges();
                    //        reminderTicketService.CreateJob(model);
                    //    }
                    //}
                    //add fb

                    db.SaveChanges();
                    tran.Commit();
                    tran.Dispose();
                    var listExcludeTicketUpdateNotice = new List<string>();
                    if (!string.IsNullOrEmpty(Request["excludeTicketUpdateNotice"]))
                    {
                        listExcludeTicketUpdateNotice = Request["excludeTicketUpdateNotice"].ToString().Split(',').ToList();
                    }
                    await TicketViewService.SendNoticeAfterTicketUpdate(tic, "feedback", null, cMem.MemberNumber, null, listExcludeTicketUpdateNotice);
                    //await TicketViewController.AutoTicketScenario.UpdateSatellite(tic.Id, null, cMem.FullName);

                    // await _connectorJiraIssueService.CreateJiraComment(fb.Id);

                    TempData["feedback_sent"] = true;
                    _logService.Info($"[Ticket][AddFeedback] completed add feedback ticket id:${ticketId}");
                    return Json(new object[] { true, "Feedback successfull" });
                }
                catch (Exception e)
                {
                    _logService.Info($"[Ticket][AddFeedback] error when add feeback ticket id:${ticketId}");
                    tran.Dispose();
                    return Json(new object[] { false, e.Message });
                }
            }



        }
        [HttpPost]
        public async Task<ActionResult> FeedbackKB()
        {
            long ticketId = long.Parse(Request["fb_ticketId"]);
            var db = new WebDataModel();
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    //upload attachments
                    string fb_content = Request.Unvalidated["fb_content"];
                    long fbId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                    var tic = db.T_SupportTicket.Find(ticketId);
                    DateTime createFb = DateTime.UtcNow;
                    var fb = new T_TicketFeedback
                    {
                        CreateAt = createFb,
                        CreateByName = cMem.FullName,
                        CreateByNumber = cMem.MemberNumber,
                        Feedback = fb_content,
                        FeedbackTitle = "Note",
                        Id = fbId,
                        DateCode = createFb.ToString("yyyyMMdd"),
                        GlobalStatus = "publish",
                        TicketId = ticketId
                    };
                    //add fb
                    db.T_TicketFeedback.Add(fb);
                    db.SaveChanges();
                    tran.Commit();
                    tran.Dispose();
                    await TicketViewService.SendNoticeAfterTicketUpdate(tic, "feedback", null, cMem.MemberNumber);
                    TempData["s"] = "Add Note completed";
                }
                catch (Exception e)
                {
                    TempData["e"] = e.Message;
                }
                return Redirect("/kb/KnowledgeBaseDetail?Id=" + ticketId);
            }

        }
        /// <summary>
        /// Edit Feedback Entry
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        public async Task<JsonResult> EditFeedback(T_RemindersTicket model)
        {
            long ticketId = long.Parse(Request["fb_ticketId"]);
            _logService.Info($"[Ticket][EditFeedback] start edit feedback ticket id:${ticketId}");
            var db = new WebDataModel();
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {

                    string fb_content = Request.Unvalidated["fb_edit_content"];
                    long fbId = long.Parse(Request["feedbackId"]);
                    DateTime createFb = DateTime.UtcNow;
                    // update Ticket Feedback
                    var feedback = await db.T_TicketFeedback.FindAsync(fbId);
                    if (feedback == null)
                    {
                        throw new Exception("Action not found !");
                    }
                    // upload attachments
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(feedback.Id, "T_TicketFeedback", filesTotal);
                    var attachments = string.Join(",", UploadIds);
                    feedback.Feedback = fb_content;
                    feedback.GlobalStatus = Request["globalStatus"];
                    feedback.MentionMemberNumbers = Request["MentionMemberNumbers"];
                    if (!string.IsNullOrEmpty(feedback.Attachments))
                    {
                        feedback.Attachments = feedback.Attachments + "," + attachments;
                    }
                    else
                    {
                        feedback.Attachments = attachments;
                    }

                    feedback.UpdatedHistory += "Updated - " + AppFunc.HistoryBy(createFb, cMem.FullName) + "|";
                    feedback.UpdateBy = cMem.FullName;
                    feedback.UpdateAt = DateTime.UtcNow;
                    // update ticket
                    var tic = await db.T_SupportTicket.FindAsync(ticketId);
                    if (tic != null)
                    {
                        tic.FeedbackTicketHistory += AppFunc.HistoryBy(createFb, cMem.FullName) + "|";
                        tic.UpdateTicketHistory += AppFunc.HistoryBy(createFb, cMem.FullName) + "|";
                        tic.UpdateBy = cMem.FullName;
                        tic.UpdateAt = DateTime.UtcNow;
                        db.Entry(tic).State = System.Data.Entity.EntityState.Modified;
                    }
                    // add feedback
                    db.T_TicketFeedback.AddOrUpdate(feedback);
                    //reminder

                    //add fb
                    await db.SaveChangesAsync();

                    // await _connectorJiraIssueService.UpdateJiraComment(feedback.Id);

                    tran.Commit();
                    tran.Dispose();
                    _logService.Info($"[Ticket][EditFeedback] completed edit feedback ticket id:${ticketId}");
                    return Json(new object[] { true, "Feedback successful" });
                }
                catch (Exception e)
                {
                    _logService.Info($"[Ticket][EditFeedback] error when add feeback ticket id:${ticketId}");
                    tran.Dispose();
                    return Json(new object[] { false, e.Message });
                }
            }

        }
        #endregion

        #region Ajax/angular

        /// <summary>
        /// get ds members cung group
        /// </summary
        /// <returns></returns>
        public JsonResult GetMembersInTheSameGroup()
        {
            var db = new WebDataModel();
            List<string> listMemberNumber = TicketViewService.GetMembersInTheSameGroup(db, cMem.MemberNumber);
            var members = db.P_Member.Where(m => listMemberNumber.Contains(m.MemberNumber) && m.Active == true).ToList();
            return Json(members);
        }


        public JsonResult LoadMemberInGroup(long? deptid)
        {
            try
            {
                var db = new WebDataModel();
                if (deptid != 0 && deptid != null)
                {

                    var dept = db.P_Department.Find(deptid);
                    if (dept == null)
                    {
                        throw new Exception("Department not found");
                    }

                    if (dept.ParentDepartmentId == null)
                    {
                        string _deptid = deptid.ToString();
                        var member = db.P_Member.Where(t => t.DepartmentId.Contains(_deptid) == true).OrderBy(t => t.FullName).Select(t => new { t.MemberNumber, t.FullName }).ToList();
                        return Json(member);
                    }
                    else
                    {
                        string[] listMember = dept.GroupMemberNumber?.Split(new char[] { ',' });
                        var member = db.P_Member.Where(t => listMember.Contains(t.MemberNumber)).OrderBy(t => t.FullName).Select(t => new { t.MemberNumber, t.FullName }).ToList();
                        return Json(member);
                    }
                }
                else
                {
                    //admin sp and pos sp
                    var member = db.P_Member.Where(t => t.DepartmentId.Contains("19120002") || t.DepartmentId.Contains("19120003")).OrderBy(t => t.FullName).Select(t => new { t.MemberNumber, t.FullName }).ToList();
                    return Json(member);
                }


            }
            catch (Exception)
            {
                return Json(null);
            }
        }

        public JsonResult LoadAllMember()
        {
            try
            {
                var db = new WebDataModel();
                var member = db.P_Member.Where(t => t.Delete != true && t.Active == true).OrderBy(t => t.FullName).Select(t => new { t.MemberNumber, t.FullName }).ToList();
                return Json(member);
            }
            catch (Exception)
            {
                return Json(null);
            }
        }

        public JsonResult LoadAllCate()
        {
            try
            {
                var db = new WebDataModel();
                var cate = db.T_TicketAttribute.Where(c => c.Active == true).ToList();
                return Json(cate);
            }
            catch (Exception)
            {

                throw;
            }
        }


        /// <summary>
        /// get all merchant owner info
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult GetMerchant()
        {
            try
            {
                var db = new WebDataModel();
                var ClosedStatusOrder = InvoiceStatus.Closed.ToString();
                var PaidStatusOrder = InvoiceStatus.Paid_Wait.ToString();

                string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
                var merchant = (from c in db.C_Customer
                                let order = db.O_Orders.Where(o => c.CustomerCode == o.CustomerCode).Count() > 0
                                where c.Active != 0 && (order || c.Type == STORE_IN_HOUSE) && (c.SiteId == cMem.SiteId || cMem.SiteId == 1)
                                orderby c.Id descending
                                select c).Distinct().Select(c => new { c.Id, c.CustomerCode, c.BusinessName, c.BusinessAddressStreet, c.BusinessCity, c.BusinessState, c.BusinessZipCode, c.BusinessCountry, c.PartnerCode }).ToList();
                return Json(merchant, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult GetCustomerByCode(string customerCode)
        {
            using (var db = new WebDataModel())
            {
                var customer = db.C_Customer.Where(x => x.CustomerCode == customerCode).FirstOrDefault();
                return Json(customer);
            }

        }

        /// <summary>
        /// list product ton trong kho + product da ban cho merchant(code)
        /// </summary>
        /// <param name="code">CustomerCode</param>
        /// <returns></returns>
        public JsonResult GetProducts(string code)
        {
            try
            {
                var db = new WebDataModel();
                List<string> InvNumbers = new List<string>();
                var prod = new List<Models.CustomizeModel.ProductInfoModel>();

                if (!string.IsNullOrWhiteSpace(code))
                {
                    InvNumbers = (from op in db.Order_Products
                                  join o in db.O_Orders on op.OrderCode equals o.OrdersCode
                                  where o.CustomerCode == code
                                  select op.InvNumbers).ToList();

                    var prod_for_merchant = (from d in db.O_Device
                                             where InvNumbers.Contains(d.InvNumber)
                                             select new Models.CustomizeModel.ProductInfoModel
                                             {
                                                 Id = d.DeviceId,
                                                 SerialNo = "",
                                                 ModelCode = d.ModelCode,
                                                 InvNo = d.InvNumber,
                                                 ProductName = d.ProductName
                                             }).Distinct().ToList();
                    prod = prod_for_merchant;
                }
                return Json(prod);
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// ticket amount
        /// </summary>
        /// <returns></returns>
        public object GetTicketAmount()
        {
            var ticketList = Session["all_ticket"] as List<T_SupportTicket>;
            int all = ticketList.Count();
            int open = 0;
            int unassigned = 0;
            int closed = 0;
            int invisible = 0;
            if (all > 0)
            {
                open = ticketList.Where(t => t.Visible == true && t.DateClosed == null && ((string.IsNullOrEmpty(t.AssignedToMemberNumber) == false || string.IsNullOrEmpty(t.ReassignedToMemberNumber) == false))).Count();
                unassigned = ticketList.Where(t => t.Visible == true && string.IsNullOrEmpty(t.AssignedToMemberNumber) == true && string.IsNullOrEmpty(t.ReassignedToMemberNumber) == true && t.DateClosed == null).Count();
                closed = ticketList.Where(t => t.Visible == true && t.DateClosed != null).Count();
                invisible = ticketList.Where(t => t.Visible == null || t.Visible == false).Count();
            }
            return new object[] { all, unassigned, open, closed, invisible };
        }

        /// <summary>
        /// ticket amount of merchant
        /// <paramref name="code">customercode</paramref>
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTicketAmountByMerchant(string code)
        {
            var ticketList = new WebDataModel().T_SupportTicket.Where(t => t.CustomerCode.Equals(code)).ToList();
            int all = ticketList.Where(t => t.Visible == true).Count();
            int open = 0;
            int unassigned = 0;
            int closed = 0;
            int invisible = 0;

            if (all > 0)
            {
                open = ticketList.Where(t => t.Visible == true && t.DateClosed == null && ((string.IsNullOrEmpty(t.AssignedToMemberNumber) == false || string.IsNullOrEmpty(t.ReassignedToMemberNumber) == false))).Count();
                unassigned = ticketList.Where(t => t.Visible == true && string.IsNullOrEmpty(t.AssignedToMemberNumber) == true && string.IsNullOrEmpty(t.ReassignedToMemberNumber) == true && t.DateClosed == null).Count();
                closed = ticketList.Where(t => t.Visible == true && t.DateClosed != null).Count();
                invisible = ticketList.Where(t => t.Visible == null || t.Visible == false).Count();

            }
            return Json(new object[] { all, unassigned, open, closed, invisible });
        }

        #region timeline and feedback

        /// <summary>
        /// get timeline
        /// </summary>
        /// <param name="id">ticketid</param>
        /// <returns></returns>
        public ActionResult Gettimeline(long? id)
        {
            try
            {
                var db = new WebDataModel();
                var tic = db.T_SupportTicket.Find(id);

                var fb = db.T_TicketFeedback.Where(f => f.TicketId == id).ToList();

                if (cMem.RoleCode?.Contains("admin") != true && tic.AssignedToMemberNumber?.Contains(cMem.MemberNumber) != true && tic.CreateByNumber != cMem.MemberNumber)
                {
                    fb = fb.Where(f => f.GlobalStatus != "private" || f.CreateByNumber == cMem.MemberNumber).ToList();
                }
                var ticFeedback = (from f in fb
                                   group f by f.CreateAt.Value.UtcToIMSDateTime().Date into fbGroup
                                   orderby fbGroup.Key descending
                                   select new TicketTimelineModel
                                   {
                                       date = fbGroup.Key.ToString("yyyyMMdd"),
                                       detail = fbGroup
                                   }).ToList();

                var curentmember = Authority.GetCurrentMember();

                var cus = string.IsNullOrEmpty(tic.CustomerCode) ? null : db.C_Customer.Where(c => c.CustomerCode == tic.CustomerCode).FirstOrDefault();
                var OwnerName = cus?.LegalName;
                var CustomerName = cus?.BusinessName;
                var company = AppLB.UserContent.GetWebInfomation();
                ViewBag.EmailTemplateData = new EmailTemplateContent
                {
                    YOUR_NAME = curentmember.FullName,
                    YOUR_EMAIL = curentmember.PersonalEmail,
                    YOUR_PHONE = curentmember.CellPhone,
                    MERCHANT_BUSINESS_NAME = CustomerName,
                    MERCHANT_OWNER_NAME = OwnerName,
                    COMPANY_NAME = company.CompanyName,
                    COMPANY_ADDRESS = company.CompanyAddress,
                    COMPANY_EMAIL = company.SupportEmail
                };
                ViewBag.timeline = ticFeedback;

                ViewBag.ShowTranfer = db.T_TicketTranferMapping.Count(x => x.TicketId == tic.Id) > 0;
                return PartialView("_TicketTimelinePartial", tic);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex);
                return PartialView("_TicketTimelinePartial", new T_SupportTicket());
            }


        }


        //PUBLIC ATION/ PRIVATE ACTION/ SHARE
        /// <summary>
        /// set action feedback
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">publish/private/share</param>
        /// <returns></returns>
        public async Task<JsonResult> SetActionFeedback(long fbid, string type)
        {
            /*
             * updatedhistory - lich su update (cai gi,boi ai,luc nao) duoc save voi format nhu sau: 
             * [ACTION_NAME - AT_TIME - BY_NAME|ACTION_NAME - AT_TIME - BY_NAME|...]
             */

            try
            {
                var db = new WebDataModel();
                var fb = db.T_TicketFeedback.Find(fbid);
                int active = 1;
                string history = string.Empty;
                if (type == "share")
                {
                    if (fb.Share == true)
                    {
                        fb.Share = false;
                        active = -1;
                        history = "Unshared - " + DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName;
                    }
                    else
                    {
                        fb.Share = true;
                        history = "Shared - " + DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName;
                    }
                }
                else
                {
                    fb.GlobalStatus = type;
                    history = type.First().ToString().ToUpper() + type.Substring(1) + " - " + DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName;

                    if (type == "publish")
                    {
                        var tic = db.T_SupportTicket.Find(fb.TicketId);
                        if (!string.IsNullOrWhiteSpace(tic.CustomerCode) && tic.GlobalStatus != "private")
                        {
                            await TicketViewService.SendNoticeToCustomer(db, tic, fb);

                        }
                    }
                }

                fb.UpdatedHistory = fb.UpdatedHistory + history + "|";
                db.Entry(fb).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, fbid, type, active, history });
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Edit action feedback
        /// </summary>
        /// <param name="fbId"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public async Task<JsonResult> DeleteFeedbackFileItem(long fbId, string filePath)
        {
            var db = new WebDataModel();
            using (var trans = db.Database.BeginTransaction())
            {
                try
                {
                    var feedback = await db.T_TicketFeedback.FindAsync(fbId);
                    var fileUpload = feedback.Attachments.Split(';');
                    feedback.Attachments = String.Join(";", fileUpload.Where(path => path.Contains(filePath) == false));
                    db.Entry(feedback).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                    trans.Commit();
                    string sPath = Path.Combine(Server.MapPath(filePath));
                    FileInfo f = new FileInfo(sPath);
                    if (f.Exists)
                    {
                        f.Delete();
                    }
                }
                catch (Exception)
                {
                    trans.Rollback();
                    return Json(false);
                }
                finally
                {
                    trans.Dispose();
                }
            }
            return Json(true);
        }

        /// <summary>
        /// Get action feedback
        /// </summary>
        /// <param name="ticketId"></param>
        /// <param name="fbId"></param>
        /// <param name="controller"></param>
        /// <returns></returns>
        public async Task<PartialViewResult> GetActionFeedback(long ticketId, long fbId, string controller)
        {
            var db = new WebDataModel();
            var currentMember = Authority.GetCurrentMember();
            var tic = db.T_SupportTicket.Find(ticketId);
            var cus = string.IsNullOrEmpty(tic?.CustomerCode) ? null : db.C_Customer.FirstOrDefault(c => c.CustomerCode == tic.CustomerCode);
            var OwnerName = cus?.LegalName;
            var CustomerName = cus?.BusinessName;
            var company = UserContent.GetWebInfomation();
            ViewBag.EmailTemplateData = new EmailTemplateContent
            {
                YOUR_NAME = currentMember.FullName,
                YOUR_EMAIL = currentMember.PersonalEmail,
                YOUR_PHONE = currentMember.CellPhone,
                MERCHANT_BUSINESS_NAME = OwnerName,
                MERCHANT_OWNER_NAME = CustomerName,
                COMPANY_NAME = company.CompanyName,
                COMPANY_ADDRESS = company.CompanyAddress,
                COMPANY_EMAIL = company.SupportEmail
            };
            var fb = await db.T_TicketFeedback.FindAsync(fbId);
            ViewBag.attachments = db.UploadMoreFiles.Where(f => f.TableId == fb.Id && f.TableName == "T_TicketFeedback").ToList();
            ViewData["fb_edit_controller"] = controller;
            var Reminder = new T_RemindersTicket();
            var reminderTicket = db.T_RemindersTicket.Where(x => x.TicketId == ticketId).FirstOrDefault();
            if (reminderTicket != null)
            {
                Reminder = reminderTicket;
            }
            Reminder.TicketId = ticketId;
            ViewBag.Reminders = Reminder;
            return PartialView("_TicketTimelineEditPartial", fb);

        }

        /// <summary>
        /// set action ticket
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">publish/private</param>
        /// <returns></returns>
        public async Task<JsonResult> SetActionTicket(long id, string type)
        {
            var db = new WebDataModel();
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var tic = db.T_SupportTicket.Find(id);
                    tic.GlobalStatus = type;
                    db.Entry(tic).State = System.Data.Entity.EntityState.Modified;

                    TicketViewService.InsertFeedback(db, id, "Tickets have been changed to " + type, "", "");

                    if (type == "publish")
                    {
                        if (!string.IsNullOrWhiteSpace(tic.CustomerCode))
                        {
                            var fb = db.T_TicketFeedback.Where(f => f.TicketId == tic.Id && f.GlobalStatus != "private").OrderByDescending(f => f.CreateAt).FirstOrDefault();
                            if (fb != null)
                            {
                                await TicketViewService.SendNoticeToCustomer(db, tic, fb);

                            }
                        }
                    }

                    db.SaveChanges();
                    tran.Commit();
                    tran.Dispose();
                    return Json(new object[] { true, id, type });
                }
                catch (Exception)
                {
                    tran.Dispose();
                    throw;
                }
            }


        }

        #endregion

        /// <summary>
        /// get task by ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetTaskStatusByTicket(long id)
        {
            try
            {
                var db = new WebDataModel();
                // var ticket_stage = db.T_TicketStage_Status.Where(x => x.TicketId == id).ToList();
                var task = db.Ts_Task.Where(t => t.TicketId == id).ToList();
                bool can_update = true;
                //// admin
                //if (cMem.RoleCode?.Contains("admin") == true)
                //{
                //    can_update = true;
                //}
                //// assing member
                //if (can_update != true)
                //{
                //    foreach (var tic in ticket_stage)
                //    {
                //        if (!string.IsNullOrEmpty(tic.AssignedMember_Numbers))
                //        {
                //            if (tic.AssignedMember_Numbers.Split('|').Contains(cMem.MemberNumber))
                //            {
                //                can_update = true;
                //            }
                //        }
                //    }
                //}
                //if (can_update != true)
                //{
                //    var checkLeader = db.T_Project_Stage_Members.AsEnumerable().Where(x => x.MemberNumber == cMem.MemberNumber && ticket_stage.Any(y => y.StageId == x.StageId) && x.IsLeader == true).Count() > 0;
                //    if (checkLeader == true)
                //    {
                //        can_update = true;
                //    }
                //}
                ViewBag.can_update = can_update;
                ViewBag.ListReminder = db.T_RemindersTicket.Where(x => x.Active == true && x.TicketId == id).ToList();
                return PartialView("_TaskStatusPartial", task);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// get thong tin status, thanh vien trong group, group cung department sau khi update ticket
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult Get_Reassign_Group_Status(long id)
        {
            try
            {
                var db = new WebDataModel();
                var tic = db.T_SupportTicket.Find(id);
                var group = new List<P_Department>();
                var memberInGroup = string.Empty;
                //get ds group khac cung department ma nhan vien nay thuoc ve.
                if (tic.GroupID > 0)
                {
                    var g = db.P_Department.Where(d => d.Id == tic.GroupID).FirstOrDefault();
                    var deparmentId = g?.ParentDepartmentId;
                    group = db.P_Department.Where(d => d.ParentDepartmentId == deparmentId && deparmentId > 0 && d.Id != tic.GroupID && d.Active == true).ToList();

                    //get nhung thanh vien cung group
                    if (!string.IsNullOrWhiteSpace(g.GroupMemberNumber))
                    {
                        memberInGroup = g.GroupMemberNumber + "|" + g.GroupMemberName;
                    }
                    else
                    {
                        string _ticGroupId = tic.GroupID.ToString();
                        var listMemInDept = db.P_Member.Where(m => m.Active == true && m.DepartmentId.Contains(_ticGroupId) == true).ToList();
                        var listmem = string.Join(",", listMemInDept.Select(m => m.MemberNumber)) + "|" + string.Join(",", listMemInDept.Select(m => m.FullName));
                        memberInGroup = listmem;

                    }

                }
                else
                {
                    var listGroupId = TicketViewService.GetGroupByMember(db, cMem.MemberNumber);
                    group = db.P_Department.Where(d => listGroupId.Contains(d.Id)).ToList();
                }

                //group Dev Department
                var DepartmentDev = db.P_Department.Where(d => d.Type == "DEVELOPMENT").ToList();
                var groupDevDepartment = DepartmentDev.Any(d => d.ParentDepartmentId > 0) ? DepartmentDev.Where(x => x.ParentDepartmentId > 0).ToList() : DepartmentDev;
                return Json(new object[] { tic.StatusName, memberInGroup, group, tic.ReassignedToMemberNumber, "", groupDevDepartment });

            }
            catch (Exception)
            {
                throw;
            }
        }

        public JsonResult NewAttribute(string attr_name, string attr_opts, List<string> apply_for_ticket_types)
        {

            try
            {
                if (!access.Any(k => k.Key.Equals("ticket_view")) == true || !access["ticket_view"] == true)
                {
                    return Json(new object[] { false, "Permission denied!" });
                }
                if (string.IsNullOrWhiteSpace(attr_name))
                {
                    return Json(new object[] { false, "Attribute name is required" });
                }

                var db = new WebDataModel();
                var attribute = new T_TicketAttribute
                {
                    Active = true,
                    Name = attr_name,
                    CreateAt = DateTime.UtcNow,
                    CreateBy = cMem.FullName,
                    Id = Guid.NewGuid().ToString("N"),
                    apply_for_ticket_type = string.Join("|", apply_for_ticket_types),
                };
                var list_options = new List<T_TicketAttribute>();
                var list_opts_name = attr_opts.Split('\n');
                foreach (var op in list_opts_name)
                {
                    if (!string.IsNullOrWhiteSpace(op))
                    {
                        list_options.Add(new T_TicketAttribute
                        {
                            Active = true,
                            Name = op.Trim(),
                            CreateAt = DateTime.UtcNow,
                            CreateBy = cMem.FullName,
                            Id = Guid.NewGuid().ToString("N"),
                            ParentId = attribute.Id,
                            ParentName = attribute.Name,
                        });
                    }
                }
                db.T_TicketAttribute.Add(attribute);
                db.T_TicketAttribute.AddRange(list_options);
                db.SaveChanges();
                return Json(new object[] { true, "Create new Attribute is completed" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Create new Attribute fail", e.Message });
            }
        }


        #endregion

        #region Tags
        //Load list tags
        public ActionResult LoadListTags(string type = "html")
        {
            WebDataModel db = new WebDataModel();
            var listTags = db.T_Tags.OrderByDescending(t => t.Name).ToList(); //Where(t => t.Type == "support_ticket")
            if (type == "html")
            {
                return PartialView("_DropdownListTagsPartial", listTags);
            }
            else
            {
                return Content(JsonConvert.SerializeObject(listTags));
            }
        }

        //select tags
        [AllowAnonymous]
        public ActionResult SelectTags(string lstTags, string Page, string Action)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                if (Action == "Add")
                {
                    ViewBag.AddNew = true;
                }
                else
                {
                    ViewBag.AddNew = false;
                }

                if (Page == PageDeployment)
                {
                    ViewBag.ListTags = db.T_Tags.Where(x => x.Type == PageDeployment && x.SiteId == cMem.SiteId).ToList();
                }
                else
                {
                    ViewBag.ListTags = db.T_Tags.Where(x => x.Type != PageDeployment && x.SiteId == cMem.SiteId).ToList();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return PartialView("_TagsPopupPartial", lstTags);
        }

        //get tag info
        public JsonResult GetTagsInfo(string tagId)
        {
            try
            {
                var result = TicketViewService.GetTagsInfo(tagId, out string ErrMsg);
                if (result == null)
                {
                    throw new Exception(ErrMsg);
                }
                return Json(new object[] { true, result });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        //Save tags
        public JsonResult SaveTags(string id, string name, string color, string Page)
        {
            try
            {
                string Type = Page;
                //if (Page == PageDeployment)
                //{
                //    Type = "19120001";
                //}
                var result = TicketViewService.SaveTags(id, name, color, cMem.FullName, out string ErrMsg, Type);
                if (result == null)
                {
                    throw new Exception(ErrMsg);
                }

                return Json(new object[] { true, "Save success", result });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        //delete tag
        public JsonResult DeleteTags(string tagId)
        {
            try
            {
                var result = TicketViewService.DeleteTags(tagId);
                if (result != "")
                {
                    throw new Exception(result);
                }

                return Json(new object[] { true, "Delete success" });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Delete failure! " + ex.Message });
            }
        }
        #endregion

        #region Merge tickets
        public ActionResult GetMergeTicket_S1(long ticket_id)
        {
            try
            {

                var db = new WebDataModel();
                var tk = db.T_SupportTicket.Find(ticket_id);
                if (tk == null)
                {
                    throw new Exception("ticket not found");
                }
                var list_tks = db.T_SupportTicket.Where(t => !string.IsNullOrEmpty(t.CustomerCode) && t.CustomerCode == tk.CustomerCode && tk.CustomerCode != null &&
                   t.ProjectId == tk.ProjectId && t.DateClosed == null).ToList();
                ViewBag.cur_tk = ticket_id;
                return PartialView("merge_tk_partials/_Partial_MergeTicket_S1", list_tks);
            }
            catch (Exception e)
            {
                TempData["e"] = "Error: " + e.Message;
                return PartialView("merge_tk_partials/_Partial_MergeTicket_S1");
            }
        }
        public ActionResult GetMergeTicket_S2(long ticket_id, List<long> select_tickets)
        {
            try
            {
                var db = new WebDataModel();
                var list_tks = db.T_SupportTicket.Where(t =>
                select_tickets.Contains(t.Id)).ToList() ?? new List<T_SupportTicket>();
                ViewBag.cur_tk = ticket_id;
                return PartialView("merge_tk_partials/_Partial_MergeTicket_S2", list_tks);
            }
            catch (Exception e)
            {
                TempData["e"] = "Error: " + e.Message;
                return PartialView("merge_tk_partials/_Partial_MergeTicket_S2");
            }
        }
        public JsonResult MergeTicket_submit(long main_ticket, List<long> select_tickets, long? cur_ticket, string controller)
        {
            try
            {
                var db = new WebDataModel();
                select_tickets.Remove(main_ticket);
                var fb_id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                List<T_TicketFeedback> fbs = new List<T_TicketFeedback>();
                foreach (var id in select_tickets)
                {
                    var tk = db.T_SupportTicket.Find(id);
                    var fb = new T_TicketFeedback
                    {
                        CreateAt = DateTime.UtcNow,
                        CreateByName = "System",
                        Feedback = "This ticket has been merge to <a href='/" + controller + "/detail/" + main_ticket + "?urlback=/" + controller + "/detail/" + tk.Id + "'>Ticket #" + CommonFunc.view_TicketId(main_ticket) + "</a><br/> Merge at " + DateTime.Now.ToString("MMM dd, yyyy hh:mm tt") + "<br/>by " + cMem.FullName,
                        Id = fb_id++,
                        DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                        GlobalStatus = "privated",
                        TicketId = tk.Id,
                    };
                    fbs.Add(fb);
                    //if (tk.TypeId == (long)UserContent.TICKET_TYPE.Sales)
                    //{
                    //    tk.StatusId = (long)UserContent.TICKET_STATUS.Sales_Closed;
                    //    tk.StatusName = "Closed";
                    //}
                    //if (tk.TypeId == (long)UserContent.TICKET_TYPE.Finance)
                    //{
                    //    tk.StatusId = (long)UserContent.TICKET_STATUS.Finance_Complete;
                    //    tk.StatusName = "Complete";
                    //}
                    var closeStatus = db.T_TicketStatus.Where(x => x.ProjectId == tk.ProjectId && x.Name.ToLower().Contains("close")).FirstOrDefault();
                    //if (tk.TypeId == (long)UserContent.TICKET_TYPE.NuveiOnboarding)
                    //{
                    //    tk.StatusId = (long)UserContent.DeploymentTicket_Status.Close;
                    //    tk.StatusName = "Close";
                    //}
                    //if (tk.TypeId == (long)UserContent.TICKET_TYPE.Deployment)
                    //{
                    //    tk.StatusId = (long)UserContent.DeploymentTicket_Status.Close;
                    //    tk.StatusName = "Close";
                    //}
                    db.T_TicketStatusMapping.RemoveRange(tk.T_TicketStatusMapping);
                    //  tic.T_TicketStatusMapping.Clear();


                    if (closeStatus != null)
                    {
                        tk.T_TicketStatusMapping.Add(new T_TicketStatusMapping
                        {
                            StatusId = closeStatus.Id,
                            StatusName = closeStatus.Name,
                            TicketId = tk.Id,
                        });
                    }

                    tk.CloseByMemberNumber = cMem.MemberNumber;
                    tk.CloseByName = cMem.FullName;
                    tk.DateClosed = DateTime.UtcNow;

                    var mfb = new T_TicketFeedback
                    {
                        CreateAt = DateTime.UtcNow,
                        CreateByName = "System",
                        FeedbackTitle = "<a href=\"/" + controller + "/detail/" + tk.Id + "\">Ticket #" + CommonFunc.view_TicketId(tk.Id) + "</a> has been joined to this ticket",
                        Feedback = tk.Description + " <br/>Join at " + DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm tt") + "<br/>by " + cMem.FullName,
                        Id = fb_id++,
                        DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                        GlobalStatus = "privated",
                        TicketId = main_ticket,
                    };
                    fbs.Add(mfb);
                    db.Entry(tk).State = System.Data.Entity.EntityState.Modified;

                }
                db.T_TicketFeedback.AddRange(fbs);
                db.SaveChanges();
                var fb_item = CommonFunc.RenderRazorViewToString("merge_tk_partials/_Partial_FB_item", fbs.Where(t => t.TicketId == cur_ticket).AsEnumerable(), this);
                return Json(new object[] { true, "Merge ticket completed", fb_item, select_tickets, select_tickets.Contains(cur_ticket ?? -1) });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Fail: " + e.Message });
            }

        }

        [HttpPost]
        public ActionResult NotificationMentions(List<string> MemberNumbers, long TicketId)
        {
            MemberNumbers = MemberNumbers.Where(x => x != cMem.MemberNumber).ToList();
            if (MemberNumbers.Count > 0)
            {
                var NotificationService = new NotificationService();
                NotificationService.TicketMentionsNotification(MemberNumbers, TicketId.ToString(), cMem.FullName, cMem.MemberNumber);
            }
            return Json(true);
        }
        [HttpGet]
        public ActionResult GetLastUpdateDetail(long? ticketId)
        {
            var db = new WebDataModel();
            var last = db.T_TicketUpdateLog.Where(x => x.TicketId == ticketId).OrderByDescending(x => x.UpdateId).AsEnumerable().GroupBy(x => x.UpdateId).Take(3).ToList();
            ViewBag.TicketId = ticketId;
            return PartialView("_TicketLastUpdateDetail", last);

        }
        #endregion

        #region transfer
        public ActionResult GetContentTransfer(long TicketId)
        {
            var db = new WebDataModel();
            var listTranferTo = (from m in db.T_TicketTranferMapping
                                 where m.TicketId == TicketId
                                 join fromProject in db.T_Project_Milestone on m.FromProjectId equals fromProject.Id
                                 join toProject in db.T_Project_Milestone on m.ToProjectId equals toProject.Id
                                 join tranferby in db.P_Member on m.CreateByMemberNumner equals tranferby.MemberNumber
                                 select new TranferHistoryModel
                                 {
                                     FromProjectId = fromProject.Id,
                                     FromProjectName = fromProject.Name,
                                     ToProjectId = toProject.Id,
                                     ToProjectName = toProject.Name,
                                     TransferAt = m.CreateAt,
                                     Note = m.Note,
                                     //BuildInCode = p.BuildInCode,
                                     TranferBy = tranferby,
                                 }).ToList();


            return PartialView("_TranferList", listTranferTo);
        }
        [HttpPost]
        public async Task<ActionResult> TransferSubmit(TranferModel model)
        {
            try
            {
                var ticket = new T_SupportTicket();
                var db = new WebDataModel();
                ticket = db.T_SupportTicket.Include(x => x.T_TicketStatusMapping).Include(x => x.T_TicketTypeMapping).FirstOrDefault(x => x.Id == model.TicketId);
                if (ticket == null)
                {
                    return Json(new { status = false, message = "Ticket not found" });
                }
                var oldProject = db.T_Project_Milestone.Find(ticket.ProjectId);
                var project = db.T_Project_Milestone.Find(model.ProjectId);
                var project_version = db.T_Project_Milestone.Find(model.VersionId);

                int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year && t.CreateAt.Value.Month == DateTime.Today.Month).Count();

                if (model.Department != null)
                {
                    ticket.GroupID = model.Department;
                    ticket.GroupName = db.P_Department.Find(model.Department)?.Name;
                }

                ticket.ProjectId = project?.Id;
                ticket.ProjectName = project?.Name;
                ticket.VersionId = model.VersionId;
                ticket.DateOpened = DateTime.UtcNow;

                //var old_stage_ticket = db.T_TicketStage_Status.Where(x => x.TicketId == ticket.Id).ToList();
                //db.T_TicketStage_Status.RemoveRange(old_stage_ticket);
                // var forward_stages = model.Stage;
                if (!string.IsNullOrEmpty(model.Assign))
                {
                    ticket.AssignedToMemberNumber = model.Assign;
                    ticket.AssignedToMemberName = string.Join(",", db.P_Member.Where(x => ticket.AssignedToMemberNumber.Contains(x.MemberNumber)).Select(x => x.FullName).ToList());
                }
                else
                {
                    ticket.AssignedToMemberName = null;
                    ticket.AssignedToMemberNumber = null;
                }


                var listStages = model.Stage.Split(',').ToList();
                ticket.StageId = model.Stage;
                ticket.StageName = string.Join(",", db.T_Project_Stage.Where(x => listStages.Any(y => y == x.Id)).Select(x => x.Name).ToList());

                // assigned
                if (!string.IsNullOrEmpty(model.Assign))
                {
                    var ListAssign = model.Assign.Split(',');
                    var ListAssignMemberNumber = new List<string>();

                    foreach (var memberNumber in ListAssign)
                    {
                        if (memberNumber == "auto")
                        {
                            if (!string.IsNullOrEmpty(project.LeaderNumber))
                            {
                                ListAssignMemberNumber.Add(project.LeaderNumber);
                            }
                            if (!string.IsNullOrEmpty(project.ManagerNumber))
                            {
                                ListAssignMemberNumber.Add(project.ManagerNumber);
                            }
                            else
                            {
                                if (model.Department > 0)
                                {
                                    var dep = db.P_Department.Where(x => x.Id == ticket.GroupID).FirstOrDefault();
                                    if (dep != null)
                                    {
                                        if (!string.IsNullOrEmpty(dep.LeaderNumber))
                                        {
                                            ListAssignMemberNumber.AddRange(dep.LeaderNumber.Split(','));
                                        }
                                        else if (!string.IsNullOrEmpty(dep.SupervisorNumber))
                                        {
                                            ListAssignMemberNumber.Add(dep.SupervisorNumber);
                                        }
                                        else if (dep.ParentDepartmentId != null)
                                        {
                                            var parentdep = db.P_Department.Where(x => x.Id == dep.ParentDepartmentId).FirstOrDefault();
                                            if (parentdep != null && !string.IsNullOrEmpty(parentdep.LeaderNumber))
                                            {
                                                ListAssignMemberNumber.AddRange(parentdep.LeaderNumber.Split(','));
                                            }
                                        }
                                    }
                                }
                            }

                        }
                        else
                        {
                            var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == memberNumber);
                            if (member != null)
                            {
                                ListAssignMemberNumber.Add(member.MemberNumber);

                            }

                        }

                    }
                    ListAssignMemberNumber = ListAssignMemberNumber.GroupBy(x => x).Select(x => x.FirstOrDefault()).ToList();
                    ticket.AssignedToMemberNumber = string.Join(",", ListAssignMemberNumber);
                    ticket.AssignedToMemberName = string.Join(",", db.P_Member.Where(x => ListAssignMemberNumber.Any(y => y == x.MemberNumber)).Select(x => x.FullName).ToList());
                }
                else
                {
                    ticket.AssignedToMemberName = null;
                    ticket.AssignedToMemberNumber = null;
                }

                var mappingTicket = new T_TicketTranferMapping();

                mappingTicket.FromProjectId = oldProject.Id;
                mappingTicket.ToProjectId = model.ProjectId;
                mappingTicket.CreateAt = DateTime.UtcNow;
                mappingTicket.Note = model.transfer_note;
                mappingTicket.CreateByName = cMem.FullName;
                mappingTicket.TicketId = ticket.Id;
                mappingTicket.CreateByMemberNumner = cMem.MemberNumber;
                db.T_TicketTranferMapping.Add(mappingTicket);
                var listFeedbackOldTicket = db.T_TicketFeedback.Where(x => x.TicketId == ticket.Id).ToList();
                var feedBackId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));

                var fromNote = "";
                fromNote += model.transfer_note;
                fromNote += "<br/><div class='tranfer-signature'>Transfer From: " + oldProject.Name + "</div>";
                feedBackId = feedBackId + 1;
                var fbTranferTicket = new T_TicketFeedback
                {
                    CreateAt = DateTime.UtcNow,
                    CreateByName = cMem.FullName,
                    CreateByNumber = cMem.MemberNumber,
                    Feedback = fromNote,
                    Id = feedBackId,
                    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                    TicketId = ticket.Id
                };

                db.T_TicketFeedback.Add(fbTranferTicket);
                ticket.UpdateAt = DateTime.UtcNow;
                ticket.UpdateBy = cMem.FullName;

                //}

                db.T_TicketStatusMapping.RemoveRange(ticket.T_TicketStatusMapping);
                db.T_TicketTypeMapping.RemoveRange(ticket.T_TicketTypeMapping);
                db.SaveChanges();
                await this.SendEmailUpdate(ticket, "new", cMem.MemberNumber);
                return Json(new { status = true, message = "Transfer ticket to " + project.Name + " success" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][Transfer] error when transfer ticket {model.TicketId} to project {model.ProjectId}");
                return Json(new { status = false, message = "transfer failed" });
            }

        }
        public ActionResult GetProjectTranfer()
        {
            var db = new WebDataModel();
            var project = from p in db.T_Project_Milestone
                          where p.Active == true && p.Type == "project"
                          select p;
            //db.T_Project_Milestone.Where(x => x.Active == true && x.Type == "project").ToList();
            return Json(project.Select(x => new { x.Id, Name = x.Name, x.BuildInCode }).ToList());
        }
        public ActionResult GetVersionTranfer(string projectId)
        {
            var db = new WebDataModel();
            var ver = db.T_Project_Milestone.Where(x => x.Active == true && x.Type == "Project_version" && x.ParentId == projectId).ToList();
            return Json(ver);
        }
        public ActionResult GetStageTranfer(string versionId)
        {
            if (string.IsNullOrEmpty(versionId))
            {
                return Json(new List<T_Project_Stage>());
            }
            var db = new WebDataModel();
            var projectVersion = db.T_Project_Milestone.Find(versionId);

            if (projectVersion == null)
            {
                return Json(new List<T_Project_Stage>());
            }
            var project = db.T_Project_Milestone.Where(x => x.Id == projectVersion.ParentId).FirstOrDefault();
            var stage = new List<T_Project_Stage>();
            if (project.BuildInCode == TypeDevelopment)
            {
                stage = db.T_Project_Stage.Where(x => x.ProjectId == projectVersion.ParentId).ToList();
            }
            else
            {
                stage = db.T_Project_Stage.Where(x => x.ProjectId == projectVersion.ParentId && x.BuildInCode == "default").ToList();
            }
            return Json(stage);
        }
        public ActionResult GetMemberStageTranfer(string stageId)
        {
            var db = new WebDataModel();
            var stage = db.T_Project_Stage.Where(x => x.Id == stageId).FirstOrDefault();
            var member = db.T_Project_Stage_Members.Where(x => x.StageId == stageId).Select(m => new { m.MemberNumber, m.MemberName }).ToList();
            var memberSupport = db.P_Member.Where(t => t.DepartmentId.Contains("19120002") || t.DepartmentId.Contains("19120003")).OrderBy(t => t.FullName).Select(t => new { t.Gender, t.MemberNumber, t.FullName, t.Picture }).ToList();
            member.AddRange(memberSupport.Where(x => !member.Any(m => m.MemberNumber == x.MemberNumber)).Select(x => new { MemberNumber = x.MemberNumber, MemberName = x.FullName }));
            return Json(member);
        }

        #endregion

        #region exportExcel
        public async Task<JsonResult> ExportExcel(DeploymentTicket_request request)
        {


            try
            {
                int statusMerchant = LeadStatus.Merchant.Code<int>();
                var db = new WebDataModel();
                string TabName = "";
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });
                // get ticket list
                IQueryable<T_SupportTicket> ticketList;
                var stg_ver = GetCurrentStage(db, request.Page);
                if (request.Page == PageDeployment)
                {
                    if (request.StageId != "all")
                    {
                        stg_ver = stg_ver.Where(x => x.stage.Id == request.StageId);
                    }
                    TabName = "Delivery";
                }

                else if (request.Page == PageSupport)
                {
                    if (request.StageId != "all")
                    {
                        stg_ver = stg_ver.Where(x => x.stage.Id == request.StageId);
                    }
                    TabName = "Support";
                }
                else
                {
                    TabName = "Development";
                }

                ticketList = from tic in db.T_SupportTicket where tic.SiteId == cMem.SiteId || (tic.SiteId == null && cMem.SiteId == 1) select tic;

                //filter by project
                if (request.Page == PageDevelopmentsTicket)
                {
                    request.ProjectId = Request.Cookies["ProjectId"].Value;
                    if (request.ProjectId != "all")
                    {


                        ticketList = ticketList.Where(x => x.ProjectId == request.ProjectId);

                        if ((request.StageId ?? "all") != "all")
                        {
                            ticketList = ticketList.Where(x => x.StageId.Contains(request.StageId));
                        }

                        if ((request.VersionId ?? "all") != "all")
                        {
                            ticketList = ticketList.Where(x => x.VersionId.Contains(request.VersionId));
                        }

                    }
                    else
                    {
                        var viewAll = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
                        IQueryable<T_Project_Milestone> listProjectAvailabel;
                        if (viewAll == true)
                        {
                            listProjectAvailabel = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDevelopment && x.Type == "project" && x.Active == true);
                        }
                        else
                        {
                            listProjectAvailabel = from project in db.T_Project_Milestone
                                                   join memberProject in db.T_Project_Milestone_Member on project.Id equals memberProject.ProjectId
                                                   where project.Active == true && project.Type == "project" && memberProject.MemberNumber == cMem.MemberNumber && project.BuildInCode == TypeDevelopment
                                                   group project by project.Id into project
                                                   select project.FirstOrDefault();
                        }

                        ticketList = ticketList.Where(x => listProjectAvailabel.Any(y => y.Id == x.ProjectId));
                    }
                }
                //filter by project deployment
                else if (request.Page == PageDeployment)
                {
                    var projectDeployment = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDeployment).FirstOrDefault();
                    ticketList = ticketList.Where(x => x.ProjectId == projectDeployment.Id);
                }
                //filter by project support
                else
                {
                    var projectSupport = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeSupport).FirstOrDefault();
                    ticketList = ticketList.Where(x => x.ProjectId == projectSupport.Id);
                }

                //check access permission 
                if (!(cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true)))
                {

                    var checkViewGroup = access.Any(k => k.Key.Equals("ticket_view")) == true && access["ticket_view"] == true;
                    if (request.Page == PageDeployment)
                    {
                        var IsDirectorDelivery = db.P_Department.Any(x => x.LeaderNumber.Contains(cMem.MemberNumber) && x.Type == "DEPLOYMENT" && x.ParentDepartmentId == null);
                        var DeploymentGroupMember = db.P_Department.Where(x => x.Type == "DEPLOYMENT" && x.GroupMemberNumber.Contains(cMem.MemberNumber) && x.ParentDepartmentId != null);

                        // var all = db.P_Department.Any(x => x.LeaderNumber.Contains(cMem.MemberNumber) && x.Type == "DEPLOYMENT" && x.ParentDepartmentId == null);
                        ticketList = ticketList.Where(tk => IsDirectorDelivery || DeploymentGroupMember.Any(x => (x.LeaderNumber.Contains(cMem.MemberNumber) && tk.GroupID == x.Id) || x.GroupMemberNumber.Contains(cMem.MemberNumber) && x.Id == tk.GroupID && checkViewGroup) || tk.AssignedToMemberNumber.Contains(cMem.MemberNumber) || tk.CreateByNumber == cMem.MemberNumber || tk.TagMemberNumber.Contains(cMem.MemberNumber));
                    }
                    else if (request.Page == PageSupport)
                    {
                        var SupportDepartmentIdLeader = from d in db.P_Department
                                                        where d.LeaderNumber.Contains(cMem.MemberNumber) && (d.Type == "SUPPORT")
                                                        select new
                                                        {
                                                            Id = d.Id,
                                                            childGroup = from c in db.P_Department
                                                                         where c.ParentDepartmentId == d.Id
                                                                         select new
                                                                         {
                                                                             Id = c.Id,
                                                                         }
                                                        };
                        var DeploymentGroupMember = db.P_Department.Where(x => x.GroupMemberNumber.Contains(cMem.MemberNumber) && x.Type == "SUPPORT" && x.ParentDepartmentId != null);
                        ticketList = ticketList.Where(tk =>
                        tk.AssignedToMemberNumber.Contains(cMem.MemberNumber)
                       || tk.CreateByNumber == cMem.MemberNumber
                       || DeploymentGroupMember.Any(x => tk.GroupID == x.Id && checkViewGroup)
                       //|| GetListStageAndVersionLeader.Any(y => y.StageId == tk.Stage.StageId && y.ProjectVersionId == tk.Stage.ProjectVersionId)
                       || SupportDepartmentIdLeader.Any(x => x.Id == tk.GroupID || x.childGroup.Any(y => y.Id == tk.GroupID))
                       || tk.TagMemberNumber.Contains(cMem.MemberNumber)
                       );
                    }

                }
                #region filter


                ticketList = this.TicketFilterFactory(db, request, ticketList);

                #endregion

                var dataresult = await ticketList.OrderByDescending(x => x.CreateAt).ToListWithNoLockAsync();
                var members = db.P_Member.ToList();
                var Tickets = dataresult.Select(tic =>
                {
                    var item = new TicketListView();
                    item.Id = tic.Id;
                    item.Name = tic.Name;
                    List<string> Avartar = new List<string>();
                    if (!string.IsNullOrEmpty(tic.AssignedToMemberNumber))
                    {
                        var ListMemberNumberAssign = tic.AssignedToMemberNumber.Split('|');
                        ListMemberNumberAssign.ForEach(c =>
                        {
                            Avartar.Add(members.Where(m => c.Contains(m.MemberNumber))
                                                            .Select(m => string.IsNullOrEmpty(m.Picture) ? "/Upload/Img/" + m.Gender + ".png" : m.Picture)
                                                            .FirstOrDefault());
                        });
                    }
                    item.StageName = tic.StageName;
                    item.AssignMemberNumbers = tic.AssignedToMemberNumber;
                    item.AssignMemberNames = tic.AssignedToMemberName;
                    item.AssignMemberAvatars = string.Join("|", Avartar);
                    item.OpenByName = tic.OpenByName ?? tic.CreateByName ?? "System";
                    if (!string.IsNullOrEmpty(tic.CustomerCode))
                    {
                        var customer = db.C_Customer.Where(x => x.CustomerCode == tic.CustomerCode).FirstOrDefault();
                        if (customer != null)
                        {
                            item.CustomerId = customer?.Id;
                            item.AccountManager = customer?.FullName;
                            item.SalonPhone = customer?.SalonPhone;
                            item.OwnerPhone = customer?.OwnerMobile;
                            var license = db.Store_Services.Where(x => x.CustomerCode == customer.CustomerCode && x.Active == 1 && x.Type == "license").FirstOrDefault();
                            item.LicenseName = license?.Productname;
                            item.LicenseExpiredDate = license != null ? string.Format("{0:r}", license.RenewDate) : "";
                        }
                    }
                    item.ProjectName = tic.ProjectName;
                    item.VersionName = tic.VersionName;
                    item.CustomerCode = tic.CustomerCode;
                    item.CustomerName = tic.CustomerName;
                    if (tic.PriorityId != null)
                    {
                        var priority = db.T_Priority.Where(x => x.Id == tic.PriorityId).FirstOrDefault();
                        if (priority != null)
                        {
                            item.Priority.Name = priority.Name;
                            item.Priority.Color = priority.Color;
                            item.Priority.Id = priority.Id;
                        }
                    }
                    //item.PriorityName = tic.SupportTicket.PriorityName ?? string.Empty;
                    item.SeverityId = tic.SeverityId;
                    item.SeverityName = tic.SeverityName ?? string.Empty;
                    if (!string.IsNullOrEmpty(tic.Tags))
                    {
                        var ListLabel = new List<string>();
                        var labels = tic.Tags.Split(',');
                        foreach (var label in labels)
                        {
                            var lb = db.T_Tags.Where(x => x.Id == label).FirstOrDefault();
                            if (lb != null)
                            {
                                ListLabel.Add(lb.Name);
                            }
                        }
                        item.Tags = string.Join(",", ListLabel);
                    }


                    var types = db.T_TicketTypeMapping.Where(x => x.TicketId == tic.Id);
                    if (types.Count() > 0)
                    {
                        item.TypeName = string.Join(",", types.Select(x => x.TypeName).ToList());
                    }
                    //var status = db.T_TicketStatusMapping.Where(x => x.TicketId == item.Id).Include(x => x.T_TicketStatus).ToList();
                    item.Status = tic.StatusName;
                    item.MemberTag = tic.TagMemberName ?? "";
                    //item.StatusType = status.Count > 0 ? (status.Any(x => x.T_TicketStatus.Type == "closed") ? "closed" : "open") : "open";
                    item.GlobalStatus = tic.GlobalStatus;
                    item.CloseDate = tic.DateClosed != null ? string.Format("{0:r}", (tic.DateClosed.Value)) : null;
                    item.CloseByName = tic.CloseByName ?? "System";
                    item.CreateAt = tic.CreateAt;
                    item.OpenDateByDate = tic.CreateAt ?? tic.DateOpened;
                    item.closed_date = tic.DateClosed;
                    item.Updated = tic.UpdateTicketHistory;
                    // item.Deadline = tic.Deadline != null ? string.Format("{0:r}", tic.Deadline) : null;
                    item.DepartmentName = tic.GroupName;
                    //item.CreateByNumber = tic.SupportTicket.CreateByNumber;
                    item.Note = tic.Note;
                    if (tic.UpdateAt != null)
                    {
                        var updateDetail = db.T_TicketUpdateLog.Where(x => x.TicketId == tic.Id).AsEnumerable().GroupBy(x => x.UpdateId).LastOrDefault();
                        if (updateDetail != null)
                        {
                            List<string> descriptionUpdate = new List<string>();
                            foreach (var u in updateDetail)
                            {

                                if (u.Name == "EstimatedCompletionTimeTo" || u.Name == "EstimatedCompletionTimeFrom" || u.Name == "Deadline")
                                {
                                    descriptionUpdate.Add(u.Name + ": <span class='time-datatable-log'>" + u.NewValue + "</span>");
                                }
                                else if (u.Name != "Label")
                                {
                                    descriptionUpdate.Add(u.Name + ": " + u.NewValue);
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
                                    descriptionUpdate.Add(u.Name + " :" + string.Join(",", labelContent));
                                }
                            }
                            item.DetailUpdate = string.Join("|", descriptionUpdate);
                        }
                    }
                    return item;
                }).ToList();

                string webRootPath = "/upload/other/";
                string fileName = TabName + "Ticket" + DateTime.UtcNow.ToString("yyyyMMddhhmmssff") + ".xlsx";
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {

                    IWorkbook workbook = new XSSFWorkbook();
                    //name style
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 14;
                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);

                    IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                    ISheet excelSheet = workbook.CreateSheet(TabName + " Ticket");
                    //set column width
                    excelSheet.SetColumnWidth(1, 15 * 256);
                    excelSheet.SetColumnWidth(2, 15 * 256);
                    excelSheet.SetColumnWidth(3, 20 * 256);
                    excelSheet.SetColumnWidth(4, 10 * 256);
                    excelSheet.SetColumnWidth(5, 20 * 256);
                    excelSheet.SetColumnWidth(6, 15 * 256);
                    excelSheet.SetColumnWidth(7, 20 * 256);
                    excelSheet.SetColumnWidth(8, 20 * 256);
                    //reprot info
                    IRow row1 = excelSheet.CreateRow(0);
                    row1.CreateCell(0).SetCellValue(webinfo?.CompanyName);
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 1));
                    IRow row2 = excelSheet.CreateRow(1);
                    row2.CreateCell(0).SetCellValue(address.Length > 0 ? address[0] : "---");
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 0, 1));
                    IRow row3 = excelSheet.CreateRow(2);
                    row3.CreateCell(0).SetCellValue(address.Length > 0 ? address[1] + "," + address[2] + address[3] : "---,--- #####");
                    excelSheet.AddMergedRegion(new CellRangeAddress(2, 2, 0, 1));
                    IRow row4 = excelSheet.CreateRow(3);
                    row4.CreateCell(0).SetCellValue("www.enrichco.us");
                    excelSheet.AddMergedRegion(new CellRangeAddress(3, 3, 0, 1));

                    IRow row5 = excelSheet.CreateRow(5);
                    ICell cell = row5.CreateCell(5);
                    cell.SetCellValue(new XSSFRichTextString(TabName.ToUpper() + " TICKET REPORT"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(5, 6, 5, 6));
                    IFont fontTitle = workbook.CreateFont();
                    fontTitle.IsBold = true;
                    fontTitle.FontHeightInPoints = 17;
                    ICellStyle styleTitle = workbook.CreateCellStyle();
                    styleTitle.SetFont(fontTitle);
                    styleTitle.Alignment = HorizontalAlignment.Center;
                    styleTitle.VerticalAlignment = VerticalAlignment.Center;
                    cell.CellStyle = styleTitle;
                    //row2.CreateCell(5).SetCellValue("Date: " + DateTime.Now.ToString("MM dd,yyyy hh:mm tt"));
                    //excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));

                    excelSheet.CreateFreezePane(0, 10, 0, 10);

                    //Search info
                    IRow s_row = excelSheet.CreateRow(8);

                    //header table
                    //header style
                    IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Color = HSSFColor.White.Index;
                    font1.FontHeightInPoints = 13;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;
                    style1.FillPattern = FillPattern.SolidForeground;
                    IRow header = excelSheet.CreateRow(9);
                    string[] head_titles = { "", "#", "Ticket Name", "Assign", "Member Tags", "Status", "Type", "Note", "Label", "Open By", "Open Date (UTC +0)", "Close Date (UTC +0)", "Aging", "Time Open To Closed", "Prioriry", "Severity", "Link IMS", "Department", "Last Update" };
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]);
                        c.CellStyle = style1;
                    }
                    int row_num = 9;
                    //table data
                    int index = 1;

                    var IMSUrl = ConfigurationManager.AppSettings["IMSUrl"].ToString();

                    foreach (var ticket in Tickets.ToList())
                    {
                        row_num++;
                        IRow row_next_1 = excelSheet.CreateRow(row_num);
                        row_next_1.CreateCell(0).SetCellValue(index);
                        row_next_1.GetCell(0).SetCellType(CellType.Numeric);
                        row_next_1.CreateCell(1).SetCellValue(ticket.Id.ToString());

                        row_next_1.CreateCell(2).SetCellValue(ticket.Name);
                        row_next_1.CreateCell(3).SetCellValue(ticket.AssignMemberNames);
                        row_next_1.CreateCell(4).SetCellValue(ticket.MemberTag);
                        row_next_1.CreateCell(5).SetCellValue(ticket.Status);


                        row_next_1.CreateCell(6).SetCellValue(ticket.TypeName);
                        row_next_1.CreateCell(7).SetCellValue(ticket.Note);
                        row_next_1.CreateCell(8).SetCellValue(ticket.Tags);


                        row_next_1.CreateCell(9).SetCellValue(ticket.OpenByName ?? ticket.CreateByName);
                        row_next_1.CreateCell(10).SetCellValue(ticket.OpenDateByDate?.ToString("MMM dd,yyyy hh:mm tt"));
                        row_next_1.CreateCell(11).SetCellValue(ticket.closed_date?.ToString("MMM dd,yyyy hh:mm tt"));
                        string aging = "";
                        if (ticket.closed_date != null)
                        {
                            aging = CommonFunc.FormatDateRemain(ticket.closed_date.Value);
                        }
                        row_next_1.CreateCell(12).SetCellValue(aging);
                        string timeOpenToClose = "";
                        if (ticket.closed_date.HasValue && ticket.OpenDateByDate.HasValue)
                        {
                            List<string> timeList = new List<string>();
                            TimeSpan timeDifference = ticket.closed_date.Value - ticket.OpenDateByDate.Value;
                            if (timeDifference.Hours > 0)
                            {
                                timeList.Add($"{timeDifference.Hours + (timeDifference.Days * 60)} hours");
                            }
                            timeList.Add($"{timeDifference.Days} minutes");
                            timeOpenToClose = string.Join(", ", timeList);
                        }
                        row_next_1.CreateCell(13).SetCellValue(timeOpenToClose);

                        row_next_1.CreateCell(14).SetCellValue(ticket.Priority?.Name);
                        IFont fontPriority = workbook.CreateFont();
                        fontPriority.IsBold = true;
                        fontPriority.FontHeightInPoints = 12;
                        ICellStyle stylePriority = workbook.CreateCellStyle();
                        stylePriority.SetFont(fontPriority);
                        row_next_1.GetCell(14).CellStyle = stylePriority;
                        row_next_1.CreateCell(15).SetCellValue(ticket.SeverityName);
                        row_next_1.CreateCell(16).SetCellValue(IMSUrl + "ticket/detail/" + ticket.Id.ToString());
                        XSSFHyperlink link = new XSSFHyperlink(HyperlinkType.Url);
                        link.Address = (IMSUrl + "ticket/detail/" + ticket.Id.ToString());

                        IFont fontLink = workbook.CreateFont();
                        fontLink.FontHeightInPoints = 12;
                        fontLink.Color = HSSFColor.LightBlue.Index;
                        ICellStyle styleLink = workbook.CreateCellStyle();
                        styleLink.SetFont(fontLink);
                        row_next_1.GetCell(16).Hyperlink = link;
                        row_next_1.GetCell(16).CellStyle = styleLink;

                        row_next_1.CreateCell(17).SetCellValue(ticket.DepartmentName);

                        if (!string.IsNullOrEmpty(ticket.DetailUpdate))
                        {
                            row_next_1.CreateCell(18).SetCellValue(StripHTML(ticket.DetailUpdate));
                            var cell9 = row_next_1.GetCell(18);
                            cell9.CellStyle.WrapText = true;
                        }

                        index++;
                    }
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        excelSheet.AutoSizeColumn(i);
                    }
                    workbook.Write(fs);
                }
                var path = Server.MapPath(Path.Combine(webRootPath, fileName));
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileStream.Close();
                }
                memoryStream.Position = 0;


                return Json(new { status = true, path = path });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][ExportExcel] error when export excel ticket");
                return Json(new { status = false, message = ex.Message });
            }


        }
        public string StripHTML(string HTMLText, bool decode = true)
        {
            Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
            var stripped = reg.Replace(HTMLText, "");
            return decode ? HttpUtility.HtmlDecode(stripped) : stripped;
        }
        public ActionResult DownloadExcelFile(string path)
        {
            Uri uri = new Uri(path);
            return File(path, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", System.IO.Path.GetFileName(uri.LocalPath));
        }
        #endregion

        #region Task
        public ActionResult CreateOrUpdateTicketTaskTemplate(long? TaskId, long? TicketId, string TicketGroup)
        {
            try
            {
                var task = new Ts_Task();
                var db = new WebDataModel();
                if (TaskId > 0)
                {
                    task = db.Ts_Task.Find(TaskId);
                    TicketId = task.TicketId;
                }

                ViewBag.TicketId = TicketId;
                ViewBag.Category = db.Ts_TaskTemplateCategory.Where(x => x.TicketGroup == TicketGroup && x.IsDeleted != true).ToList();
                return PartialView("_TicketTaskTemplate", task);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Ticket][AddTask] error when add task ticket");
                throw;
            }
        }
        public ActionResult LoadTicketTaskTemplate(long? TaskId, long? categoryId, long? TicketId)
        {
            var TaskTemplateDetail = new TicketTaskTemplateDetail();
            var ListSubTask = new List<SubTaskTemplate>();
            var db = new WebDataModel();
            if (TaskId > 0)
            {

                var task = db.Ts_Task.Find(TaskId);
                TaskTemplateDetail.Id = task.Id;
                var category = db.Ts_TaskTemplateCategory.Where(x => x.Id == task.TaskTemplateCategoryId).FirstOrDefault();
                TaskTemplateDetail.TaskTemplateCategoryId = category.Id;
                TaskTemplateDetail.TaskTemplateCategoryName = category.Name;
                TaskTemplateDetail.Requirement = task.Requirement;
                TaskTemplateDetail.TicketId = task.TicketId;
                TaskTemplateDetail.Complete = task.Complete;
                TaskTemplateDetail.TaskName = task.Name;
                TaskTemplateDetail.Description = task.Description;
                TaskTemplateDetail.AssignMemberNumber = task.AssignedToMemberNumber?.Split(',');
                TaskTemplateDetail.AssignMemberName = task.AssignedToMemberName;
                //var ListField = db.Ts_TaskTemplateField.Where(x => x.CategoryId == category.Id).OrderBy(x => x.DisplayOrder).ToList();
                var AllSubTask = db.Ts_Task.Where(x => x.ParentTaskId == task.Id).ToList();
                ViewBag.TaskFiles = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == task.Id).ToList();
                foreach (var item in AllSubTask)
                {
                    var SubTask = new SubTaskTemplate();

                    SubTask.Id = item.Id;
                    SubTask.Name = item.Name;
                    SubTask.Description = item.Description;
                    SubTask.Complete = item.Complete;
                    SubTask.Files = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == item.Id).ToList();
                    ListSubTask.Add(SubTask);

                }
            }
            else
            {
                if (categoryId == null)
                {
                    return Content("");
                }

                var category = db.Ts_TaskTemplateCategory.Where(x => x.Id == categoryId).FirstOrDefault();
                TaskTemplateDetail.TaskTemplateCategoryId = category.Id;
                TaskTemplateDetail.TaskTemplateCategoryName = category.Name;
                TaskTemplateDetail.Requirement = category.Requirement;
                TaskTemplateDetail.Description = category.Description;
                TaskTemplateDetail.TicketId = TicketId;
                TaskTemplateDetail.TaskName = category.Name;
                var ListField = db.Ts_TaskTemplateField.Where(x => x.CategoryId == categoryId).OrderBy(x => x.DisplayOrder).ToList();
                var subtaskId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                foreach (var item in ListField)
                {
                    var SubTask = new SubTaskTemplate();
                    SubTask.Id = subtaskId;
                    SubTask.Name = item.Name;
                    SubTask.Required = item.IsRequired;
                    SubTask.Description = item.Description;
                    ListSubTask.Add(SubTask);
                    subtaskId++;
                }
            }
            TaskTemplateDetail.SubTaskTemplateList = ListSubTask;

            ViewBag.ListMember = db.P_Member.Where(x => x.Active == true).Select(x => new MemberSelect_View
            {
                CellPhone = x.CellPhone,
                Department = x.DepartmentId,
                MemberNumber = x.MemberNumber,
                Name = x.FullName,
                PersonalEmail = x.PersonalEmail
            }).ToList();
            var Reminder = new T_RemindersTicket();
            var reminderTicket = db.T_RemindersTicket.Where(x => x.TicketId == TicketId && x.TaskId == TaskId).FirstOrDefault();
            if (reminderTicket != null)
            {
                Reminder = reminderTicket;
            }

            ViewBag.Reminders = Reminder;
            return PartialView("_TicketTaskTemplateDetail", TaskTemplateDetail);
        }
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateTicketTaskTemplate(TicketTaskTemplateDetail model, T_RemindersTicket reminderModel)
        {
            WebDataModel db = new WebDataModel();
            using (var Trans = db.Database.BeginTransaction())
            {
                try
                {
                    //var task_id = long.Parse(Request["ts_id"]);
                    var _id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                    string message = "";
                    long taskId;
                    if (model.Id > 0) //Update task
                    {
                        var task = db.Ts_Task.Find(model.Id);
                        if (task != null)
                        {


                            #region Update Task

                            task.Name = model.TaskName;
                            task.Description = model.Description;
                            task.Complete = model.Complete;
                            task.Requirement = model.Requirement;
                            task.TaskTemplateCategoryId = model.TaskTemplateCategoryId;
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
                            if (model.AssignMemberNumber != null)
                            {
                                task.AssignedToMemberNumber = string.Join(",", model.AssignMemberNumber);
                                List<string> name = new List<string>();
                                foreach (var number in model.AssignMemberNumber)
                                {
                                    name.Add(db.P_Member.Where(m => m.MemberNumber == number).FirstOrDefault()?.FullName + "(#" + number + ")");
                                }
                                task.AssignedToMemberName = string.Join(",", name);
                            }
                            else
                            {
                                task.AssignedToMemberNumber = null;
                                task.AssignedToMemberName = null;
                            }
                            task.UpdateBy = DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - By: " + cMem.FullName + " | " + task.UpdateBy;

                            db.Entry(task).State = System.Data.Entity.EntityState.Modified;

                            var taskfiles = Request["upload_id_" + task.Id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();

                            var task_old_files = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == task.Id).ToList();
                            var remove_files = task_old_files.Where(s => !taskfiles.Contains(s.UploadId.ToString())).ToList();
                            //remove files
                            MoreFileDelete(remove_files);
                            db.UploadMoreFiles.RemoveRange(remove_files);
                            // add new files
                            foreach (var s in taskfiles.ToList())
                            {
                                var file = new UploadMoreFile
                                {
                                    TableId = task.Id,
                                    TableName = "Ts_Task",
                                    FileName = UploadAttachFile("/Upload/ts_task/" + task.Id.ToString() + "/", s, null, null, out string n, true),
                                    UploadId = _id++,
                                };
                                if (!string.IsNullOrEmpty(file.FileName))
                                    db.UploadMoreFiles.Add(file);
                            }
                            var AllSubtask = db.Ts_Task.Where(x => x.ParentTaskId == model.Id).ToList();
                            //update sub task
                            if (model.SubTaskTemplateList != null && model.SubTaskTemplateList.Count() > 0)
                            {
                                foreach (var subtask in AllSubtask)
                                {
                                    var item = model.SubTaskTemplateList.Where(x => x.Id == subtask.Id).FirstOrDefault();
                                    if (item == null)
                                    {
                                        db.Ts_Task.Remove(subtask);
                                        continue;
                                    }

                                    subtask.Id = item.Id;
                                    subtask.Name = item.Name;
                                    subtask.Description = item.Description;
                                    subtask.Complete = item.Complete;
                                    if (item.Complete == true)
                                    {
                                        if (subtask.CompletedDate == null)
                                        {
                                            subtask.CompletedDate = DateTime.UtcNow;
                                        }
                                    }
                                    else
                                    {
                                        subtask.CompletedDate = null;
                                    }

                                    var files = Request["upload_id_" + subtask.Id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
                                    var subtaskfiles = Request["upload_id_" + subtask.Id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
                                    var subtask_old_files = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == subtask.Id).ToList();
                                    var remove_subtaskfiles = subtask_old_files.Where(s => !subtaskfiles.Contains(s.UploadId.ToString())).ToList();
                                    MoreFileDelete(remove_subtaskfiles);
                                    db.UploadMoreFiles.RemoveRange(remove_subtaskfiles);
                                    foreach (var s in files)
                                    {
                                        var subfile = new UploadMoreFile
                                        {
                                            TableId = subtask.Id,
                                            TableName = "Ts_Task",
                                            FileName = UploadAttachFile("/Upload/ts_task/sub_task/" + subtask.Id.ToString() + "/", s, null, null, out string n, true),
                                            UploadId = _id++,
                                        };
                                        if (!string.IsNullOrEmpty(subfile.FileName))
                                            db.UploadMoreFiles.Add(subfile);
                                    }
                                    model.SubTaskTemplateList.Remove(item);
                                }
                                if (model.SubTaskTemplateList.Count() > 0)
                                {
                                    foreach (var item in model.SubTaskTemplateList)
                                    {
                                        if (string.IsNullOrEmpty(item.Name))
                                        {
                                            continue;
                                        }
                                        var subtask = new Ts_Task
                                        {
                                            Name = item.Name,
                                            Complete = item.Complete,
                                        };

                                        subtask.Id = item.Id;
                                        subtask.TicketId = task.TicketId;
                                        subtask.TicketName = task.TicketName;
                                        subtask.ParentTaskId = task.Id;
                                        subtask.ParentTaskName = task.Name;
                                        subtask.Description = task.Description;
                                        subtask.CreateBy = cMem.FullName;
                                        subtask.CreateByMemberNumber = cMem.MemberNumber;

                                        subtask.CreateAt = DateTime.UtcNow;
                                        db.Ts_Task.Add(subtask);
                                        var files = Request["upload_id_" + subtask.Id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
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
                            }
                            else if (AllSubtask.Count() > 0)
                            {
                                db.Ts_Task.RemoveRange(AllSubtask);
                            }

                            #endregion
                            await db.SaveChangesAsync();
                            Trans.Commit();
                            Trans.Dispose();
                            //email notice
                            await ViewControler.TaskViewService.SendNoticeAfterTaskUpdate(task, "update", cMem);
                            // TempData["s"] = "Edit success!";
                            taskId = task.Id;
                            message = "edit task success";

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
                            Name = model.TaskName,
                            Complete = model.Complete,
                            Description = model.Description,
                            Requirement = model.Requirement,
                            TaskTemplateCategoryId = model.TaskTemplateCategoryId,
                            CreateBy = cMem.FullName,
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
                        var ticket = db.T_SupportTicket.Where(x => x.Id == model.TicketId).FirstOrDefault();

                        if (ticket != null)
                        {
                            task.TicketId = ticket.Id;
                            task.TicketName = ticket.Name;
                        }

                        if (model.AssignMemberNumber != null)
                        {
                            task.AssignedToMemberNumber = string.Join(",", model.AssignMemberNumber);
                            List<string> name = new List<string>();
                            foreach (var number in model.AssignMemberNumber)
                            {
                                name.Add(db.P_Member.Where(m => m.MemberNumber == number).FirstOrDefault()?.FullName + "(#" + number + ")");
                            }
                            task.AssignedToMemberName = string.Join(",", name);
                        }
                        db.Ts_Task.Add(task);
                        taskId = task.Id;
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
                        if (model.SubTaskTemplateList != null)
                        {
                            //var list_subtask = Session[SUB_TASK] as List<Ts_Task_session>;
                            foreach (var item in model.SubTaskTemplateList)
                            {
                                if (string.IsNullOrEmpty(item.Name))
                                {
                                    continue;
                                }
                                var subtask = new Ts_Task
                                {
                                    Name = item.Name,
                                    Complete = item.Complete,
                                };

                                subtask.Id = item.Id;
                                subtask.TicketId = task.TicketId;
                                subtask.TicketName = task.TicketName;
                                subtask.ParentTaskId = task.Id;
                                subtask.ParentTaskName = task.Name;
                                subtask.Description = task.Description;
                                subtask.CreateBy = cMem.FullName;
                                subtask.CreateByMemberNumber = cMem.MemberNumber;
                                subtask.CreateAt = DateTime.UtcNow;
                                db.Ts_Task.Add(subtask);
                                var files = Request["upload_id_" + subtask.Id].Split(',').Where(l => l != "{{id}}").ToList() ?? new List<string>();
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
                        db.SaveChanges();
                        Trans.Commit();
                        Trans.Dispose();
                        //email notice
                        await ViewControler.TaskViewService.SendNoticeAfterTaskUpdate(task, "new", cMem);
                        // TempData["s"] = "Add success!";
                        message = "add task success";

                    }
                    var reminder = db.T_RemindersTicket.FirstOrDefault(x => x.TicketId == model.TicketId && x.TaskId == model.Id);

                    if (reminder != null)
                    {
                        var reminderTicketService = new ReminderTicketService();

                        if (Request["enableReminder"] == "true")
                        {
                            var reminderDateUTC = (reminderModel.Date.Value.Date + reminderModel.Time).Value.IMSToUTCDateTime();
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
                            var reminderDateUTC = (reminderModel.Date.Value.Date + reminderModel.Time).Value.IMSToUTCDateTime();
                            reminderModel.Time = reminderDateUTC.TimeOfDay;
                            reminderModel.Date = reminderDateUTC.Date;
                            reminderModel.Note = reminderModel.Note;
                            reminderModel.CreateAt = DateTime.UtcNow;
                            reminderModel.CreateBy = cMem.FullName;
                            reminderModel.TicketId = model.TicketId.Value;
                            reminderModel.TaskId = taskId;
                            reminderModel.Active = true;
                            reminderModel.HangfireJobId = Guid.NewGuid().ToString();
                            db.T_RemindersTicket.Add(reminderModel);
                            db.SaveChanges();
                            reminderTicketService.CreateJob(reminderModel);
                        }
                    }
                    return Json(new { status = true, message });
                }
                catch (Exception ex)
                {
                    Trans.Dispose();
                    _logService.Error(ex, $"[Ticket][AddTask] error when save ticket task");
                    return Json(new { status = true, message = ex.Message });
                }
            }

        }

        [HttpPost]
        public ActionResult AddSubTask(int? Index)
        {
            var model = new SubTaskTemplate();
            ViewBag.Index = Index;
            model.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
            return PartialView("_AddSubTask", model);
        }

        [HttpPost]
        public ActionResult ExpandTask(long? TaskId)
        {
            var TaskTemplateDetail = new TicketTaskTemplateDetail();
            var ListSubTask = new List<SubTaskTemplate>();
            var db = new WebDataModel();
            var task = db.Ts_Task.Find(TaskId);
            TaskTemplateDetail.Id = task.Id;
            //var category = db.Ts_TaskTemplateCategory.Where(x => x.Id == task.TaskTemplateCategoryId).FirstOrDefault();
            //TaskTemplateDetail.TaskTemplateCategoryId = category.Id;
            //TaskTemplateDetail.TaskTemplateCategoryName = category.Name;
            TaskTemplateDetail.Requirement = task.Requirement;
            TaskTemplateDetail.TicketId = task.TicketId;
            TaskTemplateDetail.Complete = task.Complete;
            TaskTemplateDetail.TaskName = task.Name;
            TaskTemplateDetail.Description = task.Description;
            TaskTemplateDetail.AssignMemberNumber = task.AssignedToMemberNumber?.Split(',');
            TaskTemplateDetail.AssignMemberName = task.AssignedToMemberName;
            TaskTemplateDetail.Note = task.Note;
            var AllSubTask = db.Ts_Task.Where(x => x.ParentTaskId == task.Id).ToList();
            foreach (var item in AllSubTask)
            {
                var SubTask = new SubTaskTemplate();

                SubTask.Id = item.Id;
                SubTask.Name = item.Name;
                SubTask.Description = item.Description;
                SubTask.Complete = item.Complete;
                SubTask.Files = db.UploadMoreFiles.Where(s => s.TableName == "Ts_Task" && s.TableId == item.Id).ToList();
                ListSubTask.Add(SubTask);
            }

            TaskTemplateDetail.SubTaskTemplateList = ListSubTask;
            return PartialView("_ExpandTask", TaskTemplateDetail);
        }

        [HttpPost]
        public ActionResult CompleteTask(long? TaskId)
        {
            try
            {
                var completeAll = false;
                var db = new WebDataModel();
                var task = db.Ts_Task.Where(x => x.Id == TaskId).FirstOrDefault();
                if (task == null)
                {
                    return Json(new { status = false, message = "task not found" });
                }
                task.Complete = !(task.Complete ?? false);
                if (task.Complete == true)
                {
                    task.CompletedDate = DateTime.UtcNow;
                }
                else
                {
                    task.CompletedDate = null;
                }
                if (task.ParentTaskId == null)
                {
                    var subtask = db.Ts_Task.Where(x => x.ParentTaskId == task.Id).ToList();
                    foreach (var sub in subtask)
                    {
                        if (task.Complete == true)
                        {
                            sub.Complete = true;
                            sub.CompletedDate = DateTime.UtcNow;
                        }
                        else
                        {
                            sub.Complete = false;
                            sub.CompletedDate = null;
                        }
                    }
                }
                else
                {
                    var allSubTask = db.Ts_Task.Where(x => x.ParentTaskId == task.ParentTaskId && x.Id != task.Id).ToList();
                    var parentTask = db.Ts_Task.Where(x => x.Id == task.ParentTaskId).FirstOrDefault();
                    if (!allSubTask.Any(x => x.Complete != true) && task.Complete == true)
                    {
                        parentTask.Complete = true;
                        parentTask.CompletedDate = DateTime.UtcNow;
                        completeAll = true;
                    }
                    else
                    {
                        parentTask.Complete = false;
                        parentTask.CompletedDate = null;
                        completeAll = false;
                    }
                }
                db.SaveChanges();
                return Json(new { status = true, message = "update task success", completeAll });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        #endregion

        #region Note
        public ActionResult GetNoteByTicketId(long TicketId)
        {
            var db = new WebDataModel();
            var ticket = db.T_SupportTicket.Find(TicketId);
            if (ticket == null)
            {
                return Json(new { status = false, message = "ticket not found !" }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { status = true, Note = ticket.Note }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> NoteSave(long TicketId, string Note)
        {
            var db = new WebDataModel();
            var ticket = db.T_SupportTicket.Find(TicketId);
            if (ticket == null)
            {
                return Json(new { status = false, message = "ticket not found !" });
            }
            ticket.Note = Note;
            ticket.UpdateAt = DateTime.UtcNow;
            ticket.UpdateBy = cMem.FullName;
            ticket.UpdateTicketHistory += DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName + "|";
            db.SaveChanges();
            await TicketViewService.SendNoticeAfterTicketUpdate(ticket, "update", db, cMem.MemberNumber);
            return Json(new { status = true });
        }
        [HttpPost]
        public ActionResult GetLastUpdate(long TicketId)
        {
            var db = new WebDataModel();
            var ticket = db.T_SupportTicket.Find(TicketId);
            string DetailUpdate = "";
            string Updated = "";
            Updated = ticket.UpdateTicketHistory;
            if (ticket.UpdateAt != null)
            {
                var updateDetail = db.T_TicketUpdateLog.Where(x => x.TicketId == ticket.Id).AsEnumerable().GroupBy(x => x.UpdateId).LastOrDefault();
                if (updateDetail != null)
                {
                    List<string> descriptionUpdate = new List<string>();
                    foreach (var u in updateDetail)
                    {
                        if (u.Name != "Label")
                        {
                            descriptionUpdate.Add(u.Name + ": " + u.NewValue);
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
                            descriptionUpdate.Add(u.Name + " :" + string.Join(",", labelContent));
                        }
                    }
                    DetailUpdate = string.Join("|", descriptionUpdate);
                }
            }

            return Json(new { DetailUpdate, Updated });
        }
        #endregion

        #region status
        public JsonResult SaveStageStatus(T_TicketStatus status, string projectId)
        {
            var db = new WebDataModel();
            var project = db.T_Project_Milestone.Find(projectId);
            if (project == null)
            {
                return Json(new { status = false, message = "Please select a project!" });
            }
            if (status.Id == 0)
            {
                try
                {

                    if (db.T_TicketStatus.Any(v => v.Name == status.Name && v.ProjectId == projectId && v.SiteId == cMem.SiteId))
                    {
                        return Json(new { status = false, message = "Status name already exist!" });
                    }
                    status.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                    status.ProjectId = project.Id;
                    status.SiteId = cMem.SiteId;
                    db.T_TicketStatus.Add(status);
                    db.SaveChanges();
                    return Json(new { status = true, message = "Create new status completed!", StatusId = status.Id, StatusName = status.Name });
                }
                catch (Exception e)
                {
                    return Json(new { status = false, message = "Create status fail!: " + e.Message });
                }
            }
            else
            {
                try
                {
                    if (db.T_TicketStatus.Any(v => v.Name == status.Name && v.ProjectId == project.Id && v.Id != status.Id && v.SiteId == cMem.SiteId))
                    {
                        return Json(new { status = false, message = "Status name already exist!" });
                    }
                    var o_status = db.T_TicketStatus.Find(status.Id);
                    if (o_status == null || o_status.ProjectId != project.Id)
                    {
                        return Json(new { status = false, message = "status not found!" });
                    }
                    if (o_status.Name == "Went Live (Done Delivery)")
                    {
                        return Json(new { status = false, message = "Status can't update" });
                    }
                    o_status.Name = status.Name;
                    o_status.Type = status.Type;
                    o_status.UpdateBy = cMem.FullName;
                    o_status.UpdateAt = DateTime.UtcNow;
                    o_status.Order = status.Order;
                    db.Entry(o_status).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { status = true, message = "Edit status completed!", StatusId = status.Id, StatusName = status.Name });
                }
                catch (Exception e)
                {
                    return Json(new { status = false, message = "Edit status fail! :" + e.Message });
                }
            }

        }
        public JsonResult LoadTicketStatus(string projectId)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var list_status = db.T_TicketStatus.Where(t => t.ProjectId == projectId && t.IsDeleted != true && t.SiteId == cMem.SiteId).OrderBy(x => x.Order).Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Order,
                    x.Type,
                    x.ProjectId
                }).ToList();
                return Json(list_status);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        public ActionResult GetListStatusUpdate(string ProjectId)
        {
            var db = new WebDataModel();
            var query = from status in db.T_TicketStatus where status.ProjectId == ProjectId && status.IsDeleted != true && status.SiteId == cMem.SiteId select status;
            var project = db.T_Project_Milestone.Where(x => x.Id == ProjectId).FirstOrDefault();
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = project.Id;
            return PartialView("_ListTicketStatus", query.ToList());
        }
        public JsonResult DeleteStatus(long Id)
        {
            var db = new WebDataModel();
            var status = db.T_TicketStatus.Find(Id);
            if (status == null)
            {
                return Json(new { status = false, message = "status not found" });
            }
            if (status.Name.Trim().ToLower().Equals("open") || status.Name.Trim().ToLower().Equals("close") || status.Name.Trim().ToLower().Equals("opened") || status.Name.Trim().ToLower().Equals("closed"))
            {
                return Json(new { status = false, message = "cannot delete open or close status" });
            }
            if (status.Name == "Went Live (Done Delivery)")
            {
                return Json(new { status = false, message = "Cannot delete status" });
            }
            status.IsDeleted = true;
            status.UpdateBy = cMem.FullName;
            status.UpdateAt = DateTime.UtcNow;
            db.SaveChanges();
            return Json(new { status = true, message = "delete status success" });
        }
        //public JsonResult GetStageStatus(long Id)
        //{
        //    var status = db.T_TicketStatus.Find(Id);

        //    if (status == null)
        //    {
        //        return Json(new { status = false, message = "Status not found!" });
        //    }
        //    var project = db.T_Project_Milestone.Find(status.ProjectId);
        //    return Json(new
        //    {
        //        status = true,
        //        status.Id,
        //        status.Name,
        //        status.Order,
        //        status.Type,
        //        projectName = project.Name,
        //        projectId = project.Id
        //    });
        //}


        //[HttpGet]
        //public JsonResult GetStatusById(long? Id)
        //{
        //    try
        //    {
        //        WebDataModel db = new WebDataModel();
        //        var status = db.T_TicketStatus.Find(Id);
        //        return Json(new object[] { true, status }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new object[] { false, ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}
        #endregion

        #region type
        public ActionResult GetListTypeUpdate(string ProjectId)
        {
            var db = new WebDataModel();
            var query = from type in db.T_TicketType where type.ProjectId == ProjectId && type.IsDeleted != true && type.SiteId == cMem.SiteId select type;
            var project = db.T_Project_Milestone.Where(x => x.Id == ProjectId).FirstOrDefault();
            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = project.Id;
            return PartialView("_ListTicketType", query.ToList());
        }
        public JsonResult LoadTicketType(string projectId)
        {
            try
            {
                if (string.IsNullOrEmpty(projectId))
                {
                    return Json(new List<SelectListItem>());
                }
                var db = new WebDataModel();
                var type = db.T_TicketType.Where(x => x.ProjectId == projectId && x.IsDeleted != true && x.SiteId == cMem.SiteId).Select(t => new SelectListItem
                {
                    Text = t.TypeName,
                    Value = t.Id.ToString()
                }).ToList();
                return Json(type);
            }
            catch (Exception)
            {
                return Json(null);
            }
        }

        public JsonResult SaveStageType(T_TicketType type)
        {
            var db = new WebDataModel();
            var stage = db.T_Project_Milestone.Find(type.ProjectId);
            var project = db.T_Project_Milestone.FirstOrDefault(x => x.Id == type.ProjectId);
            if (stage == null)
            {
                return Json(new object[] { false, "Please select a project!" });
            }
            if (db.T_TicketType.Any(v => v.TypeName == type.TypeName && v.Id != type.Id && v.ProjectId == type.ProjectId && v.SiteId == cMem.SiteId))
            {
                return Json(new object[] { false, "type name already exist!" });
            }
            if (type.Id == 0)
            {
                try
                {
                    type.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                    type.Active = true;
                    type.SiteId = cMem.SiteId;
                    type.CreateAt = DateTime.UtcNow;
                    type.CreateBy = cMem.FullName;
                    //type.BuildInCode = project.BuildInCode;
                    if (project.BuildInCode == TypeSupport || project.BuildInCode == TypeDevelopment)
                    {
                        type.SpecialType = "DEVELOPMENT";
                    }
                    else
                    {
                        type.SpecialType = "DEPLOYMENT";
                    }
                    db.T_TicketType.Add(type);
                    db.SaveChanges();

                    return Json(new { status = true, message = "Create new type completed !", TypeId = type.Id, type.TypeName });
                }
                catch (Exception e)
                {
                    return Json(new { status = false, message = "Create type fail :" + e.Message });
                }
            }
            else
            {
                try
                {
                    var o_type = db.T_TicketType.Find(type.Id);
                    if (o_type == null)
                    {
                        return Json(new object[] { false, "type not found!" });
                    }
                    o_type.TypeName = type.TypeName;
                    o_type.Order = type.Order;
                    db.Entry(o_type).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { status = true, message = "Edit type completed !", TypeId = type.Id, type.TypeName });
                }
                catch (Exception e)
                {
                    return Json(new { status = false, message = "Edit type fail :" + e.Message });
                }
            }
        }

        public JsonResult DeleteType(long TypeId)
        {
            var db = new WebDataModel();
            var type = db.T_TicketType.Find(TypeId);
            if (type == null)
            {
                return Json(new { status = false, message = "type not found!" });
            }
            if (type.BuildInCode == "Onboarding_Ticket" || type.BuildInCode == "Deployment_Ticket")
            {
                return Json(new { status = false, message = "cannot delete the type generated by the system" });
            }
            type.UpdateBy = cMem.FullName;
            type.UpdateAt = DateTime.UtcNow;
            type.IsDeleted = true;
            db.SaveChanges();
            return Json(new { status = true, message = "delete type success !" });
        }
        #endregion

        #region priority
        public ActionResult GetListPriorityUpdate(string ProjectId)
        {
            var db = new WebDataModel();
            var query = from type in db.T_Priority where type.IsDeleted != true select type;
            return PartialView("_ListPriority", query.ToList());
        }
        public JsonResult LoadPriority()
        {
            try
            {
                var db = new WebDataModel();
                var type = db.T_Priority.Where(x => x.IsDeleted != true).OrderBy(x => x.DisplayOrder).ToList();
                return Json(type);
            }
            catch (Exception)
            {
                return Json(null);
            }
        }
        public JsonResult SavePriority(T_Priority priority)
        {
            var db = new WebDataModel();
            if (db.T_Priority.Any(v => v.Name == priority.Name && v.Id != priority.Id))
            {
                return Json(new object[] { false, "type name already exist!" });
            }
            if (priority.Id == 0)
            {
                try
                {
                    priority.CreatedDate = DateTime.UtcNow;
                    priority.CreatedBy = cMem.FullName;
                    db.T_Priority.Add(priority);
                    db.SaveChanges();

                    return Json(new { status = true, message = "Create new priority completed !", priority = priority });
                }
                catch (Exception e)
                {
                    return Json(new { status = false, message = "Create type fail :" + e.Message });
                }
            }
            else
            {
                try
                {
                    var o_priority = db.T_Priority.Find(priority.Id);
                    if (o_priority == null)
                    {
                        return Json(new object[] { false, "priority not found!" });
                    }
                    o_priority.Color = priority.Color;
                    o_priority.Name = priority.Name;
                    o_priority.DisplayOrder = priority.DisplayOrder;
                    o_priority.DeadLineOfHours = priority.DeadLineOfHours;
                    db.Entry(o_priority).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { status = true, message = "Edit priority completed !", priority = o_priority });
                }
                catch (Exception e)
                {
                    return Json(new { status = false, message = "Edit priority fail :" + e.Message });
                }
            }
        }
        public JsonResult DeletePriority(int Id)
        {
            var db = new WebDataModel();
            var priority = db.T_Priority.Find(Id);
            if (priority == null)
            {
                return Json(new { status = false, message = "priority not found!" });
            }

            priority.UpdatedBy = cMem.FullName;
            priority.UpdatedDate = DateTime.UtcNow;
            priority.IsDeleted = true;
            db.SaveChanges();
            return Json(new { status = true, message = "delete priority success !" });
        }
        #endregion

        #region version
        public JsonResult LoadVersion(string ProjectId)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var query = db.T_Project_Milestone.Where(x => x.ParentId == ProjectId && x.Type == "Project_version").OrderBy(x => x.Order).ThenBy(x => x.Name);
                var data = query.Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToList();
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult LoadSubVersion(string VersionId)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var query = from subVersion in db.T_Project_Milestone where subVersion.ParentId == VersionId && subVersion.Type == "version" && subVersion.Active == true orderby subVersion.Name select subVersion;
                var data = query.Select(x => new
                {
                    x.Id,
                    x.Name
                }).ToList();
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        public ActionResult GetListVersionUpdate(string VersionId)
        {
            var db = new WebDataModel();
            var query = from subVersion in db.T_Project_Milestone where subVersion.ParentId == VersionId && subVersion.Type == "version" && subVersion.Active == true orderby subVersion.Name select subVersion;
            var version = db.T_Project_Milestone.Where(x => x.Id == VersionId).FirstOrDefault();
            var project = db.T_Project_Milestone.Where(x => x.Id == version.ParentId).FirstOrDefault();

            ViewBag.VersionName = version.Name;
            ViewBag.VersionId = version.Id;

            ViewBag.ProjectName = project.Name;
            ViewBag.ProjectId = project.Id;
            return PartialView("_ListSubversion", query.ToList());
        }
        public JsonResult SaveSubversion(string Id, string Name, string projectVerId)
        {
            try
            {
                var db = new WebDataModel();
                if (string.IsNullOrEmpty(Id))
                {
                    var proj_ver = db.T_Project_Milestone.Find(projectVerId);
                    if (db.T_Project_Milestone.Any(v => v.Name == Name && v.ParentId == projectVerId))
                    {
                        return Json(new { status = false, message = "Version name already exist !" });
                    }
                    var version = new T_Project_Milestone
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Active = true,
                        Name = Name,
                        ParentName = proj_ver.Name,
                        ParentId = proj_ver.Id,
                        Type = "version",
                    };
                    db.T_Project_Milestone.Add(version);
                    db.SaveChanges();
                    return Json(new { status = true, message = "Create new version completed !", version });
                }
                else
                {
                    var version = db.T_Project_Milestone.Find(Id);
                    if (db.T_Project_Milestone.Any(v => v.Name == Name && v.ParentId == version.ParentId && v.Id != Id))
                    {
                        return Json(new { status = false, message = "Version name already exist !" });
                    }

                    if (version == null)
                    {
                        return Json(new { status = false, message = "Version not found!" });
                    }
                    version.Name = Name;
                    db.Entry(version).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json(new { status = true, message = "Update version completed!", version });
                }

            }
            catch (Exception e)
            {
                return Json(new { status = false, message = "Create or update version fail!" });
            }

        }
        public JsonResult DeleteSubversion(string Id)
        {
            var db = new WebDataModel();
            var version = db.T_Project_Milestone.Find(Id);
            if (version == null)
            {
                return Json(new { status = false, message = "subversion not found" });
            }

            db.T_Project_Milestone.Remove(version);
            db.SaveChanges();
            return Json(new { status = true, message = "delete subversion success" });
        }
        #endregion

        #region stages 
        public ActionResult GetStages(string ProjectId)
        {
            var db = new WebDataModel();
            var project = db.T_Project_Milestone.Find(ProjectId);

            if (project.BuildInCode == TypeDevelopment)
            {
                var stages = db.T_Project_Stage.Where(x => x.ProjectId == ProjectId).OrderBy(x => x.Name).Select(x => new { x.Name, x.Id });
                return Json(stages);

            }
            else
            {
                var stages = db.T_Project_Stage.Where(x => x.ProjectId == ProjectId && x.BuildInCode == "default").OrderBy(x => x.Name).Select(x => new { x.Name, x.Id });
                return Json(stages);
            }
        }
        #endregion

        #region reminders ticket
        public ActionResult LoadReminderTicket(long TicketId)
        {
            var db = new WebDataModel();
            var reminderTicket = new T_RemindersTicket();
            var reminderCurrentTicket = db.T_RemindersTicket.Where(x => x.TicketId == TicketId).FirstOrDefault();
            if (reminderCurrentTicket != null)
            {
                reminderTicket = reminderCurrentTicket;
            }
            ViewBag.TicketId = TicketId;
            return PartialView("_ReminderTicket", reminderTicket);
        }
        public ActionResult CreateOrUpdateReminder(long TicketId)
        {
            var db = new WebDataModel();
            var model = new T_RemindersTicket();
            var reminderTicket = db.T_RemindersTicket.Where(x => x.TicketId == TicketId).FirstOrDefault();
            if (reminderTicket != null)
            {
                model = reminderTicket;
            }
            model.TicketId = TicketId;
            return PartialView("_ReminderPopup", model);
        }
        public ActionResult ReminderSubmit(T_RemindersTicket model, string GMT)
        {
            var db = new WebDataModel();
            double GMTNumber = 7;
            if (!string.IsNullOrEmpty(GMT))
            {
                GMTNumber = double.Parse(GMT.Substring(0, 3));
            }
            var reminderTicketService = new ReminderTicketService();
            var reminderDateUTC = (model.Date.Value.Date + model.Time).Value.AddHours(-GMTNumber);
            if (model.Id > 0)
            {
                var reminderTicket = db.T_RemindersTicket.Find(model.Id);
                reminderTicket.Note = model.Note;
                reminderTicket.Repeat = model.Repeat;
                reminderTicket.Time = reminderDateUTC.TimeOfDay;
                reminderTicket.Date = reminderDateUTC.Date;
                reminderTicket.UpdateAt = DateTime.UtcNow;
                reminderTicket.UpdateBy = cMem.FullName;
                db.SaveChanges();
                reminderTicketService.CreateJob(reminderTicket);
                //var feedBackId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                //var fromNote = "";
                //fromNote += "<div>Update reminder success</div><br/>";
                //fromNote += "<div>Reminder At: <span class='entry-time-ticket-feedback'>" + string.Format("{0:r}", reminderDateUTC) +"</span></div>";
                //fromNote += "<div>Repeat : " + model.Repeat+ "</div>";
                //fromNote += "<div>Note: "+ model.Note+"</div>";

                //var reminderFeedback = new T_TicketFeedback
                //{
                //    CreateAt = DateTime.UtcNow,
                //    CreateByName = "System",
                //    CreateByNumber = "",
                //    FeedbackTitle = "@" + cMem.FullName + " updated the ticket",
                //    Feedback = fromNote,
                //    Id = feedBackId,
                //    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                //    TicketId = reminderTicket.TicketId
                //};

                //db.T_TicketFeedback.Add(reminderFeedback);
                // db.SaveChanges();

                return Json(new { status = true });
            }
            else
            {
                model.Time = reminderDateUTC.TimeOfDay;
                model.Date = reminderDateUTC.Date;
                model.CreateAt = DateTime.UtcNow;
                model.CreateBy = cMem.FullName;
                model.HangfireJobId = Guid.NewGuid().ToString();
                db.T_RemindersTicket.Add(model);
                db.SaveChanges();
                //reminderTicketService.CreateJob(model);
                //var feedBackId = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                //var fromNote = "";
                //fromNote += "<div>Add reminder success</div><br/>";
                //fromNote += "<div>Reminder At: <span class='entry-time-ticket-feedback'>" + string.Format("{0:r}", reminderDateUTC) + "</span></div>";
                //fromNote += "<div>Repeat : " + model.Repeat + "</div>";
                //fromNote += "<div>Note: " + model.Note + "</div>";

                //var reminderFeedback = new T_TicketFeedback
                //{
                //    CreateAt = DateTime.UtcNow,
                //    CreateByName = "System",
                //    CreateByNumber = "",
                //    Feedback = fromNote,
                //    FeedbackTitle = "@" + cMem.FullName + " updated the ticket",
                //    Id = feedBackId,
                //    DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                //    TicketId = model.TicketId
                //};
                //db.T_TicketFeedback.Add(reminderFeedback);
                //db.SaveChanges();

                return Json(new { status = true });
            }
        }
        public ActionResult DeleteReminder(long Id)
        {
            var db = new WebDataModel();
            var reminderTicket = db.T_RemindersTicket.Find(Id);
            var reminderTicketService = new ReminderTicketService();
            reminderTicketService.DeleteJob(reminderTicket.HangfireJobId);
            db.T_RemindersTicket.Remove(reminderTicket);
            db.SaveChanges();
            return Json(new { status = true, message = "delete ticket reminders success" });
        }
        #endregion

        #region more info

        public ActionResult loadFilesRelated(long? TicketId)
        {
            var db = new WebDataModel();
            var file_related = db.T_FileRelated.Where(x => x.TicketId == TicketId).ToList();
            var files_related = new List<T_FileRelatedModel>();
            foreach (var file in file_related)
            {
                files_related.Add(new T_FileRelatedModel
                {
                    Id = file.Id,
                    TicketId = file.TicketId,
                    Note = file.Note,
                    FilesRelated = db.UploadMoreFiles.Where(f => f.TableId == file.Id && f.TableName.Equals("T_FileRelated")).ToList()
                });
            }
            return PartialView("_ListFilesRelated", files_related);
        }
        public ActionResult addOrUpdateMoreInfo(long? TicketId, long? Id)
        {
            var viewModel = new T_FileRelatedModel();
            var db = new WebDataModel();
            var ticket = db.T_SupportTicket.Where(x => x.Id == TicketId).FirstOrDefault();
            if (ticket == null)
            {
                return Content("Ticket Not Found");
            }
            if (Id > 0)
            {
                var file_related = db.T_FileRelated.Find(Id);
                viewModel.Id = file_related.Id;
                viewModel.TicketId = file_related.TicketId;
                viewModel.Note = file_related.Note;
                viewModel.Id = file_related.Id;
                viewModel.FilesRelated = db.UploadMoreFiles.Where(f => f.TableId == viewModel.Id && f.TableName.Equals("T_FileRelated")).ToList();
                db.SaveChanges();
                return PartialView("_TicketMoreInfo", viewModel);
            }
            else
            {
                viewModel.TicketId = TicketId.Value;
                return PartialView("_TicketMoreInfo", viewModel);
            }
        }
        public async Task<ActionResult> SaveMoreInfo(T_FileRelated files_related_model)
        {
            try
            {
                var db = new WebDataModel();
                if (files_related_model.Id > 0)
                {

                    var files_related = db.T_FileRelated.Where(x => x.Id == files_related_model.Id).FirstOrDefault();
                    if (files_related == null)
                    {
                        return Json(new { status = false, message = "Files related not found" });
                    }
                    files_related.Note = files_related_model.Note;
                    files_related.UpdateAt = DateTime.UtcNow;
                    files_related.UpdateBy = cMem.FullName;
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(files_related.Id, "T_FileRelated", filesTotal);
                    db.SaveChanges();
                    return Json(new { status = true, message = "update success" });
                }
                else
                {
                    files_related_model.CreatedAt = DateTime.UtcNow;
                    files_related_model.CreateBy = cMem.FullName;
                    db.T_FileRelated.Add(files_related_model);
                    db.SaveChanges();
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(files_related_model.Id, "T_FileRelated", filesTotal);
                    return Json(new { status = true, message = "add success" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        public ActionResult DeleteFileRelated(long? Id)
        {
            try
            {
                var db = new WebDataModel();
                var fileRelated = db.T_FileRelated.Where(x => x.Id == Id).FirstOrDefault();
                if (fileRelated == null)
                {
                    return Content("Files Related Not Found");
                }
                fileRelated.UpdateBy = cMem.FullName;
                db.SaveChanges();
                db.T_FileRelated.Remove(fileRelated);
                db.SaveChanges();
                return Json(new { status = true, message = "delete file related success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        #endregion



    }
}
