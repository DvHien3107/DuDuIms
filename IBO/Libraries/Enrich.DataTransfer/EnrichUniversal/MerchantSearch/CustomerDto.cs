using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Enrich.DataTransfer
{
    public class CustomerDto
    {
        public long Id { get; set; }
        public string CustomerCode { get; set; }
        public string ContactName { get; set; }
        public string CreateBy { get; set; }
        public DateTime? CreateAt { get; set; }
        public string UpdateBy { get; set; }
        public string StoreCode { get; set; }
        public string Password { get; set; }
        public string Type { get; set; }
        public string MangoEmail { get; set; }
        public int? Active { get; set; }

        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string Email { get; set; }
        public string MemberNumber { get; set; }
        public string FullName { get; set; }
        public string CellPhone { get; set; }
        public string Zipcode { get; set; }
        public string BusinessName { get; set; }
        public string SalonEmail { get; set; }
        public string SalonPhone { get; set; }
        public string SalonAddress1 { get; set; }
        public string SalonState { get; set; }
        public string SalonCity { get; set; }
        public string SalonCountry { get; set; }
        public string SalonZipcode { get; set; }
        public string SalonTimeZone { get; set; }

        public string SalonTimeZoneNumber { get; set; }
        public int? ServiceType { get; set; }
        public string LastUpdateNote { get; set; }
        public string SalonNote { get; set; }
        public string LastUpdateBy { get; set; }
        public DateTime? LastUpdateDate { get; set; }
        public DateTime? CancelDate { get; set; }
        public string BusinessDescription { get; set; }
        public int? TerminalType { get; set; }
        public int? TerminalStatus { get; set; }
        public string MID { get; set; }
        public int? Processor { get; set; }
        public string ProcessorName { get; set; }
        public int? Source { get; set; }
        public double? SubscriptionAmount { get; set; }

        // public Terminal Terminal { get; set; }
        public License License { get; set; }
    }

    //public class Terminal
    //{
    //    public string MID { get; set; }
    //    public int? Status { get; set; }
    //}
    public class License
    {
        public string LicenseName { get; set; }
        public int? RemainingDate { get; set; }
        public DateTime? EffectiveDate { get; set; }
        public int? LicenseStatus { get; set; }
    }
}