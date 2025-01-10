using DataTables.AspNet.Core;
using Enrich.Core.Infrastructure;
using Enrich.Core.Ultils;
using Enrich.DataTransfer;
using Enrich.DataTransfer.EnrichSMSService;
using Enrich.DataTransfer.EnrichUniversalService.MerchantReport;
using Enrich.IServices;
using Enrich.IServices.Utils;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Enrich.IServices.Utils.Universal;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.AppLB.OptionConfig;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.CustomizeModel.Merchant;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.Utils.OptionConfig;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.ViewModel;
using Hangfire;
using Inner.Libs.Helpful;
using iTextSharp.tool.xml.css;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.SqlServer;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class MerchantManController : Controller
    {
        private Dictionary<string, bool> access;
        private P_Member cMem;
        const string MERCHANT_LIST = "MerchantList";

        private readonly WebDataModel db;
        private readonly ISMSService _sMSService;
        private readonly IEnrichSMSService _enrichSMSService;
        private readonly IMerchantService _merchantService;
        private readonly IMailingService _mailingService;
        private readonly ILogService _logService;
        private readonly IEnrichUniversalService _enrichUniversalService;
        private readonly IClickUpConnectorService _clickUpConnectorService;
        public MerchantManController(
            ISMSService sMSService,
            IMerchantService merchantService,
            IMailingService mailingService,
            ILogService logService,
            IEnrichUniversalService enrichUniversalService,
            IEnrichSMSService enrichSMSService,
            IClickUpConnectorService clickUpConnectorService)
        {
            _sMSService = sMSService;
            _merchantService = merchantService;
            _mailingService = mailingService;
            db = new WebDataModel();
            cMem = AppLB.Authority.GetCurrentMember();
            access = AppLB.Authority.GetAccessAuthority();
            _logService = logService;
            _enrichUniversalService = enrichUniversalService;
            _enrichSMSService = enrichSMSService;
            _clickUpConnectorService = clickUpConnectorService;
        }

        private async Task<CustomerSearchResponse> GetMerchantFromUniversal(IDataTablesRequest dataTable, List<int> siteIds, List<string> accountManager, List<int> merchantStatus/*, int? licenseStatus, int? terminalStatus*/, int? remainingDate, int? number_Remaining, string type, int? NODaysCreated, int? custom_NODaysCreated, string atRisk, int? serviceType, string searchText, string state, string city, string zipCode, string processor, bool getAll = false)
        {
            var request = new CustomerSearchRequest();

            //paging
            if (getAll == true)
            {
                request.PageSize = int.MaxValue;
                request.PageIndex = 1;
            }
            else
            {
                request.PageSize = dataTable.Length;
                request.PageIndex = dataTable.Start / dataTable.Length + 1;
            }

            // filter by site id
            if (cMem.SiteId == 1)
            {
                if (siteIds != null && siteIds.Count > 0)
                {
                    request.Condition.SiteIds = siteIds;
                }
            }
            else
            {
                var partnerSiteId = new List<int> { cMem.SiteId.Value };
                request.Condition.SiteIds = partnerSiteId;
            }

            // filter by site id
            if (accountManager != null && accountManager.Count > 0)
            {
                request.Condition.AccountManagers = accountManager;
            }       
            if (merchantStatus != null)
            {
                request.Condition.MerchantStatus = merchantStatus;
            }
            // filter by type
            request.Condition.Type = string.IsNullOrEmpty(type) ? "STORE_OF_MERCHANT" : type;
            // filter created days 
            if (remainingDate != null)
            {
                var numOfRemaining = 0;
                var now = DateTime.UtcNow;
                if (remainingDate == 1) numOfRemaining = 7;//7 days
                else if (remainingDate == 2) numOfRemaining = 14;//14 days
                else if (remainingDate == 3) numOfRemaining = (now - now.AddMonths(1)).Days;//1 month
                else if (remainingDate == 4) numOfRemaining = (now - now.AddMonths(2)).Days;//2 months
                else if (remainingDate == 5) numOfRemaining = (now - now.AddMonths(3)).Days;//3 months
                else if (remainingDate == 6) numOfRemaining = (now - now.AddMonths(4)).Days;//4 months
                else if (remainingDate == 7) numOfRemaining = (now - now.AddMonths(6)).Days;//6 months
                else if (remainingDate == 8) numOfRemaining = (now - now.AddMonths(12)).Days;//12 months
                else if (remainingDate == 9) numOfRemaining = number_Remaining ?? 0;
                request.Condition.RemainingDays = (numOfRemaining * -1).ToString();
            }

            // filter created days 
            if (NODaysCreated != null)
            {
                var numOfCreateDate = 0;
                var now = DateTime.UtcNow;

                if (NODaysCreated == 1) numOfCreateDate = 7;//7 days
                else if (NODaysCreated == 2) numOfCreateDate = 14;//14 days
                else if (NODaysCreated == 3) numOfCreateDate = (now - now.AddMonths(1)).Days;//1 month
                else if (NODaysCreated == 4) numOfCreateDate = (now - now.AddMonths(2)).Days;//2 months
                else if (NODaysCreated == 5) numOfCreateDate = (now - now.AddMonths(3)).Days;//3 months
                else if (NODaysCreated == 6) numOfCreateDate = (now - now.AddMonths(4)).Days;//4 months
                else if (NODaysCreated == 7) numOfCreateDate = (now - now.AddMonths(6)).Days;//6 months
                else if (NODaysCreated == 8) numOfCreateDate = (now - now.AddMonths(12)).Days;//12 months
                else if (NODaysCreated == 9) numOfCreateDate = number_Remaining ?? 0;
                request.Condition.NODaysCreated = (numOfCreateDate * -1).ToString();
            }
            // filter by atRisk
            if (!string.IsNullOrEmpty(atRisk))
            {
                request.Condition.AtRisk = atRisk;
            }

            if (!string.IsNullOrEmpty(state))
            {
                request.Condition.State = state;
            }
            if (!string.IsNullOrEmpty(city))
            {
                request.Condition.City = city;
            }
            if (!string.IsNullOrEmpty(zipCode))
            {
                request.Condition.ZipCode = zipCode;
            }

            // filter by processor
            if (!string.IsNullOrEmpty(processor))
            {
                request.Condition.Processor = processor;
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                request.Condition.SearchText = searchText;
            }
            string OrderBy = "-CreateAt";
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

            //var merchantSearch = await _enrichUniversalService.GetMerchantFromUniversal(request);
            using (MerchantService service = new MerchantService(true))
            {
                //var store = service.PrimitiveStoreProfile(StoreCode);

                var merchantSearch = await service.GetMerchantFromUniversal(request);
                //service.SyncStoreBaseService(active_service.Id, 1);
                return merchantSearch;
            }
        }
        // GET: MerchantMan
        public async Task<ActionResult> Index(string SearchText, List<int> Sites, List<string> AccountManager, List<int> MerchantStatus, /*int? LicenseStatus, int? TerminalStatus,*/ int? Remaining, int? number_Remaining, string Type, int? NODaysCreated, int? AtRisk, int? ServiceType, string State, string City, string ZipCode, string Processor)
        {

            //UserContent.TabHistory = "Merchant dashboard|/merchantman/";
            WebDataModel db = new WebDataModel();
            ViewBag.p = access;
            ViewBag.SearchText = SearchText;
            TempData["SearchText"] = SearchText;
            TempData["Sites"] = Sites;
            TempData["AccountManager"] = AccountManager;
            //TempData["LicenseStatus"] = LicenseStatus;
            //TempData["TerminalStatus"] = TerminalStatus;
            TempData["MerchantStatus"] = MerchantStatus;
            TempData["Remaining"] = Remaining;
            TempData["number_Remaining"] = number_Remaining;
            TempData["Type"] = Type;
            TempData["NODaysCreated"] = NODaysCreated;
            TempData["AtRisk"] = AtRisk;
            TempData["ServiceType"] = ServiceType;
            TempData["State"] = State;
            TempData["ZipCode"] = ZipCode;
            TempData["City"] = City;
            TempData["Processor"] = Processor;
            var sites = db.Sites.Where(x => x.IsEnable != false && x.IsDeleted != true).ToList();
            ViewBag.SiteAvailables = sites;
            ViewBag.ListAccountManager = db.P_Member.Where(x => x.Active == true && x.Delete != true && (cMem.SiteId == 1 || cMem.SiteId == x.SiteId)).ToList();
            string ProcessorNameSpace = Utils.IEnums.MerchantOptionEnum.Processor.Code<string>();
            ViewBag.ProcessorAvailables = db.EnumValues.Where(x => x.Namespace == ProcessorNameSpace).ToList();
            _logService.Info($"[MerchantMan][Index] completed");
            return View();
        }


        public async Task<ActionResult> LoadListMerchant(IDataTablesRequest dataTablesRequest, List<int> Sites, List<string> AccountManager, List<int> MerchantStatus /*int? LicenseStatus, int? TerminalStatus*/, int? Remaining, int? number_Remaining, string Type, int? NODaysCreated, int? custom_NODaysCreated, string AtRisk, int? ServiceType, string SearchText, string State, string City, string ZipCode, string Processor)
        {
            try
            {
                _logService.Info($"[MerchantMan][LoadListMerchant] start load list merchant with site: {Sites},AccountManager {AccountManager},MerchantStatus {MerchantStatus},Remaining {Remaining},number_Remaining {number_Remaining}, Type {Type}, NODaysCreated {NODaysCreated},custom_NODaysCreated {custom_NODaysCreated},AtRisk {AtRisk},ServiceType {ServiceType},SearchText: {SearchText}");
                WebDataModel db = new WebDataModel();
                ViewBag.p = access;
                // Session.Remove(MERCHANT_LIST);

                var response = await this.GetMerchantFromUniversal(dataTable: dataTablesRequest, siteIds: Sites, accountManager: AccountManager, merchantStatus: MerchantStatus /*licenseStatus: LicenseStatus, terminalStatus: TerminalStatus,*/, remainingDate: Remaining, number_Remaining: number_Remaining, type: Type, NODaysCreated: NODaysCreated, custom_NODaysCreated: custom_NODaysCreated, atRisk: AtRisk, serviceType: ServiceType, searchText: SearchText, state: State, city: City, zipCode: ZipCode, processor: Processor);
                var data = response.Records.Select(x => new
                {
                    x.Id,
                    x.CustomerCode,
                    x.StoreCode,
                    x.BusinessName,
                    x.Active,
                    Address = string.Format("{0}, {1}, {2}, {3} {4}", x.SalonAddress1, x.SalonCity, x.SalonState, x.SalonZipcode, x.SalonCountry),
                    x.PartnerCode,
                    x.PartnerName,
                    x.CellPhone,
                    x.ContactName,
                    x.ServiceType,
                    x.SalonPhone,
                    x.UpdateBy,
                    x.LastUpdateNote,
                    x.LastUpdateBy,
                    x.ProcessorName,
                    x.Processor,
                    x.FullName,
                    x.MID,
                    x.TerminalStatus,
                    x.TerminalType,
                    LastUpdateDate = x.LastUpdateDate?.ToString("MMM dd, yyyy"),
                    StartedDate = x.CreateAt?.UtcToIMSDateTime().ToString("MMM dd,yyyy"),
                    x.CreateAt,
                    LifeTime = x.CreateAt.HasValue ? (DateTime.UtcNow - x.CreateAt.Value).Days : 0,
                    //  x.PreferredLanguage,
                    x.BusinessDescription,
                    x.License
                });
                _logService.Info($"[MerchantMan][LoadListMerchant] completed");
                return Json(new
                {
                    recordsFiltered = response.Pagination.TotalRecords,
                    recordsTotal = response.Pagination.TotalRecords,
                    draw = dataTablesRequest.Draw,
                    data = data,
                    summary = response.Summary
                });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][LoadListMerchant] error load list merchant with site: {Sites},AccountManager {AccountManager},MerchantStatus {MerchantStatus},Remaining {Remaining},number_Remaining {number_Remaining}, Type {Type}, NODaysCreated {NODaysCreated},custom_NODaysCreated {custom_NODaysCreated},AtRisk {AtRisk},ServiceType {ServiceType},SearchText: {SearchText}");

                throw;
            }
        }


        //public JsonResult LoadSearchCount(string SearchText, string SearchBy, string Type, string AccountManager, int? Status, string Remaining, string number_Remaining, string DateCreated, string number_DateCreated, string AtRick, string State, string City, string ZipCode, string Partner, string Tab, string Page = "")
        //{
        //    _logService.Info($"[MerchantMan][LoadSearchCount] start SearchText:{SearchText} - Type:{Type} - Status:{Status} - Remaining:{Remaining} - State:{State} - City:{City} - ZipCode:{ZipCode} - Partner:{Partner} - Tab:{Tab} - Page:{Page}");
        //    try
        //    {
        //        if (access.Any(k => k.Key.Equals("customer_view_count_total")) != true || access["customer_view_count_total"] != true)
        //        {
        //            throw new Exception("You don't have permission");
        //        }
        //        //var customerSlice = db.Store_Services
        //        //           .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new
        //        //           {
        //        //               customerCode = ss.CustomerCode,
        //        //               allowSlice = lp.AllowSlice,
        //        //               active = ss.Active
        //        //           })
        //        //           .Where(c => c.active == 1 && c.allowSlice == true)
        //        //           .Select(s => s.customerCode.ToString())
        //        //           .Distinct().ToList();

        //        string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
        //        var dataCountSlice = new List<C_Customer>();
        //        //var dataCountStoreInHouse = new List<C_Customer>();
        //        //var dataDefault = new List<C_Customer>();
        //        //dataCountSlice = db.C_Customer.Where(c => (cMem.BelongToPartner ?? "").Contains(c.PartnerCode) || string.IsNullOrEmpty(cMem.BelongToPartner))
        //        //                                .Where(x => (x.WordDetermine.Trim().ToLower() == "slice" || customerSlice.Any(c => c.Equals(x.CustomerCode))) && !STORE_IN_HOUSE.Equals(x.Type)).ToList();
        //        var dataDefault = db.C_Customer.Where(c => (cMem.SiteId == 1 || cMem.SiteId == c.SiteId))
        //                                        .Where(x => !STORE_IN_HOUSE.Equals(x.Type) /*&& !customerSlice.Any(c => c.Equals(x.CustomerCode))*/);
        //        var dataCountStoreInHouse = db.C_Customer.Where(c => (cMem.SiteId == 1 || cMem.SiteId == c.SiteId))
        //                                        .Where(x => STORE_IN_HOUSE.Equals(x.Type));

        //        //filter search merchant type [all, Merchant with License, Merchant with MID]
        //        string mType = string.IsNullOrEmpty(Request["mType"]) ? "all" : Request["mType"];

        //        //var ClosedStatusOrder = InvoiceStatus.Closed.ToString();
        //        //var PaidStatusOrder = InvoiceStatus.Paid_Wait.ToString();
        //        //var PaymentLaterStatusOrder = InvoiceStatus.PaymentLater.ToString();
        //        var paymentSuccess = PaymentStatus.Success.ToString();
        //        var paymentApproved = PaymentStatus.Approved.ToString();
        //        if (mType != "TrialAccount")
        //        {
        //            dataDefault = dataDefault.Where(c =>
        //                //db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode) && (o.Status == ClosedStatusOrder || o.Status == PaidStatusOrder || o.Status == PaymentLaterStatusOrder))
        //                db.C_CustomerTransaction.Any(t => t.CustomerCode == c.CustomerCode && (t.PaymentStatus == paymentApproved || t.PaymentStatus == paymentSuccess)));
        //            //dataCountSlice = dataCountSlice.Where(c => (db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode) && (o.Status == ClosedStatusOrder || o.Status == PaidStatusOrder || o.Status == PaymentLaterStatusOrder)))).ToList();
        //        }
        //        if (!string.IsNullOrEmpty(mType))
        //        {
        //            if ("Merchant_with_License".Equals(mType)) //Merchant with license
        //            {
        //                dataDefault = dataDefault.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) && db.Store_Services.Any(m => m.CustomerCode == c.CustomerCode));
        //                //dataCountSlice = dataCountSlice.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) && db.Store_Services.Any(m => m.CustomerCode == c.CustomerCode));
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) && db.Store_Services.Any(m => m.CustomerCode == c.CustomerCode));

        //            }
        //            else if ("Merchant_with_MID".Equals(mType)) //Merchant with MID
        //            {
        //                dataDefault = dataDefault.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) && db.C_MerchantSubscribe.Any(m => m.CustomerCode == c.CustomerCode && m.Status == "active"));
        //                //dataCountSlice = dataCountSlice.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) && db.C_MerchantSubscribe.Any(m => m.CustomerCode == c.CustomerCode && m.Status == "active"));
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) && db.C_MerchantSubscribe.Any(m => m.CustomerCode == c.CustomerCode && m.Status == "active"));
        //                //list_customer = list_customer.Where(c => (db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode))
        //                //                                            || (c.WordDetermine ?? "").ToLower() == "slice") || STORE_IN_HOUSE.Equals(c.Type));
        //            }
        //            else if ("TrialAccount".Equals(mType))
        //            {
        //                dataDefault = dataDefault.Where(c => c.WordDetermine == "Trial" /*|| !db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode))*/);
        //                //dataCountSlice = dataCountSlice.Where(c => c.WordDetermine == "Trial" /*|| !db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode))*/);
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(c => c.WordDetermine == "Trial" /*|| !db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode))*/);
        //            }
        //            else if ("all".Equals(mType))
        //            {
        //                dataDefault = dataDefault.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) &&
        //                                                        (db.Store_Services.Any(m => m.CustomerCode == c.CustomerCode) || db.C_MerchantSubscribe.Any(m => m.CustomerCode == c.CustomerCode && m.Status == "active")));
        //                //dataCountSlice = dataCountSlice.Where(c => c.WordDetermine != "Trial" && db.O_Orders.Any(o => o.CustomerCode.Equals(c.CustomerCode)) &&
        //                //                                        (db.Store_Services.Any(m => m.CustomerCode == c.CustomerCode) || db.C_MerchantSubscribe.Any(m => m.CustomerCode == c.CustomerCode && m.Status == "active")));
        //                //dataCountStoreInHouse = dataCountStoreInHouse.Where(c => Page.ToLower() == "storeinhouse");
        //            }
        //        }
        //        if (!string.IsNullOrEmpty(SearchText))
        //        {
        //            SearchText = cv_SText(SearchText);
        //            switch (SearchBy)
        //            {
        //                case "phone":
        //                    dataDefault = dataDefault.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.CellPhone) && cv_SText(CommonFunc.CleanPhone(x.CellPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.BusinessPhone) && cv_SText(CommonFunc.CleanPhone(x.BusinessPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonPhone) && cv_SText(CommonFunc.CleanPhone(x.SalonPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.OwnerMobile) && cv_SText(CommonFunc.CleanPhone(x.OwnerMobile)).Contains(SearchText));
        //                    }).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.CellPhone) && cv_SText(CommonFunc.CleanPhone(x.CellPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.BusinessPhone) && cv_SText(CommonFunc.CleanPhone(x.BusinessPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonPhone) && cv_SText(CommonFunc.CleanPhone(x.SalonPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.OwnerMobile) && cv_SText(CommonFunc.CleanPhone(x.OwnerMobile)).Contains(SearchText));
        //                    }).AsQueryable();
        //                    break;
        //                case "contactName":

        //                    dataDefault = dataDefault.Where(delegate (C_Customer x)
        //                    {
        //                        return !string.IsNullOrEmpty(x.OwnerName) && cv_SText((x.OwnerName)).Contains(SearchText);
        //                    }).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(delegate (C_Customer x)
        //                    {
        //                        return !string.IsNullOrEmpty(x.OwnerName) && cv_SText((x.OwnerName)).Contains(SearchText);
        //                    }).AsQueryable();
        //                    break;
        //                case "email":
        //                    dataDefault = dataDefault.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.Email) && cv_SText(x.Email).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonEmail) && cv_SText(x.SalonEmail).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.MangoEmail) && cv_SText(x.MangoEmail).Contains(SearchText));
        //                    }).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.Email) && cv_SText(x.Email).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonEmail) && cv_SText(x.SalonEmail).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.MangoEmail) && cv_SText(x.MangoEmail).Contains(SearchText));
        //                    }).AsQueryable();
        //                    break;
        //                case "zipCode":
        //                    dataDefault = dataDefault.Where(x => x.SalonZipcode.Contains(SearchText)).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(x => x.SalonZipcode.Contains(SearchText)).AsQueryable();
        //                    break;
        //                case "storeId":
        //                    dataDefault = dataDefault.Where(x => x.StoreCode.Contains(SearchText)).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(x => x.StoreCode.Contains(SearchText)).AsQueryable();
        //                    break;
        //                case "salonName":
        //                    dataDefault = dataDefault.Where(delegate (C_Customer x)
        //                    {
        //                        return cv_SText(x.BusinessName).Contains(SearchText);
        //                    }).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(delegate (C_Customer x)
        //                    {
        //                        return cv_SText(x.BusinessName).Contains(SearchText);
        //                    }).AsQueryable();
        //                    break;
        //                case "address":
        //                    dataDefault = dataDefault.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.SalonAddress1) && cv_SText(x.SalonAddress1).Contains(SearchText))
        //                        || (!string.IsNullOrEmpty(x.ContactAddress) && cv_SText(x.ContactAddress).Contains(SearchText))
        //                        ;

        //                    }).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.SalonAddress1) && cv_SText(x.SalonAddress1).Contains(SearchText))
        //                        || (!string.IsNullOrEmpty(x.ContactAddress) && cv_SText(x.ContactAddress).Contains(SearchText))
        //                        ;

        //                    }).AsQueryable();
        //                    break;
        //                default:
        //                    dataDefault = dataDefault.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.BusinessName) && cv_SText(x.BusinessName).Contains(SearchText.ToLower()))
        //                            || (!string.IsNullOrEmpty(x.LegalName) && cv_SText(x.LegalName).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.DepositBankName) && cv_SText(x.DepositBankName).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.OwnerName) && cv_SText(x.OwnerName).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.CellPhone) && cv_SText(CommonFunc.CleanPhone(x.CellPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.BusinessPhone) && cv_SText(CommonFunc.CleanPhone(x.BusinessPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonPhone) && cv_SText(CommonFunc.CleanPhone(x.SalonPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.OwnerMobile) && cv_SText(CommonFunc.CleanPhone(x.OwnerMobile)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.BusinessEmail) && cv_SText(x.BusinessEmail).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.Email) && cv_SText(x.Email).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.CustomerCode) && cv_SText(x.CustomerCode).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.StoreCode) && cv_SText(x.StoreCode).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonZipcode) && cv_SText(x.SalonZipcode).Contains(SearchText));
        //                    }).AsQueryable();
        //                    dataCountStoreInHouse = dataCountStoreInHouse.Where(delegate (C_Customer x)
        //                    {
        //                        return (!string.IsNullOrEmpty(x.BusinessName) && cv_SText(x.BusinessName).Contains(SearchText.ToLower()))
        //                            || (!string.IsNullOrEmpty(x.LegalName) && cv_SText(x.LegalName).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.DepositBankName) && cv_SText(x.DepositBankName).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.OwnerName) && cv_SText(x.OwnerName).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.CellPhone) && cv_SText(CommonFunc.CleanPhone(x.CellPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.BusinessPhone) && cv_SText(CommonFunc.CleanPhone(x.BusinessPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonPhone) && cv_SText(CommonFunc.CleanPhone(x.SalonPhone)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.OwnerMobile) && cv_SText(CommonFunc.CleanPhone(x.OwnerMobile)).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.BusinessEmail) && cv_SText(x.BusinessEmail).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.Email) && cv_SText(x.Email).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.CustomerCode) && cv_SText(x.CustomerCode).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.StoreCode) && cv_SText(x.StoreCode).Contains(SearchText))
        //                            || (!string.IsNullOrEmpty(x.SalonZipcode) && cv_SText(x.SalonZipcode).Contains(SearchText));
        //                    }).AsQueryable();
        //                    break;
        //            }
        //        }

        //        // filter State
        //        if (Tab == "enrich")
        //        {
        //            dataDefault = dataDefault.Where(x => string.IsNullOrEmpty(x.PartnerCode));
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => string.IsNullOrEmpty(x.PartnerCode));
        //        }
        //        // filter State
        //        if (!string.IsNullOrEmpty(State))
        //        {
        //            dataDefault = dataDefault.Where(x => !string.IsNullOrEmpty(x.BusinessState) && x.BusinessState.Contains(State));
        //            //dataCountSlice = dataCountSlice.Where(x => !string.IsNullOrEmpty(x.BusinessState) && x.BusinessState.Contains(State));
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => !string.IsNullOrEmpty(x.BusinessState) && x.BusinessState.Contains(State));
        //        }
        //        // filter City
        //        if (!string.IsNullOrEmpty(City))
        //        {
        //            dataDefault = dataDefault.Where(x => !string.IsNullOrEmpty(x.BusinessCity) && x.BusinessCity.Contains(City));
        //            //dataCountSlice = dataCountSlice.Where(x => !string.IsNullOrEmpty(x.BusinessCity) && x.BusinessCity.Contains(City));
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => !string.IsNullOrEmpty(x.BusinessCity) && x.BusinessCity.Contains(City));
        //        }
        //        // filter ZipCode
        //        if (!string.IsNullOrEmpty(ZipCode))
        //        {
        //            dataDefault = dataDefault.Where(x => !string.IsNullOrEmpty(x.BusinessZipCode) && x.BusinessZipCode.ToLower().Contains(ZipCode.ToLower()));
        //            //dataCountSlice = dataCountSlice.Where(x => !string.IsNullOrEmpty(x.BusinessZipCode) && x.BusinessZipCode.ToLower().Contains(ZipCode.ToLower()));
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => !string.IsNullOrEmpty(x.BusinessZipCode) && x.BusinessZipCode.ToLower().Contains(ZipCode.ToLower()));
        //        }
        //        //filter by partner
        //        if (!string.IsNullOrEmpty(Partner))
        //        {
        //            if (Partner == "mango")
        //            {
        //                dataDefault = dataDefault.Where(x => string.IsNullOrEmpty(x.PartnerCode));
        //                //dataCountSlice = dataCountSlice.Where(x => db.O_Orders.Where(c => string.IsNullOrEmpty(c.PartnerCode)).Any(c => c.CustomerCode == x.CustomerCode));
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(x => string.IsNullOrEmpty(x.PartnerCode));
        //            }
        //            else
        //            {
        //                dataDefault = dataDefault.Where(x => x.PartnerCode == Partner);
        //                //dataCountSlice = dataCountSlice.Where(x => db.O_Orders.Where(c => c.PartnerCode == Partner).Any(c => c.CustomerCode == x.CustomerCode));
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(x => x.PartnerCode == Partner);
        //            }
        //        }
        //        //filter by account manager
        //        if (!string.IsNullOrEmpty(AccountManager))
        //        {
        //            dataDefault = dataDefault.Where(x => x.MemberNumber == AccountManager);
        //        }
        //        // filter created days 
        //        if (!string.IsNullOrEmpty(DateCreated))
        //        {
        //            var CreDate = DateTime.UtcNow;
        //            var customDays = int.Parse(number_DateCreated ?? "0");
        //            if (DateCreated == "1") CreDate = CreDate.Date.AddDays(-7);//7 days
        //            else if (DateCreated == "2") CreDate = CreDate.Date.AddDays(-14);//14 days
        //            else if (DateCreated == "3") CreDate = CreDate.AddMonths(-1);//1 month
        //            else if (DateCreated == "4") CreDate = CreDate.AddMonths(-2);//2 months
        //            else if (DateCreated == "5") CreDate = CreDate.AddMonths(-3);//3 months
        //            else if (DateCreated == "6") CreDate = CreDate.AddMonths(-4);//4 months
        //            else if (DateCreated == "7") CreDate = CreDate.AddMonths(-6);//6 months
        //            else if (DateCreated == "8") CreDate = CreDate.AddMonths(-12);//12 months
        //            else if (DateCreated == "9") CreDate = CreDate.Date.AddDays(-customDays);
        //            dataDefault = dataDefault.Where(c => c.CreateAt >= CreDate);
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(c => c.CreateAt >= CreDate);
        //        }
        //        // filter created days 
        //        if (!string.IsNullOrEmpty(Remaining))
        //        {
        //            var RemainingDate = DateTime.UtcNow;
        //            var customDays = int.Parse(number_Remaining ?? "0");
        //            if (Remaining == "1") RemainingDate = RemainingDate.Date.AddDays(7);//7 days
        //            else if (Remaining == "2") RemainingDate = RemainingDate.Date.AddDays(14);//14 days
        //            else if (Remaining == "3") RemainingDate = RemainingDate.AddMonths(1);//1 month
        //            else if (Remaining == "4") RemainingDate = RemainingDate.AddMonths(2);//2 months
        //            else if (Remaining == "5") RemainingDate = RemainingDate.AddMonths(3);//3 months
        //            else if (Remaining == "6") RemainingDate = RemainingDate.AddMonths(4);//4 months
        //            else if (Remaining == "7") RemainingDate = RemainingDate.AddMonths(6);//6 months
        //            else if (Remaining == "8") RemainingDate = RemainingDate.AddMonths(12);//12 months
        //            else if (Remaining == "9") RemainingDate = RemainingDate.Date.AddDays(customDays);
        //            dataDefault = dataDefault.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null && sub.RenewDate.Value <= RemainingDate).Count() > 0);
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null && sub.RenewDate.Value <= RemainingDate).Count() > 0);
        //        }
        //        //at rick
        //        if (!string.IsNullOrEmpty(AtRick))
        //        {
        //            if (AtRick == "1")
        //            {
        //                var expiresDay = DateTime.UtcNow.AddDays(3);
        //                dataDefault = dataDefault.Where(x => db.Store_Services.Any(c => c.CustomerCode == x.CustomerCode && c.Active == 1 && c.Type == "license" && c.RenewDate <= expiresDay) &&
        //                                                        !db.C_CustomerCard.Any(c => c.CustomerCode == x.CustomerCode && c.MxMerchant_Id > 0 && c.Active == true)); // expiring in 3 days and no card on file
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(x => db.Store_Services.Any(c => c.CustomerCode == x.CustomerCode && c.Active == 1 && c.Type == "license" && c.RenewDate <= expiresDay) &&
        //                                                        !db.C_CustomerCard.Any(c => c.CustomerCode == x.CustomerCode && c.MxMerchant_Id > 0 && c.Active == true)); // expiring in 3 days and no card on file
        //            }
        //            else if (AtRick == "2")
        //            {
        //                dataDefault = dataDefault.Where(x => !db.C_CustomerCard.Any(c => c.CustomerCode == x.CustomerCode && c.MxMerchant_Id > 0 && c.Active == true && c.IsDefault == true)); //Have not process credit card
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(x => !db.C_CustomerCard.Any(c => c.CustomerCode == x.CustomerCode && c.MxMerchant_Id > 0 && c.Active == true && c.IsDefault == true)); //Have not process credit card
        //            }
        //            else if (AtRick == "3")
        //            {
        //                var expiresDay = DateTime.UtcNow.AddDays(-7);
        //                dataDefault = dataDefault.Where(x => db.Store_Services.Where(c => c.CustomerCode == x.CustomerCode).OrderByDescending(c => c.RenewDate).FirstOrDefault().RenewDate <= expiresDay); //Inactive store for 7 days or more
        //                dataCountStoreInHouse = dataCountStoreInHouse.Where(x => db.Store_Services.Where(c => c.CustomerCode == x.CustomerCode).OrderByDescending(c => c.RenewDate).FirstOrDefault().RenewDate <= expiresDay); //Inactive store for 7 days or more
        //            }
        //        }

        //        if (Status == 2)
        //        {
        //            dataDefault = dataDefault.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null && sub.RenewDate.Value > DateTime.UtcNow).Count() > 0);
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null && sub.RenewDate.Value > DateTime.UtcNow).Count() > 0);
        //        }
        //        else if (Status == 1)
        //        {
        //            dataDefault = dataDefault.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null && sub.RenewDate.Value <= DateTime.UtcNow).Count() > 0);
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null && sub.RenewDate.Value <= DateTime.UtcNow).Count() > 0);
        //        }
        //        else if (Status == 0)
        //        {
        //            dataDefault = dataDefault.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null).Count() == 0);
        //            dataCountStoreInHouse = dataCountStoreInHouse.Where(x => db.Store_Services.Where(sub => sub.CustomerCode == x.CustomerCode && sub.Active == 1 && sub.Type == "license" && sub.RenewDate != null).Count() == 0);
        //        }
        //        _logService.Info($"[MerchantMan][LoadSearchCount] completed");
        //        return Json(new object[] { true, "Success", dataDefault.Count(), dataCountSlice.Count(), dataCountStoreInHouse.Count() });
        //    }
        //    catch (Exception e)
        //    {
        //        _logService.Error(e, $"[MerchantMan][LoadSearchCount] error SearchText:{SearchText} - Type:{Type} - Status:{Status} - Remaining:{Remaining} - State:{State} - City:{City} - ZipCode:{ZipCode} - Partner:{Partner} - Tab:{Tab} - Page:{Page}");
        //        return Json(new object[] { false, e.Message, 0, 0, 0 });
        //    }


        //}

        /// <summary>
        /// Get last date update in string updateby
        /// </summary>
        /// <param name="string_list_update"></param>
        /// <returns></returns>
        public DateTime? glDateUpdate(string string_list_update)
        {
            _logService.Info($"[MerchantMan][glDateUpdate] start string_list_update:{string_list_update}");
            try
            {
                _logService.Info($"[MerchantMan][glDateUpdate] complete");
                return DateTime.Parse(string_list_update.Split(new string[] { "</i>|" }, StringSplitOptions.None)[0]
                    .Split(new string[] { "at " }, StringSplitOptions.None)[1].ToString());
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][glDateUpdate] error string_list_update:{string_list_update}");
                return null;
            }
        }

        public ActionResult Detail(long? id, string code = "", string tab = "")
        {
            _logService.Info($"[MerchantMan][Detail] start id:{id} - code:{code} - tab:{tab}");
            try
            {
                WebDataModel db = new WebDataModel();
                ViewBag.p = access;
                //if (TempData["Page"]?.ToString() == "trialaccount")
                //{
                //    ViewBag.url_back = "/merchantman?page=trialaccount";
                //}
                //else if (TempData["Page"]?.ToString() == "sliceaccount")
                //{
                //    ViewBag.url_back = "/merchantman?page=sliceaccount";
                //}
                //else if (TempData["Page"]?.ToString() == "storeinhouse")
                //{
                //    ViewBag.url_back = "/merchantman?page=storeinhouse";
                //}
                //else
                //{
                //    ViewBag.url_back = "/merchantman";
                //}
                //TempData.Keep("Page");
                ViewBag.backUrl = TempData["url_back"] ?? "/merchantman";
                TempData["tabname"] = tab;
                var customer = new C_Customer();
                if (id > 0)
                {
                    customer = db.C_Customer.Find(id);
                }
                else
                {
                    customer = db.C_Customer.Where(c => c.CustomerCode == code).FirstOrDefault();
                }
                if (customer == null)
                {
                    return RedirectToAction("Index");
                }

                //add to view history top button
                UserContent.TabHistory = "Merchant @" + Regex.Replace(customer.BusinessName ?? customer.OwnerName, "[^0-9a-zA-Z]+", " ") + "|" + Request.Url.AbsolutePath;
                var sub = db.Store_Services.Where(s => s.Active == 1 && s.Type == "license" && s.CustomerCode == customer.CustomerCode).FirstOrDefault();
                // ViewBag.EffectiveDate = sub?.EffectiveDate;
                //  ViewBag.Remaning = CommonFunc.FormatNumberRemainDate((sub?.RenewDate != null) ? (int?)(sub?.RenewDate.Value.Date - DateTime.UtcNow.Date).Value.Days : null);
                ViewBag.Partner = db.C_Partner.FirstOrDefault(c => c.Code == customer.PartnerCode) ?? new C_Partner() { };
                ViewBag.TicketCount = db.T_SupportTicket.Where(t => t.CustomerCode == customer.CustomerCode && t.Visible == true && !string.IsNullOrEmpty(t.ProjectId)).Count();
                ViewBag.FilesCount = countFile(customer.Id).Data;

                var _optionConfigurationService = new OptionConfigService();
                var config = _optionConfigurationService.LoadSetting<Config>();
                ViewBag.SMSProvideSystem = config.SMS_Show_Provide;


                //   ViewBag.DueDate = db.O_Orders.Where(c => c.CustomerCode == customer.CustomerCode && c.Status == "PaymentLater" &&
                //                            db.Store_Services.Any(s => s.CustomerCode == customer.CustomerCode &&
                //                         (c.OrdersCode == s.OrderCode || c.OrdersCode == s.LastRenewOrderCode) &&
                //                         s.Active == 1 && s.Type =="license")).OrderBy(c => c.DueDate).FirstOrDefault()?.DueDate;
                //ViewBag.PaymentLaterLicense = db.O_Orders.Where(c => c.CustomerCode == customer.CustomerCode && c.Status == "PaymentLater" &&
                //                            db.Store_Services.Any(s => s.CustomerCode == customer.CustomerCode &&
                //                            (c.OrdersCode == s.OrderCode || c.OrdersCode == s.LastRenewOrderCode) &&
                //                            s.Active == 1 && s.Type == "license")).Count() > 0;
                _logService.Info($"[MerchantMan][Detail] completed");
                return View(customer);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][Detail] error id:{id} - code:{code} - tab:{tab}");
                throw;
            }

        }

        /// <summary>
        /// gui email link questionare form cho khach hang submit
        /// </summary>
        /// <param name="code">cusotmer code</param>
        /// <returns></returns>
        public async Task<JsonResult> SendQuestionareEmail(string code, bool sms = false)
        {
            _logService.Info($"[MerchantMan][SendQuestionareEmail] start code:{code} - sms {sms}");
            try
            {
                var cus = db.C_Customer.Where(c => c.CustomerCode == code).FirstOrDefault();
                if (cus == null)
                {
                    _logService.Warning($"[MerchantMan][SendQuestionareEmail] This merchant is not found");
                    throw new Exception("This merchant is not found");
                }
                if (string.IsNullOrEmpty(cus.SalonEmail))
                    return Json(new object[] { false, "salon email not exist" });
                //send email
                string emailResult = "n/a";
                string qnLink = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "Page/salon/Questionare/" + cus.Id + "?n=" + AppLB.CommonFunc.ConvertNonUnicodeURL(cus.BusinessName).Replace("'", "").Replace("-", "_"));
                if (!string.IsNullOrWhiteSpace(cus.Email) || !string.IsNullOrWhiteSpace(cus.BusinessEmail))
                {
                    string pass = cus.Password;
                    if (string.IsNullOrEmpty(cus.Password))
                    {
                        string seed = DateTime.Now.ToString("O");
                        string newPass = SecurityLibrary.Md5Encrypt(seed).Substring(0, 6);
                        pass = newPass;
                        cus.MD5PassWord = SecurityLibrary.Md5Encrypt(newPass);
                        // Todo : REMOVE PASS
                        // cus.Password = null;
                        cus.Password = newPass;
                        cus.UpdateBy = cMem.FullName;
                        db.Entry(cus).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        //StoreProfile storeProfile = new MerchantService().StoreProfileWithDefault(cus.StoreCode);
                        StoreProfileReq storeProfile = new MerchantService().GetStoreProfileReady("", false, null, cus.StoreCode);
                        storeProfile.Password = cus.MD5PassWord;
                        new MerchantService().DoRequest(storeProfile);
                    }
                    string email = string.IsNullOrEmpty(cus.SalonEmail?.Trim() ?? "") ? (string.IsNullOrEmpty(cus.BusinessEmail?.Trim() ?? "") ? cus.Email : cus.BusinessEmail) : cus.SalonEmail;
                    var email_data = new SendGridEmailTemplateData.QuestionareEmailData
                    {
                        client = cus.OwnerName,
                        link = qnLink,
                        salon_email = email,
                        salon_password = pass
                    };
                    ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                    XmlNode node = xml.GetNode("/root/sendgrid_template/questionare_email");

                    string to = cus.SalonEmail;
                    string firstname = cus.BusinessName;
                    string cc = string.Join(";", cMem.PersonalEmail, cus.MangoEmail, cus.Email);
                    emailResult = await _mailingService.SendBySendGridWithTemplate(to, firstname, node["template_id"].InnerText, cc, email_data);
                }

                bool noticeSMS = false;
                if (sms)
                {

                    ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                    XmlNode node = xml.GetNode("/root/sms_template");
                    var contentSMS = node["questionare_notice_merchant"].InnerText;
                    if (contentSMS.Contains("{url}"))
                    {
                        contentSMS = contentSMS.Replace("{url}", qnLink);
                    }
                    string ownerCountry = string.IsNullOrWhiteSpace(cus.Country) == true ? cus.BusinessCountry : cus.Country;
                    string result = await _sMSService.SendSMSTextline(cus.CellPhone, ownerCountry, contentSMS);
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        noticeSMS = true;
                    }
                }
                //update ticket/add new feedback
                //if (string.IsNullOrWhiteSpace(emailResult))
                //{
                //    var ims_onboarding_ticket = db.T_SupportTicket.Where(t => t.TypeId == (long)UserContent.TICKET_TYPE.Deployment
                //    && t.CustomerCode == code).OrderByDescending(t => t.Id).FirstOrDefault();
                //    if (ims_onboarding_ticket != null)
                //    {
                //        string fbConent = "Questionare email has been sent to " + (string.IsNullOrWhiteSpace(cus.Email) == true ? cus.BusinessEmail : cus.Email);
                //        fbConent += "<br/><a target='_blank' href='" + qnLink + "'>" + qnLink + "</a>";
                //        ViewControler.TicketViewController.InsertFeedback(db, ims_onboarding_ticket.Id, "Questionare email has been sent.", fbConent, "", -1);
                //    }

                //    return Json(new object[] { true, "Questionare email Email has been sent" });
                //}

                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Send questionare email");
                new MerchantService().WriteLogMerchant(cus, "Send questionare email", "Send questionare email has been completed");

                _logService.Info($"[MerchantMan][SendQuestionareEmail] completed");
                if (!noticeSMS)
                {
                    return Json(new object[] { true, "Send email success" });
                }
                else
                {
                    return Json(new object[] { true, "SMS has ben sent" });
                }

            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][SendQuestionareEmail]  error code:{code} - sms {sms}");
                return Json(new object[] { false, e.Message });
            }

        }
        public async Task<JsonResult> SendGiftCardForm(string code, bool sms = false)
        {
            _logService.Info($"[MerchantMan][SendGiftCardForm] start code:{code} - sms {sms}");

            try
            {
                var cus = db.C_Customer.Where(c => c.CustomerCode == code).FirstOrDefault();
                if (cus == null)
                {
                    throw new Exception("This merchant is not found");
                }
                var GiftCardsOrderingForm = new C_GiftCardsOrderingForm()
                {
                    CustomerCode = cus.CustomerCode,
                    CreatedAt = DateTime.UtcNow,
                    SalesPerson = cMem.MemberNumber,
                };
                db.C_GiftCardsOrderingForm.Add(GiftCardsOrderingForm);
                db.SaveChanges();
                //send email
                string emailResult = "N/A";
                string Key = SecurityLibrary.Encrypt(GiftCardsOrderingForm.Id.ToString());
                string Link = string.Format("{0}://{1}/{2}", Request.Url.Scheme, Request.Url.Authority, "Page/Salon/GiftCardsOrderingForm?Key=" + Key);
                if (!string.IsNullOrWhiteSpace(cus.Email) || !string.IsNullOrWhiteSpace(cus.BusinessEmail))
                {
                    //string pass = cus.Password;
                    //if (string.IsNullOrEmpty(cus.Password))
                    //{
                    //    string seed = DateTime.Now.ToString("O");
                    //    string newPass = SecurityLibrary.Md5Encrypt(seed).Substring(0, 6);
                    //    pass = newPass;
                    //    cus.MD5PassWord = SecurityLibrary.Md5Encrypt(newPass);
                    //    cus.Password = newPass;
                    //    cus.UpdateBy = cMem.FullName;
                    //    db.Entry(cus).State = EntityState.Modified;
                    //    await db.SaveChangesAsync();

                    //    //StoreProfile storeProfile = new MerchantService().StoreProfileWithDefault(cus.StoreCode);
                    //    StoreProfileReq storeProfile = new MerchantService().GetStoreProfileReady("", false, null, cus.StoreCode);
                    //    storeProfile.Password = cus.MD5PassWord;
                    //    new MerchantService().DoRequest(storeProfile);
                    //}
                    var email_data = new SendGridEmailTemplateData.GiftCardsOrderingForm
                    {
                        owner_name = cus.OwnerName,
                        link = Link
                    };
                    ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                    XmlNode node = xml.GetNode("/root/sendgrid_template/giftcards_ordering_form_email");

                    string to = cus.SalonEmail;
                    string firstname = cus.BusinessName;
                    var SystemConfigurations = db.SystemConfigurations.FirstOrDefault();
                    string cc = string.Join(";", cMem.PersonalEmail, cus.MangoEmail, cus.Email, SystemConfigurations.SalesEmail, SystemConfigurations.SupportEmail);
                    emailResult = await _mailingService.SendBySendGridWithTemplate(to, "", node["template_id"].InnerText, cc, email_data);
                }

                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Send giftcard");
                new MerchantService().WriteLogMerchant(cus, "Send giftcard", "Send giftcard has been completed");

                _logService.Info($"[MerchantMan][SendGiftCardForm] completed");
                return Json(new object[] { true, "Gift Cards Ordering Form Email has been sent" });
                //}
                //if (!noticeSMS)
                //{
                //    throw new Exception("Owner's email is invalid or not found");
                //}
                //else
                //{
                //    return Json(new object[] { true, "SMS has ben sent" });
                //}
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][SendGiftCardForm] start code:{code} - sms {sms}");
                return Json(new object[] { false, e.Message });
            }

        }

        /// <summary>
        /// ON/OFF mango pos
        /// </summary>
        /// <param name="status">ON|OFF</param>
        /// <returns></returns>
        public JsonResult MangoSwitch(string code, string status)
        {
            _logService.Info($"[MerchantMan][MangoSwitch] start code:{code} - status:{status}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_mango_control")) != true || access["customer_mango_control"] != true)
                {
                    throw new Exception("You dont have permission to change!");
                }
                string msg = "";
                var db = new WebDataModel();
                var customer = db.C_Customer.Where(c => c.CustomerCode == code).FirstOrDefault();
                var order_subcription = db.Order_Subcription.Where(c => c.CustomerCode == customer.CustomerCode && c.Actived == true).FirstOrDefault();
                if (status.ToLower() == "on")
                {
                    //can kiem tra logic
                    //neu ton tai storeid roi.
                    //
                    //
                    //else
                    //
                    //copy order subscription -> store_product va store product_license
                    //nen save store_product va store product_license doc lap nhau
                    //

                    if (order_subcription != null)
                    {
                        customer.StoreCode = order_subcription.StoreCode;
                        customer.StoreStatus = true;
                        db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        throw new Exception("SUBSCRIPTION WAS NOT FOUND");
                    }
                    msg = "SIMPLY POS SYSTEM ALREADY ACTIVATED";
                }
                else
                {
                    //off
                    customer.StoreStatus = false;
                    db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    msg = "SIMPLY POS SYSTEM HAS BEEN DE-ACTIVATED";

                }
                _logService.Info($"[MerchantMan][MangoSwitch] complete");
                return Json(new object[] { true, msg });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][MangoSwitch] error code:{code} - status:{status}");
                return Json(new object[] { false, e.Message });
            }

        }
        public JsonResult RenewTrial(string StoreCode)
        {
            _logService.Info($"[MerchantMan][RenewTrial] start StoreCode:{StoreCode}");
            try
            {
                var cus = db.C_Customer.Where(c => c.StoreCode == StoreCode).FirstOrDefault();
                if (cus?.WordDetermine != "Trial")
                {
                    _logService.Warning($"[MerchantMan][RenewTrial] This merchant is not Trial");
                    return Json(new object[] { false, "This merchant is not Trial" });
                }
                if (cus.Version != IMSVersion.POS_VER2.Code<string>())
                {
                    cus.Version = IMSVersion.POS_VER2.Code<string>();
                    db.Entry(cus).State = EntityState.Modified;
                }
                var active_service = db.Store_Services.FirstOrDefault(s => s.StoreCode == StoreCode && s.Active == 1 && s.Type == "license");
                if (active_service == null)
                {
                    _logService.Warning($"[MerchantMan][RenewTrial] Trial Subscription not found");
                    return Json(new object[] { false, "Trial Subscription not found" });
                }
                if (!int.TryParse(AppConfig.Cfg.MangoDemoTrial?.TrialDuration, out int demo_duration))
                {
                    demo_duration = 15;
                };
                if (active_service.RenewDate > DateTime.UtcNow)
                {
                    active_service.RenewDate = /*CommonFunc.EndMonth(*/active_service.RenewDate.Value.AddDays(demo_duration)/*)*/;
                }
                else
                {
                    active_service.RenewDate = /*CommonFunc.EndMonth(*/DateTime.UtcNow.Date.AddDays(demo_duration)/*)*/;
                }
                db.Entry(active_service).State = EntityState.Modified;
                db.SaveChanges();
                active_service.LastUpdateAt = DateTime.UtcNow;
                active_service.LastUpdateBy = cMem.FullName;

                //using (MerchantService service = new MerchantService(true))
                //{
                //    //var store = service.PrimitiveStoreProfile(StoreCode);
                //    StoreProfileReq store = new MerchantService().GetStoreProfileReady(active_service.Id, true, "active");
                //    service.DoRequest(store);
                //    service.SyncStoreBaseService(active_service.Id, 1);
                //}
                SalesLeadService salesLeadService = new SalesLeadService();
                var sl = db.C_SalesLead.FirstOrDefault(x => x.CustomerCode == cus.CustomerCode);
                if (sl != null)
                {
                    salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.CustomerName, title: "Renew trial account success", description: "Renew trial account success ", MemberNumber: cMem.MemberNumber);
                }
                _logService.Info($"[MerchantMan][RenewTrial] completed");
                return Json(new object[] { true, "Renew Trial completed", (active_service?.RenewDate.Value.Date - DateTime.UtcNow.Date).Value.Days });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][RenewTrial] error StoreCode:{StoreCode}");
                return Json(new object[] { false, e.Message });
            }

        }
        public JsonResult HardReset(string StoreCode)
        {
            _logService.Info($"[MerchantMan][HardReset] start StoreCode:{StoreCode}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_hardreset")) != true || access["customer_hardreset"] != true)
                {
                    throw new Exception("You dont have permission to change!");
                }

                var c = db.C_Customer.Where(x => x.StoreCode == StoreCode).FirstOrDefault();
                string partnerLink = new MerchantService().GetPartner(c.CustomerCode).PosApiUrl;
                string url = AppConfig.Cfg.HardResetUrl(partnerLink);
                string SalonNameLog = "";
                if (c != null)
                {
                    SalonNameLog = c.BusinessName + " (#" + c.StoreCode + ")";
                }
                HttpResponseMessage result = ClientRestAPI.CallRestApi(url + StoreCode, "", "", "get", null, SalonName: SalonNameLog);
                if (result?.IsSuccessStatusCode == true)
                {
                    string responseJson = result.Content.ReadAsStringAsync().Result;
                    var responeData = JsonConvert.DeserializeObject<ApiPOSResponse>(responseJson);
                    _logService.Warning($"[MerchantMan][HardReset] completed responeData.Status:{responeData.Status}");
                    if (responeData.Status.Equals("200.0"))
                    {
                        return Json(new object[] { true, responeData.Message }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new object[] { false, responeData.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _logService.Warning($"[MerchantMan][HardReset] Mango POS system not responding!");
                    throw new Exception("Simply POS system not responding!");
                }

            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][HardReset] error StoreCode:{StoreCode}");
                return Json(new object[] { false, e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SoftReset(string StoreCode)
        {
            _logService.Info($"[MerchantMan][SoftReset] start StoreCode:{StoreCode}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_hardreset")) != true || access["customer_hardreset"] != true)
                {
                    throw new Exception("You dont have permission to change!");
                }

                var c = db.C_Customer.Where(x => x.StoreCode == StoreCode).FirstOrDefault();
                string partnerLink = new MerchantService().GetPartner(c.CustomerCode).PosApiUrl;
                string url = AppConfig.Cfg.SoftResetUrl(partnerLink);
                string SalonNameLog = "";
                if (c != null)
                {
                    SalonNameLog = c.BusinessName + " (#" + c.StoreCode + ")";
                }
                HttpResponseMessage result = ClientRestAPI.CallRestApi(url + StoreCode, "", "", "get", null, SalonName: SalonNameLog);
                if (result?.IsSuccessStatusCode == true)
                {
                    string responseJson = result.Content.ReadAsStringAsync().Result;
                    var responeData = JsonConvert.DeserializeObject<ApiPOSResponse>(responseJson);
                    _logService.Info($"[MerchantMan][SoftReset] responeData.Status:{responeData.Status}");
                    if (responeData.Status.Equals("200.0"))
                    {
                        return Json(new object[] { true, responeData.Message }, JsonRequestBehavior.AllowGet);
                    }
                    return Json(new object[] { false, responeData.Message }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("Simply POS system not responding!");
                }
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][SoftReset] error StoreCode:{StoreCode}");
                return Json(new object[] { false, e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public void DelPairingCode(string StoreCode)
        {
            _logService.Info($"[MerchantMan][DelPairingCode] start StoreCode:{StoreCode}");
            try
            {
                var c = db.C_Customer.Where(x => x.StoreCode == StoreCode).FirstOrDefault();
                string partnerLink = new MerchantService().GetPartner(c.CustomerCode).PosApiUrl;
                string url = AppConfig.Cfg.DelPairingCodeUrl(partnerLink);
                string SalonNameLog = "";
                if (c != null)
                {
                    SalonNameLog = c.BusinessName + " (#" + c.StoreCode + ")";
                }
                HttpResponseMessage result = ClientRestAPI.CallRestApi(url + StoreCode, "", "", "get", null, SalonName: SalonNameLog);

                _logService.Info($"[MerchantMan][DelPairingCode] api response", new { HttpResponseMessage = Newtonsoft.Json.JsonConvert.SerializeObject(result) });
                //if (result.IsSuccessStatusCode)
                //{
                //string responseJson = result.Content.ReadAsStringAsync().Result;
                //var responeData = JsonConvert.DeserializeObject<ApiPOSResponse>(responseJson);
                //if (responeData.Status.Equals("200.0"))
                //{
                //    return Json(new object[] { true, responeData.Message }, JsonRequestBehavior.AllowGet);
                //}
                //return Json(new object[] { false, responeData.Message }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    var e = new Exception("Mango POS system not responding!");
                //    e.Data.Add("Result", result); throw e;
                //}
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][DelPairingCode] error StoreCode:{StoreCode}");
                // new WriteLogErrorService().InsertLogError(e);
            }
        }

        #region Merchant Popup

        /// <summary>
        /// Get Merchant Info
        /// </summary>
        /// <param name="id">Customer Id</param>
        /// <param name="update">[update = true]-la Add New or Edit --- [update = false]-la View Detail</param>
        /// <returns></returns>
        public ActionResult GetMerchantInfo(long? id, bool? update, string cuscode = "", bool storeinhouse = false)
        {
            _logService.Info($"[MerchantMan][GetMerchantInfo] start id:{id} - update:{update} - cuscode:{cuscode} - storeinhouse:{storeinhouse}");
            try
            {
                WebDataModel db = new WebDataModel();
                ViewBag.p = access;
                var customer = new C_Customer();
                if (id > 0) //Edit merchant
                {
                    ViewData["update"] = update;
                    customer = db.C_Customer.Find(id);
                    ViewBag.Contacts = db.C_CustomerContact.Where(c => c.CustomerId == id).ToList();
                    ViewBag.ListAddInfo = db.C_CustomerAdditionalInfo.Where(a => a.CustomerID == customer.Id).ToList();
                }
                else if (!string.IsNullOrWhiteSpace(cuscode))
                {
                    ViewData["update"] = update;
                    customer = db.C_Customer.Where(c => c.CustomerCode == cuscode).FirstOrDefault();
                    ViewBag.Contacts = db.C_CustomerContact.Where(c => c.CustomerId == customer.Id).ToList();
                    ViewBag.ListAddInfo = db.C_CustomerAdditionalInfo.Where(a => a.CustomerID == customer.Id).ToList();
                }
                var AllOptionEnum = db.EnumValues.ToList();
                ViewBag.SourceAvailables = AllOptionEnum.Where(x => x.Namespace == Utils.IEnums.MerchantOptionEnum.Source.Code<string>()).ToList();
                ViewBag.ProcessorAvailables = AllOptionEnum.Where(x => x.Namespace == Utils.IEnums.MerchantOptionEnum.Processor.Code<string>()).ToList();
                ViewBag.TerminalTypeAvailables = AllOptionEnum.Where(x => x.Namespace == Utils.IEnums.MerchantOptionEnum.TerminalType.Code<string>()).ToList();
                ViewBag.DeviceNameAvailables = AllOptionEnum.Where(x => x.Namespace == Utils.IEnums.MerchantOptionEnum.DeviceName.Code<string>()).ToList();
                ViewBag.POSStructureAvailables = AllOptionEnum.Where(x => x.Namespace == Utils.IEnums.MerchantOptionEnum.POSStructure.Code<string>()).ToList();
                ViewBag.DeviceSetupStructureAvailables = AllOptionEnum.Where(x => x.Namespace == Utils.IEnums.MerchantOptionEnum.DeviceSetupStructure.Code<string>()).ToList();
                //chi can load united state
                var ct = from c in db.Ad_Country
                         where c.CountryCode == "US"
                         select new
                         {
                             Country = c.Name,
                             Name = c.Name
                         };

                ViewData["STORE_IN_HOUSE"] = string.Empty;
                if (storeinhouse)
                    ViewData["STORE_IN_HOUSE"] = MerchantType.STORE_IN_HOUSE.Code<string>();

                ViewBag.BCountries = new SelectList(ct, "Country", "Name", customer.BusinessCountry ?? "United States");
                ViewBag.OCountries = new SelectList(ct, "Country", "Name", customer.Country ?? "United States");
                ViewBag.SupportingInfo = db.C_CustomerSupportingInfo.FirstOrDefault(c => c.CustomerId == customer.Id) ?? new C_CustomerSupportingInfo { };
                ViewBag.Hardwares = db.Order_Products.Where(c => db.O_Orders.Any(o => o.CustomerCode == customer.CustomerCode && o.OrdersCode == c.OrderCode && (o.Status == "Paid_Wait" || o.Status == "Closed"))).ToList();
                ViewBag.MemberList = db.P_Member.Where(c => c.Active == true && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId)).ToList();
                ViewBag.PartnerList = db.C_Partner.Where(c => c.Status == 1 && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId)).OrderBy(c => c.SiteId).ToList();
                int LicenseActiveStatus = EnrichcousBackOffice.Utils.IEnums.LicenseStatus.ACTIVE.Code<int>();
                var IsExistLicenseActive = db.Store_Services.AsNoTracking().Any(x => x.Type == "license" && x.Active == LicenseActiveStatus && x.StoreCode == customer.StoreCode);
                ViewBag.IsExistLicenseActive = IsExistLicenseActive;
                //[update = true]-la Add New or Edit --- [update = false]-la View Detail
                ViewData["update"] = update;
                _logService.Info($"[MerchantMan][GetMerchantInfo] completed", new { customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer) });
                return PartialView("_MerchantPopupPartial", customer);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][GetMerchantInfo] error id:{id} - update:{update} - cuscode:{cuscode} - storeinhouse:{storeinhouse}");
                throw ex;
                //TempData["e"] = ex.Message;
                //return RedirectToAction("index");
            }
        }

        //public void SaveHistoryUpdate(C_Customer Customer, string Action)
        //{
        //    try
        //    {
        //        if (Customer != null)
        //        {
        //            var customer = db.C_Customer.Where(x => x.CustomerCode == Customer.CustomerCode).FirstOrDefault();
        //            if (customer != null)
        //            {
        //                var newHistory = (cMem?.FullName ?? "IMS system") + "$" + DateTime.UtcNow + "$" + Action;
        //                var oldHistory = (Customer.UpdateBy ?? string.Empty).Split('|').ToList();
        //                if (oldHistory.Count == 3) oldHistory = oldHistory.Skip(1).ToList();
        //                oldHistory.Add(newHistory);
        //                customer.UpdateBy = string.Join("|", oldHistory);
        //                //db.Entry(Customer).State = EntityState.Modified;
        //                db.SaveChanges();
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //ignore
        //    }
        //}

        #region Merchant popup estimete
        public async Task<JsonResult> NewMerchantEstimete(C_Customer c)
        {
            _logService.Info($"[MerchantMan][NewMerchantEstimete] start customer-id:{c.Id} - customer-OwnerName:{c.OwnerName}");
            try
            {
                if (string.IsNullOrEmpty(c.SalonEmail))
                {
                    c.SalonEmail = c.Email;
                }
                if (string.IsNullOrEmpty(c.SalonPhone))
                {
                    c.SalonPhone = c.CellPhone;
                }
                if (db.C_Customer.Any(m =>
                (m.SalonEmail == c.SalonEmail || m.BusinessEmail == c.SalonEmail) && !string.IsNullOrEmpty(c.SalonEmail)))
                {
                    _logService.Warning($"[MerchantMan][NewMerchantEstimete] Salon Email already exists!");
                    return Json(new object[] { false, null, "Salon Email already exists!" });
                    //     return Json(new object[] { false, null, "Salon Email already exists!" });
                }

                var partner = db.C_Partner.FirstOrDefault(p => p.Code == cMem.BelongToPartner) ?? new C_Partner { };

                c.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                c.CustomerCode = new MerchantService().MakeId();
                c.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(c.CustomerCode, "[^.0-9]", "");
                c.OwnerName = c.ContactName;
                c.OwnerMobile = c.CellPhone;
                c.CreateAt = DateTime.UtcNow;
                c.CreateBy = cMem.FullName;
                c.Password = db.SystemConfigurations.FirstOrDefault().MerchantPasswordDefault ?? string.Empty;
                c.MD5PassWord = SecurityLibrary.Md5Encrypt(c.Password);
                c.MangoEmail = c.MangoEmail ?? c.SalonEmail;
                c.BusinessEmail = c.SalonEmail;
                c.BusinessAddressStreet = c.SalonAddress1;
                c.BusinessCity = c.SalonCity;
                c.BusinessPhone = c.SalonPhone;
                c.BusinessState = c.SalonState;
                c.BusinessZipCode = c.SalonZipcode;
                c.Zipcode = c.SalonZipcode;
                c.SiteId = cMem.SiteId;
                c.SalonTimeZone_Number = Ext.EnumParse<TIMEZONE_NUMBER>(c.SalonTimeZone).Text();
                c.Active = 1;//[1: Active], [0: Inactive], [-1: Not processing]
                c.PartnerCode = partner.Code;
                c.PartnerName = partner.Name;

                if (!string.IsNullOrWhiteSpace(Request["expected_open_date"]))
                {
                    c.BusinessStartDate = DateTime.Parse(Request["expected_open_date"].ToString());
                }
                db.C_Customer.Add(c);

                var check_in = Request["c_checkin"];
                var c_payroll = Request["c_payroll"];
                var c_automation = Request["c_automation"];
                var c_techmanagement = Request["c_techmanagement"];
                string features = "<h3 style='padding:5px;border-left:solid 5px red;'>FEATURES OF INTERES</h3>";
                features += "<table style='border:1px solid gray;border-collapse:collapse;'>" +
                    "<tr><th style ='width:150px;padding:7px'>- CHECK IN</th><td style='padding:7px'>" + (string.IsNullOrWhiteSpace(check_in) == false ? "<b style='color:blue'>Yes</b>" : "") +
                    "</td></tr><tr><th style='padding:7px'>- PAYROLL:</th> <td style='padding:7px'>" + (string.IsNullOrWhiteSpace(c_payroll) == false ? "<b  style='color:blue'>Yes</b>" : "") +
                    "</td></tr><tr><th style='padding:7px'>- AUTOMATION:</th> <td style='padding:7px'>" + (string.IsNullOrWhiteSpace(c_automation) == false ? "<b  style='color:blue'>Yes</b>" : "") +
                    "</td></tr><tr><th style='padding:7px'>- TECH MANAGEMENT:</th><td style='padding:7px'>" + (string.IsNullOrWhiteSpace(c_techmanagement) == false ? "<b  style='color:blue'>Yes</b>" : "") +
                    "</td></tr><tr><th style='padding:7px'>- OTHER:</th><td style='padding:7px'>" + Request["c_other_note"] +
                    "</td></tr></table>";

                string salonInfo = "<br/><h3 style='padding:5px;border-left:solid 5px red;'>SALON INFO</h3>" +
                    "<table style='border:1px solid gray;border-collapse:collapse;'>" +
                    "<tr><th style ='width:150px;padding:7px'>- Salon name:</th><td style='padding:7px'>" + c.BusinessName +
                    "</td></tr><tr><th style='padding:7px'>- Contact name:</th> <td style='padding:7px'>" + c.OwnerName +
                    "</td></tr><tr><th style='padding:7px'>- Contact number:</th> <td style='padding:7px'>" + c.OwnerMobile +
                    "</td></tr><tr><th style='padding:7px'>- Salon number:</th> <td style='padding:7px'>" + c.BusinessPhone +
                    "</td></tr><tr><th style='padding:7px'>- Salon Address:</th> <td style='padding:7px'>" + c.BusinessAddressStreet + ", " + c.BusinessCity + ", " + c.BusinessState + ", " + c.BusinessZipCode + ", " + c.BusinessCountry +
                    "</td></tr><tr><th style='padding:7px'>- Note:</th> <td style='padding:7px'>" + Request["salonType"];
                if (!string.IsNullOrWhiteSpace(Request["expected_open_date"]))
                {
                    salonInfo += "</td></tr><tr><th style='padding:7px'>- Expected open date:</th> <td style='padding:7px'>" + Request["expected_open_date"];
                }
                salonInfo += "</td></tr></table>";
                string appointmentInfo = "";
                if (Request["appointment"] == "1")
                {
                    appointmentInfo += "<br/><h3 style='padding:5px;border-left:solid 5px red;'>APPOINTMENT INFO</h3><table style='border:1px solid gray;border-collapse:collapse;'><tr><th style='padding:7px'><label style='font-size:1.3em'> Schedule time: </label></th><td style='font-size:1.3em;color:blue;padding:7px'>" + Request["schedule_date"]
                        + " - " + Request["schedule_hours"].PadLeft(2, '0')
                        + ":" + Request["schedule_minute"].PadLeft(2, '0')
                        + " " + Request["schedule_am_pm"]
                        + " /Time zone: " + Request["schedule_timezone"]
                        + "</td></tr></table>";

                }
                var feature_interes = new List<string>();
                if (!string.IsNullOrWhiteSpace(check_in)) { feature_interes.Add("checkin"); }
                if (!string.IsNullOrWhiteSpace(c_payroll)) { feature_interes.Add("payroll"); }
                if (!string.IsNullOrWhiteSpace(c_automation)) { feature_interes.Add("automation"); }
                if (!string.IsNullOrWhiteSpace(c_techmanagement)) { feature_interes.Add("techmanagement"); }
                var lead = new C_SalesLead
                {
                    Id = Guid.NewGuid().ToString(),
                    CustomerCode = c.CustomerCode,
                    CustomerName = c.BusinessName,
                    MemberNumber = cMem.MemberNumber,
                    MemberName = cMem.FullName,
                    SL_Status = LeadStatus.Lead.Code<int>(),
                    SL_StatusName = LeadStatus.Lead.Text(),
                    Features_Interes = string.Join(",", feature_interes ?? new List<string>()),
                    Features_Interes_other = Request["c_other"] == "true" ? Request["c_other_note"] : "",
                    CreateAt = DateTime.UtcNow,
                    CreateBy = cMem.FullName,
                    CreateByMemberNumber = cMem.MemberNumber,
                    UpdateAt = DateTime.UtcNow,
                    UpdateBy = cMem.FullName,
                    L_Type = LeadType.RegisterOnIMS.Text(),
                    PotentialRateScore = int.Parse(Request["rate"] ?? "0")
                };

                db.C_SalesLead.Add(lead);
                db.SaveChanges();
                long ticketId = 0;
                if (Request["newsalon_form"] != "ticket")
                {
                    //neu merchant duoc tao tu ticket, thi khong can phai tao them 1 ticket for sales cho merchant nay nua.
                    var d = salonInfo + "<br/>" + features + "<br/>" + appointmentInfo;
                    ticketId = await TicketViewService.AutoTicketScenario.NewTicketSalesLead(c.CustomerCode, "", d);
                }
                if (!string.IsNullOrWhiteSpace(appointmentInfo))
                {
                    string gmt = Request["schedule_timezone"];
                    gmt = gmt.Substring(4, 6);
                    int hour = int.Parse(Request["schedule_hours"].PadLeft(2, '0'));
                    if (Request["schedule_am_pm"] == "PM")
                    {
                        hour += 12;
                    }
                    string start = DateTime.Parse(Request["schedule_date"]).ToString("yyyy-MM-ddT") + hour.ToString().PadLeft(2, '0') + ":" + Request["schedule_minute"].PadLeft(2, '0') + ":00" + gmt;

                    string des = "- Salon name: " + c.BusinessName + "\n" +
                        @"- Owner name: " + c.OwnerName + "\n" +
                         @"- Phone: " + c.OwnerMobile + "\n" +
                          @"- Address: " + c.BusinessAddressStreet + ", " + c.BusinessCity + ", " + c.BusinessState + ", " + c.BusinessZipCode + "\n" +
                          @"- Featues of interest: " + "\n";

                    if (string.IsNullOrWhiteSpace(Request["c_checkin"]) == false)
                    {
                        des += "+ CHECKIN" + "\n";
                    }
                    if (string.IsNullOrWhiteSpace(Request["c_payroll"]) == false)
                    {
                        des += "+ PAYROLL" + "\n";
                    }
                    if (string.IsNullOrWhiteSpace(Request["c_automation"]) == false)
                    {
                        des += "+ AUTOMATION" + "\n";
                    }
                    if (string.IsNullOrWhiteSpace(Request["c_techmanagement"]) == false)
                    {
                        des += "+ TECHMANAGEMENT" + "\n";
                    }
                    if (string.IsNullOrWhiteSpace(Request["c_other_note"]) == false)
                    {
                        des += "+ OTHER:_" + Request["c_other_note"] + "\n";
                    }

                    var calendarEve = new Calendar_Event
                    {
                        Description = Request["appoiment_description"],
                        Color = "#FF8C00",
                        StartEvent = start,
                        GMT = gmt,
                        Name = Request["appoiment_title"],
                        TimeZone = Request["schedule_timezone"],
                        LastUpdateAt = DateTime.UtcNow,
                        LastUpdateBy = cMem.FullName,
                        LastUpdateByNumber = cMem.MemberNumber,
                        CreateAt = DateTime.UtcNow,
                        CreateBy = cMem.FullName,
                        CustomerCode = c.CustomerCode,
                        CustomerName = c.BusinessName,
                        MemberNumber = cMem.MemberNumber,
                        Done = Request["event_done"].ToString() == "1" ? 1 : 0,

                    };
                    if (ticketId > 0)
                    {
                        calendarEve.TicketId = ticketId.ToString();
                        calendarEve.TicketName = "New salon: " + (c.BusinessName) ?? c.OwnerName;

                    }
                    var ce = new CalendarViewService(db);
                    ce.AddNewEvent(calendarEve, out string err);
                }
                //"
                await EngineContext.Current.Resolve<IEnrichUniversalService>().InitialStoreDataAsync(c.StoreCode);
                _logService.Info($"[MerchantMan][NewMerchantEstimete] completed", new { customer = Newtonsoft.Json.JsonConvert.SerializeObject(c) });
                return Json(new object[] { true, c, "Merchant has been saved!" });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][NewMerchantEstimete] start customer-id:{c.Id} - customer-OwnerName:{c.OwnerName}");
                return Json(new object[] { false, null, e.Message });
            }

        }

        #endregion

        [AllowAnonymous]
        public async Task<JsonResult> HookSynsClickUp(string CustomerCode)
        {
            C_Customer customer = await db.C_Customer.FirstOrDefaultAsync(x => x.CustomerCode == CustomerCode);

            if(customer == null)
            {
                return Json(new object[] { false, "Customer Not Found" });
            }
            await _clickUpConnectorService.SyncMerchantToClickUpAsync(customer.Id.ToString());
            return Json(new object[] { true, "Syncing to Clickup" });
        }


            /// <summary>
            /// Save Merchant
            /// </summary>
            /// <param name="CM_Model"></param>
            /// <returns></returns>
        public async Task<JsonResult> Save(C_Customer customer, C_CustomerSupportingInfo customerSP)
        {
            _logService.Info($"[MerchantMan][Save] start", new { customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer), customerSP = Newtonsoft.Json.JsonConvert.SerializeObject(customerSP) });
            try
            {
                WebDataModel db = new WebDataModel();
                string a = Request["State"];
                var updCus = new C_Customer();
                var listTimezione = new MerchantService().ListTimeZone();

                // using (var Trans = db.Database.BeginTransaction())
                {
                    string mess = "";
                    try
                    {
                        #region Validate
                        if (string.IsNullOrEmpty(customer.CellPhone) == true)
                        {
                            throw new Exception("Owner phone is required.");
                        }

                        #endregion

                        var customer_id = string.IsNullOrEmpty(Request["hd_cus_id"]) == true ? 0 : long.Parse(Request["hd_cus_id"]);

                        if (customer_id > 0) //Edit
                        {

                            //Check access
                            if ((access.Any(k => k.Key.Equals("customer_addnew")) == false || access["customer_addnew"] != true) &&
                                (access.Any(k => k.Key.Equals("customer_update")) == false || access["customer_update"] != true))
                            {
                                throw new Exception("Access denied");
                            }
                            //.End
                            var editcustomer = db.C_Customer.Find(customer_id);
                            var logCustomer = new C_Customer { };
                            if (db.C_Customer.Any(c => c.SalonEmail == customer.SalonEmail && c.Id != editcustomer.Id))
                            {
                                throw new Exception("Salon Email already exist.");
                            }
                            if (db.C_Customer.Any(c => c.MangoEmail == customer.MangoEmail && c.Id != editcustomer.Id))
                            {
                                throw new Exception("Mango Email already exist.");
                            }

                            if (editcustomer != null)
                            {
                                if (access.Any(k => k.Key.Equals("customer_update")) == true || access["customer_update"] == true)
                                {
                                    #region Update Customer
                                    logCustomer.BusinessName = editcustomer.BusinessName != customer.BusinessName ? customer.BusinessName : null;
                                    logCustomer.BusinessEmail = editcustomer.BusinessEmail != customer.BusinessEmail ? customer.BusinessEmail : null;
                                    logCustomer.BusinessAddressCivicNumber = editcustomer.BusinessAddressCivicNumber != customer.BusinessAddressCivicNumber ? customer.BusinessAddressCivicNumber : null;
                                    logCustomer.BusinessAddressStreet = editcustomer.BusinessAddressStreet != customer.BusinessAddressStreet ? customer.BusinessAddressStreet : null;
                                    logCustomer.BusinessPhone = editcustomer.BusinessPhone != customer.SalonPhone ? customer.SalonPhone : null;
                                    logCustomer.Website = editcustomer.Website != customer.Website ? customer.Website : null;
                                    logCustomer.Active = editcustomer.Active != customer.Active ? customer.Active : null;
                                    logCustomer.DepositBankName = editcustomer.DepositBankName != customer.DepositBankName ? customer.DepositBankName : null;
                                    logCustomer.DepositAccountNumber = editcustomer.DepositAccountNumber != customer.DepositAccountNumber ? customer.DepositAccountNumber : null;
                                    logCustomer.ContactName = editcustomer.ContactName != customer.ContactName ? customer.ContactName : null;
                                    logCustomer.CellPhone = editcustomer.CellPhone != customer.CellPhone ? customer.CellPhone : null;
                                    logCustomer.OwnerMobile = editcustomer.OwnerMobile != customer.OwnerMobile ? customer.OwnerMobile : null;
                                    logCustomer.OwnerName = editcustomer.OwnerName != customer.OwnerName ? customer.OwnerName : null;
                                    logCustomer.PreferredLanguage = editcustomer.PreferredLanguage != customer.PreferredLanguage ? customer.PreferredLanguage : null;
                                    logCustomer.PreferredName = editcustomer.PreferredName != customer.PreferredName ? customer.PreferredName : null;
                                    logCustomer.ManagerPhone = editcustomer.ManagerPhone != customer.ManagerPhone ? customer.ManagerPhone : null;
                                    logCustomer.SocialSecurity = editcustomer.SocialSecurity != customer.SocialSecurity ? customer.SocialSecurity : null;
                                    logCustomer.LegalName = editcustomer.LegalName != customer.LegalName ? customer.LegalName : null;
                                    logCustomer.Email = editcustomer.Email != customer.Email ? customer.Email : null;
                                    logCustomer.BusinessDescription = editcustomer.BusinessDescription != customer.BusinessDescription ? customer.BusinessDescription : null;
                                    logCustomer.BusinessCountry = editcustomer.BusinessCountry != customer.BusinessCountry ? customer.BusinessCountry : null;
                                    logCustomer.BusinessState = editcustomer.BusinessState != customer.BusinessState ? customer.BusinessState : null;
                                    logCustomer.BusinessCity = editcustomer.BusinessCity != customer.SalonCity ? customer.SalonCity : null;
                                    logCustomer.BusinessZipCode = editcustomer.BusinessZipCode != customer.SalonZipcode ? customer.SalonZipcode : null;
                                    logCustomer.BusinessStartDate = editcustomer.BusinessStartDate != customer.BusinessStartDate ? customer.BusinessStartDate : null;
                                    logCustomer.GoLiveDate = editcustomer.GoLiveDate != customer.GoLiveDate ? customer.GoLiveDate : null;
                                    logCustomer.CancelDate = editcustomer.CancelDate != customer.CancelDate ? customer.CancelDate : null;
                                    logCustomer.OwnerAddressCivicNumber = editcustomer.OwnerAddressCivicNumber != customer.OwnerAddressCivicNumber ? customer.OwnerAddressCivicNumber : null;
                                    logCustomer.OwnerAddressStreet = editcustomer.OwnerAddressStreet != customer.OwnerAddressStreet ? customer.OwnerAddressStreet : null;
                                    logCustomer.City = editcustomer.City != customer.City ? customer.City : null;
                                    logCustomer.State = editcustomer.State != customer.State ? customer.State : null;
                                    logCustomer.Country = editcustomer.Country != customer.Country ? customer.Country : null;
                                    logCustomer.Zipcode = editcustomer.Zipcode != customer.Zipcode ? customer.Zipcode : null;
                                    logCustomer.FederalTaxId = editcustomer.FederalTaxId != customer.FederalTaxId ? customer.FederalTaxId : null;
                                    logCustomer.Birthday = editcustomer.Birthday != customer.Birthday ? customer.Birthday : null;
                                    logCustomer.MangoEmail = editcustomer.MangoEmail != customer.MangoEmail ? customer.MangoEmail : null;
                                    logCustomer.BusinessAddress = editcustomer.BusinessAddress != customer.SalonAddress1 ? customer.SalonAddress1 : null;
                                    logCustomer.SalonAddress2 = editcustomer.SalonAddress2 != customer.SalonAddress2 ? customer.SalonAddress2 : null;
                                    logCustomer.SalonCity = editcustomer.SalonCity != customer.SalonCity ? customer.SalonCity : null;
                                    logCustomer.SalonState = editcustomer.SalonState != customer.SalonState ? customer.SalonState : null;
                                    logCustomer.SalonZipcode = editcustomer.SalonZipcode != customer.SalonZipcode ? customer.SalonZipcode : null;
                                    logCustomer.SalonPhone = editcustomer.SalonPhone != customer.SalonPhone ? customer.SalonPhone : null;
                                    logCustomer.SalonTimeZone = editcustomer.SalonTimeZone != customer.SalonTimeZone ? customer.SalonTimeZone : null;
                                    logCustomer.SalonEmail = editcustomer.SalonEmail != customer.SalonEmail ? customer.SalonEmail : null;
                                    logCustomer.FullName = editcustomer.MemberNumber != customer.MemberNumber ? "#" + customer.MemberNumber + " - " + customer.FullName : null;
                                    logCustomer.PartnerName = editcustomer.PartnerCode != customer.PartnerCode ?
                                        (db.C_Partner.FirstOrDefault(c => c.Code == customer.PartnerCode && !string.IsNullOrEmpty(customer.PartnerCode))?.Name ?? "Simply Pos")
                                        + (string.IsNullOrEmpty(customer.PartnerCode) ? "" : $" (#{customer.PartnerCode})") : null;

                                    var allEnum = db.EnumValues.AsNoTracking().ToList();
                                    logCustomer.Source = editcustomer.Source != customer.Source ? (string.IsNullOrEmpty(customer.Source) ? "" : allEnum.FirstOrDefault(x => x.Value == customer.Source && x.Namespace == Utils.IEnums.MerchantOptionEnum.Source.Code<string>())?.Name) : null;
                                    logCustomer.Processor = editcustomer.Processor != customer.Processor ? (string.IsNullOrEmpty(customer.Processor) ? "" : allEnum.FirstOrDefault(x => x.Value == customer.Processor && x.Namespace == Utils.IEnums.MerchantOptionEnum.Processor.Code<string>())?.Name) : null;
                                    logCustomer.TerminalType = editcustomer.TerminalType != customer.TerminalType ? (string.IsNullOrEmpty(customer.TerminalType) ? "" : allEnum.FirstOrDefault(x => x.Value == customer.TerminalType && x.Namespace == Utils.IEnums.MerchantOptionEnum.TerminalType.Code<string>())?.Name) : null;
                                    logCustomer.SalonZipcode = editcustomer.SalonZipcode != customer.SalonZipcode ? customer.SalonZipcode : null;
                                    logCustomer.MID = editcustomer.MID != customer.MID ? customer.MID : null;
                                    logCustomer.TerminalStatus = editcustomer.TerminalStatus != customer.TerminalStatus ? customer.TerminalStatus : null;

                                    logCustomer.DeviceName = editcustomer.DeviceName != customer.DeviceName ? (string.IsNullOrEmpty(customer.DeviceName) ? "" : allEnum.FirstOrDefault(x => x.Value == customer.DeviceName && x.Namespace == Utils.IEnums.MerchantOptionEnum.DeviceName.Code<string>())?.Name) : null;
                                    logCustomer.POSStructure = editcustomer.POSStructure != customer.POSStructure ? (string.IsNullOrEmpty(customer.POSStructure) ? "" : allEnum.FirstOrDefault(x => x.Value == customer.POSStructure && x.Namespace == Utils.IEnums.MerchantOptionEnum.POSStructure.Code<string>())?.Name) : null;
                                    logCustomer.DeviceSetupStructure = editcustomer.DeviceSetupStructure != customer.DeviceSetupStructure ? (string.IsNullOrEmpty(customer.DeviceSetupStructure) ? "" : allEnum.FirstOrDefault(x => x.Value == customer.DeviceSetupStructure && x.Namespace == Utils.IEnums.MerchantOptionEnum.DeviceSetupStructure.Code<string>())?.Name) : null;
                                    logCustomer.DeviceNote = editcustomer.DeviceNote != customer.DeviceNote ? customer.DeviceNote : null;


                                    editcustomer.BusinessName = customer.BusinessName;
                                    editcustomer.BusinessEmail = customer.SalonEmail;
                                    editcustomer.BusinessAddressCivicNumber = customer.BusinessAddressCivicNumber;
                                    editcustomer.BusinessAddressStreet = customer.BusinessAddressStreet;
                                    editcustomer.BusinessPhone = customer.SalonPhone;
                                    editcustomer.Website = customer.Website;
                                    editcustomer.Active = customer.Active/*true*/;
                                    editcustomer.DepositBankName = customer.DepositBankName;
                                    //editcustomer.BankDDA = customer.BankDDA;
                                    editcustomer.DepositAccountNumber = customer.DepositAccountNumber;
                                    editcustomer.DepositRoutingNumber = customer.DepositRoutingNumber;
                                    editcustomer.ContactName = customer.ContactName;
                                    editcustomer.CellPhone = customer.CellPhone;
                                    editcustomer.Website = customer.Website;
                                    editcustomer.OwnerMobile = customer.CellPhone;
                                    editcustomer.OwnerName = customer.ContactName;
                                    editcustomer.PreferredLanguage = customer.PreferredLanguage;
                                    editcustomer.PreferredName = customer.PreferredName;
                                    editcustomer.ManagerPhone = customer.ManagerPhone;
                                    editcustomer.SocialSecurity = customer.SocialSecurity;
                                    editcustomer.LegalName = customer.LegalName;
                                    editcustomer.Email = customer.Email;
                                    editcustomer.BusinessDescription = customer.BusinessDescription;
                                    editcustomer.BusinessCountry = customer.Country;
                                    editcustomer.BusinessState = customer.SalonState;
                                    editcustomer.BusinessCity = customer.SalonCity;
                                    editcustomer.BusinessZipCode = customer.SalonZipcode;
                                    editcustomer.BusinessStartDate = customer.BusinessStartDate;
                                    editcustomer.GoLiveDate = customer.GoLiveDate;
                                    editcustomer.CancelDate = customer.CancelDate;
                                    editcustomer.OwnerAddressCivicNumber = customer.OwnerAddressCivicNumber;
                                    editcustomer.OwnerAddressStreet = customer.OwnerAddressStreet;
                                    editcustomer.City = customer.City;
                                    editcustomer.State = customer.State;
                                    editcustomer.Country = customer.Country;
                                    editcustomer.Zipcode = customer.Zipcode;
                                    editcustomer.FederalTaxId = customer.FederalTaxId;
                                    //editcustomer.UpdateBy = "<i>" + DateTime.UtcNow.ToString("MMM dd yyyy HH:mm tt") + "</i><br/> by <b>" + cMem.FullName + "</b>|" + customer.UpdateBy;
                                    editcustomer.Birthday = customer.Birthday;
                                    editcustomer.MangoEmail = customer.MangoEmail ?? customer.SalonEmail;
                                    editcustomer.SalonAddress1 = editcustomer.BusinessAddress = customer.SalonAddress1;
                                    editcustomer.SalonAddress2 = customer.SalonAddress2;
                                    editcustomer.SalonCity = customer.SalonCity;
                                    editcustomer.SalonState = customer.SalonState;
                                    editcustomer.SalonZipcode = customer.SalonZipcode;
                                    editcustomer.SalonPhone = customer.SalonPhone;
                                    editcustomer.FullName = customer.FullName;
                                    editcustomer.MemberNumber = customer.MemberNumber;
                                    if (!string.IsNullOrEmpty(customer.PartnerCode))
                                    {
                                        var partner = db.C_Partner.FirstOrDefault(c => c.Code == customer.PartnerCode);
                                        editcustomer.PartnerCode = customer?.PartnerCode;
                                        editcustomer.PartnerName = partner?.Name;
                                        editcustomer.SiteId = partner?.SiteId;
                                    }
                                    else
                                    {
                                        editcustomer.PartnerCode = null;
                                        editcustomer.PartnerName = null;
                                        editcustomer.SiteId = 1;
                                    }
                                    editcustomer.Source = customer.Source;
                                    editcustomer.Processor = customer.Processor;
                                    editcustomer.TerminalType = customer.TerminalType;
                                    editcustomer.TerminalStatus = customer.TerminalStatus;
                                    editcustomer.MID = customer.MID;
                                    editcustomer.DeviceName = customer.DeviceName;
                                    editcustomer.POSStructure = customer.POSStructure;
                                    editcustomer.DeviceSetupStructure = customer.DeviceSetupStructure;
                                    editcustomer.DeviceNote = customer.DeviceNote;
                                    if (editcustomer.SalonTimeZone != customer.SalonTimeZone)
                                    {
                                        editcustomer.SalonTimeZone = customer.SalonTimeZone;
                                        editcustomer.SalonTimeZone_Number = listTimezione.FirstOrDefault(c => c.Name == customer.SalonTimeZone)?.TimeDT ?? Ext.EnumParse<TIMEZONE_NUMBER>(customer.SalonTimeZone).Text();
                                    }
                                    editcustomer.SalonEmail = customer.SalonEmail;
                                    //editcustomer.UpdateBy = cMem.FullName;
                                    db.Entry(editcustomer).State = EntityState.Modified;
                                    updCus = editcustomer;
                                    var sl = db.C_SalesLead.Where(x => x.CustomerCode == editcustomer.CustomerCode).FirstOrDefault() ?? new C_SalesLead();
                                    if (sl.Id != null)
                                    {
                                        sl.CustomerName = editcustomer.BusinessName;
                                        sl.UpdateAt = DateTime.UtcNow;
                                        sl.UpdateBy = cMem.FullName;
                                        sl.L_SalonName = editcustomer.BusinessName;
                                        sl.L_Address = editcustomer.BusinessAddress;
                                        sl.L_State = editcustomer.BusinessState;
                                        sl.L_City = editcustomer.BusinessCity;
                                        sl.L_Zipcode = editcustomer.Zipcode;
                                        sl.L_Country = editcustomer.Country;
                                        sl.L_Phone = editcustomer.BusinessPhone;
                                        db.Entry(sl).State = EntityState.Modified;
                                    }
                                    await db.SaveChangesAsync();
                                    _merchantService.SaveHistoryUpdate(editcustomer.CustomerCode, "Updated merchant information");
                                    if (sl.Id != null)
                                    {
                                        var contentLog = "Merchant information has been updated.<br/>";
                                        foreach (PropertyInfo propertyInfo in logCustomer.GetType().GetProperties())
                                        {
                                            var value = propertyInfo.GetValue(logCustomer, null);
                                            if (value != null && propertyInfo.Name != "Id")
                                            {
                                                if (propertyInfo.Name == "FullName")
                                                {
                                                    contentLog += " - Account manager: " + value + "<br/>";
                                                }
                                                else if (propertyInfo.Name == "PartnerName")
                                                {
                                                    contentLog += " - Company / Partner: " + value ?? "Simply Pos" + "<br/>";
                                                }
                                                else if (propertyInfo.Name == "TerminalStatus")
                                                {
                                                    contentLog += "Terminal Status: Active" ?? "Inactive";
                                                }
                                                else
                                                {
                                                    contentLog += " - " + propertyInfo.Name + ": " + value + "<br/>";
                                                }
                                            }
                                            // do stuff here
                                        }
                                        SalesLeadService _salesLeadService = new SalesLeadService();
                                        _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.L_SalonName, title: "Update merchant information", description: contentLog, MemberNumber: cMem.MemberNumber);
                                    }

                                    //if (db.Store_Services.Any(s => s.StoreCode == editcustomer.StoreCode && s.Active == 1) && !string.IsNullOrEmpty(editcustomer.Version))
                                    //{
                                    //    //PushStoreToMangoPos
                                    //    var active_service_id = db.Store_Services.FirstOrDefault(s => s.StoreCode == editcustomer.StoreCode && s.Active == 1)?.Id;
                                    //    using (MerchantService service = new MerchantService(true))
                                    //    {
                                    //        //var rs = await service.ApproveAction(editcustomer.StoreCode, active_service_id, true, "same-active");
                                    //        StoreProfileReq storeProfile = service.GetStoreProfileReady(null, false, null, editcustomer.StoreCode);
                                    //        service.DoRequest(storeProfile);
                                    //    }
                                    //}

                                    //push data to MangoPos
                                    var isExistLicenseActive = db.Store_Services.Where(x => x.StoreCode == editcustomer.StoreCode && x.Type == "license" && x.Active == 1 && DbFunctions.TruncateTime(x.RenewDate) >= DateTime.UtcNow).Count() > 0;
                                    if (isExistLicenseActive)
                                    {
                                        using (MerchantService service = new MerchantService(true))
                                        {
                                            //var rs = await service.ApproveAction(editcustomer.StoreCode, active_service_id, true, "same-active");
                                            StoreProfileReq storeProfile = service.GetStoreProfileReady(null, false, null, editcustomer.StoreCode);
                                            service.DoRequest(storeProfile);
                                        }

                                    }
                                    #endregion
                                }
                            }

                            var editCSP = db.C_CustomerSupportingInfo.FirstOrDefault(c => c.CustomerId == editcustomer.Id) ??
                                new C_CustomerSupportingInfo
                                {
                                    Id = long.Parse(DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")),
                                    CreateBy = cMem.FullName,
                                    CreateAt = DateTime.UtcNow,
                                    CustomerId = editcustomer.Id
                                };
                            editCSP.RemoteLogin = customerSP.RemoteLogin;
                            editCSP.MoreHardware = customerSP.MoreHardware;
                            editCSP.OtherNotes = customerSP.OtherNotes;
                            editCSP.UpdateBy = cMem.FullName;
                            editCSP.UpdateAt = DateTime.UtcNow;
                            db.C_CustomerSupportingInfo.AddOrUpdate(editCSP);
                            await db.SaveChangesAsync();
                            try
                            {
                                var _clickUpConnectorService = EngineContext.Current.Resolve<IClickUpConnectorService>();
                                await _clickUpConnectorService.SyncMerchantToClickUpAsync(editcustomer.Id.ToString());

                            }
                            catch (Exception ex)
                            {
                                ImsLogService.WriteLog(ex.ToString(), "Exception", "MerchantMan.Save");
                            }
                           

                            mess = "Update merchant info completed!";
                        }
                        else //Create
                        {

                            //Check access edit
                            if (access.Any(k => k.Key.Equals("customer_addnew")) == false || access["customer_addnew"] != true)
                            {
                                throw new Exception("Access denied");
                            }
                            //.End

                            if (db.C_Customer.Any(c => c.SalonEmail == customer.SalonEmail) == true)
                            {
                                throw new Exception("Salon email already exist.");
                            }
                            if (db.C_Customer.Any(c => c.MangoEmail == customer.MangoEmail) == true)
                            {
                                throw new Exception("Simply Email already exist.");
                            }
                            #region Create Customer

                            customer.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                            /*
                            var num_cus = db.C_Customer.Where(m => m.CreateAt.Value.Year == DateTime.Now.Year && m.CreateAt.Value.Month == DateTime.Now.Month).Count();
                            customer.CustomerCode = DateTime.Now.ToString("yyMM") + (num_cus + 1).ToString().PadLeft(4, '0');
                            */
                            customer.MangoEmail = customer.MangoEmail ?? customer.SalonEmail;
                            customer.CustomerCode = new MerchantService().MakeId();
                            customer.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(customer.CustomerCode, "[^.0-9]", "");
                            customer.CreateAt = DateTime.UtcNow;
                            customer.BusinessCountry = customer.Country;
                            customer.BusinessPhone = customer.SalonPhone;
                            customer.BusinessAddressStreet = customer.SalonAddress1;
                            customer.BusinessState = customer.SalonState;
                            customer.BusinessCity = customer.SalonCity;
                            customer.BusinessZipCode = customer.SalonZipcode;
                            customer.OwnerMobile = customer.CellPhone;
                            customer.OwnerName = customer.ContactName;
                            customer.SalonTimeZone_Number = listTimezione.FirstOrDefault(c => c.Name == customer.SalonTimeZone)?.TimeDT ?? Ext.EnumParse<TIMEZONE_NUMBER>(customer.SalonTimeZone).Text();
                            customer.Active = 1/*true*/;//[1: Active], [0: Inactive], [-1: Not processing]
                            customer.BusinessEmail = customer.SalonEmail;
                            customer.CreateBy = cMem.FullName;
                            customer.Version = IMSVersion.POS_VER2.Code<string>();

                            var partner = db.C_Partner.FirstOrDefault(c => c.Code == customer.PartnerCode && !string.IsNullOrEmpty(customer.PartnerCode)) ?? new C_Partner();
                            customer.PartnerName = partner.Name;
                            customer.SiteId = partner.SiteId;

                            //add contact list
                            var listcontact = db.C_CustomerContact.Where(c => c.CustomerId == 0)
                                .ToList();
                            foreach (var contact in listcontact)
                            {
                                contact.CustomerId = customer.Id;
                            }
                            //add merchant process list
                            var listmp = db.C_MerchantProcess.Where(c => c.CustomerCode == "0")
                                .ToList();
                            foreach (var mp in listmp)
                            {
                                mp.CustomerCode = customer.CustomerCode;
                            }

                            db.C_Customer.Add(customer);
                            await db.SaveChangesAsync();
                            await EngineContext.Current.Resolve<IEnrichUniversalService>().InitialStoreDataAsync(customer.StoreCode);

                            var sl = new C_SalesLead();
                            sl.Id = Guid.NewGuid().ToString();
                            sl.SL_Status = LeadStatus.Merchant.Code<int>();
                            sl.SL_StatusName = LeadStatus.Merchant.Text();
                            sl.CustomerCode = customer.CustomerCode;
                            sl.CustomerName = customer.BusinessName;
                            sl.CreateAt = DateTime.UtcNow;
                            sl.CreateBy = cMem.FullName;
                            sl.CreateByMemberNumber = cMem.MemberNumber;
                            sl.L_SalonName = customer.BusinessName;
                            sl.L_Address = customer.BusinessAddress;
                            sl.L_State = customer.BusinessState;
                            sl.L_City = customer.BusinessCity;
                            sl.L_Zipcode = customer.Zipcode;
                            sl.L_Country = customer.Country;
                            sl.L_Phone = customer.BusinessPhone;
                            sl.L_Type = LeadType.RegisterOnIMS.Text();
                            db.C_SalesLead.Add(sl);
                            await db.SaveChangesAsync();
                            _merchantService.SaveHistoryUpdate(customer.CustomerCode, "Created new merchant");

                            updCus = customer;

                            #endregion

                            TempData["s"] = "Create new merchant info completed!";


                        }
                        //save additional info
                        long DTid = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff"));
                        for (int i = 1; Request["info_name_" + i] != null; i++)
                        {
                            long info_id = long.Parse(Request["info_id_" + i] ?? "0");
                            string info_name = Request["info_name_" + i];
                            string info_content = Request["info_content_" + i];
                            if (info_id > 0)
                            {
                                var edit = db.C_CustomerAdditionalInfo.Find(info_id);
                                if (!string.IsNullOrEmpty(info_name))
                                {
                                    edit.InfoName = info_name;
                                    edit.InfoContent = info_content;
                                    db.Entry(edit).State = System.Data.Entity.EntityState.Modified;
                                }
                                else
                                {
                                    db.C_CustomerAdditionalInfo.Remove(edit);
                                }
                            }
                            else
                            {

                                if (!string.IsNullOrEmpty(info_name))
                                {
                                    C_CustomerAdditionalInfo newinfo = new C_CustomerAdditionalInfo
                                    {
                                        Id = DTid++,
                                        CustomerID = customer_id,
                                        InfoName = info_name,
                                        InfoContent = info_content,
                                        CreateBy = cMem.FullName?.ToUpper(),
                                        CreateAt = DateTime.UtcNow,
                                    };
                                    db.C_CustomerAdditionalInfo.Add(newinfo);
                                }
                            }

                        }
                        _logService.Info($"[MerchantMan][Save] completed");
                        await db.SaveChangesAsync();
                        // Trans.Commit();
                        // Trans.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logService.Error(ex, $"[MerchantMan][Save] error");
                        return Json(new object[] { false, "Error: " + ex.Message });
                    }
                    return Json(new object[] { true, updCus, mess });
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][Save] error");
                throw;
                return Json(new object[] { false, ex.Message });
            }

        }

        public JsonResult SaveSalonNote(C_Customer customer)
        {
            _logService.Info($"[MerchantMan][SaveSalonNote] start", new { customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer) });
            try
            {
                var cus = db.C_Customer.Find(customer.Id);
                if (cus == null) throw new Exception("Merchant not found");
                cus.BusinessDescription = customer.BusinessDescription;
                db.Entry(cus).State = EntityState.Modified;
                db.SaveChanges();
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Updated salon note");
                new MerchantService().WriteLogMerchant(cus, "Updated salon note", "Salon note has been updated: <b>" + customer.BusinessDescription + "</b>.");
                _logService.Info($"[MerchantMan][SaveSalonNote] completed");
                return Json(new object[] { true, "Update success" });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][SaveSalonNote] error");
                return Json(new object[] { false, "Error: " + e.Message });
            }
        }

        public JsonResult ChangeStoreType(string storeId, string type)
        {
            _logService.Info($"[MerchantMan][ChangeStoreType] start storeId:{storeId} - type:{type} ");
            try
            {
                var cus = db.C_Customer.FirstOrDefault(c => c.StoreCode.Equals(storeId));
                if (cus == null)
                    throw new Exception("Store not found");

                cus.Type = type;
                db.SaveChanges();
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Updated merchant type to " + type);
                new MerchantService().WriteLogMerchant(cus, "Updated merchant type", "Merchant type has been updated: <b>" + type + "</b>.");
                _logService.Info($"[MerchantMan][ChangeStoreType] completed");
                return Json(new object[] { true, "Update success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][ChangeStoreType] error storeId:{storeId} - type:{type} ");
                return Json(new object[] { false, "Update fail, " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteMerchant(long Id)
        {
            _logService.Info($"[MerchantMan][DeleteMerchant] start Id:{Id}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_delete")) == false || access["customer_delete"] != true)
                {
                    throw new Exception("You don't have permission delete contact!");
                }
                WebDataModel db = new WebDataModel();
                var del = db.C_Customer.Find(Id);
                db.C_Customer.Remove(del);
                db.SaveChanges();
                _merchantService.SaveHistoryUpdate(del.CustomerCode, "Merchant deleted");
                _logService.Info($"[MerchantMan][DeleteMerchant] completed");
                TempData["s"] = "Merchant deleted!";
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][DeleteMerchant] error Id:{Id}");
                TempData["e"] = "Error: " + e.Message;
            }
            return RedirectToAction("index");
        }
        #endregion

        #region tab partial
        public async Task<ActionResult> ChangeTab(string tabname, long id, string tkstatus)
        {
            _logService.Info($"[MerchantMan][ChangeTab] start tabname:{tabname} - customerid:{id} - tkstatus:{tkstatus}");
            try
            {
                var customer = db.C_Customer.Find(id);
                string customercode = customer.CustomerCode;
                ViewBag.Customer = customer;
                ViewBag.CustomerID = id;
                ViewBag.p = access;
                if (tabname == "ticket")
                {
                    TempData["t"] = tkstatus;
                    ViewBag.CustomerCode = customercode;
                    var sl = db.C_SalesLead.Where(x => x.CustomerCode == customer.CustomerCode).FirstOrDefault();
                    string salesleadId = sl?.Id ?? string.Empty;
                    ViewBag.SalesLeadId = salesleadId;
                    IEnumerable<Calendar_Event> appointments = db.Calendar_Event.Where(a => a.SalesLeadId == salesleadId).OrderByDescending(a => a.StartEvent).ToList();
                    ViewBag.ListLog = new DetailSalesLeadCustomizeModel
                    {
                        even = appointments,
                        lead = getLead(salesleadId)
                    };
                    return PartialView("_PartialTicketTab");
                }
                else if (tabname == "files")
                {
                    ViewBag.MerchantID = id;
                    ViewBag.FileList = db.UploadMoreFiles.Where(u => u.TableName == "C_Customer" && u.TableId == id).ToList();
                 
                    var resultProc =  db.Database.SqlQuery<P_ChangeTab_files>($"exec P_ChangeTab_files '{customercode}'").FirstOrDefault();
                    List<FilesAttachmentModel> data = new List<FilesAttachmentModel>();
                    try
                    {
                        if (resultProc.TK != null)
                        {
                            List<T_SupportTicket> tk = JsonConvert.DeserializeObject<List<T_SupportTicket>>(resultProc.TK); 
                            foreach(var item in tk)
                            {
                                try{
                                    FilesAttachmentModel itemData = new FilesAttachmentModel();
                                    itemData.Ticket = item;
                                    List<UploadMoreFile> ups = new List<UploadMoreFile>();
                                    if (resultProc.UP != null)
                                    {
                                        ups = JsonConvert.DeserializeObject<List<UploadMoreFile>>(resultProc.UP);
                                        itemData.TicketUploadFiles = ups.Where(x => x.TableId == item.Id).ToList();
                                    }
                                    if (resultProc.FB != null)
                                    {
                                        List<T_TicketFeedback> FB = JsonConvert.DeserializeObject<List<T_TicketFeedback>>(resultProc.FB);
                                        itemData.Feedback = FB.Where(x => x.TicketId == item.Id).ToList();
                                        itemData.FilesFeedback = ups.Where(x => itemData.Feedback.Any(y => y.Id == x.TableId)).ToList();
                                    }
                                    if (resultProc.filesRelated != null)
                                    {
                                        List<T_FileRelated> filesRelated = JsonConvert.DeserializeObject<List<T_FileRelated>>(resultProc.filesRelated);
                                        itemData.T_FilesRelated = filesRelated.Where(x => x.TicketId == item.Id).ToList();
                                    }
                                    if (resultProc.FR != null)
                                    {
                                        List<UploadMoreFile> FR = JsonConvert.DeserializeObject<List<UploadMoreFile>>(resultProc.FR);
                                        itemData.FilesRelatedUploads = FR.Where(x => x.TableId == item.Id).ToList();
                                    }
                                    data.Add(itemData);
                                } catch { }
                            }
                        }
                    } catch { }

                    return PartialView("_PartialFilesTab", data);

                }
                else if (tabname == "products")
                {
                    var order_dev = new List<Devices_in_Order>();
                    var order_list = db.O_Orders.Where(o => o.CustomerCode == customercode && o.InvoiceNumber > 0 && o.IsDelete != true).OrderByDescending(o => o.CreatedAt).ToList();
                    foreach (var order in order_list)
                    {
                        var devices = db.Order_Products.Where(o => o.OrderCode == order.OrdersCode).ToList();
                        order_dev.Add(new Devices_in_Order { Order = order, Devices = devices });
                    }
                    return PartialView("_PartialProducts", order_dev);
                }
                else if (tabname == "merchant")
                {
                    ViewBag.FileList = db.UploadMoreFiles.Where(u => u.TableName == "C_Customer" && u.TableId == id).ToList();
                    ViewBag.ReplyTo = WebConfigurationManager.AppSettings["ReplyEmail"];
                    ViewBag.ListOrder = db.O_Orders.Where(c => c.CustomerCode == customercode).ToList();
                    var merchant_form = new MerchantFormView { Id = customer.Id, WordDetermine = customer.WordDetermine, Code = customer.CustomerCode, Name = (string.IsNullOrEmpty(customer.BusinessName) ? customer.OwnerName : customer.BusinessName), List_pdf = db.O_MerchantForm.Where(m => m.MerchantCode == customer.CustomerCode).ToList() };
                    return PartialView("_PartialMerchantForm", merchant_form);
                }
                else if (tabname == "services")
                {

                    var statusRemove = LicenseStatus.REMOVED.Code<int>();
                    var statusCancel = InvoiceStatus.Canceled.ToString();
                    var services = (from ss in db.Store_Services
                                    where ss.CustomerCode == customercode && ss.Active != statusRemove
                                    join p in db.License_Product
                                    on ss.ProductCode equals p.Code

                                    join os in db.Order_Subcription.Where(c => c.CustomerCode == customercode && c.SubscriptionType != "setupfee" && c.SubscriptionType != "interactionfee")
                                    on new { c1 = ss.ProductCode, c2 = ss.OrderCode } equals new { c1 = os.Product_Code, c2 = os.OrderCode } into licens
                                    from os in licens.DefaultIfEmpty()

                                    join o in db.O_Orders
                                    on ss.OrderCode equals o.OrdersCode into order
                                    from o in order.DefaultIfEmpty()

                                    where os != null
                                    select new Store_Services_Product_view { Store = ss, Product = p, Order = o, Subscription = os }).ToList();

                    ViewBag.customerCode = customer.CustomerCode;
                    ViewBag.storeid = customer.StoreCode;
                    ViewBag.typeCus = customer.Type;
                    ViewBag.control_allow = (access.Any(k => k.Key.Equals("customer_mango_control")) == true && access["customer_mango_control"] == true);
                    ViewBag.PosVersion2 = !string.IsNullOrEmpty(customer.Version) ? customer.Version.Equals("v2") : services.Any(s => s.Store.Type == "license" && s.Store.Active == 1 && string.IsNullOrEmpty(s.Product.Code_POSSystem));
                    ViewBag.IsTrial = customer.WordDetermine?.Equals("Trial");
                    return PartialView("_PartialServices", services);
                }
                else if (tabname == "history")
                {
                    var Invoices = db.O_Orders.Where(o => o.CustomerCode == customercode && o.InvoiceNumber > 0 && o.IsDelete != true).OrderByDescending(o => o.OrdersCode).ToList();
                    ViewBag.Subscr_his = (from sub in db.Store_Services
                                          where sub.CustomerCode == customer.CustomerCode
                                          group sub by sub.ProductCode into gr_sub
                                          join p in db.License_Product on gr_sub.Key equals p.Code
                                          select new Subscription_History_view
                                          {
                                              Product = p,
                                              Subscription = gr_sub.OrderByDescending(s => s.EffectiveDate).ToList(),
                                          }).OrderBy(p => p.Product.isAddon == true).ThenBy(p => p.Product.Name).ToList();

                    return PartialView("_PartialInvoices", Invoices);
                }
                else if (tabname == "transaction")
                {
                    var datenow = DateTime.UtcNow.Date;
                    var lstCustomerCredit = db.C_CustomerCard.AsEnumerable().Where(card => card.CustomerCode == customer.CustomerCode);
                    ViewBag.credit = lstCustomerCredit.Where(card => card.Active == true).ToList();
                    ViewBag.RecurringCards = db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == "Active" && s.CustomerCode == customercode).Select(c => c.MxMerchant_cardAccountId).ToList();
                    ViewBag.deactive_credit = lstCustomerCredit.Where(card => card.Active != true).ToList();
                
                    ViewBag.CompanyInfo = db.SystemConfigurations.FirstOrDefault();
                    ViewBag.country = db.Ad_Country.ToList();
                    ViewBag.states = db.Ad_USAState.ToList();
                    ViewBag.cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == customer.CustomerCode);
                    ViewBag.access = access;
                    return PartialView("_PartialPaymentTransaction", customer);
                }
                else if (tabname == "recurring")
                {
                    return PartialView("_PartialRecurringPlan");
                }
                else if (tabname == "smshistory")
                {
                    var request = new HistoryTimeLineRequest();
                    request.Condition.StoreCodes = new List<string> { customer.StoreCode };
                    var timeline = new HistoryTimeLineResponse();
                    try
                    {
                        timeline = await _enrichSMSService.GetTimeLineAsync(request);
                    }
                    catch (Exception)
                    {
                        timeline.Records = new List<HistoryListItemDto>();
                    }
                    return PartialView("_PartialSMSHistory", timeline.Records);
                }
                else
                {
                    if (customer != null)
                    {
                        var us_state = db.Ad_USAState.Find(customer.BusinessState);
                        if (us_state != null)
                        {
                            customer.BusinessState = us_state.name;
                        }
                    }
                    ViewBag.AdditionalInfo = db.C_CustomerAdditionalInfo.Where(t => t.CustomerID == id).ToList();
                    ViewBag.AddContact = db.C_CustomerContact.Where(c => c.CustomerId == id).ToList();
                    //ViewBag.accountManagers = db.C_CustomerAccountManager.Where(c => c.CustomerCode == customer.CustomerCode).ToList(); 
                    ViewBag.Hardwares = db.Order_Products.Where(c => db.O_Orders.Any(o => o.CustomerCode == customer.CustomerCode && o.OrdersCode == c.OrderCode && (o.Status == "Paid_Wait" || o.Status == "Closed"))).ToList();
                    ViewBag.SupportingInfo = db.C_CustomerSupportingInfo.FirstOrDefault(c => c.CustomerId == customer.Id) ?? new C_CustomerSupportingInfo { };
                    var sl = db.C_SalesLead.Where(x => x.CustomerCode == customer.CustomerCode).FirstOrDefault();
                    string salesleadId = sl?.Id ?? string.Empty;
                    ViewBag.SalesLeadId = salesleadId;
                    IEnumerable<Calendar_Event> appointments = db.Calendar_Event.Where(a => a.SalesLeadId == salesleadId).OrderByDescending(a => a.StartEvent).ToList();
                    ViewBag.ListLog = new DetailSalesLeadCustomizeModel
                    {
                        even = appointments,
                        lead = getLead(salesleadId)
                    };
                    return PartialView("_PartialDetailsTab", customer);
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][ChangeTab] error tabname:{tabname} - customerid:{id} - tkstatus:{tkstatus}");
                throw;
            }

        }

        [HttpPost]
        public ActionResult GetTransactionByStoreCode(string CustomerCode, string FilterCard, string SearchText, IDataTablesRequest dataTablesRequest)
        {
            var query = (
                      from t in db.C_CustomerTransaction
                      where t.CustomerCode == CustomerCode
                      join o in db.O_Orders on t.OrdersCode equals o.OrdersCode
                      select new Transaction_view
                      {
                          Id = t.Id,
                          Card_id = t.Card,
                          createAt = t.CreateAt,
                          order = t.OrdersCode,
                          type = t.PaymentMethod ?? o.PaymentMethod,
                          paymentNote = t.PaymentNote ?? o.PaymentNote,
                          amount = t.Amount,
                          status = t.PaymentStatus,
                          bankName = t.BankName,
                          cardNumber = t.CardNumber,
                          createBy = t.CreateBy,
                          responeText = t.ResponseText,
                          updateNote = t.UpdateDescription
                      }
                  );
            if (!string.IsNullOrEmpty(FilterCard))
            {
                query = query.Where(x => x.Card_id == FilterCard);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => !string.IsNullOrEmpty(x.responeText) && x.responeText.Contains(SearchText)
                || (!string.IsNullOrEmpty(x.cardNumber) && x.cardNumber.Contains(SearchText))
                || (!string.IsNullOrEmpty(x.bankName) && x.bankName.Contains(SearchText))
                || (!string.IsNullOrEmpty(x.updateNote) && x.updateNote.Contains(SearchText))
                || (!string.IsNullOrEmpty(x.order) && x.order.Contains(SearchText))
                || (SqlFunctions.StringConvert((decimal?)x.amount).Contains(SearchText))
                );
            }
            var filtered_count = query.Count();

            #endregion
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch?.Name)
            {
                case "Date":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.createAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.createAt);
                    }
                    break;
                case "BankName":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.bankName);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.bankName);
                    }
                    break;
                case "OrderCode":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.order);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.order);
                    }
                    break;
                case "Type":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.type);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.type);
                    }
                    break;

                case "CardNumber":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.cardNumber);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.cardNumber);
                    }
                    break;
                case "Amount":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.amount);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.amount);
                    }
                    break;
                case "Status":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.status);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.status);
                    }
                    break;
                case "ResponeText":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.responeText);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.responeText);
                    }
                    break;
                default:
                    query = query.OrderByDescending(x => x.createAt);
                    break;
            };
            var data = query.Skip(dataTablesRequest.Start).
                            Take(dataTablesRequest.Length).ToList();
            data.ForEach(c => c.CreateAtStr = c.createAt.Value.UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt"));

            return Json(new
            {
                draw = dataTablesRequest.Draw,
                recordsFiltered = filtered_count,
                recordsTotal = filtered_count,
                data = data,
            });
        }

        public async Task<ActionResult> ChangeSubTab(string tabname, long id)
        {
            _logService.Info($"[MerchantMan][ChangeSubTab] start tabname:{tabname} - customerid:{id}");
            try
            {


                WebDataModel db = new WebDataModel();
                var customer = db.C_Customer.Find(id);
                var CustomerCode = customer.CustomerCode;
                ViewBag.p = access;
                ViewBag.EnumValues = db.EnumValues.ToList();
                if (tabname == "TimeLine")
                {
                    var sl = db.C_SalesLead.Where(x => x.CustomerCode == customer.CustomerCode).FirstOrDefault();
                    string salesleadId = sl?.Id ?? string.Empty;
                    ViewBag.SalesLeadId = salesleadId;
                    IEnumerable<Calendar_Event> appointments = db.Calendar_Event.Where(a => a.SalesLeadId == salesleadId).OrderByDescending(a => a.StartEvent).ToList();
                    ViewBag.ListLog = new DetailSalesLeadCustomizeModel
                    {
                        even = appointments,
                        lead = getLead(salesleadId)
                    };
                    return PartialView("_SubPartial_Conversation");
                }
                else if (tabname == "SupportingInfo")
                {
                    var SupportingInfo = db.C_CustomerSupportingInfo.FirstOrDefault(c => c.CustomerId == customer.Id) ?? new C_CustomerSupportingInfo { };
                    ViewBag.Hardwares = db.Order_Products.Where(c => db.O_Orders.Any(o => o.CustomerCode == customer.CustomerCode && o.OrdersCode == c.OrderCode && (o.Status == "Paid_Wait" || o.Status == "Closed"))).ToList();
                    ViewBag.Customer = customer;
                    ViewBag.Services = db.Store_Services.Where(c => c.CustomerCode == customer.CustomerCode && c.Active == 1 || (c.Type == "other" && c.CustomerCode == customer.CustomerCode && c.Active == -1)).OrderByDescending(c => c.Type == "license").ToList();
                    ViewBag.Contacts = db.C_CustomerContact.Where(c => c.CustomerId == id).ToList();
                    return PartialView("_SubPartial_SupportingInfo", SupportingInfo);
                }
                else if (tabname == "TicketInfo")
                {
                    var ticketlist = db.T_SupportTicket.Where(t => t.CustomerCode == CustomerCode &&
                                            t.Visible == true //&&
                                                              //(string.IsNullOrEmpty(tkstatus) ||
                                                              //(tkstatus == "closed" && t.Visible == true && t.DateClosed.HasValue) ||
                                                              //(tkstatus == "opened" && t.Visible == true && !t.DateClosed.HasValue) ||
                                                              //(tkstatus == "all")
                                                              //)
                        ).OrderBy(t => t.CreateAt).ToList();
                    ViewBag.CustomerID = customer.Id;
                    return PartialView("_SubPartial_Ticket", ticketlist);
                }
                else if (tabname == "MerchantProcessing")
                {
                    if (access.Any(k => k.Key.Equals("customer_update_merchantprocessing")) == true && access["customer_update_merchantprocessing"] == true)
                    {
                        ViewBag.access = true;
                    }
                    ViewBag.CustomerCode = CustomerCode;
                    ViewBag.MerchantID = customer.Id;
                    var merchantprocess = db.C_MerchantSubscribe.Where(m => m.CustomerCode == customer.CustomerCode && !string.IsNullOrEmpty(m.MerchantID)).OrderBy(m => m.Status).ToList().Select(x => new MerchantProcessingCustomizeModel
                    {
                        Id = x.Id,
                        MerchantID = x.MerchantID,
                        ProcessorName = x.DbaName,
                        CardTypeAccept = x.CardTypeAccept?.Split(','),
                        StartDate = x.StartDate,
                        EndDate = x.EndDate,
                        Status = x.Status,
                        ListMerchantProcess = db.C_MerchantProcessing.Where(y => y.MerchantSubscribeId == x.Id)
                    });
                    return PartialView("_PartialMerchantProcessing", merchantprocess);
                }
                else if (tabname == "CreditCardStore")
                {
                    //tam thoi chua dung den
                    //da an tab nay cshtml
                    ViewBag.CustomerCode = id;
                    var Creditcards = db.C_CustomerCard.Where(c => c.CustomerCode == CustomerCode).ToList();
                    return PartialView("_PartialCreditCardStore", Creditcards);
                }
                else if (tabname == "MangoPOS")
                {
                    return PartialView("_PartialMangoPOS", db.C_Customer.Find(id)?.StoreCode ?? "");
                }
                else if (tabname == "StoreBoss")
                {
                    var boss = db.C_BossStore.Where(b => (b.StoreCodes ?? string.Empty).Contains(customer.StoreCode)).FirstOrDefault();
                    if (boss != null)
                    {
                        var list_customer = db.C_Customer.Where(c => boss.StoreCodes.Contains(c.StoreCode)).ToList();
                        var viewModel = (from c in list_customer.OrderByDescending(x => x.CreateAt)
                                         join s in (from sub in db.Store_Services where sub.Active == 1 && sub.Type == "license" select sub) on c.CustomerCode equals s.CustomerCode into gj
                                         from s in gj.DefaultIfEmpty()
                                         select new Merchant_IndexView { Customer = c, Remaning = (s?.RenewDate != null) ? (int?)(s?.RenewDate.Value.Date - DateTime.UtcNow.Date).Value.Days : null, RenewDate = s?.RenewDate }).ToList();

                        ViewBag.otherStores = viewModel;
                        ViewBag.Boss = boss;
                    }
                    var list_Boss = db.C_BossStore.Where(c => c.IsActive == true).ToList();
                    ViewBag.ListBoss = list_Boss;
                    return PartialView("_PartialStoreBoss", customer);
                }
                else if (tabname == "Overview")
                {
                    ViewBag.CustomerID = customer.Id;
                    ViewBag.SupportingInfo = db.C_CustomerSupportingInfo.FirstOrDefault(c => c.CustomerId == customer.Id) ?? new C_CustomerSupportingInfo { };
                    ViewBag.Hardwares = db.Order_Products.Where(c => db.O_Orders.Any(o => o.CustomerCode == customer.CustomerCode && o.OrdersCode == c.OrderCode && (o.Status == "Paid_Wait" || o.Status == "Closed"))).ToList();
                    ViewBag.Services = db.Store_Services.Where(c => c.CustomerCode == customer.CustomerCode && c.Active == 1 || (c.Type == "other" && c.CustomerCode == customer.CustomerCode && c.Active == -1)).OrderByDescending(c => c.Type == "license").ToList();
                    ViewBag.Customer = customer;
                    ViewBag.AddContact = db.C_CustomerContact.Where(c => c.CustomerId == id).ToList();
                    var sl = db.C_SalesLead.Where(x => x.CustomerCode == customer.CustomerCode).FirstOrDefault();
                    string salesleadId = sl?.Id ?? string.Empty;
                    ViewBag.SalesLeadId = salesleadId;
                    string salesPersonNumber = sl.MemberNumber;
                    string salesPersonName = "N/A";
                    if (!string.IsNullOrEmpty(salesPersonNumber))
                    {
                        var salePersonMember = db.P_Member.FirstOrDefault(x => x.MemberNumber == sl.MemberNumber);
                        salesPersonName = salePersonMember?.FullName ?? string.Empty;
                    }
                    ViewBag.SalesPersonNumber = salesPersonNumber;
                    ViewBag.SalesPersonName = salesPersonName;
                    IEnumerable<Calendar_Event> appointments = db.Calendar_Event.Where(a => a.SalesLeadId == salesleadId).OrderByDescending(a => a.StartEvent).ToList();
                    ViewBag.ListLog = new DetailSalesLeadCustomizeModel
                    {
                        even = appointments,
                        lead = getLead(salesleadId)
                    };
                    ViewBag.ticketlist = db.T_SupportTicket.Where(t => t.CustomerCode == CustomerCode && t.Visible == true).OrderBy(t => t.CreateAt).ToList();
                    string clickUpId = await _clickUpConnectorService.GetMappingByIMSIdAsync(customer.Id.ToString());
                    ViewBag.ClickUpId = clickUpId;

					return PartialView("_SubPartial_Overview");
                }
                else
                {
                    ViewBag.AdditionalInfo = db.C_CustomerAdditionalInfo.Where(t => t.CustomerID == id).ToList();
                    ViewBag.AddContact = db.C_CustomerContact.Where(c => c.CustomerId == id).ToList();
                    return PartialView("_PartialOwnerInfo", customer);
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][ChangeSubTab] error tabname:{tabname} - customerid:{id}");
                throw;
            }

        }
        public ActionResult recurring_tab(string CusCode)
        {
            _logService.Info($"[MerchantMan][recurring_tab] start CusCode:{CusCode}");
            try
            {
                ViewBag.control_allow = (access.Any(k => k.Key.Equals("customer_mango_control")) == true && access["customer_mango_control"] == true);
                var recurrings = db.Store_Services.Where(s => s.CustomerCode == CusCode && s.Active == 1 && s.MxMerchant_contractId > 0).ToList();
                return PartialView("_recurring_tab", recurrings);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][recurring_tab] error CusCode:{CusCode}");
                throw;
            }

        }
        public JsonResult CreateMerchantProcess(MerchantProcessingCustomizeModel Sub)
        {
            _logService.Info($"[MerchantMan][CreateMerchantProcess] start", new { sub = Newtonsoft.Json.JsonConvert.SerializeObject(Sub) });
            try
            {
                if (access.Any(k => k.Key.Equals("customer_update_merchantprocessing")) != true || access["customer_update_merchantprocessing"] != true)
                    throw new Exception("You dont have permission!");

                var oldProc = db.C_MerchantSubscribe.FirstOrDefault(c => c.MerchantID.Equals(Sub.MerchantID));
                if (oldProc != null)
                    throw new Exception("Merchant ID is exist!");

                var newSub = new C_MerchantSubscribe()
                {
                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssffff")),
                    DbaName = Sub.ProcessorName,
                    Status = Sub.Status ?? "inactive",
                    StartDate = Sub.StartDate,
                    EndDate = Sub.EndDate,
                    CardTypeAccept = Sub.CardTypeAccept != null ? string.Join(",", Sub.CardTypeAccept) : null,
                    CustomerCode = Sub.CustomerCode,
                    MerchantID = Sub.MerchantID
                };
                db.C_MerchantSubscribe.Add(newSub);
                var entityListMerchentProcess = Sub.ListMerchantProcess.Select(x => new C_MerchantProcessing
                {
                    Auth = x.Auth,
                    RegisterID = x.RegisterID,
                    Note = x.Note,
                    TPN = x.TPN,
                    MerchantSubscribeId = newSub.Id
                });
                db.C_MerchantProcessing.AddRange(entityListMerchentProcess);
                db.SaveChanges();
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == Sub.CustomerCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Created merchant processing " + Sub.ProcessorName);
                new MerchantService().WriteLogMerchant(cus, "Created merchant processing", "Merchant processing <b>" + Sub.ProcessorName + "</b> has been created.");
                string WordDetermine = UpdateWordDetermine(Sub.CustomerCode);
                _logService.Info($"[MerchantMan][CreateMerchantProcess] completed");
                return Json(new object[] { true, "Create success", WordDetermine });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][CreateMerchantProcess] error");
                return Json(new object[] { false, "Create fail. " + e.Message });
            }
        }
        public JsonResult SaveMerchantProcessing(MerchantProcessingCustomizeModel Sub)
        {
            _logService.Info($"[MerchantMan][SaveMerchantProcessing] start", new { sub = Newtonsoft.Json.JsonConvert.SerializeObject(Sub) });
            try
            {
                if (access.Any(k => k.Key.Equals("customer_update_merchantprocessing")) != true || access["customer_update_merchantprocessing"] != true)
                    throw new Exception("You dont have permission!");

                var merchantSub = db.C_MerchantSubscribe.FirstOrDefault(m => m.Id.Equals(Sub.Id));
                if (merchantSub == null)
                    throw new Exception("Merchant processing not found");

                var oldProc = db.C_MerchantSubscribe.FirstOrDefault(c => c.MerchantID == Sub.MerchantID && Sub.MerchantID != merchantSub.MerchantID);
                if (oldProc != null)
                    throw new Exception("Merchant ID is exist!");
                using (var scope = new TransactionScope())
                {
                    try
                    {
                        merchantSub.MerchantID = Sub.MerchantID;
                        merchantSub.DbaName = Sub.ProcessorName;
                        merchantSub.Status = Sub.Status;
                        merchantSub.StartDate = Sub.StartDate;
                        merchantSub.EndDate = Sub.EndDate;
                        merchantSub.CardTypeAccept = Sub.CardTypeAccept != null ? string.Join(",", Sub.CardTypeAccept) : null;
                        db.Entry(merchantSub).State = EntityState.Modified;
                        var ListMerchantProcess = db.C_MerchantProcessing.Where(x => x.MerchantSubscribeId == Sub.Id);
                        db.C_MerchantProcessing.RemoveRange(ListMerchantProcess);
                        var entityListMerchentProcess = Sub.ListMerchantProcess.Select(x => new C_MerchantProcessing
                        {
                            Auth = x.Auth,
                            RegisterID = x.RegisterID,
                            Note = x.Note,
                            TPN = x.TPN,
                            MerchantSubscribeId = merchantSub.Id
                        });
                        db.C_MerchantProcessing.AddRange(entityListMerchentProcess);
                        db.SaveChanges();
                        scope.Complete();
                    }
                    catch (Exception e)
                    {
                        scope.Dispose();
                        return Json(new object[] { false, "Update fail. " + e.Message });
                    }
                }

                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == merchantSub.CustomerCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Updated merchant processing " + Sub.ProcessorName);
                new MerchantService().WriteLogMerchant(cus, "Updated merchant processing", "Merchant processing <b>" + Sub.ProcessorName + "</b> has been updated.");
                string Wordetermine = UpdateWordDetermine(merchantSub.CustomerCode);
                _logService.Info($"[MerchantMan][SaveMerchantProcessing] completed", new { Wordetermine = Newtonsoft.Json.JsonConvert.SerializeObject(Wordetermine) });
                return Json(new object[] { true, "Update success", Wordetermine });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][SaveMerchantProcessing] error");
                return Json(new object[] { false, "Update fail. " + e.Message });
            }
        }
        public JsonResult RemoveMerchantProcess(string key)
        {
            _logService.Info($"[MerchantMan][RemoveMerchantProcess] start key:{key}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_update_merchantprocessing")) != true || access["customer_update_merchantprocessing"] != true)
                {
                    throw new Exception("You dont have permission!");
                }

                long id = long.Parse(key);
                var merchantSub = db.C_MerchantSubscribe.FirstOrDefault(m => m.Id.Equals(id));
                if (merchantSub == null)
                    throw new Exception("Merchant processing not found");
                db.C_MerchantSubscribe.Remove(merchantSub);
                var listMerchantProcessing = db.C_MerchantProcessing.Where(x => x.MerchantSubscribeId == id);
                if (listMerchantProcessing.Count() > 0)
                {
                    db.C_MerchantProcessing.RemoveRange(listMerchantProcessing);
                }
                db.SaveChanges();
                string WordDetermine = UpdateWordDetermine(merchantSub.CustomerCode);
                //update WordDetermine 
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == merchantSub.CustomerCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Removed merchant processing " + merchantSub.DbaName);
                new MerchantService().WriteLogMerchant(cus, "Removed merchant processing", "Merchant processing <b>" + merchantSub.DbaName + "</b> has been removed.");
                _logService.Info($"[MerchantMan][RemoveMerchantProcess] completed key:{key}");
                return Json(new object[] { true, "Remove success", WordDetermine }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][RemoveMerchantProcess] error key:{key}");
                return Json(new object[] { false, "Remove fail. " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ReLoadFile(long id)
        {
            _logService.Info($"[MerchantMan][ReLoadFile] start id:{id}");
            try
            {
                WebDataModel db = new WebDataModel();
                var list_upfile = db.UploadMoreFiles.Where(u => u.TableName == "C_Customer" && u.TableId == id).ToList();
                ViewBag.TableId = id;
                ViewBag.TableName = "C_Customer";
                return PartialView("_UploadFilesPartial", list_upfile);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][ReLoadFile] error id:{id}");
                throw;
            }

        }
        public JsonResult GetAddInfo(long id)
        {
            _logService.Info($"[MerchantMan][GetAddInfo] start id:{id}");
            try
            {
                WebDataModel db = new WebDataModel();
                var list_info = (from info in db.C_CustomerAdditionalInfo
                                 where info.CustomerID == id
                                 select new
                                 {
                                     Id = info.Id,
                                     CustomerID = info.CustomerID,
                                     InfoName = info.InfoName,
                                     InfoContent = info.InfoContent,
                                     CreateBy = info.CreateBy,
                                     CreateAt = info.CreateAt.ToString()
                                 }).ToList();
                _logService.Info($"[MerchantMan][GetAddInfo] completed");
                return Json(list_info);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][GetAddInfo] error id:{id}");
                throw;
            }

        }

        //public JsonResult SetAutoRecurring(string CusCode, bool AutoRecurring)
        //{
        //    var cus = db.C_Customer.Where(c => c.CustomerCode == CusCode).FirstOrDefault();
        //    if (cus == null)
        //    {
        //        return Json(new object[] { false, "Merchant not found!" });
        //    }
        //    else
        //    {
        //        cus.ActiveRecuring = AutoRecurring;
        //        db.SaveChanges();
        //        var Recurrings = db.Store_Services.Where(s => s.CustomerCode == cus.CustomerCode && (s.Active == -1 || s.Active == 1) && s.MxMerchant_SubscriptionStatus == "active");
        //        var paysv = new PaymentService();
        //        Recurrings.ForEach(s =>
        //        {
        //            paysv.SetStatusRecurring(s, "inactive");
        //        });

        //        return Json(new object[] { true, AutoRecurring ? "Active auto recurring completed!" : "Inactive auto recurring completed" });
        //    }
        //}

        /// <summary>
        /// xac dinh license boi store
        /// </summary>
        /// <param name="db"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        private List<License_Product_Item> ProductLicenseItemsByStore(WebDataModel db, Store_Services store)
        {
            _logService.Info($"[MerchantMan][ProductLicenseItemsByStore] start", new { store = Newtonsoft.Json.JsonConvert.SerializeObject(store) });
            if (db == null)
            {
                db = new WebDataModel();
            }
            try
            {
                List<License_Product_Item> productItems = (
                       from order in db.O_Orders.AsEnumerable()
                       where order.OrdersCode == store.OrderCode
                       join order_s in db.Order_Subcription on order.OrdersCode equals order_s.OrderCode
                       join product_item in db.License_Product_Item on order_s.ProductId equals product_item.License_Product_Id
                       where product_item.Enable == true
                       group product_item by product_item.License_Item_Code into rs
                       select new License_Product_Item
                       {
                           License_Item_Code = rs.Key,
                           Value = rs.Sum(item => item.Value)
                       }
                   ).ToList();
                _logService.Info($"[MerchantMan][ProductLicenseItemsByStore] completed total-productItems{productItems.Count}");
                return productItems;
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][ProductLicenseItemsByStore] error");
                throw new Exception("Not determine is identified in this plan.." + e.InnerException == null ? e.Message : e.InnerException.Message);
            }

        }

        /// <summary>
        /// Get store profile
        /// </summary>
        /// <param name="storeCode"></param>
        /// <param name="licenseId"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public ActionResult MangoPOSInfo(string storeCode, string licenseId, string stage = null)
        {
            _logService.Info($"[MerchantMan][MangoPOSInfo] start storeCode:{storeCode} - licenseId:{licenseId} - stage:{stage}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_view_mangopos")) != true || access["customer_view_mangopos"] != true)
                {
                    throw new Exception("You dont have permission to change!");
                }
                string _storeCode = storeCode;
                StoreProfile profile;
                Store_Services main;
                if (string.IsNullOrEmpty(_storeCode))
                {
                    Store_Services license = db.Store_Services.Find(licenseId);
                    ViewBag.actionActive = true;
                    ViewBag.licenseId = licenseId;
                    ViewBag.SMSWarning = false;
                    if (license.Type == LicenseType.ADD_ON.Text())
                    {
                        List<License_Product_Item> productItems = ProductLicenseItemsByStore(db, license);
                        Dictionary<string, int> addons = new Dictionary<string, int>();
                        new StoreServices(db).LicensesAddOn(license.StoreCode, licenseId).ForEach(item => { addons.Add(item.LicenseCode, int.Parse(item.Count_limit ?? "0")); });
                        ViewBag.addons = addons;
                    }

                    ViewBag.reactive = license.Active == 0;
                    if (stage == "active" && license.Type == LicenseType.LICENSE.Text())
                    {
                        main = license;
                        profile = new MerchantService().ProfileForNewPlan(licenseId);
                    }
                    else
                    {
                        main = new MerchantService().GetMainSubscription(license.StoreCode, true);
                        profile = new MerchantService().StoreProfileWithDefault(license.StoreCode);
                    }
                    if (license.Type == LicenseType.ADD_ON.Text())
                    {
                        //var smsAddonActivated = new StoreServices(db).LicensesPlan(main.StoreCode, null).FirstOrDefault(c => c.LicenseCode == "SMS")?.Count_limit;
                        //var smsAddonNew = new StoreServices(db).LicensesAddOn(main.StoreCode, license.Id).FirstOrDefault(c => c.LicenseCode == "SMS")?.Count_limit;
                        //ViewBag.SMSWarning = smsAddonNew != "-1" && smsAddonNew != "0" && smsAddonActivated == "-1";
                        ViewBag.SMSWarning = false;
                    }
                }
                else
                {
                    var mainlicense = db.Store_Services.Where(c => c.StoreCode == storeCode && c.Active == 1 && c.Type == "license");
                    if (mainlicense.Count() == 0) throw new Exception("Store not yet activated main license!");
                    ViewBag.actionActive = false;
                    main = new MerchantService().GetMainSubscription(_storeCode);
                    profile = new MerchantService().PrimitiveStoreProfile(_storeCode, null, null, true);
                }
                var _optionConfigurationService = new OptionConfigService();
                var config = _optionConfigurationService.LoadSetting<Config>();
                if (config.SMS_Show_Provide == "IMS")
                {
                    var IMSBaseService = db.StoreBaseServices.Where(x => x.StoreCode == storeCode).ToList();
                    var SMSBaseService = IMSBaseService.SingleOrDefault(x => x.KeyName == "SMS");
                    profile.Licenses.Where(x => x.LicenseCode == "SMS").ForEach(item =>
                    {
                        item.Count_current = (SMSBaseService?.RemainingValue ?? 0).ToString();
                        item.Count_limit = (SMSBaseService?.MaximumValue ?? 0).ToString();
                    });
                }
                ViewBag.stage = stage;
                ViewBag.main = main;
                var customerCode = main?.CustomerCode;
                ViewBag.merchant_ims_version = db.C_Customer.FirstOrDefault(c => c.CustomerCode == customerCode)?.Version;
                ViewBag.features_pos = new FeatureService().GetListDefineFeatureByStore(profile?.StoreId, licenseId, stage != null && !stage.Equals("same-active"));

                _logService.Info($"[MerchantMan][MangoPOSInfo] completed", new { MangoPOSInfo = Newtonsoft.Json.JsonConvert.SerializeObject(profile) });
                return PartialView("_PartialMangoPOS_Detail", profile);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][MangoPOSInfo] error storeCode:{storeCode} - licenseId:{licenseId} - stage:{stage}");
                TempData["error"] = e.Message;
                ViewBag.stage = stage;
                ViewBag.main = new Store_Services { };
                return PartialView("_PartialMangoPOS_Detail", new StoreProfile { });
            }
        }

        public JsonResult CheckRemainingSMS(string licenseId)
        {
            _logService.Info($"[MerchantMan][CheckRemainingSMS] start licenseId = {licenseId}");
            try
            {
                var License = db.Store_Services.Find(licenseId);
                if (License == null) return Json(new object[] { true, false });
                var licenseAddon = new StoreServices(db).LicensesAddOn(License.StoreCode, License.Id).Select(l => new LicensesReq
                {
                    Rollover = l.Count_limit == "0" ? -1 : 0,
                    LicenseCode = l.LicenseCode,
                    LicenseType = l.LicenseType,
                    Count_limit = l.Count_limit,
                    Start_period = l.Start_period,
                    End_period = l.End_period
                }).ToList();
                var currentSMS = new StoreServices(db).GetStore(License.StoreCode).Licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Count_current ?? "0";
                var remaining = int.Parse(currentSMS.Replace(".0", "")) - int.Parse(licenseAddon.FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit);
                if (remaining < 0)
                {
                    _logService.Info($"[MerchantMan][CheckRemainingSMS] complete Result = True");
                    return Json(new object[] { true, true });
                }
                _logService.Info($"[MerchantMan][CheckRemainingSMS] complete Result = False");
                return Json(new object[] { true, false });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][ChangeSubscriptionRenew] error licenseId = {licenseId}");
                return Json(new object[] { false, false, e.Message });
            }
        }

        /// <summary>
        /// Do action store active
        /// </summary>
        /// <param name="licenseId"></param>
        /// <param name="active"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public async Task<JsonResult> StoreActivation(string licenseId, bool active = false, string stage = null)
        {
            _logService.Info($"[MerchantMan][StoreActivation] start licenseId:{licenseId} - active:{active} - stage:{stage}");
            bool version_setted = false;
            try
            {
                Store_Services newPlan = await db.Store_Services.FindAsync(licenseId);
                if (newPlan == null) throw new Exception("License not found!");
                var cus = db.C_Customer.Where(c => c.CustomerCode == newPlan.CustomerCode).FirstOrDefault();
                if (cus == null) throw new Exception("Merchant not found!");

                //if (!string.IsNullOrEmpty(ims_version) && !Enum.GetValues(typeof(IMSVersion)).Cast<IMSVersion>().Any(v => v.Text() == ims_version))
                //{
                //    throw new Exception($"ims_version: '{ims_version}' is not correct");
                //}
                //if (!string.IsNullOrEmpty(ims_version) && string.IsNullOrEmpty(cus.Version))
                //{
                Store_Services curPlan = db.Store_Services.Where(s => s.Type == "license" && s.StoreCode == newPlan.StoreCode && s.Active == 1).FirstOrDefault();
                if ((active && newPlan.Type == "license") ||
                    active && newPlan.Type == "addon" && newPlan.RenewDate != null)
                {
                    var new_ver = IMSVersion.POS_VER2.Code<string>();
                    if (cus.Version != new_ver)
                    {
                        //cus.Version = new_ver;
                        //db.Entry(cus).State = EntityState.Modified;
                        //await db.SaveChangesAsync();
                        if (curPlan != null)
                        {
                            using (MerchantService service = new MerchantService(true))
                            {
                                await service.ApproveAction(newPlan.StoreCode, curPlan.Id, false, "deActive");
                            }
                        }
                        cus.Version = new_ver;
                        await db.SaveChangesAsync();
                    }
                }

                version_setted = true;

                var license_active = licenseId;

                using (MerchantService service = new MerchantService(true))
                {
                    //active new service
                    license_active = await service.ApproveAction(newPlan.StoreCode, licenseId, active, stage);
                }

                var cancelStatus = InvoiceStatus.Canceled.ToString();
                var order = db.O_Orders.FirstOrDefault(o => o.OrdersCode == newPlan.OrderCode && o.Status != cancelStatus);
                if (order != null)
                {
                    if (license_active == licenseId && order.Status != InvoiceStatus.PaymentLater.ToString() && order.Status != InvoiceStatus.Closed.ToString()
                        && !db.Order_Products.Any(p => p.OrderCode == newPlan.OrderCode) && !db.Store_Services.Any(ss => ss.OrderCode == newPlan.OrderCode && ss.Active == -1))
                    {
                        var result = await OrderViewService.ChangeInvoiceSatus(newPlan.OrderCode, InvoiceStatus.Closed.ToString(), cMem);
                    }

                    if (newPlan.AutoRenew == true && order?.PaymentMethod != "Recurring")
                    {
                        if (curPlan != null) { PaymentService.SetStatusRecurring(curPlan.Id, "inactive"); }
                        PaymentService.SetStatusRecurring(license_active, "active", stage == "same-active" ? newPlan.RecurringType : order?.PaymentMethod);
                    }
                    PaymentService.UpdateRecurringStatus(order.CustomerCode);
                }
                if (stage == "deActive" && newPlan.Type == "license")
                {
                    await OrderViewService.ChangeInvoiceSatus(newPlan.OrderCode, "Canceled", cMem, true); //cancel invoice
                }
                _logService.Info($"[MerchantMan][StoreActivation] completed");
                return new Func.JsonStatusResult(newPlan.Productname + " " + (active ? "actived" : "deactived") + " Completed", HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][StoreActivation] error licenseId:{licenseId} - active:{active} - stage:{stage}");
                return new Func.JsonStatusResult(new object[] { "Process fail : " + (string.IsNullOrEmpty(e.InnerException?.Message) ? e.Message : e.InnerException.Message), version_setted, e.ToString() }, HttpStatusCode.ExpectationFailed);
            }
        }
        public async Task<JsonResult> StoreActivationImmediate(string licenseId/*, string ims_version*/, bool active = false, string stage = null)
        {
            _logService.Info($"[MerchantMan][StoreActivationImmediate] start licenseId:{licenseId} - active:{active} - stage:{stage}");
            bool version_setted = false;
            try
            {
                Store_Services newPlan = await db.Store_Services.FindAsync(licenseId);
                if (newPlan == null) throw new Exception("License not found!");
                var cus = db.C_Customer.Where(c => c.CustomerCode == newPlan.CustomerCode).FirstOrDefault();
                if (cus == null) throw new Exception("Merchant not found!");

                using (MerchantService service = new MerchantService(true))
                {
                    //active immediate
                    await service.ApproveActionImmediate(newPlan.StoreCode, licenseId, active, stage);
                }

                _logService.Info($"[MerchantMan][StoreActivationImmediate] completed");
                return new Func.JsonStatusResult(newPlan.Productname + " " + (active ? "actived" : "deactived") + " Completed", HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][StoreActivationImmediate] error licenseId:{licenseId} - active:{active} - stage:{stage}");
                return new Func.JsonStatusResult(new object[] { "Process fail : " + (string.IsNullOrEmpty(e.InnerException?.Message) ? e.Message : e.InnerException.Message), version_setted, e.ToString() }, HttpStatusCode.ExpectationFailed);
            }
        }

        public JsonResult ChangeSubscriptionRenew(string sto_id, bool Autorenew, string type = "")
        {
            _logService.Info($"[MerchantMan][ChangeSubscriptionRenew] start sto_id:{sto_id} - Autorenew:{Autorenew} - type:{type}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_mango_control")) != true || access["customer_mango_control"] != true)
                {
                    throw new Exception("You dont have permission to change!");
                }
                var store_item = db.Store_Services.Find(sto_id);
                if (store_item == null) throw new Exception("License not found!");
                string mes = "Turn off auto-recurring completed";
                bool noti = false;
                if (Autorenew)
                {
                    if (store_item.MxMerchant_contractId == null)
                    {
                        mes = "Create new recurring completed!";
                    }
                    else
                    {
                        mes = "Turn on auto recurring completed!";
                    }
                    PaymentService.addRecurringList(store_item.StoreCode, store_item.ProductCode);
                    noti = PaymentService.SetStatusRecurring(store_item.Id, "active", type);
                }
                else
                {
                    PaymentService.removeRecurringList(store_item.StoreCode, store_item.ProductCode);
                    noti = PaymentService.SetStatusRecurring(store_item.Id, "inactive", type);
                }
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == store_item.CustomerCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, (Autorenew ? "Turn on" : "Turn off") + " recurring subscription " + store_item.Productname);
                new MerchantService().WriteLogMerchant(cus, "Updated recurring subscription", (Autorenew ? "Turn on" : "Turn off") + " recurring subscription <b>" + store_item.Productname + "</b>.");
                _logService.Info($"[MerchantMan][ChangeSubscriptionRenew] completed");
                return Json(new object[] { true, noti, mes });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][ChangeSubscriptionRenew] error sto_id:{sto_id} - Autorenew:{Autorenew} - type:{type}");
                return Json(new object[] { false, false, e.Message });
            }
        }
        public JsonResult ChangeSubscriptionRenew_byCard(long MxCardId, bool isActive)
        {
            _logService.Info($"[MerchantMan][ChangeSubscriptionRenew] start MxCardId:{MxCardId} - isActive:{isActive}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_mango_control")) != true || access["customer_mango_control"] != true)
                {
                    throw new Exception("You dont have permission to change!");
                }
                var cardData = db.C_CustomerCard.FirstOrDefault(c => c.MxMerchant_Id == MxCardId && MxCardId > 0);
                if (cardData == null) throw new Exception("Card not found");
                var statusFilter = isActive ? "Inactive" : "Active";
                var statusChange = isActive ? "active" : "inactive";
                var store_item = db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == statusFilter && s.MxMerchant_cardAccountId == MxCardId).ToList();
                if (isActive != true)
                {
                    store_item.ForEach(s =>
                    {
                        PaymentService.SetStatusRecurring(s.Id, statusChange);
                    });
                }
                cardData.IsRecurring = isActive;
                db.Entry(cardData).State = EntityState.Modified;
                db.SaveChanges();
                if (isActive == true)
                {
                    store_item.ForEach(s =>
                    {
                        PaymentService.SetStatusRecurring(s.Id, statusChange);
                    });
                }
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == cardData.CustomerCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Updated payment card");
                new MerchantService().WriteLogMerchant(cus, "Updated payment card", (isActive ? "Turn on " : "Turn off ") + "recurring with card <b>" + cardData.CardNumber + "</b> completed");
                _logService.Info($"[MerchantMan][ChangeSubscriptionRenew] completed");
                return Json(new object[] { true, isActive ? "Turn on recurring completed" : "Turn off recurring completed" });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][ChangeSubscriptionRenew] error MxCardId:{MxCardId} - isActive:{isActive}");
                return Json(new object[] { false, e.Message });
            }
        }
        /// <summary>
        /// Change store password if not exist
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public async Task<JsonResult> ResetPassWord(string storeCode, bool set_pass, string new_pass, string confirm_pass, string email_cc, bool require_change, bool is_sendNoty)
        {
            _logService.Info($"[MerchantMan][ResetPassWord] start storeCode:{storeCode}");
            try
            {
                if (set_pass && (string.IsNullOrWhiteSpace(new_pass) || new_pass != confirm_pass))
                {
                    throw new Exception("New password or Confirm password is Invalid!");
                }
                using (MerchantService service = new MerchantService())
                {
                    await service.GenerateStorePassword(storeCode, set_pass ? new_pass : "", email_cc, require_change, is_sendNoty, Request);
                }
                //DelPairingCode(storeCode);
                var cus = db.C_Customer.FirstOrDefault(c => c.StoreCode == storeCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Resset merchant password");
                new MerchantService().WriteLogMerchant(cus, "Resset merchant password", "Resset merchant password has been completed");
                _logService.Info($"[MerchantMan][ResetPassWord] completed");
                return Json(new Dictionary<string, string>
                {
                    {"msg","Reset password success!"},
                });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][ResetPassWord] error storeCode:{storeCode}");
                return new Func.JsonStatusResult("Process fail : " + e.Message, HttpStatusCode.ExpectationFailed);
            }
        }

        public JsonResult getBankInfo(long id)
        {
            _logService.Info($"[MerchantMan][getBankInfo] start customerId:{id}");
            try
            {
                if (access?.Any(k => k.Key.Equals("customer_view_merchantaccount")) != true || access?["customer_view_merchantaccount"] != true)
                {
                    throw new Exception("You do not have permission to access");
                }
                var cus = db.C_Customer.Find(id);
                if (cus == null)
                {
                    throw new Exception("Bank infomation not found");
                }
                _logService.Info($"[MerchantMan][getBankInfo] completed");
                return Json(new object[] { true, new { cus.DepositBankName, cus.DepositAccountNumber, cus.DepositRoutingNumber } });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][getBankInfo] error customerId:{id}");
                return Json(new object[] { false, e.Message });
            }

        }
        public ActionResult loadContactPartial(long Id)
        {
            _logService.Info($"[MerchantMan][loadContactPartial] start Id:{Id}");
            try
            {
                WebDataModel db = new WebDataModel();
                ViewBag.Id = Id;
                ViewBag.p = access;
                var listContact = db.C_CustomerContact.Where(m => m.CustomerId == Id).ToList();
                _logService.Info($"[MerchantMan][loadContactPartial] completed");
                return PartialView("_MerchantPopupPartial_Contact", listContact);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][loadContactPartial] error Id:{Id}");
                throw;
            }
        }
        public JsonResult getContactInfo(long Id)
        {
            _logService.Info($"[MerchantMan][getContactInfo] start Id:{Id}");
            try
            {
                WebDataModel db = new WebDataModel();
                var ct = db.C_CustomerContact.Find(Id);
                _logService.Info($"[MerchantMan][getContactInfo] completed");
                return Json(new object[] { true, ct });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][getContactInfo] error Id:{Id}");
                return Json(new object[] { false, "Error: " + ex });
            }
        }
        public async Task<ActionResult> SaveContact(long? Id, string Name, string Authorization, string PreferredLanguage,
               string PreferredContact, string PhoneNumber, string Relationship, string Address, string Email, long CustomerId = 0)
        {
            _logService.Info($"[MerchantMan][SaveContact] start Id:{Id} - Name:{Name} - Authorization:{Authorization} - PreferredLanguage:{PreferredLanguage} - PreferredContact:{PreferredContact} - PhoneNumber:{PhoneNumber} - Relationship:{Relationship} - Address:{Address} - Email:{Email} - CustomerId:{CustomerId}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_addnew")) == false || access["customer_addnew"] != true)
                {
                    throw new Exception("You don't have permission save contact!");
                }
                if (Id > 0)
                {
                    var contact = db.C_CustomerContact.Find(Id);
                    contact.Name = Name;
                    contact.Authorization = Authorization;
                    contact.PreferredLanguage = PreferredLanguage;
                    contact.PreferredContact = PreferredContact;
                    contact.PhoneNumber = PhoneNumber;
                    contact.Relationship = Relationship;
                    contact.Address = Address;
                    contact.Email = Email;
                    db.Entry(contact).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                    var customer = db.C_Customer.Find(contact.CustomerId);
                    _merchantService.SaveHistoryUpdate(customer.CustomerCode, "Edited contact");
                    new MerchantService().WriteLogMerchant(customer, "Edited contact", "Edited contact <b>" + contact.Name + "</b> has been completed");
                    return Json(new object[] { true, "Edit Contact completed!", contact });
                }
                else
                {
                    var newcontact = new C_CustomerContact
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff")),
                        CustomerId = CustomerId,
                        Name = Name,
                        Authorization = Authorization,
                        PreferredLanguage = PreferredLanguage,
                        PreferredContact = PreferredContact,
                        PhoneNumber = PhoneNumber,
                        Relationship = Relationship,
                        Address = Address,
                        Email = Email
                    };
                    db.C_CustomerContact.Add(newcontact);
                    await db.SaveChangesAsync();
                    var customer = db.C_Customer.Find(newcontact.CustomerId);
                    _merchantService.SaveHistoryUpdate(customer.CustomerCode, "Added new contact");
                    new MerchantService().WriteLogMerchant(customer, "Added new contact", "Added new contact <b>" + newcontact.Name + "</b> has been completed");
                    return Json(new object[] { true, "Add new Contact completed!", newcontact.Id });
                }
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][SaveContact] error Id:{Id} - Name:{Name} - Authorization:{Authorization} - PreferredLanguage:{PreferredLanguage} - PreferredContact:{PreferredContact} - PhoneNumber:{PhoneNumber} - Relationship:{Relationship} - Address:{Address} - Email:{Email} - CustomerId:{CustomerId}");
                return Json(new object[] { false, "Error: " + e });
            }

        }
        public JsonResult DeleteContact(long Id)
        {
            _logService.Info($"[MerchantMan][DeleteContact] start Id:{Id}");
            try
            {
                if (access.Any(k => k.Key.Equals("customer_delete")) == false || access["customer_delete"] != true)
                {
                    throw new Exception("You don't have permission delete contact!");
                }

                var delContact = db.C_CustomerContact.Find(Id);
                var customer = db.C_Customer.Find(delContact.CustomerId);
                db.C_CustomerContact.Remove(delContact);
                db.SaveChanges();
                _merchantService.SaveHistoryUpdate(customer.CustomerCode, "Deleted contact");
                new MerchantService().WriteLogMerchant(customer, "Deleted contact", "Deleted contact <b>" + delContact.Name + "</b> has been completed");
                _logService.Info($"[MerchantMan][DeleteContact] completed");
                return Json(new object[] { true });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][DeleteContact] error Id:{Id}");
                return Json(new object[] { false, "Error: " + e });
            }

        }

        //public ActionResult loadAccountManagerPartial(string customerCode)
        //{
        //    ViewBag.p = access;
        //    ViewBag.CustomerCode = customerCode;
        //    var listAccount = db.C_CustomerAccountManager.Where(m => m.CustomerCode == customerCode).ToList();
        //    return PartialView("_MerchantPopupPartial_AccountManager", listAccount);
        //}
        //public JsonResult getAccountManager(string Id)
        //{
        //    try
        //    {
        //        var am = db.C_CustomerAccountManager.Find(Id);
        //        return Json(new object[] { true, am });
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, "Error: " + e });
        //    }
        //}
        //public async Task<ActionResult> SaveAccountManager(C_CustomerAccountManager accManager)
        //{
        //    try
        //    {
        //        //if (access.Any(k => k.Key.Equals("customer_addnew")) == false || access["customer_addnew"] != true)
        //        //{
        //        //    throw new Exception("You don't have permission save contact!");
        //        //}
        //        if (!string.IsNullOrEmpty(accManager.Id))
        //        {
        //            var _acc = db.C_CustomerAccountManager.Find(accManager.Id);
        //            _acc.Name = accManager.Name;
        //            _acc.Username = accManager.Username;
        //            _acc.Password = accManager.Password;
        //            _acc.Description = accManager.Description;
        //            _acc.UpdateAt = DateTime.UtcNow;
        //            _acc.UpdateBy = cMem?.FullName ?? "IMS System";
        //            db.Entry(_acc).State = System.Data.Entity.EntityState.Modified;
        //            await db.SaveChangesAsync();
        //            var customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == _acc.CustomerCode);
        //            SaveHistoryUpdate(customer, "Edited account manager");
        //            new MerchantService().WriteLogMerchant(customer, "Edited account manager", "Account manager <b>" + _acc.Name + "</b> has been edited");
        //            return Json(new object[] { true, "Edit account manager completed!", _acc });
        //        }
        //        else
        //        {
        //            var newAcc = new C_CustomerAccountManager
        //            {
        //                Id = Guid.NewGuid().ToString(),
        //                CustomerCode = accManager.CustomerCode,
        //                Name = accManager.Name,
        //                Username = accManager.Username,
        //                Password = accManager.Password,
        //                Description = accManager.Description,
        //                CreateAt = DateTime.UtcNow,
        //                CreateBy = cMem?.FullName ?? "IMS System"
        //            };
        //            db.C_CustomerAccountManager.Add(newAcc);
        //            await db.SaveChangesAsync();
        //            var customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == newAcc.CustomerCode);
        //            SaveHistoryUpdate(customer, "Added new account manager");
        //            new MerchantService().WriteLogMerchant(customer, "Added new account manager", "New account manager <b>" + accManager.Name + "</b> has been added");
        //            return Json(new object[] { true, "Add new account manager completed!", newAcc.Id });
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, "Error: " + e });
        //    }

        //}
        //public JsonResult DeleteAccountManager(string Id)
        //{
        //    try
        //    {
        //        //if (access.Any(k => k.Key.Equals("customer_delete")) == false || access["customer_delete"] != true)
        //        //{
        //        //    throw new Exception("You don't have permission delete contact!");
        //        //}
        //        var delAccountmanager = db.C_CustomerAccountManager.Find(Id);
        //        var customer = db.C_Customer.FirstOrDefault(c=>c.CustomerCode == delAccountmanager.CustomerCode);
        //        db.C_CustomerAccountManager.Remove(delAccountmanager);
        //        db.SaveChanges();
        //        SaveHistoryUpdate(customer, "Deleted account manager");
        //        new MerchantService().WriteLogMerchant(customer, "Deleted account manager", "Deleted account manager <b>" + delAccountmanager.Name + "</b> has been completed");
        //        return Json(new object[] { true }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new object[] { false, "Error: " + e }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        /// <summary>
        /// thong tin onboarding merchant nay
        /// </summary>
        /// <param name="Code"> customer code</param>
        /// <returns></returns>
        public ActionResult loadMerchantProcessPartial(string Code)
        {
            _logService.Info($"[MerchantMan][loadMerchantProcessPartial] start Code:{Code}");
            try
            {
                //Neu processId
                Code = string.IsNullOrEmpty(Code) ? "0" : Code;
                WebDataModel db = new WebDataModel();
                ViewBag.code = Code;
                ViewBag.p = access;
                var mcprocesses = db.C_MerchantSubscribe.Where(m => m.CustomerCode == Code).ToList();
                _logService.Info($"[MerchantMan][loadMerchantProcessPartial] completed");
                return PartialView("_MerchantPopupPartial_MerchantProcess", mcprocesses);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][loadMerchantProcessPartial] error Code:{Code}");
                throw;
            }
        }

        public JsonResult clearContactMP()
        {
            _logService.Info($"[MerchantMan][clearContactMP] start");
            try
            {
                WebDataModel db = new WebDataModel();
                var List_mp = db.C_MerchantProcess.Where(m => m.CustomerCode == "0").ToList();
                db.C_MerchantProcess.RemoveRange(List_mp);
                var list_contact = db.C_CustomerContact.Where(m => m.CustomerId == 0).ToList();
                db.C_CustomerContact.RemoveRange(list_contact);
                db.SaveChanges();
                _logService.Info($"[MerchantMan][clearContactMP] completed");
                return null;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][clearContactMP] error");
                throw;
            }

        }
        public JsonResult GetPasswordValue(long id, string name)
        {
            if (access?.Any(k => k.Key.Equals("customer_view_merchantaccount")) == true && access?["customer_view_merchantaccount"] == true)
            {
                WebDataModel db = new WebDataModel();
                var cs = db.C_Customer.Find(id);
                var pm = db.C_MerchantProcess.Find(id);
                string value = "";
                if (name == "password")
                    value = cs.Password;
                if (name == "SocialSecurity" || name == "SocialNumber")
                    value = cs.SocialSecurity;
                value = string.IsNullOrEmpty(value) ? "---" : value;
                return Json(new object[] { true, value });
            }
            else
            {
                return Json(new object[] { false, "You don't have permission to view this information!" });
            }

        }

        /*
        public JsonResult SaveCreditCard(C_CustomerCredit cardInfo)
        {
            var result = new C_CustomerCredit();
            try
            {
                if (cardInfo.CARDNUMBER.Length < 10)
                {
                    throw new Exception("Card number must have a length of at least 10!");
                }
                if (cardInfo.CARDEXPIRY.Length != 4)
                {
                    throw new Exception("Card expiry length must be equal to 4!");
                }
                if (!string.IsNullOrEmpty(cardInfo.CVV) && cardInfo.CVV.Length < 3)
                {
                    throw new Exception("CVV must have a length of at least 3");
                }
                result = AppLB.NuveiLB.SendXML.Nuvei_CreditCardRegis(cardInfo, "33001", "SandboxSecret001");
                result.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                var db = new WebDataModel();
                db.C_CustomerCredit.Add(result);
                db.SaveChanges();
                return Json(new object[] { true, result, result.NuveiResponse_DATETIME?.ToString("dd MMM yyyy hh:mm:ss").ToUpper() });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult SetDefaultCreditCard(long Id)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                db.C_CustomerCredit.Where(c => c.@default == true).ToList().ForEach(c => c.@default = false);
                db.C_CustomerCredit.Find(Id).@default = true;
                db.SaveChanges();
                return Json(new object[] { true, "Default credit card has been changed" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });
            }

        }
        */

        //dem so luong file theo cumtomrer id

        public JsonResult countFile(long id)
        {
            WebDataModel db = new WebDataModel();
            var CustomerCode = db.C_Customer.Find(id).CustomerCode;
            int count = 0;
            count += db.UploadMoreFiles.Where(u => u.TableName == "C_Customer" && u.TableId == id).Count();
            foreach (var tk_Id in db.T_SupportTicket.Where(t => t.CustomerCode == CustomerCode).Select(t => t.Id).ToList())
            {
                count += db.UploadMoreFiles.Where(u => u.TableId == tk_Id).Count();
                foreach (var fb in db.T_TicketFeedback.Where(f => f.TicketId == tk_Id))
                {
                    count += db.UploadMoreFiles.Where(u => u.TableId == fb.Id).Count();
                }
                foreach (var fr in db.T_FileRelated.Where(f => f.TicketId == tk_Id))
                {
                    count += db.UploadMoreFiles.Where(u => u.TableId == fr.Id).Count();
                }
            }
            return Json(count);
        }
        //convert string chi so va chu
        public string cv_SText(string st)
        {
            return Regex.Replace(CommonFunc.ConvertNonUnicodeURL(st), "[^0-9a-zA-Z]*", "").ToLower();
        }


        #region load US state
        public JsonResult loadState()
        {
            WebDataModel db = new WebDataModel();
            var states = db.Ad_USAState.ToList();
            return Json(new object[] { true, states });
        }
        public JsonResult LoadListState()
        {
            var db = new WebDataModel();
            var list_state = db.Ad_USAState.Select(s => s.abbreviation).ToList();
            var list_country = db.Ad_Country.Where(c => c.CountryCode == "US").Select(c => c.Name).ToList();
            return Json(new object[] { list_state, list_country }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //Outgoing email
        public ActionResult LoadOutGoingEmail(IDataTablesRequest dataTablesRequest, string CustomerCode)
        {
            var viewModel = db.C_OutgoingEmail.Where(c => c.CustomerCode == CustomerCode && c.IsDelete != true).AsEnumerable();
            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "EmailCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        viewModel = viewModel.OrderBy(s => s.Id);
                    }
                    else
                    {
                        viewModel = viewModel.OrderByDescending(s => s.Id);
                    }
                    break;
                case "Subject":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        viewModel = viewModel.OrderBy(s => s.Subject);
                    }
                    else
                    {
                        viewModel = viewModel.OrderByDescending(s => s.Subject);
                    }
                    break;
                case "Content":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        viewModel = viewModel.OrderBy(s => s.Content);
                    }
                    else
                    {
                        viewModel = viewModel.OrderByDescending(s => s.Content);
                    }
                    break;
                case "SendBy":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        viewModel = viewModel.OrderBy(s => getLastHisorySent(s.HistorySend));
                    }
                    else
                    {
                        viewModel = viewModel.OrderByDescending(s => getLastHisorySent(s.HistorySend));
                    }
                    break;
                case "SendDate":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        viewModel = viewModel.OrderBy(s => DateTime.Parse(getLastHisorySent(s.HistorySend, "date")));
                    }
                    else
                    {
                        viewModel = viewModel.OrderByDescending(s => DateTime.Parse(getLastHisorySent(s.HistorySend, "date")));
                    }
                    break;
                default:
                    viewModel = viewModel.OrderByDescending(s => s.Id);
                    break;
            }

            var totalRecord = viewModel.Count();
            viewModel = viewModel.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var dataView = viewModel.ToList();
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }

        public string getLastHisorySent(string history = "", string text = "name")
        {
            var last = history?.Split('|').Last();
            var result = text == "name" ? last?.Split(',').First() : (last?.Split(',').Last() ?? new DateTime(2020, 1, 1).ToString());
            return result;
        }

        [HttpPost, ValidateInput(false)]
        public async Task<JsonResult> SaveOutgoingEmail(C_OutgoingEmail newEmail)
        {
            _logService.Info($"[MerchantMan][SaveOutgoingEmail] start", new { newEmail = Newtonsoft.Json.JsonConvert.SerializeObject(newEmail) });
            try
            {
                var msg = string.Empty;
                bool IsSend = false;
                if (Request != null)
                    IsSend = (Request["btn-submit"] ?? string.Empty) == "send";
                if (newEmail.Id > 0) //Edit
                {
                    var oldEmail = db.C_OutgoingEmail.Find(newEmail.Id);
                    if (oldEmail == null) throw new Exception("Email not found.");

                    if (Request["btn-submit"] == "onlysend")
                    {
                        msg = await OutgoingEmailToCustomer(oldEmail);
                        if (!string.IsNullOrEmpty(msg))
                            return Json(new object[] { true, "Send email fail. " + msg });
                        oldEmail.HistorySend += ("|" + cMem.FullName + "," + DateTime.UtcNow.ToString());
                        db.Entry(oldEmail).State = EntityState.Modified;
                        db.SaveChanges();
                        return Json(new object[] { true, "Send email success." });
                    }

                    oldEmail.ToEmail = newEmail.ToEmail;
                    oldEmail.CCEmail = newEmail.CCEmail;
                    oldEmail.ReplyEmail = newEmail.ReplyEmail;
                    oldEmail.Subject = newEmail.Subject;
                    oldEmail.Content = newEmail.Content;
                    oldEmail.Attachment = newEmail.Attachment;
                    if (IsSend)
                    {
                        msg = await OutgoingEmailToCustomer(oldEmail);
                        if (string.IsNullOrEmpty(msg))
                            oldEmail.HistorySend += ("|" + cMem.FullName + "," + DateTime.UtcNow.ToString());
                    }

                    db.Entry(oldEmail).State = EntityState.Modified;
                    db.SaveChanges();
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == oldEmail.CustomerCode);
                    _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Updated outgoing email");
                    new MerchantService().WriteLogMerchant(cus, "Updated outgoing email", "Outgoing email email has been updated");
                    if (!string.IsNullOrEmpty(msg))
                        return Json(new object[] { true, "Send email fail. " + msg });

                    return Json(new object[] { true, "Update success." });
                }
                else //Create
                {
                    newEmail.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
                    newEmail.CreateBy = cMem?.FullName;
                    newEmail.CreateAt = DateTime.UtcNow;
                    newEmail.IsDelete = false;

                    if (IsSend)
                    {
                        msg = await OutgoingEmailToCustomer(newEmail);
                        newEmail.HistorySend = cMem.FullName + "," + DateTime.UtcNow.ToString();
                    }
                    db.C_OutgoingEmail.Add(newEmail);
                    db.SaveChanges();
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == newEmail.CustomerCode);
                    _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Create outgoing email");
                    new MerchantService().WriteLogMerchant(cus, "Create outgoing email", "Outgoing email email has been created");
                    if (!string.IsNullOrEmpty(msg))
                        return Json(new object[] { true, "Send email fail. " + msg });
                    return Json(new object[] { true, "Create new email success." });
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][loadMerchantProcessPartial] error");
                return Json(new object[] { false, ex.Message });
            }
        }

        public JsonResult RemoveOutgoingEmail(long key)
        {
            _logService.Info($"[MerchantMan][RemoveOutgoingEmail] start key:{key}");
            try
            {
                var email = db.C_OutgoingEmail.Find(key);
                if (email == null)
                    throw new Exception("Ads not found.");
                email.IsDelete = true;
                db.Entry(email).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == email.CustomerCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Removed outgoing email");
                new MerchantService().WriteLogMerchant(cus, "Removed outgoing email", "Outgoing email email has been removed");
                _logService.Info($"[MerchantMan][RemoveOutgoingEmail] completed");
                return Json(new object[] { true, "Remove " + email.Subject + " success." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[MerchantMan][RemoveOutgoingEmail] error key:{key}");
                return Json(new object[] { false, "Remove fail. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<string> OutgoingEmailToCustomer(C_OutgoingEmail oEmail)
        {
            _logService.Info($"[MerchantMan][OutgoingEmailToCustomer] start", new { oEmail = Newtonsoft.Json.JsonConvert.SerializeObject(oEmail) });
            try
            {
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == oEmail.CustomerCode);
                if (cus == null)
                    throw new Exception("Customer not found.");

                var emailData = new { content = oEmail.Content, subject = oEmail.Subject };
                var msg = await _mailingService.SendNotifyOutgoingWithTemplate(oEmail.ToEmail, cus.OwnerName, oEmail.Subject, oEmail.CCEmail, emailData, oEmail.Attachment);
                if (!string.IsNullOrEmpty(msg))
                    throw new Exception(msg);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Sent outgoing email to merchant");
                new MerchantService().WriteLogMerchant(cus, "Sent outgoing email to merchant", "Sent outgoing email to merchant has been completed");
                _logService.Info($"[MerchantMan][OutgoingEmailToCustomer] completed");
                return string.Empty;
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][OutgoingEmailToCustomer] error");
                return "Send fail! " + e.Message;
            }
        }

        #region current_plan -additional

        public ActionResult LoadPartialAddonPopup()
        {
            string AddonType = LicenseType.ADD_ON.Text();
            string GiftcardType = LicenseType.GiftCard.Text();
            string VirtualHardware_OtherType = LicenseType.VirtualHardware_Other.Text();

            var addons = from p in db.License_Product
                         where (p.Type == AddonType || p.Type == GiftcardType || p.Type == VirtualHardware_OtherType) && p.Active == true
                         orderby p.Price
                         let checkExistLicenseItems = db.License_Product_Item.Where(x => x.License_Product_Id == p.Id).Count() > 0
                         where !(p.Type == AddonType && !checkExistLicenseItems)
                         select p;
            return PartialView("_Partial_AddonPopup", addons);
        }

        public ActionResult LoadPartialProductPopup(string cus_code)
        {
            var cus = db.C_Customer.Where(c => c.CustomerCode == cus_code).FirstOrDefault() ?? new C_Customer { };
            ViewBag.partnerCode = cus?.PartnerCode;
            ViewBag.Partner = db.C_Partner.FirstOrDefault(c => c.Code == cus.PartnerCode) ?? new C_Partner { };
            var v2 = Utils.IEnums.IMSVersion.POS_VER2.Code<string>();
            var current_plan_code = db.Store_Services.Where(s => s.CustomerCode == cus_code && s.Active == 1 && s.Type == "license").FirstOrDefault()?.ProductCode ?? "";
            string mLicenseType = LicenseType.LICENSE.Text();
            var product = db.License_Product.Where(p => p.Type == mLicenseType &&
                                                        p.Code != current_plan_code &&
                                                        p.Active == true &&
                                                        p.IsDelete != true
                                                        ).OrderBy(p => p.Name).ToList();
            return PartialView("_Partial_AddonPopup", product);
        }

        public ActionResult LoadPartialActiveProductPopup(string cus_code)
        {
            var cus = db.C_Customer.Where(c => c.CustomerCode == cus_code).FirstOrDefault();
            var v2 = Utils.IEnums.IMSVersion.POS_VER2.Code<string>();
            var current_plan_code = db.Store_Services.Where(s => s.CustomerCode == cus_code && s.Active == 1 && s.Type == "license").FirstOrDefault()?.ProductCode ?? "";
            string mLicenseType = LicenseType.LICENSE.Text();
            var product = db.License_Product.Where(p => p.Type == mLicenseType &&
            (string.IsNullOrEmpty(cus.Version) || ((string.IsNullOrEmpty(p.Code_POSSystem) == (cus.Version == v2)) || string.IsNullOrEmpty(cus.Version))))
                .OrderBy(p => p.Price).ToList();
            return PartialView("_Partial_AddonPopup", product);
        }

        /// <summary>
        /// Load List Order Subscription License
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //public ActionResult LoadOrderSubscriptionLicense(long? id)
        //{
        //    try
        //    {
        //        var list_OrderSubscriptionLicense = db.Order_Subcription_License.Where(x => x.Order_Subcription_Id == id).ToList();
        //        return PartialView("_O_SubscriptionLicensePartial", list_OrderSubscriptionLicense);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.ErrMsg = ex.Message;
        //        return PartialView("_O_SubscriptionLicensePartial", null);
        //    }
        //}

        /// <summary>
        /// Resend notify email login account to salon
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public async Task<JsonResult> ResendMailActiveStore(string customerCode)
        {
            _logService.Info($"[MerchantMan][ResendMailActiveStore] start customerCode:{customerCode}");
            try
            {
                using (MerchantService service = new MerchantService(true))
                {
                    await service.SendMailStoreActive(customerCode);
                }
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == customerCode);
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Resend notify email login account to salon");
                new MerchantService().WriteLogMerchant(cus, "Resend notify email login account to salon", "Resend notify email login account to salon");
                _logService.Info($"[MerchantMan][ResendMailActiveStore]  completed");
                return new Func.JsonStatusResult("Email has been sent!", HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[MerchantMan][ResendMailActiveStore] error customerCode:{customerCode}");
                Console.WriteLine(e.StackTrace);
                return new Func.JsonStatusResult(e.Message, HttpStatusCode.ExpectationFailed);
            }
        }
        public ActionResult HandleSummaryLicenseSelected(string LicenseId, string CustomerCode)
        {
            var partnerCode = db.O_Orders.FirstOrDefault(c => !string.IsNullOrEmpty(c.PartnerCode) && c.CustomerCode == CustomerCode)?.PartnerCode;
            var partner_price_type = db.C_Partner.FirstOrDefault(c => c.Code == partnerCode)?.PriceType;
            var License = db.License_Product.Find(LicenseId);
            if (!string.IsNullOrEmpty(partnerCode))
            {
                License.Price = partner_price_type == "membership" ? License.MembershipPrice : License.PartnerPrice;
            }
            var ApplyTrial = false;
            var ApplyPromotional = false;
            if (License.Trial_Months > 0)
                ApplyTrial = true;

            if (License.Promotion_Apply_Months > 0)
            {
                var removeLicenseStatus = LicenseStatus.REMOVED.Code<int>();
                var watingLicenseStatus = LicenseStatus.WAITING.Code<int>();

                var FirstLicenseApply = db.Store_Services.Where(x => x.ProductCode == License.Code && x.CustomerCode == CustomerCode && x.Type == "license" && x.Active != removeLicenseStatus && x.Active != watingLicenseStatus).OrderBy(x => x.EffectiveDate).FirstOrDefault();
                var LastLicenseApply = db.Store_Services.Where(x => x.ProductCode == License.Code && x.CustomerCode == CustomerCode && x.Type == "license" && x.Active != removeLicenseStatus && x.Active != watingLicenseStatus).OrderByDescending(x => x.RenewDate).FirstOrDefault();
                if (License.Promotion_Time_To_Available == 0)
                {
                    ApplyPromotional = true;
                }
                if (FirstLicenseApply != null && LastLicenseApply != null && ApplyPromotional == false)
                {
                    if (License.Promotion_Time_To_Available <= (LastLicenseApply.RenewDate - FirstLicenseApply.EffectiveDate).Value.TotalDays / 30)
                    {
                        ApplyPromotional = true;
                    }
                }
            }
            return Json(new { License, ApplyTrial, ApplyPromotional });
        }

        #endregion


        #region transaction

        //public DateTime CardExpiryDate(string CardExpiry)
        //{
        //    try
        //    {
        //        int month = int.Parse(CardExpiry.Substring(0, 2));
        //        int year = int.Parse(CardExpiry.Substring(2, 2)) + 2000;
        //        return new DateTime(year, month, DateTime.DaysInMonth(year, month));
        //    }
        //    catch
        //    {
        //        return DateTime.MinValue;
        //    }
        //}
        public JsonResult GetCardInfo(string CardId)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var card = db.C_CustomerCard.Find(CardId);
                    return Json(new object[] { true, card, true });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }

        internal PaymentService _payment = new PaymentService();

        public JsonResult SaveEditCard(string customer_code, C_CustomerCard card)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var cus = db.C_Customer.Where(c => c.CustomerCode == customer_code).FirstOrDefault();
                    if (cus == null)
                    {
                        throw new Exception("authentication failed");
                    }
                    var EditCard = db.C_CustomerCard.Find(card.Id);
                    if (cus?.CustomerCode != EditCard?.CustomerCode)
                    {
                        throw new Exception("Card number is not match!");
                    }
                    card.StoreCode = cus.StoreCode;
                    //_payment.EditCard(card);
                    return Json(new object[] { true, EditCard.Active, Check_expiry(EditCard.CardExpiry) });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult AddNewCard(TransRequest info)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    var cus = db.C_Customer.Where(c => c.CustomerCode == info.CustomerCode).FirstOrDefault();
                    info.StoreCode = cus.StoreCode;
                    info.MxMerchant_Id = long.Parse(cus.MxMerchant_Id ?? "0");
                    var newCard = _payment.NewCard(info);
                    _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Added new payment card");
                    new MerchantService().WriteLogMerchant(cus, "Added new payment card", "New credit card has been added. Card number: <b>" + newCard.CardNumber + "</b>");
                    return Json(new object[] { true, "Add new card completed" });
                }
                catch (AppHandleException ex)
                {
                    return Json(new object[] { false, ex.Message });
                }


            }
        }
        public JsonResult ChangeStatusCard(string id, bool active)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var card = db.C_CustomerCard.Find(id);
                    if (card == null)
                    {
                        throw new Exception("Card not found!");
                    }
                    if (card.IsDefault && !active)
                    {
                        //throw new Exception("Can't deactivate Default card!");
                        db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == "Active" && s.MxMerchant_cardAccountId == card.MxMerchant_Id).ToList().ForEach(s =>
                        {
                            PaymentService.SetStatusRecurring(s.Id, "inactive");
                        });
                    }
                    card.IsDefault = false;
                    card.Active = active;
                    db.Entry(card).State = EntityState.Modified;
                    db.SaveChanges();
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == card.CustomerCode);
                    _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Updated payment card");
                    new MerchantService().WriteLogMerchant(cus, "Updated payment card", "Credit card has been updated. Card number: <b>" + card.CardNumber + "</b>");
                    return Json(new object[] { true, active, true });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }

        public JsonResult DeleteCard(string id)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var card = db.C_CustomerCard.Find(id);
                    if (card == null)
                    {
                        throw new Exception("Card not found!");
                    }
                    if (card.IsDefault)
                    {
                        //throw new Exception("Can't deactivate Default card!");
                        db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == "Active" && s.MxMerchant_cardAccountId == card.MxMerchant_Id).ToList().ForEach(s =>
                        {
                            PaymentService.SetStatusRecurring(s.Id, "inactive");
                        });
                    }
                    db.C_CustomerCard.Remove(card);
                    db.SaveChanges();
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == card.CustomerCode);
                    _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Delete payment card");
                    new MerchantService().WriteLogMerchant(cus, "Delete payment card", "Credit card has been deleted. Card number: <b>" + card.CardNumber + "</b>");
                    return Json(new object[] { true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        private bool Check_expiry(string cardExpiry)
        {
            try
            {
                int month = int.Parse(cardExpiry.Substring(0, 2));
                int year = int.Parse(cardExpiry.Substring(2, 2)) + 2000;
                if (new DateTime(year, month, DateTime.DaysInMonth(year, month)) >= DateTime.UtcNow.Date)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost]
        public JsonResult UpdateTransactionAmount(string Id, decimal? Amount, string Note = "")
        {
            try
            {
                var trans = db.C_CustomerTransaction.Find(Id);
                if (trans == null) throw new Exception("Transaction not found");
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == trans.CustomerCode);
                var oldAmount = trans.Amount;
                trans.Amount = Amount ?? decimal.Zero;
                trans.UpdateDescription = Note + " - By: " + (cMem.FullName ?? "IMS System") + " at " + DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm") + " (UTC)";
                db.Entry(trans).State = EntityState.Modified;
                db.SaveChanges();
                _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Update transaction amount of order <b>#" + trans.OrdersCode + "</b>");
                var description =
                    $"<b>#{trans.OrdersCode}</b><br /><a href='#' onclick='show_invoice({trans.OrdersCode})' style='color:dodgerblue'> [View receipt]</a><br/><br/>" +
                    $"Transaction amount has been updated from <i style='font-weight:bolder'>{oldAmount.ToString("$#,##0.00")}</i> to <i style='font-weight:bolder'>{trans.Amount.ToString("$#,##0.00")}</i>.<br/>" +
                    $"Update note: <i>{Note}</i><br/>" +
                    $"By: {cMem.FullName ?? "IMS System"} at {DateTime.UtcNow.ToString("MMM dd, yyyy hh:mm")} (UTC)";
                new MerchantService().WriteLogMerchant(cus, "Update transaction amount of order <b>#" + trans.OrdersCode + "</b>", description);

                return Json(new object[] { true, "Update success" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Update error: " + e.Message });
            }
        }
        #endregion

        #region handletickettab
        private SaleLeadInfo getLead(string SalesLeadId)
        {
            var sl = db.C_SalesLead.AsNoTracking().FirstOrDefault(x => x.Id == SalesLeadId);
            var sl_status = db.C_SalesLead_Status.AsNoTracking().FirstOrDefault(x => sl.SL_Status == x.Order);
            var customer = db.C_Customer.AsNoTracking().FirstOrDefault(x => x.CustomerCode == sl.CustomerCode);

            SaleLeadInfo lead = SaleLeadInfo.MakeSelect(customer, sl, sl_status, true);
            return lead;
        }
        [HttpPost]
        public ActionResult GetHistoryLog(string CustomerCode)
        {
            var sl = db.C_SalesLead.Where(x => x.CustomerCode == CustomerCode).FirstOrDefault();
            ViewBag.SalesLeadId = sl.Id;
            IEnumerable<Calendar_Event> appointments = db.Calendar_Event.Where(a => a.SalesLeadId == sl.Id).OrderByDescending(a => a.StartEvent).ToList();
            ViewBag.ListLog = new DetailSalesLeadCustomizeModel
            {
                even = appointments,
                lead = getLead(sl.Id)
            };
            return PartialView("_LogHistoryPartial");
        }
        public ActionResult LoadTicket(IDataTablesRequest dataTablesRequest, string SearchText, string Status, string CustomerCode)
        {
            var query = from t in db.T_SupportTicket where t.CustomerCode == CustomerCode && !string.IsNullOrEmpty(t.ProjectId) select t;
            int allTicket = 0;
            int openTicket = 0;
            int unassignedTicket = 0;
            int closedTicket = 0;
            int invisibleTicket = 0;
            allTicket = query.Count();
            openTicket = query.Where(t => t.Visible == true && t.DateClosed == null && ((string.IsNullOrEmpty(t.AssignedToMemberNumber) == false || string.IsNullOrEmpty(t.ReassignedToMemberNumber) == false))).Count();
            unassignedTicket = query.Where(t => t.Visible == true && string.IsNullOrEmpty(t.AssignedToMemberNumber) == true && string.IsNullOrEmpty(t.ReassignedToMemberNumber) == true && t.DateClosed == null).Count();
            closedTicket = query.Where(t => t.Visible == true && t.DateClosed != null).Count();
            invisibleTicket = query.Where(t => t.Visible == null || t.Visible == false).Count();
            //if (Status== "all")
            //{
            //    query = query.Where(t => t.DateClosed.HasValue);
            //}
            if (Status == "opened")
            {
                query = query.Where(t => t.Visible == true && !t.DateClosed.HasValue);
            }
            if (Status == "closed")
            {
                query = query.Where(t => t.Visible == true && t.DateClosed.HasValue);
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(t => t.Name.Contains(SearchText));
            }
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch.Name)
            {
                case "Name":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.Name);
                    }
                    break;
                case "Status":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.StatusName);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.StatusName);
                    }
                    break;

                default:
                    query = query.OrderByDescending(s => s.CreateAt);
                    break;
            }
            int totalRecord = query.Count();
            var ViewData = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x => new
            {
                Name = x.Name,
                CreateAt = string.Format("{0:r}", x.CreateAt),
                CreateByName = x.CreateByName,
                DateClosed = x.DateClosed,
                Id = x.Id,
                x.AssignedToMemberName,
                x.ReassignedToMemberName,
                CustomerCode = x.CustomerCode,
                DateOpened = x.DateOpened != null ? string.Format("{0:r}", x.DateOpened) : string.Empty,
                TypeName = x.TypeName,
                StatusName = x.StatusName,
                UpdateTicketHistory = EnrichcousBackOffice.ViewControler.TicketViewService.GetLastestUpdate(x.UpdateTicketHistory)
            });
            return Json(new
            {
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                draw = dataTablesRequest.Draw,
                data = ViewData,
                allTicket,
                openTicket,
                unassignedTicket,
                closedTicket,
                invisibleTicket,
            });
        }
        public string UpdateWordDetermine(string CustomerCode)
        {

            var cus = db.C_Customer.Where(x => x.CustomerCode == CustomerCode).FirstOrDefault();

            if (cus != null)
            {
                var listMerchantSub = db.C_MerchantSubscribe.Where(x => x.CustomerCode == CustomerCode && (x.Status == "closed" || x.Status == "active")).Count() > 0;
                var checkExistStore = db.Store_Services.Where(x => x.CustomerCode == cus.CustomerCode && x.Type == "license" && x.Active == 1).Count() > 0;
                if (listMerchantSub && checkExistStore)
                {
                    cus.WordDetermine = "Q";
                }
                else
                {
                    cus.WordDetermine = "H";
                }
            }
            db.SaveChanges();
            return cus.WordDetermine;
        }
        #endregion
        #region transaction report
        public ActionResult TransactionReport()
        {
            if ((access.Any(k => k.Key.Equals("report_transaction")) == false || access["report_transaction"] != true))
            {
                throw new Exception("Access denied");
            }
            DateTime From = DateTime.UtcNow.AddMonths(-6);
            ViewBag.FromDate = new DateTime(From.Year, From.Month, 1).ToString("MM/dd/yyyy");
            ViewBag.ToDate = DateTime.UtcNow.ToString("MM/dd/yyyy");
            ViewBag.Partners = db.C_Partner.Where(c => c.Status > 0 && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId)).OrderBy(c => c.SiteId).ToList();
            ViewBag.ProductName = db.SystemConfigurations.FirstOrDefault().ProductName ?? db.SystemConfigurations.FirstOrDefault().CompanyName;
            return View();
        }
        [HttpPost]
        public ActionResult GetListTransaction(IDataTablesRequest dataTable, DateTime? FromDate, DateTime? ToDate, string PaymentType, string SearchText, string Status, string Partner)
        {
            var query = from t in db.C_CustomerTransaction
                        join cus in db.C_Customer.Where(c => (cMem.SiteId == 1 || cMem.SiteId == c.SiteId)) on t.CustomerCode equals cus.CustomerCode
                        join o in db.O_Orders on t.OrdersCode equals o.OrdersCode
                        select new { t, cus, o };
            //filter data  from date
            if (FromDate != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.t.CreateAt) >= FromDate);
            }
            //filter data to date
            if (ToDate != null)
            {
                query = query.Where(x => DbFunctions.TruncateTime(x.t.CreateAt) <= ToDate);
            }
            if (!string.IsNullOrEmpty(Partner))
            {
                if (Partner == "mango")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.o.PartnerCode));
                }
                else
                {
                    query = query.Where(x => x.o.PartnerCode == Partner);
                }
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                query = query.Where(x => x.o.OrdersCode == SearchText.Trim()
                       || x.cus.StoreCode.Contains(SearchText.ToLower().Trim())
                       || x.cus.SalonPhone.Contains(SearchText.ToLower().Trim())
                       || x.cus.SalonEmail.Contains(SearchText.ToLower().Trim())
                       || x.cus.BusinessName.Contains(SearchText.ToLower().Trim())
                       || x.t.MxMerchant_id.Contains(SearchText.ToLower().Trim()));
            }
            if (!string.IsNullOrEmpty(PaymentType))
            {
                switch (PaymentType)
                {
                    case "PaymentGateway":
                        query = query.Where(x => x.o.PaymentMethod == "CreditCard" || x.t.PaymentMethod == "CreditCard");
                        break;
                    case "FOC":
                        query = query.Where(x => x.o.PaymentMethod == "FOC" || x.t.PaymentMethod == "FOC");
                        break;
                    case "Other":
                        query = query.Where(x => !string.IsNullOrEmpty(x.o.PaymentMethod) && x.o.PaymentMethod != "FOC" && x.o.PaymentMethod != "CreditCard" && x.t.PaymentMethod != "FOC" && x.t.PaymentMethod != "CreditCard");
                        break;
                    default:
                        break;
                }
            }

            if (!string.IsNullOrEmpty(Status))
            {
                if (Status == "success")
                {
                    query = query.Where(x => x.t.PaymentStatus.Contains(Status) || x.t.PaymentStatus.Contains("Approved"));
                }
                else if (Status == "pending")
                {
                    query = query.Where(x => x.t.PaymentStatus.Contains(Status) || x.t.PaymentStatus.Contains("Waiting"));
                }
                else
                {
                    query = query.Where(x => x.t.PaymentStatus.Contains(Status) || x.t.PaymentStatus.Contains("Declined"));
                }
            }
            var colSearch = dataTable.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch.Name)
            {
                case "CreateAt":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.t.CreateAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.t.CreateAt);
                    }

                    break;
                case "BusinessName":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.cus.BusinessName);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.cus.BusinessName);
                    }

                    break;
                case "OrdersCode":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.t.OrdersCode);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.t.OrdersCode);
                    }

                    break;
                case "PaymentMethod":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.o.PaymentMethod);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.o.PaymentMethod);
                    }
                    break;
                case "Amount":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.t.Amount);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.t.Amount);
                    }
                    break;
                case "CardNumber":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.t.CardNumber);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.t.CardNumber);
                    }
                    break;
                case "PaymentStatus":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.t.PaymentStatus);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.t.PaymentStatus);
                    }
                    break;
                case "MxMerchant_id":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.t.MxMerchant_id);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.t.MxMerchant_id);
                    }
                    break;
                case "ResponseText":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(s => s.t.ResponseText);
                    }
                    else
                    {
                        query = query.OrderByDescending(s => s.t.ResponseText);
                    }
                    break;
                default:
                    query = query.OrderByDescending(s => s.t.CreateAt);
                    break;
            }
            var totalRecord = query.Count();
            var totalAmount = totalRecord > 0 ? query.Sum(x => x.t.Amount) : decimal.Zero;
            var ViewData = query.Skip(dataTable.Start).Take(dataTable.Length).ToList().Select(x => new
            {
                x.t.Id,
                x.t.MxMerchant_id,
                x.t.PaymentStatus,
                Amount = x.t.Amount < 0 ? string.Format("-${0:#,0.00}", Math.Abs(x.t.Amount)) : string.Format("${0:#,0.00}", x.t.Amount),
                x.t.BankName,
                x.cus.CustomerCode,
                x.o.OrdersCode,
                x.cus.StoreCode,
                x.cus.BusinessName,
                CustomerId = x.cus.Id,
                PaymentMethod = x.t.PaymentMethod ?? x.o.PaymentMethod,
                Address = x.cus.AddressLine(),
                PaymentNote = x.t.PaymentNote ?? x.o.PaymentNote,
                x.t.CardNumber,
                CreateAt = x.t.CreateAt.HasValue ? string.Format("{0:r}", x.t.CreateAt) : string.Empty,
                x.t.CreateBy,
                x.t.ResponseText,
                x.cus.PartnerCode,
                x.cus.PartnerName,
                x.t.UpdateDescription
            });
            return Json(new
            {
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord,
                totalAmount = string.Format("${0:#,0.00}", totalAmount),
                draw = dataTable.Draw,
                data = ViewData
            });
        }
        #endregion

        public async Task<FileStreamResult> ExportGiftcardToExcel(string Key)
        {
            try
            {
                var SService = db.Store_Services.Find(Key);
                if (SService == null) throw new Exception();
                var Customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == SService.CustomerCode);
                if (Customer == null) throw new Exception();
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });
                var Giftcards = (from gc in db.C_CustomerGiftcard.Where(c => c.StoreServiceId == SService.Id)
                                 join od in db.O_Orders on gc.OrderCode equals od.OrdersCode
                                 where od.Status != InvoiceStatus.Canceled.ToString()
                                 select gc
                                 ).ToList();

                //view all sales lead if permission is view all
                string webRootPath = "/Upload/Merchant/Template";
                string templateName = @"Template_Giftcard.xlsx";
                string tempPath = "/upload/other/";
                string tempFile = @"TempData.xlsx";

                var memoryStream = new MemoryStream();

                using (var fstream = new FileStream(Server.MapPath(Path.Combine(tempPath, tempFile)), FileMode.Create, FileAccess.Write))
                {
                    var fs = System.IO.File.OpenRead(Server.MapPath(Path.Combine(webRootPath, templateName)));
                    IWorkbook fs_workbook = new XSSFWorkbook(fs);
                    ISheet fs_sheet = fs_workbook.GetSheetAt(0);

                    //style Bold
                    IFont font1 = fs_workbook.CreateFont();
                    font1.IsBold = true;
                    ICellStyle style1 = fs_workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    //style center
                    ICellStyle style2 = fs_workbook.CreateCellStyle();
                    style2.VerticalAlignment = VerticalAlignment.Center;
                    style2.Alignment = HorizontalAlignment.Center;
                    //style right
                    ICellStyle style3 = fs_workbook.CreateCellStyle();
                    style3.VerticalAlignment = VerticalAlignment.Center;
                    style3.Alignment = HorizontalAlignment.Right;
                    //add info salon
                    fs_sheet.GetRow(1).CreateCell(3).SetCellValue(Customer.StoreCode);
                    fs_sheet.GetRow(1).GetCell(3).CellStyle = style1;
                    fs_sheet.GetRow(2).CreateCell(3).SetCellValue(Customer.BusinessName);
                    fs_sheet.GetRow(2).GetCell(3).CellStyle = style1;
                    fs_sheet.GetRow(3).CreateCell(3).SetCellValue(SService.OrderCode);
                    fs_sheet.GetRow(3).GetCell(3).CellStyle = style1;
                    fs_sheet.GetRow(4).CreateCell(3).SetCellValue(DateTime.UtcNow.ToString("MMMM dd, yyyy hh:mm UTC"));
                    fs_sheet.GetRow(4).GetCell(3).CellStyle = style1;

                    var QRCodes = string.Join(",", Giftcards.Select(c => c.QRCode).ToList()).Split(',').ToList();
                    var Printeds = string.Join(",", Giftcards.Select(c => c.Printed).ToList()).Split(',').ToList();
                    int startRow = 8;
                    for (var i = 0; i < QRCodes.Count; i++)
                    {
                        fs_sheet.CreateRow(startRow + i).CreateCell(0).SetCellValue(i + 1);
                        fs_sheet.GetRow(startRow + i).GetCell(0).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).CreateCell(1).SetCellValue(QRCodes[i]);
                        fs_sheet.GetRow(startRow + i).CreateCell(2).SetCellValue(Printeds[i]);
                        fs_sheet.GetRow(startRow + i).GetCell(1).CellStyle = style3;
                        fs_sheet.GetRow(startRow + i).GetCell(2).CellStyle = style3;
                    }
                    fs_workbook.Write(fstream);
                    fs.Close();
                    fstream.Close();
                }

                using (var fileStream = new FileStream(Server.MapPath(Path.Combine(tempPath, tempFile)), FileMode.OpenOrCreate))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }

                memoryStream.Position = 0;
                memoryStream = new MemoryStream(memoryStream.ToArray());
                string _fileName = DateTime.UtcNow.ToString("yyyyMMddhhmmss") + "_GiftcardList_" + Customer.StoreCode + ".xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<ActionResult> ExportMerchantToExcel(List<int> Sites, List<string> AccountManager, List<int> MerchantStatus /*int? LicenseStatus, int? TerminalStatus*/, int? Remaining, int? number_Remaining, string Type, int? NODaysCreated, int? custom_NODaysCreated, string AtRisk, int? ServiceType, string SearchText, string State, string City, string ZipCode, string Processor)
        {
            try
            {
                var response = await this.GetMerchantFromUniversal(dataTable: null, siteIds: Sites, accountManager: AccountManager, merchantStatus: MerchantStatus /*licenseStatus: LicenseStatus, terminalStatus: TerminalStatus*/, remainingDate: Remaining, number_Remaining: number_Remaining, type: Type, NODaysCreated: NODaysCreated, custom_NODaysCreated: custom_NODaysCreated, atRisk: AtRisk, serviceType: ServiceType, searchText: SearchText, state: State, city: City, zipCode: ZipCode, processor: Processor, getAll: true);
                //view all sales lead if permission is view all
                string webRootPath = "/Upload/Merchant/Template";
                string templateName = @"Template_Merchant_List.xlsx";
                string tempPath = "/upload/other/";
                //string tempFile = @"TempData.xlsx";
                string fileName = "Merchant" + DateTime.UtcNow.ToString("yyyyMMddhhmmssff") + ".xlsx";
                var memoryStream = new MemoryStream();
                using (var fstream = new FileStream(Server.MapPath(Path.Combine(tempPath, fileName)), FileMode.Create, FileAccess.Write))
                {
                    var fs = System.IO.File.OpenRead(Server.MapPath(Path.Combine(webRootPath, templateName)));
                    IWorkbook fs_workbook = new XSSFWorkbook(fs);
                    ISheet fs_sheet = fs_workbook.GetSheetAt(0);

                    //style Bold
                    IFont font1 = fs_workbook.CreateFont();
                    font1.IsBold = true;
                    ICellStyle style1 = fs_workbook.CreateCellStyle();
                    style1.SetFont(font1);
                    //style center
                    ICellStyle style2 = fs_workbook.CreateCellStyle();
                    style2.VerticalAlignment = VerticalAlignment.Center;
                    style2.Alignment = HorizontalAlignment.Left;
                    style2.WrapText = true;
                    //style right
                    //ICellStyle style3 = fs_workbook.CreateCellStyle();
                    //style3.VerticalAlignment = VerticalAlignment.Center;
                    //style3.Alignment = HorizontalAlignment.Right;
                    //add info salon
                    fs_sheet.GetRow(1).CreateCell(3).SetCellValue(DateTime.UtcNow.ToString("MMMM dd, yyyy hh:mm UTC"));
                    fs_sheet.GetRow(1).GetCell(3).CellStyle = style1;

                    int startRow = 8;
                    var allEnum = db.EnumValues.AsNoTracking().ToList();
                    var i = 0;
                    var merchants = response.Records.ToList();

                    var deliveryTypeBuildInCode = BuildInCodeProject.Deployment_Ticket.ToString();
                    var deliveryProject = db.T_Project_Milestone.FirstOrDefault(x => x.BuildInCode == deliveryTypeBuildInCode);
                    var now = DateTime.UtcNow.Date;
                    foreach (var merchant in merchants)
                    {
                        //salon code
                        fs_sheet.CreateRow(startRow + i).CreateCell(0).SetCellValue(merchant.StoreCode);
                        //salon name
                        fs_sheet.GetRow(startRow + i).CreateCell(1).SetCellValue(merchant.BusinessName);
                        fs_sheet.GetRow(startRow + i).CreateCell(2).SetCellValue(merchant.SalonPhone);
                        fs_sheet.GetRow(startRow + i).CreateCell(3).SetCellValue(merchant.SalonEmail);
                        fs_sheet.GetRow(startRow + i).CreateCell(4).SetCellValue(merchant.ContactName);
                        fs_sheet.GetRow(startRow + i).CreateCell(5).SetCellValue(merchant.CellPhone);
                        fs_sheet.GetRow(startRow + i).CreateCell(6).SetCellValue(merchant.Email);
                        fs_sheet.GetRow(startRow + i).CreateCell(7).SetCellValue(merchant.SalonNote);
                        var startedDate = merchant.CreateAt.Value.UtcToIMSDateTime().ToString("MMM dd,yyyy");
                        fs_sheet.GetRow(startRow + i).CreateCell(8).SetCellValue(startedDate);
                        fs_sheet.GetRow(startRow + i).CreateCell(9).SetCellValue(merchant.License?.LicenseName);
                        if (merchant.License?.LicenseStatus != null)
                        {
                            var LicenseStatusName = "";
                            Enum.TryParse(merchant.License.LicenseStatus.ToString(), out LicenseStatusUniversal licenseStatusEnum);
                            LicenseStatusName = licenseStatusEnum.ToString();
                            if (LicenseStatusName == LicenseStatusUniversal.Active.ToString())
                            {
                                LicenseStatusName = LicenseStatusName + $"({merchant.License?.RemainingDate} days)";
                            }
                            fs_sheet.GetRow(startRow + i).CreateCell(10).SetCellValue(LicenseStatusName);
                        }

                        //life time
                        var LifeTime = (DateTime.UtcNow - merchant.CreateAt.Value).Days;
                        fs_sheet.GetRow(startRow + i).CreateCell(11).SetCellValue(LifeTime);

                        // cancel date
                        fs_sheet.GetRow(startRow + i).CreateCell(12).SetCellValue(merchant.CancelDate?.ToString("MMM dd, yyyy"));

                        // service type
                        if (merchant.ServiceType > 0)
                        {
                            var ServiceTypeName = "";
                            Enum.TryParse(merchant.ServiceType.ToString(), out ServiceType ServiceTypeEnum);
                            ServiceTypeName = ServiceTypeEnum.ToString();
                            fs_sheet.GetRow(startRow + i).CreateCell(13).SetCellValue(ServiceTypeName);
                        }
                        //MID
                        fs_sheet.GetRow(startRow + i).CreateCell(14).SetCellValue(merchant.MID);

                        // terminal status

                        if (merchant.TerminalStatus > 0)
                        {
                            var TerminalName = "";
                            Enum.TryParse(merchant.TerminalStatus.ToString(), out TerminalStatus terminalStatusEnum);
                            TerminalName = terminalStatusEnum.ToString();
                            fs_sheet.GetRow(startRow + i).CreateCell(15).SetCellValue(TerminalName);
                        }


                        if (merchant.TerminalType > 0)
                        {
                            fs_sheet.GetRow(startRow + i).CreateCell(16).SetCellValue(allEnum.FirstOrDefault(x => x.Value == merchant.TerminalType.ToString() && x.Namespace == Utils.IEnums.MerchantOptionEnum.TerminalType.Code<string>())?.Name);
                        }
                        if (merchant.Processor > 0)
                        {
                            fs_sheet.GetRow(startRow + i).CreateCell(17).SetCellValue(allEnum.FirstOrDefault(x => x.Value == merchant.Processor.ToString() && x.Namespace == Utils.IEnums.MerchantOptionEnum.Processor.Code<string>())?.Name);
                        }
                        if (merchant.Source > 0)
                        {
                            fs_sheet.GetRow(startRow + i).CreateCell(18).SetCellValue(allEnum.FirstOrDefault(x => x.Value == merchant.Source.ToString() && x.Namespace == Utils.IEnums.MerchantOptionEnum.Source.Code<string>())?.Name);
                        }

                        //subscription amount
                        fs_sheet.GetRow(startRow + i).CreateCell(19).SetCellValue(merchant.SubscriptionAmount ?? 0);

                        //merchant status
                        string merchantStatus = "";
                        if (merchant.License?.RemainingDate >= 0 && !string.IsNullOrEmpty(merchant.MID))
                        {
                            merchantStatus = MerchantStatusSearch.LicenseTerminal.Code<string>();
                        }
                        else if (merchant.License?.RemainingDate >= 0)
                        {
                            merchantStatus = MerchantStatusSearch.OnlyLicense.Code<string>();
                        }
                        else if (!string.IsNullOrEmpty(merchant.MID))
                        {
                            merchantStatus = MerchantStatusSearch.OnlyTerminal.Code<string>();
                        }
                        else if (merchant.CancelDate != null)
                        {
                            merchantStatus = MerchantStatusSearch.Cancel.Code<string>();
                        }
                        fs_sheet.GetRow(startRow + i).CreateCell(20).SetCellValue(merchantStatus);

                        var package = db.Store_Services.Where(x => x.StoreCode == merchant.StoreCode && x.Active == 1 && (x.RenewDate == null || x.RenewDate >= now)).ToList();

                        if (package.Count > 0)
                        {
                            var packageNames = string.Join("\n", package.OrderByDescending(x => x.Type == "license").Select(x => x.Productname + " x" + (x.Quantity ?? 1)));
                            var packageCell = fs_sheet.GetRow(startRow + i).CreateCell(21);
                            packageCell.SetCellValue(packageNames);
                            packageCell.CellStyle.WrapText = true;
                        }
                        //ticket deployment
                        var ticketDeploymentHaveAssign = db.T_SupportTicket.Where(x => x.CustomerCode == merchant.CustomerCode && (x.ProjectId == deliveryProject.Id || x.Name.Contains("Deployment")) && !string.IsNullOrEmpty(x.AssignedToMemberName)).Select(x => x.AssignedToMemberNumber).ToList();
                        if (ticketDeploymentHaveAssign != null)
                        {
                            var memberNumbers = string.Join(",", ticketDeploymentHaveAssign).Split(',').Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                            var memberNames = db.P_Member.Where(x => memberNumbers.Any(y => y == x.MemberNumber)).Select(x => x.FullName);
                            fs_sheet.GetRow(startRow + i).CreateCell(22).SetCellValue(string.Join(",", memberNames));
                        }
                        i++;
                    }
                    fs_workbook.Write(fstream);
                    fs.Close();
                    fstream.Close();
                }

                using (var fileStream = new FileStream(Server.MapPath(Path.Combine(tempPath, fileName)), FileMode.OpenOrCreate))
                {
                    await fileStream.CopyToAsync(memoryStream);
                    fileStream.Close();
                }

                memoryStream.Position = 0;
                memoryStream = new MemoryStream(memoryStream.ToArray());
                // string _fileName = "MerchantList_" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".xlsx";
                var path = Server.MapPath(Path.Combine(tempPath, fileName));
                //  var path = Server.MapPath(Path.Combine(webRootPath, fileName));
                return Json(new { status = true, path = path });
                // return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //public string DeactiveAllRecurring()
        //{
        //    try
        //    {
        //        if (!cMem.RoleCode.Contains("admin"))
        //        {
        //            return "You don't have permission.";
        //        }
        //        var msg = PaymentService.DeactiveAllRecurring();
        //        return msg;
        //    }
        //    catch (Exception ex)
        //    {
        //        return "Error: " + ex.Message;
        //    }
        //}
        [HttpPost]
        public JsonResult ShowCardNumber(string Id, string Hiddenkey)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("customer_view_cardnumber")) != true || access["customer_view_cardnumber"] != true)
                {
                    throw new Exception("You don't have permission");
                }
                string cardNumberhash = string.Empty;
                string timestamps = string.Empty;
                var card = db.C_CustomerCard.Find(Id);
                var pcard = new C_PartnerCard();
                if (card == null)
                {
                    pcard = db.C_PartnerCard.Find(Id);
                    cardNumberhash = pcard?.MerchantCardReference;
                    timestamps = pcard?.CreateAt.Value.Ticks.ToString();
                }
                else
                {
                    cardNumberhash = card.MerchantCardReference;
                    timestamps = card.CreateAt.Value.Ticks.ToString();
                }
                var cardNumber = CardSecurity.DecryptionCardNumber(cardNumberhash, timestamps, Hiddenkey);
                return Json(new object[] { true, cardNumber });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error:" + e.Message });
            }
        }

        [HttpPost]
        public ActionResult GetRemainingDate(string CustomerCode)
        {
            var customer = db.C_Customer.FirstOrDefault(x => x.CustomerCode == CustomerCode);
            var sub = db.Store_Services.Where(s => s.Active == 1 && s.Type == "license" && s.CustomerCode == CustomerCode).FirstOrDefault();
            DateTime startDate = sub?.EffectiveDate?.Date > DateTime.UtcNow.Date ? sub.EffectiveDate.Value : DateTime.UtcNow;
            ViewBag.CustomerCode = customer.CustomerCode;
            ViewBag.CancelDate = customer.CancelDate;
            ViewBag.EffectiveDate = sub?.EffectiveDate;
            ViewBag.Remaning = CommonFunc.FormatNumberRemainDate((sub?.RenewDate != null) ? (int?)(sub?.RenewDate.Value.Date - startDate.Date).Value.Days : null);
            ViewBag.DueDate = db.O_Orders.Where(c => c.CustomerCode == CustomerCode && c.Status == "PaymentLater" &&
                                        db.Store_Services.Any(s => s.CustomerCode == CustomerCode &&
                                        (c.OrdersCode == s.OrderCode || c.OrdersCode == s.LastRenewOrderCode) &&
                                        s.Active == 1 && s.Type == "license")).OrderBy(c => c.DueDate).FirstOrDefault()?.DueDate;

            return PartialView("_RemainingDate");
        }

        [HttpPost]
        public ActionResult MarkMerchantIsCanceled(string CustomerCode)
        {
            var customer = db.C_Customer.FirstOrDefault(x => x.CustomerCode == CustomerCode);
            customer.CancelDate = DateTime.UtcNow;
            db.SaveChanges();
            return Json(true);
        }


        [HttpGet]
        public JsonResult UpdateToLifeTimeMAC()
        {
            try
            {
                if (!cMem.RoleCode.Contains("admin"))
                {
                    _logService.Info($"[UpdateToLifeTimeMAC] You dont have permission");
                    return Json(new object[] { "You dont have permission" }, JsonRequestBehavior.AllowGet);
                }
                _logService.Info($"[UpdateToLifeTimeMAC] Start function update");
                var limitDate = new DateTime(2099, 1, 1);
                var macs = from customer in db.C_Customer.Where(c => c.SiteId == 2)
                           join store in db.Store_Services.Where(c => c.Active == 1 && c.Type == "license" && c.RenewDate < limitDate)
                           on customer.CustomerCode equals store.CustomerCode
                           join subscription in db.Order_Subcription
                           on new { s1 = store.CustomerCode, s2 = store.OrderCode } equals new { s1 = subscription.CustomerCode, s2 = subscription.OrderCode }
                           where store != null
                           orderby store.CustomerCode descending
                           select new { store, subscription };
                int countSuccess = 0;
                int total = 0;
                var storeErrors = new List<string>();

                _logService.Info($"[UpdateToLifeTimeMAC] Update {macs.Count()} customers");
                using (MerchantService service = new MerchantService(true))
                {
                    foreach (var item in macs)
                    {
                        total++;
                        try
                        {
                            service.SyncLifeTimeToPos(item.store, item.subscription);
                            countSuccess++;
                        }
                        catch (Exception ex)
                        {
                            storeErrors.Add(item.store.CustomerCode);
                            _logService.Error(ex, $"[UpdateToLifeTimeMAC] ERROR store {item.store.StoreCode}");
                        }
                    }
                }
                _logService.Info($"[UpdateToLifeTimeMAC] End {total} total, {countSuccess} success, {storeErrors.Count} error", new { storeErrors = string.Join(", ", storeErrors) });

                return Json(new object[] { $"End {total} total, {countSuccess} success, {storeErrors.Count} error", string.Join(", ", storeErrors) }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[UpdateToLifeTimeMAC] ERROR");
                return Json(new object[] { "Error:" + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public async Task<string> GetRecurringCard(string customerCode)
        {
            try
            {
                if (string.IsNullOrEmpty(customerCode)) throw new Exception();
                return await _enrichUniversalService.GetRecurringCardAsync(customerCode);
            }
            catch (Exception)
            {
                return "N/A";
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetMerchantSummariesAsync()
        {
            try
            {
                var summaries = await _enrichUniversalService.GetMerchantSummariesAsync();
                return Json(new object[] { true, summaries }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ReportCustomerForChart(CustomerChartReportRequest request)
        {
            try
            {
                var summaries = await _enrichUniversalService.ReportCustomerForChartAsync(request);
                return Json(new object[] { true, summaries });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }

        //[HttpGet]
        //public async Task<JsonResult> GetTimeLine(string storeCode)rr
        //{
        //    try
        //    {
        //        var response = await _enrichSMSService.GetTimeLineAsync(storeCode);
        //        return Json(new object[] { true, response }, JsonRequestBehavior.AllowGet);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new object[] { false, ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        #region Import data
        [HttpPost]
        public ActionResult ProcessImportExcelFile(HttpPostedFileBase file)
        {
            // try
            // {
            _logService.Info($"[MerchantMan][ImportExcel] start import excel file");
            if (file != null && file.ContentLength > 0)
            {
                _logService.Info($"[MerchantMan][ImportExcel] start process file name {file.FileName}");
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                var fileExtension = Path.GetExtension(file.FileName);
                var path = Path.Combine(Server.MapPath("~/upload/other"), fileName + DateTime.UtcNow.ToString("yyyyMMddhhmmssffff") + fileExtension);
                file.SaveAs(path);
                ExcelPackage.LicenseContext = LicenseContext.Commercial;

                int totalRowImport = 0;
                int totalSuccess = 0;
                int totalFailed = 0;
                int statusMessage = 1;
                using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

                    int startRow = 2; // Skip the header row
                    int endRow = worksheet.Dimension.End.Row;
                    int storeNameColumnIndex = 1;
                    int storeCodeColumnIndex = 2;
                    int salesPersonColumnIndex = 3;
                    int accountManagerColumnIndex = 4;
                    int goLiveDateColumnIndex = 5;
                    int cancelDateColumnIndex = 6;
                    int midColumnIndex = 7;
                    int processingPartnerColumnIndex = 8;
                    int posStructureColumnIndex = 9;
                    int deviceNoteColumnIndex = 10;
                    int deviceSetupStructureColumnIndex = 11;
                    int managerPhoneColumnIndex = 12;
                    string defaultNullString = "N/A";
                    var excelMerchantData = new List<MerchantImportData>();

                    string messageFailed = "";
                    int importSuccessRow = 0;
                    int importFailedRow = 0;
                    // status message to noty , 1 success all, 2 warning
                    string storeCodeLog = "";
                    for (int row = startRow; row <= endRow; row++)
                    {
                        try
                        {
                            string storeName = worksheet.Cells[row, storeNameColumnIndex].Value?.ToString();
                            string storeCode = worksheet.Cells[row, storeCodeColumnIndex].Value?.ToString();
                            storeCodeLog = storeCode;
                            if (string.IsNullOrEmpty(storeCode))
                                continue;
                            string salesPerson = worksheet.Cells[row, salesPersonColumnIndex].Value?.ToString();
                            string accountManager = worksheet.Cells[row, accountManagerColumnIndex].Value?.ToString();
                            string goLiveDateString = worksheet.Cells[row, goLiveDateColumnIndex].Value?.ToString();
                            string cancelDateString = worksheet.Cells[row, cancelDateColumnIndex].Value?.ToString();
                            DateTime? goLiveDate = null;
                            DateTime? cancelDate = null;
                            if (!string.IsNullOrEmpty(goLiveDateString) && goLiveDateString.ToUpper() != defaultNullString)
                            {
                                goLiveDate = DateTime.Parse(goLiveDateString);
                            }

                            if (!string.IsNullOrEmpty(cancelDateString) && cancelDateString.ToUpper() != defaultNullString)
                            {
                                cancelDate = DateTime.Parse(cancelDateString);
                            }

                            string mid = worksheet.Cells[row, midColumnIndex].Value?.ToString();
                            string processingPartner = worksheet.Cells[row, processingPartnerColumnIndex].Value?.ToString();
                            string posStructure = worksheet.Cells[row, posStructureColumnIndex].Value?.ToString();
                            string deviceNote = worksheet.Cells[row, deviceNoteColumnIndex].Value?.ToString();
                            string deviceSetupStructure = worksheet.Cells[row, deviceSetupStructureColumnIndex].Value?.ToString();
                            string managerPhone = worksheet.Cells[row, managerPhoneColumnIndex].Value?.ToString();

                            //add item to data import
                            excelMerchantData.Add(new MerchantImportData
                            {
                                StoreCode = storeCode,
                                SalePerson = salesPerson,
                                AccountManager = accountManager,
                                GoliveDate = goLiveDate,
                                CancelDate = cancelDate,
                                MID = mid,
                                ProcessingPartner = processingPartner,
                                POSStructure = posStructure,
                                DeviceNote = deviceNote,
                                DeviceNameStructure = deviceSetupStructure,
                                ManagerPhone = managerPhone
                            });
                            importSuccessRow++;
                        }
                        catch (Exception ex)
                        {
                            statusMessage = 2;
                            importFailedRow++;
                            messageFailed += $"<br/> import failed in row {row}";
                        }
                        totalRowImport++;
                    }
                    if (excelMerchantData.Count == 0)
                        return Json(true);
                    var allMembers = db.P_Member.AsNoTracking().ToList();
                    string ProcessorNameSpace = Utils.IEnums.MerchantOptionEnum.Processor.Code<string>();
                    string PosStructureNameSpace = Utils.IEnums.MerchantOptionEnum.POSStructure.Code<string>();
                    string DeviceNameNameSpace = Utils.IEnums.MerchantOptionEnum.DeviceName.Code<string>();
                    string DeviceSetupStructureNameSpace = Utils.IEnums.MerchantOptionEnum.DeviceSetupStructure.Code<string>();
                    var processorsList = db.EnumValues.AsNoTracking().Where(x => x.Namespace == ProcessorNameSpace).ToList();
                    var posstructureList = db.EnumValues.AsNoTracking().Where(x => x.Namespace == PosStructureNameSpace).ToList();
                    var deviceNameList = db.EnumValues.AsNoTracking().Where(x => x.Namespace == DeviceNameNameSpace).ToList();
                    var deviceSetupStructureList = db.EnumValues.AsNoTracking().Where(x => x.Namespace == DeviceSetupStructureNameSpace).ToList();

                    foreach (var dataItem in excelMerchantData)
                    {
                        try
                        {
                            _logService.Info($"[MerchantMan][ImportExcel] start update excel merchant data {JsonConvert.SerializeObject(dataItem)}");
                            var merchant = db.C_Customer.Where(x => x.StoreCode == dataItem.StoreCode).FirstOrDefault();
                            if (merchant == null)
                            {
                                totalFailed++;
                                continue;
                            }


                            var salesLead = db.C_SalesLead.Where(x => x.CustomerCode == merchant.CustomerCode).FirstOrDefault();

                            if (!string.IsNullOrEmpty(dataItem.SalePerson) && dataItem.SalePerson.ToUpper() == defaultNullString)
                            {
                                var salesPerson = allMembers.FirstOrDefault(x => x.FullName.Trim().ToLower() == dataItem.SalePerson.Trim().ToLower());
                                if (salesPerson != null)
                                {
                                    salesLead.MemberName = salesPerson.FullName;
                                    salesLead.MemberNumber = salesPerson.MemberNumber;
                                }
                            }
                            else
                            {

                                salesLead.MemberName = null;
                                salesLead.MemberNumber = null;
                            }

                            if (!string.IsNullOrEmpty(dataItem.AccountManager) && dataItem.AccountManager.ToUpper() != defaultNullString)
                            {
                                var accountManager = allMembers.FirstOrDefault(x => x.FirstName.Trim().ToLower() == dataItem.AccountManager.Trim().ToLower());
                                if (accountManager != null)
                                {
                                    merchant.FullName = accountManager.FullName;
                                    merchant.MemberNumber = accountManager.MemberNumber;
                                }
                            }
                            else
                            {

                                merchant.MemberNumber = null;
                                merchant.FullName = null;
                            }

                            merchant.GoLiveDate = dataItem.GoliveDate;
                            merchant.CancelDate = dataItem.CancelDate;
                            merchant.MID = dataItem.MID;
                            if (!string.IsNullOrEmpty(merchant.MID))
                                merchant.TerminalStatus = true;
                            merchant.ManagerPhone = dataItem.ManagerPhone;

                            if (!string.IsNullOrEmpty(dataItem.ProcessingPartner) && dataItem.ProcessingPartner.ToUpper() != defaultNullString)
                            {
                                var processor = processorsList.FirstOrDefault(x => x.Name.Trim().ToLower() == dataItem.ProcessingPartner.Trim().ToLower());
                                if (processor != null)
                                {
                                    merchant.Processor = processor.Value;
                                }
                            }
                            else
                            {
                                merchant.Processor = null;
                            }

                            if (!string.IsNullOrEmpty(dataItem.POSStructure) && dataItem.POSStructure.ToUpper() != defaultNullString)
                            {
                                var posstruc = posstructureList.FirstOrDefault(x => x.Name.Trim().ToLower() == dataItem.POSStructure.Trim().ToLower());
                                if (posstruc != null)
                                {
                                    merchant.POSStructure = posstruc.Value;
                                }
                            }
                            else
                            {
                                merchant.POSStructure = null;
                            }


                            if (!string.IsNullOrEmpty(dataItem.DeviceNote) && dataItem.DeviceNote.ToUpper() != defaultNullString)
                            {
                                merchant.DeviceNote = dataItem.DeviceNote;
                            }
                            else
                            {
                                merchant.DeviceNote = null;
                            }

                            if (!string.IsNullOrEmpty(dataItem.DeviceNameStructure) && dataItem.DeviceNameStructure.ToUpper() != defaultNullString)
                            {
                                var deviceNameStructure = deviceSetupStructureList.FirstOrDefault(x => x.Name.Trim().ToLower() == dataItem.DeviceNameStructure.Trim().ToLower());
                                if (deviceNameStructure != null)
                                {
                                    merchant.DeviceSetupStructure = deviceNameStructure.Value;
                                }
                            }
                            else
                            {
                                merchant.DeviceSetupStructure = null;
                            }
                            db.SaveChanges();
                            _logService.Info($"[MerchantMan][ImportExcel]  update success merchant data {JsonConvert.SerializeObject(dataItem)}");
                            totalSuccess++;
                        }
                        catch (Exception ex)
                        {
                            statusMessage = 2;
                            _logService.Error($"[MerchantMan][ImportExcel] process a merchant failed {JsonConvert.SerializeObject(dataItem)}", ex);
                            totalFailed++;
                        }
                    }
                }
                _logService.Info($"[MerchantMan][ImportExcel] end process import with result: total import: {totalRowImport} , success: {totalSuccess}, failed: {totalFailed}");
                return Json(new { success = true, status = statusMessage, message = $"Update data from import file success, total import: {totalRowImport} , success: {totalSuccess}, failed: {totalFailed}" });
            }
            else
            {
                _logService.Info($"[MerchantMan][ImportExcel]  upload failed: no file selected");
                return Json(new { success = false, message = "No file selected." });
            }


            //}
            //catch (Exception ex)
            //{
            //    _logService.Error($"[MerchantMan][ImportExcel] process import merchant failed", ex);
            //    return Json(new { success = false, message = ex.Message });
            //}
        }


        #endregion

        #region SyncDataToPos
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> SyncStoreToPosAsync(string StoreCode)
        {
            var customer = db.C_Customer.Where(x => x.StoreCode == StoreCode).FirstOrDefault();
            if (customer == null) return Content("store not found");
            var isExistLicenseActive = db.Store_Services.Where(x => x.StoreCode == customer.StoreCode && x.Type == "license" && x.Active == 1 && DbFunctions.TruncateTime(x.RenewDate) >= DateTime.UtcNow).Count() > 0;
            if (isExistLicenseActive)
            {
                using (MerchantService service = new MerchantService(true))
                {
                    // var rs = await service.ApproveAction(editcustomer.StoreCode, active_service_id, true, "same-active");
                    StoreProfileReq storeProfile = service.GetStoreProfileReady(null, false, null, customer.StoreCode);
                    service.DoRequest(storeProfile);
                }

            }
            return Content("update success");
        }
		#endregion
	}
}