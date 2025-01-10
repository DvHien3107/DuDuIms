using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadInteractionStatusService : EnrichBaseService<SalesLeadInteractionStatus, SalesLeadInteractionStatusDto>, ISalesLeadInteractionStatusService
    {
        public SalesLeadInteractionStatusService(
            ISalesLeadInteractionStatusMapper mapper,
            ISalesLeadInteractionStatusRepository repository
            ) : base(repository, mapper)
        {
        }

        /// <summary>
        /// Optimize request for get lookup data
        /// </summary>
        /// <param name="request"></param>
        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
            request.FieldNames.Add(new SqlMapDto("Id", "Value"));
            request.FieldNames.Add(new SqlMapDto("Name", "Name"));
            request.ExtendConditions = $"[Status] = 1";
        }
    }
}