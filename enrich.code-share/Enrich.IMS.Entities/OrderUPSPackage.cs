namespace Enrich.IMS.Entities
{
    public partial class OrderUPSPackage
    {
        public string Id { get; set; }
        public string OrderUPSTrackingId { get; set; }
        public string PackageWeight { get; set; }
        public string ImageLabelBase64 { get; set; }
        public string PackageType { get; set; }
    }
}
