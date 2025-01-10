using AutoMapper;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;
using Enrich.Services.Interface.Mappers;

namespace Enrich.IMS.Services.Implement.Mappers
{
    public class TicketMapper : BaseMapperAutoMapper<Ticket, TicketDto>, ITicketMapper
    {
        public TicketMapper() : base()
        {
        }
        protected override void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        {
            cfg.CreateMap<Ticket, TicketDto>()
                .ReverseMap();
            cfg.CreateMap<TicketStatus, TicketStatusDto>()
                .ReverseMap();
            cfg.CreateMap<TicketType, TicketTypeDto>()
                .ReverseMap();
            cfg.CreateMap<UploadMoreFile, TicketAttachmentFileDto>().ForMember(des => des.FileUrl, // Property của DTO
                act => act.MapFrom(src => src.FileName))
              .ReverseMap();

        }
    }
}
