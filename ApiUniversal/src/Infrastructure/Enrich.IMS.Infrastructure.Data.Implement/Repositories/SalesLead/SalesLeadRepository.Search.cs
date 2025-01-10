using Dapper;
using Enrich.Common;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SalesLead;
using Enrich.Infrastructure.Data.Dapper;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class SalesLeadRepository
    {
        /// <summary>
        /// search sales lead
        /// </summary>
        /// <param name="request">SalesLeadSearchRequest</param>
        /// <returns>SalesLeadSearchResponse</returns>
        public async Task<SalesLeadSearchResponse> SearchAsync(SalesLeadSearchRequest request)
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
            PopulateConditionCommon(request.Condition, extendJoins, conditions, parameters);

            // filter by serach text
            PopulateConditionFullTextSearch(request.Condition, extendJoins, conditions, parameters);

            // filter by column
            PopulateFilterConditionCommon(request.FilterCondition, filterContdition, parameters);

        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);
            //if (extendJoinKeys.Count > 0) extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Ticket.JoinsSearch, extendJoinKeys));

            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = filterContdition,
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.SalesLead.QuerySearch
            };
            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);

            //final
            var response = new SalesLeadSearchResponse();
            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);
                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                var exeResponse = await ExecuteQueryCTEAsync<SalesLeadListItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, reader =>
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


        /// <summary>
        /// Search sales lead. Use for dropdownlist Need to improve to full text search
        /// </summary>
        /// <param name="request">search text</param>
        /// <returns>SalesLeadSearchResponse</returns>
        public async Task<IEnumerable<SalesLeadQuickSearchItemDto>> QuickSearchAsync(string text)
        {
            var param = new DynamicParameters();
            string statementTOP = $"TOP({ Constants.DEFAULT_QUICK_SEARCH_ITEMS})";
            var searchFields = GridSqlHelper.GetFields<SalesLeadQuickSearchItemDto>();
            var selectFields = GenerateSqlSelect(searchFields, includeKeywordSelect: false, includeKeyworkDistinct: false);

            var query = $"SELECT {statementTOP} {selectFields} " +
                        $"FROM {AliasSalesLead} " +
                        $"WHERE {SqlColumns.SalesLead.ContactName} like @SearhText OR " +
                        $" {SqlColumns.SalesLead.SalonName} like @SearhText";
            param.Add("SearhText", $"%{text}%");
            return await GetEnumerableAsync<SalesLeadQuickSearchItemDto>(query, param);
        }

        private void PopupateCountSummariesAsync(SalesLeadSearchResponse response, GridReader reader)
        {
            //if (response.Summary == null)
            //    response.Summary = new SalesLeadSearchSummary();
        }
        private void PopulateConditionCommon(SalesLeadSearchCondition request, List<string> extendJoins, List<string> conditions, DynamicParameters parameters)
        {
            AddConditionInList(conditions, $"{AliasSalesLead}.Id", request.Ids);
            AddConditionNotInList(conditions, $"{AliasSalesLead}.Id", request.ExcludeIds);

            if (request.TeamNumbers != null) AddConditionInList(conditions, $"{AliasSalesLead}.TeamNumber", request.TeamNumbers);
            if (request.SaleNumbers != null) AddConditionInList(conditions, $"{AliasSalesLead}.MemberNumber", request.SaleNumbers);
            if (request.Status != null) AddConditionInList(conditions, $"{AliasSalesLead}.{SqlColumns.SalesLead.InteractionStatusId}", request.Status);
            if (request.Types != null) AddConditionInList(conditions, $"{AliasSalesLead}.{SqlColumns.SalesLead.Status}", request.Types);
        }

        private void PopulateConditionFullTextSearch(SalesLeadSearchCondition request, List<string> extendJoins, List<string> conditions, DynamicParameters parameters)
        {
            var fieldsName = SqlColumns.SalesLead.ContactName;
            //request.SearchText = StringHelper.RemoveSpecialCharacter(request.SearchText);
            AddConditionWithFullTextSearch(conditions, fieldsName, request.SearchText);
        }
    }
}