using Enrich.IMS.Dto;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class SalesLeadInteractionStatusMapper : BaseMapperAutoMapper<SalesLeadInteractionStatus, SalesLeadInteractionStatusDto>, ISalesLeadInteractionStatusMapper
    {
        public SalesLeadInteractionStatusMapper() : base()
        {
        }
    }
}