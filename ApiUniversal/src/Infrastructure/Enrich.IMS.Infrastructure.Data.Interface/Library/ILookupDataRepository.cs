using Enrich.Dto.Base;
using Enrich.IMS.Dto.Lookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface
{
    public interface ILookupDataRepository
    {
        Task<IEnumerable<IdNameDto>> GetIdNamesAsync(LookupDataRequest request);
    }
}
