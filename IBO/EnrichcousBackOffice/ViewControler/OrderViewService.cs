using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Enrich.Core.Infrastructure;
using Enrich.Core.Ultils;
using Enrich.DataTransfer;
using Enrich.IServices.Utils;
using Enrich.IServices.Utils.GoHighLevelConnector;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Enrichcous.Payment.Mxmerchant.Config.Enums;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Twilio.Rest.Api.V2010.Account;

namespace EnrichcousBackOffice.ViewControler
{
    public class OrderViewService
    {
        /// <summary>
        ///@Nancy: and for client ID's. I need to be able to specify if a customer is subscription only, terminal only, or both.  Im thikning
        //Client ID - a letter
        //Example, K- subscription, H- Processing(Terminal Only), Q- Both subscription &#38; processing
        //123456- K
        //123456- H
        //123456- Q
        //So when we run reports we know H &#38; Q count towards our minimim requirements
        /// </summary>
        /// <param name="ordCode"></param>
        /// <param name="cusCode"></param>
        /// <returns></returns>
        /// 
        private readonly ISMSService _sMSService;
		private readonly IMailingService _mailingService;

        public OrderViewService(ISMSService sMSService, IMailingService mailingService)
        {
            _sMSService = sMSService;
            _mailingService = mailingService;
		}

