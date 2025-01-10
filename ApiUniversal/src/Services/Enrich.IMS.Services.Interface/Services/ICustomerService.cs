using Enrich.Core.Infrastructure.Services;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.Customer.QueryRequests.NewRequest;
using Enrich.IMS.Entities;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Interface.Services
{
    public interface ICustomerService : IGenericService<Customer, CustomerDto> 
    {
        public Task<Customer> GetById(int id);
        public Task<Customer> GetByStoreCode(string storeCode);
        public Task<Customer> GetByCustomerCodeAsync(string customerCode);

        /// <summary>
        /// Search customer. Use for data grid
        /// </summary>
        /// <param name="request">CustomerSearchRequest</param>
        /// <returns>CustomerSearchResponse</returns>
        Task<CustomerSearchResponse> SearchAsync(CustomerSearchRequest request);
        Task<CustomerSearchResponse> SearchAsyncv2(CustomerSearchRequest request);

        Task<CustomerReportResponse> ReportAsync(CustomerReportRequest request);

        Task<CustomerChartReportResponse> ReportCustomerForChartAsync(CustomerChartReportRequest request);
        Task<ObjectResponse> ReportCustomerForChartProc(CustomerChartReportRequest request);
        Task<ObjectResponse> ReportCustomerByMonth(ReportCustomerByMonthRqt request);
        /// <summary>
        /// Merchant detail
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="loadOption"></param>
        /// <returns></returns>
        Task<CustomerDetailResponse> GetDetailAsync(long customerId, CustomerDetailLoadOption loadOption = null);
    }
}
