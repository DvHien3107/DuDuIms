using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ISystemConfigurationRepository : IGenericRepository<SystemConfiguration>
    {
        public SystemConfiguration GetSystemConfiguration();
        public Task<SystemConfiguration> GetSystemConfigurationAsync();
        public string GetConfigurationRecuringTime();
    }
}
