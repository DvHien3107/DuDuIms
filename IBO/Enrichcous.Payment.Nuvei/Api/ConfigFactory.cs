
namespace Card_Account_Transactions
{
    public class ConfigFactory
    {

        public string consumerKey;
        public string consumerSecret;
        public string merchantId;
        public string baseURL;

        public static string[] endPointRequestToken = { "https://sandbox.api.mxmerchant.com/checkout/v3/oauth/1a/requesttoken", "POST" };
        public static string[] endPointAccessToken = { "https://sandbox.api.mxmerchant.com/checkout/v3/oauth/1a/accesstoken", "POST" };
        public static string[] payment = { "https://sandbox.api.mxmerchant.com/checkout/v3/payment", "POST" };
        
        public ConfigFactory()
        {
            this.consumerKey = getConsumerKey();
            this.consumerSecret = getConsumerSecret();
            this.merchantId = getMerchantId();
        }

        private string getConsumerKey()
        {
            return "";
        }

        private string getConsumerSecret() 
        {
            return "";
        }

        private string getMerchantId()
        {
            string temp = "";
            return temp;
        }
    }
}
