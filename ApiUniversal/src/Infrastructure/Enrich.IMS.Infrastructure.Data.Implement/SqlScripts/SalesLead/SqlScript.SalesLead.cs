using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class SalesLead
        {
            private const string Alias = SqlTables.SalesLead;
            public const string QuerySearch = @"#SELECT#
FROM " + Alias + @" WITH (NOLOCK)
#EXTENDJOIN#
WHERE 1=1 
	#CONDITION#
#ORDERBY#";

            /// <summary>
            /// SQL return number of email input
            /// </summary>
            public const string CountEmail = $"SELECT COUNT(1) FROM {Alias} WITH (NOLOCK) WHERE [{SqlColumns.SalesLead.Email}] = @email";
            public const string GetByCustomerCode = $"SELECT TOP 1 * FROM {Alias} WITH (NOLOCK) WHERE [CustomerCode] = @customercode";
            public const string GenerateCode = $"SELECT TOP 1 MAX(CustomerCode) FROM {Alias} WITH (NOLOCK)";
        }
    }
}
