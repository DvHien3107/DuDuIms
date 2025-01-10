using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.IEnums
{
    public enum LeadStatus
    {
        [EnumAttr(1, "Lead")] Lead,
        [EnumAttr(2, "Trial Account")] TrialAccount,
        [EnumAttr(3, "Merchant")] Merchant,
        [EnumAttr(4, "Slice Account")] SliceAccount,
    }
    public enum LeadType
    {
        [EnumAttr(1, "DATA")] ImportData,
        [EnumAttr(2, "CREATE")] CreateBySaler,
        [EnumAttr(3, "REGISTER_TRIAL_ACCOUNT")] TrialAccount,
        [EnumAttr(4, "REGISTER_SLICE_ACCOUNT")] SliceAccount,
        [EnumAttr(5, "REGISTER_ON_MANGO")] SubscribeMango,
        [EnumAttr(6, "REGISTER_ON_IMS")] RegisterOnIMS,
    }
}