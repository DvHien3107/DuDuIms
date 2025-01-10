using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class JsonAssignedModel
    {
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string AssignMemberNumber { get; set; }
        public string AssignMemberName { get; set; }
    }
}