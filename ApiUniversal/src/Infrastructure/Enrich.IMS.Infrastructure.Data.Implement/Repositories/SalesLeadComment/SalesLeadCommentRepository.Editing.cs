using Enrich.IMS.Entities;
using System;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class SalesLeadCommentRepository
    {
        public async Task<int> AddGetAsync(bool isNew, SalesLeadComment salesLeadComment)
        {
            if (isNew)
            {
                var response = await AddGetAsync(salesLeadComment);
                if(response != null)
                {
                    return response.Id;
                }
            }
            else
            {
                var affect = await UpdateAsync(salesLeadComment);
                if (affect == 0)
                    throw new Exception("Cannot save property");
            }
            return 0;
        }
    }
}