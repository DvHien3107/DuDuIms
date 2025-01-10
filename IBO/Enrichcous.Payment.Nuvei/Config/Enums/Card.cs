using Inner.Libs.Helpful;

namespace Enrichcous.Payment.Nuvei.Config.Enums
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
        [EnumAttr("AMEX", "American Express")] AmericanExpress,
        [EnumAttr("DINERS", "Diners")] Diners,
        [EnumAttr("JCB", "JCB")] JCB,
        [EnumAttr("DISCOVER", "Discover")] Discover,
        // ???
        [EnumAttr("CUP SECUREPAY", "Cup SecurePay")] CupSecurePay,
        [EnumAttr("VISA DEBIT", "Visa Debit")] VisaDebit,
        [EnumAttr("DEBIT MASTERCARD", "Debit MasterCard")] DebitMasterCard,
    }

    public enum ECurrency
    {
        [EnumAttr(0, "Unknown")] Unknown,
        [EnumAttr(840, "USD")] USD,
        [EnumAttr(978, "EUR")] EUR,
        [EnumAttr(826, "GBP")] GBP,
    }
}