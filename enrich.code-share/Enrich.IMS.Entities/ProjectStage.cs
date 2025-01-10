namespace Enrich.IMS.Entities
{    
    public partial class ProjectStage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Desc { get; set; }
        public int? MemberCount { get; set; }
        public string BuildInCode { get; set; }
    }
}
