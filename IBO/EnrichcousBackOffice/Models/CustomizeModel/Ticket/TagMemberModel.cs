using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel.Ticket
{
    public class AssignMemberModel
    {
        public string MemberNumber { get; set; }
        public string MemberName { get; set; }
        public string Gender { get; set; }
        public string Avatar { get; set; }
        public bool Selected { get; set; }

    }
}