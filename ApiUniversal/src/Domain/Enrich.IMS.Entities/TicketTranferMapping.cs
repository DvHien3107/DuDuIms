
namespace Enrich.IMS.Entities
{    
    public partial class TicketTranferMapping
    {
        public int Id { get; set; }
        public long? TicketId { get; set; }
        public long? FromTicketId { get; set; }
        public long? ToTicketId { get; set; }
        public string FromProjectId { get; set; }
        public string ToProjectId { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateByName { get; set; }
        public string CreateByMemberNumner { get; set; }
        public string Note { get; set; }
    }
}
