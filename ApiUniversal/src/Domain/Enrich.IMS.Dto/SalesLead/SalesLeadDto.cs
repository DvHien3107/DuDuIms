using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Attributes;
using System;

namespace Enrich.IMS.Dto
{

    [FieldDb(table: $"{SqlTables.SalesLead}")]
    public partial class SalesLeadDto
    {
        [FieldDb(nameof(Id))]
        public string Id { get; set; }

        /// <summary>
        /// Status 
        /// Exp: lead, merchant, slice, trial
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Status}")]
        public int? Status { get; set; } = (int)SalesLeadEnum.Status.Lead;

        /// <summary>
        /// Exp: lead, merchant, slice, trial. Need improve
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.StatusName}")]
        public string StatusName { get; set; } = EnumHelper.DisplayName(SalesLeadEnum.Status.Lead);

        /// <summary>
        /// Star rating
        /// </summary>
        [FieldDb(nameof(PotentialRateScore))] 
        public int? PotentialRateScore { get; set; }

        /// <summary>
        /// Sale person
        /// </summary>
        [FieldDb(nameof(MemberNumber))]
        public string MemberNumber { get; set; }

        /// <summary>
        /// Customer code. Null when first create
        /// </summary>
        [FieldDb(nameof(CustomerCode))]
        public string CustomerCode { get; set; }

        /// <summary>
        /// use SalonName
        /// </summary>
        [FieldDb(nameof(CustomerName))]
        public string CustomerName { get; set; }

        /// <summary>        
        /// Features of interest
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.FeaturesInteres}")] 
        public string FeaturesInteres { get; set; }

        /// <summary>
        /// Create at
        /// </summary>
        [FieldDb(nameof(CreateAt))]
        public DateTime? CreateAt { get; set; }

        /// <summary>
        /// Create by
        /// </summary>
        [FieldDb(nameof(CreateBy))]
        public string CreateBy { get; set; }

        /// <summary>
        /// Update at
        /// </summary>
        [FieldDb(nameof(UpdateAt))]
        public DateTime? UpdateAt { get; set; }

        /// <summary>
        /// Update by
        /// </summary>
        [FieldDb(nameof(UpdateBy))]
        public string UpdateBy { get; set; }

        /// <summary>
        /// Salon name
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.SalonName}")] 
        public string SalonName { get; set; }

        /// <summary>
        /// Address (Street)
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Address}")] 
        public string SalonAddress { get; set; }

        /// <summary>
        /// State
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.State}")] 
        public string State { get; set; }

        /// <summary>
        /// City 
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.City}")] 
        public string City { get; set; }

        /// <summary>
        /// Zipcode
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Zipcode}")] 
        public string Zipcode { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Country}")] 
        public string Country { get; set; }

        /// <summary>
        /// Salon phone
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Phone}")] 
        public string SalonPhone { get; set; }

        /// <summary>
        /// Contact name (Customer OwnerName)
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.ContactName}")] 
        public string ContactName { get; set; }

        /// <summary>
        /// Contact phone
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.ContactPhone}")] 
        public string ContactPhone { get; set; }

        /// <summary>
        /// Spa/Salon Information
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.MoreInfo}")] 
        public string MoreInfo { get; set; }

        /// <summary>
        /// Type. Use to determine: import data, lead, unveryfied
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Type}")] 
        public string Type { get; set; } = EnumHelper.DisplayName(SalesLeadEnum.Type.RegisterOnIMS);

        /// <summary>
        /// Password
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Password}")] 
        public string Password { get; set; }

        /// <summary>
        /// Salon email 
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Email}")] 
        public string SalonEmail { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.IsVerify}")] 
        public bool? IsVerify { get; set; }

        /// <summary>
        ///  dont use
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.IsSendMail}")] 
        public bool? IsSendMail { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.CreateTrialBy}")] 
        public string CreateTrialBy { get; set; }

        /// <summary>
        /// dont use
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.CreateTrialAt}")] 
        public DateTime? CreateTrialAt { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Product}")] 
        public string Product { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.Version}")] 
        public string Version { get; set; }

        /// <summary>
        /// Time zone
        /// </summary>
        [FieldDb(nameof(SalonTimeZone))]
        public string SalonTimeZone { get; set; } = TIMEZONE.Eastern.ToString();

        /// <summary>
        /// Salon timezone number
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.SalonTimeZoneNumber}")] 
        public string SalonTimeZoneNumber { get; set; } = EnumHelper.DisplayName(TIMEZONE.Eastern);

        /// <summary>
        /// need to check
        /// </summary>
        [FieldDb(nameof(TeamNumber))] 
        public long? TeamNumber { get; set; }

        /// <summary>
        /// Interaction Status
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.InteractionStatus}")] 
        public string InteractionStatus { get; set; }

        /// <summary>
        /// Call Of Number
        /// </summary>
        [FieldDb(nameof(CallOfNumber))]
        public string CallOfNumber { get; set; }

        /// <summary>
        /// who sale
        /// </summary>
        [FieldDb(nameof(MemberName))]
        public string MemberName { get; set; }

        /// <summary>
        ///  sale name of team
        /// </summary>
        [FieldDb(nameof(TeamName))]
        public string TeamName { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.LicenseName}")] 
        public string LicenseName { get; set; }

        /// <summary>
        /// need to check
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.LicenseCode}")] 
        public string LicenseCode { get; set; }

        /// <summary>
        /// Last update. Need improve
        /// </summary>
        [FieldDb(nameof(LastUpdateDesc))]
        public string LastUpdateDesc { get; set; }

        /// <summary>
        /// Last note time. Need improve
        /// </summary>
        [FieldDb(nameof(LastNoteAt))]
        public DateTime? LastNoteAt { get; set; }

        /// <summary>
        /// Last note. Need improve
        /// </summary>
        [FieldDb(nameof(LastNote))]
        public string LastNote { get; set; }

        /// <summary>
        /// Interaction status id
        /// </summary>
        [FieldDb($"{SqlColumns.SalesLead.InteractionStatusId}")] 
        public int? InteractionStatusId { get; set; }

        /// <summary>
        /// Create by memberNumber
        /// </summary>
        [FieldDb(nameof(CreateByMemberNumber))]
        public string CreateByMemberNumber { get; set; }
    }
}
