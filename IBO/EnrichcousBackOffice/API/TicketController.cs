 using EnrichcousBackOffice.ViewModel;
using System;
using System.Linq;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.App_Start;
using EnrichcousBackOffice.ViewControler;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Http.Results;
using EnrichcousBackOffice.API.DTO;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Inner.Libs.Validation;
using Newtonsoft.Json;
using System.Web.Http.Cors;

namespace EnrichcousBackOffice.API
{
    //[APIAuthorize]
    //[RoutePrefix("api/[Controller]")]
    [AllowAnonymous]
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class TicketController : ApiController
    {
        //private object getByStoreId(string store_id)
        //{
        //    var db = new WebDataModel();
        //    var merchant_code = db.Store_Services.Where(ss => ss.StoreCode == store_id).FirstOrDefault()?.CustomerCode;
        //    var query = (from t in db.T_SupportTicket
        //                 where t.CustomerCode == merchant_code && t.Visible == true && t.TypeId == 5
        //                 join u in from u in db.UploadMoreFiles
        //                           where u.TableName == "T_SupportTicket"
        //                           group u by u.TableId on t.Id equals u.Key into gu
        //                 from us in gu.DefaultIfEmpty()
        //                 join f in from f in db.T_TicketFeedback
        //                           where f.Share == true
        //                           group f by f.TicketId on t.Id equals f.Key into gf
        //                 from fs in gf.DefaultIfEmpty()
        //                 select new ticket
        //                 {
        //                     ticket_id = t.Id,
        //                     ticket_date_opened = t.DateOpened,
        //                     ticket_date_closed = t.DateClosed,
        //                     ticket_name = t.Name,
        //                     ticket_description = t.Description,
        //                     ticket_attachments = us.Select(u => u.FileName).ToList(),
        //                     //                    ticket_status_id = t.StatusId,
        //                     ticket_status = t.StatusName,
        //                     ticket_feedbacks = (from f in fs
        //                                         select new ticket_feedbacks
        //                                         {
        //                                             feedback_id = f.Id,
        //                                             feedback_create_at = f.CreateAt,
        //                                             feedback_attachments = new List<string>() { f.Attachments },
        //                                             feedback_tittle = f.FeedbackTitle,
        //                                             feedback_content = f.Feedback,
        //                                             feedback_by = f.CreateByName
        //                                         }).OrderByDescending(fb => fb.feedback_create_at).ToList(),
        //                 }).ToList();

        //    query.ForEach(item =>
        //    {
        //        item.ticket_attachments = item.ticket_attachments.Select(ta => ToAbsoluteUrl(ta)).ToList();
        //        item.ticket_feedbacks.ForEach(fb => fb.feedback_attachments = fb.feedback_attachments[0]?.Split(';').Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => ToAbsoluteUrl(a)).ToList());
        //    });

        //    return query;
        //}

        private string ToAbsoluteUrl(string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl))
                return relativeUrl;

            if (HttpContext.Current == null)
                return relativeUrl;

            if (relativeUrl.StartsWith("/"))
                relativeUrl = relativeUrl.Insert(0, "~");
            if (!relativeUrl.StartsWith("~/"))
                relativeUrl = relativeUrl.Insert(0, "~/");

            var url = HttpContext.Current.Request.Url;
            var port = url.Port != 80 ? (":" + url.Port) : String.Empty;

