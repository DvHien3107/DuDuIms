using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Department;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IDepartmentService : IGenericService<Department, DepartmentDto>, ILookupDataService
    {
        Task<IEnumerable<DepartmentOptionItemDto>> GetSalesTeamAsync();
    }
}