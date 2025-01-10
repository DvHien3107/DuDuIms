using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.NewCustomerGoal;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.RestApi.Library.Filters;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.NewCustomerGoal)]
    public class NewCustomerGoalController : EnrichAuthApiController<NewCustomerGoal, NewCustomerGoalDto>
    {
        private INewCustomerGoalService _service => _genericService as INewCustomerGoalService;

        public NewCustomerGoalController(
            INewCustomerGoalService service,
            EnrichContext context,
            IEnrichContainer container
            ) : base(service, context, container)
        {
        }

        [HttpPost("search")]
        public async Task<ActionResult<NewCustomerGoalSearchResponse>> SearchAsync(NewCustomerGoalSearchRequest request)
        {
            var response = await _service.SearchAsync(request);
            return Paging(response);
        }

        [HttpPost]
        public override async Task<ActionResult> CreateAsync([FromBody] NewCustomerGoalDto request)
        {
            var response = await _service.CreateGoalAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPatch("{id:int}")]
        public override async Task<ActionResult> UpdateByPatchAsync(int id, [FromBody][EnrichNotNull] List<Operation<NewCustomerGoalDto>> operations)
        {
            var loadOption = GetLoadOption<NewCustomerGoalDto, NewCustomerGoalUpdateOption>(operations);

            var goal = await _service.GetDetailAsync(id);
            if (goal == null)
            {
                return ExceptionNotFound(ExceptionCodes.Generic_NotFound, $@"New customer goal id {id} not found");
            }
            var changes = GetJsonPatchOperationsAsDto(operations);
            var oldGoal = goal.CloneByJson();

            //merge current with data patch
            JsonPatchApplyTo(goal, operations);

            await _service.UpdateGoalAsync(new NewCustomerGoalUpdateRequest(goal, oldGoal)
            {
                PatchChanges = changes,
                UpdateOption = loadOption
            });

            return StatusOk();
        }
    }
}
