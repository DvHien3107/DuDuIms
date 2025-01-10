using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Enrich.Core.Infrastructure;
using Enrich.Core.Ultils;
using Enrich.DataTransfer;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Enrichcous.Payment.Mxmerchant.Config.Enums;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.AppLB.OptionConfig;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Extensions;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.Respon;
using EnrichcousBackOffice.NextGen;
using EnrichcousBackOffice.Utils;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.Utils.OptionConfig;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.ViewModel;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using NPOI.HSSF.Record.Chart;
using static EnrichcousBackOffice.Services.FeatureService;
using static iTextSharp.text.pdf.AcroFields;

namespace EnrichcousBackOffice.Services
{
    public class MerchantService : IServicesBase
    {
        private StoreProfile profileInPos;
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        public MerchantService() : base()
        {
        }
        public MerchantService(bool trans = false) : base(trans)
        {
        }
        public MerchantService(WebDataModel db) : base(db)
        {
        }


        /// <summary>
        /// Make merchant id.
        /// Merchant #'s: A+  5 Digits + [Q/K/H]
        /// -Q for Terminal & POS customers
        /// -K for POS only
        /// -H for Terminal Only customers
        /// Example : A12345-Q
        /// </summary>
        /// <returns></returns>
        /// -----------------Fixbug-----------------
        public string MakeId()
        {
            return $"A{(int.Parse((DB.C_Customer.Where(c => c.CustomerCode.Substring(0, 1) == "A").Max(c => c.CustomerCode) ?? "A0").Substring(1)) + 1).ToString().PadLeft(5, '0')}";
        }

        /// <summary>
        /// PrimitiveStoreProfile
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public StoreProfile PrimitiveStoreProfile(string storeCode, string stage = null, string deactive_store = null, bool getDirectFromPosApi = false)
        {
            List<Licenses> licensesCurrent = new StoreServices(DB).LicensesCurrent(storeCode, deactive_store);
            StoreProfile model = new StoreServices(DB).GetStore(storeCode);
            model?.Licenses?.ForEach(license =>
            {
                Licenses licenseCurrent = licensesCurrent.FirstOrDefault(li => li.LicenseCode == license.LicenseCode);
                license.Start_period = AppFunc.ParseTime(license.Start_period ?? licenseCurrent?.Start_period, StoreServices.F_DATE);
                if (!string.IsNullOrEmpty(stage) && stage.ToLower() == "deactive")
                {
                    license.End_period = getDirectFromPosApi == false ? DateTime.UtcNow.ToString("MM/dd/yyyy") : null;
                }
                else
                {
                    var licenseEndDate = getDirectFromPosApi == false ? licenseCurrent.End_period : license?.End_period;
                    if (licenseEndDate != null)
                        license.End_period = AppFunc.ParseTime(licenseEndDate, StoreServices.F_DATE);
                }

                license.Count_current = (license.Count_current ?? "0").Split('.')[0];
                license.Count_limit = (license?.Count_limit ?? licenseCurrent?.Count_limit ?? "0").Split('.')[0];
            });
            return model;
        }



