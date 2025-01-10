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
    public class MemberAccessKeyRepository : DapperGenericRepository<MemberAccessKey>, IMemberAccessKeyRepository
    {
        public MemberAccessKeyRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }
     
        public async Task<MemberAccessKey> GetAccessKeyByKey(string secretKey)
        {

            var query = $"SELECT TOP(1) * FROM {SqlTables.MemberAccessKey} WITH (NOLOCK) WHERE secretKey = @secretKey and Password = @Password and ISNULL(IsActive,1)=1 AND DeletedDate is null";
            var parameters = new DynamicParameters();
            parameters.Add("secretKey", secretKey);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<MemberAccessKey>(query, parameters);
            }
        }
    }
}
