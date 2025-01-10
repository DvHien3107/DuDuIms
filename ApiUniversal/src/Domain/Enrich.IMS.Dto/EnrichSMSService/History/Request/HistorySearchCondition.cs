using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.EnrichSMSService
{
    public partial class HistorySearchCondition 
    {
        public List<string> StoreCodes { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public List<int> HistoryTypes { get; set; }

        public List<int> ExcludeHistoryTypes { get; set; }

        public string SearchText { get; set; }
    }
}
