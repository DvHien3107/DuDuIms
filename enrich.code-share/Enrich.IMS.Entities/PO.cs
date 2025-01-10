namespace Enrich.IMS.Entities
{    
    public partial class PO
    {
        public string POCode { get; set; }
        public long? VendorId { get; set; }
        public string VendorName { get; set; }
        public decimal?Total { get; set; }
        public string Status { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime? UpdatedAt { get; set; }
        public string SaleOrderNumber { get; set; }
        public bool? AllCheckedIn { get; set; }
    }
}
