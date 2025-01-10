using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class TranferToModel
    {
     
        public T_SupportTicket Ticket { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public string DepartmentId { get; set; }
        public string BuildInCode { get; set; }
        public List<T_TicketStage_Status> StageTicket { get; set; }
        public P_Member TranferBy { get; set; }
        public List<T_TicketUpdateLog> ListUpdate { get; set; }
        public List<P_Member> AssignMember { get; set; }
    }
}