using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class TranferModel
    {
        public long TicketId { get; set; }
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string VersionId { get; set; }
        public string VersionName { get; set; }
        public long? Department { get; set; }
        public string Stage { get; set; }
        public string Assign { get; set; }
        [AllowHtml]
        public string transfer_note { get; set; }
    }
}