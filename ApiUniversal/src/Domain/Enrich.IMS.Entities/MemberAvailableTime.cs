using System;

namespace Enrich.IMS.Entities
{    public partial class MemberAvailableTime
    {
        public int Id { get; set; }
        public string MemberNumber { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public System.DateTime? StartDate { get; set; }
        public System.DateTime? EndDate { get; set; }
        public string DaysOfWeek { get; set; }
        public System.DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string TimeZone { get; set; }
        public string Type { get; set; }
        public System.DateTime? UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public bool? Status { get; set; }
        public string TimeZoneNumber { get; set; }
        public string Note { get; set; }
    }
}
