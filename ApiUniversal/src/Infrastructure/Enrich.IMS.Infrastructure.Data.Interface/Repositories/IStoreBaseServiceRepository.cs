using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Entities;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IStoreBaseServiceRepository : IGenericRepository<StoreBaseService>
    {
        /// <summary>
        /// Get base service by store code
        /// </summary>
        /// <param name="key"></param>
        /// <param name="store"></param>
        /// <returns></returns>
        Task<StoreBaseService> GetBaseServiceByStoreCode(string key, string store);

    }
}