using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public partial class RecurringPlanningDto
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string SubscriptionCode { get; set; }
        public string OrderCode { get; set; }
        public decimal? Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? DiscountPercent { get; set; }
        public bool? ApplyDiscountAsRecurring { get; set; }
        public string SubscriptionType { get; set; }
        public int? SubscriptionQuantity { get; set; }
        public string UnitsOfTime { get; set; }
        public int? Period { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedBy { get; set; }
        public int? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? RecurringDate { get; set; }
        public bool? RecurringWithLicense { get; set; }
        public decimal? TotalRecurringPrice => Price * SubscriptionQuantity - Discount;
    }
}
