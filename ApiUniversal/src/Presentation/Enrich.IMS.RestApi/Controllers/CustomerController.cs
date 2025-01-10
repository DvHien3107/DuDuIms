using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Customer;
using Enrich.IMS.Dto.Customer.QueryRequests.NewRequest;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NPOI.XSSF.Streaming.Values;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.Customer)]
    [AllowAnonymous]
    public class CustomerController : EnrichAuthApiController<Customer, CustomerDto>
    {
        private readonly IEnrichLog _enrichLog;
        private readonly ICustomerService _service;
        private readonly ICustomerCardService _serviceCustomerCard;

        public CustomerController(
            IEnrichLog enrichLog,
            ICustomerService service,
            EnrichContext context,
            IEnrichContainer container,
            ICustomerCardService serviceCustomerCard) : base(service, context, container)
        {
            _enrichLog = enrichLog;
            _service = service;
            _serviceCustomerCard = serviceCustomerCard;
        }

        public override async Task<ActionResult> GetDetailAsync(int id)
        {
            return Ok(await _service.GetById(id));
        }

        /// <summary>
        /// Get customer data by StoreCode
        /// </summary>
        /// <param name="storeCode">store code</param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-by-store/{storeCode}")]
        public async Task<ActionResult<Customer>> GetDetailByStoreCodeAsync(string storeCode)
        {
            return Ok(await _service.GetByStoreCode(storeCode));
        }

        /// <summary>
        /// Get customer data by customerCode
        /// </summary>
        /// <param name="customerCode">customer code</param>
        /// <returns></returns>
        [HttpGet]
        [Route("get-by-code/{customerCode}")]
        public async Task<ActionResult<Customer>> GetByCustomerCode(string customerCode)
        {
            return Ok(await _service.GetByCustomerCodeAsync(customerCode));
        }

        /// <summary>
        /// Search customer
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("search")]
        public async Task<ActionResult<CustomerSearchResponse>> SearchAsync(CustomerSearchRequest request)
        {
            var response = await _service.SearchAsync(request);
            return Paging(response);
        }


        [HttpPost("searchv2")]
        public async Task<ActionResult<CustomerSearchResponse>> SearchAsyncv2(CustomerSearchRequest request)
        {
            var response = await _service.SearchAsyncv2(request);
            return Paging(response);
        }
        /// <summary>
        /// Get Merchant detail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDetailAsync(long id)
        {
            var prop = Query["prop"] ?? string.Empty;
            var loadTab = Query["load-tab"] ?? string.Empty;

            var loadOption = GetLoadOption<CustomerDetailLoadOption>(prop, CustomerDetailLoadOption.Default);
            var conditionLoadInfomation = true;

            if (loadTab == CustomerEnum.TabName.Profile.ToString())
                loadOption = GetLoadOption<CustomerDetailLoadOption>(prop,
                    CustomerDetailLoadOption.GetProfile(conditionLoadInfomation));
            else if (loadTab == CustomerEnum.TabName.Service.ToString())
                loadOption = GetLoadOption<CustomerDetailLoadOption>(prop,
                    CustomerDetailLoadOption.GetService(conditionLoadInfomation));
            else if (loadTab == CustomerEnum.TabName.Transaction.ToString())
                loadOption = GetLoadOption<CustomerDetailLoadOption>(prop,
                    CustomerDetailLoadOption.GetTransaction(conditionLoadInfomation));
            else if (loadTab == CustomerEnum.TabName.History.ToString())
                loadOption = GetLoadOption<CustomerDetailLoadOption>(prop,
                    CustomerDetailLoadOption.GetHistory(conditionLoadInfomation));
            else if (loadTab == CustomerEnum.TabName.Other.ToString())
                loadOption = GetLoadOption<CustomerDetailLoadOption>(prop,
                    CustomerDetailLoadOption.GetOther(conditionLoadInfomation));
            else if (loadTab == CustomerEnum.TabName.Support.ToString())
                loadOption = GetLoadOption<CustomerDetailLoadOption>(prop,
                    CustomerDetailLoadOption.GetSupport(conditionLoadInfomation));

            var person = await _service.GetDetailAsync(id, loadOption);
            if (person == null) return ExceptionNotFound(ExceptionCodes.Customer_NotFound, $@"Customer id {id} not found");

            return Ok(person);
        }

        [HttpGet("get-recurring-card/{customerCode}")]
        public async Task<ActionResult> GetRecurringCardAsync([FromRoute][Required] string customerCode)
        {
            if (string.IsNullOrEmpty(customerCode)) return BadRequest();
            var result = await _serviceCustomerCard.GetRecurringCardAsync(customerCode);
            return Ok(result);
        }

        [HttpPost("report/{reportType}")]
        public async Task<ActionResult<CustomerReportResponse>> ReportAsync(CustomerReportRequest request, [FromRoute] string reportType)
        {
            if (reportType == EnumHelper.DisplayName(CustomerEnum.ReportType.Canceled))
                request.Condition.IsActive = false;
            var response = await _service.ReportAsync(request);
            return Paging(response);
        }

        [HttpPost("report-for-chart/{reportType}")]
        public async Task<ActionResult<ObjectResponse>> ReportNewCustomerForChartAsync(CustomerChartReportRequest request, [FromRoute] string reportType)
        {
            request.Condition.Type = reportType;
            var response = await _service.ReportCustomerForChartProc(request);
            return Paging(response);
        }
        [HttpPost("report-for-chart/ReportCustomerByMonth")]
        public async Task<ActionResult> ReportCustomerByMonth(ReportCustomerByMonthRqt request)
        {
            try
            {
                var response = await _service.ReportCustomerByMonth(request);
                return Ok(new { status = 200, response });
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}