using AutoMapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class SalesLeadCommentMapper : BaseMapperAutoMapper<SalesLeadComment, SalesLeadCommentDto>, ISalesLeadCommentMapper
    {
        public SalesLeadCommentMapper() : base()
        {
        }
    }
}