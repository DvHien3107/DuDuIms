using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;

namespace Register.MangoVietnam.Controllers
{
    public class SubscribeController : Controller
    {
        private WebDataModel db = new WebDataModel();
        private RegisterMangoService _registerMangoService = new RegisterMangoService();

        // GET: Subscribe
        public ActionResult Index()
        {
            return View();
        }

        // GET: Subscribe/Details/5
        public ActionResult Success()
        {
            return View();
        }

        // GET: Subscribe/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Subscribe/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create([Bind(Include = "L_Email,L_Password,L_ContactName,L_Phone")] C_SalesLead c_SalesLead)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //check exist email
                    if (db.C_Customer.Any(m => (m.SalonEmail == c_SalesLead.L_Email && !string.IsNullOrEmpty(c_SalesLead.L_Email))) ||
                        db.C_SalesLead.Any(m => (m.L_Email == c_SalesLead.L_Email && !string.IsNullOrEmpty(c_SalesLead.L_Email))))
                    {
                        return Json(new object[] { false, "Email đã được đăng ký, vui lòng thử lại với email khác hoặc liên hệ với chúng tôi để được hỗ trợ" });
                    }
                    //check exist phone
                    c_SalesLead.L_Phone = CommonFunc.CleanPhone(c_SalesLead.L_Phone);
                    if (db.C_Customer.Any(m => (m.BusinessPhone == c_SalesLead.L_Phone && !string.IsNullOrEmpty(c_SalesLead.L_Phone))) ||
                        db.C_SalesLead.Any(m => (m.L_Phone == c_SalesLead.L_Phone && !string.IsNullOrEmpty(c_SalesLead.L_Phone))))
                    {
                        return Json(new object[] { false, "Số điện thoại đã được sử dụng!" });
                    }

                    c_SalesLead.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                    c_SalesLead.CreateAt = DateTime.UtcNow;
                    c_SalesLead.CreateBy = "Subscribe on Mango";
                    c_SalesLead.L_Type = LeadType.SubscribeMango.Text();
                    c_SalesLead.L_ContactPhone = c_SalesLead.L_Phone;
                    c_SalesLead.L_IsVerify = false;
                    c_SalesLead.SL_Status = LeadStatus.Lead.Code<int>(); ;
                    c_SalesLead.SL_StatusName = LeadStatus.Lead.Text();
                    c_SalesLead.L_Product = ListLicensesTrial.Default.Text();
                    db.C_SalesLead.Add(c_SalesLead);
                    db.SaveChanges();

                    await _registerMangoService.SendMailSubscribe(c_SalesLead.L_ContactName, c_SalesLead.L_ContactPhone, c_SalesLead.L_Email);

                    return Json(new object[] { true, "Đăng ký thành công"});
                }
                throw new Exception("Thông tin không hợp lệ");
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
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
