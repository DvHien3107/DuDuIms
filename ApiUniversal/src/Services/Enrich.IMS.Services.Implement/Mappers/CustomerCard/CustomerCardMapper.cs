using AutoMapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class CustomerCardMapper : BaseMapperAutoMapper<CustomerCard, CustomerCardDto>, IPartnerMapper
    {
        public CustomerCardMapper() : base()
        {
        }

    }
}
