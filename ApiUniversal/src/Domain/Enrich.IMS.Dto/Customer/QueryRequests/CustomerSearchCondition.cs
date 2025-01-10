using Enrich.Dto.Base;
using Enrich.IMS.Dto.Common;
using System.Collections.Generic;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerSearchCondition : BaseSearchCondition
    {
        /// custom for customer dashboard
        //public List<string> Partners { get; set; }
        public List<string> AccountManagers { get; set; }

        /// <summary>
        /// Merchant Status. Active, Inactive, Expried
        /// </summary>
        public int? LicenseStatus { get; set; }

        /// <summary>
        /// Merchant Status. Active, Inactive, Expried
        /// </summary>
        public int? TerminalStatus { get; set; }


        /// <summary>
        /// Merchant Status. Live or not live
        /// </summary>
        public List<int> MerchantStatus { get; set; }

        /// <summary>
        /// All, in house or merchant
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// software or terminal. software , terminal 
        /// </summary>
        public int? ServiceType { get; set; }

        /// <summary>
        /// Mac, mango, ...
        /// </summary>
        public List<int> SiteIds { get; set; }
    
        public int? RemainingDays { get; set; }
        public int? NODaysCreated { get; set; }
        public int? AtRisk { get; set; }

        public string SearchText { get; set; }

        public string State { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string Processor { get; set; }

        public bool PopulateCountSummaries { get; set; }
        
        /// <summary>
        /// Dont use
        /// </summary>
        public List<KeyValueDto<string>> OnStringFields { get; set; }
    }
}