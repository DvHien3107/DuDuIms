using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Department;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class DepartmentService : EnrichBaseService<Department, DepartmentDto>, IDepartmentService
    {
        IDepartmentRepository _repository;
        public DepartmentService(
            IDepartmentMapper mapper,
            IDepartmentRepository repository
            ) : base(repository, mapper)
        {
            _repository = repository;
        }


        /// <summary>
        /// Optimize request for get lookup data
        /// </summary>
        /// <param name="request"></param>
        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
            request.FieldNames.Add(new SqlMapDto("Id", "Value"));
            request.FieldNames.Add(new SqlMapDto("Name", "Name"));
            request.ExtendConditions = $"[Type] = 'SALES' AND [Active] = 1 AND [ParentDepartmentId] IS NOT NULL";
        }

        public async Task<IEnumerable<DepartmentOptionItemDto>> GetSalesTeamAsync()
        {
            return await _repository.GetSalesTeamAsync();
        }
    }
}