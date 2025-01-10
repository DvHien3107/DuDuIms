namespace Enrich.IMS.Dto
{
    public partial class CustomerTransactionDto
    {
        public string Id { get; set; }
        public string Card { get; set; }
        public string PaymentStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string ApprovalCode { get; set; }
        public string AvsResponse { get; set; }
        public string CVVResponse { get; set; }
        public string UniqueREF { get; set; }
        public string CustomerCode { get; set; }
        public string StoreCode { get; set; }
        public string OrdersCode { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string BankName { get; set; }
        public string CardNumber { get; set; }
        public string MxMerchant_id { get; set; }
        public string MxMerchant_token { get; set; }
        public string MxMerchant_authMessage { get; set; }
        public string CreateBy { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentNote { get; set; }
    }
}
