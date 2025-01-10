using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Entities;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ISalesLeadInteractionStatusService : IGenericService<SalesLeadInteractionStatus, SalesLeadInteractionStatusDto>, ILookupDataService
    {
    }
}