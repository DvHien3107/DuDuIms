using Dapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class AccessKeyRepository : DapperGenericRepository<AccessKey>, IAccessKeyRepository
    {
        public AccessKeyRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
     
        public async Task<AccessKey> GetAccessKeyByKey(string secretKey)
        {

            var query = $"SELECT TOP(1) * FROM {SqlTables.AccessKey} WITH (NOLOCK) WHERE [Key] = @secretKey and ISNULL(IsActive,1)=1";
            var parameters = new DynamicParameters();
            parameters.Add("secretKey", secretKey);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<AccessKey>(query, parameters);
            }
        }
    }
}
