using Enrich.Core.Infrastructure.Services;
using Enrich.Dto.Base;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.EnumValue;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    /// <summary>
    /// EnumValue, using to get system value, user can not add or update
    /// </summary>
    public interface IEnumValueService : IGenericService<EnumValue, EnumValueDto>, ILookupDataService
    {
        /// <summary>
        /// Get data lookup. Such as salesLead type, person gender,....
        /// </summary>
        /// <param name="type"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<(string Namespace, IEnumerable<IdNameDto> IdNames)> GetLookupAsync(LookupDataType type, EnumValueLookupDataRequest request);
    }
}
