using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IPartnerService : IGenericService<Partner, PartnerDto>, ILookupDataService
    {
    }
}