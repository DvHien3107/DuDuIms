namespace Enrich.IMS.Entities
{    
    public partial class PODetailCheckin
    {
        public long Id { get; set; }
        public long? PO_Detail_id { get; set; }
        public string LocationId { get; set; }
        public int? Qty { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string POCode { get; set; }
        public string InvNumbers { get; set; }
        public string LocationName { get; set; }
    }
}
