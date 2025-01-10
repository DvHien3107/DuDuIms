using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Sms
{
    public class SendSmsRequest 
    {
        public string Sender { get; set; }

        public string Receiver { get; set; }

        public string Text { get; set; }

        public List<Uri> MediaUrl { get; set; }

    }
}
