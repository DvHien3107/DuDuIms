using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.AppLB.Alepay.Model
{
    class ResAlepay
    {
        public string errorCode { get; set; }
        public string data { get; set; }
        public string checksum { get; set; }
        public string errorDescription { get; set; }
    }
    class ResAlepayView
    {
        public bool error { get; set; }
        public ResCheckout data { get; set; }
        public ResTransaction dataTransaction { get; set; }
        public string url { get; set; }
        public string errorDescription { get; set; }
    }
    class ResCheckout
    {
        public string token { get; set; }
        public string checkoutUrl { get; set; }
    }

    class ResProfile
    {
        public string url { get; set; }
    }

    class ResPaymentSuccess
    {
        public string errorCode { get; set; }
        public string data { get; set; }
        public bool cancel { get; set; }
    }

    class DataCardLink
    {
        public string alepayToken { get; set; }
        public string transactionCode { get; set; }
        public string cardLinkCode { get; set; }
    }
    class ResPaymentSuccessWithCardLink
    {
        public string errorCode { get; set; }
        public DataCardLink data { get; set; }
        public bool cancel { get; set; }
    }

    class ResTransaction
    {
        public string transactionCode { get; set; }
        public string orderCode { get; set; }
        public double amount { get; set; }
        public string currency { get; set; }
        public string buyerEmail { get; set; }
        public string buyerPhone { get; set; }
        public string cardNumber { get; set; }
        public string buyerName { get; set; }
        public string status { get; set; }
        public string message { get; set; }
        public bool installment { get; set; }
        public bool is3D { get; set; }
        public int month { get; set; }
        public string bankCode { get; set; }
        public string bankName { get; set; }
        public string method { get; set; }
        public long transactionTime { get; set; }
        public long successTime { get; set; }
        public string bankHotline { get; set; }
        public double merchantFee { get; set; }
        public double payerFee { get; set; }
    }
}
