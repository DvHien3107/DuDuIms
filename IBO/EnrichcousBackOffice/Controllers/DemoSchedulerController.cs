using DataTables.AspNet.Core;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.AppLB.GoogleAuthorize;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils;
using EnrichcousBackOffice.Utils.IEnums;
using Google.Apis.Auth.OAuth2.Mvc;
using Hangfire;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using TimeZoneConverter;

namespace EnrichcousBackOffice.Controllers
{
    public class DemoSchedulerController : Controller
    {
        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        private P_Member cMem = Authority.GetCurrentMember();
        private const string ColorDemoscheduler = "#3399ff";
        private const string ColorSuccess = "#2eb85c";
        private const string ColorCancel = "#e55353";

        private readonly ILogService _logService;
        private readonly IMailingService _mailingService;
        #region Utilities

        public string[] DayOfWeekRecurring(string Dayofweek)
        {
            if (string.IsNullOrEmpty(Dayofweek))
            {
                return null;
            }
            List<string> listDayOfWeekData = new List<string>();
            var dayofweek1 = Dayofweek.Split(',').Select(Int32.Parse).ToArray();
            if (dayofweek1.Contains(0))
            {
              
                    listDayOfWeekData.Add("SU");
               
            }
            if (dayofweek1.Contains(1))
            {
                
                    listDayOfWeekData.Add("MO");
                
            }
            if (dayofweek1.Contains(2))
            {
                    listDayOfWeekData.Add("TU");
            }
            if (dayofweek1.Contains(3))
            {
               
                    listDayOfWeekData.Add("WE");
                
            }
            if (dayofweek1.Contains(4))
            {
              
                    listDayOfWeekData.Add("TH");
                
            }
            if (dayofweek1.Contains(5))
            {
               
                    listDayOfWeekData.Add("FR");
              
            }
            if (dayofweek1.Contains(6))
            {
                
                    listDayOfWeekData.Add("SA");
               
            }
            return listDayOfWeekData.ToArray();
        }

        #endregion Utilities

        // GET: DemoScheduler
        public ActionResult Index()
        {
            if (access.Any(k => k.Key.Equals("manage_demo_scheduler")) == false || access["manage_demo_scheduler"] != true)
            {
                return Redirect("/home/forbidden");
            }
            var db = new WebDataModel();
            var From = DateTime.UtcNow.AddMonths(-3);
            ViewBag.FromDate = new DateTime(From.Year, From.Month, 1).ToString("MM/dd/yyyy");
            ViewBag.ToDate = DateTime.UtcNow.ToString("MM/dd/yyyy");
            var member = db.P_Member.Where(x => x.Active == true && x.Delete != true).Select(m => new SelectListItem
            {
                Value = m.MemberNumber,
                Text = m.FullName + " #" + m.MemberNumber
            }).ToList();
            ViewBag.Member = member;
            return View();
        }

        /// <summary>Changes the tab.</summary>
        /// <param name="TabName">Tab Name</param>
        /// <returns>Partial View</returns>
        public ActionResult ChangeTab(string TabName)
        {
            switch (TabName)
            {
                case "scheduler":
                    return PartialView("_TabScheduler");

                case "completed":
                    return PartialView("_TabCompleted");

                default:
                    return PartialView("_TabCancel");
            }
        }

        #region DemoScheduler

