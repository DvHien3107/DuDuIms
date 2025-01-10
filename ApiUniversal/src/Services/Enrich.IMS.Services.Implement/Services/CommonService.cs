using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto.Common;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.EnumValue;
using Enrich.IMS.Dto.Lookup;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public class CommonService : BaseService, ICommonService
    {
        private readonly IEnrichContainer _container;
        private readonly EnrichContext _context;
        public CommonService(
            EnrichContext context,
            IEnrichContainer container)
        {
            _context = context;
            _container = container;
        }

        public async Task<object> GetLookupDataAsync(GetLookupDataRequest request)
        {
            return
                await GetLookupDataByDbAsync(request) ??
                await GetLookupDataByDbEnumValuesAsync(request) ??
                await GetLookupDataByEnumValueAsync(request);
        }

        /// <summary>
        /// Get data from DB
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<object> GetLookupDataByDbAsync(GetLookupDataRequest request)
        {
            var finalRequest = new LookupDataRequest();
            ILookupDataService service = null;

            switch (request.Type)
            {
                case LookupDataType.SalesTeam:
                    service = _container.Resolve<IDepartmentService>();
                    break;
                case LookupDataType.SalesMember:
                case LookupDataType.AccountManager:
                    service = _container.Resolve<IMemberService>();
                    break;
                case LookupDataType.SalesLeadInteractionStatus:
                    service = _container.Resolve<ISalesLeadInteractionStatusService>();
                    break;
                case LookupDataType.Partner:
                    service = _container.Resolve<IPartnerService>();
                    break;
                case LookupDataType.TicketStatus:
                    service = _container.Resolve<ITicketStatusService>();
                    break;
                case LookupDataType.TicketType:
                    service = _container.Resolve<ITicketTypeService>();
                    break;
                default:
                    break;
            }

            if (service != null)
            {
                finalRequest.SearchText = request.QueryString["searchText"];
                service.OptimizeConditionRequest(finalRequest, request.Type);
                return await service.GetIdNamesAsync(finalRequest);
            }

            return null;
        }

        /// <summary>
        /// get data from system value
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<object> GetLookupDataByDbEnumValuesAsync(GetLookupDataRequest request)
        {
            var dbRequest = new EnumValueLookupDataRequest();
            dbRequest.SearchText = request.QueryString["searchText"];

            var response = await _container.Resolve<IEnumValueService>().GetLookupAsync(request.Type, dbRequest);

            return !string.IsNullOrWhiteSpace(response.Namespace) ? response.IdNames : null;
        }

        /// <summary>
        /// Get data from enum config in code
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<object> GetLookupDataByEnumValueAsync(GetLookupDataRequest request)
        {
            switch (request.Type)
            {
                case LookupDataType.MerchantStatus:
                    return EnumHelper.ToDictionary<CustomerEnum.MerchantStatusSearch>().Select(c => new { Value = c.Key, Name = c.Value }).AsEnumerable();
                case LookupDataType.ServiceType:
                    return EnumHelper.ToDictionaryDisplay<CustomerEnum.ServiceType>(false).Select(c => new { Value = c.Key, Name = c.Value }).AsEnumerable();
                case LookupDataType.LicenseStatus:
                    return EnumHelper.ToDictionaryDisplay<SubscriptionEnum.LicenseStatus>(false).Select(c => new { Value = c.Key, Name = c.Value }).AsEnumerable();                    
                case LookupDataType.NODaysCreatedSearch:
                    return EnumHelper.ToDictionaryDisplay<CustomerEnum.NODaysCreatedSearch>().Select(c => new { Value = c.Key, Name = c.Value }).AsEnumerable();
                case LookupDataType.RemainingDaySearch:
                    return EnumHelper.ToDictionaryDisplay<CustomerEnum.RemainingDaySearch>().Select(c => new { Value = c.Key, Name = c.Value }).AsEnumerable();
                case LookupDataType.MerchantTabName:
                    return EnumHelper.ToDictionaryDisplay<CustomerEnum.TabName>().Select(c => new { Value = c.Key, Name = c.Value }).AsEnumerable();
                default:
                    return null;
            }
        }
    }
}