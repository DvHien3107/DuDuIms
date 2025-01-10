
namespace Enrich.IMS.Entities
{
    public partial class FileRelated
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public long TicketId { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }    
        public virtual Ticket T_SupportTicket { get; set; }
    }
}
