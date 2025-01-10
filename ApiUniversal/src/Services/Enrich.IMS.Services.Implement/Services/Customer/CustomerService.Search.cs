using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.Subscription;
using Enrich.Infrastructure.Data.Dapper.Library;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class CustomerService
    {
        /// <summary>
        /// Search customer. Use for data grid
        /// </summary>
        /// <param name="request">SalesLeadSearchRequest</param>
        /// <returns>SalesLeadSearchResponse</returns>
        public async Task<CustomerSearchResponse> SearchAsync(CustomerSearchRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<CustomerListItemDto>(request);

            OptimizeConditionSearch(request);
            //var replicationRequest = request.Clone();
            var response = await _repository.SearchAsync(request);

            //if (response != null && response.Summary != null)
            //{
            //    response.Summary = await _repository.GetMerchantSumaryAsync(replicationRequest);
            //}

            // format data base on require
            await PopulateItems(response);

            return response;
        }
        public async Task<CustomerSearchResponse> SearchAsyncv2(CustomerSearchRequest request)
        {
            var response = await _repository.SearchAsyncv2(request);
            await PopulateItems(response);
            return response;
        }
        private void OptimizeConditionSearch(CustomerSearchRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new CustomerSearchCondition();
            request.Condition.PopulateCountSummaries = false; // !queryParam.HasPaging() || queryParam.Paging.PageIndex == 1;

            // OnStringFields
            if (request.Condition.OnStringFields?.Count > 0)
            {
                request.Condition.OnStringFields.RemoveAll(a => string.IsNullOrWhiteSpace(a.Key) || string.IsNullOrWhiteSpace(a.Value));
            }
            if (request.Condition.OnStringFields?.Count > 0)
            {
                // convert dto-name to db-name
                var dbFields = FieldDbHelper.GetFields<CustomerDto>();
                request.Condition.OnStringFields = request.Condition.OnStringFields
                    .Select(a => new { FieldDb = dbFields.FirstOrDefault(b => b.Prop.Name.Equals(a.Key)), FieldDto = a })
                    .Where(a => a.FieldDb != null).Select(a => new KeyValueDto<string> { Key = a.FieldDb.Name, Value = a.FieldDto.Value }).ToList();
            }
        }
        private async Task PopulateItems(CustomerSearchResponse response)
        {
            if (response == null || response.Records == null) return;

            var listStoreCode = response.Records.Select(x => x.StoreCode).Where(c=> !string.IsNullOrEmpty(c));
            var listLicense = await _storeServiceRepository.GetLicenseStatusAsync(listStoreCode);
            //customize data after get from responsitory
            foreach (var customer in response.Records)
            {
                var license = listLicense.FirstOrDefault(x => x.StoreCode == customer.StoreCode);
                if (license != null)
                {
                    customer.License = new LicenseStatusDto()
                    {
                        LicenseName = license.LicenseName,
                        RemainingDate = license.RemainingDate,
                        EffectiveDate = license.EffectiveDate,
                        LicenseStatus = (int)SubscriptionEnum.LicenseStatus.Active
                    };
                    if (license.RemainingDate <= 0)
                    {
                        customer.License.LicenseStatus = (int)SubscriptionEnum.LicenseStatus.Expired;
                    }
                    if (license.EffectiveDate.Date > System.DateTime.UtcNow.Date)
                    {
                        customer.License.LicenseStatus = (int)SubscriptionEnum.LicenseStatus.FutureActive;
                    }
                }
                if (!string.IsNullOrEmpty(customer.MID))
                {
                    customer.Terminal = new TerminalStatusDto()
                    {
                        MID = customer.MID,
                        Status = customer.TerminalStatus == 1 ? (int)SubscriptionEnum.TerminalStatus.Active : (int)SubscriptionEnum.TerminalStatus.InActive,
                        TerminalType = customer.TerminalType
                    };
         
                }

            }
        }
    }
}
