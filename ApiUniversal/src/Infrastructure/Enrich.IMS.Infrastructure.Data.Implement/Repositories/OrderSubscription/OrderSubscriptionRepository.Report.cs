using Dapper;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Infrastructure.Data;
using Enrich.Infrastructure.Data.Dapper;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class OrderSubscriptionRepository
    {

        private readonly string Alias = SqlTables.OrderSubscription;
        public async Task<OrderSubscriptionSearchResponse> SearchAsync(OrderSubscriptionSearchRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string> { };
            var extendJoinKeys = new List<string> { };

            //search on column
            var filterContdition = new List<string>();
            var parameters = new DynamicParameters();
            if (request.Condition == null)
            {
                goto ExecuteSearch;
            }

            // filter on search zone
            PopulateConditionCommon(request.Condition, extendJoins, extendJoinKeys, conditions, parameters);

            // filter by serach text
            PopulateConditionFullTextSearch(request.Condition, extendJoins, conditions, parameters);

            // filter by column
            PopulateFilterConditionCommon(request.FilterCondition, filterContdition, parameters);

        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);
            if (extendJoinKeys.Count > 0)
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Order.JoinsKeys, extendJoinKeys));

            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = filterContdition,
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QuerySearch(SqlTables.Orders)
            };
            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);

            //final
            var response = new OrderSubscriptionSearchResponse();
            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);
                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                var exeResponse = await ExecuteQueryCTEAsync<OrderSubscriptionListItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, reader =>
                {
                    //if (request.Condition != null && request.Condition.PopulateCountSummaries)
                    //{
                    //    PopupateCountSummariesAsync(response, sqlInfo, reader);
                    //}
                });

                await PopularSummaryAsync(response, sqlInfo);
                response.Records = exeResponse.Records;
                response.Pagination = exeResponse.Pagination;
            }
            return response;
        }

        public async Task<OrderSubscriptionSearchResponse> SearchAsyncByProc(OrderSubscriptionSearchRequest request)
        {
            var response = new OrderSubscriptionSearchResponse();
            string SubscriptionTypes = "";
            string Partners = "";
            string PaymentMethod = "";
            string ProductId = "";
            string Hardware = "";
            string SubscriptionCodes = "";
            if (request.Condition != null)
            {
                try
                {
                    if (request.Condition.SubscriptionTypes != null)
                        SubscriptionTypes = String.Join(",", request.Condition.SubscriptionTypes.ToArray());
                }
                catch { }
                try
                {
                    if (request.Condition.Partners != null)
                        Partners = String.Join(",", request.Condition.Partners.ToArray());
                }
                catch { }
                try
                {
                    if (request.Condition.PaymentMethod != null)
                        PaymentMethod = String.Join(",", request.Condition.PaymentMethod.ToArray());
                }
                catch { }
                try
                {
                    if (request.Condition.ProductId != null)
                        ProductId = String.Join(",", request.Condition.ProductId.ToArray());
                }
                catch { }
                try
                {
                    if (request.Condition.SubscriptionCodes != null)
                        SubscriptionCodes = String.Join(",", request.Condition.SubscriptionCodes.ToArray());
                }
                catch { } 
                try
                {
                    if (request.Condition.Hardware != null)
                        Hardware = String.Join(",", request.Condition.Hardware.ToArray());
                }
                catch { }
            }
            

            String sql = String.Format("exec P_GetListTransaction '{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}',{10},{11},'{12}'",
                new object[] {
                    request.Condition.OrderStatus,
                    Hardware,
                    SubscriptionTypes,
                    Partners,
                    PaymentMethod,
                    ProductId,
                    SubscriptionCodes,
                    request.Condition.FromDate,
                    request.Condition.ToDate,
                    request.Condition.SearchText,
                    request.PageSize,
                    request.PageIndex,
                    request.OrderBy
                });
            var exeResponse =  await GetEnumerableAsync<OrderSubscriptionListItemDto>(sql);
            var listREssult = exeResponse.ToList();
            response.Records = exeResponse;
            response.Pagination = new PaginationDto { TotalRecords = listREssult.FirstOrDefault()?.MaxRows??0 };
            response.Summary = new OrderSubscriptionSearchSummary { TotalAmount = listREssult.FirstOrDefault()?.TotalAmount ?? 0  };
            response.Income = listREssult.FirstOrDefault()?.Income??0;

            return response;
        }


        private void PopulateConditionCommon(OrderSubscriptionSearchCondition request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            extendJoinKeys.Add(SqlScript.Order.JoinKeys.SubscriptionProduct);
            extendJoinKeys.Add(SqlScript.Order.JoinKeys.Customer);
            extendJoinKeys.Add(SqlScript.Order.JoinKeys.Transaction);
            //conditions.Add(SqlScript.Order.Conditions.HasPaid);
            AddConditionInList(conditions, $"{Alias}.Id", request.Ids);
            AddConditionNotInList(conditions, $"{Alias}.Id", request.ExcludeIds);

            if (request.SubscriptionCodes != null && request.SubscriptionCodes.Any()) AddConditionInList(conditions, $"{Alias}.{SqlColumns.OrderSubcription.ProductCode}", request.SubscriptionCodes);
            if (request.SubscriptionTypes != null && request.SubscriptionTypes.Any()) AddConditionInList(conditions, $"{Alias}.SubscriptionType", request.SubscriptionTypes);
            if (request.OrderCodes != null && request.OrderCodes.Any() ) AddConditionInList(conditions, $"{SqlTables.Orders}.OrdersCode", request.OrderCodes);
            if (request.OrderStatus != null && request.OrderStatus.Any()) AddConditionInList(conditions, $"{SqlTables.Orders}.Status", request.OrderStatus);
            if (request.Customers != null && request.Customers.Any()) AddConditionInList(conditions, $"{SqlTables.Customer}.StoreCode", request.Customers);
            if (request.Partners != null && request.Partners.Any())
            {
                var extendCondition = new List<string>();
                AddConditionInList(extendCondition, $"{SqlTables.Customer}.PartnerCode", request.Partners);
                if(request.Partners.Count(c=>string.IsNullOrEmpty(c)) > 0) extendCondition.Add($"{SqlTables.Customer}.PartnerCode IS NULL");
                conditions.Add($" ({string.Join(" OR ", extendCondition)}) ");
            }
            if (request.PaymentMethod != null) AddConditionInList(conditions, $"{SqlTables.CustomerTransaction}.PaymentMethod", request.PaymentMethod);
            if (request.FromDate.HasValue && request.ToDate.HasValue) AddConditionRangeDateTime(conditions, $"{SqlTables.CustomerTransaction}.CreateAt", request.FromDate, request.ToDate);
        }

        private async Task PopularSummaryAsync(OrderSubscriptionSearchResponse response, ParsedSqlQuery sqlQuery)
        {
            if (response.Summary == null)
                response.Summary = new OrderSubscriptionSearchSummary();
            var sqlSummary = $"{sqlQuery.Query} SELECT CASE WHEN SUM([SubscriptionAmount]) IS NULL THEN 0 ELSE SUM([SubscriptionAmount]) END FROM #QueryFinalTmp";
            using (var connection = GetDbConnection())
            {
                response.Summary.TotalAmount = await connection.QueryFirstOrDefaultAsync<decimal>(sqlSummary);
            }
        }

        private void PopulateConditionFullTextSearch(OrderSubscriptionSearchCondition request, List<string> extendJoins, List<string> conditions, DynamicParameters parameters)
        {
            var fieldNames = new List<string>
            {
                $"{SqlTables.OrderSubscription}.ProductName",
                $"{SqlTables.OrderSubscription}.{SqlColumns.OrderSubcription.ProductCode}",
                $"{SqlTables.Customer}.StoreCode",
                $"{SqlTables.Customer}.BusinessName",
                $"{SqlTables.Orders}.OrdersCode",
                $"{SqlTables.Orders}.CreatedBy"
            };
            //request.SearchText = StringHelper.RemoveSpecialCharacter(request.SearchText);
            AddConditionWithSearchText(conditions, fieldNames, request.SearchText);
        }

    }
}
