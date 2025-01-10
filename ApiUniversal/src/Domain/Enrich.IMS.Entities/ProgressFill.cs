namespace Enrich.IMS.Entities
{
    public partial class ProgressFill
    {
        public long Id { get; set; }
        public string OrderCode { get; set; }
        public long? FieldFillId { get; set; }
        public string Content { get; set; }
    }
}
