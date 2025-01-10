namespace Enrich.IMS.Entities
{    
    public partial class Bundle
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string BundleCode { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public decimal? Price { get; set; }
    }
}
