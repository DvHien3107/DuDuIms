using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto
{
    public sealed partial class SqlColumns
    {
        public sealed class LicenseProduct
        {
            public const string CodePOSSystem = "Code_POSSystem";
            public const string PromotionApplyMonths = "Promotion_Apply_Months"; 
            public const string PromotionPrice = "Promotion_Price";
            public const string PromotionTimeToAvailable = "Promotion_Time_To_Available";
            public const string TrialDays = "Trial_Days";
            public const string TrialMonths = "Trial_Months";
        }
    }
}
