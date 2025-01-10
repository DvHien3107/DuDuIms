using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.ViewModel
{
    public class TimeZoneModel
    {
        public string Name { get; set; }
        public string TimeDT { get; set; }
        public bool IsUsingST { get; set; }
    }

    public class ResApiListTimeZone {
        public int status { get; set; }
        public List<TimeZoneModel> lst { get; set; }
        
    }
}