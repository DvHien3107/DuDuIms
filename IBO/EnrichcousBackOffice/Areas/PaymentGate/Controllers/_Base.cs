using System;
using System.Linq;
using System.Web.Mvc;
using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.Services;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Areas.PaymentGate.Controllers
{
    public abstract class Base : Controller
    {
        internal string PURCHASES_AGENT = "PURCHASES_AGENT";
        internal string PAYMENT_PURCHASES_HISTORY = "PAYMENT_PURCHASES_HISTORY";
        internal string PAYMENT_INVOICE_HISTORY = "PAYMENT_INVOICE_HISTORY";
        internal string PAYMENT_ERROR = "PAYMENT_ERROR";
        internal string PAYMENT_COLLECT_CARD_ERROR = "PAYMENT_COLLECT_CARD_ERROR";
        internal string PAYMENT_CARD_INFO = "PAYMENT_CARD_INFO";
        internal string CARD_ERROR = "CARD_ERROR";
        //
        internal string PAYMENT_INFO_MSG = "PAYMENT_INFO_MSG";
        internal string KEY_REF = "KEY_REF";
        internal string PC_PAY = "PC_PAY";
        public void BasicAuthClear()
        {
            Session[AreaPayConst.AUTH_BASIC_KEY] = "";
        }

        /// <summary>
        /// Auto detect customer
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string AuthParse(string key)
        {
            // Already Auth
            string authInfo = Session[AreaPayConst.AUTH_BASIC_KEY]?.ToString();
            if (string.IsNullOrEmpty(authInfo) == false) return authInfo;

            // Try generate
            var keyPair = (key ?? "").Split(':');
            var orderCode = keyPair[0].FromBase64();
            var md5Pass = keyPair.Length == 2 ? keyPair[1].FromBase64() : "";
            if (string.IsNullOrEmpty(md5Pass)) return authInfo;
            using (var db = new WebDataModel())
            {
                var orders = db.O_Orders.FirstOrDefault(o => o.OrdersCode == orderCode);
                if (orders != null)
                {
                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == orders.CustomerCode && c.MD5PassWord == md5Pass);
                    if (cus != null)
                        authInfo = $"{AreaPayConst.AUTH_BASIC_PREFIX} {(cus.Id + ":" + CustomerService.Email(cus)).ToBase64()}";
                }
            }
            return authInfo;
        }

        /// <summary>
        /// Get customer id from session login
        /// </summary>
        /// <returns></returns>
        /// <exception cref="AppHandleException"></exception>
        public long CustomerIdAuth()
        {
            string keyRef = Session[KEY_REF]?.ToString() ?? Request.Params["Key"] ?? Request.Params["key"];
            string authInfo = AuthParse(keyRef);
            if (!(authInfo != null && authInfo.StartsWith("Basic")))
            {
                throw new AppHandleException("Please login first");
            }
            CustomerService.StoreSessionLogin(authInfo);
            string credentials = authInfo.Substring("Basic ".Length).Trim();
            return long.Parse(credentials.FromBase64().Split(':')[0]);
        }

        /// <summary>
        /// Get customer info from session login
        /// </summary>
        /// <returns></returns>
        public C_Customer CustomerAuth()
        {
            using (var db = new WebDataModel()) { return db.C_Customer.Find(CustomerIdAuth()); }
        }
        /// <summary>
        /// Get partner info from session
        /// </summary>
        /// <returns></returns>
        public C_Partner PartnerAuth()
        {
            using (var db = new WebDataModel())
            {
                var pc = SecurityLibrary.Decrypt(Session[PC_PAY]?.ToString());
                return (db.C_Partner.FirstOrDefault(p => p.Code == pc) ?? new C_Partner { });
            }
        }

        /// <summary>
        /// Wrap exception msg
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="key"></param>
        public void MakeErrorResponse(Exception ex, string key)
        {
            Session[key] = Func.Error(MSG.ERROR_DEFAULT);
            Session[key] = ex.Message;
            if (ex != null)
            {
                Console.WriteLine(ex.StackTrace);
                if (ex is AppHandleException exception)
                {
                    Session[key] = exception.Message;
                }
            }
        }

        /// <summary>
        /// Try get msg from other request
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string SessionMsg(string key)
        {
            string _msg = Session[key] as string;
            if (string.IsNullOrEmpty(_msg) == false)
            {
                Session[key] = "";
                return _msg;
            }
            return null;
        }

        /// <summary>
        /// Anonymous Agent Order 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string AgentAction(string key)
        {
            if (string.IsNullOrEmpty(key)) goto NOT_AGENT;
            var decode = key.FromBase64();
            if (decode == key) goto NOT_AGENT;
            using (var db = new WebDataModel())
            {
                try
                {
                    Session[PURCHASES_AGENT] = key;
                    var cus = db.C_Customer.FirstOrDefault(c => c.StoreCode == decode);
                    return cus?.StoreCode;
                }
                catch
                {
                    // ignore
                }
            }
        NOT_AGENT:
            Session[PURCHASES_AGENT] = null;
            return null;
        }
    }
}