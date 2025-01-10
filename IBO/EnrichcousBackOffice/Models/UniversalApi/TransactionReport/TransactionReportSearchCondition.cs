using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.TransactionReport
{
    public class TransactionReportSearchCondition
    {
        public List<string> SubscriptionCodes { get; set; }
        public List<string> SubscriptionTypes { get; set; }
        public List<string> Hardware { get; set; }
        public List<string> OrderCodes { get; set; }
        public List<string> Customers { get; set; }
        public List<string> OrderStatus { get; set; }
        public List<string> PaymentMethod { get; set; }
        public List<string> Partners { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SearchText { get; set; }
    }
}