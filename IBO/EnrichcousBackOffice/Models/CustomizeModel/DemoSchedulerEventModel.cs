using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class DemoSchedulerEventModel
    {
        public DemoSchedulerEventModel()
        {
            AvailabelTimeZone = new List<SelectListItem>();
        }
        public string Id { get; set; }
        public string Name { get; set; }
        public string CustomerCode { get; set; }
        public string SalonName { get; set; }

        public string SalesLeadId { get; set; }
        public string Description { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int? DemoSchedulerId { get; set; }
        public string MemberNumber { get; set; }
        public string GoogleCalendarId { get; set; }
        public string TimeZoneNumber { get; set; }
        public string TimeZone { get; set; }
        public string Location { get; set; }
        public string AttendeesNumber { get; set; }
        public string AttendeesName { get; set; }
        public string REF { get; set; }
        public int? Status { get; set; }
        public List<SelectListItem> AvailabelTimeZone { get; set; }
    }
}