using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Task
{
    public class TaskListModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Nullable<long> TicketId { get; set; }
        public string TicketName { get; set; }
        public Nullable<bool> Complete { get; set; }
        public string DueDate { get; set; }
        public Nullable<int> ReminderWeeklyAt { get; set; }
        public Nullable<int> ReminderMonthlyAt { get; set; }
        public string AssignedToMemberNumber { get; set; }
        public string AssignedToMemberName { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateAt { get; set; }
        public Nullable<long> ParentTaskId { get; set; }
        public string ParentTaskName { get; set; }
        public string CreateByMemberNumber { get; set; }
        public Nullable<bool> Reminded { get; set; }
        public Nullable<System.DateTime> CompletedDate { get; set; }
        public int? TotalSubtask { get; set; }
        public int? SubtaskComplete { get; set; }
        public string OpenDate { get; set; }
    }
}