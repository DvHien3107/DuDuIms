using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.CustomerTransaction
{
    public partial class CustomerTransactionSearchCondition : BaseSearchCondition
    {
        /// custom for sale lead dashboard
        public List<string> Subscriptions { get; set; }
        public List<string> Orders { get; set; }
        public List<string> Customers { get; set; }
        public List<string> Status { get; set; }
        public string SearchText { get; set; }

        public bool PopulateCountSummaries { get; set; }
        
        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}
