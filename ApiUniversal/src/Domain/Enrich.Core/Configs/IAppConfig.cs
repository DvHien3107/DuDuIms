using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.Core.Configs
{
    public interface IAppConfig 
    {
        #region Caches

        (string Name, int ExpireMinutes) CacheInfo(string key);

        #endregion

        #region IMS Email Service
        public string IMSEmailBaseServiceUrl { get; }
        public string IMSEmailSendUrl { get; }
        public string IMSEmailSendBySendGridIdUrl { get; }
        public string IMSEmailServiceClientVersion { get; }    
        public string IMSEmailServiceClientSecret { get; }

        #endregion
    }
}
