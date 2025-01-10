using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Areas.PaymentGate.ModelsView
{
    public class OrderRequest
    {
        public string OrderCode { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public int TotalItem { get; set; }
        public string BuyerName { get; set; }
        public string BuyerEmail { get; set; }
        public string BuyerPhone { get; set; }
        public string BuyerAddress { get; set; }
        public string BuyerCity { get; set; }
        public string BuyerCountry { get; set; }
        public string MerchantSideUserId { get; set; }
        public string BuyerPostalCode { get; set; }
        public string BuyerState { get; set; }
        public bool IsCardLink { get; set; }
        public string Status { get; set; }
    }

    public class ProductView
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double TotalAmount { get; set; }
        public string Period { get; internal set; }
    }
}