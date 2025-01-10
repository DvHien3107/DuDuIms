using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel.ApiRequest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class IMSLogsController : Controller
    {
        private WebDataModel db = new WebDataModel();
        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        private P_Member cMem = Authority.GetCurrentMember();
        const string LogPage = "Logs";
        const string ApiRequestPage = "ApiRequest";
        //const string SUB_TASK = "List sub task";

        // GET: TasksMan
        //public ActionResult Index(string key)
        //{
        //    WebDataModel db = new WebDataModel();
        //    var list_task = new List<Ts_Task>();
        //    ViewBag.Key = key;

        //    if (key == "assignedtask")
        //    {
        //        //add to view history top button
        //        UserContent.TabHistory = "Assigned task" + "|" + Request.Url.PathAndQuery;

        //        var list_task_assigned = db.Ts_Task.Where(t => t.AssignedToMemberNumber.Contains(cMem.MemberNumber) && t.ParentTaskId == null).ToList();
        //        if (list_task_assigned != null && list_task_assigned.Count() > 0)
        //        {

        //            list_task.AddRange(list_task_assigned);

        //        }


        //    }
        //    else if (key == "completetask")
        //    {
        //        //add to view history top button
        //        UserContent.TabHistory = "Completed task" + "|" + Request.Url.PathAndQuery;

        //        var list_task_assigned = db.Ts_Task.Where(t => (t.AssignedToMemberNumber.Contains(cMem.MemberNumber) || t.CreateByMemberNumber == cMem.MemberNumber) && t.ParentTaskId == null && t.Complete == true).ToList();
        //        if (list_task_assigned != null && list_task_assigned.Count() > 0)
        //        {
        //            list_task.AddRange(list_task_assigned);
        //        }

        //        #region Inactive
        //        //var list_subtask_assigned = db.Ts_Task.Where(t => t.AssignedToMemberNumber == cMem.MemberNumber && t.ParentTaskId != null && t.Complete == true).ToList();
        //        //if (list_subtask_assigned != null && list_subtask_assigned.Count() > 0)
        //        //{
        //        //    foreach (var subtask in list_subtask_assigned)
        //        //    {
        //        //        if (list_task.Any(t => t.Id == subtask.ParentTaskId) == false)
        //        //        {
        //        //            var task = db.Ts_Task.Find(subtask.ParentTaskId);
        //        //            list_task.Add(task);
        //        //        }
        //        //    }
        //        //}
        //        #endregion

        //    }
        //    else
        //    {
        //        //add to view history top button
        //        UserContent.TabHistory = "task" + "|" + Request.Url.PathAndQuery;

        //        //my task
        //        list_task = db.Ts_Task.Where(t => t.CreateByMemberNumber == cMem.MemberNumber && t.ParentTaskId == null).ToList();
        //    }



        //    return View(list_task);
        //}
        public ActionResult Index(string Page)
        {
            switch (Page)
            {
                case ApiRequestPage:
                    if(!User.IsInRole("admin") == true){
                        return Redirect("/home/forbidden");
                    }
                    ViewBag.Page = ApiRequestPage;
                    break;
                default:
                    if (!(access.Any(k => k.Key.Equals("log_viewall")) == true && access["log_viewall"] == true))
                    {
                        return Redirect("/home/forbidden");
                    }
                    ViewBag.Page = LogPage;
                    
                    break;
            }
            return View();
        }
        [ChildActionOnly]
        public ActionResult PageContent(string Page)
        {
            if (Page == LogPage)
            {
              
                    WebDataModel db = new WebDataModel();
                    ViewBag.p = access;

                    // add to view history top button
                    UserContent.TabHistory = "IMS Logs";

                    return PartialView("_Logs");
               
            }
            else
            {

                return PartialView("_ApiRequest");
            }
        }

        // GET: IMSLogs
        //public ActionResult Index()
        //{
        //    try
        //    {
        //        WebDataModel db = new WebDataModel();
        //        ViewBag.p = access;

        //        // add to view history top button
        //        UserContent.TabHistory = "IMSLogs";

        //        #region search option
        //        if (string.IsNullOrWhiteSpace(Request.Url.Query) == true)
        //        {
        //            //TempData.Clear();
        //            TempData.Remove("search_url");
        //            TempData.Remove("search_name");
        //            TempData.Remove("search_r_url");
        //            TempData.Remove("search_description");
        //            TempData.Remove("search_success");
        //            TempData.Remove("search_from_date");
        //            TempData.Remove("search_to_date");
        //        }
        //        if (Request["search_submit"] != null)
        //        {
        //            TempData["search_url"] = Request["search_url"];
        //            TempData["search_name"] = Request["search_name"];
        //            TempData["search_r_url"] = Request["search_r_url"];
        //            TempData["search_description"] = Request["search_description"];
        //            TempData["search_success"] = Request["search_success"];
        //            TempData["search_from_date"] = Request["search_from_date"];
        //            TempData["search_to_date"] = Request["search_to_date"];
        //        }

        //        string search_name = TempData["search_name"] == null ? "" : TempData["search_name"].ToString();
        //        string search_url = TempData["search_url"] == null ? "" : TempData["search_url"].ToString();
        //        string search_r_url = TempData["search_r_url"] == null ? "" : TempData["search_r_url"].ToString();
        //        string search_description = TempData["search_description"] == null ? "" : TempData["search_description"].ToString();
        //        string search_success = TempData["search_success"] == null || TempData["search_success"].ToString().Contains("2") == true ? "" : TempData["search_success"].ToString();
        //        string search_from_date = TempData["search_from_date"] == null ? DateTime.Today.ToShortDateString() : TempData["search_from_date"].ToString();
        //        string search_to_date = TempData["search_to_date"] == null ? DateTime.Today.AddDays(1).ToShortDateString() : TempData["search_to_date"].ToString();

        //        TempData["search_url"] = search_url;
        //        TempData["search_name"] = search_name;
        //        TempData["search_r_url"] = search_r_url;
        //        TempData["search_description"] = search_description;
        //        TempData["search_success"] = search_success;
        //        TempData["search_from_date"] = search_from_date;
        //        TempData["search_to_date"] = search_to_date;
        //        DateTime fdate = DateTime.Parse(search_from_date);
        //        DateTime tdate = DateTime.Parse(search_to_date + " 23:59:59");
        //        TempData.Keep();
        //        #endregion

        //        var logHistory = new List<IMSLog>();
        //        //string search_code = CommonFunc.ConvertNonUnicodeURL(search_name).ToLower();
        //        search_url = search_url.ToLower();
        //        search_name = search_name.ToLower();
        //        search_r_url = search_r_url.ToLower();
        //        search_description = search_description.ToLower();
        //        bool search_success_code = true;
        //        if (!string.IsNullOrEmpty(search_success) && search_success.Contains("0") == true)
        //            search_success_code = false;
        //        else if (!string.IsNullOrEmpty(search_success) && search_success.Contains("1") == true)
        //            search_success_code = true;

        //        if (access.Any(k => k.Key.Equals("log_viewall")) == true && access["log_viewall"] == true)
        //        {
        //            logHistory = db.IMSLogs.Where(l => (search_name == "" || l.CreateBy.Contains(search_name)) &&
        //                                                (search_url == "" || l.Url.Contains(search_url)) &&
        //                                                (search_r_url == "" || l.RequestUrl.Contains(search_r_url)) &&
        //                                                (search_description == "" || l.Description.Contains(search_description)) &&
        //                                                (search_success == "" || l.Success == search_success_code) &&
        //                                                (l.CreateOn >= fdate && l.CreateOn <= tdate))
        //                                                .ToList();
        //        }
        //        else
        //        {
        //            return Redirect("/home/forbidden");
        //        }
        //        return View(logHistory);
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["e"] = ex.Message;
        //        return View(new List<IMSLog>());
        //    }
        //}
        [HttpPost]
        public ActionResult LoadListLog(IDataTablesRequest dataTablesRequest, DateTime? FromDate, DateTime? ToDate, string search_url, string search_name, string search_r_url, string search_salon, bool? search_success)
        {
            var query = from log in db.IMSLogs select log;
            int totalRecord = 0;
            if (FromDate != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.CreateOn) >= FromDate);
            }
            //filter data to date
            if (ToDate != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.CreateOn) <= ToDate);
            }
            if (!string.IsNullOrEmpty(search_url))
            {
                query = query.Where(x => x.Url.Contains(search_url.Trim()));
            }
            if (!string.IsNullOrEmpty(search_name))
            {
                query = query.Where(x => x.CreateBy.Contains(search_name.Trim()));
            }
            if (!string.IsNullOrEmpty(search_r_url))
            {
                query = query.Where(x => x.RequestUrl.Contains(search_r_url.Trim()));
            }
            if (!string.IsNullOrEmpty(search_salon))
            {
                query = query.Where(x => x.SalonName.Contains(search_salon.Trim()));
            }
            if (search_success != null)
            {
                query = query.Where(x => x.Success == search_success);
            }
            totalRecord = query.Count();
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch.Name)
            {
                case "CreateOn":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.CreateOn);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.CreateOn);
                    }

                    break;
                case "Url":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.Url);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.Url);
                    }

                    break;
                case "CreateBy":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.CreateBy);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.CreateBy);
                    }
                    break;
                case "RequestUrl":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.RequestUrl);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.RequestUrl);
                    }
                    break;
                case "Method":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.RequestMethod);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.RequestMethod);
                    }
                    break;
                case "StatusCode":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.StatusCode);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.StatusCode);
                    }
                    break;
                case "SalonName":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.SalonName);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.SalonName);
                    }
                    break;
                default:
                    query = query.OrderByDescending(s => s.CreateOn);
                    break;
            }
            query = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var data = query.ToList();
            var ViewData = data.Select(x => new
            {
                x.Id,
                x.CreateBy,
                CreateOn = x.CreateOn.HasValue ? string.Format("{0:r}", x.CreateOn) : string.Empty,
                x.Description,
                x.JsonRequest,
                x.JsonRespone,
                x.RequestMethod,
                x.StatusCode,
                x.RequestUrl,
                x.Success,
                x.Url,
                x.SalonName
            });
            return Json(new
            {
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                draw = dataTablesRequest.Draw,
                data = ViewData
            });
        }

        // GET: IMSLogs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IMSLog iMSLog = db.IMSLogs.Find(id);
            if (iMSLog == null)
            {
                return HttpNotFound();
            }
            return View(iMSLog);
        }
        [HttpPost]

        public JsonResult GetLogById(string id)
        {
            if (id == null)
            {
                return Json(null);
            }
            IMSLog iMSLog = db.IMSLogs.Find(id);
            if (iMSLog == null)
            {
                return Json(null);
            }
            return Json(iMSLog);
        }
        // GET: IMSLogs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: IMSLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public bool Create([Bind(Include = "Id,CreateBy,CreateOn,Url,RequestUrl,RequestMethod,StatusCode,Success,JsonRequest,JsonRespone,Description")] IMSLog iMSLog)
        {
            if (ModelState.IsValid)
            {
                db.IMSLogs.Add(iMSLog);
                db.SaveChanges();
                //return RedirectToAction("Index");
                return true;
            }

            //return View(iMSLog);
            return false;
        }

        [HttpPost]
        public ActionResult SendApi(RequestModel request)
        {
            try
            {
                dynamic jsonRequest =null;
                if (!string.IsNullOrEmpty(request.JsonRequest))
                {
                     jsonRequest = JsonConvert.DeserializeObject(request.JsonRequest);
                }
                HttpResponseMessage result = ClientRestAPI.CallRestApi(request.Url.Trim(), "", "", request.Method, jsonRequest);
                var response = new ResponseModel();
                response.Method = request.Method;
                if (result == null || result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return Json(new { status = false, message = "error code" });
                }
                if(result.StatusCode == HttpStatusCode.OK)
                {
                    response.Success = true;
                }
                response.Url = request.Url;
                response.StatusCode = (int)result.StatusCode;
                response.JsonResponse = result.Content.ReadAsStringAsync().Result;
                return Json(new { status = true, message = "send success", response = response });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
       
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
