namespace Enrich.Payment.MxMerchant.Models
{
    public partial class IMSLog
    {
        public string Url { get; set; }
        public string RequestUrl { get; set; }
        public string RequestMethod { get; set; }
        public Nullable<int> StatusCode { get; set; }
        public Nullable<bool> Success { get; set; }
        public string JsonRequest { get; set; }
        public string JsonRespone { get; set; }
        public string Description { get; set; }
        public DateTime CreateOn { get; internal set; }
        public string CreateBy { get; internal set; }
    }
    public class ExResponse
    {
        public string errorCode { get; set; }
        public string message { get; set; }
        public List<string> details { get; set; }
        public string responseCode { get; set; }
    }
}