using Enrich.Common.Enums;
using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.StoreBaseService;
using Enrich.IMS.Entities;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IStoreBaseServiceService : IGenericService<StoreBaseService, StoreBaseServiceDto>
    {
        /// <summary>
        /// Validation SMS remaining for send
        /// </summary>
        /// <param name="store"></param>
        /// <param name="totalnumber"></param>
        /// <returns></returns>
        Task<BaseServiceRemainingResponse> SyncRemainingValidationAsync(string store, int totalnumber, BaseServiceEnum.Code baseService);

    }
}