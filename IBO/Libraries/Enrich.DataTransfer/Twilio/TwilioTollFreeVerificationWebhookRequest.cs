using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.Twilio
{
    public class TwilioTollFreeVerificationWebhookRequest
    {
        public TwilioTollFreeVerificationWebhookData data { get; set; }
        public string datacontenttype { get; set; }
        public string dataschema { get; set; }
        public string id { get; set; }
        public string source { get; set; }
        public string specversion { get; set; }
        public DateTime time { get; set; }
        public string type { get; set; }
    }
    public class TwilioTollFreeVerificationWebhookData
    {
        public string accountsid { get; set; }
        public string customerprofilesid { get; set; }
        public int errorcode { get; set; }
        public string errordescription { get; set; }
        public string parentaccountsid { get; set; }
        public string phonenumbersid { get; set; }
        public long processeddate { get; set; }
        public string regulateditemsid { get; set; }
        public string tollfreeverificationsid { get; set; }
        public string trustproductsid { get; set; }
        public string verificationstatus { get; set; }
    }
}
