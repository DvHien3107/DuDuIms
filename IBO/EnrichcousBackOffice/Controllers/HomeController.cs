using EnrichcousBackOffice.ViewControler;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using System.Threading.Tasks;
using TimeZoneConverter;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.AppLB;
using Enrich.IServices.Utils.Universal;
using Enrich.IServices;
using Enrich.IServices.Utils.JiraConnector;
using Enrich.Core.Ultils;

namespace EnrichcousBackOffice.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private readonly ILogService _logService;
        private readonly IEnrichUniversalService _enrichUniversalService;
        private readonly IJiraConnectorAuthService _jiraConnectorAuthService;
        public HomeController(
            ILogService logService,
            IEnrichUniversalService enrichUniversalService
,
            IJiraConnectorAuthService jiraConnectorAuthService)
        {
            _logService = logService;
            _enrichUniversalService = enrichUniversalService;
            _jiraConnectorAuthService = jiraConnectorAuthService;
        }


        // GET: Home
        public ActionResult Index(bool? show_all_event)
        {
            //Task t =  ViewControler.TicketViewController.AutoTicketScenario.UpdateTicketNuveiOnboarding("19120002", "Completed");
            //t.Wait();
            var days = (int)DateTime.UtcNow.DayOfWeek;

            WebDataModel db = new WebDataModel();

            //ViewBag.Bundle = db.I_Bundle.Count();
            //ViewBag.Inventory = db.O_Device.Where(d => d.Inventory > 0 && d.Junkyard != true).Count();
            var thismonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var sales_income = db.O_Orders.Where(o => o.CreatedAt >= thismonth && (o.Status == "Paid_Wait" || o.Status == "Closed") && o.IsDelete != true).ToList().Sum(o => o.GrandTotal);
            //ViewBag.SalesIncome = sales_income?.ToString("$#,##0.##");
            //ViewBag.Merchant = db.C_Customer.Where(m => m.Active == 1).Count();//[Active: 1"Active", 0"Inactive", -1"Not processing"]

            if ((access.Any(k => k.Key.Equals("home_calendarevent_viewall")) == true && access["home_calendarevent_viewall"] == true))
            {
                if (show_all_event != null)
                {
                    Session["view_all_event"] = show_all_event == true ? "1" : "0";
                }
                else
                {
                    Session["view_all_event"] = Session["view_all_event"] ?? "0";
                }
            }
            else
            {
                Session["view_all_event"] = null;
            }
            return View();
        }

        #region Scheduler calendar

        public JsonResult GetAllEvent(string year)
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            var db = new WebDataModel();
            var TypeEvent = Calendar_Event_Type.Event.Text();
            var DemoschedulerEvent = Calendar_Event_Type.DemoScheduler.Text();
            var viewall = (access.Any(k => k.Key.Equals("home_calendarevent_viewall")) == true && access["home_calendarevent_viewall"] == true)
                     && Session["view_all_event"].ToString() == "1";
            var e = db.Calendar_Event.Where(c => c.StartEvent.Contains(year.ToString()) && (viewall || c.MemberNumber == cmem.MemberNumber) && (c.Type == TypeEvent || (c.Type == DemoschedulerEvent && c.Status == true))).Select(t => new
            {
                id = t.Id,
                title = t.Name,
                start = t.StartEvent,
                end = t.EndEvent,
                backgroundColor = t.Color,
                type = t.Type,
            });
            return Json(e.ToArray());

        }

        public JsonResult GetEventById(string id)
        {
            try
            {
                var ce = new CalendarViewService();
                var result = ce.GetEventById(id, out string err);
                if (result == null)
                {
                    throw new Exception(err);
                }
                return Json(new object[] { true, result });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, null, (e.InnerException?.Message) ?? e.Message });
            }

        }

        public JsonResult UpdateEvent()
        {
            try
            {
                var cmem = AppLB.Authority.GetCurrentMember();
                string hd_event_id = Request["hd_event_id"];
                string title = Request["title"];
                string start_date = Request["start_date"];
                string start_hour = Request["start_hours"];
                string start_minute = Request["start_minute"];
                string start_am_pm = Request["start_am_pm"];
                string end_date = Request["end_date"];
                string end_hour = Request["end_hours"];
                string end_minute = Request["end_minute"];
                string end_am_pm = Request["end_am_pm"];
                string desc = Request.Unvalidated["description"];
                string gmt = Request["schedule_timezone"];
                gmt = gmt.Substring(4, 6);
                int hour = int.Parse(Request["start_hours"].PadLeft(2, '0'));
                hour = hour == 12 ? 0 : hour;
                if (Request["start_am_pm"] == "PM")
                {
                    hour += 12;
                }
                string start = DateTime.Parse(Request["start_date"]).ToString("yyyy-MM-ddT") + hour.ToString().PadLeft(2, '0') + ":" + Request["start_minute"].PadLeft(2, '0') + ":00" + gmt;
                string end = "";
                if (!string.IsNullOrWhiteSpace(end_date))
                {
                    int ehour = int.Parse(Request["end_hours"].PadLeft(2, '0'));
                    if (Request["end_am_pm"] == "PM")
                    {
                        ehour += 12;
                    }
                    end = DateTime.Parse(Request["end_date"]).ToString("yyyy-MM-ddT") + ehour.ToString().PadLeft(2, '0') + ":" + Request["end_minute"].PadLeft(2, '0') + ":00" + gmt;
                }
                var @event = new Calendar_Event
                {
                    Description = desc,
                    Color = "#3788d8",
                    StartEvent = start,
                    EndEvent = end,
                    GMT = gmt,
                    Name = title,
                    TimeZone = Request["schedule_timezone"],
                    LastUpdateAt = DateTime.UtcNow,
                    LastUpdateBy = cmem.FullName,
                    MemberNumber = cmem.MemberNumber,
                    Type = Calendar_Event_Type.Event.Text(),
                };
                var ce = new CalendarViewService();
                if (!string.IsNullOrWhiteSpace(hd_event_id))
                {
                    string done = Request["event_done"];
                    if (done == "1")
                    {
                        @event.Done = 1;
                    }
                    else
                    {
                        @event.Done = 0;
                    }
                    var result = ce.UpdateEvent(hd_event_id, @event, cmem.FullName, out string err);
                    if (result == null)
                    {
                        throw new Exception(err);
                    }
                    return Json(new object[] { true, result, "Event has been updated" });
                }
                else
                {

                    var result = ce.AddNewEvent(@event, out string err);
                    if (result == null)
                    {
                        throw new Exception(err);
                    }
                    return Json(new object[] { true, result, "Event has been added" });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, null, (e.InnerException?.Message) ?? e.Message });
            }
        }

        public JsonResult UpdateEventDate(string id, string start, string end)
        {
            try
            {
                var ce = new CalendarViewService();
                var result = ce.UpdateEvent(id, start, end, AppLB.Authority.GetCurrentMember().FullName, out string err);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    throw new Exception(err);
                }
                return Json(new object[] { true, result });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }

        public JsonResult GetClientTimezone(string id)
        {
            //any os
            TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo(id);
            //TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(id);
            return Json(tzi.DisplayName);
        }
        #endregion

        #region chart report - disabled

        public JsonResult GetColumnChartData(string MonthlyQuarter, int Year)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                List<object> ChartData = new List<object>();

                var ListChartData = (from o in db.O_Orders
                                     where o.IsDelete != true && o.CreatedAt.Value.Year == Year
                                     group o by o.CreatedAt.Value.Month into g
                                     select new ColumnChartData
                                     {
                                         Month = g.Key,
                                         GrandTotal = g.Sum(x => x.GrandTotal) ?? 0
                                     }).ToList();

                for (int i = 1; i <= 12; i++)
                {
                    if (ListChartData.Any(x => x.Month == i) == false)
                    {
                        var _data = new ColumnChartData()
                        {
                            Month = i,
                            GrandTotal = 0
                        };
                        ListChartData.Add(_data);
                    }
                }

                if (MonthlyQuarter == "Monthly")
                {
                    ChartData.Add(new object[] { "Month", "Sales Income ($)" });

                    foreach (var item in ListChartData.OrderBy(x => x.Month))
                    {
                        ChartData.Add(new object[] { item.Month, item.GrandTotal });
                    }
                }
                else
                {
                    ChartData.Add(new object[] { "Quarter", "Sales Income ($)" });
                    decimal? GrandTotal_Quarter_I = ListChartData.Where(x => x.Month == 1 || x.Month == 2 || x.Month == 3).ToList().Sum(x => x.GrandTotal);
                    decimal? GrandTotal_Quarter_II = ListChartData.Where(x => x.Month == 4 || x.Month == 5 || x.Month == 6).ToList().Sum(x => x.GrandTotal);
                    decimal? GrandTotal_Quarter_III = ListChartData.Where(x => x.Month == 7 || x.Month == 8 || x.Month == 9).ToList().Sum(x => x.GrandTotal);
                    decimal? GrandTotal_Quarter_IV = ListChartData.Where(x => x.Month == 10 || x.Month == 11 || x.Month == 12).ToList().Sum(x => x.GrandTotal);

                    ChartData.Add(new object[] { "I", GrandTotal_Quarter_I });
                    ChartData.Add(new object[] { "II", GrandTotal_Quarter_II });
                    ChartData.Add(new object[] { "III", GrandTotal_Quarter_III });
                    ChartData.Add(new object[] { "IV", GrandTotal_Quarter_IV });
                }

                return Json(new object[] { true, ChartData });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        public JsonResult GetBarChartData(DateTime f_date, DateTime t_date)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                List<object> ChartData = new List<object>();
                ChartData.Add(new object[] { "Product", "Quantity sold" });

                var ListChartData = (from p in db.Order_Products
                                     where (p.CreateAt >= f_date) && (p.CreateAt <= t_date)
                                     group p by p.ProductCode into g
                                     select new BarChartData
                                     {
                                         ProductCode = g.Key,
                                         ProductName = g.FirstOrDefault().ProductName,
                                         Quantity = g.Sum(x => x.Quantity)
                                     }).ToList();

                foreach (var _product in db.O_Product.ToList())
                {
                    if (ListChartData.Any(x => x.ProductCode == _product.Code) == false)
                    {
                        var _data = new BarChartData()
                        {
                            ProductCode = _product.Code,
                            ProductName = _product.Name,
                            Quantity = 0
                        };
                        ListChartData.Add(_data);
                    }
                }

                foreach (var item in ListChartData.OrderByDescending(x => x.Quantity).Take(5))
                {
                    ChartData.Add(new object[] { item.ProductName, item.Quantity });
                }

                return Json(new object[] { true, ChartData });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        #endregion

        [HttpGet]
        public ActionResult SetPinNotification()
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            WebDataModel db = new WebDataModel();
            var member = db.P_Member.Where(x => x.MemberNumber == cmem.MemberNumber).FirstOrDefault();
            member.PinNotification = !(member.PinNotification ?? false);
            db.SaveChanges();
            AppLB.Authority.GetCurrentMember(true);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Dashboard()
        {
            return View();
        }
        public ActionResult IMSSmartsheet()
        {
            if (access.Any(k => k.Key.Equals("ims_smartsheet")) == true && access["ims_smartsheet"] == true)
            {
                return View();
            }
            else
            {
                return Redirect("/home/Forbidden");
            }
        }
        public JsonResult Remove_Tabhistory(string name)
        {
            var history = AppLB.UserContent.TabHistory;
            if (history.Contains(";" + name))
            {
                history = history.Replace(";" + name, "");
            }
            else if (history.Contains(name + ";"))
            {
                history = history.Replace(name + ";", "");
            }
            else
            {
                history = history.Replace(name, "");
            }
            Session[AppLB.UserContent.session_view_history] = history;
            return Json(true);
        }

        #region 404 - Error - Forbidden - DocuSign Auth return
        [AllowAnonymous]
        public ActionResult Err404()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Oops()
        {
            return View();
        }

        public ActionResult Forbidden()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult AuthDocusignResult(string token)
        {
            try
            {
                TimeSpan a = new TimeSpan(long.Parse(token));
                TimeSpan b = new TimeSpan(DateTime.UtcNow.Ticks);

                double days = (b - a).Days;

                if (days >= 7)
                {
                    return RedirectToAction("Err404");
                }
                return View();
            }
            catch (Exception)
            {
                return RedirectToAction("Err404");
            }

        }

        #endregion

        #region notice alert

        /// <summary>
        /// test notice
        /// </summary>
        public void TestNotice()
        {
            var result = NoticeViewService.BroadcastNotice("system notice", "Thong bao lúc: " + DateTime.Now.ToLongTimeString(), "/ticket/");
            Response.Write(result);
        }

        /// <summary>
        /// subcribe 1 push code
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string NoticeSubscribe(string code)
        {
            //System.Diagnostics.Debug.WriteLine(code);
            HttpCookie nameCookie = Request.Cookies["NoticeCode"];
            if (nameCookie == null || string.IsNullOrWhiteSpace(nameCookie.Value) == true)
            {

                if (nameCookie == null)
                {
                    //Create a Cookie with a suitable Key.
                    nameCookie = new HttpCookie("NoticeCode");
                    //Set the Cookie value.
                    nameCookie.Value = SecurityLibrary.Encrypt(code);
                    //Add the Cookie to Browser.
                    Response.Cookies.Add(nameCookie);
                }
                else
                {
                    //Set the Cookie value.
                    nameCookie.Value = SecurityLibrary.Encrypt(code);

                    Response.Cookies.Set(nameCookie);
                }


                try
                {
                    using (var db = new Models.WebDataModel())
                    {
                        var memberNum = AppLB.Authority.GetCurrentMember().MemberNumber;
                        var member = db.P_Member.Where(m => m.MemberNumber == memberNum).FirstOrDefault();
                        member.NoticeSubscribeCode = code;
                        db.Entry(member).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception e)
                {
                    return "NoticeSubscribe: ERROR-" + e.Message;
                }

                return "NoticeSubscribe: Success";
            }
            else
            {
                return "NoticeSubscribe existes.";
            }


        }

        #endregion

        #region notification
        [HttpGet]
        public async Task<ActionResult> GetNotification(int start, int length, string tab)
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            var notificationService = new NotificationService();
            bool? Read = null;
            if (tab == "read")
            {
                Read = true;
            }
            else if (tab == "unread")
            {
                Read = false;
            }
            var model = await notificationService.GetNotiByMemberNumber(cmem.MemberNumber, Read, start, length);
            var totalCount = notificationService.CountNotificationByMember(cmem.MemberNumber, Read);
            var Html = CommonFunc.RenderRazorViewToString("_NotificationList", model, this);
            return Json(new { data = Html, count = totalCount }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        [OutputCache(Duration = 1, VaryByParam = "none", Location = System.Web.UI.OutputCacheLocation.Client)]
        public ActionResult CountNotificationNotRead()
        {
            var cmem = Authority.GetCurrentMember();
            var notificationService = new NotificationService();
            var count = notificationService.CountNotificationNotRead(cmem.MemberNumber);
            var totalItemUnread = count;
            return Json(new { count, totalItemUnread }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MarkReadNoti(int Id, bool isread)
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            var notificationService = new NotificationService();
            notificationService.MarkReadById(Id, cmem.MemberNumber, isread);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult MarkReadAll()
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            var notificationService = new NotificationService();
            notificationService.MarkAllRead(cmem.MemberNumber);
            return Json(true);
        }
        [HttpGet]
        public ActionResult GetNewNotification(int Id)
        {
            var notificationService = new NotificationService();
            var model = notificationService.GetNotificationById(Id);
            return PartialView("_NotificationItem", model);
        }
        #endregion

        [HttpGet]
        public ActionResult CheckSurvey()
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            WebDataModel db = new WebDataModel();
            var now = DateTime.UtcNow;
            var showSurvey = false;
            var survey = db.SurveyMeetings.AsNoTracking().FirstOrDefault(x => x.AssignMemberNumbers.Contains(cmem.MemberNumber) && x.StartDate <= now && x.EndDate.Value >= now);
            if (survey != null)
            {
                var checkExistSurvey = db.SurveyMeetingMappings.Any(x => x.MeetingSurveyId == survey.Id && survey.AssignMemberNumbers.Contains(x.MemberNumber) && x.MemberNumber == cmem.MemberNumber);
                showSurvey = !checkExistSurvey;
            }
            //var check = (from survey in db.SurveyMeetings where survey.AssignMemberNumbers.Contains(cmem.MemberNumber) && survey.StartDate >= now && survey.EndDate.Value <= now
            //            join surveyMapping in db.SurveyMeetingMappings on survey.Id equals surveyMapping.MeetingSurveyId where surveyMapping.MemberNumber == cmem.MemberNumber select survey).Any() ;
            //var check = db.SurveyMeetings.Any(x => x.AssignMemberNumbers.Contains(cmem.MemberNumber) && x.StartDate >= now && x.EndDate.Value<= now)).;
            return Json(new { show = showSurvey, Id = survey?.Id }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]

        public ActionResult LoadSurvey(int? Id)
        {
            var survey = new SurveyMeeting();
            using (var db = new WebDataModel())
            {
                survey = db.SurveyMeetings.AsNoTracking().FirstOrDefault(x => x.Id == Id);
            }
            return PartialView("_MeetingSurvey", survey);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult sendSurvey(SurveyMeetingMapping model)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                model.CreatedAt = DateTime.UtcNow;
                db.SurveyMeetingMappings.Add(model);
                db.SaveChanges();
                return Json(new { status = true, message = "Thanks You" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }

        }


        [HttpGet]
        public async Task<object> GetLookupDataAsync(string type)
        {
            try
            {
                return Json(new object[] { true, await _enrichUniversalService.GetLookupDataAsync(type) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new object[] { false }, JsonRequestBehavior.AllowGet);
            }
        }

        #region check jira authorize

        [HttpGet]
        [OutputCache(Duration = int.MaxValue, VaryByParam = "none", Location = System.Web.UI.OutputCacheLocation.Client)]

        public ActionResult CheckAuthorizeJira()
        {
            //var cmem = AppLB.Authority.GetCurrentMember();
            //WebDataModel db = new WebDataModel();
            //var check = db.AuthJiraVerifies.Any(x => x.MemberNunber == cmem.MemberNumber);
            //return Json(check,JsonRequestBehavior.AllowGet);
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<ActionResult> JiraAuthorize(string locationUrl)
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            WebDataModel db = new WebDataModel();
            var url = await _jiraConnectorAuthService.GetAuthorizeUrl();
            Session["jiraCurrentLocationUrl"] = locationUrl;
            return PartialView("_JiraAuthorize", url);
        }
        public async Task<ActionResult> JiraConnectorCallback(string code,string error)
        {
            WebDataModel db = new WebDataModel();
            var cmem = AppLB.Authority.GetCurrentMember();
            bool status =false;
            if (!string.IsNullOrEmpty(code))
            {
                try
                {
                    var result = await _jiraConnectorAuthService.Auth(code);
                    TempData["success"] = "Authorize jira success, thank you for verifing";
                    status = true;
                }
                catch (Exception ex)
                {
                    TempData["error"] = "Authorize jira failed: " + ex.Message;
                }
            }
            else
            {
                TempData["warning"] = "Request was canceled";
            }
            
            var authJiraVerify = db.AuthJiraVerifies.Where(x => x.MemberNunber == cmem.MemberNumber).FirstOrDefault();
            if (authJiraVerify != null)
            {
                authJiraVerify.IsAuthorizeJira = true;
            }
            else{
            var checkVerify = new AuthJiraVerify
            {
                IsSkip = false,
                IsAuthorizeJira = status,
                CreatedDate = DateTime.UtcNow,
                MemberNunber = cmem.MemberNumber,
            };
            db.AuthJiraVerifies.Add(checkVerify);
        }  
            db.SaveChanges();
            var locationUrl = Session["jiraCurrentLocationUrl"]?.ToString()??"/";
            return Redirect(locationUrl);
        }


        [HttpPost]
        public async Task<ActionResult> CheckAuth()
        {
            var check = await _jiraConnectorAuthService.CheckAuth();
            return Json(check);
        }

        [HttpPost]
        public async Task<ActionResult> skipJiraAuth()
        {
            var cmem = AppLB.Authority.GetCurrentMember();
            WebDataModel db = new WebDataModel();
            var checkVerify = new AuthJiraVerify
            {
                IsSkip = false,
                IsAuthorizeJira = false,
                CreatedDate = DateTime.UtcNow,
                MemberNunber = cmem.MemberNumber,
            };
            db.AuthJiraVerifies.Add(checkVerify);
            db.SaveChanges();
            return Json(true);
        }
        #endregion
    }

    public class BarChartData
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? Quantity { get; set; }
    }

    public class ColumnChartData
    {
        public int Month { get; set; }
        public decimal? GrandTotal { get; set; }
    }

}