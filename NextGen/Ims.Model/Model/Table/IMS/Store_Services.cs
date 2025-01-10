using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class Store_Services
    {
        public string Id { get; set; }

        public string StoreCode { get; set; }

        public string StoreName { get; set; }

        public string CustomerCode { get; set; }

        public DateTime? EffectiveDate { get; set; }

        public DateTime? RenewDate { get; set; }

        public string ProductCode { get; set; }

        public string Productname { get; set; }

        public string Type { get; set; }

        public string Period { get; set; }

        public bool? AutoRenew { get; set; }

        public int? Active { get; set; }

        public DateTime? LastUpdateAt { get; set; }

        public string LastUpdateBy { get; set; }

        public DateTime? LastRenewAt { get; set; }

        public string LastRenewBy { get; set; }

        public string LastRenewOrderCode { get; set; }

        public bool? hasRenewInvoiceIncomplete { get; set; }

        public string OrderCode { get; set; }

        public string Product_Code_POSSystem { get; set; }

        public string StoreApply { get; set; }

        public long? MxMerchant_SubscriptionId { get; set; }

        public long? MxMerchant_contractId { get; set; }

        public long? MxMerchant_cardAccountId { get; set; }

        public string MxMerchant_SubscriptionStatus { get; set; }

        public bool? ApplyDiscountAsRecurring { get; set; }

        public string PeriodRecurring { get; set; }

        public int? Quantity { get; set; }

        public string RecurringType { get; set; }

    }
}
