using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SendEmail
{
    public class BaseSendEmailRequest
    {
        public string Subject { get; set; }
        public string StoreCode { get; set; }
        public string Service { get; set; }
        public EmailAddress From { get; set; }
        public IEnumerable<EmailAddress> To { get; set; }
        public IEnumerable<EmailAddress> Cc { get; set; }
        public IEnumerable<EmailAddress> Bcc { get; set; }
        public EmailAddress ReplyTo { get; set; }
        public string Category { get; set; }
        public EmailAttachments FileAttachment { get; set; }
    }
}
