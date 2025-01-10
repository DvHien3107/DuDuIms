using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB.OptionConfig;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize(Roles = "admin")]
    public class CommSettingController : Controller
    {

        #region level config

        // GET: Comm & bonus setting
        public ActionResult Index()
        {
            WebDataModel db = new WebDataModel();
            var listCommLevel = db.P_CommLevelSetting.OrderByDescending(c => c.EffectiveDate);
            var lastEfftiveDate = listCommLevel.FirstOrDefault()?.EffectiveDate;
            var model = listCommLevel.Where(c => c.EffectiveDate == lastEfftiveDate).ToList();
            ViewBag.levels = db.P_Level.Where(l => l.IsActive == true).OrderBy(cl => cl.Level).ToList();
            var dt = (from cl in listCommLevel
                      select cl.EffectiveDate).Distinct().ToList();
            ViewBag.effectiveDate = dt;

            //bonus
            ViewBag.bonus = (from b in db.P_BonusStrategySetting
                             orderby new { b.Active, b.EffectiveDate } descending
                             select b).ToList();
            ViewBag.memberTypes = db.P_MemberType.ToList();
            string EmpType = CommSetting.Employee.Text();
            ViewBag.CommEmpSettings = db.P_CommEmployeeSetting.Where(x => x.Type == EmpType).ToList();
            return View(model);
        }

        /// <summary>
        /// khi thay doi effective date
        /// </summary>
        /// <param name="effDate"></param>
        /// <returns></returns>
        public ActionResult ChangeEffectiveDate(string effDate)
        {

            WebDataModel db = new WebDataModel();
            List<P_CommLevelSetting> model = new List<P_CommLevelSetting>();
            ViewBag.levels = db.P_Level.Where(l => l.IsActive == true).OrderBy(cl => cl.Level).ToList();
            var dt = (from cl in db.P_CommLevelSetting
                      select cl.EffectiveDate).Distinct().ToList();
            ViewBag.effectiveDate = dt;

            try
            {

                if (effDate == "default")
                {
                    //effDate == default: lay ra cac CommLevelSetting khong co EffectiveDate
                    model = db.P_CommLevelSetting.Where(c => c.EffectiveDate == null).ToList();
                }
                else
                {
                    //lay ra cac CommLevelSetting theo EffectiveDate
                    DateTime.TryParse(effDate, out DateTime _effDate);
                    model = db.P_CommLevelSetting.Where(c => c.EffectiveDate == _effDate).ToList();
                }

            }
            catch (Exception)
            {

            }

            return PartialView("_CommLevelPartial", model);
        }


        [HttpPost]
        public ActionResult SaveCommLevel()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                string effectiveDateSelect = Request["effectiveDateSelect"];
                bool resultDateConvert = DateTime.TryParse(effectiveDateSelect, out DateTime effectiveDate);
                foreach (var item in db.P_Level.OrderBy(o => o.Level))
                {
                    decimal CommPercent_Direct = decimal.Parse(string.IsNullOrWhiteSpace(Request["CommPercent_Directly_" + item.Level]) == true ? "0" : Request["CommPercent_Directly_" + item.Level]);
                    decimal CommPercent = decimal.Parse(string.IsNullOrWhiteSpace(Request["CommPercent_Override_" + item.Level]) == true ? "0" : Request["CommPercent_Override_" + item.Level]);
                    decimal CommPercent_ManagementOffice = decimal.Parse(string.IsNullOrWhiteSpace(Request["CommPercent_ManagementOffice_" + item.Level]) == true ? "0" : Request["CommPercent_ManagementOffice_" + item.Level]);

                    P_CommLevelSetting commLevel = new P_CommLevelSetting();
                    if (resultDateConvert == true)
                    {
                        commLevel = db.P_CommLevelSetting.Where(cl => cl.LevelNumber == item.Level && cl.EffectiveDate == effectiveDate).FirstOrDefault();
                    }
                    else
                    {
                        if (effectiveDateSelect != "new")
                        {
                            commLevel = db.P_CommLevelSetting.Where(cl => cl.LevelNumber == item.Level && cl.EffectiveDate == null).FirstOrDefault();
                        }
                        else
                        {
                            string newDate = Request["Opt_EffectiveDate"];
                            DateTime.TryParse(newDate, out DateTime effectiveNewDate);
                            commLevel = db.P_CommLevelSetting.Where(cl => cl.LevelNumber == item.Level && cl.EffectiveDate == effectiveNewDate).FirstOrDefault();
                        }
                    }

                    if (commLevel != null && commLevel.Id > 0)
                    {
                        //update
                        commLevel.CommPercent_Override = CommPercent;
                        commLevel.CommPercent_Directly = CommPercent_Direct;
                        commLevel.CommPercent_ManagementOffice = CommPercent_ManagementOffice;
                        db.Entry(commLevel).State = EntityState.Modified;
                    }
                    else
                    {
                        //save new
                        commLevel.Id = DateTime.UtcNow.Ticks;
                        commLevel.LevelNumber = item.Level;
                        commLevel.CommPercent_Directly = CommPercent_Direct;
                        commLevel.CommPercent_Override = CommPercent;
                        commLevel.CommPercent_ManagementOffice = CommPercent_ManagementOffice;

                        if (resultDateConvert == true)
                        {
                            commLevel.EffectiveDate = effectiveDate;
                        }
                        else
                        {
                            string newDate = Request["Opt_EffectiveDate"];
                            DateTime.TryParse(newDate, out DateTime effectiveNewDate);
                            commLevel.EffectiveDate = effectiveNewDate;
                        }

                        db.P_CommLevelSetting.Add(commLevel);
                    }

                }

                db.SaveChanges();

                TempData["s"] = "Save successfully";

            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
            }
            return RedirectToAction("index");
        }

        [HttpPost]
        public ActionResult SaveCommEmp()
        {
            try
            {
                var EmployeeCommType = CommSetting.Employee.Text();
                WebDataModel db = new WebDataModel();
                foreach (var item in db.P_CommEmployeeSetting.Where(x => x.Type == EmployeeCommType))
                {
                    var CommEmpSetting = db.P_CommEmployeeSetting.Find(item.Id);
                    decimal CommPercent = decimal.Parse(string.IsNullOrWhiteSpace(Request["CommEmpPercent_" + item.Id]) == true ? "0" : Request["CommEmpPercent_" + item.Id]);
                    CommEmpSetting.CommPercent = CommPercent;
                }
                db.SaveChanges();
                TempData["s"] = "Save successfully";
            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
            }
            return RedirectToAction("Index");
        }

        #endregion

        #region Bonus adwards

        [HttpPost]
        public ActionResult BonusSave()
        {
            try
            {
                var db = new WebDataModel();

                string bonus_name = Request["bonus_name"];
                string bonus_term = Request["bonus_term"];
                string from = Request["from"];
                string to = Request["to"];
                string bonus_totalincome = Request["bonus_totalincome"] ?? "";
                string bonus_numberofcontract = Request["bonus_numberofcontract"] ?? "";
                string bonus_totalnewmember = Request["bonus_totalnewmember"] ?? "";
                string bonus_amount = Request["bonus_amount"] ?? "";
                string bonus_membertype = Request["bonus_membertype"];
                string bonus_comment = Request["bonus_comment"];
                string bonus_id = Request["bonus_id"];
                bool bonus_active = Request["bonus_active"] == null ? false : true;
                string bonus_effectivedate = Request["bonus_effectivedate"];

                string bonus_membertypename = "";
                var arrMT = bonus_membertype.Split(new char[] { ',' });
                foreach (var item in arrMT)
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }

                    bonus_membertypename += db.P_MemberType.Find(item).Name + ",";

                }

                bonus_membertypename = bonus_membertypename.Substring(0, bonus_membertypename.Length - 1);


                if (string.IsNullOrWhiteSpace(bonus_id))
                {
                    //add new
                    var bonus = new P_BonusStrategySetting
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff")),
                        Active = bonus_active,
                        StrategyName = bonus_name,
                        ApplyByTerm = bonus_term,
                        ApplyForMemberType = bonus_membertype,
                        ApplyForMemberType_Name = bonus_membertypename,
                        Comment = bonus_comment,
                        EffectiveDate = DateTime.Parse(bonus_effectivedate),
                        UpdateAt = DateTime.UtcNow,
                        UpdateBy = AppLB.Authority.GetCurrentMember().FullName
                    };
                    if (!string.IsNullOrWhiteSpace(bonus_totalincome))
                    {
                        decimal totalincome = decimal.Parse(bonus_totalincome.Replace("$", "").Replace(",", ""));
                        bonus.Opt_TotalIncome_EqualOrThan = totalincome;
                    }
                    if (!string.IsNullOrWhiteSpace(bonus_numberofcontract))
                    {

                        int totalnumberofcontract = int.Parse(bonus_numberofcontract);
                        bonus.Opt_TotalQuantityFullContracts_EqualOrThan = totalnumberofcontract;
                    }
                    if (!string.IsNullOrWhiteSpace(bonus_totalnewmember))
                    {
                        int totalnewmember = int.Parse(bonus_totalnewmember);
                        bonus.Opt_NewMemberTotal_EqualOrThan = totalnewmember;
                    }
                    if (bonus_term == "bydate")
                    {
                        if (DateTime.TryParse(Request["bonus_from"], out DateTime fromd) == true)
                        {
                            bonus.StartDate = fromd;
                        }
                        if (DateTime.TryParse(Request["bonus_to"], out DateTime tod) == true)
                        {
                            bonus.EndDate = tod;
                        }
                    }
                    else
                    {
                        bonus.StartDate = null;
                        bonus.EndDate = null;
                    }
                    bonus.BonusAmount = decimal.Parse(bonus_amount.Replace("$", "").Replace(",", ""));
                    db.P_BonusStrategySetting.Add(bonus);
                }
                else
                {
                    //update
                    var bonus = db.P_BonusStrategySetting.Find(long.Parse(bonus_id));
                    bonus.Active = bonus_active;
                    bonus.StrategyName = bonus_name;
                    bonus.ApplyByTerm = bonus_term;
                    bonus.ApplyForMemberType = bonus_membertype;
                    bonus.Comment = bonus_comment;
                    bonus.ApplyForMemberType_Name = bonus_membertypename;
                    bonus.EffectiveDate = DateTime.Parse(bonus_effectivedate);
                    bonus.UpdateAt = DateTime.UtcNow;
                    bonus.UpdateBy = AppLB.Authority.GetCurrentMember().FullName;
                    if (!string.IsNullOrWhiteSpace(bonus_totalincome))
                    {
                        decimal totalincome = decimal.Parse(bonus_totalincome.Replace("$", "").Replace(",", ""));
                        bonus.Opt_TotalIncome_EqualOrThan = totalincome;
                    }
                    if (!string.IsNullOrWhiteSpace(bonus_numberofcontract))
                    {

                        int totalnumberofcontract = int.Parse(bonus_numberofcontract);
                        bonus.Opt_TotalQuantityFullContracts_EqualOrThan = totalnumberofcontract;
                    }
                    if (!string.IsNullOrWhiteSpace(bonus_totalnewmember))
                    {
                        int totalnewmember = int.Parse(bonus_totalnewmember);
                        bonus.Opt_NewMemberTotal_EqualOrThan = totalnewmember;
                    }
                    if (bonus_term == "bydate")
                    {
                        if (DateTime.TryParse(Request["bonus_from"], out DateTime fromd) == true)
                        {
                            bonus.StartDate = fromd;
                        }
                        if (DateTime.TryParse(Request["bonus_to"], out DateTime tod) == true)
                        {
                            bonus.EndDate = tod;
                        }
                    }
                    else
                    {
                        bonus.StartDate = null;
                        bonus.EndDate = null;
                    }
                    bonus.BonusAmount = decimal.Parse(bonus_amount.Replace("$", "").Replace(",", ""));

                    db.Entry(bonus).State = EntityState.Modified;
                }
                db.SaveChanges();
                TempData["s"] = "Save successfull";
            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
            }
            return RedirectToAction("Index", new { t = 'b' });
        }

        public JsonResult GetBonusById(long id)
        {
            try
            {
                var db = new WebDataModel();
                var bonus = db.P_BonusStrategySetting.Find(id);
                return Json(
                    new
                    {
                        bonus.Id,
                        bonus.Opt_NewMemberTotal_EqualOrThan,
                        Opt_TotalIncome_EqualOrThan = bonus.Opt_TotalIncome_EqualOrThan?.ToString("#,##0.##"),
                        bonus.Opt_TotalQuantityFullContracts_EqualOrThan,
                        bonus.StartDate,
                        bonus.StrategyName,
                        UpdateAt = bonus.UpdateAt?.ToString("MM/dd/yyyy hh:mm tt"),
                        bonus.UpdateBy,
                        bonus.Active,
                        bonus.ApplyByTerm,
                        bonus.ApplyForMemberType,
                        bonus.ApplyForMemberType_Name,
                        BonusAmount = bonus.BonusAmount?.ToString("#,##0.##"),
                        bonus.Comment,
                        EffectiveDate = bonus.EffectiveDate?.ToShortDateString(),
                        EndDate = bonus.EndDate?.ToShortDateString()

                    }
                    );
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult DeleteBonus(long id)
        {
            try
            {
                var db = new WebDataModel();
                db.P_BonusStrategySetting.Remove(db.P_BonusStrategySetting.Find(id));
                db.SaveChanges();
                TempData["s"] = "Delete successfull";

            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
            }
            return RedirectToAction("Index", new { t = 'b' });
        }

        public ActionResult RestartBonus(long id)
        {
            try
            {
                var db = new WebDataModel();
                var bonus = db.P_BonusStrategySetting.Find(id);
                bonus.Active = true;
                bonus.UpdateAt = DateTime.UtcNow;
                bonus.UpdateBy = AppLB.Authority.GetCurrentMember().FullName;
                db.Entry(bonus).State = EntityState.Modified;
                db.SaveChanges();
                TempData["s"] = bonus.StrategyName.ToUpper() + " program has restarted";
            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
            }
            return RedirectToAction("Index", new { t = 'b' });
        }


        public ActionResult StopBonus(long id)
        {
            try
            {
                var db = new WebDataModel();
                var bonus = db.P_BonusStrategySetting.Find(id);
                bonus.Active = false;
                bonus.UpdateAt = DateTime.UtcNow;
                bonus.UpdateBy = AppLB.Authority.GetCurrentMember().FullName;
                db.Entry(bonus).State = EntityState.Modified;
                db.SaveChanges();
                TempData["s"] = bonus.StrategyName.ToUpper() + " program has stopped";

            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
            }
            return RedirectToAction("Index", new { t = 'b' });
        }


        #endregion

        #region CommEmpSetting

        #endregion

        #region Option Configuration 
        public ActionResult OptionConfiguration()
        {
            if (AppLB.Authority.GetCurrentMember().RoleCode.Contains("admin") == false)
            {
                return Redirect("/home/forbidden");
            }
            return View();
        }
        [HttpPost]
        public ActionResult AllOptionConfiguration(IDataTablesRequest dataTablesRequest, string SearchName, string SearchDescription)
        {

            var db = new WebDataModel();
            var query = from o in db.Option_Config select o;
            int recordsTotal = 0;
            if (!string.IsNullOrEmpty(SearchName))
            {
                query = query.Where(x => x.Key.Contains(SearchName.Trim()));
            }
            if (!string.IsNullOrEmpty(SearchDescription))
            {
                query = query.Where(x => x.Description.Contains(SearchDescription.Trim()));
            }
            recordsTotal = query.Count();
            query = query.OrderByDescending(x => x.Id).Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);//.SortAndPage(dataTablesRequest);
            var data = query.ToList();
            var ViewData = data.Select(x => new
            {
                x.Id,
                x.Key,
                x.Value,
                x.Description
            });
            return Json(new
            {
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                draw = dataTablesRequest.Draw,
                data = ViewData
            });
        }
        [HttpPost]
        public virtual ActionResult OptionConfigUpdate(Option_Config model)
        {
            var db = new WebDataModel();
            if (model.Value != null)
                model.Value = model.Value.Trim();
            if (model.Description != null)
                model.Description = model.Description.Trim();
            var optioncg = db.Option_Config.Find(model.Id);
            if (optioncg == null)
               return Json(new { status = false, messsage = "No option configuration could be loaded with the specified ID" });

            if (db.Option_Config.FirstOrDefault(x => x.Id != optioncg.Id && x.Key == model.Key) != null)
                return Json( new { status = false, messsage = "Exist Key" });
            optioncg.Value = model.Value;
            optioncg.Description = model.Description;
            db.SaveChanges();
            return Json(new { status = true }) ;
        }

        public ActionResult RestartApplication()
        {
            if (AppLB.Authority.GetCurrentMember().RoleCode.Contains("admin") == false)
            {
                return Redirect("/home/forbidden");
            }
            WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
            HttpRuntime.UnloadAppDomain();
            IMSLog logStartApplication = new IMSLog()
            {
                Id = Guid.NewGuid().ToString(),
                CreateBy = "IMS System",
                CreateOn = DateTime.UtcNow,
                StatusCode = 200,
                Success = true,
                Description = "Restart Application Success",
            };
            _writeLogErrorService.InsertLogError(logStartApplication);
            TempData["success"] = "Restart Application Success";
            return Redirect("/systemsetting");
        }
        #endregion
    }
}
//[HttpPost]
//public virtual ActionResult OptionConfigDelete(int id)
//{
//    var db = new WebDataModel();
//    if (model.Key != null)
//        model.Key = model.Key.Trim();
//    if (model.Value != null)
//        model.Value = model.Value.Trim();
//    if (model.Description != null)
//        model.Description = model.Description.Trim();
//    var optioncg = db.Option_Config.Find(model.Id);
//    if (optioncg == null)
//        return Content("No option configuration could be loaded with the specified ID");

//    db.SaveChanges();
//    return Content("Update success");

//}
