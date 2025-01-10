using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto.Department;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IDepartmentRepository : IGenericRepository<Department>, ILookupDataRepository
    {
        public Task<IEnumerable<DepartmentOptionItemDto>> GetSalesTeamAsync();
    }
}