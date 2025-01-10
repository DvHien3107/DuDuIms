using Enrich.Core.Infrastructure;
using Enrich.DataTransfer;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Enrichcous.Payment.Mxmerchant.Api;
using Enrichcous.Payment.Mxmerchant.Config.Enums;
using Enrichcous.Payment.Mxmerchant.Models;
using EnrichcousBackOffice.AppLB.OptionConfig;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.Respon;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.Utils.OptionConfig;
using EnrichcousBackOffice.ViewControler;
using Inner.Libs.Helpful;
using iTextSharp.tool.xml.css;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EnrichcousBackOffice.Models.Proc;

namespace EnrichcousBackOffice.Areas.AutoServices.Services
{
    public class RecurringService
    {
        private readonly IMailingService _mailingService = EngineContext.Current.Resolve<IMailingService>();
        private readonly ILogService _logService = EngineContext.Current.Resolve<ILogService>();
        readonly string Trial = Store_Apply_Status.Trial.Text();
        readonly string Promotional = Store_Apply_Status.Promotional.Text();
        readonly string Real = Store_Apply_Status.Real.Text();
        P_Member cmem = new P_Member { FullName = "Recurring system" };
        private readonly MxMerchantFunc PaymentFunc = new MxMerchantFunc();
        List<scanlog> scanlogs = new List<scanlog>();

