using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket_Task_Template
{
    public class TicketTaskTemplateDetail
    {
        public long Id { get; set; }
        public long? TicketId { get; set; }
        public string TaskName { get; set; }
        public string Description { get; set; }
        public string[] AssignMemberNumber { get; set; }
        public string AssignMemberName { get; set; }
        public bool? Complete { get; set; }
        public bool? Requirement { get; set; }
        public int TaskTemplateCategoryId { get; set; }
        public string TaskTemplateCategoryName { get; set; }
      
        public string Note { get; set; }
        public List<SubTaskTemplate> SubTaskTemplateList { get; set; }
    }
}