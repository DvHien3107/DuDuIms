namespace Enrich.IMS.Entities
{    public partial class PersonalLevel
    {
        public int Level { get; set; }
        public string LevelName { get; set; }
        public string Decription { get; set; }
        public int? OptionPromote1_RequirementLevel { get; set; }
        public int? OptionPromote1_RequirementLevel_Qty { get; set; }
        public int? OptionPromote1_RequirementLevel_RequimentEveryLevel_QtyContract { get; set; }
        public int? OptionPromote2_RequimentQtyContractReached { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public bool? IsActive { get; set; }
    }
}
