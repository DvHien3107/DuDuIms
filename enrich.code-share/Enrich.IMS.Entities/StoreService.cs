using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public partial class StoreService
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]

        public string Id { get; set; }

        /// <summary>
        /// Store Code
        /// </summary>
        public string StoreCode { get; set; }

        /// <summary>
        /// Store Name
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Customer Code
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Effective Date (start date)
        /// </summary>
        public System.DateTime? EffectiveDate { get; set; }

        /// <summary>
        /// Renew Date (end date)
        /// </summary>
        public System.DateTime? RenewDate { get; set; }

        /// <summary>
        /// Product Code
        /// </summary>
        public string ProductCode { get; set; }

        /// <summary>
        /// Product name
        /// </summary>
        public string Productname { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Period
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Auto Renew
        /// </summary>
        public bool? AutoRenew { get; set; }

        /// <summary>
        /// Active
        /// </summary>
        public int? Active { get; set; }

        /// <summary>
        /// LastUpdateAt
        /// </summary>
        public System.DateTime? LastUpdateAt { get; set; }

        /// <summary>
        /// Last Update By
        /// </summary>
        public string LastUpdateBy { get; set; }

        /// <summary>
        /// Last Renew At
        /// </summary>
        public System.DateTime? LastRenewAt { get; set; }

        /// <summary>
        /// Last Renew By
        /// </summary>
        public string LastRenewBy { get; set; }

        /// <summary>
        /// Last Renew Order Code
        /// </summary>
        public string LastRenewOrderCode { get; set; }

        /// <summary>
        /// has Renew Invoice ncomplete
        /// </summary>
        public bool? HasRenewInvoiceIncomplete { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public string OrderCode { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ProductCodePOSSystem { get; set; }

        /// <summary>
        /// Store Apply
        /// </summary>
        public string StoreApply { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public long? MxMerchantSubscriptionId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public long? MxMerchantContractId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public long? MxMerchantCardAccountId { get; set; }

        /// <summary>
        /// Order Code
        /// </summary>
        public string MxMerchantSubscriptionStatus { get; set; }

        /// <summary>
        /// Apply Discount As Recurring
        /// </summary>
        public bool? ApplyDiscountAsRecurring { get; set; }

        /// <summary>
        /// Period Recurring
        /// </summary>
        public string PeriodRecurring { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public int? Quantity { get; set; }
    }
}
