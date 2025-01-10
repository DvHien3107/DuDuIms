using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ISalesLeadRepository : IGenericRepository<SalesLead>, ILookupDataRepository
    {
        /// <summary>
        /// Validation email is exist in data
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// Return true when it exists.
        /// Return false when it not exists.
        /// </returns>
        bool IsExistEmail(string email);
        SalesLead GetByCustomerCode(string customercode);
        Task<SalesLead> GetByCustomerCodeAsync(string customercode);

        /// <summary>
        /// search sales lead
        /// </summary>
        /// <param name="request">SalesLeadSearchRequest</param>
        /// <returns>SalesLeadSearchResponse</returns>
        Task<SalesLeadSearchResponse> SearchAsync(SalesLeadSearchRequest request);

        /// <summary>
        /// Search sales lead. Use for dropdownlist Need to improve to full text search
        /// </summary>
        /// <param name="request">search text</param>
        /// <returns>SalesLeadSearchResponse</returns>
        Task<IEnumerable<SalesLeadQuickSearchItemDto>> QuickSearchAsync(string text);

        /// <summary>
        /// Generate new customer code
        /// </summary>
        /// <returns></returns>
        Task<string> GenerateCustomerCode();

        /// <summary>
        /// save sale lead by transaction
        /// </summary>
        /// <param name="isNew"></param>
        /// <param name="extendSave"></param>
        /// <returns></returns>
        Task SaveSalesLeadByTranAsync(bool isNew, SalesLead salesLead, Func<string, Task> extendSave = null);

        /// <summary>
        /// Update sales lead customer
        /// </summary>
        /// <param name="customerCode"></param>
        /// <param name="customer"></param>
        /// <param name="isNew"></param>
        /// <returns></returns>
        Task<bool> SaveSaleLeadCustomerAsync(string customerCode, Customer customer, bool isNew = false);

        /// <summary>
        /// get customer by salesLeadId
        /// </summary>
        /// <param name="salesLeadId">saleslead Id</param>
        /// <returns></returns>
        Task<Customer> GetSalesLeadCustomerAsync(string salesLeadId);

        /// <summary>
        /// Delete multiple salesLead
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        Task<int> DeleteSalesLeadsAsync(IEnumerable<string> ids);
    }
}