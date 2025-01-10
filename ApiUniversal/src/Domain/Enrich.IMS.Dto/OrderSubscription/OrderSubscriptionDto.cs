using Enrich.Dto.Base.Attributes;

namespace Enrich.IMS.Dto
{   
    public partial class OrderSubscriptionDto
    {
        [FieldDb(nameof(Id))]
        public long Id { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        [FieldDb(nameof(StoreCode))]
        public string StoreCode { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb(nameof(OrderCode))]
        public string OrderCode { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        [FieldDb(nameof(ProductId))]
        public string ProductId { get; set; }

        /// <summary>
        /// Product Name
        /// </summary>
        [FieldDb(nameof(ProductName))]
        public string ProductName { get; set; }

        /// <summary>
        /// Customer Code
        /// </summary>
        [FieldDb(nameof(CustomerCode))]
        public string CustomerCode { get; set; }

        /// <summary>
        /// Customer Name
        /// </summary>
        [FieldDb(nameof(CustomerName))]
        public string CustomerName { get; set; }

        /// <summary>
        /// subscription Price
        /// </summary>
        [FieldDb(nameof(Price))]
        public decimal? Price { get; set; }

        /// <summary>
        /// Period
        /// </summary>
        [FieldDb(nameof(Period))]
        public string Period { get; set; }

        /// <summary>
        /// Purcharsed Day
        /// </summary>
        [FieldDb(nameof(PurcharsedDay))]
        public System.DateTime? PurcharsedDay { get; set; }

        /// <summary>
        /// Number Of Item
        /// </summary>
        [FieldDb(nameof(NumberOfItem))]
        public int? NumberOfItem { get; set; }

        /// <summary>
        /// Auto Renew
        /// </summary>
        [FieldDb(nameof(AutoRenew))]
        public bool? AutoRenew { get; set; }

        /// <summary>
        /// Start Date
        /// </summary>
        [FieldDb(nameof(StartDate))]
        public System.DateTime? StartDate { get; set; }

        /// <summary>
        /// Actived
        /// </summary>
        [FieldDb(nameof(Actived))]
        public bool? Actived { get; set; }

        /// <summary>
        /// Is Addon
        /// </summary>
        [FieldDb(nameof(IsAddon))]
        public bool? IsAddon { get; set; }

        /// <summary>
        /// End Date
        /// </summary>
        [FieldDb(nameof(EndDate))]
        public System.DateTime? EndDate { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        [FieldDb($"{SqlColumns.OrderSubcription.ProductCode}")]
        public string ProductCode { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb($"{SqlColumns.OrderSubcription.ProductCodePOSSystem}")]
        public string ProductCodePOSSystem { get; set; }

        /// <summary>
        /// Quantity
        /// If Period = ONETIME then quantity of subscription
        /// If Period = MONTHLY then number of period
        /// </summary>
        [FieldDb(nameof(Quantity))]
        public int? Quantity { get; set; }

        /// <summary>
        /// Discount
        /// </summary>
        [FieldDb(nameof(Discount))]
        public decimal? Discount { get; set; }

        /// <summary>
        /// Discount Percent
        /// </summary>
        [FieldDb(nameof(DiscountPercent))]
        public decimal? DiscountPercent { get; set; }

        /// <summary>
        /// Price Type (Trial, Promotional, Real)
        /// </summary>
        [FieldDb(nameof(PriceType))]
        public string PriceType { get; set; }

        /// <summary>
        /// Promotion Apply Months
        /// </summary>
        [FieldDb($"{SqlColumns.OrderSubcription.PromotionApplyMonths}")]
        public int? PromotionApplyMonths { get; set; }

        /// <summary>
        /// Subscription Type (license, addon, giftcard, other)
        /// </summary>
        [FieldDb(nameof(SubscriptionType))]
        public string SubscriptionType { get; set; }

        /// <summary>
        /// Apply Discount As Recurring
        /// </summary>
        [FieldDb(nameof(ApplyDiscountAsRecurring))]
        public bool? ApplyDiscountAsRecurring { get; set; }

        /// <summary>
        /// Period Recurring
        /// </summary>
        [FieldDb(nameof(PeriodRecurring))]
        public string PeriodRecurring { get; set; }

        /// <summary>
        /// Subscription Quantity
        /// If Period = ONETIME then SubscriptionQuantity = Quantity
        /// If Period = MONTHLY then quantity of subscription
        /// </summary>
        [FieldDb(nameof(SubscriptionQuantity))]
        public int? SubscriptionQuantity { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        [FieldDb(nameof(Amount))]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Apply Paid Date
        /// This is same custom date
        /// If = true then StartDate = picked date
        /// If = false then StartDate = paid date
        /// </summary>
        [FieldDb(nameof(ApplyPaidDate))]
        public bool? ApplyPaidDate { get; set; }

        /// <summary>
        /// Recurring Price
        /// </summary>
        [FieldDb(nameof(RecurringPrice))]
        public decimal? RecurringPrice { get; set; }
    }
}
