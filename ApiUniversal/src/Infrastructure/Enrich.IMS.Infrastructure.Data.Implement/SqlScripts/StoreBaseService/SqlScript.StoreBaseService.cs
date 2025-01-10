using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class StoreBaseService
        {
            private const string Alias = SqlTables.StoreBaseService;

            public static class Key
            {
                public const string StoreCode = "StoreCode";
                public const string KeyName = "KeyName";
            }

            /// <summary>
            /// SQL return Remaining value by store code, key
            /// </summary>
            public const string GetRemaining = $"SELECT * FROM {Alias} WITH (NOLOCK) WHERE [StoreCode] = @{Key.StoreCode} AND [KeyName] = @{Key.KeyName}";
        }
    }
}
