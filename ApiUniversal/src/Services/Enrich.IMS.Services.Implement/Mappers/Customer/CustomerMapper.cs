using AutoMapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Implement;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class CustomerMapper : BaseMapperAutoMapper<Customer, CustomerDto>, ICustomerMapper
    {
        public CustomerMapper() : base()
        {
        }

        protected override void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Customer, SalesLeadDto>()
               .ForMember(d => d.ContactName, m => m.MapFrom(s => s.ContactName))
               .ForMember(d => d.City, m => m.MapFrom(s => s.City))
               .ForMember(d => d.State, m => m.MapFrom(s => s.State))
               .ForMember(d => d.SalonAddress, m => m.MapFrom(s => s.SalonAddress1))
               .ForMember(d => d.SalonName, m => m.MapFrom(s => s.BusinessName))
               .ReverseMap();
        }
    }
}
