using Enrich.Dto.Base;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ILookupDataService
    {
        /// <summary>
        /// To get data such as: person gender, salesLeadType, ...
        /// </summary>
        /// <param name="request">LookupDataRequest</param>
        /// <returns>IEnumerable<IdNameDto></returns>
        Task<IEnumerable<IdNameDto>> GetIdNamesAsync(LookupDataRequest request);
        void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type);
    }
}
