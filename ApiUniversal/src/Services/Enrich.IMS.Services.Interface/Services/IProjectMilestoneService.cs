using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IProjectMilestoneService : IGenericService<ProjectMilestone, ProjectMilestoneDto>, ILookupDataService
    {
        Task<IEnumerable<ProjectMilestone>> GetAllProjectAsync();
        Task<ProjectMilestone> GetAsync(string projectId);
    }
}
