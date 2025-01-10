using Enrich.IMS.Entities;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class SalesLeadLogRepository
    {
        public async Task<int> AddGetAsync(bool isNew, SalesLeadLog salesLeadLog)
        {
            if (isNew)
            {
                var response = await AddGetAsync(salesLeadLog);
                if(response != null)
                {
                    return response.Id;
                }
            }
            else
            {
                var affect = await UpdateAsync(salesLeadLog);
                if (affect == 0)
                    throw new Exception("Cannot save property");
            }
            return 0;
        }
    }
}