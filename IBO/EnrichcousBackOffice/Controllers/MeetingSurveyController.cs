using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel.Survey;
using EnrichcousBackOffice.Utils.IEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class MeetingSurveyController : Controller
    {
        // GET: MettingSurvey
        public ActionResult Index()
        {
            Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
            if (!(access.Any(k => k.Key.Equals("log_viewall")) == true && access["log_viewall"] == true))
            {
                return Redirect("/home/forbidden");
            }
            return View();
        }

        [HttpPost]
        public ActionResult ListSurvey(IDataTablesRequest request, string Search, DateTime? FromDate,DateTime? ToDate)
        {
            try
            { 

                    var db = new WebDataModel();
                    var query = from survey in db.SurveyMeetings select survey;
                    if (!string.IsNullOrEmpty(Search))
                    {
                        query = query.Where(x => x.SurveyName.ToLower().Contains(Search.Trim().ToLower()));
                    }
                    if (FromDate != null)
                    {
                        var From = FromDate.Value.Date + new TimeSpan(0, 0, 0);
                        query = query.Where(x => x.StartDate >= FromDate);
                    }
                    if (ToDate != null)
                    {
                        ToDate = ToDate.Value.Date + new TimeSpan(23, 59, 59);
                        query = query.Where(x => x.StartDate <= ToDate);
                    }
                    var colSearch = request.Columns.Where(c => c.Sort != null).FirstOrDefault();
                    switch (colSearch?.Name)
                    {
                        case "Id":
                            if (colSearch.Sort.Direction == SortDirection.Ascending)
                            {
                                query = query.OrderBy(x => x.Id);
                            }
                            else
                            {
                                query = query.OrderByDescending(x => x.Id);
                            }
                            break;
                        case "StartDate":
                            if (colSearch.Sort.Direction == SortDirection.Ascending)
                            {
                                query = query.OrderBy(x => x.StartDate);
                            }
                            else
                            {
                                query = query.OrderByDescending(x => x.StartDate);
                            }
                            break;
                        case "Name":
                            if (colSearch.Sort.Direction == SortDirection.Ascending)
                            {
                                query = query.OrderBy(x => x.SurveyName);
                            }
                            else
                            {
                                query = query.OrderByDescending(x => x.SurveyName);
                            }
                            break;
                        default:
                            query = query.OrderByDescending(x => x.CreatedOn);
                            break;
                    };
                    var total = query.Count();
                    query = query.Skip(request.Start).Take(request.Length);
                    var surveys = query.ToList(); 
                    var data = surveys.Select(x => 
                    {
                        var survey = new SurveyViewListModel();
                        survey.Id = x.Id;
                        survey.CreatedBy = x.CreatedBy;
                        if (x.StartDate.Value<=DateTime.UtcNow)
                        {
                            var allFeedback = db.SurveyMeetingMappings.Where(y => y.MeetingSurveyId == x.Id && y.Action != "skip" && y.Rate !=null && y.Rate>0).Select(y => y.Rate).ToList();
                            if (allFeedback.Count > 0)
                            {
                                survey.AverageRate = Math.Round((double)allFeedback.Sum(y => y.Value) / allFeedback.Count(), 1);
                            }
                            else
                            {
                                survey.AverageRate = 0;
                            }
                        }
                     
                        survey.SurveyName = x.SurveyName;
                        if (x.StartDate >= DateTime.UtcNow)
                        {
                            //comming soon
                            survey.Status = 1;     
                        }
                        else if (x.EndDate >= DateTime.UtcNow)
                        {
                            // in progress
                            survey.Status = 2;
                        }
                        else
                        {
                            //end
                            survey.Status = 3;
                        }
                        survey.MinuteDuration = x.MinuteDuration;
                        survey.StartDate = CommonFunc.ConvertToSpecificTime(x.StartDate.Value, (int)TimezoneNumber.EasternTime).ToString("MMM dd,yyyy hh:mm tt");
                        if (x.EndDate < DateTime.UtcNow)
                        {
                            survey.Reopen = true;
                        }
                        return survey;
                    });
                    return Json(new
                    {
                        draw = request.Draw,
                        recordsFiltered = total,
                        recordsTotal = total,
                        data = data,
                    });
                

            }
            catch(Exception ex)
            {
                return View();
            }

        }

        public ActionResult ListFeedback(int SurveyId)
        {
            WebDataModel db = new WebDataModel();
            var survey = db.SurveyMeetings.AsNoTracking().FirstOrDefault(x => x.Id == SurveyId);
            if (survey.StartDate < DateTime.UtcNow)
            {
                ViewBag.ListFeedback = db.SurveyMeetingMappings.Where(x => x.MeetingSurveyId == SurveyId && x.Action!= "skip" && x.Rate != null && x.Rate>0).ToList();
            }
            return PartialView("_MeetingSurveyFeedback", survey);
        }


        [HttpPost]
        public ActionResult GetPopupCreateOrUpdateSurvey(int? Id, bool reopen = false)
        {
            var model = new SurveyMeeting();
            using (var db = new WebDataModel())
            {
                if (Id > 0)
                {
                    model = db.SurveyMeetings.FirstOrDefault(x => x.Id == Id);
                    model.StartDate = CommonFunc.ConvertToSpecificTime(model.StartDate.Value, (int)TimezoneNumber.EasternTime);
                    if (reopen)
                    {
                        model.StartDate = null;
                        model.EndDate = null;
                        model.Id = 0;
                        ViewBag.Reopen = reopen;
                    }
                }
                ViewBag.Members = db.P_Member.Where(x => x.Delete != true && x.Active == true).ToList();
            }
            return PartialView("_MeetingSurveyPopup", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertOrUpdateSurvey(SurveyMeeting model,bool Reopen = false)
        {
            P_Member cMem = AppLB.Authority.GetCurrentMember();
            model.StartDate = CommonFunc.ConvertToUtc(model.StartDate.Value, (int)TimezoneNumber.EasternTime);
            model.EndDate = model.StartDate.Value.AddMinutes(model.MinuteDuration.Value);
            if (model.EndDate < DateTime.UtcNow)
            {
                return Json(new { status = false, message = "please select start time in the future" });
            }
            using (var db = new WebDataModel())
            {
                if (model.Id > 0)
                {
                    var survey = db.SurveyMeetings.Find(model.Id);
                    if (survey == null)
                    {
                        return Json(new { status = false, message = "survey not found" });
                    }
                    survey.MinuteDuration = model.MinuteDuration;
                    survey.StartDate = model.StartDate;
                    survey.EndDate = survey.StartDate.Value.AddMinutes(survey.MinuteDuration.Value);
                    survey.SurveyName = model.SurveyName;
                    survey.AssignMemberNumbers = Request["AssignMemberNumbers"];
                    survey.Question = model.Question;
                    survey.UpdatedOn = DateTime.UtcNow;
                    survey.UpdatedBy = cMem.FullName;
                    db.SaveChanges();
                    return Json(new { status = true, message = "update survey success" });
                }
                else
                {
                    model.EndDate = model.StartDate.Value.AddMinutes(model.MinuteDuration.Value);
                    model.AssignMemberNumbers = Request["AssignMemberNumbers"];
                    model.CreatedOn = DateTime.UtcNow;
                    model.CreatedBy = cMem.FullName;
                    db.SurveyMeetings.Add(model);
                    db.SaveChanges();
                    var message = Reopen ? "reopen survey success" : "add survey success";
                    return Json(new { status = true, message = message });
                }
            }
             
        }

        [HttpPost]
        public ActionResult DeleteSurvey(int Id)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var survey = db.SurveyMeetings.Find(Id);
                    if (survey == null)
                    {
                        return Json(new { status = false, message = "survey not found !" });
                    }
                    db.SurveyMeetings.Remove(survey);
                    db.SaveChanges();
                    return Json(new { status = true, message = "delete survey success" });
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }
   
    }
}