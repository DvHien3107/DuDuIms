namespace Enrich.IMS.Entities
{
    public partial class TicketUpdateLog
    {
        public int Id { get; set; }
        public int? UpdateId { get; set; }
        public long? TicketId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
    }
}
