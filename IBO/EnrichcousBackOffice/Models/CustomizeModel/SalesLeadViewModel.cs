using System;
namespace EnrichcousBackOffice.Models.CustomizeModel
{

    public class SalesLeadViewModel
    {

        public string Id { get; set; }
        public string Password { get; set; }
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string SalonName { get; set; }
        public string SalonPhone { get; set; }
        public string SalonEmail { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address { get; set; }
        public string SalesPerson { get; set; }
        public long? TeamNumber { get; set; }
        public string ZipCode { get; set; }
        public string TimeZone { get; set; }
        public string TimeZoneNumber { get; set; }
        public int PotentialRateScore { get; set; }
        public bool IsSendMail { get; set; }
        public string Note { get; set; }
        public string CallOfNumber { get; set; }
        public int? InteractionStatus { get; set; }
        public MoreInfo MoreInfo { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public class MoreInfo
    {
        public string NumberBranches { get; set; }
        public string NumberEmployees { get; set; }
        public string AreUsingSoftware { get; set; }
        public string Hardware { get; set; }
        public string ServicePackage { get; set; }
    }
    
    public class FilterDataView
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string Status { get; set; }
        public string Team { get; set; }
        public string Member { get; set; }
        public string SearchText { get; set; }
        public string State { get; set; }

    }

}