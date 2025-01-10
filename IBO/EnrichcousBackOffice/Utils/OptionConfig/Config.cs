using EnrichcousBackOffice.AppLB.OptionConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Utils.OptionConfig
{
    public partial class Config : IOptionConfig
    {
        public TimeSpan? AutoScan_Time { get; set; }
        public bool? AutoScan_Enable { get; set; }
        public int? License_RecurringCheck { get; set; }
        public bool? License_Addon_Expires_Reset { get; set; }
        public string License_Expires_Leftdays { get; set; }

        public string Google_ClientID { get; set; }
        public string Google_ClientSecret { get; set; }

        public int? Refund_Remaining_Amount { get; set; }
        public int? Refund_Setupfee { get; set; }
        public int? Refund_All_Amount { get; set; }
        public string SMS_Show_Provide { get; set; }
    }
}