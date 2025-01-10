using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class PartnerCard
        {
            private const string Alias = SqlTables.PartnerCard;

            /// <summary>
            /// SQL query partner card by MxMerchantId
            /// </summary>
            public const string GetByMxMerchantId = $"SELECT TOP(1) * FROM {Alias} WITH (NOLOCK) WHERE [MxMerchant_Id] = {Parameters.MxMerchantId}";
        }
    }
}
