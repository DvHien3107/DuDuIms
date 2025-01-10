namespace Enrich.IMS.Entities
{
    public partial class OutgoingEmail
    {
        public long Id { get; set; }
        public string CustomerCode { get; set; }
        public string ToEmail { get; set; }
        public string CCEmail { get; set; }
        public string ReplyEmail { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string HistorySend { get; set; }
        public string Attachment { get; set; }
        public bool? IsDelete { get; set; }
    }
}
