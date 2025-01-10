using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IWebGridConfigRepository : IGenericRepository<WebGridConfig>, ILookupDataRepository
    {
        Task<WebGridConfig> GetUserGridConfigAsync(GridType type, long userId, bool includeGlobal = true);

        Task<int> SaveGridConfigAsync(SaveWebGridConfigRequest request);
    }
}
