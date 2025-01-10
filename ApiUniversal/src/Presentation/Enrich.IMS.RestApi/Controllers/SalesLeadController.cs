using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.Base.Exceptions;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Common;
using Enrich.IMS.Dto.SalesLead;
using Enrich.IMS.Dto.SalesLeadComment;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.RestApi.Library.Filters;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Services.Implement.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.SalesLead)]
    public class SalesLeadController : EnrichAuthApiController<SalesLead, SalesLeadDto>
    {
        private ISalesLeadService _service => _genericService as ISalesLeadService;
        private readonly ISalesLeadCommentService _salesLeadCommentService;

        public SalesLeadController(
            ISalesLeadService service,
            EnrichContext context,
            IEnrichContainer container,
            ISalesLeadCommentService salesLeadCommentService
            ) : base(service, context, container)
        {
            _salesLeadCommentService = salesLeadCommentService;
        }

        #region Load

        [HttpGet("{id}")]
        public async Task<ActionResult> GetSalesLeadDetailAsync(string id)
        {
            var prop = Query["prop"] ?? string.Empty;
            var loadTab = Query["load-tab"] ?? string.Empty;

            var loadOption = GetLoadOption<SalesLeadDetailLoadOption>(prop, SalesLeadDetailLoadOption.Default);

            if (loadTab == "*")
            {
                loadOption = null; // full-details
            }

            var person = await _service.GetSalesLeadDetailAsync(id, loadOption);
            if (person == null)
            {
                return ExceptionNotFound(ExceptionCodes.SaleLead_NotFound, $@"SaleLead id {id} not found");
            }

            return Ok(person);
        }

        #endregion

        #region Search

        [HttpPost("search")]
        public async Task<ActionResult<SalesLeadSearchResponse>> SearchAsync(SalesLeadSearchRequest request)
        {
            var response = await _service.SearchAsync(request);
            return Paging(response);
        }

        [HttpGet("quicksearch")]
        public async Task<ActionResult<IEnumerable<SalesLeadQuickSearchItemDto>>> QuickSearchAsync()
        {
            var searchText = Query["text"];
            var response = await _service.QuickSearchAsync(searchText);
            return Ok(response);
        }

        #endregion

        #region Edit

        [HttpPost]
        public override async Task<ActionResult> CreateAsync([FromBody] SalesLeadDto request)
        {
            var response = await _service.CreateSalesLeadAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> UpdateSalesLeadByPatchAsync(string id, [FromBody][EnrichNotNull] List<Operation<SalesLeadDto>> operations)
        {
            var loadOption = GetLoadOption<SalesLeadDto, SalesLeadUpdateOption>(operations);

            //alway set true be customer beyond saleslead
            loadOption.Customer = true;
            var fullSalesLead = await _service.GetSalesLeadDetailAsync(id, loadOption);
            if (fullSalesLead == null)
            {
                return ExceptionNotFound(ExceptionCodes.SaleLead_NotFound, $@"SaleLead id {id} not found");
            }
            var changes = GetJsonPatchOperationsAsDto(operations);
            var oldSalesLead = fullSalesLead.CloneByJson();

            //merge current with data patch
            JsonPatchApplyTo(fullSalesLead, operations);

            await _service.UpdateSalesLeadAsync(new SalesLeadUpdateRequest(fullSalesLead, oldSalesLead)
            {
                PatchChanges = changes,
                UpdateOption = loadOption
            });

            return StatusOk();
        }

        #endregion

        #region Delete

        [HttpDelete("{ids}")]
        public async Task<ActionResult> DeleteMultipleAsync(string ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
                return Exception(ExceptionCodes.SaleLead_MissingIdsToDelete, "Please specify Ids of sales-lead need to delete");
            var salesLeadIds = ids.ToListString();
            var affects = await _service.DeleteSalesLeadsAsync(salesLeadIds);
            if (affects <= 0)
            {
                return Exception(ExceptionCodes.SaleLead_NotFound, $@"These Ids {ids} not existed");
            }
            return StatusOk();
        }

        #endregion

        #region Excel

        [HttpPost("import")]
        public async Task<ActionResult> ImportAsync([FromForm] ImportSalesLeadRequest request)
        {
            if (!Request.HasFormContentType || Request.Form.Files.Count <= 0)
            {
                return BadRequest();
            }

            request.ExcelContent = Request.Form.Files.FirstOrDefault()?.OpenReadStream();

            var response = await _service.ImportSalesLeadAsync(request);

            return Ok(response);
        }

        #endregion

        #region Comment

        [HttpPost("{salesLeadId}/comment")]
        public async Task<ActionResult<SalesLeadSearchResponse>> SearchCommentAsync(SalesLeadCommentSearchRequest request, string salesLeadId)
        {
            request.FilterCondition.Fields.Add(new FieldFilterDetail { Name = "SalesLeadId", Value = salesLeadId });
            var response = await _salesLeadCommentService.SearchAsync(request);
            return Paging(response);
        }

        [HttpPost("{salesLeadId}/new-comment")]
        public async Task<ActionResult> AddNewCommentAsync(string salesLeadId, [FromBody] SalesLeadCommentDto request)
        {
            if (string.IsNullOrEmpty(salesLeadId))
            {
                return BadRequest();
            }

            request.SalesLeadId = salesLeadId;

            var response = await _salesLeadCommentService.CreateAsync(request);
            return StatusCode(StatusCodes.Status201Created, response);
        }


        [HttpPatch("edit-comment/{id}")]
        public async Task<ActionResult> UpdateCommentByPatchAsync(int id, [FromBody][EnrichNotNull] List<Operation<SalesLeadCommentDto>> operations)
        {
            var loadOption = GetLoadOption<SalesLeadCommentDto, SalesLeadCommentUpdateOption>(operations);

            //alway set true be customer beyond saleslead
            var salesLeadComment = await _salesLeadCommentService.GetByIdAsync(id);
            if (salesLeadComment == null)
            {
                return ExceptionNotFound(ExceptionCodes.SaleLeadComment_NotFound, $@"SaleLead comment id {id} not found");
            }

            var changes = GetJsonPatchOperationsAsDto(operations);
            var oldComment = salesLeadComment.CloneByJson();

            //merge current with data patch
            JsonPatchApplyTo(salesLeadComment, operations);

            await _salesLeadCommentService.UpdateAsync(new SalesLeadCommentUpdateRequest(salesLeadComment, oldComment){
                PatchChanges = changes,
                UpdateOption = loadOption
            });

            return StatusOk();
        }


        #endregion

        #region Other  

        /// <summary>
        /// move unverify salesLead (from import data) to salesLead
        /// </summary>
        /// <param name="id">string id</param>
        /// <returns></returns>
        [HttpPatch("{id}/verify")]
        public async Task<IActionResult> VerifySalesLead(string id, [FromBody][EnrichNotNull] List<Operation<SalesLeadDto>> operations)
        {
            var loadOption = GetLoadOption<SalesLeadDto, SalesLeadUpdateOption>(operations);

            //alway set true be customer beyond saleslead
            loadOption.Customer = true;
            var fullSalesLead = await _service.GetSalesLeadDetailAsync(id, loadOption);
            if (fullSalesLead == null)
            {
                return ExceptionNotFound(ExceptionCodes.SaleLead_NotFound, $@"SaleLead id {id} not found");
            }
            var changes = GetJsonPatchOperationsAsDto(operations);
            var oldSalesLead = fullSalesLead.CloneByJson();

            //merge current with data patch
            JsonPatchApplyTo(fullSalesLead, operations);

            await _service.UpdateSalesLeadAsync(new SalesLeadUpdateRequest(fullSalesLead, oldSalesLead)
            {
                IsVerify = true,
                PatchChanges = changes,
                UpdateOption = loadOption
            });

            return StatusOk();
        }

        #endregion
    }
}