
namespace Enrich.IMS.Entities
{
    public partial class BundleDevice
    {
        public long Id { get; set; }
        public long? BundleId { get; set; }
        public int? Quantity { get; set; }
        public string ModelCode { get; set; }
        public decimal? Price { get; set; }
        public string ModelName { get; set; }
    }
}
