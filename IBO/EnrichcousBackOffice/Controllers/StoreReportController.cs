using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using DataTables.AspNet.Core;
using Enrich.IServices;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.Proc;
using EnrichcousBackOffice.Models.UniversalApi.TransactionReport;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Microsoft.AspNet.SignalR.Json;
using Newtonsoft.Json;
using NPOI.HSSF.Util;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace EnrichcousBackOffice.Controllers
{

    [MyAuthorize]
    public class StoreReportController : Controller
    {
        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        private P_Member cMem = Authority.GetCurrentMember();
        WebDataModel db = new WebDataModel();
        private readonly ILogService _logService;

        public StoreReportController(ILogService logService)
        {
            _logService = logService;
        }

        // GET: StoreReport
        #region Filter store
        public ActionResult FilterStore()
        {
            //tam thoi. can them quyen rieng cho access nay. - son ho/date20.12.04
            if (access.Any(k => k.Key.Equals("report_expires")) == false || access["report_expires"] == false)
            {
                return RedirectToAction("Forbidden", "home");
            }

            ViewBag.Services = db.License_Product.Where(c => c.Active == true).ToList();
            var ParnerSelectedList = new List<SelectListItem>();
            ParnerSelectedList.Add(new SelectListItem
            {
                Text = "Simply Pos",
                Value = ""
            });
            var AllPartner = db.C_Partner.Where(x => x.Status == 1);
            var PartnersSelecteListItems = AllPartner.Where(x => x.Status == 1).Select(x => new SelectListItem
            {
                Value = x.Code,
                Text = x.Name
            }).ToList();
            ParnerSelectedList.AddRange(PartnersSelecteListItems);

            if (!string.IsNullOrEmpty(cMem.BelongToPartner))
            {
                ParnerSelectedList = ParnerSelectedList.Where(x => x.Value == cMem.BelongToPartner).ToList();
            }
            ViewBag.PartnersSelecteListItems = ParnerSelectedList;
            return View();
        }

        public ActionResult LoadListStore(IDataTablesRequest dataTablesRequest, string licenseType, string partnerCode, int? expiresAbout, string search_text)
        {
            var db = new WebDataModel();
            if (cMem.SiteId != 1)
            {
                partnerCode = cMem.BelongToPartner;
            }
            var storeServices = db.ExpiresReport(null, expiresAbout, licenseType ?? "", partnerCode ?? "", search_text).ToList();

            if (!string.IsNullOrEmpty(cMem.BelongToPartner))
            {
                storeServices = storeServices.Where(x => x.PartnerCode == cMem.BelongToPartner).ToList();
            }
            var totalRecord = storeServices.Count();
            storeServices = storeServices.OrderBy(s => s.Order).ThenBy(x => x.RenewDate).ToList();
            storeServices = storeServices.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList();
            var dataView = storeServices.Select(x => new
            {
                x.LastUpdateBy,
                EffectiveDate = x.EffectiveDate.HasValue ? x.EffectiveDate.Value.ToString("MMM dd, yyyy") : "",
                RenewDate = x.RenewDate.HasValue ? x.RenewDate.Value.ToString("MMM dd, yyyy") : "",
                x.DueDate,
                x.Id,
                x.StoreCode,
                x.AutoRenew,
                x.StoreName,
                x.CustomerCode,
                x.CustomerId,
                x.ExpiresNote,
                x.Order,
                x.OrderCode,
                x.ServiceType,
                x.Productname,
                x.ProductCode,
                x.PartnerCode,
                x.LastRenewAt,

            }).ToList();

            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }

        [HttpPost]
        public JsonResult FilterStore_reload(int? due_in, string search_services, string search_text)
        {
            var db = new WebDataModel();
            var fs = (from store in db.Store_Services
                      where store.Active == 1
                      join c in db.C_Customer on store.CustomerCode equals c.CustomerCode
                      select new StoreReportView { Customer = c, Store = store, OrderCode = store.LastRenewOrderCode ?? store.OrderCode }).OrderByDescending(f => f.Store.EffectiveDate).AsQueryable();

            if (due_in != null)
            {
                var due = DateTime.UtcNow.AddMonths(due_in ?? 0);
                due = due.AddDays(-due.Day);
                fs = fs.Where(f => f.Customer.WordDetermine == "Slice" || (f.Store.RenewDate.HasValue && due >= f.Store.RenewDate.Value)).OrderByDescending(f => f.Store.EffectiveDate);
            }
            if (!string.IsNullOrEmpty(search_services))
            {
                fs = fs.Where(c => c.Store.Id == search_services).AsQueryable();
            }

            if (!string.IsNullOrWhiteSpace(search_text))
            {
                fs = fs.ToList().Where(delegate (StoreReportView j)
                {
                    return AppLB.CommonFunc.SearchName(j.Store.StoreName, search_text) || AppLB.CommonFunc.SearchName(j.Store.StoreCode, search_text)
                    || AppLB.CommonFunc.SearchName(j.Customer.OwnerMobile, search_text) || AppLB.CommonFunc.SearchName(j.Customer.OwnerName, search_text);
                }).AsQueryable();
            }
            var a = fs.Count();
            string partial = AppLB.CommonFunc.RenderRazorViewToString("_Partial_FilterStore", fs.OrderByDescending(f => f.Store.EffectiveDate).ToList(), this);
            return Json(new object[] { true, partial });
        }
        public ActionResult FilterStore_partial_invoice(string code)
        {
            var db = new WebDataModel();
            var Invoices = db.O_Orders.Where(o => o.CustomerCode == code && o.InvoiceNumber > 0).OrderByDescending(o => o.OrdersCode).ToList();
            ViewBag.CustomerCode = code;
            return PartialView("_Partial_FilterStore_invoices", Invoices);
        }
        #endregion

        // Services report
        public ActionResult Services()
        {
            //tam thoi. can them quyen rieng cho access nay. - son ho/date20.12.04
            if (access.Any(k => k.Key.Equals("report_service")) == false || access["report_service"] == false)
            {
                return RedirectToAction("Forbidden", "home");
            }

            ViewBag.Services = db.License_Product.Where(c => c.Active == true && c.SubscriptionDuration == "MONTHLY").ToList();
            ViewBag.Partners = db.C_Partner.Where(c => c.Status > 0).ToList();
            ViewBag.ProductName = db.SystemConfigurations.FirstOrDefault().ProductName ?? db.SystemConfigurations.FirstOrDefault().CompanyName;
            ViewBag.BeforeDate = db.SystemConfigurations.FirstOrDefault()?.RecurringBeforeDueDate ?? 0;
            return View();
        }
        public ActionResult GetServices(IDataTablesRequest dataTablesRequest)
        {
            var s_service = Request["s_service"];
            var s_text = Request["s_text"];
            var s_partner = Request["s_partner"];
            var s_paymentlater = Request["s_paymentlater"];
            var s_remaining = int.Parse(Request["s_remaining"]);
            var s_recurring_status = Request["s_recurring_status"];
            string store_in_house = MerchantType.STORE_IN_HOUSE.Code<string>();
            var CardList = db.C_CustomerCard.Where(c => c.Active == true && c.MxMerchant_Id > 0);
            var PartnerCard = db.C_PartnerCard.Where(c => c.Active == true && c.MxMerchant_Id > 0);
            var fs = (from store in db.Store_Services.Where(c => c.Active == 1 && c.Period == "MONTHLY")
                      join s in db.Order_Subcription.Where(c => c.Period == "MONTHLY")
                      on new { c1 = store.OrderCode, c2 = store.ProductCode } equals new { c1 = s.OrderCode, c2 = s.Product_Code }
                      join c in db.C_Customer.Where(c => c.Type != store_in_house && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId))
                      on store.CustomerCode equals c.CustomerCode
                      join order in db.O_Orders.Where(c => c.IsDelete != true)
                      on store.OrderCode equals order.OrdersCode
                      join ca in CardList on store.MxMerchant_cardAccountId equals ca.MxMerchant_Id into card
                      from ca in card.DefaultIfEmpty()
                      join pa in PartnerCard on store.MxMerchant_cardAccountId equals pa.MxMerchant_Id into part
                      from pa in part.DefaultIfEmpty()
                      select new ServiceReportView { Customer = c, Order = order, Store = store, CustomerCard = ca, PartnerCard = pa, Subcription = s }).OrderByDescending(f => f.Store.EffectiveDate).AsQueryable();
            if (!string.IsNullOrEmpty(s_service))
            {
                fs = fs.Where(c => c.Store.ProductCode == s_service).AsQueryable();
            }
            if (!string.IsNullOrEmpty(s_partner))
            {
                fs = fs.Where(c => c.Customer.PartnerCode == s_partner || (string.IsNullOrEmpty(c.Customer.PartnerCode) && s_partner == "mango")).AsQueryable();
            }
            if (!string.IsNullOrEmpty(s_paymentlater))
            {
                bool isPaymentLater = bool.Parse(s_paymentlater);
                if (isPaymentLater)
                {
                    fs = fs.Where(c => c.Order.Status == "PaymentLater").AsQueryable();
                }
                else
                {
                    fs = fs.Where(c => c.Order.Status != "PaymentLater").AsQueryable();
                }
            }
            if (s_remaining > -1)
            {
                if (s_remaining == 0)
                {
                    var date = DateTime.UtcNow.Date;
                    fs = fs.Where(c => c.Store.RenewDate < date).AsQueryable();
                }
                if (s_remaining > 0)
                {
                    var date = s_remaining == 30 ? DateTime.UtcNow.AddMonths(1) : DateTime.UtcNow.AddDays(s_remaining);
                    var now = DateTime.UtcNow.Date;
                    fs = fs.Where(c => now <= c.Store.RenewDate && c.Store.RenewDate <= date).AsQueryable();
                }
            }
            if (!string.IsNullOrEmpty(s_text))
            {
                fs = fs.Where(delegate (ServiceReportView c)
                {
                    return CommonFunc.SearchName(c.Customer.BusinessName, s_text) ||
                            CommonFunc.SearchName(c.Customer.BusinessPhone, s_text) ||
                            CommonFunc.SearchName(c.Customer.CustomerCode, s_text) ||
                            CommonFunc.SearchName(c.Customer.BusinessEmail, s_text) ||
                            CommonFunc.SearchName(c.Customer.AddressLine(), s_text) ||
                            CommonFunc.SearchName(c.Customer.MangoEmail, s_text) ||
                            CommonFunc.SearchName(c.Customer.SalonEmail, s_text) ||
                            CommonFunc.SearchName(c.Customer.CompanyEmail, s_text) ||
                            CommonFunc.SearchName(c.Customer.OwnerName, s_text) ||
                            CommonFunc.SearchName(c.Customer.OwnerMobile, s_text) ||
                            CommonFunc.SearchName(c.Customer.SalonAddressLine(), s_text) ||
                            CommonFunc.SearchName(c.Customer.BusinessAddressLine(), s_text) ||
                            CommonFunc.SearchName(c.Customer.StoreCode, s_text) ||
                            CommonFunc.SearchName(c.Customer.BusinessDescription, s_text);
                }).AsQueryable();
            }

            if (!string.IsNullOrEmpty(s_recurring_status))
            {
                fs = fs.Where(c => c.Store.AutoRenew == (s_recurring_status == "1") || (s_recurring_status == "0" && c.Store.AutoRenew == null));
            }

            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "StoreCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Store.StoreCode);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Store.StoreCode);
                    }
                    break;
                case "StoreName":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Store.StoreName);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Store.StoreName);
                    }
                    break;
                case "Owner":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Customer.OwnerName);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Customer.OwnerName);
                    }
                    break;
                case "Productname":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Store.Productname);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Store.Productname);
                    }
                    break;
                case "ExpiresDate":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Store.RenewDate);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Store.RenewDate);
                    }
                    break;
                case "NextPayment":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Store.RenewDate);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Store.RenewDate);
                    }
                    break;
                case "RecurringStatus":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Store.AutoRenew);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Store.AutoRenew);
                    }
                    break;
                case "CardNumber":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.CustomerCard.CardNumber).ThenBy(s => s.PartnerCard.CardNumber);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.CustomerCard.CardNumber).ThenByDescending(s => s.PartnerCard.CardNumber);
                    }
                    break;
                default:
                    fs = fs.OrderByDescending(s => s.Customer.StoreCode);
                    break;
            }
            var totalRecord = fs.Count();
            fs = fs.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var dataView = fs.ToList();
            dataView.ForEach(c =>
            {
                c.RemainingDate = CommonFunc.FormatNumberRemainDate((c.Store?.RenewDate != null) ? (int?)(c.Store?.RenewDate.Value.Date - DateTime.UtcNow.Date).Value.Days : null);
                string partnerCode = c.PartnerCard?.PartnerCode;
                c.PartnerId = db.C_Partner.FirstOrDefault(p => p.Code == partnerCode)?.Id;


            });
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }
        public bool CheckCard(string stores = "")
        {
            try
            {
                var hascard = true;
                stores.Split(',').ForEach(sto =>
                {
                    if (hascard)
                    {
                        var customer = db.C_Customer.FirstOrDefault(s => s.StoreCode == sto) ?? new C_Customer { };
                        hascard = db.C_CustomerCard.Where(ca => ca.CustomerCode == customer.CustomerCode && ca.IsDefault == true && ca.Active == true && ca.IsRecurring == true).ToList().Count > 0 ||
                        db.C_PartnerCard.Where(ca => ca.PartnerCode == customer.PartnerCode && ca.IsDefault == true && ca.Active == true && ca.IsRecurring == true).ToList().Count > 0;
                    }
                });
                return hascard;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //Cancel report
        public ActionResult CancelReport()
        {
            if (access.Any(k => k.Key.Equals("report_cancel")) == false || access["report_cancel"] == false)
            {
                return RedirectToAction("Forbidden", "home");
            }
            ViewBag.Partners = db.C_Partner.Where(c => c.Status > 0).ToList();
            return View();
        }
        public ActionResult CancelChangeTab(string TabName)
        {
            // add to view history top button
            UserContent.TabHistory = "Cancel report" + "|/storereport/cancelreport";
            ViewBag.Partners = db.C_Partner.Where(c => c.Status > 0).ToList();
            ViewBag.ProductName = db.SystemConfigurations.FirstOrDefault().ProductName ?? db.SystemConfigurations.FirstOrDefault().CompanyName;
            switch (TabName)
            {
                case "chart":
                    return PartialView("_Tab_CancelReportChart");
                default:
                    return PartialView("_Tab_CancelReport");
            }
        }
        public ActionResult GetCancelInvoice(IDataTablesRequest dataTablesRequest)
        {
            var s_canceldays = int.Parse(Request["s_canceldays"]);
            var s_refund_status = Request["s_refund_status"];
            var s_partner = Request["s_partner"];
            var fs = (from os in db.Order_Subcription.Where(c => c.SubscriptionType != "setupfee" && c.SubscriptionType != "interactionfee")
                      join tr in db.C_CustomerTransaction.Where(c => c.PaymentMethod == "Refund") on new { c1 = os.OrderCode, c2 = os.Product_Code } equals new { c1 = tr.OrdersCode, c2 = tr.PaymentNote }
                      join otr in db.C_CustomerTransaction.Where(c => c.PaymentMethod != "Refund" && (c.PaymentStatus == "Approved" || c.PaymentStatus == "Success")) on os.OrderCode equals otr.OrdersCode
                      join od in db.O_Orders.Where(c => c.Status == "Canceled") on os.OrderCode equals od.OrdersCode
                      join cm in db.C_Customer on os.CustomerCode equals cm.CustomerCode
                      join lp in db.License_Product on tr.PaymentNote equals lp.Code
                      select new { os, tr, od, cm, otr, lp }).OrderByDescending(c => c.od.UpdatedAt).AsQueryable();

            if (!string.IsNullOrEmpty(s_partner))
            {
                fs = fs.Where(c => c.cm.PartnerCode == s_partner || (string.IsNullOrEmpty(c.cm.PartnerCode) && s_partner == "mango")).AsQueryable();
            }
            if (s_canceldays > 0)
            {
                if (s_canceldays <= 60) //about 14/60 days
                {
                    var date = DateTime.UtcNow.UtcToIMSDateTime().AddDays(-s_canceldays);
                    fs = fs.Where(c => c.od.UpdatedAt >= date);
                }
                else //after 60 days
                {
                    var date = DateTime.UtcNow.UtcToIMSDateTime().AddDays(-60);
                    fs = fs.Where(c => c.od.UpdatedAt < date);
                }
            }

            if (!string.IsNullOrEmpty(s_refund_status))
            {
                fs = fs.Where(c => (s_refund_status == "0" && c.tr.PaymentStatus != "Approved" && c.tr.PaymentStatus != "Success") ||
                                   (s_refund_status == "1" && (c.tr.PaymentStatus == "Approved" || c.tr.PaymentStatus == "Success")));
            }

            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "InvoiceNumber":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.od.InvoiceNumber);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.od.InvoiceNumber);
                    }
                    break;
                case "CreateOn":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.lp.Name);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.lp.Name);
                    }
                    break;
                case "Cancel":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.tr.CreateAt);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.tr.CreateAt);
                    }
                    break;
                case "UsedTime":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.otr.CreateAt ?? s.os.StartDate);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.otr.CreateAt ?? s.os.StartDate);
                    }
                    break;
                case "TotalAmout":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.od.GrandTotal);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.od.GrandTotal);
                    }
                    break;
                case "Refund":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.tr.Amount);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.tr.Amount);
                    }
                    break;
                case "RefundStatus":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.tr.PaymentStatus);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.tr.PaymentStatus);
                    }
                    break;
                case "Note":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.tr.ResponseText);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.tr.ResponseText);
                    }
                    break;
                default:
                    fs = fs.OrderByDescending(s => s.tr.CreateAt);
                    break;
            }
            var totalRecord = fs.Count();
            //var totalAmount = totalRecord == 0 ? 0 : fs.Sum(c => c.od.GrandTotal);
            var totalEstimate = totalRecord == 0 ? 0 : fs.Sum(c => c.tr.Amount);
            var totalRefund = totalRecord == 0 ? 0 : fs.Sum(c => (c.tr.PaymentStatus == "Approved" || c.tr.PaymentStatus == "Success") ? c.tr.Amount : 0);
            fs = fs.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);

            var dataView = fs.ToList().Select(c => new
            {
                c.os,
                c.tr,
                c.od,
                c.cm,
                c.otr,
                c.lp,
                createAtStr = c.tr.CreateAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy"),
                usedTime = (int)(DateTime.UtcNow.UtcToIMSDateTime().Date - c.otr.CreateAt.Value.UtcToIMSDateTime().Date).TotalDays,
            });
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                //totalAmount = totalAmount,
                totalEstimate = totalEstimate,
                totalRefund = totalRefund,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }
        public JsonResult GetCancelChart()
        {
            try
            {
                var s_time = Request["s_time"];
                var s_partner = Request["s_partner"];
                var dateNow = DateTime.UtcNow.Date;
                var fs = (from os in db.Order_Subcription.Where(c => c.SubscriptionType != "setupfee" && c.SubscriptionType != "interactionfee")
                          join tr in db.C_CustomerTransaction.Where(c => c.PaymentMethod == "Refund") on new { c1 = os.OrderCode, c2 = os.Product_Code } equals new { c1 = tr.OrdersCode, c2 = tr.PaymentNote }
                          join od in db.O_Orders.Where(c => c.Status == "Canceled") on os.OrderCode equals od.OrdersCode
                          join cm in db.C_Customer on os.CustomerCode equals cm.CustomerCode
                          select new { os, tr, od, cm }
                          ).AsQueryable();

                if (!string.IsNullOrEmpty(s_partner))
                {
                    fs = fs.Where(c => c.cm.PartnerCode == s_partner || (string.IsNullOrEmpty(c.cm.PartnerCode) && s_partner == "mango")).AsQueryable();
                }
                string title = string.Empty;
                DateTime currentTimeNow = DateTime.UtcNow;
                switch (s_time)
                {
                    //this month
                    case "current-month":
                        title = "TOTAL ESTIMATE REFUND STATISTICS IN THIS MONTH";
                        var RangeNumber = Enumerable.Range(1, currentTimeNow.Day);
                        var label = new string[RangeNumber.Count()];
                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, currentTimeNow.Month, i + 1).ToString("dd ddd");
                        }
                        fs = fs.Where(c => c.tr.CreateAt.Value.Month == currentTimeNow.Month && c.tr.CreateAt.Value.Year == currentTimeNow.Year);
                        var invoice = from m in RangeNumber
                                      join d in fs.OrderBy(gg => gg.tr.CreateAt.Value.Day).GroupBy(gg => gg.tr.CreateAt.Value.Day)
                                      on m equals d.Key into gj
                                      from j in gj.DefaultIfEmpty()
                                      select j;
                        var total = invoice.Select(c => c?.Sum(s => Math.Abs(s.tr.Amount)) ?? 0).ToList();
                        var refund = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus == "Approved" || s.tr.PaymentStatus == "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        var notyet = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus != "Approved" && s.tr.PaymentStatus != "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        return Json(new { total, refund, notyet, title, label });
                    //last month
                    case "last-month":
                        title = "TOTAL ESTIMATE REFUND STATISTICS IN LAST MONTH";
                        currentTimeNow = currentTimeNow.AddMonths(-1);
                        RangeNumber = Enumerable.Range(1, DateTime.DaysInMonth(currentTimeNow.Year, currentTimeNow.Month));
                        label = new string[RangeNumber.Count()];
                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, currentTimeNow.Month, i + 1).ToString("dd ddd");
                        }
                        fs = fs.Where(c => c.tr.CreateAt.Value.Month == currentTimeNow.Month && c.tr.CreateAt.Value.Year == currentTimeNow.Year);
                        invoice = from m in RangeNumber
                                  join d in fs.OrderBy(gg => gg.tr.CreateAt.Value.Day).GroupBy(gg => gg.tr.CreateAt.Value.Day)
                                  on m equals d.Key into gj
                                  from j in gj.DefaultIfEmpty()
                                  select j;
                        total = invoice.Select(c => c?.Sum(s => Math.Abs(s.tr.Amount)) ?? 0).ToList();
                        refund = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus == "Approved" || s.tr.PaymentStatus == "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        notyet = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus != "Approved" && s.tr.PaymentStatus != "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        return Json(new { total, refund, notyet, title, label });
                    case "nearest-3-months":
                        title = "TOTAL ESTIMATE REFUND STATISTICS IN 3 MONTHS";
                        RangeNumber = Enumerable.Range(1, currentTimeNow.Month);
                        currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-2);
                        var ThreeMonth = new int[3] { DateTime.UtcNow.Month, DateTime.UtcNow.AddMonths(-1).Month, DateTime.UtcNow.AddMonths(-2).Month };
                        label = new string[3];
                        for (int i = 0; i < 3; i++)
                        {
                            label[i] = DateTime.UtcNow.AddMonths(-(i)).ToString("MMMM yyyy");
                        }
                        fs = fs.Where(c => DbFunctions.TruncateTime(c.tr.CreateAt) >= currentTimeNow);
                        invoice = from m in RangeNumber
                                  join d in fs.OrderBy(gg => gg.tr.CreateAt.Value.Month).GroupBy(gg => gg.tr.CreateAt.Value.Month)
                                  on m equals d.Key into gj
                                  from j in gj.DefaultIfEmpty()
                                  select j;
                        total = invoice.Select(c => c?.Sum(s => Math.Abs(s.tr.Amount)) ?? 0).ToList();
                        refund = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus == "Approved" || s.tr.PaymentStatus == "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        notyet = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus != "Approved" && s.tr.PaymentStatus != "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        return Json(new { total, refund, notyet, title, label });
                    case "current-year":
                        title = "TOTAL ESTIMATE REFUND STATISTICS IN THIS YEAR";
                        RangeNumber = Enumerable.Range(1, currentTimeNow.Month);
                        label = new string[RangeNumber.Count()];
                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, i + 1, 1).ToString("MMMM yyyy");
                        }
                        fs = fs.Where(c => c.tr.CreateAt.Value.Year == currentTimeNow.Year);
                        invoice = from m in RangeNumber
                                  join d in fs.OrderBy(gg => gg.tr.CreateAt.Value.Month).GroupBy(gg => gg.tr.CreateAt.Value.Month)
                                  on m equals d.Key into gj
                                  from j in gj.DefaultIfEmpty()
                                  select j;
                        var asdasdasd = fs.ToList();
                        total = invoice.Select(c => c?.Sum(s => Math.Abs(s.tr.Amount)) ?? 0).ToList();
                        refund = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus == "Approved" || s.tr.PaymentStatus == "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        notyet = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus != "Approved" && s.tr.PaymentStatus != "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        return Json(new { total, refund, notyet, title, label });
                    case "last-year":
                        title = "TOTAL ESTIMATE REFUND STATISTICS IN LAST YEAR";
                        currentTimeNow = currentTimeNow.AddYears(-1);
                        RangeNumber = Enumerable.Range(1, 12);
                        label = new string[RangeNumber.Count()];
                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, i + 1, 1).ToString("MMMM yyyy");
                        }
                        fs = fs.Where(c => c.tr.CreateAt.Value.Year == currentTimeNow.Year);
                        invoice = from m in RangeNumber
                                  join d in fs.OrderBy(gg => gg.tr.CreateAt.Value.Month).GroupBy(gg => gg.tr.CreateAt.Value.Month)
                                  on m equals d.Key into gj
                                  from j in gj.DefaultIfEmpty()
                                  select j;
                        total = invoice.Select(c => c?.Sum(s => Math.Abs(s.tr.Amount)) ?? 0).ToList();
                        refund = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus == "Approved" || s.tr.PaymentStatus == "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        notyet = invoice.Select(c => c?.Sum(s => (s.tr.PaymentStatus != "Approved" && s.tr.PaymentStatus != "Sucess") ? Math.Abs(s.tr.Amount) : 0) ?? 0).ToList();
                        return Json(new { total, refund, notyet, title, label });
                    default:
                        throw new Exception("Time Report Incorrect");
                }
            }
            catch (Exception e)
            {
                return Json(new { e.Message });
            }
        }
        //public JsonResult MaskAsRefund(string Id = "", decimal Amount = 0, string MessageText = "", bool CheckedCard = false)
        //{
        //    try
        //    {
        //        var trans = db.C_CustomerTransaction.FirstOrDefault(c => Id.Contains(c.Id));
        //        if (trans == null) throw new Exception("Refund transaction not found");

        //        if (CheckedCard && !string.IsNullOrEmpty(trans.Card))
        //        {
        //            var Cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == trans.CustomerCode) ?? new C_Customer { };
        //            var card = new C_CustomerCard { };
        //            long MxMerchantID = 0;
        //            var p_card = db.C_PartnerCard.Find(trans.Card);
        //            if (p_card == null)
        //            {
        //                card = db.C_CustomerCard.Find(trans.Card);
        //                MxMerchantID = Cus.MxMerchant_Id ?? 0;
        //            }
        //            else
        //            {
        //                card = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(p_card));
        //                MxMerchantID = db.C_Partner.FirstOrDefault(c => c.Code == p_card.PartnerCode)?.MxMerchant_Id ?? 0;
        //            }
        //            var request = new TransRequest()
        //            {
        //                MxMerchant_Id = MxMerchantID
        //            };
        //            trans.ResponseText = MessageText;
        //            trans.Amount = -Math.Abs(Amount);
        //            var recur_payment = new PaymentService().RefundTrans(request, card, trans);
        //        }
        //        else
        //        {
        //            trans.Card = null;
        //            trans.CardNumber = null;
        //            trans.BankName = null;
        //            trans.PaymentStatus = "Approved";
        //            trans.ResponseText = MessageText;
        //            trans.Amount = -Math.Abs(Amount);
        //            db.Entry(trans).State = EntityState.Modified;
        //        }
        //        db.SaveChanges();
        //        if (trans.PaymentStatus == "Approved" || trans.PaymentStatus == "Success") return Json(new object[] { true, "Mask as refund success." });
        //        else throw new Exception("Mask as refund error.");
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, e.Message });
        //    }
        //}

        public ActionResult RecurringReport()
        {
            ViewBag.Services = db.License_Product.Where(c => c.Active == true &&
                (c.Type != "license" ||
                (c.Type == "license" && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId))
                )).ToList();
            return View();
        }
        public ActionResult LoadRecurringPlan(IDataTablesRequest dataTablesRequest, string searchText, DateTime? fromDate, DateTime? toDate, int status = -2, List<string> slicense = null, string storeCode = "")
        {
            string store_in_house = MerchantType.STORE_IN_HOUSE.Code<string>();
            //var fs = (from planning in db.RecurringPlannings
            //              //join store in db.Store_Services on new { s1 = planning.SubscriptionCode, s2 = planning.OrderCode } equals new { s1 = store.ProductCode, s2 = store.OrderCode }
            //          join license in db.License_Product on planning.SubscriptionCode equals license.Code
            //          join order in db.O_Orders on planning.OrderCode equals order.OrdersCode
            //          join history in db.RecurringHistories.GroupBy(c => c.RecurringId) on planning.Id equals history.Key into histories
            //          from history in histories.DefaultIfEmpty()
            //          join customer in db.C_Customer.Where(c => c.Type != store_in_house) on planning.CustomerCode equals customer.CustomerCode
            //          where (cMem.SiteId == 1 || cMem.SiteId == customer.SiteId)
            //          select new { customer, license, planning, order, histories = history.ToList().OrderBy(c => c.CreatedAt) }).AsEnumerable();
            var fs = db.Database.SqlQuery<ModelLoadRecurringPlan>($"exec P_LoadRecurringPlan {cMem.SiteId}").AsEnumerable();

            if (!string.IsNullOrEmpty(storeCode))
            {
                fs = fs.Where(c => c.customerStoreCode == storeCode).AsEnumerable();
            }
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                fs = fs.Where(c => c.customerCustomerCode.Contains(searchText) ||
                                    c.customerStoreCode.Contains(searchText) ||
                                    c.customerBusinessName.Contains(searchText) ||
                                    c.planningOrderCode.Contains(searchText) ||
                                    c.planningSubscriptionCode.Contains(searchText)).AsEnumerable();
            }
            if (fromDate != null && toDate != null)
            {
                fs = fs.Where(c => (c.planningRecurringDate?.Date >= fromDate?.Date && c.planningRecurringDate?.Date <= toDate?.Date)).AsEnumerable();
            }
            if (status >= -1)
            {
                fs = fs.Where(c => c.planningStatus == status).AsEnumerable();
            }
            if (slicense!=null && slicense.Count>0)
            {
                fs = fs.Where(c => slicense.Contains(c.licenseCode)).AsEnumerable();
            }
            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "Id":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.planningId);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.planningId);
                    }
                    break;
                case "CustomerCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.customerCustomerCode);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.customerCustomerCode);
                    }
                    break;
                case "ProductCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.licenseCode);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.licenseCode);
                    }
                    break;
                case "Recurring":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.planningRecurringDate);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.planningRecurringDate);
                    }
                    break;
                case "RecurringStatus":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.planningStatus);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.planningStatus);
                    }
                    break;
                default:
                    fs = fs.OrderByDescending(s => s.planningId);
                    break;
            }
            var totalRecord = fs.Count();
            fs = fs.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var dataView = fs.ToList();
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }
        //[HttpPost]
        //public JsonResult SaveRecurringPlan(RecurringPlanning planning)
        //{
        //    try
        //    {
        //        //var recurringPlanning = db.RecurringPlannings.Find(planning.Id);
        //        //if (recurringPlanning == null) throw new Exception("Recurring planning not found");

        //        ////check store service is exist
        //        //var store = db.Store_Services.FirstOrDefault(c => c.OrderCode == recurringPlanning.OrderCode && c.ProductCode == recurringPlanning.SubscriptionCode);
        //        //if (store == null) //if store service not exist then remove current recurring plan
        //        //{
        //        //    db.RecurringPlannings.Remove(recurringPlanning);
        //        //    db.SaveChanges();
        //        //    return Json(new object[] { true, "Because store service not found, current recurring has been deleted." });
        //        //}
        //        //if (planning.Status != recurringPlanning.Status)
        //        //{
        //        //    PaymentService.SetStatusRecurring(store.Id, planning.Status == 1 ? "active" : "inactive");
        //        //}

        //        //recurringPlanning.ApplyDiscountAsRecurring = planning.ApplyDiscountAsRecurring;
        //        //recurringPlanning.Price = planning.Price;
        //        //recurringPlanning.Status = planning.Status;
        //        //recurringPlanning.DiscountPercent = planning.DiscountPercent;
        //        //recurringPlanning.Discount = planning.Discount;
        //        //recurringPlanning.RecurringDate = planning.RecurringDate;
        //        //recurringPlanning.TotalRecurringPrice = planning.TotalRecurringPrice;
        //        //recurringPlanning.LastUpdatedAt = DateTime.UtcNow;
        //        //recurringPlanning.LastUpdatedBy = cMem.FullName;
        //        //db.Entry(recurringPlanning).State = EntityState.Modified;
        //        //db.SaveChanges();
        //        return Json(new object[] { true, "Change recurring planning complete." });
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, e.Message });
        //    }
        //}
        //public JsonResult RemoveRecurringPlan(int id = 0)
        //{
        //    try
        //    {
        //        //var recurringPlanning = db.RecurringPlannings.Find(id);
        //        //if (recurringPlanning == null) throw new Exception("Recurring planning not found");
        //        //var store = db.Store_Services.FirstOrDefault(c => c.OrderCode == recurringPlanning.OrderCode && c.ProductCode == recurringPlanning.SubscriptionCode);
        //        //if (store != null)
        //        //{
        //        //    PaymentService.SetStatusRecurring(store.Id, "inactive");
        //        //}
        //        //recurringPlanning.Status = -1;
        //        //recurringPlanning.LastUpdatedAt = DateTime.UtcNow;
        //        //recurringPlanning.LastUpdatedBy = cMem.FullName;
        //        //db.SaveChanges();
        //        return Json(new object[] { true, "Remove recurring planning complete." }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, e.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        #region expires note 
        [HttpGet]
        public ActionResult GetExpiresLastNote(string StoreCode)
        {
            try
            {
                var expiresLastNote = db.ExpiresReportNotes.AsNoTracking().Where(x => x.StoreCode == StoreCode).OrderByDescending(x => x.CreatedDate).FirstOrDefault()?.Note;
                return Json(new { status = true, note = expiresLastNote }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult UpdateExpiresNote(string StoreCode, string Note)
        {
            try
            {
                var expiresLastNote = new ExpiresReportNote();
                expiresLastNote.StoreCode = StoreCode;
                expiresLastNote.Note = Note;
                expiresLastNote.CreatedDate = DateTime.UtcNow;
                db.ExpiresReportNotes.Add(expiresLastNote);
                db.SaveChanges();
                return Json(new { status = true, message = "add note success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        #endregion

        #region transaction report
        public ActionResult TransactionReport()
        {
            if ((access.Any(k => k.Key.Equals("report_transaction")) == false || access["report_transaction"] != true))
            {
                throw new Exception("Access denied");
            }
            ViewBag.Services = db.License_Product.Where(c => c.Active == true).ToList();
            ViewBag.Hardware = db.O_Product_Model.Where(c => c.Active == true).ToList();
            ViewBag.Partners = db.C_Partner.Where(c => c.Status > 0 && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId)).ToList();
            ViewBag.ProductName = db.SystemConfigurations.FirstOrDefault().ProductName ?? db.SystemConfigurations.FirstOrDefault().CompanyName;
            return View();
        }

       

        private TransactionReportResponse GetTransactionFromIniversalApi(IDataTablesRequest dataTable, DateTime? FromDate, DateTime? ToDate, List<string> PaymentMethods, List<string> Partners, List<string> Services, List<string> SubscriptionTypes,List<string> Hardware, string SearchText, bool GetAll = false)
        {

            var request = new TransactionReportRequest();


            //paging
            if (GetAll == true)
            {
                request.PageSize = int.MaxValue;
                request.PageIndex = 1;
                request.OrderBy = "-PaymentDate";
            }
            else
            {
                request.PageSize = dataTable.Length;
                request.PageIndex = dataTable.Start / dataTable.Length + 1;
            }

            //filter data  from date
            if (FromDate != null)
            {
                request.Condition.FromDate = FromDate.Value;
            }
            //filter data  from date
            if (ToDate != null)
            {
                request.Condition.ToDate = ToDate.Value;
            }
            if (PaymentMethods != null && PaymentMethods.Count > 0)
            {
                request.Condition.PaymentMethod = PaymentMethods;
            }
            if (Partners != null && Partners.Count > 0)
            {
                Partners = Partners.Select(x => x.Replace("Mango", "")).ToList();
                request.Condition.Partners = Partners;
            }
            if (Services != null && Services.Count > 0)
            {
                request.Condition.SubscriptionCodes = Services;
            }
            if (SubscriptionTypes != null && SubscriptionTypes.Count > 0)
            {
                request.Condition.SubscriptionTypes = SubscriptionTypes;
            }
            if (Hardware != null && Hardware.Count > 0)
            {
                request.Condition.Hardware = Hardware;
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                request.Condition.SearchText = SearchText;
            }
            string OrderBy = "PaymentDate";
            if (dataTable != null)
            {
                var colSearch = dataTable.Columns.Where(c => c.Sort != null).FirstOrDefault();
                if (colSearch != null)
                {
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        OrderBy = colSearch.Name;
                    }
                    else
                    {
                        OrderBy = "-" + colSearch.Name;
                    }
                }
                request.OrderBy = OrderBy;
            }
            string strJson = JsonConvert.SerializeObject(request);

            string BaseUniversalApiUrl = WebConfigurationManager.AppSettings["ApiUniversalUrl"];
            string TransactionReportUniversalAPI = WebConfigurationManager.AppSettings["UniversalTransactionReportApi"];
            string BasicKey = WebConfigurationManager.AppSettings["UniversalBasicKey"];
            HttpResponseMessage result = ClientRestAPI.CallRestApi($"{BaseUniversalApiUrl}/{TransactionReportUniversalAPI}", "", BasicKey, "POST", request);
            if (result.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("fetch message error");
            }
            string contentResponse = result.Content.ReadAsStringAsync().Result;
            var reponse = JsonConvert.DeserializeObject<TransactionReportResponse>(contentResponse);
            reponse.Records.ForEach(x =>
            {
                x.CreateAtRaw = x.CreateAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy <br/> hh:mm:ss tt");
                x.PaymentDateUtcRaw = x.PaymentDate.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy <br/> hh:mm:ss tt");
            });
            return reponse;
        }
        [HttpPost]
        public ActionResult GetListTransaction(IDataTablesRequest dataTable, DateTime? FromDate, DateTime? ToDate, List<string> PaymentMethods, List<string> Partners, List<string> Services, List<string> SubscriptionTypes, List<string> Hardware, string SearchText)
        {
            FromDate = FromDate.Value.IMSToUTCDateTime();
            ToDate = ToDate.Value.IMSToUTCDateTime();
            var transactionPaging = this.GetTransactionFromIniversalApi(dataTable, FromDate.Value, ToDate.Value, PaymentMethods, Partners, Services, SubscriptionTypes, Hardware, SearchText);
            return Json(new
            {
                recordsTotal = transactionPaging.Pagination.TotalRecords,
                recordsFiltered = transactionPaging.Pagination.TotalRecords,
                totalAmount = string.Format("${0:#,0.00}", transactionPaging.Summary.TotalAmount),
                incom = string.Format("${0:#,0.00}", transactionPaging.Income),
                draw = dataTable.Draw,
                data = transactionPaging.Records
            });
        }

        public async Task<ActionResult> ExportTransaction(DateTime? FromDate, DateTime? ToDate, List<string> PaymentMethods, List<string> Partners, List<string> Services, List<string> SubscriptionTypes, string SearchText)
        {
            try
            {

                FromDate = FromDate.Value.IMSToUTCDateTime();
                ToDate = ToDate.Value.IMSToUTCDateTime();

                //reupdate from date and to date
                var transactionPaging = this.GetTransactionFromIniversalApi(null, FromDate.Value, ToDate.Value, PaymentMethods, Partners, Services, SubscriptionTypes,null, SearchText, true);
                string webRootPath = "/upload/other/";
                string fileName = "TransactionReport_From" + FromDate.Value.ToString("yyyyMMdd_") + ToDate.Value.ToString("yyyyMMdd_") + DateTime.UtcNow.ToString("yyyyMMddhhmmssff") + ".xlsx";
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {
                    IWorkbook workbook = new XSSFWorkbook();
                    //name style
                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 14;
                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);

                    IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                    ISheet excelSheet = workbook.CreateSheet("Transaction");
                    //set column width
                    excelSheet.SetColumnWidth(1, 15 * 256);
                    excelSheet.SetColumnWidth(2, 15 * 256);
                    excelSheet.SetColumnWidth(3, 20 * 256);
                    excelSheet.SetColumnWidth(4, 10 * 256);
                    excelSheet.SetColumnWidth(5, 20 * 256);
                    excelSheet.SetColumnWidth(6, 15 * 256);
                    excelSheet.SetColumnWidth(7, 15 * 256);
                    excelSheet.SetColumnWidth(8, 20 * 256);
                    excelSheet.SetColumnWidth(9, 20 * 256);
                    excelSheet.SetColumnWidth(10, 20 * 256);
                    excelSheet.SetColumnWidth(11, 20 * 256);

                    excelSheet.CreateFreezePane(0, 1, 0, 1);

                    //Search info
                    //   IRow s_row = excelSheet.CreateRow(8);

                    //header table
                    //header style
                    IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Color = HSSFColor.White.Index;
                    font1.FontHeightInPoints = 13;
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;
                    style1.FillPattern = FillPattern.SolidForeground;
                    IRow header = excelSheet.CreateRow(0);
                    string[] head_titles = { "Transaction Date (UTC +0)", "Salon Name", "Salon Code", "Invoice", "Discount For Invoice", "Subscription Name", "Subscription Type", "Price", "Total Price", "Discount", "Total Amount", "Payment Method" };
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]);
                        c.CellStyle = style1;
                    }
                    int row_num = 1;
                    //table data
                    int index = 1;
                    foreach (var transaction in transactionPaging.Records.ToList())
                    {

                        IRow row_next_1 = excelSheet.CreateRow(row_num);
                        row_next_1.CreateCell(0).SetCellValue(transaction.PaymentDate.Value.UtcToIMSDateTime().ToString("MM/dd/yyyy hh:mm tt"));
                        row_next_1.CreateCell(1).SetCellValue(transaction.StoreName);
                        row_next_1.CreateCell(2).SetCellValue(transaction.StoreCode);
                        row_next_1.CreateCell(3).SetCellValue(transaction.OrderCode);
                        row_next_1.CreateCell(4).SetCellValue(transaction.OrderDiscountPercent > 0 ? transaction.OrderDiscountPercent.Value.ToString() + "%" : (transaction.OrderDiscountAmount > 0 ? "$" + transaction.OrderDiscountAmount.ToString() : ""));
                        row_next_1.CreateCell(5).SetCellValue($"{transaction.ProductName} x{transaction.Quantity ?? 1}");
                        row_next_1.CreateCell(6).SetCellValue(transaction.Type);
                        row_next_1.CreateCell(7).SetCellValue(transaction.Price.ToString());
                        row_next_1.CreateCell(8).SetCellValue((transaction.Price * (transaction.Quantity ?? 1)).ToString());
                        row_next_1.CreateCell(9).SetCellValue(transaction.DiscountPercent > 0 ? transaction.DiscountPercent.Value.ToString() + "%" : (transaction.Discount > 0 ? "$" + transaction.Discount.ToString() : ""));
                        row_next_1.CreateCell(10).SetCellValue(transaction.SubscriptionAmount.ToString());
                        row_next_1.CreateCell(11).SetCellValue(transaction.PaymentMethod);
                        row_num++;
                    }
                    for (int i = 0; i < head_titles.Length; i++)
                    {
                        excelSheet.AutoSizeColumn(i);
                    }
                    workbook.Write(fs);
                }
                var path = Server.MapPath(Path.Combine(webRootPath, fileName));
                using (var fileStream = new FileStream(path, FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileStream.Close();
                }
                memoryStream.Position = 0;
                return Json(new { status = true, path = path });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Transaction][ExportExcel] error when export excel transaction");
                return Json(new { status = false, message = ex.Message });
            }
        }
        //get list support for filter
        [HttpPost]
        public JsonResult GetServiceByType(List<string> Types)
        {
            try
            {
                var query = from service in db.License_Product where service.Active == true select service;
                if (Types != null && Types.Count > 0)
                {
                    query = query.Where(x => Types.Any(y => y == x.Type));
                }
                var result = query.ToList();
                return Json(result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

        public ActionResult DashboardDetail()
        {
            //if ((access.Any(k => k.Key.Equals("report_transaction")) == false || access["report_transaction"] != true))
            //{
            //    throw new Exception("Access denied");
            //}
            //ViewBag.Services = db.License_Product.Where(c => c.Active == true).ToList();
            //ViewBag.Hardware = db.O_Product_Model.Where(c => c.Active == true).ToList();
            //ViewBag.Partners = db.C_Partner.Where(c => c.Status > 0 && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId)).ToList();
            //ViewBag.ProductName = db.SystemConfigurations.FirstOrDefault().ProductName ?? db.SystemConfigurations.FirstOrDefault().CompanyName;
            return View();
        }
    }
}