using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using Register.Mango.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Mvc;
using System.Xml;
using static EnrichcousBackOffice.ViewControler.TicketViewService;

namespace Register.Mango.Controllers
{
    public class RegisterController : Controller
    {
        WebDataModel db = new WebDataModel();
        private readonly SalesLeadService _registerMangoService = new SalesLeadService();
        #region Utilities
        #endregion
        #region step 1 register
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(RegisterModel model)
        {
            try
            {
                model.PhoneNumber = CommonFunc.CleanPhone(model.PhoneNumber);
                model.CreatedDate = DateTime.Now;
                //check exist email
                if (db.C_Customer.Where(m => m.SalonEmail == model.Email).Count() > 0 || db.C_SalesLead.Where(m => m.L_Email == model.Email).Count() > 0)
                {
                    return Json(new object[] { false, "Email already exists ! Please try again with another email or contact us for assistance" });
                }
                //check exist phone
                //if (db.C_Customer.Any(m => (m.BusinessPhone == model.PhoneNumber && !string.IsNullOrEmpty(model.PhoneNumber))))
                //{
                //    return Json(new object[] { false, "Phone number already exists!" });
                //}
                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/customer/word_determine");

                var sl = new C_SalesLead();
                sl.CreateAt = DateTime.UtcNow;
                sl.CreateBy = "Trial Register";
                sl.Id = Guid.NewGuid().ToString().Replace("-", "");
                sl.SL_Status = LeadStatus.Lead.Code<int>(); ;
                sl.SL_StatusName = LeadStatus.Lead.Text();
                sl.PotentialRateScore = 0;
                sl.L_ContactName = sl.L_SalonName = model.FName + " " + model.LName;
                sl.L_Email = model.Email;
                sl.L_Phone = sl.L_ContactPhone = model.PhoneNumber;
                sl.L_Password = model.Password;
                sl.L_Type = LeadType.TrialAccount.Text();
                sl.L_IsVerify = false;
                sl.L_IsSendMail = false;
                sl.L_Product = ListLicensesTrial.MangoTrial.Text();
                sl.L_Version = node["trial"].InnerText;

                db.C_SalesLead.Add(sl);
                db.SaveChanges();
                var key = SecurityLibrary.Encrypt(sl.L_Email);
                return Json(new object[] { true, key });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Sorry ! currently unable to register,please try again later!", e.Message });
            }
        }
        #endregion
        #region step 2 register (update info)
        public ActionResult UpdateInfoStore(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Redirect("/Register");
            }
            string email = SecurityLibrary.Decrypt(key);
            if (db.C_SalesLead.Where(x => x.L_Email == email).Count() == 0)
            {
                return Redirect("/Register");
            }
            ViewBag.State = db.Ad_USAState.Where(x => x.country == "USA").ToList();
            ViewBag.Key = key;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateInfoStore(string key, InfoSalonModel model)
        {
            try
            {
                string email = SecurityLibrary.Decrypt(key);
                var sl = db.C_SalesLead.Where(x => x.L_Email == email).FirstOrDefault();
                sl.L_SalonName = model.SalonName;
                sl.L_Address = model.Address;
                sl.L_City = model.City;
                sl.L_State = model.State;
                sl.L_Zipcode = model.ZipCode;
                sl.L_Country = "United States";
                sl.L_MoreInfo = JsonConvert.SerializeObject(model.MoreInfo);
                db.SaveChanges();
                await _registerMangoService.SendMailVerify(email: sl.L_Email, name: sl.L_ContactName, phone: sl.L_Phone, link: AppConfig.Host + "/verify?key=" + SecurityLibrary.Encrypt(sl.L_Email));
                return Json(new object[] { true, "Register Success" });
            }
            catch
            {
                return Json(new object[] { false, "Sorry ! currently unable to register,please try again later!" });
            }
        }
        #endregion
        #region thanks page
        public ActionResult Thanks()
        {
            ViewBag.linkSaloncenterSlice = AppConfig.Cfg.StoreUrl.V2.SaloncenterSliceLink;
            return View();
        }
        #endregion
        #region create store when user click link email
        public ActionResult Verify(string key)
        {
            var email = SecurityLibrary.Decrypt(key);
            // incorrect key
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.MessageError = "-1";
                return View();
            }
            var sl = db.C_SalesLead.Where(x => x.L_Email == email).FirstOrDefault();
            if (sl == null)
            {
                ViewBag.MessageError = "-1";
                return View();
            }
            // check created trial
            if (db.C_Customer.Any(m => m.Email == email && !string.IsNullOrEmpty(m.WordDetermine)))
            {
                ViewBag.MessageError = "1";
                return View();
            }
            if (sl.L_Version == "Trial" || string.IsNullOrEmpty(sl.L_Version))
            {
                if (!string.IsNullOrEmpty(sl.L_License_Code))
                {
                    var license = db.License_Product.Where(x => x.Code.Trim().ToLower() == sl.L_License_Code.Trim().ToLower() && x.isAddon == false).FirstOrDefault();
                    if (license != null)
                    {
                        if (license.Trial_Months <= 0)
                        {
                            ViewBag.MessageError = "0";
                            return View();
                        }
                    }
                    else
                    {
                        ViewBag.MessageError = "0";
                        return View();
                    }
                }
            }

