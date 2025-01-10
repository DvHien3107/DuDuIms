using Dapper;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class ProjectMilestoneRepository : DapperGenericRepository<ProjectMilestone>, IProjectMilestoneRepository
    {
        public ProjectMilestoneRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }


        public async Task<IEnumerable<ProjectMilestone>> GetAllProjectAsync()
        {
                var query = $"SELECT * FROM {SqlTables.ProjectMilestone} {SqlScript.NoLock} WHERE Type = 'project' and ParentId IS NULL";

                using (var connection = GetDbConnection())
                {
                    return await connection.QueryAsync<ProjectMilestone>(query);
                }
        }
    }
}
