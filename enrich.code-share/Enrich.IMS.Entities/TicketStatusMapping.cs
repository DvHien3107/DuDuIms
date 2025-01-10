
namespace Enrich.IMS.Entities
{
    public partial class TicketStatusMapping
    {
        public int Id { get; set; }
        public long StatusId { get; set; }
        public string StatusName { get; set; }
        public long TicketId { get; set; }
    
        public virtual TicketStatus T_TicketStatus { get; set; }
        public virtual Ticket T_SupportTicket { get; set; }
    }
}
