using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public partial class LogDetailRICustomizeModel
    {
        public string SalesLeadId { get; set; }
        public string SalesLeadName { get; set; }

        public bool? StatusVerify { get; set; }
        public bool? IsSendEmailVerify { get; set; }
        public List<Calendar_Event> ListLog { get; set; }
        public Calendar_Event NextAppoiment { get; set; }
    }
    public class scanlog
    {
        public DateTime time { get; set; }
        public string log { get; set; }
        public string ex { get; set; }
    }
    public class IMSLog_scan
    {
        public List<scanlog> scanlogs { get; set; }
        public string ScanBy { get; internal set; }
        public DateTime? CreateAt { get; internal set; }
    }
}