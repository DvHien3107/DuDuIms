namespace Enrich.IMS.Entities
{
    public partial class TicketStage_Status
    {
        public string Id { get; set; }
        public long? TicketId { get; set; }
        public string StageId { get; set; }
        public string StageName { get; set; }
        public string AffectedVersion { get; set; }
        public string FixedVersion { get; set; }
        public string AssignedMember_Numbers { get; set; }
        public string AssignedMember_Names { get; set; }
        public long? StatusId { get; set; }
        public bool? Active { get; set; }
        public System.DateTime? CloseDate { get; set; }
        public string ProjectVersionId { get; set; }
        public string ProjectVersionName { get; set; }
        public System.DateTime? OpenDate { get; set; }
        public long? TypeId { get; set; }
    }
}
