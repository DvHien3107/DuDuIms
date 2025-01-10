namespace Enrich.IMS.Entities
{
    public partial class CommEmployeeSetting
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string LevelNumber { get; set; }
        public string LevelName { get; set; }
        public decimal?CommPercent { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    }
}
