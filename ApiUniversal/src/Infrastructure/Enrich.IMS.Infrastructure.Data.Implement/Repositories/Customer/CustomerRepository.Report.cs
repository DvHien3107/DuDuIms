using Dapper;
using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base.Parameters;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.IMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio.Http;
using static Dapper.SqlMapper;

namespace Enrich.IMS.Infrastructure.Data.Implement.Repositories
{
    public partial class CustomerRepository
    {
        public async Task<CustomerReportResponse> ReportAsync(CustomerReportRequest request)
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

            //populate report response data
            PopulateConditionReport(request, extendJoins, extendJoinKeys, conditions, parameters);

        ExecuteSearch:

            SqlScript.PopulateTemplateFields(request.SqlQueryParam.Fields);

            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = filterContdition,
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QuerySearch(Alias)
            };

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, extendJoinKeys));
            }

            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);

            //final
            var response = new CustomerReportResponse();
            if (request.SqlQueryParam.IsOnlyGetTotalRecords)
            {
                var queryTotalRecords = sqlInfo.PopulateQueryExtend(sqlInfo.QueryTotalRecords);
                var totalRecords = await GetScalarValueAsync<int>(queryTotalRecords, parameters);
                response.Pagination = new PaginationDto { TotalRecords = totalRecords };
            }
            else
            {
                if (!request.Condition.OnlyGetSummaries)
                {
                    var exeResponse = await ExecuteQueryCTEAsync<CustomerReportListItemDto>(sqlInfo, request.SqlQueryParam.Paging, parameters, reader =>
                    {
                        //if (request.Condition != null && request.Condition.PopulateCountSummaries)
                        //{
                        //    PopupateSummariesReportAsync(response, reader);
                        //}
                    });
                    response.Records = exeResponse.Records;
                    response.Pagination = exeResponse.Pagination;
                }

                var tasks = new MultipleTasks();
                tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantLicense, parameters),
                    val => response.Summary.TotalMerchantLicense = (int)val);
                tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantMID, parameters),
                    val => response.Summary.TotalMerchantMID = (int)val);
                tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantLicenseAndMID, parameters),
                    val => response.Summary.TotalMerchantLicenseAndMID = (int)val);
                tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantLive, parameters),
                    val => response.Summary.TotalMerchantLive = (int)val);
                tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantPendingDelivery, parameters),
                    val => response.Summary.TotalMerchantPendingDelivery = (int)val);
                await tasks.WhenAll();
                response.Summary.TotalMerchant = response.Summary.TotalMerchantMID + response.Summary.TotalMerchantLicense + response.Summary.TotalMerchantLicenseAndMID;
            }
            return response;
        }
        public async Task<CustomerReportResponse> ReportSummariesAsync(CustomerReportRequest request)
        {
            var parameters = new DynamicParameters();
            PopulateSummariesReport(request);
            var response = new CustomerReportResponse();

            var tasks = new MultipleTasks();
            tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantLicense, parameters),
                val => response.Summary.TotalMerchantLicense = (int)val);
            tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantMID, parameters),
                val => response.Summary.TotalMerchantMID = (int)val);
            tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantLicenseAndMID, parameters),
                val => response.Summary.TotalMerchantLicenseAndMID = (int)val);
            tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantLive, parameters),
                val => response.Summary.TotalMerchantLive = (int)val);
            tasks.Add(GetScalarValueAsync<int>(request.SummaryQuery.QueryTotalMerchantPendingDelivery, parameters),
                val => response.Summary.TotalMerchantPendingDelivery = (int)val);
            await tasks.WhenAll();
            response.Summary.TotalMerchant = response.Summary.TotalMerchantMID + response.Summary.TotalMerchantLicense + response.Summary.TotalMerchantLicenseAndMID;
            return response;
        }
        public async Task<IEnumerable<Object>> ExcuteSqlAsync(string sqlQuery)
        {
            IEnumerable<Object> response = null;
            using (var connection = GetDbConnection())
            {
                response = await connection.QueryAsync<Object>(sqlQuery);
                return response;
            }
        }
        public async Task<CustomerChartReportResponse> ReportCustomerForChartAsync(CustomerChartReportRequest request)
        {
            string query = string.Empty;
            string strDate = string.Empty;
            var response = new CustomerChartReportResponse();
            if (request.Condition.Type == EnumHelper.DisplayName(CustomerEnum.ReportChartType.Cancel))
                strDate = nameof(Customer.CancelDate);
            else
                strDate = nameof(Customer.CreateAt);

            query = SqlScript.Customer.ReportCustomerForChartBy(request.Condition.Unit, request.Condition.Year, strDate);

            using (var connection = GetDbConnection())
            {
                response.Records = await connection.QueryAsync<CustomerChartReportListItemDto>(query);
                response.TotalCustomer = response.Records.Sum(c => c.NumberCustomer);
                return response;
            }
        }
        public async Task<CustomerChartReportResponse> ReportCustomerForChartProc(CustomerChartReportRequest request)
        {
            string query = string.Empty;
            string strDate = string.Empty;
            var response = new CustomerChartReportResponse();
            if (request.Condition.Type == EnumHelper.DisplayName(CustomerEnum.ReportChartType.Cancel))
                strDate = nameof(Customer.CancelDate);
            else
                strDate = nameof(Customer.CreateAt);

            query = SqlScript.Customer.ReportCustomerForChartByProc(request.Condition.Year);

            using (var connection = GetDbConnection())
            {
                response.Records = await connection.QueryAsync<CustomerChartReportListItemDto>(query);
                response.TotalCustomer = response.Records.Sum(c => c.NumberCustomer);
                return response;
            }
        }
        private void PopulateSummariesReport(CustomerReportRequest request)
        {
            PopulateSummaries_TotalMerchantMID(request);
            PopulateSummaries_TotalMerchantLive(request);
            PopulateSummaries_TotalMerchantLicense(request);
            PopulateSummaries_TotalMerchantLicenseAndMID(request);
            PopulateSummaries_ToTalMerchantPendingDelivery(request);
        }

        private void PopulateConditionReport(CustomerReportRequest request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions, DynamicParameters parameters)
        {
            PopulateMerchantReport(request, extendJoins, extendJoinKeys, conditions);
            PopupateConditionDateReport(request, extendJoins, extendJoinKeys, conditions);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.SourceEnumValue);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.ProcessorEnumValue);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.CustomerTransaction);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.Partner);
        }

        private void PopulateMerchantReport(CustomerReportRequest request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions)
        {
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.StoreService);
            if (!request.Condition.IsActive)
            {
                conditions.Add(SqlScript.Customer.Conditions.MerchantIsCanceled);
            }
            else
            {
                //Get summaries
                PopulateSummariesReport(request);
                conditions.Add(SqlScript.Customer.Conditions.MerchantIsNotCanceled);
            }
            conditions.Add(SqlScript.Customer.Conditions.Merchant);
            conditions.Add(SqlScript.Customer.Conditions.IsExistStoreCode);
            conditions.Add(SqlScript.Customer.Conditions.HaveTransaction);
        }

        private void PopupateSummariesReportAsync(CustomerReportResponse response, GridReader reader)
        {
            //if (response.Summary == null)
            //    response.Summary = new SalesLeadSearchSummary();
        }

        private void PopupateConditionDateReport(CustomerReportRequest request, List<string> extendJoins, List<string> extendJoinKeys, List<string> conditions)
        {
            //AddConditionRangeDate(conditions, $"{SqlTables.CustomerTransaction}.CreateAt", request.Condition.FromDate, request.Condition.ToDate);
        }

        #region Get sql query total

        private void PopulateSummaries_TotalMerchantLicenseAndMID(CustomerReportRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string>();
            var extendJoinKeys = new List<string>();

            //populate data report
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.StoreService);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.CustomerTransaction);
            conditions.Add(SqlScript.Customer.Conditions.MerchantLicenseAndMID);
            conditions.Add(SqlScript.Customer.Conditions.IsExistStoreCode);
            conditions.Add(SqlScript.Customer.Conditions.HaveTransaction);

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, extendJoinKeys));
            }
            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = new List<string>(),
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QueryCountSumariesSearch(Alias)
            };

            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);
            request.SummaryQuery.QueryTotalMerchantLicenseAndMID = sqlInfo.QueryTotalRecords;
        }

        private void PopulateSummaries_TotalMerchantLicense(CustomerReportRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string>();
            var extendJoinKeys = new List<string>();

            //populate data report
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.StoreService);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.CustomerTransaction);
            conditions.Add(SqlScript.Customer.Conditions.MerchantLicense);
            conditions.Add(SqlScript.Customer.Conditions.IsExistStoreCode);
            conditions.Add(SqlScript.Customer.Conditions.HaveTransaction);

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, extendJoinKeys));
            }
            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = new List<string>(),
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QueryCountSumariesSearch(Alias)
            };

            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);
            request.SummaryQuery.QueryTotalMerchantLicense = sqlInfo.QueryTotalRecords;
        }

        private void PopulateSummaries_TotalMerchantMID(CustomerReportRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string>();
            var extendJoinKeys = new List<string>();

            //populate data report
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.StoreService);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.CustomerTransaction);
            conditions.Add(SqlScript.Customer.Conditions.MerchantMID);
            conditions.Add(SqlScript.Customer.Conditions.IsExistStoreCode);
            conditions.Add(SqlScript.Customer.Conditions.HaveTransaction);

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, extendJoinKeys));
            }
            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = new List<string>(),
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QueryCountSumariesSearch(Alias)
            };

            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);
            request.SummaryQuery.QueryTotalMerchantMID = sqlInfo.QueryTotalRecords;
        }

        private void PopulateSummaries_ToTalMerchantPendingDelivery(CustomerReportRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string>();
            var extendJoinKeys = new List<string>();

            //populate data report
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.StoreService);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.CustomerTransaction);
            conditions.Add(SqlScript.Customer.Conditions.MerchantPendingDelivery);
            conditions.Add(SqlScript.Customer.Conditions.IsExistStoreCode);
            conditions.Add(SqlScript.Customer.Conditions.HaveTransaction);

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, extendJoinKeys));
            }
            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = new List<string>(),
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QueryCountSumariesSearch(Alias)
            };

            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);
            request.SummaryQuery.QueryTotalMerchantPendingDelivery = sqlInfo.QueryTotalRecords;
        }

        private void PopulateSummaries_TotalMerchantLive(CustomerReportRequest request)
        {
            var extendJoins = new List<string>();
            var conditions = new List<string>();
            var extendJoinKeys = new List<string>();

            //populate data report
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.StoreService);
            extendJoinKeys.Add(SqlScript.Customer.JoinKeys.CustomerTransaction);
            conditions.Add(SqlScript.Customer.Conditions.MerchantLive);
            conditions.Add(SqlScript.Customer.Conditions.IsExistStoreCode);
            conditions.Add(SqlScript.Customer.Conditions.HaveTransaction);

            if (extendJoinKeys.Count > 0)
            {
                extendJoinKeys = extendJoinKeys.Distinct().ToList();
                extendJoins.AddRange(SqlScript.GetJoins(SqlScript.Customer.JoinsSearch, extendJoinKeys));
            }
            var parserQuery = new SqlParserParameter()
            {
                ExtendJoins = extendJoins,
                Conditions = conditions,
                FilterResultByColumnConditions = new List<string>(),
                QueryParam = request.SqlQueryParam,
                QueryTemplate = SqlScript.QueryCountSumariesSearch(Alias)
            };

            var sqlInfo = ParseSqlQueryInfoByTemplateCTE(parserQuery);
            request.SummaryQuery.QueryTotalMerchantLive = sqlInfo.QueryTotalRecords;
        }
        #endregion
    }
}