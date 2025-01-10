using API.IMS.JSON.ActionResults;
using API.IMS.Models;
using Enrich.Core.Infrastructure;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Security;

namespace API.IMS.Controllers
{
    public class LicenseController : ApiController
    {
        private WebDataModel db = new WebDataModel();

        // GET: api/License
        /// <summary>
        /// Get list license in IMS
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(ResponseLicense))]
        [Route("api/License")]
        [HttpGet]
        public IHttpActionResult GetLicense()
        {
            try
            {
                var licenses = db.License_Product.Where(l => l.Available == true && l.Active == true).ToList();
                var resLicense = new List<License>();
                licenses.ForEach(c =>
                {
                    var baseservices = db.License_Product_Item.Where(l => l.License_Product_Id == c.Id && l.Enable == true)
                    .Join(db.License_Item.Where(l => l.GroupName == "Base Services"), lp => lp.License_Item_Code, li => li.Code, (lp, li) => new BaseService_Define
                    {
                        licenseCode = li.Code,
                        licenseType = li.Type,
                        count_limit = lp.Value ?? 1,
                        count_warning_value = 0,
                        status = "active"
                    }).ToList();
                    var features = db.License_Product_Item.Where(l => l.License_Product_Id == c.Id && l.Enable == true)
                    .Join(db.License_Item.Where(l => l.GroupID == 1000000), lp => lp.License_Item_Code, li => li.Code, (lp, li) => new Feature
                    {
                        IdKey = li.Code,
                        FeatureNames = li.Name,
                        Category = li.Name,
                        Description = li.Description,
                        RequestNum = li.Type == "COUNT",
                        NumRequest = lp.Value ?? 1
                    }).ToList();
                    resLicense.Add(new License
                    {
                        Name = c.Name,
                        Code = c.Code,
                        Price = c.Price ?? decimal.Zero,
                        Type = c.Type,
                        SubscriptionDuration = c.SubscriptionDuration,
                        SubscriptionEndWarningDays = c.SubscriptionEndWarningDays ?? 0,
                        TrialDay = ConvertMonthsToDaysTrial(c.Trial_Months),
                        AllowDemo = c.AllowDemo ?? false,
                        AllowSlice = c.AllowSlice ?? false,
                        PromoOfMonth = c.Promotion_Apply_Months ?? 0,
                        PromoPrice = int.Parse((c.Promotion_Price ?? 0).ToString()),
                        Level = c.Level ?? -1,
                        BasesServices = baseservices,
                        Features = features
                    });
                });
                return Ok(new ResponseLicense
                {
                    status = "200",
                    message = "Success",
                    data = resLicense.OrderByDescending(o => o.Type).ToList()
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseLicense
                {
                    status = "400",
                    message = ex.Message
                });
            }
        }

        // POST: api/ChangeLicense
        /// <summary>
        /// Store change license (Subscription / Add-on)
        /// </summary>
        /// <param name="StoreCode">Store Code</param>
        /// <param name="LicenseCode">License Code</param>
        /// <param name="Quantity">this is the month if the product is license / is the quantity if the product is an addon (default is 1)</param>
        /// <param name="OnlyRegularPrice">true: real price / false: trial or promotion</param>
        /// <param name="Renew">Applies only to the license and its default is: true</param>
        /// <returns></returns>
        [Route("api/ChangeLicense")]
        [AllowAnonymous]
        //[HttpPost]
        [ResponseType(typeof(ResponsePaymentOrder))]
        public async Task<IHttpActionResult> ChangeLicense(string StoreCode, string LicenseCode, int Quantity = 1, bool Renew = false, bool OnlyRegularPrice = true)
        {
            if (string.IsNullOrEmpty(StoreCode)|| string.IsNullOrEmpty(LicenseCode))
            {
                var data = new ResponsePaymentOrder
                {
                    status = "400",
                    message = "StoreCode and LicenseCode is required"
                };
                string reponse = JsonConvert.SerializeObject(data);
                return new ErrorActionResult(reponse, HttpStatusCode.BadRequest);
            }
            try
            {
                var cus = db.C_Customer.FirstOrDefault(c => c.StoreCode == StoreCode);
                if (cus == null)
                    return Ok(new ResponsePaymentOrder
                    {
                        status = "404",
                        message = "Store not found"
                    });
                var lic = db.License_Product.FirstOrDefault(l => l.Code == LicenseCode);
                if (lic == null)
                    return Ok(new ResponsePaymentOrder
                    {
                        status = "404",
                        message = "License not found"
                    });

                var resLicense = new List<License>();
                var systemaccount = ConfigurationManager.AppSettings["SystemAccount"]?.ToString();
                var member = db.P_Member.FirstOrDefault(c => c.PersonalEmail == systemaccount);

                string StoreApply = string.Empty;
                if (OnlyRegularPrice == true)
                {
                    StoreApply = Store_Apply_Status.Real.Text();
                }
                else
                {
                    if (lic.Trial_Months > 0)
                        StoreApply = Store_Apply_Status.Trial.Text();
                    else
                        StoreApply = Store_Apply_Status.Promotional.Text();
                }

                HttpContext.Current.Session["member"] = member;
                FormsAuthentication.SetAuthCookie(member.PersonalEmail, false);
                Authority.GetAccessAuthority(true);
                var orderController = EngineContext.Current.Resolve<OrderController>();


                var jsonAddon =  await orderController.add_addon(lic.Id, cus.CustomerCode, DateTime.UtcNow, null, false, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, Quantity, false, StoreApply);
                var order = db.O_Orders.Where(c => c.CustomerCode == cus.CustomerCode).OrderByDescending(c => c.CreatedAt).FirstOrDefault();
                cus.CheckPassword();
                string urlPayment = ConfigurationManager.AppSettings["IMSUrl"] + "/PaymentGate/Pay/?key=" + order.OrdersCode.ToBase64() + ":" + cus?.MD5PassWord.ToBase64();
                return Ok(new ResponsePaymentOrder
                {
                    status = "200",
                    message = "Success",
                    addonResult = jsonAddon,
                    data = new ResponseOrder
                    {
                        OrderCode = order.OrdersCode,
                        Total = order.GrandTotal ?? decimal.Zero,
                        Url = urlPayment
                    }
                });
            }
            catch (Exception ex)
            {
                var data = new ResponsePaymentOrder
                {
                    status = "400",
                    message = ex.Message
                };
                string reponse = JsonConvert.SerializeObject(data);
                return new ErrorActionResult(reponse, HttpStatusCode.BadRequest);
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

        private bool License_ProductExists(string id)
        {
            return db.License_Product.Count(e => e.Id == id) > 0;
        }

        private int ConvertMonthsToDaysTrial(int? trialmonth)
        {
            if (trialmonth > 0)
            {
                var Now = DateTime.UtcNow;
                var NextTrialMonths = DateTime.UtcNow.AddMonths(trialmonth.Value);

                return (int)(NextTrialMonths - Now).TotalDays;
            }
            else
            {
                return 0;
            }

        }
    }
}