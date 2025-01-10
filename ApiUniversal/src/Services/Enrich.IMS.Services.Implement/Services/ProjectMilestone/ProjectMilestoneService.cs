using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class ProjectMilestoneService : EnrichBaseService<ProjectMilestone, ProjectMilestoneDto>, IProjectMilestoneService
    {
        #region field
        private readonly IProjectMilestoneRepository _repository;
        private readonly EnrichContext _context;
        private IProjectMilestoneMapper _mapper => _mapperGeneric as IProjectMilestoneMapper;
      
        #endregion

        #region Contructor
        public ProjectMilestoneService(IProjectMilestoneRepository repository, IProjectMilestoneMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
        }
        #endregion

        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
            request.FieldNames.Add(new SqlMapDto("Code", "Value"));
            request.FieldNames.Add(new SqlMapDto("Name", "Name"));
            request.ExtendConditions = $"[Status] = 1";
        }

        public async Task<IEnumerable<ProjectMilestone>> GetAllProjectAsync()
        {
            return await _repository.GetAllProjectAsync();
        }

        public async Task<ProjectMilestone> GetAsync(string projectId)
        {
            return await _repository.FindByIdAsync(projectId);
        }
    }
}
