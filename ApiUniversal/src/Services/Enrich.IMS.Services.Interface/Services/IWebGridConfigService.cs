using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.WebGridConfig;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IWebGridConfigService : IGenericService<WebGridConfig, WebGridConfigDto>, ILookupDataService
    {
        Task<GridConfigDto> GetUserGridConfigAsync(GridType type, long userId, bool defaltIfNotFound = true);

        Task<GridConfigDto> GetFavFilterGridConfigAsync(GridType type, long userId, int favFilterId, bool defaltIfNotFound = false);

        Task<bool> SaveConfigAsync(SaveWebGridConfigRequest request);
       
    }
}
