using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Nofitication
{
    public class NotificationQueryModel
    {
        public int Id { get; set; }
        public string Description { get; set; }

        public bool? IsView { get; set; }
        public string Url { get; set; }
        public int TemplateId { get; set; }
        public string Category { get; set; }
        public string MemberNumber { get; set; }
        public int? UpdateId { get; set; }
        public string CreateBy { get; set; }
        

        public string EntityId { get; set; }
        public string Image { get; set; }
        public DateTime? ViewTime { get; set; }
        public DateTime CreateAt { get; set; }
        public string GroupTitle { get; set; }
    }
}