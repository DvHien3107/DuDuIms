namespace Enrich.IMS.Entities
{
    public partial class Campaign
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? NumberOfPeopleReached { get; set; }
        public int? TotalSuccess { get; set; }
        public int? TotalFailed { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? LastUpdateAt { get; set; }
        public string LastUpdateBy { get; set; }
        public int? Status { get; set; }
        public string Note { get; set; }
    }
}
