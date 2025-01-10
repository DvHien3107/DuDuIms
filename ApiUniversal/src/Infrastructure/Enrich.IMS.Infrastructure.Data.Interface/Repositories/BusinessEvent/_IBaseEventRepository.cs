using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface
{
    public interface IBaseEventRepository
    {
        Task<IEnumerable<BaseEvent<TValue>>> GetEventAsync<TValue>(BusinessEventRequest request);
    }
}