using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.Twilio
{
    public class PosUpdateTwilioPhoneNumberRequest
    {
        public string StoreId { get; set; }
        public string AccountSId { get; set; }
        public string AuthToken { get; set; }
        public string Phone { get; set; }
        public string Type { get; set; }

    }
}
