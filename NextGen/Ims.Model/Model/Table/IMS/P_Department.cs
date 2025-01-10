using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class P_Department
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string LeaderNumber { get; set; }

        public string LeaderName { get; set; }

        public bool? Active { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateAt { get; set; }

        public string UpdateBy { get; set; }

        public DateTime? UpdateAt { get; set; }

        public long? ParentDepartmentId { get; set; }

        public string ParentDepartmentName { get; set; }

        public string GroupMemberNumber { get; set; }

        public string GroupMemberName { get; set; }

        public string Type { get; set; }

        public string SupervisorNumber { get; set; }

        public string SupervisorName { get; set; }

        public string SaleStates { get; set; }

        public string PartnerCode { get; set; }

        public string PartnerName { get; set; }

        public int? SiteId { get; set; }

    }

    public class LoadDepartment
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string LeaderNumber { get; set; }

        public string LeaderName { get; set; }

        public bool? Active { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateAt { get; set; }

        public string UpdateBy { get; set; }

        public DateTime? UpdateAt { get; set; }

        public long? ParentDepartmentId { get; set; }

        public string ParentDepartmentName { get; set; }

        public string GroupMemberNumber { get; set; }

        public string GroupMemberName { get; set; }

        public string Type { get; set; }

        public string SupervisorNumber { get; set; }

        public string SupervisorName { get; set; }

        public string SaleStates { get; set; }

        public string PartnerCode { get; set; }

        public string PartnerName { get; set; }

        public int? SiteId { get; set; }

        public List<V_TicketType>? TicketTypes { get; set; }


    }
    public class V_TicketType
    {
        public long Id { get; set; }
        public string TypeName { get; set; }
        public string BuildInCode { get; set; }


    }
}
