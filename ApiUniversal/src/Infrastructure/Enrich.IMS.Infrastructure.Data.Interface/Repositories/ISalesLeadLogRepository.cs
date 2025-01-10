using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ISalesLeadLogRepository : IGenericRepository<SalesLeadLog>, ILookupDataRepository
    {
        Task<IEnumerable<SalesLeadLogDto>> GetByCustomerCodeAsync(string CustomerCode);

        /// <summary>
        /// Add or update sales lead comment and get created Id
        /// </summary>
        /// <param name="isNew"></param>
        /// <param name="salesLeadComment"></param>
        /// <returns>created Id</returns>
        Task<int> AddGetAsync(bool isNew, SalesLeadLog salesLeadComment);

        /// <summary>
        /// Search sales lead comment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SalesLeadCommentSearchResponse> SearchAsync(SalesLeadCommentSearchRequest request);
    }
}