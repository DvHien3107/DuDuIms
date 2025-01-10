using Enrich.Common.Enums;
using Enrich.IMS.Dto;
using System;
using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class Order
        {
            private const string Alias = SqlTables.Orders;

            public static class Conditions
            {
                public static string HasPaid =
                    $"({Alias}.Status = '{OrderEnum.Status.Paid_Wait.ToString()}' OR {Alias}.Status = '{OrderEnum.Status.Closed.ToString()}')";
            }

            public static class JoinKeys
            {
                /// <summary>
                /// Extend SQL join with table OrderSubscription and OrderProduct
                /// </summary>
                public const string SubscriptionProduct = "SubscriptionProductJoin";
                public const string Transaction = "TransactionJoin";
                public const string Customer = "CustomerJoin";
            }

            public static DictionaryString JoinsKeys = new DictionaryString
            {
                [JoinKeys.Customer] = @$"LEFT JOIN {SqlTables.Customer} WITH(NOLOCK) ON {Alias}.CustomerCode = {SqlTables.Customer}.CustomerCode",
                [JoinKeys.Transaction] =
                    @$"LEFT JOIN {SqlTables.CustomerTransaction} WITH(NOLOCK) ON {Alias}.OrdersCode = {SqlTables.CustomerTransaction}.OrdersCode AND {SqlScript.CustomerTransaction.Conditions.Approved}",
                [JoinKeys.SubscriptionProduct] = 
                    @$"INNER JOIN ( SELECT Id, ProductId, Product_Code, ProductName, Price, Amount, [Period],
                    OrderCode, Discount, DiscountPercent, Quantity, SubscriptionQuantity, SubscriptionType, PriceType
                    FROM {SqlTables.OrderSubscription} {NoLock}
                    WHERE Product_Code IS NOT NULL
                    UNION ALL
                    SELECT Id, ModelCode AS ProductId, ProductCode AS Product_Code, ModelName, Price, TotalAmount AS Amount, NULL AS [Period], OrderCode, 
                    Discount, DiscountPercent, Quantity, Quantity AS SubscriptionQuantity, ProductName AS SubscriptionType, NULL AS PriceType
                    FROM {SqlTables.OrderProducts} {NoLock}
                    WHERE ProductCode IS NOT NULL
                    ) AS {SqlTables.OrderSubscription} ON {SqlTables.OrderSubscription}.OrderCode = {SqlTables.Orders}.OrdersCode",
            };

            public static string GetPaymentLaterByDate(DateTime fromDate) =>
                $"SELECT od.*, cs.StoreCode AS StoreCode, cs.BusinessName AS StoreName FROM {Alias} AS od WITH (NOLOCK) JOIN {SqlTables.Customer} AS cs on od.CustomerCode = cs.CustomerCode" +
                $" WHERE [Status] = '{OrderEnum.Status.PaymentLater.ToString()}' AND [UpdatedAt] >= '{fromDate}'";

            public const string GetMetaDataPaymentLater = $"SELECT TOP 1 od.Id as OrderId, od.OrdersCode as OrderCode, od.GrandTotal, cs.StoreCode, cs.BusinessName as StoreName, od.UpdatedBy as ActionBy" +
                $"FROM O_Orders as od JOIN C_Customer as cs ON od.CustomerCode = cs.CustomerCode " +
                $"WHERE od.OrdersCode = @orderCode";
        }
    }
}