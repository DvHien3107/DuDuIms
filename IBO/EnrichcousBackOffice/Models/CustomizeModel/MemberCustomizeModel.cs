using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class MemberSelect_View
    {
        public string MemberNumber { get; internal set; }
        public string Name { get; internal set; }
        public string Department { get; internal set; }
        public string PersonalEmail { get; internal set; }
        public string CellPhone { get; internal set; }
    }

    public class MemberMan_View
    {
        public string MemberNumber { get; set; }
        public string DepartmentName { get; set; }
        public string FullName { get; set; }
        public string EmployeeId { get; set; }
        public string PersonalEmail { get; set; }
        public string CellPhone { get; set; }
        public string EmployeeStatus { get; set; }
        public string ReferedByName { get; set; }
        public Nullable<bool> Delete { get; set; }
        public int MaxRoleLevel { get; set; }
        public Nullable<bool> Active { get; set; }
        public string Avatar { get; set; }
        public string Gender { get; set; }
        public string CreateAt { get; set; }
        public string CreateBy { get; set; }
        public string BelongToPartner { get; set; }
        public bool? GoogleAuth { get; set; }
        public bool? IsSendEmailGoogleAuth { get; set; }
        public bool? IsCompleteProfile { get; set; }
    }
}