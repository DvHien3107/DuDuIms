using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public partial class TicketSearchCondition : BaseSearchCondition
    {
        public string Name { get; set; }

        public string Comment { get; set; }

        public int[] MerchantIds { get; set; }

        public int[] PersonIds { get; set; }     

        public DateTime? StartDateFrom { get; set; }

        public DateTime? StartDateTo { get; set; }

        public DateTime? EndDateFrom { get; set; }

        public DateTime? EndDateTo { get; set; }

        public DateTime? CreatedDateFrom { get; set; }

        public DateTime? CreatedDateTo { get; set; }

        public int[] CreatedUserIds { get; set; }

        public DateTime? ModifiedDateFrom { get; set; }

        public DateTime? ModifiedDateTo { get; set; }

        public int[] ModifiedUserIds { get; set; }

        public bool PopulateCountSummaries { get; set; }

        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}
