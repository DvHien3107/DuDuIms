using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Task
{
    public class TaskTemplateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string TicketGroup { get; set; }
        public bool? Requirement { get; set; }
        public bool? Status { get; set; }
        public DateTime? UpdateAt { get; set; }
        public string UpdateBy { get; set; }
        public  List<TaskTemplateFieldModel> SubTaskTemplateList { get; set; }
    }
}