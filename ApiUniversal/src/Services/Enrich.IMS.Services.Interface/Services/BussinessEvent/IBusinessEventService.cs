using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IBusinessEventService
    {
        Task<IEnumerable<BaseEvent<TValue>>> GetBusinessEventAsync<TValue>(BusinessEventRequest request);
    }
}