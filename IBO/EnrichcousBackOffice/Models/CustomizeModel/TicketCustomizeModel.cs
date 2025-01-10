using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{


    public class ForwardStage_view
    {
        public string Id { get; set; }
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string affected_version { get; set; }
        public string fixed_version { get; set; }
        public string status { get; set; }
        public string status_type { get; set; }
        public List<P_Member> assigned_member { get; set; }
        public bool? active { get; set; }
    }

    public class DevelopmetTicket_view
    {
        public T_SupportTicket Ticket { get; set; }
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string affected_version { get; set; }
        public string fixed_version { get; set; }
        public string status { get; set; }
        public string status_type { get; set; }
        public string assigned_member_numbers { get; set; }
        public string assigned_member_names { get; set; }
        public DateTime? closed_date { get; set; }
        public List<T_TicketStage_Status> shared_stages { get; set; }
    }
    public class TicketTimelineModel
    {
        public string date { get; set; }
        public IEnumerable<T_TicketFeedback> detail { get; set; }
    }

    [Table("T_SupportTicket", Schema = "dbo")]
    public class TicketDependencyModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreateByNumber { get; set; }
        public string CreateByName { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public Nullable<long> StatusId { get; set; }
        public string StatusName { get; set; }
        public string SubscribeMemberNumber { get; set; }
        public string SubscribeName { get; set; }
        public string AssignedToMemberNumber { get; set; }
        public string AssignedToMemberName { get; set; }
        public Nullable<long> GroupID { get; set; }
        public string GroupName { get; set; }
        public Nullable<System.DateTime> DateClosed { get; set; }
        public string CloseByMemberNumber { get; set; }
        public string CloseByName { get; set; }


    }

    public class Stage_view
    {
        public T_Project_Stage stage { get; set; }
        public List<StageVerMem_View> ver_members { get; set; }
    }
    public class StageVerMem_View
    {
        public T_Project_Milestone Version { get; set; }
        public List<Stage_member_view> ver_mems { get; set; }
    }
    public class Stage_member_view
    {
        public P_Member member { get; set; }
        public bool? IsLeader { get; set; }
    }
    //public class ForwardStage_view
    //{
    //    public string Id { get; set; }
    //    public string StageId { get; set; }
    //    public string StageName { get; set; }
    //    public string affected_version { get; set; }
    //    public string fixed_version { get; set; }
    //    public string status { get; set; }
    //    public string status_type { get; set; }
    //    public List<P_Member> assigned_member { get; set; }
    //    public bool? active { get; internal set; }
    //}
    //public class DevelopmetTicket_view
    //{
    //    public string Status { get; internal set; }
    //    public DateTime? closed_date { get; internal set; }
    //    public List<T_TicketStage_Status> shared_stages { get; internal set; }
    //    public long? Id { get; internal set; }
    //    public string Name { get; internal set; }
    //    public string CreateByName { get; internal set; }
    //    public string CustomerCode { get; internal set; }
    //    public string CustomerName { get; internal set; }
    //    public Priority Priority { get; internal set; }
    //    public long? SeverityId { get; internal set; }
    //    public string SeverityName { get; internal set; }
    //    public string Tags { get; internal set; }
    //    public string StatusType { get; internal set; }
    //    public string AssignedMember_Numbers { get; internal set; }
    //    public bool? Visible { get; internal set; }
    //    public string AssignedMember_Names { get; internal set; }
    //    public string AssignedMember_Avatar { get; internal set; }
    //    public long? Type { get; internal set; }
    //    public string TypeName { get; internal set; }
    //    public long? StatusId { get; internal set; }
    //    public List<T_TicketStage_Status> SharedStages { get; internal set; }
    //    public DateTime? CloseDate { get; internal set; }
    //    public string Updated { get; internal set; }
    //    public DateTime UpdatedDate { get; internal set; }
    //    public DateTime? CreateAt { get; internal set; }
    //    public string OpenedTimeAgo { get; internal set; }
    //    public T_Project_Stage stage_info { get; internal set; }
    //    public T_Project_Milestone version_info { get; internal set; }
    //    public string ProjectName { get; internal set; }
    //    public string VersionName { get; internal set; }
    //    public string VersionId { get; internal set; }
    //    public string StageName { get; internal set; }
    //    public string StageId { get; internal set; }
    //    public DateTime? Deadline { get; internal set; }
    //    public string GlobalStatus { get; internal set; }
    //    public string CreateByNumber { get; internal set; }
    //    public string TransferFrom { get; internal set; }
     
    //}

    public class TicketListView
    {
        public TicketListView()
        {
            Priority = new Priority();
        }
        public string Status { get; set; }
        public DateTime? closed_date { get; set; }
        public List<T_TicketStage_Status> shared_stages { get; set; }
        public long? Id { get; set; }
        public string Name { get; internal set; }
        public string CreateByName { get;  set; }
        public string AssignMemberNumbers { get; set; }
        public string AssignMemberNames { get; set; }
        public string AssignMemberAvatars { get; set; }
        public string OpenByName { get; set; }
        public string CloseByName { get; set; }
        public long? CustomerId { get;  set; }
        public string AccountManager { get; set; }
        public string CustomerCode { get;  set; }
        public string CustomerName { get;  set; }
        public string MemberTag { get; set; }
        public string SalonPhone { get; set; }
        public string OwnerPhone { get; set; }
        public string LicenseName { get; set; }
        public string LicenseExpiredDate { get; set; }
        public int? RemainingDate { get; set; }
        public Priority Priority { get; internal set; }
        public long? SeverityId { get;  set; }
        public string SeverityName { get;  set; }
        public string Tags { get;  set; }
        public string StatusType { get;  set; }
        public List<TicketStageModel> TicketStages { get; set; }
        public bool? Visible { get;  set; }
    
        public long? Type { get;  set; }
        public string TypeName { get; set; }
        public long? StatusId { get;  set; }
        public List<T_TicketStage_Status> SharedStages { get;  set; }
        public string CloseDate { get;  set; }
        public string CloseDateAgo { get; set; }
        public string Updated { get;  set; }
        public DateTime UpdatedDate { get;  set; }
        public DateTime? CreateAt { get;  set; }
        public string OpenedTimeAgo { get;  set; }
        public T_Project_Stage stage_info { get;  set; }
        public T_Project_Milestone version_info { get;  set; }
        public string ProjectName { get;  set; }
        public string ProjectId { get; set; }
        public string VersionName { get;  set; }
        public string VersionId { get;  set; }
        public string StageName { get;  set; }
        public string StageId { get;  set; }
        public string Deadline { get;  set; }
        public string DeadlineText { get; set; }
        public int? DeadlineLevel { get; set; }
        public string GlobalStatus { get;  set; }
        public string CreateByNumber { get; set; }
        public string DepartmentName { get; set; }
        public string Note { get; set; }
        public string OpenDate { get; set; }
        public string OpenDateAgo { get; set; }
        public DateTime? OpenDateByDate { get; set; }
        public string DetailUpdate { get; internal set; }

    }

    public class Priority
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class TicketStageModel
    {
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string AssignedMember_Numbers { get; set; }
        public string AssignedMember_Names { get; set; }
        public string AssignedMember_Avatar { get; set; }
    }

}