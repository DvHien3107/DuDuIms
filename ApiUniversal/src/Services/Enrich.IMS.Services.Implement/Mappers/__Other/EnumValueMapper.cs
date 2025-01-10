using AutoMapper;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.EnumValue;
using Enrich.IMS.Entities;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Mappers.Other
{
    public class EnumValueMapper : BaseMapperAutoMapper<EnumValue, EnumValueDto>, IEnumValueMapper
    {
        protected override void ConfigMapperProvider(IMapperConfigurationExpression cfg)
        {
            base.ConfigMapperProvider(cfg);

            cfg.CreateMap<EnumValueDto, IdNameDto>().ReverseMap();
        }
    }
}
