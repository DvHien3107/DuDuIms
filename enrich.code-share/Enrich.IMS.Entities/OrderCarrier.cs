namespace Enrich.IMS.Entities
{
    public partial class OrderCarrier
    {
        public string Id { get; set; }
        public string OrderCode { get; set; }
        public string TrackingNumber { get; set; }
        public string CarrierName { get; set; }
        public System.DateTime? DateShipped { get; set; }
        public string CarrierNote { get; set; }
    }
}
