namespace Enrich.IMS.Entities
{   
    public partial class ProjectStageMembers
    {
        public string Id { get; set; }
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string ProjectVersionId { get; set; }
        public string ProjectVersionName { get; set; }
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public bool? IsLeader { get; set; }
    }
}
