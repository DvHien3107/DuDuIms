using API.IMS.JSON.ActionResults;
using API.IMS.Models;
using EnrichcousBackOffice.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Description;

namespace API.IMS.Controllers
{
    public class StoreController : ApiController
    {
        private WebDataModel db = new WebDataModel();

        // GET: api/Store/5
        /// <summary>
        /// Get store profile by store code (idIms)
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(ResponseStore))]
        [Route("api/store/{StoreCode}")]
        [HttpGet]
        public IHttpActionResult GetStore(string StoreCode)
        {
            if (string.IsNullOrEmpty(StoreCode))
            {
                var data = new ResponseStore
                {
                    status = "400",
                    message = "StoreCode param is required"
                };
                string reponse = JsonConvert.SerializeObject(data);
                return new ErrorActionResult(reponse,HttpStatusCode.BadRequest);
            }
            try
            {
                C_Customer cus = db.C_Customer.FirstOrDefault(c => c.StoreCode.Equals(StoreCode));
                var storeInfo = db.Store_Services.Where(s => s.StoreCode.Equals(StoreCode) && s.Active == 1).FirstOrDefault();
                if (cus == null)
                    return Ok(new ResponseStore
                    {
                        status = "404",
                        message = "Store not found"
                    });

                var baseservices = (from ss in db.Store_Services
                                    where ss.StoreCode.Equals(cus.StoreCode) && ss.Active == 1
                                    join lp in db.License_Product on ss.ProductCode equals lp.Code
                                    join lpi in db.License_Product_Item on lp.Id equals lpi.License_Product_Id
                                    join li in db.License_Item on lpi.License_Item_Code equals li.Code
                                    where li.GroupName == "Base Services"
                                    select new BaseService
                                    {
                                        serviceCode = li.Code,
                                        serviceName = li.Name,
                                        count_limit = lpi.Value ?? 0,
                                        start_period = ss.EffectiveDate.ToString(),
                                        end_period = ss.RenewDate.ToString(),
                                        status = "active",
                                    }).ToList();
                baseservices = baseservices.GroupBy(l => l.serviceCode)
                                            .Select(li => new BaseService
                                            {
                                                serviceCode = li.First().serviceCode,
                                                serviceName = li.First().serviceName,
                                                count_limit = li.Sum(s => s.count_limit),
                                                start_period = li.First().start_period,
                                                end_period = li.First().end_period,
                                                status = li.First().status,
                                            }).ToList();

                var features = (from ss in db.Store_Services
                                where ss.StoreCode.Equals(cus.StoreCode) && ss.Active == 1
                                join lp in db.License_Product on ss.ProductCode equals lp.Code
                                join lpi in db.License_Product_Item on lp.Id equals lpi.License_Product_Id
                                join li in db.License_Item on lpi.License_Item_Code equals li.Code
                                where li.GroupID == 1000000
                                select new Feature
                                {
                                    IdKey = li.Code,
                                    FeatureNames = li.Name,
                                    Category = li.Name,
                                    Description = li.Description,
                                    RequestNum = li.Type == "COUNT",
                                    NumRequest = lpi.Value ?? 1
                                }).ToList();
                features = features.GroupBy(f => f.IdKey)
                                    .Select(f => new Feature
                                    {
                                        IdKey = f.First().IdKey,
                                        FeatureNames = f.First().FeatureNames,
                                        Category = f.First().Category,
                                        Description = f.First().Description,
                                        RequestNum = f.First().RequestNum,
                                        NumRequest = f.Sum(s => s.NumRequest)
                                    }).ToList();


                var storeprofile = new StoreProfile()
                {
                    storeId = cus.StoreCode,
                    storeName = cus.BusinessName,
                    contactName = cus.ContactName,
                    lastUpdate = storeInfo?.LastUpdateAt?.ToString("MM/dd/yyyy"),
                    updateBy = storeInfo?.LastUpdateBy,
                    email = string.IsNullOrWhiteSpace(cus.SalonEmail) == true ? cus.BusinessEmail : cus.SalonEmail,
                    password = cus.MD5PassWord,
                    cellPhone = cus.BusinessPhone,
                    createBy = cus.CreateBy,
                    createAt = cus.CreateAt?.ToString("MM/dd/yyyy"),
                    status = db.Store_Services.Any(ss => ss.StoreCode.Equals(StoreCode)) == true ? "Activated" : "Deactivated",
                    businessName = cus.BusinessName,
                    businessPhone = string.IsNullOrWhiteSpace(cus.SalonPhone) == true ? cus.BusinessPhone : cus.SalonPhone,
                    businessEmail = string.IsNullOrWhiteSpace(cus.SalonEmail) == true ? cus.BusinessEmail : cus.SalonEmail,
                    businessAddress = cus.AddressLine(),
                    licenseCode = storeInfo?.ProductCode,
                    licenseName = storeInfo?.Productname,
                    baseservices = baseservices,
                    features = features
                };
                return Ok(new ResponseStore
                {
                    status = "200",
                    message = "Success",
                    data = storeprofile
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseStore
                {
                    status = "400",
                    message = ex.Message
                });
            }
        }

        // GET: api/Store/gettransaction/5
        /// <summary>
        /// Get transaction  by store code (idIms)
        /// </summary>
        /// <param name="StoreCode">IMS Store#</param>
        /// <returns></returns>
        [ResponseType(typeof(ResponseTransaction))]
        [Route("api/store/gettransaction/{StoreCode}")]
        [HttpGet]
        public IHttpActionResult GetTransactionStore(string StoreCode)
        {
            if (string.IsNullOrEmpty(StoreCode))
            {
                var data = new ResponseStore
                {
                    status = "400",
                    message = "StoreCode param is required"
                };
                string reponse = JsonConvert.SerializeObject(data);
                return new ErrorActionResult(reponse, HttpStatusCode.BadRequest);
            }
            try
            {
                C_Customer cus = db.C_Customer.FirstOrDefault(c => c.StoreCode.Equals(StoreCode));
                if (cus == null)
                    return Ok(new ResponseTransaction
                    {
                        status = "404",
                        message = "Store not found"
                    });

                var trans = db.C_CustomerTransaction.Where(x => x.CustomerCode == cus.CustomerCode);
                var data = trans.ToList().Select(x => new StoreTransaction
                {
                    OrderCode = x.OrdersCode,
                    Status = x.PaymentStatus,
                    Total = x.Amount,
                    Date = x.CreateAt == null ? "" : x.CreateAt.Value.ToString("MM/dd/yyyy hh:mm tt"),
                    TransToken = x.MxMerchant_token ?? "",
                    Url = ConfigurationManager.AppSettings["IMSUrl"] + "order/ImportInvoiceToPDF?_code=" + x.OrdersCode,
                    Subscription = db.Order_Subcription.Where(o => o.OrderCode == x.OrdersCode).Select(s => new ProductBaseDto { Code = s.Product_Code, Name = s.ProductName, Amount = s.Price, Quantity = s.Quantity ?? 1 }).ToList(),
                    Hardware = db.Order_Products.Where(o => o.OrderCode == x.OrdersCode).Select(h => new ProductBaseDto { Code = h.ModelCode, Name = h.ModelName, Amount = h.Price, Quantity = h.Quantity ?? 1 }).ToList(),
                    PaymentInfo = db.C_CustomerCard.Where(c => c.Id == x.Card).Select(c => new CreditOnShow { CardNumber = c.CardNumber, CardType = c.CardType, CardHolderName = c.CardHolderName, StreetAddress = c.CardAddressStreet, State = c.CardState, City = c.CardCity, Zipcode = c.CardZipCode, Country = c.CardCountry }).FirstOrDefault() ?? new CreditOnShow { CardNumber = x.CardNumber }
                }).ToList();

                return Ok(new ResponseStore
                {
                    status = "200",
                    message = "Success",
                    data = data
                });
            }
            catch (Exception ex)
            {
                return Ok(new ResponseStore
                {
                    status = "400",
                    message = ex.Message
                });
            }
        }




        /// <summary>
        /// get thông tin thẻ của khách
        /// </summary>
        /// <param name="StoreCode">IMS Store#</param>
        /// <returns></returns>
        [ResponseType(typeof(ResponseStore))]
        [Route("api/store/getpaymentmethod/{StoreCode}")]
        [HttpGet]
        public IHttpActionResult GetPaymentMethod(string StoreCode)
        {
            if (string.IsNullOrEmpty(StoreCode))
            {
                var data = new ResponseStore
                {
                    status = "400",
                    message = "StoreCode param is required"
                };
                string reponse = JsonConvert.SerializeObject(data);
                return new ErrorActionResult(reponse, HttpStatusCode.BadRequest);
            }
            try
            {

                var pm = (from cc in db.C_CustomerCard.Where(c => c.StoreCode.Equals(StoreCode))
                          select new PaymentCredit
                          {
                              ID = cc.Id,
                              Active = cc.Active,
                              Default = cc.IsDefault,
                              CardExpiry = cc.CardExpiry,
                              CardHolderName = cc.CardHolderName,
                              CardNumber = cc.CardNumber,
                              CardType = cc.CardType,
                              City = cc.CardCity,
                              Country = cc.CardCountry,
                              State = cc.CardState,
                              StreetAddress = cc.CardAddressStreet,
                              Zipcode = cc.CardZipCode
                          }).ToList();

                return Ok(new ResponseStore
                {
                    status = "200",
                    message = "Success",
                    data = pm
                });
            }
            catch (Exception e)
            {

                return Ok(new ResponseStore
                {
                    status = HttpStatusCode.BadRequest.ToString(),
                    message = e.InnerException == null ? e.Message : e.InnerException.Message,
                    data = new List<PaymentCredit>()
                });
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