using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public partial class IndexTicketModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Page { get; set; }
        public string BuildInCode { get; set; }

        public bool AllProject { get; set; }
        public bool AllStage { get; set; }
        public bool AllVersion { get; set; }

        public  List<T_Project_Stage> ListStage { get; set; }
        public T_Project_Milestone Project {get;set; }

        public T_Project_Stage Stage { get; set; }
        public T_Project_Milestone Version { get; set; }
    }
}