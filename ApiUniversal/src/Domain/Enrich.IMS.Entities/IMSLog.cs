namespace Enrich.IMS.Entities
{
    public partial class IMSLog
    {
        public string Id { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateOn { get; set; }
        public string Url { get; set; }
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public int? StatusCode { get; set; }
        public bool? Success { get; set; }
        public string JsonRequest { get; set; }
        public string JsonRespone { get; set; }
        public string Description { get; set; }
        public string SalonName { get; set; }
    }
}
