using Inner.Libs.Helpful;

namespace Enrichcous.Payment.Nuvei.Config.Enums
{
    public enum IMS_MSG
    {
        // Card info check
        [EnumAttr("", "This card is already in use, please use another")] E_CARD_EXIST,
        [EnumAttr("", "This card has expired, please use another card")] E_CARD_EXPIRED,

        // Card type
        [EnumAttr("", "Unknown Card type")] E_CARD_UNKNOWN,
        [EnumAttr("", "Card number is not formatted correctly")] E_CARD_NOT_FORMATTED,
        [EnumAttr("", "Expiry date is not formatted correctly")] E_CARD_EXPIRY_FORMATTED,

        // Security
        [EnumAttr("", "Security is not expected! Please contact to us !")] E_NUVEI_SECURITY,
        
        [EnumAttr("", "An error has occurred! Please try again later!")] E_SYS_DEFAULT,
    }
}