using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Entities
{
    public class Member
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Id { get; set; }

        /// <summary>
        /// Member number
        /// </summary>
        public string MemberNumber { get; set; }

        /// <summary>
        /// Department Id
        /// </summary>
        public string DepartmentId { get; set; }

        /// <summary>
        /// Department name
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// login email, Email come from enrich domain
        /// </summary>
        public string PersonalEmail { get; set; }

        /// <summary>
        /// private personal 's email
        /// </summary>
        public string Email1 { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string Email2 { get; set; }

        /// <summary>
        /// personal 's phone number
        /// </summary>
        public string CellPhone { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Role code
        /// </summary>
        public string RoleCode { get; set; }

        /// <summary>
        /// Role name
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Active status
        /// </summary>
        public bool? Active { get; set; }

        /// <summary>
        /// Update by
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// Update at
        /// </summary>
        public System.DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        public System.DateTime? CreateAt { get; set; }

        /// <summary>
        /// Birthday
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// Gender
        /// </summary>
        public string Gender { get; set; }

        /// <summary>
        /// Picture
        /// </summary>
        public string Picture { get; set; }


        /// <summary>
        /// Delete status
        /// </summary>
        public bool? Delete { get; set; }

        /// <summary>
        /// Belong to partner
        /// </summary>
        public string BelongToPartner { get; set; }

        /// <summary>
        /// Pin notification
        /// </summary>
        public bool? PinNotification { get; set; }

        /// <summary>
        /// timeZone
        /// </summary>
        public string TimeZone { get; set; }

        /// <summary>
        /// GMTNumber
        /// </summary>

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

        public int SiteId { get; set; }
    }
}
