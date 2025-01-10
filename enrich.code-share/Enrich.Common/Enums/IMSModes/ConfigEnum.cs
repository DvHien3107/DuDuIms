using System.ComponentModel.DataAnnotations;

namespace Enrich.Common.Enums
{
    public class ConfigEnum
    {
        public sealed class Key
        {
            public const string lastScanTransactionFailed = "config.lastScanTransactionFailed";
            public const string periodScanTransactionFailed = "config.periodScanTransactionFailed";
            public const string autoScanTime = "config.autoscan_time";
        }
    }
}
