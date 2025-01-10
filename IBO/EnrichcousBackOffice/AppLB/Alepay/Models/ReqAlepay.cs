using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.AppLB.Alepay.Model
{
    class ReqAlepay
    {
        public string token { get; set; }
        public string data { get; set; }
        public string checksum { get; set; }
    }

    public class Order
    {
        public bool allowDomestic { get; set; }
        public string orderCode { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string orderDescription { get; set; }
        public int totalItem { get; set; }
        public int checkoutType { get; set; }
        public bool installment { get; set; }
        public int month { get; set; }
        public string bankCode { get; set; }
        public string paymentMethod { get; set; }
        public string returnUrl { get; set; }
        public string cancelUrl { get; set; }
        public string buyerName { get; set; }
        public string buyerEmail { get; set; }
        public string buyerPhone { get; set; }
        public string buyerAddress { get; set; }
        public string buyerCity { get; set; }
        public string buyerCountry { get; set; }
        public string paymentHours { get; set; }
        public string merchantSideUserId { get; set; }
        public string buyerPostalCode { get; set; }
        public string buyerState { get; set; }
        public bool isCardLink { get; set; }
    }
    public class ReqTransaction
    {
        public string transactionCode { get; set; }
    }
    public class ProfileAlepay
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string street { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
        public string callback { get; set; }
    }
}