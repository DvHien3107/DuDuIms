using Dapper;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Ticket;
using Enrich.IMS.Dto.Ticket.Queries;
using Enrich.IMS.Dto.Ticket.Reponses;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class TicketRepository
    { 

        public async Task<SearchTicketResponse> SearchAsync(SearchTicketRequest request)
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

            PopulateConditionCommon(request.Condition, extendJoins, conditions, parameters);
            PopulateConditionFeedback(request.Condition, extendJoins, conditions, parameters);
          
            // filter by column
            PopulateFilterConditionCommon(request.FilterCondition, filterContdition, parameters);


        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);
            if (extendJoinKeys.Count > 0) extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Ticket.JoinsSearch, extendJoinKeys));

            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = filterContdition,
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.Ticket.QuerySearch
            };
            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);          
          

            //final
            var response = new SearchTicketResponse();

            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);

                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                var exeResponse = await ExecuteQueryCTEAsync<TicketListItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, async reader =>
                {
                    if (request.Condition != null && request.Condition.PopulateCountSummaries)
                    {
                        await PopupateCountSummariesAsync(response, reader);
                    }
                });

                response.Records = exeResponse.Records;
                response.Pagination = exeResponse.Pagination;
            }

            return response;
        }

        #region SummaryCount

     
        private async System.Threading.Tasks.Task PopupateCountSummariesAsync(SearchTicketResponse response, GridReader reader)
        {
            if (response.Summary == null)
                response.Summary = new SearchTicketSummaryDto();
        }

        #endregion

        private void PopulateConditionCommon(TicketSearchCondition request, List<string> extendJoins, List<string> conditions, DynamicParameters parameters)
        {
            AddConditionInList(conditions, $"{AliasTicket}.ID", request.Ids);
            AddConditionNotInList(conditions, $"{AliasTicket}.ID", request.ExcludeIds); 
        }
        private void PopulateConditionFeedback(TicketSearchCondition request, List<string> extendJoins, List<string> conditions, DynamicParameters parameters)
        {
            if (request.Comment.IsNotEmpty())
            {
                extendJoins.Add($"INNER JOIN {SqlTables.TicketFeedback}  on {SqlTables.TicketFeedback}.TicketId = {AliasTicket}.ID");
                conditions.Add($"({SqlTables.TicketFeedback}.Feedback like N'%' +@Comment +'%' ) ");
                parameters.Add("Comment", request.Comment);
            }
        }


     
    }
}
