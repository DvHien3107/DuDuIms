using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Ticket
{
    public class TicketUpdateOption : LoadOrUpdateBaseOption
    {
        public new static TicketUpdateOption Default => new TicketUpdateOption
        {
        };
    }
}
