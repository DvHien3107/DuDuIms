using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize(Roles = "admin")]
    public class LevelSettingController : Controller
    {
        // GET: LevelSetting
        public ActionResult Index()
        {
            var level = new WebDataModel().P_Level.ToList();
            return View(level);
        }

        public ActionResult Save(int? id)
        {
            try
            {
                var db = new WebDataModel();
                var listLevel = db.P_Level.Where(l => l.IsActive == true && l.Level != id).OrderBy(l => l.Level).ToList();
                ViewBag.listLevel = listLevel;
                if (id > 0)
                {
                    ViewBag.Title = "Update level";
                    return View(db.P_Level.Find(id));
                }
                else
                {
                    int mLevelNo = listLevel.Count() > 0 ? listLevel.Max(l => l.Level) : 0;
                    ViewBag.Title = "Add new level";
                    return View(new P_Level { Level = mLevelNo + 1, IsActive = true });
                }

            }
            catch (Exception)
            {
                TempData["e"] = "Oops! Something went wrong. Please try again later.";
                return RedirectToAction("index");
            }


        }

        [HttpPost]
        public ActionResult Save(P_Level lvModel)
        {
            var db = new WebDataModel();
             P_Member cMem = AppLB.Authority.GetCurrentMember();

              P_Level levelUpd = db.P_Level.Find(lvModel.Level);
            try
            {
                if (lvModel.Level == 0 || string.IsNullOrWhiteSpace(lvModel.LevelName) == true)
                {
                    throw new Exception("(*) is importance,it can not empty");
                }

                if (levelUpd != null)
                {
                    levelUpd.Level = lvModel.Level;
                    levelUpd.LevelName = lvModel.LevelName;
                    levelUpd.OptionPromote2_RequimentQtyContractReached = lvModel.OptionPromote2_RequimentQtyContractReached;
                    levelUpd.OptionPromote1_RequirementLevel_RequimentEveryLevel_QtyContract = lvModel.OptionPromote1_RequirementLevel_RequimentEveryLevel_QtyContract;
                    levelUpd.OptionPromote1_RequirementLevel_Qty = lvModel.OptionPromote1_RequirementLevel_Qty;
                    levelUpd.OptionPromote1_RequirementLevel = lvModel.OptionPromote1_RequirementLevel;
                    levelUpd.IsActive = lvModel.IsActive;
                    levelUpd.UpdateAt = DateTime.UtcNow;
                    levelUpd.UpdateBy = cMem.FullName;
                    db.Entry(levelUpd).State = EntityState.Modified;
                }
                else
                {

                    P_Level lv = new P_Level();
                    lv.Level = lvModel.Level;
                    lv.LevelName = lvModel.LevelName;
                    lv.OptionPromote2_RequimentQtyContractReached = lvModel.OptionPromote2_RequimentQtyContractReached;
                    lv.OptionPromote1_RequirementLevel_RequimentEveryLevel_QtyContract = lvModel.OptionPromote1_RequirementLevel_RequimentEveryLevel_QtyContract;
                    lv.OptionPromote1_RequirementLevel_Qty = lvModel.OptionPromote1_RequirementLevel_Qty;
                    lv.OptionPromote1_RequirementLevel = lvModel.OptionPromote1_RequirementLevel;
                    lv.IsActive = lvModel.IsActive;
                    db.P_Level.Add(lv);
                }
                db.SaveChanges();
                TempData["s"] = "Save successfully";
                return RedirectToAction("index");
            }
            catch (Exception e)
            {
                if (levelUpd != null)
                {
                    ViewBag.Title = "Edit level setting";
                }
                else
                {
                    ViewBag.Title = "Add new level";
                }
                TempData["e"] = e.Message;
                ViewBag.listLevel = db.P_Level.Where(l => l.Level != lvModel.Level && l.IsActive == true).OrderBy(l => l.Level).ToList();
                return View(lvModel);
            }

        }


        public ActionResult DeleteLevel(int id)
        {

            var db = new WebDataModel();
            db.P_Level.Remove(db.P_Level.Find(id));
            db.SaveChanges();
            TempData["s"] = "Delete successfully";
            return RedirectToAction("level");

        }
        
    }
}