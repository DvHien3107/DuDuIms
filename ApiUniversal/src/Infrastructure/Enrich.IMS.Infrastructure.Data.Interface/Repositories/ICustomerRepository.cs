using Enrich.Core.Infrastructure.Repository;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Interface.Repositories
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Validation email is exist in data
        /// </summary>
        /// <param name="email"></param>
        /// <returns>
        /// Return true when it exists.
        /// Return false when it not exists.
        /// </returns>
        public bool IsExistEmail(string email);
        bool IsExistACH(string CustomerCode);

        /// <summary>
        /// Validation merchant is pending delivery
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        bool IsPendingDelivery(string storeCode);

        public Task<Customer> GetById(int Id);
        public Task<Customer> GetByStoreCode(string StoreCode);
        public Customer GetByCustomerCode(string CustomerCode);
        public Task<Customer> GetByCustomerCodeAsync(string CustomerCode);

        /// <summary>
        /// Get base infomation by Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<CustomerBaseDto> GetBaseInfomationByIdAsync(long Id);

        /// <summary>
        /// Get customer Dto data 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<CustomerDto> GetByIdAsync(long Id);

        /// <summary>
        /// search customer
        /// </summary>
        /// <param name="request">CustomerSearchRequest</param>
        /// <returns>CustomerSearchResponse</returns>
        Task<CustomerSearchResponse> SearchAsync(CustomerSearchRequest request);
        Task<CustomerSearchResponse> SearchAsyncv2(CustomerSearchRequest request);

        /// <summary>
        /// Get merchant sumary 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //Task<CustomerSearchSummary> GetMerchantSumaryAsync(CustomerSearchRequest request);

        /// <summary>
        /// Report list merchant active / cancel
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<CustomerReportResponse> ReportAsync(CustomerReportRequest request);

        /// <summary>
        /// Report new account per week/month for display chart
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<CustomerChartReportResponse> ReportCustomerForChartAsync(CustomerChartReportRequest request);
        Task<IEnumerable<Object>> ExcuteSqlAsync(string sqlQuery);
    }
}