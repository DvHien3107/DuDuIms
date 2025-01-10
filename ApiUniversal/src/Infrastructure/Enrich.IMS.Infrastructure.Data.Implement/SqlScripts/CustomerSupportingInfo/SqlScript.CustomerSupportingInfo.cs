using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class CustomerSupportingInfo
        {
            private const string Alias = SqlTables.CustomerSupportingInfo;

            /// <summary>
            /// condition
            /// </summary>
            private sealed class Condition
            {
                public const string Approve = $"";
            }

            /// <summary>
            /// SQL return by customer id
            /// </summary>
            public const string GetByCustomerId = $"SELECT * FROM {Alias} WHERE CustomerId = @customerId";

        }
    }
}
