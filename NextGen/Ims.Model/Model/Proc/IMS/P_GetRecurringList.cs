using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Proc.IMS
{
    public class P_GetRecurringList
    {
        public int ID { get; set; }
        public string CustomerCode { get; set; }
        public string SubscriptionCode { get; set; }
        public string OrderCode { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public bool? ApplyDiscountAsRecurring { get; set; }
        public string SubscriptionType { get; set; }
        public int? SubscriptionQuantity { get; set; }
        public string PeriodRecurring { get; set; }
        public string Period { get; set; }
        public int? Active { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateOnly? RenewDate { get; set; }
        public decimal? RecurringPrice { get; set; }
        public string RecurringType { get; set; }
        
    }
}
