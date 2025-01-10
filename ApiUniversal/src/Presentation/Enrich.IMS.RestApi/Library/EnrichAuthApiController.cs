using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Core.Infrastructure.Services;
using Enrich.Dto;
using Enrich.Dto.Requests;
using Enrich.IMS.Services.Implement.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Library
{
    public class EnrichAuthApiController<T, TDto> : EnrichAuthApiController where TDto : class
    {
        protected readonly IGenericService<T, TDto> _genericService;

        public EnrichAuthApiController(IGenericService<T, TDto> genericService, EnrichContext context = null, IEnrichContainer container = null)
            : base(context, container)
        {
            _genericService = genericService;
        }

        #region Basic Operations

        [HttpGet]
        public virtual async Task<ActionResult> GetAllAsync([FromQuery] QueryPagingRequest request)
        {
            request = request ?? new QueryPagingRequest();
            var response = await _genericService.QueryPagingAsync(request);

            return Paging(response);
        }

        
        [HttpGet("{id:int}")]
        public virtual async Task<ActionResult> GetDetailAsync(int id)
        {
            var dto = id > 0 ? await _genericService.GetAsync<TDto>(id) : default(TDto);
            if (dto == null)
            {
                return ExceptionNotFound(StatusCodes.Status404NotFound, $"Resource {id} not found");
            }

            return Ok(dto);
        }

        [HttpPost]
        public virtual async Task<ActionResult> CreateAsync([FromBody] TDto dto)
        {
            if (dto == null)
            {
                return ExceptionBadRequest(StatusCodes.Status404NotFound, $"Resource not found");
            }

            await ValidateAsync(dto);

            var created = await _genericService.AddAsync(dto);

            return Ok(new { Created = created });
        }

        [HttpPut]
        public virtual async Task<ActionResult> UpdateAsync([FromBody] TDto dto)
        {
            if (dto == null)
            {
                return ExceptionBadRequest(StatusCodes.Status404NotFound, $"Resource not found");
            }

            await ValidateAsync(dto);
            var affected = await _genericService.UpdateAsync(dto);

            return Ok(new { Updated = affected > 0 });
        }

        [HttpPatch("{id:int}")]
        public virtual async Task<ActionResult> UpdateByPatchAsync(int id, [FromBody] List<Operation<TDto>> operations)
        {
            var dto = id > 0 ? await _genericService.GetAsync<TDto>(id) : default(TDto);
            if (dto == null)
            {
                return ExceptionNotFound(StatusCodes.Status404NotFound, $"Resource {id} not found");
            }

            JsonPatchApplyTo(dto, operations);

            await ValidateAsync(dto);
            var affected = await _genericService.UpdateAsync(dto);

            return Ok(new { Updated = affected > 0 });
        }

        #endregion

        #region Delete

        [HttpDelete("{id:int}")]
        public virtual async Task<ActionResult> DeleteAsync(int id)
        {
            var exists = id > 0 ? await _genericService.ExistAsync(id) : false;
            if (!exists)
            {
                return ExceptionNotFound(StatusCodes.Status404NotFound, $"Resource {id} not found");
            }

            var affected = await _genericService.DeleteByIdAsync(id);

            return Ok(new { Deleted = affected > 0 });
        }

        #endregion


        protected virtual async Task<bool> ValidateAsync(TDto dto)
        {
            return await Task.FromResult(true);
        }
    }

    [EnrichAuth]
    [ApiController]
    public class EnrichAuthApiController : EnrichApiController
    {
        /// <summary>
        /// Must be pass in constructor
        /// </summary>
        protected readonly EnrichContext EnrichContext;

        private EnrichValidationHelper _validationHelper;
        /// <summary>
        /// Make sure EnrichContext already init
        /// </summary>
        protected EnrichValidationHelper ValidationHelper
        {
            get
            {
               return _validationHelper ??
          (_validationHelper = EnrichContainer?.Resolve<EnrichValidationHelper>());

            }

        }

        public EnrichAuthApiController(EnrichContext context, IEnrichContainer container = null)
            : base(container)
        {
            EnrichContext = context;
        }

        public EnrichAuthApiController()
        {
        }

        protected virtual async Task ValidateAsync<TModel>(TModel model)
        {
            await Task.CompletedTask;
        }

        #region From Query

        protected Language? LanguageFromQuery(string queryName = "lang", bool currentIfInvalid = true, EnrichContext EnrichContext = null)
            => Language.EN;

        #endregion     
    }
}
