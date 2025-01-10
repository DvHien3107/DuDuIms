using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class Order_Subcription
    {
        public long Id { get; set; }

        public string StoreCode { get; set; }

        public string OrderCode { get; set; }

        public string ProductId { get; set; }

        public string ProductName { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal? Price { get; set; }

        public string Period { get; set; }

        public DateTime? PurcharsedDay { get; set; }

        public int? NumberOfItem { get; set; }

        public bool? AutoRenew { get; set; }

        public DateTime? StartDate { get; set; }

        public bool? Actived { get; set; }

        public bool? IsAddon { get; set; }

        public DateTime? EndDate { get; set; }

        public string Product_Code { get; set; }

        public string Product_Code_POSSystem { get; set; }

        public int? Quantity { get; set; }

        public decimal? Discount { get; set; }

        public decimal? DiscountPercent { get; set; }

        public string PriceType { get; set; }

        public int? Promotion_Apply_Months { get; set; }

        public string SubscriptionType { get; set; }

        public bool? ApplyDiscountAsRecurring { get; set; }

        public string PeriodRecurring { get; set; }

        public int? SubscriptionQuantity { get; set; }

        public decimal? Amount { get; set; }

        public bool? ApplyPaidDate { get; set; }

        public decimal? RecurringPrice { get; set; }

        public int? PreparingDate { get; set; }

    }

}
