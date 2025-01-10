using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace Enrich.Payment.MxMerchant.Models
{
    public class ConfigFactory
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string MerchantId { get; set; }
        public string Host { get; set; }
        public string[] EndPointRequestToken { get => new string[] { Host + "/checkout/v3/oauth/1a/requesttoken", "POST" }; }
        public string[] EndPointAccessToken { get => new string[] { Host + "/checkout/v3/oauth/1a/accesstoken", "POST" }; }
        public string[] CreatePayment { get => new string[] { Host + "/checkout/v3/payment", "POST" }; }
        public string[] CreateCard { get => new string[] { Host + "/checkout/v3/customercardaccount", "POST" }; }
        public string[] CreateCustomer { get => new string[] { Host + "/checkout/v3/customer", "POST" }; }
        public string[] CreateRecurring { get => new string[] { Host + "/checkout/v3/contractsubscription", "POST" }; }
        public string[] CheckRecurring { get => new string[] { Host + "/checkout/v3/contract/payment", "GET" }; }
        public string[] FullRefund { get => new string[] { Host + "/checkout/v3/payment", "DELETE" }; }
    }
}
