using Enrich.Core.Infrastructure.Repository;
using Enrich.Dto.Base;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface IEnumValueRepository : IGenericRepository<EnumValue>, ILookupDataRepository
    {      
        Task<List<IdNameDto>> GetEnumValuesAsync(string nameSpace, params string[] enumValues);
    }
}
