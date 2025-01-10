using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Utils;
using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Models
{
    public partial class C_Customer
    {
        /// <summary>
        /// SalonEmail => BusinessEmail => Contact Email
        /// </summary>
        /// <returns></returns>
        public string DisplayEmail()
        {
            return SalonEmail ?? BusinessEmail ?? Email;
        }

        public string AddressLine()
        {
            var address = SalonAddressLine();
            if (string.IsNullOrEmpty(address)) address = BusinessAddressLine();
            if (string.IsNullOrEmpty(address)) address = DefaultsAddressLine();
            return address;
        }

        private string DefaultsAddressLine()
        {
            return string.Join(", ", new string[] {
                BusinessAddressStreet?.Replace(",", "") ?? " ", City?.Replace(",", "") ?? " ", State?.Replace(",", "") ?? " ", (Zipcode?.Replace(",", "") ?? " ") + " " + (Country?.Replace(",", "") ?? BusinessCountry?.Replace(",", "") ?? "United States")
            }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public string SalonAddressLine()
        {
            if (string.IsNullOrEmpty(SalonAddress1)) return "";
            return string.Join(", ", new string[] {
                SalonAddress1?.Replace(",", "") ?? " " + SalonAddress2?.Replace(",", "") ?? " ", SalonCity?.Replace(",", "") ?? " ", SalonState?.Replace(",", "") ?? " ", (SalonZipcode?.Replace(",", "") ?? " ") + " " + (BusinessCountry?.Replace(",", "") ?? "United States")
            }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public string BusinessAddressLine()
        {
            if (string.IsNullOrEmpty(BusinessAddressStreet)) return "";
            return string.Join(", ", new string[] {
                BusinessAddressStreet?.Replace(",", "") ?? " ", BusinessCity?.Replace(",", "") ?? " ", BusinessState?.Replace(",", "") ?? " ", (BusinessZipCode?.Replace(",", "") ?? " ") + " " + (BusinessCountry?.Replace(",", "") ?? "United States")
            }.Where(s => !string.IsNullOrEmpty(s)));
        }

        public string GetSalonAddress()
        {
            return SalonAddress1 ?? BusinessAddressStreet ?? OwnerAddressStreet ?? "";
        }

        public string GetSalonCity()
        {
            return SalonCity ?? BusinessCity ?? City ?? "";
        }

        public string GetSalonState()
        {
            return SalonState ?? BusinessState ?? State ?? "";
        }

        public string GetSalonZipCode()
        {
            return string.Format("{0} {1}", SalonZipcode ?? BusinessZipCode ?? Zipcode ?? "0", BusinessCountry ?? Country ?? "United States");
        }
    }

    public partial class C_Customer
    {
        public static C_Customer Login(string email, string password, HttpSessionStateBase Session)
        {
            try
            {
                using (var db = new WebDataModel())
                {
                    var md5Pass = SecurityLibrary.Md5Encrypt(password);
                    var cus = db.C_Customer.ToList().FirstOrDefault(c => c.DisplayEmail() == email && c.MD5PassWord == md5Pass);
                    if (cus == null)
                    {
                        throw new AppHandleException("Sorry, your email or password is incorrect!");
                    }
                    StoreSessionLogin($"{Constants.AUTH_BASIC_PREFIX} {(cus.Id + ":" + email).ToBase64()}");
                    return cus;
                }
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
            HttpContext.Current.Session[Constants.AUTH_BASIC_KEY] = authInfo;
        }

        /// <summary>
        /// Check Password
        /// </summary>
        public void CheckPassword()
        {
            using (var DB = new WebDataModel())
            {
                if (string.IsNullOrEmpty(Password))
                {
                    Password = SecurityLibrary.Md5Encrypt(DateTime.UtcNow.ToString("O")).Substring(0, 6);
                    MD5PassWord = SecurityLibrary.Md5Encrypt(Password);
                    DB.Entry(this).State = EntityState.Modified;
                }
                else if (string.IsNullOrEmpty(MD5PassWord))
                {
                    MD5PassWord = SecurityLibrary.Md5Encrypt(Password);
                    DB.Entry(this).State = EntityState.Modified;
                }
                if (string.IsNullOrEmpty(RequirePassChange))
                {
                    RequirePassChange = "off";
                    DB.Entry(this).State = EntityState.Modified;
                }
                if (DB.Entry(this).State == EntityState.Modified)
                {
                    try { DB.SaveChanges(); } catch (Exception e) { Console.WriteLine(e.Message); }
                }
            }
        }
    }
}