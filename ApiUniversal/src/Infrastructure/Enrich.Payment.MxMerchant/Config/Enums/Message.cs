
namespace Enrich.Payment.MxMerchant.Config.Enums
{
    public sealed class IMSMessage
    {
        // Card info check
        public const string CardExist = "This card is already in use, please use another";
        public const string CardExpired = "This card has expired, please use another card";
        // Card type
        public const string CardUnknown = "Unknown Card type";
        public const string CardNotFormatted = "Card number is not formatted correctly";
        public const string CardExpiryFormatted = "Expiry date is not formatted correctly";
        // Security
        public const string NuveiSecurity = "Security is not expected! Please contact to us !";
        public const string SystemDefault = "An error has occurred! Please try again later!";
    }
    public enum MxResponeCode
    {
        ValidationError,
        ContactCustomerSupport
    }
}