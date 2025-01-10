using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS.Views
{
    public class V_ManageTicket
    {
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string Tags { get; set; }
        public string TagMemberNumber { get; set; }
        public string AssignedToMemberNumber { get; set; }
        public string TypeId { get; set; }

        public long TicketId { get; set; }

        public string TicketName { get; set; }

        public string ProjectId { get; set; }

        public string ProjectName { get; set; }

        public DateTime? CreateAt { get; set; }

        public string CreateByName { get; set; }

        public string LeaderName { get; set; }

        public DateTime? Deadline { get; set; }

        public string StatusName { get; set; }

    }
    public class LoadTicket : V_ManageTicket
    {
        public List<T_Tags>? lstTags { get; set; }
        public List<P_Member>? lstAssigned { get; set; }
        public List<P_Member>? lstMember { get; set; }
    }
}
