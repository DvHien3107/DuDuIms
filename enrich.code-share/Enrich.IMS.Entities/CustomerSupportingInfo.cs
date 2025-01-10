namespace Enrich.IMS.Entities
{  
    public partial class CustomerSupportingInfo
    {
        public long Id { get; set; }
        public long? CustomerId { get; set; }
        public string RemoteLogin { get; set; }
        public string MoreHardware { get; set; }
        public string OtherNotes { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
    }
}
