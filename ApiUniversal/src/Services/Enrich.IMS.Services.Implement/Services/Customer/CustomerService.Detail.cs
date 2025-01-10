using Enrich.Common;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class CustomerService
    {
        public async Task<CustomerDetailResponse> GetDetailAsync(long customerId, CustomerDetailLoadOption loadOption = null)
        {
            var baseInfomation = await _repository.GetBaseInfomationByIdAsync(customerId);
            if (baseInfomation == null)
            {
                return null;
            }

            var response = new CustomerDetailResponse();
            var tasks = new MultipleTasks<CustomerDetailResponse>(response);

            if (loadOption == null || loadOption.Information) response.Information = baseInfomation;

            if (loadOption.SupportingInfo)
                tasks.Add(nameof(CustomerDetailResponse.SupportingInfos), GetCustomerSupportingInfoAsync(customerId));

            if (loadOption.Comment)
                tasks.Add(nameof(CustomerDetailResponse.Comments), GetCommentAsync(baseInfomation.CustomerCode));

            if (loadOption.Log)
                tasks.Add(nameof(CustomerDetailResponse.Logs), GetLogAsync(baseInfomation.CustomerCode));

            if (loadOption.Card)
                tasks.Add(nameof(CustomerDetailResponse.Cards), GetCardAsync(baseInfomation.CustomerCode));

            await tasks.WhenAll();
            return response;
        }

        private async Task<IEnumerable<CustomerSupportingInfoDto>> GetCustomerSupportingInfoAsync(long customerId)
        {
            var repository = _container.Resolve<ICustomerSupportingInfoRepository>();
            return await repository.GetByCustomerIdAsync(customerId);
        }

        private async Task<IEnumerable<SalesLeadCommentDto>> GetCommentAsync(string customerCode)
        {
            var repository = _container.Resolve<ISalesLeadCommentRepository>();
            return await repository.GetByCustomerCodeAsync(customerCode);
        }

        private async Task<IEnumerable<SalesLeadLogDto>> GetLogAsync(string customerCode)
        {
            var repository = _container.Resolve<ISalesLeadLogRepository>();
            return await repository.GetByCustomerCodeAsync(customerCode);
        }

        private async Task<CustomerDto> GetProfileAsync(long customerId)
        {
            var repository = _container.Resolve<ICustomerRepository>();
            return await repository.GetByIdAsync(customerId);
        }

        private async Task<IEnumerable<RecurringPlanningDto>> GetRecurringAsync(string customerCode)
        {
            var repository = _container.Resolve<IRecurringPlanningRepository>();
            return await repository.GetByCustomerCodeAsync(customerCode);
        }

        private async Task<IEnumerable<CustomerCardDto>> GetCardAsync(string customerCode)
        {
            var repository = _container.Resolve<ICustomerCardRepository>();
            return await repository.GetByCustomerCodeAsync(customerCode);
        }

        private async Task<IEnumerable<CustomerTransactionDto>> GetTransactionAsync(string customerCode)
        {
            var repository = _container.Resolve<ICustomerTransactionRepository>();
            return await repository.GetByCustomerCodeAsync(customerCode);
        }
    }
}