using Inner.Libs.Helpful;

namespace Enrichcous.Payment.Mxmerchant.Config.Enums
{
    public enum PaymentStatus
    {
        [EnumAttr(0, "Declined")] Declined,
        [EnumAttr(1, "Approved")] Approved,
        [EnumAttr(2, "Success")] Success,
        [EnumAttr(3, "Failed")] Failed,
    }

    public enum RecurringInterval
    {
        [EnumAttr(0, "week")] Weekly,
        [EnumAttr(1, "month")] Monthly,
        [EnumAttr(2, "year")] Yearly,
    }
}