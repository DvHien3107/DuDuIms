using Enrich.IMS.Dto;

namespace Enrich.IMS.Infrastructure.Data.Implement
{
    internal static partial class SqlScript
    {
        public static class SystemConfiguration
        {
            private const string Alias = SqlTables.SystemConfiguration;
            private const string AliasOptionConfig = SqlTables.OptionConfig;

            public const string Get = $"SELECT TOP(1) * FROM {Alias} WITH (NOLOCK)";
            public const string GetConfigurationRecuringTime = 
                $"SELECT TOP(1) Value FROM {AliasOptionConfig} WITH (NOLOCK) WHERE [Key] = 'config.autoscan_time'";
        }
    }
}
