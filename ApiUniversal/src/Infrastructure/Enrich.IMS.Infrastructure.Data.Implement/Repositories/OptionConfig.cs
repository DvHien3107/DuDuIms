using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class OptionConfigRepository : DapperGenericRepository<OptionConfig>, IOptionConfigRepository
    {
        public OptionConfigRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        /// <summary>
        /// Get value in option config
        /// </summary>
        /// <param name="configKey"></param>
        /// <returns>Return value of key config in option config</returns>
        public async Task<OptionConfig> GetConfigAsync(string configKey)
        {
            var query = SqlScript.OptionConfig.GetConfig(configKey);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<OptionConfig>(query);
            }
        }
    }
}
