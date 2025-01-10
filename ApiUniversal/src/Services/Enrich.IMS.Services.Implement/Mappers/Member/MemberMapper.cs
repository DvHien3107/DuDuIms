using AutoMapper;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;


namespace Enrich.IMS.Services.Implement.Mappers
{
    public class MemberMapper : BaseMapperAutoMapper<Member, MemberDto>, IMemberMapper
    {
        public MemberMapper() : base()
        {
        }

        //protected override void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        //{
        //    //base.CongigMapperProvider(cfg);
        //    cfg.CreateMap<Member, MemberDto>()
        //        .ForMember(dest => dest.Id, opts => opts.MapFrom(source => source.Id))
        //        .ReverseMap();
        //}
    }
}
