using EnrichcousBackOffice.Areas.Page.Models.Customize.Ticket;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel.Ticket;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Areas.Page.Controllers
{
    public class TicketController : UploadController
    {
        // GET: Page/Ticket
        const string ContactInformationStepCookieKey = "ContactInformationStepCookieKey";
        const string SelectTypeStepCookieKey = "SelectTypeStepCookieKey";
        const string TicketIdCookieKey = "TicketIdCookieKey";
        string TypeDeployment = BuildInCodeProject.Deployment_Ticket.ToString();
        string TypeDevelopment = BuildInCodeProject.Development_Ticket.ToString();
        string TypeSupport = BuildInCodeProject.Support_Ticket.ToString();
        const string PageDeployment = "DeploymentTicket";
        const string PageSupport = "SupportTicket";
        const string PageDevelopmentsTicket = "DevelopmentsTicket";
        public ActionResult Index(bool update=false)
        {
           var enableCreateTicket = bool.Parse(ConfigurationManager.AppSettings["EnableQuickTicket"]);
            if (enableCreateTicket == false)
            {
                return Redirect("/");
            }
            var imsMemberLogin = EnrichcousBackOffice.AppLB.Authority.GetCurrentMember();
            var model = new EmailAndNameModel();
            if (imsMemberLogin != null&& update == false)
            {
                model.Email = imsMemberLogin.PersonalEmail;
                model.IsMember = true;
                Response.Cookies[ContactInformationStepCookieKey].Value = JsonConvert.SerializeObject(model);
                Response.Cookies[ContactInformationStepCookieKey].Expires = DateTime.UtcNow.AddDays(1);
                return RedirectToAction("SelectType");
            }
           
            if (Request.Cookies[ContactInformationStepCookieKey] != null)
            {
                model = JsonConvert.DeserializeObject<EmailAndNameModel>(Request.Cookies[ContactInformationStepCookieKey].Value.ToString());
                Response.Cookies[ContactInformationStepCookieKey].Expires = DateTime.UtcNow.AddDays(-1);
            }
            return View(model);
        }
     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(EmailAndNameModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            WebDataModel db = new WebDataModel();
            var IsUserEmail = db.P_Member.Any(m => m.PersonalEmail == model.Email);
            if (!IsUserEmail)
            {
                var IsCustomerEmail = db.C_Customer.Any(m => m.SalonEmail == model.Email||m.MangoEmail==model.Email||m.Email==model.Email||m.BusinessEmail==model.Email||m.CompanyEmail==model.Email);
                if (!IsCustomerEmail)
                {
                    ModelState.AddModelError("email", "Email does not exist in the system. Please try again !");
                    return View(model);
                }
                model.IsMember = false;
            }
            else
            {
                model.IsMember = true;
            }
            Response.Cookies[ContactInformationStepCookieKey].Value = JsonConvert.SerializeObject(model); ;
            Response.Cookies[ContactInformationStepCookieKey].Expires = DateTime.UtcNow.AddDays(1);
            return RedirectToAction("SelectType");
        }
        public ActionResult SelectType()
        {
            if (Request.Cookies[ContactInformationStepCookieKey] == null)
            {
                return RedirectToAction("Index");
            }
            WebDataModel db = new WebDataModel();
            var model = new SelectTypeModel();
            var EmailAndName = new EmailAndNameModel();
            EmailAndName = JsonConvert.DeserializeObject<EmailAndNameModel>(Request.Cookies[ContactInformationStepCookieKey].Value.ToString());

            var SupportOption = new SelectListItem
            {
                Value = TypeSupport,
                Text = "Support"
            };
            var DeliveryOption = new SelectListItem
            {
                Value = TypeDeployment,
                Text = "Delivery"
            };
            var DevelopmentOption = new SelectListItem
            {
                Value = TypeDevelopment,
                Text = "Development"
            };
            model.TypeList.Add(SupportOption);
            if (EmailAndName.IsMember==true)
            {
                var member = db.P_Member.Where(x => x.PersonalEmail == EmailAndName.Email).FirstOrDefault();
             
                if (member == null)
                {
                    return RedirectToAction("Index");
                }
                var access = AppLB.Authority.GetAccessAuthorityByMember(member);
                if (member.RoleCode?.Contains("admin") == true)
                {
                    model.TypeList.Add(DeliveryOption);
                    model.TypeList.Add(DevelopmentOption);
                }
                else
                {
                    if (member.DepartmentId?.Contains("19120001") == true || member.DepartmentId?.Contains("19120002") == true || member.DepartmentId?.Contains("19120003") == true)
                    {
                        model.TypeList.Add(DeliveryOption);
                    }
                    var check_access_devTicket = (access.Any(k => k.Key.Equals("development_ticket_viewall")) == true && access["development_ticket_viewall"] == true)
                          || (access.Any(k => k.Key.Equals("development_ticket_view")) == true && access["development_ticket_view"] == true);
                    if (check_access_devTicket)
                    {
                        model.TypeList.Add(DevelopmentOption);
                    }
                }
               
            }
            if (Request.Cookies[SelectTypeStepCookieKey] != null)
            {
                if (Request.Cookies[SelectTypeStepCookieKey] != null)
                {
                    model.Type = Request.Cookies[SelectTypeStepCookieKey].Value;
                  
                }
                Response.Cookies[SelectTypeStepCookieKey].Expires = DateTime.UtcNow.AddDays(-1);
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SelectType(SelectTypeModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Response.Cookies[SelectTypeStepCookieKey].Value = model.Type ;
            Response.Cookies[SelectTypeStepCookieKey].Expires = DateTime.UtcNow.AddDays(1);
            return RedirectToAction("TicketInformation");
        }
        public IQueryable<Project_StageVersion> GetCurrentStage(WebDataModel db,string Page,string MemberNumber="")
        {
            try
            {
               
              
                if (Page == PageDeployment)
                {
                    var projectId = db.T_Project_Milestone.FirstOrDefault(x=>x.BuildInCode==TypeDeployment&&x.Type=="project").Id;
                  
                    var stg_ver = (from stg in db.T_Project_Stage
                                   join ver in db.T_Project_Milestone on stg.ProjectId equals ver.ParentId
                                   where ver.Type == "Project_version"
                                   join project in db.T_Project_Milestone on ver.ParentId equals project.Id
                                   where ((projectId == stg.ProjectId
                                     && stg.BuildInCode == "default" && project.BuildInCode == TypeDeployment))
                                   select new Project_StageVersion { stage = stg, version = ver }).Distinct();
                    return stg_ver;
                }
                else if (Page == PageSupport)
                {
                    var projectId = db.T_Project_Milestone.FirstOrDefault(x => x.BuildInCode == TypeSupport && x.Type == "project").Id;

                    var stg_ver = (from stg in db.T_Project_Stage
                                   join ver in db.T_Project_Milestone on stg.ProjectId equals ver.ParentId
                                   where ver.Type == "Project_version"
                                   join project in db.T_Project_Milestone on ver.ParentId equals project.Id
                                   where ((projectId == stg.ProjectId
                                   && stg.BuildInCode == "default"
                                   && project.BuildInCode == TypeSupport))
                                   orderby stg.BuildInCode descending
                                   select new Project_StageVersion { stage = stg, version = ver }).Distinct();
                    return stg_ver;
                }
                else
                {
                    var  projectId = "all";
                    var  stageId = "all";
                    var  versionId = "all";
                 
                    var stg_ver = (from stg in db.T_Project_Stage
                                join ver in db.T_Project_Milestone on stg.ProjectId equals ver.ParentId
                                where ver.Type == "Project_version"
                                join project in db.T_Project_Milestone on ver.ParentId equals project.Id
                                join mem in db.T_Project_Stage_Members on new { s = stg.Id, v = ver.Id } equals new { s = mem.StageId, v = mem.ProjectVersionId }
                                where (projectId == "all" || (projectId == stg.ProjectId
                                && (stageId == "all" || stageId == stg.Id)
                                && (versionId == "all" || versionId == ver.Id)))
                                && project.BuildInCode == TypeDevelopment
                                && mem.MemberNumber == MemberNumber
                                select new Project_StageVersion { stage = stg, version = ver }).Distinct();
                    return stg_ver;
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        [HttpGet]
        public ActionResult TicketInformation()
        {
            try
            {
                if (Request.Cookies[ContactInformationStepCookieKey] == null)
                {
                    return RedirectToAction("Index");
                }
                if (Request.Cookies[SelectTypeStepCookieKey] == null)
                {
                    return RedirectToAction("SelectType");
                }
                WebDataModel db = new WebDataModel();
                //var DeploymentProject = db.T_Project_Milestone.Where(x => x.BuildInCode == TypeDeployment).FirstOrDefault();
                //var DeploymentVersion = db.T_Project_Milestone.Where(v => v.ParentId == DeploymentProject.Id && v.Type == "Project_version").FirstOrDefault();
                  
                var ContactInformation = JsonConvert.DeserializeObject<EmailAndNameModel>(Request.Cookies[ContactInformationStepCookieKey].Value.ToString());
                var cMem = new P_Member();
                // customer send ticket
                if (ContactInformation.IsMember == true)
                {
                     cMem = db.P_Member.Where(x => x.PersonalEmail == ContactInformation.Email).FirstOrDefault();
              
                }
                var BuildInCode = Request.Cookies[SelectTypeStepCookieKey].Value;
                string Page="";
                if (BuildInCode==TypeDeployment)
                {
                    Page = PageDeployment;
                }
                else if(BuildInCode==TypeSupport)
                {
                    Page = PageSupport;
                }
                else
                {
                    Page = PageDevelopmentsTicket;
                }
                ViewBag.Page = Page;
                IQueryable<Project_StageVersion> proj_info;
                proj_info = GetCurrentStage(db, Page, cMem.MemberNumber);
                //if (Page == PageDeployment)
                //{
                //    ViewBag.Severity = db.T_TicketSeverity.Where(s => s.Active == true && s.SpecialType == "DEPLOYMENT").OrderBy(s => s.SeverityLevel).ToList();
                //    ViewBag.Label = db.T_Tags.Where(x => x.Type == "DeploymentTicket").ToList();
                //}
                //else
                //{

                //    ViewBag.Severity = db.T_TicketSeverity.Where(s => s.Active == true && s.SpecialType == "DEVELOPMENT").OrderBy(s => s.SeverityLevel).ToList();
                //    ViewBag.Label = db.T_Tags.Where(x => x.Type == "DevelopAndSupportTicket").ToList();
                //}
               
                //ViewBag.TagsData = db.T_Tags.OrderByDescending(t => t.Id).ToList();

                //var isAdmin =cMem.RoleCode?.Contains("admin") == true;
                IQueryable<T_Project_Milestone> prj_vers;
                if (Page == PageDevelopmentsTicket)
                {
                    prj_vers = (from version in db.T_Project_Milestone
                                join versionMember in db.T_Project_Stage_Members on version.Id equals versionMember.ProjectVersionId
                                where (version.Type == "Project_version" && versionMember.MemberNumber == cMem.MemberNumber)
                                orderby version.Name
                                group version by version.Id into v
                                select v.FirstOrDefault());
                }
                else
                {
                    prj_vers = (from version in db.T_Project_Milestone
                                where (version.Type == "Project_version")
                                orderby version.Name
                                select
                        version);
                }

            
                ViewBag.project_version_lists = prj_vers.ToList();
                IQueryable<StagesModel> stages;
                if (Page == PageDevelopmentsTicket)
                {
                    stages = from stage in db.T_Project_Stage
                             join stageMember in db.T_Project_Stage_Members on stage.Id equals stageMember.StageId
                             where stageMember.MemberNumber == cMem.MemberNumber
                             orderby stage.Name
                             group stage by stage.Id into s
                             select new StagesModel
                             {
                                 Stage = s.FirstOrDefault(),
                                 VersionIds = (from v in db.T_Project_Stage_Members where v.MemberNumber == cMem.MemberNumber && v.StageId == s.Key select v).Select(x => x.ProjectVersionId).ToList()
                             };
                }
                else
                {
                    stages = from stage in db.T_Project_Stage
                             orderby stage.Name

                             select new StagesModel
                             {
                                 Stage = stage,
                                 VersionIds = (from v in db.T_Project_Milestone where v.ParentId == stage.ProjectId select v).Select(x => x.Id).ToList()
                             };
                }
           
                ViewBag.list_stages = stages.OrderBy(x => x.Stage.Name).ToList();
                if (Page == PageDeployment)
                {
                    ViewBag.projects = db.T_Project_Milestone.Where(p => p.Type == "project" && (p.BuildInCode == TypeDeployment) && prj_vers.Any(v => v.ParentId == p.Id) && stages.Any(st => st.Stage.ProjectId == p.Id)).OrderBy(s => s.Name).ToList();
                    ViewBag.Department = db.P_Department.Where(x => x.Type == "DEPLOYMENT" || x.Type == "SUPPORT").ToList();
                }
                else if (Page == PageSupport)
                {
                    ViewBag.projects = db.T_Project_Milestone.Where(p => p.Type == "project" && (p.BuildInCode == TypeSupport) && prj_vers.Any(v => v.ParentId == p.Id) && stages.Any(st => st.Stage.ProjectId == p.Id)).OrderBy(s => s.Name).ToList();
                    ViewBag.Department = db.P_Department.Where(x => x.Type == "DEPLOYMENT" || x.Type == "SUPPORT").ToList();
                }
                else
                {
                    ViewBag.projects = db.T_Project_Milestone.Where(p => p.Type == "project" && (p.BuildInCode == TypeDevelopment) && prj_vers.Any(v => v.ParentId == p.Id) && stages.Any(st => st.Stage.ProjectId == p.Id)).OrderBy(s => s.Name).ToList();
                }

                var Default = proj_info.OrderByDescending(x => x.stage.BuildInCode == "default").FirstOrDefault();
              
                var projectVersion = new T_Project_Milestone();
                if (Default == null)
                {
                    var _default = (from s in db.T_TicketStage_Status
                                    join m in db.T_Project_Stage_Members on s.StageId equals m.StageId
                                    where m.MemberNumber.Contains(cMem.MemberNumber)
                                    select s).FirstOrDefault();
                    ViewBag.stage = db.T_Project_Stage.Find(_default.StageId);
                    projectVersion = db.T_Project_Milestone.Find(_default.ProjectVersionId);
                }
                else
                {
                    ViewBag.stage = db.T_Project_Stage.Find(Default.stage?.Id);
                    projectVersion = db.T_Project_Milestone.Find(Default.version.Id);
                }

                ViewBag.project_version = projectVersion;
                ViewBag.versions = db.T_Project_Milestone.Where(s => s.Type == "version" && s.ParentId == projectVersion.Id).OrderBy(s => s.Name).ToList();
                ViewBag.Type = db.T_TicketType.Where(x => x.ProjectId == projectVersion.ParentId && x.Active == true).ToList();

                var newTicket = new T_SupportTicket { Id = 0, Visible = true };
                if (ContactInformation.IsMember==false)
                {
                    ViewBag.status = db.T_TicketStatus.Where(x => x.ProjectId == Default.stage.ProjectId&&x.Type!="closed").OrderBy(s => s.Name=="Open").ToList();
                    var Customer = db.C_Customer.FirstOrDefault(m => m.SalonEmail == ContactInformation.Email || m.MangoEmail == ContactInformation.Email || m.Email == ContactInformation.Email || m.BusinessEmail == ContactInformation.Email || m.CompanyEmail == ContactInformation.Email);
                    newTicket.CustomerCode = Customer.CustomerCode;
                    newTicket.CustomerName = Customer?.BusinessName;
                }
                else
                {
                    ViewBag.status = db.T_TicketStatus.Where(x => x.ProjectId == Default.stage.ProjectId).OrderBy(s => s.Type != "open").ThenBy(s => s.Type == "closed").ThenBy(s => s.Name).ToList();
                }
                ViewBag.IsMember = ContactInformation.IsMember;
               // ViewBag.Priority = db.T_Priority.Where(x => x.IsDeleted != true).OrderBy(x=>x.DisplayOrder).ToList();
                //newTicket.EstimatedCompletionTimeTo = DateTime.UtcNow.AddDays(1);
                //newTicket.EstimatedCompletionTimeFrom = DateTime.UtcNow;
                return View(newTicket);
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }
        [HttpPost]
        public async Task<ActionResult> SaveTicketInformation()
        {
            var db = new WebDataModel();
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    if (Request.Cookies[ContactInformationStepCookieKey] == null)
                    {
                        return Json(new object[] { false , "something went wrong." });
                    }
                    if (Request.Cookies[SelectTypeStepCookieKey] == null)
                    {
                        return Json(new object[] { false, "something went wrong." });
                    }

                    var ContactInformation = JsonConvert.DeserializeObject<EmailAndNameModel>(Request.Cookies[ContactInformationStepCookieKey].Value.ToString());

                    var cMem = new P_Member();
                    var Customer = new C_Customer();
                    if (ContactInformation.IsMember == true)
                    {
                        cMem = db.P_Member.Where(x => x.PersonalEmail == ContactInformation.Email).FirstOrDefault();
                    }
                    else
                    {
                        Customer = db.C_Customer.FirstOrDefault(m => m.SalonEmail == ContactInformation.Email || m.MangoEmail == ContactInformation.Email || m.Email == ContactInformation.Email || m.BusinessEmail == ContactInformation.Email || m.CompanyEmail == ContactInformation.Email);
                        
                    }

                    var tic = new T_SupportTicket();
                    int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
                                                       && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
                    //var BuildInCode = Request["BuildInCode"].ToString();
                    tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("ff"));
                    tic.Name = Request["name"];
                    tic.Description = Request.Unvalidated["comment"];
                    tic.CreateAt = DateTime.UtcNow;   
                   
                    tic.CreateByName = ContactInformation.IsMember == true ? cMem.FullName : Customer.BusinessName;
                    tic.CreateByNumber = ContactInformation.IsMember ==true ? cMem.MemberNumber:Customer.CustomerCode;
                    tic.OpenByMemberNumber = ContactInformation.IsMember == true ? cMem.MemberNumber : Customer.CustomerCode;
                    tic.OpenByName = ContactInformation.IsMember == true ? cMem.FullName : Customer.BusinessName;

                    tic.KB = string.IsNullOrWhiteSpace(Request["kb"]) == false ? true : false;
                    if (!string.IsNullOrEmpty(Request["merchant"]))
                    {
                        var cus = db.C_Customer.Where(c => c.CustomerCode == tic.CustomerCode).FirstOrDefault();
                        if (cus != null)
                        {
                            tic.CustomerCode = cus.CustomerCode;
                            tic.CustomerName = cus.BusinessName;
                            tic.SiteId = cus.SiteId;
                        }
                       
                    }
                    else
                    {
                        tic.CustomerCode = null;
                        tic.CustomerName = null;
                    }
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

                 
                    tic.Visible = true;
                    //ticket info
                
                
                  
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

               

                    //add stage to ticket
                    var list_member_num = (Request["AssignedMember_Numbers"] ?? "").Split(',');
                    var project_version = db.T_Project_Milestone.Find(Request["ProjectVersion"] ?? "");
                    var project = db.T_Project_Milestone.FirstOrDefault(x => x.Id == project_version.ParentId);
                    if (project_version == null && project_version.Type != "Project_version")
                    {
                        throw new Exception("Project version not found!");
                    }
                  

                    var shared_mem = new List<string>();
                    var aff_ver = Request["AffectedVersion"]?.ToString();
                    var fix_ver = Request["FixedVersion"]?.ToString();
            
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(tic.Id, "T_SupportTicket", filesTotal);
                    tic.UploadIds = string.Join(",", UploadIds);             
                    tic.Note = Request["Note"];
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
                    //tic.StageId = (Request["forward_stage"] ?? "");
                    //tic.StageName = string.Join(",", StageNameList);
                    tic.VersionId = project_version.Id;
                    tic.VersionName = project_version.Name;
                    tic.ProjectId = project_version.ParentId;
                    tic.ProjectName = db.T_Project_Milestone.Find(project_version.ParentId).Name;
                    tic.UpdateAt = DateTime.UtcNow;
                    tic.UpdateBy = Customer.BusinessName;
                    //add forward stage to ticket
                    db.T_SupportTicket.Add(tic);
                    db.SaveChanges();
                    //Debug.WriteLine(errAutoAssigment);
                    tran.Commit();
                    await TicketViewService.SendNoticeAfterTicketUpdate(tic, "new", null, cMem.MemberNumber);
                    //await TicketViewController.AutoAssignment(new WebDataModel(), tic, "new");
                    Response.Cookies[TicketIdCookieKey].Value = tic.Id.ToString();
                    Response.Cookies[TicketIdCookieKey].Expires = DateTime.UtcNow.AddDays(1);
                    //BackgroundJob.Enqueue(() => this.SendEmailUpdate(tic, "new", cMem.MemberNumber));
                    return Json(new object[] { true });
                }
                catch (Exception e)
                {
                    tran.Dispose();

                    return Json(new object[] { false, e.Message });
                }
            }
        }
        public ActionResult Complete()
        {
            if (Request.Cookies[TicketIdCookieKey] == null)
            {
                return RedirectToAction("TicketInformation");
            }
            var model = new CompleteModel();
            model.TicketId = Request.Cookies[TicketIdCookieKey].Value.ToString();
            var ContactInformation = JsonConvert.DeserializeObject<EmailAndNameModel>(Request.Cookies[ContactInformationStepCookieKey].Value.ToString());
            if (ContactInformation.IsMember == true || EnrichcousBackOffice.AppLB.Authority.GetCurrentMember() != null)
            {
                model.ShowLinkDetail = true;
            }
            
            Response.Cookies[TicketIdCookieKey].Expires = DateTime.UtcNow.AddDays(-1);
            Response.Cookies[SelectTypeStepCookieKey].Expires = DateTime.UtcNow.AddDays(-1);
            return View(model);
        }

        #region
      
        public JsonResult GetMerchant()
        {
            try
            {
                var db = new WebDataModel();
                var ClosedStatusOrder = InvoiceStatus.Closed.ToString();
                var PaidStatusOrder = InvoiceStatus.Paid_Wait.ToString();
                var ContactInformation = JsonConvert.DeserializeObject<EmailAndNameModel>(Request.Cookies[ContactInformationStepCookieKey].Value.ToString());
                if (ContactInformation.IsMember == true)
                {
                    string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
                    var merchant = (from c in db.C_Customer
                                    let order = db.O_Orders.Where(o => c.CustomerCode == o.CustomerCode).Count() > 0
                                    where c.Active != 0 && (order || c.Type == STORE_IN_HOUSE)
                                    orderby c.Id descending
                                    select c).Distinct().Select(c => new { c.CustomerCode, c.BusinessName, c.BusinessAddressStreet, c.BusinessCity, c.BusinessState, c.BusinessZipCode, c.BusinessCountry, c.PartnerCode }).ToList();
                    return Json(merchant, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new List<C_Customer>(), JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region Child Action
        [ChildActionOnly]
        public virtual ActionResult TicketProgress(TicketProgressStep step)
        {
            var model =  new TicketProgressModel { TicketProgressStep = step };
            if(step== TicketProgressStep.NameAndEmail)
            {
                model.ProgressTitle = "Contact Information";
            }
            else if (step == TicketProgressStep.Type)
            {
                model.ProgressTitle = "Ticket Type";
            }
            else if (step == TicketProgressStep.TicketInformation)
            {
                model.ProgressTitle = "Ticket Information";
            }
            else if (step == TicketProgressStep.Complete)
            {
                model.ProgressTitle = "Complete";
            }
            return PartialView(model);
        }
     #endregion
    }
}