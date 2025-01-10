namespace Enrich.IMS.Entities
{
    public partial class TaskTemplateField
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public string TypeField { get; set; }
        public bool? IsRequired { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public int? DisplayOrder { get; set; }
        public string Description { get; set; }
    
        public virtual TaskTemplateCategory Ts_TaskTemplateCategory { get; set; }
    }
}
