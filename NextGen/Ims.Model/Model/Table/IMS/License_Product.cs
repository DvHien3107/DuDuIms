using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class License_Product
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool? isAddon { get; set; }

        public decimal? Price { get; set; }

        public string Type { get; set; }

        public string SubscriptionDuration { get; set; }

        public int? SubscriptionEndWarningDays { get; set; }

        public string Code { get; set; }

        public string Code_POSSystem { get; set; }

        public bool? AllowDemo { get; set; }

        public bool? AllowSlice { get; set; }

        public int? Trial_Days { get; set; }

        public decimal? Promotion_Price { get; set; }

        public int? Promotion_Apply_Months { get; set; }

        public bool? Available { get; set; }

        public int? Level { get; set; }

        public bool? Active { get; set; }

        public int? NumberOfPeriod { get; set; }

        public int? Trial_Months { get; set; }

        public int? Promotion_Time_To_Available { get; set; }

        public int? GiftCardQuantity { get; set; }

        public decimal? PartnerPrice { get; set; }

        public decimal? MembershipPrice { get; set; }

        public decimal? ActivationFee { get; set; }

        public int? PreparingDays { get; set; }

        public bool? IsDelete { get; set; }

        public string PeriodRecurring { get; set; }

        public decimal? InteractionFee { get; set; }

        public bool? DeploymentTiketAuto { get; set; }

        public int? FlagDeactivateExpires { get; set; }

        public int? SiteId { get; set; }

    }
}
