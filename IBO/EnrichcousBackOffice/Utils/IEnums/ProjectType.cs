using Inner.Libs.Helpful;

namespace EnrichcousBackOffice.Utils.IEnums
{
    public enum ProjectType
    {
        [EnumAttr(1, "project")] PROJECT,
        [EnumAttr(2, "category")] CATEGORY,
        [EnumAttr(3, "version")] VERSION,
    }
}