using AutoMapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class SalesLeadMapper : BaseMapperAutoMapper<SalesLead, SalesLeadDto>, ISalesLeadMapper
    {
        public SalesLeadMapper() : base()
        {
        }

        protected override void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<SalesLeadDetailLoadOption, SalesLeadUpdateOption>();
            cfg.CreateMap<SalesLead, SalesLeadDto>()
                .ForMember(dest => dest.Id, opts => opts.MapFrom(source => source.Id)) 
                .ReverseMap();
            cfg.CreateMap<Customer, CustomerDto>()
                .ReverseMap();

        }
    }
}
