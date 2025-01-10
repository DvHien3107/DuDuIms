using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{   
    public partial class OrderSubscription
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public long Id { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        public string ProductId { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Customer Code
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// subscription Price
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Period
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Purcharsed Day
        /// </summary>
        public System.DateTime? PurcharsedDay { get; set; }

        /// <summary>
        /// Number Of Item
        /// </summary>
        public int? NumberOfItem { get; set; }

        /// <summary>
        /// Auto Renew
        /// </summary>
        public bool? AutoRenew { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        public System.DateTime? StartDate { get; set; }

        /// <summary>
        /// Actived
        /// </summary>
        public bool? Actived { get; set; }

        /// <summary>
        /// Is Addon
        /// </summary>
        public bool? IsAddon { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        public System.DateTime? EndDate { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ProductCodePOSSystem { get; set; }

        /// <summary>
        /// Quantity
        /// If Period = ONETIME then quantity of subscription
        /// If Period = MONTHLY then number of period
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// Discount
        /// </summary>
        public decimal? Discount { get; set; }

        /// <summary>
        /// Discount Percent
        /// </summary>
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// Price Type (Trial, Promotional, Real)
        /// </summary>
        public string PriceType { get; set; }

        /// <summary>
        /// Promotion Apply Months
        /// </summary>
        public int? PromotionApplyMonths { get; set; }

        /// <summary>
        /// Subscription Type (license, addon, giftcard, other)
        /// </summary>
        public string SubscriptionType { get; set; }

        /// <summary>
        /// Apply Discount As Recurring
        /// </summary>
        public bool? ApplyDiscountAsRecurring { get; set; }

        /// <summary>
        /// Period Recurring
        /// </summary>
        public string PeriodRecurring { get; set; }

        /// <summary>
        /// Subscription Quantity
        /// If Period = ONETIME then SubscriptionQuantity = Quantity
        /// If Period = MONTHLY then quantity of subscription
        /// </summary>
        public int? SubscriptionQuantity { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Apply Paid Date
        /// This is same custom date
        /// If = true then StartDate = picked date
        /// If = false then StartDate = paid date
        /// </summary>
        public bool? ApplyPaidDate { get; set; }

        /// <summary>
        /// Recurring Price
        /// </summary>
        public decimal? RecurringPrice { get; set; }
    }
}
