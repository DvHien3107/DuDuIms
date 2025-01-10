using Dapper;
using Enrich.Dto;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class SalesLeadCommentRepository : DapperGenericRepository<SalesLeadComment>, ISalesLeadCommentRepository
    {
        private const string Alias = SqlTables.SalesLeadComment;

        public SalesLeadCommentRepository(IConnectionFactory connectionFactory)
            : base(connectionFactory)
        {
        }

        public async Task<IEnumerable<SalesLeadCommentDto>> GetByCustomerCodeAsync(string CustomerCode)
        {
            var query = SqlScript.SalesLeadComment.GetByCustomerCode;
            var parameters = new DynamicParameters();
            parameters.Add(SqlScript.Parameters.CustomerCode, CustomerCode);
            using (var connection = GetDbConnection())
            {
                return await connection.QueryAsync<SalesLeadCommentDto>(query, parameters);
            }
        }

        /// <summary>
        /// search sales lead
        /// </summary>
        /// <param name="request">SalesLeadSearchRequest</param>
        /// <returns>SalesLeadSearchResponse</returns>
        public async Task<SalesLeadCommentSearchResponse> SearchAsync(SalesLeadCommentSearchRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string> { };

            PopularJoinQuery(request);
            var extendJoinKeys = request.SqlQueryParam.GetJoinKeys();

            //search on column
            var filterContdition = new List<string>();
            var parameters = new DynamicParameters();
            if (request.Condition == null)
            {
                goto ExecuteSearch;
            }

            PopulateConditionCommon(request.Condition, extendJoins, conditions, parameters);
            // filter by column
            PopulateFilterConditionCommon(request.FilterCondition, filterContdition, parameters);

        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);
            if (extendJoinKeys.Count > 0) extendJoins.AddRange(SqlScript.GetJoins(SqlScript.SalesLeadComment.JoinsSearch, extendJoinKeys));

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
            var response = new SalesLeadCommentSearchResponse();
            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);
                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                var exeResponse = await ExecuteQueryCTEAsync<SalesLeadCommentItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, reader =>
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
        private void PopupateCountSummariesAsync(SalesLeadCommentSearchResponse response, SqlMapper.GridReader reader)
        {
        }
        private void PopulateConditionCommon(SalesLeadCommentSearchCondition request, List<string> extendJoins, List<string> conditions, DynamicParameters parameters)
        {
            AddConditionInList(conditions, $"{Alias}.Id", request.Ids);
            AddConditionNotInList(conditions, $"{Alias}.Id", request.ExcludeIds);
        }
        private void PopularJoinQuery(SalesLeadCommentSearchRequest request)
        {
            request.SqlQueryParam.Fields.Add(new SqlMapDto { SqlJoinKeys = "Member", SqlName = "FullName", DtoName = "CreateByName" });
        }
    }
}