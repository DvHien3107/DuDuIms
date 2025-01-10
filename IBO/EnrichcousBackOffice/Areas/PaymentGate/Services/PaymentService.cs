using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Enrichcous.Payment.Mxmerchant.Api;
using Enrichcous.Payment.Mxmerchant.Config.Enums;
using Enrichcous.Payment.Mxmerchant.Models;
using Enrichcous.Payment.Mxmerchant.Utils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using EnrichcousBackOffice.Areas.PaymentGate;
using System.Configuration;
using System.Net;
using System.Web.Configuration;
using EnrichcousBackOffice.Services.Notifications;
using EnrichcousBackOffice.Utils.AppConfig;
using Enrich.IServices.Utils.Mailing;
using Enrich.Core.Infrastructure;
using System.Net.PeerToPeer;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers.Bases;

namespace EnrichcousBackOffice.Areas.PaymentGate.Services
{
    public class PaymentService
    {
        private readonly CardUtil _cardUtil = new CardUtil();
        private static readonly MxMerchantFunc PaymentFunc = new MxMerchantFunc();
        //private readonly XmlPayments NuveiPayments = new XmlPayments();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        /// <summary>
        /// NewCard
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="AppHandleException"></exception>
        /// 

        public AuthDotNetEnv getAuthEnv()
        {
            AuthDotNetEnv env = new AuthDotNetEnv();
            env.ApiLoginID = WebConfigurationManager.AppSettings["ApiLoginID"];
            env.ApiTransactionKey = WebConfigurationManager.AppSettings["ApiTransactionKey"];
            string IsSandBox = WebConfigurationManager.AppSettings["IsSandBox"];
            if (IsSandBox.ToUpper() == "SANBOX")
            {
                env.IsSanbox = true;
            }
            else
            {
                env.IsSanbox = false;
            }
            return env;
        }

