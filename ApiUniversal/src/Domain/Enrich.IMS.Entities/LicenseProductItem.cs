namespace Enrich.IMS.Entities
{    
    public partial class LicenseProductItem
    {
        public long Id { get; set; }
        public string License_Product_Id { get; set; }
        public int? CountWarning { get; set; }
        public string License_Item_Code { get; set; }
        public int? Value { get; set; }
        public bool? Enable { get; set; }
    }
}
