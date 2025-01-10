using Dapper;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Dto;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.Base.POSApi;
using Enrich.Dto.List;
using Enrich.IMS.Dto.NewCustomerGoal;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class NewCustomerGoalRepository
    {
        public async Task<NewCustomerGoalSearchResponse> SearchAsync(NewCustomerGoalSearchRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string> { };
            var extendJoinKeys = request.SqlQueryParam.GetJoinKeys();
            //search on column
            var filterContdition = new List<string>();
            var parameters = new DynamicParameters();
            if (request.Condition == null)
            {
                goto ExecuteSearch;
            }

            // filter on search zone
            PopulateConditionCommon(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            // filter by column
            PopulateFilterConditionCommon(request.FilterCondition, filterContdition, parameters);

        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.NewCustomerGoal.JoinsSearch, extendJoinKeys));
            }

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
            var response = new NewCustomerGoalSearchResponse();
            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);
                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                var exeResponse = await ExecuteQueryCTEAsync<NewCustomerGoalListItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, reader =>
                {
                    //if (request.Condition != null && request.Condition.PopulateCountSummaries)
                    //{
                    //    PopupateCountSummariesAsync(response, reader);
                    //}
                });
                response.Records = exeResponse.Records;
                response.Pagination = exeResponse.Pagination;
            }
            return response;
        }

        private void PopulateConditionCommon(NewCustomerGoalSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            extendJoinKeys.Add(SqlScript.NewCustomerGoal.JoinKeys.Partner);
            AddConditionInList(conditions, $"{Alias}.Id", request.Ids);
            AddConditionNotInList(conditions, $"{Alias}.Id", request.ExcludeIds);
            conditions.Add($"({Alias}.[SiteId] = {_context.SiteId} OR {_context.SiteId} = 1)");

            if (request.Year > 0)
                conditions.Add($"{Alias}.[Year] = {request.Year}");
            if (request.Month > 0)
                conditions.Add($"{Alias}.[Month] = {request.Month}");
        }
    }
}