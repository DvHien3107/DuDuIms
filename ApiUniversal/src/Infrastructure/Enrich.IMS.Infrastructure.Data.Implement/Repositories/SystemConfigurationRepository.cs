using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class SystemConfigurationRepository : DapperGenericRepository<SystemConfiguration>, ISystemConfigurationRepository
    {
        public SystemConfigurationRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        { }

        public string GetConfigurationRecuringTime()
        {
            var query = SqlScript.SystemConfiguration.GetConfigurationRecuringTime;
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<string>(query);
            }
        }
        public SystemConfiguration GetSystemConfiguration()
        {
            var query = SqlScript.SystemConfiguration.Get;
            using (var connection = GetDbConnection())
            {
                return connection.QueryFirstOrDefault<SystemConfiguration>(query);
            }
        }
        public async Task<SystemConfiguration> GetSystemConfigurationAsync()
        {
            var query = SqlScript.SystemConfiguration.Get;
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<SystemConfiguration>(query);
            }
        }
    }
}
