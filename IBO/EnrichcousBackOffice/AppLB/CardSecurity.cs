using Enrich.Core.Ultils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.AppLB
{
    public class CardSecurity
    {
        //enrich@2019
        private static string default_pass = System.Configuration.ConfigurationManager.AppSettings["PassCode"];

        public static String EncryptionCardNumber(string cardnumber, string timestamp)
        {
            try
            {
                if (string.IsNullOrEmpty(cardnumber) || string.IsNullOrEmpty(timestamp)) throw new Exception();
                var customkey = "yoe315qf4w";
                //genarate 1fa
                string sr_pass_1fa = SecurityLibrary.Md5Encrypt(default_pass + customkey);
                //encode to hash 1fa
                string sr_hash = SecurityLibrary.Encrypt(cardnumber, sr_pass_1fa);
                //genarate 2fa
                string sr_pass_2fa = SecurityLibrary.Md5Encrypt(customkey + default_pass + timestamp);
                //encode to final hash
                string fhash = SecurityLibrary.Encrypt(sr_hash, sr_pass_2fa);
                return fhash;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static String DecryptionCardNumber(string hash, string timestamp, string customkey)
        {
            try
            {
                if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(timestamp)) throw new Exception();
                //genarate 2fa
                string sr_pass_2fa = SecurityLibrary.Md5Encrypt(customkey + default_pass + timestamp);
                //decode final hash
                string sr_hash = SecurityLibrary.Decrypt(hash, sr_pass_2fa);
                //genarate 1fa
                string sr_pass_1fa = SecurityLibrary.Md5Encrypt(default_pass + customkey);
                //decode hash 1fa to card number
                string carnumber = SecurityLibrary.Decrypt(sr_hash, sr_pass_1fa);
                return carnumber;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}