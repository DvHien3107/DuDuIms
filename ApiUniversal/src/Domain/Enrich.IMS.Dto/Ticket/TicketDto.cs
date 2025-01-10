using Enrich.Dto.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Ticket
{
    public partial class TicketDto
    {
        public long Id { get; set; }

        [GridField(Index = 1, IsDefault = true, IsShow = true, CanSort = true), SqlMapDto("Name")]
        public string Name { get; set; }
        public string Description { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string ContactEmail { get; set; }
        public string CreateByNumber { get; set; }
        public string CreateByName { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string StatusId { get; set; }
        public string StatusName { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? Deadline { get; set; }
        public string TypeId { get; set; }
        public string TypeName { get; set; }
        public System.DateTime? DateOpened { get; set; }
        public string OpenByMemberNumber { get; set; }
        public string OpenByName { get; set; }
        public bool? Visible { get; set; }
        public bool? KB { get; set; }
        public string ProductId { get; set; }
        public string Productname { get; set; }
        public string RelatedTicketId { get; set; }
        public string SubscribeMemberNumber { get; set; }
        public string SubscribeName { get; set; }
        public string AssignedToMemberNumber { get; set; }
        public string AssignedToMemberName { get; set; }
        public long? SeverityId { get; set; }
        public string SeverityName { get; set; }
        public long? GroupID { get; set; }
        public string GroupName { get; set; }
        public System.DateTime? DateClosed { get; set; }
        public string CloseByMemberNumber { get; set; }
        public string CloseByName { get; set; }
        public string UpdateTicketHistory { get; set; }
        public string ReassignedToMemberNumber { get; set; }
        public string ReassignedToMemberName { get; set; }
        public string EscalateToStageId { get; set; }
        public string EscalateToStageNote { get; set; }
        public string FeedbackTicketHistory { get; set; }
        public string GlobalStatus { get; set; }
        public string OrderCode { get; set; }
        public string Tags { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string MilestoneId { get; set; }
        public string MilestoneName { get; set; }
        public string Mail_MessageId { get; set; }
        public string PriorityName { get; set; }
        public string FixedMilestoneId { get; set; }
        public string FixedMilestoneName { get; set; }
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string AttributeIds { get; set; }
        public string IdentityType { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public string UploadIds { get; set; }
        public string AffectedVersionId { get; set; }
        public string FixedVersionId { get; set; }
        public string TagName { get; set; }
        public string AssignJson { get; set; }
        public string TagMemberNumber { get; set; }
        public string TagMemberName { get; set; }
        public string Note { get; set; }
        public string AffectedVersionName { get; set; }
        public string FixedVersionName { get; set; }
        public string OtherGroupId { get; set; }
        public string OtherGroupName { get; set; }
        public System.DateTime? EstimatedCompletionTimeFrom { get; set; }
        public System.DateTime? EstimatedCompletionTimeTo { get; set; }
        public int? PriorityId { get; set; }
        public string JiraIssueLink { get; set; }

    }
}
