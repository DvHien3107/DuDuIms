using AutoMapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Authentication;
using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Implement;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class AccessKeyMapper : BaseMapperAutoMapper<AccessKey, AccessKeyDto>, IAccessKeyMapper
    {
        public AccessKeyMapper() : base()
        {
        }
        protected override void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<AccessKey, AccessKeyDto>()
               .ReverseMap();
        }
    }
}
