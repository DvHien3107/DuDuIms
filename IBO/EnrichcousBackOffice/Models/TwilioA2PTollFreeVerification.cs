//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EnrichcousBackOffice.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class TwilioA2PTollFreeVerification
    {
        public int Id { get; set; }
        public string StoreCode { get; set; }
        public string SId { get; set; }
        public string TwilioAccountSId { get; set; }
        public string CustomerProfileSId { get; set; }
        public string UseCaseCategories { get; set; }
        public string UseCaseSummary { get; set; }
        public string ProductionMessageSample { get; set; }
        public string OptInImageUrls { get; set; }
        public string OptInType { get; set; }
        public string AdditionalInformation { get; set; }
        public string ExternalReferenceId { get; set; }
        public string VerificationStatus { get; set; }
        public string PhoneNumber { get; set; }
        public string TollfreePhoneNumberSid { get; set; }
        public string MessageVolume { get; set; }
        public string LastHookJson { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public Nullable<bool> SyncToPosStatus { get; set; }
        public string SyncToPosType { get; set; }
        public Nullable<bool> PosUsingForOperation { get; set; }
        public Nullable<bool> PosUsingForPosPromotion { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<System.DateTime> TwilioUpdateDate { get; set; }
    }
}
