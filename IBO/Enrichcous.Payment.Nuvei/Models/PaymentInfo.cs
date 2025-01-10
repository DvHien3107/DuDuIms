namespace Enrichcous.Payment.Nuvei.Models
{
    public class PaymentInfo
    {
        public double Amount { get; set; } = 0;
        public string OrderCode { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
        public string ApprovalCode { get; set; }
        public string AvsResponse { get; set; }
        public string CvvResponse { get; set; }
        public string UniqueRef { get; set; }
    }
}