using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.ViewModel;
using Inner.Libs.Helpful;
using Newtonsoft.Json;

namespace EnrichcousBackOffice.Areas.PaymentGate.Controllers
{
    public class PurchasesController : Base
    {
        internal PurchasesService _purchases = new PurchasesService();
        internal CustomerService _customer = new CustomerService();

        public ActionResult Index(string license, string device, string key)
        {
            using (var db = new WebDataModel())
            {
                // List add-on
                ViewBag.Addons = db.License_Product.Where(l => l.Type == "addon").AsEnumerable().Where(l=>!l.Code.ToLower().Contains("setup")).ToList();
                // List device
                ViewBag.Devices = db.O_Product_Model.OrderBy(pd => pd.ProductName).ToList();
                
                // Agent check
                ViewBag.AgentKey = key; 
                ViewBag.Auth = AgentAction(key) != null || (Session[AreaPayConst.AUTH_BASIC_KEY]?.ToString() ?? "") != "";
                ViewBag.History = Session[PAYMENT_PURCHASES_HISTORY] as InvoiceRequest;
                if (Session[PAYMENT_PURCHASES_HISTORY] != null)
                {
                    var customerCode = "";
                    var agentStoreCode = AgentAction(key);
                    if (string.IsNullOrEmpty(agentStoreCode) == false)
                        customerCode = db.C_Customer.FirstOrDefault(cus => cus.StoreCode == agentStoreCode)?.CustomerCode;
                    var lastOrderCode = _purchases.LastOrderCode(customerCode, Session);
                    if (string.IsNullOrEmpty(lastOrderCode) == false)
                    {
                        ViewBag.LastOrder = lastOrderCode;
                        ViewBag.LastOrderBase64 = lastOrderCode.ToBase64();
                    }
                }
            }

            return View();
        }

        public JsonResult Login(string email, string password)
        {
            try
            {
                var cus = _customer.Login(email, password, Session);
                if (Session[PAYMENT_PURCHASES_HISTORY] != null)
                {
                    return Json(_purchases.LastOrderCode(cus.CustomerCode) ?? "");
                }
                return Json("");
            }
            catch (Exception e)
            {
                var msg = "Sorry, An error has occurred! Please try again later!";
                if (e is AppHandleException)
                {
                    msg = e.Message;
                }
                return new Func.JsonStatusResult(msg, HttpStatusCode.Forbidden);
            }
        }

        public async Task<JsonResult> NewInvoice([FromBody] InvoiceRequest request)
        {
            Session[PAYMENT_PURCHASES_HISTORY] = request;
            long customerId = -1;
            try
            {
                if (string.IsNullOrEmpty(request.AgentKey))
                    customerId = CustomerIdAuth();
            }
            catch (Exception e)
            {
                return new Func.JsonStatusResult("Please login first!", HttpStatusCode.Forbidden);
            }

            using (var db = new WebDataModel())
            {
                if (string.IsNullOrEmpty(request.AgentKey) == false)
                {
                    var storeCode = AgentAction(request.AgentKey.Trim());
                    customerId = db.C_Customer.FirstOrDefault(cus => cus.StoreCode == storeCode).Id;
                }

                try
                {
                    O_Orders order = _purchases.MakeNewOrder(customerId, request);
                    List<Order_Subcription> orderLicense = _purchases.MakeNewSubscriptions(customerId, request, order.OrdersCode);
                    List<Order_Products> orderDevices = _purchases.MakeNewDevices(customerId, request, order.OrdersCode);
                    // New Or Replace
                    if (string.IsNullOrEmpty(request.LastOrder) == false)
                    {
                        db.Order_Subcription.RemoveRange(db.Order_Subcription.Where(_license => _license.OrderCode == request.LastOrder));
                        db.Order_Products.RemoveRange(db.Order_Products.Where(_device => _device.OrderCode == request.LastOrder));
                        db.Entry(order).State = EntityState.Modified;
                    }
                    else
                    {
                        db.O_Orders.Add(order);
                    }

                    // Save invoice
                    orderLicense.ForEach(license => db.Order_Subcription.Add(license));
                    orderDevices.ForEach(device => db.Order_Products.Add(device));
                    await db.SaveChangesAsync();
                    if (string.IsNullOrEmpty(request.LastOrder)) {
                        var des = $"<iframe  width='600' height='900' src='${Request.Url.Scheme}://{Request.Url.Authority}/order/ImportInvoiceToPDF?_code=${order.OrdersCode}&flag=Estimates'></iframe>";
                        await TicketViewService.AutoTicketScenario.NewTicketSalesLead(order.CustomerCode, order.OrdersCode, des);
                    }
                    Session[PAYMENT_INVOICE_HISTORY] = order.OrdersCode;
                    // New store license
                    var cus = await db.C_Customer.FindAsync(customerId);
                    await new StoreServices(db).AddStoreLicense(order.OrdersCode);
                    // Send mail confirm
                    await new InvoiceService().SendMailConfirmPayment(cus, order, false, true);
                    var response = $"{order.OrdersCode.ToBase64()}";
                    if (string.IsNullOrEmpty(request.AgentKey))
                    {
                        cus?.CheckPassword();
                        response = $"{response}:{cus?.MD5PassWord.ToBase64()}";
                    }
                    await InventoryViewService.ConvertToInvoice(order, db, true, "IMS System");

                    //auto paid if grandtotal is 0$
                    if (order.GrandTotal == 0)
                    {
                        //update status invoice
                        await ChangeInvoiceSatus(order.OrdersCode, orderDevices.Count>0? InvoiceStatus.Paid_Wait.ToString(): InvoiceStatus.Closed.ToString());
                        // complete finance ticket
                        //var ticket = db.T_SupportTicket.FirstOrDefault(t => t.OrderCode == order.OrdersCode && t.TypeId == (long)UserContent.TICKET_TYPE.Finance);
                        // await TicketViewController.AutoTicketScenario.UpdateSatellite(ticket.Id);
                    }
                    return Json(response);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    return new Func.JsonStatusResult("Sorry, An error has occurred! Please try again later!", HttpStatusCode.BadRequest);
                }
            }
        }