        /// <summary>
        /// GetStoreProfile
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public StoreProfile GetStoreProfile(string storeCode, string stage = null, string deactive_store = null)
        {
            if (profileInPos != null) return profileInPos;
            try
            {
                StoreProfile model = PrimitiveStoreProfile(storeCode, stage, deactive_store);
                model = RepairStore(model, stage, deactive_store);
                profileInPos = model;
                return model;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// GetStoreProfile
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public StoreProfile StoreProfileWithDefault(string storeCode, string deactive_store = null)
        {
            StoreProfile profile = null;
            try
            {
                profile = GetStoreProfile(storeCode, null, deactive_store);
            }
            catch (Exception e) { Console.WriteLine(e.StackTrace); }
            if (profile == null)
            {
                Store_Services main = GetMainSubscription(storeCode);
                if (main == null)
                {
                    var cus = DB.C_Customer.FirstOrDefault(_cus => _cus.StoreCode == storeCode)?.Version;
                    return cus == IMSVersion.POS_VER2.Code<string>() ? new StoreProfileV2() : new StoreProfile();
                }
                profile = BuildStoreProfile(main.Id);
                profile.Status = "DeActivated";
            }

            if (profile.activeProducts == null || profile.activeProducts.Count == 0)
            {
                profile.activeProducts = MakeActiveProducts(storeCode);
            }

            return profile;
        }

        /// <summary>
        /// GenerateStorePassword
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<string> GenerateStorePassword(string storeCode, string newPass, string cc, bool requireChange, bool is_sendNoty, HttpRequestBase request)
        {
            // Generate password
            if (string.IsNullOrEmpty(newPass))
            {
                string seed = DateTime.UtcNow.ToString("O");
                newPass = SecurityLibrary.Md5Encrypt(seed).Substring(0, 6);
            }

            {
                try
                {
                    // Update customer password
                    C_Customer cus = DB.C_Customer.FirstOrDefault(c => c.StoreCode == storeCode);
                    if (cus == null)
                    {
                        throw new Exception("Customer not found!");
                    }

                    cus.MD5PassWord = SecurityLibrary.Md5Encrypt(newPass);
                    // Todo : REMOVE PASS
                    // cus.Password = null;
                    cus.Password = newPass;
                    cus.UpdateBy = cMem?.FullName ?? "IMS System";
                    cus.RequirePassChange = requireChange ? "on" : "off";
                    DB.Entry(cus).State = EntityState.Modified;

                    // Call api update store
                    //StoreProfile storeProfile = StoreProfileWithDefault(storeCode);
                    StoreProfileReq storeProfile = GetStoreProfileReady(null, false, null, storeCode);
                    storeProfile.Password = cus.MD5PassWord;
                    storeProfile.RequirePassChange = cus.RequirePassChange;
                    DoRequest(storeProfile);
                    string link = requireChange ? $"{AppConfig.Host}/Page/salon/info/{cus.StoreCode}?code={cus.MD5PassWord}&target=reset" : "";
                    // Send mail template
                    string to = string.Join(";", cus.SalonEmail);
                    string firstname = string.Join(";", cus.BusinessName);
                    string ccM = string.Join(";", new string[] { cus.MangoEmail, cc });

                    if (is_sendNoty)
                        await EngineContext.Current.Resolve<IMailingService>().SendNotifyResetPassword(to, cus.ContactName, cus.BusinessName, cus.SalonEmail ?? cus.Email, newPass, ccM, link);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    throw new Exception("There was an error. Please try again later or contact us.");
                }
            }
            return newPass;
        }

        /// <summary>
        /// Post store info
        /// </summary>
        /// <param name="storeCode"></param>
        /// <param name="licenseId"></param>
        /// <param name="activeStatus"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public async Task<string> ApproveAction(string storeCode, string licenseId, bool activeStatus, string stage = null)
        {
            try
            {
                var processDate = DateTime.UtcNow;
                var isStayActive = false;
                var current_plan = new Store_Services { };
                var activeCode = LicenseStatus.ACTIVE.Code<int>();
                var resetCode = LicenseStatus.RESET.Code<int>();
                C_Customer cus = DB.C_Customer.FirstOrDefault(c => c.StoreCode == storeCode);
                if (cus == null) throw new Exception("Customer not found!");
                Store_Services storeOrder = DB.Store_Services.FirstOrDefault(s => s.Id == licenseId);
                if (storeOrder == null) throw new Exception("Store service not found!");
                var order = DB.O_Orders.FirstOrDefault(c => c.OrdersCode == storeOrder.OrderCode) ?? new O_Orders();
                storeOrder.StoreApply = storeOrder.StoreApply ?? "Real";
                int license_qty = DB.Order_Subcription.Where(o => o.OrderCode == storeOrder.OrderCode && o.Product_Code == storeOrder.ProductCode).FirstOrDefault()?.Quantity ?? 1;
                Store_Services current_Services = DB.Store_Services.FirstOrDefault(s =>
                    (s.Active == activeCode || s.Active == resetCode) &&
                    s.StoreCode == storeOrder.StoreCode &&
                    s.ProductCode == storeOrder.ProductCode &&
                    s.LastRenewOrderCode == storeOrder.OrderCode &&
                    //(s.StoreApply == storeOrder.StoreApply || string.IsNullOrEmpty(s.StoreApply)) &&
                    s.Period == "MONTHLY" &&
                    s.Id != licenseId);
                if (storeOrder != null)
                {
                    if (current_Services != null && activeStatus && current_Services?.LastRenewOrderCode == storeOrder.OrderCode)
                    {
                        isStayActive = true;
                         current_Services.hasRenewInvoiceIncomplete = false;
                        current_Services.AutoRenew = storeOrder.AutoRenew;

                        //
                        current_Services.Active = LicenseStatus.REMOVED.Code<int>();
                        current_Services.LastUpdateAt = processDate;
                        current_Services.LastUpdateBy = cMem?.FullName ?? "IMS System";
                        current_Services.LastRenewOrderCode = storeOrder.OrderCode;

                        storeOrder.EffectiveDate = current_Services.EffectiveDate;
                        storeOrder.Active = LicenseStatus.ACTIVE.Code<int>();
                        storeOrder.LastUpdateAt = processDate;
                        storeOrder.LastUpdateBy = cMem?.FullName ?? "IMS System";

                        DB.Entry(storeOrder).State = EntityState.Modified;
                        DB.Entry(current_Services).State = EntityState.Modified;

                    }
                    else
                    {
                        isStayActive = storeOrder.Active == LicenseStatus.ACTIVE.Code<int>();
                        storeOrder.Active = (activeStatus ? LicenseStatus.ACTIVE : (stage == "reset" ? LicenseStatus.RESET : LicenseStatus.DE_ACTIVE)).Code<int>();
                        storeOrder.LastUpdateAt = processDate;
                        storeOrder.LastUpdateBy = cMem?.FullName ?? "IMS System";
                        DB.Entry(storeOrder).State = EntityState.Modified;
                        if (activeStatus)
                        {
                            var list_services = DB.Store_Services.Where(s => s.OrderCode == storeOrder.OrderCode && s.Id == storeOrder.Id).ToList();

                            if (storeOrder.Type == LicenseType.LICENSE.Text() || list_services.Any(s => s.Type == LicenseType.LICENSE.Text()))
                            {
                                current_plan = DB.Store_Services.FirstOrDefault(s => s.Type == "license" && s.StoreCode == storeCode && s.Active == 1);
                                if (current_plan != null)
                                {
                                    current_plan.Active = stage == "active" ? -2 : 0;
                                    current_plan.LastUpdateAt = processDate;
                                    current_plan.LastUpdateBy = cMem?.FullName ?? "IMS System";
                                    DB.Entry(current_plan).State = EntityState.Modified;
                                    PaymentService.SetStatusRecurring(current_plan.Id, "inactive");
                                    WriteLogMerchant(cus, "Updated subscription", "Deactived subscription <b>" + current_plan.Productname + "</b>.");
                                }
                            }

                            if (list_services.Count > 0)
                            {
                                foreach (var sv in list_services)
                                {
                                    sv.Active = LicenseStatus.ACTIVE.Code<int>();
                                    sv.LastUpdateAt = processDate;
                                    sv.LastUpdateBy = cMem?.FullName ?? "IMS System";
                                    DB.Entry(sv).State = EntityState.Modified;

                                    //create addon scheduler activation
                                    var AddonScheduler = DB.S_AddonSchedulerActivation.Where(c => c.OrderCode == sv.OrderCode && c.ProductCode == sv.ProductCode);
                                    for (int i = 1; i < (sv.Quantity ?? 1) && sv.Type == "addon" && AddonScheduler.Count() == 0; i++)
                                    {
                                        var effectiveDate = sv.EffectiveDate ?? DateTime.UtcNow;
                                        if (sv.PeriodRecurring == "Yearly") effectiveDate = effectiveDate.AddYears(license_qty * i);
                                        else if (sv.PeriodRecurring == "Weekly") effectiveDate = effectiveDate.AddDays(license_qty * 7 * i);
                                        else effectiveDate = effectiveDate.AddMonths(license_qty * i);
                                        DB.S_AddonSchedulerActivation.Add(new S_AddonSchedulerActivation
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            StoreCode = sv.StoreCode,
                                            StoreName = sv.StoreName,
                                            CustomerCode = sv.CustomerCode,
                                            OrderCode = sv.OrderCode,
                                            ProductCode = sv.ProductCode,
                                            ProductName = sv.Productname,
                                            EffectiveDate = effectiveDate,
                                            Status = 0,
                                            StoreServiceId = sv.Id,
                                            CreateAt = DateTime.UtcNow,
                                            CreateBy = cMem?.FullName
                                        });
                                    }
                                }
                            }
                        }
                        //deactive subsciption
                        else if (stage == "deActive")
                        {
                            //remove addon scheduler
                            var AddonSchedulers = DB.S_AddonSchedulerActivation.Where(c => c.StoreServiceId == licenseId && c.Status == 0);
                            foreach (var item in AddonSchedulers)
                            {
                                item.Status = -1;
                                DB.Entry(item).State = EntityState.Modified;
                            }

                            //remove recurring planning
                            //var recurringPlanning = DB.RecurringPlannings.FirstOrDefault(c => c.OrderCode == storeOrder.OrderCode && c.SubscriptionCode == storeOrder.ProductCode);
                            //if (recurringPlanning != null)
                            //{
                            //    DB.RecurringPlannings.Remove(recurringPlanning);
                            //}
                        }
                    }
                }
                else
                {
                    throw new Exception("Select product not found");
                }

                cus.StoreStatus = activeStatus;
                var pass = cus.Password;
                // Todo : REMOVE PASS
                // cus.Password = null;

                if (cus.Active != 1 && activeStatus)
                {
                    cus.Active = 1;
                }

                DB.Entry(cus).State = EntityState.Modified;
                await DB.SaveChangesAsync();
                var new_ver = cus.Version;
                if (current_plan != null)
                    new_ver = IMSVersion.POS_VER2.Code<string>();
                else if (storeOrder != null)
                    new_ver = IMSVersion.POS_VER2.Code<string>();
                if (cus.Version != new_ver)
                {
                    cus.Version = new_ver;
                    DB.Entry(cus).State = EntityState.Modified;
                    await DB.SaveChangesAsync();
                }

                if (storeOrder.Type == LicenseType.LICENSE.Text() || storeOrder.Type == LicenseType.ADD_ON.Text())
                {
                    if (storeOrder.EffectiveDate <= DateTime.UtcNow.Date)
                    {
                        // Call api update store
                        await CloseOrder(storeOrder.OrderCode);
                    }
                    else
                    {
                        if (stage == "active")
                        {
                            DB.S_AddonSchedulerActivation.Add(new S_AddonSchedulerActivation
                            {
                                Id = Guid.NewGuid().ToString(),
                                StoreCode = storeOrder.StoreCode,
                                StoreName = storeOrder.StoreName,
                                CustomerCode = storeOrder.CustomerCode,
                                OrderCode = storeOrder.OrderCode,
                                ProductCode = storeOrder.ProductCode,
                                ProductName = storeOrder.Productname,
                                EffectiveDate = storeOrder.EffectiveDate,
                                Status = 0,
                                StoreServiceId = storeOrder.Id,
                                CreateAt = DateTime.UtcNow,
                                CreateBy = cMem?.FullName ?? "IMS System"
                            });
                        }
                    }
                }
                else if (storeOrder.Type == LicenseType.GiftCard.Text())
                {
                    RequestGiftcard(storeOrder.Id);
                }

                string action = (stage == "same-active" ? "Reactived" : (stage == "deActive" ? "Deactived" : "Actived")) + " subscription " + storeOrder.Productname;
                var newHistory = cMem?.FullName ?? "IMS system" + "$" + DateTime.UtcNow + "$" + action;
                var oldHistory = (cus.UpdateBy ?? string.Empty).Split('|').ToList();
                if (oldHistory.Count == 3)
                    oldHistory = oldHistory.Skip(1).ToList();
                oldHistory.Add(newHistory);
                cus.UpdateBy = string.Join("|", oldHistory);
                //db.Entry(Customer).State = EntityState.Modified;
                DB.SaveChanges();
                string log = $"{(stage == "same-active" ? "Reactived" : (stage == "deActive" ? "Deactived" : "Actived"))} subscription <b>{storeOrder.Productname}</b><br/>" +
                                $"Effective date: {storeOrder.EffectiveDate.Value.ToString("MMM dd, yyyy")}{(storeOrder.RenewDate.HasValue ? storeOrder.RenewDate.Value.ToString("- MMM dd, yyyy") : string.Empty)}";
                if (order?.Status == "PaymentLater")
                {
                    log += $"<br/><span class='label label-warning'>Payment later</span>";
                }
                WriteLogMerchant(cus, "Updated subscription", log);
            }
             catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            // Send mail active
            /*
            // TODO: Skip -> Use at MerchantManager Dashboard
            if (storeOrder.Type == LicenseType.SUBSCRIPTION.Text() && activeStatus && isStayActive == false)
            {
                // cus.Password = null;
                await SendMailStoreActive(cus);
            }
            */

            //Active in POS feature


            return licenseId;
        }

        public async Task<bool> RefundAction(Store_Services storeService)
        {
            try
            {
                decimal refund = 0;
                var _payment = new PaymentService();
                var _optionConfigurationService = new OptionConfigService();
                var config = _optionConfigurationService.LoadSetting<Config>();
                int fullRefundDays = config.Refund_All_Amount ?? 14;
                int partialRefundDays = config.Refund_Remaining_Amount ?? 0;
                int setupfeeRefundDays = config.Refund_Setupfee ?? 60;

                var Order = DB.O_Orders.FirstOrDefault(c => c.OrdersCode == storeService.OrderCode);
                if (Order == null || Order?.GrandTotal == 0) return true; //invoice = $0
                var oldTransRefund = DB.C_CustomerTransaction.FirstOrDefault(c => c.OrdersCode == storeService.OrderCode && c.PaymentMethod == "Refund" && c.PaymentNote == storeService.ProductCode);
                if (oldTransRefund != null) return true; //da co transaction refund

                var Cus = DB.C_Customer.FirstOrDefault(c => c.CustomerCode == storeService.CustomerCode) ?? new C_Customer { };
                var Trans = DB.C_CustomerTransaction.Where(c => c.OrdersCode == Order.OrdersCode && (c.PaymentStatus == "Approved" || c.PaymentStatus == "Success"))
                                                    .OrderBy(c => c.PaymentMethod == "CreditCard" || c.PaymentMethod == "Recurring").FirstOrDefault() ?? new C_CustomerTransaction { };
                var OrderSubcription = DB.Order_Subcription.FirstOrDefault(c => c.OrderCode == Order.OrdersCode && c.Product_Code == storeService.ProductCode && c.SubscriptionType != "setupfee" && c.SubscriptionType != "interactionfee");
                var setupSub = DB.Order_Subcription.FirstOrDefault(c => c.OrderCode == Order.OrdersCode && c.Product_Code == storeService.ProductCode && c.SubscriptionType == "setupfee");
                var interactionSub = DB.Order_Subcription.FirstOrDefault(c => c.OrderCode == Order.OrdersCode && c.Product_Code == storeService.ProductCode && c.SubscriptionType == "interactionfee");
                refund = OrderSubcription.Amount ?? ((OrderSubcription.Price ?? decimal.Zero) - (OrderSubcription.Discount ?? decimal.Zero));
                var numOfDaysPaid = (DateTime.UtcNow.Date - (Trans.CreateAt.HasValue ? Trans.CreateAt.Value.Date : OrderSubcription.StartDate.Value)).Days;
                var remainingDays = (OrderSubcription.EndDate.Value - DateTime.UtcNow.Date).Days;
                var totalDays = (OrderSubcription.EndDate.Value - OrderSubcription.StartDate.Value).Days;

                if (remainingDays <= 0) return true;
                if (numOfDaysPaid <= fullRefundDays)
                {
                    if (setupSub != null) refund += ((setupSub.Price ?? decimal.Zero) - (setupSub.Discount ?? decimal.Zero)); // + setup fee
                    if (interactionSub != null) refund += ((interactionSub.Price ?? decimal.Zero) - (interactionSub.Discount ?? decimal.Zero)); // + interaction fee
                } //ngay thanh toan <= config ngay full refund -> full refund amount
                else // ngay thanh toan > config ngay full refund
                {
                    if (numOfDaysPaid <= setupfeeRefundDays) // ngay thanh toan <= config ngay setupfee
                    {
                        if (setupSub != null) refund = ((setupSub.Price ?? decimal.Zero) - (setupSub.Discount ?? decimal.Zero)); // + setup fee
                        if (interactionSub != null) refund = ((interactionSub.Price ?? decimal.Zero) - (interactionSub.Discount ?? decimal.Zero)); // + interaction fee
                    }
                    else refund = 0;
                    if (remainingDays <= partialRefundDays)
                    {
                        if (setupSub != null)
                        {
                            var setupDays = (setupSub.EndDate.Value - setupSub.StartDate.Value).Days;
                            refund += refund * (decimal)(remainingDays / (totalDays - setupDays)); //so tien con lai sau khi tru so ngay setup
                        }
                        else
                        {
                            refund += refund * (decimal)(remainingDays / totalDays); //so tien con lai
                        }
                    }
                }

                var card = new C_CustomerCard { };
                //long MxMerchantID = 0;
                var p_card = DB.C_PartnerCard.Find(Trans.Card);
                if (p_card == null)
                {
                    card = DB.C_CustomerCard.Find(Trans.Card);
                    //MxMerchantID = Cus.MxMerchant_Id ?? 0;
                }
                else
                {
                    card = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(p_card));
                    //MxMerchantID = DB.C_Partner.FirstOrDefault(c => c.Code == p_card.PartnerCode)?.MxMerchant_Id ?? 0;
                }

                var trans = new C_CustomerTransaction()
                {
                    Id = Guid.NewGuid().ToString("N"),
                    OrdersCode = OrderSubcription.OrderCode,
                    CustomerCode = Cus.CustomerCode,
                    StoreCode = Cus.StoreCode,
                    Amount = -(long)(refund),
                    CreateAt = DateTime.UtcNow,
                    PaymentStatus = refund == 0 ? "Success" : "Waiting",
                    UpdateAt = DateTime.UtcNow,
                    PaymentMethod = "Refund",
                    PaymentNote = OrderSubcription.Product_Code,
                    CreateBy = cMem?.FullName ?? "IMS System",
                    Card = card?.Id
                };
                DB.C_CustomerTransaction.Add(trans);
                await DB.SaveChangesAsync();

                //if (string.IsNullOrEmpty(card?.MxMerchant_Token) || refund == 0) //khong co card
                //{

                //}
                //else //da thanh toan bang card
                //{
                //var transacion = new TransRequest()
                //{
                //    MxMerchant_Id = MxMerchantID,
                //    CustomerCode = Cus.CustomerCode,
                //    StoreCode = Cus.StoreCode,
                //    Key = Trans.OrdersCode.ToBase64(),
                //    PaymentMethod = "Refund",
                //    PaymentNote = storeService.ProductCode
                //};
                //var recur_payment = _payment.RefundTrans(transacion, card, refund);
                //}

            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return true;
        }

        public void RequestGiftcard(string StoreServiceId)
        {
            var SService = DB.Store_Services.Find(StoreServiceId);
            if (SService == null) return;
            var License = DB.License_Product.FirstOrDefault(l => l.Code == SService.ProductCode);
            if (License != null && License.GiftCardQuantity > 0)
            {
                var cus = DB.C_Customer.Where(x => x.StoreCode == SService.StoreCode).FirstOrDefault();
                string partnerLink = new MerchantService(DB).GetPartner(cus.CustomerCode).PosApiUrl;
                string url = AppConfig.Cfg.OrderGiftCardUrl(partnerLink) + "IDIMS=" + SService.StoreCode + "&Qty=" + License.GiftCardQuantity;
                string SalonNameLog = "";
                if (cus != null)
                {
                    SalonNameLog = cus.BusinessName + " (#" + cus.StoreCode + ")";
                }
                var result = ClientRestAPI.CallRestApi(url, "", "", "get", null, SalonName: SalonNameLog);

                if (result?.IsSuccessStatusCode == true)
                {
                    string responseJson = result.Content.ReadAsStringAsync().Result;
                    responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                    var responeData = JsonConvert.DeserializeObject<ResponeApiMangoPosFeature>(responseJson);
                    if (responeData.Result.Equals("200") && responeData.Message.Equals("Success"))
                    {
                        DB.C_CustomerGiftcard.Add(new C_CustomerGiftcard
                        {
                            Id = Guid.NewGuid().ToString(),
                            OrderCode = SService.OrderCode,
                            CustomerCode = SService.CustomerCode,
                            QRCode = string.Join(",", responeData.Data.Select(c => c.QRCode).ToList()),
                            Printed = string.Join(",", responeData.Data.Select(c => c.Printed).ToList()),
                            CreateAt = DateTime.UtcNow,
                            CreateBy = cMem?.FullName ?? "IMS System",
                            StoreServiceId = SService.Id
                        });
                        DB.SaveChanges();
                    }
                }
                else
                {
                    throw new Exception("POS connect failure, can not push data to API");
                }
            }
        }

        /// <summary>
        /// Send Mail active to customer
        /// </summary>
        /// <param name="customer">CustomerCode</param>
        /// <returns></returns>
        public async Task SendMailStoreActive(string customer)
        {
            var cus = DB.C_Customer.FirstOrDefault(_cus => _cus.CustomerCode == customer);
            try
            {
                var md5Pass = cus.MD5PassWord;
                if (cus != null)
                {
                    this.CheckCustomerPassword(cus);
                    DB.SaveChanges();
                }
                if (md5Pass != cus.MD5PassWord)
                {
                    //StoreProfile store = GetStoreProfile(cus?.StoreCode);
                    StoreProfileReq store = GetStoreProfileReady(null, false, null, cus?.StoreCode);
                    if (store != null) DoRequest(store);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            await SendMailStoreActive(cus);
        }

        /// <summary>
        /// Send Mail active to customer
        /// </summary>
        /// <param name="cus"></param>
        /// <returns></returns>
        public async Task SendMailStoreActive(C_Customer cus)
        {
            StoreProfile profile = StoreProfileWithDefault(cus.StoreCode);
            //List<Licenses> licenses = profile.Licenses.Where(ls => new[] { "SALONCENTER", "CHECKIN" }.Contains(ls.LicenseCode)).ToList();
            //var salonHis = new SendGridEmailTemplateData.History();
            //var checkinHis = new SendGridEmailTemplateData.History();
            //salonHis.orderHistory = new List<SendGridEmailTemplateData.OrderHistory>();
            //checkinHis.orderHistory = new List<SendGridEmailTemplateData.OrderHistory>();
            //foreach (Licenses license in licenses)
            //{
            //    if (license.PairCodes == null)
            //    {
            //        continue;
            //    }

            //    var readyToPair = license.PairCodes.Where(pair => pair.PairingStatus == "READY_TO_PAIR").ToList();
            //    if (readyToPair.Count > 0)
            //    {
            //        salonHis.orderHistory.Add(new SendGridEmailTemplateData.OrderHistory
            //        {
            //            salon_numerical_order = $"{salonHis.orderHistory.Count + 1}",
            //            salon_pairing_code = license.LicenseCode,
            //        });

            //        foreach (var pair in readyToPair)
            //        {
            //            checkinHis.orderHistory.Add(new SendGridEmailTemplateData.OrderHistory
            //            {
            //                checkin_numerical_order = $"{checkinHis.orderHistory.Count + 1}",
            //                checkin_pairing_code = pair.PairingCode,
            //            });
            //        }
            //    }
            //}

            //if (salonHis.orderHistory.Count == 0) salonHis = null;
            //if (checkinHis.orderHistory.Count == 0) checkinHis = null;
            int at = LicenseStatus.ACTIVE.Code<int>();
            var addon = LicenseType.ADD_ON.Text();
            var mangoslice = LicenseDefinded.MANGO_SLICE.Text();
            var storelicense = DB.Store_Services.Where(s => s.Active == at && s.StoreCode.Equals(profile.StoreId) && s.Type != addon).FirstOrDefault();
            if (storelicense == null)
            {
                throw new Exception("This license is not activation. You cannot email this.");
            }
            var checkMangoSlice = (from p in DB.License_Product
                                   join pi in DB.License_Product_Item on p.Id equals pi.License_Product_Id into licenseitem
                                   from i in licenseitem.DefaultIfEmpty()
                                   where p.Code.Equals(storelicense.ProductCode) && i.License_Item_Code.Equals(mangoslice) && i.Enable == true
                                   select p).Any();

            var emailData = new Enrich.DataTransfer.SendGridEmailTemplateData.InstallCompleteTemplate()
            {
                salon_name = profile.StoreName ?? cus.BusinessName,
                login_user = profile.Email ?? cus.BusinessEmail ?? cus.SalonEmail ?? cus.Email,
                login_password = cus.Password ?? "<Your password>",
                pos_link = checkMangoSlice ? AppConfig.Cfg.StoreUrl.V2.SaloncenterSliceLink : AppConfig.Cfg.SaloncenterLink(GetPartner(cus.CustomerCode).ManageUrl),
                checkin_link = AppConfig.Cfg.CheckinLink(GetPartner(cus.CustomerCode).CheckinUrl),
            };

            string to = string.Join(";", cus.SalonEmail);
            string firstname = string.Join(";", cus.BusinessName);
            string cc = string.Join(";", cus.MangoEmail, profile.Email);
            await EngineContext.Current.Resolve<IMailingService>().SendNotifyInstallationComplete(to, firstname, cc, emailData);
            //await SendEmail.SendNotifyInstallationComplete(profile.Email, profile.ContactName, "", emailData);
        }

        public void DoRequest(StoreProfileReq data)
        {
            var postData = JsonConvert.DeserializeObject<Dictionary<string, Object>>
            (JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }));

            Trace.WriteLine(JsonConvert.SerializeObject(postData));
            var c = DB.C_Customer.FirstOrDefault(x => x.StoreCode == data.storeId);
            string partnerLink = GetPartner(c.CustomerCode).PosApiUrl;
            //string url = AppConfig.Cfg.StoreChangeUrl(partnerLink);
            string url = DomainConfig.NextGenApi+ "/v1/License/StoreChange";

            string SalonNameLog = "";
            if (c != null)
            {
                SalonNameLog = c.BusinessName + " (#" + c.StoreCode + ")";
            }
            LogModel.WriteLog(SalonNameLog + ":" + JsonConvert.SerializeObject(postData));
            var result = ClientRestAPI.CallRestApi(url, "", "", "get", postData, SalonName: SalonNameLog);
            LogModel.WriteLog(SalonNameLog+":" + JsonConvert.SerializeObject(result));
            var posResponse = result?.Content.ReadAsAsync<ApiPOSResponse>().Result;
            if (result?.IsSuccessStatusCode != true || posResponse?.IsSuccess() != true)
            {
                if (posResponse == null)
                    throw new Exception("POS connect failure, can not push data to API");
                throw new Exception(posResponse.Message);
            }
        }


        public async Task<rsData> CloseOrder(string OrderCode)
        {
            httpResponse result = await ClientRestAPI.NextGen.GetAsync("/v1/License/CloseOrder", "?OrderCode="+OrderCode);
            rsData resultData = new rsData();
            resultData.Status = result.StatusCode;
            resultData.Message = result.Message;
            return resultData;
        }
        public async Task<CustomerSearchResponse> GetMerchantFromUniversal(CustomerSearchRequest request)
        {
            try
            {
                var postData = JsonConvert.DeserializeObject<Dictionary<string, Object>>
                (JsonConvert.SerializeObject(request, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }));
                string strrequest = JsonConvert.SerializeObject(postData);
                Trace.WriteLine(strrequest);
                //string url = AppConfig.Cfg.StoreChangeUrl(partnerLink);
                string url = DomainConfig.NextGenApi + "/v1/customer/searchv2";
                url = "https://api-ims.swiftpos.us" + "/v1/customer/searchv2";
                //url = "http://10.142.0.36:8018/v1/customer/searchv2";


                var result = ClientRestAPI.CallRestApi(url, "", "", "post", postData);
                LogModel.WriteLog("GetMerchantFromUniversal:" + JsonConvert.SerializeObject(result));
                var posResponse = result?.Content.ReadAsAsync<CustomerSearchResponse>().Result;
                //var reponse = JsonConvert.DeserializeObject<CustomerSearchResponse>(posResponse);
                return posResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Update data sync with server
        /// </summary>
        /// <param name="store"></param>
        /// <returns></returns>
        private StoreProfile RepairStore(StoreProfile store, string stage = null, string deactive_store = null)
        {
            try
            {
                {
                    C_Customer cus = DB.C_Customer.FirstOrDefault(c => c.StoreCode == store.StoreId);
                    var _store = DB.Store_Services.Where(s => s.StoreCode == cus.StoreCode).OrderByDescending(s => s.LastUpdateAt).FirstOrDefault();
                    store.StoreName = cus.BusinessName;
                    store.BusinessName = cus.BusinessName;
                    store.BusinessPhone = cus.BusinessPhone;
                    store.BusinessEmail = string.IsNullOrWhiteSpace(cus.SalonEmail) ? cus.BusinessEmail : cus.SalonEmail;
                    store.BusinessStartDate = cus.BusinessStartDate?.ToString(StoreServices.F_DATE);
                    store.CreateBy = cus.CreateBy;
                    store.CreateAt = cus.CreateAt?.ToString(StoreServices.F_DATE);
                    store.UpdateBy = _store.LastUpdateBy;
                    store.LastUpdate = _store.LastUpdateAt.HasValue ? _store.LastUpdateAt.Value.ToString(StoreServices.F_DATE) : "";
                    store.BusinessAddress = cus.AddressLine();
                    store.activeProducts = MakeActiveProducts(cus.StoreCode);
                    store.NewLicense = "0";
                    AddTimeZone(store, cus);
                    var licenseList = new StoreServices(DB).LicensesCurrent(cus.StoreCode, deactive_store);
                    licenseList.ForEach(license =>
                    {
                        var posLicenseItem = store.Licenses.FirstOrDefault(_license => _license.LicenseCode == license.LicenseCode);
                        license.Start_period = AppFunc.ParseTime(license.Start_period, StoreServices.F_DATE);
                        if (!string.IsNullOrEmpty(stage) && stage.ToLower() == "deactive")
                        {
                            license.End_period = DateTime.UtcNow.ToString("MM/dd/yyyy");

                        }
                        else
                        {
                            license.End_period = AppFunc.ParseTime(license.End_period, StoreServices.F_DATE);
                        }

                        license.Subscription_warning_date = AppFunc.ParseTime(license.Subscription_warning_date, StoreServices.F_DATE);
                        license.Count_current = (posLicenseItem?.Count_current ?? "0").Split('.')[0];
                        license.PairCodes = posLicenseItem?.PairCodes ?? new List<Pairing>();
                    });
                    store.Licenses = licenseList;
                    this.CheckCustomerPassword(cus);
                    DB.SaveChanges();
                    store.Password = cus.MD5PassWord;
                    store.RequirePassChange = cus.RequirePassChange;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
            }

            return store;
        }

        /// <summary>
        /// Add timezone item to ver1.2
        /// </summary>
        /// <param name="store"></param>
        /// <param name="cus"></param>
        private void AddTimeZone(StoreProfile store, C_Customer cus)
        {
            if (cus.Version == IMSVersion.POS_VER2.Code<string>())
            {
                try
                {
                    ((StoreProfileV2)store).timeZone = !string.IsNullOrEmpty(cus.SalonTimeZone_Number) ? cus.SalonTimeZone_Number : TIMEZONE_NUMBER_BY_ID.Eastern.Text();
                    ((StoreProfileV2)store).timeZoneName = !string.IsNullOrEmpty(cus.SalonTimeZone_Number) ? cus.SalonTimeZone : TIMEZONE_NUMBER_BY_ID.Eastern.Code<string>();
                }
                catch
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Create StoreProfile object for Active/DeActive
        /// </summary>
        /// <param name="licenseId"></param>
        /// <param name="activeStage">
        ///     true :  Active
        ///     false : DeActive
        /// </param>
        /// <param name="stage">active : New plan/ change plan </param>
        /// <returns></returns>
        public StoreProfile Activation(string licenseId, bool activeStage, string stage = null)
        {
            {
                Store_Services targetPlan = DB.Store_Services.Find(licenseId);
                if (stage == "active" && targetPlan.Type == LicenseType.LICENSE.Text()) //active
                {
                    return ProfileForNewPlan(licenseId);
                }

                StoreProfile currentPlan;
                if (activeStage) //same-active license
                {
                    currentPlan = BuildStoreProfile(targetPlan.Id);
                    StoreProfile currentPlanServer = GetStoreProfile(targetPlan.StoreCode);
                    if (currentPlanServer == null)
                    {
                        currentPlanServer = currentPlan;
                    }

                    currentPlanServer.Licenses.ForEach(li =>
                    {
                        Licenses fromUpdate = currentPlan.Licenses.FirstOrDefault(ls => ls.LicenseCode == li.LicenseCode);
                        if (targetPlan.Active == -1 && targetPlan.Type == LicenseType.ADD_ON.Text())
                        {
                            if (li?.Count_limit == "-1" || fromUpdate?.Count_limit == "-1")
                            {
                                li.Count_limit = "-1";
                            }
                            else
                            {
                                li.Count_limit = (int.Parse(li?.Count_limit ?? "0") + int.Parse(fromUpdate?.Count_limit ?? "0")).ToString();
                            }
                        }
                    });
                    currentPlan.Licenses = currentPlanServer.Licenses;
                    //if (string.IsNullOrEmpty(currentPlan.Email))
                    //{
                    //    currentPlan.Email = currentPlanServer.Email;
                    //}
                }
                else // deactive
                {
                    var deactive_store = (stage == "deActive") ? licenseId : null;
                    currentPlan = StoreProfileWithDefault(targetPlan.StoreCode, deactive_store);
                    if (currentPlan == null)
                    {
                        var msg = "Mango POS system not responding! Please wait a few minutes before trying again.";
                        throw new Exception(msg);
                    }
                    currentPlan.activeProducts = MakeActiveProducts(targetPlan.StoreCode);
                }

                currentPlan.Status = activeStage ? "Activated" : "DeActivated";
                return currentPlan;
            }
        }

        public StoreProfile BuildStoreProfile(string licenseId)
        {
            {
                Store_Services license = DB.Store_Services.Find(licenseId);
                if (license == null) return null;
                var cus = DB.C_Customer.FirstOrDefault(c => c.CustomerCode == license.CustomerCode);
                if (cus == null) return null;
                this.CheckCustomerPassword(cus);
                DB.SaveChanges();
                var version = string.IsNullOrEmpty(DB.License_Product.Where(l => l.Code == license.ProductCode).FirstOrDefault().Code_POSSystem) ? "v2" : "v1";
                var storeData = (version == "v2") ? new StoreProfileV2() : new StoreProfile();
                storeData.StoreId = cus.StoreCode;
                storeData.StoreName = cus.BusinessName;
                storeData.ContactName = cus.ContactName ?? cus.OwnerName;
                storeData.Email = !string.IsNullOrWhiteSpace(cus.MangoEmail) ? cus.MangoEmail : (!string.IsNullOrWhiteSpace(cus.SalonEmail) ? cus.SalonEmail : cus.Email);
                storeData.CellPhone = cus.SalonPhone ?? cus.CellPhone;
                storeData.BusinessName = cus.BusinessName;
                storeData.BusinessPhone = cus.BusinessPhone;
                storeData.BusinessEmail = !string.IsNullOrWhiteSpace(cus.BusinessEmail) ? cus.BusinessEmail : cus.SalonEmail;
                storeData.BusinessStartDate = cus.BusinessStartDate?.ToString(StoreServices.F_DATE);
                storeData.BusinessAddress = cus.AddressLine();
                storeData.RequirePassChange = cus.RequirePassChange;
                storeData.Password = cus.MD5PassWord;
                storeData.Status = DB.Store_Services.Any(s => s.StoreCode == license.StoreCode && s.Type == "license" && s.Active == 1) ? "Activated" : "DeActivated";
                storeData.CreateBy = cus.CreateBy;
                storeData.CreateAt = cus.CreateAt?.ToString(StoreServices.F_DATE);
                storeData.UpdateBy = license.LastUpdateBy;
                storeData.LastUpdate = license.LastUpdateAt.HasValue ? license.LastUpdateAt.Value.ToString(StoreServices.F_DATE) : "";
                AddTimeZone(storeData, cus);
                storeData.activeProducts = MakeActiveProducts(license.StoreCode);
                storeData.Licenses = new StoreServices(DB).LicensesPlan(license.StoreCode, license.Id);
                return storeData;
            }
        }

        public List<ActiveProducts> MakeActiveProducts(string storeCode)
        {
            List<ActiveProducts> activeProducts = new List<ActiveProducts>();
            MainActiveSubscription(storeCode).Where(st => st != null).ForEach(st =>
                {
                    activeProducts.Add(new ActiveProducts
                    {
                        Code = st.ProductCode,
                        Name = st.Productname,
                        Type = st.Type
                        //    EffectiveDate = st.EffectiveDate != null ? st.EffectiveDate.Value.ToString(StoreServices.F_DATE) : DateTime.Today.ToString(StoreServices.F_DATE)
                    });
                });
            return activeProducts;
        }

        public Store_Services GetMainSubscription(string storeCode, bool activeOnly = false)
        {
            {
                var storeLicenses = DB.Store_Services.Where(st => st.Type == "license" && st.StoreCode == storeCode).ToList();
                Store_Services main = storeLicenses.FirstOrDefault(st => st.Active == 1);
                if (activeOnly) return main;
                if (main == null)
                    main = storeLicenses.Where(st => st.Active == 0).OrderByDescending(st => st.LastUpdateAt).FirstOrDefault();
                return main ?? (storeLicenses.Where(st => st.Active == -1).OrderByDescending(st => st.LastUpdateAt).FirstOrDefault());
            }
        }

        public List<Store_Services> MainActiveSubscription(string storeCode)
        {
            {
                var storeLicenses = DB.Store_Services.Where(st => st.Type == "license" && st.StoreCode == storeCode).ToList();
                List<Store_Services> active = storeLicenses.Where(st => st.Active == 1).ToList();
                if (active.Count == 0)
                {
                    active = new List<Store_Services> { storeLicenses.OrderByDescending(st => st.LastUpdateAt).FirstOrDefault() };
                }
                return active;
            }
        }

        public string GetLicenseStatus(Store_Services license)
        {
            switch (license.Active)
            {
                case -1: return "waiting";
                case -2:
                case 0: return "deactive";
                case 1:
                    if (license.RenewDate.HasValue && license.RenewDate < DateTime.Today) return "expired";
                    break;
            }

            return "active";
        }

        public string LicenseOrderStatus(string orderCode)
        {
            using (WebDataModel _db = new WebDataModel())
            {
                switch (_db.O_Orders.FirstOrDefault(o => o.OrdersCode == orderCode)?.Status ?? "")
                {
                    case "Submitted": return "New";
                    case "Completed":
                    case "Payment cleared": return "Collected";
                    default: return "";
                }
            }
        }

        public StoreProfile ProfileForNewPlan(string licenseId)
        {
            {
                // Get profile
                Store_Services targetLicense = DB.Store_Services.Find(licenseId);
                if (targetLicense == null) throw new Exception("License not found!");
                StoreProfile newProfile = BuildStoreProfile(targetLicense.Id);
                StoreProfile currentProfile = GetStoreProfile(targetLicense.StoreCode);
                StoreProfile sendProfile = RepairStore(/*currentProfile ?? */newProfile);
                List<Licenses> activeLicensesLimit = new StoreServices(DB).LicensesPlan(targetLicense.StoreCode, targetLicense.Id);
                sendProfile.Licenses.ForEach(li =>
                {
                    Licenses active = activeLicensesLimit.FirstOrDefault(ls => ls.LicenseCode == li.LicenseCode);
                    li.Count_limit = active?.Count_limit ?? "0";
                    li.Count_current = currentProfile?.Licenses.FirstOrDefault(_license => _license.LicenseCode == li.LicenseCode)?.Count_current ?? "0";
                    li.Count_current = li.Count_current.Split('.')[0];
                });
                //sendProfile.Licenses = newProfile.Licenses;

                // Update status
                string statusPlan = "Activated";
                string statusLicense = "active";
                if (targetLicense.RenewDate.HasValue && targetLicense.RenewDate < DateTime.Today)
                {
                    statusPlan = "Expired";
                    statusLicense = "expired";
                }

                sendProfile.Status = statusPlan;
                sendProfile.Licenses.ForEach(license => { license.Status = statusLicense; });
                // ActiveProducts
                sendProfile.activeProducts = sendProfile.activeProducts ?? new List<ActiveProducts>();

                //List<Store_Services> listLicense = DB.Store_Services.ToList();
                //listLicense.Where(st => st.StoreCode == targetLicense.StoreCode && st.Active == 1 && st.Type == LicenseType.ADD_ON.Text()).ToList()
                //    .ForEach(st =>
                //    {
                //        sendProfile.activeProducts.Add(new ActiveProducts
                //        {
                //            Code = st.ProductCode,
                //            Name = st.Productname,
                //        });
                //    });
                return sendProfile;
            }
        }

        public List<TimeZoneModel> ListTimeZone()
        {
            return new List<TimeZoneModel>();
            //try
            //{
            //    string url = AppConfig.Cfg.MangoPOS.TimeZoneUrl;
            //    HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "get", null);
            //    if (result?.IsSuccessStatusCode == true)
            //    {
            //        string responseJson = result.Content.ReadAsStringAsync().Result;
            //        //responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
            //        ResApiListTimeZone responeData = JsonConvert.DeserializeObject<ResApiListTimeZone>(responseJson);
            //        if (responeData.status.Equals(200))
            //        {
            //            return responeData.lst.Select(c => new TimeZoneModel
            //            {
            //                Name = c.Name,
            //                TimeDT = c.TimeDT.First() != '-' && c.TimeDT.First() != '+' ? "+" + c.TimeDT : c.TimeDT,
            //                IsUsingST = c.IsUsingST
            //            }).OrderByDescending(c => c.TimeDT).ToList();
            //        }
            //        else
            //        {
            //            throw new Exception();
            //        }
            //    }
            //    else
            //    {
            //        throw new Exception();
            //    }
            //}
            //catch (Exception)
            //{
            //    return new List<TimeZoneModel>();
            //}
        }

        //--Build store profile for StoreChange new version pos
        public StoreProfileReq GetStoreProfileReady(string licenseId, bool activeStage, string stage = null, string storeCode = "")
        {
            var License = new Store_Services();
            var Customer = new C_Customer();
            if (!string.IsNullOrEmpty(licenseId))
            {
                License = DB.Store_Services.Find(licenseId);
                if (License == null) return null;
                Customer = DB.C_Customer.Where(c => c.CustomerCode == License.CustomerCode).FirstOrDefault();
                if (Customer == null) return null;
                this.CheckCustomerPassword(Customer);
                DB.SaveChanges();
            }
            else if (!string.IsNullOrEmpty(storeCode))
            {
                //License = DB.Store_Services.Where(c=>c.StoreCode == storeCode).OrderByDescending(c=>c.LastUpdateAt).FirstOrDefault();
                Customer = DB.C_Customer.Where(c => c.StoreCode == storeCode).FirstOrDefault();
                if (Customer == null) return null;
                this.CheckCustomerPassword(Customer);
                DB.SaveChanges();
            }
            else throw new Exception("Data merchant not found");
            var Order = DB.O_Orders.Where(c => c.OrdersCode == License.OrderCode).FirstOrDefault() ?? new O_Orders() { };
            var MainLicense = DB.Store_Services.Where(c => c.CustomerCode == Customer.CustomerCode && c.Type == "license" && c.Active == 1).FirstOrDefault() ?? new Store_Services() { };
            var data = new StoreProfileReq()
            {
                storeId = Customer.StoreCode,
                storeName = Customer.BusinessName,
                StoreAddress = Customer.SalonAddress1,
                City = Customer.SalonCity,
                State = Customer.SalonState,
                ZipCode = string.Format("{0} {1}", Customer.SalonZipcode, "United States"),
                StoreEmail = Customer.MangoEmail ?? Customer.SalonEmail ?? Customer.Email,
                StorePhone = Customer.BusinessPhone,
                ContactName = Customer.OwnerName ?? Customer.ContactName,
                lastUpdate = DateTime.UtcNow.ToString(StoreServices.F_DATE),
                UpdateBy = License?.LastUpdateBy ?? cMem?.FullName,
                Password = Customer.MD5PassWord ?? "",
                RequirePassChange = Customer.RequirePassChange ?? "off",
                Status = DB.Store_Services.Any(s => (s.StoreCode == License.StoreCode || s.StoreCode == Customer.StoreCode) && s.Type == "license" && s.Active == 1) ? "Activated" : "DeActivated",
                SubscriptionCode = License?.ProductCode,
                SubscriptionName = License?.Productname,
                TimeZone = Customer.SalonTimeZone_Number ?? TIMEZONE_NUMBER_BY_ID.Eastern.Text(),
                TimeZoneName = Customer.SalonTimeZone ?? TIMEZONE_NUMBER_BY_ID.Eastern.Code<string>(),
                CompanyPartner = Customer.PartnerCode ?? Order.PartnerCode ?? string.Empty,
            };
            DB.SaveChanges();
            return data;
        }

        public List<LicensesReq> GetLicenseForStoreChange(Store_Services License, C_Customer Customer, bool activeStage, string stage = null)
        {
            var licenses = new List<LicensesReq>();
            string StoreCode = string.Empty;
            if (!string.IsNullOrEmpty(License?.Id))
            {
                StoreCode = License.StoreCode;
                if (stage == "active") //active
                {
                    if (License.Type == LicenseType.ADD_ON.Text()) //active addon
                    {
                        licenses = new StoreServices(DB).LicensesAddOn(License.StoreCode, License.Id).Select(l => new LicensesReq
                        {
                            Rollover = l.Count_limit == "0" ? -1 : (l.Count_limit == "-1" ? 0 : 1),
                            LicenseCode = l.LicenseCode,
                            LicenseType = l.LicenseType,
                            Count_limit = l.Count_limit,
                            Start_period = l.Start_period,
                            End_period = l.End_period
                        }).ToList();
                    }
                    else // active license
                    {
                        licenses = new StoreServices(DB).LicensesPlan(License.StoreCode, License.Id).Select(l => new LicensesReq
                        {
                            Rollover = 0,
                            LicenseCode = l.LicenseCode,
                            LicenseType = l.LicenseType,
                            Count_limit = l.Count_limit,
                            Start_period = l.Start_period,
                            End_period = l.End_period
                        }).ToList();
                        var temp = new StoreServices(DB).LicensesAddOn(License.StoreCode, License.Id).Where(c => c.LicenseCode == "SMS")
                            .Select(l => new LicensesReq { Count_limit = l.Count_limit }).FirstOrDefault();
                        if (temp != null)
                        {
                            licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Rollover = 1;
                            licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit = temp.Count_limit;
                        }
                    }

                    var dataStore = new StoreServices(DB).GetStore(License.StoreCode);
                    if (dataStore != null && dataStore.Licenses != null)
                    {
                            var packageSMS = dataStore.Licenses.FirstOrDefault(c => c.LicenseCode == "SMS")?.Count_limit;
                            if (packageSMS == "-1")
                            {
                                licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Rollover = 0;
                                licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit = "-1";
                            }
                    }
                }
                else if (activeStage) //same-active license
                {
                    licenses = new StoreServices(DB).LicensesCurrent(License.StoreCode, License.Id).Select(l => new LicensesReq
                    {
                        Rollover = 0,
                        LicenseCode = l.LicenseCode,
                        LicenseType = l.LicenseType,
                        Count_limit = l.Count_limit,
                        Start_period = l.Start_period,
                        End_period = l.End_period
                    }).ToList();
                }
                else // deactive
                {
                    var licenseAddon = new StoreServices(DB).LicensesAddOn(License.StoreCode, License.Id).Select(l => new LicensesReq
                    {
                        Rollover = l.Count_limit == "0" ? -1 : 0,
                        LicenseCode = l.LicenseCode,
                        LicenseType = l.LicenseType,
                        Count_limit = l.Count_limit,
                        Start_period = l.Start_period,
                        End_period = l.End_period
                    }).ToList();
                    if (License.Type == LicenseType.ADD_ON.Text()) // deactive addon
                    {
                        licenses = new StoreServices(DB).LicensesCurrent(License.StoreCode).Select(l => new LicensesReq
                        {
                            Rollover = licenseAddon.FirstOrDefault(c => c.LicenseCode == l.LicenseCode).Rollover,
                            LicenseCode = l.LicenseCode,
                            LicenseType = l.LicenseType,
                            Count_limit = l.Count_limit,
                            Start_period = l.Start_period,
                            End_period = l.End_period
                        }).ToList();
                    }
                    else // deactive license
                    {
                        var dateNow = DateTime.UtcNow.ToString("MM/dd/yyyy");
                        licenses = new StoreServices(DB).LicensesCurrent(License.StoreCode).Select(l => new LicensesReq
                        {
                            Rollover = licenseAddon.FirstOrDefault(c => c.LicenseCode == l.LicenseCode).Rollover,
                            LicenseCode = l.LicenseCode,
                            LicenseType = l.LicenseType,
                            Count_limit = l.Count_limit,
                            Start_period = dateNow,
                            End_period = dateNow
                        }).ToList();
                    }
                    var dataStore = new StoreServices(DB).GetStore(License.StoreCode);


                    if (dataStore != null && dataStore.Licenses != null)
                    {
                        var currentSMS = dataStore.Licenses.FirstOrDefault(c => c.LicenseCode == "SMS")?.Count_current ?? "";
                        var countSMS = licenseAddon.FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit;
                        int.TryParse(currentSMS.Replace(".0", ""), out int currentNumberSMS);
                        int.TryParse(countSMS, out int countNumberSMS);
                        var remaining = currentNumberSMS - countNumberSMS;
                        licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Rollover = 0;
                        licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit = (remaining < 0 ? 0 : remaining).ToString();
                    }
                }
            }
            else
            {
                StoreCode = Customer?.StoreCode;
                licenses = new StoreServices(DB).LicensesCurrent(Customer?.StoreCode).Select(l => new LicensesReq
                {
                    Rollover = -1,
                    LicenseCode = l.LicenseCode,
                    LicenseType = l.LicenseType,
                    Count_limit = l.Count_limit,
                    Start_period = l.Start_period,
                    End_period = l.End_period
                }).ToList();
            }
            if (!string.IsNullOrEmpty(StoreCode))
            {
                var mainLicense = DB.Store_Services.Where(ss => ss.StoreCode == StoreCode && ss.Active == 1 && ss.Type == "license").ToList();
                var enddate = mainLicense.Count() > 0 ?
                    (mainLicense.First()?.RenewDate.Value.ToString("MM/dd/yyyy") ?? DateTime.UtcNow.ToString("MM/dd/yyyy")) :
                    DateTime.UtcNow.ToString("MM/dd/yyyy");
                licenses.ForEach(l =>
                {
                    if (string.IsNullOrEmpty(l.End_period))
                        l.End_period = enddate;
                });
            }

            return licenses;
        }
        //--Build store profile for StoreChange new version pos

        public C_Partner GetPartner(string CustomerCode)
        {
            try
            {
                var firstOrder = DB.O_Orders.FirstOrDefault(c => !string.IsNullOrEmpty(c.PartnerCode) && c.CustomerCode == CustomerCode) ?? new O_Orders() { };
                return DB.C_Partner.FirstOrDefault(c => c.Code == firstOrder.PartnerCode && !string.IsNullOrEmpty(c.Code)) ?? new C_Partner() { };
            }
            catch (Exception)
            {
                return new C_Partner() { };
            }
        }

        public void WriteLogMerchant(C_Customer customer, string title = "", string description = "")
        {
            try
            {
                SalesLeadService _salesLeadService = new SalesLeadService();
                var sl = DB.C_SalesLead.FirstOrDefault(c => c.CustomerCode == customer.CustomerCode);
                if (sl == null) throw new Exception("Sale lead not found.");
                if (!string.IsNullOrEmpty(title))
                {
                    _salesLeadService.CreateLog(sl.Id, sl.CustomerName, title, description, cMem?.FullName);
                }
            }
            catch (Exception)
            {
                //ignore
            }
        }

        public void CheckCustomerPassword(C_Customer cus)
        {
            if (string.IsNullOrEmpty(cus.Password))
            {
                cus.Password = SecurityLibrary.Md5Encrypt(DateTime.UtcNow.ToString("O")).Substring(0, 6);
                cus.MD5PassWord = SecurityLibrary.Md5Encrypt(cus.Password);

            }
            else if (string.IsNullOrEmpty(cus.MD5PassWord))
            {
                cus.MD5PassWord = SecurityLibrary.Md5Encrypt(cus.Password);

            }
            if (string.IsNullOrEmpty(cus.RequirePassChange))
            {
                cus.RequirePassChange = "off";

            }

        }


        public BaseServicePackage GetBaseServicePackage(string storeId)
        {
            var storeService = DB.Store_Services.Find(storeId);
            if (storeService == null) return new BaseServicePackage();

            var licenseProduct = DB.License_Product.AsNoTracking().SingleOrDefault(x => x.Code == storeService.ProductCode);
            var licenseProductItemBaseService = (from licenseProductItem in DB.License_Product_Item join licenseItem in DB.License_Item on licenseProductItem.License_Item_Code equals licenseItem.Code where licenseItem.GroupName == "Base Services" && licenseProductItem.License_Product_Id == licenseProduct.Id select licenseProductItem).ToList();
           // var baseServices = new StoreServices(DB).LicensesPlan(storeService.StoreCode, storeService.Id);
            return new BaseServicePackage
            {
                StoreCode = storeService.StoreCode,
                SubscriptionCode = storeService.ProductCode,
                baseServices = licenseProductItemBaseService.Select(c => new BaseServiceQuantity { 
                    Code = c.License_Item_Code, 
                    Quantity =  (c.Value == -1 ? Constants.UnlimitedValue : c.Value * storeService.Quantity) ??0
                }).ToList(),
            };
        }

        /// <summary>
        /// Symc store base service
        /// </summary>
        /// <param name="store"></param>
        /// <param name="operation">Deactive = -1 | Acctive = 1</param>
        public void SyncStoreBaseService(string storeId, int operation = 1)
        {
            int remaining = 0;
            var package = GetBaseServicePackage(storeId);
            var lst = DB.StoreBaseServices.Where(c => c.StoreCode == package.StoreCode);
            package.baseServices.ForEach(baseService =>
            {
                try
                {
                    var baseServiceDB = lst.FirstOrDefault(c => c.KeyName == baseService.Code);
                    if (baseServiceDB == null)
                    {
                        var quantity = operation < 0 ? 0 : operation * baseService.Quantity;
                        baseServiceDB = new StoreBaseService
                        {
                            StoreCode = package.StoreCode,
                            KeyName = baseService.Code,
                            RemainingValue = quantity,
                            MaximumValue = quantity,
                            CreateAt = DateTime.UtcNow,
                            CreateBy = cMem?.MemberNumber ?? "System"
                        };
                        DB.StoreBaseServices.Add(baseServiceDB);
                        remaining = baseServiceDB.RemainingValue ?? 0;
                    }
                    else
                    {
                        if (baseServiceDB.MaximumValue == null) baseServiceDB.MaximumValue = 0;
                        if (baseServiceDB.RemainingValue == null) baseServiceDB.RemainingValue = 0;

                        baseServiceDB.MaximumValue += operation * baseService.Quantity;
                        baseServiceDB.RemainingValue += operation * baseService.Quantity;
                        baseServiceDB.MaximumValue = baseServiceDB.MaximumValue < 0 ? 0 : baseServiceDB.MaximumValue;
                        baseServiceDB.RemainingValue = baseServiceDB.RemainingValue < 0 ? 0 : baseServiceDB.RemainingValue;
                        baseServiceDB.UpdateAt = DateTime.UtcNow;
                        baseServiceDB.UpdateBy = cMem?.MemberNumber ?? "System";
                        remaining = baseServiceDB.RemainingValue ?? 0;
                        DB.Entry(baseServiceDB).State = EntityState.Modified;
                    }
                   
                }
                catch (Exception ex)
                {
                    //sync remaining error then throw
                    throw ex;
                }
                //try
                //{
                //    if (baseService.Code == "SMS")
                //    {
                //        var history = new SMSHistoryRemaining
                //        {
                //            StoreCode = package.StoreCode,
                //            ObjectId = 0,
                //            RemainingSMS = remaining,
                //            Caller = "IMS System",
                //            Type = operation == 1 ? 200 : 300,
                //            CreatedDate = DateTime.UtcNow
                //        };
                //        EngineContext.Current.Resolve<IEnrichSMSService>().InsertHistorySMSRemainingAsync(history);
                //    }
                //}
                //catch (Exception)
                //{
                //    //insert history error
                //    //ignore
                //}
                remaining = 0;
            });
            DB.SaveChanges();
        }
        public async Task ApproveActionImmediate(string storeCode, string licenseId, bool activeStatus, string stage = null)
        {
            //deactive scheduler
            var scheduler = DB.S_AddonSchedulerActivation.FirstOrDefault(c => c.StoreCode == storeCode && c.StoreServiceId == licenseId);
            if (scheduler != null)
            {
                scheduler.Status = 1;
                DB.Entry(scheduler).State = EntityState.Modified;
            }

            //update start date, end date
            Store_Services storeService = DB.Store_Services.Find(licenseId);
            if (storeService != null)
            {
                Order_Subcription orderSubscription = DB.Order_Subcription.FirstOrDefault(c =>
                    c.Product_Code == storeService.ProductCode && c.StoreCode == storeService.StoreCode);
                if (orderSubscription != null)
                {
                    var product = DB.License_Product.FirstOrDefault(c => c.Code == storeService.ProductCode) ?? new License_Product();
                    DateTime nextDate = DateTime.UtcNow;
                    storeService.EffectiveDate = nextDate;
                    orderSubscription.StartDate = nextDate;
                    var quantity = storeService.Quantity ?? 1;
                    if (orderSubscription.PeriodRecurring == RecurringInterval.Yearly.ToString()) nextDate = nextDate.AddYears(product.NumberOfPeriod.Value * quantity);
                    else if (orderSubscription.PeriodRecurring == RecurringInterval.Weekly.ToString()) nextDate = nextDate.AddDays(product.NumberOfPeriod.Value * 7 * quantity);
                    else nextDate = nextDate.AddMonths(product.NumberOfPeriod.Value * quantity);

                    orderSubscription.EndDate = nextDate;
                    DB.Entry(orderSubscription).State = EntityState.Modified;
                    storeService.RenewDate = nextDate;
                    DB.Entry(storeService).State = EntityState.Modified;

                    //if (storeService.AutoRenew == true)
                    //{
                    //    var recurringPlan = DB.RecurringPlannings.FirstOrDefault(c =>
                    //        c.CustomerCode == storeService.CustomerCode && c.OrderCode == storeService.OrderCode &&
                    //        c.SubscriptionCode == storeService.ProductCode);
                    //    if (recurringPlan != null)
                    //    {
                    //        recurringPlan.RecurringDate = nextDate;
                    //        DB.Entry(recurringPlan).State = EntityState.Modified;
                    //    }
                    //}
                }
            }
            await DB.SaveChangesAsync();

            //call api to pos
            StoreProfileReq storeProfile = GetStoreProfileReady(licenseId, activeStatus, stage);
            DoRequest(storeProfile);
            new FeatureService(DB).ActiveDefineFeatureByStore(storeCode);
        }

        private StoreProfileReq GetStoreProfileReadyLifetime(Store_Services storeService)
        {
            var db = new WebDataModel();
            var License = storeService ?? new Store_Services();
            var Customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == License.CustomerCode) ?? new C_Customer();
            var data = new StoreProfileReq()
            {
                storeId = Customer.StoreCode,
                storeName = Customer.BusinessName,
                StoreAddress = Customer.GetSalonAddress(),
                City = Customer.GetSalonCity(),
                State = Customer.GetSalonState(),
                ZipCode = Customer.GetSalonZipCode(),
                StoreEmail = Customer.MangoEmail ?? Customer.SalonEmail ?? Customer.Email,
                StorePhone = Customer.BusinessPhone,
                ContactName = Customer.OwnerName ?? Customer.ContactName,
                lastUpdate = DateTime.UtcNow.ToString(StoreServices.F_DATE),
                UpdateBy = License?.LastUpdateBy ?? cMem?.FullName,
                Password = Customer.MD5PassWord ?? "",
                RequirePassChange = Customer.RequirePassChange ?? "off",
                Status = DB.Store_Services.Any(s => (s.StoreCode == License.StoreCode || s.StoreCode == Customer.StoreCode) && s.Type == "license" && s.Active == 1) ? "Activated" : "DeActivated",
                SubscriptionCode = License?.ProductCode,
                SubscriptionName = License?.Productname,
                TimeZone = Customer.SalonTimeZone_Number ?? TIMEZONE_NUMBER_BY_ID.Eastern.Text(),
                TimeZoneName = Customer.SalonTimeZone ?? TIMEZONE_NUMBER_BY_ID.Eastern.Code<string>(),
                CompanyPartner = Customer.PartnerCode ?? ""
            };

            return data;
        }

        public void SyncLifeTimeToPos(Store_Services storeService, Order_Subcription orderSubscription)
        {
            var db = new WebDataModel();
            storeService.RenewDate = storeService.RenewDate.HasValue ?
                storeService.RenewDate.Value.AddYears(100) : new DateTime(2100, 1, 1);
            storeService.AutoRenew = false;
            orderSubscription.EndDate = orderSubscription.EndDate.HasValue ?
                orderSubscription.EndDate.Value.AddYears(100) : new DateTime(2100, 1, 1);
            orderSubscription.AutoRenew = false;
            db.Entry(storeService).State = EntityState.Modified;
            db.Entry(orderSubscription).State = EntityState.Modified;
            db.SaveChanges();
            StoreProfileReq storeProfile = GetStoreProfileReadyLifetime(storeService);
            DoRequest(storeProfile);
        }
    }
}