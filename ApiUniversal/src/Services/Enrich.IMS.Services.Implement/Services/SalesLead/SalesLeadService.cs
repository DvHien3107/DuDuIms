using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Implement.Validation;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Infrastructure.Data.Dapper.Library;
using Enrich.Services.Implement;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class SalesLeadService : EnrichBaseService<SalesLead, SalesLeadDto>, ISalesLeadService
    {
        private readonly EnrichContext _context;
        private ISalesLeadMapper _mapper => _mapperGeneric as ISalesLeadMapper;
        private readonly ISalesLeadBuilder _builder;
        private readonly ICustomerBuilder _builderCustomer;
        private readonly ISalesLeadRepository _repository;
        private readonly ICustomerRepository _repositoryCustomer;
        private readonly IEnrichContainer _container;
        private readonly IEnrichLog _log;
        public SalesLeadService(
            EnrichContext context,
            ISalesLeadMapper mapper,
            ISalesLeadBuilder builder,
            ICustomerBuilder builderCustomer,
            ISalesLeadRepository repository,
            ICustomerRepository repositoryCustomer,
            IEnrichContainer container,
            IEnrichLog log) : base(repository, mapper)
        {
            _context = context;
            _builder = builder;
            _builderCustomer = builderCustomer;
            _repository = repository;
            _repositoryCustomer = repositoryCustomer;
            _container = container;
            _log = log;
        }

        #region Get

        /// <summary>
        /// Get salesLead by customer code
        /// </summary>
        /// <param name="customercode">customercode</param>
        /// <returns>SalesLead</returns>
        public async Task<SalesLead> GetByCustomerCode(string customercode)
        {
            return await _repository.GetByCustomerCodeAsync(customercode);
        }

        /// <summary>
        /// Get sales lead detail  
        /// </summary>
        /// <param name="salesLeadId"> sales lead Id </param>
        /// <param name="loadOption"> SalesLeadLoadOption </param>
        /// <returns></returns>
        public async Task<SalesLeadDto> GetSalesLeadDetailAsync(string salesLeadId, SalesLeadDetailLoadOption loadOption = null)
        {
            var entity = await _repository.FindByIdAsync(salesLeadId);
            if (entity == null)
            {
                return null;
            }

            var dto = _mapper.Map<SalesLeadDto>(entity);

            var tasks = new MultipleTasks<SalesLeadDto>(dto);
            if (loadOption == null || loadOption.Customer)
                tasks.Add(nameof(SalesLeadDto.Customer), GetSalesLeadCustomerAsync(salesLeadId));

            await tasks.WhenAll();
            return dto;
        }


        #endregion

        #region Search

        /// <summary>
        /// Search sales lead. Use for data grid
        /// </summary>
        /// <param name="request">SalesLeadSearchRequest</param>
        /// <returns>SalesLeadSearchResponse</returns>
        public async Task<SalesLeadSearchResponse> SearchAsync(SalesLeadSearchRequest request)
        {
            request.SqlQueryParam = _mapper.CreateSearchSqlQueryParameter<SalesLeadListItemDto>(request);
            OptimizeConditionSearch(request);
            var response = await _repository.SearchAsync(request);
            // format data base on require
            PopulateItems(response);
            return response;
        }

        /// <summary>
        /// quick search sales lead
        /// </summary>
        /// <param name="text">search texy</param>
        /// <returns>IEnumerable<SalesLeadQuickSearchItemDto></returns>
        public async Task<IEnumerable<SalesLeadQuickSearchItemDto>> QuickSearchAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Enumerable.Empty<SalesLeadQuickSearchItemDto>();
            }

            return await _repository.QuickSearchAsync(text);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete multiple salesLead
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<int> DeleteSalesLeadsAsync(string[] ids)
        {
            if (ids.IsNullOrEmpty())
            {
                return -1;
            }
            return await _repository.DeleteSalesLeadsAsync(ids);
        }

        #endregion

        #region function

        public void OptimizeConditionRequest(LookupDataRequest request, LookupDataType type)
        {
        }

        private void OptimizeConditionSearch(SalesLeadSearchRequest request)
        {
            // optimize Condition
            if (request.Condition == null) request.Condition = new SalesLeadSearchCondition();
            request.Condition.PopulateCountSummaries = false; // !queryParam.HasPaging() || queryParam.Paging.PageIndex == 1;

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
        private void PopulateItems(SalesLeadSearchResponse response)
        {
            if (response == null || response.Records == null) return;

            //customize data after get from responsitory
            //foreach (var salesLead in response.Records)
            //{

            //}
        }
        private async Task<CustomerDto> GetSalesLeadCustomerAsync(string salesLeadId)
        {
            var customer = await _repository.GetSalesLeadCustomerAsync(salesLeadId);
            return _mapper.Map<CustomerDto>(customer);
        }

        private async Task PopulateImportData(SalesLeadDto salesLead, IRow rowData)
        {
            salesLead.CustomerName = salesLead.ContactName = salesLead.SalonName = ConvertHelper.GetString(rowData.GetCell(0));
            salesLead.SalonEmail = ConvertHelper.GetString(rowData.GetCell(1));
            salesLead.SalonPhone = ConvertHelper.GetString(rowData.GetCell(2));
            salesLead.SalonAddress = ConvertHelper.GetString(rowData.GetCell(3));
            salesLead.City = ConvertHelper.GetString(rowData.GetCell(4));
            salesLead.State = ConvertHelper.GetString(rowData.GetCell(5));
            salesLead.Country = ConvertHelper.GetString(rowData.GetCell(6));
            salesLead.Zipcode = ConvertHelper.GetString(rowData.GetCell(7));
            salesLead.PotentialRateScore = ConvertHelper.GetInt(rowData.GetCell(8));
        }

        #endregion
    }
}