using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ISalesLeadService : IGenericService<SalesLead, SalesLeadDto>, ILookupDataService
    {
        /// <summary>
        /// Get salesLead by customer code
        /// </summary>
        /// <param name="customercode">customercode</param>
        /// <returns>SalesLead</returns>
        Task<SalesLead> GetByCustomerCode(string customercode);

        /// <summary>
        /// Search sales lead. Use for data grid
        /// </summary>
        /// <param name="request">SalesLeadSearchRequest</param>
        /// <returns>SalesLeadSearchResponse</returns>
        Task<SalesLeadSearchResponse> SearchAsync(SalesLeadSearchRequest request);

        /// <summary>
        /// quick search sales lead
        /// </summary>
        /// <param name="text">search texy</param>
        /// <returns>IEnumerable<SalesLeadQuickSearchItemDto></returns>
        Task<IEnumerable<SalesLeadQuickSearchItemDto>> QuickSearchAsync(string text);

        /// <summary>
        /// Create new salesLead
        /// </summary>
        /// <param name="dto">new SalesLeadDto</param>
        /// <returns>UpdateSalesLeadResponse</returns>
        Task<SalesLeadUpdateResponse> CreateSalesLeadAsync(SalesLeadDto dto);

        /// <summary>
        /// Update an existed saleslead
        /// </summary>
        /// <param name="request">UpdateSalesLeadRequest</param>
        /// <returns>UpdateSalesLeadResponse</returns>
        Task<SalesLeadUpdateResponse> UpdateSalesLeadAsync(SalesLeadUpdateRequest request);

        /// <summary>
        /// Get sales lead detail  
        /// </summary>
        /// <param name="salesLeadId"> sales lead Id </param>
        /// <param name="loadOption"> SalesLeadLoadOption </param>
        /// <returns></returns>
        Task<SalesLeadDto> GetSalesLeadDetailAsync(string salesLeadId, SalesLeadDetailLoadOption loadOption = null);

        /// <summary>
        /// Delete multiple salesLead
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<int> DeleteSalesLeadsAsync(string[] ids);

        /// <summary>
        /// Import salesLead
        /// </summary>
        /// <param name="request">ImportSalesLeadRequest</param>
        /// <returns></returns>
        Task<ImportSalesLeadResponse> ImportSalesLeadAsync(ImportSalesLeadRequest request);

    }
}
