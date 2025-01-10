using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public partial class TicketFeedback
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }
        public long? TicketId { get; set; }
        public string Feedback { get; set; }
        public string CreateByNumber { get; set; }
        public string CreateByName { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string TicketStatusChanges { get; set; }
        public string GlobalStatus { get; set; }
        public bool? Share { get; set; }
        public string FeedbackTitle { get; set; }
        public string DateCode { get; set; }
        public string Attachments { get; set; }
        public string UpdatedHistory { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public bool? DisableWriteLog { get; set; }
    }
}
