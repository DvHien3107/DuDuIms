using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SendEmail
{
    public class SendEmailBySendGridTemplateId:BaseSendEmailRequest
    {
        public string TemplateId { get; set; }
        public object Data { get; set; }
    }
}
