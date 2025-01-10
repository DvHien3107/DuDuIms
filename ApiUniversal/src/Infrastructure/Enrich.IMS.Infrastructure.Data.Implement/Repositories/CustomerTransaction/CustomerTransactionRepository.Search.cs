using Dapper;
using Enrich.Dto;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.CustomerTransaction;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class CustomerTransactionRepository
    {
        private readonly string Alias = SqlTables.CustomerTransaction;
        public async Task<CustomerTransactionSearchResponse> SearchAsync(CustomerTransactionSearchRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string> { };

            //PopularJoinQuery(request);
            var extendJoinKeys = request.SqlQueryParam.GetJoinKeys();

            //search on column
            var filterContdition = new List<string>();
            var parameters = new DynamicParameters();
            if (request.Condition == null)
            {
                goto ExecuteSearch;
            }

            // filter on search zone
            PopulateConditionCommon(request.Condition, extendJoins, conditions, parameters);

            // filter by serach text
            //PopulateConditionFullTextSearch(request.Condition, extendJoins, conditions, parameters);

            // filter by column
            PopulateFilterConditionCommon(request.FilterCondition, filterContdition, parameters);

        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);
            //if (extendJoinKeys.Count > 0) 
            extendJoins.AddRange(SqlScript.GetJoins(SqlScript.CustomerTransaction.JoinsSearch, extendJoinKeys));

            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = filterContdition,
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QuerySearch(Alias)
            };
            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);

            //final
            var response = new CustomerTransactionSearchResponse();
            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);
                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                var exeResponse = await ExecuteQueryCTEAsync<CustomerTransactionListItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, reader =>
                {
                    if (request.Condition != null && request.Condition.PopulateCountSummaries)
                    {
                        PopupateCountSummariesAsync(response, reader);
                    }
                });
                response.Records = exeResponse.Records;
                response.Pagination = exeResponse.Pagination;
            }
            return response;
        }

        private void PopulateConditionCommon(CustomerTransactionSearchCondition request, List<string> extendJoins, List<string> conditions, DynamicParameters parameters)
        {
            AddConditionInList(conditions, $"{Alias}.Id", request.Ids);
            AddConditionNotInList(conditions, $"{Alias}.Id", request.ExcludeIds);

            //if (request.TeamNumbers != null) AddConditionInList(conditions, $"{Alias}.TeamNumber", request.TeamNumbers);
            //if (request.SaleNumbers != null) AddConditionInList(conditions, $"{Alias}.MemberNumber", request.SaleNumbers);
            //if (request.Status != null) AddConditionInList(conditions, $"{Alias}.{SqlColumns.SalesLead.InteractionStatusId}", request.Status);
            //if (request.Types != null) AddConditionInList(conditions, $"{Alias}.{SqlColumns.SalesLead.Status}", request.Types);
        }

        private void PopupateCountSummariesAsync(CustomerTransactionSearchResponse response, GridReader reader)
        {
            //if (response.Summary == null)
            //    response.Summary = new SalesLeadSearchSummary();
        }

        //private void PopularJoinQuery(CustomerTransactionSearchRequest request)
        //{
        //    request.SqlQueryParam.Fields.Add(new SqlMapDto { SqlJoinKeys = "TransactionReport" });
        //}
    }
}
