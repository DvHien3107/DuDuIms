using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class TranferHistoryModel
    {
   
        public T_SupportTicket Ticket { get; set; }
        public string FromProjectId { get; set; }
        public string FromProjectName { get; set; }
        public string ToProjectId { get; set; }
        public string ToProjectName { get; set; }
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public string DepartmentId { get; set; }
        public string BuildInCode { get; set; }
        public string Note { get; set; }
        public DateTime? TransferAt { get; set; }
       // public List<T_TicketStage_Status> StageTicket { get; set; }
       // public List<T_TicketUpdateLog> ListUpdate { get; set; }
        public P_Member TranferBy { get; set; }
    }
}