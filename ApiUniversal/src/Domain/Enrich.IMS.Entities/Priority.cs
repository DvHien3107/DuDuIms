namespace Enrich.IMS.Entities
{
    public partial class Priority
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Color { get; set; }
        public bool? IsDeleted { get; set; }
        public int? DisplayOrder { get; set; }
        public System.DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public System.DateTime? UpdatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public int? DeadLineOfHours { get; set; }
    }
}
