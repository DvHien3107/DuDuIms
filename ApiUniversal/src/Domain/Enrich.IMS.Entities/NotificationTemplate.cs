namespace Enrich.IMS.Entities
{
    using System;
    using System.Collections.Generic;

    public partial class NotificationTemplate
    {
        public NotificationTemplate()
        {
            this.P_Notification = new HashSet<Notification>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }

        public virtual ICollection<Notification> P_Notification { get; set; }
    }
}
