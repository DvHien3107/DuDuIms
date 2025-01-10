using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.IMS.Models
{
    public partial class ChangeLicenseParametersModel
    {
        public ChangeLicenseParametersModel()
        {
            Month = 1;
            Renew = true;
        }
        public string StoreCode { get; set; }

       public string LicenseCode { get; set; }

        public int Month { get; set; }

        /// <summary>
        /// Applies only to the license and its default is true
        /// </summary>
        /// <example>true/false</example>
        public bool Renew { get; set; }
    }
}