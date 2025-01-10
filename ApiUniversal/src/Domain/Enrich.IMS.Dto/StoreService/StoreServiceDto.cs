using Enrich.Dto.Base.Attributes;

namespace Enrich.IMS.Dto
{    
    public partial class StoreServiceDto
    {
        public string Id { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        [FieldDb(nameof(StoreCode))]
        public string StoreCode { get; set; }

        /// <summary>
        /// Store Name
        /// </summary>
        [FieldDb(nameof(StoreName))]
        public string StoreName { get; set; }

        /// <summary>
        /// Customer Code
        /// </summary>
        [FieldDb(nameof(CustomerCode))]
        public string CustomerCode { get; set; }

        /// <summary>
        /// Effective Date (start date)
        /// </summary>
        [FieldDb(nameof(EffectiveDate))]
        public System.DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Renew Date (end date)
        /// </summary>
        [FieldDb(nameof(RenewDate))]
        public System.DateTime? RenewDate { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        [FieldDb(nameof(ProductCode))]
        public string ProductCode { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        [FieldDb(nameof(Productname))]
        public string Productname { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [FieldDb(nameof(Type))]
        public string Type { get; set; }

        /// <summary>
        /// Period
        /// </summary>
        [FieldDb(nameof(Period))]
        public string Period { get; set; }

        /// <summary>
        /// Auto Renew
        /// </summary>
        [FieldDb(nameof(AutoRenew))]
        public bool? AutoRenew { get; set; }

        /// <summary>
        /// Active
        /// </summary>
        [FieldDb(nameof(Active))]
        public int? Active { get; set; }

        /// <summary>
        /// LastUpdateAt
        /// </summary>
        [FieldDb(nameof(LastUpdateAt))]
        public System.DateTime? LastUpdateAt { get; set; }

        /// <summary>
        /// Last Update By
        /// </summary>
        [FieldDb(nameof(LastUpdateBy))]
        public string LastUpdateBy { get; set; }

        /// <summary>
        /// Last Renew At
        /// </summary>
        [FieldDb(nameof(LastRenewAt))]
        public System.DateTime? LastRenewAt { get; set; }

        /// <summary>
        /// Last Renew By
        /// </summary>
        [FieldDb(nameof(LastRenewBy))]
        public string LastRenewBy { get; set; }

        /// <summary>
        /// Last Renew Order Code
        /// </summary>
        [FieldDb(nameof(LastRenewOrderCode))]
        public string LastRenewOrderCode { get; set; }

        /// <summary>
        /// has Renew Invoice ncomplete
        /// </summary>
        [FieldDb($"{SqlColumns.StoreServices.HasRenewInvoiceIncomplete}")]
        public bool? HasRenewInvoiceIncomplete { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb(nameof(OrderCode))]
        public string OrderCode { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb($"{SqlColumns.StoreServices.ProductCodePOSSystem}")]
        public string ProductCodePOSSystem { get; set; }

        /// <summary>
        /// Store Apply
        /// </summary>
        [FieldDb(nameof(StoreApply))]
        public string StoreApply { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb($"{SqlColumns.StoreServices.MxMerchantSubscriptionId}")]
        public long? MxMerchantSubscriptionId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb($"{SqlColumns.StoreServices.MxMerchantContractId}")]
        public long? MxMerchantContractId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb($"{SqlColumns.StoreServices.MxMerchantCardAccountId}")]
        public long? MxMerchantCardAccountId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        [FieldDb($"{SqlColumns.StoreServices.MxMerchantSubscriptionStatus}")]
        public string MxMerchantSubscriptionStatus { get; set; }

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
        /// Quantity
        /// </summary>
        [FieldDb(nameof(Quantity))]
        public int? Quantity { get; set; }
    }
}
