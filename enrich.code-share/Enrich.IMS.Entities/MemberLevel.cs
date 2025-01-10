
namespace Enrich.IMS.Entities
{
    public partial class MemberLevel
    {
        public long Id { get; set; }
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public int? LevelNumber { get; set; }
        public string LevelName { get; set; }
        public System.DateTime? EffectiveDate { get; set; }
        public System.DateTime? PromotedAtDate { get; set; }
        public string PromotedBy { get; set; }
    }
}
