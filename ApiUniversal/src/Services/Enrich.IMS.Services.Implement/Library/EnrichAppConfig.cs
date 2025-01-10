using Enrich.Common;
using Enrich.Core.Configs;
using Enrich.Services.Implement.Library;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Library
{
    public class EnrichAppConfig : ImpBaseConfig, IAppConfig
    {
        private readonly IConfiguration _config;

        public EnrichAppConfig(IConfiguration config)
        {
            _config = config;
        }

        #region IMS Email Service
        public string IMSEmailBaseServiceUrl => _config["Services:Email:BaseUrl"];
        public string IMSEmailSendUrl => _config["Services:Email:Send"];
        public string IMSEmailSendBySendGridIdUrl => _config["Services:Email:SendByGridTemplateId"];
        public string IMSEmailServiceClientSecret => _config["Services:Email:SecretKey"];
        public string IMSEmailServiceClientVersion => _config["Services:Email:Version"];
        #endregion

        public (string Name, int ExpireMinutes) CacheInfo(string key)
        {
            var info = (Name:"", ExpireMinutes:1);
            info.Name = Constants.CacheName.MemoryCache;
            return info;
        }
    }
}
