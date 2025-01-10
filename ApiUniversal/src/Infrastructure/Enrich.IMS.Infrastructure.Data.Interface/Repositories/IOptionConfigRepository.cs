using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Entities;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IOptionConfigRepository : IGenericRepository<OptionConfig>
    {
        /// <summary>
        /// Get option config by config key
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns>Return option config</returns>
        public Task<OptionConfig> GetConfigAsync(string configKey);
    }
}
