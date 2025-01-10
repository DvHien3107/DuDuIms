using System.Collections.Generic;

namespace Enrich.IMS.Entities
{
    public partial class TaskTemplateCategory
    {
        public TaskTemplateCategory()
        {
            this.Ts_TaskTemplateField = new HashSet<TaskTemplateField>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TicketGroup { get; set; }
        public System.DateTime? CreatedAt { get; set; }
        public string CreateBy { get; set; }
        public bool? Requirement { get; set; }
        public bool? Status { get; set; }
        public bool? IsDeleted { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
    
        public virtual ICollection<TaskTemplateField> Ts_TaskTemplateField { get; set; }
    }
}
