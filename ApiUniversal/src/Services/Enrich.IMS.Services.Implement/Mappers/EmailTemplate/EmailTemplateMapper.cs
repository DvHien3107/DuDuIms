using AutoMapper;
using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class EmailTemplateMapper : BaseMapperAutoMapper<EmailTemplate, EmailTemplateDto>, IEmailTemplateMapper
    {
        public EmailTemplateMapper() : base()
        {
        }

        //protected override void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        //{
        //    //base.CongigMapperProvider(cfg);
        //    cfg.CreateMap<EmailTemplate, EmailTemplateDto>()
        //        .ForMember(dest => dest.Id, opts => opts.MapFrom(source => source.Id))
        //        .ReverseMap();
        //}
    }
}
