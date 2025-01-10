using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.CustomerTransaction;
using Enrich.Infrastructure.Data.Dapper.Library;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class CustomerTransactionService
    {
        public async Task<CustomerTransactionSearchResponse> SearchAsync(CustomerTransactionSearchRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<CustomerTransactionListItemDto>(request);
            OptimizeConditionSearch(request);
            var response = await _repository.SearchAsync(request);
            // format data base on require
            PopulateItems(response);
            return response;
        }

        private void OptimizeConditionSearch(CustomerTransactionSearchRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new CustomerTransactionSearchCondition();
            request.Condition.PopulateCountSummaries = false; // !queryParam.HasPaging() || queryParam.Paging.PageIndex == 1;

            // OnStringFields
            if (request.Condition.OnStringFields?.Count > 0)
            {
                request.Condition.OnStringFields.RemoveAll(a => string.IsNullOrWhiteSpace(a.Key) || string.IsNullOrWhiteSpace(a.Value));
            }
            if (request.Condition.OnStringFields?.Count > 0)
            {
                // convert dto-name to db-name
                var dbFields = FieldDbHelper.GetFields<CustomerTransactionDto>();
                request.Condition.OnStringFields = request.Condition.OnStringFields
                    .Select(a => new { FieldDb = dbFields.FirstOrDefault(b => b.Prop.Name.Equals(a.Key)), FieldDto = a })
                    .Where(a => a.FieldDb != null).Select(a => new KeyValueDto<string> { Key = a.FieldDb.Name, Value = a.FieldDto.Value }).ToList();
            }
        }
        private void PopulateItems(CustomerTransactionSearchResponse response)
        {
            if (response == null || response.Records == null) return;

            //customize data after get from responsitory
            //foreach (var salesLead in response.Records)
            //{

            //}
        }
    }
}