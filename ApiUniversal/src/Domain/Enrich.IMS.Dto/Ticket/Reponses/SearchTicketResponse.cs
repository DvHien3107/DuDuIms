using Enrich.Dto.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Ticket.Reponses
{
    public class SearchTicketResponse : PagingResponseDto<TicketListItemDto>
    {
        public SearchTicketSummaryDto Summary { get; set; } = new SearchTicketSummaryDto();
    }
}
