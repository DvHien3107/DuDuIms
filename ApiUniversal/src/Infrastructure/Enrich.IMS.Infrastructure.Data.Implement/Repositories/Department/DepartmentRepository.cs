using Dapper;
using Enrich.IMS.Dto.Department;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public class DepartmentRepository : DapperGenericRepository<Department>, IDepartmentRepository
    {
        public DepartmentRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task<IEnumerable<DepartmentOptionItemDto>> GetSalesTeamAsync()
        {
            var query = SqlScript.Department.GetSalesTeam;
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<DepartmentOptionItemDto>(query);
            }
        }
    }
}
