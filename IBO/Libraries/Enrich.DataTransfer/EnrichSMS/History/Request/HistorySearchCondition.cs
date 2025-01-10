using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.EnrichSMSService
{
    public partial class HistorySearchCondition
    {
        public List<string> StoreCodes { get; set; }

        public DateTime FromDate { get; set; } = new DateTime(2010, 1, 1);

        public DateTime ToDate { get; set; } = new DateTime(2110, 1, 1);

        public List<int> HistoryTypes { get; set; }

        public List<int> ExcludeHistoryTypes { get; set; }

        public string GroupBy { get; set; }

        public string SearchText { get; set; }
    }
}
