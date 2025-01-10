namespace Enrich.Payment.MxMerchant.Models
{
    public class OauthInfo
    {
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
        public bool Updated { get; set; } = false;
    }
}