        public C_CustomerCard NewCard(TransRequest request, string PartnerCode = "")
        {
            try
            {
                AuthDotNetEnv env = getAuthEnv();
                // Make card info
                // Verify & register
                //_cardUtil.CardExpiryCheck(request.CardExpiry);

                if (request.MxMerchant_Id == null)
                {
                    var card = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(request));
                    request.MxMerchant_Id = CreateCustomerProfile(card, PartnerCode);
                }

                var respone = new ADotNetResponse();
                string customerProfileId = "";
                try
                {
                    AuthCardInfo newCard = new AuthCardInfo();
                    newCard.cardNumber = request.CardNumber;
                    newCard.cardCode = request.CardCSC;
                    newCard.expirationDate = request.CardExpiry;
                    if (request.MxMerchant_Id == 0)
                    {
                        return null;
                    }
                    customerProfileId = (request.MxMerchant_Id ?? 0).ToString();
    
                    respone = AuthDotNet.CreateCustomerPaymentProfile(customerProfileId, newCard, env);
                    if(respone.status != 200)
                    {
                        throw new Exception("CreateCustomerPaymentProfile:" + respone.message);
                    }
                }
                catch (Exception e)
                {
                    PaymentExceptionLog(e);
                    throw e;
                }
                string CusPaymentProfile = respone.ResponId;
                var getCustomerPaymentProfile = AuthDotNet.GetCustomerPaymentProfile(env, customerProfileId, CusPaymentProfile);
                var CustomerProfileResponse = (getCustomerPaymentProfile.Respon as getCustomerPaymentProfileResponse);
                var paymentProfile = CustomerProfileResponse.paymentProfile;
                var creditCardRespone = (paymentProfile.payment.Item as creditCardMaskedType);
                if (getCustomerPaymentProfile.status == 200 && CustomerProfileResponse.paymentProfile.payment.Item is creditCardMaskedType)
                {
                    if (!string.IsNullOrEmpty(PartnerCode))
                    {
                        using (var db = new WebDataModel())
                        {
                            var cur_card = db.C_PartnerCard.Where(c => c.PartnerCode == PartnerCode && c.MxMerchant_CardId == CusPaymentProfile).FirstOrDefault();
                            var today = DateTime.UtcNow;
                            bool IsDefault = db.C_PartnerCard.FirstOrDefault(c => c.PartnerCode == PartnerCode && c.IsDefault == true && c.MxMerchant_Id == long.Parse(customerProfileId)) == null;

                            C_PartnerCard card = cur_card ?? new C_PartnerCard
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                CardType = creditCardRespone.cardType.ToUpper(),
                                CardNumber = creditCardRespone.cardNumber,
                                CreateAt = DateTime.UtcNow,
                            };
                            card.CardHolderName = paymentProfile.billTo?.firstName??"" + " " + paymentProfile.billTo?.lastName ?? "";
                            card.CardAddressStreet = paymentProfile.billTo?.address ?? "";
                            card.CardZipCode = paymentProfile.billTo?.zip ?? "";
                            card.Active = true;
                            card.CardExpiry = creditCardRespone.expirationDate;
                            card.UpdateAt = today;
                            card.MxMerchant_Id = long.Parse(customerProfileId);
                            card.MxMerchant_CardId = CusPaymentProfile;
                            card.MxMerchant_Token = paymentProfile.originalNetworkTransId;
                            card.CardPhone = request.CardPhone;
                            card.CardEmail = request.CardEmail;
                            card.PartnerCode = PartnerCode;
                            card.IsDefault = IsDefault;
                            card.IsRecurring = true;
                            card.MerchantCardReference = CardSecurity.EncryptionCardNumber(request.CardNumber, paymentProfile.billTo.ToString());
                            //db.C_PartnerCard.Where(_card => _card.PartnerCode == PartnerCode && _card.MxMerchant_Id != respone.id).ToList().ForEach(_card =>
                            //{
                            //    _card.IsDefault = false;
                            //    db.Entry(_card).State = EntityState.Modified;
                            //});
                            db.C_PartnerCard.AddOrUpdate(card);
                            db.SaveChanges();
                            return JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(card));

                        }

                    }
                    else
                    {
                        using (var db = new WebDataModel())
                        {
                            var cur_card = db.C_CustomerCard.Where(c => c.CustomerCode == request.CustomerCode && c.MxMerchant_CardId == CusPaymentProfile).FirstOrDefault();
                            var today = DateTime.UtcNow;
                            long longCusProfileId = long.Parse(customerProfileId);
                            bool IsDefault = db.C_CustomerCard.FirstOrDefault(c => c.CustomerCode == request.CustomerCode && c.IsDefault == true && c.MxMerchant_Id == longCusProfileId) == null;
                            C_CustomerCard card = cur_card ?? new C_CustomerCard
                            {
                                Id = Guid.NewGuid().ToString("N"),
                                CardType = creditCardRespone.cardType.ToUpper(),
                                CardNumber = creditCardRespone.cardNumber,
                                CreateAt = DateTime.UtcNow,
                                CustomerCode = request.CustomerCode,
                                StoreCode = request.StoreCode,
                            };
                            card.CardHolderName = paymentProfile.billTo?.firstName + " " + paymentProfile.billTo?.lastName ?? "";
                            card.CardAddressStreet = paymentProfile.billTo?.address ?? "";
                            card.CardZipCode = paymentProfile.billTo?.zip??"";
                            card.Active = true;
                            card.CardExpiry = creditCardRespone.expirationDate;
                            card.UpdateAt = today;
                            card.MxMerchant_Id = long.Parse(customerProfileId);
                            card.MxMerchant_CardId = CusPaymentProfile;
                            card.MxMerchant_Token = paymentProfile.originalNetworkTransId;
                            card.CardPhone = request.CardPhone;
                            card.CardEmail = request.CardEmail;
                            card.IsDefault = IsDefault;
                            card.IsRecurring = true;
                            card.MerchantCardReference = CardSecurity.EncryptionCardNumber(request.CardNumber, paymentProfile.billTo?.ToString());
                            db.C_CustomerCard.AddOrUpdate(card);
                            db.SaveChanges();
                            if (card.IsDefault == true)
                            {
                                //update recurring cho tat ca nhung service chua co mxID
                                var storeServices = db.Store_Services.Where(c => c.CustomerCode == card.CustomerCode &&
                                                                                c.AutoRenew == true &&
                                                                                c.RenewDate > DateTime.UtcNow &&
                                                                                c.MxMerchant_cardAccountId == null
                                                                                ).ToList();
                                foreach (var ss in storeServices)
                                {
                                    SetStatusRecurring(ss.Id, "active");
                                }
                            }
                            return card;
                        }

                    }
                }
                else
                {

                    throw new Exception("GetCustomerPaymentProfile:" + getCustomerPaymentProfile.message);
                }
            }
            catch (AppHandleException ex)
            {
                throw new AppHandleException(ex.ExCode, ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new AppHandleException(null, ex.Message, ex.InnerException);
            }
        }
        private static void PaymentExceptionLog(Exception e)
        {
            var slog = e.Data["Log"]?.ToString();
            if (!string.IsNullOrEmpty(slog))
            {
                var imslog = JsonConvert.DeserializeObject<IMSLog>(slog);
                imslog.Id = Guid.NewGuid().ToString("N");
                using (var db = new WebDataModel())
                {
                    db.IMSLogs.Add(imslog);
                    db.SaveChanges();
                }
            }
            else
            {
                using (WebDataModel db = new WebDataModel())
                {
                    IMSLog imslog = new IMSLog()
                    {
                        Id = Guid.NewGuid().ToString(),
                        CreateBy = "IMS System",
                        CreateOn = DateTime.UtcNow,
                        Url = System.Web.HttpContext.Current?.Request.Url.PathAndQuery,
                        RequestUrl = System.Web.HttpContext.Current?.Request.Url.PathAndQuery,
                        StatusCode = 500,
                        Success = false,
                        RequestMethod = "Get",
                        Description = "System Error",
                        JsonRespone = JsonConvert.SerializeObject(e)
                    };
                    db.IMSLogs.Add(imslog);
                    db.SaveChanges();
                }
            }
        }

        public long CreateCustomerProfile(C_CustomerCard card, string PartnerCode = "")
        {
            try
            {
                AuthDotNetEnv env = getAuthEnv();
                long MxMerchant_cusId = 0;
                using (var db = new WebDataModel())
                {
                    var partner = db.C_Partner.FirstOrDefault(c => c.Code == PartnerCode) ?? new C_Partner { };
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == card.CustomerCode) ?? new C_Customer { };
                    if (!string.IsNullOrEmpty(PartnerCode))
                    {
                        cus.OwnerName = partner.ContactName;
                        cus.CellPhone = partner.Phone;
                        cus.BusinessAddress = partner.Address;
                        cus.BusinessEmail = partner.Email;
                        cus.BusinessCity = partner.City;
                        cus.BusinessState = partner.State;
                        cus.BusinessZipCode = partner.Zipcode;
                    }
                    var Customer = new CustomerProfile
                    {
                        firstName = (card.CardHolderName ?? "").Split(' ').FirstOrDefault(),
                        lastName = (card.CardHolderName ?? "").Split(' ').LastOrDefault(),
                        address = card.CardAddressStreet,
                        city = card.CardCity,
                        state = card.CardState,
                        zip = card.CardZipCode,
                    };
                    var result = AuthDotNet.CreateCustomerProfile(cus.Email, Customer, env);
                    string ResponId = "0";
                    if (result.status == 200)
                    {
                        ResponId = result.ResponId;
                    }
                    else
                    {
                        throw new Exception("CreateCustomerProfile:" + result.message);
                    }
                    
                    MxMerchant_cusId = long.Parse(ResponId);
                    if (!string.IsNullOrEmpty(PartnerCode))
                    {
                        partner.MxMerchant_Id = MxMerchant_cusId;
                        db.Entry(partner).State = EntityState.Modified;
                    }
                    else
                    {
                        cus.MxMerchant_Id = MxMerchant_cusId.ToString();
                        db.Entry(cus).State = EntityState.Modified;
                    }
                    db.SaveChanges();
                }
                return MxMerchant_cusId;
            }
            catch (Exception e)
            {
                PaymentExceptionLog(e);
                throw e;
            }


        }
        public static void Save_MxMerchantTokens(OauthInfo auth)
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
        //public C_CustomerCard EditCard(C_CustomerCard card, string PartnerCode = "")
        //{
        //    try
        //    {
        //        using (var db = new WebDataModel())
        //        {

        //            var card_info = new card_info
        //            {
        //                id = card.MxMerchant_Id,
        //                token = card.MxMerchant_Token,
        //                avsStreet = card.CardAddressStreet,
        //                avsZip = card.CardZipCode,
        //                expiryMonth = card.CardExpiry.Split('/').FirstOrDefault(),
        //                expiryYear = card.CardExpiry.Split('/').LastOrDefault(),
        //                name = card.CardHolderName,
        //                //number = card.CardNumber, /*không thể thay đổi card number*/
        //            };
        //            var auth = UserContent.GetOauthInfo();
        //            var respone = new card_info_response();
        //            long cus_MxMerchant_Id = 0;
        //            if (!string.IsNullOrEmpty(PartnerCode)) cus_MxMerchant_Id = db.C_Partner.FirstOrDefault(c => c.Code == PartnerCode)?.MxMerchant_Id ?? 0;
        //            else cus_MxMerchant_Id = db.C_Customer.Where(c => c.CustomerCode == card.CustomerCode).FirstOrDefault()?.MxMerchant_Id ?? 0;
        //            if (cus_MxMerchant_Id == 0) cus_MxMerchant_Id = Create_MxMerchant_Customer(card, PartnerCode);

        //            respone = PaymentFunc.CreateCard(card_info, ref auth, cus_MxMerchant_Id);
        //            if (auth.Updated) { Save_MxMerchantTokens(auth); }

        //            if (!string.IsNullOrEmpty(PartnerCode))
        //            {
        //                var EditCard = db.C_PartnerCard.Find(card.Id);
        //                EditCard.CardHolderName = respone.name;
        //                EditCard.CardAddressStreet = respone.avsStreet;
        //                EditCard.CardZipCode = respone.avsZip;
        //                EditCard.Active = true;
        //                EditCard.CardExpiry = respone.expiryMonth + respone.expiryYear.Substring(2, 2);
        //                EditCard.MxMerchant_Id = respone.id;
        //                EditCard.MxMerchant_CardId = respone.cardId;
        //                EditCard.MxMerchant_Token = respone.token;
        //                EditCard.CardPhone = card.CardPhone;
        //                EditCard.CardEmail = card.CardEmail;
        //                EditCard.UpdateAt = DateTime.UtcNow;
        //                db.Entry(EditCard).State = EntityState.Modified;
        //                db.SaveChanges();
        //                return JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(EditCard));
        //            }
        //            else
        //            {
        //                var EditCard = db.C_CustomerCard.Find(card.Id);
        //                EditCard.CardHolderName = respone.name;
        //                EditCard.CardAddressStreet = respone.avsStreet;
        //                EditCard.CardZipCode = respone.avsZip;
        //                EditCard.Active = true;
        //                EditCard.CardExpiry = respone.expiryMonth + respone.expiryYear.Substring(2, 2);
        //                EditCard.MxMerchant_Id = respone.id;
        //                EditCard.MxMerchant_CardId = respone.cardId;
        //                EditCard.MxMerchant_Token = respone.token;
        //                EditCard.CardPhone = card.CardPhone;
        //                EditCard.CardEmail = card.CardEmail;
        //                EditCard.UpdateAt = DateTime.UtcNow;
        //                db.Entry(EditCard).State = EntityState.Modified;
        //                db.SaveChanges();
        //                return EditCard;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        PaymentExceptionLog(e);
        //        throw e;
        //    }
        //}

        //public async Task<string> ACHCollect(TransRequest info)
        //{
        //    try
        //    {
        //        using (var db = new WebDataModel())
        //        {
        //            var cardData = new C_CustomerCard();
        //            bool paylater = false;
        //            paylater = (db.O_Orders.Where(o => o.OrdersCode == info.OrdersCode).FirstOrDefault()?.Status == InvoiceStatus.PaymentLater.ToString());
        //            var transData = NewTrans(info, cardData);
        //            if (transData?.PaymentStatus != "Approved")
        //            {
        //                //update transaction to finace ticket
        //                //await AddTransactionToFinanceTicket(info, transData, cardData, false);
        //                throw new Exception(transData.ResponseText);
        //            }
        //            await SendEmail_ReceiptPayment(info.OrdersCode, "ACH");
        //            await UpdateOrder(info, transData, cardData);

        //            var order = db.O_Orders.Where(o => o.OrdersCode == info.OrdersCode).FirstOrDefault();
        //            List<Tuple<string, string, Boolean, string>> tmp = new List<Tuple<string, string, Boolean, string>>();
        //            if (UserContent.GetWebInfomation(true).AutoActiveRecurringLicense == true)
        //            {
        //                var old_recurring = db.Store_Services.FirstOrDefault(s => s.StoreCode == info.StoreCode && s.Active == 1 && s.RenewDate > DateTime.UtcNow && s.Type == "license" && s.AutoRenew == true);
        //                var ls = db.Store_Services.Where(s => s.OrderCode == info.OrdersCode && s.Type == "license" && s.Active == -1).FirstOrDefault();
        //                if (paylater)
        //                {
        //                    var license = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode && s.Type == "license" && s.RenewDate >= DateTime.UtcNow).FirstOrDefault();
        //                    if (license != null)
        //                    {
        //                        using (MerchantService service = new MerchantService(true))
        //                        {
        //                            var rs = await service.ApproveAction(license.StoreCode, license.Id, true, "same-active");
        //                        }
        //                    }
        //                }
        //                else if (ls != null)
        //                {
        //                    var activelicense = ls.Id;
        //                    using (MerchantService service = new MerchantService(true))
        //                    {
        //                        activelicense = await service.ApproveAction(ls.StoreCode, ls.Id, true, "active");
        //                    }
        //                    if (ls.AutoRenew == true && order?.PaymentMethod != "Recurring")
        //                    {
        //                        if (old_recurring != null) { PaymentService.SetStatusRecurring(old_recurring.Id, "inactive"); }
        //                        NewRecurring(activelicense, cardData, "ACH");
        //                    }
        //                }
        //                var adds = db.Store_Services.Where(s => s.OrderCode == info.OrdersCode && s.Type == "addon" && s.Active == -1).ToList();
        //                if (adds.Count > 0)
        //                {
        //                    foreach (var add in adds)
        //                    {
        //                        //xử lý khi active addon SMS khi dã kích hoạt SMS unlimited
        //                        if (add.Type == LicenseType.ADD_ON.Text())
        //                        {
        //                            //var smsAddonActivated = new StoreServices(db).LicensesPlan(add.StoreCode, null).FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit;
        //                            //var smsAddonNew = new StoreServices(db).LicensesAddOn(add.StoreCode, add.Id).FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit;
        //                            //if (!(smsAddonNew != "-1" && smsAddonNew != "0" && smsAddonActivated == "-1"))
        //                            //{
        //                            //    tmp.Add(Tuple.Create(add.StoreCode, add.Id, true, "active"));
        //                            //}
        //                            tmp.Add(Tuple.Create(add.StoreCode, add.Id, true, "active"));

        //                        }

        //                    }
        //                }
        //                if (tmp.Count > 0)
        //                {
        //                    //Parallel.ForEach(tmp, async add =>
        //                    foreach (var add in tmp)
        //                    {
        //                        using (MerchantService service = new MerchantService(true))
        //                        {
        //                            await service.ApproveAction(add.Item1, add.Item2, add.Item3, "active");
        //                        }
        //                    };
        //                }
        //            }
        //        }
        //        PaymentService.UpdateRecurringStatus(info.CustomerCode);
        //        return string.Empty;
        //    }
        //    catch (Exception ex)
        //    {
        //        PaymentExceptionLog(ex);
        //        return ex.Message;
        //    }
        //}

        /// <summary>
        /// NewTrans
        /// </summary>
        /// <param name="request"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        /// <exception cref="AppHandleException"></exception>
        public C_CustomerTransaction NewTrans(TransRequest request, C_CustomerCard card, string type = "")
        {
            AuthDotNetEnv Env = getAuthEnv();

            using (var db = new WebDataModel())
            {
                O_Orders order = db.O_Orders.FirstOrDefault(o => o.OrdersCode == request.OrdersCode);
                C_Customer cus = db.C_Customer.Where(c => c.CustomerCode == request.CustomerCode).FirstOrDefault();
                var old_trans = db.C_CustomerTransaction.Where(c => c.OrdersCode == request.OrdersCode).FirstOrDefault();
                if (old_trans?.PaymentStatus == "Approved" || old_trans?.PaymentStatus == "Success")
                {
                    return old_trans;
                }
                try
                {
                    string customerProfileId = "";
                    string customerPaymentProfileId = "";
                    var payment_info = new MxMerchant_payment
                    {
                        amount = (double)(order.GrandTotal ?? 0)
                    };
                    if (request.PaymentMethod == "ACH" || type == "ACH")
                    {
                        throw new Exception("Not Support ACH Method");
                    }
                    else
                    {
                        customerProfileId = card.MxMerchant_Id.ToString();
                        customerPaymentProfileId = card.MxMerchant_CardId;
                        if (string.IsNullOrEmpty(customerPaymentProfileId)|| string.IsNullOrEmpty(customerProfileId)) throw new Exception("Not yet credit card");
                    }
                    var respone = AuthDotNet.ChargeCustomerProfile(customerProfileId, customerPaymentProfileId, order.GrandTotal ?? 0, Env);
                    C_CustomerTransaction trans = null;

                    var id = Guid.NewGuid().ToString("N");
                    trans = new C_CustomerTransaction()
                    {
                        Id = id,
                        OrdersCode = request.OrdersCode,
                        CustomerCode = request.CustomerCode,
                        StoreCode = request.StoreCode,
                        Amount = (long)order.GrandTotal,
                        CreateAt = DateTime.UtcNow,
                    };
                    trans.Card = card.Id;
                    trans.PaymentStatus = respone.status.ToString();
                    trans.MxMerchant_id = respone.ResponId;
                    trans.MxMerchant_authMessage = respone.message;
                    trans.MxMerchant_token = customerProfileId;
                    trans.UpdateAt = DateTime.UtcNow;
                    trans.BankName = card.CardType;
                    trans.CardNumber = card.CardNumber;
                    trans.PaymentMethod = request.PaymentMethod;
                    trans.PaymentNote = request.PaymentNote;
                    trans.CreateBy = cMem?.FullName;
                    trans.ResponseText = respone.JsonResult;
                    db.C_CustomerTransaction.Add(trans);
                    db.SaveChanges();

                    if (respone.status !=200)
                    {
                        PaymentExceptionLog(new Exception(respone.message));
                    }

                    return trans;
                }
                catch (Exception e)
                {
                    PaymentExceptionLog(e);
                    var id = Guid.NewGuid().ToString("N");
                    var trans = new C_CustomerTransaction()
                    {
                        Id = id,
                        OrdersCode = request.OrdersCode,
                        CustomerCode = request.CustomerCode,
                        StoreCode = request.StoreCode,
                        Amount = (long)order.GrandTotal,
                        CreateAt = DateTime.UtcNow,
                    };
                    trans.Card = card.Id;
                    trans.PaymentStatus = "Failed";
                    trans.UpdateAt = DateTime.UtcNow;
                    trans.BankName = card.CardType;
                    trans.CardNumber = card.CardNumber;
                    trans.PaymentMethod = request.PaymentMethod;
                    trans.PaymentNote = request.PaymentNote;
                    trans.CreateBy = cMem?.FullName;
                    trans.ResponseText = e.Message;
                    db.C_CustomerTransaction.Add(trans);
                    db.SaveChanges();

                    return trans;
                    //throw e;
                }

            }
        }

        //public C_CustomerTransaction RefundTrans(TransRequest request, C_CustomerCard card, C_CustomerTransaction transaction)
        //{
        //    using (var db = new WebDataModel())
        //    {
        //        C_Customer cus = db.C_Customer.Where(c => c.CustomerCode == request.CustomerCode).FirstOrDefault();
        //        if (transaction?.PaymentStatus == "Approved" || transaction?.PaymentStatus == "Success") return transaction;
        //        try
        //        {
        //            var auth = UserContent.GetOauthInfo();
        //            var payment_info = new MxMerchant_payment
        //            {
        //                amount = (double)(transaction.Amount),
        //                cardAccount = new MxMerchant_payment.CardAccount { token = card.MxMerchant_Token },
        //                customer = new MxMerchant_payment.Customer { id = request.MxMerchant_Id?.ToString() ?? cus.MxMerchant_Id?.ToString() }
        //            };

        //            var respone = PaymentFunc.MakePayment(payment_info, ref auth);
        //            if (auth.Updated) { Save_MxMerchantTokens(auth); }

        //            transaction.PaymentStatus = respone.status;
        //            transaction.MxMerchant_id = respone.id;
        //            transaction.MxMerchant_authMessage = respone.authMessage;
        //            transaction.MxMerchant_token = respone.paymentToken;
        //            transaction.UpdateAt = DateTime.UtcNow;
        //            transaction.Card = card.Id;
        //            transaction.BankName = card.CardType;
        //            transaction.CardNumber = card.CardNumber;
        //            db.Entry(transaction).State = EntityState.Modified;
        //            db.SaveChanges();
        //            if (respone.status != PaymentStatus.Approved.Text())
        //            {
        //                PaymentExceptionLog(new Exception(respone.authMessage));
        //            }
        //            return transaction;
        //        }
        //        catch (Exception e)
        //        {
        //            PaymentExceptionLog(e);
        //            transaction.PaymentStatus = "Failed";
        //            transaction.UpdateAt = DateTime.UtcNow;
        //            transaction.Card = card.Id;
        //            transaction.BankName = card.CardType;
        //            transaction.CardNumber = card.CardNumber;
        //            transaction.ResponseText = e.Message;
        //            db.Entry(transaction).State = EntityState.Modified;
        //            db.SaveChanges();
        //            return transaction;
        //        }
        //    }
        //}

        public bool NewRecurring(string serviceId, C_CustomerCard card, string type = "")
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    //var auth = UserContent.GetOauthInfo();
                    var store = db.Store_Services.Find(serviceId);
                    if (store.StoreApply == "trail" || store.StoreApply == "Promotional")
                    {
                        throw new Exception("Only Regular Subscriptions can set recurring!");
                    }

                    if (type == "ACH")
                    {
                        store.MxMerchant_cardAccountId = null; //respone.subscription.cardAccountId;
                        store.MxMerchant_SubscriptionStatus = "Inactive"; //respone.subscription.status;
                        store.RecurringType = "ACH";
                    }
                    else
                    {
                        store.MxMerchant_cardAccountId = card?.MxMerchant_Id; //respone.subscription.cardAccountId;
                        store.MxMerchant_SubscriptionStatus = "active"; //respone.subscription.status;
                        store.RecurringType = "CreditCard";
                    }

                    store.AutoRenew = true;
                    db.Entry(store).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception e)
            {
                PaymentExceptionLog(e);
                return false;
            }

        }
        public static bool UpdateRecurringStatus(string customerCode = "")
        {
            if (string.IsNullOrEmpty(customerCode)) return false;
            //update recuring status of MxMerchant
            //var db = new WebDataModel();
            using (var db = new WebDataModel())
            {
                db.Store_Services.Where(d => d.Active != 1 && d.CustomerCode == customerCode).ForEach(s =>
                {
                    //nếu mxmerchant recurring đang active, nhưng store không bật autorecurring hoặc store không active và cũng ko waiting (có trường hợp đã thanh toán và bật recuring như chưa được active)
                    try
                    {
                        SetStatusRecurring(s.Id, "inactive");
                    }
                    catch (Exception)
                    {

                    }

                });

                db.Store_Services.Where(d => d.AutoRenew == true && d.Active == 1).ForEach(s =>
                {
                    //nếu store active & set AutoRecurring, nhưng chưa active mxmerchant recurring
                    try
                    {
                        SetStatusRecurring(s.Id, "active", s.RecurringType);
                    }
                    catch (Exception e)
                    {
                        //nếu active mxmerchant recurring thất bại, tắt autorecurring của store
                        s.AutoRenew = false;
                        db.SaveChanges();
                    }
                });
            }
            return true;
        }
        public static void addRecurringList(string StoreCode, string ProductCode)
        {
            WebDataModel db = new WebDataModel();
            db.Database.ExecuteSqlCommand($@"
if not exists (select * from RecurringList where StoreCode = '{StoreCode}' and ProductCode =  '{ProductCode}' )
begin
    insert into RecurringList 
    values('{StoreCode}', '{ProductCode}', GETUTCDATE())
end
");
        }
        public static void removeRecurringList(string StoreCode, string ProductCode)
        {
            WebDataModel db = new WebDataModel();
            db.Database.ExecuteSqlCommand($"delete RecurringList where StoreCode = '{StoreCode}' and ProductCode = '{ProductCode}'");
        }
        public static bool SetStatusRecurring(string store_id, string status, string type = "",bool forceCreateRecurringPlan = false)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    //var auth = UserContent.GetOauthInfo();
                    //var data = new MxMerchant_recurring();
                    var store = db.Store_Services.Find(store_id);
                    var o_subscription = db.Order_Subcription.FirstOrDefault(c => c.OrderCode == store.OrderCode && 
                        c.Product_Code == store.ProductCode && c.SubscriptionType != "setupfee" && c.SubscriptionType != "interactionfee");
                    if (o_subscription == null) return true;
                    //var recurringPlan = db.RecurringPlannings.FirstOrDefault(c => c.OrderCode == store.OrderCode && c.SubscriptionCode == store.ProductCode);
                    var customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == store.CustomerCode) ?? new C_Customer { };
                    var card = new C_CustomerCard();
                    var p_card = db.C_PartnerCard.FirstOrDefault(c => c.PartnerCode == customer.PartnerCode && c.Active == true && c.IsDefault == true); //partner card default
                    if (p_card == null)  card = db.C_CustomerCard.FirstOrDefault(c => c.CustomerCode == store.CustomerCode && c.Active == true && c.IsDefault == true); //card default cua merchant
                    else card = JsonConvert.DeserializeObject<C_CustomerCard>(JsonConvert.SerializeObject(p_card)); //convert to format C_CustomerCard
                 
                    if (status == "active" && type != "ACH")
                    {
                        if (card == null)
                        {
                            o_subscription.AutoRenew = true;
                            store.AutoRenew = true;
                            store.MxMerchant_cardAccountId = 0;
                            db.Entry(store).State = EntityState.Modified;
                            db.Entry(o_subscription).State = EntityState.Modified;
                            //return false;
                            //return false;
                            //throw new Exception("This merchant does not have any default Cards yet.");
                        }
                        else if (card?.MxMerchant_Id == null)
                        {
                            o_subscription.AutoRenew = false;
                            store.AutoRenew = false;
                            db.Entry(store).State = EntityState.Modified;
                            db.Entry(o_subscription).State = EntityState.Modified;
                            db.SaveChanges();
                            //return false;
                            throw new Exception("Default Cards of this Merchant can't payment.");
                        }
                        //data = getDataRecurring(store, card.MxMerchant_Id ?? 0);
                    }
                    if (type == "ACH")
                    {
                        store.MxMerchant_cardAccountId = null; //respone.subscription.cardAccountId;
                        store.MxMerchant_SubscriptionStatus = "Inactive"; //respone.subscription.status;
                        store.RecurringType = "ACH";
                    }
                    else if (type == "Creditcard")
                    {
                        store.MxMerchant_cardAccountId = card?.MxMerchant_Id; //respone.subscription.cardAccountId;
                        store.MxMerchant_SubscriptionStatus = status == "active" ? "Active" : "Inactive"; //respone.subscription.status;
                        store.RecurringType = "CreditCard";
                    }
                    else
                    {
                        store.MxMerchant_cardAccountId = null; //respone.subscription.cardAccountId;
                        store.MxMerchant_SubscriptionStatus = "Inactive"; //respone.subscription.status;
                        store.RecurringType = string.Empty;
                    }
                    store.AutoRenew = (status == "active");
                    o_subscription.AutoRenew = (status == "active");

                    //db.Entry(card).State = EntityState.Modified;
                    db.Entry(store).State = EntityState.Modified;
                    db.Entry(o_subscription).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    PaymentExceptionLog(e);
                    throw e;
                }
            }
        }

        //public static string DeactiveAllRecurring()
        //{
        //    using (var db = new WebDataModel())
        //    {
        //        var countErr = 0;
        //        var msgError = string.Empty;
        //        var recurrings = db.Store_Services.Where(c => c.MxMerchant_SubscriptionStatus == "Active" && c.Active == 1).ToList();
        //        recurrings.ForEach(store =>
        //        {
        //            try
        //            {
        //                var auth = UserContent.GetOauthInfo();
        //                var data = getDataRecurring(store, store.MxMerchant_cardAccountId ?? 0);
        //                data.subscription.status = "inactive";
        //                data.contract.id = store.MxMerchant_contractId;
        //                data.contract.subid = store.MxMerchant_SubscriptionId;
        //                var respone = PaymentFunc.MakeRecuring(data, ref auth);
        //            }
        //            catch (Exception ex)
        //            {
        //                countErr++;
        //                msgError += ex.Message + "\n";
        //            }
        //        });
        //        PaymentExceptionLog(new Exception(msgError));
        //        return countErr + " contracts error, please check MxMerchant";
        //    }
        //}

        public static MxMerchant_recurring getDataRecurring(Store_Services store, long cardId)
        {
            //if (!int.TryParse(ConfigurationManager.AppSettings.Get("RecurringDay"), out int RecurringDay))
            //{
            //    RecurringDay = 25;
            //};
            using (var db = new WebDataModel())
            {
                var beforeDays = db.SystemConfigurations.FirstOrDefault().RecurringBeforeDueDate ?? 0;
                var RecurringDate = store.RenewDate.Value.AddDays(-beforeDays);
                int RecurringDay = RecurringDate.Day;
                var cus = db.C_Customer.Where(c => c.CustomerCode == store.CustomerCode).FirstOrDefault();
                //var product = db.License_Product.Where(p => p.Code == store.ProductCode).FirstOrDefault();
                var order_product = db.Order_Subcription.Where(op => op.OrderCode == store.OrderCode && op.Product_Code == store.ProductCode).FirstOrDefault();
                var p_card = db.C_PartnerCard.FirstOrDefault(c => c.MxMerchant_Id == cardId) ?? new C_PartnerCard { };
                var partner = db.C_Partner.FirstOrDefault(c => c.Code == p_card.PartnerCode) ?? new C_Partner { };

                var purchases = new List<MxMerchant_recurring.Purchase>();
                purchases.Add(new MxMerchant_recurring.Purchase
                {
                    productName = store.Productname,
                    price = (double)(order_product.Price - (store.ApplyDiscountAsRecurring == true ? order_product.Discount : 0)),
                    quantity = order_product.Quantity ?? 1,
                    subTotalAmount = (double)(order_product.Price * (order_product.Quantity ?? 1) - (store.ApplyDiscountAsRecurring == true ? order_product.Discount : 0)),
                    //discountAmount = (double)(store.ApplyDiscountAsRecurring == true ? order_product.Discount : 0)
                });
                store.RenewDate = store.RenewDate ?? DateTime.UtcNow;
                var startdate = new DateTime(store.RenewDate.Value.Year, store.RenewDate.Value.Month, RecurringDay).AddDays(-1);
                if (startdate < DateTime.UtcNow)
                    startdate = DateTime.UtcNow;
                var data = new MxMerchant_recurring
                {
                    contract = new MxMerchant_recurring.Contract
                    {
                        customer = new MxMerchant_recurring.Customer
                        {
                            id = long.Parse(cus.MxMerchant_Id ?? "0"),
                        },
                        purchases = purchases,
                        frequency = purchases.Max(p => p.quantity),
                        interval = store.PeriodRecurring ?? "Monthly",
                        subTotalAmount = purchases.Sum(p => p.subTotalAmount),
                        totalAmount = purchases.Sum(p => p.subTotalAmount),
                        quantity = purchases.Count,
                        dayType = "Day",
                        dayNumber = RecurringDay.ToString(),
                        month = RecurringDate.ToString("MMMM"),
                        weekDay = RecurringDate.ToString("dddd")
                    },

                    subscription = new MxMerchant_recurring.Subscription
                    {
                        customerId = partner.MxMerchant_Id ?? long.Parse(cus.MxMerchant_Id ?? "0"),
                        cardAccountId = cardId,
                        status = "active",
                        startDate = startdate.ToString("yyyy-MM-dd"),
                        occurrences = 0,
                    }
                };
                return data;
            }
        }
        /// <summary>
        /// UpdateOrder
        /// </summary>
        /// <param name="info"></param>
        /// <param name="trans"></param>
        /// <param name="card"></param>
        /// <returns></returns>
        public async Task UpdateOrder(TransRequest info, C_CustomerTransaction trans, C_CustomerCard card)
        {
            try
            {
                var db = new WebDataModel();
                // Update store services
                O_Orders orderItem = db.O_Orders.FirstOrDefault(order => order.OrdersCode == info.OrdersCode);
                if (orderItem == null) return;

                var _oSubscriptions = db.Order_Subcription.Where(c => c.OrderCode == orderItem.OrdersCode && c.Period == "MONTHLY" && c.ApplyPaidDate != true).ToList();
                foreach (var sub in _oSubscriptions)
                {
                    var ser = db.Store_Services.FirstOrDefault(c => c.OrderCode == sub.OrderCode && c.ProductCode == sub.Product_Code);
                    if (ser == null) continue;
                    sub.StartDate = DateTime.UtcNow.Date;
                    var license = db.License_Product.AsNoTracking().FirstOrDefault(x => x.Id == sub.ProductId);
                    var enddate = DateTime.UtcNow.Date;
                    if (string.IsNullOrEmpty(sub.PriceType))
                    {
                        sub.PriceType = Store_Apply_Status.Real.Text();
                    }
                    if(sub.PriceType.Split(',').Contains(Store_Apply_Status.Trial.Text()))
                    {
                        enddate = enddate.AddMonths(license.Trial_Months??1);
                    }
                    if (sub.PriceType.Split(',').Contains(Store_Apply_Status.Promotional.Text()))
                    {
                        enddate = enddate.AddMonths(license.Promotion_Apply_Months??1);
                    }
                    if(sub.PriceType.Split(',').Contains(Store_Apply_Status.Real.Text()))
                    {
                        int license_qty = (sub.SubscriptionQuantity ?? 1) * (sub.Quantity ?? 1);
                        if (sub.PeriodRecurring == "Yearly") enddate = enddate.AddYears(license_qty);
                        else if (sub.PeriodRecurring == "Weekly") enddate = enddate.AddDays(license_qty * 7);
                        else enddate = enddate.AddMonths(license_qty);   
                    }

                    if (sub.PreparingDate > 0)
                    {
                        enddate = enddate.AddDays(sub.PreparingDate??0);
                    }
                    sub.EndDate = enddate;
                    ser.EffectiveDate = sub.StartDate;
                    ser.RenewDate = enddate;
                    db.Entry(sub).State = EntityState.Modified;
                    db.Entry(ser).State = EntityState.Modified;
                }
                var closeOrder = true;
                if (!db.Order_Products.Any(p => p.OrderCode == orderItem.OrdersCode) && closeOrder && db.SystemConfigurations.FirstOrDefault()?.AutoActiveRecurringLicense == true)
                {
                    orderItem.Status = InvoiceStatus.Closed.ToString();
                }
                else
                {
                    orderItem.Status = InvoiceStatus.Paid_Wait.ToString();
                }
                orderItem.PaymentMethod = info.PaymentMethod;
                orderItem.PaymentNote = info.PaymentNote;
                db.Entry(orderItem).State = EntityState.Modified;
                await db.SaveChangesAsync();

                //await AddTransactionToFinanceTicket(info, trans, card);

                if (db.Store_Services.Any(st => st.OrderCode == orderItem.OrdersCode) == false)
                    await new StoreServices(db).AddStoreLicense(orderItem.OrdersCode);
            }
            catch (Exception ex)
            {
                PaymentExceptionLog(ex);
            }
        }

        //public async Task AddTransactionToFinanceTicket(TransRequest info, C_CustomerTransaction trans, C_CustomerCard card, bool isSuccess = true)
        //{
        //    using (var db = new WebDataModel())
        //    {
        //        O_Orders orderItem = db.O_Orders.FirstOrDefault(order => order.OrdersCode == info.OrdersCode);
        //        //Create Finance ticket if null
        //        //var ticket = db.T_SupportTicket.FirstOrDefault(t => t.OrderCode == orderItem.OrdersCode && t.TypeId == (long)UserContent.TICKET_TYPE.Finance);
        //        //if (ticket == null)
        //        //{
        //        //    if (orderItem.GrandTotal > 0)
        //        //    {
        //        //        string linkViewInvoiceFull = ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + orderItem.OrdersCode + "&flag=Invoices";
        //        //        await TicketViewController.AutoTicketScenario.NewTicketFinance(orderItem.InvoiceNumber.ToString(), "<iframe  width='850' height='900' src='" + linkViewInvoiceFull + "'></iframe>");
        //        //    }
        //        //}

        //        //// Close Finance ticket
        //        //ticket = db.T_SupportTicket.FirstOrDefault(t => t.OrderCode == orderItem.OrdersCode && t.TypeId == (long)UserContent.TICKET_TYPE.Finance);
        //        //if (ticket != null)
        //        //{
        //        //    ticket.StatusId = (long)UserContent.TICKET_STATUS.Finance_Complete;
        //        //    ticket.StatusName = "Complete";
        //        //    ticket.DateClosed = DateTime.UtcNow;
        //        //    db.Entry(ticket).State = EntityState.Modified;

        //        //    // Add log history sales
        //        //    var sale = db.C_SalesLead.FirstOrDefault(c => c.CustomerCode.Equals(trans.CustomerCode));
        //        //    SalesLeadService _registerMangoService = new SalesLeadService();
        //        //    if (sale != null)
        //        //    {
        //        //        //var host = ConfigurationManager.AppSettings["IMSUrl"] + "/order/estimatesdetail/" + orderItem.Id;
        //        //        string titleLog = isSuccess ? "Invoice <b>#" + trans.OrdersCode + "</b> has been paid" : "<b style='color:red'>Payment error</b>";
        //        //        string description = $"Invoice: <a onclick='show_invoice(" + trans.OrdersCode + ")'>#" + trans.OrdersCode + "</a><br/>" +
        //        //                            $"Payment at: {trans.CreateAt?.ToString("MMM dd, yyyy hh:mm tt")} <br/>" +
        //        //                            (card == null ? "" : $"Card Number: {card?.CardType} {card?.CardNumber} <br/>" +
        //        //                            $"Card Holder: {card?.CardHolderName} <br/>");
        //        //        if (!string.IsNullOrEmpty(info?.PaymentMethod))
        //        //        {
        //        //            description += $"Payment method: {info.PaymentMethod} <br/>" +
        //        //                $"Bank name: {trans.BankName} <br/>" +
        //        //                $"Card number (4 last digits): {trans.CardNumber} <br/>" +
        //        //                $"Payment note: {info.PaymentNote}";
        //        //        }
        //        //        _registerMangoService.CreateLog(sale.Id, sale.CustomerName, titleLog, description, "IMS System", cMem?.MemberNumber, trans.CreateAt);
        //        //    }
        //        //    // Add feedback
        //        //    var fb = new T_TicketFeedback
        //        //    {
        //        //        CreateAt = DateTime.UtcNow,
        //        //        CreateByName = string.IsNullOrEmpty(cMem?.FullName) == true ? "IMS System" : cMem?.FullName,
        //        //        FeedbackTitle = isSuccess ? "Payment Cleared" : "<b style='color:red'>Payment error</b>",
        //        //        Feedback = $"Payment at: {trans.CreateAt?.ToString("MMM dd, yyyy hh:mm tt")} <br/>" +
        //        //                   (card == null ? "" : $"Card Number: {card?.CardType} {card?.CardNumber} <br/>" +
        //        //                   $"Card Holder: {card?.CardHolderName} <br/>"),
        //        //        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
        //        //        DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
        //        //        GlobalStatus = "private",
        //        //        TicketId = ticket.Id
        //        //    };
        //        //    if (!string.IsNullOrEmpty(info?.PaymentMethod))
        //        //    {
        //        //        fb.Feedback += $"Payment method: {info.PaymentMethod} <br/>" +
        //        //            $"Bank name: {trans.BankName} <br/>" +
        //        //            $"Card number (4 last digits): {trans.CardNumber} <br/>" +
        //        //            $"Payment note: {info.PaymentNote}";
        //        //    }
        //        //    db.T_TicketFeedback.Add(fb);
        //        //    await db.SaveChangesAsync();
        //        //    await TicketViewController.AutoTicketScenario.UpdateSatellite(ticket.Id);
        //        //}
        //    }
        //}

        public async Task<string> SendEmail_ReceiptPayment(string Order_code, string payment_type = "",bool IsRecurring = false)
        {
            using (var db = new WebDataModel())
            {
                var order = db.O_Orders.FirstOrDefault(o => o.OrdersCode == Order_code);
                if (order == null)
                    throw new Exception("Order not found");

                var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == order.CustomerCode);

                var email_data = new ReceiptPayment_MailData { };
             
                if (payment_type == "ACH")
                {
                    var trans = db.C_CustomerTransaction.FirstOrDefault(c => c.OrdersCode == order.OrdersCode);
                   
                    if (trans != null)
                    {
                        email_data = new ReceiptPayment_MailData
                        {
                            short_description = IsRecurring ? "A recurring payment was posted to the account (s) listed below" : $"You have successfully made a payment of ${trans.Amount.ToString("#,##0.#0")}",
                            salon_name = order.CustomerName,
                            grand_total = (trans.Amount).ToString("#,##0.#0"),
                            grand_date_paid = trans.CreateAt.HasValue ? trans.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                            receipt = new List<receipt_data>{new receipt_data
                            {
                                salon = $"{cus.BusinessName} (#{cus.StoreCode})",
                                card = $"ACH - {cus.DepositAccountNumber?.Substring((cus.DepositAccountNumber?.Length ?? 4) - 4)}",
                                //name = trans.card.CardHolderName,
                                order_code = "#" + trans.OrdersCode,
                                date_paid = trans.CreateAt.HasValue ? trans.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                                total = trans.Amount.ToString("#,##0.#0"),
                            }}
                        };
                    }
                }
                else
                {
                    var trans = (
                        from trs in db.C_CustomerTransaction
                        where trs.OrdersCode == order.OrdersCode
                        join card in db.C_CustomerCard on trs.Card equals card.Id
                        select new { trs, card }
                        ).FirstOrDefault();
                    if (trans != null)
                    {
                        email_data = new ReceiptPayment_MailData
                        {
                            short_description = IsRecurring ? "A recurring payment was posted to the account (s) listed below" : $"You have successfully made a payment of ${trans.trs.Amount.ToString("#,##0.#0")}",
                            salon_name = order.CustomerName,
                            grand_total = (trans.trs.Amount).ToString("#,##0.#0"),
                            grand_date_paid = trans.trs.CreateAt.HasValue ? trans.trs.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                            receipt = new List<receipt_data>{new receipt_data
                            {
                                salon = $"{cus.BusinessName} (#{cus.StoreCode})",
                                card = $"{trans.card.CardType} - {trans.card.CardNumber.Substring(4,6)}",
                                name = trans.card.CardHolderName,
                                order_code = "#" + trans.trs.OrdersCode,
                                date_paid = trans.trs.CreateAt.HasValue ? trans.trs.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                                total = trans.trs.Amount.ToString("#,##0.#0"),
                            }}
                        };
                    }
                    else
                    {
                        var p_trans = (
                        from trs in db.C_CustomerTransaction
                        where trs.OrdersCode == order.OrdersCode
                        join card in db.C_PartnerCard on trs.Card equals card.Id
                        select new { trs, card }
                        ).FirstOrDefault();
                        email_data = new ReceiptPayment_MailData
                        {
                            short_description = IsRecurring ? "A recurring payment was posted to the account (s) listed below" : $"You have successfully made a payment of ${p_trans.trs.Amount.ToString("#,##0.#0")}",
                            salon_name = order.CustomerName,
                            grand_total = (p_trans.trs.Amount).ToString("#,##0.#0"),
                            grand_date_paid = p_trans.trs.CreateAt.HasValue ? p_trans.trs.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                            receipt = new List<receipt_data>{new receipt_data
                            {
                                salon = $"{cus.BusinessName} (#{cus.StoreCode})",
                                card = $"{p_trans.card.CardType} - {p_trans.card.CardNumber.Substring(4,6)}",
                                name = p_trans.card.CardHolderName,
                                order_code = "#" + p_trans.trs.OrdersCode,
                                date_paid = p_trans.trs.CreateAt.HasValue ? p_trans.trs.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                                total = p_trans.trs.Amount.ToString("#,##0.#0"),
                            }}
                        };
                    }
                }

                string to = string.Join(";", cus.Email ?? cus.SalonEmail);
                string firstname = string.Join(";", cus.BusinessName);
                string cc = string.Join(";", cus.MangoEmail);
                var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                return await _mailingService.SendBySendGrid_ReceiptPayment(to, firstname, email_data, cc);
                //return await SendEmail.SendBySendGrid_ReceiptPayment(to, firstname, email_data, cc, db.SystemConfigurations.FirstOrDefault()?.BillingNotification);
                //return await SendEmail.SendBySendGrid_ReceiptPayment(CustomerService.Email(cus), cus.BusinessName, email_data);
            }
        }

        public async Task<string> SendEmail_Payment_ToBillingEmail(C_CustomerCard card, O_Orders order, C_CustomerTransaction transaction)
        {
            try
            {
                string msg = string.Empty;
                using (var db = new WebDataModel())
                {
                    var billingemail = db.SystemConfigurations.FirstOrDefault().BillingNotification;
                    var customer = db.C_Customer.FirstOrDefault(c => c.CustomerCode == order.CustomerCode) ?? new C_Customer();
                    string status = (transaction.PaymentStatus == "Approved" || transaction.PaymentStatus == "Success") ? "successful" : "failed";
                    string color = status == "successful" ? "green" : "red";
                    string subject = $"Invoice #{order.OrdersCode} payment has been {status}";
                    string content = $"Hello,<br/><br/>" +
                                     $"<h3 style='color:{color}'>Payment {status}</h3>" +
                                     $"<h4>Invoice information:</h4>" +
                                     $"<span style='margin-left: 25px;'>Invoice number: <a href='{AppConfig.Host}/order/estimatesdetail/{order.Id}'>#<b>{order.OrdersCode}</b></a></span><br/>" +
                                     $"<span style='margin-left: 25px;'>Amount total: ${order.GrandTotal}</span><br/>" +
                                     $"<span style='margin-left: 25px;'>Payment type: {transaction.PaymentNote}</span><br/>" +
                                     $"<span style='margin-left: 25px;'>Salon: <a href='{AppConfig.Host}/merchantman/detail/{customer.Id}'>{order.CustomerName} (#{order.CustomerCode})</a></span><br/>" +
                                     $"<span style='margin-left: 25px;'>Note transaction: {transaction.ResponseText}</span><br/>" +
                                     $"<h4>Card information:</h4>" +
                                     $"<span style='margin-left: 25px;'>Card holder: {card.CardHolderName}</span><br/>" +
                                     $"<span style='margin-left: 25px;'>Card number: {card.CardNumber}</span><br/>" +
                                     $"<span style='margin-left: 25px;'>Card type: {card.CardType}</span><br/>" +
                                     $"<br/>Thank you!";
                    var emailData = new { content = content, subject = subject };
                    var _mailingService = EngineContext.Current.Resolve<IMailingService>();
                    msg = await _mailingService.SendNotifyOutgoingWithTemplate(billingemail, "Billing admin", subject, "", emailData);
                }
                return msg;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
