using System.ComponentModel.DataAnnotations.Schema;

namespace Enrich.IMS.Entities
{
    public partial class SalesLead
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        /// <summary>
        /// Status 
        /// Exp: lead, merchant, slice, trial
        /// </summary>
        public int? Status { get; set; }

        /// <summary>
        /// Exp: lead, merchant, slice, trial. Need improve
        /// </summary>
        public string StatusName { get; set; }

        /// <summary>
        /// Star rating
        /// </summary>
        public int? PotentialRateScore { get; set; }

        /// <summary>
        /// Sale person
        /// </summary>
        public string MemberNumber { get; set; }

        /// <summary>
        /// Customer code. Null when first create
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// use SalonName
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>        
        /// Features of interest
        /// </summary>
        public string FeaturesInteres { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        public System.DateTime? CreateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        public string CreateBy { get; set; }

        /// <summary>
        /// Update at
        /// </summary>
        public System.DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Update by
        /// </summary>
        public string UpdateBy { get; set; }

        /// <summary>
        /// Salon name
        /// </summary>
        public string SalonName { get; set; }

        /// <summary>
        /// Address (Street)
        /// </summary>
        public string SalonAddress { get; set; }

        /// <summary>
        /// State
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// City 
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Zipcode
        /// </summary>
        public string Zipcode { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Salon phone
        /// </summary>
        public string SalonPhone { get; set; }

        /// <summary>
        /// Contact name
        /// </summary>
        public string ContactName { get; set; }

        /// <summary>
        /// Contact phone
        /// </summary>
        public string ContactPhone { get; set; }

        /// <summary>
        /// Spa/Salon Information
        /// </summary>
        public string MoreInfo { get; set; }

        /// <summary>
        /// Type. Use to determine: import data, lead, unveryfied
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Salon email 
        /// </summary>
        public string SalonEmail { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        public bool? IsVerify { get; set; }

        /// <summary>
        ///  need to check
        /// </summary>
        public bool? IsSendMail { get; set; }

        /// <summary>
        ///  need to check
        /// </summary>
        public string CreateTrialBy { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        public System.DateTime? CreateTrialAt { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        public string Product { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Time zone
        /// </summary>
        public string SalonTimeZone { get; set; }

        /// <summary>
        /// Salon timezone number
        /// </summary>
        public string SalonTimeZoneNumber { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        public long? TeamNumber { get; set; }

        /// <summary>
        /// Interaction Status
        /// </summary>
        public string InteractionStatus { get; set; }

        /// <summary>
        /// Call Of Number
        /// </summary>
        public string CallOfNumber { get; set; }

        /// <summary>
        /// who sale
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        ///  sale name of team
        /// </summary>
        public string TeamName { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        public string LicenseName { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        public string LicenseCode { get; set; }

        /// <summary>
        /// Last update. Need improve
        /// </summary>
        public string LastUpdateDesc { get; set; }

        /// <summary>
        /// Last note time. Need improve
        /// </summary>
        public System.DateTime? LastNoteAt { get; set; }

        /// <summary>
        /// Last note. Need improve
        /// </summary>
        public string LastNote { get; set; }

        /// <summary>
        /// Interaction status id
        /// </summary>
        public int? InteractionStatusId { get; set; }

        /// <summary>
        /// Create by memberNumber
        /// </summary>
        public string CreateByMemberNumber { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        public string FeaturesInteresOther { get; set; }

        public string ReferralSource { get; set; }
    }
}
