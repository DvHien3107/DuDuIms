using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface
{
    public interface IBusinessEventRepository : IGenericRepository<BusinessEvent>, IBaseEventRepository
    {
        Task CreateAsync(string NameSpace, string Event, string Description, string MetaData);
    }
}