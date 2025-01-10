namespace Enrich.IMS.Entities
{
    public partial class IMSTask
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long? TicketId { get; set; }
        public string TicketName { get; set; }
        public bool? Complete { get; set; }
        public System.DateTime? DueDate { get; set; }
        public int? ReminderWeeklyAt { get; set; }
        public int? ReminderMonthlyAt { get; set; }
        public string AssignedToMemberNumber { get; set; }
        public string AssignedToMemberName { get; set; }
        public string CreateBy { get; set; }
        public System.DateTime? CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public System.DateTime? UpdateAt { get; set; }
        public long? ParentTaskId { get; set; }
        public string ParentTaskName { get; set; }
        public string CreateByMemberNumber { get; set; }
        public bool? Reminded { get; set; }
        public System.DateTime? CompletedDate { get; set; }
        public int? TaskTemplateCategoryId { get; set; }
        public bool? Requirement { get; set; }
        public string Note { get; set; }
    }
}
