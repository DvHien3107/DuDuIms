using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.IMS.Dto;
using System;
using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class OrderSubscription
        {
            private const string Alias = SqlTables.OrderSubscription;
            private const string AliasOrder = SqlTables.Orders;
            private const string AliasCustomer = SqlTables.Customer;
            private const string AliasTransaction = SqlTables.CustomerTransaction;

            /// <summary>
            /// customer transaction condition
            /// </summary>
            private sealed class Conditions
            {
            }

            public static string SearchTransactionPackage(string package, string storeCodes, DateTime fromDate, DateTime toDate) =>
$@"SELECT ods.StoreCode AS StoreCode, 
SUM(CASE WHEN ods.Amount IS NOT NULL THEN ods.Amount ELSE (ods.Price * (CASE WHEN ods.Period = 'MONTHLY' 
				THEN (CASE WHEN ods.SubscriptionQuantity IS NOT NULL THEN ods.SubscriptionQuantity ELSE 1 END) 
				ELSE (CASE WHEN ods.Quantity IS NOT NULL THEN ods.Quantity ELSE 1 END) END) - (CASE WHEN ods.Discount IS NOT NULL THEN ods.Discount ELSE 0 END)) END) AS Paid, 
Count(DISTINCT od.OrdersCode) AS NumberTransaction, 
'{package}' AS Package
FROM [Order_Subcription] AS ods
LEFT JOIN O_Orders AS od ON od.OrdersCode = ods.OrderCode
LEFT JOIN License_Product_Item AS lpi ON lpi.License_Product_Id = ods.ProductId
WHERE ods.StoreCode IN ({storeCodes})
AND od.CreatedAt >= {fromDate.SqlVal()} AND od.CreatedAt <= {toDate.SqlVal()}
AND (od.Status = '{OrderEnum.Status.Paid_Wait.ToString()}' OR od.Status = '{OrderEnum.Status.Closed.ToString()}' OR od.Status = '{OrderEnum.Status.PaymentLater.ToString()}')
AND lpi.License_Item_Code = '{package}' AND (lpi.Value > 0 OR lpi.value = -1)
GROUP BY ods.StoreCode";


        }
    }
}
