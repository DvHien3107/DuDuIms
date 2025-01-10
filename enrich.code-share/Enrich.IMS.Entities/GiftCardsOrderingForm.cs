namespace Enrich.IMS.Entities
{
    public partial class GiftCardsOrderingForm
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string OrderCode { get; set; }
        public string SalesPerson { get; set; }
        public string SalonHours { get; set; }
        public string ProductCode { get; set; }
        public string FrontDesign { get; set; }
        public string BackDesign { get; set; }
        public string FrontDesignFiles { get; set; }
        public string BackDesignFiles { get; set; }
        public string Note { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public System.DateTime? UpdateAt { get; set; }
    }
}
