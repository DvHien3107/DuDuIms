using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Threading.Tasks;
using DataTables.AspNet.Core;
using EnrichcousBackOffice.Models.CustomizeModel;
using Google.Apis.Auth.OAuth2.Mvc;
using EnrichcousBackOffice.AppLB.GoogleAuthorize;
using System.Threading;
using System.Web.Configuration;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using EnrichcousBackOffice.Services;
using System.Data.Entity;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.Util;
using EnrichcousBackOffice.Models.CustomizeModel.Employees;
using Enrich.Core.Ultils;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class MemberManController : UploadController
    {
        private readonly Dictionary<string, bool> access;
        private readonly P_Member cMem;
        private readonly IMailingService _mailingService;
        private readonly ILogService _logService;

        public MemberManController(IMailingService mailingService, ILogService logService)
        {
            _mailingService = mailingService;
            _logService = logService;
            access = AppLB.Authority.GetAccessAuthority();
            cMem = AppLB.Authority.GetCurrentMember();
        }

        // GET: MemberMan
        public ActionResult Index()
        {
            if (access.Any(k => k.Key.Equals("partners_accessmember")) == false || access["partners_accessmember"] != true)
            {
                return Redirect("/home/forbidden");
            }



            WebDataModel db = new WebDataModel();
            ViewBag.p = access;
            ViewBag.listMemberType = db.P_MemberType.Where(m => m.MemberType.IndexOf("partner") < 0).ToList();
            //chi admin co the view all.
            //cac quyen khac chi co the xem member cung department.
            ViewBag.listDepartment = db.P_Department.AsEnumerable().Where(d => d.Active == true && (cMem.RoleCode.Contains("admin") == true || (cMem.DepartmentId !=null && cMem.DepartmentId.Contains(d.Id.ToString()) == true && d.ParentDepartmentId == null))).ToList();
            return View();
        }

        public ActionResult GetListMember(IDataTablesRequest dataTablesRequest,DateTime? FromDate,DateTime? ToDate,bool? s_completeprofile, string s_search_text, string s_member_type, string s_department, int? siteId, string s_active,string s_workstatus)
        {
            _logService.Info($"[MemberMan][GetListMember] start s_search_text:{s_search_text} - s_member_type:{s_member_type} - s_department:{s_department} - siteId:{siteId} - s_active:{s_active}");

            try
            {

                WebDataModel db = new WebDataModel();
                var members = from m in db.P_Member where m.Delete != true select m;
                if (!string.IsNullOrEmpty(s_search_text))
                {
                    members = members.Where(x =>
                    x.FullName.Contains(s_search_text)
                    || x.CellPhone.Contains(s_search_text)
                      || x.Email1.Contains(s_search_text)
                        || x.PersonalEmail.Contains(s_search_text)
                          || x.EmployeeId.Contains(s_search_text)
                           || x.MemberNumber.Contains(s_search_text)
                    );
                }

                if (FromDate != null)
                {
                    var From = FromDate.Value.Date + new TimeSpan(0, 0, 0);
                    members = members.Where(x => x.CreateAt >= From);
                }
                if (ToDate != null)
                {
                    var To = ToDate.Value.Date + new TimeSpan(23, 59, 59);
                    members = members.Where(x => x.CreateAt <= To);
                }
                if (s_completeprofile != null)
                {
                    members = members.Where(x => (x.IsCompleteProfile ?? false) == s_completeprofile);
                }
                if (!string.IsNullOrEmpty(s_member_type))
                {
                    members = members.Where(x => x.MemberType == s_member_type);

                }
                if (!string.IsNullOrEmpty(s_department))
                {
                    members = members.Where(x => x.DepartmentId.Contains(s_department));

                }

                if (cMem.SiteId!= 1)
                {
                    members = members.Where(x => x.SiteId == cMem.SiteId);
                }
                else if (siteId!=null)
                {
                    members = members.Where(x => x.SiteId == siteId);
                }

                if (!string.IsNullOrEmpty(s_active))
                {
                    if (s_active == "active")
                    {
                        members = members.Where(x => x.Active != false);
                    }
                    else
                    {
                        members = members.Where(x => x.Active == false);
                    }

                }
                if (!string.IsNullOrEmpty(s_workstatus))
                {
                    if (s_workstatus == "Working")
                    {
                        members = members.Where(x => x.EmployeeStatus == s_workstatus || string.IsNullOrEmpty(x.EmployeeStatus));
                    }
                    else
                    {
                        members = members.Where(x => x.EmployeeStatus == s_workstatus);
                    }

                }

                // check is partner
                //if (!string.IsNullOrEmpty(cMem.BelongToPartner))
                //{
                //    members = members.Where(x => x.BelongToPartner == cMem.BelongToPartner);
                //}

                //sort
                var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
                switch (colSort.Name)
                {
                    case "EmployeeId":
                        if (colSort.Sort.Direction == SortDirection.Ascending)
                        {
                            members = members.OrderBy(s => s.EmployeeId);
                        }
                        else
                        {
                            members = members.OrderByDescending(s => s.EmployeeId);
                        }
                        break;
                    case "MemberNumber":
                        if (colSort.Sort.Direction == SortDirection.Ascending)
                        {
                            members = members.OrderBy(s => s.MemberNumber);
                        }
                        else
                        {
                            members = members.OrderByDescending(s => s.MemberNumber);
                        }
                        break;

                    case "FullName":
                        if (colSort.Sort.Direction == SortDirection.Ascending)
                        {
                            members = members.OrderBy(s => s.FullName);
                        }
                        else
                        {
                            members = members.OrderByDescending(s => s.FullName);
                        }
                        break;

                    case "Department":
                        if (colSort.Sort.Direction == SortDirection.Ascending)
                        {
                            members = members.OrderBy(s => s.DepartmentName);
                        }
                        else
                        {
                            members = members.OrderByDescending(s => s.DepartmentName);
                        }
                        break;
                    case "CreateAt":
                        if (colSort.Sort.Direction == SortDirection.Ascending)
                        {
                            members = members.OrderBy(s => s.CreateAt);
                        }
                        else
                        {
                            members = members.OrderByDescending(s => s.CreateAt);
                        }
                        break;

                    default:
                        members = members.OrderByDescending(s => s.MemberNumber);
                        break;
                }
                var recordsTotal = members.Count();
                members = members.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
                var memberView = members.ToList().Select(m =>
                {
                    var item = new MemberMan_View();
                    item.MemberNumber = m.MemberNumber;
                    item.EmployeeId = m.EmployeeId ?? "";
                    item.DepartmentName = m.DepartmentName;
                    item.EmployeeStatus = m.EmployeeStatus ?? "Working";
                    item.FullName = m.FullName;
                    item.CellPhone = m.CellPhone ?? "";
                    item.PersonalEmail = m.PersonalEmail;
                    item.CreateAt = m.CreateAt.Value.ToString("MMM dd, yyyy hh:mm tt");
                    item.CreateBy = m.CreateBy;
                    item.Delete = m.Delete;
                    item.ReferedByName = m.ReferedByName;
                    item.Active = m.Active;
                    item.MaxRoleLevel = db.A_Role.Where(r => m.RoleCode.Contains(r.RoleCode)).ToList().Max(r => r.RoleLevel) ?? 0;
                    item.Avatar = m.Picture;
                    item.Gender = m.Gender;
                    item.GoogleAuth = m.IsAuthorizedGoogle ?? false;
                    item.IsSendEmailGoogleAuth = m.IsSendEmailGoogleAuth ?? false;
                    item.IsCompleteProfile = m.IsCompleteProfile ?? false;
                    item.BelongToPartner = db.Sites.FirstOrDefault(x => x.Id == m.SiteId)?.Name;
                    return item;
                }).ToList();
    
            return Json(new
            {
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                draw = dataTablesRequest.Draw,
                data = memberView.ToList()
            });
            }
            catch(Exception ex)
            {
                _logService.Error(ex, $"[MemberMan][GetListMember] error s_search_text:{s_search_text} - s_member_type:{s_member_type} - s_department:{s_department} - siteId:{siteId} - s_active:{s_active}");
                throw;
            }
        }

        #region Member

        /// <summary>
        /// add new/update member
        /// </summary>
        /// <param name="mn"></param>
        /// <returns></returns>
        public ActionResult Save(string mn)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partners_updatemember")) == false || access["partners_updatemember"] != true)
                {
                    return Redirect("/home/forbidden");
                }
                ViewBag.p = access;

                WebDataModel db = new WebDataModel();
                var member = db.P_Member.Where(m => m.MemberNumber.Equals(mn)).FirstOrDefault();

                if (member == null)
                {
                    return RedirectToAction("index");
                }

                var ct = from c in db.Ad_Country
                         select new
                         {
                             Country = c.Name,
                             Name = c.Name
                         };

                ViewBag.MoreFiles = db.UploadMoreFiles.Where(f => f.TableId == member.Id && f.TableName.Equals("P_Member")).ToList();
                ViewBag.ListEmployees = db.P_Member.Where(m => m.Id != member.Id && m.MemberType.Contains("distributor") && m.Active == true).ToList();
                ViewBag.ListMemberType = db.P_MemberType.ToList();
                ViewBag.ListDepartment = db.P_Department.Where(d => d.ParentDepartmentId == null).ToList();
                ViewBag.ListLevel = db.P_Level.OrderBy(l => l.Level).ToList();
                ViewBag.MemLevel = db.P_MemberLevel.Where(l => l.MemberNumber == member.MemberNumber).FirstOrDefault();
                ViewBag.Sites = db.Sites.Where(c => c.IsEnable==true).ToList();
                return View(member);
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                return RedirectToAction("index");
            }
        }

        /// <summary>
        /// Update member
        /// </summary>
        /// <param name="memModal"></param>
        /// <returns></returns>
        /// 
        public ActionResult GetTab(string Tab, string MemberNumber)
        {
            WebDataModel db = new WebDataModel();
            var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == MemberNumber);
            switch (Tab)
            {
                case "information":
                    if (!string.IsNullOrEmpty(member.Country))
                    {
                        ViewBag.Country = db.Ad_Country.FirstOrDefault(x => x.CountryCode == member.Country)?.Name;
                    }
                    if (!string.IsNullOrEmpty(member.State))
                    {
                        ViewBag.State = db.Ad_Provinces.FirstOrDefault(x => x.Code == member.State)?.Name;
                    }
                    ViewBag.SiteName = db.Sites.FirstOrDefault(x => x.Id == member.SiteId)?.Name;
                    return PartialView("_MemberInfomationTab", member);
                case "bankcard":
                    if (access.Any(k => k.Key.Equals("partners_accessmember")) == false || access["partners_accessmember"] != true)
                    {
                        return Content("access is denied");
                    }
                    return PartialView("_MemberBankCard", member);
                case "payroll":
                    var Payslip = db.P_PayrollFiles.Where(x => x.MemberNumber == member.MemberNumber && x.IsApproved == true && x.IsActive == true).ToList();
                    return PartialView("_MemberPayrollTab", Payslip);
                case "schedule":
                    return PartialView("_MemberSchedulerTab", member);
                case "attachment":
                    return PartialView("_MemberAttachmentTab", member);
                default:
                    return PartialView("_MemberInfomationTab", member);
            }
        }
        [HttpGet]
        public ActionResult GetPopUpInsertOrUpdateMember(string MemberNumber,int? SiteId)
        {
            WebDataModel db = new WebDataModel();
            var member = new P_Member();
            if (!string.IsNullOrEmpty(MemberNumber))
            {
                member = db.P_Member.Where(m => m.MemberNumber.Equals(MemberNumber)).FirstOrDefault();

                if (member == null)
                {
                    return RedirectToAction("index");
                }
            }
            else
            {

                member.SiteId = SiteId?? cMem.SiteId;
                member.Active = true;
                member.MemberType = "emp";
            }

            if (string.IsNullOrEmpty(member.Country))
            {
                member.Country = "US";
            }

            var ct = from c in db.Ad_Country.Where(x=>x.CountryCode =="US"||x.CountryCode=="VN")
                     select new
                     {
                         Country = c.CountryCode,
                         Name = c.Name
                     };
            var provinces = db.Ad_Provinces.Where(x => x.CountryId == member.Country).ToList();
            // ViewBag.MoreFiles = db.UploadMoreFiles.Where(f => f.TableId == member.Id && f.TableName.Equals("P_Member")).ToList();
            ViewBag.Provinces =  new SelectList(provinces, "Code", "Name");
            ViewBag.Countries = new SelectList(ct, "Country", "Name");
            ViewBag.ListEmployees = db.P_Member.Where(m =>m.MemberType.Contains("distributor") && m.Active == true).ToList();
            ViewBag.ListMemberType = db.P_MemberType.ToList();
            ViewBag.ListDepartment = db.P_Department.Where(d => d.ParentDepartmentId == null && d.SiteId == cMem.SiteId).ToList();
            var allListRole = new List<A_Role>();
            if (member.SiteId != null)
            {
                 allListRole = (from siteRole in db.SiteRoles join r in db.A_Role on siteRole.RoleId equals r.Id where siteRole.SiteId == member.SiteId orderby r.RoleLevel descending select r).ToList();
            }
          
            ViewBag.ListRole = allListRole;
            ViewBag.ListLevel = db.P_Level.OrderBy(l => l.Level).ToList();
            // ViewBag.MemLevel = db.P_MemberLevel.Where(l => l.MemberNumber == member.MemberNumber).FirstOrDefault();
            ViewBag.Sites = db.Sites.Where(c => c.IsEnable == true).ToList();
            return PartialView("_MemberInsertOrUpdatePopup", member);
        }
        [HttpPost]
        public async Task<ActionResult> UpdateMember(P_Member memModal,string command)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partners_updatemember")) == false || access["partners_updatemember"] != true)
                {
                    return Json(new { status = false, message = "access is denied" });
                }
                WebDataModel db = new WebDataModel();
                string BelongToParner = null;
                // if user create or update is same partner set belong to partner equal same code
                if (cMem.SiteId !=1)
                {
                    memModal.SiteId = cMem.SiteId;
                    var site = db.Sites.FirstOrDefault(x=>x.Id == cMem.SiteId);
                    if (site == null)
                    {
                        throw new Exception("Site Value is null");
                    }
                    if (site != null)
                    {
                        memModal.BelongToPartner = site.Code;
                    }
                }
                else
                {
                    var site = db.Sites.FirstOrDefault(x => x.Id == memModal.SiteId);
                    if (site == null)
                    {
                        throw new Exception("Site Value is null");
                    }
                    if (site.Id !=1)
                    {
                        memModal.BelongToPartner = site.Code;
                    }
                    else
                    {
                        memModal.BelongToPartner = null;
                    }
                } 
                if (memModal.Id > 0)
                {
                    var member = db.P_Member.Find(memModal.Id);
                    var checkExistEmail = db.P_Member.Any(m => m.PersonalEmail.Equals(memModal.PersonalEmail, StringComparison.InvariantCultureIgnoreCase) == true && m.MemberNumber != member.MemberNumber && m.Delete != true);
                    if (checkExistEmail)
                    {
                        return Json(new { status = false, message = "Email already exists !" });
                    }
                    if (!string.IsNullOrEmpty(memModal.EmployeeId))
                    {
                        var checkEmployeesId = db.P_Member.Any(m => m.EmployeeId.Equals(memModal.EmployeeId, StringComparison.InvariantCultureIgnoreCase) == true && m.MemberNumber != member.MemberNumber && m.Delete != true);
                        if (checkEmployeesId)
                        {
                            return Json(new { status = false, message = "Employees Id already exists !" });
                        }
                    }
                    if (string.IsNullOrEmpty(Request["DepartmentId"]) == false)
                    {
                        member.DepartmentId = Request["DepartmentId"];
                        var dept = db.P_Department.Where(delegate (P_Department d)
                        {
                            foreach (var item in member.DepartmentId?.Split(new char[] { ',' }))
                            {
                                if (item == d.Id.ToString())
                                {
                                    return true;
                                }
                            }

                            return false;
                        }).Select(d => d.Name).ToList();
                        member.DepartmentName = string.Join(",", dept);
                    }
                    else
                    {
                        member.DepartmentId = null;
                        member.DepartmentName = string.Empty;
                    }

                    member.FirstName = memModal.FirstName;
                    member.LastName = memModal.LastName;
                    member.FullName = member.FirstName + " " + member.LastName;
                    member.PersonalEmail = memModal.PersonalEmail;
                    member.CellPhone = memModal.CellPhone;
                    member.DriverLicense = memModal.DriverLicense;
                    member.SocialSecurity = Request["Social1"] + "-" + Request["Social2"] + "-" + Request["Social3"];
                    member.TimeZone = memModal.TimeZone;
                    member.GMTNumber = Ext.EnumParse<TIMEZONE_NUMBER>(memModal.TimeZone).Code<int>();
                    member.Address = memModal.Address;
                    member.Country = memModal.Country;
                    member.State = memModal.State;
                    member.City = memModal.City;
                    member.ZipCode = memModal.ZipCode;
                    member.Active = Request["Active"] != null ? true : false;
                    member.UpdateBy = cMem.FullName;
                    member.UpdateAt = DateTime.UtcNow;
                    member.Gender = Request["Gender"];
                    member.ProfileDefinedColor = memModal.ProfileDefinedColor;
                    member.BelongToPartner = BelongToParner;
                    member.IdentityCardNumber = memModal.IdentityCardNumber;
                    member.Email1 = memModal.Email1;
                    member.SocialInsuranceCode = memModal.SocialInsuranceCode;
                    member.JobPosition = memModal.JobPosition;
                    member.EmployeeStatus = memModal.EmployeeStatus;

                    member.BelongToPartner = memModal.BelongToPartner;
                    member.SiteId = memModal.SiteId;
                    //avatar
                    string Avatar_Path = "/upload/employees/" + member.MemberNumber + "/avatar";
                    string Avatar_AbsolutePath = Server.MapPath(Avatar_Path);
                    if (!Directory.Exists(Avatar_AbsolutePath))
                        Directory.CreateDirectory(Avatar_AbsolutePath);

                    if (Request.Files["avatar_upload"] != null)
                    {
                        var avatar = Request.Files["avatar_upload"];
                        if (avatar.ContentLength > 0)
                        {
                            avatar.SaveAs(Avatar_AbsolutePath + "/" + avatar.FileName);
                            member.Picture = Avatar_Path + "/" + avatar.FileName;
                        }
                    }

                    //identity image
                    string IdentityCard_Path = "/upload/employees/" + member.MemberNumber + "/identitycard";
                    string IdentityCard_AbsolutePath = Server.MapPath(IdentityCard_Path);
                    if (!Directory.Exists(IdentityCard_AbsolutePath))
                        Directory.CreateDirectory(IdentityCard_AbsolutePath);

                    if (Request.Files["IdentityCardImageBefore"] != null)
                    {
                        var IdentityCardImageBefore = Request.Files["IdentityCardImageBefore"];
                        if (IdentityCardImageBefore.ContentLength > 0)
                        {
                            IdentityCardImageBefore.SaveAs(IdentityCard_AbsolutePath + "/" + IdentityCardImageBefore.FileName);
                            member.IdentityCardImageBefore = IdentityCard_Path + "/" + IdentityCardImageBefore.FileName;
                        }
                    }

                    if (Request.Files["IdentityCardImageAfter"] != null)
                    {
                        var IdentityCardImageAfter = Request.Files["IdentityCardImageAfter"];
                        if (IdentityCardImageAfter.ContentLength > 0)
                        {
                            IdentityCardImageAfter.SaveAs(IdentityCard_AbsolutePath + "/" + IdentityCardImageAfter.FileName);
                            member.IdentityCardImageAfter = IdentityCard_Path + "/" + IdentityCardImageAfter.FileName;
                        }
                    }
                    member.EmployeeId = memModal.EmployeeId;
                    member.IdentityCardNumber = memModal.IdentityCardNumber;
                    member.PersonalIncomeTax = memModal.PersonalIncomeTax;
                    member.ProbationDate = memModal.ProbationDate;
                    member.EmploymentContractDate = memModal.EmploymentContractDate;
                    member.TerminateContractDate = memModal.TerminateContractDate;

                    //chi distributor moi co ReferedBy
                    if (string.IsNullOrEmpty(Request["MemberType"]) == true || Request["MemberType"] == "distributor")
                    {
                        member.MemberType = "distributor";
                        member.MemberTypeName = db.P_MemberType.Where(mt => mt.MemberType == member.MemberType).FirstOrDefault()?.Name;
                        var memberNumber = Request["ReferedByMemberNumber"];
                        member.ReferedByNumber = memberNumber;
                        member.ReferedByName = db.P_Member.Where(m => m.MemberNumber == memberNumber).FirstOrDefault()?.FullName;

                        //co ReferedBy thi k co TypeSalary va BaseSalary
                        member.TypeSalary = string.Empty;
                        member.BaseSalary = 0;
                    }
                    else
                    {
                        if (member.MemberType == "distributor")
                        {
                            var list_refered = db.P_Member.Where(m => m.ReferedByNumber == member.MemberNumber).ToList();
                            foreach (var m in list_refered)
                            {
                                m.ReferedByNumber = member.ReferedByNumber;
                                m.ReferedByName = member.ReferedByName;
                                db.Entry(m).State = System.Data.Entity.EntityState.Modified;
                            }
                            member.ReferedByNumber = "";
                            member.ReferedByName = "";
                        }

                        member.MemberType = Request["MemberType"];
                        member.MemberTypeName = db.P_MemberType.Where(mt => mt.MemberType == member.MemberType).FirstOrDefault()?.Name;
                        member.TypeSalary = Request["SalaryType"];
                        member.BaseSalary = memModal.BaseSalary ?? 0;
                        //co TypeSalary va BaseSalary thi k co ReferedBy
                        member.ReferedByNumber = string.Empty;
                        member.ReferedByName = string.Empty;
                    }

                    member.RoleCode = Request["AccessRoles"];
                    member.RoleName = string.Empty;
                    if (string.IsNullOrEmpty(member.RoleCode) == false)
                    {
                        var role_code = member.RoleCode.Split(',');
                        for (int i = 0; i < role_code.Length; i++)
                        {
                            var r_code = role_code[i];
                            var role_name = db.A_Role.Where(r => r.RoleCode == r_code).FirstOrDefault()?.RoleName;
                            member.RoleName = member.RoleName + role_name + ",";
                        }
                    }

                    DateTime.TryParse(Request["Birthday"], out DateTime Birthday);
                    if (Birthday.Year > 1900)
                    {
                        member.Birthday = Birthday;
                    }

                    member.UpdateAt = DateTime.UtcNow;
                    member.UpdateBy = cMem.FullName;
               
                    db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    return Json(new { status = true, message = "update success" });
                }
                else
                {

                    var checkExistEmail = db.P_Member.Any(m => m.PersonalEmail.Equals(memModal.PersonalEmail, StringComparison.InvariantCultureIgnoreCase) == true && m.Delete != true);
                    if (checkExistEmail)
                    {
                        return Json(new { status = false, message = "Email already exists !" });
                    }
                    if (!string.IsNullOrEmpty(memModal.EmployeeId))
                    {
                        var checkEmployeesId = db.P_Member.Any(m => m.EmployeeId.Equals(memModal.EmployeeId, StringComparison.InvariantCultureIgnoreCase) == true && m.Delete != true);
                        if (checkEmployeesId)
                        {
                            return Json(new { status = false, message = "Employees Id already exists !" });
                        }
                    } 
                    int countMem = db.P_Member.Count();
                    memModal.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                    memModal.MemberNumber = (countMem + 1).ToString().PadLeft(5, '0') + (new Random().Next(1, 9));
                    if (string.IsNullOrEmpty(Request["DepartmentId"]) == false)
                    {
                        memModal.DepartmentId = Request["DepartmentId"];
                        var dept = db.P_Department.Where(delegate (P_Department d)
                        {
                            foreach (var item in memModal.DepartmentId?.Split(new char[] { ',' }))
                            {
                                if (item == d.Id.ToString())
                                {
                                    return true;
                                }
                            }
                            return false;
                        }).Select(d => d.Name).ToList();
                        memModal.DepartmentName = string.Join(",", dept);
                    }
                    else
                    {
                        memModal.DepartmentId = null;
                        memModal.DepartmentName = string.Empty;
                    }
                    memModal.FirstName = memModal.FirstName;
                    memModal.LastName = memModal.LastName;
                    memModal.FullName = memModal.FirstName + " " + memModal.LastName;
                    memModal.PersonalEmail = memModal.PersonalEmail;
                    string new_pass = new Random().Next(0, 999999).ToString().PadLeft(6, '0');
                    memModal.Password = new_pass;
                    memModal.CellPhone = memModal.CellPhone;
                    memModal.DriverLicense = memModal.DriverLicense;
                    memModal.SocialSecurity = Request["Social1"] + "-" + Request["Social2"] + "-" + Request["Social3"];
                    memModal.TimeZone = memModal.TimeZone;
                    memModal.GMTNumber = Ext.EnumParse<TIMEZONE_NUMBER>(memModal.TimeZone).Code<int>();
                    memModal.Address = memModal.Address;
                    memModal.Country = memModal.Country;
                    memModal.State = memModal.State;
                    memModal.City = memModal.City;
                    memModal.ZipCode = memModal.ZipCode;
                    memModal.Active = Request["Active"] != null ? true : false;
                    memModal.UpdateBy = cMem.FullName;
                    memModal.UpdateAt = DateTime.UtcNow;
                    memModal.Gender = Request["Gender"];
                    memModal.ProfileDefinedColor = memModal.ProfileDefinedColor;
                    memModal.IdentityCardNumber = memModal.IdentityCardNumber;
                    memModal.Email1 = memModal.Email1;
                    memModal.SocialInsuranceCode = memModal.SocialInsuranceCode;
                    memModal.JobPosition = memModal.JobPosition;
                    memModal.EmployeeStatus = memModal.EmployeeStatus;

                    //avatar
                    string Avatar_Path = "/upload/employees/" + memModal.MemberNumber + "/avatar";
                    string Avatar_AbsolutePath = Server.MapPath(Avatar_Path);
                    if (!Directory.Exists(Avatar_AbsolutePath))
                        Directory.CreateDirectory(Avatar_AbsolutePath);

                    if (Request.Files["avatar_upload"] != null)
                    {
                        var avatar = Request.Files["avatar_upload"];
                        if (avatar.ContentLength > 0)
                        {
                            avatar.SaveAs(Avatar_AbsolutePath + "/" + avatar.FileName);
                            memModal.Picture = Avatar_Path + "/" + avatar.FileName;
                        }
                    }
                    //identity image
                    string IdentityCard_Path = "/upload/employees/" + memModal.MemberNumber + "/identitycard";
                    string IdentityCard_AbsolutePath = Server.MapPath(IdentityCard_Path);
                    if (!Directory.Exists(IdentityCard_AbsolutePath))
                        Directory.CreateDirectory(IdentityCard_AbsolutePath);

                    if (Request.Files["IdentityCardImageBefore"] != null)
                    {
                        var IdentityCardImageBefore = Request.Files["IdentityCardImageBefore"];
                        if (IdentityCardImageBefore.ContentLength > 0)
                        {
                            IdentityCardImageBefore.SaveAs(IdentityCard_AbsolutePath + "/" + IdentityCardImageBefore.FileName);
                            memModal.IdentityCardImageBefore = IdentityCard_Path + "/" + IdentityCardImageBefore.FileName;
                        }
                    }


                    if (Request.Files["IdentityCardImageAfter"] != null)
                    {
                        var IdentityCardImageAfter = Request.Files["IdentityCardImageAfter"];
                        if (IdentityCardImageAfter.ContentLength > 0)
                        {
                            IdentityCardImageAfter.SaveAs(IdentityCard_AbsolutePath + "/" + IdentityCardImageAfter.FileName);
                            memModal.IdentityCardImageAfter = IdentityCard_Path + "/" + IdentityCardImageAfter.FileName;
                        }
                    }
                    memModal.EmployeeId = memModal.EmployeeId;
                    memModal.IdentityCardNumber = memModal.IdentityCardNumber;
                    memModal.PersonalIncomeTax = memModal.PersonalIncomeTax;
                    memModal.ProbationDate = memModal.ProbationDate;
                    memModal.EmploymentContractDate = memModal.EmploymentContractDate;
                    memModal.TerminateContractDate = memModal.TerminateContractDate;

                    //chi distributor moi co ReferedBy
                    if (string.IsNullOrEmpty(Request["MemberType"]) == true || Request["MemberType"] == "distributor")
                    {
                        memModal.MemberType = "distributor";
                        memModal.MemberTypeName = db.P_MemberType.Where(mt => mt.MemberType == memModal.MemberType).FirstOrDefault()?.Name;
                        var memberNumber = Request["ReferedByMemberNumber"];
                        memModal.ReferedByNumber = memberNumber;
                        memModal.ReferedByName = db.P_Member.Where(m => m.MemberNumber == memberNumber).FirstOrDefault()?.FullName;

                        //co ReferedBy thi k co TypeSalary va BaseSalary
                        memModal.TypeSalary = string.Empty;
                        memModal.BaseSalary = 0;
                    }
                    else
                    {
                        if (memModal.MemberType == "distributor")
                        {
                            var list_refered = db.P_Member.Where(m => m.ReferedByNumber == memModal.MemberNumber).ToList();
                            foreach (var m in list_refered)
                            {
                                m.ReferedByNumber = memModal.ReferedByNumber;
                                m.ReferedByName = memModal.ReferedByName;
                                db.Entry(m).State = System.Data.Entity.EntityState.Modified;
                            }
                            memModal.ReferedByNumber = "";
                            memModal.ReferedByName = "";
                        }
                        memModal.MemberType = Request["MemberType"];
                        memModal.MemberTypeName = db.P_MemberType.Where(mt => mt.MemberType == memModal.MemberType).FirstOrDefault()?.Name;
                        memModal.TypeSalary = Request["SalaryType"];
                        memModal.BaseSalary = memModal.BaseSalary ?? 0;
                        //co TypeSalary va BaseSalary thi k co ReferedBy
                        memModal.ReferedByNumber = string.Empty;
                        memModal.ReferedByName = string.Empty;
                    }
                    memModal.RoleCode = Request["AccessRoles"];
                    memModal.RoleName = string.Empty;
                    if (string.IsNullOrEmpty(memModal.RoleCode) == false)
                    {
                        var role_code = memModal.RoleCode.Split(',');
                        for (int i = 0; i < role_code.Length; i++)
                        {
                            var r_code = role_code[i];
                            var role_name = db.A_Role.Where(r => r.RoleCode == r_code).FirstOrDefault()?.RoleName;
                            memModal.RoleName = memModal.RoleName + role_name + ",";
                        }
                    }
                    DateTime.TryParse(Request["Birthday"], out DateTime Birthday);
                    if (Birthday.Year > 1900)
                    {
                        memModal.Birthday = Birthday;
                    }
                    memModal.CreateAt = DateTime.UtcNow;
                    memModal.CreateBy = cMem.FullName;

                    if (cMem.SiteId != 1)
                    {
                        memModal.SiteId = cMem.SiteId;
                    }
                    else if (string.IsNullOrEmpty(memModal.BelongToPartner))
                    {
                        memModal.SiteId = 1;
                    }

                    db.P_Member.Add(memModal);
                    db.SaveChanges();
                    await SendWellcomeEmail(memModal);
                    string message = "add employees success";
                    if (command == "save_and_sendemail")
                    {
                        message = "add employees and send email update info success";
                        await this.SendEmailUpdate(memModal.MemberNumber);
                    }
                 
                    return Json(new { status = true, message = message });

                }
            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex);
                return Json(new { status = true, message = ex.Message });
            }
        }

        /// <summary>
        /// Delete member
        /// </summary>
        /// <param name="mn"></param>
        /// <returns></returns>
        public ActionResult Delete(string mn)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partners_deletemember")) == false || access["partners_deletemember"] != true)
                {
                    return Redirect("/home/forbidden");
                }

                WebDataModel db = new WebDataModel();
                var member = db.P_Member.Where(m => m.MemberNumber == mn).FirstOrDefault();

                //current mem chi delete dc nhung member co RoleLevel <= currenr mem
                if ((db.A_Role.Where(r => r.RoleCode == cMem.RoleCode).FirstOrDefault()?.RoleLevel) < (db.A_Role.Where(r => r.RoleCode == member.RoleCode).FirstOrDefault()?.RoleLevel))
                {
                    return Redirect("/home/forbidden");
                }

                if (member != null)
                {
                    member.Active = false;
                    member.Delete = true;
                    member.UpdateAt = DateTime.UtcNow;
                    member.UpdateBy = cMem.FullName;
                    db.Entry(member).State = System.Data.Entity.EntityState.Modified;

                    db.SaveChanges();
                }
                else
                {
                    throw new Exception("Member number is not found");
                }

                TempData["s"] = "Delete successfully";
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
            }

            return RedirectToAction("index");
        }

        /// <summary>
        /// Delete file Attachment
        /// </summary>
        /// <param name="id"></param>
        /// <param name="memNumber"></param>
        /// <returns></returns>
        public ActionResult DeleteFileAttachment(long? uploadid, string memNumber)
        {
            try
            {
                string sPath = "~/Upload/Employees";
                WebDataModel db = new WebDataModel();
                UploadMoreFile more_file_upload = db.UploadMoreFiles.Find(uploadid);

                sPath = Path.Combine(Server.MapPath(sPath), more_file_upload.FileName?.Split('\\')[1]);
                FileInfo f = new FileInfo(sPath);
                if (f.Exists)
                {
                    f.Delete();
                }

                db.UploadMoreFiles.Remove(more_file_upload);
                db.SaveChanges();
                TempData["s"] = "Delete file success!";
            }
            catch (Exception e)
            {
                TempData["error"] = e.Message;
            }

            return RedirectToAction("save", new { mn = memNumber });
        }

        /// <summary>
        /// Promote level
        /// </summary>
        /// <param name="MemNumber"></param>
        /// <param name="Level"></param>
        /// <param name="EffectiveDate"></param>
        /// <returns></returns>
        public JsonResult UpLevel(string MemNumber, int? Level, DateTime? EffectiveDate)
        {
            try
            {
                WebDataModel db = new WebDataModel();

                if (db.P_MemberLevel.Any(x => x.MemberNumber == MemNumber && x.LevelNumber == Level && x.EffectiveDate == EffectiveDate) == true)
                {
                    throw new Exception("Level up infomation is already exist.");
                }

                var member_level = new P_MemberLevel
                {
                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff")),
                    MemberNumber = MemNumber,
                    MemberName = db.P_Member.Where(m => m.MemberNumber == MemNumber).FirstOrDefault()?.FullName,
                    LevelNumber = Level,
                    LevelName = db.P_Level.Find(Level)?.LevelName,
                    EffectiveDate = EffectiveDate,
                    PromotedAtDate = DateTime.UtcNow,
                    PromotedBy = cMem.FullName + " - #" + cMem.MemberNumber
                };
                db.P_MemberLevel.Add(member_level);
                db.SaveChanges();

                return Json(new object[] { true, Level, member_level.LevelName });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        public JsonResult ResetPassword(long id)
        {
            try
            {
                if (!cMem.RoleCode.Contains("admin"))
                {
                    throw new Exception("Only Administrator can reset member password!");
                }
                WebDataModel db = new WebDataModel();
                //ok. reset pass
                string new_pass = new Random().Next(0, 999999).ToString().PadLeft(6, '0');
                var member = db.P_Member.Find(id);
                if (member != null)
                {
                    member.Password = new_pass;
                    db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                    //send email after changed password
                    _ = _mailingService.SendEmailAfterResetPass(member.FirstName, member.PersonalEmail, member.Password, member.PersonalEmail);
                }
                else
                {
                    throw new Exception("Member not found");
                }

                db.SaveChanges();
                return Json(new object[] { true, "Reset password successfully! New password has been sent to " + member.PersonalEmail });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });
            }
        }

        [HttpGet]
        public ActionResult LoadAcessRoleBySiteId(int SiteId)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var allListRole = (from siteRole in db.SiteRoles join r in db.A_Role on siteRole.RoleId equals r.Id where siteRole.SiteId == SiteId orderby r.RoleLevel descending select r).ToList();
                return Json(allListRole,JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        
        }
        #endregion Member

        #region subcription

        /// <summary>
        /// ql thanh vien dang ky
        /// </summary>
        /// <returns></returns>
        public ActionResult Subcription()
        {
            if (access.Any(k => k.Key.Equals("partners_addnewmember")) == false || access["partners_addnewmember"] != true)
            {
                return Redirect("/home/forbidden");
            }
            ViewBag.p = access;
            var db = new WebDataModel();
            var _subcription = from s in db.P_MemberSubscription
                               orderby s.Id descending, s.ConfirmAt
                               select s;
            return View(_subcription);
        }

        /// <summary>
        /// approve thanh vien da dang ky
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Subcription_approve(long? id)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partners_addnewmember")) == false || access["partners_addnewmember"] != true)
                {
                    return Redirect("/home/forbidden");
                }
                var db = new WebDataModel();
                var subcription = db.P_MemberSubscription.Find(id);
                if (subcription == null)
                {
                    return HttpNotFound();
                }
                int countMem = db.P_Member.Count();

                if (db.P_Member.Any(e => (e.PersonalEmail.Equals(subcription.PersonalEmail, StringComparison.InvariantCultureIgnoreCase)
                    || (e.CellPhone != null && e.CellPhone == subcription.CellPhone)) && e.Delete != true))
                {
                    throw new Exception("This member already exists");
                }

                var mem = new P_Member
                {
                    Active = true,
                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff")),
                    FirstName = subcription.FirstName,
                    LastName = subcription.LastName,
                    FullName = subcription.FirstName + " " + subcription.LastName,
                    PersonalEmail = subcription.PersonalEmail,
                    Password = subcription.Password,
                    CellPhone = subcription.CellPhone,
                    Address = subcription.Address,
                    //default female
                    Gender = "Female",
                    City = subcription.City,
                    State = subcription.State,
                    ZipCode = subcription.ZipCode,
                    Country = subcription.Country,
                    DriverLicense = subcription.DriverLicense,
                    SocialSecurity = subcription.SocialSecurity,
                    TimeZone = "Eastern",
                    GMTNumber = -5,
                    MemberType = subcription.MemberType,
                    MemberTypeName = subcription.MemberTypeName,
                    SiteId=1,
                    ReferedByName = subcription.ReferedByName,
                    ReferedByNumber = subcription.ReferedByNumber,
                    MemberNumber = (countMem + 1).ToString().PadLeft(5, '0') + (new Random().Next(1, 9)),
                    CreateAt = subcription.CreateAt,
                    CreateBy = "Approved by " + cMem.FullName,
                    UpdateAt = DateTime.UtcNow,
                    UpdateBy = cMem.FullName,
                    JoinDate = DateTime.Today
                };

                mem.Email1 = CommonFunc.ConvertNonUnicodeURL(mem.FirstName) + mem.MemberNumber + "@enrico.us";
                mem.EmailTempPass = DateTime.UtcNow.ToString("HHmmss");
                if (subcription.MemberType.Equals("distributor", StringComparison.InvariantCultureIgnoreCase))
                {
                    mem.RoleCode = "distributor";
                    mem.RoleName = db.A_Role.Where(r => r.RoleCode.Equals(mem.RoleCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.RoleName;
                }
                else
                {
                    mem.RoleCode = "member";
                    mem.RoleName = db.A_Role.Where(r => r.RoleCode.Equals(mem.RoleCode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault()?.RoleName;
                }

                //add to member
                db.P_Member.Add(mem);

                //update subcription record
                subcription.ConfirmAt = DateTime.UtcNow;
                subcription.ConfirmBy = cMem.FullName;
                subcription.MemberNumber = mem.MemberNumber;
                db.Entry(subcription).State = System.Data.Entity.EntityState.Modified;

                db.SaveChanges();

                //send email thong bao
                string rs = await SendWellcomeEmail(mem);
                if (rs == "OK")
                {
                    TempData["s"] = "Wellcome email has been sent. You can update more information for this member.";
                }
                else
                {
                    TempData["e"] = "Wellcome email send failure, " + rs;
                }

                return RedirectToAction("save", new { mn = mem.MemberNumber });
            }
            catch (Exception e)
            {
                TempData["e"] = "Oops! Can not approve. " + e.Message;
            }

            return Redirect("/Memberman/Subcription");
        }

        /// <summary>
        /// delete subcription member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Subcription_delete(long id)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("partners_addnewmember")) == false || access["partners_addnewmember"] != true)
                {
                    return Redirect("/home/forbidden");
                }
                var db = new WebDataModel();
                var subcription = db.P_MemberSubscription.Find(id);
                if (subcription != null)
                {
                    db.P_MemberSubscription.Remove(subcription);
                    db.SaveChanges();
                    TempData["s"] = subcription.FirstName + "'s subcription has been deleted";
                }
                else
                {
                    TempData["e"] = "Subcription not found";
                }
            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
            }

            return RedirectToAction("Subcription");
        }

        /// <summary>
        /// wellcome email
        /// </summary>
        /// <param name="mem"></param>
        /// <param name="err"></param>
        /// <returns></returns>
        private async Task<string> SendWellcomeEmail(P_Member mem)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                if (mem == null || string.IsNullOrWhiteSpace(mem.PersonalEmail) == true)
                {
                    return "";
                }
                var webinfo = db.SystemConfigurations.FirstOrDefault();

                string result1 = await _mailingService.SendEmailAfterEmployeesCreated(mem.FirstName, mem.PersonalEmail, mem.Password, mem.MemberNumber.ToUpper(), mem.Email1, mem.EmailTempPass, mem.PersonalEmail);
                if (string.IsNullOrWhiteSpace(result1))
                {
                    await _mailingService.SendEmailToITNoticeNewStaff(webinfo.SupportEmail, mem.FirstName, mem.LastName, mem.CellPhone, mem.FullName, mem.Email1, mem.EmailTempPass, "", webinfo.NotificationEmail);
                }
                else
                {
                    throw new Exception(result1);
                }
                return "OK";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #endregion subcription

        #region ajax function

        /// <summary>Gets the member by member number.</summary>
        /// <param name="MemberNumber">The member number.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        /// 
        [HttpGet]
        public ActionResult GetProvincesByCountryId(string CountryId)
        {
            var db = new WebDataModel();
            var provinces = db.Ad_Provinces.Where(x => x.CountryId == CountryId).Select(x=> new { 
                x.Name,
                x.Code
            }).ToList();
            return Json(provinces, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMemberByMemberNumber(string MemberNumber)
        {
            var db = new WebDataModel();
            var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == MemberNumber);
            return Json(member, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// get ds member
        /// </summary>
        /// <param name="type">member type code</param>
        /// <returns></returns>
        [AllowAnonymous]
        public JsonResult LoadMemberList(string type)
        {
            try
            {
                var db = new WebDataModel();
                var result = db.P_Member.Where(m => m.MemberType.Equals(type) && m.Active == true).OrderBy(m => m.FullName).ToList();
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public ActionResult AddOrUpdateEmployeeFiles(int? EmployeesFileId,string MemberNumber)
        {
            WebDataModel db = new WebDataModel();
            var files = new P_EmployeeFiles();
            if (EmployeesFileId != null)
            {
                files = db.P_EmployeeFiles.Find(EmployeesFileId);
                if (files == null)
                {
                    return Content("document file not found");
                }
                ViewBag.attachments = db.UploadMoreFiles.Where(x => x.TableId == EmployeesFileId && x.TableName == "P_EmployeeFiles").ToList();
            }
            else
            {
                files.MemberNumber = MemberNumber;
            }

            return PartialView("_EmployeesFileCreateOrUpdate", files);
        }


        [HttpPost]
        public async Task<ActionResult> AddOrUpdateEmployeeFilesSubmit(P_EmployeeFiles model)
        {
            try
            {
                WebDataModel db = new WebDataModel();

                if (model.Id > 0)
                {
                    var file = db.P_EmployeeFiles.Find(model.Id);
                    if (file == null)
                    {
                        return Json(new { status = false, message = " file not found" });
                    }
                    file.Name = model.Name;
                    file.Description = model.Description;
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(file.Id, "P_EmployeeFiles", filesTotal);
                    file.UpdateAt = DateTime.UtcNow;
                    file.UpdateBy = cMem.FullName;
                    db.SaveChanges();
                    return Json(new { status = true, message = "update file success" });
                }
                else
                {
                    model.CreateAt = DateTime.UtcNow;
                    model.CreateBy = cMem.FullName;
                    db.P_EmployeeFiles.Add(model);
                    db.SaveChanges();
                    int filesTotal = Request.Files.Count;
                    var UploadIds = await UploadMultipleFilesAsync(model.Id, "P_EmployeeFiles", filesTotal);
                    db.SaveChanges();
                    return Json(new { status = true, message = "add file success" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult DeleteEmployeeFile(int EmployeesFileId)
        {
            WebDataModel db = new WebDataModel();
            var file = db.P_EmployeeFiles.Find(EmployeesFileId);
            if (file == null)
            {
                return Json(new { status = false, message = "file not found" });
            }
            db.P_EmployeeFiles.Remove(file);
            db.SaveChanges();
            return Json(new { status = true, message = "delete file success" });
        }


        public ActionResult GetListFile(IDataTablesRequest dataTablesRequest,string MemberNumber)
        {
            WebDataModel db = new WebDataModel();
            var query = from files in db.P_EmployeeFiles where files.MemberNumber == MemberNumber select files;
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch?.Name)
            {

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
                    query = query.OrderByDescending(x => x.CreateAt);
                    break;
            };
            int totalRecord = query.Count();
            var data = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x => new
            {
              x.Name,
              CreateAt= string.Format("{0:r}",x.CreateAt),
              x.Description,
              x.Id,
              x.CreateBy,
            });
            return Json(new
            {
                data = data,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult MarkasCompleteProfile(string MemberNumber)
        {
            WebDataModel db = new WebDataModel();
            var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == MemberNumber);
            member.IsCompleteProfile = !(member.IsCompleteProfile ?? false);
            db.SaveChanges();
            return Json(new { status = true, message = member.IsCompleteProfile == true ? "Mark as profile is completed success" : "Mark as profile is uncompleted success" });
        }
        #endregion ajax function

        #region authorize google

        [HttpPost]
        public async Task<ActionResult> SendEmailRequireGoogleAuth(string email)
        {
            try
            {
                var db = new WebDataModel();
                var member = db.P_Member.FirstOrDefault(x => x.PersonalEmail == email);
                string emailEncrypt = SecurityLibrary.Encrypt(member.MemberNumber);
                var url = WebConfigurationManager.AppSettings["IMSUrl"] + "/MemberMan/VerifyGoogleAuth?key=" + emailEncrypt;
                string subject = $"IMS Require Google Auth for email : {member.PersonalEmail}";
                string content = $"Dear {member.FullName},<br/><br/>" +
                                 $"<p>To make sure you're still using organizational email for your work.</p>" +
                                 $"<p>We need you to confirm by clicking the button below and login with enrich's email account.</p>" +
                                 $"<a style='background: #00c0ef; color: white; padding: 4px 18px; margin-top: 11px; display: inline-block; text-decoration: none; border-radius: 3px;' href=" + url + " style='margin-left: 25px;'><b>Verification</b></a><br/><br/>" +
                                 $"<p>Thank you for your cooperation !</p>";

                var emailData = new { content = content, subject = subject };
                var msg = await _mailingService.SendNotifyOutgoingWithTemplate(member.PersonalEmail, "", subject, "", emailData);
                var resultMsg = member.IsSendEmailGoogleAuth == true ? "resend email success" : "send email success";
                member.IsSendEmailGoogleAuth = true;
                db.SaveChanges();
                return Json(new { status = true, message = resultMsg });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [AllowAnonymous]
        public ActionResult VerifyGoogleAuth(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Content("oops something went wrong !");
            }
            var db = new WebDataModel();
            var MemberNumber = SecurityLibrary.Decrypt(key);
            var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == MemberNumber);
            if (member == null)
            {
                return Content("oops something went wrong !");
            }
            if (member.IsAuthorizedGoogle == true)
            {
                ViewBag.Page = "ThankYou";
            }
            else
            {
                ViewBag.Page = "Verification";
            }
            ViewBag.MemberName = member.FullName;
            ViewBag.Key = key;

            return View();
        }
        [AllowAnonymous]
        public async Task<ActionResult> VerifyGoogleAuthUserConfirm(string key)
        {
            var email = SecurityLibrary.Decrypt(key);
            var result = await new AuthorizationCodeMvcApp(this, new AppFlowMetadata(email)).
              AuthorizeAsync(CancellationToken.None);
            if (result.Credential != null)
            {
                return Content("Account is authorized !");
            }
            Session["EmailAuthorize"] = email;
            return new RedirectResult(result.RedirectUri);
        }

        #endregion authorize google

        #region AvailableTime

        /// <summary>
        /// Availabels the time.
        /// </summary>
        /// <returns></returns>
        public ActionResult AvailabelTime()
        {
            var db = new WebDataModel();
            var model = new P_MemberAvailableTime();
            var RecurringType = MemberAvailabelTimeType.Recurring.ToString();
            var recurringTime = db.P_MemberAvailableTime.FirstOrDefault(x => x.MemberNumber == cMem.MemberNumber && x.Type == RecurringType);
            if (recurringTime != null)
            {
                model = recurringTime;
            }
            else
            {
                model.DaysOfWeek = "1,2,3,4,5,6";
                model.StartTime = new TimeSpan(09, 00, 00);
                model.EndTime = new TimeSpan(21, 00, 00);
            }
            return View(model);
        }

        /// <summary>
        /// Gets the content of the availabel time.
        /// </summary>
        /// <param name="Type">The type.</param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult SaveAvailableTime(P_MemberAvailableTime model, DateTime? StartTime, DateTime? EndTime, string[] DayOfWeek)
        {
            var db = new WebDataModel();
            var listTimeZone = new MerchantService().ListTimeZone();
            var RecurringType = MemberAvailabelTimeType.Recurring.ToString();
            var RecurringAvailabelTime = db.P_MemberAvailableTime.Where(x => x.MemberNumber == cMem.MemberNumber && x.Type == RecurringType).FirstOrDefault();
            if (RecurringAvailabelTime != null)
            {
                RecurringAvailabelTime.TimeZone = model.TimeZone;
                if (!string.IsNullOrEmpty(model.TimeZone))
                {
                    RecurringAvailabelTime.TimeZoneNumber = listTimeZone.FirstOrDefault(c => c.Name == model.TimeZone)?.TimeDT ?? Ext.EnumParse<TIMEZONE_NUMBER>(model.TimeZone).Text();
                }
                else
                {
                    RecurringAvailabelTime.TimeZoneNumber = null;
                }
                RecurringAvailabelTime.StartDate = model.StartDate;
                RecurringAvailabelTime.EndDate = model.EndDate;
                RecurringAvailabelTime.DaysOfWeek = DayOfWeek != null ? string.Join(",", DayOfWeek) : null;
                RecurringAvailabelTime.StartTime = new TimeSpan(StartTime.Value.Hour, StartTime.Value.Minute, 0);
                RecurringAvailabelTime.EndTime = new TimeSpan(EndTime.Value.Hour, EndTime.Value.Minute, 0);
                RecurringAvailabelTime.UpdatedOn = DateTime.UtcNow;
                RecurringAvailabelTime.UpdatedBy = cMem.FullName;
            }
            else
            {
                model.MemberNumber = cMem.MemberNumber;
                if (!string.IsNullOrEmpty(model.TimeZone))
                {
                    model.TimeZoneNumber = listTimeZone.FirstOrDefault(c => c.Name == model.TimeZone)?.TimeDT ?? Ext.EnumParse<TIMEZONE_NUMBER>(model.TimeZone).Text();
                }
                else
                {
                    model.TimeZoneNumber = null;
                }
                model.Status = true;
                model.DaysOfWeek = DayOfWeek != null ? string.Join(",", DayOfWeek) : null;
                model.Type = RecurringType;
                model.StartTime = new TimeSpan(StartTime.Value.Hour, StartTime.Value.Minute, 0);
                model.EndTime = new TimeSpan(EndTime.Value.Hour, EndTime.Value.Minute, 0);
                model.CreatedOn = DateTime.UtcNow;
                model.CreatedBy = cMem.FullName;
                db.P_MemberAvailableTime.Add(model);
            }
            db.SaveChanges();
            TempData["s"] = "Update success";
            return RedirectToAction("AvailabelTime");
        }

        [HttpPost]
        public ActionResult GetAllSpecificAvailabelTime()
        {
            var db = new WebDataModel();
            var ListEvent = new List<EventCalendarCustomizeModel>();
            var RecurringType = MemberAvailabelTimeType.Recurring.ToString();
            var SpecificType = MemberAvailabelTimeType.Specific.ToString();
            var eventSpecificAvailableTime = (from e in db.P_MemberAvailableTime where e.MemberNumber == cMem.MemberNumber && e.Type == SpecificType select e).ToList().Select(x => new EventCalendarCustomizeModel
            {
                id = x.MemberNumber,
                groupId = "specific",
                classNames = x.Status == true ? "availabel" : "not-availabel",
                title = x.Note ?? string.Empty,
                display = "auto",
                backgroundColor = "#c4c9d0",
                start = x.StartDate != null ? (x.StartDate.Value.ToString("yyyy-MM-ddT") + string.Format("{0}:{1}:00", x.StartTime.Value.Hours.ToString("D2"), x.StartTime.Value.Minutes.ToString("D2"))) : null,
                end = x.EndDate != null ? (x.EndDate.Value.ToString("yyyy-MM-ddT") + string.Format("{0}:{1}:00", x.EndTime.Value.Hours.ToString("D2"), x.EndTime.Value.Minutes.ToString("D2"))) : null,
            });

            //var ColorDemoscheduler = "#3399ff";
            //var ColorSuccess = "#2eb85c";
            //var ColorCancel = "	#e55353";
            //var eventDemoscheduler = db.Calendar_Event.Where(x => x.Type == EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString()).ToList().Select(x => new EventCalendarCustomizeModel
            //{
            //    id = x.Id,
            //    resourceId = x.MemberNumber,
            //    groupId = "event-calendar",
            //    display = "auto",
            //    classNames = "event-calendar",
            //    backgroundColor = x.Done == null ? ColorDemoscheduler : x.Done == 0 ? ColorCancel : ColorSuccess,
            //    start = x.StartUTC.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
            //    end = x.EndUTC.Value.ToString("yyyy-MM-ddTHH:mm:ss"),
            //});
            ListEvent.AddRange(eventSpecificAvailableTime);
            //ListEvent.AddRange(eventDemoscheduler);
            var eventRecurringAvailableTime = db.P_MemberAvailableTime.FirstOrDefault(e => e.MemberNumber == cMem.MemberNumber && e.Type == RecurringType);

            ListEvent.Add(new EventCalendarCustomizeModel
            {
                id = cMem.MemberNumber,
                groupId = "recurring",
                display = "auto",
                classNames = "availabel",
                backgroundColor = "#c4c9d0",
                startTime = eventRecurringAvailableTime != null ? string.Format("{0}:{1}:00", eventRecurringAvailableTime.StartTime.Value.Hours.ToString("D2"), eventRecurringAvailableTime.StartTime.Value.Minutes.ToString("D2")) : "09:00:00",
                endTime = eventRecurringAvailableTime != null ? string.Format("{0}:{1}:00", eventRecurringAvailableTime.EndTime.Value.Hours.ToString("D2"), eventRecurringAvailableTime.EndTime.Value.Minutes.ToString("D2")) : "21:00:00",
                daysOfWeek = eventRecurringAvailableTime != null ? eventRecurringAvailableTime.DaysOfWeek.Split(',').Select(Int32.Parse).ToArray() : (new int[] { 1, 2, 3, 4, 5, 6 })
            });

            return Json(ListEvent.ToArray());
        }

        [HttpPost]
        public ActionResult CreateOrUpdateSpecificTime(P_MemberAvailableTime model, DateTime? StartTime, DateTime? EndTime)
        {
            var db = new WebDataModel();

            var RecurringType = MemberAvailabelTimeType.Recurring.ToString();
            var SpecificType = MemberAvailabelTimeType.Specific.ToString();
            var RecurringAvailabelTime = db.P_MemberAvailableTime.Where(x => x.MemberNumber == cMem.MemberNumber && x.Type == RecurringType).FirstOrDefault();
            var SpecificTime = db.P_MemberAvailableTime.Where(x => x.MemberNumber == cMem.MemberNumber && x.Type == SpecificType && DbFunctions.TruncateTime(model.StartDate) == DbFunctions.TruncateTime(x.StartDate)).FirstOrDefault();
            if (SpecificTime == null)
            {
                model.MemberNumber = cMem.MemberNumber;
                model.Status = model.Status;
                model.Type = SpecificType;
                model.StartTime = new TimeSpan(StartTime.Value.Hour, StartTime.Value.Minute, 0);
                model.EndTime = new TimeSpan(EndTime.Value.Hour, EndTime.Value.Minute, 0);
                model.CreatedOn = DateTime.UtcNow;
                model.CreatedBy = cMem.FullName;
                db.P_MemberAvailableTime.Add(model);
            }
            else
            {
                SpecificTime.StartTime = new TimeSpan(StartTime.Value.Hour, StartTime.Value.Minute, 0);
                SpecificTime.EndTime = new TimeSpan(EndTime.Value.Hour, EndTime.Value.Minute, 0);
                SpecificTime.Note = model.Note;
                SpecificTime.Status = model.Status;
                SpecificTime.UpdatedBy = cMem.FullName;
                SpecificTime.UpdatedOn = DateTime.UtcNow;
            }

            db.SaveChanges();
            return Json(new { status = true, message = "update success" });
        }


        public ActionResult MemberInformationTop(string MemberNumber)
        {
            var db = new WebDataModel();
            var model = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
            return PartialView("_MemberInformationTop", model);
        }

        public async Task<ActionResult> SendEmailUpdate(string MemberNumber)
        {
            var db = new WebDataModel();
            var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == MemberNumber);
            string emailEncrypt = SecurityLibrary.Encrypt(member.MemberNumber);
            var url = WebConfigurationManager.AppSettings["IMSUrl"] + "/Page/employees?key=" + emailEncrypt;
            string subject = $"Enrich: Update Information";
            string content = $"Hi {member.FullName},<br/>" +
                             $"<p>You need to update your information !</p>" +
                             $"<a style='background: #00c0ef; color: white; padding: 4px 18px; margin-top: 11px; display: inline-block; text-decoration: none; border-radius: 3px;' href=" + url + " style='margin-left: 25px;'><b>Update Info</b></a><br/><br/>" +
                             $"<p>Thank you for your cooperation !</p>";

            var emailData = new { content = content, subject = subject };
            var msg = await _mailingService.SendEmail_HR(member.PersonalEmail, "", subject, "", emailData);
            var resultMsg = member.IsSendEmailUpdateInfo == true ? "resend email success" : "send email success";
            member.IsSendEmailUpdateInfo = true;
            db.SaveChanges();
            return Json(new { status = true, message = resultMsg });
        }
        #endregion AvailableTime

        #region exportExcel
        public ActionResult ExportExcel(DateTime? FromDate, DateTime? ToDate,bool? s_completeprofile,string s_search_text, string s_member_type, string s_department, string s_partner, string s_active, string s_workstatus)
        {
            WebDataModel db = new WebDataModel();
            var members = from m in db.P_Member where m.Delete != true select m;
            if (!string.IsNullOrEmpty(s_search_text))
            {
                members = members.Where(x =>
                x.FullName.Contains(s_search_text)
                || x.CellPhone.Contains(s_search_text)
                  || x.Email1.Contains(s_search_text)
                    || x.PersonalEmail.Contains(s_search_text)
                      || x.EmployeeId.Contains(s_search_text)
                       || x.MemberNumber.Contains(s_search_text)
                );
            }
            if (FromDate != null)
            {
                var From = FromDate.Value.Date + new TimeSpan(0, 0, 0);
                members = members.Where(x => x.CreateAt >= From);
            }
            if (ToDate != null)
            {
                var To = ToDate.Value.Date + new TimeSpan(23, 59, 59);
                members = members.Where(x => x.CreateAt <= To);
            }

            if (s_completeprofile != null)
            {
                members = members.Where(x => (x.IsCompleteProfile ?? false) == s_completeprofile);
            }

            if (!string.IsNullOrEmpty(s_member_type))
            {
                members = members.Where(x => x.MemberType == s_member_type);

            }
            if (!string.IsNullOrEmpty(s_department))
            {
                members = members.Where(x => x.DepartmentId.Contains(s_department));

            }
            if (!string.IsNullOrEmpty(s_partner))
            {
                if (s_partner == "mango")
                {
                    members = members.Where(x => string.IsNullOrEmpty(x.BelongToPartner) || x.BelongToPartner == s_partner);

                }
                else
                {
                    members = members.Where(x => x.BelongToPartner == s_partner);

                }
            }

            if (!string.IsNullOrEmpty(s_active))
            {
                if (s_active == "active")
                {
                    members = members.Where(x => x.Active != false);
                }
                else
                {
                    members = members.Where(x => x.Active == false);
                }

            }
            if (!string.IsNullOrEmpty(s_workstatus))
            {
                if (s_workstatus == "Working")
                {
                    members = members.Where(x => x.EmployeeStatus == s_workstatus || string.IsNullOrEmpty(x.EmployeeStatus));
                }
                else
                {
                    members = members.Where(x => x.EmployeeStatus == s_workstatus);
                }

            }

            // check is partner
            if (!string.IsNullOrEmpty(cMem.BelongToPartner))
            {
                members = members.Where(x => x.BelongToPartner == cMem.BelongToPartner);
            }


            var memoryStream = new MemoryStream();
            // --- Below code would create excel file with dummy data----
            string folderExport = "/upload/export/excel/employees/";
            DirectoryInfo d = new DirectoryInfo(Server.MapPath(folderExport));
            if (!d.Exists)
            {
                d.Create();
            }
            var path = Path.Combine(folderExport, "Employees_" + DateTime.UtcNow.ToString("yymmddhhmmssfff") + ".xlsx");
            var fullpath = Server.MapPath(path);
            using (var fs = new FileStream(fullpath, FileMode.Create, FileAccess.Write))
            {

                IWorkbook workbook = new XSSFWorkbook();
                //name style
                IFont font = workbook.CreateFont();
                font.IsBold = true;
                font.FontHeightInPoints = 14;
                ICellStyle style = workbook.CreateCellStyle();
                style.SetFont(font);

                IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                ISheet excelSheet = workbook.CreateSheet("Sheet 1");
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

                IFont fontTitle = workbook.CreateFont();
                fontTitle.IsBold = true;
                fontTitle.FontHeightInPoints = 17;

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
                IRow header = excelSheet.CreateRow(0);
                string[] head_titles = { "Employee Id", "Employee Name", "Work Status", "Status", "Department", "Email", "Phone", "Probation Date", "Official Date", "Off Date", "Identity card", "Personal Income Tax", "Social Insurance Code", "Country", "State/Province", "City/District", "Address", "Zip code" };
                for (int i = 0; i < head_titles.Length; i++)
                {
                    ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]);
                    c.CellStyle = style1;

                }

                int row_num = 1;

                var countries = db.Ad_Country.Where(x => x.CountryCode == "US" || x.CountryCode == "VN").ToList();
                var provinces = db.Ad_Provinces.ToList();
                foreach (var member in members)
                {
                    IRow row_next_1 = excelSheet.CreateRow(row_num);
                    row_next_1.CreateCell(0).SetCellValue(member.EmployeeId);
                    row_next_1.CreateCell(1).SetCellValue(member.FullName);
                    row_next_1.CreateCell(2).SetCellValue(member.EmployeeStatus);
                    row_next_1.CreateCell(3).SetCellValue(member.Active==true?"1":"0");
                    row_next_1.CreateCell(4).SetCellValue(member.DepartmentName);
                    row_next_1.CreateCell(5).SetCellValue(member.PersonalEmail);
                    row_next_1.CreateCell(6).SetCellValue(member.CellPhone);
                    row_next_1.CreateCell(7).SetCellValue(member.ProbationDate!=null? member.ProbationDate.Value.ToString("dd/MM/yyyy"):"");
                    row_next_1.CreateCell(8).SetCellValue(member.EmploymentContractDate != null ? member.EmploymentContractDate.Value.ToString("dd/MM/yyyy") : "");
                    row_next_1.CreateCell(9).SetCellValue(member.TerminateContractDate != null ? member.TerminateContractDate.Value.ToString("dd/MM/yyyy") : "");
                    row_next_1.CreateCell(10).SetCellValue(member.IdentityCardNumber);
                    row_next_1.CreateCell(11).SetCellValue(member.PersonalIncomeTax);
                    row_next_1.CreateCell(12).SetCellValue(member.SocialInsuranceCode);
                    row_next_1.CreateCell(13).SetCellValue(!string.IsNullOrEmpty(member.Country)? countries.FirstOrDefault(x=>x.CountryCode== member.Country)?.Name:"");
                    row_next_1.CreateCell(14).SetCellValue(!string.IsNullOrEmpty(member.State) ? provinces.FirstOrDefault(x => x.Code == member.State)?.Name : "");
                    row_next_1.CreateCell(15).SetCellValue(member.City);
                    row_next_1.CreateCell(16).SetCellValue(member.Address);
                    row_next_1.CreateCell(17).SetCellValue(member.ZipCode);
                    row_num++;
                }
                workbook.Write(fs);
            }
            using (var fileStream = new FileStream(fullpath, FileMode.Open))
            {
                fileStream.CopyTo(memoryStream);
                fileStream.Close();
            }
            memoryStream.Position = 0;
            return Json(new { status = true, path = path });
        }
        #endregion

        #region bank card
        public ActionResult AddOrUpdateEmployeeCard(int? EmployeeCardId,string MemberNumber)
        {
            WebDataModel db = new WebDataModel();
            var card = new P_EmployeeBankCard();
            if (EmployeeCardId != null)
            {
                card = db.P_EmployeeBankCard.Find(EmployeeCardId);
                if (card == null)
                {
                    return Content("card not found");
                }
                string default_pass = System.Configuration.ConfigurationManager.AppSettings["PassCode"];
                string CardNumber = SecurityLibrary.Decrypt(card.CardNumber, default_pass + card.CreatedAt.Date.Ticks);
             
                card.CardNumber = CardNumber;
            }
            else
            {
                card.MemberNumber = MemberNumber;
            }
            
            return PartialView("_EmployeesCardCreateOrUpdate", card);
        }


        [HttpPost]
        public  ActionResult AddOrUpdateEmployeeCardSubmit(P_EmployeeBankCard model)
        {
            string default_pass = System.Configuration.ConfigurationManager.AppSettings["PassCode"];
            try
            {
                WebDataModel db = new WebDataModel();

                if (model.Id > 0)
                {
                    var card = db.P_EmployeeBankCard.Find(model.Id);
                    if (card == null)
                    {
                        return Json(new { status = false, message = " card not found" });
                    }
           
                    card.Note = model.Note;
                    card.BankName = model.BankName;
                    card.BranchNameBank = model.BranchNameBank;
                    card.CardNumber = SecurityLibrary.Encrypt(model.CardNumber, default_pass + card.CreatedAt.Date.Ticks);
                    card.UpdateAt = DateTime.UtcNow;
                    card.UpdateBy = cMem.FullName;
                    db.SaveChanges();
                    return Json(new { status = true, message = "update card success" });
                }
                else
                {
                    model.CreatedAt = DateTime.UtcNow;
                    model.CreatedBy = cMem.FullName;
                    model.CardNumber = SecurityLibrary.Encrypt(model.CardNumber, default_pass + model.CreatedAt.Date.Ticks);
                    db.P_EmployeeBankCard.Add(model);
                    db.SaveChanges();
                    return Json(new { status = true, message = "add card success" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult DeleteEmployeeCard(int? CardId)
        {
            WebDataModel db = new WebDataModel();
            var card = db.P_EmployeeBankCard.Find(CardId);
            if (card == null)
            {
                return Json(new { status = false, message = "card not found" });
            }
            db.P_EmployeeBankCard.Remove(card);
            db.SaveChanges();
            return Json(new { status = true, message = "delete card success" });
        }

        [HttpPost]
        public ActionResult MarkCardDefault(int? EmployeeCardId)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var card = db.P_EmployeeBankCard.Find(EmployeeCardId);
                if (card == null)
                {
                    return Json(new { status = false, message = "card not found" });
                }
                card.IsDefault = true;
                var anotherCards = db.P_EmployeeBankCard.Where(x => x.Id != EmployeeCardId).ToList();
                if (anotherCards.Count() > 0)
                {
                    anotherCards.ForEach(item => item.IsDefault = false);
                }
                db.SaveChanges();
                return Json(new { status = true, message = "set default card success" });
            }
            catch(Exception ex)
            {
                return Json(new { status = true, message = ex.Message });
            }
         
        }

        
        public ActionResult GetListCard(IDataTablesRequest dataTablesRequest, string MemberNumber)
        {
            if (access.Any(k => k.Key.Equals("employee_card_management")) == false || access["employee_card_management"] != true)
            {
                return Json(new {status=false,message="access is denied" });
            }
            WebDataModel db = new WebDataModel();

            string default_pass = System.Configuration.ConfigurationManager.AppSettings["PassCode"];
            var query = from cards in db.P_EmployeeBankCard where cards.MemberNumber == MemberNumber orderby cards.CreatedAt select cards;
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            int totalRecord = query.Count();
            var data = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x => 
            {
                var item = new CardListModel();
                item.CreatedAt = string.Format("{0:r}", x.CreatedAt);
                string CardNumber = SecurityLibrary.Decrypt(x.CardNumber, default_pass + x.CreatedAt.Date.Ticks);
                //string HideCharacter = new string('*', CardNumber.Length - 4);
                //string LastAccountNumber = CardNumber.Substring(CardNumber.Length - 4);
                item.CardNumber = CardNumber;
                item.CreatedBy = x.CreatedBy;
                item.Note = x.Note;
                item.Id = x.Id;
                 item.IsDefault = x.IsDefault;
                item.BankName = x.BankName;
                item.BranchNameBank = x.BranchNameBank;
                return item;
            });
            return Json(new
            {
                data = data,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}