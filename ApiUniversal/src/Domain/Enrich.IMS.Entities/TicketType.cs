namespace Enrich.IMS.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class TicketType
    {
        public TicketType()
        {
            this.T_TicketTypeMapping = new HashSet<TicketTypeMapping>();
        }
    
        public long Id { get; set; }
        public string TypeName { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public bool? Active { get; set; }
        public int? Order { get; set; }
        public string SpecialType { get; set; }
        public string BuildInCode { get; set; }
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string ProjectVersionId { get; set; }
        public string ProjectVersionName { get; set; }
        public string ProjectId { get; set; }
        public bool? IsDeleted { get; set; }    
        public virtual ICollection<TicketTypeMapping> T_TicketTypeMapping { get; set; }
    }
}
