using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Mvc;
using Enrich.Core.Infrastructure;
using Enrich.IServices;
using Enrich.IServices.Utils.GoHighLevelConnector;
using Enrich.IServices.Utils;
using Enrichcous.Payment.Mxmerchant.Utils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.ViewModel;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using Enrich.Core.Ultils;

namespace EnrichcousBackOffice.Areas.PaymentGate.Controllers
{
    public class PayController : Base
    {
        internal CardUtil cardUtil = new CardUtil();
        internal CustomerService _customer = new CustomerService();
        internal PaymentService _payment = new PaymentService();
        private readonly IMerchantService _merchantService;
        private readonly ILogService _logService;

        public PayController(IMerchantService merchantService, ILogService logService)
        {
            _merchantService = merchantService;
            _logService = logService;
        }

        // GET
        public async Task<ActionResult> Index(string key, string pc)
        {
            _logService.Info($"[PaymentGate][Pay-Index] start load Payment page: key = {key} | pc = {pc}");
            ViewBag.Error = SessionMsg(PAYMENT_ERROR);
            ViewBag.Info = SessionMsg(PAYMENT_INFO_MSG);
            O_Orders orders = null;
            try
            {
                Session[KEY_REF] = key;
                string agent = (Session[PURCHASES_AGENT] ?? "") as string;
                using (var db = new WebDataModel())
                {
                    C_Customer cus = null;
                    var keyPair = (key ?? "").Split(':');
                    var orderCode = keyPair[0].FromBase64();
                    if (keyPair.Length == 2 && string.IsNullOrEmpty(keyPair[0]) == false && string.IsNullOrEmpty(keyPair[1]) == false)
                    {
                        CustomerService.StoreSessionLogin(AuthParse(key));
                        AgentAction("");
                        agent = "";
                    }
                    else if (string.IsNullOrEmpty(agent))
                    {
                        key = (key ?? "").Split(':')[0];
                        Session[KEY_REF] = key;
                        AgentAction(key);
                        agent = key;
                    }
                    Session[PC_PAY] = pc;
                    orders = db.O_Orders.FirstOrDefault(o => o.OrdersCode == orderCode);
                    if (orders != null)
                    {
                        //check payment approve or invoice paid/close
                        var old_trans = db.C_CustomerTransaction.Where(c => c.OrdersCode == orders.OrdersCode).FirstOrDefault();
                        ViewBag.closedInvoice = orders.Status == InvoiceStatus.Closed.ToString() ||
                                                orders.Status == InvoiceStatus.Paid_Wait.ToString() ||
                                                old_trans?.PaymentStatus == "Approved" ||
                                                old_trans?.PaymentStatus == "Success";

                        var pay_pc = SecurityLibrary.Decrypt(pc);
                        if (string.IsNullOrEmpty(agent))
                            cus = CustomerAuth();
                        else
                            cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == orders.CustomerCode);
                        if (cus == null) throw new Exception();
                        ViewBag.Customer = cus;
                        if (!string.IsNullOrEmpty(pay_pc))
                        {
                            ViewBag.creditPartner = db.C_PartnerCard.Where(card => card.PartnerCode == pay_pc && card.Active).ToList().Where(card =>
                            {
                                try
                                {
                                    return true;
                                }
                                catch (Exception)
                                {
                                    return false;
                                }
                            }).ToList();
                        }
                        else
                        {
                            ViewBag.credit = db.C_CustomerCard.Where(card => card.CustomerCode == orders.CustomerCode && card.Active).ToList();
                        }

                        var _listOrderProduct = db.Order_Products.Where(p => p.OrderCode == orders.OrdersCode)
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
                        var listSubscription = db.Order_Subcription.Where(s => s.OrderCode == orders.OrdersCode && s.Actived == true).AsEnumerable();
                        var ListOrderSubcription = listSubscription.Join(db.License_Product.Where(c => c.Active == true), s => s.ProductId, p => p.Id, (s, p) => new VmOrderService
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
                            IsAddon = s.IsAddon,
                            SubscriptionType = s.SubscriptionType,
                            PeriodRecurring = s.PeriodRecurring,
                            SubscriptionQuantity = s.SubscriptionQuantity,
                            PriceType = s.PriceType ?? "",
                            ApplyPaidDate = s.ApplyPaidDate,
                            TrialMonths = p.Trial_Months ?? 0,
                            Promotion_Apply_Months = p.Promotion_Apply_Months ?? 0,
                            PreparingDate = s.PreparingDate,
                            Promotion_Price = p.Promotion_Price ?? 0
                        }).ToList();
                        ViewBag.ListOrderSubcription = ListOrderSubcription;
                        var listDevices = db.Order_Products.Where(c => c.OrderCode == orders.OrdersCode).ToList();
                        var _total_money_order = new TotalMoneyOrder { };
                        _total_money_order.DeviceTotalAmount = listDevices.Sum(c => c.TotalAmount) ?? 0;
                        _total_money_order.SubscriptionTotalAmount = ListOrderSubcription.Sum(c => c.Amount);
                        _total_money_order.SubTotal = _total_money_order.DeviceTotalAmount + _total_money_order.SubscriptionTotalAmount;
                        _total_money_order.ShippingFee = orders.ShippingFee ?? 0;
                        _total_money_order.DiscountAmount = orders.DiscountAmount ?? 0;
                        _total_money_order.DiscountPercent = _total_money_order.SubTotal * Convert.ToDecimal(orders.DiscountPercent ?? 0) / 100;
                        _total_money_order.TaxRate = (_total_money_order.SubTotal - _total_money_order.DiscountPercent - _total_money_order.DiscountAmount) * orders.TaxRate / 100 ?? 0;
                        _total_money_order.GrandTotal = _total_money_order.SubTotal + _total_money_order.TaxRate + _total_money_order.ShippingFee - _total_money_order.DiscountPercent - _total_money_order.DiscountAmount;
                        ViewBag._total_money_order = _total_money_order;


                        ViewBag.CompanyInfo = db.SystemConfigurations.FirstOrDefault();
                        ViewBag.country = db.Ad_Country.ToList();
                        ViewBag.cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == orders.CustomerCode);
                        if ((Request.Headers["Referer"]?.ToLower() ?? "").Contains("paymentgate/purchases"))
                        {
                            ViewBag.HadHistory = (Request.Headers["Referer"]?.ToLower() ?? "").Contains("paymentgate/purchases");
                            ViewBag.History = Request.Headers["Referer"];
                        }
                        ViewBag.states = db.Ad_USAState.ToList();
                        ViewBag.checkRecurring = db.Order_Subcription.Any(c => c.OrderCode == orders.OrdersCode && c.AutoRenew == true) ? 1:0;
                        ViewBag.SubscriptionList = db.Order_Subcription.Where(c => c.OrderCode == orders.OrdersCode && c.AutoRenew == true).ToList();
                    }

