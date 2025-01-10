using Enrich.Common.Enums;
using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using System;

namespace Enrich.IMS.Dto.OrderSubscription
{
    public partial class SubscriptionPackageListItemDto : ListItemDto
    {
        public string StoreCode { get; set; }

        public string Package { get; set; } = BaseServiceEnum.Code.SMS.ToString();

        public decimal? Paid { get; set; }

        public int NumberTransaction { get; set; }

    }
}