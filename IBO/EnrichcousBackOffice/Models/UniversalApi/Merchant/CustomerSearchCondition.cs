using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.UniversalApi.Merchant
{
    public class CustomerSearchCondition
    {
        public List<int> SiteIds { get; set; }
        public List<string> AccountManagers { get; set; }
        public int? LicenseStatus { get; set; }
        public int? TermialStatus { get; set; }
        public string Type { get; set; }
        public string RemainingDays { get; set; }
        public string NODaysCreated { get; set; }
        public string AtRisk { get; set; }
        public int? ServiceType { get; set; }
        public string SearchText { get; set; }
    }
}