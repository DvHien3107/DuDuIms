namespace Enrich.IMS.Entities
{
    public partial class OrdersServiceRenewal
    {
        public long Id { get; set; }
        public long? OrdersServiceId { get; set; }
        public string OrdersCode { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public decimal? Price { get; set; }
        public int? Monthly { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal? TotalAmount { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public System.DateTime? UpdatedAt { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
        public System.DateTime? Stop_BeforeTheTerm_AtDate { get; set; }
    }
}
