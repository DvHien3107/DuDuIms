namespace Enrich.IMS.Entities
{
    public partial class CustomerAdditionalInfo
    {
        public long Id { get; set; }
        public long? CustomerID { get; set; }
        public string InfoName { get; set; }
        public string InfoContent { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
    }
}
