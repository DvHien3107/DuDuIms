
namespace Enrich.IMS.Entities
{    public partial class BonusStrategySetting
    {
        public long Id { get; set; }
        public string StrategyName { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
        public decimal? Opt_TotalIncome_EqualOrThan { get; set; }
        public int? Opt_NewMemberTotal_EqualOrThan { get; set; }
        public bool? Active { get; set; }
        public string ApplyForMemberType { get; set; }
        public decimal? BonusAmount { get; set; }
        public string Comment { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public int? Opt_TotalQuantityFullContracts_EqualOrThan { get; set; }
        public string ApplyByTerm { get; set; }
        public System.DateTime? EffectiveDate { get; set; }
        public string ApplyForMemberType_Name { get; set; }
    }
}
