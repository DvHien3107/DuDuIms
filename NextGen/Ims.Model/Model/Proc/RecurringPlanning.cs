using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Proc
{
    public partial class RecurringPlanning
    {
        public string CustomerCode { get; set; }
        public string SubscriptionCode { get; set; }
        public string OrderCode { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> Discount { get; set; }
        public Nullable<decimal> DiscountPercent { get; set; }
        public Nullable<bool> ApplyDiscountAsRecurring { get; set; }
        public string SubscriptionType { get; set; }
        public Nullable<int> SubscriptionQuantity { get; set; }
        public string UnitsOfTime { get; set; }
        public Nullable<int> Period { get; set; }
        public Nullable<System.DateTime> CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> LastUpdatedAt { get; set; }
        public string LastUpdatedBy { get; set; }
        public Nullable<int> Status { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }
        public Nullable<System.DateTime> RenewDate { get; set; }
        public Nullable<bool> RecurringWithLicense { get; set; }
        public Nullable<decimal> TotalRecurringPrice { get; set; }
        public string RecurringType { get; set; }
    }
}
