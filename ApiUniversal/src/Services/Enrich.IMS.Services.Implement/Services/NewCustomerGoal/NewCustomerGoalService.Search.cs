using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.NewCustomerGoal;
using Enrich.IMS.Dto.SalesLead;
using Enrich.Infrastructure.Data.Dapper.Library;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class NewCustomerGoalService
    {
        public async Task<NewCustomerGoalSearchResponse> SearchAsync(NewCustomerGoalSearchRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<NewCustomerGoalListItemDto>(request);
            OptimizeConditionSearch(request);
            var response = await _repository.SearchAsync(request);
            // format data base on require
            PopulateItems(response);
            return response;
        }

        private void OptimizeConditionSearch(NewCustomerGoalSearchRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new NewCustomerGoalSearchCondition();
            
            // OnStringFields
            if (request.Condition.OnStringFields?.Count > 0)
            {
                request.Condition.OnStringFields.RemoveAll(a => string.IsNullOrWhiteSpace(a.Key) || string.IsNullOrWhiteSpace(a.Value));
            }
            if (request.Condition.OnStringFields?.Count > 0)
            {
                // convert dto-name to db-name
                var dbFields = FieldDbHelper.GetFields<SalesLeadDto>();
                request.Condition.OnStringFields = request.Condition.OnStringFields
                    .Select(a => new { FieldDb = dbFields.FirstOrDefault(b => b.Prop.Name.Equals(a.Key)), FieldDto = a })
                    .Where(a => a.FieldDb != null).Select(a => new KeyValueDto<string> { Key = a.FieldDb.Name, Value = a.FieldDto.Value }).ToList();
            }
        }

        private void PopulateItems(NewCustomerGoalSearchResponse response)
        {
            
        }
    }
}
