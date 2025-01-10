namespace Enrich.IMS.Entities
{
    public partial class Device
    {
        public long DeviceId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? Inventory { get; set; }
        public string Description { get; set; }
        public string TechnicalDescription { get; set; }
        public string Picture { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public bool? Active { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string AddonDevicesId { get; set; }
        public string AddonDevicesName { get; set; }
        public bool? Junkyard { get; set; }
        public string JunkyardDescription { get; set; }
        public string Version { get; set; }
        public System.DateTime? WarrantyExpiration { get; set; }
        public string InvNumber { get; set; }
        public string ModelCode { get; set; }
        public string SerialNumber { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public long? VendorId { get; set; }
        public string VendorName { get; set; }
        public string ModelName { get; set; }
        public string Comment { get; set; }
    }
}
