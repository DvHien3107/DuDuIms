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
using System.Xml;
using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using Register.Mango.Models;

namespace Register.Mango.Controllers
{
    public class SubscribeController : Controller
    {
        private WebDataModel db = new WebDataModel();
        private SalesLeadService _registerMangoService = new SalesLeadService();

        // GET: Subscribe
        public ActionResult Index(string license_code)
        {
     
            if (!string.IsNullOrEmpty(license_code))
            {
                var license = db.License_Product.Where(x => x.Code.Trim().ToLower() == license_code.Trim().ToLower() && x.isAddon ==false).FirstOrDefault();
                if (license == null)
                {
                    return Redirect("/");
                }
                ViewBag.License_Code = license.Code;
            }
            ViewBag.State = db.Ad_USAState.Where(x => x.country == "USA").ToList();
            return View();
        }

        public ActionResult Success(string key)
        {
            if (string.IsNullOrEmpty(key))
                return Redirect("/");
            string email = SecurityLibrary.Decrypt(key);
            if(string.IsNullOrEmpty(email))
                return Redirect("/");
            var sl = db.C_SalesLead.Where(x => x.L_Email == email).FirstOrDefault();
            if (sl == null)
                return Redirect("/");
            if (sl.L_IsSendMail == true)
            {
                ViewBag.IsSendEmail = true;
            }
            else
            {
                ViewBag.IsSendEmail = false;
            }
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
     
        public async Task<JsonResult> Create(SubscribeModel subscribeModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //check exist email
                    if (db.C_Customer.Any(m => (m.SalonEmail == subscribeModel.L_Email && !string.IsNullOrEmpty(subscribeModel.L_Email))) ||
                        db.C_SalesLead.Any(m => (m.L_Email == subscribeModel.L_Email && !string.IsNullOrEmpty(subscribeModel.L_Email))))
                    {
                        return Json(new object[] { false, "Email already registered, please try again with another email or contact us for assistance" });
                    }
                    //check exist phone
                    subscribeModel.L_Phone = CommonFunc.CleanPhone(subscribeModel.L_Phone);
                    if (db.C_Customer.Any(m => (m.BusinessPhone == subscribeModel.L_Phone && !string.IsNullOrEmpty(subscribeModel.L_Phone))) ||
                        db.C_SalesLead.Any(m => (m.L_Phone == subscribeModel.L_Phone && !string.IsNullOrEmpty(subscribeModel.L_Phone))))
                    {
                        return Json(new object[] { false, "Phone number already in exist!" });
                    }
                    ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                    XmlNode node = xml.GetNode("/root/customer/word_determine");
                    var sl = new C_SalesLead();
                    sl.Id = Guid.NewGuid().ToString().Replace("-","");
                    sl.L_Email = subscribeModel.L_Email;
                    sl.L_ContactName = subscribeModel.L_ContactName;
                    sl.L_Phone = subscribeModel.L_Phone;
                    sl.L_Password = subscribeModel.L_Password;
                    sl.L_City = subscribeModel.L_City;
                    sl.L_State = subscribeModel.L_State;
                    sl.SalonTimeZone = subscribeModel.L_Timezone;
                    var listTimeZone = new MerchantService().ListTimeZone();
                    var listIMSTimeZone = new string[] { "Eastern", "Central", "Mountain", "Pacific", "VietNam" };
                    if (listIMSTimeZone.Any(t => t.Contains(subscribeModel.L_Timezone)))
                    {
                        sl.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(subscribeModel.L_Timezone).Text();

                    }
                    else
                    {
                        sl.SalonTimeZone_Number = listTimeZone.FirstOrDefault(t => t.Name == subscribeModel.L_Timezone)?.TimeDT;
                    }
                    //sl.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER_BY_ID>(subscribeModel.L_Timezone).Text();
                    sl.CreateAt = DateTime.UtcNow;
                    sl.CreateBy = "Subscribe on Mango";
                    sl.L_Type = LeadType.SubscribeMango.Text();
                    sl.L_ContactPhone = subscribeModel.L_Phone;
                    sl.L_SalonName = subscribeModel.L_BusinessName;
                    sl.L_IsVerify = false;
                    sl.SL_Status = LeadStatus.Lead.Code<int>();
                    sl.SL_StatusName = LeadStatus.Lead.Text();
                    sl.L_Product = ListLicensesTrial.Default.Text();
                    //sl.SalonTimeZone = TIMEZONE_NUMBER_BY_ID.Eastern.Code<string>();
                    //sl.SalonTimeZone_Number = TIMEZONE_NUMBER_BY_ID.Eastern.Text();
                    sl.L_Version = node["trial"].InnerText;
                    bool sendEmail = false;
                    if (!string.IsNullOrEmpty(subscribeModel.L_Packages))
                    {
                        var license = db.License_Product.Where(x => x.Code.Trim().ToLower() == subscribeModel.L_Packages.Trim().ToLower() && x.isAddon == false).FirstOrDefault();
                        if (license != null)
                        {
                            sl.L_License_Name = license.Name;
                            sl.L_License_Code = license.Code;
                            if (license.Trial_Months > 0 && false)
                            {
                                sendEmail = true;
                            }
                        }
                    }
                    var moreinfo = new MoreInfo();
                    moreinfo.NumberBranches = subscribeModel.L_NumberofSalons;
                    moreinfo.NumberEmployees = subscribeModel.L_NumberofTechnicians;
                    moreinfo.ServicePackage = subscribeModel.L_Packages;
                    sl.L_MoreInfo = JsonConvert.SerializeObject(moreinfo);
                    db.C_SalesLead.Add(sl);
                    db.SaveChanges();
                    if (sendEmail)
                    {
                        await _registerMangoService.SendMailVerify(email: sl.L_Email, name: sl.L_ContactName, phone: sl.L_Phone, link: AppConfig.Host + "/verify?key=" + SecurityLibrary.Encrypt(sl.L_Email));
                    }
                    await _registerMangoService.SendMailSubscribe(sl.L_ContactName, sl.L_ContactPhone, sl.L_Email);
                    string email = SecurityLibrary.Encrypt(sl.L_Email);
                    return Json(new object[] { true, "Subscribe success" , email });
                }
                throw new Exception("Invalid information");
            }
            catch (Exception e)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
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
