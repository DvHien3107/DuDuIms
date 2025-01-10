using Enrich.Dto.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Ticket
{
    public class TicketUpdateRequest : BaseSaveByJsonPatchRequest<TicketDto>
    {
        /// <summary>
        /// use to verify a salesLead (unverify to verify)
        /// </summary>
        public bool IsVerify { get; set; } = false;
        public TicketUpdateOption UpdateOption { get; set; }

        public TicketUpdateRequest(TicketDto dto, TicketDto oldDto = null) : this()
        {
            NewDto = dto;
            OldDto = oldDto;
        }

        public TicketUpdateRequest()
        {
        }
    }
}
