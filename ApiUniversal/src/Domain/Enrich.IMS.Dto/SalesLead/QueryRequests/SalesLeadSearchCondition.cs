using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.SalesLead
{
    public partial class SalesLeadSearchCondition : BaseSearchCondition
    {
        /// custom for sale lead dashboard
        public List<string> SaleNumbers { get; set; }
        public List<string> TeamNumbers { get; set; }
        public List<string> Types { get; set; }
        public List<string> Status { get; set; }
        public string SearchText { get; set; }

        public bool PopulateCountSummaries { get; set; }
        
        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}
