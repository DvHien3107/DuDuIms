namespace Enrich.IMS.Entities
{   
    public partial class Advertisement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AdsType { get; set; }
        public string Resource { get; set; }
        public string LicenseType { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public int? NumberOfPeopleReached { get; set; }
        public string Additional { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public System.DateTime? StatusDate { get; set; }
        public int? TotalSuccess { get; set; }
        public int? TotalFailed { get; set; }
        public string TemplateID { get; set; }
        public string FailedReason { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? LastUpdateAt { get; set; }
        public string LastUpdateBy { get; set; }
        public string CampaignId { get; set; }
        public string Attachment { get; set; }
    }
}
