using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ICommonService : IService
    {
        Task<object> GetLookupDataAsync(GetLookupDataRequest request);

    }
}