                    try
                    {
                        if (string.IsNullOrEmpty(agent) == false)
                        {
                            ViewBag.Auth = true;
                            ViewBag.AgentAction = true;
                        }
                        else
                        {
                            ViewBag.Auth = (Session[AreaPayConst.AUTH_BASIC_KEY]?.ToString() ?? "") != "";
                            if (ViewBag.Auth && CustomerIdAuth() > 0 && CustomerIdAuth() != cus.Id)
                            {
                                ViewBag.Auth = false;
                                BasicAuthClear();
                                ViewBag.Error = "Please use your account!";
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ViewBag.Auth = false;
                    }

                    ViewBag.Key = key;
                    ViewBag.Pc = pc;
                    ViewBag.CardSession = Session[PAYMENT_CARD_INFO] as TransRequest;
                }
                if (orders.GrandTotal == 0 && orders.Status != InvoiceStatus.Paid_Wait.ToString() && orders.Status != InvoiceStatus.Closed.ToString())
                {
                    var rs = await OrderViewService.UpdatePayment(orders.OrdersCode, "", "", "", "", "", InvoiceStatus.Closed.ToString());
                    if ((bool)rs.FirstOrDefault() == true)
                    {
                        orders.Status = InvoiceStatus.Closed.ToString();
                    }
                }
                if (UserContent.GetWebInfomation(true).AutoActiveRecurringLicense == true)
                {
                    List<Tuple<string, string, Boolean, string>> tmp = new List<Tuple<string, string, Boolean, string>>();
                    using (var db = new WebDataModel())
                    {

                        if (orders.Status == InvoiceStatus.Closed.ToString() && db.Store_Services.Any(o => o.OrderCode == orders.OrdersCode && o.Active == -1))
                        {
                            var licenseWaiting = db.Store_Services.Where(o => o.OrderCode == orders.OrdersCode && o.Active == -1 && o.Type != "other").ToList();
                            foreach (var add in licenseWaiting)
                            {
                                //xử lý khi active addon SMS khi dã kích hoạt SMS unlimited
                                if (add.Type == LicenseType.ADD_ON.Text())
                                {
                                    tmp.Add(Tuple.Create(add.StoreCode, add.Id, true, "active"));
                                }
                            }
                        }
                    }


                    //xử lý khi active addon SMS khi dã kích hoạt SMS unlimited


                    if (tmp.Count > 0)
                    {
                        foreach (var add in tmp)
                        {
                            using (MerchantService service = new MerchantService(true))
                            {
                                await service.ApproveAction(add.Item1, add.Item2, add.Item3, "active");
                            }
                        }

                    }
                    //PaymentService.UpdateRecurringStatus();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                _logService.Error(e, $"[PaymentGate][Pay-Index] error load Payment page: key = {key} | pc = {pc}");
                //orders = null;
            }
            _logService.Info($"[PaymentGate][Pay-Index] completed load Payment page", new { orders = Newtonsoft.Json.JsonConvert.SerializeObject(orders) });
            return View(orders);
        }

