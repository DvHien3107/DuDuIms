using System;

namespace Enrich.IMS.Entities
{
    
    public partial class RemindersTicket
    {
        public int Id { get; set; }
        public long TicketId { get; set; }
        public System.DateTime? Date { get; set; }
        public TimeSpan Time { get; set; }
        public bool? Active { get; set; }
        public string Note { get; set; }
        public string Repeat { get; set; }
        public string HangfireJobId { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public long? TaskId { get; set; }
    
        public virtual Ticket T_SupportTicket { get; set; }
    }
}
