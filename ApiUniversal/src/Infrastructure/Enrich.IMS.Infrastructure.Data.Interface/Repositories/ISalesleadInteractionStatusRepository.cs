using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ISalesLeadInteractionStatusRepository : IGenericRepository<SalesLeadInteractionStatus>, ILookupDataRepository
    {
    }
}