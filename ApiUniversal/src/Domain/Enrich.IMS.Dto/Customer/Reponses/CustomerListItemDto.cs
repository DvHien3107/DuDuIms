using Enrich.Common.Enums;
using Enrich.Dto.Attributes;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.Subscription;
using System;
using System.Collections.Generic;
using Enrich.Common;

namespace Enrich.IMS.Dto.Customer
{
    public partial class CustomerListItemDto : ListItemDto
    {
        #region summary

        [GridField(Index = 1, ColumnName = "Id", IsDefault = true, IsShow = false, CanSort = true, CanSearch = false)]
        [SqlMapDto(SqlTables.Customer + ".Id")]
        public string Id { get; set; }

        [GridField(Index = 2, ColumnName = "CustomerCode", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.CustomerCode")]
        public string CustomerCode { get; set; }

        [GridField(Index = 3, ColumnName = "CreateBy", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".CreateBy")]
        public string CreateBy { get; set; }

        [GridField(Index = 4, ColumnName = "CreateAt", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".CreateAt")]
        public DateTime? CreateAt { get; set; }

        [GridField(Index = 5, ColumnName = "UpdateBy", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".UpdateBy")]
        public string UpdateBy { get; set; }

        [GridField(Index = 6, ColumnName = "StoreCode", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".StoreCode")]
        public string StoreCode { get; set; }

        [GridField(Index = 7, ColumnName = "SocialSecurity", IsDefault = true, IsShow = true, CanSort = true, CanSearch = false)]
        [SqlMapDto($"{SqlTables.Customer}.SocialSecurity")]
        public string SocialSecurity { get; set; }

        [GridField(Index = 8, ColumnName = "Password", IsDefault = true, IsShow = true, CanSort = true, CanSearch = false)]
        [SqlMapDto($"{SqlTables.Customer}.Password")]
        public string Password { get; set; }

        [GridField(Index = 9, ColumnName = "Type", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.Type")]
        public string Type { get; set; } = CustomerEnum.CustomerType.STORE_OF_MERCHANT.ToString();

        [GridField(Index = 10, ColumnName = "MangoEmail", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".MangoEmail")]
        public string MangoEmail { get; set; }

        [GridField(Index = 11, ColumnName = "Active", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.Active")]
        public int Active { get; set; }

        [GridField(Index = 12, ColumnName = "Website", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".Website")]
        public string Website { get; set; }

        [GridField(Index = 13, ColumnName = "MoreInfo", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.{SqlColumns.Customer.MoreInfo}")]
        public string MoreInfo { get; set; }

        [GridField(Index = 14, ColumnName = "PartnerCode", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".PartnerCode")]
        public string PartnerCode { get; set; }

        [GridField(Index = 15, ColumnName = "PartnerName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".PartnerName")]
        public string PartnerName { get; set; }

        [GridField(Index = 16, ColumnName = "MemberNumber", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".MemberNumber")]
        public string MemberNumber { get; set; }

        [GridField(Index = 17, ColumnName = "FullName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".FullName")]
        public string FullName { get; set; }

        #endregion

        #region Owner information

        [GridField(Index = 18, ColumnName = "ContactName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".ContactName")]
        public string ContactName { get; set; }

        [GridField(Index = 19, ColumnName = "Birthday", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".Birthday")]
        public System.DateTime? Birthday { get; set; }

        [GridField(Index = 20, ColumnName = "OwnerEmail", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".Email")]
        public string Email { get; set; }

        [GridField(Index = 21, ColumnName = "CellPhone", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.CellPhone")]
        public string CellPhone { get; set; }

        [GridField(Index = 22, ColumnName = "State", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.State")]
        public string State { get; set; }

        [GridField(Index = 23, ColumnName = "Country", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.Country")]
        public string Country { get; set; }

        [GridField(Index = 24, ColumnName = "City", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.City")]
        public string City { get; set; }

        [GridField(Index = 25, ColumnName = "Zipcode", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.Zipcode")]
        public string Zipcode { get; set; }

        /// <summary>
        /// salon note
        /// </summary>
        [GridField(Index = 25, ColumnName = "BusinessDescription", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.BusinessDescription")]
        public string BusinessDescription { get; set; }

        /// <summary>
        /// salon note
        /// </summary>
        [GridField(Index = 25, ColumnName = "LastUpdateNote", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"Calendar_EventTmp.[Name]", sqlJoinKeys: "CustomerLastUpdateNote")]        
        public string LastUpdateNote { get; set; }

        /// <summary>
        /// salon note
        /// </summary>
        [GridField(Index = 25, ColumnName = "LastUpdateBy", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"Calendar_EventTmp.[CreateBy]")]
        public string LastUpdateBy { get; set; }

        /// <summary>
        /// salon note
        /// </summary>
        [GridField(Index = 25, ColumnName = "LastUpdateDate", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"Calendar_EventTmp.[CreateAt]")]
        public string LastUpdateDate { get; set; }



        #endregion

        #region Salon information

        [GridField(Index = 26, ColumnName = "SalonName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.BusinessName")]
        public string BusinessName { get; set; }

        [GridField(Index = 27, ColumnName = "LegalName", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.LegalName")]
        public string LegalName { get; set; }

        [GridField(Index = 28, ColumnName = "SalonEmail", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.SalonEmail")]
        public string SalonEmail { get; set; }

        [GridField(Index = 29, ColumnName = "SalonPhone", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto(SqlTables.Customer + ".SalonPhone")]
        public string SalonPhone { get; set; }

        [GridField(Index = 30, ColumnName = "SalonAddress1", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.SalonAddress1")]
        public string SalonAddress1 { get; set; }

        [GridField(Index = 31, ColumnName = "SalonAddress2", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.SalonAddress2")]
        public string SalonAddress2 { get; set; }

        [GridField(Index = 32, ColumnName = "SalonState", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.SalonState")]
        public string SalonState { get; set; }

        [GridField(Index = 33, ColumnName = "BusinessCountry", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.BusinessCountry")]
        public string SalonCountry { get; set; }

        [GridField(Index = 34, ColumnName = "SalonCity", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.SalonCity")]
        public string SalonCity { get; set; }

        [GridField(Index = 35, ColumnName = "SalonZipcode", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.SalonZipcode")]
        public string SalonZipcode { get; set; }

        [GridField(Index = 36, ColumnName = "SalonTimeZone", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.SalonTimeZone")]
        public string SalonTimeZone { get; set; }

        [GridField(Index = 37, ColumnName = "SalonTimeZoneNumber", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.{SqlColumns.Customer.SalonTimeZoneNumber}")]
        public string SalonTimeZoneNumber { get; set; }

        [GridField(Index = 38, ColumnName = "MID", IsDefault = true, IsShow = false, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.MID")]
        public string MID { get; set; }


        [GridField(Index = 39, ColumnName = "TerminalStatus", IsDefault = true, IsShow = false, CanSort = true, CanSearch = true)]
        [SqlMapDto($"CASE WHEN ({SqlTables.Customer}.MID IS NOT NULL AND {SqlTables.Customer}.TerminalStatus = 1)" +
            $"THEN 1 ELSE 0 END")]
        public int TerminalStatus { get; set; }

        [GridField(Index = 40, ColumnName = "TerminalType", IsDefault = true, IsShow = false, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.TerminalType")]
        public int TerminalType { get; set; }

        [GridField(Index = 40, ColumnName = "Processor", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.Processor")]
        public int Processor { get; set; }

        [GridField(Index = 5, ColumnName = "Processor", IsDefault = true, IsShow = true, CanSort = true)]
        [SqlMapDto(@$"CASE WHEN {Constants.AliasEnumValueTable.CustomerProcessor}.Name IS NULL THEN NULL ELSE {Constants.AliasEnumValueTable.CustomerProcessor}.Name END", sqlJoinKeys: "ProcessorEnumValue")]
        public string ProcessorName { get; set; }

        [GridField(Index = 40, ColumnName = "Source", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.Source")]
        public int Source { get; set; }

        [GridField(Index = 40, ColumnName = "CancelDate", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]
        [SqlMapDto($"{SqlTables.Customer}.CancelDate")]
        public DateTime CancelDate { get; set; }


        [GridField(Index = 40, ColumnName = "SubscriptionAmount", IsDefault = true, IsShow = true, CanSort = true, CanSearch = true)]

        [SqlMapDto(@$"CASE WHEN {SqlTables.OrderSubscription}.Amount IS NOT NULL THEN {SqlTables.OrderSubscription}.Amount ELSE ({SqlTables.OrderSubscription}.Price * (CASE WHEN {SqlTables.OrderSubscription}.Period = 'MONTHLY' 
				THEN (CASE WHEN {SqlTables.OrderSubscription}.SubscriptionQuantity IS NOT NULL THEN {SqlTables.OrderSubscription}.SubscriptionQuantity ELSE 1 END) 
				ELSE (CASE WHEN {SqlTables.OrderSubscription}.Quantity IS NOT NULL THEN {SqlTables.OrderSubscription}.Quantity ELSE 1 END) END) - (CASE WHEN {SqlTables.OrderSubscription}.Discount IS NOT NULL THEN {SqlTables.OrderSubscription}.Discount ELSE 0 END)) END", sqlJoinKeys: "StoreServiceLicenseJoin")]
        public decimal SubscriptionAmount { get; set; }

        #endregion

        #region Subscription

        /// <summary>
        /// Service type. software or terminal. software , terminal 
        /// </summary>
        public int ServiceType { get;set; }

        /// <summary>
        /// terminal status. 
        /// 0 ==> dont use 
        /// 1 ==> using
        /// </summary>
        public TerminalStatusDto Terminal { get; set; }

        /// <summary>
        /// License detail
        /// </summary>
        public LicenseStatusDto License { get; set; }

        #endregion

        #region Subscription
        public int MaxRows { get; set; }
        #endregion
    }
}