            //set key 
            ViewBag.Key = key;
            return View();
        }
        //public ActionResult VerifyForEmail(string key)
        //{
        //    var email = SecurityLibrary.Decrypt(key);
        //    // incorrect key
        //    if (string.IsNullOrEmpty(email))
        //    {
        //        ViewBag.MessageError = "-1";
        //        return View();
        //    }
        //    var sl = db.C_Customer.Where(x => x.BusinessEmail == email).FirstOrDefault();
        //    if (sl == null)
        //    {
        //        ViewBag.MessageError = "-1";
        //        return View();
        //    }
        //    // check created trial
        //    if (db.Store_Services.Where(m => m.CustomerCode == sl.CustomerCode).Count() > 0)
        //    {
        //        ViewBag.MessageError = "1";
        //        return View();
        //    }
        //    //set key 
        //    ViewBag.Key = key;
        //    return View();
        //}
        #endregion
        public async Task<JsonResult> Verify_submit(string key, bool salesPerson = false)
        {
            var email = SecurityLibrary.Decrypt(key);
            var sl = db.C_SalesLead.Where(x => x.L_Email == email).FirstOrDefault();
            string SalonNumberLog = "";
            string TypeAction = string.IsNullOrEmpty(sl.L_Version) ? "Trial" : sl.L_Version;
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var sendApi = new Tuple<bool, string>(false, "");
                    if (!salesPerson)
                    {
                        var resultActive = await _registerMangoService.CreateNewRegisterData(Id: sl.Id, phone: sl.L_Phone, TypeAction);
                        if (resultActive.Item1 == false)
                        {
                            scope.Dispose();
                            return Json(new object[] { false, resultActive.Item2 });
                        }
                    }
                    //  create store from pos system
                    var c = db.C_Customer.Where(x => x.BusinessEmail == email).FirstOrDefault();
                    SalonNumberLog = "(#" + c.StoreCode + ")";
                    if (TypeAction == "Slice")
                        sendApi = await CreateSlice(c);
                    else
                        sendApi = await CreateTrial(c);

                    if (sendApi.Item1 == false)
                    {
                        _registerMangoService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "Verify " + TypeAction + " account failed", description: sendApi.Item2, createBy: "IMS System", UpdateLastSalesLead: false);
                        sl.UpdateAt = DateTime.UtcNow;
                        sl.LastUpdateDesc = "Verify " + TypeAction + " account failed";
                        db.SaveChanges();
                        scope.Dispose();
                        return Json(new object[] { false, "The system refuses your confirmation, something is not right, please try again later !" });
                    }
                    _registerMangoService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "Verify " + TypeAction + " account successfully", description: "Verify " + sl.L_Version + " account successfully", createBy: "IMS System", UpdateLastSalesLead: false);
                    sl.UpdateAt = DateTime.UtcNow;
                    sl.LastUpdateDesc = "Verify " + TypeAction + " account successfully";
                    db.SaveChanges();
                    scope.Complete();
                    return Json(new object[] { true, c.SalonEmail, c.ContactName });

                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                    _writeLogErrorService.InsertLogError(ex, (sl.L_SalonName + " " + SalonNumberLog).Trim());
                    return Json(new object[] { false, "The system refuses your confirmation, something is not right !", ex.Message });
                }
            }
        }

        public async Task<Tuple<bool, string>> CreateTrial(C_Customer c)
        {
            try
            {
                var lp = new License_Product();
                var sl = db.C_SalesLead.Where(x => x.CustomerCode == c.CustomerCode).FirstOrDefault();
                if (!string.IsNullOrEmpty(sl.L_License_Code))
                {
                    lp = db.License_Product.Where(x => x.Code == sl.L_License_Code).FirstOrDefault();
                    if (lp == null)
                    {
                        lp = db.License_Product.Where(l => l.isAddon == false && l.AllowDemo == true).FirstOrDefault();
                    }
                }
                else
                {
                    lp = db.License_Product.Where(l => l.isAddon == false && l.AllowDemo == true).FirstOrDefault();
                }
                var lp_code = lp?.Code;
                if (db.Store_Services.Any(s => s.StoreCode == c.StoreCode && s.OrderCode == "" && s.ProductCode == (lp_code ?? "") && s.Active == 1))
                {
                    return Tuple.Create(true, "Slice account has been created!");
                }

                var d = "<table class='table'><thead><tr><th colspan='2'>Customer information for registration trial</th></tr></thead><tbody>" +
               "<tr><th style='width:130px'> - Name: </th><td>" + c.ContactName +
               "</td></tr><tr><th> - Phone: </th><td>" + c.BusinessPhone +
               "</td></tr><tr><th> - Email: </th><td>" + c.BusinessEmail +
               "</td></tr><tr><th> - Salon Name: </th><td>" + c.BusinessName +
               "</td></tr><tr><th> - Address: </th><td>" + c.BusinessAddressStreet + ", " + c.BusinessCity + ", " + c.BusinessState + " " + c.BusinessZipCode + ", United States </td></tr></tbody></table>";
                //var ticketId = TicketViewController.AutoTicketScenario.NewTicketSalesLead(c.CustomerCode, "", d);
                if (!int.TryParse(AppConfig.Cfg.MangoDemoTrial?.TrialDuration, out int demo_duration))
                {
                    demo_duration = 15;
                };

                var prd = new Store_Services
                {
                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                    StoreCode = c.StoreCode,
                    StoreName = c.BusinessName,
                    CustomerCode = c.CustomerCode,
                    AutoRenew = false,
                    EffectiveDate = DateTime.UtcNow,
                    RenewDate = DateTime.UtcNow.AddMonths(lp.Trial_Months.Value),
                    Period = lp?.SubscriptionDuration,
                    ProductCode = lp?.Code,
                    Productname = lp?.Name,
                    StoreApply = Store_Apply_Status.Trial.Text(),
                    OrderCode = "",
                    Product_Code_POSSystem = lp?.Code_POSSystem,
                    Type = LicenseType.LICENSE.Text(),
                    Active = 1,
                    LastUpdateAt = DateTime.UtcNow,
                    LastUpdateBy = ""
                };
                db.Store_Services.Add(prd);
                db.SaveChanges();
                //using (MerchantService service = new MerchantService(true))
                //{
                //    var rs = await service.ApproveAction(c.StoreCode, prd.Id, true, "active");
                //}
                var resultApi = _registerMangoService.CreateStoreFromPosApi(prd.Id, c);
                if (resultApi == false)
                {
                    return Tuple.Create(false, "Cannot connect to mango pos");
                }
                await _registerMangoService.SendMailStoreActive(c);
                await this.CreateOrderAndTicketOrder(c, lp, d);
                return Tuple.Create(true, "Create Trial account success");
            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex, c.BusinessName + " (#" + c.StoreCode + ")");
                return Tuple.Create(false, ex.Message);
            }
        }

        public async Task<Tuple<bool, string>> CreateSlice(C_Customer c)
        {
            try
            {
                var lp = new License_Product();
                var sl = db.C_SalesLead.Where(x => x.CustomerCode == c.CustomerCode).FirstOrDefault();
                if (!string.IsNullOrEmpty(sl.L_License_Code))
                {
                    lp = db.License_Product.Where(x => x.Code == sl.L_License_Code).FirstOrDefault();
                    if (lp == null)
                    {
                        lp = db.License_Product.Where(l => l.AllowSlice == true).FirstOrDefault();
                    }
                }
                else
                {
                    lp = db.License_Product.Where(l => l.AllowSlice == true).FirstOrDefault();
                }
                var lp_code = lp?.Code;
                if (db.Store_Services.Any(s => s.StoreCode == c.StoreCode && s.OrderCode == "" && s.ProductCode == (lp_code ?? "") && s.Active == 1))
                {
                    return Tuple.Create(true, "Slice account has been created!");
                }
                var d = "<table class='table'><thead><tr><th colspan='2'>Customer information for registration trial</th></tr></thead><tbody>" +
               "<tr><th style='width:130px'> - Name: </th><td>" + c.ContactName +
               "</td></tr><tr><th> - Phone: </th><td>" + c.BusinessPhone +
               "</td></tr><tr><th> - Email: </th><td>" + c.BusinessEmail +
               "</td></tr><tr><th> - Salon Name: </th><td>" + c.BusinessName +
               "</td></tr><tr><th> - Address: </th><td>" + c.BusinessAddressStreet + ", " + c.BusinessCity + ", " + c.BusinessState + " " + c.BusinessZipCode + ", United States </td></tr></tbody></table>";
                var ticketId = TicketViewService.AutoTicketScenario.NewTicketSalesLead(c.CustomerCode, "", d);
                var prd = new Store_Services
                {
                    Id = Guid.NewGuid().ToString(),
                    StoreCode = c.StoreCode,
                    StoreName = c.BusinessName,
                    CustomerCode = c.CustomerCode,
                    AutoRenew = false,
                    EffectiveDate = DateTime.UtcNow,
                    RenewDate = DateTime.UtcNow.AddYears(100),
                    Period = lp?.SubscriptionDuration,
                    ProductCode = lp?.Code,

                    Productname = lp?.Name,
                    StoreApply = Store_Apply_Status.Real.Text(),
                    OrderCode = "",
                    Product_Code_POSSystem = lp?.Code_POSSystem,
                    Type = LicenseType.LICENSE.Text(),
                    Active = 1,
                    LastUpdateAt = DateTime.UtcNow,
                    LastUpdateBy = ""
                };
                db.Store_Services.Add(prd);
                await db.SaveChangesAsync();

                //using (MerchantService service = new MerchantService(true))
                //{
                //    await service.ApproveAction(c.StoreCode, prd.Id, true, "active");
                //}

                var resultApi = _registerMangoService.CreateStoreFromPosApi(prd.Id, c);
                if (resultApi == false)
                {
                    return Tuple.Create(false, "Mango POS is not reponse. Please try again later");
                }
                await _registerMangoService.SendMailStoreActive(c);
                return Tuple.Create(true, "Slice account is verify successfully");
            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex, c.BusinessName + " (#" + c.StoreCode + ")");
                return Tuple.Create(false, ex.Message);
            }
        }

        public ActionResult Slice()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Slice(RegisterModel model)
        {
            try
            {
                model.PhoneNumber = CommonFunc.CleanPhone(model.PhoneNumber);
                model.CreatedDate = DateTime.Now;
                //check exist email
                if (db.C_Customer.Where(m => m.SalonEmail == model.Email).Count() > 0 || db.C_SalesLead.Where(m => m.L_Email == model.Email).Count() > 0)
                {
                    return Json(new object[] { false, "Email already exists! Please try again with another email or contact us for assistance" });
                }
                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/customer/word_determine");

                var sl = new C_SalesLead();
                sl.CreateAt = DateTime.UtcNow;
                sl.CreateBy = "Slice Register";
                sl.Id = Guid.NewGuid().ToString().Replace("-", "");
                sl.SL_Status = LeadStatus.SliceAccount.Code<int>();
                sl.SL_StatusName = LeadStatus.SliceAccount.Text();
                sl.SalonTimeZone = model.Timezone;
                sl.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER_BY_ID>(model.Timezone).Text();
                sl.PotentialRateScore = 0;
                sl.L_ContactName = model.FName + " " + model.LName;
                sl.L_SalonName = model.FName + " " + model.LName;
                sl.L_Email = model.Email;
                sl.L_Phone = sl.L_ContactPhone = model.PhoneNumber;
                sl.L_Password = model.Password;
                sl.L_Type = LeadType.SliceAccount.Text();
                sl.L_IsVerify = false;
                sl.L_IsSendMail = true;
                sl.L_Product = ListLicensesTrial.Default.Text();
                sl.L_Zipcode = model.ZipCode;
                sl.L_Country = "United States";
                sl.L_Version = node["slice"].InnerText;
                db.C_SalesLead.Add(sl);
                db.SaveChanges();

                await _registerMangoService.SendMailVerify(email: sl.L_Email, name: sl.L_ContactName, phone: sl.L_Phone, link: AppConfig.Host + "/verify?key=" + SecurityLibrary.Encrypt(sl.L_Email));

                var key = SecurityLibrary.Encrypt(sl.L_Email);
                return Json(new object[] { true, key });
            }
            catch (Exception e)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(e);
                return Json(new object[] { false, "Sorry! Currently unable to register,please try again later!", e.Message });
            }
        }

        public async Task<Tuple<bool, string>> CreateOrderAndTicketOrder(C_Customer c, License_Product l, string d)
        {
            try
            {
                var neworder = new O_Orders();
                int countOfOrder = db.O_Orders.Where(o => o.CreatedAt.Value.Year == DateTime.Today.Year
                                         && o.CreatedAt.Value.Month == DateTime.Today.Month).Count();

                neworder.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                neworder.OrdersCode = DateTime.Now.ToString("yyMM") + (countOfOrder + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("fff");
                neworder.CustomerCode = c.CustomerCode;
                neworder.CustomerName = c.BusinessName;
                neworder.Status = InvoiceStatus.Closed.ToString();
                neworder.GrandTotal = 0;
                neworder.CreatedAt = DateTime.UtcNow;
                neworder.CreatedBy = "IMS System";
                neworder.TotalHardware_Amount = 0;
                neworder.ShippingFee = 0;
                //neworder.Service_Amount = 0;
                neworder.DiscountAmount = 0;
                neworder.InvoiceNumber = long.Parse(neworder.OrdersCode);
                neworder.DiscountPercent = 0;
                neworder.TaxRate = 0;
                neworder.BundelStatus = "Complete";
                db.O_Orders.Add(neworder);
                db.SaveChanges();
                var order_Subcription = new Order_Subcription();
                order_Subcription.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + new Random().Next(1, 9999).ToString().PadLeft(4, '0'));
                order_Subcription.StoreCode = c.StoreCode;
                order_Subcription.OrderCode = neworder.OrdersCode;
                order_Subcription.ProductId = l.Id;
                order_Subcription.ProductName = l.Name;
                order_Subcription.CustomerCode = c.CustomerCode;
                order_Subcription.CustomerName = c.BusinessName;
                order_Subcription.Price = neworder.GrandTotal;
                order_Subcription.Period = "MONTHLY";
                order_Subcription.PriceType = Store_Apply_Status.Trial.Text();
                order_Subcription.PurcharsedDay = DateTime.UtcNow;
                order_Subcription.AutoRenew = false;
                order_Subcription.Actived = true;
                order_Subcription.Product_Code = l.Code;
                order_Subcription.Quantity = 1;
                order_Subcription.StartDate = DateTime.UtcNow;
                order_Subcription.EndDate = DateTime.UtcNow.AddMonths(l.Trial_Months.Value);
                order_Subcription.SubscriptionType = LicenseType.LICENSE.Text();
                db.Order_Subcription.Add(order_Subcription);
                db.SaveChanges();
                //var ticketId = await AutoTicketScenario.NewTicketSalesLead(c.CustomerCode, neworder.OrdersCode, d);
                //var ticketstatus = db.T_TicketStatus.Where(x => x.Type == "closed" && x.TicketTypeId == 2).FirstOrDefault();
                //var tickettype = db.T_TicketType.Where(x => x.TypeName == "Finance").FirstOrDefault();
                //var ticketFinance = db.T_SupportTicket.Where(x => x.CustomerCode == c.CustomerCode && x.TypeId == tickettype.Id).FirstOrDefault();
                //if (ticketFinance != null)
                //{
                //    ticketFinance.StatusId = ticketstatus?.Id;
                //    ticketFinance.StatusName = ticketstatus?.Name;
                //    ticketFinance.DateClosed = DateTime.UtcNow;
                //    db.SaveChanges();
                //}
                return Tuple.Create(true, "");

            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex);
                return Tuple.Create(false, ex.Message);
            }

        }
    }
}