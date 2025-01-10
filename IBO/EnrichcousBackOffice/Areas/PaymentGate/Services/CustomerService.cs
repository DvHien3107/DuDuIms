using System;
using System.Linq;
using System.Web;
using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Areas.PaymentGate.Services
{
    public class CustomerService
    {
        public C_Customer Login(string email, string password, HttpSessionStateBase Session)
        {
            try
            {
                var md5Pass = SecurityLibrary.Md5Encrypt(password);
                var db = new WebDataModel();
                var cus = db.C_Customer.FirstOrDefault(c => ((c.SalonEmail ?? c.Email) ?? "").ToLower().Equals(email.ToLower()) && c.MD5PassWord.Equals(md5Pass) && c.Active == 1);
                //var cv = db.C_Customer.Find(200728081620389);
                //if(((cv.SalonEmail ?? cv.Email) ?? "").ToLower().Equals(email.ToLower()) && cv.MD5PassWord.Equals(md5Pass) && cv.Active == 1)
                //{
                //    string a = "";
                //}
                if (cus == null)
                {
                    throw new AppHandleException("Sorry, your email or password is incorrect!");
                }
                StoreSessionLogin($"{AreaPayConst.AUTH_BASIC_PREFIX} {(cus.Id + ":" + email).ToBase64()}");
                HttpContext.Current.Session["StoreName"] = cus.BusinessName;
                HttpContext.Current.Session["StoreCode"] = cus.CustomerCode;
                return cus;
            }
            catch (Exception e)
            {
                var msg = "Sorry, An error has occurred! Please try again later!";
                if (e is AppHandleException)
                {
                    msg = e.Message;
                }
                throw new AppHandleException(msg);
            }
        }
        
        public static void StoreSessionLogin(string authInfo)
        {
            if (string.IsNullOrEmpty(authInfo)) return;
            HttpContext.Current.Session.Timeout = 60;
            HttpContext.Current.Session[AreaPayConst.AUTH_BASIC_KEY] = authInfo;
        }

        public static string Email(C_Customer cus)
        {
            return (string.IsNullOrEmpty(cus.SalonEmail) ? cus.SalonEmail : cus.BusinessEmail) ?? cus.Email;
        }
    }
}