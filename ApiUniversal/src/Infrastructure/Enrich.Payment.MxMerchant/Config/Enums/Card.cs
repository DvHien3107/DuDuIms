
namespace Enrich.Payment.MxMerchant.Config.Enums
{
    public enum ECardType
    {
        // Invalid card
        Unknown,
        NotFormatted,
        // Valid card
        VisaCredit,
        MasterCard,
        VisaElectron,
        Maestro,
        Laser,
        AmericanExpress,
        Diners,
        JCB,
        Discover,
        CupSecurePay,
        VisaDebit,
        DebitMasterCard,
    }
    public enum TenderType
    {
        ACH,
    }
    public enum BankType
    {
        Checking,
        Savings
    }
}