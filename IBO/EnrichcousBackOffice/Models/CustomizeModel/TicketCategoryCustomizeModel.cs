using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class T_TicketCategory_custom
    {
        public string name { get; set; }
        public string id { get; set; }
        public string parent { get; set; }
        public int count { get; set; }
    }
    public class T_SupportTicketKB_custom
    {
        public long Id { get; set; }
        public string name { get; set; }
        public string decs { get; set; }
        public string categoryId { get; internal set; }
    }
    public class TicketFeedback_Custom
    {
        public long Id { get; set; }
        public string Feedback { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateAt { get; set; }
        public string Avatar { get; set; }
        public List<UploadMoreFile> AttachFile { get; internal set; }
        public string Attachment { get; set; } 
    }
    public class T_TicketAttribute_GroupView
    {
        public T_TicketAttribute parent { get; set; }
        public List<T_TicketAttribute> child { get; set; }
    }
}