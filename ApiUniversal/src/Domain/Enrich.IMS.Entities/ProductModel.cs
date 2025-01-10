namespace Enrich.IMS.Entities
{    
    public partial class ProductModel
    {
        public string ModelCode { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Color { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string Picture { get; set; }
        public bool? Active { get; set; }
        public string ModelName { get; set; }
        public bool? DeviceRequired { get; set; }
        public bool? MerchantOnboarding { get; set; }
        public bool? IsDeleted { get; set; }
        public decimal? SalePrice { get; set; }
    }
}
