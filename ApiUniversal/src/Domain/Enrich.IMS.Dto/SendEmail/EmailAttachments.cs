using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.SendEmail
{
    public class EmailAttachments
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
    }
}
