using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.OrderSubscription
{
    public partial class OrderSubscriptionSearchCondition : BaseSearchCondition
    {
        /// custom for sale lead dashboard
        public List<string> SubscriptionCodes { get; set; }
        public List<string> SubscriptionTypes { get; set; }
        public List<string> Hardware { get; set; }
        public List<string> Partners { get; set; }
        public List<string> OrderCodes { get; set; }
        public List<string> Customers { get; set; }
        public List<string> OrderStatus { get; set; }
        public List<string> PaymentMethod { get; set; }
        public List<string> ProductId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchText { get; set; }

        public bool PopulateCountSummaries { get; set; } = true;
        
        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}
