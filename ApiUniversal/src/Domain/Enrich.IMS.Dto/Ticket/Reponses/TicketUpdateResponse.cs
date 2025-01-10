using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Ticket
{
    public class TicketUpdateResponse
    {
        public string CreatedId { get; set; }

        public TicketDto SalesLead { get; set; }
    }
}
