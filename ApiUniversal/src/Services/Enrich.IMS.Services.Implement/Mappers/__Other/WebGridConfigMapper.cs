using Enrich.IMS.Dto.WebGridConfig;
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
    public class WebGridConfigMapper : BaseMapperAutoMapper<WebGridConfig, WebGridConfigDto>, IWebGridConfigMapper
    {
    }
}