        public async Task<JsonResult> ChangeInvoiceSatus(string code, string status)
        {
            WebDataModel db = new WebDataModel();
            using (var Trans = db.Database.BeginTransaction())
            {
                try
                {
                    var order = db.O_Orders.Where(o => o.OrdersCode == code).FirstOrDefault();

                    if (order != null)
                    {
                        order.UpdatedAt = DateTime.UtcNow;
                        order.UpdatedHistory += "|" + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - By: IMS System";

                        //update status history
                        var oldStatus = order.Status;
                        if (status != order.Status)
                        {
                            order.StatusHistory += "|" + status + "  - Update by: IMS System - At: " + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt");
                        }
                        order.Status = status;
                        if (order.InvoiceNumber == null)
                        {
                            await InventoryViewService.ConvertToInvoice(order, db, true, "IMS System");
                        }
                        if (order.InvoiceNumber > 0)
                        {
                            await OrderViewService.CheckMerchantWordDetermine(order.CustomerCode, db);
                            db.Entry(order).State = EntityState.Modified;

                        }
                        #region UPDATE SALE LEAD STATUS
                        var lead = db.C_SalesLead.Where(l => l.CustomerCode == order.CustomerCode).FirstOrDefault();
                        if (lead != null)
                        {
                            if (/*status == UserContent.ORDER_STATUS.Closed.ToString() && */lead.SL_Status != LeadStatus.Merchant.Code<int>())
                            {
                                lead.SL_Status = LeadStatus.Merchant.Code<int>();
                                lead.SL_StatusName = LeadStatus.Merchant.Text();
                                db.Entry(lead).State = EntityState.Modified;
                            }
                        }
                        #endregion
                        db.SaveChanges();
                        Trans.Commit();
                        Trans.Dispose();
                        var cus = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();
                        //if (status != UserContent.ORDER_STATUS.Lead.ToString() && status != UserContent.ORDER_STATUS.Lost.ToString() && status != UserContent.ORDER_STATUS.Draft.ToString())// || Request["Status"] == "Payment cleared"
                        //{
                        string result = await new StoreViewService().CloseStoreService(order.OrdersCode, cus.StoreCode, "IMS System", true);
                        if (result != "OK")
                        {
                            throw new Exception(result);
                        }
                        else if (status == InvoiceStatus.Open.ToString())
                        {
                            //await TicketViewController.AutoTicketScenario.UpdateTicketFromSatellite(order.OrdersCode, UserContent.TICKET_TYPE.Sales, "IMS System");
                            using (InvoiceService invoiceService = new InvoiceService())
                            {
                                await invoiceService.SendMailConfirmPayment(cus, order);
                            }
                        }
                        return Json(new object[] { true, order.Status, Ext.EnumParse<InvoiceStatus>(order.Status).ToString() });
                    }
                    else
                    {
                        throw new Exception("Invoice not found");
                    }
                }
                catch (Exception ex)
                {
                    return Json(new object[] { false, ex.Message });
                }
            }
        }
    }
}