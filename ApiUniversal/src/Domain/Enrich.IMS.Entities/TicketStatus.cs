using System.Collections.Generic;

namespace Enrich.IMS.Entities
{    
    public partial class TicketStatus
    {
       
        public TicketStatus()
        {
            this.T_TicketStatusMapping = new HashSet<TicketStatusMapping>();
        }
    
        public long Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public long? TicketTypeId { get; set; }
        public string TicketTypeName { get; set; }
        public int? Order { get; set; }
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string ProjectVersionId { get; set; }
        public string ProjectVersionName { get; set; }
        public string ProjectId { get; set; }
        public bool? IsDeleted { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }    
        public virtual ICollection<TicketStatusMapping> T_TicketStatusMapping { get; set; }
    }
}
