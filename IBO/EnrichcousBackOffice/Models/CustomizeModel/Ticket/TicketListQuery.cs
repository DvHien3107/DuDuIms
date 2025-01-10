using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class Project_StageVersion
    {
        public T_Project_Stage stage { get; internal set; }
        public T_Project_Milestone version { get; internal set; }
    }
    public class TicketListQuery
    {
        public T_SupportTicket SupportTicket { get; set; }
        //public T_TicketStatus Status { get; set; }
        //public T_TicketType Type { get; set; }
        public T_TicketStage_Status Stage { get; set; }
      //  public T_Project_Milestone Version { get; set; }
      //  public T_Priority Priority { get; set; }
   //     public Project_StageVersion StageVersion { get; set; }
      //  public IGrouping<long?, T_TicketStage_Status> ShareStage { get; set; }
      //  public Store_Services storeService { get; set; }
      //  public C_Customer Customer {get;set;}
    

        //public T_Project_Milestone AffectedVersion { get; set; }
        //public T_Project_Milestone FixedVersion { get; set; }

    }
    public class TicketListQueryGroupById
    {
        public T_SupportTicket SupportTicket { get; set; }
      
        public IEnumerable<T_TicketStage_Status> Stage { get; set; }

    }
    //public class CustomerTicketModel
    //{
    //    public string CustomerCode { get; set; }
    //    public string CustomerName { get; set; }
    //    public string SalonPhone { get; set; }
    //    public string OwnerPhone { get; set; }

    //}
}