using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using System;
using static Enrich.Common.Enums.CustomerEnum;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerReportListItemDto : ListItemDto
    {
        [GridField(Index = 1, ColumnName = "StoreCode", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.StoreCode")]
        public string StoreCode { get; set; }

        [GridField(Index = 2, ColumnName = "AccountStatus", IsDefault = true, IsShow = true, CanSort = true)]
        public string AccountStatus { get; set; }

        [GridField(Index = 3, ColumnName = "ServiceType", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto($@"CASE WHEN (({SqlTables.StoreServices}.Type = 'license' AND {SqlTables.StoreServices}.Active = 1) AND {SqlTables.Customer}.MID IS NUll) THEN 'Software Only' ELSE
                    CASE WHEN ({SqlTables.Customer}.MID IS NOT NUll AND ({SqlTables.Customer}.CustomerCode NOT IN (SELECT CustomerCode FROM {SqlTables.StoreServices} WITH(NOLOCK) WHERE Type = 'license' AND Active = 1))) THEN 'Terminal Only' ELSE 'Software and Terminal' END END")]
        //[SqlMapDto($@"CASE WHEN (({SqlTables.StoreServices}.Type = 'license' AND {SqlTables.StoreServices}.Active = 1) AND ({SqlTables.Customer}.MID IS NUll OR {SqlTables.Customer}.TermialStatus = 0 OR {SqlTables.Customer}.TermialStatus IS NULL)) THEN 'Software Only' ELSE
        //            CASE WHEN ({SqlTables.Customer}.MID IS NOT NUll AND ({SqlTables.Customer}.CustomerCode NOT IN (SELECT CustomerCode FROM {SqlTables.StoreServices} WITH(NOLOCK) WHERE Type = 'license' AND Active = 1))) THEN 'Terminal Only' ELSE 'Software and Terminal' END END")] 
        public string ServiceType { get; set; }

        [GridField(Index = 4, ColumnName = "AccountSource", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(@$"CASE WHEN {Constants.AliasEnumValueTable.CustomerSource}.Name IS NULL THEN '{Constants.ServiceName.Company}' ELSE {Constants.AliasEnumValueTable.CustomerSource}.Name END")]
        public string AccountSource { get; set; }

        [GridField(Index = 5, ColumnName = "Processor", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(@$"CASE WHEN {Constants.AliasEnumValueTable.CustomerProcessor}.Name IS NULL THEN NULL ELSE {Constants.AliasEnumValueTable.CustomerProcessor}.Name END")]
        public string Processor { get; set; }

        [GridField(Index = 6, ColumnName = "BusinessName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.BusinessName")]
        public string BusinessName { get; set; }

        [GridField(Index = 7, ColumnName = "PartnerName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($@"CASE WHEN {SqlTables.Partner}.Name IS NULL THEN '{Constants.ServiceName.Product}' ELSE {SqlTables.Partner}.Name END")]
        public string PartnerName { get; set; }

        [GridField(Index = 8, ColumnName = "Subscription", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.OrderSubscription}.ProductName")]
        public string Subscription { get; set; }

        [GridField(Index = 9, ColumnName = "SubscriptionPeriod", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($@"CASE WHEN {SqlTables.StoreServices}.RenewDate > DATEADD(year, 10, GETUTCDATE()) THEN 'Lifetime' ELSE 
                    CASE WHEN {SqlTables.StoreServices}.PeriodRecurring IS NULL THEN 'Monthly' ELSE {SqlTables.StoreServices}.PeriodRecurring END END")]
        public string SubscriptionPeriod { get; set; }

        [GridField(Index = 10, ColumnName = "SubscriptionAmount", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(@$"CASE WHEN {SqlTables.OrderSubscription}.Amount IS NOT NULL THEN {SqlTables.OrderSubscription}.Amount ELSE ({SqlTables.OrderSubscription}.Price * (CASE WHEN {SqlTables.OrderSubscription}.Period = 'MONTHLY' 
				THEN (CASE WHEN {SqlTables.OrderSubscription}.SubscriptionQuantity IS NOT NULL THEN {SqlTables.OrderSubscription}.SubscriptionQuantity ELSE 1 END) 
				ELSE (CASE WHEN {SqlTables.OrderSubscription}.Quantity IS NOT NULL THEN {SqlTables.OrderSubscription}.Quantity ELSE 1 END) END) - (CASE WHEN {SqlTables.OrderSubscription}.Discount IS NOT NULL THEN {SqlTables.OrderSubscription}.Discount ELSE 0 END)) END")]
        public decimal? SubscriptionAmount { get; set; }

        [GridField(Index = 11, ColumnName = "OpenedDate", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.CustomerTransaction}.CreateAt")]
        public DateTime? OpenedDate { get; set; }

        [GridField(Index = 12, ColumnName = "CanceledDate", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.CancelDate")]
        public DateTime? CanceledDate { get; set; }

        [GridField(Index = 13, ColumnName = "StartedDate", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.GoLiveDate")]
        public DateTime? StartedDate { get; set; }

        [GridField(Index = 14, ColumnName = "CreateAt", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.CreateAt")]
        public DateTime? CreateAt { get; set; }

    }
}