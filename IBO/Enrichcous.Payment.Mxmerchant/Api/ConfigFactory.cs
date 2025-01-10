
using System.Configuration;

namespace Enrichcous.Payment.Mxmerchant.Api
{
    public class ConfigFactory
    {

        public string consumerKey;
        public string consumerSecret;
        public string merchantId { get; }
        public string baseURL;
        public static string Host { get => ConfigurationManager.AppSettings.Get("MxMerchant_Host"); }
        public static string[] endPointRequestToken { get => new string[] { Host + "/checkout/v3/oauth/1a/requesttoken", "POST" }; }
        public static string[] endPointAccessToken { get => new string[] { Host + "/checkout/v3/oauth/1a/accesstoken", "POST" }; }
        public static string[] Create_payment { get => new string[] { Host + "/checkout/v3/payment", "POST" }; }
        public static string[] Create_card { get => new string[] { Host + "/checkout/v3/customercardaccount", "POST" }; }
        public static string[] Create_Customer { get => new string[] { Host + "/checkout/v3/customer", "POST" }; }
        public static string[] Create_Recurring { get => new string[] { Host + "/checkout/v3/contractsubscription", "POST" }; }
        public static string[] Check_Recurring { get => new string[] { Host + "/checkout/v3/contract/payment", "GET" }; }
        public static string[] Full_Refund { get => new string[] { Host + "/checkout/v3/payment", "DELETE" }; }
        public ConfigFactory()
        {
            this.consumerKey = getConsumerKey();
            this.consumerSecret = getConsumerSecret();
            this.merchantId = getMerchantId();
        }

        private static string getConsumerKey()
        {
            return ConfigurationManager.AppSettings.Get("MxMerchant_ConsumerKey");
        }

        private string getConsumerSecret()
        {
            return ConfigurationManager.AppSettings.Get("MxMerchant_ConsumerSecret");
        }

        private string getMerchantId()
        {
            return ConfigurationManager.AppSettings.Get("MxMerchant_MerchantId");
        }
    }
}
