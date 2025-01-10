namespace Enrich.IMS.Entities
{   
    public partial class TicketSeverity
    {
        public long Id { get; set; }
        public string SeverityName { get; set; }
        public int? SeverityLevel { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public bool? Active { get; set; }
        public string SpecialType { get; set; }
    }
}
