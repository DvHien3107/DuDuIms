namespace Enrich.IMS.Entities
{    
    public partial class OrderUPSTracking
    {
        public string Id { get; set; }
        public string OrderCode { get; set; }
        public string TrackingNumber { get; set; }
        public string ShippmentDescription { get; set; }
        public string ShipperStreetAddress { get; set; }
        public string ShipperState { get; set; }
        public string ShipperCity { get; set; }
        public string ShipperZipcode { get; set; }
        public string ShipperCountry { get; set; }
        public string Status { get; set; }
        public int? Active { get; set; }
        public string UPS_Service_Code { get; set; }
        public string ActivityStatus { get; set; }
    }
}
