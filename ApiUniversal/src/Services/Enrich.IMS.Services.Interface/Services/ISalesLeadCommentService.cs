using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Entities;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ISalesLeadCommentService : IGenericService<SalesLeadComment, SalesLeadCommentDto>, ILookupDataService
    {
        /// <summary>
        /// Get sales lead comment by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// Data sales lead comment
        /// </returns>
        Task<SalesLeadCommentDto> GetByIdAsync(int id);

        /// <summary>
        /// create new sales lead comment
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<SalesLeadCommentUpdateResponse> CreateAsync(SalesLeadCommentDto dto);

        /// <summary>
        /// Update data sales lead comment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SalesLeadCommentUpdateResponse> UpdateAsync(SalesLeadCommentUpdateRequest request);

        /// <summary>
        /// Search sales lead comment
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<SalesLeadCommentSearchResponse> SearchAsync(SalesLeadCommentSearchRequest request);
    }
}