        public async Task<List<scanlog>> autoRecurringScan()
        {
            var _context = EngineContext.Current.Resolve<EnrichContext>();
            _context.ApplicationName = "Hangfire";
            try
            {
                _logService.Info($"[System][AutoScan] start auto scan");
                scanlogs = new List<scanlog>();

                // await CreatePromoOrRealAsync();
                await ActiveAddonScheduler();
                await Deactive_duePaylater();

                //remove deactive addon expires
                await Deactive_Addon_Expires();

                await SendNotyLicenseExpires();

                await SendNotyNextAppointmentToAgent();
                _logService.Info($"[System][AutoScan] completed auto scan");
                return scanlogs;
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[System][AutoScan] error when scan");
                return new List<scanlog>();
            }
        }
        public async Task CreatePromoOrRealAsync()
        {
            WebDataModel db = new WebDataModel();
            var beforeDays = db.SystemConfigurations.FirstOrDefault().RecurringBeforeDueDate ?? 0;
            if (beforeDays < 0) return;

            _logService.Info($"[System][CreatePromoOrRealAsync] start auto recurring subscriptions");
            var _optionConfigurationService = new OptionConfigService();
            var config = _optionConfigurationService.LoadSetting<Config>();
            var rn_date = DateTime.UtcNow.Date.AddDays(beforeDays);
            var rn_date_txt = rn_date.ToString("yyyy-MM-dd");
            var end_differentreal = db.Store_Services.Where(s => s.StoreApply != Real && !string.IsNullOrEmpty(s.StoreApply) &&
                                                            s.RenewDate.Value.ToString() == rn_date_txt &&
                                                            s.hasRenewInvoiceIncomplete != true &&
                                                            s.Active == 1 &&
                                                            s.AutoRenew != true).ToList();
            foreach (var item in end_differentreal)
            {
                //skip check promotion license
                //var lp_promo_months = db.License_Product.Where(p => p.Code == item.t.ProductCode).FirstOrDefault().Promotion_Apply_Months;
                //if (lp_promo_months > 0)
                //{
                //    await CreatePromoInvoice(item.t);
                //}
                //else
                //{
                //    await CreateRealInvoice(item.t);
                //}

                //always create regular license
                await CreateRealInvoice(item);
            }
            //var end_Promos =
            //(from t in db.Store_Services.Where(s => s.StoreApply == Promotional && s.RenewDate == rn_date && s.hasRenewInvoiceIncomplete != true && s.Active == 1)
            // join s in db.Order_Subcription on new { p = t.ProductCode, o = t.OrderCode } equals new { p = s.Product_Code, o = s.OrderCode }
            // select new { t, s });

            //foreach (var item in end_Promos)
            //{

            //    if (item.t.EffectiveDate.Value.AddMonths(item.s.Promotion_Apply_Months ?? 0) < rn_date)
            //    {
            //        await CreateRealInvoice(item.t);

            //    }
            //    //else if (item.t.AutoRenew == true)
            //    //{
            //    //    await CheckRecurring_n_AutoRenew(item.t, Promotional);
            //    //}

            //}
            //int RecurringDay = config.License_RecurringCheck ?? 25;
            //if (DateTime.UtcNow.Day >= RecurringDay)
            //{
            //    var end_Real = db.Store_Services.Where(s => s.StoreApply == Real && s.RenewDate.HasValue && s.hasRenewInvoiceIncomplete != true && s.Active == 1 && s.AutoRenew == true).ToList();
            //    end_Real = end_Real.Where(s => s.RenewDate.Value.Month == DateTime.UtcNow.Month && s.RenewDate > DateTime.UtcNow).ToList();
            //    foreach (var item in end_Real)
            //    {
            //        await CheckRecurring_n_AutoRenew(item, Real);
            //    }
            //}

            var end_Real = db.Store_Services.Where(s => (s.StoreApply == Real || string.IsNullOrEmpty(s.StoreApply)) &&
                                                        s.RenewDate.HasValue &&
                                                        DbFunctions.TruncateTime(s.RenewDate.Value) <= DbFunctions.TruncateTime(rn_date) &&
                                                        s.hasRenewInvoiceIncomplete != true &&
                                                        s.Active == 1 &&
                                                        s.AutoRenew == true).ToList();
            foreach (var item in end_Real)
            {
                await CheckRecurring_n_AutoRenew(item, Real, !(item.RenewDate.Value.Date == rn_date.Date));
            }

            _logService.Info($"[System][CreatePromoOrRealAsync] complete auto recurring subscriptions");
        }
        public async Task<O_Orders> CreatePromoInvoice(Store_Services curStore)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var rs = await NewOrderApply(curStore.CustomerCode, curStore.ProductCode, Promotional, cmem, (curStore.RenewDate ?? DateTime.UtcNow.Date).AddDays(1));
                curStore.hasRenewInvoiceIncomplete = true;
                curStore.LastRenewOrderCode = rs.OrdersCode;
                db.Entry(curStore).State = EntityState.Modified;
                db.SaveChanges();
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Create Promo Invoice <b>#" + rs.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                });
                return rs;
            }
            catch (Exception e)
            {
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Create Promo Invoice of merchant <b>#" + curStore.CustomerCode + "</b>",
                    ex = JsonConvert.SerializeObject(e),
                });
                return new O_Orders();
            }

        }
        public async Task<O_Orders> CreateRealInvoice(Store_Services curStore)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                _logService.Info($"[System][CreateRealInvoice][{curStore.StoreCode}] start create new invoice: StoreCode = {curStore.StoreCode} | StoreServiceId = {curStore.Id}", curStore);
                var rs = await NewOrderApply(curStore.CustomerCode, curStore.ProductCode, Real, cmem, (curStore.RenewDate ?? DateTime.UtcNow.Date).AddDays(1));
                curStore.hasRenewInvoiceIncomplete = true;
                curStore.LastRenewOrderCode = rs.OrdersCode;
                db.Entry(curStore).State = EntityState.Modified;
                db.SaveChanges();
                _logService.Info($"[System][CreateRealInvoice][{curStore.StoreCode}]  complete create new invoice: StoreCode = {curStore.StoreCode}", rs);
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Create Real Invoice <b>#" + rs.OrdersCode + "</b> For merchant <b>#" + curStore.CustomerCode + "</b>",
                });
                return rs;
            }
            catch (Exception e)
            {
                _logService.Error(e, $"[System][CreateRealInvoice][{curStore.StoreCode}]  error create new invoice: StoreCode = {curStore.StoreCode} | StoreServiceId = {curStore.Id}");
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Create Real Invoice for merchant <b>#" + curStore.CustomerCode + "</b>",
                    ex = JsonConvert.SerializeObject(e),
                });
                return new O_Orders();
            }
        }
        public async Task<O_Orders> CheckRecurring_n_AutoRenew(Store_Services curStore, string apply, bool now)
        {
            var _context = EngineContext.Current.Resolve<EnrichContext>();
            _context.Id = (Guid.NewGuid()).ToString();
            try
            {
                //tạo renew Invoice;
                _logService.Info($"[System][Recurring][{curStore.StoreCode}] start recurring - Store Code {curStore.StoreCode}", curStore);
                WebDataModel db = new WebDataModel();
                var configSystem = db.SystemConfigurations.First();
                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == curStore.CustomerCode);
                if (cus.Type == MerchantType.STORE_IN_HOUSE.Code<string>()) { return new O_Orders(); }
                var beforeDays = configSystem.RecurringBeforeDueDate ?? 0;
                var rn_date = DateTime.UtcNow.Date.AddDays(beforeDays);
                var checkActivated = db.Store_Services.Any(x => x.Active == 1 && x.StoreCode == curStore.StoreCode && x.ProductCode == curStore.ProductCode &&
                                                                DbFunctions.TruncateTime(x.RenewDate.Value) > DbFunctions.TruncateTime(rn_date));
                var datetime_now = DateTime.UtcNow.Date;
                var checkDeactivedAccount = db.Store_Services.Where(c => c.CustomerCode == curStore.CustomerCode && c.Active == 1 && c.Type == "license" && c.RenewDate.Value >= datetime_now).Count() == 0; // true
                //var subscription = db.Order_Subcription.FirstOrDefault(c => c.CustomerCode == curStore.CustomerCode && c.OrderCode == curStore.OrderCode && c.Product_Code == curStore.ProductCode && c.);
                //var itemProduct = db.License_Product_Item.FirstOrDefault(c => c.License_Product_Id == subscription.ProductId && c.Enable == true && c.License_Item_Code == "SMS") ?? new License_Product_Item { Value = 0 };
                //var checkCountSMS = itemProduct?.Value != 0;
                //khong recurring khi deactive va addon
                if (!checkActivated && !(checkDeactivedAccount && curStore.Type == "addon"))
                {
                    PaymentService _payment = new PaymentService();
                    O_Orders o_Orders = await OrderViewService.RenewLicense(curStore, cmem, now ? null : curStore.RenewDate, apply);
                    scanlogs.Add(new scanlog
                    {
                        time = DateTime.UtcNow,
                        log = "Create Renew Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                    });
                    _logService.Info($"[System][Recurring][{curStore.StoreCode}] Create Renew Invoice ${o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                    //amount = $0
                    if (o_Orders.GrandTotal == 0)
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Recurring payment has been paid with $0. Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Recurring payment has been paid with $0, Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        return o_Orders;
                    }
                    //Nếu đã thanh toán recurring => tạo renew Invoice + active;
                    //var auth = UserContent.GetOauthInfo();
                    //var recur_payment = PaymentFunc.CheckRecurring(curStore.MxMerchant_contractId.ToString(), ref auth) ?? new MxMerchant_recurring_payment() { cardAccount = new Enrichcous.Payment.Mxmerchant.Models.cardAccount() };

                    var p_card = db.C_PartnerCard.FirstOrDefault(c => c.MxMerchant_Id == curStore.MxMerchant_cardAccountId);
                    var card = new C_CustomerCard { };
                    string MxMerchantID = "0";
                    if (p_card != null)
                    {
                        MxMerchantID = (db.C_Partner.FirstOrDefault(c => c.Code == p_card.PartnerCode)?.MxMerchant_Id ?? 0).ToString();
                        card = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(p_card));
                    }
                    else
                    {
                        card = db.C_CustomerCard.FirstOrDefault(c => c.MxMerchant_Id == curStore.MxMerchant_cardAccountId) ?? new C_CustomerCard { };
                        MxMerchantID = cus.MxMerchant_Id ?? "0";
                    }
                    var transacion = new TransRequest()
                    {
                        MxMerchant_Id = long.Parse(MxMerchantID),
                        CustomerCode = cus.CustomerCode,
                        StoreCode = cus.StoreCode,
                        Key = o_Orders.OrdersCode.ToBase64(),
                        PaymentMethod = "Recurring",
                        PaymentNote = "Auto Recurring payment"
                    };
                    var license = db.Store_Services.Where(s => s.OrderCode == o_Orders.OrdersCode && s.ProductCode == curStore.ProductCode).FirstOrDefault();
                    var recur_payment = _payment.NewTrans(transacion, card, o_Orders.PaymentMethod);
                    //payment success
                    if (recur_payment.PaymentStatus == "Approved")
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Recurring payment has been paid. Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Recurring payment has been paid, Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        await _payment.SendEmail_ReceiptPayment(o_Orders.OrdersCode, o_Orders?.PaymentMethod, true);
                        //closed invoice
                        //await OrderViewController.ChangeInvoiceSatus(o_Orders.OrdersCode, InvoiceStatus.Closed.ToString(), cmem);
                        //await OrderViewController.UpdatePayment(o_Orders.OrdersCode, "Recurring", "Auto Recurring payment", "", card.Id, recur_payment.CreateAt.Value.ToString("MM/dd/yyyy"), InvoiceStatus.Paid_Wait.ToString(), true, cmem);
                        await _payment.UpdateOrder(transacion, recur_payment, card);
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Set Paid/Wait Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Set Paid/Wait Invoice, Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        if (db.SystemConfigurations.FirstOrDefault().AutoActiveRecurringLicense == true)//nếu option auto active được bật
                        {
                            using (MerchantService service = new MerchantService(true))
                            {
                                var rs = await service.ApproveAction(curStore.StoreCode, license.Id, true, "active");
                            }
                            scanlogs.Add(new scanlog
                            {
                                time = DateTime.UtcNow,
                                log = "Active renew license Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                            });
                            _logService.Info($"[System][Recurring][{curStore.StoreCode}] Active renew license Invoice, Invoice: {o_Orders.OrdersCode} - {curStore.StoreCode}");
                            //active renew subscription
                        }
                        // invoice > 0 and mango store send sms
                        if (o_Orders.GrandTotal > 0 && string.IsNullOrEmpty(cus.PartnerCode))
                        {
                            _logService.Info($"[Recurring][SMS] start send sms payment success - Store Code: {curStore.StoreCode}");
                            string smsMessage = $"SPOS POS: We have applied your {"$" + string.Format("{0:#,0.00}", o_Orders.GrandTotal)} subscription payment. Thank you";

                            var dataSMS = new List<NotificationSMSModel>();
                            dataSMS.Add(new NotificationSMSModel
                            {
                                PhoneNumber = cus.OwnerMobile ?? cus.SalonPhone,
                                Message = smsMessage
                            });
                            var _smsService = EngineContext.Current.Resolve<ISMSService>();
                            var resNoty = await _smsService.Create(dataSMS);
                            if (resNoty.TotalSuccess > 0)
                            {
                                _logService.Info($"[Recurring][SMS]  send sms payment success - Store Code: {cus.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS payment success store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>success</b>."
                                });
                            }
                            else
                            {
                                _logService.Error($"[Recurring][SMS] send sms payment success failed - Store Code: {curStore.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS payment success store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>failed</b>."
                                });
                            }
                        }

                    }
                    else //payment fail -> vẫn active subscription và deactive sau 3 ngày mà chưa thanh toán
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Merchant cannot paid recurring payments Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Merchant cannot paid recurring payments Invoice ,Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        var dueDate = db.SystemConfigurations.FirstOrDefault().ExtensionDay ?? 0;
                        o_Orders.DueDate = DateTime.UtcNow.AddDays(dueDate);
                        db.Entry(o_Orders).State = EntityState.Modified;
                        db.SaveChanges();
                        var rs = await OrderViewService.ChangeInvoiceSatus(o_Orders.OrdersCode, InvoiceStatus.PaymentLater.ToString(), cmem);
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Active renew license Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Active renew license Invoice, Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        // invoice > 0 and mango store send sms
                        if (o_Orders.GrandTotal > 0 && string.IsNullOrEmpty(cus.PartnerCode))
                        {
                            _logService.Info($"[Recurring][SMS] start send sms cannot payment - Store Code: {curStore.StoreCode}");
                            string smsMessage = $"SPOS POS: Payment failed. Your subscription will expire in 3 days. Please check your email for the payment details. Thank you";

                            var dataSMS = new List<NotificationSMSModel>();
                            dataSMS.Add(new NotificationSMSModel
                            {
                                PhoneNumber = cus.OwnerMobile ?? cus.SalonPhone,
                                Message = smsMessage
                            });
                            var _smsService = EngineContext.Current.Resolve<ISMSService>();
                            var resNoty = await _smsService.Create(dataSMS);
                            if (resNoty.TotalSuccess > 0)
                            {
                                _logService.Info($"[Recurring][SMS]  send sms cannot payment success - Store Code: {cus.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS payment success store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>success</b>."
                                });
                            }
                            else
                            {
                                _logService.Error($"[Recurring][SMS] send sms cannot payment success failed - Store Code: {curStore.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS cannot payment store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>failed</b>."
                                });
                            }
                        }
                    }

                    if (configSystem.EnableSendRecurringEmailToBilling == true)
                    {
                        var transaction = db.C_CustomerTransaction.Where(c => c.OrdersCode == o_Orders.OrdersCode && c.PaymentMethod == "Recurring").OrderByDescending(c => c.CreateAt).FirstOrDefault() ?? new C_CustomerTransaction();
                        await _payment.SendEmail_Payment_ToBillingEmail(card, o_Orders, transaction);
                    }

                    //if (auth.Updated)
                    //{
                    //    Save_MxMerchantTokens(auth);
                    //}
                    _logService.Info($"[System][Recurring][{curStore.StoreCode}] Completed");
                    return o_Orders;
                }
                else
                {
                    scanlogs.Add(new scanlog
                    {
                        time = DateTime.UtcNow,
                        log = "Merchant <b>cannot</b> recurring <b>" + curStore.Productname + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        ex = JsonConvert.SerializeObject(new { Message = "Recurring conditions are not satisfied." }),
                    });
                    _logService.Error($"[System][Recurring][{curStore.StoreCode}] Recurring conditions are not satisfied - Store Code: ${curStore.StoreCode}");
                    return new O_Orders();
                }

            }
            catch (Exception e)
            {
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "auto recurring error <b>#" + curStore.CustomerCode + "</b>",
                    ex = JsonConvert.SerializeObject(e),
                });
                _logService.Info($"[System][Recurring] Auto recurring error - Store Code: {curStore.StoreCode}");
                return new O_Orders();
            }
        }




        //khong recurring khi deactive va addon
        //Nếu đã thanh toán recurring => tạo renew Invoice + active;
        //payment fail -> vẫn active subscription và deactive sau 3 ngày mà chưa thanh toán
        public async Task<rsData> RunRecurringPlan()
        {
            rsData _rsData = new rsData();
            var _context = EngineContext.Current.Resolve<EnrichContext>();
            _context.Id = (Guid.NewGuid()).ToString();
            try
            {
                //tạo renew Invoice;
                _logService.Info($"[System][Recurring] start recurring");
                WebDataModel db = new WebDataModel();
                var configSystem = db.SystemConfigurations.First();
                var beforeDays = configSystem.RecurringBeforeDueDate ?? 0;
                var rn_date = DateTime.UtcNow.Date.AddDays(beforeDays);

                List<RecurringPlanning> result = db.Database.SqlQuery<RecurringPlanning>("exec P_GetToDayRecurringPlan").ToList();

                var datetime_now = DateTime.UtcNow.Date;
                //var subscription = db.Order_Subcription.FirstOrDefault(c => c.CustomerCode == curStore.CustomerCode && c.OrderCode == curStore.OrderCode && c.Product_Code == curStore.ProductCode && c.);
                //var itemProduct = db.License_Product_Item.FirstOrDefault(c => c.License_Product_Id == subscription.ProductId && c.Enable == true && c.License_Item_Code == "SMS") ?? new License_Product_Item { Value = 0 };
                //var checkCountSMS = itemProduct?.Value != 0;
                //khong recurring khi deactive va addon
                foreach(var item  in result)
                {
                    Store_Services curStore =  db.Database.SqlQuery<Store_Services>($"select * from Store_Services where Active =1 and CustomerCode='{item.CustomerCode}' and ProductCode='{item.SubscriptionCode}'").FirstOrDefault();
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == curStore.CustomerCode);

                    PaymentService _payment = new PaymentService();
                    O_Orders o_Orders = await OrderViewService.RenewLicense(curStore, cmem, curStore.RenewDate);
                    scanlogs.Add(new scanlog
                    {
                        time = DateTime.UtcNow,
                        log = "Create Renew Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                    });
                    _logService.Info($"[System][Recurring][{curStore.StoreCode}] Create Renew Invoice ${o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                    var p_card = db.C_PartnerCard.FirstOrDefault(c => c.MxMerchant_Id == curStore.MxMerchant_cardAccountId);
                    var card = new C_CustomerCard { };
                    long MxMerchantID = 0;
                    if (p_card != null)
                    {
                        MxMerchantID = db.C_Partner.FirstOrDefault(c => c.Code == p_card.PartnerCode)?.MxMerchant_Id ?? 0;
                        card = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(p_card));
                    }
                    else
                    {
                        card = db.C_CustomerCard.FirstOrDefault(c => c.MxMerchant_Id == curStore.MxMerchant_cardAccountId) ?? new C_CustomerCard { };
                        MxMerchantID = long.Parse(cus.MxMerchant_Id ?? "0");
                    }
                    //Nếu đã thanh toán recurring => tạo renew Invoice + active;
                    var transacion = new TransRequest()
                    {
                        MxMerchant_Id = MxMerchantID,
                        CustomerCode = cus.CustomerCode,
                        StoreCode = cus.StoreCode,
                        Key = o_Orders.OrdersCode.ToBase64(),
                        PaymentMethod = "Recurring",
                        PaymentNote = "Auto Recurring payment"
                    };
                    var license = db.Store_Services.Where(s => s.OrderCode == o_Orders.OrdersCode && s.ProductCode == curStore.ProductCode).FirstOrDefault();
                    var recur_payment = _payment.NewTrans(transacion, card, o_Orders.PaymentMethod);
                    //payment success
                    if (recur_payment.PaymentStatus == "Approved")
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Recurring payment has been paid. Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Recurring payment has been paid, Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        await _payment.SendEmail_ReceiptPayment(o_Orders.OrdersCode, o_Orders?.PaymentMethod, true);
                        //closed invoice
                        //await OrderViewController.ChangeInvoiceSatus(o_Orders.OrdersCode, InvoiceStatus.Closed.ToString(), cmem);
                        //await OrderViewController.UpdatePayment(o_Orders.OrdersCode, "Recurring", "Auto Recurring payment", "", card.Id, recur_payment.CreateAt.Value.ToString("MM/dd/yyyy"), InvoiceStatus.Paid_Wait.ToString(), true, cmem);
                        await _payment.UpdateOrder(transacion, recur_payment, card);
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Set Paid/Wait Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Set Paid/Wait Invoice, Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        if (db.SystemConfigurations.FirstOrDefault().AutoActiveRecurringLicense == true)//nếu option auto active được bật
                        {
                            using (MerchantService service = new MerchantService(true))
                            {
                                var rs = await service.ApproveAction(curStore.StoreCode, license.Id, true, "active");
                            }
                            scanlogs.Add(new scanlog
                            {
                                time = DateTime.UtcNow,
                                log = "Active renew license Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                            });
                            _logService.Info($"[System][Recurring][{curStore.StoreCode}] Active renew license Invoice, Invoice: {o_Orders.OrdersCode} - {curStore.StoreCode}");
                            //active renew subscription
                        }
                        // invoice > 0 and mango store send sms
                        if (o_Orders.GrandTotal > 0 && string.IsNullOrEmpty(cus.PartnerCode))
                        {
                            _logService.Info($"[Recurring][SMS] start send sms payment success - Store Code: {curStore.StoreCode}");
                            string smsMessage = $"SPOS POS: We have applied your {"$" + string.Format("{0:#,0.00}", o_Orders.GrandTotal)} subscription payment. Thank you";

                            var dataSMS = new List<NotificationSMSModel>();
                            dataSMS.Add(new NotificationSMSModel
                            {
                                PhoneNumber = cus.OwnerMobile ?? cus.SalonPhone,
                                Message = smsMessage
                            });
                            var _smsService = EngineContext.Current.Resolve<ISMSService>();
                            var resNoty = await _smsService.Create(dataSMS);
                            if (resNoty.TotalSuccess > 0)
                            {
                                _logService.Info($"[Recurring][SMS]  send sms payment success - Store Code: {cus.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS payment success store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>success</b>."
                                });
                            }
                            else
                            {
                                _logService.Error($"[Recurring][SMS] send sms payment success failed - Store Code: {curStore.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS payment success store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>failed</b>."
                                });
                            }
                        }

                    }
                    else //payment fail -> vẫn active subscription và deactive sau 3 ngày mà chưa thanh toán
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Merchant cannot paid recurring payments Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Merchant cannot paid recurring payments Invoice ,Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        var dueDate = db.SystemConfigurations.FirstOrDefault().ExtensionDay ?? 0;
                        o_Orders.DueDate = DateTime.UtcNow.AddDays(dueDate);
                        db.Entry(o_Orders).State = EntityState.Modified;
                        db.SaveChanges();
                        var rs = await OrderViewService.ChangeInvoiceSatus(o_Orders.OrdersCode, InvoiceStatus.PaymentLater.ToString(), cmem);
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Active renew license Invoice <b>#" + o_Orders.OrdersCode + "</b> of merchant <b>#" + curStore.CustomerCode + "</b>",
                        });
                        _logService.Info($"[System][Recurring][{curStore.StoreCode}] Active renew license Invoice, Invoice: {o_Orders.OrdersCode} - Store Code: {curStore.StoreCode}");
                        // invoice > 0 and mango store send sms
                        if (o_Orders.GrandTotal > 0 && string.IsNullOrEmpty(cus.PartnerCode))
                        {
                            _logService.Info($"[Recurring][SMS] start send sms cannot payment - Store Code: {curStore.StoreCode}");
                            string smsMessage = $"SPOS POS: Payment failed. Your subscription will expire in 3 days. Please check your email for the payment details. Thank you";

                            var dataSMS = new List<NotificationSMSModel>();
                            dataSMS.Add(new NotificationSMSModel
                            {
                                PhoneNumber = cus.OwnerMobile ?? cus.SalonPhone,
                                Message = smsMessage
                            });
                            var _smsService = EngineContext.Current.Resolve<ISMSService>();
                            var resNoty = await _smsService.Create(dataSMS);
                            if (resNoty.TotalSuccess > 0)
                            {
                                _logService.Info($"[Recurring][SMS]  send sms cannot payment success - Store Code: {cus.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS payment success store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>success</b>."
                                });
                            }
                            else
                            {
                                _logService.Error($"[Recurring][SMS] send sms cannot payment success failed - Store Code: {curStore.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS cannot payment store:" + curStore.StoreName + " (#" + curStore.CustomerCode + ") <b>failed</b>."
                                });
                            }
                        }
                    }

                    if (configSystem.EnableSendRecurringEmailToBilling == true)
                    {
                        var transaction = db.C_CustomerTransaction.Where(c => c.OrdersCode == o_Orders.OrdersCode && c.PaymentMethod == "Recurring").OrderByDescending(c => c.CreateAt).FirstOrDefault() ?? new C_CustomerTransaction();
                        await _payment.SendEmail_Payment_ToBillingEmail(card, o_Orders, transaction);
                    }

                    //if (auth.Updated)
                    //{
                    //    Save_MxMerchantTokens(auth);
                    //}
                    _logService.Info($"[System][Recurring][{curStore.StoreCode}] Completed");
                }


                _rsData.Status = 200;
                _rsData.Message = "Recurring Success";
                return _rsData;

            }
            catch (Exception e)
            {
                _rsData.Status = 500;
                _rsData.Message = e.Message;
                return _rsData;
            }
        }
        public async Task<O_Orders> NewOrderApply(string CustomerCode, string LicenseCode, string StoreApply, P_Member cMem, DateTime StartDate)
        {
            var db = new WebDataModel();
            string newWordDetermine = string.Empty;
            var license = db.License_Product.Where(p => p.Code == LicenseCode).FirstOrDefault();
            if (license == null/* || addon.isAddon != true*/)
            {
                throw new Exception("Addon not found");
            }

            var result = await OrderViewService.AddAddon(CustomerCode, license, cMem, StartDate, null, 1, true, StoreApply);
            var cus = db.C_Customer.First(_cus => _cus.CustomerCode == CustomerCode);
            if (!MerchantType.STORE_IN_HOUSE.Code<string>().Equals(cus.Type))
            {
                //var ReqURL = Request.Url.Authority;

                var des = "<iframe  width='600' height='900' src='" + ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + result.OrdersCode + "&flag=Estimates'></iframe>";
                await TicketViewService.AutoTicketScenario.NewTicketSalesLead(result.CustomerCode, result.OrdersCode, des);

                ////// create finance ticket
                //var order = db.O_Orders.FirstOrDefault(c => c.OrdersCode == result.OrdersCode);
                //if (order != null && order?.GrandTotal > 0)
                //{
                //    await TicketViewController.AutoTicketScenario.NewTicketFinance(order.InvoiceNumber.ToString(), des);
                //}
                //////
                db.SaveChanges();

                using (InvoiceService invoiceService = new InvoiceService())
                {
                    await invoiceService.SendMailConfirmPayment(cus, result);
                }
            }
            return result;
        }
        public async Task Deactive_duePaylater()
        {
            WebDataModel db = new WebDataModel();
            var order = db.O_Orders.Where(o => o.Status == InvoiceStatus.PaymentLater.ToString() && o.DueDate < DateTime.UtcNow && o.GrandTotal > decimal.Zero);
            var end_due_subs = db.Store_Services.Where(s => s.Active == 1 && s.Period == "MONTHLY"
                                                            && order.Any(o => o.OrdersCode == s.OrderCode || o.OrdersCode == s.LastRenewOrderCode)).ToList();
            //var a = db.Store_Services.Where(x => order.Any(o => o.OrdersCode == x.OrderCode)).ToList();
            foreach (var sub in end_due_subs)
                using (MerchantService service = new MerchantService(true))
                {
                    try
                    {
                        var rs = await service.ApproveAction(sub.StoreCode, sub.Id, false, "deActive");
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "<b>PaymentLater #" + sub.OrderCode + "</b> has due! Deactive Subscription <b>" + sub.Productname + "</b> of store <b>#" + sub.StoreName + "(#" + sub.StoreCode + ")</b>",
                        });
                        _logService.Info($"[System][PaymentLater] PaymentLater: #${sub.OrderCode} has due! Deactive Subscription {sub.Productname} - Store Code: {sub.StoreCode}");
                    }
                    catch (Exception ex)
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "<b>PaymentLater #" + sub.OrderCode + "</b> has due! Deactive Subscription <b>" + sub.Productname + "</b> of store <b>#" + sub.StoreName + "(#" + sub.StoreCode + ")</b>",
                            ex = JsonConvert.SerializeObject(ex),
                        });
                        _logService.Info($"[System][PaymentLater] Deactive fail PaymentLater: {sub.OrderCode} - {sub.StoreCode}");
                    }
                }
        }
        public async Task SendNotyNextAppointmentToAgent()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                List<AppoitmentView> appointments = new List<AppoitmentView> { };
                db.Calendar_Event.Where(o => o.Type == "Event" && !string.IsNullOrEmpty(o.MemberNumber))
                                .ForEach(c =>
                                {
                                    var app = new AppoitmentView
                                    {
                                        Name = c.Name,
                                        Description = c.Description,
                                        MemberNumber = c.MemberNumber,
                                        StartEvent = DateTime.Parse(c.StartEvent),
                                        TimeZone = c.GMT,
                                        SalesLeadId = c.SalesLeadId
                                    };
                                    if (DateTime.Parse(c.StartEvent).ToUniversalTime() > DateTime.UtcNow)
                                    {
                                        appointments.Add(app);
                                    }
                                });
                if (appointments.Count() > 0)
                {
                    appointments = appointments.GroupBy(c => c.MemberNumber).OrderByDescending(c => c.First().StartEvent).Select(ap => ap.First()).ToList();
                    var appointmentData = (from ap in appointments
                                           join mb in db.P_Member on ap.MemberNumber equals mb.MemberNumber
                                           join sl in db.C_SalesLead on ap.SalesLeadId equals sl.Id
                                           select new
                                           {
                                               appointment = ap,
                                               member = mb,
                                               salelead = sl
                                           }).ToList();
                    foreach (var ap in appointmentData)
                    {
                        try
                        {
                            string subject = $"Appointment is comming: {ap.appointment.Name}";
                            string content = $"Dear {ap.member.FullName},<br/><br/>" +
                                             $"Your appointment is comming:<br/>" +
                                             $"<span style='margin-left: 25px;'>Store name: <b>{ap.salelead.L_SalonName}</b></span><br/>" +
                                             $"<span style='margin-left: 25px;'>Store phone: {ap.salelead.L_Phone}</span><br/>" +
                                             $"<span style='margin-left: 25px;'>Address: {ap.salelead.L_Address + ", " + ap.salelead.L_City + ", " + ap.salelead.L_State + ", " + ap.salelead.L_Zipcode + ", " + ap.salelead.L_Country}</span><br/>" +
                                             $"<span style='margin-left: 25px;'>Owner name: {ap.salelead.L_ContactName}</span><br/>" +
                                             $"<span style='margin-left: 25px;'>Owner phone: {ap.salelead.L_ContactPhone}</span><br/>" +
                                             $"Note: {ap.appointment.Name}<br/>" +
                                             $"{ap.appointment.Description}<br/>" +
                                             $"At time: <b>{ap.appointment.StartEvent.ToString("MMMM dd, yyyy hh:mm tt")} ({ap.appointment.TimeZone})</b>" +
                                             $"<br/><br/>Thank you!";

                            var emailData = new { content = content, subject = subject };
                            var msg = await _mailingService.SendNotifyOutgoingWithTemplate(ap.member.PersonalEmail, ap.member.FullName, subject, "", emailData);
                            scanlogs.Add(new scanlog
                            {
                                time = DateTime.UtcNow,
                                log = "Send noty to Agent " + ap.member.FullName + " success."
                            });
                        }
                        catch (Exception ex)
                        {
                            scanlogs.Add(new scanlog
                            {
                                time = DateTime.UtcNow,
                                log = "Send noty to Agent " + ap.member.FullName + " fail.",
                                ex = JsonConvert.SerializeObject(ex),
                            });
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Send noty to Agent can't start.",
                    ex = JsonConvert.SerializeObject(ex),
                });
            }
        }
        public async Task Deactive_Addon_Expires()
        {
            try
            {
                var _optionConfigurationService = new OptionConfigService();
                var config = _optionConfigurationService.LoadSetting<Config>();
                if (config.License_Addon_Expires_Reset != true) return;
                _logService.Info($"[System][Deactive_Addon_Expires] start deactive subscription expires");
                WebDataModel db = new WebDataModel();
                var timeNow = DateTime.UtcNow.Date;
                var addon_expires = db.Store_Services.Where(c => c.Period == "MONTHLY" && c.Active == 1 && c.RenewDate.Value < timeNow &&
                                                                db.License_Product.Any(l => l.Code == c.ProductCode && l.FlagDeactivateExpires == 1)).ToList();
                foreach (var addon in addon_expires)
                {
                    try
                    {
                        using (MerchantService service = new MerchantService(true))
                        {
                            string msg = await service.ApproveAction(addon.StoreCode, addon.Id, false, "deActive");
                        }
                        addon.Active = LicenseStatus.DE_ACTIVE.Code<int>();
                        _logService.Info($"[System][Deactive_Addon_Expires] processing deactive subscription expires success - Store Code {addon.StoreCode} | Subscription {addon.ProductCode}");
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = $"Deactive add-on expires <b>success</b>: Id = {addon.Productname}, StoreCode: {addon.StoreCode}"
                        });

                    }
                    catch (Exception ex)
                    {
                        _logService.Error(ex, $"[System][Deactive_Addon_Expires] processing deactive subscription expires error - Store Code {addon.StoreCode} | Subscription {addon.ProductCode}");
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = $"Deactive add-on expires <b>error</b>: Id = {addon.Productname}, StoreCode: {addon.StoreCode}",
                            ex = JsonConvert.SerializeObject(ex),
                        });
                    }
                }
                db.SaveChanges();
                _logService.Info($"[System][Deactive_Addon_Expires] complete deactive subscription expires");
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[System][Deactive_Addon_Expires] error deactive subscription expires");
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Deactive add-on expires can't start.",
                    ex = JsonConvert.SerializeObject(ex),
                });
            }
        }
        public async Task SendNotyLicenseExpires()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var configSystem = db.SystemConfigurations.FirstOrDefault() ?? new SystemConfiguration { };
                var _optionConfigurationService = new OptionConfigService();
                var config = _optionConfigurationService.LoadSetting<Config>();
                var day_valid = (config.License_Expires_Leftdays ?? "3").Split(',').Select(c => DateTime.UtcNow.AddDays(int.Parse(c)).ToString("yyyy-MM-dd")).ToList();
                var day_valid_1 = day_valid.First();
                var day_valid_2 = day_valid.Last();
                var beforeRecurringDays = configSystem.RecurringBeforeDueDate ?? 0;
                var activeCode = LicenseStatus.ACTIVE.Code<int>();
                var weeklyText = RecurringInterval.Weekly.ToString();
                string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
                if (configSystem.EnableSendRecurringEmailToBilling == true && !string.IsNullOrEmpty(configSystem.BillingNotification))
                {
                    var store_service_leftdays = db.Store_Services.Where(s => s.Active == activeCode && s.RenewDate.HasValue && s.PeriodRecurring != weeklyText
                            && (s.RenewDate.ToString() == day_valid_1 || s.RenewDate.ToString() == day_valid_2)).ToList();
                    var main_data = (from ss in store_service_leftdays
                                     join od in db.O_Orders on ss.OrderCode equals od.OrdersCode
                                     join cu in db.C_Customer on od.CustomerCode equals cu.CustomerCode
                                     select new
                                     {
                                         storerservice = ss,
                                         customer = cu,
                                         order = od
                                     }).ToList();
                    // send recurring email to billing email
                    foreach (var data in main_data)
                    {
                        try
                        {
                            var merchantlink = ConfigurationManager.AppSettings["IMSUrl"] + "/merchantman/detail/" + data.customer.Id;
                            string subject = $"{data.storerservice.StoreName} {data.storerservice.Productname} service will expire in {(data.storerservice.RenewDate.Value - DateTime.UtcNow).Days + 1} days";
                            string PaymentSchedule = "";
                            if (data.storerservice.AutoRenew == true)
                            {
                                PaymentSchedule = $"<span style='margin-left: 25px;'>Payment Schedule: {data.storerservice.RenewDate.Value.Date.AddDays(-beforeRecurringDays).ToString("MMM dd, yyyy")}</span><br/>";
                            }
                            string content = $"Hello,<br/><br/>" +
                                             $"{data.storerservice.StoreName}'s {data.storerservice.Productname} service will expire in {(data.storerservice.RenewDate.Value - DateTime.UtcNow).Days + 1} days (expires on {data.storerservice.RenewDate.Value.ToString("MMM dd, yyyy")})<br/>" +
                                             $"Salon infomation: <br/>" +
                                             $"<span style='margin-left: 25px;'>Salon Name: <b>{data.storerservice.StoreName}</b> (#{data.storerservice.StoreCode})</span><br/>" +
                                             $"<span style='margin-left: 25px;'>Owner: {data.customer.OwnerName}</span><br/>" +
                                             $"<span style='margin-left: 25px;'>Address: {data.customer.AddressLine()}</span><br/>" +

                                             $"Service details: <br/>" +
                                             $"<span style='margin-left: 25px;'>Service Name: {data.storerservice.Productname}</span><br/>" +
                                             $"<span style='margin-left: 25px;'>Total Amount : {"$" + string.Format("{0:#,0.00}", data.order.GrandTotal)}</span><br/>" +
                                             $"{PaymentSchedule}" +
                                             $"<span style='margin-left: 25px;'>Start Date: {data.storerservice.EffectiveDate.Value.ToString("MMM dd, yyyy")}</span><br/>" +
                                             $"<span style='margin-left: 25px;'>Expiry Date: {data.storerservice.RenewDate.Value.ToString("MMM dd, yyyy")}</span><br/><br/>" +

                                             $"Please click on the link below for more details.<br/>" +
                                             $"<a href='{merchantlink}'>Click to open link</a><br/>" +
                                             $"<br/>Best regards,<br/>IMS support team";

                            var emailData = new { content = content, subject = subject };

                            var toEmail = configSystem.BillingNotification;
                            List<string> ListCCEmail = new List<string>();
                            var ccEmail = db.P_Member.FirstOrDefault(c => c.MemberNumber == data.order.SalesMemberNumber)?.PersonalEmail ?? db.SystemConfigurations.First().SalesEmail;
                            if (!string.IsNullOrEmpty(ccEmail))
                            {
                                ListCCEmail.Add(ccEmail);
                            }
                            if (!string.IsNullOrEmpty(data.customer.MemberNumber))
                            {
                                var AccountManager = db.P_Member.FirstOrDefault(x => x.MemberNumber == data.customer.MemberNumber);
                                if (AccountManager != null)
                                {
                                    ListCCEmail.Add(AccountManager.PersonalEmail);
                                }
                            }
                            await _mailingService.SendNotifyOutgoingWithTemplate(toEmail, "Sale Agent", subject, string.Join(";", ListCCEmail), emailData);
                            scanlogs.Add(new scanlog
                            {
                                time = DateTime.UtcNow,
                                log = "Send noty email License expires store:" + data.storerservice.StoreName + " (#" + data.storerservice.CustomerCode + ") to billing admin <b>success</b>."
                            });
                            _logService.Info($"[System][LicenseExpiresNotice] Send noty email License expires success - Store Code: {data.storerservice.StoreCode}");
                        }
                        catch (Exception ex)
                        {
                            scanlogs.Add(new scanlog
                            {
                                time = DateTime.UtcNow,
                                log = "Send noty email License expires store: <b>" + data.storerservice.StoreName + " (#" + data.storerservice.CustomerCode + ") to billing admin </b> <b>error</b>.",
                                ex = JsonConvert.SerializeObject(ex),
                            });
                            _logService.Error($"[System][LicenseExpiresNotice] Send noty email License expires failed - Store Code: {data.storerservice.StoreCode}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Send noti license expires fail.",
                    ex = JsonConvert.SerializeObject(ex),
                });
            }
        }
        public async Task SendNotyAutoRenew()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var beforeDate = db.SystemConfigurations.FirstOrDefault().NotificationBeforeExpiration ?? 0;
                if (beforeDate <= 0) return;
                var dateFilter = DateTime.UtcNow.AddDays(beforeDate).ToString("yyyy-MM-dd");
                var activeCode = LicenseStatus.ACTIVE.Code<int>();
                var store_service_leftdays_renew = db.Store_Services.Where(s => s.Active == activeCode && s.RenewDate.HasValue && s.RenewDate.ToString() == dateFilter && s.AutoRenew == true).ToList();
                var beforeRecurringDays = db.SystemConfigurations.FirstOrDefault().RecurringBeforeDueDate ?? 0;
                int daysNumberRenew = (beforeDate - beforeRecurringDays);
                string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
                var main_data = (from ss in store_service_leftdays_renew
                                 join os in db.Order_Subcription on new { s1 = ss.OrderCode, s2 = ss.ProductCode } equals new { s1 = os.OrderCode, s2 = os.Product_Code }
                                 where (os.SubscriptionType != "setupfee" && os.SubscriptionType != "interactionfee")
                                 //only get mango store and not store in house
                                 join cu in db.C_Customer.Where(c => string.IsNullOrEmpty(c.PartnerCode) && c.Type != STORE_IN_HOUSE) on ss.CustomerCode equals cu.CustomerCode
                                 select new
                                 {
                                     storerservice = ss,
                                     customer = cu,
                                     ordersubscription = os
                                 }).ToList();
                foreach (var data in main_data)
                {
                    try
                    {

                        //  var merchantlink = ConfigurationManager.AppSettings["IMSUrl"] + "/merchantman/detail/" + data.customer.Id;
                        var discount = data.ordersubscription.DiscountPercent > 0 ? (data.ordersubscription.DiscountPercent * (data.ordersubscription.RecurringPrice ?? data.ordersubscription.Price) / 100) : data.ordersubscription.Discount;
                        data.ordersubscription.Amount = (data.ordersubscription.RecurringPrice ?? data.ordersubscription.Price) - ((data.ordersubscription.ApplyDiscountAsRecurring == true ? 1 : 0) * (discount ?? 0));
                        string subject = "";
                        string content = "";
                        //if (data.storerservice.AutoRenew == true)
                        //{
                        subject = $"{data.storerservice.Productname} service will be auto renew in " + (daysNumberRenew == 1 ? daysNumberRenew + " day" : daysNumberRenew + " days");
                        content = $"Hello {data.customer.BusinessName},<br/><br/>" +
                                        $"<b>{data.storerservice.Productname}</b> service will be auto renew in " + (daysNumberRenew == 1 ? daysNumberRenew + " day" : daysNumberRenew + " days") + $" (renew on {DateTime.UtcNow.AddDays(beforeDate).ToString("MMM dd, yyyy")})<br/>" +
                                        //$"Salon infomation: <br/>" +
                                        //$"<span style='margin-left: 25px;'>Salon name: <b>{data.storerservice.StoreName}</b> (#{data.storerservice.StoreCode})</span><br/>" +
                                        //$"<span style='margin-left: 25px;'>Owner: {data.customer.OwnerName}</span><br/>" +
                                        //$"<span style='margin-left: 25px;'>Address: {data.customer.AddressLine()}</span><br/>" +

                                        $"Service details: <br/>" +
                                        $"<span style='margin-left: 25px;'>Service Name: <b>{data.storerservice.Productname}</b></span><br/>" +
                                        $"<span style='margin-left: 25px;'>Total Amount : {"$" + string.Format("{0:#,0.00}", data.ordersubscription.Amount)}</span><br/>" +
                                        $"<span style='margin-left: 25px;'>Payment Schedule: {data.storerservice.RenewDate.Value.Date.AddDays(-beforeRecurringDays).ToString("MMM dd, yyyy")}</span><br/>" +
                                        $"<span style='margin-left: 25px;'>Start Date: {data.storerservice.EffectiveDate.Value.ToString("MMM dd, yyyy")}</span><br/>" +
                                        $"<span style='margin-left: 25px;'>Expiry Date: {data.storerservice.RenewDate.Value.ToString("MMM dd, yyyy")}</span><br/><br/>";


                        // send sms noty expires with condision invoice price >0 
                        if (data.ordersubscription.Amount > 0)
                        {
                            _logService.Info($"[Recurring][SMS] start send sms renewal - Store Code: {data.storerservice.StoreCode}");
                            string smsMessage = $"SPOS POS: Autopay -  {"$" + string.Format("{0:#,0.00}", data.ordersubscription.Amount)}  will be charged for {data.storerservice.Productname} on {data.storerservice.RenewDate.Value.ToString("MM/dd/yyyy")}. Thank you";

                            var dataSMS = new List<NotificationSMSModel>();
                            dataSMS.Add(new NotificationSMSModel
                            {
                                PhoneNumber = data.customer.OwnerMobile ?? data.customer.SalonPhone,
                                Message = smsMessage
                            });
                            var _smsService = EngineContext.Current.Resolve<ISMSService>();
                            var resNoty = await _smsService.Create(dataSMS);
                            if (resNoty.TotalSuccess > 0)
                            {
                                _logService.Info($"[Recurring][SMS] Send sms renewal success - Store Code: {data.storerservice.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS to merchant: License expires store:" + data.storerservice.StoreName + " (#" + data.storerservice.CustomerCode + ") <b>success</b>."
                                });
                            }
                            else
                            {
                                _logService.Error($"[Recurring][SMS] Send sms renewal error - Store Code: {data.storerservice.StoreCode}");
                                scanlogs.Add(new scanlog
                                {
                                    time = DateTime.UtcNow,
                                    log = "Send SMS to merchant: License expires store:" + data.storerservice.StoreName + " (#" + data.storerservice.CustomerCode + ") <b>failed</b>."
                                });
                            }
                        }


                        //$"Thank you, Simply Pos";
                        //$"Please click on the link below for more details.<br/>" +
                        //$"<a href='{merchantlink}'>Click to open link</a><br/>" +
                        //$"<br/>Best regards,<br/>IMS support team";

                        var emailData = new { content = content, subject = subject };
                        var toEmail = data.customer.SalonEmail;
                        var ccEmail = string.Join(";", new string[] { data.customer.MangoEmail, data.customer.BusinessEmail, data.customer.Email });
                        await _mailingService.SendNotifyOutgoingWithTemplate(toEmail, data.customer.BusinessName, subject, ccEmail, emailData);
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Send email " + (data.storerservice.AutoRenew == true ? "Auto renew  to " : "expires notice ") + data.customer.BusinessName + " (#" + data.customer.CustomerCode + ") <b>success</b>",
                        });
                        _logService.Info($"[System][LicenseRenewalNotice] Send noty renewal success - Store Code: {data.storerservice.StoreCode}");
                    }
                    catch (Exception ex)
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Send email Auto renew after " + beforeRecurringDays + " left days to " + data.customer.BusinessName + " (#" + data.customer.CustomerCode + ") <b>error</b>",
                            ex = JsonConvert.SerializeObject(ex),
                        });
                        _logService.Error($"[System][LicenseRenewalNotice] Send noty renewal failed - Store Code: {data.storerservice.StoreCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Send email auto renew fail.",
                    ex = JsonConvert.SerializeObject(ex),
                });
            }
        }
        public async Task ActiveAddonScheduler()
        {
            try
            {
                _logService.Info($"[System][ActiveAddonScheduler] start active subscription scheduler");
                WebDataModel db = new WebDataModel();
                var timeNow = DateTime.UtcNow.Date;
                var AddonSchedulers = (from add in db.S_AddonSchedulerActivation.Where(c => c.Status == 0 && c.EffectiveDate.Value <= timeNow)
                                       join or in db.O_Orders
                                       on add.OrderCode equals or.OrdersCode
                                       where (or.Status == InvoiceStatus.Closed.ToString() ||
                                             or.Status == InvoiceStatus.Paid_Wait.ToString() ||
                                             or.Status == InvoiceStatus.PaymentLater.ToString())
                                       select add).ToList();
                foreach (var item in AddonSchedulers)
                {
                    try
                    {
                        using (MerchantService service = new MerchantService(true))
                        {
                            var rs = await service.ApproveAction(item.StoreCode, item.StoreServiceId, true, "active");
                        }
                        item.Status = 1;
                        db.Entry(item).State = EntityState.Modified;
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Active next quantity addon <b>" + item.ProductName + "</b> of merchant <b>#" + item.CustomerCode + "</b> success.",
                        });
                        _logService.Info($"[System][ActiveAddonScheduler] Active next quantity addon success - Store Code: {item.StoreCode}");
                    }
                    catch (Exception ex)
                    {
                        scanlogs.Add(new scanlog
                        {
                            time = DateTime.UtcNow,
                            log = "Active next quantity addon <b>" + item.ProductName + "</b> of merchant <b>#" + item.CustomerCode + "</b> error.",
                            ex = JsonConvert.SerializeObject(ex),
                        });
                        _logService.Error($"[System][ActiveAddonScheduler] Active next quantity addon failed - Store Code: {item.StoreCode}");
                    }
                }
                await db.SaveChangesAsync();
                _logService.Info($"[System][ActiveAddonScheduler] complete active subscription scheduler");
            }
            catch (Exception ex)
            {
                _logService.Info(ex, $"[System][ActiveAddonScheduler] error active subscription scheduler");
                scanlogs.Add(new scanlog
                {
                    time = DateTime.UtcNow,
                    log = "Addon scheduler activation error.",
                    ex = JsonConvert.SerializeObject(ex),
                });
            }
        }
        public void Save_MxMerchantTokens(OauthInfo auth)
        {
            using (var db = new WebDataModel())
            {
                var webinfo = db.SystemConfigurations.FirstOrDefault();
                webinfo.MxMerchant_AccessToken = auth.AccessToken;
                webinfo.MxMerchant_AccessSecret = auth.AccessSecret;
                db.Entry(webinfo).State = EntityState.Modified;
                db.SaveChanges();
            }
        }
    }

    public class AppoitmentView
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string SalesLeadId { get; set; }
        public string MemberNumber { get; set; }
        public DateTime StartEvent { get; set; }
        public string TimeZone { get; set; }

    }
}