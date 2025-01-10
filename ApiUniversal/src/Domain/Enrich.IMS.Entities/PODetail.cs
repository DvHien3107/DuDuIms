namespace Enrich.IMS.Entities
{
    public partial class PODetail
    {
        public long Id { get; set; }
        public string POCode { get; set; }
        public string RequestCode { get; set; }
        public decimal?Price { get; set; }
        public int? Qty { get; set; }
        public string Note { get; set; }
        public bool? Purchased { get; set; }
        public string ModelCode { get; set; }
        public bool? CheckedIn { get; set; }
        public System.DateTime? CheckedInDate { get; set; }
        public string InvNumbers { get; set; }
        public int? RemainQty { get; set; }
        public string ModelPicture { get; set; }
        public string ModelName { get; set; }
    }
}
