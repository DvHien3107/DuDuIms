using Inner.Libs.Helpful;

namespace Enrichcous.Payment.Mxmerchant.Config.Enums
{
    public enum ECardType
    {
        // Invalid card
        [EnumAttr("UN_KNOWN", "Unknown")] Unknown,
        [EnumAttr("NotFormatted", "NotFormatted")] NotFormatted,
        // Valid card
        [EnumAttr("VISA", "Visa Credit")] VisaCredit,
        [EnumAttr("MASTERCARD", "MasterCard")] MasterCard,
        [EnumAttr("ELECTRON", "Visa Electron")] VisaElectron,
        [EnumAttr("MAESTRO", "Maestro")] Maestro,
        [EnumAttr("LASER", "Laser")] Laser,
        [EnumAttr("AMERICAN EXPRESS", "American Express")] AmericanExpress,
        [EnumAttr("DINER'S CLUB CARTE BLANCHE", "DINER'S CLUB CARTE BLANCHE")] Diners,
        [EnumAttr("JCB", "JCB")] JCB,
        [EnumAttr("DISCOVER", "Discover")] Discover,
        // ???
        [EnumAttr("CUP SECUREPAY", "Cup SecurePay")] CupSecurePay,
        [EnumAttr("VISA DEBIT", "Visa Debit")] VisaDebit,
        [EnumAttr("DEBIT MASTERCARD", "Debit MasterCard")] DebitMasterCard,
    }
}