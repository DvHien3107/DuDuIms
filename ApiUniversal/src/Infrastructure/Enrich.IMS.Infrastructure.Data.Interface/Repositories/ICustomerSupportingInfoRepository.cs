using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ICustomerSupportingInfoRepository : IGenericRepository<CustomerSupportingInfo>
    {
        Task<IEnumerable<CustomerSupportingInfoDto>> GetByCustomerIdAsync(long CustomerId);
    }
}