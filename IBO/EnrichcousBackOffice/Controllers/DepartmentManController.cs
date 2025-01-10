using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.CustomizeModel.Project;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class DepartmentManController : Controller
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();

        // GET: DepartmentMan
        public ActionResult Index(string urlback)
        {
            //check access
            if (access.Any(k => k.Key.Equals("department_access")) == false || access["department_access"] != true)
            {
                return Redirect("/home/forbidden");
            }

            ViewBag.p = access;
            //end check
            if (string.IsNullOrWhiteSpace(urlback))
            {
                urlback = "/memberman/";
            }
            ViewBag.urlback = urlback;

            #region Search Option

            if (HttpContext.Request.Url.Query == "")
            {
                //TempData.Clear();
                TempData.Remove("search_name");
            }

            if (Request["search"] != null)
            {
                TempData["search_name"] = Request["search"];
            }
            var search_name = Request["search"] == null ? null : AppLB.CommonFunc.ConvertNonUnicodeURL(Request["search"]).ToUpper();

            #endregion

            WebDataModel db = new WebDataModel();

            var list_department = db.P_Department.Where(d => d.ParentDepartmentId == null && d.SiteId== cMem.SiteId).OrderByDescending(d => new { d.Active, d.Id }).ToList();

            if (string.IsNullOrEmpty(search_name) == false)
            {
                list_department = list_department.Where(d => d.Name.ToUpper().Contains(search_name)).ToList();

                var list_group = db.P_Department.Where(d => d.ParentDepartmentId != null && d.Type != null).ToList();
                foreach (var item in list_group)
                {
                    if (item.Name.ToUpper().Contains(search_name) == true)
                    {
                        var department = db.P_Department.Where(d => d.Name == item.ParentDepartmentName).FirstOrDefault();
                        if (list_department.Any(d => d.Id == department.Id) == false)
                        {
                            list_department.Add(department);
                        }
                    }
                }
            }

            ViewBag.ListEmployees = db.P_Member.Where(m =>  m.Active == true && m.SiteId==cMem.SiteId).Select(m => new MemberSelect_View { MemberNumber = m.MemberNumber, Name = m.FullName, Department = m.DepartmentId }).ToList();
            ViewBag.Partner = db.C_Partner.Where(x => x.Status == 1).ToList();
            return View(list_department);
        }

        /// <summary>
        /// Get department infomation
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetDepartmentInfo(long? id)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var department = db.P_Department.Find(id);
                if (department == null)
                {
                    throw new Exception("Department does not exist.");
                }
                string _id = id.ToString();
                var list_director = db.P_Member.Where(m => m.MemberType.Equals("emp") && m.Active == true && m.DepartmentId.Contains(_id) == true).ToList();

                return Json(new object[] { true, department, list_director }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult ReloadDepartment(long? id)
        {
            WebDataModel db = new WebDataModel();
            var department = db.P_Department.Where(x => x.Id == id).FirstOrDefault();
            ViewBag.p = access;
            if (department.Type == "DEVELOPMENT")
            {
                var ProjectDevelopment = db.T_Project_Milestone.Where(x => x.Type == "project" && x.BuildInCode == "Development_Ticket" && x.Active==true).ToList();
                ViewBag.ProjectDevelopment = ProjectDevelopment;
            }
            return PartialView("_Department", department);
        }
        /// <summary>
        /// Save department
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                string message = "";
                var id = long.Parse(Request["hd_d_id"]);
                var name = Request["d_name"];
                var director_mn = Request["d_director"];
                var type = Request["d_type"];
                var list_director_name = "";
                var active = Request["Active"] == "1" ? true : false;

                if (string.IsNullOrEmpty(director_mn) == false)
                {
                    var list_mn = director_mn.Split(',');
                    for (int i = 0; i < list_mn.Length; i++)
                    {
                        var memNumber = list_mn[i];
                        var director_name = db.P_Member.Where(m => m.MemberNumber == memNumber).FirstOrDefault()?.FullName;
                        if (i == (list_mn.Length - 1))
                        {
                            list_director_name = list_director_name + director_name;
                        }
                        else
                        {
                            list_director_name = list_director_name + director_name + ",";
                        }
                    }
                }
                if (string.IsNullOrEmpty(name) == true)
                {
                    throw new Exception("Department name is required.");
                }
                if (id > 0)
                {
                    //check access
                    if (access.Any(k => k.Key.Equals("department_update")) == false || access["department_update"] != true)
                    {
                        return Redirect("/home/forbidden");
                    }
                    ViewBag.p = access;
                    //end check
                    var department = db.P_Department.Find(id);
                    if (db.P_Department.Any(d => d.Name == name && d.SiteId == cMem.SiteId) && department.Name != name)
                    {
                        throw new Exception("Department name already exists.");
                    }
                    department.Name = name;
                    department.LeaderNumber = director_mn;
                    department.LeaderName = list_director_name;
                    department.Active = active;
                    department.Type = type;
                    department.UpdateBy = cMem.FullName;
                    department.UpdateAt = DateTime.UtcNow;

                    db.Entry(department).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    message = "Edit department success!";
                    return Json(new { status = true,action="edit", message, Department = department });
                }
                else
                {
                    //add new department

                    //check access
                    if (access.Any(k => k.Key.Equals("department_addnew")) == false || access["department_addnew"] != true)
                    {
                        return Json(new { status = false, message = "access is denied"});
                    }
                    ViewBag.p = access;
                    //end check
                    if (db.P_Department.Any(d => d.Name == name && d.SiteId == cMem.SiteId))
                    {
                        throw new Exception("Department name already exists.");
                    }
                    var department = new P_Department();
                    department.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + db.P_Department.Where(d => d.CreateAt.Value.Year == DateTime.Today.Year && d.CreateAt.Value.Month == DateTime.Today.Month).Count().ToString().PadLeft(3, '0') + (new Random().Next(1, 9).ToString()));
                    department.Name = name;
                    department.LeaderNumber = director_mn;
                    department.LeaderName = list_director_name;
                    department.Active = active;
                    department.CreateBy = cMem.FullName;
                    department.Type = type;
                    department.SiteId = cMem.SiteId;
                    department.CreateAt = DateTime.UtcNow;

                    db.P_Department.Add(department);
                    db.SaveChanges();
                    message = "Add new department success!";
                    return Json(new { status = true, action = "add", message, Department = department });
                }
              
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message= ex.Message });
            }

          
        }

        /// <summary>
        /// Delete department
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(long? id)
        {
            try
            {
                //check access
                if (access.Any(k => k.Key.Equals("department_delete")) == false || access["department_delete"] != true)
                {
                    return Redirect("/home/forbidden");
                }

                ViewBag.p = access;
                //end check

                WebDataModel db = new WebDataModel();
                var department = db.P_Department.Find(id);
                if (department != null)
                {
                    //if (department.Active == true)
                    //{
                    //    department.Active = false;
                    //    db.Entry(department).State = System.Data.Entity.EntityState.Modified;
                    //    TempData["success"] = "Inactive department success!";
                    //}
                    //else
                    //{
                        db.P_Department.Remove(department);
                        TempData["success"] = "Delete department success!";
                    //}
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("index");
        }



        #region Group

   
        /// <summary>
        /// View Group Detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetGroupDetail(long? id)
        {
            try
            {
                //check access
                if (access.Any(k => k.Key.Equals("department_access")) == false || access["department_access"] != true)
                {
                    throw new Exception("Access denied");
                }
                //end check

                WebDataModel db = new WebDataModel();
                var group = db.P_Department.Find(id);

                var list_mem = new List<MemberSelect_View>();
                if (group != null && (string.IsNullOrEmpty(group.GroupMemberNumber) == false))
                {
                    var list_memNumber = group.GroupMemberNumber.Split(',');
                    list_mem = db.P_Member.Where(m => list_memNumber.Contains(m.MemberNumber) && m.Active == true).Select(m => new MemberSelect_View { MemberNumber = m.MemberNumber, Name = m.FullName, PersonalEmail = m.PersonalEmail, CellPhone= m.CellPhone }).ToList();
                }
                var list_state = new List<Ad_USAState>();
                if(group.ParentDepartmentId == 19120010 && !string.IsNullOrEmpty(group.SaleStates))
                {
                    var state = group.SaleStates.Split(',');
                    list_state = db.Ad_USAState.Where(s => state.Contains(s.abbreviation)).ToList();
                    if (state.Contains("Other"))
                    {
                        list_state.Add(new Ad_USAState { abbreviation = "Other", name = "Include undefined" });
                    }
                }
                return Json(new object[] { true, group, list_mem, list_state }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Get group info
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult GetGroupInfo(long? id)
        {
            try
            {
                //check access
                if (access.Any(k => k.Key.Equals("department_update")) == false || access["department_update"] != true)
                {
                    throw new Exception("Access denied");
                }
                //end check

                WebDataModel db = new WebDataModel();
                var group = db.P_Department.Find(id);
                if (group == null)
                {
                    throw new Exception("Group does not exist.");
                }

                return Json(new object[] { true, group }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Save Group
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveGroup()
        {
            try
            {
                WebDataModel db = new WebDataModel();

                var department_id = long.Parse(Request["hd_dg_id"]);
                var department_name = Request["dg_name"];

                var group_id = long.Parse(Request["hd_g_id"]);
                var group_name = Request["g_name"];
                var list_mem_mn = Request["g_member"] ?? "";//list memberNumber of members in group
                var leader_id = Request["g_leader"];
                var leaders = new List<P_Member>();
                if (!string.IsNullOrEmpty(leader_id))
                {
                    var leaderIds = leader_id.Split(',');
                     leaders = db.P_Member.Where(m => leaderIds.Any(x=>x==m.MemberNumber)).ToList();
                }
               
                var manager_id = Request["g_manager"];
                var manager = db.P_Member.Where(m => m.MemberNumber == manager_id).FirstOrDefault();
                var active = Request["g_active"] == "1" ? true : false;
                var list_mn = list_mem_mn.Split(',');
                var listmem = db.P_Member.Where(m => list_mn.Contains(m.MemberNumber)).ToList();
                var sale_states = Request["g_states"];

                if (string.IsNullOrEmpty(group_name) == true)
                {
                    throw new Exception("Group name is required.");
                }

                if (group_id > 0)
                {
                    //edit group

                    //check access
                    if (access.Any(k => k.Key.Equals("department_update")) == false || access["department_update"] != true)
                    {
                        return Redirect("/home/forbidden");
                    }

                    ViewBag.p = access;
                    //end check

                    var group = db.P_Department.Find(group_id);

                    if (db.P_Department.Any(d => d.ParentDepartmentId == department_id && d.Name == group_name) == true && group.Name != group_name)
                    {
                        throw new Exception("Group name already exists.");
                    }
                    group.Name = group_name;
                    //group.ParentDepartmentId = department_id;
                    //group.ParentDepartmentName = department_name;
                    group.GroupMemberNumber = string.Join(",", listmem.Select(m => m.MemberNumber));
                    group.GroupMemberName = string.Join(",", listmem.Select(m => m.FullName));
                    group.Active = active;
                    group.UpdateBy = cMem.FullName;
                    group.UpdateAt = DateTime.UtcNow;
                    group.LeaderName = string.Join(",",leaders.Select(x=>x.FullName));
                    group.Type = db.P_Department.Find(group.ParentDepartmentId)?.Type;
                    //if (cMem.SiteId == 1)
                    //{
                    //    if (Request["PartnerCode"] != null)
                    //    {
                    //        if (Request["PartnerCode"] == "All")
                    //        {
                    //            group.PartnerCode = "All";
                    //            group.PartnerName = "All";
                    //        }
                    //        else
                    //        {
                    //            var partner = db.C_Partner.Where(x => x.Code == group.PartnerCode).FirstOrDefault();
                    //            if (partner != null)
                    //            {
                    //                group.PartnerCode = partner.Code;
                    //                group.PartnerName = partner.Name;
                    //                group.SiteId = partner.SiteId;
                    //            }
                    //        }
                    //    }
                    //    else
                    //    {
                    //        group.PartnerCode = null;
                    //        group.PartnerName = null;
                    //        group.SiteId = null;
                    //    }
                    //}
                    group.LeaderNumber = string.Join(",", leaders.Select(x => x.MemberNumber));
                    group.SupervisorName = manager?.FullName;
                    group.SupervisorNumber = manager?.MemberNumber;
                  
                    if (group.ParentDepartmentId == 19120010)
                    {
                        group.SaleStates = sale_states;
                    }
                    else
                    {
                        group.SaleStates = string.Empty;
                    }
                    db.Entry(group).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
          
                    return Json(new { status = true, message = "Edit group success!", departmentId = group.ParentDepartmentId });
                }
                else
                {
                    //add new group

                    //check access
                    if (access.Any(k => k.Key.Equals("department_addnew")) == false || access["department_addnew"] != true)
                    {
                        return Redirect("/home/forbidden");
                    }
                    ViewBag.p = access;
                    //end check
                    if (db.P_Department.Any(d => d.ParentDepartmentId == department_id && d.Name == group_name) == true)
                    {
                        throw new Exception("Group name already exists.");
                    }
                    var group = new P_Department();
                    group.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + db.P_Department.Where(d => d.CreateAt.Value.Year == DateTime.Today.Year && d.CreateAt.Value.Month == DateTime.Today.Month).Count().ToString().PadLeft(3, '0') + (new Random().Next(1, 9).ToString()));
                    group.Name = group_name;
                    group.ParentDepartmentId = department_id;
                    group.ParentDepartmentName = department_name;
                    group.GroupMemberNumber = string.Join(",", listmem.Select(m => m.MemberNumber));
                    group.GroupMemberName = string.Join(",", listmem.Select(m => m.FullName));
                    group.Type = db.P_Department.Find(department_id)?.Type;
                    if (cMem.SiteId == 1)
                    {
                        if (Request["PartnerCode"] != null)
                        {
                            if (Request["PartnerCode"] == "All")
                            {
                                group.PartnerCode = "All";
                                group.PartnerName = "All";
                                group.SiteId = 1;
                            }
                            else
                            {
                                var partner = db.C_Partner.Where(x => x.Code == group.PartnerCode).FirstOrDefault();
                                if (partner != null)
                                {
                                    group.PartnerCode = partner.Code;
                                    group.PartnerName = partner.Name;
                                    group.SiteId = partner.SiteId;
                                }
                            }
                        }
                        else
                        {
                            group.PartnerCode = null;
                            group.PartnerName = null;
                            group.SiteId = null;
                        }
                    }
                    else
                    {
                        var currentPartner = db.C_Partner.Where(x => x.SiteId== cMem.SiteId).FirstOrDefault();
                        if (currentPartner != null)
                        {
                            group.PartnerCode = currentPartner.Code;
                            group.PartnerName = currentPartner.Name;
                            group.SiteId = currentPartner.SiteId;
                        }
                    }
                    group.Active = active;
                    group.CreateBy = cMem.FullName;
                    group.CreateAt = DateTime.UtcNow;
                    if (group.ParentDepartmentId == 19120010)
                    {
                        group.SaleStates = sale_states;
                    }
                    else
                    {
                        group.SaleStates = string.Empty;
                    }
                    group.LeaderName = string.Join(",", leaders.Select(x => x.FullName));
                    group.LeaderNumber = string.Join(",", leaders.Select(x => x.MemberNumber));
                    group.SupervisorName = manager?.FullName;
                    group.SupervisorNumber = manager?.MemberNumber;
                    db.P_Department.Add(group);
                    db.SaveChanges();
                
                    return Json(new { status = true, message= "Add new group success!", departmentId = group.ParentDepartmentId });
                }
            }
            catch (Exception ex)
            {

                return Json(new { status = false, message = ex.Message});
            }

          
        }

        /// <summary>
        /// Delete Group
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteGroup(long? id)
        {
            try
            {
                //check access
                if (access.Any(k => k.Key.Equals("department_delete")) == false || access["department_delete"] != true)
                {
                    return Json(new { status = false, message = "access is denied!" });
                }
                ViewBag.p = access;
                //end check
                WebDataModel db = new WebDataModel();
                var department = db.P_Department.Find(id);
                if (department != null)
                {
                    db.P_Department.Remove(department); 
                    db.SaveChanges();
                }
                return Json(new { status = true, message = "delete group success!",departmentId=department.ParentDepartmentId });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "delete group error!"});
            }

         
        }

        #endregion
        public JsonResult GetState()
        {
            var db = new WebDataModel();
            return Json(new object[] { true, db.Ad_USAState.ToList() });
        }

        #region project management  
        public ActionResult DetailProject(string projectId)
        {
            var db = new WebDataModel();
            var project = db.T_Project_Milestone.Find(projectId);
            if (project == null)
            {
                throw new ArgumentException("project not found");
            }
            var model = new ProjectDetailModel();
            model.ProjectName = project.Name;
            model.ProjectId = project.Id;
            var versions = db.T_Project_Milestone.Where(x => x.ParentId == project.Id  && x.Type == "Project_version").ToList();
            model.Versions = versions;
            var stages = db.T_Project_Stage.Where(x => x.ProjectId == project.Id).ToList();
            model.Stages = stages;
            ViewBag.member = (from projectmember in db.T_Project_Milestone_Member join member in db.P_Member on projectmember.MemberNumber equals member.MemberNumber where projectmember.ProjectId == projectId && member.Delete != true && member.Active != false select member).ToList();
            //ViewBag.deparments = (from depa in db.P_Department.AsEnumerable()
            //                      where depa.ParentDepartmentId == null && depa.Active == true
            //                      join gs in (from g in db.P_Department where g.ParentDepartmentId != null group g by g.ParentDepartmentId into gg select gg)
            //                      on depa.Id equals gs.Key
            //                      select new P_Department
            //                      {
            //                          Id = depa.Id,
            //                          Name = depa.Name,
            //                          GroupMemberNumber = depa.GroupMemberNumber + "," + string.Join(",", gs.Select(g => g.GroupMemberNumber)),
            //                      }).ToList();
            return PartialView("_ProjectDetail", model);
        }
        public ActionResult getMembers(string stage_id)
        {
            var db = new WebDataModel();
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
        #endregion
    }
}