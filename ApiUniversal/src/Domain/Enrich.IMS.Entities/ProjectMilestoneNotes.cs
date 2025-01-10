namespace Enrich.IMS.Entities
{
    public partial class ProjectMilestoneNotes
    {
        public string Id { get; set; }
        public string MilestoneId { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public int? Order { get; set; }
    }
}
