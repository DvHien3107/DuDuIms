namespace Enrich.IMS.Entities
{
    public partial class PORequest
    {
        public string Code { get; set; }
        public string ModelCode { get; set; }
        public string ModelPicture { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Color { get; set; }
        public int? Qty { get; set; }
        public string Status { get; set; }
        public bool? Purchased { get; set; }
        public string Note { get; set; }
        public long? CreatedbyId { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public int? RequestQty { get; set; }
        public string ModelName { get; set; }
    }
}
