using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using System;
using System.Linq;
using static Enrich.Common.Enums.CustomerEnum;
using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class Customer
        {
            private const string Alias = SqlTables.Customer;
            private static int ESTHours = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(c => c.Id == "Eastern Standard Time").BaseUtcOffset.Hours;
            public static class Parameters
            {
                public const string Id = "Id";
                public const string Email = "email";
            }

            public static class Conditions
            {
                public static string HaveTransaction = $@"({SqlTables.CustomerTransaction}.Id IS NOT NULL)";
                /// <summary>
                /// SQL condition get Customer have license active
                /// </summary>
                public static string HaveLicenseActive = $"({SqlTables.StoreServices}.Type = '{SubscriptionEnum.Type.license.ToString()}' " +
                    $"AND {SqlTables.StoreServices}.Active = {(int)SubscriptionEnum.Status.Active} " +
                    $"AND {SqlTables.StoreServices}.RenewDate >= CONVERT(date ,GETUTCDATE()))";

                /// <summary>
                /// SQL condition get Customer have license expires
                /// </summary>
                public static string HaveLicenseExpires = $"({SqlTables.StoreServices}.Type = '{SubscriptionEnum.Type.license.ToString()}' " +
                    $"AND {SqlTables.StoreServices}.Active = {(int)SubscriptionEnum.Status.Active} " +
                    $"AND {SqlTables.StoreServices}.RenewDate < CONVERT(date ,GETUTCDATE()))";

                /// <summary>
                /// SQL condition get Customer don't have license active
                /// </summary>
                public static string DontHaveLicenseActive = $"({SqlTables.Customer}.CustomerCode NOT IN (" +
                    $"SELECT CustomerCode FROM {SqlTables.StoreServices} {NoLock} " +
                    $"WHERE Type = '{SubscriptionEnum.Type.license.ToString()}' " +
                    $"AND Active = {(int)SubscriptionEnum.Status.Active}))";
                //$"AND {SqlTables.StoreServices}.RenewDate >= CONVERT(date ,GETUTCDATE())))";

                /// <summary>
                /// SQL condition get Customer have MID
                /// </summary>
                public static string HaveMID = $"({Alias}.MID IS NOT NUll AND ({Alias}.TerminalStatus = 1 OR {Alias}.TerminalStatus IS NULL))";

                /// <summary>
                /// SQL condition get Customer don't have MID
                /// </summary>
                public static string DontHaveMID = $"({Alias}.MID IS NUll OR {Alias}.TerminalStatus = 0)";

                /// <summary>
                /// SQL condition get Customer have cancel date
                /// </summary>
                public static string HaveCancelDate = $@"({Alias}.CancelDate IS NOT NULL AND {Alias}.CancelDate <= CONVERT(date ,GETUTCDATE()))";

                /// <summary>
                /// SQL condition get Customer don't have have cancel date
                /// </summary>
                public static string DontHaveCancelDate = $"({Alias}.CancelDate IS NULL OR {Alias}.CancelDate > CONVERT(date ,GETUTCDATE()))";

                /// <summary>
                /// SQL condition get Customer exist StoreCode
                /// </summary>
                public static string IsExistStoreCode = $"({Alias}.StoreCode IS NOT NULL AND {Alias}.StoreCode != '' AND {Alias}.CustomerCode IS NOT NULL AND {Alias}.CustomerCode != '')";

                /// <summary>
                /// SQL condition get Merchant using Terminal (hardware)
                /// </summary>
                public static string MerchantMID = $@"({DontHaveCancelDate} AND {HaveMID} AND ({DontHaveLicenseActive} OR {HaveLicenseExpires}))";

                /// <summary>
                /// SQL condition get Merchant active License (software)
                /// </summary>
                public static string MerchantLicense = @$"({DontHaveCancelDate} AND {HaveLicenseActive} AND {DontHaveMID})";

                /// <summary>
                /// SQL condition get all merchant active
                /// </summary>
                public static string Merchant = $"({HaveLicenseActive} OR {HaveMID})";

                /// <summary>
                /// SQL condition get Merchant active license and using Terminal
                /// </summary>
                public static string MerchantLicenseAndMID = $@"({DontHaveCancelDate} AND {HaveMID} AND {HaveLicenseActive})";

                /// <summary>
                /// SQL condition get canceled Merchant
                /// </summary>
                public static string MerchantIsCanceled = $"{HaveCancelDate}";

                /// <summary>
                /// SQL condition get canceled Merchant
                /// </summary>
                public static string MerchantIsNotCanceled = $"{DontHaveCancelDate}";

                /// <summary>
                /// SQl condition get pending delivery Merchant
                /// </summary>
                public static string MerchantPendingDelivery = $"({Merchant} AND {DontHaveCancelDate} " +
                    $"AND {Alias}.CustomerCode IN (" +
                    $"SELECT CustomerCode FROM {SqlTables.SupportTicket} " +
                    $"INNER JOIN {SqlTables.TicketStatus} " +
                    $"ON {SqlTables.SupportTicket}.StatusId = {SqlTables.TicketStatus}.Id " +
                    $"AND {SqlTables.TicketStatus}.TicketTypeName = '{TicketEnum.TypeName.Deployment}' " +
                    $"AND {SqlTables.TicketStatus}.Type = '{TicketEnum.Status.closed.ToString()}' " +
                    $"WHERE {SqlTables.SupportTicket}.TypeName = '{TicketEnum.TypeName.Deployment}'))";

                /// <summary>
                /// SQl condition get pending delivery with out store service
                /// </summary>
                public static string MerchantPendingDeliveryWithOutStoreService = $"{Alias}.CustomerCode IN (" +
                    $"SELECT CustomerCode FROM {SqlTables.SupportTicket} " +
                    $"INNER JOIN {SqlTables.TicketStatus} " +
                    $"ON {SqlTables.SupportTicket}.StatusId = {SqlTables.TicketStatus}.Id " +
                    $"AND {SqlTables.TicketStatus}.TicketTypeName = '{TicketEnum.TypeName.Deployment}' " +
                    $"AND {SqlTables.TicketStatus}.Type = '{TicketEnum.Status.closed.ToString()}' " +
                    $"WHERE {SqlTables.SupportTicket}.TypeName = '{TicketEnum.TypeName.Deployment}')";

                /// <summary>
                /// SQl condition get pending delivery Merchant
                /// </summary>
                public static string MerchantLive = $"{IsExistStoreCode} AND {Merchant} AND {DontHaveCancelDate} " +
                    $"AND {Alias}.CustomerCode NOT IN (" +
                    $"SELECT CustomerCode FROM {SqlTables.SupportTicket} " +
                    $"INNER JOIN {SqlTables.TicketStatus} " +
                    $"ON {SqlTables.SupportTicket}.StatusId = {SqlTables.TicketStatus}.Id " +
                    $"AND {SqlTables.TicketStatus}.TicketTypeName = '{TicketEnum.TypeName.Deployment}' " +
                    $"AND {SqlTables.TicketStatus}.Type = '{TicketEnum.Status.closed.ToString()}' " +
                    $"WHERE {SqlTables.SupportTicket}.TypeName = '{TicketEnum.TypeName.Deployment}')";

                /// <summary>
                /// SQL condition get store in house
                /// </summary>
                public static string IsStoreInHouse = $"{Alias}.Type = '{CustomerEnum.CustomerType.STORE_IN_HOUSE.ToString()}'";

                /// <summary>
                /// SQL condition get store of merchant
                /// </summary>
                public static string IsStoreOfMerchant = $"({Alias}.Type IS NULL OR {Alias}.Type = '{CustomerEnum.CustomerType.STORE_OF_MERCHANT.ToString()}')";
            }

            /// <summary>
            /// Return all SQL join key of customer
            /// </summary>
            public static class JoinKeys
            {
                /// <summary>
                /// Extend SQL join with table StoreService and OrderSubscription
                /// </summary>
                public const string StoreService = "StoreServiceLicenseJoin";

                /// <summary>
                /// Extend SQL join with table CustomerTransaction
                /// </summary>
                public const string CustomerTransaction = "CustomerTransactionJoin";

                /// <summary>
                /// Extend SQL join Customer with CustomerTransaction table
                /// </summary>
                public const string CustomerWTransaction = "CustomerWTransactionJoin";

                /// <summary>
                /// Extend SQL join with table Partner
                /// </summary>
                public const string Partner = "PartnerJoin";

                /// <summary>
                /// Extend SQL join with table EnumValue
                /// </summary>
                public const string SourceEnumValue = "SourceEnumValue";

                /// <summary>
                /// Extend SQL join with table EnumValue
                /// </summary>
                public const string ProcessorEnumValue = "ProcessorEnumValue";
            }


            /// <summary>
            /// SQL return number of email input
            /// </summary>
            public const string CountEmail = $"SELECT COUNT(1) FROM {Alias} {NoLock} WHERE SalonEmail = @{Parameters.Email} or MangoEmail = @{Parameters.Email}";

            /// <summary>
            /// SQL return number 
            /// </summary>
            public const string IsExistACH = $"SELECT COUNT(1) FROM {Alias} {NoLock} WHERE CustomerCode = @customerCode AND DepositBankName IS NOT NULL AND DepositAccountNumber IS NOT NULL AND DepositRoutingNumber IS NOT NULL";

            /// <summary>
            /// SQL check merchant is pending delivery
            /// </summary>
            /// <param name="storeCode"></param>
            /// <returns></returns>
            public static string CheckMerchantPendingDelivery(string storeCode) => $@"SELECT COUNT(1) FROM {Alias} {NoLock} WHERE StoreCode = '{storeCode}' AND {Conditions.MerchantPendingDeliveryWithOutStoreService}";

            public const string GetById = $@"SELECT * FROM {Alias} {NoLock} WHERE Id = @{Parameters.Id}";

            public const string GetBaseInformation = $@"
                SELECT cus.*, ISNULL(MangoEmail,SalonEmail) as LoginEmail,sl.MemberNumber AS SalesPersonNumber,
                (CASE WHEN ISNULL(cus.MemberNumber,'')!='' THEN (SELECT MemberName FROM {SqlTables.Member} as mm WHERE mm.MemberNumber = sl.MemberNumber) ELSE '' END) AS SalesPersonName,
                (CASE WHEN ISNULL(cus.Processor,'')!='' THEN (SELECT TOP 1 [Name] FROM {SqlTables.EnumValue} WHERE Namespace ='{MerchantOptionNameSpaceEnum.Processor}' AND Value = 1 ) ELSE '' END) Processor,
                (CASE WHEN ISNULL(cus.Source,'')!='' THEN (SELECT TOP 1 [Name] FROM {SqlTables.EnumValue} WHERE Namespace = '{MerchantOptionNameSpaceEnum.Source}' AND Value = 1 ) ELSE '' END) Source,
                (CASE WHEN ISNULL(cus.TerminalType,'')!='' THEN (SELECT TOP 1 [Name] FROM {SqlTables.EnumValue} WHERE Namespace ='{MerchantOptionNameSpaceEnum.TerminalType}' AND Value = 1 ) ELSE '' END) TerminalType,
                (CASE WHEN ISNULL(cus.DeviceName,'')!='' THEN (SELECT TOP 1 [Name] FROM {SqlTables.EnumValue} WHERE Namespace ='{MerchantOptionNameSpaceEnum.DeviceName}' AND Value = 1 ) ELSE '' END) DeviceName,
                (CASE WHEN ISNULL(cus.POSStructure,'')!='' THEN (SELECT TOP 1 [Name] FROM {SqlTables.EnumValue} WHERE Namespace ='{MerchantOptionNameSpaceEnum.POSStructure}' AND Value = 1 ) ELSE '' END) POSStructure,
                (CASE WHEN ISNULL(cus.DeviceSetupStructure,'')!='' THEN (SELECT TOP 1 [Name] FROM {SqlTables.EnumValue} WHERE Namespace ='{MerchantOptionNameSpaceEnum.DeviceSetupStructure}' AND Value = 1 ) ELSE '' END) DeviceSetupStructure,
                DATEDIFF( day,
                (SELECT TOP 1 CASE WHEN CAST(sto.EffectiveDate AS DATE) <= GETUTCDATE() THEN  GETUTCDATE() ELSE sto.EffectiveDate END
                FROM {SqlTables.StoreServices} AS sto {NoLock}
                WHERE cus.CustomerCode = sto.CustomerCode AND sto.Active = 1 AND sto.Type = 'license'),
                (SELECT sto.RenewDate
                FROM {SqlTables.StoreServices} AS sto {NoLock}
                WHERE cus.CustomerCode = sto.CustomerCode AND sto.Active = 1 AND sto.Type = 'license')) AS RemainingDays,
                (SELECT sto.EffectiveDate
                FROM {SqlTables.StoreServices} AS sto {NoLock}
                WHERE cus.CustomerCode = sto.CustomerCode AND sto.Active = 1 AND sto.Type = 'license') AS EffectiveDate,
                (SELECT TOP 1 od.DueDate FROM {SqlTables.Orders} AS od {NoLock}
                JOIN {SqlTables.StoreServices} AS ss {NoLock} ON od.OrdersCode = ss.OrderCode
                WHERE od.CustomerCode = cus.CustomerCode AND od.Status = 'PaymentLater' AND ss.Active = 1 AND ss.Type = 'license') AS DueDate
                FROM {Alias} AS cus {NoLock}
                LEFT JOIN {SqlTables.SalesLead} AS sl ON Cus.CustomerCode = SL.CustomerCode
                WHERE cus.Id = @{Parameters.Id}";

            public static string ReportCustomerForChartBy(string unit, int year, string strDate)
                => $@"SELECT COUNT(DATEPART({unit}, DATEADD(HOUR, {ESTHours}, {strDate}))) AS NumberCustomer,
                    DATEPART({unit}, DATEADD(HOUR, {ESTHours}, {strDate})) AS ColumnNumber,
                    {(unit == TimeUnit.Week.ToString().ToLower() ? "'Week ' + " : "")} CONVERT(nvarchar, DATENAME({unit}, MAX(DATEADD(HOUR, {ESTHours}, {strDate})))) AS ColumnName
                    FROM {Alias}
                    WHERE DATEPART(YEAR, DATEADD(HOUR, {ESTHours}, {strDate})) = {year}
                    AND 0 < (SELECT COUNT(1) FROM C_CustomerTransaction WHERE C_Customer.CustomerCode = C_CustomerTransaction.CustomerCode 
                    AND (PaymentStatus = 'Approved' OR PaymentStatus = 'Success'))
                    GROUP BY DATEPART({unit}, DATEADD(HOUR, {ESTHours}, {strDate}))";

            public static string ReportCustomerForChartByProc(int year)
                => $@"exec P_ReportCustomerForChart {year}";

            public static DictionaryString JoinsSearch = new DictionaryString
            {
                ["StoreServicesLicense"] =
                @$"LEFT JOIN {SqlTables.StoreServices} {NoLock}
                    ON {SqlTables.Customer}.CustomerCode = {SqlTables.StoreServices}.CustomerCode
                    AND {SqlTables.StoreServices}.Type = '{SubscriptionEnum.Type.license.ToString()}'
                    AND {SqlTables.StoreServices}.Active = {(int)SubscriptionEnum.Status.Active}
                LEFT JOIN {SqlTables.OrderSubscription} {NoLock}
                    ON {SqlTables.OrderSubscription}.OrderCode = {SqlTables.StoreServices}.OrderCode
                    AND {SqlTables.OrderSubscription}.Product_Code = {SqlTables.StoreServices}.ProductCode",

                ["CustomerLastUpdateNote"] = $@"
                    LEFT JOIN C_SalesLead on C_Customer.CustomerCode = C_SalesLead.CustomerCode
                    LEFT JOIN ( SELECT  Calendar_Event.[Name], Calendar_Event.SalesLeadId,Calendar_Event.CreateAt,Calendar_Event.CreateBy, ROW_NUMBER()
                                    OVER (PARTITION BY Calendar_Event.SalesLeadId ORDER BY Calendar_Event.CreateAt desc) AS RowNum
                            FROM    Calendar_Event ) as Calendar_EventTmp ON Calendar_EventTmp.SalesLeadId = C_SalesLead.Id AND  Calendar_EventTmp.RowNum =1
                    ",

                //["CustomerTerminal"] = $"INNER JOIN ( SELECT {SqlTables.Orders}.CustomerCode FROM {SqlTables.Orders} INNER JOIN {SqlTables.OrderProducts} on {SqlTables.Orders}.OrdersCode = {SqlTables.OrderProducts}.OrderCode AND  {SqlTables.OrderProducts}.ProductCode in ( SELECT ProductLineCode FROM {SqlTables.Product} WHERE ProductLineCode = 'terminal'))  C_CustomerTerminal ON {SqlTables.Customer}.CustomerCode = C_CustomerTerminal.CustomerCode",

                ["FullTextSearch"] = @$"INNER JOIN  ( 
                                        SELECT top 1000 [Key], SUM(RankNum) AS CRank FROM (

                                        SELECT top 5 [Key], SUM(RankNum1) AS RankNum FROM (SELECT [Key] AS 'Key', [RANK] * 1300 AS RankNum1 FROM CONTAINSTABLE(C_Customer, (CustomerCode,StoreCode), @TextPattern) 
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *1200 AS RankNum1 FROM CONTAINSTABLE(C_Customer, (BusinessName, BusinessEmail), @TextPattern)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *1100 AS RankNum1 FROM CONTAINSTABLE(C_Customer, (OwnerName, ContactName), @TextPattern)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *1000 AS RankNum1 FROM CONTAINSTABLE(C_Customer, (Email), @TextPattern)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *900 AS RankNum1 FROM CONTAINSTABLE(C_Customer, (BusinessPhone, SalonPhone, OwnerMobile), @TextPattern)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *130 AS RankNum1 FROM FREETEXTTABLE(C_Customer, (CustomerCode, StoreCode), @TextPatternFreetext)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *120 AS RankNum1 FROM FREETEXTTABLE(C_Customer, (BusinessName, BusinessEmail), @TextPatternFreetext)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *110 AS RankNum1 FROM FREETEXTTABLE(C_Customer, (OwnerName, ContactName), @TextPatternFreetext)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *100 AS RankNum1 FROM FREETEXTTABLE(C_Customer, (Email), @TextPatternFreetext)
                                        UNION
                                        SELECT[Key] AS 'Key',
                                                        [RANK] *90 AS RankNum1 FROM FREETEXTTABLE(C_Customer, (BusinessPhone, SalonPhone, OwnerMobile), @TextPatternFreetext)) as fulltextSearchTmp GROUP BY [Key]  ORDER BY RankNum desc 
                                       UNION
                                       SELECT ID AS 'Key', 9000 AS RankNum FROM C_Customer
                                    WHERE ( LOWER(StoreCode) like N'%' + @SearchText + '%' OR LOWER (BusinessAddress) LIKE N'%' + @SearchText + '%' OR LOWER(ContactName) LIKE N'%' + @SearchText + '%' OR LOWER(Email) LIKE N'%' + @SearchText + '%' OR LOWER(BusinessEmail)    LIKE N'%' + @SearchText + '%' OR LOWER(OwnerName) LIKE N'%' + @SearchText + '%' OR LOWER   (SalonEmail) LIKE N'%' + @SearchText + '%' OR LOWER(SalonPhone) LIKE N'%' + @SearchText + '%'   OR LOWER(BusinessName) LIKE N'%' + @SearchText + '%' OR LOWER(Zipcode) LIKE N'%' + @SearchText + '%' OR LOWER(SalonState) LIKE N'%' + @SearchText + '%'  OR LOWER(SalonCity)  LIKE N'%' + @SearchText + '%' 
                                          )) as rankTmp GROUP BY [Key]  ORDER BY CRank desc ) as   CustomerRank ON C_Customer.Id = CustomerRank.[Key]",

                ["CustomerTerminal"] =
                @$"INNER JOIN ( SELECT {SqlTables.Orders}.CustomerCode 
                    FROM {SqlTables.Orders} {NoLock}
                    INNER JOIN {SqlTables.OrderProducts} {NoLock}
                    ON {SqlTables.Orders}.OrdersCode = {SqlTables.OrderProducts}.OrderCode 
                    AND {SqlTables.OrderProducts}.ProductCode IN (
                        SELECT ProductLineCode FROM {SqlTables.Product} {NoLock}
                        WHERE ProductLineCode = 'terminal')) C_CustomerTerminal 
                    ON {SqlTables.Customer}.CustomerCode = C_CustomerTerminal.CustomerCode",

                [JoinKeys.SourceEnumValue] =
                $"LEFT JOIN {SqlTables.EnumValue} {Constants.AliasEnumValueTable.CustomerSource} {NoLock}" +
                $"ON {SqlTables.Customer}.Source =  {Constants.AliasEnumValueTable.CustomerSource}.Value " +
                $"AND {Constants.AliasEnumValueTable.CustomerSource}.Namespace = '{MerchantOptionNameSpaceEnum.Source}'",

                [JoinKeys.ProcessorEnumValue] =
                $"LEFT JOIN {SqlTables.EnumValue} {Constants.AliasEnumValueTable.CustomerProcessor} {NoLock} " +
                $"ON {SqlTables.Customer}.Processor = {Constants.AliasEnumValueTable.CustomerProcessor}.Value " +
                $"AND {Constants.AliasEnumValueTable.CustomerProcessor}.Namespace = '{MerchantOptionNameSpaceEnum.Processor}'",

                [JoinKeys.Partner] =
                $"LEFT JOIN {SqlTables.Partner} {NoLock} " +
                $"ON {SqlTables.Customer}.SiteId = {SqlTables.Partner}.SiteId",

                [JoinKeys.CustomerTransaction] =
                $"LEFT JOIN (" +
                $"SELECT OrdersCode, MAX(Id) AS Id, MAX(CreateAt) AS CreateAt " +
                $"FROM {SqlTables.CustomerTransaction} {NoLock} " +
                $"WHERE PaymentStatus = '{PaymentEnum.Status.Approved.ToString()}' " +
                $"OR PaymentStatus = '{PaymentEnum.Status.Success.ToString()}'" +
                $"GROUP BY OrdersCode) AS {SqlTables.CustomerTransaction} " +
                $"ON {SqlTables.CustomerTransaction}.OrdersCode = {SqlTables.StoreServices}.OrderCode",

                [JoinKeys.CustomerWTransaction] =
                $"LEFT JOIN (" +
                $"SELECT CustomerCode, MAX(Id) AS Id, MAX(CreateAt) AS CreateAt " +
                $"FROM {SqlTables.CustomerTransaction} {NoLock} " +
                $"WHERE PaymentStatus = '{PaymentEnum.Status.Approved.ToString()}' " +
                $"OR PaymentStatus = '{PaymentEnum.Status.Success.ToString()}'" +
                $"GROUP BY CustomerCode) AS {SqlTables.CustomerTransaction} " +
                $"ON {SqlTables.CustomerTransaction}.CustomerCode = {SqlTables.Customer}.CustomerCode",

                [JoinKeys.StoreService] =
                $"LEFT JOIN {SqlTables.StoreServices} {NoLock} " +
                $"ON {SqlTables.Customer}.CustomerCode = {SqlTables.StoreServices}.CustomerCode " +
                $"AND {SqlTables.StoreServices}.Type = '{SubscriptionEnum.Type.license.ToString()}' " +
                $"AND {SqlTables.StoreServices}.Active = {(int)SubscriptionEnum.Status.Active} " +
                $"LEFT JOIN {SqlTables.OrderSubscription} {NoLock} " +
                $"ON {SqlTables.OrderSubscription}.OrderCode = {SqlTables.StoreServices}.OrderCode " +
                $"AND {SqlTables.OrderSubscription}.Product_Code = {SqlTables.StoreServices}.ProductCode " +
                $"AND {SqlTables.OrderSubscription}.SubscriptionType = '{SubscriptionEnum.Type.license.ToString()}'"
            };
        }
    }
}