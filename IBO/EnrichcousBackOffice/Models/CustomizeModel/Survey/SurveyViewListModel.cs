using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Survey
{
    public class SurveyViewListModel
    {
        public int Id { get; set; }
        public int? Status { get; set; }
        public int? MinuteDuration { get; set; }
        public string SurveyName { get; set; }
        public string CreatedBy { get; set; }
        public double? AverageRate { get; set; } 
        public string StartDate { get; set; }
        public bool? ShowDelete { get; set; }
        public bool? Reopen { get; set; }
    }
}