            return String.Format("{0}://{1}{2}{3}",
                url.Scheme, url.Host, port, VirtualPathUtility.ToAbsolute(relativeUrl));
        }


        #region POST: Create ticket

        //[HttpPost]
        //public object Post([FromBody] T_SupportTicket tkModel)
        //{
        //    try
        //    {
        //        WebDataModel db = new WebDataModel();
        //        if (tkModel.Id > 0 && db.T_SupportTicket.Any(tk => tk.Id == tkModel.Id) == true)
        //        {
        //            //update ticket
        //            //UpdateTicket(tkModel);
        //        }
        //        else
        //        {
        //            //New ticket
        //            NewTicket(tkModel);
        //        }
        //        return new ResponseModel { Result = 1 };
        //    }
        //    catch (Exception e)
        //    {
        //        return new ResponseModel { Message = e.Message, Result = -1 };
        //    }
        //}


        //private void NewTicket(T_SupportTicket tkModel)
        //{
        //    WebDataModel db = new WebDataModel();
        //    //ticket
        //    var tic = new T_SupportTicket();
        //    int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
        //                                           && t.CreateAt.Value.Month == DateTime.Today.Month
        //                                           && t.CreateAt.Value.Day == DateTime.Today.Day).Count();

        //    tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMMdd") + (countOfTicket + 1).ToString());
        //    tic.Name = tkModel.Name;
        //    tic.Description = tkModel.Description;
        //    tic.CreateAt = DateTime.UtcNow;
        //    tic.CreateByName = tkModel.CreateByName;
        //    tic.Visible = true;
        //    tic.GroupID = tkModel.GroupID;
        //    tic.GroupName = tkModel.GroupName;
        //    tic.CustomerCode = tkModel.CustomerCode;
        //    tic.CustomerName = tkModel.CustomerName;

        //    //submit
        //    db.T_SupportTicket.Add(tic);
        //    db.SaveChanges();

        //    //auto assign
        //    string message_error = string.Empty;
        //    var result = TicketViewController.AutoAssignment(db, tic, "new").Result;


        //    //return true;

        //}

        #endregion

        [HttpPost]
        [Route("api/ticket")]
        [ValidRequest(Message = "Invalid item request!")]
        public async Task<JsonResult<object>> Create(Ticket request)
        {
            return await Func.JsonHandle(async () =>
            {
                Dictionary<string, object> rs = new Dictionary<string, object>{} ;
                rs["ticket"] = await new TicketServices().CreateTicket(request);
                return Json(Func.JResult.Success(rs));
            }, Json);
        }

        [HttpPost]
        [Route("api/ticket/newfeedback")]
        [ValidRequest(Message = "Invalid item request!")]
        public async Task<JsonResult<object>> NewFeedBack(Feedback request)
        {
            return await Func.JsonHandle(async () =>
            {
                Dictionary<string, object> rs = new Dictionary<string, object>();
                T_TicketFeedback feedback = await new TicketServices().CreateFeedBack(request);
                // Make response
                rs["feedback"] = new Feedback
                {
                    TicketId = feedback.TicketId,
                    Id = feedback.Id,
                    Title = feedback.FeedbackTitle,
                    Content = feedback.Feedback,
                    Attachments = feedback.Attachments?.Split(';').ToList()
                };
                return Json(Func.JResult.Success(rs));
            }, Json);
        }

        [HttpGet]
        [Route("api/ticket/{id}")]
        public JsonResult<object> GetById(long? id)
        {
            return Func.JsonHandle(() =>
            {
                Dictionary<string, object> rs = new Dictionary<string, object>();
                Ticket ticket = new TicketServices().GetTicket(null, id)?.FirstOrDefault();
                rs["ticket"] = ticket;
                return Json(Func.JResult.Success(rs));
            }, Json);
        }

        [HttpGet]
        [Route("api/ticket/store/{storeId}")]
        public JsonResult<object> GetByStoreId(string storeId)
        {
            return Func.JsonHandle(() =>
            {
                Dictionary<string, object> rs = new Dictionary<string, object>();
                rs["tickets"] = new TicketServices().GetTicket(storeId);
                return Json(Func.JResult.Success(rs));
            }, Json);
        }
    }

    internal class ticket
    {
        public long ticket_id { get; set; }
        public DateTime? ticket_date_opened { get; set; }
        public DateTime? ticket_date_closed { get; set; }
        public string ticket_name { get; set; }
        public string ticket_description { get; set; }
        public List<string> ticket_attachments { get; set; }
        public string ticket_status { get; set; }
        public long? ticket_status_id { get; set; }
        public List<ticket_feedbacks> ticket_feedbacks { get; set; }
    }

    internal class ticket_feedbacks
    {
        public long? feedback_id { get; set; }
        public DateTime? feedback_create_at { get; set; }
        public List<string> feedback_attachments { get; set; }
        public string feedback_tittle { get; set; }
        public string feedback_content { get; set; }
        public string feedback_by { get; set; }
        public string feedback_by_number { get; set; }
    }
}