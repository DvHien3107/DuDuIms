using System.Collections.Generic;

namespace Enrich.IMS.Entities
{    
    public partial class Notification
    {    
        public Notification()
        {
            this.P_NotificationMapping = new HashSet<NotificationMapping>();
        }
    
        public int Id { get; set; }
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public string Data { get; set; }
        public string Category { get; set; }
        public string Action { get; set; }
        public int TemplateId { get; set; }
        public string EntityId { get; set; }
        public string EntityCode { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string CreateBy { get; set; }
        public int? UpdateId { get; set; }
        public string EntityName { get; set; }    
        public virtual NotificationTemplate P_NotificationTemplate { get; set; }      
        public virtual ICollection<NotificationMapping> P_NotificationMapping { get; set; }
    }
}
