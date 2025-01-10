using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.Subscription
{
    public class LicenseStatusDto
    {
        /// <summary>
        /// store code
        /// </summary>
        public string StoreCode { get; set; }
        /// <summary>
        /// LicenseName      
        /// </summary>
        public string LicenseName { get; set; }

        /// <summary>
        /// The remaining date of license      
        /// </summary>
        public int RemainingDate { get; set; }

        /// <summary>
        /// The day, which license will be active 
        /// </summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>
        /// License status. 1 is active
        /// </summary>
        public int LicenseStatus { get; set; }
    }
}
