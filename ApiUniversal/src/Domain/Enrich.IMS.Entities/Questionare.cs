namespace Enrich.IMS.Entities
{
    public partial class Questionare
    {
        public string Id { get; set; }
        public long? CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string SalonName { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public bool? AllowTexting { get; set; }
        public string SalonHours { get; set; }
        public bool? EthernetWrite { get; set; }
        public string ServiceFee { get; set; }
        public string TechOrder { get; set; }
        public double? DollarAmount { get; set; }
        public string FileAttach { get; set; }
        public bool? ComboSpecialDiscount { get; set; }
        public string Note { get; set; }
        public bool? PayoutCreditCard { get; set; }
        public string AUTHORIZED_Seller { get; set; }
        public string AUTHORIZED_Tech { get; set; }
        public string OtherRequest { get; set; }
        public System.DateTime? DateSubmit { get; set; }
        public string Status { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public System.DateTime? LastUpdate { get; set; }
        public string UpdateHistory { get; set; }
        public bool? CustomerAddTips { get; set; }
        public bool? SuggestPercentTip { get; set; }
        public string SalonEmail { get; set; }
        public string OwnerEmail { get; set; }
        public bool? ChargeFeeTips { get; set; }
        public bool? CommissionProductSales { get; set; }
        public string SalonAddress1 { get; set; }
        public string SalonAddress2 { get; set; }
        public string SalonCity { get; set; }
        public string SalonState { get; set; }
        public string SalonZipcode { get; set; }
        public string SalonPhone { get; set; }
        public string SalonTimeZone { get; set; }
        public string Drivers_License_Front_Image { get; set; }
        public string Drivers_License_Back_Image { get; set; }
        public string SS4 { get; set; }
        public string VoidedBusinessCheck { get; set; }
        public string SendToOwnersEmail { get; set; }
    }
}
