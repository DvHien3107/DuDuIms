using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class CustomerCard
        {
            private const string Alias = SqlTables.CustomerCard;

            private sealed class Conditions
            {
                public const string Active = "[Active] = 1";
                public const string Default = "[IsDefault] = 1";
            }
            /// <summary>
            /// SQL query return customer card by MxmerchantId
            /// </summary>
            public const string GetByMxMerchantId = $"SELECT TOP(1) * FROM {Alias} {NoLock} WHERE [MxMerchant_Id] = {Parameters.MxMerchantId}";

            /// <summary>
            /// SQL query return customer card default by customer code
            /// </summary>
            public const string GetDefaultByCustomerCode = $"SELECT TOP(1) * FROM {Alias} {NoLock} WHERE {Conditions.Active} and {Conditions.Default} AND [CustomerCode] = {Parameters.CustomerCode}";
            
            /// <summary>
            /// SQL query return customer card by customer code
            /// </summary>
            public const string GetByCustomerCode = $"SELECT * FROM {Alias} {NoLock} WHERE {Conditions.Active} AND [CustomerCode] = {Parameters.CustomerCode}";
        }
    }
}
