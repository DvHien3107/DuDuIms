using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.IEnums
{

    /// <summary>
    /// IMS Version
    /// </summary>
    public enum CampaignStatus
    {
        [EnumAttr(0, "Open")] Open = 0,
        [EnumAttr(1, "Closed")] Closed = 1,
        [EnumAttr(-1, "Failed")] Failed = -1,
        [EnumAttr(-2, "Removed")] Removed = -2
    }

    public enum AdsStatus
    {
        [EnumAttr(2, "Sent")] Sent = 2,
        [EnumAttr(1, "Failed")] Failed = 1,
        [EnumAttr(0, "Draft")] Draft = 0,
        [EnumAttr(-1, "Cancel")] Cancel = -1,
        [EnumAttr(-2, "Removed")] Removed = -2
    }

    public enum AdsCustommer
    {
        [EnumAttr(1, "Merchant")] Merchant = 1,
        [EnumAttr(2, "Lead")] Lead = 2,
        [EnumAttr(3, "Potential")] Potential = 3,
        [EnumAttr(4, "Data")] Data = 4,
        [EnumAttr(5, "Other")] Other = 5,
        [EnumAttr(6, "Trial")] Trial = 6,
    }
}