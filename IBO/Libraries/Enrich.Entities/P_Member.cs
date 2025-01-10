using Enrich.Core.UnitOfWork.Data;
using System;
using System.Collections.Generic;

namespace Enrich.Entities
{
    
    public partial class P_Member: BaseEntity
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
        public Nullable<decimal> BaseSalary { get; set; }
        public Nullable<System.DateTime> JoinDate { get; set; }
        public string TypeSalary { get; set; }
        public Nullable<bool> Active { get; set; }
        public string UpdateBy { get; set; }
        public Nullable<System.DateTime> UpdateAt { get; set; }
        public string CreateBy { get; set; }
        public Nullable<System.DateTime> CreateAt { get; set; }
        public Nullable<System.DateTime> Birthday { get; set; }
        public string Gender { get; set; }
        public string Picture { get; set; }
        public string EmailTempPass { get; set; }
        public Nullable<bool> Delete { get; set; }
        public string NoticeSubscribeCode { get; set; }
        public string ProfileDefinedColor { get; set; }
        public string GoogleAuth { get; set; }
        public Nullable<bool> IsSendEmailGoogleAuth { get; set; }
        public Nullable<bool> IsAuthorizedGoogle { get; set; }
        public Nullable<bool> IsEnableWatchEvent { get; set; }
        public Nullable<System.DateTime> EventWatchingExpirationTime { get; set; }
        public string WorkEmail { get; set; }
        public string BelongToPartner { get; set; }
        public Nullable<bool> PinNotification { get; set; }
        public string TimeZone { get; set; }
        public Nullable<int> GMTNumber { get; set; }
        public string EmployeeId { get; set; }
        public string IdentityCardNumber { get; set; }
        public string PersonalIncomeTax { get; set; }
        public Nullable<System.DateTime> ProbationDate { get; set; }
        public Nullable<System.DateTime> EmploymentContractDate { get; set; }
        public Nullable<System.DateTime> TerminateContractDate { get; set; }
        public string IdentityCardImageBefore { get; set; }
        public string IdentityCardImageAfter { get; set; }
        public Nullable<bool> IsSendEmailUpdateInfo { get; set; }
        public Nullable<bool> IsCompletedUpdateInfo { get; set; }
    }
}
