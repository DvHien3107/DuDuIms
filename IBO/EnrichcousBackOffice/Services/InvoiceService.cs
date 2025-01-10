using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Enrich.Core.Infrastructure;
using Enrich.Core.Ultils;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Enrich.Web.Framework;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Services.NextGen.Mail;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.ViewModel;
using Inner.Libs.Helpful;
using Newtonsoft.Json;

namespace EnrichcousBackOffice.Services
{
    public class InvoiceService : IServicesBase
    {
        private IEmailService emailService = EngineContext.Current.Resolve<IEmailService>();
        #region Constructor
        public InvoiceService() : base()
        {
        }
        public InvoiceService(bool trans = false) : base(trans)
        {
        }
        public InvoiceService(WebDataModel db = null) : base(db)
        {
        }
        #endregion

        protected string Display(decimal? num)
        {
            if (num == null || num == 0)
            {
                return null;
            }
            return num?.ToString("#,##0.#0") ?? null;
        }
        protected string DisplayDefault(decimal? num)
        {
            if (num == null || num == 0)
            {
                return "0";
            }
            return num?.ToString("#,##0.#0") ?? "0";
        }

        private P_Member cMem = Authority.GetCurrentMember();

        public async Task<string> SendMailConfirmPayment(C_Customer cus, O_Orders order, bool isResend = false, bool immediate = false)
        {
            string urlPaymentLink = GetPaymentLink(order.OrdersCode, cus.MD5PassWord);
            await emailService.sendEmailInvoice(cus, order, urlPaymentLink);

            return "sucess";
        }

        public async Task<string> SendSMSconfirmPayment(C_Customer cus, O_Orders order)
        {
            var contacts = DB.C_CustomerContact.Where(c => c.CustomerId == cus.Id &&
                                                            !string.IsNullOrEmpty(c.PhoneNumber) &&
                                                            (c.Authorization.Contains("Owner") || c.Authorization.Contains("Billing Inquiries"))).Select(c => new { PhoneNumber = c.PhoneNumber, Name = c.Name });

            string url = GetPaymentLink(order.OrdersCode, cus.MD5PassWord);
            string phone = string.Join(", ", cus.OwnerMobile ?? cus.CellPhone, string.Join(", ", contacts.Select(c => c.PhoneNumber)));
            string name = string.Join(", ", cus.OwnerName ?? cus.ContactName, string.Join(", ", contacts.Select(c => c.Name)));
            var _smsService = EngineContext.Current.Resolve<ISMSService>();
            string result = await _smsService.NotifyPaymentLink(phone, name, url);
            return result;
        }

        public string GetPaymentLink(string OrdersCode, string MD5PassWord, string PartnerCode = "")
        {
            try
            {
                string pc = string.IsNullOrEmpty(PartnerCode) ? "" : "&pc=" + SecurityLibrary.Encrypt(PartnerCode);
                return $"{AppConfig.Host}/PaymentGate/Pay/?key={OrdersCode.ToBase64()}:{MD5PassWord.ToBase64()}{pc}";
            }
            catch (Exception ex)
            {
                return "Get link error. " + ex.Message;
            }
        }
    }
}