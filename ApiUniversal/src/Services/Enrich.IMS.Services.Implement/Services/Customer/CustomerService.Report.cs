using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Dto.Base;
using Enrich.Dto.List;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.Customer.QueryRequests.NewRequest;
using Enrich.IMS.Dto.OrderSubscription;
using Enrich.IMS.Entities;
using Enrich.Infrastructure.Data.Dapper.Library;
using NPOI.SS.Formula.Functions;
using NPOI.Util.ArrayExtensions;
using NPOI.XSSF.Streaming.Values;
using SlackAPI.WebSocketMessages;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public async Task<CustomerReportResponse> ReportAsync(CustomerReportRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<CustomerReportListItemDto>(request);
            OptimizeConditionReport(request);
            var response = await _repository.ReportAsync(request);
            // format data base on require
            PopulateItemsReport(response);
            return response;
        }

        public async Task<CustomerChartReportResponse> ReportCustomerForChartAsync(CustomerChartReportRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<CustomerReportListItemDto>(request);
            OptimizeConditionReportCustomerForChart(request);
            var response = await _repository.ReportCustomerForChartAsync(request);
            // format data base on require
            await PopulateColumnForChart(request, response);
            return response;
        }

        public async Task<ObjectResponse> ReportCustomerForChartProc(CustomerChartReportRequest request)
        {
            var response = new ObjectResponse();
            String sql = String.Format("exec P_ReportCustomerForChart '{0}'",
                new object[] {
                    request.Condition.Year,
                });
            var ressql = await _repository.ExcuteSqlAsync(sql);
            response.Records = ressql;
            //response.TotalCustomer = response.Records.Sum(c => c.NumberCustomer);
            return response;
        }

        public async Task<ObjectResponse> ReportCustomerByMonth(ReportCustomerByMonthRqt request)
        {
            var response = new ObjectResponse();
            String sql = String.Format("exec P_ReportCustomerForChartByMonth '{0}','{1}','{2}'",
                new object[] {
                    request.Year,
                    request.Month,
                    request.Type,
                });
            var ressql = await _repository.ExcuteSqlAsync(sql);
            response.Records = ressql;
            //response.TotalCustomer = response.Records.Sum(c => c.NumberCustomer);
            return response;
        }

        private void OptimizeConditionReportCustomerForChart(CustomerChartReportRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new CustomerChartReportCondition();
            request.Condition.Type = request.Condition.Type.ToLower();
        }

        private void OptimizeConditionReport(CustomerReportRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new CustomerReportCondition();
            //request.Condition.PopulateCountSummaries = false; // !queryParam.HasPaging() || queryParam.Paging.PageIndex == 1;

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

        private void PopulateItemsReport(CustomerReportResponse response)
        {
            if (response == null || response.Records == null) return;
            PopulateAccountStatusReport(response);
        }

        private void PopulateAccountStatusReport(CustomerReportResponse response)
        {
            Parallel.ForEach(response.Records, customer =>
            {
                if (_repository.IsPendingDelivery(customer.StoreCode))
                    customer.AccountStatus = EnumHelper.DisplayName(CustomerEnum.AccountStatus.PendingDelivery);
                else if (customer.ServiceType == EnumHelper.DisplayDescription(CustomerEnum.ServiceType.Merchant_License))
                    customer.AccountStatus = EnumHelper.DisplayName(CustomerEnum.AccountStatus.Live);
                else if (customer.ServiceType == EnumHelper.DisplayDescription(CustomerEnum.ServiceType.Merchant_MID))
                    customer.AccountStatus = EnumHelper.DisplayName(CustomerEnum.AccountStatus.Processing);
                else if (customer.ServiceType == EnumHelper.DisplayDescription(CustomerEnum.ServiceType.Merchant_License_MID))
                    customer.AccountStatus = EnumHelper.DisplayName(CustomerEnum.AccountStatus.LiveNProcessing);
                else customer.AccountStatus = "N/A";
            });
        }

        private async Task PopulateColumnForChart(CustomerChartReportRequest request, CustomerChartReportResponse response)
        {
            var missingColumn = response.Records.ToList();
            var columnNames = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames.Where(c => !string.IsNullOrEmpty(c));
            if (request.Condition.Unit.ToLower() == TimeUnit.Week.ToString().ToLower())
            {
                var maxWeek = CultureInfo.CurrentCulture.Calendar.
                    GetWeekOfYear(new DateTime(2023, 12, 31), CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                columnNames = Enumerable.Range(0, maxWeek).Select(c => "Week " + (c + 1)).AsEnumerable();
            }

            int index = 0;
            foreach (var name in columnNames)
            {
                if (!response.Records.Any(c => c.ColumnNumber == index + 1))
                {
                    missingColumn.Add(new CustomerChartReportListItemDto
                    {
                        ColumnName = name,
                        ColumnNumber = index + 1,
                        NumberCustomer = 0
                    });
                }
                index++;
            }

            if (request.Condition.Unit.ToLower() == TimeUnit.Month.ToString().ToLower() &&
                request.Condition.Type == EnumHelper.DisplayName(CustomerEnum.ReportChartType.NewCustomer))
            {
                var goalScores = await _newCustomerGoalRepository.GetByYear(request.Condition.Year);
                missingColumn.ForEach(c =>
                {
                    var goal = goalScores.FirstOrDefault(x => x.Month == c.ColumnNumber);
                    if (goal != null)
                    {
                        c.Goal = goal.Goal;
                    }
                });
            }

            response.Records = missingColumn.OrderBy(c => c.ColumnNumber);
        }
    }
}