        // Get List
        /// <summary>Gets the list demo scheduler.</summary>
        /// <param name="dataTablesRequest">The data tables request.</param>
        /// <param name="SearchText">The search text.</param>
        /// <param name="Status">The status.</param>
        /// <param name="From">From.</param>
        /// <param name="To">To.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost]
        public ActionResult GetListDemoScheduler(IDataTablesRequest dataTablesRequest, string SearchText, string Status, DateTime? From, DateTime? To)
        {
            var db = new WebDataModel();
            int totalRecord = 0;
            var query = from d in db.C_DemoScheduler
                        join c in db.C_Customer on d.CustomerCode equals c.CustomerCode
                        let LastEvent = db.Calendar_Event.Where(o => d.Id == o.DemoSchedulerId && o.Status == true).FirstOrDefault()
                            //let LastEvent = db.Calendar_Event.Where(y => y.DemoSchedulerId == d.Id).OrderByDescending(y=>y.)StartEvent.FirstOrDefault()
                        select new { d, c, LastEvent };
            if (!string.IsNullOrEmpty(Status))
            {
                switch (Status)
                {
                    case "scheduler":
                        query = query.Where(x => x.d.Status ==null);
                        break;

                    case "completed":
                        query = query.Where(x => x.d.Status == 1);
                        break;

                    default:
                        query = query.Where(x => x.d.Status == 0);
                        break;
                }
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.c.BusinessName.Contains(SearchText.Trim())
                                    || x.c.SalonPhone.Contains(SearchText.Trim())
                                    || x.c.SalonEmail.Contains(SearchText.Trim())
                                    || x.c.ContactName.Contains(SearchText.Trim())
                                    || x.c.CellPhone.Contains(SearchText.Trim())
                );
            }
            if (From != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.d.CreatedDate) >= From);
            }
            if (To != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.d.CreatedDate) <= To);
            }
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch.Name)
            {
                default:
                    query = query.OrderByDescending(s=>s.d.UpdatedDate).ThenByDescending(s => s.d.CreatedDate);
                    break;
            }
            totalRecord = query.Count();
            query = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var ListData = query.ToList();
            var viewData = ListData.Select(x => new
            {
                x.d.Id,
                x.d.Note,
                x.d.CustomerCode,
                x.c.BusinessName,
                x.c.SalonAddress1,
                x.c.OwnerName,
                x.c.CellPhone,
                x.c.Email,
                Assign = x.LastEvent != null ? x.LastEvent.MemberName: "",
                NextStartTime = x.LastEvent != null ? string.Format("{0:r}", x.LastEvent?.StartUTC) : null,
                Status = x.d?.Status,
                x.d.AttendeesNumber,
                x.d.AttendeesName,
                Address = x.c.SalonAddress1 + ", " + x.c.SalonCity + ", " + x.c.SalonState + " " + x.c.SalonZipcode + ", " + x.c.BusinessCountry,
                UpdateAt = x.d.UpdatedDate != null ? string.Format("{0:r}", x.d.UpdatedDate) : string.Format("{0:r}", x.d.CreatedDate),
                UpdatedBy = x.d.UpdatedBy != null ? x.d.UpdatedBy : x.d.CreatedBy
            });
            return Json(new
            {
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                draw = dataTablesRequest.Draw,
                data = viewData
            });
        }

        /// <summary>Updates the row data table.</summary>
        /// <param name="DemoSchedulerId">The demo scheduler identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public ActionResult UpdateRowDataTable(int? DemoSchedulerId)
        {
            var db = new WebDataModel();

            var query = from d in db.C_DemoScheduler
                        join c in db.C_Customer on d.CustomerCode equals c.CustomerCode
                        where d.Id == DemoSchedulerId
                        let LastEvent = db.Calendar_Event.Where(o => d.Id == o.DemoSchedulerId && o.Status == true).FirstOrDefault()
                        //let LastEvent = db.Calendar_Event.Where(y => y.DemoSchedulerId == d.Id).OrderByDescending(y=>y.)StartEvent.FirstOrDefault()
                        select new { d, c, LastEvent };
            var x = query.FirstOrDefault();
            var viewData = new
            {
                x.d.Id,
                x.d.Note,
                x.d.CustomerCode,
                x.c.BusinessName,
                x.c.SalonAddress1,
                x.c.OwnerName,
                x.c.CellPhone,
                x.c.Email,
                Assign = x.LastEvent != null ? x.LastEvent.MemberName : "",
                NextStartTime = x.LastEvent != null ? string.Format("{0:r}", x.LastEvent?.StartUTC) : null,
                Status = x.d?.Status,
                x.d.AttendeesNumber,
                x.d.AttendeesName,
                Address = x.c.SalonAddress1 + ", " + x.c.SalonCity + ", " + x.c.SalonState + " " + x.c.SalonZipcode + ", " + x.c.BusinessCountry,
                UpdateAt = x.d.UpdatedDate != null ? string.Format("{0:r}", x.d.UpdatedDate) : string.Format("{0:r}", x.d.CreatedDate),
                UpdatedBy = x.d.UpdatedBy != null ? x.d.UpdatedBy : x.d.CreatedBy
            };
            return Json(viewData);
        }

        /// <summary>Searches the salon.</summary>
        /// <param name="NameSearch">The name search.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public JsonResult SearchSalon(string NameSearch)
        {
            try
            {
                WebDataModel db = new WebDataModel();

                var list_merchant = from c in db.C_Customer
                                    where c.Active == 1

                                    select c;
                if (!string.IsNullOrEmpty(NameSearch))
                {
                    list_merchant = list_merchant.Where(x => x.BusinessName.Trim().Contains(NameSearch.Trim())
                                                            || x.OwnerName.Contains(NameSearch.Trim())
                                                            || x.CustomerCode.Contains(NameSearch.Trim())
                                                            || x.CellPhone.Contains(NameSearch.Trim())
                                                            || x.BusinessPhone.Contains(NameSearch.Trim())
                                                            || x.BusinessEmail.Contains(NameSearch.Trim()));
                }
                foreach (var item in list_merchant.OrderByDescending(c => c.Id).ToList())
                {
                    var us_state = db.Ad_USAState.Find(item.BusinessState);
                    if (us_state != null)
                    {
                        item.BusinessState = us_state.name;
                    }
                }
                return Json(new object[] { true, list_merchant.Count(), list_merchant });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        /// <summary>Selects the merchant.</summary>
        /// <param name="cus_code">The cus code.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public JsonResult SelectMerchant(string cus_code)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var cus = db.C_Customer.Where(c => c.CustomerCode == cus_code).FirstOrDefault();
                return Json(new { status = true, cus });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        /// <summary>Shows the popup create or update.</summary>
        /// <param name="Id">The identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public ActionResult ShowPopUpCreateOrUpdate(int? Id)
        {
            //update
            if (Id != null)
            {
                var db = new WebDataModel();
                var DemoSche = db.C_DemoScheduler.Find(Id);
                var Cus = db.C_Customer.FirstOrDefault(x => x.CustomerCode == DemoSche.CustomerCode);
                ViewBag.Cus = Cus;
                ViewBag.LastEvent = db.Calendar_Event.OrderByDescending(x => x.CreateAt).FirstOrDefault();
                return PartialView("_CreateOrUpdatePopup", DemoSche);
            }
            //create
            else
            {
                return PartialView("_CreateOrUpdatePopup", new C_DemoScheduler());
            }
        }

        /// <summary>Creates the or update demo scheduler.</summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost]
        public ActionResult CreateOrUpdateDemoScheduler(C_DemoScheduler model)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                if (model.Id == 0)
                {
                    model.CreatedBy = cMem.MemberNumber;
                    model.CreatedDate = DateTime.UtcNow;
                    db.C_DemoScheduler.Add(model);
                    db.SaveChanges();
                    return Json(new { status = true, message = "Create success !", DemoSchedulerId = model.Id });
                }
                else
                {
                    var demoScheduler = db.C_DemoScheduler.Find(model.Id);
                    demoScheduler.Status = model.Status;
                    demoScheduler.Note = model.Note;
                    demoScheduler.UpdatedBy = cMem.FullName;
                    demoScheduler.UpdatedDate = DateTime.UtcNow;
                    UpdateEventAfterChangeStatusDemoscheduler(demoScheduler.Id, demoScheduler.Status);
                    db.SaveChanges();
                    return Json(new { status = true, message = "Update success !", DemoSchedulerId = demoScheduler.Id });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        /// <summary>Deletes the demoscheduler.</summary>
        /// <param name="Id">The identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost]
        public ActionResult DeleteDemoscheduler(int? Id)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var demoScheduler = db.C_DemoScheduler.Find(Id);
                db.C_DemoScheduler.Remove(demoScheduler);
                var allEventDemo = db.Calendar_Event.Where(x => x.DemoSchedulerId == demoScheduler.Id);
                foreach (var item in allEventDemo)
                {
                    db.Calendar_Event.Remove(item);
                }
                db.SaveChanges();
                //BackgroundJob.Enqueue(() => this.DeleteAllGoogleEventCalendar(Id));
                return Json(new { status = true, message = "Delete success !" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        //public  void DeleteAllGoogleEventCalendar(int? DemoschedulerId)
        //{
        //    WebDataModel db = new WebDataModel();
        //    var allEventDemo = db.Calendar_Event.Where(x => x.DemoSchedulerId == DemoschedulerId);
        //    foreach (var item in allEventDemo.GroupBy(e=>e.MemberNumber))
        //    {
        //        var member = db.P_Member.FirstOrDefault(m => m.MemberNumber == item.Key);
        //        var credential =  new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).AuthorizeAsync(CancellationToken.None);
        //        var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
        //       foreach(var e in item)
        //        {
        //            _calendarService.Delete(e.GoogleCalendarId);
        //            db.Calendar_Event.Remove(e);
        //         }
        //    }
        //    db.SaveChanges();
        //}

        /// <summary>Shows the detail calendar demo.</summary>
        /// <param name="DemoSchedulerId">The demo scheduler identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost]
        public ActionResult ShowDetailCalendarDemo(int? DemoSchedulerId)
        {
            var db = new WebDataModel();
            var TypeDemoScheduler = EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString();
            var ListEvent = db.Calendar_Event.Where(x => x.Type == TypeDemoScheduler && x.DemoSchedulerId == DemoSchedulerId).OrderBy(x => x.StartEvent).ToList();
            ViewBag.DemoScheduler = db.C_DemoScheduler.Find(DemoSchedulerId);
            ViewBag.DemoSchedulerId = DemoSchedulerId;
            ViewBag.Status = db.C_DemoScheduler.Find(DemoSchedulerId).Status;
            return PartialView("_DetailDemoScheduler", ListEvent);
        }

        /// <summary>Shows the popup assign.</summary>
        /// <param name="DemoSchedulerId">The demo scheduler identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpGet]
        public ActionResult ShowPopupAssign(int? DemoSchedulerId)
        {
            var db = new WebDataModel();
            var DemoScheduler = db.C_DemoScheduler.Find(DemoSchedulerId);
            ViewBag.ListMember = db.P_Member.Where(x => x.Active == true).ToList();
            ViewBag.DemoSchedulerId = DemoSchedulerId;
            return PartialView("_AssignAppointmentModal", DemoScheduler);
        }
        [HttpPost]
        public ActionResult UpdateStatus(int? DemoSchedulerId, int? Status , string Note)
        {
            try
            {
                var db = new WebDataModel();
                var DemoScheduler = db.C_DemoScheduler.Find(DemoSchedulerId);
                DemoScheduler.Status = Status;
                DemoScheduler.Note = Note;
                DemoScheduler.UpdatedBy = cMem.FullName;
                DemoScheduler.UpdatedDate = DateTime.UtcNow;
                UpdateEventAfterChangeStatusDemoscheduler(DemoSchedulerId, Status);
                db.SaveChanges();
                return Json(new { status = true, message = "change status success" });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = ex.Message});
            }
           
        }
        //public ActionResult AssignDemoScheduler(int? DemoSchedulerId, string MemberNumber,string DefaultDate)
        //{
        //    try
        //    {
        //        var db = new WebDataModel();
        //        var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
        //        var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == DemoSchedulerId);
        //        if (string.IsNullOrEmpty(DefaultDate))
        //        {
        //            DefaultDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        //        }
        //        ViewBag.DefaultDate = DefaultDate;
        //        if (member.IsAuthorizedGoogle != true)
        //        {
        //            // not yet authorize google api
        //            return Json(new { status = 0, message = member.PersonalEmail });
        //        }
        //        else
        //        {
        //            ViewBag.DemoSchedulerId = DemoSchedulerId;
        //            ViewBag.MemberNumber = MemberNumber;
        //            ViewBag.MemberName = member.FullName;
        //            ViewBag.SalonName = db.C_Customer.FirstOrDefault(x => x.CustomerCode == DemoScheduler.CustomerCode).BusinessName;
        //            var calendarHTML = CommonFunc.RenderRazorViewToString("_CalendarPopup", null, this);
        //            return Json(new { status = 1, message = calendarHTML });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { status = -1, message = ex.Message });
        //    }
        //
        /// <summary>Assigns the demo scheduler.</summary>
        /// <param name="DemoSchedulerId">The demo scheduler identifier.</param>
        /// <param name="DefaultDate">The default date.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public ActionResult AssignDemoScheduler(int? DemoSchedulerId, string DefaultDate)
        {
            try
            {
                var db = new WebDataModel();

                var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == DemoSchedulerId);
                if (string.IsNullOrEmpty(DefaultDate))
                {
                    DefaultDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                }
                ViewBag.DefaultDate = DefaultDate;

                ViewBag.DemoSchedulerId = DemoSchedulerId;

                ViewBag.SalonName = db.C_Customer.FirstOrDefault(x => x.CustomerCode == DemoScheduler.CustomerCode).BusinessName;
                var calendarHTML = CommonFunc.RenderRazorViewToString("_CalendarPopup", null, this);
                return Json(new { status = 1, message = calendarHTML });
            }
            catch (Exception ex)
            {
                return Json(new { status = -1, message = ex.Message });
            }
        }

        #endregion DemoScheduler

        #region calendar plugin

        /// <summary>Shows the event popup.</summary>
        /// <param name="EventId">The event identifier.</param>
        /// <param name="MemberNumber">The member number.</param>
        /// <param name="DemoSchedulerId">The demo scheduler identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost]
        public ActionResult ShowEventPopup(string EventId, string MemberNumber, int? DemoSchedulerId)
        {
            var db = new WebDataModel();
       
          
            if (!string.IsNullOrEmpty(EventId))
            {
                var model = new DemoSchedulerEventModel();
                var Event = db.Calendar_Event.FirstOrDefault(x => x.Id == EventId);
                var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == Event.DemoSchedulerId);
                var Cus = db.C_Customer.FirstOrDefault(x => x.CustomerCode == DemoScheduler.CustomerCode);
                model.MemberNumber = Event.MemberNumber;
                model.DemoSchedulerId = Event.DemoSchedulerId;
                model.CustomerCode = Event.CustomerCode;
                model.SalonName = Event.CustomerName;
                model.Location = Event.Location;
                model.Id = Event.Id;
                model.Description = Event.Description;
                model.StartTime = DateTime.ParseExact(Event.StartEvent.Substring(0,19), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                model.EndTime = DateTime.ParseExact(Event.EndEvent.Substring(0, 19), "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                model.Name = Event.Name;
                model.TimeZone = Event.TimeZone;
                model.Status = Event.Done;
                model.REF = Event.REF;
                model.TimeZoneNumber = Event.GMT;
                ViewBag.TimeZone = "(" + Event.StartEvent.Substring(19) + ") " + model.TimeZone;
                return PartialView("_CreateOrUpdateEvent", model);
            }
            else
            {
                var model = new DemoSchedulerEventModel();
                var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == DemoSchedulerId);
                var Cus = db.C_Customer.FirstOrDefault(x => x.CustomerCode == DemoScheduler.CustomerCode);
                model.MemberNumber = MemberNumber;
                model.DemoSchedulerId = DemoSchedulerId;
                model.CustomerCode = Cus.CustomerCode;
                model.SalonName = Cus.BusinessName;
                model.TimeZone = Cus.SalonTimeZone?? "Eastern";
                model.TimeZoneNumber = Cus.SalonTimeZone_Number ?? "-05:00";
                model.Name = Cus.BusinessName;
                model.Location = Cus.SalonAddress1 + ", " + Cus.SalonCity + ", " + Cus.SalonState + " " + Cus.SalonZipcode + ", " + Cus.BusinessCountry;
                string timeZoneNumber = Cus.SalonTimeZone_Number ?? "-05:00";
                ViewBag.TimeZone = "(" + timeZoneNumber + ") " + model.TimeZone;
                return PartialView("_CreateOrUpdateEvent", model);
            }
        }

        //[HttpPost]
        //public ActionResult GetAllEventDemoschedulerCalendar(string MemberNumber)
        //{
        //    var db = new WebDataModel();
        //    var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
        //    var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).
        //      AuthorizeAsync(CancellationToken.None).Result;
        //    var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
        //    var events = _calendarService.GetList();
        //    var ViewData = events.Select(t => new
        //    {
        //        id = t.Id,
        //        title = t.Name,
        //        start = t.StartEvent,
        //        end = t.EndEvent,
        //        backgroundColor = t.Color
        //    });
        //    return Json(ViewData.ToArray());
        //}

        /// <summary>Gets all event demoscheduler calendar.</summary>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpGet]
        public ActionResult GetAllEventDemoschedulerCalendar(string TimeZone,string NumberTimeZone, DateTime? requestStartDate, DateTime? requestEndDate)
        {
            var db = new WebDataModel();
            string TypeDemoScheduler = EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString();
            var ListEvent = new List<EventCalendarCustomizeModel>();
            var RecurringType = MemberAvailabelTimeType.Recurring.ToString();
            var SpecificType = MemberAvailabelTimeType.Specific.ToString();
            var listTimeZone = new MerchantService().ListTimeZone();
            if (string.IsNullOrEmpty(NumberTimeZone))
            {
                NumberTimeZone = "-05:00";
            }
            //if (!string.IsNullOrEmpty(TimeZone))
            //{
            //    if (TimeZone != "Eastern" && TimeZone != "Central" && TimeZone != "Mountain" && TimeZone != "Pacific" && TimeZone != "VietNam")
            //    {
            //        NumberTimeZone = listTimeZone.FirstOrDefault(c => c.Name == TimeZone)?.TimeDT;
            //    }
            //    else
            //    {
            //        NumberTimeZone = Ext.EnumParse<TIMEZONE_NUMBER>(TimeZone).Text();
            //    }
            //}
            if (NumberTimeZone.Length != 6)
            {
                NumberTimeZone= NumberTimeZone.Substring(0, 1) + "0" + NumberTimeZone.Substring(1);
            }
            var ListSpecific = (from availabel in db.P_MemberAvailableTime
                                where availabel.Type == SpecificType && DbFunctions.TruncateTime(availabel.StartDate) >=requestStartDate && DbFunctions.TruncateTime(availabel.EndDate) <= requestEndDate
                                
                                select availabel).ToList();
            //DateTime today = requestEndDate.Value;
            //int currentDayOfWeek = (int)today.DayOfWeek;
            //DateTime sunday = today.AddDays(-currentDayOfWeek);
            //DateTime monday = sunday.AddDays(1);
            //// If we started on Sunday, we should actually have gone *back*
            //// 6 days instead of forward 1...
            //if (currentDayOfWeek == 0)
            //{
            //    monday = monday.AddDays(-7);
            //}
            //var dates = Enumerable.Range(0, 7).Select(days => monday.AddDays(days)).ToList();

            var eventSpecificAvailableTime = ListSpecific.Where(t => t.Status == true &&t.StartDate.Value.Date>= requestStartDate.Value.Date && t.EndDate.Value.Date<=requestEndDate.Value.Date).Select(x =>
                                                   {
                                                       var m = new EventCalendarCustomizeModel();
                                                       m.id = x.MemberNumber;
                                                       m.resourceId = x.MemberNumber;
                                                       m.groupId = "disable-time";
                                                       m.display = "inverse-background";
                                                       m.classNames = "disable-time";
                                                       m.backgroundColor = "#c4c9d0";
                                                       string gmt = "";
                                                       string TimeZoneNumb = db.P_MemberAvailableTime.FirstOrDefault(y => y.MemberNumber == x.MemberNumber && y.Type == RecurringType)?.TimeZoneNumber;
                                                       int balanceTimeZone = 0;
                                                       if (!string.IsNullOrEmpty(TimeZoneNumb))
                                                       {
                                                           balanceTimeZone = int.Parse(NumberTimeZone.Substring(0, NumberTimeZone.IndexOf(":"))) - int.Parse(TimeZoneNumb.Substring(0, TimeZoneNumb.IndexOf(":")));
                                                           gmt = NumberTimeZone;
                                                       }
                                                       //daysOfWeek = x.p != null && !string.IsNullOrEmpty(x.p.DaysOfWeek) ? x.p.DaysOfWeek.Split(',').Select(Int32.Parse).ToArray() : null,
                                                       var StartDate = (x.StartDate.Value + x.StartTime).Value.AddHours(balanceTimeZone).ToString("yyyy-MM-ddTHH:mm:ss") + gmt;
                                                       var EndDate = (x.EndDate.Value + x.EndTime).Value.AddHours(balanceTimeZone).ToString("yyyy-MM-ddTHH:mm:ss") + gmt;
                                                       m.start = StartDate;
                                                       m.end = EndDate;
                                                       return m;
                                                       //startTime = x.p != null && x.p.StartDate == null ? string.Format("{0}:{1}:00", x.p.StartTime.Value.Hours.ToString("D2"), x.p.StartTime.Value.Minutes.ToString("D2")) : "00:00:00",
                                                       //endTime = x.p != null && x.p.StartDate == null ? string.Format("{0}:{1}:00", x.p.EndTime.Value.Hours.ToString("D2"), x.p.EndTime.Value.Minutes.ToString("D2")) : "00:00:00",
                                                   });

            var eventRecurringAvailableTime = (from member in db.P_Member
                                               where member.DepartmentId.Contains("19120002") && member.Active == true
                                               join setAvailableTime in db.P_MemberAvailableTime.Where(p=>p.Type == RecurringType) on member.MemberNumber equals setAvailableTime.MemberNumber
                                               into ps
                                               from p in ps.DefaultIfEmpty()
                                               select new { member, p }).ToList().Select(x =>
                                               {
                                                   var m = new EventCalendarCustomizeModel();
                                                   m.id = x.member.MemberNumber;
                                                   m.resourceId = x.member.MemberNumber;
                                                   m.groupId = "disable-time";
                                                   m.display = "inverse-background";
                                                   m.classNames = "disable-time";
                                                   m.backgroundColor = "#c4c9d0";
                                                   //m.rrule = new Rrule();
                                                   //m.rrule.interval = 1;
                                   
                                                   string start = "090000";
                                                   string gmt = "";
                                                   string duration = "12:00";
                                                   string[] byweekday = new string[] { "MO", "TU", "WE", "TH", "FR", "SA" };

                                                   if (x.p != null)
                                                   {
                                                       int balanceTimeZone = 0;
                                                       if (!string.IsNullOrEmpty(x.p.TimeZoneNumber))
                                                       {
                                                           balanceTimeZone = int.Parse(NumberTimeZone.Substring(0, NumberTimeZone.IndexOf(":"))) - int.Parse(x.p.TimeZoneNumber.Substring(0, x.p.TimeZoneNumber.IndexOf(":")));
                                                           gmt = "Z";
                                                       }
                                                       var TmpStartDateTime = new DateTime(1900, 01, 01, x.p.StartTime.Value.Hours, x.p.StartTime.Value.Minutes, x.p.StartTime.Value.Seconds);
                                                       var TmpEndDateTime = new DateTime(2095, 01, 01, x.p.EndTime.Value.Hours, x.p.EndTime.Value.Minutes, x.p.EndTime.Value.Seconds);
                                                      var startDate =(TmpStartDateTime.AddHours(balanceTimeZone));
                                                       var endDate = (TmpEndDateTime.AddHours(balanceTimeZone));
                                                       var durationTime = (endDate - startDate);
                                                       start = startDate.ToString("HHmmss") + gmt;
                                                       duration = durationTime.Hours.ToString("D2") + ":" + durationTime.Minutes.ToString("D2");
                                                       byweekday = this.DayOfWeekRecurring(x.p.DaysOfWeek);

                                                   }
                                                   m.duration = duration;
                                                   m.rrule = new Rrule();
                                                   m.rrule.interval = 1;
                                                   m.rrule.dtstart = requestStartDate.Value.AddDays(-1).ToString("yyyyMMddT") + start;
                                                   m.rrule.until = requestEndDate.Value.AddDays(1).ToString("yyyyMMdd");
                                                   m.rrule.byweekday = byweekday;
                                                   m.rrule.freq = "weekly";
                                                   m.exdate = ListSpecific.Where(y => y.MemberNumber == x.member.MemberNumber).Select(z => z.StartDate.Value.ToString("yyyyMMddT")+start).ToArray();
                                                   return m;
                                                   //daysOfWeek = x.p != null && !string.IsNullOrEmpty(x.p.DaysOfWeek) ? x.p.DaysOfWeek.Split(',').Select(Int32.Parse).ToArray() : null,
                                                   //start = x.p != null && x.p.StartDate != null ? (x.p.StartDate.Value.ToString("yyyy-MM-ddT") + string.Format("{0}:{1}:00", x.p.StartTime.Value.Hours.ToString("D2"), x.p.StartTime.Value.Minutes.ToString("D2"))) : null,
                                                   //end = x.p != null && x.p.EndDate != null ? (x.p.EndDate.Value.ToString("yyyy-MM-ddT") + string.Format("{0}:{1}:00", x.p.EndTime.Value.Hours.ToString("D2"), x.p.EndTime.Value.Minutes.ToString("D2"))) : null,
                                                   //startTime = x.p != null && x.p.StartDate == null ? string.Format("{0}:{1}:00", x.p.StartTime.Value.Hours.ToString("D2"), x.p.StartTime.Value.Minutes.ToString("D2")) : "00:00:00",
                                                   //endTime = x.p != null && x.p.StartDate == null ? string.Format("{0}:{1}:00", x.p.EndTime.Value.Hours.ToString("D2"), x.p.EndTime.Value.Minutes.ToString("D2")) : "00:00:00",
                                               });

            var eventDemoscheduler = db.Calendar_Event.Where(x => x.Type == EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString() && x.Status==true).ToList().Select(x => 
            {
                var m = new EventCalendarCustomizeModel();
                m.id = x.Id;
                m.resourceId = x.MemberNumber;
                
                m.timeZoneNumber = x.GMT;
                m.groupId = x.Id;
                m.title = x.Name;
                m.display = "auto";
                m.classNames = "event-calendar";
                m.backgroundColor = x.Done == null ? ColorDemoscheduler : x.Done == 0 ? ColorCancel : ColorSuccess;
                var AddHour = int.Parse(NumberTimeZone.Substring(0, NumberTimeZone.IndexOf(":")));
                m.start = x.StartUTC.Value.AddHours(AddHour).ToString("yyyy-MM-ddTHH:mm:ss")+ NumberTimeZone;
                m.end = x.EndUTC.Value.AddHours(AddHour).ToString("yyyy-MM-ddTHH:mm:ss") + NumberTimeZone;
                return m;
            }) ;
            ListEvent.AddRange(eventSpecificAvailableTime);
            ListEvent.AddRange(eventRecurringAvailableTime);
            ListEvent.AddRange(eventDemoscheduler);
            return Json(ListEvent.ToArray(), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult GetEventById(string EventId, string MemberNumber)
        //{
        //    var db = new WebDataModel();
        //    var member = db.P_Member.Where(x => x.MemberNumber == MemberNumber).FirstOrDefault();
        //    var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).
        //    AuthorizeAsync(CancellationToken.None).Result;
        //    var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
        //    var mEvent = _calendarService.GetEventById(EventId);
        //    return Json(mEvent);
        //}
        /// <summary>Inserts the or update event.</summary>
        /// <param name="model">The model.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost]
        public ActionResult InsertOrUpdateEvent(DemoSchedulerEventModel model)
        {
            try
            {
                var db = new WebDataModel();
                var member = db.P_Member.Where(x => x.MemberNumber == model.MemberNumber).FirstOrDefault();
                var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).AuthorizeAsync(CancellationToken.None).Result;
                if (credential.Credential == null)
                {
                    member.GoogleAuth = null;
                    member.IsAuthorizedGoogle = null;
                    db.SaveChanges();
                    var sendemail = _mailingService.SendEmailRequireGoogleAuth(member.PersonalEmail);
                    return Json(new { status = false, message = "This account's google authorization has expired" });
                }
                var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
                var listTimeZone = new MerchantService().ListTimeZone();
                //var listTimeZone = new List<ViewModel.TimeZoneModel>();
                string NumberTimeZone = model.TimeZoneNumber??"-05:00";

                //if (model.TimeZone != "Eastern" && model.TimeZone != "Central" && model.TimeZone != "Mountain" && model.TimeZone != "Pacific" && model.TimeZone != "VietNam")
                //{
                //    NumberTimeZone = listTimeZone.FirstOrDefault(c => c.Name == model.TimeZone)?.TimeDT;
                //}
                //else
                //{
                //    NumberTimeZone = Ext.EnumParse<TIMEZONE_NUMBER>(model.TimeZone).Text();
                //}
                if (NumberTimeZone.Length != 6)
                {
                    NumberTimeZone = NumberTimeZone.Substring(0, 1) + "0" + NumberTimeZone.Substring(1);
                }
                //create
                if (string.IsNullOrEmpty(model.Id))
                {
                    var sl = db.C_SalesLead.FirstOrDefault(x => x.CustomerCode == model.CustomerCode);
                    var mEvent = new Calendar_Event();
                    mEvent.Id = DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(11, 9999).ToString();
                    mEvent.CreateBy = cMem.FullName;
                    mEvent.CreateAt = DateTime.UtcNow;
                    string gmt = NumberTimeZone;
                    mEvent.GMT = NumberTimeZone;
                    mEvent.StartEvent = model.StartTime != null ? model.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ss") + gmt : "";
                    mEvent.EndEvent = model.EndTime != null ? model.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ss") + gmt : "";
                    mEvent.Name = model.Name;
                    mEvent.StartUTC = AppFunc.ParseTimeToUtc(mEvent.StartEvent);
                    mEvent.EndUTC = AppFunc.ParseTimeToUtc(mEvent.EndEvent);
                    mEvent.Location = model.Location;
                    mEvent.Description = model.Description;
                    mEvent.SalesLeadId = sl.Id;
                    mEvent.SalesLeadName = sl.L_SalonName;
                    mEvent.Type = EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString();
                    mEvent.MemberNumber = model.MemberNumber;
                    mEvent.MemberName = member.FullName;

                    mEvent.Color= model.Status == null ? ColorDemoscheduler : model.Status == 0 ? ColorCancel : ColorSuccess;
                    mEvent.CustomerCode = model.CustomerCode;
                    mEvent.Done = model.Status;
                    mEvent.CustomerName = model.SalonName;
                    mEvent.DemoSchedulerId = model.DemoSchedulerId;
                    mEvent.TimeZone = model.TimeZone;
                    mEvent.REF = model.REF;
                    _calendarService.Insert(mEvent);
                    var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == model.DemoSchedulerId);
                    DemoScheduler.CustomerCode = model.CustomerCode;
                    DemoScheduler.Status = mEvent.Done;

                    db.SaveChanges();
                    this.FetchAndChangeStatusForLastEvent(model.DemoSchedulerId);
                    return Json(new { status = true, message = "Create Event Success." });
                }
                //update
                else
                {
                    var mEvent = _calendarService.GetEventById(model.Id);
                    var sl = db.C_SalesLead.FirstOrDefault(x => x.CustomerCode == model.CustomerCode);
                    string gmt = NumberTimeZone;
                    mEvent.GMT = NumberTimeZone;
                    mEvent.LastUpdateBy = cMem.FullName;
                    mEvent.LastUpdateByNumber = cMem.MemberNumber;
                    mEvent.LastUpdateAt = DateTime.UtcNow;
                    mEvent.StartEvent = model.StartTime != null ? model.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ss")+ gmt : "";
                    mEvent.EndEvent = model.EndTime != null ? model.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ss")+ gmt : "";
                    mEvent.Name = model.Name;
                    mEvent.TimeZone = model.TimeZone;
                    mEvent.Done = model.Status;
                    mEvent.Color = model.Status == null ? ColorDemoscheduler : model.Status == 0 ? ColorCancel : ColorSuccess;
                    mEvent.Location = model.Location;
                    mEvent.Description = model.Description;
                    mEvent.SalesLeadId = sl.Id;
                    mEvent.StartUTC = AppFunc.ParseTimeToUtc(mEvent.StartEvent);
                    mEvent.EndUTC = AppFunc.ParseTimeToUtc(mEvent.EndEvent);
                    mEvent.SalesLeadName = sl.L_SalonName;
                    mEvent.Type = EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString();
                    mEvent.MemberNumber = model.MemberNumber;
                    mEvent.MemberName = member.FullName;
                    mEvent.DemoSchedulerId = model.DemoSchedulerId;
                    mEvent.REF = model.REF;
                    _calendarService.Update(mEvent);
                    var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == model.DemoSchedulerId);
                    DemoScheduler.CustomerCode = model.CustomerCode;
                    DemoScheduler.Status = mEvent.Done;
                    DemoScheduler.UpdatedBy = cMem.FullName;
                    DemoScheduler.UpdatedDate = DateTime.UtcNow;
                    db.SaveChanges();
                    this.FetchAndChangeStatusForLastEvent(model.DemoSchedulerId);
                    return Json(new { status = true, message = "Update Event Success." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.InnerException.Message });
            }
        }

        //[HttpPost]
        //public ActionResult InsertOrUpdateEvent(DemoSchedulerEventModel model)
        //{
        //    try
        //    {
        //        var db = new WebDataModel();
        //        var member = db.P_Member.Where(x => x.MemberNumber == model.MemberNumber).FirstOrDefault();
        //        var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).AuthorizeAsync(CancellationToken.None).Result;
        //        var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
        //        //create
        //        if (string.IsNullOrEmpty(model.Id))
        //        {
        //            var sl = db.C_SalesLead.FirstOrDefault(x => x.CustomerCode == model.CustomerCode);
        //            var mEvent = new Calendar_Event();
        //            mEvent.Id = DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(11, 9999).ToString();
        //            mEvent.CreateBy = cMem.FullName;
        //            mEvent.CreateAt = DateTime.UtcNow;
        //            string gmt = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone).DisplayName.Substring(4, 6);
        //            mEvent.GMT = gmt;
        //            mEvent.StartEvent = model.StartTime != null ? model.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ss") + gmt : "";
        //            mEvent.EndEvent = model.EndTime != null ? model.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ss") + gmt : "";
        //            mEvent.Name = model.Name;
        //            mEvent.StartUTC = AppFunc.ParseTimeToUtc(mEvent.StartEvent);
        //            mEvent.EndUTC = AppFunc.ParseTimeToUtc(mEvent.EndEvent);
        //            mEvent.Location = model.Location;
        //            mEvent.Description = model.Description;
        //            mEvent.SalesLeadId = sl.Id;
        //            mEvent.SalesLeadName = sl.L_SalonName;
        //            mEvent.Type = EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString();
        //            mEvent.MemberNumber = model.MemberNumber;
        //            mEvent.MemberName = member.FullName;
        //            mEvent.CustomerCode = model.CustomerCode;
        //            mEvent.Done = model.Status;
        //            mEvent.CustomerName = model.SalonName;
        //            mEvent.DemoSchedulerId = model.DemoSchedulerId;
        //            mEvent.TimeZone = model.TimeZone;
        //            mEvent.REF = model.REF;
        //            _calendarService.Insert(mEvent);
        //            var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == model.DemoSchedulerId);
        //            DemoScheduler.CustomerCode = model.CustomerCode;
        //            db.SaveChanges();
        //            return Json(new { status = true, message = "Create Event Success." });
        //        }
        //        //update
        //        else
        //        {
        //            var mEvent = _calendarService.GetEventById(model.Id);
        //            var sl = db.C_SalesLead.FirstOrDefault(x => x.CustomerCode == model.CustomerCode);
        //            string gmt = TimeZoneInfo.FindSystemTimeZoneById(model.TimeZone).DisplayName.Substring(4, 6);
        //            mEvent.GMT = gmt;
        //            mEvent.CreateBy = cMem.FullName;
        //            mEvent.CreateAt = DateTime.UtcNow;
        //            mEvent.StartEvent = model.StartTime != null ? model.StartTime.Value.ToString("yyyy-MM-ddTHH:mm:ssK") : "";
        //            mEvent.EndEvent = model.EndTime != null ? model.EndTime.Value.ToString("yyyy-MM-ddTHH:mm:ssK") : "";
        //            mEvent.Name = model.Name;
        //            mEvent.TimeZone = model.TimeZone;
        //            mEvent.Done = model.Status;
        //            mEvent.Location = model.Location;
        //            mEvent.Description = model.Description;
        //            mEvent.SalesLeadId = sl.Id;
        //            mEvent.StartUTC = AppFunc.ParseTimeToUtc(mEvent.StartEvent);
        //            mEvent.EndUTC = AppFunc.ParseTimeToUtc(mEvent.EndEvent);
        //            mEvent.SalesLeadName = sl.L_SalonName;
        //            mEvent.Type = EnrichcousBackOffice.Utils.IEnums.Calendar_Event_Type.DemoScheduler.ToString();
        //            mEvent.MemberNumber = model.MemberNumber;
        //            mEvent.MemberName = member.FullName;
        //            mEvent.DemoSchedulerId = model.DemoSchedulerId;
        //            mEvent.REF = model.REF;
        //            _calendarService.Update(mEvent);
        //            var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == model.DemoSchedulerId);
        //            DemoScheduler.CustomerCode = model.CustomerCode;
        //            DemoScheduler.UpdatedBy = cMem.FullName;
        //            DemoScheduler.UpdatedDate = DateTime.UtcNow;
        //            db.SaveChanges();
        //            return Json(new { status = true, message = "Update Event Success." });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { status = false, message = ex.Message });
        //    }
        //}
        /// <summary>Deletes the event.</summary>
        /// <param name="EventId">The event identifier.</param>
        /// <param name="MemberNumber">The member number.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        [HttpPost]
        public ActionResult DeleteEvent(string EventId, string MemberNumber)
        {
            try
            {
                var db = new WebDataModel();
                var member = db.P_Member.FirstOrDefault(x => x.MemberNumber == MemberNumber);
                var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).
                     AuthorizeAsync(CancellationToken.None).Result;
                if (credential.Credential == null)
                {
                    member.GoogleAuth = null;
                    member.IsAuthorizedGoogle = null;
                    db.SaveChanges();
                    var sendemail = _mailingService.SendEmailRequireGoogleAuth(member.PersonalEmail);
                    return Json(new { status = false, message = "This account's google authorization has expired" });
                }
                var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
                var ev = db.Calendar_Event.Find(EventId);
                _calendarService.Delete(ev.GoogleCalendarId);
                db.Calendar_Event.Remove(ev);
                db.SaveChanges();
                this.FetchAndChangeStatusForLastEvent(ev.DemoSchedulerId);
             
                return Json(new { status=true});
            }
            catch (Exception ex)
            {
                return Json(new { status = false,message= ex.InnerException.Message });
            }
        }
        private void FetchAndChangeStatusForLastEvent(int? DemoSchedulerId)
        {
            WebDataModel db = new WebDataModel();
            var ListEvent = db.Calendar_Event.Where(x => x.DemoSchedulerId == DemoSchedulerId&&!string.IsNullOrEmpty(x.GoogleCalendarId));
            var Demoscheduler = db.C_DemoScheduler.Where(x => x.Id == DemoSchedulerId).FirstOrDefault();
            int i = 0;
            foreach (var item in ListEvent.OrderByDescending(x => x.CreateAt))
            {
                //each and change last event 
                if (i == 0)
                {
                    item.Status = true;
                    if (Demoscheduler != null)
                    {
                        Demoscheduler.Status = item.Done;
                    }
                }
                else
                {
                    item.Status = false;

                    var member = db.P_Member.Where(m => m.MemberNumber == item.MemberNumber).FirstOrDefault();
                    var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).
                    AuthorizeAsync(CancellationToken.None).Result;
                    if (credential.Credential == null)
                    {
                        member.GoogleAuth = null;
                        member.IsAuthorizedGoogle = null;
                    }
                    else
                    {
                        var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
                        _calendarService.Delete(item.GoogleCalendarId);
                        item.GoogleCalendarId = null;
                    }
                }
                i++;
            }
            db.SaveChanges();
        }
        private void UpdateEventAfterChangeStatusDemoscheduler(int? DemoSchedulerId,int? Status)
        {
            var db = new WebDataModel();
            var LastEvent = db.Calendar_Event.Where(x => x.DemoSchedulerId == DemoSchedulerId).OrderByDescending(x => x.CreateAt).FirstOrDefault();
            if (LastEvent != null)
            {
                LastEvent.Done = Status;
                LastEvent.LastUpdateAt = DateTime.UtcNow;
                LastEvent.LastUpdateBy = cMem.FullName;
                LastEvent.LastUpdateByNumber = cMem.MemberNumber;
                db.SaveChanges();
                var member = db.P_Member.Where(x => x.MemberNumber == LastEvent.MemberNumber).FirstOrDefault();
                var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(member.PersonalEmail)).
                  AuthorizeAsync(CancellationToken.None).Result;
                if (credential.Credential == null)
                {
                    member.GoogleAuth = null;
                    member.IsAuthorizedGoogle = null;
                    db.SaveChanges();
                }
                else
                {
                    var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
                    _calendarService.Update(LastEvent);

                }
            }
        }

        /// <summary>Gets the client timezone.</summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public JsonResult GetClientTimezone(string id)
        {
            //any os
            TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo(id);
            //TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById(id);
            return Json(tzi.Id);
        }

        //public void CallReloadDatav1()
        //{
        //    var text = JsonConvert.SerializeObject(HttpContext.Request.Params);
        //    System.IO.File.WriteAllText(Server.MapPath("/log.txt"), text);

        //}
        /// <summary>Gets the calendar.</summary>
        /// <param name="email">The email.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        //[HttpGet]
        //public ActionResult GetCalendar(string email)
        //{
        //    var credential = new AuthorizationCodeMvcApp(this, new AppFlowMetadata(email)).
        //            AuthorizeAsync(CancellationToken.None).Result;
        //    var _calendarService = new CalendarDemoSchedulerService(credential.Credential);
        //    var calendar = _calendarService.GetCalendar();
        //    return Json(calendar, JsonRequestBehavior.AllowGet);
        //}

        /// <summary>Gets the member by support deparment.</summary>
        /// <param name="MemberNumber">The member number.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        [HttpPost]
        public ActionResult getMemberBySupportDeparment(string MemberNumber)
        {
            var db = new WebDataModel();
            var ListMemberSP = db.P_Member.Where(m => m.Active == true && m.DepartmentId.Contains("19120002"));
            if (!string.IsNullOrEmpty(MemberNumber)&& MemberNumber!="all")
            {
                ListMemberSP = ListMemberSP.Where(x => x.MemberNumber == MemberNumber);
            }
            var Data = ListMemberSP.ToList().Select(x => new
            {
                id = x.MemberNumber,
                title = string.Join("|", new string[] { (string.IsNullOrEmpty(x.Picture) ? (x.Gender == "Male" ? "/Upload/Img/Male.png" : "/Upload/Img/FeMale.png") : x.Picture), x.FullName, x.IsAuthorizedGoogle != true ? "0" : "1" }),
            });
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>Calendars the specified demo scheduler identifier.</summary>
        /// <param name="DemoSchedulerId">The demo scheduler identifier.</param>
        /// <param name="DefaultDate">The default date.</param>
        /// <returns>
        ///   <br />
        /// </returns>
        public ActionResult Calendar(int? DemoSchedulerId, string DefaultDate,string MemberNumber)
        {
           
                var db = new WebDataModel();
                var DemoScheduler = db.C_DemoScheduler.FirstOrDefault(x => x.Id == DemoSchedulerId);
                if (DemoScheduler == null)
                {
                    return Content("oops something went wrong !");
                }
                var cus = db.C_Customer.FirstOrDefault(x => x.CustomerCode == DemoScheduler.CustomerCode);
                string timezoneNumber = cus.SalonTimeZone_Number ?? "-05:00";
                if (string.IsNullOrEmpty(DefaultDate))
                {
                    DefaultDate = DateTime.UtcNow.AddHours(int.Parse((timezoneNumber).Substring(0, timezoneNumber.IndexOf(":")))).ToString("yyyy-MM-dd");
                }
                var ListMemberSP = db.P_Member.Where(m => m.Active == true && m.DepartmentId.Contains("19120002")).ToList();
                ViewBag.MemberNumber = MemberNumber;
                ViewBag.ListMember = ListMemberSP;
                string TimeZoneId = cus.SalonTimeZone?? "Eastern";
            
                if (TimeZoneId == "Eastern" || TimeZoneId == "Central" || TimeZoneId == "Mountain" || TimeZoneId == "Pacific" || TimeZoneId == "VietNam")
                {
                    TimeZoneId = Ext.EnumParse<TIMEZONE_NUMBER_BY_ID>(TimeZoneId).Code<string>();
                }
                ViewBag.TimeZoneId = TimeZoneId;
                ViewBag.DefaultDate = DefaultDate;
                ViewBag.DemoSchedulerId = DemoSchedulerId;
                ViewBag.Salon = cus;
                return View();
            }
        [HttpGet]
        public ActionResult GetDemoScheduler(int? DemoSchedulerId)
        {
            var db = new WebDataModel();
            var d = db.C_DemoScheduler.Find(DemoSchedulerId);
            return Json(d,JsonRequestBehavior.AllowGet);
        }
        #endregion calendar plugin
    }
}