        /// <summary>
        /// Cancel payment
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Cancel(string key)
        {
            return View();
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public ActionResult Login(string email, string password, string key)
        {
            try
            {
                C_Customer.Login(email, password, Session);
            }
            catch (Exception e)
            {
                MakeErrorResponse(e, PAYMENT_ERROR);
            }
            return RedirectToAction("Index", new { key = key });
        }

        /// <summary>
        /// ChangeDefaultCard
        /// </summary>
        /// <param name="cardId"></param>
        /// <returns></returns>
        public JsonResult ChangeDefaultCard(string cardId)
        {
            _logService.Info($"[PaymentGate][Pay-ChangeDefaultCard] start Change Default Card: cardId = {cardId}");
            try
            {
                using (var db = new WebDataModel())
                {
                    //new default card
                    C_CustomerCard newdefault_card = db.C_CustomerCard.Find(cardId);
                    if (newdefault_card == null) throw new Exception("Card not found");
                    //current default card
                    var current_card = db.C_CustomerCard.FirstOrDefault(c => c.CustomerCode == newdefault_card.CustomerCode && c.IsDefault == true) ?? new C_CustomerCard { };
                    //inactive all services Active of current default card
                    db.Store_Services.Where(s => s.AutoRenew == true && s.MxMerchant_cardAccountId == current_card.MxMerchant_Id).ToList().ForEach(s =>
                    {
                        PaymentService.SetStatusRecurring(s.Id, "inactive");
                    });
                    newdefault_card.IsDefault = true;
                    newdefault_card.IsRecurring = true;
                    db.Entry(newdefault_card).State = EntityState.Modified;
                    if (!string.IsNullOrEmpty(current_card.Id))
                    {
                        current_card.IsDefault = false;
                        current_card.IsRecurring = false;
                        db.Entry(current_card).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                    //active all services Inactive of new default card
                    //db.Store_Services.Where(s => s.MxMerchant_SubscriptionStatus == "Inactive" && s.MxMerchant_cardAccountId == newdefault_card.MxMerchant_Id).ToList().ForEach(s =>
                    //{
                    //    PaymentService.SetStatusRecurring(s.Id, "active");
                    //});
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == current_card.CustomerCode);
                    _merchantService.SaveHistoryUpdate(current_card.CustomerCode, "Updated default card");
                    new MerchantService().WriteLogMerchant(cus, "Updated default card", "Default credit card has been updated. Card number: <b>" + newdefault_card.CardNumber + "</b>");
                    _logService.Info($"[PaymentGate][Pay-ChangeDefaultCard] completed Change Default Card");
                    return Json(new object[] { true });
                }
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[PaymentGate][Pay-ChangeDefaultCard] error Change Default Card: cardId = {cardId}");
                return Json(new object[] { false, e.Message });
            }
        }

        /// <summary>
        /// Do payment
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<ActionResult> Collect([FromBody] TransRequest info)
        {
            _logService.Info($"[PaymentGate][Pay-Collect] start Action pay");
            var pc = Session[PC_PAY]?.ToString();
            bool msgSuccess = false;
            try
            {
                C_CustomerCard cardData = new C_CustomerCard();
                var cus = CustomerAuth();
                var partner = PartnerAuth();
                bool paylater = false;
                var order = new O_Orders();

                if (cus == null)
                {
                    BasicAuthClear();
                    goto REDIRECT;
                }

                using (var db = new WebDataModel())
                {
                    order = db.O_Orders.Where(o => o.OrdersCode == info.OrdersCode).FirstOrDefault();
                    var old_trans = db.C_CustomerTransaction.Where(c => c.OrdersCode == info.OrdersCode).FirstOrDefault();
                    if (old_trans?.PaymentStatus == "Approved" || old_trans?.PaymentStatus == "Success")
                    {
                        TempData["s"] = "Invoice has been closed.";
                        return RedirectToAction("Index", new { key = info?.Key });
                    }
                    if (!string.IsNullOrEmpty(partner.Id))
                    {
                        var p_card = await db.C_PartnerCard.FindAsync(info.CardId) ?? new C_PartnerCard { };
                        cardData = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(p_card));
                        info.MxMerchant_Id = partner.MxMerchant_Id;
                    }
                    else
                    {
                        cardData = await db.C_CustomerCard.FindAsync(info.CardId);
                    }
                    try
                    {
                        if (cardData?.Id == null)
                        {
                            Session[PAYMENT_CARD_INFO] = info;
                            info.MxMerchant_Id = string.IsNullOrEmpty(partner.Id) ? long.Parse(cus.MxMerchant_Id ?? "0") : partner.MxMerchant_Id;
                            cardData = _payment.NewCard(info, partner.Code);
                            new MerchantService().WriteLogMerchant(cus, "Added new payment card", "New credit card has been added. Card number: <b>" + cardData.CardNumber + "</b>");
                        }
                    }
                    catch (Exception e)
                    {
                        _logService.Error(e, $"[PaymentGate][Pay-Collect] error Action pay");
                        MakeErrorResponse(new Exception($"Can not add your card : {e.Message}"), PAYMENT_ERROR);
                        goto REDIRECT;

                        //var msg = e.Message;
                        //if (e is AppHandleException exception) msg = exception.Message;
                        //throw new AppHandleException($"Can not add your card : {msg}");
                    }
                    Session[PAYMENT_CARD_INFO] = null;
                    Session[PAYMENT_INFO_MSG] = "Payment successful!";
                    if (db.C_CustomerTransaction.Any(trans => trans.OrdersCode == info.OrdersCode && trans.PaymentStatus == "Success"))
                        goto REDIRECT;

                    Session[PAYMENT_PURCHASES_HISTORY] = null;
                    info.PaymentMethod = "CreditCard";
                    info.PaymentNote = "IMS payment gateway";
                    paylater = (db.O_Orders.Where(o => o.OrdersCode == info.OrdersCode).FirstOrDefault()?.Status == InvoiceStatus.PaymentLater.ToString());
                }


                var transData = _payment.NewTrans(info, cardData);
                await _payment.SendEmail_Payment_ToBillingEmail(cardData, order, transData);
                if (transData.PaymentStatus.Trim() == "200") //payment success
                {
                    msgSuccess = true;
                    //await _payment.SendEmail_ReceiptPayment(info.OrdersCode);
                    // Update orders
                    await _payment.UpdateOrder(info, transData, cardData);
                    await TicketViewService.AutoTicketScenario.NewTicketDeployment(info.OrdersCode);
                    try
                    {
						using (var db = new WebDataModel())
						{
							var checkFirstInvoicePaid = db.C_CustomerTransaction.Any(x => x.CustomerCode == cus.CustomerCode && x.OrdersCode != order.OrdersCode);
							if (checkFirstInvoicePaid == false)
							{
								//var lead = db.C_SalesLead.Where(x => x.CustomerCode == cus.CustomerCode).FirstOrDefault();
								//if (lead != null)
								//{
								//	var _goHighLevelConnectorService = EngineContext.Current.Resolve<IGoHighLevelConnectorService>();
								//	await _goHighLevelConnectorService.ChangeContactTypeToCustomerAsync(lead.Id);
								//}
							}
						}
					}
					catch(Exception ex)
                    {
                        ImsLogService.WriteLog(ex.ToString(), "Exception", "Collect");
                    }

                    //try
                    //{
                    //    var _clickUpConnectorService = EngineContext.Current.Resolve<IClickUpConnectorService>();
                    //    await _clickUpConnectorService.SyncMerchantToClickUpAsync(cus.Id.ToString());
                    //}
                    //catch (Exception ex)
                    //{
                    //    ImsLogService.WriteLog(ex.ToString(), "Exception", "Collect");
                    //}
                }
                else
                {
                    _logService.Error(new Exception(transData.ResponseText), $"[PaymentGate][Pay-Collect] error Action pay");
                    MakeErrorResponse(new Exception(transData.MxMerchant_authMessage), PAYMENT_ERROR);
                    goto REDIRECT;
                    //throw new AppHandleException(transData.ResponseText);
                }
                List<Tuple<string, string, Boolean, string>> tmp = new List<Tuple<string, string, Boolean, string>>();
      
                using (var db = new WebDataModel())
                {
                    if (UserContent.GetWebInfomation(true).AutoActiveRecurringLicense == true)
                    {
                        var old_recurring = db.Store_Services.FirstOrDefault(s => s.StoreCode == info.StoreCode && s.Active == 1 && s.RenewDate > DateTime.UtcNow && s.Type == "license" && s.AutoRenew == true);
                        var ls = db.Store_Services.Where(s => s.OrderCode == info.OrdersCode && s.Type == "license" && s.Active == -1).FirstOrDefault();
                        if (paylater)
                        {
                            var license = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Type == "license" && s.RenewDate >= DateTime.UtcNow).FirstOrDefault();
                            if (license != null)
                            {
                                using (MerchantService service = new MerchantService(true))
                                {
                                    var rs = await service.ApproveAction(license.StoreCode, license.Id, true, "same-active");
                                }
                            }
                            if (license.AutoRenew == true)
                            {
                                PaymentService.SetStatusRecurring(license.Id, "active");
                            }
                        }
                        else if (ls != null)
                        {
                            var activelicense = ls.Id;
                            using (MerchantService service = new MerchantService(true))
                            {
                                activelicense = await service.ApproveAction(ls.StoreCode, ls.Id, true, "active");
                            }
                            if (ls.AutoRenew == true && order?.PaymentMethod != "Recurring")
                            {
                                if (old_recurring != null) { PaymentService.SetStatusRecurring(old_recurring.Id, "inactive"); }
                                PaymentService.SetStatusRecurring(activelicense, "active");
                                PaymentService.UpdateRecurringStatus(order.CustomerCode);
                                //_payment.NewRecurring(activelicense, cardData);
                            }
                        }
                        var adds = db.Store_Services.Where(s => s.OrderCode == info.OrdersCode && s.Type == "addon" && s.Active == -1).ToList();
                        if (adds.Count > 0)
                        {
                            foreach (var add in adds)
                            {
                                //xử lý khi active addon SMS khi dã kích hoạt SMS unlimited
                                if (add.Type == LicenseType.ADD_ON.Text())
                                {
                                    tmp.Add(Tuple.Create(add.StoreCode, add.Id, true, "active"));
                                }

                                if (add.AutoRenew == true) PaymentService.SetStatusRecurring(add.Id, "active","",true);
                                
                            }
                        }

                        var others = db.Store_Services.Where(s => s.OrderCode == info.OrdersCode && s.Type == "other" && s.Active == -1).ToList();
                        if (others.Count > 0)
                        {
                            foreach (var other in others)
                            {
                                tmp.Add(Tuple.Create(other.StoreCode, other.Id, true, "active"));
                                if (other.AutoRenew == true) PaymentService.SetStatusRecurring(other.Id, "active", "", true);
                            }
                        }
                    }
                }
                if (tmp.Count > 0)
                {
                  
                    foreach (var add in tmp)
                    {
                        using (MerchantService service = new MerchantService(true))
                        {
                            await service.ApproveAction(add.Item1, add.Item2, add.Item3, "active");
                        }
                    };
                }
               PaymentService.UpdateRecurringStatus(order.CustomerCode);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[PaymentGate][Pay-Collect] error Action pay");
                if (!msgSuccess)
                    MakeErrorResponse(ex, PAYMENT_ERROR);
            }
        REDIRECT:
            _logService.Info($"[PaymentGate][Pay-Collect] compelted Action pay");
            return RedirectToAction("Index", new { key = info?.Key, pc = pc });
        }

    }
}