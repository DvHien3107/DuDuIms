using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Ticket
{
    public partial class TicketDto
    {
        public TicketStatusDto Status { get; set; }
        public IEnumerable<TicketTypeDto> Types { get; set; }
        public IEnumerable<TicketAttachmentFileDto> Attachments { get; set; }
    }
}
