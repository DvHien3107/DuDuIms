using Enrich.Dto.Base.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    [FieldDb(table: "P_Member")]
    public class MemberDto
    {
        [FieldDb("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Member number
        /// </summary>
        [FieldDb(nameof(MemberDto.MemberNumber))]
        public string MemberNumber { get; set; }

        /// <summary>
        /// Department Id
        /// </summary>
        [FieldDb(nameof(MemberDto.DepartmentId))]
        public string DepartmentId { get; set; }

        /// <summary>
        /// Department name
        /// </summary>
        [FieldDb(nameof(MemberDto.DepartmentName))]
        public string DepartmentName { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [FieldDb(nameof(MemberDto.FirstName))]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [FieldDb(nameof(MemberDto.LastName))]
        public string LastName { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        [FieldDb(nameof(MemberDto.FullName))]
        public string FullName { get; set; }

        /// <summary>
        /// login email, Email come from enrich domain
        /// </summary>
        [FieldDb(nameof(MemberDto.PersonalEmail))]
        public string PersonalEmail { get; set; }

        /// <summary>
        /// private personal 's email
        /// </summary>
        [FieldDb(nameof(MemberDto.Email1))]
        public string Email1 { get; set; }

        /// <summary>
        /// personal 's phone number
        /// </summary>
        [FieldDb(nameof(MemberDto.CellPhone))]
        public string CellPhone { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        [FieldDb(nameof(MemberDto.Password))]
        public string Password { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [FieldDb(nameof(MemberDto.Address))]
        public string Address { get; set; }

        /// <summary>
        /// Role code
        /// </summary>
        [FieldDb(nameof(MemberDto.RoleCode))] 
        public string RoleCode { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        [FieldDb(nameof(MemberDto.RoleName))]
        public string RoleName { get; set; }

        /// <summary>
        /// Active status
        /// </summary>
        [FieldDb(nameof(MemberDto.Active))]
        public bool? Active { get; set; }

        /// <summary>
        /// Update by
        /// </summary>
        [FieldDb(nameof(MemberDto.UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// Update at
        /// </summary>
        [FieldDb(nameof(MemberDto.UpdateAt))]
        public System.DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        [FieldDb(nameof(MemberDto.CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        [FieldDb(nameof(MemberDto.CreateAt))]
        public System.DateTime? CreateAt { get; set; }

        /// <summary>
        /// Birthday
        /// </summary>
        [FieldDb(nameof(MemberDto.Birthday))]
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        [FieldDb(nameof(MemberDto.Gender))]
        public string Gender { get; set; }

        /// <summary>
        /// Picture
        /// </summary>
        [FieldDb(nameof(MemberDto.Picture))]
        public string Picture { get; set; }


        /// <summary>
        /// Delete status
        /// </summary>
        [FieldDb(nameof(MemberDto.Delete))]
        public bool? Delete { get; set; }

        /// <summary>
        /// Belong to partner
        /// </summary>
        [FieldDb(nameof(MemberDto.BelongToPartner))]
        public string BelongToPartner { get; set; }

        /// <summary>
        /// Pin notification
        /// </summary>
        [FieldDb(nameof(MemberDto.PinNotification))]
        public bool? PinNotification { get; set; }

        /// <summary>
        /// timeZone
        /// </summary>
        [FieldDb(nameof(MemberDto.TimeZone))]
        public string TimeZone { get; set; }

        /// <summary>
        /// GMTNumber
        /// </summary>
        [FieldDb(nameof(MemberDto.GMTNumber))]
        public int? GMTNumber { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string DriverLicense { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string SocialSecurity { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ReferedByNumber { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ReferedByName { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string MemberType { get; set; }


        /// <summary>
        /// dont use
        /// </summary>
        public string MemberTypeName { get; set; }


        /// <summary>
        /// dont use
        /// </summary>
        public decimal? BaseSalary { get; set; }


        /// <summary>
        /// dont use
        /// </summary>
        public System.DateTime? JoinDate { get; set; }


        /// <summary>
        /// dont use
        /// </summary>
        public string TypeSalary { get; set; }


        /// <summary>
        /// dont use
        /// </summary>
        public string EmailTempPass { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string NoticeSubscribeCode { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string ProfileDefinedColor { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string GoogleAuth { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? IsSendEmailGoogleAuth { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? IsAuthorizedGoogle { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public bool? IsEnableWatchEvent { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public DateTime? EventWatchingExpirationTime { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string WorkEmail { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string Email2 { get; set; }

        public int? SiteId { get; set; }
    }
}
