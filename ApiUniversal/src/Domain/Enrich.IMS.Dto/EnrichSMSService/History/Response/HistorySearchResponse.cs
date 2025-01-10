using Enrich.Dto.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.EnrichSMSService
{
    public class HistorySearchResponse : PagingResponseDto<HistoryListItemDto>
    {
        public HistorySearchSummary Summary { get; set; } = new HistorySearchSummary();
    }
}
