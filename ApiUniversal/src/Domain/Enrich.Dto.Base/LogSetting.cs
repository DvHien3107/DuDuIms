using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Dto.Base
{
    public class LogSetting
    {

        public ProviderSetting Provider { get; set; } = new ProviderSetting();

        public static LogSetting Default => new LogSetting { Provider = { RollingFile = true } };

        #region Extend

        public class ProviderSetting
        {
            #region RollingFile

            public bool RollingFile { get; set; }

            public RollingFileSetting RollingFileSetting { get; set; } = new RollingFileSetting();

            public bool IsValidRollingFile => RollingFile;

            #endregion

            #region GrayLog

            public bool GrayLog { get; set; }

            public GrayLogSetting GrayLogSetting { get; set; } = new GrayLogSetting();

            public bool IsValidGrayLog => GrayLog && !string.IsNullOrWhiteSpace(GrayLogSetting.SteamTokenName) && !string.IsNullOrWhiteSpace(GrayLogSetting.SteamTokenKey);

            #endregion        

        }

        public class RollingFileSetting
        {
            public string PathFormat { get; set; }

            public string OutputTemplate { get; set; }

            public int RetainedDays { get; set; }
        }

        public class GrayLogSetting
        {
            public string Facility { get; set; }

            public string RemoteAddress { get; set; }

            public int RemotePort { get; set; }

            public string MinimumLogEventLevel { get; set; }

            public string SteamTokenName { get; set; }

            public string SteamTokenKey { get; set; }
        }

        #endregion
    }
}
