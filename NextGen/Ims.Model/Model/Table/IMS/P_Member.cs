using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class P_Member
    {
        public long Id { get; set; }

        public string MemberNumber { get; set; }

        public string DepartmentId { get; set; }

        public string DepartmentName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string FullName { get; set; }

        public string PersonalEmail { get; set; }

        public string Email1 { get; set; }

        public string Email2 { get; set; }

        public string CellPhone { get; set; }

        public string Password { get; set; }

        public string DriverLicense { get; set; }

        public string SocialSecurity { get; set; }

        public string Address { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string Country { get; set; }

        public string ReferedByNumber { get; set; }

        public string ReferedByName { get; set; }

        public string RoleCode { get; set; }

        public string RoleName { get; set; }

        public string MemberType { get; set; }

        public string MemberTypeName { get; set; }

        public decimal? BaseSalary { get; set; }

        public DateTime? JoinDate { get; set; }

        public string TypeSalary { get; set; }

        public bool? Active { get; set; }

        public string UpdateBy { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string CreateBy { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? Birthday { get; set; }

        public string Gender { get; set; }

        public string Picture { get; set; }

        public string EmailTempPass { get; set; }

        public bool? Delete { get; set; }

        public string NoticeSubscribeCode { get; set; }

        public string ProfileDefinedColor { get; set; }

        public string GoogleAuth { get; set; }

        public bool? IsSendEmailGoogleAuth { get; set; }

        public bool? IsAuthorizedGoogle { get; set; }

        public bool? IsEnableWatchEvent { get; set; }

        public DateTime? EventWatchingExpirationTime { get; set; }

        public string WorkEmail { get; set; }

        public string BelongToPartner { get; set; }

        public bool? PinNotification { get; set; }

        public string TimeZone { get; set; }

        public int? GMTNumber { get; set; }

        public string EmployeeId { get; set; }

        public string IdentityCardNumber { get; set; }

        public string PersonalIncomeTax { get; set; }

        public DateTime? ProbationDate { get; set; }

        public DateTime? EmploymentContractDate { get; set; }

        public DateTime? TerminateContractDate { get; set; }

        public string IdentityCardImageBefore { get; set; }

        public string IdentityCardImageAfter { get; set; }

        public bool? IsSendEmailUpdateInfo { get; set; }

        public bool? IsCompletedUpdateInfo { get; set; }

        public bool? IsCompleteProfile { get; set; }

        public string SocialInsuranceCode { get; set; }

        public string EmployeeStatus { get; set; }

        public string JobPosition { get; set; }

        public int? SiteId { get; set; }

    }
}
