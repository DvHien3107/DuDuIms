namespace Enrich.IMS.Entities
{    
    public partial class TicketAttribute
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ParentId { get; set; }
        public string ParentName { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public bool? Active { get; set; }
        public string apply_for_ticket_type { get; set; }
    }
}
