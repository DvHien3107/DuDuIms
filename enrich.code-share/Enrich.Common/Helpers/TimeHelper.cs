using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Enrich.Common.Helpers
{
    public static class TimeHelper
    {
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(unixTimeStamp).ToLocalTime();
        }
        public static DateTime GetNow() => DateTime.Now;

        /// <summary>
        /// Get UTC date time
        /// </summary>
        /// <returns>Return UTC date time now</returns>
        public static DateTime GetUTCNow() => DateTime.UtcNow;
    }
}
