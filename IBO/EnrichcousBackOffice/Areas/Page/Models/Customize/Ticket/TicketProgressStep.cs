using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Areas.Page.Models.Customize.Ticket
{
    public partial class TicketProgressModel 
    {
        public string ProgressTitle { get; set; }
        public TicketProgressStep TicketProgressStep { get; set; }
    }

    public enum TicketProgressStep
    {
        NameAndEmail,
        Type,
        TicketInformation,
        Complete
    }
}