        public static async Task CheckMerchantWordDetermine(string cusCode, WebDataModel db = null)
        {
            if (db == null)
            {
                db = new WebDataModel();
            }
            try
            {
                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/customer/word_determine");
                //var service = (from o in db.O_Orders
                //               join os in db.Order_Subcription on o.OrdersCode equals os.OrderCode
                //               where o.CustomerCode == cusCode && o.InvoiceNumber > 0 && o.Cancel != true && os.Actived == true
                //               select os).Any();
                //var terminal = (from t in db.Order_Products
                //                join o in db.O_Orders on t.OrderCode equals o.OrdersCode
                //                join pd in db.O_Product on t.ProductCode equals pd.Code
                //                where pd.ProductLineCode == "terminal" && o.CustomerCode == cusCode && o.InvoiceNumber > 0 && o.Cancel != true
                //                select t).Any();
                string wd = string.Empty;

                var listMerchantSub = db.C_MerchantSubscribe.Where(x => x.CustomerCode == cusCode && (x.Status == "closed" || x.Status == "active")).Count() > 0;
                var checkExistStore = db.Store_Services.Where(x => x.CustomerCode == cusCode && x.Type == "license" && x.Active == 1).Count() > 0;
                if (listMerchantSub && checkExistStore)
                {
                    wd = node["both"].InnerText; ;
                }
                else
                {
                    wd = node["processing"].InnerText;
                }
                var merchant = db.C_Customer.Where(c => c.CustomerCode == cusCode).FirstOrDefault();
                if (wd != merchant.WordDetermine)
                {
                    merchant.WordDetermine = wd;
                    db.Entry(merchant).State = System.Data.Entity.EntityState.Modified;
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("CheckMerchantWordDetermine\n" + e.Message);

            }

        }

        public static O_Orders NewOrder(WebDataModel db, List<Device_Service_ModelCustomize> list_device_service,
            TotalMoneyOrder total_money_order, P_Member cMem, string sale_mn, string cus_code, string desc, string ship_address, DateTime? InvoiceDate = null, DateTime? DueDate = null, bool? Renewal = false)
        {
            try
            {
                #region CREATE ORDER
                var new_order = new O_Orders();
                int countOfOrder = db.O_Orders.Where(o => o.CreatedAt.Value.Year == DateTime.Today.Year
                                           && o.CreatedAt.Value.Month == DateTime.Today.Month).Count();

                new_order.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                new_order.OrdersCode = DateTime.Now.ToString("yyMM") + (countOfOrder + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("fff");
                new_order.CustomerCode = cus_code;
                new_order.CustomerName = db.C_Customer.Where(c => c.CustomerCode == cus_code).FirstOrDefault()?.BusinessName;

                if (string.IsNullOrEmpty(sale_mn) != true)
                {
                    new_order.SalesMemberNumber = sale_mn;
                    new_order.SalesName = db.P_Member.Where(m => m.MemberNumber == sale_mn).FirstOrDefault()?.FullName;
                    if (new_order.SalesMemberNumber != cMem.MemberNumber)
                    {
                        var notificationService = new NotificationService();
                        notificationService.OrderAddNotification(new_order.SalesMemberNumber, new_order.Id.ToString(), new_order.OrdersCode, cMem.FullName, cMem.MemberNumber);
                    }
                }
                new_order.Comment = desc;
                new_order.CreatedAt = DateTime.UtcNow;
                new_order.CreatedBy = cMem.FullName;
                new_order.CreateByMemNumber = cMem.MemberNumber;
                new_order.DiscountAmount = total_money_order.DiscountAmount;
                new_order.DiscountPercent = float.Parse(total_money_order.DiscountPercent.ToString());
                new_order.ShippingFee = total_money_order.ShippingFee;
                new_order.TaxRate = total_money_order.TaxRate;
                new_order.TotalHardware_Amount = total_money_order.SubTotal;
                //new_order.Service_Amount = total_money_order.SubTotal - total_money_order.DeviceTotalAmount;
                //new_order.Service_MonthlyFee = list_device_service.Where(sv => sv.Type == "Service")?.Sum(sv => sv.MonthlyFee) ?? 0;
                new_order.GrandTotal = total_money_order.GrandTotal;
                //ShippingAddress: "address|city|state|zipcode|country"
                new_order.ShippingAddress = ship_address;
                new_order.InvoiceDate = InvoiceDate;
                //new_order.DueDate = DueDate;
                new_order.Renewal = Renewal ?? false;
                #endregion

                return new_order;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// New Order Product
        /// </summary>
        /// <param name="db"></param>
        /// <param name="list_device"></param>
        /// <param name="new_order"></param>
        /// <param name="cMem"></param>
        /// <returns></returns>
        public static string NewOrderProduct(WebDataModel db,
            List<Device_Service_ModelCustomize> list_device, O_Orders new_order, P_Member cMem)
        {
            try
            {
                var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                foreach (var item in list_device)
                {
                    if (item.BundleId > 0)
                    {

                        var mbs = db.I_Bundle_Device.Where(m => m.Bundle_Id == item.BundleId).ToList();
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
                            new_device.TotalAmount = (mb.Price ?? 0) * mb.Quantity * item.Quantity;
                            new_device.CreateBy = cMem.FullName;
                            new_device.CreateAt = DateTime.UtcNow;
                            new_device.Feature = model.Color;
                            new_device.BundleId = item.BundleId;
                            new_device.ModelCode = model.ModelCode;
                            new_device.ModelName = model.ModelName;
                            new_device.BundleQTY = item.Quantity;
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
                        new_device.Price = item.Price;
                        new_device.Quantity = item.Quantity;
                        new_device.TotalAmount = item.Price * item.Quantity;
                        new_device.CreateBy = cMem.FullName;
                        new_device.CreateAt = DateTime.UtcNow;
                        new_device.Feature = item.Feature;
                        new_device.ModelCode = item.ModelCode;
                        new_device.ModelName = item.ModelName;
                        new_device.BundleId = null;
                        new_device.BundleQTY = null;
                        db.Order_Products.Add(new_device);
                    }

                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /*
        /// <summary>
        /// New Order Service
        /// </summary>
        /// <param name="db"></param>
        /// <param name="list_service"></param>
        /// <param name="new_order"></param>
        /// <param name="cMem"></param>
        /// <returns></returns>
        public static string NewOrderService(WebDataModel db,
            List<Device_Service_ModelCustomize> list_service, O_Orders new_order, P_Member cMem)
        {
            try
            {
                var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                foreach (var item in list_service)
                {
                    var new_service = new O_Orders_Service();
                    new_service.Id = id++;
                    new_service.OrdersCode = new_order.OrdersCode;
                    new_service.ServiceCode = item.ServiceCode;
                    new_service.ServiceName = item.ServiceName;
                    new_service.MonthlyFee = item.MonthlyFee;
                    new_service.Monthly = 1;
                    new_service.SetupFee = item.SetupFee;
                    new_service.TotalAmount = item.SetupFee + item.MonthlyFee;
                    new_service.ServicePlan = item.ServicePlan;

                    DateTime start_date;
                    if (DateTime.TryParseExact(item.StartDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out start_date) == true)
                    {
                        new_service.StartDate = start_date;
                    }
                    new_service.CreateAt = DateTime.Now;
                    new_service.CreateBy = cMem.FullName;
                    db.O_Orders_Service.Add(new_service);
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        */

        /// <summary>
        /// gui email + sms sau khi deployment complete/ san sang giao hang.
        /// </summary>
        /// <param name="customerCode"></param>
        /// <param name="trackingNumber"></param>
        /// <returns></returns>
        public async Task<string> Send_SMS_Email_After_Deployment_Complete(C_Customer cus, string cc, object email_data)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    //var cus = db.C_Customer.Where(c => c.CustomerCode == customerCode).FirstOrDefault();
                    //var companyInfo = db.SystemConfigurations.FirstOrDefault();
                    if (cus == null)
                    {
                        return string.Empty;
                    }
                    ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                    if (!string.IsNullOrWhiteSpace(cus.Email))
                    {
                        XmlNode node = xml.GetNode("/root/sendgrid_template/shipping_email");
                        //Loc Inactive 20200514
                        //await SendEmail.SendBySendGridWithTemplate(cus.Email, cus.OwnerName, node["template_id"].InnerText, cc, email_data);
                        await _mailingService.SendBySendGridWithTemplate(cus.Email, cus.OwnerName, node["template_id"].InnerText, cc, email_data);
                        //await SendEmailAfterDelivery(cus, UPSTracking, cc);//Loc update 20200514
                    }
                    if (!string.IsNullOrWhiteSpace(cus.CellPhone))
                    {
                        XmlNode node = xml.GetNode("/root/sms_template");
                        var contentSMS = node["shipping_ready"].InnerText;
                        string ownerCountry = string.IsNullOrWhiteSpace(cus.Country) == true ? cus.BusinessCountry : cus.Country;
                        await _sMSService.SendSMSTextline(cus.CellPhone, ownerCountry, contentSMS);
                    }
                    return string.Empty;
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
        }

        public async static Task<O_Orders> RenewLicense(Store_Services s_services, P_Member cmem, DateTime? effective_date, string StoreApply = "Real")
        {
            try
            {
                var renew_invoice = new O_Orders();
                var des = string.Empty;
                using (var db = new WebDataModel())
                {
                    var cus = db.C_Customer.Where(c => c.CustomerCode == s_services.CustomerCode).FirstOrDefault();
                    var partnerCode = cus.PartnerCode;
                    var partner_price_type = db.C_Partner.FirstOrDefault(c => c.Code == partnerCode)?.PriceType;
                    var order_license = db.Order_Subcription.Where(o => o.OrderCode == s_services.OrderCode && o.Product_Code == s_services.ProductCode && o.SubscriptionType != "setupfee" && o.SubscriptionType != "interactionfee").FirstOrDefault();
                    if (order_license == null) throw new Exception("Order subscription not found");
                    var efdate = effective_date ?? DateTime.UtcNow.Date;
                    int Qty = order_license.Period == "MONTHLY" ? (order_license.SubscriptionQuantity ?? 1) : (order_license.Quantity ?? 1);
                    var lp = db.License_Product.AsNoTracking().Where(p => p.Code == s_services.ProductCode).FirstOrDefault();
                    var old_invoice = db.O_Orders.FirstOrDefault(c => c.OrdersCode == s_services.OrderCode);
                    if (lp == null)
                    {
                        throw new Exception("The license does not exist or has been deleted. Please changes other license.");
                    }
                    lp.Price = order_license.RecurringPrice ?? order_license.Price;
                    int countOfOrder = db.O_Orders.Where(o => o.CreatedAt.Value.Year == DateTime.Today.Year
                                               && o.CreatedAt.Value.Month == DateTime.Today.Month).Count();
                    #region Create renew

                    renew_invoice.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                    renew_invoice.OrdersCode = DateTime.Now.ToString("yyMM") + (countOfOrder + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("fff");
                    renew_invoice.CustomerCode = cus.CustomerCode;
                    renew_invoice.CustomerName = cus.BusinessName;
                    renew_invoice.CreatedAt = DateTime.UtcNow;
                    renew_invoice.CreatedBy = cmem.FullName;
                    renew_invoice.Status = InvoiceStatus.Open.ToString();
                    //renew_invoice.TotalHardware_Amount = 0;
                    renew_invoice.ShippingFee = 0;
                    //--note--  TotalHardware_Amount == sub_total
                    renew_invoice.DiscountAmount = 0;
                    renew_invoice.DiscountPercent = 0;
                    renew_invoice.TaxRate = 0;
                    renew_invoice.CreateByMemNumber = cmem.MemberNumber;
                    renew_invoice.InvoiceDate = DateTime.UtcNow.Date;
                    //renew_invoice.DueDate = efdate.AddMonths(Qty);
                    renew_invoice.InvoiceNumber = long.Parse(renew_invoice.OrdersCode);
                    renew_invoice.PartnerCode = partnerCode;
                    renew_invoice.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                    renew_invoice.Renewal = true;
                    renew_invoice.PaymentMethod = old_invoice.PaymentMethod;

                    //handle quantity and price 
                    if (lp.Type == "license")
                    {
                        if (StoreApply == Store_Apply_Status.Real.ToString())
                        {
                            //renew_invoice.TotalHardware_Amount = lp.Price;
                            renew_invoice.GrandTotal = lp.Price * Qty - (order_license.ApplyDiscountAsRecurring == true ? order_license.Discount : 0);
                        }
                        else if (StoreApply == Store_Apply_Status.Promotional.ToString())
                        {
                            //renew_invoice.TotalHardware_Amount = lp.Promotion_Price;
                            renew_invoice.GrandTotal = lp.Promotion_Price;
                        }
                        else if (StoreApply == Store_Apply_Status.Trial.ToString())
                        {
                            //renew_invoice.TotalHardware_Amount = 0;
                            renew_invoice.GrandTotal = 0;
                        }
                    }
                    else
                    {
                        renew_invoice.GrandTotal = lp.Price * Qty - (order_license.ApplyDiscountAsRecurring == true ? order_license.Discount : 0);
                        //renew_invoice.TotalHardware_Amount = lp.Price * Qty;
                    }

                    #endregion
                    #region create order subscription
                    var new_sub = new Order_Subcription()
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + new Random().Next(1, 9999).ToString().PadLeft(4, '0')),
                        Actived = true,
                        AutoRenew = s_services.AutoRenew ?? false,
                        CustomerCode = cus.CustomerCode,
                        CustomerName = cus.BusinessName,
                        IsAddon = lp.isAddon,
                        Price = lp.Price,
                        PriceType = Store_Apply_Status.Real.Text(),
                        RecurringPrice = lp.Price,
                        NumberOfItem = db.License_Product_Item.Where(i => i.License_Product_Id == lp.Id).Count(),
                        OrderCode = renew_invoice.OrdersCode,
                        Period = lp.SubscriptionDuration,
                        ProductId = lp.Id,
                        Quantity = Qty,
                        SubscriptionType = lp.Type,
                        ProductName = lp.Name,
                        Product_Code = lp.Code,
                        StoreCode = s_services.StoreCode,
                        PurcharsedDay = DateTime.UtcNow.Date,
                        ApplyDiscountAsRecurring = s_services.ApplyDiscountAsRecurring,
                        Discount = s_services.ApplyDiscountAsRecurring == true ? (order_license.DiscountPercent > 0 ? (order_license.DiscountPercent * lp.Price / 100) : order_license.Discount) : 0,
                        DiscountPercent = s_services.ApplyDiscountAsRecurring == true ? order_license.DiscountPercent : 0,
                        ApplyPaidDate = true,
                        PeriodRecurring = order_license.PeriodRecurring,
                        SubscriptionQuantity = order_license.SubscriptionQuantity
                    };
                    new_sub.StartDate = efdate;
                    //DateTime nextDate = effective_date.Value;
                    //if (lp.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(lp.NumberOfPeriod.Value);
                    //if (lp.PeriodRecurring == RecurringInterval.Monthly.ToString()) nextDate = nextDate.AddMonths(lp.NumberOfPeriod.Value);
                    //if (lp.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(lp.NumberOfPeriod.Value * 7);
                    //new_sub.EndDate = nextDate;

                    if (lp.SubscriptionDuration == "MONTHLY")
                    {
                        DateTime nextDate = effective_date ?? DateTime.UtcNow.Date;
                        if (lp.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(lp.NumberOfPeriod.Value * (new_sub.SubscriptionQuantity ?? 1));
                        else if (lp.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(lp.NumberOfPeriod.Value * 7 * (new_sub.SubscriptionQuantity ?? 1));
                        else nextDate = nextDate.AddMonths(lp.NumberOfPeriod.Value * (new_sub.SubscriptionQuantity ?? 1));
                        new_sub.EndDate = nextDate;
                    }

                    if (StoreApply == Store_Apply_Status.Real.ToString())
                    {
                        new_sub.Quantity = lp.NumberOfPeriod;
                        new_sub.Price = lp.Price;
                        new_sub.Amount = new_sub.Price * new_sub.Quantity * new_sub.SubscriptionQuantity - new_sub.Discount;
                    }
                    else if (StoreApply == Store_Apply_Status.Promotional.ToString())
                    {
                        new_sub.Quantity = lp.Promotion_Apply_Months;
                        new_sub.Price = lp.Promotion_Price;
                        new_sub.Amount = new_sub.Price - new_sub.Discount;
                    }
                    else if (StoreApply == Store_Apply_Status.Trial.ToString())
                    {
                        new_sub.Quantity = lp.Trial_Months;
                        new_sub.Price = 0;
                        new_sub.Amount = 0;
                    }

                    if (!MerchantType.STORE_IN_HOUSE.Code<string>().Equals(cus.Type))
                    {
                        s_services.LastRenewOrderCode = renew_invoice.OrdersCode;
                        db.O_Orders.Add(renew_invoice);
                    }

                    s_services.LastRenewAt = DateTime.UtcNow;
                    s_services.LastRenewBy = cmem.FullName;
                    s_services.hasRenewInvoiceIncomplete = true;
                    db.Entry(s_services).State = EntityState.Modified;
                    db.Order_Subcription.Add(new_sub);
                    await db.SaveChangesAsync();

                    var result = new StoreViewService().CloseStoreService(renew_invoice.OrdersCode, cus.StoreCode, cmem.FullName, false);
                    if (result.Result != "OK")
                    {
                        throw new Exception(result.Result);
                    }

                    des = "<iframe  width='600' height='900' src='" + ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + renew_invoice.OrdersCode + "&flag=Estimates'></iframe>";

                    DateTime time = DateTime.UtcNow;
                    SalesLeadService _registerMangoService = new SalesLeadService();
                    var host = "/order/estimatesdetail/" + renew_invoice.OrdersCode;
                    string titleLog = "Renew Invoice <b>#" + renew_invoice.OrdersCode + "</b> has automation created.";
                    string description = $"Invoice: <a onclick='show_invoice(" + renew_invoice.OrdersCode + ")'>#" + renew_invoice.OrdersCode + "</a><br/>" +
                                        $"Create at: <span class='UTC-LOCAL'>" + time.ToString("o") + "<span> <br/>";
                    var sale = db.C_SalesLead.Where(s => s.CustomerCode == renew_invoice.CustomerCode).FirstOrDefault();
                    _registerMangoService.CreateLog(sale.Id, sale.CustomerName, titleLog, description, cmem.FullName, cmem.MemberNumber, time);

                    if (renew_invoice.GrandTotal == 0)
                    {
                        await ChangeInvoiceSatus(renew_invoice.OrdersCode, InvoiceStatus.Closed.ToString(), cmem);
                    }

                    ////update recurring planning
                    //var recurringPlanning = db.RecurringPlannings.FirstOrDefault(c => c.OrderCode == s_services.OrderCode && c.SubscriptionCode == s_services.ProductCode);
                    //if (recurringPlanning != null)
                    //{
                    //    //add history recurring
                    //    var newHistory = new RecurringHistory
                    //    {
                    //        RecurringId = recurringPlanning.Id,
                    //        OldOrderCode = recurringPlanning.OrderCode,
                    //        RecurringOrder = new_sub.OrderCode,
                    //        Status = 1,
                    //        CreatedBy = cmem.FullName,
                    //        CreatedAt = DateTime.UtcNow,
                    //        TotalPrice = renew_invoice.GrandTotal
                    //    };
                    //    db.RecurringHistories.Add(newHistory);
                    //    recurringPlanning.EndDate = new_sub.EndDate;
                    //    recurringPlanning.RecurringDate = new_sub.EndDate;
                    //    recurringPlanning.OrderCode = new_sub.OrderCode;
                    //    db.Entry(recurringPlanning).State = EntityState.Modified;
                    //}
                    //db.SaveChanges();
                    await TicketViewService.AutoTicketScenario.NewTicketSalesLead(renew_invoice.CustomerCode, renew_invoice.OrdersCode, des);
                    #endregion
                }
                return renew_invoice;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
        //public async static Task<O_Orders> RenewLicenseV2(string CustomerCode, string OrderCode, string ProductCode, P_Member cmem, DateTime? effective_date, string StoreApply = "Real")
        //{
        //    try
        //    {
        //        var renew_invoice = new O_Orders();
        //        var des = string.Empty;
        //        using (var db = new WebDataModel())
        //        {
        //            var cus = db.C_Customer.Where(c => c.CustomerCode == CustomerCode).FirstOrDefault();
        //            var partnerCode = cus.PartnerCode;
        //            var partner_price_type = db.C_Partner.FirstOrDefault(c => c.Code == partnerCode)?.PriceType;
        //            var order_license = db.Order_Subcription.Where(o => o.OrderCode == OrderCode && o.Product_Code == ProductCode && o.SubscriptionType != "setupfee" && o.SubscriptionType != "interactionfee").FirstOrDefault();
        //            if (order_license == null) throw new Exception("Order subscription not found");
        //            var efdate = effective_date ?? DateTime.UtcNow.Date;
        //            int Qty = order_license.Period == "MONTHLY" ? (order_license.SubscriptionQuantity ?? 1) : (order_license.Quantity ?? 1);
        //            var lp = db.License_Product.AsNoTracking().Where(p => p.Code == ProductCode).FirstOrDefault();
        //            var old_invoice = db.O_Orders.FirstOrDefault(c => c.OrdersCode == OrderCode);
        //            if (lp == null)
        //            {
        //                throw new Exception("The license does not exist or has been deleted. Please changes other license.");
        //            }
        //            lp.Price = order_license.RecurringPrice ?? order_license.Price;
        //            int countOfOrder = db.O_Orders.Where(o => o.CreatedAt.Value.Year == DateTime.Today.Year
        //                                       && o.CreatedAt.Value.Month == DateTime.Today.Month).Count();
        //            #region Create renew

        //            renew_invoice.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
        //            renew_invoice.OrdersCode = DateTime.Now.ToString("yyMM") + (countOfOrder + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("fff");
        //            renew_invoice.CustomerCode = cus.CustomerCode;
        //            renew_invoice.CustomerName = cus.BusinessName;
        //            renew_invoice.CreatedAt = DateTime.UtcNow;
        //            renew_invoice.CreatedBy = cmem.FullName;
        //            renew_invoice.Status = InvoiceStatus.Open.ToString();
        //            //renew_invoice.TotalHardware_Amount = 0;
        //            renew_invoice.ShippingFee = 0;
        //            //--note--  TotalHardware_Amount == sub_total
        //            renew_invoice.DiscountAmount = 0;
        //            renew_invoice.DiscountPercent = 0;
        //            renew_invoice.TaxRate = 0;
        //            renew_invoice.CreateByMemNumber = cmem.MemberNumber;
        //            renew_invoice.InvoiceDate = DateTime.UtcNow.Date;
        //            //renew_invoice.DueDate = efdate.AddMonths(Qty);
        //            renew_invoice.InvoiceNumber = long.Parse(renew_invoice.OrdersCode);
        //            renew_invoice.PartnerCode = partnerCode;
        //            renew_invoice.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
        //            renew_invoice.Renewal = true;
        //            renew_invoice.PaymentMethod = old_invoice.PaymentMethod;

        //            //handle quantity and price 
        //            if (lp.Type == "license")
        //            {
        //                if (StoreApply == Store_Apply_Status.Real.ToString())
        //                {
        //                    //renew_invoice.TotalHardware_Amount = lp.Price;
        //                    renew_invoice.GrandTotal = lp.Price * Qty - (order_license.ApplyDiscountAsRecurring == true ? order_license.Discount : 0);
        //                }
        //                else if (StoreApply == Store_Apply_Status.Promotional.ToString())
        //                {
        //                    //renew_invoice.TotalHardware_Amount = lp.Promotion_Price;
        //                    renew_invoice.GrandTotal = lp.Promotion_Price;
        //                }
        //                else if (StoreApply == Store_Apply_Status.Trial.ToString())
        //                {
        //                    //renew_invoice.TotalHardware_Amount = 0;
        //                    renew_invoice.GrandTotal = 0;
        //                }
        //            }
        //            else
        //            {
        //                renew_invoice.GrandTotal = lp.Price * Qty - (order_license.ApplyDiscountAsRecurring == true ? order_license.Discount : 0);
        //                //renew_invoice.TotalHardware_Amount = lp.Price * Qty;
        //            }

        //            #endregion
        //            #region create order subscription
        //            var new_sub = new Order_Subcription()
        //            {
        //                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + new Random().Next(1, 9999).ToString().PadLeft(4, '0')),
        //                Actived = true,
        //                AutoRenew = s_services.AutoRenew ?? false,
        //                CustomerCode = cus.CustomerCode,
        //                CustomerName = cus.BusinessName,
        //                IsAddon = lp.isAddon,
        //                Price = lp.Price,
        //                PriceType = Store_Apply_Status.Real.Text(),
        //                RecurringPrice = lp.Price,
        //                NumberOfItem = db.License_Product_Item.Where(i => i.License_Product_Id == lp.Id).Count(),
        //                OrderCode = renew_invoice.OrdersCode,
        //                Period = lp.SubscriptionDuration,
        //                ProductId = lp.Id,
        //                Quantity = Qty,
        //                SubscriptionType = lp.Type,
        //                ProductName = lp.Name,
        //                Product_Code = lp.Code,
        //                StoreCode = StoreCode,
        //                PurcharsedDay = DateTime.UtcNow.Date,
        //                ApplyDiscountAsRecurring = s_services.ApplyDiscountAsRecurring,
        //                Discount = s_services.ApplyDiscountAsRecurring == true ? (order_license.DiscountPercent > 0 ? (order_license.DiscountPercent * lp.Price / 100) : order_license.Discount) : 0,
        //                DiscountPercent = s_services.ApplyDiscountAsRecurring == true ? order_license.DiscountPercent : 0,
        //                ApplyPaidDate = true,
        //                PeriodRecurring = order_license.PeriodRecurring,
        //                SubscriptionQuantity = order_license.SubscriptionQuantity
        //            };
        //            new_sub.StartDate = efdate;
        //            if (lp.SubscriptionDuration == "MONTHLY")
        //            {
        //                DateTime nextDate = effective_date ?? DateTime.UtcNow.Date;
        //                if (lp.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(lp.NumberOfPeriod.Value * (new_sub.SubscriptionQuantity ?? 1));
        //                else if (lp.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(lp.NumberOfPeriod.Value * 7 * (new_sub.SubscriptionQuantity ?? 1));
        //                else nextDate = nextDate.AddMonths(lp.NumberOfPeriod.Value * (new_sub.SubscriptionQuantity ?? 1));
        //                new_sub.EndDate = nextDate;
        //            }

        //            if (StoreApply == Store_Apply_Status.Real.ToString())
        //            {
        //                new_sub.Quantity = lp.NumberOfPeriod;
        //                new_sub.Price = lp.Price;
        //                new_sub.Amount = new_sub.Price * new_sub.Quantity * new_sub.SubscriptionQuantity - new_sub.Discount;
        //            }
        //            else if (StoreApply == Store_Apply_Status.Promotional.ToString())
        //            {
        //                new_sub.Quantity = lp.Promotion_Apply_Months;
        //                new_sub.Price = lp.Promotion_Price;
        //                new_sub.Amount = new_sub.Price - new_sub.Discount;
        //            }
        //            else if (StoreApply == Store_Apply_Status.Trial.ToString())
        //            {
        //                new_sub.Quantity = lp.Trial_Months;
        //                new_sub.Price = 0;
        //                new_sub.Amount = 0;
        //            }

        //            if (!MerchantType.STORE_IN_HOUSE.Code<string>().Equals(cus.Type))
        //            {
        //                s_services.LastRenewOrderCode = renew_invoice.OrdersCode;
        //                db.O_Orders.Add(renew_invoice);
        //            }

        //            s_services.LastRenewAt = DateTime.UtcNow;
        //            s_services.LastRenewBy = cmem.FullName;
        //            s_services.hasRenewInvoiceIncomplete = true;
        //            db.Entry(s_services).State = EntityState.Modified;
        //            db.Order_Subcription.Add(new_sub);
        //            await db.SaveChangesAsync();

        //            var result = new StoreViewService().CloseStoreService(renew_invoice.OrdersCode, cus.StoreCode, cmem.FullName, false);
        //            if (result.Result != "OK")
        //            {
        //                throw new Exception(result.Result);
        //            }

        //            des = "<iframe  width='600' height='900' src='" + ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + renew_invoice.OrdersCode + "&flag=Estimates'></iframe>";

        //            DateTime time = DateTime.UtcNow;
        //            SalesLeadService _registerMangoService = new SalesLeadService();
        //            var host = "/order/estimatesdetail/" + renew_invoice.OrdersCode;
        //            string titleLog = "Renew Invoice <b>#" + renew_invoice.OrdersCode + "</b> has automation created.";
        //            string description = $"Invoice: <a onclick='show_invoice(" + renew_invoice.OrdersCode + ")'>#" + renew_invoice.OrdersCode + "</a><br/>" +
        //                                $"Create at: <span class='UTC-LOCAL'>" + time.ToString("o") + "<span> <br/>";
        //            var sale = db.C_SalesLead.Where(s => s.CustomerCode == renew_invoice.CustomerCode).FirstOrDefault();
        //            _registerMangoService.CreateLog(sale.Id, sale.CustomerName, titleLog, description, cmem.FullName, cmem.MemberNumber, time);

        //            if (renew_invoice.GrandTotal == 0)
        //            {
        //                await ChangeInvoiceSatus(renew_invoice.OrdersCode, InvoiceStatus.Closed.ToString(), cmem);
        //            }
        //            await TicketViewService.AutoTicketScenario.NewTicketSalesLead(renew_invoice.CustomerCode, renew_invoice.OrdersCode, des);
        //            #endregion
        //        }
        //        return renew_invoice;
        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }

        //}

        public static async Task<object[]> ChangeInvoiceSatus(string code, string status, P_Member cMem = null, bool writelog = true)
        {
            WebDataModel db = new WebDataModel();
            cMem = cMem ?? new P_Member
            {
                FullName = "IMS Sytem",
            };
            using (var Trans = db.Database.BeginTransaction())
            {
                var order = db.O_Orders.Where(o => o.OrdersCode == code).FirstOrDefault();
                if (order != null)
                {
                    var customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == order.CustomerCode);
                    //Regex pattern = new Regex("[_ ]");
                    //var payment_clear = pattern.Replace(UserContent.ORDER_STATUS.Payment_cleared.ToString(),"");
                    var eStatus = Ext.EnumParse<InvoiceStatus>(status);
                    var eCurStatus = Ext.EnumParse<InvoiceStatus>(order.Status);

                    #region UPDATE STATUS
                    //if (status == InvoiceStatus.Closed.ToString() && !CanCompletedInvoice(order.OrdersCode))
                    //{
                    //    throw new Exception("Order can't be completed when Finance ticket or Deployment ticket is incompleted!");
                    //}


                    order.UpdatedAt = DateTime.UtcNow;
                    order.UpdatedHistory += "|" + DateTime.UtcNow.ToString("dd MMM, yyyy hh:mm tt") + " - By: " + cMem.FullName;

                    //update status history
                    var oldStatus = order.Status;
                    if (status != order.Status)
                    {
                        order.StatusHistory += "|" + Ext.EnumParse<InvoiceStatus>(status).Code<string>() + "  - Update by: " + cMem.FullName + " - At: " + DateTime.UtcNow.ToString("dd MMM, yyyy hh:mm tt");
                    }
                    order.Status = status;
                    if (order.InvoiceNumber == null)
                    {
                        await InventoryViewService.ConvertToInvoice(order, db, true, cMem.FullName);
                    }
                    #endregion
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
                    await db.SaveChangesAsync();
                    Trans.Commit();
                    Trans.Dispose();
                    if (writelog)
                        WriteLogSalesLead(order, true, cMem);

                    await TicketViewService.AutoTicketScenario.NewTicketDeployment(code);

                    if (!status.Equals(InvoiceStatus.Canceled.ToString()))
                    {
                        await new StoreViewService().CloseStoreService(order.OrdersCode, customer.StoreCode, cMem.FullName, true);
                    }
                    long deployment_type = (long)UserContent.TICKET_TYPE.Deployment;
                    if (status == InvoiceStatus.Open.ToString())
                    {
                        //await TicketViewController.AutoTicketScenario.UpdateTicketFromSatellite(order.OrdersCode, UserContent.TICKET_TYPE.Sales, cMem.FullName);
                        using (InvoiceService invoiceService = new InvoiceService())
                        {
                            await invoiceService.SendMailConfirmPayment(customer, order);
                        }
                    }
                    else if (status == InvoiceStatus.Canceled.ToString())
                    {
                        //Nếu có deployment ticket thì cancel ticket và detach devices của deployment đó
                        var deploy_ticket = db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).FirstOrDefault(t => t.T_TicketTypeMapping.Any(x => x.TypeId == deployment_type) && t.OrderCode == order.OrdersCode);
                        if (deploy_ticket != null)
                        {
                            CreateFeedbackCancelInvoice(deploy_ticket.Id, order.Id, order.OrdersCode, db, cMem);
                        }

                        //Refund license
                        if (oldStatus == InvoiceStatus.Paid_Wait.ToString() || oldStatus == InvoiceStatus.Closed.ToString() || oldStatus == InvoiceStatus.Canceled.ToString())
                        {
                            using (MerchantService service = new MerchantService(true))
                            {
                                var licenses = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Type == "license").ToList(); //monthly
                                foreach (var ls in licenses)
                                {
                                    if (ls.Active != 0) await service.ApproveAction(ls.StoreCode, ls.Id, false, "deActive");
                                    else
                                    {
                                        var AddonSchedulers = db.S_AddonSchedulerActivation.Where(c => c.StoreServiceId == ls.Id && c.Status == 0);
                                        foreach (var item in AddonSchedulers)
                                        {
                                            item.Status = -1;
                                            db.Entry(item).State = EntityState.Modified;
                                        }
                                    }
                                    await service.RefundAction(ls); //create refund transaction
                                }
                            }
                        }
                        db.SaveChanges();
                    }
                    if ((eStatus == InvoiceStatus.Closed || eStatus == InvoiceStatus.Paid_Wait) && order.GrandTotal == 0)
                    {
                        var trans = new C_CustomerTransaction()
                        {
                            Id = Guid.NewGuid().ToString("N"),
                            OrdersCode = order.OrdersCode,
                            CustomerCode = order.CustomerCode,
                            StoreCode = customer.StoreCode,
                            Amount = (long)order.GrandTotal,
                            CreateAt = DateTime.UtcNow,
                            PaymentStatus = "Success",
                            UpdateAt = DateTime.UtcNow,
                            PaymentMethod = "Free",
                            PaymentNote = "Auto success with total payment equal 0$",
                            CreateBy = cMem?.FullName
                        };
                        db.C_CustomerTransaction.Add(trans);
                        order.PaymentMethod = "Free";
                        order.PaymentNote = "Auto success with total payment equal 0$";
                        db.SaveChanges();

                    }

                    if (writelog)
                    {

                        if (eStatus == InvoiceStatus.Closed || eStatus == InvoiceStatus.Paid_Wait)
                        {
                            var notificationService = new NotificationService();
                            notificationService.OrderPaidOrCloseNotification(order.SalesMemberNumber, order.Id.ToString(), order.OrdersCode, cMem.FullName, cMem.MemberNumber);
                        }
                        else
                        {
                            var notificationService = new NotificationService();
                            notificationService.OrderUpdateNotification(order.SalesMemberNumber, order.Id.ToString(), order.OrdersCode, cMem.FullName, cMem.MemberNumber);
                        }

                    }

                    //active other khi auto active = false
                    var otherServices = db.Store_Services.Where(c => c.OrderCode == order.OrdersCode && c.Type == "other" && c.Active == -1).ToList();
                    if ((eStatus == InvoiceStatus.Closed || eStatus == InvoiceStatus.Paid_Wait) && otherServices.Count() > 0 && UserContent.GetWebInfomation(true).AutoActiveRecurringLicense == false)
                    {
                        using (MerchantService service = new MerchantService(true))
                        {
                            foreach (var ls in otherServices)
                            {
                                var activelicense = ls.Id;
                                activelicense = await service.ApproveAction(ls.StoreCode, ls.Id, true, "active");
                                if (ls.Period == "MONTHLY")
                                {
                                    var old_recurring = db.Store_Services.FirstOrDefault(s => s.StoreCode == ls.StoreCode && s.Active == 1 && s.RenewDate > DateTime.UtcNow && s.AutoRenew == true && s.LastRenewOrderCode == order.OrdersCode);
                                    if (ls.AutoRenew == true && order.PaymentMethod != "Recurring")
                                    {
                                        if (old_recurring != null && old_recurring?.AutoRenew == true) { PaymentService.SetStatusRecurring(old_recurring.Id, "inactive"); }
                                        PaymentService.SetStatusRecurring(activelicense, "active");
                                    }
                                }
                            }
                        }
                    }

                    //active khi set auto active = true
                    if ((eStatus == InvoiceStatus.Closed || eStatus == InvoiceStatus.Paid_Wait || eStatus == InvoiceStatus.PaymentLater) && UserContent.GetWebInfomation(true).AutoActiveRecurringLicense == true)
                    {
                        var msg = true;
                        var licenses = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Period == "MONTHLY" && s.Active == -1).ToList(); //monthly
                        if (licenses.Count > 0)
                        {
                            foreach (var ls in licenses)
                            {
                                var activelicense = ls.Id;
                                using (MerchantService service = new MerchantService(true))
                                {
                                    activelicense = await service.ApproveAction(ls.StoreCode, ls.Id, true, "active");
                                    if (activelicense != ls.Id) msg = false;
                                }
                                var old_recurring = db.Store_Services.FirstOrDefault(s => s.StoreCode == ls.StoreCode && s.Active == 1 && s.RenewDate > DateTime.UtcNow && s.AutoRenew == true && s.LastRenewOrderCode == order.OrdersCode);
                                if (ls.AutoRenew == true && order.PaymentMethod != "Recurring")
                                {
                                    if (old_recurring != null && old_recurring?.AutoRenew == true) { PaymentService.SetStatusRecurring(old_recurring.Id, "inactive"); }
                                    PaymentService.SetStatusRecurring(activelicense, "active");
                                }
                            }
                        }
                        var adds = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Period != "MONTHLY" && s.Active == -1).ToList(); //one time
                        if (adds.Count > 0)
                        {
                            using (MerchantService service = new MerchantService(true))
                            {
                                foreach (var add in adds)
                                {
                                    var activelicense = await service.ApproveAction(add.StoreCode, add.Id, true, "active");
                                    if (activelicense != add.Id) msg = false;
                                }
                            }
                        }

                        if (msg && !db.Order_Products.Any(p => p.OrderCode == order.OrdersCode) && eStatus != InvoiceStatus.PaymentLater)
                        {
                            order.Status = InvoiceStatus.Closed.ToString();
                            db.Entry(order).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                        }

                        PaymentService.UpdateRecurringStatus(order.CustomerCode);
                    }
                    if (order.InvoiceNumber > 0)
                    {
                        await OrderViewService.CheckMerchantWordDetermine(order.CustomerCode, db);
                    }

                    //check to make go high level contact type = customer 
                    if(eStatus == InvoiceStatus.Closed || eStatus == InvoiceStatus.Paid_Wait || eStatus == InvoiceStatus.PaymentLater)
                    {
						var checkFirstInvoicePaid = db.C_CustomerTransaction.Any(x => x.CustomerCode == customer.CustomerCode && x.OrdersCode != order.OrdersCode);
                        if (checkFirstInvoicePaid == false)
                        {
							try
							{
								var _goHighLevelConnectorService = EngineContext.Current.Resolve<IGoHighLevelConnectorService>();
								await _goHighLevelConnectorService.ChangeContactTypeToCustomerAsync(lead.Id);
							}
							catch (Exception ex)
							{
                                ImsLogService.WriteLog(ex.ToString(), "Exception", "ChangeInvoiceSatus");
                            }
						}
						try
						{
                            // var _clickUpConnectorService = EngineContext.Current.Resolve<IClickUpConnectorService>();
							//await _clickUpConnectorService.SyncMerchantToClickUpAsync(customer.Id.ToString());
                            //StoreServices _storeService = EngineContext.Current.Resolve<StoreServices>();
                            await new StoreServices(db).SynDeliveryToClickUpAsync(order.OrdersCode);


                        }
						catch (Exception ex)
						{
                            ImsLogService.WriteLog(ex.ToString(), "Exception", "ChangeInvoiceSatus");
                        }
					}
                    return new object[] { true, order.Status, Ext.EnumParse<InvoiceStatus>(order.Status).ToString() };
                }
                else
                {
                    throw new Exception("Invoice not found");
                }
            }
        }
        public static void WriteLogSalesLead(O_Orders order, bool isUpdate, P_Member cMem, bool isInvoice = false)
        {
            WebDataModel db = new WebDataModel();
            SalesLeadService _salesLeadService = new SalesLeadService();
            var sl = db.C_SalesLead.Where(x => x.CustomerCode == order.CustomerCode).FirstOrDefault();
            //var host = $"{HttpContext.Request.Url.Scheme}://{HttpContext.Request.Url.Authority}";
            string titleLog = "";
            string descriptionLog = "";
            if (isUpdate)
            {
                if (order.Status != InvoiceStatus.Paid_Wait.ToString() && order.Status != InvoiceStatus.Closed.ToString())
                {
                    titleLog = "Invoice <b>#" + order.OrdersCode + "</b> has been updated";
                    //descriptionLog = "Invoice <a  target='_blank' href='" + host + "/order/estimatesdetail/" + order.Id + "' > #" + order.OrdersCode + "</a> was updated";
                    descriptionLog = "Invoice <a onclick='show_invoice(" + order.OrdersCode + ")'> #" + order.OrdersCode + "</a> has been updated";
                }
                else if (order.Status == InvoiceStatus.Closed.ToString())
                {
                    titleLog = "Invoice <b>#" + order.OrdersCode + "</b> has been completed";
                    //descriptionLog = "Estimate <a  target='_blank' href='" + host + "/order/estimatesdetail/" + order.Id + "' > #" + order.OrdersCode + "</a> was completed";
                    descriptionLog = "Invoice <a onclick='show_invoice(" + order.OrdersCode + ")'> #" + order.OrdersCode + "</a> has been completed";
                }
            }
            else
            {
                if (isInvoice)
                {
                    titleLog = "Invoice #<b>" + order.OrdersCode + "</b> has been created";
                    descriptionLog = "Invoice <a onclick='show_invoice(" + order.OrdersCode + ")'> #" + order.OrdersCode + "</a> has been created";
                }
                else
                {
                    titleLog = "Estimate #<b>" + order.OrdersCode + "</b> has been created";
                    descriptionLog = "Estimate <a onclick='show_invoice(" + order.OrdersCode + ")'> #" + order.OrdersCode + "</a> has been created";
                }
            }

            if (!string.IsNullOrEmpty(titleLog) && sl != null)
                _salesLeadService.CreateLog(SalesLeadId: sl.Id, SalesLeadName: sl.L_SalonName, title: titleLog, description: descriptionLog, MemberNumber: cMem.MemberNumber);
        }

        /// <summary>
        /// CanCompletedInvoice
        /// </summary>
        /// <param name="orderCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        //public static bool CanCompletedInvoice(string orderCode)
        //{
        //    using (var db = new WebDataModel())
        //    {
        //        var Paid_Wait = InvoiceStatus.Paid_Wait.ToString();
        //        var finanl_type = (long)UserContent.TICKET_TYPE.Finance;
        //        var finanl_completed = (long)UserContent.TICKET_STATUS.Finance_Complete;
        //        if (db.T_SupportTicket.Any(o => o.OrderCode == orderCode && o.TypeId == finanl_type && o.StatusId != finanl_completed))
        //            throw new AppHandleException("Orders cannot be completed without payment!");

        //        if (db.T_SupportTicket.Any(t => t.OrderCode == orderCode && t.TypeId == 4 && t.StatusId != 17))
        //            throw new AppHandleException("Orders cannot be completed when Deployment ticket uncompleted !");

        //        return true;
        //    }
        //}
        //Loc Update 20200514
        public async Task<string> SendEmailAfterDelivery(C_Customer customer, Order_UPSTracking UPSTracking, string cc)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var order = db.O_Orders.Where(o => o.OrdersCode == UPSTracking.OrderCode).FirstOrDefault();
                if (order == null)
                {
                    throw new Exception("Customer not found");
                }

                var customerAddress = customer?.BusinessAddressStreet + ", " + customer?.BusinessCity + ", " + customer?.BusinessState + ", " + customer?.BusinessZipCode + ", " + customer.BusinessCountry;
                var packageCount = db.Order_UPSPackage.Where(x => x.OrderUPSTrackingId == UPSTracking.TrackingNumber).Count();

                var _bodyData = "<tbody>";
                foreach (var item in db.Order_Products.Where(p => p.OrderCode == order.OrdersCode).ToList())
                {
                    _bodyData += "<tr><td>" + item.ModelName + "</td><td>" + item.ProductName + "</td><td style='text-align:center'>" + item.Quantity + "</td></tr>";
                }
                _bodyData += "</tbody>";

                var _subject = "[ENRICHCOUS]Product shipment and tracking info";
                var _body = "<b><i style='font-family:Georgia,Times,serif; font-size:20px'>Dear " + customer?.OwnerName + ",</i></b><br/>" +
                    "Thanks so much for choosing Enrich. We have shipped out your order products.<br/><br/>" +
                    "<table style='border-collapse:collapse; width:100%'><tbody>" +
                    "<tr style='border-bottom:1px solid grey'><td colspan='2' style='text-align:right'>ORDER <b>#" + order?.OrdersCode + "</b></td></tr>" +
                    "<tr><td><br><b>Customer</b><br>" + customer?.OwnerName + "<br>" + customer?.OwnerMobile + "<br>" + (!string.IsNullOrEmpty(customerAddress) ? customerAddress : "") + "</td>" +
                    "<td><br><b>Shipping Address</b ><br>" + customer?.OwnerName + "<br>" + customer?.OwnerMobile + "<br>" + order?.ShippingAddress?.Replace("|", ",") + "</td></tr>" +
                    "<tr><td colspan='2'></td></tr>" +
                    "<tr><td colspan='2'><br/><b>Delivered by UPS</b><br><b>Status:</b> ready to delivery</td></tr>" +
                    "<tr><td colspan='2'></td></tr>" +
                    "<tr><td colspan='2'><br/>" +
                    "<table class='table_border' style='width:100%;border-collapse:collapse;border:1px solid grey;' cellpadding='5'>" +
                    "<thead style='background-color:#f1eeee'><tr style='font-weight:bold;text-align:center'>" +
                    "<td>Model#</td><td>Hardware Name</td><td>Quantity</td>" +
                    "</tr></thead>" + _bodyData +
                    "</table>" +
                    "</td></tr>" +
                    "</tbody></table><br/>" +
                    "The tracking number is <b>" + UPSTracking?.TrackingNumber + "</b><br/>Number of package(s): <b>" + packageCount + "</b><br/><br/>" +
                    "You can track via this link:<br/>" +
                    "<a href='https://www.ups.com/track?loc=en_US&requester=ST/' taget='_blank'>https://www.ups.com/track?loc=en_US&requester=ST/</a><br/><br/>" +
                    "Best regards,<br/>Enrich Inventory Team";

                var result = await _mailingService.SendBySendGrid(customer?.Email, customer?.OwnerName, _subject, _body, cc);
                if (!string.IsNullOrEmpty(result))
                {
                    throw new Exception(result);
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async static Task<O_Orders> AddAddon(string cus_code, License_Product addon, P_Member cmem, DateTime effect_date, DateTime? end_date, int month_qty, bool autorenew = false, string StoreApply = "Real", int quantity = 1, bool customdate = false)
        {
            using (var db = new WebDataModel())
            {
                    var cus = db.C_Customer.Where(c => c.CustomerCode == cus_code).FirstOrDefault() ?? new C_Customer();
                    var sl = db.C_SalesLead.FirstOrDefault(s => s.CustomerCode == cus.CustomerCode);
                    var partner_price_type = db.C_Partner.FirstOrDefault(c => c.Code == cus.PartnerCode)?.PriceType;
                    var systemaccount = ConfigurationManager.AppSettings["SystemAccount"]?.ToString();
                    if (!string.IsNullOrEmpty(cus.PartnerCode) && systemaccount != cmem.PersonalEmail)
                    {
                        addon.Price = partner_price_type == "membership" ? addon.MembershipPrice : addon.PartnerPrice;
                    }

                    int countOfOrder = db.O_Orders.Where(o => o.CreatedAt.Value.Year == DateTime.Today.Year
                                               && o.CreatedAt.Value.Month == DateTime.Today.Month).Count();
                    if (sl != null)
                    {
                        //can xem lai....
                        sl.SL_Status = LeadStatus.Merchant.Code<int>();
                        sl.SL_StatusName = LeadStatus.Merchant.Text();
                        db.Entry(sl).State = EntityState.Modified;
                    }

                    #region new order order
                    var addon_invoice = new O_Orders();

                    addon_invoice.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                    addon_invoice.OrdersCode = DateTime.Now.ToString("yyMM") + (countOfOrder + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("fff");
                    addon_invoice.CustomerCode = cus.CustomerCode;
                    addon_invoice.CustomerName = cus.BusinessName;
                    addon_invoice.CreatedAt = DateTime.UtcNow;
                    addon_invoice.CreatedBy = cmem.FullName;
                    addon_invoice.Status = InvoiceStatus.Open.ToString();
                    // addon_invoice.TotalHardware_Amount = 0;
                    addon_invoice.ShippingFee = 0;
                    addon_invoice.DiscountAmount = 0;
                    addon_invoice.DiscountPercent = 0;
                    addon_invoice.TaxRate = 0;
                    addon_invoice.CreateByMemNumber = cmem.MemberNumber;
                    addon_invoice.InvoiceDate = DateTime.UtcNow.Date;
                    //addon_invoice.DueDate = effect_date.AddMonths(1);
                    addon_invoice.InvoiceNumber = long.Parse(addon_invoice.OrdersCode);
                    addon_invoice.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
                    addon_invoice.TotalHardware_Amount = 0;
                    addon_invoice.GrandTotal = 0;
                    addon_invoice.PartnerCode = cus.PartnerCode;
                    //var priceAddon = addon.Price; //string.IsNullOrEmpty(partner_price_type) ? addon.Price : (partner_price_type == "parnerprice" ? addon.PartnerPrice : addon.MembershipPrice);
                    //handle quantity and price 
                    if (addon.Type == "license")
                    {
                        if (!string.IsNullOrEmpty(StoreApply))
                        {
                            if (StoreApply.Split(',').Contains(Store_Apply_Status.Trial.Text()))
                            {
                                addon_invoice.TotalHardware_Amount += 0;
                                addon_invoice.GrandTotal += 0;
                            }
                            if (StoreApply.Split(',').Contains(Store_Apply_Status.Promotional.Text()))
                            {
                                addon_invoice.TotalHardware_Amount += addon.Promotion_Price;
                                addon_invoice.GrandTotal += addon.Promotion_Price;
                            }
                            if (StoreApply.Split(',').Contains(Store_Apply_Status.Real.Text()))
                            {
                                addon_invoice.TotalHardware_Amount += (addon.Price * quantity);
                                addon_invoice.GrandTotal += (addon.Price * quantity);
                            }
                        }
                        else
                        {
                            addon_invoice.TotalHardware_Amount = (addon.Price * quantity);
                            addon_invoice.GrandTotal = (addon.Price * quantity);
                        }
                    }
                    else
                    {
                        if (addon.SubscriptionDuration == "MONTHLY")
                        {
                            addon_invoice.GrandTotal += (addon.Price * quantity);
                            addon_invoice.TotalHardware_Amount += (addon.Price * quantity);
                        }
                        else
                        {
                            addon_invoice.GrandTotal += addon.Price * month_qty * quantity;
                            addon_invoice.TotalHardware_Amount += addon.Price * month_qty * quantity;
                        }

                    }
                    #endregion
                    #region new order subscription
                    var new_sub = new Order_Subcription()
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + new Random().Next(1, 9999).ToString().PadLeft(4, '0')),
                        Actived = true,
                        AutoRenew = autorenew,
                        CustomerCode = cus.CustomerCode,
                        CustomerName = cus.BusinessName,
                        IsAddon = addon.isAddon,
                        NumberOfItem = db.License_Product_Item.Where(i => i.License_Product_Id == addon.Id).Count(),
                        OrderCode = addon_invoice.OrdersCode,
                        Period = addon.SubscriptionDuration ?? "ONETIME",
                        ProductId = addon.Id,
                        ProductName = addon.Name,
                        Product_Code = addon.Code,
                        Product_Code_POSSystem = addon.Code_POSSystem,
                        SubscriptionType = addon.Type,
                        StoreCode = cus.StoreCode,
                        PurcharsedDay = DateTime.UtcNow.Date,
                        Quantity = 0,
                        Price = addon.Price, //string.IsNullOrEmpty(partner_price_type) ? addon.Price : (partner_price_type == "parnerprice" ? addon.PartnerPrice : addon.MembershipPrice),
                        PriceType = StoreApply,
                        PeriodRecurring = addon.PeriodRecurring,
                        SubscriptionQuantity = addon.SubscriptionDuration != "MONTHLY" ? 1 : quantity,
                        Amount = 0,
                        ApplyPaidDate = customdate,
                        RecurringPrice = addon.Price
                    };
                    if (addon.Type == "license")
                    {
                        if (!string.IsNullOrEmpty(StoreApply))
                        {
                            if (StoreApply.Split(',').Contains(Store_Apply_Status.Trial.Text()))
                            {
                                //new_sub.Quantity = addon.Trial_Months;
                                new_sub.Amount = 0;
                            }
                            if (StoreApply.Split(',').Contains(Store_Apply_Status.Promotional.Text()))
                            {
                                //new_sub.Quantity += addon.Promotion_Apply_Months;
                                new_sub.Amount += addon.Promotion_Price;
                            }
                            if (StoreApply.Split(',').Contains(Store_Apply_Status.Real.Text()))
                            {
                                new_sub.Quantity += addon.NumberOfPeriod;
                                new_sub.Amount += new_sub.Price * quantity;
                            }
                        }
                        else
                        {
                            new_sub.Quantity = addon.NumberOfPeriod;
                            new_sub.Amount = new_sub.Price * quantity;
                        }
                    }
                    else if (addon.Type != "license" && addon.SubscriptionDuration == "MONTHLY")
                    {
                        new_sub.Quantity = month_qty;
                        new_sub.Amount = new_sub.Price * quantity;
                    }
                    else
                    {
                        new_sub.Quantity = month_qty * quantity;
                        new_sub.Amount = new_sub.Price * month_qty * quantity;
                    }

                    new_sub.StartDate = effect_date;
                    if (new_sub.Period == "MONTHLY")
                    {
                        new_sub.EndDate = end_date ?? effect_date.AddMonths(new_sub.Quantity.Value);
                    }

                    #endregion


                        addon_invoice.GrandTotal = addon_invoice.GrandTotal ?? 0;
                        addon_invoice.TotalHardware_Amount = addon_invoice.TotalHardware_Amount ?? 0;
                        db.Order_Subcription.Add(new_sub);
                        db.O_Orders.Add(addon_invoice);

                        await db.SaveChangesAsync();
                        WriteLogSalesLead(addon_invoice, false, cmem, true);
                        var result = new StoreViewService().CloseStoreService(addon_invoice.OrdersCode, cus.StoreCode, cmem.FullName, true, new_sub);
                        if (result.Result != "OK")
                        {
                            throw new Exception(result.Result);
                        }
                        if (addon_invoice.GrandTotal == 0)
                        {
                            await ChangeInvoiceSatus(addon_invoice.OrdersCode, InvoiceStatus.Paid_Wait.ToString(), cmem);
                        }
                        return addon_invoice;

            }
        }

        public static async Task<object[]> UpdatePayment(string invoiceCode, string paymentMethod = "", string paymentNote = "", string bankName = "", string cardNumber = "", string paymentDate = "", string status = "", bool writeLog = false, P_Member cMem = null)
        {
            using (WebDataModel db = new WebDataModel())
            {
                var order = db.O_Orders.Where(o => o.OrdersCode == invoiceCode).FirstOrDefault();
                var cus = db.C_Customer.Where(x => x.CustomerCode == order.CustomerCode).FirstOrDefault();
                var p_card = db.C_PartnerCard.FirstOrDefault(c => c.Id == cardNumber);
                var card = new C_CustomerCard { };
                if (p_card != null)
                {
                    card = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(p_card));
                }
                else
                {
                    card = db.C_CustomerCard.FirstOrDefault(c => c.Id == cardNumber) ?? new C_CustomerCard { };
                }
                

                if (order != null)
                {
                    //update start date for merchant
                    var checkFisrtTransaction = db.C_CustomerTransaction.Any(x => x.CustomerCode == cus.CustomerCode);
                    if (checkFisrtTransaction == false && cus.BusinessStartDate == null)
                    {
                        cus.BusinessStartDate = DateTime.UtcNow;
                    }

                    var payDate = string.IsNullOrEmpty(paymentDate) ? DateTime.UtcNow : Convert.ToDateTime(paymentDate);
                    var transaction = new C_CustomerTransaction
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        PaymentStatus = "Approved",
                        CustomerCode = order.CustomerCode,
                        OrdersCode = order.OrdersCode,
                        StoreCode = cus.StoreCode,
                        Amount = order.GrandTotal ?? decimal.Zero,
                        CreateAt = DateTime.UtcNow,
                        BankName = bankName,
                        CardNumber = card.CardNumber,
                        PaymentMethod = paymentMethod,
                        PaymentNote = paymentNote,
                        CreateBy = cMem?.FullName,
                        Card = card.Id,
                        ResponseText = "Approved or completed successfully",
                    };
                    db.C_CustomerTransaction.Add(transaction);
                    await db.SaveChangesAsync();
                    var tranReq = new TransRequest
                    {
                        Key = order.OrdersCode.ToBase64(),
                        PaymentMethod = paymentMethod,
                        PaymentNote = paymentNote
                    };
                    //UpdateWordDetermine(cus.CustomerCode);
                    await new PaymentService().UpdateOrder(tranReq, transaction, null);
                    await ChangeInvoiceSatus(invoiceCode, status, cMem, writeLog);
                    //if (writeLog)
                    //    this.WriteLogSalesLead(order, true);

                    return new object[] { true, status, Ext.EnumParse<InvoiceStatus>(status).Code<string>() };
                }
                throw new Exception("Invoice not found");
            }
        }

        

        public static void UpdateWordDetermine(string CustomerCode)
        {
            WebDataModel db = new WebDataModel();
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
        }

        public static void CreateFeedbackCancelInvoice(long TicketId, long OrderId, string OrderCode, WebDataModel db, P_Member cMem = null)
        {
            var feedback = new T_TicketFeedback
            {
                Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                TicketId = TicketId,
                Feedback = "Order <a href='/order/estimatesdetail/" + OrderId + "' target='_blank'>#" + OrderCode + "</a> has been canceled - at: " + DateTime.Now.ToString("MMM dd, yyyy hh:mm tt") + ", by: " + cMem.FullName,
                //CreateByNumber = cMem.MemberNumber,
                CreateByName = "System",
                CreateAt = DateTime.UtcNow,
                FeedbackTitle = "Order #" + OrderCode + "</a> has been canceled",
                DateCode = DateTime.UtcNow.ToString("yyyyMMdd")
            };
            db.T_TicketFeedback.Add(feedback);
        }

    }
}