namespace Enrich.IMS.Entities
{    
    public partial class TicketTypeMapping
    {
        public int Id { get; set; }
        public long? TypeId { get; set; }
        public string TypeName { get; set; }
        public long? TicketId { get; set; }
    
        public virtual TicketType T_TicketType { get; set; }
        public virtual Ticket T_SupportTicket { get; set; }
    }
}
