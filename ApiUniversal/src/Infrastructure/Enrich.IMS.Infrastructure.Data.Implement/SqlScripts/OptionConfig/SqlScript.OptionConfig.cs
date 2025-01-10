using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class OptionConfig
        {
            private const string Alias = SqlTables.OptionConfig;

            /// <summary>
            /// SQL return config by config key
            /// </summary>
            public static string GetConfig(string configKey) => $"SELECT TOP (1) * FROM {Alias} WHERE [Key] = '{configKey}'";
        }
    }
}
