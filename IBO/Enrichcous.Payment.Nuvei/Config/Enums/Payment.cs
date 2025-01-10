using Inner.Libs.Helpful;

namespace Enrichcous.Payment.Nuvei.Config.Enums
{
    public enum PaymentStatus
    {
        [EnumAttr(0, "Unknown")] Unknown,
        [EnumAttr(1, "Success")] Success,
        [EnumAttr(10, "NuveiError")] NuveiError,
        [EnumAttr(20, "CanSave")] CanSave,
        [EnumAttr(30, "SystemError")] SystemError
    }
}