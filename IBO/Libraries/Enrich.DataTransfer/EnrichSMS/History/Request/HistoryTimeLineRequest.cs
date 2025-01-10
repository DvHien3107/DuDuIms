using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.EnrichSMSService
{
    public class HistoryTimeLineRequest : BaseSearchWithFilterRequest<HistorySearchCondition, HistoryFilterCondition>
    {
        public HistoryTimeLineRequest()
        {
            Condition = new HistorySearchCondition();
        }
    }
}
