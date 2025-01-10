using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services.Site;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class GrantAccessController : Controller
    {
        // GET: GrantAccess
        public ActionResult Index(int? siteId, string roleCode)
        {
            WebDataModel db = new WebDataModel();
            P_Member cMem = Authority.GetCurrentMember();
            Dictionary<string, bool> access = Authority.GetAccessAuthority();
            var canAccess = cMem.RoleCode?.Contains("admin") == true || (access.Any(k => k.Key.Equals("setting_manage")) == true && access["setting_manage"] == true);
            if (!canAccess)
            {
                return Redirect("/home/forbidden");
            }
            siteId = siteId ?? cMem.SiteId;
            if (cMem.SiteId != 1) {
                siteId = cMem.SiteId;
            }
            A_Role rolesInfo =null;
            if (!string.IsNullOrEmpty(roleCode))
            {
                rolesInfo = (from map in db.SiteRoles join role in db.A_Role on map.RoleId equals role.Id where  map.SiteId == siteId && role.RoleCode == roleCode orderby role.RoleLevel descending select role).FirstOrDefault();

            }
            //get default
            if (rolesInfo == null)
            {
                rolesInfo = (from map in db.SiteRoles join role in db.A_Role on map.RoleId equals role.Id where map.SiteId == siteId orderby role.RoleLevel descending select role).FirstOrDefault();
                if (rolesInfo == null)
                {
                     return Redirect("/home/forbidden");
                }
                return RedirectToAction("index", new {siteid = siteId,rolecode = rolesInfo.RoleCode });
            }

            var mappingRole = db.SiteRoles.FirstOrDefault(x => x.RoleId == rolesInfo.Id);
            if (mappingRole != null)
            {
                siteId = mappingRole.SiteId;
            }
            else
            {
                siteId = 1;
            }
            var siteAvailables = db.Sites.Where(x => x.IsEnable == true);
            var siteService = new SiteService();
            var lst_roles = siteService.GetRolesBySiteId(siteId.Value);
            ViewBag.rolesList = lst_roles;
            var pages = db.A_Page.OrderBy(x => x.Order).ToList();
            var functions = siteService.GetFunctionBySite(siteId.Value);
          //  var grandAccess = siteService.GetFunctionBySite(siteId ?? cMem.SiteId.Value);
            ViewBag.rolesInfo = rolesInfo;
            ViewBag.sites = siteAvailables.ToList();
            ViewBag.Page = pages;
            ViewBag.SiteId = siteId.Value;
            ViewBag.Functions = functions;
            return View();
        }


        #region Change role - Save

        /// <summary>
        /// Change role
        /// </summary>
        /// <returns></returns>
        //public ActionResult ChangeRoles()
        //{
        //    try
        //    {
        //        WebDataModel db = new WebDataModel();

        //        var lst_roles = db.A_Role.OrderByDescending(r => r.RoleLevel).ToList();
        //        ViewBag.rolesList = lst_roles;

        //        var roleCode = Request["RoleCode"];
        //        var rolesInfo = lst_roles.Where(x => x.RoleCode.Equals(roleCode)).FirstOrDefault();

        //        if (rolesInfo != null)
        //        {
        //            ViewBag.rolesInfo = rolesInfo;
        //            ViewBag.Page = db.A_Page.OrderBy(x => x.Order).ToList();
        //        }
        //        else
        //        {
        //            throw new Exception();
        //        }
        //        var siteService = new SiteService();
        //        var functions = siteService.GetFunctionBySite();
        //        ViewBag.Functions = functions;
        //    }
        //    catch (Exception e)
        //    {
        //        TempData["error"] = "Error: " + e.Message;
        //    }
        //    return PartialView("_RolesAccessDetail");
        //}


        /// <summary>
        /// Save change role
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save()
        {
            var roleCode = Request["hd_role_code"];
            string roleName = Request["hd_role_name"];
            var SiteId = int.Parse(Request["siteid"]);
            try
            {
                WebDataModel db = new WebDataModel();

                var FunctionInPage = db.A_FunctionInPage.OrderBy(f => f.PageCode).ToList();
                foreach (var item in FunctionInPage)
                {
                    var accessReq = Request["chk-func-" + item.FunctionCode];
                    bool access = false;
                    bool.TryParse(accessReq, out access);

                    A_GrandAccess GrandAccess = db.A_GrandAccess.Where(fr => fr.FunctionCode.Equals(item.FunctionCode) == true && fr.RoleCode == roleCode).FirstOrDefault();
                    if (GrandAccess == null)
                    {
                        GrandAccess = new A_GrandAccess { FunctionCode = item.FunctionCode, FunctionName = item.FunctionName, RoleCode = roleCode, RoleName = roleName, Access = access };
                        db.A_GrandAccess.Add(GrandAccess);
                    }
                    else
                    {
                        GrandAccess.Access = access;
                        db.Entry(GrandAccess).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();

                }

                TempData["success"] = "Save successfully";
            }
            catch (Exception e)
            {
                TempData["error"] = "Error: " + e.Message;
            }
            return RedirectToAction("index", new { rolecode = roleCode, siteid = SiteId });
        }

        #endregion


        #region Role management

        public JsonResult GetRoleInfo(string RoleCode)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var role = db.A_Role.Where(r => r.RoleCode.Equals(RoleCode)).FirstOrDefault();
                if (role == null)
                {
                    throw new Exception("Access role does not exist.");
                }
                else
                {
                    var list_role = db.A_Role.Where(r => r.RoleCode != role.RoleCode).OrderBy(r => r.RoleLevel).ToList();
                    return Json(new object[] { true, role, list_role }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { true, "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public ActionResult SaveRole()
        {
            try
            {
                var RoleCode = Request["hd_rolde_code"];
                var RoleName = Request["RoleName"];
                var RoleLevel = int.Parse(Request["RoleLevel"]);
                var SiteId = int.Parse(Request["SiteId"]);
                bool IsForPartner;
                if (!bool.TryParse(Request["IsForPartner"], out IsForPartner))
                {
                    IsForPartner = false;
                }
                var Description = Request["Description"];
                var _roleCode = "";
                WebDataModel db = new WebDataModel();
                if (string.IsNullOrEmpty(RoleCode) == true) //Add new
                {
                    var role = new A_Role
                    {
                        RoleCode = Guid.NewGuid().ToString("N"),
                        RoleName = RoleName,
                        RoleLevel = RoleLevel,
                        IsForPartner = IsForPartner,
                        Description = Description
                    };
                    db.A_Role.Add(role);
                    RoleCode = role.RoleCode;
                    db.SaveChanges();
                    // add mapping
                    var siteMappingRole = new SiteRole();
                    siteMappingRole.RoleId = role.Id;
                    siteMappingRole.SiteId = SiteId;
                    db.SiteRoles.Add(siteMappingRole);
                    db.SaveChanges();

                }
                else //Edit
                {
                    var role = db.A_Role.Where(r => r.RoleCode.Equals(RoleCode)).FirstOrDefault();
                    if (role == null)
                    {
                        throw new Exception("Access role does not exist.");
                    }
                    role.RoleName = RoleName;
                    role.RoleLevel = RoleLevel;
                    role.Description = Description;
                    role.IsForPartner = IsForPartner;
                    db.Entry(role).State = System.Data.Entity.EntityState.Modified;
                    _roleCode = role.RoleCode;
                }

                db.SaveChanges();
                if (string.IsNullOrEmpty(_roleCode) == true)
                {
                    TempData["success"] = "Add new access role success!";
                }
                else
                {
                    TempData["success"] = "Edit access role success!";
                }

                return RedirectToAction("index", new { RoleCode = RoleCode,siteId = SiteId });
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error: " + ex.Message;
                return RedirectToAction("index");
            }
        }
        public ActionResult DeleteRole()
        {

            var RoleCodeDelete = Request["hd_rolde_code_delete"];
            var RoleNameDelete = Request["RoleNameDelete"];
            var select_NewRoleCode = Request["select_new_access_role"];

            WebDataModel db = new WebDataModel();

            using (var TranS = db.Database.BeginTransaction())
            {
                try
                {
                    //Lay thong tin role duoc chon de thay the role cu
                    var select_NewRole = db.A_Role.Where(r => r.RoleCode.Equals(select_NewRoleCode)).FirstOrDefault();

                    //Lay ra ds member thuoc role bi xoa va update lai role
                    var list_member_of_role_delete = db.P_Member.Where(m => m.RoleCode.Contains(RoleCodeDelete)).ToList();
                    if (list_member_of_role_delete != null)
                    {
                        foreach (var item in list_member_of_role_delete)
                        {
                            //Update role code
                            var arr_role_code = item.RoleCode.Split(',');
                            var role_code = "";
                            for (int i = 0; i < arr_role_code.Length; i++)
                            {
                                if (arr_role_code[i].Trim() != RoleCodeDelete)
                                {
                                    role_code = role_code + arr_role_code[i].Trim() + ",";
                                }
                            }
                            item.RoleCode = role_code + select_NewRole.RoleCode;


                            //Update role name
                            var arr_role_name = item.RoleName.Split(',');
                            var role_name = "";
                            for (int i = 0; i < arr_role_name.Length; i++)
                            {
                                if (arr_role_name[i].Trim() != RoleNameDelete)
                                {
                                    role_name = role_name + arr_role_name[i].Trim() + ",";
                                }
                            }
                            item.RoleName = role_name + select_NewRole.RoleName;

                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    //Delete role
                    var RoleDelete = db.A_Role.Where(r => r.RoleCode.Equals(RoleCodeDelete)).FirstOrDefault();
                    db.A_Role.Remove(RoleDelete);

                    db.SaveChanges();
                    TranS.Commit();
                    TranS.Dispose();

                    TempData["success"] = "Delete role success!";
                    return RedirectToAction("index");
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Error: " + ex.Message;
                    return RedirectToAction("index");
                }
            }

        }


        #endregion

    }
}