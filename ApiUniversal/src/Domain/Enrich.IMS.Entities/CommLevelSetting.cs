namespace Enrich.IMS.Entities
{
    public partial class CommLevelSetting
    {
        public long Id { get; set; }
        public decimal?CommPercent_Override { get; set; }
        public int? LevelNumber { get; set; }
        public System.DateTime? EffectiveDate { get; set; }
        public decimal?CommPercent_Directly { get; set; }
        public decimal?CommPercent_ManagementOffice { get; set; }
        public string ProductTypeCode { get; set; }
        public string ProductTypeName { get; set; }
    }
}
