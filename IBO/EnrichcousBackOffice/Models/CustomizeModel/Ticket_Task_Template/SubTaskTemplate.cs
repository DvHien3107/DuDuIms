using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket_Task_Template
{
    public class SubTaskTemplate
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Complete { get; set; }
     
        public bool? Required { get; set; }
        public List<UploadMoreFile> Files { get; set; }
    }
}