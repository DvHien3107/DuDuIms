using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface
{
    public interface IAccessKeyRepository : IGenericRepository<AccessKey>
    {
        public Task<AccessKey> GetAccessKeyByKey(string secretKey);
    }
}
