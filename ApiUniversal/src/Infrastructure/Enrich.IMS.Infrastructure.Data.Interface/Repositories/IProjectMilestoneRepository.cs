using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IProjectMilestoneRepository : IGenericRepository<ProjectMilestone>, ILookupDataRepository
    {
        /// <summary>
        /// Get all ticket project
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProjectMilestone>> GetAllProjectAsync();
    }
}
