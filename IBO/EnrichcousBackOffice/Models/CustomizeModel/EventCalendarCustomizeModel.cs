using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public partial class EventCalendarCustomizeModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string resourceId { get; set; }
        public string groupId { get; set; }
        public string display { get; set; }
        public string classNames { get; set; }
        public string backgroundColor { get; set; }
        public int[] daysOfWeek { get; set; }
        public string start { get; set; }
        public string end { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public Rrule rrule { get; set; }

        public string[] exdate { get; set; }
        public string duration { get; set; }
        public string timeZoneNumber { get; set; }
    }

    public partial class Rrule
    {
        public string freq { get; set; }
        public int interval { get; set; }
        public string[] byweekday { get; set; }
        public string dtstart { get; set; }
        public string until { get; set; }
    }
}