namespace Enrich.IMS.Entities
{
    public partial class CustomerPayment
    {
        public string Id { get; set; }
        public string TransactionCode { get; set; }
        public string OrderCode { get; set; }
        public string CustomerCode { get; set; }
        public decimal?Amount { get; set; }
        public decimal?MerchantFee { get; set; }
        public decimal?PayerFee { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string Status { get; set; }
        public string StatusCode { get; set; }
        public string Method { get; set; }
        public string BankCode { get; set; }
        public string BankName { get; set; }
        public string CardNumber { get; set; }
        public string CreateBy { get; set; }
        public string PaymentNote { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    }
}
