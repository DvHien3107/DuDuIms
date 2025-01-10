using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Authentication;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface IAccessKeyService : IGenericService<AccessKey, AccessKeyDto> 
    {
        Task<AccessKeyDto> GetAccessKeyByKey(string secretKey);
    }
}
