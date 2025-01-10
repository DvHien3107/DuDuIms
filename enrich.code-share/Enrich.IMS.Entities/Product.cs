namespace Enrich.IMS.Entities
{    
    public partial class Product
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ProductLineCode { get; set; }
        public string Picture { get; set; }
        public string Description { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
