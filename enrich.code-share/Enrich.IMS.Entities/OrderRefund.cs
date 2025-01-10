namespace Enrich.IMS.Entities
{    
    public partial class OrderRefund
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string OrderCode { get; set; }
        public string CustomerCode { get; set; }
        public decimal? RefundAmout { get; set; }
        public string Reason { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    }
}
