using Enrich.Common.Enums;
using Enrich.IMS.Dto;
using DictionaryString = System.Collections.Generic.Dictionary<string, string>;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class CustomerTransaction
        {
            private const string Alias = SqlTables.CustomerTransaction;
            private const string AliasOrder = SqlTables.Orders;
            private const string AliasOsubscription = SqlTables.OrderSubscription;
            private const string AliasCustomer = SqlTables.Customer;

            /// <summary>
            /// customer transaction condition
            /// </summary>
            public sealed class Conditions
            {
                public static string Approved = $"({Alias}.PaymentStatus = '{PaymentEnum.Status.Approved.ToString()}' OR {Alias}.PaymentStatus = '{PaymentEnum.Status.Success.ToString()}')";
                public static string Decline = $"({Alias}.PaymentStatus = '{PaymentEnum.Status.Declined.ToString()}' or {Alias}.PaymentStatus = '{PaymentEnum.Status.Failed.ToString()}')";
            }

            public static DictionaryString JoinsSearch = new DictionaryString
            {
                ["TransactionReport"] = @$"LEFT JOIN {AliasOrder} WITH(NOLOCK) ON {Alias}.OrdersCode = {AliasOrder}.OrdersCode
LEFT JOIN {AliasOsubscription} WITH(NOLOCK) ON {Alias}.OrdersCode = {AliasOsubscription}.OrderCode
LEFT JOIN {AliasCustomer} WITH(NOLOCK) ON {Alias}.CustomerCode = {AliasCustomer}.CustomerCode"
            };

            /// <summary>
            /// SQL return transactions
            /// </summary>
            public const string GetByCustomerCode = $"SELECT * FROM {Alias} {NoLock} WHERE CustomerCode = {Parameters.CustomerCode}";

            /// <summary>
            /// SQL return number of email input
            /// </summary>
            public static string CountApproveTransaction = $"SELECT COUNT(*) FROM {Alias} {NoLock} WHERE OrdersCode = {Parameters.OrderCode} AND {Conditions.Approved}";

            /// <summary>
            /// SQL query return approve transaction
            /// </summary>
            public static string GetApproveTransaction = $"SELECT TOP (1) * FROM {Alias} {NoLock} WHERE OrdersCode = {Parameters.OrderCode} AND {Conditions.Approved} ORDER BY CreateAt DESC";

            /// <summary>
            /// SQL query return failed transactions from date
            /// </summary>
            public static string GetFailedTransactionFromDate =
                @$"SELECT ct.OrdersCode as OrderCode, c.Id as CustomerId, od.Id as OrderId, 
                ct.Amount, ct.PaymentNote as PaymentType, ct.ResponseText as Message, c.BusinessName as CustomerName, c.StoreCode, cc.CardHolderName, cc.CardNumber, cc.CardType
                FROM {Alias} as ct {NoLock}
                LEFT JOIN {SqlTables.Customer} as c on ct.CustomerCode = c.CustomerCode
                LEFT JOIN {SqlTables.CustomerCard} as cc on ct.Card = cc.Id
                LEFT JOIN {SqlTables.Orders} as od on ct.OrdersCode = od.OrdersCode
                WHERE {Conditions.Decline} and CAST(ct.CreateAt AS DATETIME) >= CAST({Parameters.FromDate} AS DATETIME) ORDER BY ct.CreateAt DESC";

            /// <summary>
            /// SQL query return payment failed event by transaction Id
            /// </summary>
            public const string GetMetaDataPaymentFailed =
                @$"SELECT TOP 1 ct.Id as TransactionId, ct.OrdersCode as OrderCode, c.Id as CustomerId, od.Id as OrderId, 
                ct.Amount as GrandTotal, ct.PaymentNote, ct.ResponseText, c.BusinessName as CustomerName, c.StoreCode FROM {Alias} as ct {NoLock}
                LEFT JOIN {SqlTables.Customer} as c on ct.CustomerCode = c.CustomerCode
                LEFT JOIN {SqlTables.Orders} as od on ct.OrdersCode = od.OrdersCode
                WHERE ct.Id = {Parameters.TransactionId}";
        }
    }
}
