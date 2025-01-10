using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Utils.IEnums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class ProjectManagementController : Controller
    {
        WebDataModel db = new WebDataModel();
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();


        // GET: ProjectManager
        public ActionResult Index()
        {
            ViewBag.projects = db.T_Project_Milestone.Where(d => d.Type == "project").OrderBy(d => d.Name).ToList();
            ViewBag.project_versions = db.T_Project_Milestone.Where(d => d.Type == "project_version").OrderBy(d => d.Name).ToList();
            ViewBag.member = db.P_Member.Where(m => m.Active == true).ToList();
            ViewBag.deparments = (from depa in db.P_Department.AsEnumerable()
                                  where depa.ParentDepartmentId == null && depa.Active == true
                                  join gs in (from g in db.P_Department where g.ParentDepartmentId != null group g by g.ParentDepartmentId into gg select gg)
                                  on depa.Id equals gs.Key
                                  select new P_Department
                                  {
                                      Id = depa.Id,
                                      Name = depa.Name,
                                      GroupMemberNumber = depa.GroupMemberNumber + "," + string.Join(",", gs.Select(g => g.GroupMemberNumber)),
                                  }).ToList();
            var stages = db.T_Project_Stage.ToList();
            return View(stages);
        }
        public ActionResult getMembers(string stage_id)
        {
            var stage = db.T_Project_Stage.Find(stage_id);
            var groupmem = from s_m in db.T_Project_Stage_Members.Where(m => m.StageId == stage_id)
                           join member in db.P_Member.Where(m => m.Active == true) on s_m.MemberNumber equals member.MemberNumber into mem
                           from member in mem
                           group new Stage_member_view { member = member, IsLeader = s_m.IsLeader } by s_m.ProjectVersionId into ver_mems
                           select ver_mems;
            var stage_mem_by_ver = (from version in db.T_Project_Milestone.Where(v => v.Type == "project_version" && v.ParentId == stage.ProjectId && v.Active == true)
                                    join ver_mems in groupmem on version.Id equals ver_mems.Key into list_ver_mems
                                    from ver_mems in list_ver_mems
                                    select new StageVerMem_View { Version = version, ver_mems = ver_mems.ToList() }).OrderBy(v => v.Version.Name).ToList();
            ViewBag.stageId = stage_id;
            return PartialView("_ListMemberPartial", stage_mem_by_ver);
        }

        [AllowAnonymous]
        public JsonResult loadStageVersion(string projectId)
        {
          //  var viewall = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("ticket_viewall")) == true && access["ticket_viewall"] == true);
          //  var stages = db.T_Project_Stage.Where(s => s.ProjectId == projectId).Select(s=>s.Id);
          //  var ver_ids = db.T_Project_Stage_Members.Where(sm => (sm.MemberNumber == cMem.MemberNumber|| viewall) && (stage_Id=="all" && stages.Contains(sm.StageId)|| sm.StageId == stage_Id)).Select(sm => sm.ProjectVersionId).ToList();
            var list_vers = db.T_Project_Milestone.Where(v => v.Type == "Project_version" && v.ParentId== projectId&&v.Active!=false).OrderBy(v => v.Order).ThenBy(x=>x.Name).ToList();
            return Json(new object[] { true, list_vers });
        }
        #region project
        public JsonResult Project_Save(T_Project_Milestone project)
        {
            try
            {
                if (string.IsNullOrEmpty(project.Name) == true)
                {
                    throw new Exception("Project name is required.");
                }
                if (access.Any(k => k.Key.Equals("department_addnew")) == false || access["department_addnew"] != true)
                {
                    throw new Exception("Permission denied!");
                }
                if (db.T_Project_Milestone.Any(d => d.Type == "project" && d.Name == project.Name && d.Id != project.Id))
                {
                    throw new Exception("Project name already exists.");
                }
                string membernumbers = Request["Member"];
                if (string.IsNullOrEmpty(project.Id))
                {
                    project.Id = Guid.NewGuid().ToString("N");
                    project.Type = "project";
                    project.Active = true;
                    project.BuildInCode = BuildInCodeProject.Development_Ticket.ToString();
                    if (!string.IsNullOrEmpty(project.ManagerNumber))
                    {
                        project.ManagerName = db.P_Member.FirstOrDefault(x => x.MemberNumber == project.ManagerNumber).FullName;
                    }
                    if (!string.IsNullOrEmpty(project.LeaderNumber))
                    {
                        project.LeaderName = db.P_Member.FirstOrDefault(x => x.MemberNumber == project.LeaderNumber).FullName;
                    }
                    var memberMappings = new ConcurrentBag<T_Project_Milestone_Member>();
                    if (!string.IsNullOrEmpty(membernumbers))
                    {
                        Parallel.ForEach(membernumbers.Split(','), member =>
                        {
                            memberMappings.Add(new T_Project_Milestone_Member
                            {
                               MemberNumber= member,
                               CreateAt = DateTime.UtcNow,
                               CreateBy = cMem.FullName
                            });
                        });
                        
                    }
                    project.T_Project_Milestone_Member = memberMappings.ToList();
                    db.T_Project_Milestone.Add(project);
                   
                    db.SaveChanges();
                    return Json(new  { status= true, message = "Create new project completed" });
                }
                else
                {
                    var _project = db.T_Project_Milestone.Find(project.Id);
                    if (_project == null)
                    {
                        throw new Exception("Project not found!");
                    }
                    _project.ManagerNumber = project.ManagerNumber;
                    if (!string.IsNullOrEmpty(project.ManagerNumber))
                    {
                        _project.ManagerName = db.P_Member.FirstOrDefault(x => x.MemberNumber == project.ManagerNumber).FullName;
                    }
                    else
                    {
                        _project.ManagerName = null;
                    }
                    _project.LeaderNumber = project.LeaderNumber;
                    if (!string.IsNullOrEmpty(project.LeaderNumber))
                    {
                        _project.LeaderName = db.P_Member.FirstOrDefault(x => x.MemberNumber == project.LeaderNumber).FullName;
                    }
                    else
                    {
                        _project.LeaderName = null;
                    }
                    _project.Name = project.Name;
                    _project.Order = project.Order;
                    _project.Active = project.Active;
                    _project.UpdateAt = DateTime.UtcNow;
                    _project.UpdateByNumber = cMem.MemberNumber;
                    //_project.BuildInCode = BuildInCodeProject.Deployment_Ticket.ToString();
                    _project.Description = project.Description;
                    var project_members = db.T_Project_Milestone_Member.Where(x => x.ProjectId == project.Id).ToList();
                    db.T_Project_Milestone_Member.RemoveRange(project_members);
                   // _project.T_Project_Milestone_Member.Clear();
                    var memberMappings = new ConcurrentBag<T_Project_Milestone_Member>();
                    if (!string.IsNullOrEmpty(membernumbers))
                    {
                        Parallel.ForEach(membernumbers.Split(','), member =>
                        {
                            memberMappings.Add(new T_Project_Milestone_Member
                            {
                                ProjectId = _project.Id,
                                MemberNumber = member,
                                CreateAt = DateTime.UtcNow,
                                CreateBy = cMem.FullName
                            });
                        });

                    }
                    db.T_Project_Milestone_Member.AddRange(memberMappings.ToList());

                    db.SaveChanges();
                    return Json(new  {status=  true,message= "Update project completed" });
                }



            }
            catch (Exception e)
            {
                return Json(new  { status =false,message = e.Message });
            }

        }

        [AllowAnonymous]
        public JsonResult getProject(string Id)
        {
            try
            {
                var project = db.T_Project_Milestone.Where(x=>x.Id==Id).Include(x=>x.T_Project_Milestone_Member).FirstOrDefault();
                if (project == null)
                {
                    throw new Exception("Project not found!");
                }
                return Json(new  { status= true,project.Name,project.Id,project.Description,project.Order,project.ManagerNumber,project.LeaderNumber,MemberNumbers =project.T_Project_Milestone_Member.Select(x=>x.MemberNumber).ToList(),project.Active });

            }
            catch (Exception e)
            {
                return Json(new  { status =false,message= e.Message });              }
        }
        public JsonResult Project_Delete(string Id)
        {
            try
            {
                var project = db.T_Project_Milestone.Find(Id);
                if (project == null)
                {
                    throw new Exception("Project not found!");
                }
                var project_stages = db.T_Project_Stage.Where(s => s.ProjectId == project.Id);
                var stage_ticket = db.T_TicketStage_Status.Where(t => project_stages.Any(s => s.Id == t.StageId));
                //var stage_ticket_version = db.T_Project_Milestone.Where(t => project_stages.Any(s => s.Id == t.StageId));
                var stage_member = db.T_Project_Stage_Members.Where(t => project_stages.Any(s => s.Id == t.StageId));
                db.T_TicketStage_Status.RemoveRange(stage_ticket);
                db.T_Project_Stage.RemoveRange(project_stages);
                //db.T_Project_Milestone.RemoveRange(stage_ticket_version);
                db.T_Project_Stage_Members.RemoveRange(stage_member);
                db.T_Project_Milestone.Remove(project);
                db.SaveChanges();
                return Json(new object[] { true, project });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        #endregion
        #region stage
        public JsonResult Stage_save(string id, string name, string description, string project_id, string BuildInCode,string leader,int? order,bool? active)
        {
            try
            {
                if (string.IsNullOrEmpty(name) == true)
                {
                    throw new Exception("Stage name is required.");
                }
                if (access.Any(k => k.Key.Equals("department_addnew")) == false || access["department_addnew"] != true)
                {
                    throw new Exception("Permission denied!");
                }
                if (db.T_Project_Stage.Any(d => d.Name == name && d.ProjectId == project_id && d.Id != id))
                {
                    throw new Exception("Stage name already exists.");
                }
                if (string.IsNullOrEmpty(id))
                {
                    var project = db.T_Project_Milestone.Find(project_id);
                    if (project == null)
                    {
                        throw new Exception("Project not found!");
                    }

                    var stage = new T_Project_Stage();
                    stage.Id = Guid.NewGuid().ToString("N");
                    stage.Name = name;
                    stage.Desc = description;
                    stage.ProjectId = project.Id;
                    stage.ProjectName = project.Name;
                    stage.DisplayOrder = order;
                    stage.Active = active ?? false;
                    stage.BuildInCode = BuildInCode;
                    stage.LeaderNumber = leader;
                    if (!string.IsNullOrEmpty(stage.LeaderNumber))
                    {
                        stage.LeaderName = db.P_Member.Where(x => x.MemberNumber == stage.LeaderNumber).FirstOrDefault().FullName;
                    }
                   
                    db.T_Project_Stage.Add(stage);
                    db.SaveChanges();
                    return Json(new object[] { true, "Create new stage completed",true, stage });
                }
                else
                {
                    var stage = db.T_Project_Stage.Find(id);
                    if (stage == null)
                    {
                        throw new Exception("Project not found!");
                    }
                    stage.Name = name;
                    stage.Desc = description;
                    stage.LeaderNumber = leader;
                    stage.DisplayOrder = order;
                    stage.Active = active ?? false;
                    if (!string.IsNullOrEmpty(stage.LeaderNumber))
                    {
                        stage.LeaderName = db.P_Member.Where(x => x.MemberNumber == stage.LeaderNumber).FirstOrDefault().FullName;
                    }
                    else
                    {
                        stage.LeaderName = null;
                    }
                    db.T_TicketStage_Status.Where(d => d.StageId == stage.Id).ToList().ForEach(s => s.StageName = stage.Name);

                    db.Entry(stage).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, "Update stage completed",false, stage });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        public JsonResult Delete_Stage(string Id)
        {
            try
            {
                var stage = db.T_Project_Stage.Find(Id);
                if (stage == null)
                {
                    throw new Exception("Stage not found!");
                }
                var stage_ticket = db.T_TicketStage_Status.Where(t => stage.Id == t.StageId).ToList();
                //var stage_ticket_version = db.T_Project_Milestone.Where(t => stage.Id == t.StageId).ToList();
                var stage_member = db.T_Project_Stage_Members.Where(t => stage.Id == t.StageId).ToList();
                db.T_TicketStage_Status.RemoveRange(stage_ticket);
                //db.T_Project_Milestone.RemoveRange(stage_ticket_version);
                db.T_Project_Stage_Members.RemoveRange(stage_member);
                db.T_Project_Stage.Remove(stage);
                db.SaveChanges();
                return Json(new object[] { true, stage });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        [AllowAnonymous]
        public JsonResult getStage(string Id)
        {
            try
            {
                var stage = db.T_Project_Stage.Find(Id);
                if (stage == null)
                {
                    throw new Exception("Stage not found!");
                }
                return Json(new object[] { true, stage });

            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult getStageMember(string version_id, string stage_id)
        {
            try
            {
                var Listmember = db.T_Project_Stage_Members.Where(m => m.ProjectVersionId == version_id && m.StageId == stage_id).Select(m => m.MemberNumber);
                return Json(new object[] { true, Listmember });

            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }

        [AllowAnonymous]
        public JsonResult Select_members(string project_version, string stage_id, List<string> Members)
        {
            try
            {
                var stage = db.T_Project_Stage.Find(stage_id);
                if (stage == null)
                {
                    throw new Exception("Project not found!");
                }
                var project_ver = db.T_Project_Milestone.Find(project_version);
                if (project_ver == null || project_ver?.Type != "Project_version" || project_ver?.Active != true)
                {
                    throw new Exception("Version not found!");
                }
                var list_mem = new List<P_Member>();
                var list_old_mem = db.T_Project_Stage_Members.Where(m => m.ProjectVersionId == project_ver.Id && m.StageId == stage.Id).ToList();
                if (Members != null)
                {
                    db.T_Project_Stage_Members.RemoveRange(list_old_mem.Where(m => !Members.Contains(m.MemberNumber)));
                    list_mem = db.P_Member.Where(m => Members.Contains(m.MemberNumber) && m.Active == true).OrderBy(m => m.FullName).ToList();
                    list_mem.ForEach(m =>
                    {
                        if (!list_old_mem.Any(om => om.MemberNumber == m.MemberNumber))
                        {
                            var mem = new T_Project_Stage_Members
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                MemberNumber = m.MemberNumber,
                                MemberName = m.FullName,
                                ProjectVersionId = project_ver.Id,
                                ProjectVersionName = project_ver.Name,
                                StageId = stage.Id,
                                StageName = stage.Name,
                            };
                            db.T_Project_Stage_Members.Add(mem);
                        }
                    });
                }
                else
                {
                    db.T_Project_Stage_Members.RemoveRange(list_old_mem);
                }
                db.SaveChanges();
                return Json(new object[] { true, "Update members completed!", stage.Id });

            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        public JsonResult unselectMember(string version_id, string stage_id, string member_number)
        {
            try
            {
                var stage = db.T_Project_Stage.Find(stage_id);
                if (stage == null)
                {
                    throw new Exception("Project not found!");
                }
                var project_ver = db.T_Project_Milestone.Find(version_id);
                if (project_ver == null || project_ver?.Type != "Project_version" || project_ver?.Active != true)
                {
                    throw new Exception("Version not found!");
                }
                var un_member = db.T_Project_Stage_Members.Where(m => m.ProjectVersionId == project_ver.Id && m.StageId == stage.Id && m.MemberNumber == member_number).FirstOrDefault();
                if (un_member == null)
                {
                    throw new Exception("Member not found!");
                }
                db.T_Project_Stage_Members.Remove(un_member);
                db.SaveChanges();
                return Json(new object[] { true, "Update members completed!" });

            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        #endregion
        #region Version
        public JsonResult ProjectVersionSave(string name,int? order, string description, string versionId, string project_Id,bool? active)
        {
            try
            {
                var project = db.T_Project_Milestone.Find(project_Id);
                if (project?.Type != "project")
                {
                    return Json(new object[] { false, "Project not found!" });
                }

                var version = new T_Project_Milestone();
                if (string.IsNullOrEmpty(versionId))
                {
                    version = new T_Project_Milestone
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Name = name,
                        Type = "Project_version",
                        ParentId = project_Id,
                        Order = order,
                        Active = active??false,
                        Description = description,
                        UpdateAt = DateTime.UtcNow,
                        UpdateByNumber = cMem.MemberNumber,
                    };
                    db.T_Project_Milestone.Add(version);
                }
                else
                {
                    version = db.T_Project_Milestone.Find(versionId);
                    if (version?.Type != "Project_version")
                    {
                        return Json(new object[] { false, "Project version not found!" });
                    }
                    version.Name = name;
                    version.Description = description;
                    version.Active = active ?? false;
                    version.Order = order;
                    version.UpdateAt = DateTime.UtcNow;
                    version.UpdateByNumber = cMem.MemberNumber;
                    db.Entry(version).State = System.Data.Entity.EntityState.Modified;
                    db.T_Project_Milestone_Notes.RemoveRange(db.T_Project_Milestone_Notes.Where(n => n.MilestoneId == version.Id));
                };

                int i = 0;
                string title = null;
                do
                {
                    title = Request["note_title" + i];
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        string content = Request["note_content" + i];
                        int orderNote = int.Parse(Request["note_order" + i] ?? "0");
                        db.T_Project_Milestone_Notes.Add(new T_Project_Milestone_Notes { Id = Guid.NewGuid().ToString("N"), Name = title, Content = content, Order = orderNote, MilestoneId = version.Id });
                    };
                    i++;
                }
                while (title != null);
                db.SaveChanges();
                return Json(new object[] { true, string.IsNullOrEmpty(versionId)? "Create Project Version Completed!": "Update Project Version Completed!", string.IsNullOrEmpty(versionId), version });

            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message, e.ToString() });
            }

        }
        public JsonResult ProjectVersionLoad(string versionId)
        {
            var version = db.T_Project_Milestone.Find(versionId);
            if(version!=null && version.Type == "Project_version")
            {
                var notes = db.T_Project_Milestone_Notes.Where(n => n.MilestoneId == versionId).OrderBy(n=>n.Order).ToList();
                var member_name = db.P_Member.Where(m => m.MemberNumber == version.UpdateByNumber).FirstOrDefault()?.FullName;
                return Json(new object[] { true, new { version, member_name, notes } });
            }
            return Json(new object[] { false, "Project version not found" });
        }
        public JsonResult ProjectVersionDelete(string versionId)
        {
            try
            {
                var version = db.T_Project_Milestone.Find(versionId);
                if (version?.Type != "Project_version")
                {
                    return Json(new object[] { false, "Project version not found!" });
                }
                db.T_Project_Milestone.Remove(version);
                var version_members = db.T_Project_Stage_Members.Where(s => s.ProjectVersionId == versionId);
                //remove membe
                db.T_Project_Stage_Members.RemoveRange(version_members);
                //remove note
                db.T_Project_Milestone_Notes.RemoveRange(db.T_Project_Milestone_Notes.Where(n => n.MilestoneId == version.Id));
                db.SaveChanges();
                return Json(new object[] { true, "Delete Project Version Completed!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message, e.ToString() });
            }
        }
        #endregion
        public JsonResult SetLeader(string version_id, string stage_id, string member_number)
        {
            try
            {
                var stage = db.T_Project_Stage.Find(stage_id);
                if (stage == null)
                {
                    throw new Exception("Project not found!");
                }
                var project_ver = db.T_Project_Milestone.Find(version_id);
                if (project_ver == null || project_ver?.Type != "Project_version" || project_ver?.Active != true)
                {
                    throw new Exception("Version not found!");
                }
                var old_leader = db.T_Project_Stage_Members.Where(m => m.ProjectVersionId == project_ver.Id && m.StageId == stage.Id && m.IsLeader == true).FirstOrDefault();
                if (old_leader != null)
                {
                    old_leader.IsLeader = false;
                    db.Entry(old_leader).State = System.Data.Entity.EntityState.Modified;
                }
                var leader = db.T_Project_Stage_Members.Where(m => m.ProjectVersionId == project_ver.Id && m.StageId == stage.Id && m.MemberNumber == member_number).FirstOrDefault();
                if (leader == null)
                {
                    throw new Exception("Member not found!");
                }
                leader.IsLeader = true;
                db.Entry(leader).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Set Leader completed!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }

        public JsonResult UnsetLeader(string version_id, string stage_id, string member_number)
        {
            try
            {
                var stage = db.T_Project_Stage.Find(stage_id);
                if (stage == null)
                {
                    throw new Exception("Project not found!");
                }
                var project_ver = db.T_Project_Milestone.Find(version_id);
                if (project_ver == null || project_ver?.Type != "Project_version" || project_ver?.Active != true)
                {
                    throw new Exception("Version not found!");
                }
              
                var leader = db.T_Project_Stage_Members.Where(m => m.ProjectVersionId == project_ver.Id && m.StageId == stage.Id && m.MemberNumber == member_number).FirstOrDefault();
                if (leader == null)
                {
                    throw new Exception("Member not found!");
                }
                leader.IsLeader = false;
                db.Entry(leader).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Unset Leader success !" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)//
        {
            //.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
            if (AppLB.Authority.GetCurrentMember(true)?.RoleCode.Split(',').Contains("admin") == true || filterContext.ActionDescriptor.GetCustomAttributes(true).Any(em => em.GetType() == typeof(AllowAnonymousAttribute))== true)
                return;
            if (((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType.Name == "JsonResult")
            {
                filterContext.Result = new RedirectResult("forbid");
            }
            else
            {
                filterContext.Result = new RedirectResult("/home/Forbidden");
            }
        }
        public JsonResult forbid()
        {
            return new JsonResult()
            {
                Data = new object[] { false, "Permission Denied" },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


    }


}

