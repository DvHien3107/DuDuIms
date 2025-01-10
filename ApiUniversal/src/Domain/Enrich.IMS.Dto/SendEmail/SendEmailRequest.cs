using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SendEmail
{
    public class SendEmailRequest: BaseSendEmailRequest
    {
        public string Body { get; set; }

    }
}
