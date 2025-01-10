namespace Enrich.IMS.Entities
{
    public partial class MerchantProcess
    {
        public long Id { get; set; }
        public string CustomerCode { get; set; }
        public string M_ID { get; set; }
        public string Status { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Model { get; set; }
        public string InvNumber { get; set; }
        public string OrderProductId { get; set; }
    }
}
