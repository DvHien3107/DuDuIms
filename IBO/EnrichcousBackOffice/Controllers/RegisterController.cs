using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Services;
using System.Threading.Tasks;
using System.Transactions;
using System;
using System.Web.Mvc;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.Models;
using System.Linq;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        WebDataModel db = new WebDataModel();
        private readonly SalesLeadService _registerMangoService = new SalesLeadService();
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