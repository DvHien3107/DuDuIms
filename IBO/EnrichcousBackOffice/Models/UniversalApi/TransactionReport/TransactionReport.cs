using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.TransactionReport
{
    public class TransactionReport
    {
        public string Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public double? Price { get; set; }
        public double? SubscriptionAmount { get; set; }
        public double? Discount { get; set; }
        public double? DiscountPercent { get; set; }
        public int? Quantity { get; set; }
        public string PriceType { get; set; }
        public string Type { get; set; }
        public string CustomerId { get; set; }
        public string StoreCode { get; set; }
        public string StoreName { get; set; }
        public string PartnerCode { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentDescription { get; set; }
        public string OrderCode { get; set; }
        public string OrderStatus { get; set; }
        public double? OrderTotal { get; set; }
        public double? OrderAmount { get; set; }
        public double? OrderDiscountAmount { get; set; }
        public double? OrderDiscountPercent { get; set; }
        public DateTime? CreateAt { get; set; }
        public string CreateAtRaw { get; set; }
        public string CreateBy { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string PaymentDateUtcRaw { get; set; }
    }
}