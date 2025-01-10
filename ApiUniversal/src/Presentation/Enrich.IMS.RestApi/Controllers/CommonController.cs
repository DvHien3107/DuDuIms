using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Common;
using Enrich.IMS.Dto.Department;
using Enrich.IMS.Dto.Enums;
using Enrich.IMS.Dto.Member;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.Common)]
    public class CommonController : EnrichAuthApiController
    {
        #region field

        private readonly IEnrichContainer _container;
        private readonly ICommonService _service;
        private readonly IDepartmentService _serviceDepartment;

        #endregion

        #region contructor

        public CommonController(
            ICommonService service, 
            EnrichContext context, 
            IEnrichContainer container, 
            IDepartmentService serviceDepartment
            ) : base(context, container)
        {
            _container = container;
            _service = service;
            _serviceDepartment = serviceDepartment;
        }

        #endregion

        #region Grid Config

        [HttpGet("grid-configs/{type}")]
        public async Task<ActionResult<GridConfigDto>> GetGridConfigsAsync(string type)
        {
            if (!type.TryGetEnum<GridType>(out var gridType))
            {
                return EmptyObject();
            }

            return Ok(await EnrichContainer.Resolve<IWebGridConfigService>().GetUserGridConfigAsync(gridType, EnrichContext.UserId));
        }

        [HttpPost("grid-configs/{type}")]
        public async Task<ActionResult<bool>> SaveGridConfigsAsync(string type, SaveWebGridConfigRequest request)
        {
            if (!EnumHelper.TryGetEnum<GridType>(type, out var gridType) || request.Config == null || request.OwnerType == null)
            {
                return BadRequest();
            }

            if (request.OwnerType == GridOwnerType.User && request.FilterId == null)
            {
                return BadRequest();
            }

            request.Type = gridType;
            request.UserId = EnrichContext.UserId;

            var success = await EnrichContainer.Resolve<IWebGridConfigService>().SaveConfigAsync(request);

            return Ok(success);
        }

        #endregion

        #region LookupData

        [HttpGet("lookup-data/{type}")]
        public async Task<ActionResult<Dictionary<string, object>>> GetLookupDataAsync(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
            {
                return EmptyObject();
            }

            var lookups = new Dictionary<string, object>();
            var request = new GetLookupDataRequest(LookupDataType.None, Query);

            foreach (var typeName in type.SplitEx(distinct: true))
            {
                if (lookups.ContainsKey(typeName) || !typeName.TryGetEnum<LookupDataType>(out var etype) || etype == LookupDataType.None)
                {
                    continue;
                }

                request.Type = etype;

                var data = await _service.GetLookupDataAsync(request);
                if (data != null)
                {
                    lookups.Add(typeName, data);
                }
            }

            return Ok(lookups);
        }

        #endregion

        #region Team sales, member sales

        [HttpGet("sales-team")]
        public async Task<ActionResult<IEnumerable<DepartmentOptionItemDto>>> GetSaleTeamAsync()
        {
            var response = await _serviceDepartment.GetSalesTeamAsync();
            return Ok(response);
        }

        [HttpGet("sales-member")]
        public async Task<ActionResult<IEnumerable<MemberOptionItemDto>>> GetSalememberAsync()
        {
            var service = _container.Resolve<IMemberService>();
            var response = await service.GetSalesMemberAsync();
            return Ok(response);
        }

        #endregion
    }
}