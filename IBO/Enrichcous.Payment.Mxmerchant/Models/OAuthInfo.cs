namespace Enrichcous.Payment.Mxmerchant.Models
{
    public class OauthInfo
    {
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
        public bool Updated { get; set; } = false;
    }
}