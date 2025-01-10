using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.OrderSubscription
{
    public partial class SubscriptionPackageSearchCondition : BaseSearchCondition
    {
        /// custom for sale lead dashboard
        public List<string> StoreCodes { get; set; }
        public string Package { get; set; }
        public DateTime FromDate { get; set; } = new DateTime(2000, 1, 1);
        public DateTime ToDate { get; set; } = new DateTime(3000, 1, 1);
        public string SearchText { get; set; }

        public bool PopulateCountSummaries { get; set; } = true;
        
        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}
