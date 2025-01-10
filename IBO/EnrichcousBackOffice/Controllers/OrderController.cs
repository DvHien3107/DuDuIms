using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Xml;
using DataTables.AspNet.Core;
using Enrich.Core.Ultils;
using Enrich.DataTransfer;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Enrichcous.Payment.Mxmerchant.Config.Enums;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Services.NextGen.Mail;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.ViewModel;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using SelectPdf;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        private P_Member cMem = Authority.GetCurrentMember();
        const string LIST_PRODUCT_SERVICE = "List product service";
        const string TOTAL_MONEY_ORDER = "Total money order";
        const string LIST_BUNDLE_DEVICE = "List bundle device";
        const string INVOICE_LIST = "InvoiceList";
        private readonly ISMSService _sMSService;
        private readonly ILogService _logService;
        private readonly IMailingService _mailingService;
        private readonly EnrichContext _enrichContext;
        public OrderController(ISMSService sMSService, ILogService logService, IMailingService mailingService, EnrichContext enrichContext)
        {
            _sMSService = sMSService;
            _logService = logService;
            _mailingService = mailingService;
            _enrichContext = enrichContext;
        }

        // GET: Order
        public ActionResult Index()
        {
            _logService.Info($"[Invoice][Index]  ");
            try
            {
                WebDataModel db = new WebDataModel();


                // add to view history top button
                UserContent.TabHistory = "Invoice" + "|/order/";

                if (string.IsNullOrWhiteSpace(Request.Url.Query) == true)
                {
                    //TempData.Clear();
                    TempData.Remove("search_name"); TempData.Remove("search_from_date"); TempData.Remove("search_to_date");
                }
                if (Request["search_submit"] != null)
                {
                    TempData["search_name"] = Request["search_name"];
                    TempData["search_status"] = Request["search_status"];
                    //TempData["search_from_date"] = Request["search_from_date"];
                    //TempData["search_to_date"] = Request["search_to_date"];
                }

                string search_name = TempData["search_name"] == null ? "" : TempData["search_name"].ToString();
                string search_status = TempData["search_status"] == null ? "" : TempData["search_status"].ToString();
                //string search_from_date = TempData["search_from_date"] == null ? DateTime.Today.AddMonths(-3).Month + "/1/" + DateTime.Today.AddMonths(-3).Year : TempData["search_from_date"].ToString();
                //string search_to_date = TempData["search_to_date"] == null ? DateTime.Today.ToShortDateString() : TempData["search_to_date"].ToString();
                TempData["search_name"] = search_name;
                //TempData["search_from_date"] = search_from_date;
                //TempData["search_to_date"] = search_to_date;
                //DateTime fdate = DateTime.Parse(search_from_date);
                //DateTime tdate = DateTime.Parse(search_to_date + " 23:59:59");
                TempData.Keep();
                var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                ViewBag.p = access;
                ViewBag.Deleted = db.O_Orders.Where(o => o.IsDelete == true || InvoiceStatus.Canceled.ToString().Equals(o.Status)).Count();
                ViewBag.All = db.O_Orders.Where(o => o.IsDelete != true).Count();
                ViewBag.Reality = db.O_Orders.Where(o => (InvoiceStatus.Paid_Wait.ToString().Equals(o.Status) || InvoiceStatus.Closed.ToString().Equals(o.Status)) && o.IsDelete != true).Count();
                ViewBag.Open = db.O_Orders.Where(o => InvoiceStatus.Open.ToString().Equals(o.Status) && o.IsDelete != true).Count();
                ViewBag.Paid_Wait = db.O_Orders.Where(o => InvoiceStatus.Paid_Wait.ToString().Equals(o.Status) && o.IsDelete != true).Count();
                ViewBag.Closed = db.O_Orders.Where(o => InvoiceStatus.Closed.ToString().Equals(o.Status) && o.IsDelete != true).Count();
                ViewBag.Canceled = db.O_Orders.Where(o => InvoiceStatus.Canceled.ToString().Equals(o.Status) && o.IsDelete != true).Count();
                ViewBag.ListMember = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).Select(x => new SelectListItem { Value = x.MemberNumber, Text = x.FullName }).ToList();
                ViewBag.Status = Session["Order_status"]?.ToString() ?? "Open";
                ViewBag.From = Session["Order_from"]?.ToString();
                ViewBag.To = Session["Order_to"]?.ToString();
                ViewBag.TeamSearch = Session["Order_team"]?.ToString();
                ViewBag.SalesPerson = Session["Order_salesperson"]?.ToString();
                ViewBag.SearchText = Session["Order_searchtext"]?.ToString();
                ViewBag.SearchBy = Session["Order_searchby"]?.ToString();
                ViewBag.SearchPartner = Session["Order_searchpartner"]?.ToString();
                ViewBag.FromSystem = Session["Order_fromsystem"]?.ToString();
              
                return View();
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                return View(new List<O_Orders>());
            }
        }
        public ActionResult ChangeTab(string TabName)
        {
            _logService.Info($"[Invoice][ChangeTab] {TabName}");
            WebDataModel db = new WebDataModel();
            // add to view history top button
            UserContent.TabHistory = "Invoice" + "|/order/";


            switch (TabName)
            {

                case "report":
                    var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                    var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                    bool ShowSalesPerson = false;
                    bool ShowTeam = false;
                    if (access.Any(k => k.Key.Equals("report_invoice_allteam")) == true && access["report_invoice_allteam"] == true)
                    {
                        ViewBag.ListMemberFilter = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n)) &&
                                                        (cMem.SiteId == 1 || cMem.SiteId == m.SiteId)).ToList();
                        ShowSalesPerson = true;
                        ShowTeam = true;
                        ViewBag.Team = db.P_Department.Where(d => d.Type == "SALES" && d.Active.Value && d.ParentDepartmentId != null
                                        && (cMem.SiteId == 1 || cMem.SiteId == d.SiteId)).ToList();
                    }
                    else
                    {
                        List<string> listMemberNumber = new List<string>();
                        var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES"
                                        && (cMem.SiteId == 1 || cMem.SiteId == x.SiteId)).ToList();
                        ViewBag.Team = ManagerDep;
                        if (ManagerDep.Count() > 0)
                        {
                            foreach (var dep in ManagerDep)
                            {
                                listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                                ShowSalesPerson = true;
                            }
                            ShowTeam = true;
                        }
                        if (currentDeparmentsUser.Count() > 0)
                        {
                            foreach (var deparment in currentDeparmentsUser)
                            {
                                if (deparment.LeaderNumber == cMem.MemberNumber)
                                {
                                    listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                                    ShowSalesPerson = true;
                                }
                            }
                        }
                        listMemberNumber.Add(cMem.MemberNumber);
                        listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                        ViewBag.ListMemberFilter = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).ToList();
                    }
                    ViewBag.ShowSalesPerson = ShowSalesPerson;
                    ViewBag.ShowTeam = ShowTeam;

                    return PartialView("_ReportTab");
                default:
                    var tmpDepartment = db.P_Department.Where(d => d.Type == "SALES");
                    sale_dep = tmpDepartment.Select(d => d.Id).ToList().Select(d => d.ToString());
                    currentDeparmentsUser = tmpDepartment.Where(d => d.GroupMemberNumber.Contains(cMem.MemberNumber));
                    ShowSalesPerson = false;
                    ShowTeam = false;
                    if (access.Any(k => k.Key.Equals("orders_viewall")) == true && access["orders_viewall"] == true)
                    {
                        ViewBag.ListMemberFilter = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))
                                                    && (cMem.SiteId == 1 || cMem.SiteId == m.SiteId)).ToList();
                        ShowSalesPerson = true;
                        ShowTeam = true;
                        ViewBag.Team = tmpDepartment.Where(d => d.Active.Value && d.ParentDepartmentId != null
                                            && (cMem.SiteId == 1 || cMem.SiteId == d.SiteId)).ToList();
                    }
                    else if (access.Any(k => k.Key.Equals("orders_viewteam")) == true && access["orders_viewteam"] == true)
                    {
                        List<string> listMemberNumber = new List<string>();
                        var ManagerDep = tmpDepartment.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null
                                            && (cMem.SiteId == 1 || cMem.SiteId == x.SiteId)).ToList();
                        ViewBag.Team = ManagerDep;
                        if (ManagerDep.Count() > 0)
                        {
                            foreach (var dep in ManagerDep)
                            {
                                listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                                ShowSalesPerson = true;
                            }
                            ShowTeam = true;
                        }
                        if (currentDeparmentsUser.Count() > 0)
                        {
                            foreach (var deparment in currentDeparmentsUser)
                            {
                                if (deparment.LeaderNumber == cMem.MemberNumber)
                                {
                                    listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                                    ShowSalesPerson = true;
                                }
                            }
                        }
                        listMemberNumber.Add(cMem.MemberNumber);
                        listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                        ViewBag.ListMemberFilter = (from m in db.P_Member join l in listMemberNumber on m.MemberNumber equals l select m).ToList();
                    }
                    ViewBag.ShowSalesPerson = ShowSalesPerson;
                    ViewBag.ShowTeam = ShowTeam;

                    sale_dep = tmpDepartment.Select(d => d.Id).ToList().Select(d => d.ToString());
                    ViewBag.p = access;
                    var SystemConfigurations = db.SystemConfigurations.FirstOrDefault();
                    ViewBag.ProductName = SystemConfigurations.ProductName ?? SystemConfigurations.CompanyName;
                    ViewBag.ListMember = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).Select(x => new SelectListItem { Value = x.MemberNumber, Text = x.FullName }).ToList();
                    ViewBag.Status = Session["Order_status"]?.ToString() ?? "Open";
                    var From = DateTime.UtcNow.AddMonths(-3);
                    ViewBag.From = Session["Order_from"] != null ? DateTime.Parse(Session["Order_from"].ToString()) : new DateTime(From.Year, From.Month, 1);
                    ViewBag.To = Session["Order_to"] != null ? DateTime.Parse(Session["Order_to"].ToString()) : DateTime.UtcNow;
                    ViewBag.TeamSearch = Session["Order_team"]?.ToString();
                    ViewBag.SalesPerson = Session["Order_salesperson"]?.ToString();
                    ViewBag.SearchText = Session["Order_searchtext"]?.ToString();
                    ViewBag.SearchBy = Session["Order_searchby"]?.ToString();
                    ViewBag.Partners = db.C_Partner.Where(c => c.Status > 0 && (cMem.SiteId == 1 || cMem.SiteId == c.SiteId)).ToList();
                    ViewBag.SearchPartner = Session["Order_searchpartner"]?.ToString();
                    ViewBag.FromSystem = Session["Order_fromsystem"]?.ToString();
                    return PartialView("_InvoiceManagementTab");
            }
        }
        public JsonResult LoadOrder_table(Order_Request data, IDataTablesRequest dataTablesRequest)
        {
            _logService.Info($"[Invoice][LoadOrderTable] data: {Newtonsoft.Json.JsonConvert.SerializeObject(data)} - dataTablesRequest {Newtonsoft.Json.JsonConvert.SerializeObject(dataTablesRequest)}");
            if (string.IsNullOrEmpty(data.Status))
            {
                data.Status = InvoiceStatus.Open.ToString();
            }
            Session.Remove(INVOICE_LIST);
            Session["Order_status"] = data.Status;
            Session["Order_from"] = data.FromDate;
            Session["Order_to"] = data.Todate;
            Session["Order_team"] = data.Team;
            Session["Order_salesperson"] = data.SalesPerson;
            Session["Order_searchpartner"] = data.Partner;
            Session["Order_searchtext"] = data.SearchText;
            Session["Order_fromsystem"] = data.FromSystem;
            Session["Order_searchby"] = data.SearchBy;
            WebDataModel db = new WebDataModel();
            var recordsTotal = db.O_Orders.Count();
            var dataFromDate = data.FromDate.IMSToUTCDateTime();
            var dataTodate = data.Todate.IMSToUTCDateTime().AddDays(1).AddSeconds(-1);

            //var orders = db.O_Orders.Where(o => o.CreatedAt >= data.FromDate && o.CreatedAt <= data.Todate);
            var orders = from o in db.O_Orders
                         join cus in db.C_Customer on o.CustomerCode equals cus.CustomerCode
                         where cMem.SiteId == 1 || cMem.SiteId == cus.SiteId
                         let activedLicense = db.Store_Services.Count(s => s.OrderCode == o.OrdersCode && s.Active == 1) > 0
                         select new OrderViewListModel
                         {
                             order = o,
                             updateAt = o.UpdatedAt ?? o.CreatedAt,
                             isActivedLicense = activedLicense,
                             OwnerEmail = cus.Email,
                             SalonEmail = cus.SalonEmail,
                             OwnerPhone = cus.OwnerMobile,
                             SalonPhone = cus.SalonPhone,
                             StoreCode = cus.StoreCode
                         };

            //var query = (from o in orders
            //             join m in db.C_Customer on o.CustomerCode equals m.CustomerCode
            //             select new { o, m.StoreCode });

            #region filter

            if (access.Any(k => k.Key.Equals("orders_viewall")) == true && access["orders_viewall"] == true)
            {
                if (!string.IsNullOrEmpty(data.SalesPerson))
                {
                    if (data.SalesPerson == "Unassigned")
                    {
                        orders = orders.Where(x => string.IsNullOrEmpty(x.order.SalesMemberNumber));
                    }
                    else
                    {
                        orders = orders.Where(x => x.order.SalesMemberNumber == data.SalesPerson);
                    }
                }
                if (!string.IsNullOrEmpty(data.Team))
                {
                    List<string> listMemberNumber = new List<string>();
                    long TeamId = long.Parse(data.Team);
                    var TeamFilter = db.P_Department.Find(long.Parse(data.Team));
                    listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                    orders = orders.Where(x => listMemberNumber.Any(y => y.Contains(x.order.SalesMemberNumber)));
                }
            }
            else if (access.Any(k => k.Key.Equals("orders_viewteam")) == true && access["orders_viewteam"] == true)
            {
                List<string> listMemberNumber = new List<string>();
                var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                if (ManagerDep.Count() > 0)
                {
                    foreach (var dep in ManagerDep)
                    {
                        listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                    }
                }
                var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                if (currentDeparmentsUser.Count() > 0)
                {
                    foreach (var deparment in currentDeparmentsUser)
                    {
                        if (deparment.LeaderNumber == cMem.MemberNumber)
                        {
                            listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                        }
                    }
                }
                listMemberNumber.Add(cMem.MemberNumber);
                listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                orders = orders.Where(x => listMemberNumber.Any(y => y.Contains(x.order.SalesMemberNumber)));
                if (!string.IsNullOrEmpty(data.SalesPerson))
                {
                    if (data.SalesPerson == "Unassigned")
                    {
                        orders = orders.Where(x => string.IsNullOrEmpty(x.order.SalesMemberNumber));
                    }
                    else
                    {
                        orders = orders.Where(x => x.order.SalesMemberNumber == data.SalesPerson);
                    }
                }
            }
            else
            {
                throw new Exception("Forbidden");
            }

            int DeletedCount = orders.Where(o => o.order.IsDelete == true || InvoiceStatus.Canceled.ToString().Equals(o.order.Status)).Count();
            int AllCount = orders.Where(o => o.order.IsDelete != true).Count();
            int RealityCount = orders.Where(o => (InvoiceStatus.Paid_Wait.ToString().Equals(o.order.Status) || InvoiceStatus.Closed.ToString().Equals(o.order.Status)) && o.order.IsDelete != true).Count();
            int OpenCount = orders.Where(o => InvoiceStatus.Open.ToString().Equals(o.order.Status) && o.order.IsDelete != true).Count();
            int Paid_WaitCount = orders.Where(o => InvoiceStatus.Paid_Wait.ToString().Equals(o.order.Status) && o.order.IsDelete != true).Count();
            int ClosedCount = orders.Where(o => InvoiceStatus.Closed.ToString().Equals(o.order.Status) && o.order.IsDelete != true).Count();
            orders = orders.Where(o => o.order.CreatedAt >= dataFromDate && o.order.CreatedAt <= dataTodate);
            if (data.Status == "Deleted")
            {
                orders = orders.Where(o => o.order.IsDelete == true || InvoiceStatus.Canceled.ToString().Equals(o.order.Status));
            }
            else if (data.Status == "All")
            {
                orders = orders.Where(o => o.order.IsDelete != true);
            }
            else if (data.Status == "Reality")
            {
                orders = orders.Where(o => (InvoiceStatus.Paid_Wait.ToString().Equals(o.order.Status) || InvoiceStatus.Closed.ToString().Equals(o.order.Status)) && o.order.IsDelete != true);
            }
            else
            {
                orders = orders.Where(o => o.order.IsDelete != true && o.order.Status == data.Status || (data.Status == InvoiceStatus.Open.ToString() && o.order.Status == InvoiceStatus.PaymentLater.ToString()));
            }




            //if ((access.Any(k => k.Key.Equals("orders_viewall")) == true && access["orders_viewall"] == true))
            //{
            //    // orders = orders
            //}
            //else if (access.Any(k => k.Key.Equals("orders_viewteam")) == true && access["orders_viewteam"] == true)
            //{
            //    orders = orders.Where(o => (o.order.CreateByMemNumber == cMem.MemberNumber || o.order.SalesMemberNumber == cMem.MemberNumber));
            //}
            //else
            //{
            //    throw new Exception("Forbidden");
            //}

            if (!string.IsNullOrEmpty(data.Partner))
            {
                if (data.Partner == "mango")
                {
                    orders = orders.Where(x => string.IsNullOrEmpty(x.order.PartnerCode));
                }
                else
                {
                    orders = orders.Where(x => x.order.PartnerCode == data.Partner);
                }
            }

            if (!string.IsNullOrEmpty(data.FromSystem))
            {
                if (data.FromSystem == "ims")
                {
                    orders = orders.Where(x => x.order.CreateByMemNumber!= "000748");
                }
                else
                {
                    orders = orders.Where(x => x.order.CreateByMemNumber == "000748");
                }
            }

            if (!string.IsNullOrEmpty(data.SearchText))
            {
                switch (data.SearchBy)
                {
                    case "phone":
                        orders = orders.AsEnumerable().Where(o =>
                                      CommonFunc.SearchNameByElement(data.SearchText, o.SalonPhone?.ToString())
                           ).AsQueryable();
                        break;
                   
                    case "email":
                        orders = orders.AsEnumerable().Where(o =>
                                     CommonFunc.SearchNameByElement(data.SearchText, o.SalonEmail?.ToString())
                          ).AsQueryable();
                        break;
                   
                    case "storeId":
                        orders = orders.AsEnumerable().Where(o =>
                                       CommonFunc.SearchNameByElement(data.SearchText, o.StoreCode?.ToString())
                            ).AsQueryable();
                        break;
                    case "salonName":
                        orders = orders.AsEnumerable().Where(o =>
                                       CommonFunc.SearchNameByElement(data.SearchText, o.SalonName)
                            ).AsQueryable();
                        break;
                    case "invoice":
                        orders = orders.AsEnumerable().Where(o =>
                                       CommonFunc.SearchNameByElement(data.SearchText, o.order.OrdersCode)
                            ).AsQueryable();
                        break;
                    default:
                        orders = orders.AsEnumerable().Where(o =>
                           CommonFunc.SearchNameByElement(data.SearchText, o.order.CustomerName)
                           || (!string.IsNullOrEmpty(o.order.SalesName) && CommonFunc.SearchNameByElement(data.SearchText, o.order.SalesName))
                           || CommonFunc.SearchNameByElement(data.SearchText, o.order.InvoiceNumber?.ToString())
                             || CommonFunc.SearchNameByElement(data.SearchText, o.StoreCode)
                               || CommonFunc.SearchNameByElement(data.SearchText, o.OwnerPhone?.ToString())
                                 || CommonFunc.SearchNameByElement(data.SearchText, o.OwnerEmail?.ToString())
                                   || CommonFunc.SearchNameByElement(data.SearchText, o.SalonEmail?.ToString())
                                     || CommonFunc.SearchNameByElement(data.SearchText, o.SalonPhone?.ToString())
                           ).AsQueryable();
                        break;
                }

              
            }

            var grandtotalInvoice = orders.Sum(c => c.order.GrandTotal) ?? decimal.Zero;
            string grandtotalText = grandtotalInvoice.ToString("$#,##0.#0");

            var filtered_count = orders.Count();
            #endregion
            #region order
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch.Name)
            {
                case "UpdateAt":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        orders = orders.OrderBy(s => s.updateAt);
                    }
                    else
                    {
                        orders = orders.OrderByDescending(s => s.updateAt);
                    }
                    break;
                case "InvoiceId":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        orders = orders.OrderBy(s => s.order.OrdersCode);
                    }
                    else
                    {
                        orders = orders.OrderByDescending(s => s.order.OrdersCode);
                    }
                    break;
                case "CustomerName":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        orders = orders.OrderBy(s => s.order.CustomerName);
                    }
                    else
                    {
                        orders = orders.OrderByDescending(s => s.order.CustomerName);
                    }
                    break;
                case "SalesPerson":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        orders = orders.OrderBy(s => s.order.SalesName);
                    }
                    else
                    {
                        orders = orders.OrderByDescending(s => s.order.SalesName);
                    }
                    break;
                case "GrandTotal":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        orders = orders.OrderBy(s => s.order.GrandTotal);
                    }
                    else
                    {
                        orders = orders.OrderByDescending(s => s.order.GrandTotal);
                    }
                    break;
                case "Status":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        orders = orders.OrderBy(s => s.order.Status);
                    }
                    else
                    {
                        orders = orders.OrderByDescending(s => s.order.Status);
                    }
                    break;
                default:
                    orders = orders.OrderByDescending(s => s.order.CreatedAt);
                    break;
            }
            #endregion
            ViewBag.p = access;
            Session[INVOICE_LIST] = orders.ToList();
            var List_order = orders.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList();
            var Html = CommonFunc.RenderRazorViewToString("_ListOrders", List_order, this);
            return Json(new
            {
                draw = dataTablesRequest.Draw,
                recordsFiltered = filtered_count,
                recordsTotal = recordsTotal,
                grandtotalInvoice = grandtotalText,
                data = Html,
                DeletedCount,
                AllCount,
                RealityCount,
                OpenCount,
                Paid_WaitCount,
                ClosedCount
            });
        }
        #region Estimates
        
        public ActionResult EstimatesDetail(long? id, string code = "", string url_back = "/order")
        {
            try
            {
                ViewBag.p = access;
                WebDataModel db = new WebDataModel();
                _logService.Info($"[Invoice][EstimatesDetail] start Load invoice detail id = {id}, code = {code}");
                var order = new O_Orders();
                if (id > 0)
                {
                    order = db.O_Orders.Find(id);
                }
                else
                {
                    order = db.O_Orders.Where(o => o.OrdersCode == code).FirstOrDefault();
                }
                var cus = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();

                ViewBag.Url_Back = url_back.Replace("'", "");
                ViewBag.paymentLink = new InvoiceService().GetPaymentLink(order.OrdersCode, cus.MD5PassWord);
                ViewBag.paymentLinkPartner = new InvoiceService().GetPaymentLink(order.OrdersCode, cus.MD5PassWord, cus.PartnerCode);

                var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                ViewBag.ListMember = db.P_Member.Where(m => m.Active == true && sale_dep.Any(n => m.DepartmentId.Contains(n))).Select(x => new SelectListItem { Value = x.MemberNumber, Text = x.FullName }).ToList();
                //ViewBag.subscription = db.Order_Subcription.Any(s => s.OrderCode == order.OrdersCode && s.Actived == true);
                if (order != null)
                {
                    //20191128
                    ViewBag.actived_store = db.Store_Services.Any(s => s.OrderCode == order.OrdersCode && s.Active == 1);
                    ViewBag.customerId = cus.Id;
                    _logService.Info($"[Invoice][EstimatesDetail] completed Load invoice detail", new { order = Newtonsoft.Json.JsonConvert.SerializeObject(order) });
                    return View(order);
                }
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                _logService.Error(ex, $"[Invoice][EstimatesDetail] error Load invoice detail id = {id}, code = {code}");
            }

            return RedirectToAction("index");
        }

        public ActionResult GetDetailInfo(long? id, string url, string code)
        {
            try
            {
                _logService.Info($"[Invoice][GetDetailInfo] start id = {id}, code = {code}, url = {url}");
                WebDataModel db = new WebDataModel();
                var order_detail = db.O_Orders.Find(id);
                if (order_detail == null)
                {
                    order_detail = db.O_Orders.Where(o => o.OrdersCode == code).FirstOrDefault();
                }
                ViewBag.Url = url;

                if (order_detail != null)
                {
                    ViewBag.Customer = db.C_Customer.Where(c => c.CustomerCode == order_detail.CustomerCode).FirstOrDefault();
                    ViewBag.CompanyInfo = db.SystemConfigurations.FirstOrDefault();

                    var _listOrderProduct = db.Order_Products.Where(p => p.OrderCode == order_detail.OrdersCode).AsEnumerable();
                    var ListOrderProduct = _listOrderProduct.Where(l => l.BundleId != null).GroupBy(l => l.BundleId).Select(g =>
                          new Order_Products
                          {
                              BundleId = g.Key,
                              Quantity = g.ToList()[0].BundleQTY ?? 0,
                              Price = g.Sum(l => l.Price * l.Quantity / l.BundleQTY),
                              TotalAmount = g.Sum(l => l.Price * l.Quantity),
                          }).ToList();
                    ListOrderProduct.AddRange(_listOrderProduct.Where(l => l.BundleId == null).ToList());
                    ViewBag.ListOrderProduct = ListOrderProduct;
                    ViewBag.ListOrderSubcription = db.Order_Subcription.Where(s => s.OrderCode == order_detail.OrdersCode && s.Actived == true).ToList();
                    var list_bundle_id = ListOrderProduct.Where(p => p.BundleId != null).Select(p => p.BundleId).ToList();
                    _logService.Info($"[Invoice][GetDetailInfo] completed", new { order_detail = Newtonsoft.Json.JsonConvert.SerializeObject(order_detail) });
                    return PartialView("_InvoiceDetailPartial", order_detail);
                }
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                _logService.Error(ex, $"[Invoice][GetDetailInfo] error id = {id}, code = {code}, url = {url}");
            }

            return PartialView("_InvoiceDetailPartial", new O_Orders());
        }

        public async Task<FileStreamResult> ExportInvoiceToExcel()
        {
            try
            {
                _logService.Info($"[Invoice][ExportInvoiceToExcel] start Export invoice");
                var Invoces = Session[INVOICE_LIST] as List<OrderViewListModel> ?? new List<OrderViewListModel> { };
                _logService.Info($"[Invoice][ExportInvoiceToExcel] precesing Export invoice", new { Invoces = Newtonsoft.Json.JsonConvert.SerializeObject(Invoces) });
                //view all sales lead if permission is view all
                string webRootPath = "/Upload/Merchant/Template";
                string templateName = @"Template_Invoice_List.xlsx";
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
                    for (var i = 0; i < Invoces.Count; i++)
                    {
                        fs_sheet.CreateRow(startRow + i).CreateCell(0).SetCellValue(Invoces[i].order.OrdersCode);
                        fs_sheet.GetRow(startRow + i).CreateCell(1).SetCellValue(Invoces[i].order.CustomerName);
                        fs_sheet.GetRow(startRow + i).CreateCell(2).SetCellValue(Invoces[i].StoreCode);
                        fs_sheet.GetRow(startRow + i).CreateCell(3).SetCellValue(Invoces[i].order.SalesName);
                        fs_sheet.GetRow(startRow + i).CreateCell(4).SetCellValue(Invoces[i].order.SalesMemberNumber);
                        fs_sheet.GetRow(startRow + i).CreateCell(5).SetCellValue(Invoces[i].order.GrandTotal.Value.ToString("$#,##0.#0"));
                        fs_sheet.GetRow(startRow + i).CreateCell(6).SetCellValue(Ext.EnumParse<InvoiceStatus>(Invoces[i].order.Status).Code<string>());
                        fs_sheet.GetRow(startRow + i).CreateCell(7).SetCellValue(Invoces[i].order.CreatedAt.Value.ToString("MMM dd, yyyy"));
                        fs_sheet.GetRow(startRow + i).CreateCell(8).SetCellValue(Invoces[i].order.CreatedBy);

                        fs_sheet.GetRow(startRow + i).GetCell(0).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(1).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(2).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(3).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(4).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(5).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(6).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(7).CellStyle = style2;
                        fs_sheet.GetRow(startRow + i).GetCell(8).CellStyle = style2;
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
                string _fileName = "InvoiceList_" + DateTime.UtcNow.ToString("yyyyMMddhhmmss") + ".xlsx";
                _logService.Info($"[Invoice][ExportInvoiceToExcel] completed Export invoice");
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][ExportInvoiceToExcel] error Export invoice");
                throw ex;
            }
        }

        #endregion

        #region ORDER

        /// <summary>
        /// Add new or Update Order
        /// </summary>
        /// <param name="Id">Order Id</param>
        /// <returns></returns>
        public ActionResult Save(long? Id, string select_cus_code)
        {
            _logService.Info($"[Invoice][Save] start loading invoice {Id}");
            var order = new O_Orders();
            try
            {
                TempData.Remove("s"); TempData.Remove("e");
                Session.Remove(LIST_PRODUCT_SERVICE);
                Session.Remove(TOTAL_MONEY_ORDER);
                Session.Remove(LIST_BUNDLE_DEVICE);
                ViewBag.p = access;
                WebDataModel db = new WebDataModel();
                var list_device_service = new List<Device_Service_ModelCustomize>();


                ViewBag.ListPOS = db.O_Product.ToList();
                var sale_dep = db.P_Department.Where(d => d.Type == "SALES").Select(d => d.Id).ToList().Select(d => d.ToString());
                ViewBag.ListMember = db.P_Member.Where(m => m.Active == true && 
                                                        sale_dep.Any(n => m.DepartmentId.Contains(n)) &&
                                                        (cMem.SiteId == 1 || cMem.SiteId == m.SiteId)).ToList();
                ViewBag.ListModel = (from m in db.O_Product_Model
                                     where m.IsDeleted != true && m.Active == true
                                     join d in (from d in db.O_Device where d.Active == true && d.Junkyard != true && d.Inventory == 1 select d)
                                     on m.ModelCode equals d.ModelCode into jd
                                     select new Product_Model_view
                                     {
                                         model = m,
                                         remaining = jd.Count()
                                     }).ToList();

                //db.O_Product_Model.Where(m => m.Active == true).ToList();
               // var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == select_cus_code) ?? new C_Customer();
                ViewBag.selectCus = select_cus_code;
                // var sl = db.C_SalesLead.FirstOrDefault(x => x.CustomerCode == cus.CustomerCode) ?? new C_SalesLead();
                if (string.IsNullOrEmpty(cMem.BelongToPartner) || cMem.BelongToPartner == "mango")
                {
                    ViewBag.BelongtoPartner = db.C_Partner.Where(x => x.Status >0).OrderBy(c => c.Code).ToList();
                }
                else
                {
                    ViewBag.BelongtoPartner = db.C_Partner.Where(x => x.Code == cMem.BelongToPartner).ToList();
                }
        

                if (Id > 0) //Edit
                {
                    _logService.Info($"[Invoice][Save] start loading edit invoice {Id}");
                    //Check access
                    if (access.Any(k => k.Key.Equals("orders_update")) == false || access["orders_update"] != true)
                    {
                        return Redirect("/home/forbidden");
                    }
                    #region UPDATE ORDER
                    order = db.O_Orders.Find(Id);

                    ViewBag.Title = "Update Invoice";
                    if (order != null)
                    {
                        var partner = db.C_Partner.FirstOrDefault(c => c.Code == order.PartnerCode) ?? new C_Partner { };
                        if (((order.Status == InvoiceStatus.Closed.ToString() && (access.Any(k => k.Key.Equals("orders_updateclose")) != true || access["orders_updateclose"] != true))
                            || order.Status == InvoiceStatus.Paid_Wait.ToString()
                            || order.Status == InvoiceStatus.Canceled.ToString())
                            && (!cMem.RoleCode.Contains("admin")))
                        {
                            throw new Exception("Cannot edit invoice!");
                        }
                        //dua list order_product vao session
                        var list_product = db.Order_Products.Where(p => p.OrderCode == order.OrdersCode).ToList();
                        foreach (var item in list_product)
                        {
                            var dv = new Device_Service_ModelCustomize();
                            dv.Key = item.Id;
                            if (item.BundleId > 0)
                            {
                                if (!list_device_service.Any(lds => lds.BundleId == item.BundleId))
                                {
                                    dv.BundleId = item.BundleId;
                                    dv.BundleName = db.I_Bundle.Find(item.BundleId)?.Name;
                                    dv.list_Bundle_Device = (from d in list_product
                                                             where d.BundleId == dv.BundleId
                                                             join m in db.O_Product_Model on d.ModelCode equals m.ModelCode
                                                             select new Bundle_Model_view
                                                             {
                                                                 ProductName = m.ProductName,
                                                                 ModelCode = m.ModelCode,
                                                                 ModelName = m.ModelName,
                                                                 Color = m.Color,
                                                                 Quantity = d.Quantity / d.BundleQTY,
                                                                 Price = d.Price
                                                             }).ToList();

                                    dv.Quantity = item.BundleQTY ?? 0;
                                    //dv.Price = list_product.Where(l => l.BundleId == dv.BundleId).Sum(l => (l.Price ?? 0) * (l.Quantity ?? 0) / (l.BundleQTY ?? 0));
                                    dv.PriceApply = list_product.Where(l => l.BundleId == dv.BundleId).Sum(l => (l.Price ?? 0) * (l.Quantity ?? 0) / (l.BundleQTY ?? 0));
                                    dv.Amount = item.TotalAmount ?? 0; //(dv.Price * dv.Quantity) - (item.Discount ?? 0);
                                    dv.Discount = item.Discount ?? 0;
                                    dv.DiscountPercent = item.DiscountPercent ?? 0;
                                    dv.Price = dv.Discount + dv.Amount;
                                    dv.Type = "package";
                                    dv.DiscountType = (item.DiscountPercent > 0 ? "rate" : "amount");
                                    list_device_service.Add(dv);
                                }

                            }
                            else
                            {
                                dv.Feature = item.Feature;
                                dv.ModelCode = item.ModelCode;
                                dv.ModelName = item.ModelName;
                                dv.ProductCode = item.ProductCode;
                                dv.ProductName = item.ProductName;
                                dv.Quantity = item.Quantity ?? 0;
                                dv.PriceApply = item.Price ?? 0;
                                dv.Discount = item.Discount ?? 0;
                                dv.DiscountPercent = item.DiscountPercent ?? 0;
                                dv.Amount = item.TotalAmount ?? 0; //(dv.Price * dv.Quantity) - (item.Discount ?? 0);
                                dv.Price = dv.Discount + dv.Amount;
                                dv.Type = "device";
                                dv.DiscountType = (item.DiscountPercent > 0 ? "rate" : "amount");
                                var _pro = db.O_Product_Model.Where(p => p.ModelCode == dv.ModelCode).FirstOrDefault();
                                dv.Picture = _pro?.Picture;
                                list_device_service.Add(dv);
                            }
                        }
                        var subcriptions = db.Order_Subcription.Where(s => s.OrderCode == order.OrdersCode && s.Actived == true).ToList();
                        foreach (var item in subcriptions)
                        {
                            var dv = new Device_Service_ModelCustomize();
                            var license = db.License_Product.Where(x => x.Id == item.ProductId).FirstOrDefault();
                            var Promotion_Apply_Status = false;
                            var PromotionalStatus = Store_Apply_Status.Promotional.Text();
                            int currentApplyMonths = 0;
                            var Order = db.O_Orders.Where(x => x.Id == Id.Value).FirstOrDefault();
                            currentApplyMonths = db.Order_Subcription.Where(x => x.ProductId == item.ProductId && x.CustomerCode == select_cus_code && x.OrderCode != order.OrdersCode).Count();
                            var Promotion_Apply_Months = 0;
                            var orderSubcrition = db.Order_Subcription.FirstOrDefault(x => x.ProductId == item.ProductId && x.CustomerCode == select_cus_code && x.PriceType == PromotionalStatus);
                            if (orderSubcrition != null)
                            {
                                Promotion_Apply_Months = orderSubcrition.Promotion_Apply_Months ?? 0;
                            }
                            else
                            {
                                Promotion_Apply_Months = db.License_Product.FirstOrDefault(x => x.Id == item.ProductId).Promotion_Apply_Months ?? 0;
                            }
                            dv.Promotion_Apply_Months = Promotion_Apply_Months;
                            dv.Promotion_Time_To_Available = license.Promotion_Time_To_Available ?? 0;
                            var removeLicenseStatus = LicenseStatus.REMOVED.Code<int>();
                            var watingLicenseStatus = LicenseStatus.WAITING.Code<int>();
                            var FirstLicenseApply = db.Store_Services.Where(x => x.ProductCode == license.Code && x.CustomerCode == item.CustomerCode && x.Type == "license" && x.Active != removeLicenseStatus && x.Active != watingLicenseStatus).OrderBy(x => x.EffectiveDate).FirstOrDefault();
                            var LastLicenseApply = db.Store_Services.Where(x => x.ProductCode == license.Code && x.CustomerCode == item.CustomerCode && x.Type == "license" && x.Active != removeLicenseStatus && x.Active != watingLicenseStatus).OrderByDescending(x => x.RenewDate).FirstOrDefault();
                            if (dv.Promotion_Time_To_Available == 0)
                            {
                                Promotion_Apply_Status = true;
                            }
                            if (FirstLicenseApply != null && LastLicenseApply != null && Promotion_Apply_Status == false)
                            {
                                if (dv.Promotion_Time_To_Available <= (LastLicenseApply.RenewDate - FirstLicenseApply.EffectiveDate).Value.TotalDays / 30)
                                {
                                    Promotion_Apply_Status = true;
                                }
                            }
                            item.Amount = item.Amount ?? (item.Period == "MONTHLY" ?
                                                            ((item.Price ?? 0) - (item.Discount ?? 0)) :
                                                            ((item.Price ?? 0) * (item.Quantity ?? 1) - (item.Discount ?? 0)));
                            dv.Promotion_Apply_Status = Promotion_Apply_Status;
                            dv.RealPrice = item.Price ?? 0; //license.Price ?? 0;
                            dv.TrialMonths = license.Trial_Months ?? 0;
                            dv.Promotion_Price = license.Promotion_Price ?? 0;
                            dv.SubscriptionId = item.ProductId;
                            dv.SubscriptionName = item.ProductName;
                            dv.NumberOfPeriod = license.NumberOfPeriod ?? 1;
                            dv.Price = (item.Amount + item.Discount) ?? 0;
                            dv.PriceApply = item.Price ?? 0;
                            dv.PriceType = item.PriceType ?? Store_Apply_Status.Real.Text();
                            dv.Quantity = item.Quantity ?? 1;
                            dv.SubscriptionDuration = item.Period;
                            dv.Type = item.SubscriptionType ?? "";
                            dv.Discount = item.Discount ?? 0;
                            dv.DiscountPercent = item.DiscountPercent ?? 0;
                            dv.DiscountType = (item.DiscountPercent > 0 ? "rate" : "amount");
                            dv.StartDate = item.StartDate;
                            dv.AutoRenew = item.AutoRenew;
                            dv.ExpiryDate = item.EndDate;//item.StartDate.Value.AddMonths(dv.Quantity);
                            dv.PartnerPrice = license.PartnerPrice ?? 0;
                            dv.MembershipPrice = license.MembershipPrice ?? 0;
                            dv.ApplyDiscountAsRecurring = item.ApplyDiscountAsRecurring ?? false;
                            dv.PeriodRecurring = license.PeriodRecurring ?? (license.SubscriptionDuration == "MONTHLY" ? RecurringInterval.Monthly.ToString() : "");
                            dv.SubscriptionQuantity = item.SubscriptionQuantity ?? 1;
                            dv.ApplyPaidDate = item.ApplyPaidDate ?? false;
                            //dv.PriceApply = (string.IsNullOrEmpty(order.PartnerCode) ? item.Price : (partner.PriceType == "membership" ? license.MembershipPrice : (partner.PriceType == "partner" ? license.MembershipPrice : license.PartnerPrice))) ?? 0;
                            dv.Amount = item.Amount ?? 0;
                            dv.RecurringPrice = item.RecurringPrice;
                            dv.PreparingDays = item.PreparingDate??0;
                            list_device_service.Add(dv);
                        }
                        Session[LIST_PRODUCT_SERVICE] = list_device_service;
                        //dua cac khoang tien cua order vao session
                        TotalMoneyOrder total_money_order = new TotalMoneyOrder();
                        total_money_order.ShippingFee = order.ShippingFee ?? 0;
                        total_money_order.DiscountAmount = order.DiscountAmount ?? 0;
                        total_money_order.DiscountPercent = (decimal)(order.DiscountPercent ?? 0);
                        total_money_order.TaxRate = order.TaxRate ?? 0;

                        total_money_order = CalculatorGrandToatal(total_money_order);
                        ViewBag.TotalMoneyOrder = total_money_order;
                        ViewBag.ServicePrice = 0;// list_sv.Where(s => s.Monthly == 1).FirstOrDefault()?.MonthlyFee ?? 0;
                        ViewBag.ExistDeploymentTicket = db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Where(x => x.OrderCode == order.OrdersCode && x.T_TicketTypeMapping.Any(y => y.TypeId == 4)).Count() > 0;
                        _logService.Info($"[Invoice][Save] complete loaded edit invoice {Id}", new { order = Newtonsoft.Json.JsonConvert.SerializeObject(order) });
                    }
                    else
                    {
                        throw new Exception("Does not exist!");
                    }
                    #endregion

                }
                else //Add new
                {
                    _logService.Info($"[Invoice][Save] start loading create new invoice");
                    //Check access
                    if (access.Any(k => k.Key.Equals("orders_addnew")) == false || access["orders_addnew"] != true)
                    {
                        return Redirect("/home/forbidden");
                    }
                    //.End

                    ViewBag.Title = "New Invoice";
                    #region CREATE NEW ORDER

                   // order.SalesMemberNumber = sl.MemberNumber;
                    //TOTAL_MONEY_ORDER
                    TotalMoneyOrder total_money_order = new TotalMoneyOrder { };
                    order.PartnerCode = cMem.BelongToPartner;

                    Session[LIST_PRODUCT_SERVICE] = list_device_service;
                    Session[TOTAL_MONEY_ORDER] = total_money_order;
                    ViewBag.TotalMoneyOrder = total_money_order;
                    #endregion
                }
                ViewBag.ListDeviceService = list_device_service;
                ViewBag.bundles = (from bd in db.I_Bundle_Device
                                   join m in db.O_Product_Model on bd.ModelCode equals m.ModelCode
                                   where m.Active == true
                                   group new { bd, m } by bd.Bundle_Id into Gr
                                   join b in db.I_Bundle on Gr.Key equals b.Id
                                   select new Bundle_view { Bundle = b, Detail = Gr.Select(g => new Bundle_Detail_view { BundleDevice = g.bd, Model = g.m }).ToList() }).ToList();
                _logService.Info($"[Invoice][Save] complete loaded create new invoice");
                return View(order);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][Save] error loading {(Id > 0 ? "edit" : "create new")} invoice {Id}");
                TempData["e"] = ex.Message;
                return RedirectToAction("index");
            }
        }

        /// <summary>
        /// Create or Update Order
        /// </summary>
        /// <param name="orderModel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SaveOrder(O_Orders orderModel, string action, bool send_payment_email = false)
        {
            _logService.Info($"[Invoice][SaveOrder] start save order action = {action} | send_payment_email = {send_payment_email}", new { orderModel = Newtonsoft.Json.JsonConvert.SerializeObject(orderModel) });
            WebDataModel db = new WebDataModel();
            using (var Trans = db.Database.BeginTransaction())
            {
                try
                {
                    if (orderModel.Id > 0)//Update
                    {
                        Random Rand = new Random();
                        var order = db.O_Orders.FirstOrDefault(o => o.Id == orderModel.Id);
                        if (order != null)
                        {
                            var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == order.CustomerCode);
                            var _partner = db.C_Partner.FirstOrDefault(c => c.Code == orderModel.PartnerCode);
                            if (_partner != null && cus.PartnerCode != orderModel.PartnerCode)
                            {
                                cus.PartnerCode = _partner.Code;
                                cus.PartnerName = _partner.Name;
                                db.Entry(cus).State = EntityState.Modified;
                            }
                            #region update assign ticket new salon
                            if (!string.IsNullOrEmpty(orderModel.SalesMemberNumber) && orderModel.SalesMemberNumber != order.SalesMemberNumber)
                            {
                                var member = db.P_Member.Where(x => x.MemberNumber == orderModel.SalesMemberNumber).FirstOrDefault();
                                var tickets = db.T_SupportTicket.Where(x => x.OrderCode == order.OrdersCode);
                                List<string> ListDeparmentTicketByMember = new List<string>();
                                var ChirldDepartment = db.P_Department.Where(x => !string.IsNullOrEmpty(x.GroupMemberNumber)).ToList();
                                foreach (var dep in ChirldDepartment)
                                {
                                    var listMember = dep.GroupMemberNumber.Split(',');
                                    if (listMember.Contains(member.MemberNumber))
                                    {
                                        ListDeparmentTicketByMember.Add(dep.Id.ToString());
                                    }
                                }
                                foreach (var t in tickets)
                                {
                                    if (ListDeparmentTicketByMember.Any(x => x.Contains(t.GroupID.ToString())))
                                    {
                                        if (string.IsNullOrEmpty(t.AssignedToMemberNumber) || !(t.AssignedToMemberNumber.Contains(member.MemberNumber)))
                                        {
                                            if (!string.IsNullOrEmpty(t.AssignedToMemberNumber))
                                            {
                                                t.AssignedToMemberNumber = t.AssignedToMemberNumber.Substring(t.AssignedToMemberNumber.Length - 1) == "," ? (t.AssignedToMemberNumber + member.MemberNumber + ",") : (t.AssignedToMemberNumber + "," + member.MemberNumber + ",");
                                            }
                                            else
                                            {
                                                t.AssignedToMemberNumber = member.MemberNumber + ",";
                                            }
                                            if (!string.IsNullOrEmpty(t.AssignedToMemberName))
                                            {
                                                t.AssignedToMemberName = t.AssignedToMemberName.Substring(t.AssignedToMemberName.Length - 1) == "," ? (t.AssignedToMemberName + member.FullName + ",") : (t.AssignedToMemberName + "," + member.FullName + ",");
                                            }
                                            else
                                            {
                                                t.AssignedToMemberName = member.FullName + ",";
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                            #region UPDATE ORDER
                            var sale_mn = Request["SalesMemberNumber"];
                            var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;

                            order.CustomerCode = orderModel.CustomerCode;
                            order.PartnerCode = orderModel.PartnerCode;
                            order.CustomerName = cus?.BusinessName;
                            // order.CreateDeployTicket = orderModel.CreateDeployTicket;
                            if (string.IsNullOrEmpty(sale_mn) == true)
                            {
                                order.SalesMemberNumber = null;
                                order.SalesName = null;
                            }
                            else
                            {
                                order.SalesName = db.P_Member.FirstOrDefault(m => m.MemberNumber == sale_mn)?.FullName;
                                if (order.SalesMemberNumber != sale_mn && sale_mn != cMem.MemberNumber)
                                {
                                    var notificationService = new NotificationService();
                                    notificationService.OrderAddNotification(sale_mn, order.Id.ToString(), order.OrdersCode, cMem.FullName, cMem.FullName);
                                }
                                else if (sale_mn != cMem.MemberNumber)
                                {
                                    var notificationService = new NotificationService();
                                    notificationService.OrderUpdateNotification(sale_mn, order.Id.ToString(), order.OrdersCode, cMem.FullName, cMem.MemberNumber);
                                }
                                order.SalesMemberNumber = sale_mn;

                            }

                            order.Comment = Request.Unvalidated["desc"];
                            order.UpdatedAt = DateTime.UtcNow;
                            order.UpdatedHistory += "|" + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - By: " + cMem.FullName;
                            order.DiscountAmount = total_money_order.DiscountAmount;
                            order.DiscountPercent = float.Parse(total_money_order.DiscountPercent.ToString());
                            order.ShippingFee = total_money_order.ShippingFee;
                            order.TaxRate = total_money_order.TaxRate;
                            order.TotalHardware_Amount = total_money_order.SubTotal;
                            order.GrandTotal = total_money_order.GrandTotal;
                            order.ShippingAddress = Request["sh_street"] + "|" + Request["sh_city"] + "|" + Request["sh_state"] + "|" + Request["sh_zip"] + "|" + Request["sh_country"];
                            order.InvoiceDate = orderModel.InvoiceDate;
                            order.CreateDeployTicket = bool.Parse(Request["CreateDeployTicket"] ?? "false");
                            //order.DueDate = orderModel.DueDate;
                            order.Renewal = orderModel.Renewal;
                            if (order.InvoiceNumber == null)
                            {
                                order.InvoiceNumber = long.Parse(order.OrdersCode);
                            }

                            #endregion
                            //cap nhat lai order_product
                            #region UPDATE DEVICES
                            var list_product_db = db.Order_Products.Where(d => d.OrderCode == order.OrdersCode).ToList();
                            var list_device_ss = (Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>).Where(d => d.Type == "device" || d.Type == "package").ToList();
                            var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                            List<string> ModelCodes = new List<string>();
                            if (order?.InvoiceNumber > 0)
                            {
                                //TH: tao luon invoice ma van chua co tic
                                //ket, khong can tao ticket sales nua,=> tao luon ims onboarding va finance ticket.
                                string linkViewInvoiceFull = ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + order.OrdersCode + "&flag=Invoices";
                                string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + order.Id;


                            }
                            foreach (var item in list_device_ss)
                            {
                                //tao order_product

                                if (item.BundleId > 0)
                                {
                                    var mbs = db.I_Bundle_Device.Where(m => m.Bundle_Id == item.BundleId).ToList();
                                    var package = new Order_Products();
                                    package.Id = id++;
                                    package.OrderCode = order.OrdersCode;
                                    package.Price = item.PriceApply; //item.price ;
                                    package.CreateBy = cMem.FullName;
                                    package.CreateAt = DateTime.UtcNow;
                                    package.BundleId = item.BundleId;
                                    package.BundleQTY = item.Quantity;
                                    package.Discount = item.Discount;
                                    package.DiscountPercent = item.DiscountPercent;
                                    package.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
                                    db.Order_Products.Add(package);
                                    foreach (var mb in mbs)
                                    {
                                        var model = db.O_Product_Model.Find(mb.ModelCode);
                                        var new_device = new Order_Products();
                                        new_device.Id = id++;
                                        new_device.OrderCode = order.OrdersCode;
                                        new_device.ProductCode = model.ProductCode;
                                        new_device.ProductName = model.ProductName;
                                        new_device.Price = mb.Price ?? 0;
                                        new_device.Quantity = mb.Quantity * item.Quantity;
                                        new_device.CreateBy = cMem.FullName;
                                        new_device.CreateAt = DateTime.UtcNow;
                                        new_device.Feature = model.Color;
                                        new_device.BundleId = item.BundleId;
                                        new_device.ModelCode = model.ModelCode;
                                        new_device.ModelName = model.ModelName;
                                        new_device.BundleQTY = item.Quantity;
                                        var oldOrderProduct = list_product_db.FirstOrDefault(x => x.ModelCode == mb.ModelCode && x.ProductCode == model.ProductCode);
                                        if (oldOrderProduct != null)
                                        {
                                            new_device.SerNumbers = oldOrderProduct.SerNumbers;
                                            new_device.InvNumbers = oldOrderProduct.InvNumbers;
                                        }
                                        ModelCodes.Add(mb.ModelCode);
                                        db.Order_Products.Add(new_device);
                                    }
                                }
                                else
                                {
                                    var new_device = new Order_Products();
                                    new_device.Id = id++;
                                    new_device.OrderCode = order.OrdersCode;
                                    new_device.ProductCode = item.ProductCode;
                                    new_device.ProductName = item.ProductName;
                                    new_device.Price = item.PriceApply; //item.Price;
                                    new_device.Quantity = item.Quantity;
                                    new_device.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
                                    new_device.CreateBy = cMem.FullName;
                                    new_device.CreateAt = DateTime.UtcNow;
                                    new_device.Feature = item.Feature;
                                    new_device.ModelCode = item.ModelCode;
                                    new_device.ModelName = item.ModelName;
                                    new_device.BundleId = null;
                                    new_device.BundleQTY = null;
                                    new_device.Discount = item.Discount;
                                    new_device.DiscountPercent = item.DiscountPercent;
                                    var oldOrderProduct = list_product_db.FirstOrDefault(x => x.ModelCode == item.ModelCode && x.ProductCode == item.ProductCode);
                                    if (oldOrderProduct != null)
                                    {
                                        new_device.SerNumbers = oldOrderProduct.SerNumbers;
                                        new_device.InvNumbers = oldOrderProduct.InvNumbers;
                                    }
                                    ModelCodes.Add(item.ModelCode);
                                    db.Order_Products.Add(new_device);
                                }
                            }
                            foreach (var item in list_product_db)
                            {
                                //xoa order_product
                                db.Order_Products.Remove(item);
                            }
                            #endregion
                            if (list_device_ss.Count > 0 && string.IsNullOrEmpty(order.BundelStatus))
                            {
                                order.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                            }
                            //Cap nhat lai order_license product & addon
                            #region UPDATE PRODUCT & ADDON
                            var oldproducts = db.Order_Subcription.Where(s => s.OrderCode == order.OrdersCode).ToList();
                            foreach (var item in oldproducts)
                            {
                                db.Order_Subcription.Remove(item);
                            }
                            if (string.IsNullOrWhiteSpace(cus.StoreCode) == true)
                            {   //new
                                cus.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(cus.CustomerCode, "[^.0-9]", "");
                                db.Entry(cus).State = EntityState.Modified;
                            }
                            var list_subsc_ss = (Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>)
                                .Where(d => (d.Type == "license" || d.Type == "addon" || d.Type == "giftcard" || d.Type == "other" || d.Type == "setupfee" || d.Type == "interactionfee")).ToList();
                            foreach (var item in list_subsc_ss)
                            {
                                var product = db.License_Product.Where(p => p.Id == item.SubscriptionId).FirstOrDefault();
                                var RecurringPrice = decimal.Parse(string.IsNullOrEmpty(Request["RecurringPrice_" + item.SubscriptionId]) ? "-1" : Request["RecurringPrice_" + item.SubscriptionId]);
                                var Renewal = Request["Renewal_" + item.SubscriptionId]?.ToString() == "true";
                                var ApplyDate = Request["ApplyDate_" + item.SubscriptionId]?.ToString() == "true";
                                var applyRecurring = Request["ApplyDiscountAsRecurring_" + item.SubscriptionId]?.ToString() == "true";
                                var NumberOfItem = db.License_Product_Item.Where(lp => lp.License_Product_Id == product.Id).Count();
                                var sp = new Order_Subcription
                                {
                                    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + Rand.Next(1, 9999).ToString().PadLeft(4, '0')),
                                    Actived = true,
                                    AutoRenew = item.SubscriptionDuration == "MONTHLY" ? Renewal : false,
                                    Price = item.PriceApply,
                                    Amount = item.Amount,
                                    PriceType = item.PriceType,
                                    Promotion_Apply_Months = (item.PriceType == Store_Apply_Status.Promotional.ToString()) ? item.Promotion_Apply_Months : 0,
                                    Period = item.SubscriptionDuration,
                                    OrderCode = order.OrdersCode,
                                    NumberOfItem = item.Type == "setupfee" || item.Type == "interactionfee" ? 0 : NumberOfItem,
                                    ProductId = product.Id,
                                    ProductName = product.Name,
                                    Product_Code = product.Code,
                                    Product_Code_POSSystem = product.Code_POSSystem,
                                    PurcharsedDay = DateTime.UtcNow,
                                    IsAddon = product.isAddon == true || item.Type == "setupfee" || item.Type == "interactionfee",
                                    SubscriptionType = item.Type,
                                    CustomerCode = order.CustomerCode,
                                    CustomerName = order.CustomerName,
                                    StoreCode = cus.StoreCode,
                                    Quantity = item.Quantity,
                                    Discount = item.Discount,
                                    DiscountPercent = item.DiscountPercent,
                                    ApplyDiscountAsRecurring = item.ApplyDiscountAsRecurring,
                                    PeriodRecurring = item.PeriodRecurring,
                                    SubscriptionQuantity = item.SubscriptionQuantity < 1 ? 1 : item.SubscriptionQuantity,
                                    ApplyPaidDate = ApplyDate
                                };
                                if (RecurringPrice >= 0) sp.RecurringPrice = RecurringPrice;

                                //update date
                                string start_date = Request["Effective_start_date_" + item.SubscriptionId];
                                //if (product.isAddon != true)
                                //{
                                sp.StartDate = string.IsNullOrEmpty(start_date) ? order.InvoiceDate : DateTime.Parse(start_date);
                                if (sp.Period == "MONTHLY")
                                {
                                    string end_date = Request["Expiry_date_" + item.SubscriptionId];
                                    sp.EndDate = string.IsNullOrEmpty(end_date) ? sp.StartDate.Value.AddMonths(item.Quantity) : DateTime.Parse(end_date);
                                }
                                else if (sp.Period == "QUATERLY")
                                {
                                    sp.EndDate = sp.StartDate.Value.AddMonths(3 * item.Quantity);
                                }
                                else if (sp.Period == "ANNUAL")
                                {
                                    sp.EndDate = sp.StartDate.Value.AddYears(item.Quantity);
                                }
                                if (item.Type == "setupfee")
                                {
                                    sp.EndDate = item.ExpiryDate;
                                }
                                db.Order_Subcription.Add(sp);

                                if (sp.SubscriptionType != "setupfee" && sp.SubscriptionType != "interactionfee")
                                {
                                    var sservice = db.Store_Services.FirstOrDefault(c => c.OrderCode == order.OrdersCode && c.ProductCode == sp.Product_Code);
                                    if (sservice != null)
                                    {
                                        sservice.Quantity = sp.SubscriptionQuantity;
                                        sservice.EffectiveDate = sp.StartDate;
                                        sservice.AutoRenew = sp.AutoRenew;
                                        sservice.ApplyDiscountAsRecurring = sp.ApplyDiscountAsRecurring;
                                        sservice.StoreApply = sp.PriceType;
                                        sservice.RenewDate = sp.EndDate;
                                        sservice.LastUpdateAt = DateTime.UtcNow;
                                        db.Entry(sservice).State = System.Data.Entity.EntityState.Modified;
                                    }
                                }
                                //MODIFY & CREATE STORT_PRODUCT_LICENSE
                            }
                            #endregion

                            await db.SaveChangesAsync();
                            Trans.Commit();
                            Trans.Dispose();

                            if (order?.InvoiceNumber > 0)
                            {
                                string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + order.Id;
                                var checkTerminal = (from ordPrd in db.Order_Products
                                                     join prd in db.O_Product on ordPrd.ProductCode equals prd.Code
                                                     join prdmodel in db.O_Product_Model on ordPrd.ModelCode equals prdmodel.ModelCode
                                                     where ordPrd.OrderCode == order.OrdersCode && (prd.ProductLineCode == "terminal" || prdmodel.MerchantOnboarding == true)
                                                     select ordPrd).Any();
                                var activeCreateOnboardingTicket = db.SystemConfigurations.FirstOrDefault().ActiveOnboardingTicket;
                                if (activeCreateOnboardingTicket == true && checkTerminal && !db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Any(t => t.CustomerCode == order.CustomerCode && t.OrderCode == order.OrdersCode && t.T_TicketTypeMapping.Any(x => x.TypeId == (long)UserContent.TICKET_TYPE.NuveiOnboarding)))
                                {
                                    await TicketViewService.AutoTicketScenario.NewTicketNuveiOnboarding(order.InvoiceNumber.ToString(), "<iframe  width='600' height='900' src='" + linkViewInvoice + "'></iframe>");
                                }
                            }
                            OrderViewService.WriteLogSalesLead(order, true, cMem);
                            if (order.Status != InvoiceStatus.Canceled.ToString())// || Request["Status"] == "Payment cleared"
                            {
                                string result = await new StoreViewService().CloseStoreService(order.OrdersCode, cus.StoreCode, cMem.FullName, true);
                            }
                            TempData["s"] = "Update success !";
                            string back = order.InvoiceNumber > 0 ? "/order" : "/order/estimates";

                            //auto closed if 0$
                            if (order.GrandTotal == 0 && order.Status == InvoiceStatus.Open.ToString())
                            {
                                var deploymentTypeId = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                                //var checkexistTicketDeployment = db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Where(x => x.OrderCode == order.OrdersCode && x.T_TicketTypeMapping.Any(y => y.TypeId == deploymentTypeId)).FirstOrDefault();
                                ////check exist ticket deployment 
                                //if (checkexistTicketDeployment == null)
                                //{
                                //    TempData["ShowCreateTicketDelivery"] = "true";
                                //}
                                if (list_device_ss.Count > 0)
                                {
                                    await ChangeInvoiceSatus(order.OrdersCode, InvoiceStatus.Paid_Wait.ToString());
                                }
                                else
                                {
                                    await ChangeInvoiceSatus(order.OrdersCode, InvoiceStatus.Closed.ToString());
                                }

                            }
                            await this.SendQuestionAreForm(ModelCodes, order.CustomerCode);
                            if (send_payment_email && string.IsNullOrEmpty(order.PartnerCode))
                            {
                                _ = ResendPaymentEmail(order.OrdersCode);
                            }

                            _logService.Info($"[Invoice][SaveOrder] completed save order edit #{order.OrdersCode}");
                            return RedirectToAction("EstimatesDetail", new { id = order.Id, url_back = back });


                        }
                        else
                        {
                            throw new Exception("Does not exist.");
                        }


                    }
                    else//Create
                    {
                        Random Rand = new Random();
                        var list_device_service = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                        var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
                        var sale_mn = Request["SalesMemberNumber"];
                        var ship_address = Request["sh_street"] + "|" + Request["sh_city"] + "|" + Request["sh_state"] + "|" + Request["sh_zip"] + "|" + Request["sh_country"];

                        #region CREATE ORDER
                        var new_order = OrderViewService.NewOrder(db, list_device_service, total_money_order, cMem, sale_mn, orderModel.CustomerCode, Request.Unvalidated["desc"], ship_address, orderModel.InvoiceDate, orderModel.DueDate, orderModel.Renewal);
                        new_order.Status = InvoiceStatus.Open.ToString();
                        new_order.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                        new_order.InvoiceNumber = long.Parse(new_order.OrdersCode);
                        new_order.PartnerCode = orderModel.PartnerCode;
                        new_order.CreateDeployTicket = bool.Parse(Request["CreateDeployTicket"] ?? "false");
                        //  new_order.CreateDeployTicket = orderModel.CreateDeployTicket;
                        db.O_Orders.Add(new_order);
                        var cus = db.C_Customer.Where(s => s.CustomerCode == new_order.CustomerCode).FirstOrDefault();

                        var _partner = db.C_Partner.FirstOrDefault(c => c.Code == orderModel.PartnerCode);
                        if (_partner != null)
                        {
                            cus.PartnerCode = _partner.Code;
                            cus.PartnerName = _partner.Name;
                            db.Entry(cus).State = EntityState.Modified;
                        }
                        #endregion
                        //add new  order_product
                        #region UPDATE DEVICES
                        var list_device_ss = (Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>).Where(d => d.Type == "device" || d.Type == "package").ToList();
                        var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                        List<string> ModelCodes = new List<string>();
                        foreach (var item in list_device_ss)
                        {
                            //tao order_product
                            if (item.BundleId > 0)
                            {
                                var mbs = db.I_Bundle_Device.Where(m => m.Bundle_Id == item.BundleId).ToList();
                                var package = new Order_Products();
                                package.Id = id++;
                                package.OrderCode = new_order.OrdersCode;
                                package.Price = item.PriceApply;
                                package.CreateBy = cMem.FullName;
                                package.CreateAt = DateTime.UtcNow;
                                package.BundleId = item.BundleId;
                                package.BundleQTY = item.Quantity;
                                package.Discount = item.Discount;
                                package.DiscountPercent = item.DiscountPercent;
                                package.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
                                db.Order_Products.Add(package);
                                foreach (var mb in mbs)
                                {
                                    var model = db.O_Product_Model.Find(mb.ModelCode);
                                    var new_device = new Order_Products();
                                    new_device.Id = id++;
                                    new_device.OrderCode = new_order.OrdersCode;
                                    new_device.ProductCode = model.ProductCode;
                                    new_device.ProductName = model.ProductName;
                                    new_device.Price = mb.Price ?? 0;
                                    new_device.Quantity = mb.Quantity * item.Quantity;
                                    new_device.CreateBy = cMem.FullName;
                                    new_device.CreateAt = DateTime.UtcNow;
                                    new_device.Feature = model.Color;
                                    new_device.BundleId = item.BundleId;
                                    new_device.ModelCode = model.ModelCode;
                                    new_device.ModelName = model.ModelName;
                                    new_device.BundleQTY = item.Quantity;
                                    ModelCodes.Add(model.ModelCode);
                                    db.Order_Products.Add(new_device);
                                }
                            }
                            else
                            {
                                var new_device = new Order_Products();
                                new_device.Id = id++;
                                new_device.OrderCode = new_order.OrdersCode;
                                new_device.ProductCode = item.ProductCode;
                                new_device.ProductName = item.ProductName;
                                new_device.Price = item.PriceApply;
                                new_device.Quantity = item.Quantity;
                                new_device.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
                                new_device.CreateBy = cMem.FullName;
                                new_device.CreateAt = DateTime.UtcNow;
                                new_device.Feature = item.Feature;
                                new_device.ModelCode = item.ModelCode;
                                new_device.ModelName = item.ModelName;
                                new_device.BundleId = null;
                                new_device.BundleQTY = null;
                                new_device.Discount = item.Discount;
                                new_device.DiscountPercent = item.DiscountPercent;
                                ModelCodes.Add(item.ModelCode);
                                db.Order_Products.Add(new_device);
                            }
                        }
                        #endregion

                        //add new order_license product & addon
                        #region UPDATE PRODUCT & ADDON
                        if (string.IsNullOrEmpty(cus.StoreCode) == true)
                        {
                            //new
                            cus.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(cus.CustomerCode, "[^.0-9]", "");
                            db.Entry(cus).State = EntityState.Modified;
                        }
                        var list_subsc_ss = (Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>)
                            .Where(d => (d.Type == "license" || d.Type == "addon" || d.Type == "giftcard" || d.Type == "other" || d.Type == "setupfee" || d.Type == "interactionfee")).ToList();
                        foreach (var product in list_subsc_ss)
                        {
                            var Renewal = Request["Renewal_" + product.SubscriptionId]?.ToString() == "true";
                            var ApplyDate = Request["ApplyDate_" + product.SubscriptionId]?.ToString() == "true";
                            var applyRecurring = Request["ApplyDiscountAsRecurring_" + product.SubscriptionId]?.ToString() == "true";
                            var product_info = db.License_Product.Where(p => p.Id == product.SubscriptionId).FirstOrDefault();
                            var NumberOfItem = db.License_Product_Item.Where(lp => lp.License_Product_Id == product.SubscriptionId).Count();
                            var sp = new Order_Subcription
                            {
                                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + Rand.Next(1, 9999).ToString().PadLeft(4, '0')),
                                Actived = true,
                                AutoRenew = product.SubscriptionDuration == "MONTHLY" ? Renewal : false,
                                //Price = product_info.Price,
                                PriceType = product.PriceType,
                                Promotion_Apply_Months = (product.PriceType == Store_Apply_Status.Promotional.ToString()) ? product.Promotion_Apply_Months : 0,
                                Price = product.PriceApply,
                                RecurringPrice = product.PriceApply,
                                Amount = product.Amount,
                                Period = product.SubscriptionDuration,
                                OrderCode = new_order.OrdersCode,
                                NumberOfItem = product.Type == "setupfee" || product.Type == "interactionfee" ? 0 : NumberOfItem,
                                ProductId = product_info.Id,
                                ProductName = product_info.Name,
                                PreparingDate = product.Type =="license"?  (product.PreparingDays >0 ? product.PreparingDays:0) :0,
                                Product_Code = product_info.Code,
                                Product_Code_POSSystem = product_info.Code_POSSystem,
                                PurcharsedDay = DateTime.UtcNow,
                                IsAddon = product_info.isAddon == true || product.Type == "setupfee" || product.Type == "interactionfee",
                                CustomerCode = new_order.CustomerCode,
                                CustomerName = new_order.CustomerName,
                                StoreCode = cus.StoreCode,
                                SubscriptionType = product.Type,
                                Quantity = product.Quantity,
                                Discount = product.Discount,
                                DiscountPercent = product.DiscountPercent,
                                ApplyDiscountAsRecurring = product.SubscriptionDuration == "MONTHLY" ? applyRecurring : false,
                                PeriodRecurring = product.PeriodRecurring,
                                SubscriptionQuantity = product.SubscriptionQuantity < 1 ? 1 : product.SubscriptionQuantity,
                                ApplyPaidDate = ApplyDate
                            };
                            //update date
                            string start_date = Request["Effective_start_date_" + product_info.Id];
                            sp.StartDate = string.IsNullOrEmpty(start_date) ? new_order.InvoiceDate : DateTime.Parse(start_date);
                            if (sp.Period == "MONTHLY")
                            {
                                string end_date = Request["Expiry_date_" + product_info.Id];
                                sp.EndDate = string.IsNullOrEmpty(end_date) ? sp.StartDate.Value.AddMonths(product.Quantity) : DateTime.Parse(end_date);
                                // if type equals trial, get month default of license
                                //if (sp.PriceType == Store_Apply_Status.Trial.ToString())
                                //{
                                //    sp.EndDate = sp.StartDate.Value.AddMonths(product_info.Trial_Months.Value);
                                //}
                                //else
                                //{
                                //    sp.EndDate = sp.StartDate.Value.AddMonths(product.Quantity);
                                //}
                            }
                            else if (sp.Period == "QUATERLY")
                            {
                                sp.EndDate = sp.StartDate.Value.AddMonths(product.Quantity * 3);
                            }
                            else if (sp.Period == "ANNUAL")
                            {
                                sp.EndDate = sp.StartDate.Value.AddYears(product.Quantity);
                            }
                            if (product.Type == "setupfee")
                            {
                                sp.EndDate = product.ExpiryDate;
                            }
                            db.Order_Subcription.Add(sp);
                        }

                        #endregion
                        if (list_device_ss.Count > 0)
                        {
                            new_order.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                        }
                        db.SaveChanges();
                        Trans.Commit();
                        Trans.Dispose();
                        var ReqURL = Request.Url.Authority;
                        var des = "<iframe  width='600' height='900' src='" + Request.Url.Scheme + "://" + ReqURL + "/order/ImportInvoiceToPDF?_code=" + new_order.OrdersCode + "&flag=Estimates'></iframe>";
                        await this.SendQuestionAreForm(ModelCodes, new_order.CustomerCode);
                        string create_invoice_rs = "";

                        if (action == "Create Invoice")
                        {
                            OrderViewService.WriteLogSalesLead(new_order, false, cMem, true);
                            var rs = await ChangeInvoiceSatus(new_order.OrdersCode, InvoiceStatus.Open.ToString(), false);
                            if (!(bool)((object[])rs.Data)[0])
                            {
                                create_invoice_rs = ((object[])rs.Data)[1].ToString();
                            }
                            string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + new_order.Id;
                            var checkTerminal = (from ordPrd in db.Order_Products
                                                 join prd in db.O_Product on ordPrd.ProductCode equals prd.Code
                                                 join prdmodel in db.O_Product_Model on ordPrd.ModelCode equals prdmodel.ModelCode
                                                 where ordPrd.OrderCode == new_order.OrdersCode && (prd.ProductLineCode == "terminal" || prdmodel.MerchantOnboarding == true)
                                                 select ordPrd).Any();
                            var activeCreateOnboardingTicket = db.SystemConfigurations.FirstOrDefault().ActiveOnboardingTicket;
                            if (activeCreateOnboardingTicket == true && checkTerminal && !db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Any(t => t.CustomerCode == new_order.CustomerCode && t.OrderCode == new_order.OrdersCode && t.T_TicketTypeMapping.Any(y => y.TypeId == (long)UserContent.TICKET_TYPE.NuveiOnboarding)))
                            {
                                await TicketViewService.AutoTicketScenario.NewTicketNuveiOnboarding(new_order.InvoiceNumber.ToString(), "<iframe  width='600' height='900' src='" + linkViewInvoice + "'></iframe>");
                            }
                        }
                        else if (new_order.InvoiceNumber > 0)
                        {
                            await OrderViewService.CheckMerchantWordDetermine(new_order.CustomerCode);
                        }
                        else
                        {
                            OrderViewService.WriteLogSalesLead(new_order, false, cMem);
                        }

                        if (create_invoice_rs == "")
                        {
                            TempData["s"] = (action == "Create Invoice") ? "Create Invoice successful" : "Create Estimete successful.";
                        }
                        else
                        {
                            TempData["e"] = "Close estimete fail: " + create_invoice_rs;
                        }
                        string back = (action == "Create Invoice" || new_order.InvoiceNumber > 0) ? "/order" : "/order/estimates";


                        //auto closed if 0$
                        if (new_order.GrandTotal == 0 && new_order.Status == InvoiceStatus.Open.ToString())
                        {
                            var deploymentTypeId = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                          
                            if (list_device_ss.Count > 0)
                            {
                                await ChangeInvoiceSatus(new_order.OrdersCode, InvoiceStatus.Paid_Wait.ToString());
                            }
                            else
                            {
                                await ChangeInvoiceSatus(new_order.OrdersCode, InvoiceStatus.Closed.ToString());
                            }

                        }
                        if (send_payment_email && string.IsNullOrEmpty(new_order.PartnerCode))
                        {
                            _ = ResendPaymentEmail(new_order.OrdersCode);
                        }
                        _logService.Info($"[Invoice][SaveOrder] completed save order create #{new_order.OrdersCode}");
                        return RedirectToAction("EstimatesDetail", new { id = new_order.Id, url_back = back });
                        //return RedirectToAction("Estimates");
                    }

                }
                catch (Exception ex)
                {
                    Trans.Dispose();
                    var url_back = Request["hd_url_back"] ?? "index";
                    TempData["e"] = ex.Message;
                    _logService.Error(ex, $"[Invoice][SaveOrder] error Save order");
                    return Redirect(url_back);
                }
            }
        }
        public async Task<JsonResult> ChangeInvoiceSatus(string code, string status, bool writelog = true)
        {
            _logService.Info($"[Invoice][ChangeInvoiceSatus] start Change invoice status: code = {code} | status = {status} | writelog = {writelog}");
            try
            {
                var result = await OrderViewService.ChangeInvoiceSatus(code, status, cMem, writelog);
                TempData["s"] = "Change status Completed";
                TempData["url_back"] = "/order/estimatesdetail/" + code;
                _logService.Info($"[Invoice][ChangeInvoiceSatus] completed Change invoice status", new { result = JsonConvert.SerializeObject(result) });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][ChangeInvoiceSatus] error Change invoice status: code = {code} | status = {status} | writelog = {writelog}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public async Task<JsonResult> PaymentLaterInvoice(string code, int days, bool writelog = true)
        {
            _logService.Info($"[Invoice][PaymentLaterInvoice] start to save payment later: code = {code} | days = {days} | writelog = {writelog}");
            try
            {
                if (access.Any(k => k.Key.Equals("orders_payment_later")) == false || access["orders_payment_later"] == false)
                    throw new Exception("You don't have permission.");

                var db = new WebDataModel();
                var order = db.O_Orders.Where(o => o.OrdersCode == code).FirstOrDefault();

                if (order == null)
                    throw new Exception("The order is not found!");
                order.DueDate = DateTime.UtcNow.AddDays(days);
                db.Order_Subcription.Where(os => os.OrderCode == order.OrdersCode && os.AutoRenew == true).ForEach(os =>
                {
                    os.AutoRenew = false;
                    db.Entry(os).State = EntityState.Modified;
                });
                db.Entry(order).State = EntityState.Modified;
                await db.SaveChangesAsync();
                //  TempData["ShowCreateTicketDelivery"] = "true";
                var result = await OrderViewService.ChangeInvoiceSatus(code, InvoiceStatus.PaymentLater.ToString(), cMem, writelog);
                TempData["s"] = "Update payment later completed";

                _logService.Info($"[Invoice][PaymentLaterInvoice] completed Payment Later Invoice", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(result) });
                return Json(result);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][PaymentLaterInvoice] error Payment Later Invoice: code = {code} | days = {days} | writelog = {writelog}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public async Task<JsonResult> UpdateBankInformation(C_Customer customer)
        {
            _logService.Info($"[Invoice][UpdateBankInformation] start to update Bank Information", new { customer = Newtonsoft.Json.JsonConvert.SerializeObject(customer) });
            try
            {
                var db = new WebDataModel();
                var cus_update = db.C_Customer.FirstOrDefault(c => c.CustomerCode == customer.CustomerCode);
                if (cus_update == null)
                    throw new Exception("Merchant not found");

                cus_update.DepositAccountNumber = customer.DepositAccountNumber;
                cus_update.DepositBankName = customer.DepositBankName;
                cus_update.DepositRoutingNumber = customer.DepositRoutingNumber;
                db.Entry(cus_update).State = EntityState.Modified;
                await db.SaveChangesAsync();

                _logService.Info($"[Invoice][UpdateBankInformation] completed Update Bank Information", new { cus_update = Newtonsoft.Json.JsonConvert.SerializeObject(cus_update) });
                return Json(new object[] { true, "Update bank information success" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][UpdateBankInformation] error Update Bank Information customer code = {customer.CustomerCode}");
                return Json(new object[] { false, ex.Message });
            }
        }
      
        public async Task<JsonResult> UpdatePayment(string invoiceCode, string paymentMethod, string paymentNote, string bankName, string cardNumber, string paymentDate, string status = "Paid/Wait", bool writeLog = true)
        {
            _logService.Info($"[Invoice][UpdatePayment] start Update Payment: invoiceCode = {invoiceCode} | paymentMethod = {paymentMethod} | bankName = {bankName} | cardNumber = {cardNumber} | paymentDate = {paymentDate} | status = {status} | writeLog = {writeLog}");
            try
            {
                var db = new WebDataModel();
                var order = db.O_Orders.FirstOrDefault(c => c.OrdersCode == invoiceCode);
                if (order == null) throw new Exception("Invoice not found");

                if (db.Order_Products.Any(p => p.OrderCode == invoiceCode) || UserContent.GetWebInfomation(true).AutoActiveRecurringLicense == false)
                    status = InvoiceStatus.Paid_Wait.ToString();
                else
                    status = InvoiceStatus.Closed.ToString();

                await OrderViewService.UpdatePayment(invoiceCode, paymentMethod, paymentNote, bankName, cardNumber, paymentDate, status, writeLog, cMem);
                TempData["s"] = "Update payment completed";

                _logService.Info($"[Invoice][UpdatePayment] completed Update Payment", new { order = Newtonsoft.Json.JsonConvert.SerializeObject(order) });
                return Json(new object[] { true });

            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex);
                TempData["e"] = "Error: " + ex.Message;
                _logService.Error(ex, $"[Invoice][UpdatePayment] error Update Payment: invoiceCode = {invoiceCode} | paymentMethod = {paymentMethod} | bankName = {bankName} | cardNumber = {cardNumber} | paymentDate = {paymentDate} | status = {status} | writeLog = {writeLog}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public async Task<JsonResult> ResendPaymentEmail(string order_code)
        {
            _logService.Info($"[Invoice][ResendPaymentEmail] start Resend Payment Email: order_code = {order_code}");
            try
            {
                var db = new WebDataModel();
                var order = db.O_Orders.Where(o => o.OrdersCode == order_code).FirstOrDefault();
                var cus = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();
                if (order == null)
                {
                    throw new Exception("The order is not found!");
                }
                if (cus == null)
                {
                    throw new Exception("Customer not found!");
                }
                if (order.Status == InvoiceStatus.Paid_Wait.ToString() || order.Status == InvoiceStatus.Closed.ToString())
                {
                    throw new Exception("This order has been paid!");
                }
                using (InvoiceService invoiceService = new InvoiceService())
                {
                    await invoiceService.SendMailConfirmPayment(cus, order, true);
                }

                _logService.Info($"[Invoice][ResendPaymentEmail] completed Resend Payment Email");
                return Json(new object[] { true, "Resend Payment email completed!" });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Invoice][ResendPaymentEmail] error Resend Payment Email: order_code = {order_code}");
                return Json(new object[] { false, e.Message });
            }
        }
        public async Task<JsonResult> ResendPaymentSMS(string order_code)
        {
            _logService.Info($"[Invoice][ResendPaymentSMS] start ResendPaymentSMS: order_code = {order_code}");
            try
            {
                var db = new WebDataModel();
                var order = db.O_Orders.Where(o => o.OrdersCode == order_code).FirstOrDefault();
                var cus = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();
                if (order == null)
                {
                    throw new Exception("The order is not found!");
                }
                if (cus == null)
                {
                    throw new Exception("Customer not found!");
                }
                if (order.Status == InvoiceStatus.Paid_Wait.ToString() || order.Status == InvoiceStatus.Closed.ToString())
                {
                    throw new Exception("This order has been paid!");
                }
                using (InvoiceService invoiceService = new InvoiceService())
                {
                    string msg = await invoiceService.SendSMSconfirmPayment(cus, order);
                    if (!string.IsNullOrEmpty(msg))
                        throw new Exception(msg);
                }

                _logService.Info($"[Invoice][ResendPaymentSMS] completed Resend Payment SMS");
                return Json(new object[] { true, "Send payment SMS completed!" });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Invoice][ResendPaymentSMS] error Resend Payment SMS: order_code = {order_code}");
                return Json(new object[] { false, e.Message });
            }
        }
        public async Task<JsonResult> FOC(string order_code, string desc)
        {
            _logService.Info($"[Invoice][FOC] start FOC: order_code = {order_code} | desc = {desc}");
            try
            {
                if (access.Any(k => k.Key.Equals("orders_payment_foc")) == false || access["orders_payment_foc"] == false)
                    throw new Exception("You don't have permission.");

                var db = new WebDataModel();
                var order = db.O_Orders.Where(o => o.OrdersCode == order_code).FirstOrDefault();

                if (order == null)
                {
                    throw new Exception("The order is not found!");
                }
                var cus = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();
                if (cus == null)
                {
                    throw new Exception("Customer not found!");
                }
                if (order.Status == InvoiceStatus.Paid_Wait.ToString() || order.Status == InvoiceStatus.Closed.ToString())
                {
                    throw new Exception("This order has been paid!");
                }
                var paylater_completed = (order.Status == InvoiceStatus.PaymentLater.ToString());
                var created = DateTime.Now.ToString("MM/dd/yyyy");
                //  TempData["ShowCreateTicketDelivery"] = "true";
                await OrderViewService.UpdatePayment(order.OrdersCode, "FOC", desc, "", "", created, InvoiceStatus.Paid_Wait.ToString(), true, cMem);
                if (paylater_completed)
                {
                    var license = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Type == "license" && s.RenewDate >= DateTime.UtcNow).FirstOrDefault();
                    if (license != null)
                    {
                        using (MerchantService service = new MerchantService(true))
                        {
                            var rs = await service.ApproveAction(license.StoreCode, license.Id, true, "same-active");
                        }
                    }
                }
                else
                {
                    var license = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode).FirstOrDefault();
                    if (UserContent.GetWebInfomation(true).AutoActiveRecurringLicense == true)
                    {
                        var old_recurring = db.Store_Services.FirstOrDefault(s => s.StoreCode == cus.StoreCode && s.Active == 1 && s.AutoRenew == true && s.RenewDate > DateTime.UtcNow && s.Type == "license");
                        var ls = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Type == "license" && s.Active == -1).FirstOrDefault();
                        if (ls != null)
                        {
                            var activelicense = ls.Id;
                            using (MerchantService service = new MerchantService(true))
                            {
                                activelicense = await service.ApproveAction(ls.StoreCode, ls.Id, true, "active");
                            }
                            if (ls.AutoRenew == true && order.PaymentMethod != "Recurring")
                            {
                                if (old_recurring != null) { PaymentService.SetStatusRecurring(old_recurring.Id, "inactive"); }
                                PaymentService.SetStatusRecurring(activelicense, "active");
                            }
                        }
                        var adds = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Type == "addon" && s.Active == -1).ToList();
                        if (adds.Count > 0)
                        {
                            using (MerchantService service = new MerchantService(true))
                            {
                                foreach (var add in adds)
                                {
                                    await service.ApproveAction(add.StoreCode, add.Id, true, "active");
                                }
                            }
                        }
                    }
                    PaymentService.UpdateRecurringStatus(order.CustomerCode);
                }

                await OrderViewService.CheckMerchantWordDetermine(order.CustomerCode, db);
                TempData["s"] = "Update payment completed";

                _logService.Info($"[Invoice][FOC] completed FOC");
                return Json(new object[] { true, "Resend Payment email completed!" });
            }
            catch (Exception e)
            {
                TempData["e"] = "Error: " + e.Message;
                _logService.Error(e, $"[Invoice][FOC] error FOC: order_code = {order_code} | desc = {desc}");
                return Json(new object[] { false, e.Message });
            }
        }
        public ActionResult TicketViewInvoice(long id, bool? showPrice = false)
        {
            _logService.Info($"[Invoice][TicketViewInvoice] start Ticket View Invoice: id = {id} | showPrice = {showPrice}");
            try
            {
                //flag: ["Estimates"/"Invoices"]
                WebDataModel db = new WebDataModel();
                var order_detail = db.O_Orders.Find(id);
                var flag = order_detail?.InvoiceNumber == null ? "estimetes" : "invoice";
                _logService.Info($"[Invoice][TicketViewInvoice] completed Ticket View Invoice");
                return ImportInvoiceToPDF(order_detail.OrdersCode, flag);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][TicketViewInvoice] error Ticket View Invoice: id = {id} | showPrice = {showPrice}");
                TempData["e"] = ex.Message;
            }

            return View();
        }
        /// <summary>
        /// Delete order
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Delete(long? id)
        {
            _logService.Info($"[Invoice][Delete] start Delete: id = {id}");
            WebDataModel db = new WebDataModel();
            using (var TranS = db.Database.BeginTransaction())
            {
                try
                {
                    var order = db.O_Orders.Find(id);
                    //Check access
                    if (access.Any(k => k.Key.Equals("orders_delete")) == false || access["orders_delete"] != true || order.Status == InvoiceStatus.Paid_Wait.ToString() || (order.Status == InvoiceStatus.Closed.ToString() && (access.Any(k => k.Key.Equals("orders_updateclose")) == false || access["orders_updateclose"] != true)))
                    {
                        return Redirect("/home/forbidden");
                    }
                    //.End


                    if (order != null)
                    {
                        //Remove assign item
                        var orderProducts = db.Order_Products.Where(p => p.OrderCode == order.OrdersCode).ToList();
                        if (orderProducts != null)
                        {
                            orderProducts.ForEach(o =>
                            {
                                using (var service = new DeploymentService())
                                {
                                    (o.InvNumbers ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(inv =>
                                    {
                                        service.unAssignedDevice(inv);
                                    });
                                }
                            });
                        }

                        //Inactive order service
                        var orderSub = db.Order_Subcription.Where(p => p.OrderCode == order.OrdersCode).ToList();
                        //if(orderSub != null)
                        //{
                        //    orderSub.ForEach(o => o.Actived = false);
                        //}

                        var storeSer = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode).ToList();
                        if (storeSer != null)
                        {
                            storeSer.ForEach(s => db.Store_Services.Remove(s));
                        }

                        //xoa order
                        order.IsDelete = true;
                        order.UpdatedHistory += "|" + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - By: " + cMem.FullName;
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                        TranS.Commit();
                        TranS.Dispose();
                    }

                    TempData["s"] = "Delete success!";
                }
                catch (Exception ex)
                {
                    TranS.Dispose();
                    _logService.Error(ex, $"[Invoice][Delete] error Delete: id = {id}");
                    TempData["e"] = ex.Message;
                }
            }
            _logService.Info($"[Invoice][Delete] completed Delete");
            return RedirectToAction("index");
        }
        public ActionResult Recovery(long? id)
        {
            _logService.Info($"[Invoice][Recovery] start Recovery: id = {id}");
            WebDataModel db = new WebDataModel();
            using (var TranS = db.Database.BeginTransaction())
            {
                try
                {
                    //Check access
                    if (access.Any(k => k.Key.Equals("orders_delete")) == false || access["orders_delete"] != true)
                    {
                        return Redirect("/home/forbidden");
                    }
                    var order = db.O_Orders.Find(id);
                    if (order == null)
                        throw new Exception("Invoice not found");

                    order.IsDelete = false;
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                    TranS.Commit();
                    TranS.Dispose();
                    TempData["s"] = "Recovery success!";
                }
                catch (Exception ex)
                {
                    TranS.Dispose();
                    _logService.Error(ex, $"[Invoice][Recovery] error Recovery: id = {id}");
                    TempData["e"] = ex.Message;
                }
            }
            _logService.Info($"[Invoice][Recovery] completed Recovery");
            return RedirectToAction("index");
        }
        #endregion

        #region Merchant

        /// <summary>
        /// Search merchant
        /// </summary>
        /// <param name="NameSearch">name search</param>
        /// <returns></returns>
        public JsonResult SearchMerchant(string NameSearch)
        {
            _logService.Info($"[Invoice] SearchMerchant {NameSearch}");
            try
            {
                WebDataModel db = new WebDataModel();

                var list_merchant = from c in db.C_Customer.Where(c => cMem.SiteId == 1 || cMem.SiteId == c.SiteId)
                                    where c.Active == 1
                                    select c;
                if (!string.IsNullOrEmpty(NameSearch))
                {
                    list_merchant = list_merchant.Where(x => x.BusinessName.Trim().Contains(NameSearch.Trim())
                                                            || x.OwnerName.Contains(NameSearch.Trim())
                                                            || x.CustomerCode.Contains(NameSearch.Trim())
                                                            || x.StoreCode.Contains(NameSearch.Trim())
                                                            || x.CellPhone.Contains(NameSearch.Trim())
                                                            || x.BusinessPhone.Contains(NameSearch.Trim())
                                                            || x.BusinessEmail.Contains(NameSearch.Trim()));
                }
                foreach (var item in list_merchant.OrderByDescending(c => c.Id).ToList())
                {
                    var us_state = db.Ad_USAState.Find(item.BusinessState);
                    if (us_state != null)
                    {
                        item.BusinessState = us_state.name;
                    }
                }
                return Json(new object[] { true, list_merchant.Count(), list_merchant });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][SearchMerchant] error Search Merchant: NameSearch = {NameSearch}");
                return Json(new object[] { false, ex.Message });
            }
        }

        /// <summary>
        /// Select merchant
        /// </summary>
        /// <param name="cus_code">Customer code</param>
        /// <returns></returns>
        public JsonResult SelectMerchant(string cus_code)
        {
            _logService.Info($"[Invoice] Select Merchant {cus_code}");
            try
            {
                WebDataModel db = new WebDataModel();
                var merchant = db.C_Customer.Where(c => c.CustomerCode == cus_code).FirstOrDefault();
                if(merchant==null)
                {
                    throw new Exception("Merchant does not exist.");
                }
                var SalesLead = db.C_SalesLead.Where(x => x.CustomerCode == merchant.CustomerCode).FirstOrDefault();
                if(SalesLead == null)
                {
                    SalesLead = new C_SalesLead();
                    SalesLead.Id = Guid.NewGuid().ToString();
                    SalesLead.CustomerCode = merchant.CustomerCode;
                    SalesLead.CustomerName = merchant.BusinessName;
                    SalesLead.L_SalonName = merchant.BusinessName;
                    SalesLead.SL_Status = LeadStatus.Lead.Code<int>();
                    SalesLead.SL_StatusName = LeadStatus.Lead.Text();
                    SalesLead.CreateAt = DateTime.UtcNow;
                    SalesLead.CreateBy = cMem.FullName;
                    SalesLead.CreateByMemberNumber = cMem.MemberNumber;
                    SalesLead.UpdateAt = DateTime.UtcNow;
                    SalesLead.UpdateBy = cMem.FullName;
                    SalesLead.L_Type = LeadType.RegisterOnIMS.Text();
                    db.C_SalesLead.Add(SalesLead);
                    db.SaveChanges();
                }
                if (string.IsNullOrEmpty(merchant.SalonAddress1)|| string.IsNullOrEmpty(merchant.SalonState)|| string.IsNullOrEmpty(merchant.SalonCity) || string.IsNullOrEmpty(merchant.SalonZipcode) || string.IsNullOrEmpty(merchant.BusinessCountry))
                {
                    return Json(new object[] { false, "please complete all required information", true, SalesLead.Id });
                }
                string SalesPerson = string.Empty;
                if (merchant != null)
                {
                    SalesPerson = SalesLead?.MemberNumber;
                }
                string PartnerCode = string.Empty;
                if (merchant != null)
                {
                    PartnerCode = merchant.PartnerCode; //db.O_Orders.Where(x => x.CustomerCode == merchant.CustomerCode && !string.IsNullOrEmpty(x.PartnerCode)).FirstOrDefault()?.PartnerCode;
                }
                var open = InvoiceStatus.Open.ToString();
                var old_estimet = db.O_Orders.Where(o => o.CustomerCode == merchant.CustomerCode && o.Status == open).FirstOrDefault();
                var LicenseActivated = (from s in db.Store_Services
                                        where s.Type == "license" && s.CustomerCode == cus_code && s.Active == 1
                                        join pr in db.License_Product on s.ProductCode equals pr.Code
                                        select new CurrentLicenseIsActivedCustomizeModel
                                        {
                                            ProductName = s.Productname,
                                            ProductCode = s.ProductCode,
                                            StoreServiceId = s.Id,
                                            EffectiveDate = s.EffectiveDate,
                                            Price = pr.Price ?? 0,
                                            RenewDate = s.RenewDate,
                                            Status = s.Active,
                                            Type = s.StoreApply
                                        }).FirstOrDefault();
                var partialLicenseIsActive = CommonFunc.RenderRazorViewToString("_LicenseIsActivePartial", LicenseActivated, this);
                if (old_estimet != null)
                {
                    return Json(new object[] { true, false, merchant, old_estimet.Id.ToString(), partialLicenseIsActive, SalesPerson, PartnerCode });
                }
                return Json(new object[] { true, true, merchant, "", partialLicenseIsActive, SalesPerson, PartnerCode });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][SearchMerchant] error Select Merchant: cus_code = {cus_code}");
                return Json(new object[] { false, ex.Message, false });
            }
        }

        #endregion

        #region Device

        /// <summary>
        /// Add device
        /// </summary>
        /// <param name="Key">Key: [add/update]</param>
        /// <param name="Code">Product code</param>
        /// <param name="Quantity">Product quantity</param>
        /// <param name="Price">Product price</param>
        /// <param name="Feature">Feature</param>
        /// <returns></returns>
        public JsonResult AddDevice(int Quantity, string ModelCode, long? BundleId)
        {
            _logService.Info($"[Invoice][AddDevice] Quantity:{Quantity} - ModelCode: {ModelCode}- BundleId: {BundleId}");
            try
            {
                WebDataModel db = new WebDataModel();
                var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
                var s_device = new Device_Service_ModelCustomize();
                var model = db.O_Product_Model.Find(ModelCode);
                var inventory_count = db.O_Device.Count(d => d.ModelCode == ModelCode && d.Inventory == 1);

                var list_product = new List<Device_Service_ModelCustomize>();
                if (Session[LIST_PRODUCT_SERVICE] != null)
                {
                    list_product = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;

                    s_device = list_product.Where(x => x.Type == "device" && x.ModelCode == ModelCode && x.BundleId == null).FirstOrDefault() ?? new Device_Service_ModelCustomize();
                    if (s_device != null && s_device.Quantity > 0)
                    {
                        //cap nhat so luong
                        s_device.Quantity = Quantity;
                        s_device.Amount = s_device.Quantity * s_device.Price;
                        s_device.ShortAge = s_device.Quantity > inventory_count ? true : false;
                    }
                    else
                    {
                        //Neu product duoc chon chua ton tai trong session thi tao moi
                        s_device.Key = (list_product.Max(p => p.Key) ?? 0) + 1;
                        s_device.Type = "device";
                        s_device.ProductCode = model.ProductCode;
                        s_device.ProductName = model.ProductName;
                        s_device.Picture = model?.Picture;
                        s_device.Quantity = Quantity;
                        s_device.Price = model.Price ?? 0;
                        s_device.Amount = s_device.Quantity * s_device.Price;
                        s_device.Feature = model.Color;
                        s_device.ModelCode = ModelCode;
                        s_device.ModelName = model.ModelName;
                        s_device.ShortAge = s_device.Quantity > inventory_count ? true : false;
                        list_product.Add(s_device);
                    }
                }
                Session[LIST_PRODUCT_SERVICE] = list_product;
                //tinh grand total
                total_money_order = CalculatorGrandToatal(total_money_order);
                return Json(new object[] { true, s_device, total_money_order, list_product });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][AddDevice] error AddDevice: Quantity = {Quantity} | ModelCode = {ModelCode} | BundleId = {BundleId}");
                return Json(new object[] { false, ex.Message });
            }
        }
        /// <summary>
        /// Delete device
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public JsonResult DeleteDevice(string model_code, long? BundleId)
        {
            _logService.Info($"[Invoice][DeleteDevice] model code:{model_code} - BundleId: {BundleId}");
            try
            {
                var list_product = new List<Device_Service_ModelCustomize>();
                var total_money_order = new TotalMoneyOrder();
                var product = new Device_Service_ModelCustomize();
                if (Session[LIST_PRODUCT_SERVICE] != null)
                {
                    list_product = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                    product = list_product.Where(p => (p.Type == "device" && p.BundleId == null && p.ModelCode == model_code)).FirstOrDefault();
                    if (product != null && product.Quantity > 0)
                    {
                        list_product.Remove(product);
                        Session[LIST_PRODUCT_SERVICE] = list_product;

                        //TINH TONG TIEN ORDER
                        total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
                        //cap nhat lai tong tien thiet bi
                        total_money_order.DeviceTotalAmount -= product.Price * product.Quantity;
                        //tinh grand total
                        total_money_order = CalculatorGrandToatal(total_money_order);
                    }
                }

                return Json(new object[] { true, list_product, total_money_order, product });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][DeleteDevice] error Delete Device: model_code = {model_code} | BundleId = {BundleId}");
                return Json(new object[] { false, ex.Message });
            }
        }

        #endregion

        #region Service - Disabled

        #endregion

        #region Bundle Device
        /// <summary>
        /// Select Bundle
        /// </summary>
        /// <param name="BundleId"></param>
        /// <returns></returns>
        public ActionResult SelectBundle(long? BundleId)
        {
            var list_bundle_device = new List<Device_Service_ModelCustomize>();
            List<string> out_stock = new List<string>();
            try
            {
                WebDataModel db = new WebDataModel();
                var _bundle = db.I_Bundle.Find(BundleId);
                ViewBag.BundleTotalAmount = _bundle?.Price?.ToString("$#,##0.##");
                ViewBag.BundleName = _bundle?.Name;
                var _bundle_device = db.I_Bundle_Device.Where(x => x.Bundle_Id == BundleId).ToList();

                if (_bundle_device != null && _bundle_device.Count > 0)
                {
                    foreach (var item in _bundle_device)
                    {
                        var _product_model = db.O_Product_Model.Find(item.ModelCode);
                        int _inventory = db.O_Device.Where(d => d.ModelCode == item.ModelCode && d.Inventory > 0 && d.Junkyard != true && d.Active == true).Count();
                        if (Session[LIST_PRODUCT_SERVICE] != null)
                        {
                            var list_product = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                            var _product = list_product.Where(p => p.Type == "package" && p.ModelCode == _product_model.ModelCode).FirstOrDefault();
                            _inventory = _inventory - (_product?.Quantity ?? 0);

                        }
                        var _device = new Device_Service_ModelCustomize()
                        {
                            BundleId = _bundle?.Id,
                            BundleName = _bundle?.Name,
                            Picture = _product_model?.Picture,
                            ProductCode = _product_model?.ProductCode,
                            ProductName = _product_model?.ProductName,
                            Type = "package",
                            //VendorId = _product_model.VendorId,
                            //VendorName = _product_model.VendorName,
                            ModelCode = item.ModelCode,
                            ModelName = item.ModelName,
                            Feature = _product_model?.Color,
                            Quantity = item.Quantity ?? 0,
                            Price = item.Price ?? 0,
                            Amount = (item.Quantity ?? 0) * (item.Price ?? 0),
                            Remaining_amount = _inventory,
                        };
                        list_bundle_device.Add(_device);
                    }
                }
                Session[LIST_BUNDLE_DEVICE] = list_bundle_device;
            }
            catch (Exception ex)
            {
                ViewBag.GetBundleError = ex.Message;
                _logService.Error(ex, $"[Invoice][SelectBundle] error Select Bundle: BundleId = {BundleId}");
            }
            return PartialView("_AddDeviceBundlePartial", list_bundle_device);
        }

        /// <summary>
        /// Add Bundle
        /// </summary>
        /// <param name="_BundleId"></param>
        /// <returns></returns>
        public JsonResult AddBundle(long? BundleId, int Quantity = 1)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                string _bundle_name = string.Empty;
                var list_product = new List<Device_Service_ModelCustomize>();
                var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;

                //var list_bundle_device = Session[LIST_BUNDLE_DEVICE] as List<Device_Service_ModelCustomize>;
                if (Session[LIST_PRODUCT_SERVICE] != null)
                {
                    list_product = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                }
                //add
                var bundle = db.I_Bundle.Find(BundleId);
                var bundle_models = (from d in db.I_Bundle_Device
                                     where d.Bundle_Id == BundleId
                                     join m in db.O_Product_Model on d.ModelCode equals m.ModelCode
                                     select new Bundle_Model_view
                                     {
                                         ProductName = m.ProductName,
                                         ModelCode = m.ModelCode,
                                         ModelName = m.ModelName,
                                         Color = m.Color,
                                         Quantity = d.Quantity,
                                         Price = d.Price
                                     }).ToList();
                if (list_product.Any(x => x.BundleId == BundleId) == true)//khi add bundle tu lan 2 tro di
                {
                    var bd = list_product.Where(x => x.BundleId == BundleId).FirstOrDefault();
                    bd.Quantity = Quantity;
                    bd.Amount = bd.Price * bd.Quantity;

                }
                else//khi add bundle lan dau
                {
                    var bd = new Device_Service_ModelCustomize
                    {
                        Amount = bundle.Price ?? 0,
                        BundleId = bundle.Id,
                        BundleName = bundle.Name,
                        Price = bundle.Price ?? 0,
                        Quantity = Quantity,
                        list_Bundle_Device = bundle_models,
                    };
                    list_product.Add(bd);
                }


                Session[LIST_PRODUCT_SERVICE] = list_product;

                //TINH TONG TIEN ORDER
                //tinh grand total
                total_money_order = CalculatorGrandToatal(total_money_order);
                return Json(new object[] { true, _bundle_name, bundle.Id, total_money_order, list_product });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][AddBundle] error Add Bundle: BundleId = {BundleId} | Quantity = {Quantity}");
                return Json(new object[] { false, ex.Message });
            }
        }

        /// <summary>
        /// Delet bundle
        /// </summary>
        /// <param name="_BundleId"></param>
        /// <returns></returns>
        public JsonResult DeleteBundle(long? _BundleId)
        {
            try
            {
                var total_money_order = new TotalMoneyOrder();
                var list_product = new List<Device_Service_ModelCustomize>();

                if (Session[LIST_PRODUCT_SERVICE] != null)
                {
                    list_product = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                    var list_bundel_device = list_product.Where(x => x.BundleId == _BundleId).ToList();
                    foreach (var item in list_bundel_device)
                    {
                        list_product.Remove(item);
                        Session[LIST_PRODUCT_SERVICE] = list_product;

                        //TINH TONG TIEN ORDER
                        total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
                        //cap nhat lai tong tien thiet bi
                        total_money_order.DeviceTotalAmount -= item.Price * item.Quantity;
                        //tinh grand total
                        total_money_order = CalculatorGrandToatal(total_money_order);
                    }
                }

                return Json(new object[] { true, _BundleId, list_product, total_money_order });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][DeleteBundle] error Delete Bundle: _BundleId = {_BundleId}");
                return Json(new object[] { false, ex.Message });
            }
        }
        #endregion

        #region Product subscription & Addon

        public ActionResult getPrdLicenseItemList(string prd_id, string order_code)
        {
            try
            {
                var db = new WebDataModel();
                var prd_item_list = (from pi in db.License_Product_Item
                                     where pi.License_Product_Id == prd_id && pi.Enable == true
                                     join lc in db.License_Item on pi.License_Item_Code equals lc.Code
                                     orderby lc.GroupID, lc.ID
                                     select new ProductItemPriceView
                                     {
                                         ID = pi.Id,
                                         License_Id = lc.ID,
                                         Name = lc.Name,
                                         Desc = lc.Description,
                                         Value = pi.Value,
                                         Type = lc.Type,
                                         CountWarning = pi.CountWarning,
                                     }).ToList();
                //ViewBag.prd_id = prd_id;
                return PartialView("_ProductDetailPartialByList", prd_item_list);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][getPrdLicenseItemList] error get Prd License Item List: prd_id = {prd_id}");
                return PartialView("_ProductDetailPartialByList", new List<ProductItemPriceView>());
            }
        }
        /// <summary>
        /// return list price type { {"Onetime",10.00} , {"Monthly", 20.00},...}
        /// </summary>
        /// <param name="enabled_item">enabled items idz</param>
        /// <param name="pr_id">product Id</param>
        /// <returns></returns>
        public JsonResult GetProduct(string id, int Qty = 1)
        {
            _logService.Info($"[Invoice][GetProduct] start GetProduct: id = {id} | Qty = {Qty}");
            try
            {
                var list_device_service = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                var db = new WebDataModel();
                var select_product = db.License_Product.Find(id);
                if (select_product.isAddon != true)
                {
                    list_device_service.Where(l => l.Type == "license" && l.SubscriptionId != id).ForEach(l =>
                    { l.Quantity = 0; l.Amount = 0; });
                }
                var subs = list_device_service.Where(i => i.SubscriptionId == id && (i.Type == "license" || i.Type == "addon" || i.Type == "other")).FirstOrDefault();
                if (subs == null)
                {
                    subs = new Device_Service_ModelCustomize
                    {
                        Type = select_product.Type,
                        SubscriptionId = select_product.Id,
                        SubscriptionName = select_product.Name,
                        SubscriptionDuration = select_product.SubscriptionDuration,
                        Price = select_product.Price ?? 0,
                        Quantity = Qty,
                        Amount = (select_product.Price ?? 0) * Qty,
                    };
                    list_device_service.Add(subs);
                }
                else
                {
                    subs.Quantity = Qty;
                    subs.Amount = Qty * subs.Price;
                }
                Session[LIST_PRODUCT_SERVICE] = list_device_service;
                TotalMoneyOrder total = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
                total.LicenseTotalAmount = list_device_service.Where(l => l.Type == "license" || l.Type == "addon" || l.Type == "other").Sum(l => l.Amount);
                Session[TOTAL_MONEY_ORDER] = total;
                _logService.Info($"[Invoice][GetProduct] completed GetProduct", new { subs = Newtonsoft.Json.JsonConvert.SerializeObject(subs) });
                return Json(subs);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][GetProduct] error GetProduct: id = {id} | Qty = {Qty}");
                return Json(new List<Device_Service_ModelCustomize>());
            }
        }

        public async Task<JsonResult> Renew(string id, DateTime? effect_date)
        {
            _logService.Info($"[Invoice][Renew] start Renew: id = {id} | effect_date = {effect_date}");
            try
            {
                var db = new WebDataModel();
                var s_services = db.Store_Services.Find(id);
                if (s_services == null)
                {
                    throw new Exception("Store service not found");
                }
                if (s_services.Period != "MONTHLY" && s_services.Type != LicenseType.GiftCard.Text())
                {
                    throw new Exception("This product can't renew");
                }
                if (s_services.hasRenewInvoiceIncomplete == true)
                {
                    throw new Exception("This product has already renew");
                }
                if (s_services.RenewDate < DateTime.Now.Date && effect_date == null)
                {
                    return Json(new object[] { false, true, "Subscription expired, Please select effectived date!" });
                }
                var result = await OrderViewService.RenewLicense(s_services, cMem, effect_date ?? s_services.RenewDate);

                _logService.Info($"[Invoice][Renew] completed Renew", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(result) });
                return Json(new object[] { true, result });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Invoice][Renew] error Renew: id = {id} | effect_date = {effect_date}");
                return Json(new object[] { false, false, e.Message });
            }
        }
        #endregion

        public async Task<JsonResult> add_addon(string addon_id, string cus_code, DateTime effect_date, DateTime? end_date, bool paymentNow, string paymentMethod,
            string paymentNote, string bankName, string cardNumber, string paymentDate, int month_qty = 1, bool autoRenew = false, string StoreApply = null, int quantity = 1)
        {
            _logService.Info($"[Invoice][add_addon] start add_addon", new
            {
                input = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    addon_id,
                    cus_code,
                    effect_date,
                    end_date,
                    paymentNow,
                    paymentMethod,
                    paymentNote,
                    bankName,
                    cardNumber,
                    paymentDate,
                    month_qty,
                    autoRenew,
                    StoreApply,
                    quantity
                })
            });
            try
            {
                var db = new WebDataModel();
                string newWordDetermine = string.Empty;
                var addon = db.License_Product.Find(addon_id);
                if (addon == null/* || addon.isAddon != true*/)
                {
                    throw new Exception("addon not found");
                }
                var customdate = false;
                if (Request != null)
                {
                    bool.TryParse(Request["customdate"] ?? "False", out customdate);
                }
                var result = await OrderViewService.AddAddon(cus_code, addon, cMem, effect_date, end_date, month_qty, autoRenew, StoreApply, quantity, customdate);
                var cus = db.C_Customer.First(_cus => _cus.CustomerCode == cus_code);

                //if (!MerchantType.STORE_IN_HOUSE.Code<string>().Equals(cus.Type))
                //{
                    var des = "<iframe  width='600' height='900' src='" + ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + result.OrdersCode + "&flag=Estimates'></iframe>";
                    await TicketViewService.AutoTicketScenario.NewTicketSalesLead(result.CustomerCode, result.OrdersCode, des);
                    db.SaveChanges();

                    using (InvoiceService invoiceService = new InvoiceService())
                    {
                        await invoiceService.SendMailConfirmPayment(cus, result);
                    }

                    if (paymentNow)
                    {
                        var payDate = paymentDate;
                        await UpdatePayment(result.OrdersCode, paymentMethod, paymentNote, bankName, cardNumber, payDate);
                    }
                //}

                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/customer/word_determine");

                if (node["trial"].InnerText.Equals(cus.WordDetermine))
                {
                    await OrderViewService.CheckMerchantWordDetermine(cus_code, db);
                    newWordDetermine = db.C_Customer.FirstOrDefault(_cus => _cus.CustomerCode == cus_code)?.WordDetermine;
                }

                _logService.Info($"[Invoice][add_addon] completed add_addon", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(result) });
                return Json(new object[] { true, addon.isAddon == true ? "Add add-on completed" : "Change product completed", newWordDetermine });
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Invoice][add_addon] error add_addon");
                return Json(new object[] { false, e });
            }
        }
        /// <summary>
        /// Change money
        /// </summary>
        /// <param name="DisAmount"></param>
        /// <param name="DisPercent"></param>
        /// <param name="ShippingFee"></param>
        /// <param name="TaxPercent"></param>
        /// <returns></returns>
        public JsonResult ChangeMoney(decimal? DisAmount, decimal? DisPercent, decimal? ShippingFee, decimal? TaxRate)
        {
            _logService.Info($"[Invoice][ChangeMoney] start Change Money: DisAmount = {DisAmount} | DisPercent = {DisPercent} | ShippingFee = {ShippingFee} | TaxRate = {TaxRate}");
            try
            {
                //var list_device_service = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
                total_money_order.DiscountAmount = DisAmount ?? 0;
                total_money_order.DiscountPercent = DisPercent ?? 0;
                total_money_order.ShippingFee = ShippingFee ?? 0;
                total_money_order.TaxRate = TaxRate ?? 0;

                //tinh grand total
                total_money_order = CalculatorGrandToatal(total_money_order);

                _logService.Info($"[Invoice][ChangeMoney] completed Change Money", new { total_money_order = Newtonsoft.Json.JsonConvert.SerializeObject(total_money_order) });
                return Json(new object[] { true, total_money_order });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][ChangeMoney] error ChangeMoney: DisAmount = {DisAmount} | DisPercent = {DisPercent} | ShippingFee = {ShippingFee} | TaxRate = {TaxRate}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public ActionResult Invoice_update_selected(string id, string type, int? qty, decimal? discount, string discount_type,
                                                    string priceType, string cusCode, string partnerCode, string orderCode, bool load_license = false,
                                                    bool autoRenew = false, DateTime? startDate = null, int qtySub = 0, DateTime? enddate = null, bool applypaiddate = false)
        {
            try
            {
                var db = new WebDataModel();
                var list_o_model = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                var price_partner = db.C_Partner.FirstOrDefault(c => c.Code == partnerCode)?.PriceType;
                var model = new Device_Service_ModelCustomize();
                var setupfee = new Device_Service_ModelCustomize();
                var interactionfee = new Device_Service_ModelCustomize();
                if (type == "all")
                {
                    list_o_model = list_o_model.Where(k => k.Type == "device" || k.Type == "package").ToList();
                    Session[LIST_PRODUCT_SERVICE] = list_o_model;
                }
                else if (type == "device")
                {
                    model = list_o_model?.Find(d => d.ModelCode == id);
                }
                else if (type == "package")
                {
                    var _id = long.Parse(id);
                    model = list_o_model?.Find(d => d.BundleId == _id);
                }
                else
                {
                    var order = db.O_Orders.FirstOrDefault(c => c.OrdersCode == orderCode);
                    bool changeEffectiveDate = startDate.HasValue || order == null || order?.Status == "Open";
                    model = list_o_model?.Find(d => d.SubscriptionId == id && d.Type == type);
                    if (model != null && model.Type != "setupfee" && model.Type != "interactionfee")
                    {
                        var RecurringPrice = Request["RecurringPrice"];
                        model.RecurringPrice = string.IsNullOrEmpty(RecurringPrice) ? model.RecurringPrice : decimal.Parse(RecurringPrice);
                        var oldStartDate = model.StartDate;
                        model.PriceType = priceType;
                        model.ApplyPaidDate = applypaiddate;
                        // reset
                        model.Price = 0;
                        model.SubscriptionQuantity = qtySub < 1 ? model.SubscriptionQuantity : qtySub;
                        if (changeEffectiveDate)
                        {
                            model.StartDate = model.ApplyPaidDate != true ? DateTime.UtcNow.Date : (startDate ?? model.StartDate);
                        }
                        DateTime nextDate = model.StartDate.Value.AddDays(model.PreparingDays < 0 ? 0 : model.PreparingDays);
                        if (!string.IsNullOrEmpty(model.PriceType))
                        {
                            if (model.PriceType.Split(',').Contains(Store_Apply_Status.Trial.Text()))
                            {
                                model.Price += 0;
                                //model.Quantity += model.TrialMonths;
                                nextDate = nextDate.AddMonths(model.TrialMonths);
                            }
                            if (model.PriceType.Split(',').Contains(Store_Apply_Status.Promotional.Text()))
                            {
                                model.Price += model.Promotion_Price ?? 0;
                                //model.Quantity += model.Promotion_Apply_Months;
                                nextDate = nextDate.AddMonths(model.Promotion_Apply_Months);
                            }
                            if (model.PriceType.Split(',').Contains(Store_Apply_Status.Real.Text()))
                            {
                                model.Price += (string.IsNullOrEmpty(partnerCode) ? model.RealPrice : (price_partner == "membership" ? model.MembershipPrice : (price_partner == "partner" ? model.PartnerPrice : model.RealPrice))) * model.SubscriptionQuantity;
                                //model.Quantity = model.NumberOfPeriod;
                                if (model.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(model.NumberOfPeriod * model.SubscriptionQuantity);
                                else if (model.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(model.NumberOfPeriod * 7 * model.SubscriptionQuantity);
                                else nextDate = nextDate.AddMonths(model.NumberOfPeriod * model.SubscriptionQuantity);
                            }
                            else
                            {
                                model.SubscriptionQuantity = 1;
                            }
                        }
                        else
                        {
                            model.Price = (string.IsNullOrEmpty(partnerCode) ? model.RealPrice : (price_partner == "membership" ? model.MembershipPrice : model.PartnerPrice)) * model.SubscriptionQuantity;
                            //model.Quantity =  qty ?? 1;
                            if (model.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(model.NumberOfPeriod * model.SubscriptionQuantity);
                            else if (model.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(model.NumberOfPeriod * 7 * model.SubscriptionQuantity);
                            else nextDate = nextDate.AddMonths(model.NumberOfPeriod * model.SubscriptionQuantity);
                        }

                        //if (model.SubscriptionQuantity > 1)
                        //{
                        //    //model.Price += (string.IsNullOrEmpty(partnerCode) ? model.RealPrice : (price_partner == "membership" ? model.MembershipPrice : model.PartnerPrice)) * (model.SubscriptionQuantity - 1);
                        //    if (model.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears((model.SubscriptionQuantity - 1) * model.NumberOfPeriod);
                        //    else if (model.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays((model.SubscriptionQuantity - 1) * model.NumberOfPeriod * 7);
                        //    else nextDate = nextDate.AddMonths((model.SubscriptionQuantity - 1) * model.NumberOfPeriod);
                        //}

                        //var nextTotalDays = model.StartDate.Value.AddMonths(model.Quantity);
                        //DateTime nextDate = model.StartDate.Value;
                        //if (model.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(model.Quantity);
                        //if (model.PeriodRecurring == RecurringInterval.Monthly.ToString()) nextDate = nextDate.AddMonths(model.Quantity);
                        //if (model.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(model.Quantity * 7);

                        if (model.SubscriptionDuration == "MONTHLY")
                        {
                            model.ApplyPaidDate = applypaiddate;
                            model.AutoRenew = autoRenew;
                            if (enddate != null) model.ExpiryDate = enddate;
                            else model.ExpiryDate = nextDate;
                        }

                        //model.ExpiryDate = model.Type == "license" || model.Type == "addon" ? new DateTime(nextTotalDays.Year, nextTotalDays.Month, DateTime.DaysInMonth(nextTotalDays.Year, nextTotalDays.Month)) : nextTotalDays;
                        //model.ExpiryDate = nextDate; //model.ExpiryDate.HasValue ? model.ExpiryDate : nextDate;
                        //model.AutoRenew = autoRenew;
                    }
                }
                //Nếu qty nhỏ hơn 1 => unselect
                if (qty < 1)
                {
                    list_o_model.Remove(model);
                    if (model?.PreparingDays >= 0)
                    {
                        setupfee = list_o_model?.FirstOrDefault(d => d.SubscriptionId == id && d.Type == "setupfee");
                        list_o_model.Remove(setupfee);
                        interactionfee = list_o_model?.FirstOrDefault(d => d.SubscriptionId == id && d.Type == "interactionfee");
                        list_o_model.Remove(interactionfee);
                    }
                }
                else
                {
                    //nếu model null thì là select mới (chưa set discount)
                    if (model == null)
                    {
                        model = new_select_product(id, type, qty ?? 1, cusCode, orderCode, partnerCode);
                        list_o_model.Add(model);
                        if (model.PreparingDays >= 0 && model.SetupFee > 0)
                        {
                            setupfee = new_setupfee_product(id);
                            list_o_model.Add(setupfee);
                        }
                        if (model.PreparingDays >= 0 && model.InteractionFee > 0)
                        {
                            interactionfee = new_setupfee_product(id, "interactionfee");
                            list_o_model.Add(interactionfee);
                        }

                        //nếu select subscription mới => unselect subscription cũ
                        if (type == "license")
                        {
                            var old_subs = list_o_model.Where(item => item.Type == "license" && item.SubscriptionId != id).FirstOrDefault();
                            list_o_model.Remove(old_subs);
                            if (old_subs?.PreparingDays >= 0)
                            {
                                setupfee = list_o_model?.FirstOrDefault(d => d.SubscriptionId == old_subs.SubscriptionId && d.Type == "setupfee");
                                list_o_model.Remove(setupfee);
                                interactionfee = list_o_model?.FirstOrDefault(d => d.SubscriptionId == old_subs.SubscriptionId && d.Type == "interactionfee");
                                list_o_model.Remove(interactionfee);
                            }
                        }
                    }
                    if (model.SubscriptionDuration != "MONTHLY")
                    {
                        model.Quantity = (qty == null ? model.Quantity : qty) ?? 1;
                        model.Price = model.PriceApply * model.Quantity;
                    }
                    model.DiscountType = string.IsNullOrEmpty(discount_type) && !string.IsNullOrEmpty(model.DiscountType) ? model.DiscountType : discount_type;
                    discount = Math.Abs(discount ?? 0);
                    if (model.SubscriptionDuration != "MONTHLY")
                    {
                        if (discount_type == "rate")
                        {
                            model.DiscountPercent = Math.Min(100, discount ?? 0);
                            model.Discount = (model.Price * model.DiscountPercent / 100);
                        }
                        else
                        {
                            model.DiscountPercent = 0;
                            model.Discount = Math.Min(discount ?? 0, model.Price);
                        }
                        model.Amount = model.Price - model.Discount;
                    }
                    else
                    {
                        if (discount_type == "rate")
                        {
                            model.DiscountPercent = Math.Min(100, discount ?? 0);
                            model.Discount = (model.Price * model.DiscountPercent / 100);
                        }
                        else if (discount_type == "amount")
                        {
                            model.DiscountPercent = 0;
                            model.Discount = Math.Min(discount ?? 0, model.Price);
                        }
                        model.Amount = model.Price - model.Discount;
                    }
                }

                var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
                //total_money_order.DeviceTotalAmount = list_o_model.Sum(d => d.Amount);
                //tinh grand total
                total_money_order = CalculatorGrandToatal(total_money_order);
                Session[LIST_PRODUCT_SERVICE] = list_o_model;
                ViewBag.p = access;
                var partial = CommonFunc.RenderRazorViewToString("_selected_product_partial", list_o_model.OrderByDescending(c => c.Type == "license"), this);
                var license_partial = string.Empty;
                if (load_license)
                {
                    var prd_item_list = (from pi in db.License_Product_Item
                                         where pi.Enable == true && pi.License_Product_Id == id
                                         join lc in db.License_Item on pi.License_Item_Code equals lc.Code
                                         orderby lc.GroupID, lc.ID
                                         select new ProductItemPriceView
                                         {
                                             ID = pi.Id,
                                             License_Id = lc.ID,
                                             Name = lc.Name,
                                             Desc = lc.Description,
                                             Value = pi.Value,
                                             Type = lc.Type,
                                             CountWarning = pi.CountWarning,
                                         }).ToList();
                    var ProductDetail = new DetailProductLicenseOrder
                    {
                        AutoRenew = model.AutoRenew,
                        TotalAmount = model.Amount,
                        Effective_StartDate = model.StartDate ?? DateTime.UtcNow,
                        ProductItemPriceView = prd_item_list,
                        TrialMonths = model.TrialMonths,
                        Promotion_Price = model.Promotion_Price,
                        RealPrice = model.RealPrice,
                        Promotion_Apply_Months = model.Promotion_Apply_Months,
                        PriceType = model.PriceType,
                        NumberOfPeriod = model.NumberOfPeriod,
                        Expiry_Date = model.ExpiryDate,
                        ProductType = model.Type,
                        Promotion_Apply_Status = model.Promotion_Apply_Status,
                        SubscriptionDuration = model.SubscriptionDuration,
                        SubscriptionId = model.SubscriptionId,
                        PeriodRecurring = model.PeriodRecurring,
                        SubscriptionQuantity = model.SubscriptionQuantity,
                        ApplyPaidDate = model.ApplyPaidDate,
                        RecurringPrice = model.RecurringPrice
                    };
                    license_partial = CommonFunc.RenderRazorViewToString("_ProductDetailPartial", ProductDetail, this);
                }
                return Json(new object[] { true, partial, total_money_order, license_partial, model?.TrialMonths, string.Format("{0:#,0.00}", model?.Promotion_Price), string.Format("{0:#,0.00}", model?.RealPrice), model?.Promotion_Apply_Months, model?.PriceType, model?.NumberOfPeriod });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][Invoice_update_selected] error Invoice_update_selected");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult Change_ApplyRecurring(string productId, bool applyRecurring = false)
        {
            _logService.Info($"[Invoice][Change_ApplyRecurring] start Change Apply Recurring: productId = {productId} | applyRecurring = {applyRecurring}");
            try
            {
                var list_o_model = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                var model = list_o_model?.Find(d => d.SubscriptionId == productId && d.Type != "setupfee" && d.Type != "interactionfee");
                if (model != null)
                {
                    model.ApplyDiscountAsRecurring = applyRecurring;
                    Session[LIST_PRODUCT_SERVICE] = list_o_model;
                }
                _logService.Info($"[Invoice][Change_ApplyRecurring] completed Change Apply Recurring");
                return Json(new object[] { true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][Change_ApplyRecurring] error Change Apply Recurring: productId = {productId} | applyRecurring = {applyRecurring}");
                return Json(new object[] { false, ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private Device_Service_ModelCustomize new_select_product(string id, string type, int qty, string cusCode, string orderCode, string partnerCode)
        {
            try
            {
                var dv = new Device_Service_ModelCustomize();
                dv.PreparingDays = -1;
                var db = new WebDataModel();
                var price_partner = db.C_Partner.FirstOrDefault(c => c.Code == partnerCode)?.PriceType;
                if (type == "license" || type == "addon" || type == "giftcard" || type == "other")
                {
                    var item = db.License_Product.Where(s => s.Id == id && (s.isAddon == true ^ type == "license")).FirstOrDefault();
                    if (item == null)
                    {
                        throw new Exception(type + " not found!");
                    }
                    dv.Type = item.Type;
                    dv.SubscriptionId = item.Id;
                    dv.SubscriptionName = item.Name;
                    dv.Quantity = item.NumberOfPeriod ?? 1;
                    dv.AutoRenew = false;
                    dv.SubscriptionDuration = item.SubscriptionDuration;
                    dv.RealPrice = item.Price ?? 0;
                    dv.TrialMonths = item.Trial_Months ?? 0;
                    dv.Promotion_Price = item.Promotion_Price ?? 0;
                    dv.NumberOfPeriod = item.NumberOfPeriod ?? 1;
                    string PriceType = string.Empty;
                    dv.Price = (string.IsNullOrEmpty(partnerCode) ? item.Price : (price_partner == "membership" ? item.MembershipPrice : price_partner == "partner" ? item.PartnerPrice : item.Price)) ?? item.Price ?? 0;
                    dv.PriceApply = (string.IsNullOrEmpty(partnerCode) ? item.Price : (price_partner == "membership" ? item.MembershipPrice : price_partner == "partner" ? item.PartnerPrice : item.Price)) ?? item.Price ?? 0;
                    dv.Promotion_Time_To_Available = item.Promotion_Time_To_Available ?? 0;
                    dv.PriceType = Store_Apply_Status.Real.Text();
                    dv.StartDate = DateTime.UtcNow.UtcToIMSDateTime();
                    dv.ApplyDiscountAsRecurring = false;
                    dv.MembershipPrice = item.MembershipPrice ?? 0;
                    dv.PartnerPrice = item.PartnerPrice ?? 0;
                    dv.PeriodRecurring = item.PeriodRecurring;
                    dv.SubscriptionQuantity = 1;
                    dv.SetupFee = item.ActivationFee ?? 0;
                    dv.InteractionFee = item.InteractionFee ?? 0;
                    dv.ApplyPaidDate = false;
                    dv.RecurringPrice = dv.Price;
                    if (item.SubscriptionDuration == "MONTHLY")
                    {
                        DateTime nextDate = DateTime.UtcNow;
                        if (dv.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(item.NumberOfPeriod.Value);
                        else if (dv.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(item.NumberOfPeriod.Value * 7);
                        else nextDate = nextDate.AddMonths(item.NumberOfPeriod.Value);
                        dv.ExpiryDate = nextDate;
                    }
                    //dv.ExpiryDate = item.Type == "license" || item.Type == "addon" ? new DateTime(nextTotalDays.Year, nextTotalDays.Month, DateTime.DaysInMonth(nextTotalDays.Year, nextTotalDays.Month)) : nextTotalDays;
                    var Promotion_Apply_Status = false;
                    var PromotionalStatus = Store_Apply_Status.Promotional.Text();
                    var Promotion_Apply_Months = 0;
                    Promotion_Apply_Months = db.License_Product.FirstOrDefault(x => x.Id == item.Id).Promotion_Apply_Months ?? 0;
                    var removeLicenseStatus = LicenseStatus.REMOVED.Code<int>();
                    var watingLicenseStatus = LicenseStatus.WAITING.Code<int>();
                    var FirstLicenseApply = db.Store_Services.Where(x => x.ProductCode == item.Code && x.CustomerCode == cusCode && x.Type == "license" && x.Active != removeLicenseStatus && x.Active != watingLicenseStatus).OrderBy(x => x.EffectiveDate).FirstOrDefault();
                    var LastLicenseApply = db.Store_Services.Where(x => x.ProductCode == item.Code && x.CustomerCode == cusCode && x.Type == "license" && x.Active != removeLicenseStatus && x.Active != watingLicenseStatus).OrderByDescending(x => x.RenewDate).FirstOrDefault();
                    dv.Promotion_Apply_Months = Promotion_Apply_Months;
                    if (dv.Promotion_Time_To_Available == 0)
                    {
                        Promotion_Apply_Status = true;
                    }
                    var FirstLicenseActived = db.Store_Services.Where(x => x.CustomerCode == cusCode && x.Type == "license" && x.Active != removeLicenseStatus && x.Active != watingLicenseStatus).OrderBy(x => x.EffectiveDate).FirstOrDefault();
                    if (FirstLicenseActived == null && item.Type == "license")
                    {
                        dv.ExpiryDate = dv.ExpiryDate.Value.AddDays(item.PreparingDays ?? 0);
                        dv.PreparingDays = item.PreparingDays ?? 0;
                    }

                    if (FirstLicenseApply != null && LastLicenseApply != null && Promotion_Apply_Status == false)
                    {
                        if (dv.Promotion_Time_To_Available <= (LastLicenseApply.RenewDate - FirstLicenseApply.EffectiveDate).Value.TotalDays / 30)
                        {
                            Promotion_Apply_Status = true;
                        }
                    }
                    dv.Promotion_Apply_Status = Promotion_Apply_Status;
                }
                else if (type == "device")
                {
                    var model = db.O_Product_Model.Find(id);
                    var inventory_count = db.O_Device.Count(d => d.ModelCode == id && d.Inventory == 1);
                    if (model == null)
                    {
                        throw new Exception(type + " not found!");
                    }
                    dv.Type = "device";
                    dv.ProductCode = model.ProductCode;
                    dv.ProductName = model.ProductName;
                    dv.Picture = model?.Picture;
                    dv.Feature = model.Color;
                    dv.ModelCode = model.ModelCode;
                    dv.ModelName = model.ModelName;
                    dv.Quantity = qty;
                    if (model.SalePrice != null && model.SalePrice > 0)
                    {
                        dv.Price = model.SalePrice.Value;
                    }
                    else
                    {
                        dv.Price = model.Price ?? 0;
                    }
                    dv.ShortAge = dv.Quantity > inventory_count ? true : false;
                    dv.PriceApply = dv.Price;
                }
                else if (type == "package")
                {
                    var _id = long.Parse(id);
                    var bundle = db.I_Bundle.Find(_id);
                    if (bundle == null)
                    {
                        throw new Exception(type + " not found!");
                    }
                    var bundle_models = (from d in db.I_Bundle_Device
                                         where d.Bundle_Id == _id
                                         join m in db.O_Product_Model on d.ModelCode equals m.ModelCode
                                         select new Bundle_Model_view
                                         {
                                             ProductName = m.ProductName,
                                             ModelCode = m.ModelCode,
                                             ModelName = m.ModelName,
                                             Color = m.Color,
                                             Quantity = d.Quantity,
                                             Price = d.Price
                                         }).ToList();
                    dv.Type = "package";
                    dv.Amount = bundle.Price ?? 0;
                    dv.BundleId = bundle.Id;
                    dv.BundleName = bundle.Name;
                    dv.Price = bundle.Price ?? 0;
                    dv.PriceApply = dv.Price;
                    dv.Quantity = qty;
                    dv.list_Bundle_Device = bundle_models;
                }
                else
                {
                    throw new Exception("Type " + type + " is Invalid!");
                }
                return dv;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][new_select_product] error new select product: id = {id} | type = {type} | qty = {qty} | cusCode = {cusCode} | orderCode = {orderCode} | partnerCode = {partnerCode}");
                throw new Exception(ex.Message);
            }
        }
        private Device_Service_ModelCustomize new_setupfee_product(string id, string type = "setupfee")
        {
            try
            {
                var dv = new Device_Service_ModelCustomize();
                var db = new WebDataModel();
                var item = db.License_Product.Where(s => s.Id == id).FirstOrDefault();
                if (item == null)
                {
                    throw new Exception("License not found!");
                }
                dv.Type = type;
                dv.SubscriptionId = item.Id;
                dv.SubscriptionName = item.Name;
                dv.Quantity = 1;
                dv.AutoRenew = false;
                dv.SubscriptionDuration = "ONETIME";
                dv.RealPrice = (type == "setupfee" ? item.ActivationFee : item.InteractionFee) ?? 0;
                dv.TrialMonths = 0;
                dv.Promotion_Price = 0;
                dv.NumberOfPeriod = 1;
                dv.Price = (type == "setupfee" ? item.ActivationFee : item.InteractionFee) ?? 0;
                dv.Promotion_Time_To_Available = 0;
                dv.PriceType = Store_Apply_Status.Real.Text();
                dv.StartDate = DateTime.UtcNow.UtcToIMSDateTime();
                var nextTotalDays = DateTime.UtcNow.AddDays(item.PreparingDays ?? 0);
                dv.ExpiryDate = nextTotalDays;
                dv.Promotion_Apply_Months = 0;
                dv.SubscriptionQuantity = 1;
                dv.Amount = 0;
                dv.PriceApply = dv.RealPrice;
                return dv;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][new_setupfee_product] error new_setupfee_product: id = {id} | type = {type}");
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// On blur qty model input
        /// </summary>
        /// <param name="code">Model code</param>
        /// <returns></returns>
        //public JsonResult ChangeQTYModel(string code, int qty)
        //{
        //    try
        //    {
        //        var list_o_model = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
        //        var model = list_o_model.Find(d => d.ModelCode == code);
        //        model.Quantity = qty;
        //        model.Amount = model.Quantity * model.Price;
        //        var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
        //        total_money_order.DeviceTotalAmount = list_o_model.Sum(d => d.Amount);
        //        //tinh grand total
        //        total_money_order = CalculatorGrandToatal(total_money_order);
        //        Session[LIST_PRODUCT_SERVICE] = list_o_model;
        //        return Json(new object[] { true, total_money_order, model });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new object[] { false, ex.Message });
        //    }
        //}
        //public JsonResult ChangeQTYPackage(long id, int qty)
        //{
        //    try
        //    {
        //        var list_o_model = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
        //        var model = list_o_model.Find(d => d.BundleId == id);
        //        model.Quantity = qty;
        //        model.Amount = model.Quantity * model.Price;
        //        var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
        //        total_money_order.DeviceTotalAmount = list_o_model.Sum(d => d.Amount);
        //        //tinh grand total
        //        total_money_order = CalculatorGrandToatal(total_money_order);
        //        Session[LIST_PRODUCT_SERVICE] = list_o_model;
        //        return Json(new object[] { true, total_money_order, model });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new object[] { false, ex.Message });
        //    }
        //}
        /// <summary>
        /// tinh Grand Total
        /// </summary>
        /// <param name="_total_money_order"></param>
        /// <param name="_total_amount"></param>
        /// <returns></returns>
        public TotalMoneyOrder CalculatorGrandToatal(TotalMoneyOrder _total_money_order, bool save_session = true)
        {
            try
            {
                var list_product = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
                list_product?.ForEach(item =>
                item.Amount = item.SubscriptionDuration == "MONTHLY" ? item.Price - item.Discount : item.Price - item.Discount
                );
                //_total_money_order.DeviceTotalAmount = 
                //////Discount áp dụng cho Subtotal, Tax chỉ áp dụng cho hardware
                //sub_total|sub_total string

                _total_money_order.SubTotal = list_product.Sum(item => item.Amount);
                _total_money_order.DiscountAmount = _total_money_order.DiscountAmount > _total_money_order.SubTotal ? _total_money_order.SubTotal : _total_money_order.DiscountAmount;
                _total_money_order.DiscountPercent = _total_money_order.DiscountPercent > 100 ? 100 : _total_money_order.DiscountPercent;
                var discount_amount = _total_money_order.DiscountAmount;
                if (_total_money_order.DiscountPercent > 0)
                {
                    discount_amount = (_total_money_order.SubTotal * _total_money_order.DiscountPercent / 100);
                }
                //grand_total|grand_total string
                var price_after_discount = _total_money_order.SubTotal - discount_amount;
                var taxrate_amount = (price_after_discount * _total_money_order.TaxRate / 100);
                _total_money_order.GrandTotal = price_after_discount + taxrate_amount + _total_money_order.ShippingFee;
                if (save_session)
                {
                    //update vao session
                    Session[TOTAL_MONEY_ORDER] = _total_money_order;
                }
                return _total_money_order;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][CalculatorGrandToatal] error Calculator Grand Toatal");
                return _total_money_order;
            }
        }
        /// <summary>
        /// Kiểm tra order đủ điều kiện chuyển sang completed không
        /// </summary>
        /// <param name="Order_code"></param>
        /// <returns></returns>
        //public bool CheckAllTicketCompleted(string Order_code)
        //{
        //    var db = new WebDataModel();
        //    bool Finance_completed = !db.T_SupportTicket.Any(t => t.OrderCode == Order_code && t.TypeId == 2 && t.StatusId != 8);
        //    //bool OnBoarding_completed = !db.T_SupportTicket.Any(t => t.OrderCode == Order_code && t.TypeId == 10 && t.StatusId != 60);
        //    bool Deployment_completed = !db.T_SupportTicket.Any(t => t.OrderCode == Order_code && t.TypeId == 4 && t.StatusId != 17);
        //    return Finance_completed /*&& OnBoarding_completed*/ && Deployment_completed;
        //}
        #region ajax
        public JsonResult GetOrderDeviceInfo(long id)
        {
            WebDataModel db = new WebDataModel();
            var info = db.Order_Products.Find(id);
            return Json(new object[] { true, info });
        }

        //public JsonResult CheckCompletable(string code)
        //{
        //    try
        //    {
        //        OrderViewController.CanCompletedInvoice(code);
        //        return Json(new object[] { true });
        //    }
        //    catch (Exception e)
        //    {
        //        var current_status = new WebDataModel().O_Orders.FirstOrDefault(o => o.OrdersCode == code)?.Status;
        //        return Json(new object[] { false, e.Message, current_status });
        //    }
        //}
        #endregion

        /// <summary>
        /// Cancel or Restore order
        /// </summary>
        /// <param name="Id">Order id</param>
        /// <param name="_Flag">[cancel/restore]</param>
        /// <returns></returns>
        public ActionResult CancelRestoreOrder(long? Id, string _Flag)
        {
            _logService.Info($"[Invoice][CancelRestoreOrder] start Cancel Restore Order: Id = {Id} | _Flag = {_Flag}");
            WebDataModel db = new WebDataModel();
            using (var TranS = db.Database.BeginTransaction())
            {
                try
                {
                    //Check access
                    if (access.Any(k => k.Key.Equals("orders_update")) == false || access["orders_update"] != true)
                    {
                        return Redirect("/home/forbidden");
                    }
                    //.End

                    var _order = db.O_Orders.Find(Id);
                    string message = string.Empty;
                    if (_order != null)
                    {
                        if (_Flag == "cancel")
                        {
                            var list_order_product = db.Order_Products.Where(p => p.OrderCode == _order.OrdersCode).ToList();
                            foreach (var item in list_order_product)
                            {
                                if (string.IsNullOrEmpty(item.InvNumbers) == false)
                                {
                                    using (var service = new DeploymentService())
                                    {
                                        (item.InvNumbers ?? "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList().ForEach(o =>
                                        {
                                            service.unAssignedDevice(o);
                                        });
                                    }
                                }
                            }

                            _order.Cancel = true;
                            _order.StatusHistory += "|Cancel invoice - By: " + cMem.FullName + " - At: " + DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt");
                            message = "Cancel order success.";
                        }
                        else if (_Flag == "restore")
                        {
                            _order.Cancel = false;
                            _order.Status = InvoiceStatus.Open.ToString();
                            _order.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                            _order.StatusHistory += "|Restore invoice - By: " + cMem.FullName + " - At: " + DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt");
                            message = "Restore order success.";
                        }

                        db.Entry(_order).State = EntityState.Modified;
                        db.SaveChanges();
                        TranS.Commit();
                        TranS.Dispose();

                        TempData["s"] = message;
                        _logService.Info($"[Invoice][CancelRestoreOrder] completed Cancel Restore Order");
                        return RedirectToAction("EstimatesDetail", new { id = Id });
                    }
                    else
                    {
                        throw new Exception("Order does not exist.");
                    }
                }
                catch (Exception ex)
                {
                    TranS.Dispose();
                    TempData["e"] = ex.Message;
                    _logService.Error(ex, $"[Invoice][CancelRestoreOrder] error Cancel Restore Order: Id = {Id} | _Flag = {_Flag}");
                    return RedirectToAction("index");
                }
            }
        }

        #region Send email Estimates & Invoices
        /// <summary>
        /// Send Etimates & Invoice page
        /// </summary>
        /// <param name="_code">Order code</param>
        /// <param name="flag">[Estimates/Invoices]</param>
        /// <returns></returns>
        public ActionResult SendEtimatesInvoice(string _code, string flag)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var order = db.O_Orders.Where(o => o.OrdersCode == _code && o.IsDelete != true && o.Cancel != true).FirstOrDefault();
                if (order != null)
                {
                    #region Create PDF
                    HtmlToPdf converter = new HtmlToPdf();
                    converter.Options.PdfPageSize = PdfPageSize.A4;
                    converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                    converter.Options.MarginLeft = 20;
                    converter.Options.MarginRight = 20;
                    converter.Options.MarginTop = 20;
                    converter.Options.MarginBottom = 20;

                    var ReqURL = Request.Url.Authority;
                    PdfDocument doc = converter.ConvertUrl("https://" + ReqURL + "/order/ImportInvoiceToPDF?_code=" + _code + "&flag=" + flag + "&_action=" + (string.IsNullOrEmpty(order.PartnerCode) ? "" : "render"));

                    string strFileName = flag + "_" + _code + ".pdf";
                    string strPath = Path.Combine(Server.MapPath("~/upload/InvoicePDF/"), strFileName);
                    doc.Save(strPath);
                    doc.Close();
                    #endregion

                    ViewBag.OrderId = order.Id;
                    ViewBag.FilePath = "/upload/InvoicePdf/" + flag + "_" + order.OrdersCode + ".pdf";

                    if (flag == "Estimates")
                    {
                        ViewBag.Title = "Send Estimate";
                        ViewBag.Url = "estimates";
                    }
                    else
                    {
                        ViewBag.Title = "Send Invoice";
                        ViewBag.Url = "invoices";
                    }

                    var customer = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode && c.Active == 1).FirstOrDefault();//[Active: 1"Active", 0"Inactive", -1"Not processing"]
                    return View(customer);
                }
                else
                {
                    throw new Exception("Not found");
                }
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                return RedirectToAction(flag == "estimates" ? "estimates" : "index");
            }
        }
        //send PDF
        public async Task<JsonResult> SendEmail(string to, string cc, string subject, string content, string file_url)
        {
            _logService.Info($"[Invoice][SendEmail] start Send Email: to = {to} | cc = {cc} | subject = {subject} | content = {content} | file_url = {file_url}");
            try
            {
                file_url = Path.Combine(Server.MapPath(file_url));
                var result = await _mailingService.SendBySendGrid(to, to, subject, content.Replace("\n", "<br/>"), cc, "", true, file_url);
                if (result == "")
                {
                    _logService.Info($"[Invoice][SendEmail] completed Send Email");
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception(result);
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][SendEmail] error Send Email: to = {to} | cc = {cc} | subject = {subject} | content = {content} | file_url = {file_url}");
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Import PDF
        /// <summary>
        /// Import Invoice To PDF
        /// </summary>
        /// <param name="_code">Order Code</param>
        /// <param name="flag">["Estimates"/"Invoices"]</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult ImportInvoiceToPDF(string _code, string flag, string title = "INVOICE", string _action = "")
        {
            _logService.Info($"[Invoice][ImportInvoiceToPDF] start Import Invoice To PDF: _code = {_code} | flag = {flag} | title = {title} | _action = { _action}");
            try
            {
                //flag: ["Estimates"/"Invoices"]
                WebDataModel db = new WebDataModel();
                var order_detail = db.O_Orders.Where(x => x.OrdersCode == _code).FirstOrDefault();
                ViewBag.Flag = flag;
                ViewBag.invoice_title = title;
                if (order_detail != null)
                {
                    ViewBag.Customer = db.C_Customer.Where(c => c.CustomerCode == order_detail.CustomerCode).FirstOrDefault() ?? new C_Customer();
                    ViewBag.CompanyInfo = db.SystemConfigurations.FirstOrDefault();
                    var _listOrderProduct = db.Order_Products.Where(p => p.OrderCode == order_detail.OrdersCode)
                        .Select(p => new Order_Products_view
                        {
                            order_model = p,
                            device_Infos = db.O_Device.Where(d => p.InvNumbers.Contains(d.InvNumber))
                            .Select(d => new Order_Products_view.device_info { inv_number = d.InvNumber, serial_number = d.SerialNumber }).ToList(),
                        }).ToList();
                    var ListOrderPackage = (from p in _listOrderProduct
                                            group p by p.order_model.BundleId into g_p
                                            join b in db.I_Bundle on g_p.Key equals b.Id into _b
                                            from b in _b.DefaultIfEmpty()
                                            select new Order_Package_view
                                            {
                                                Package = b,
                                                Products = g_p.ToList(),
                                            }).ToList();
                    ViewBag.ListOrderPackage = ListOrderPackage;
                    if (_listOrderProduct != null)
                    {
                        var tracking_number = db.Order_Carrier.Where(o => o.OrderCode == _code).FirstOrDefault();
                        ViewBag.tracking = tracking_number;
                    }
                    var listSubscription = db.Order_Subcription.Where(s => s.OrderCode == order_detail.OrdersCode && s.Actived == true).AsEnumerable();
                    var ListOrderSubcription = listSubscription.Join(db.License_Product, s => s.ProductId, p => p.Id, (s, p) => new VmOrderService
                    {
                        Product_Code = s.Product_Code,
                        Period = s.Period,
                        ProductName = s.ProductName,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        Price = s.Price,
                        Quantity = s.Quantity ?? 1,
                        Discount = s.Discount ?? 0,
                        DiscountPercent = s.DiscountPercent ?? 0,
                        Amount = s.Amount ?? (s.Period == "MONTHLY" ?
                                                    ((s.Price ?? 0) - (s.Discount ?? 0)) :
                                                    ((s.Price ?? 0) * (s.Quantity ?? 1) - (s.Discount ?? 0))
                                            ),
                        //Amount = s.Period == "MONTHLY" ?
                        //            ((s.Price ?? 0) * (s.SubscriptionQuantity ?? 1)) - ((s.DiscountPercent > 0 ? ((s.Price ?? 0) * (s.SubscriptionQuantity ?? 1) * s.DiscountPercent / 100) : os.Discount) ?? 0) :
                        //            ((os.Price ?? 0) * (os.Quantity ?? 1)) - ((os.DiscountPercent > 0 ? ((os.Price ?? 0) * (os.Quantity ?? 1) * os.DiscountPercent / 100) : os.Discount) ?? 0),
                        IsAddon = s.IsAddon,
                        SubscriptionType = s.SubscriptionType,
                        PeriodRecurring = s.PeriodRecurring,
                        SubscriptionQuantity = s.SubscriptionQuantity,
                        PriceType = s.PriceType ?? "",
                        ApplyPaidDate = s.ApplyPaidDate,
                        TrialMonths = p.Trial_Months ?? 0,
                        PreparingDate = s.PreparingDate??0,
                        Promotion_Apply_Months = p.Promotion_Apply_Months ?? 0
                    }).ToList();
                    ViewBag.ListOrderSubcription = ListOrderSubcription;

                    var listDevices = db.Order_Products.Where(c => c.OrderCode == order_detail.OrdersCode).ToList();
                    var _total_money_order = new TotalMoneyOrder { };
                    _total_money_order.DeviceTotalAmount = listDevices.Sum(c => c.TotalAmount) ?? 0;
                    _total_money_order.SubscriptionTotalAmount = ListOrderSubcription.Sum(c => c.Amount);
                    _total_money_order.SubTotal = _total_money_order.DeviceTotalAmount + _total_money_order.SubscriptionTotalAmount;
                    _total_money_order.ShippingFee = order_detail.ShippingFee ?? 0;
                    _total_money_order.DiscountAmount = order_detail.DiscountAmount ?? 0;
                    _total_money_order.DiscountPercent = _total_money_order.SubTotal * Convert.ToDecimal(order_detail.DiscountPercent ?? 0) / 100;
                    _total_money_order.TaxRate = (_total_money_order.SubTotal - _total_money_order.DiscountPercent - _total_money_order.DiscountAmount) * order_detail.TaxRate / 100 ?? 0;
                    _total_money_order.GrandTotal = _total_money_order.SubTotal + _total_money_order.TaxRate + _total_money_order.ShippingFee - _total_money_order.DiscountPercent - _total_money_order.DiscountAmount;
                    ViewBag._total_money_order = _total_money_order;
                    _logService.Info($"[Invoice][ImportInvoiceToPDF] completed Import Invoice To PDF", new { order_detail = Newtonsoft.Json.JsonConvert.SerializeObject(order_detail) });
                    return View(order_detail);
                }
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.Message;
                _logService.Error(ex, $"[Invoice][ImportInvoiceToPDF] error Import Invoice To PDF: _code = {_code} | flag = {flag} | title = {title} | _action = { _action}");
            }

            return RedirectToAction(flag == "Estimates" ? "estimates" : "index");
        }

        /// <summary>
        /// Convert HTML to PDF
        /// </summary>
        /// <param name="Url">Url page invoice</param>
        /// <param name="Flag">["Estimates"/"Invoices"]</param>
        /// <param name="_Code">Order COde</param>
        /// <returns></returns>
        public JsonResult ConvertHtmlToPdf(string Flag, string _Code)
        {
            try
            {
                //Flag: ["Estimates"/"Invoices"]
                HtmlToPdf converter = new HtmlToPdf();

                // set converter options
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.MarginLeft = 20;
                converter.Options.MarginRight = 20;
                converter.Options.MarginTop = 20;
                converter.Options.MarginBottom = 20;
                //converter.Options.WebPageWidth = webPageWidth;
                //converter.Options.WebPageHeight = webPageHeight;

                // create a new pdf document converting an html string
                var ReqURL = Request.Url.Authority;
                PdfDocument doc = converter.ConvertUrl("http://" + ReqURL + "/order/ImportInvoiceToPDF?_code=" + _Code + "&flag=" + Flag);

                // save pdf document
                string strFileName = Flag + "_" + _Code + ".pdf";
                string strPath = Path.Combine(Server.MapPath("~/upload/InvoicePDF/"), strFileName);
                doc.Save(strPath);
                var _url_return = "/upload/InvoicePDF/" + strFileName;

                // close pdf document
                doc.Close();

                return Json(new object[] { true, _url_return });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        #endregion

        //public ActionResult Email_SendGrid()
        //{
        //    try
        //    {
        //        //send SMS
        //        var sid = AppLB.SendSMS.Send_SMS("0399544336", "VN", "Hello! Loc Dinh");

        //        //send Email
        //        //var response = AppLB.SendEmail.SendEmailGrid();



        //        return View();
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Success = ex.Message;
        //        return View();
        //    }
        //}

        public ActionResult GetLicenseIsActive(string CustomerCode)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var LicenseActivated = (from s in db.Store_Services
                                        where s.Type == "license" && s.CustomerCode == CustomerCode && s.Active == 1
                                        join pr in db.License_Product on s.ProductCode equals pr.Code
                                        select new CurrentLicenseIsActivedCustomizeModel
                                        {
                                            ProductName = s.Productname,
                                            ProductCode = s.ProductCode,
                                            StoreServiceId = s.Id,
                                            EffectiveDate = s.EffectiveDate,
                                            RenewDate = s.RenewDate,
                                            Price = pr.Price ?? 0,
                                            Status = s.Active,
                                            Type = s.StoreApply
                                        }).FirstOrDefault();
                return PartialView("_LicenseIsActivePartial", LicenseActivated);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][GetLicenseIsActive] error Get License Is Active: CustomerCode = {CustomerCode}");
                return PartialView("_LicenseIsActivePartial", new List<CurrentLicenseIsActivedCustomizeModel>());
            }
        }
        public JsonResult GetSalesPerson(string orderCode)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var order = db.O_Orders.Where(x => x.OrdersCode == orderCode).FirstOrDefault();
                return Json(new { status = true, SalesMemberNumber = order.SalesMemberNumber }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][GetSalesPerson] error Get Sales Person: orderCode = {orderCode}");
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult ChangeSalesPerson(string orderCode, string salesPerson)
        {
            _logService.Info($"[Invoice][ChangeSalesPerson] start Change Sales Person: orderCode = {orderCode} | salesPerson = {salesPerson}");
            try
            {
                WebDataModel db = new WebDataModel();
                var order = db.O_Orders.Where(x => x.OrdersCode == orderCode).FirstOrDefault();
                if (order != null)
                {
                    var eStatus = Ext.EnumParse<InvoiceStatus>(order.Status);
                    if (eStatus >= InvoiceStatus.Closed)
                    {  //không thể đổi sale person nếu Invoice đã closed, kể cả admin(cho tới khi xong tính năng payroll)
                        throw new Exception("This invoice has been " + eStatus.Code<string>() + ", cannot change sales person!");
                    }

                    if (!string.IsNullOrEmpty(salesPerson))
                    {
                        var member = db.P_Member.Where(x => x.MemberNumber == salesPerson).FirstOrDefault();
                        order.SalesMemberNumber = member.MemberNumber;
                        order.SalesName = member.FullName;
                        // change assign member 
                        var tickets = db.T_SupportTicket.Where(x => x.OrderCode == order.OrdersCode);
                        List<string> ListDeparmentTicketByMember = new List<string>();
                        var ChirldDepartment = db.P_Department.Where(x => !string.IsNullOrEmpty(x.GroupMemberNumber)).ToList();
                        foreach (var dep in ChirldDepartment)
                        {
                            var listMember = dep.GroupMemberNumber.Split(',');
                            if (listMember.Contains(member.MemberNumber))
                            {
                                ListDeparmentTicketByMember.Add(dep.Id.ToString());
                            }
                        }

                        foreach (var t in tickets)
                        {
                            if (ListDeparmentTicketByMember.Any(x => x.Contains(t.GroupID.ToString())))
                            {
                                if (string.IsNullOrEmpty(t.AssignedToMemberNumber) || !(t.AssignedToMemberNumber.Contains(member.MemberNumber)))
                                {
                                    if (!string.IsNullOrEmpty(t.AssignedToMemberNumber))
                                    {
                                        t.AssignedToMemberNumber = t.AssignedToMemberNumber.Substring(t.AssignedToMemberNumber.Length - 1) == "," ? (t.AssignedToMemberNumber + member.MemberNumber + ",") : (t.AssignedToMemberNumber + "," + member.MemberNumber + ",");
                                    }
                                    else
                                    {
                                        t.AssignedToMemberNumber = member.MemberNumber + ",";
                                    }
                                    if (!string.IsNullOrEmpty(t.AssignedToMemberName))
                                    {
                                        t.AssignedToMemberName = t.AssignedToMemberName.Substring(t.AssignedToMemberName.Length - 1) == "," ? (t.AssignedToMemberName + member.FullName + ",") : (t.AssignedToMemberName + "," + member.FullName + ",");
                                    }
                                    else
                                    {
                                        t.AssignedToMemberName = member.FullName + ",";
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        order.SalesMemberNumber = null;
                        order.SalesName = null;
                    }
                    order.UpdatedBy = cMem.MemberNumber;
                    order.UpdatedAt = DateTime.UtcNow;
                    order.UpdatedHistory += "|" + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - By: " + cMem.FullName;



                    var notificationService = new NotificationService();
                    notificationService.OrderAddNotification(order.SalesMemberNumber, order.Id.ToString(), order.OrdersCode, cMem.FullName, cMem.MemberNumber);


                    db.SaveChanges();
                    OrderViewService.WriteLogSalesLead(order, true, cMem);
                }
                _logService.Info($"[Invoice][ChangeSalesPerson] completed Change Sales Person");
                return Json(new { status = true, message = "Change sales person success" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][ChangeSalesPerson] error Change Sales Person: orderCode = {orderCode} | salesPerson = {salesPerson}");
                return Json(new { status = false, message = ex.Message });
            }
        }
        // HandleTicketWhenChangeStatus
        //public void HandleTicketWhenChangeStatus(string CustomerCode, string OrderCode, string Status)
        //{
        //    WebDataModel db = new WebDataModel();
        //    switch (Status)
        //    {
        //        case "Open":
        //        case "Paid_Wait":
        //        case "Closed":
        //        case "Canceled":
        //            var FinanceTypeId = (long)UserContent.TICKET_TYPE.Finance;
        //            //var StatusFailedId = (long)UserContent.TICKET_STATUS.Finance_Failed;
        //            //var ticketFinance = db.T_SupportTicket.Where(x => x.CustomerCode == CustomerCode && x.OrderCode == OrderCode && x.TypeId == FinanceTypeId);
        //            //foreach (var t in ticketFinance)
        //            //{
        //            //    t.StatusId = StatusFailedId;
        //            //    t.StatusName = "Failed";
        //            //}

        //            var DeploymentTypeId = (long)UserContent.TICKET_TYPE.Deployment;
        //            var Deployment_CancelId = (long)UserContent.DeploymentTicket_Status.Cancel;
        //            var ticketDeployment = db.T_SupportTicket.Where(x => x.CustomerCode == CustomerCode && x.OrderCode == OrderCode && x.TypeId == DeploymentTypeId);
        //            foreach (var t in ticketDeployment)
        //            {
        //                t.StatusId = Deployment_CancelId;
        //                t.StatusName = "Cancel";
        //            }
        //            if (db.Order_Products.Where(x => x.OrderCode == OrderCode).Count() > 0)
        //            {
        //                var depsv = new DeploymentService();
        //                depsv.ClearDeployment(OrderCode);
        //            }

        //            break;
        //        default:
        //            break;

        //    }
        //}
        // Send questionare form if merchant onboarding 
        public async Task SendQuestionAreForm(List<string> ProductModelCode, string CustomerCode)
        {
            _logService.Info($"[Invoice][SendQuestionAreForm] start Send Question Are Form: CustomerCode = {CustomerCode}", new { ProductModelCode = Newtonsoft.Json.JsonConvert.SerializeObject(ProductModelCode) });
            try
            {
                WebDataModel db = new WebDataModel();
                var check = db.O_Product_Model.Where(x => ProductModelCode.Any(y => y.Contains(x.ModelCode)) && x.MerchantOnboarding == true).Count() > 0;
                var cus = db.C_Customer.Where(x => x.CustomerCode == CustomerCode).FirstOrDefault();
                if (check == true && cus.IsSendQuestionare == false)
                {
                    await this.SendQuestionareEmail(CustomerCode);
                    cus.IsSendQuestionare = true;
                }
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][SendQuestionAreForm] error Send Question Are Form: CustomerCode = {CustomerCode}");
            }
        }
        public async Task<JsonResult> SendQuestionareEmail(string code, bool sms = false)
        {
            _logService.Info($"[Invoice][SendQuestionareEmail] start Send Questionare Email: code = {code} | sms = {sms}");
            WebDataModel db = new WebDataModel();
            try
            {
                var cus = db.C_Customer.Where(c => c.CustomerCode == code).FirstOrDefault();
                if (cus == null)
                {
                    throw new Exception("This merchant is not found");
                }
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

                    //string to = string.Join(";", cus.Email, cus.BusinessEmail);
                    //string firstname = string.Join(";", cus.OwnerName, cus.BusinessName);

                    string to = string.Join(";", cus.SalonEmail);
                    string firstname = string.Join(";", cus.BusinessName);
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
                if (string.IsNullOrWhiteSpace(emailResult))
                {
                    // var ims_onboarding_ticket = db.T_SupportTicket.Where(t => t.TypeId == (long)UserContent.TICKET_TYPE.Sales
                    // && t.CustomerCode == code).OrderByDescending(t => t.Id).FirstOrDefault();
                    // if (ims_onboarding_ticket == null)
                    // {
                    //     string salonInfo = "<br/><h3 style='padding:5px;border-left:solid 5px red;'>SALON INFO</h3>" +
                    //"<table style='border:1px solid gray;border-collapse:collapse;'>" +
                    //"<tr><th style ='width:150px;padding:7px'>- Salon name:</th><td style='padding:7px'>" + cus.BusinessName +
                    //"</td></tr><tr><th style='padding:7px'>- Contact name:</th> <td style='padding:7px'>" + cus.OwnerName +
                    //"</td></tr><tr><th style='padding:7px'>- Contact number:</th> <td style='padding:7px'>" + cus.OwnerMobile +
                    //"</td></tr><tr><th style='padding:7px'>- Salon number:</th> <td style='padding:7px'>" + cus.BusinessPhone +
                    //"</td></tr><tr><th style='padding:7px'>- Salon Address:</th> <td style='padding:7px'>" + cus.BusinessAddressStreet + ", " + cus.BusinessCity + ", " + cus.BusinessState + ", " + cus.BusinessZipCode + ", " + cus.BusinessCountry +
                    //"</td></tr><tr><th style='padding:7px'>- Note:</th> <td style='padding:7px'>";
                    //     await TicketViewController.AutoTicketScenario.NewTicketNewSalon(cus, salonInfo);
                    // }
                    //if (ims_onboarding_ticket != null)
                    //{
                    //    string fbConent = "Questionare email has been sent to " + (string.IsNullOrWhiteSpace(cus.Email) == true ? cus.BusinessEmail : cus.Email);
                    //    fbConent += "<br/><a target='_blank' href='" + qnLink + "'>" + qnLink + "</a>";
                    //    ViewControler.TicketViewController.InsertFeedback(db, ims_onboarding_ticket.Id, "Questionare email has been sent.", fbConent, "", -1);
                    //}
                    _logService.Info($"[Invoice][SendQuestionareEmail] completed Email has ben sent");
                    return Json(new object[] { true, "Email has ben sent" });
                }


                if (!noticeSMS)
                {
                    throw new Exception("Owner's email is invalid or not found");
                }
                else
                {
                    _logService.Info($"[Invoice][SendQuestionareEmail] completed SMS has ben sent");
                    return Json(new object[] { true, "SMS has ben sent" });
                }

            }
            catch (Exception e)
            {
                _logService.Error(e, $"[Invoice][SendQuestionareEmail] error Send Questionare Email: code = {code} | sms = {sms}");
                return Json(new object[] { false, e.Message });
            }

        }
        #region report
        public int RoundCustom(int i, int nearest)
        {
            if (nearest <= 0 || nearest % 10 != 0)
                throw new ArgumentOutOfRangeException("nearest", "Must round to a positive multiple of 10");

            return (i + 5 * nearest / 10) / nearest * nearest;
        }
        [HttpPost]
        public JsonResult ChartReport(string Team, string Time, string salesPerson)
        {
            try
            {
                _logService.Info($"[Invoice][ChartReport] start Chart Report: Team = {Team} | Time = {Time} | salesPerson = {salesPerson}");
                WebDataModel db = new WebDataModel();
                DateTime currentTimeNow = DateTime.UtcNow;
                string title;
                string Paid_WaitStatus = InvoiceStatus.Paid_Wait.ToString();
                string ClosedStatus = InvoiceStatus.Closed.ToString();
                var query = from order in db.O_Orders
                            where order.Status == Paid_WaitStatus || order.Status == ClosedStatus
                            select new { order };

                //view all sales lead if permission is view all
                if (access.Any(k => k.Key.Equals("report_invoice_allteam")) == true && access["report_invoice_allteam"] == true)
                {
                    if (!string.IsNullOrEmpty(salesPerson))
                    {
                        if (salesPerson == "Unassigned")
                        {
                            query = query.Where(x => string.IsNullOrEmpty(x.order.SalesMemberNumber));
                        }
                        else
                        {
                            query = query.Where(x => x.order.SalesMemberNumber == salesPerson);
                        }
                    }
                    if (!string.IsNullOrEmpty(Team))
                    {
                        List<string> listMemberNumber = new List<string>();
                        long TeamId = long.Parse(Team);
                        var TeamFilter = db.P_Department.Find(long.Parse(Team));
                        listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.order.SalesMemberNumber)));
                    }
                }
                else
                {
                    List<string> listMemberNumber = new List<string>();
                    List<long> listTeam = new List<long>();
                    var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                    if (ManagerDep.Count() > 0)
                    {
                        foreach (var dep in ManagerDep)
                        {
                            listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                        }
                    }
                    var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                    if (currentDeparmentsUser.Count() > 0)
                    {
                        foreach (var deparment in currentDeparmentsUser)
                        {
                            listTeam.Add(deparment.Id);
                            if (deparment.LeaderNumber == cMem.MemberNumber)
                            {
                                listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                            }
                        }
                    }
                    listMemberNumber.Add(cMem.MemberNumber);
                    listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                    query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.order.SalesMemberNumber)));
                    if (!string.IsNullOrEmpty(salesPerson))
                    {
                        if (salesPerson == "Unassigned")
                        {
                            query = query.Where(x => string.IsNullOrEmpty(x.order.SalesMemberNumber));
                        }
                        else
                        {
                            query = query.Where(x => x.order.SalesMemberNumber == salesPerson);
                        }
                    }
                }
                switch (Time)
                {
                    //this month
                    case "current-month":
                        title = "REALITY INVOICE STATISTICS IN THIS MONTH";
                        query = query.Where(x => x.order.CreatedAt.Value.Month == currentTimeNow.Month && x.order.CreatedAt.Value.Year == currentTimeNow.Year);
                        var RangeNumber = Enumerable.Range(1, currentTimeNow.Day);
                        var label = new string[RangeNumber.Count()];
                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, currentTimeNow.Month, i + 1).ToString("dd ddd");
                        }
                        var numberInvoice = from m in RangeNumber
                                            join d in
                     query.OrderBy(gg => gg.order.CreatedAt.Value.Day)
                     .GroupBy(gg => gg.order.CreatedAt.Value.Day)
                     on m equals d.Key into gj
                                            from j in gj.DefaultIfEmpty()
                                            select j != null ? j.Count() : 0;
                        var income = from m in RangeNumber
                                     join d in
                                        query.OrderBy(gg => gg.order.CreatedAt.Value.Day)
                                        .GroupBy(gg => gg.order.CreatedAt.Value.Day)
                                        on m equals d.Key into gj
                                     from j in gj.DefaultIfEmpty()
                                     select j != null ? j.Sum(x => x.order.GrandTotal) : 0;

                        var IncomeData = income.Count() > 0 ? income.ToArray() : null;
                        var numberInvoiceData = numberInvoice.Count() > 0 ? numberInvoice.ToArray() : null;
                        //var resultMaxInvoice = numberInvoiceData.Max() + (numberInvoiceData.Max() * 0.2);
                        var resultMaxInvoice = this.RoundCustom((int)numberInvoiceData.Max(), 10);
                        var resultMaxIncome = this.RoundCustom((int)IncomeData.Max(), 10000);
                        _logService.Info($"[Invoice][ChartReport] completed Chart Report", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label }) });
                        return Json(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label });
                    //last month
                    case "last-month":
                        currentTimeNow = currentTimeNow.AddMonths(-1);
                        query = query.Where(x => x.order.CreatedAt.Value.Month == currentTimeNow.Month && x.order.CreatedAt.Value.Year == currentTimeNow.Year);
                        title = "REALITY INVOICE STATISTICS IN LAST MONTH";
                        RangeNumber = Enumerable.Range(1, DateTime.DaysInMonth(currentTimeNow.Year, currentTimeNow.Month));
                        label = new string[RangeNumber.Count()];

                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, currentTimeNow.Month, i + 1).ToString("dd ddd");
                        }
                        numberInvoice = from m in RangeNumber
                                        join d in
                                  query.OrderBy(gg => gg.order.CreatedAt.Value.Day)
                                  .GroupBy(gg => gg.order.CreatedAt.Value.Day)
                                  on m equals d.Key into gj
                                        from j in gj.DefaultIfEmpty()
                                        select j != null ? j.Count() : 0;
                        income = from m in RangeNumber
                                 join d in
                                    query.OrderBy(gg => gg.order.CreatedAt.Value.Day)
                                    .GroupBy(gg => gg.order.CreatedAt.Value.Day)
                                    on m equals d.Key into gj
                                 from j in gj.DefaultIfEmpty()
                                 select j != null ? j.Sum(x => x.order.GrandTotal) : 0;
                        IncomeData = income.Count() > 0 ? income.ToArray() : null;
                        numberInvoiceData = numberInvoice.Count() > 0 ? numberInvoice.ToArray() : null;
                        //var resultMaxInvoice = numberInvoiceData.Max() + (numberInvoiceData.Max() * 0.2);
                        resultMaxInvoice = this.RoundCustom((int)numberInvoiceData.Max(), 10);
                        resultMaxIncome = this.RoundCustom((int)IncomeData.Max(), 10000);
                        _logService.Info($"[Invoice][ChartReport] completed Chart Report", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label }) });
                        return Json(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label });
                    case "nearest-3-months":
                        currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-2);
                        query = query.Where(x => DbFunctions.TruncateTime(x.order.CreatedAt) >= currentTimeNow);
                        var ThreeMonth = new int[3] { DateTime.UtcNow.Month, DateTime.UtcNow.AddMonths(-1).Month, DateTime.UtcNow.AddMonths(-2).Month };
                        label = new string[3];
                        for (int i = 0; i < 3; i++)
                        {
                            label[i] = DateTime.UtcNow.AddMonths(-(i)).ToString("MMMM yyyy");
                        }
                        title = "REALITY INVOICE STATISTICS IN 3 MONTHS";
                        numberInvoice = from m in ThreeMonth
                                        join d in
                                  query.OrderBy(gg => gg.order.CreatedAt.Value.Month)
                                  .GroupBy(gg => gg.order.CreatedAt.Value.Day)
                                  on m equals d.Key into gj
                                        from j in gj.DefaultIfEmpty()
                                        select j != null ? j.Count() : 0;
                        income = from m in ThreeMonth
                                 join d in
                                    query.OrderBy(gg => gg.order.CreatedAt.Value.Month)
                                    .GroupBy(gg => gg.order.CreatedAt.Value.Month)
                                    on m equals d.Key into gj
                                 from j in gj.DefaultIfEmpty()
                                 select j != null ? j.Sum(x => x.order.GrandTotal) : 0;
                        IncomeData = income.Count() > 0 ? income.ToArray() : null;
                        numberInvoiceData = numberInvoice.Count() > 0 ? numberInvoice.ToArray() : null;
                        //var resultMaxInvoice = numberInvoiceData.Max() + (numberInvoiceData.Max() * 0.2);
                        resultMaxInvoice = this.RoundCustom((int)numberInvoiceData.Max(), 10);
                        resultMaxIncome = this.RoundCustom((int)IncomeData.Max(), 10000);
                        _logService.Info($"[Invoice][ChartReport] completed Chart Report", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label }) });
                        return Json(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label });
                    case "current-year":
                        query = query.Where(x => x.order.CreatedAt.Value.Year == currentTimeNow.Year);
                        RangeNumber = Enumerable.Range(1, currentTimeNow.Month);
                        label = new string[RangeNumber.Count()];
                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, i + 1, 1).ToString("MMMM yyyy");
                        }
                        title = "REALITY INVOICE STATISTICS IN THIS YEAR";
                        numberInvoice = from m in RangeNumber
                                        join d in
                                  query.OrderBy(gg => gg.order.CreatedAt.Value.Month)
                                  .GroupBy(gg => gg.order.CreatedAt.Value.Day)
                                  on m equals d.Key into gj
                                        from j in gj.DefaultIfEmpty()
                                        select j != null ? j.Count() : 0;
                        income = from m in RangeNumber
                                 join d in
                                    query.OrderBy(gg => gg.order.CreatedAt.Value.Month)
                                    .GroupBy(gg => gg.order.CreatedAt.Value.Month)
                                    on m equals d.Key into gj
                                 from j in gj.DefaultIfEmpty()
                                 select j != null ? j.Sum(x => x.order.GrandTotal) : 0;
                        IncomeData = income.Count() > 0 ? income.ToArray() : null;
                        numberInvoiceData = numberInvoice.Count() > 0 ? numberInvoice.ToArray() : null;
                        //var resultMaxInvoice = numberInvoiceData.Max() + (numberInvoiceData.Max() * 0.2);
                        resultMaxInvoice = this.RoundCustom((int)numberInvoiceData.Max(), 10);
                        resultMaxIncome = this.RoundCustom((int)IncomeData.Max(), 10000);
                        _logService.Info($"[Invoice][ChartReport] completed Chart Report", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label }) });
                        return Json(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label });
                    case "last-year":
                        currentTimeNow = currentTimeNow.AddYears(-1);
                        RangeNumber = Enumerable.Range(1, 12);
                        label = new string[RangeNumber.Count()];
                        for (int i = 0; i < RangeNumber.Count(); i++)
                        {
                            label[i] = new DateTime(currentTimeNow.Year, i + 1, 1).ToString("MMMM yyyy");
                        }
                        query = query.Where(x => x.order.CreatedAt.Value.Year == currentTimeNow.Year);
                        title = "REALITY INVOICE STATISTICS IN LAST YEAR";
                        numberInvoice = from m in RangeNumber
                                        join d in
                                  query.OrderBy(gg => gg.order.CreatedAt.Value.Month)
                                  .GroupBy(gg => gg.order.CreatedAt.Value.Day)
                                  on m equals d.Key into gj
                                        from j in gj.DefaultIfEmpty()
                                        select j != null ? j.Count() : 0;
                        income = from m in RangeNumber
                                 join d in
                                    query.OrderBy(gg => gg.order.CreatedAt.Value.Month)
                                    .GroupBy(gg => gg.order.CreatedAt.Value.Month)
                                    on m equals d.Key into gj
                                 from j in gj.DefaultIfEmpty()
                                 select j != null ? j.Sum(x => x.order.GrandTotal) : 0;
                        IncomeData = income.Count() > 0 ? income.ToArray() : null;
                        numberInvoiceData = numberInvoice.Count() > 0 ? numberInvoice.ToArray() : null;
                        //var resultMaxInvoice = numberInvoiceData.Max() + (numberInvoiceData.Max() * 0.2);
                        resultMaxInvoice = this.RoundCustom((int)numberInvoiceData.Max(), 10);
                        resultMaxIncome = this.RoundCustom((int)IncomeData.Max(), 10000);
                        _logService.Info($"[Invoice][ChartReport] completed Chart Report", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label }) });
                        return Json(new { numberInvoiceData, IncomeData, resultMaxInvoice, resultMaxIncome, title, label });
                    default:
                        throw new Exception("Time Report Incorrect");
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][ChartReport] error Chart Report: Team = {Team} | Time = {Time} | salesPerson = {salesPerson}");
                return Json(new { string.Empty });
            }
        }
        [HttpPost]
        public JsonResult GetDashboardReport(string Team, string Time, string salesPerson)
        {
            _logService.Info($"[Invoice][GetDashboardReport] start Get Dashboard Report: Team = {Team} | Team = {Team} | salesPerson = {salesPerson}");
            try
            {
                WebDataModel db = new WebDataModel();
                DateTime currentTimeNow = DateTime.UtcNow;
                string title;
                var query = from order in db.O_Orders
                            select order;

                if (access.Any(k => k.Key.Equals("report_invoice_allteam")) == true && access["report_invoice_allteam"] == true)
                {
                    if (!string.IsNullOrEmpty(salesPerson))
                    {
                        if (salesPerson == "Unassigned")
                        {
                            query = query.Where(x => string.IsNullOrEmpty(x.SalesMemberNumber));
                        }
                        else
                        {
                            query = query.Where(x => x.SalesMemberNumber == salesPerson);
                        }
                    }
                    if (!string.IsNullOrEmpty(Team))
                    {
                        List<string> listMemberNumber = new List<string>();
                        var TeamFilter = db.P_Department.Find(long.Parse(Team));
                        listMemberNumber.AddRange(TeamFilter.GroupMemberNumber.Split(','));
                        query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.SalesMemberNumber)));

                    }
                }
                else
                {
                    List<string> listMemberNumber = new List<string>();
                    var ManagerDep = db.P_Department.Where(x => x.SupervisorNumber == cMem.MemberNumber && x.ParentDepartmentId != null && x.Type == "SALES").ToList();
                    if (ManagerDep.Count() > 0)
                    {
                        foreach (var dep in ManagerDep)
                        {
                            listMemberNumber.AddRange(dep.GroupMemberNumber.Split(','));
                        }
                    }
                    var currentDeparmentsUser = db.P_Department.Where(d => d.Type == "SALES" && d.GroupMemberNumber.Contains(cMem.MemberNumber));
                    if (currentDeparmentsUser.Count() > 0)
                    {
                        foreach (var deparment in currentDeparmentsUser)
                        {
                            if (deparment.LeaderNumber == cMem.MemberNumber)
                            {
                                listMemberNumber.AddRange(deparment.GroupMemberNumber.Split(','));
                            }
                        }
                    }
                    listMemberNumber.Add(cMem.MemberNumber);
                    listMemberNumber = listMemberNumber.GroupBy(x => x).Select(x => x.Key).ToList();
                    query = query.Where(x => listMemberNumber.Any(y => y.Contains(x.SalesMemberNumber)));

                    if (!string.IsNullOrEmpty(salesPerson))
                    {
                        if (salesPerson == "Unassigned")
                        {
                            query = query.Where(x => string.IsNullOrEmpty(x.SalesMemberNumber));
                        }
                        else
                        {
                            query = query.Where(x => x.SalesMemberNumber == salesPerson);
                        }
                    }
                }
                switch (Time)
                {
                    case "current-month":
                        query = query.Where(x => x.CreatedAt.Value.Month == currentTimeNow.Month && x.CreatedAt.Value.Year == currentTimeNow.Year);
                        break;
                    case "last-month":
                        currentTimeNow = currentTimeNow.AddMonths(-1);
                        query = query.Where(x => x.CreatedAt.Value.Month == currentTimeNow.Month && x.CreatedAt.Value.Year == currentTimeNow.Year);
                        break;
                    case "nearest-3-months":
                        currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-3);
                        query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= currentTimeNow);
                        break;
                    case "current-year":
                        query = query.Where(x => x.CreatedAt.Value.Year == currentTimeNow.Year);
                        break;
                    case "last-year":
                        currentTimeNow = currentTimeNow.AddYears(-1);
                        query = query.Where(x => x.CreatedAt.Value.Year == currentTimeNow.Year);
                        break;
                    default:
                        break;
                }

                string OpenStatus = InvoiceStatus.Open.ToString();
                string Paid_WaitStatus = InvoiceStatus.Paid_Wait.ToString();
                string ClosedStatus = InvoiceStatus.Closed.ToString();
                string PaymentLaterStatus = InvoiceStatus.PaymentLater.ToString();
                string CanceledStatus = InvoiceStatus.Canceled.ToString();

                int NumberInvoiceAll = query.Where(x => x.Status != CanceledStatus).Count();
                int NumberOpenPaylater = query.Where(x => x.Status == OpenStatus || x.Status == PaymentLaterStatus).Count();
                int NumberReality = query.Where(x => x.Status == Paid_WaitStatus || x.Status == ClosedStatus).Count();

                decimal IncomeInvoiceAll = query.Where(x => x.Status != CanceledStatus).Sum(x => x.GrandTotal) ?? 0;
                decimal IncomeOpenPaylater = query.Where(x => x.Status == OpenStatus || x.Status == PaymentLaterStatus).Sum(x => x.GrandTotal) ?? 0;
                decimal IncomeReality = query.Where(x => x.Status == Paid_WaitStatus || x.Status == ClosedStatus).Sum(x => x.GrandTotal) ?? 0;
                _logService.Info($"[Invoice][GetDashboardReport] completed Get Dashboard Report", new
                {
                    result = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        NumberInvoiceAll,
                        NumberOpenPaylater,
                        NumberReality,
                        IncomeInvoiceAll = string.Format("{0:#,0.00}", IncomeInvoiceAll),
                        IncomeOpenPaylater = string.Format("{0:#,0.00}", IncomeOpenPaylater),
                        IncomeReality = string.Format("{0:#,0.00}", IncomeReality)
                    })
                });
                return Json(new
                {
                    NumberInvoiceAll,
                    NumberOpenPaylater,
                    NumberReality,
                    IncomeInvoiceAll = string.Format("{0:#,0.00}", IncomeInvoiceAll),
                    IncomeOpenPaylater = string.Format("{0:#,0.00}", IncomeOpenPaylater),
                    IncomeReality = string.Format("{0:#,0.00}", IncomeReality)
                });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][GetDashboardReport] error Get Dashboard Report: Team = {Team} | Team = {Team} | salesPerson = {salesPerson}");
                return Json(new { string.Empty });
            }
        }
        [HttpPost]
        public ActionResult Top10BestSeller(string Team, string Time, string SalesPerson)
        {
            _logService.Info($"[Invoice][Top10BestSeller] start Top 10 Best Seller: Team = {Team} | Time = {Time} | SalesPerson = {SalesPerson}");
            try
            {
                WebDataModel db = new WebDataModel();
                DateTime currentTimeNow = DateTime.UtcNow;
                string title = "";
                var query = db.O_Orders.Where(x => !string.IsNullOrEmpty(x.SalesMemberNumber));
                switch (Time)
                {
                    case "current-month":
                        query = query.Where(x => x.CreatedAt.Value.Month == currentTimeNow.Month && x.CreatedAt.Value.Year == currentTimeNow.Year);
                        title = "TOP 10 SALES AGENT IN THIS MONTH";
                        break;
                    case "last-month":
                        currentTimeNow = currentTimeNow.AddMonths(-1);
                        query = query.Where(x => x.CreatedAt.Value.Month == currentTimeNow.Month && x.CreatedAt.Value.Year == currentTimeNow.Year);
                        title = "TOP 10 SALES AGENT IN LAST MONTH";
                        break;
                    case "nearest-3-months":
                        currentTimeNow = new DateTime(currentTimeNow.Year, currentTimeNow.Month, 1).AddMonths(-3);
                        query = query.Where(x => DbFunctions.TruncateTime(x.CreatedAt) >= currentTimeNow);
                        title = "TOP 10 SALES AGENT IN  3 MONTHS";
                        break;
                    case "current-year":
                        query = query.Where(x => x.CreatedAt.Value.Year == currentTimeNow.Year);
                        title = "TOP 10 SALES AGENT IN THIS YEAR";
                        break;
                    case "last-year":
                        currentTimeNow = currentTimeNow.AddYears(-1);
                        query = query.Where(x => x.CreatedAt.Value.Year == currentTimeNow.Year);
                        title = "TOP 10 SALES AGENT IN LAST YEAR";
                        break;
                    default:
                        break;
                }
                string Paid_WaitStatus = InvoiceStatus.Paid_Wait.ToString();
                string ClosedStatus = InvoiceStatus.Closed.ToString();
                var Top10 = query.Where(x => x.Status == Paid_WaitStatus || x.Status == ClosedStatus).GroupBy(x => x.SalesMemberNumber).Select(x => new
                {
                    Name = x.FirstOrDefault().SalesName,
                    NumberOrder = x.Count(),
                    Income = x.Sum(y => y.GrandTotal)
                }).OrderByDescending(x => x.NumberOrder);
                var Top10Seller = Top10.Select(x => x.Name).ToArray();
                var Top10SellerInvoice = Top10.Select(x => x.NumberOrder).ToArray();
                var Top10SellerIncome = Top10.Select(x => x.Income).ToArray();
                var MaxYInvoice = Top10SellerInvoice.Max() + (Top10SellerInvoice.Max() * 0.2);
                var MaxYIncome = Top10SellerIncome.Max() + (int)(Top10SellerIncome.Max() * (decimal)0.1);
                _logService.Info($"[Invoice][Top10BestSeller] completed Top 10 Best Seller", new { result = Newtonsoft.Json.JsonConvert.SerializeObject(new { Top10Seller, Top10SellerInvoice, Top10SellerIncome, MaxYIncome, MaxYInvoice, title }) });
                return Json(new { Top10Seller, Top10SellerInvoice, Top10SellerIncome, MaxYIncome, MaxYInvoice, title });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][Top10BestSeller] error Top 10 Best Seller: Team = {Team} | Time = {Time} | SalesPerson = {SalesPerson}");
                return Json(new { string.Empty });
            }
        }


        public JsonResult UpdateDiscountSubscription(string storeservice_id, string discount_type, decimal discount_value, bool discount_status = false)
        {
            _logService.Info($"[Invoice][UpdateDiscountSubscription] start Update Discount Subscription: storeservice_id = {storeservice_id} | discount_type = {discount_type} | discount_value = {discount_value} | discount_status = {discount_status}");
            try
            {
                WebDataModel db = new WebDataModel();
                var ss = db.Store_Services.Find(storeservice_id);
                if (ss == null) throw new Exception("Packages not found.");
                string fb_title = "Discount package " + ss.Productname + " has been updated";
                string fb_content = string.Empty;
                var o_sub = db.Order_Subcription.FirstOrDefault(c => c.Product_Code == ss.ProductCode && c.OrderCode == ss.OrderCode) ?? new Order_Subcription { };
                if (!discount_status)
                {
                    fb_content = $"Discount status: <span class='text-danger'>off</span> <br/>";
                    ss.ApplyDiscountAsRecurring = false;
                    o_sub.ApplyDiscountAsRecurring = false;
                    db.Entry(ss).State = EntityState.Modified;
                    db.Entry(o_sub).State = EntityState.Modified;
                }
                else
                {
                    if (discount_type == "rate")
                    {
                        o_sub.DiscountPercent = discount_value;
                        o_sub.Discount = o_sub.Price * o_sub.DiscountPercent / 100;
                        fb_content = $"Discount status: <span class='text-success'>on</span><br/>" +
                                    $"Discount value: {o_sub.DiscountPercent}% <br/>";
                    }
                    else
                    {
                        o_sub.DiscountPercent = 0;
                        o_sub.Discount = discount_value > o_sub.Price ? o_sub.Price : discount_value;
                        fb_content = $"Discount status: <span class='text-success'>on</span><br/>" +
                                     $"Discount value: ${o_sub.Discount} <br/>";
                    }
                    db.Entry(o_sub).State = EntityState.Modified;
                }
                db.SaveChanges();
                var deployment_ticket = db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Where(t => t.T_TicketTypeMapping.Any(x => x.TypeId == (long)UserContent.TICKET_TYPE.Deployment) && t.CustomerCode == ss.CustomerCode).OrderByDescending(t => t.Id).FirstOrDefault();
                if (deployment_ticket != null)
                {
                    ViewControler.TicketViewService.InsertFeedback(db, deployment_ticket.Id, fb_title, fb_content, "", -1);
                }
                _logService.Info($"[Invoice][UpdateDiscountSubscription] completed Update Discount Subscription", new { o_sub = Newtonsoft.Json.JsonConvert.SerializeObject(o_sub) });
                return Json(new object[] { true, "Update success" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][UpdateDiscountSubscription] error Update Discount Subscription: storeservice_id = {storeservice_id} | discount_type = {discount_type} | discount_value = {discount_value} | discount_status = {discount_status}");
                return Json(new object[] { false, ex.Message });
            }
        }

        #endregion
        #region ticket
        public async Task<ActionResult> CreateDeliveryTicket(string orderCode)
        {
            _logService.Info($"[Invoice][CreateDeliveryTicket] start Create Delivery Ticket: orderCode = {orderCode}");
            try
            {
             
                await TicketViewService.AutoTicketScenario.NewTicketDeployment(orderCode);
                _logService.Info($"[Invoice][CreateDeliveryTicket] completed Create Delivery Ticket");
                return Json(new { status = true, message = "create delivery ticket success" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Invoice][CreateDeliveryTicket] error Create Delivery Ticket: orderCode = {orderCode}");
                return Json(new { status = true, message = ex.Message });
            }

        }
        #endregion
    
    }
    #region Model Customize
    public class TotalMoneyOrder
    {
        public decimal DeviceTotalAmount { get; set; }//tong tien product
        public decimal LicenseTotalAmount { get; set; }//tong tien service add-on
        // public decimal? LicenseSubcriptionFee { get; set; }
        public decimal MonthlyFee { get; set; }//tien main service
        public decimal ShippingFee { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TaxRate { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal SubTotal { get; set; }
        public decimal SubscriptionTotalAmount { get; internal set; }
    }

    //public class Feature_Group
    //{
    //    public string FeatureName { get; set; }
    //    public int Quantity_Total { get; set; }//tong so luong theo feature_name
    //    public string GroupName { get; set; }//[available/not available]
    //}
    public class Model_count_device
    {
        public string ModelCode { get; set; }
        public string ModelName { get; set; }
        public int DeviceCount { get; set; }
    }
    public class VendorCustomize
    {
        public long? VendorId { get; set; }
        public string VendorName { get; set; }
    }
    public class Product_Model_view
    {
        public O_Product_Model model { get; set; }
        public int remaining { get; set; }
    }

    #endregion



}