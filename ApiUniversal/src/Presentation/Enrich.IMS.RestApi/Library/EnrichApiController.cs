using Enrich.Common.Helpers;
using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.List;
using Enrich.IMS.RestApi.Library.Results;
using Enrich.RestApi.NetCorePlatform;
using Enrich.Services.Implement.Library;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace Enrich.IMS.RestApi.Library
{
    [ApiController]
    public class EnrichApiController : ControllerBase
    {
        /// <summary>
        /// Must be pass in constructor
        /// </summary>
        protected readonly IEnrichContainer EnrichContainer;

        public EnrichApiController(IEnrichContainer container) : this()
        {
            EnrichContainer = container;
        }

        public EnrichApiController()
        { }

        #region JsonPatch

        protected void JsonPatchApplyTo<T>(T toObject, List<Operation<T>> operations) where T : class
        {
            JsonPatchHelper.ApplyTo<T>(toObject, operations);
        }

        protected T JsonPatchApply<T>(T original, List<Operation<T>> operations) where T : class
        {
            var newObj = original.CloneByJson();
            JsonPatchHelper.ApplyTo<T>(newObj, operations);

            return newObj;
        }

        protected List<string> GetChangedPropNames<T>(List<Operation<T>> operations) where T : class
        {
            return JsonPatchHelper.GetChangedPropNames<T>(operations);
        }

        protected List<PatchOperationDto> GetJsonPatchOperationsAsDto<T>(List<Operation<T>> operations) where T : class
        {
            return JsonPatchHelper.GetOperationsAsDto<T>(operations);
        }

        #endregion

        #region LoadOption

        /// <summary>
        /// Get load option by using LoadOrUpdateOptionAttribute
        /// </summary>
        protected TLoad GetLoadOption<T, TLoad>(List<Operation<T>> operations)
            where T : class
            where TLoad : class, new()
        {
            return LoadUpdateOptionHelper.GetLoadOption<T, TLoad>(operations);
        }

        /// <summary>
        /// Get load option by using LoadOrUpdateOptionAttribute
        /// </summary>
        protected TLoad GetLoadOption<TLoad>(string propNames, TLoad defValue = default(TLoad))
            where TLoad : class, new()
        {
            return LoadUpdateOptionHelper.GetLoadOption<TLoad>(propNames, defValue);
        }

        #endregion

        #region Results

        //Status
        protected StatusCodeResult StatusOk() => StatusCode(StatusCodes.Status200OK);

        protected StatusCodeResult StatusCreated() => StatusCode(StatusCodes.Status201Created);

        protected StatusCodeResult StatusBadRequest() => StatusCode(StatusCodes.Status400BadRequest);

        protected StatusCodeResult StatusNoContent() => StatusCode(StatusCodes.Status204NoContent);

        //Empty
        protected OkObjectResult EmptyNull() => Ok(null);

        protected OkObjectResult EmptyObject() => Ok(new EmptyObj());

        protected OkObjectResult EmptyList<T>() => Ok(Enumerable.Empty<T>());

        //Paging
        protected PagingResult<TResult> Paging<TResult>(PagingResponseDto<TResult> result, bool onlyRecords = false)
            => new PagingResult<TResult>(result, onlyRecords);

        ////OK-AsyncEnumerable
        //protected OkObjectResult OkAsAsyncEnumerable<T>(IEnumerable<T> sources, CancellationToken cancellationToken = default(CancellationToken)) => Ok(new AsyncEnumerable<T>(async yield =>
        //{
        //    var count = await sources.YieldAsync(yield, cancellationToken).ConfigureAwait(false);
        //}));

        //Exception
        protected ExceptionResult Exception(int businessCode, string message, object extendData = null)
         => Exception(StatusCodes.Status500InternalServerError, businessCode, message, extendData);

        protected ExceptionResult ExceptionBadRequest(int businessCode, string message, object extendData = null)
           => Exception(StatusCodes.Status400BadRequest, businessCode, message, extendData);

        protected ExceptionResult ExceptionNotFound(int businessCode, string message, object extendData = null)
            => Exception(StatusCodes.Status404NotFound, businessCode, message, extendData);

        protected ExceptionResult Exception(int httpStatusCode, int businessCode, string message, object extendData = null)
            => new ExceptionResult(httpStatusCode, businessCode, message, extendData);

        private class EmptyObj
        {
        }

        #endregion

        #region Value     

        private NameValueIndexer<string> _path;
        protected NameValueIndexer<string> Path => _path ?? (_path = new NameValueIndexer<string>(name => RouteData.Values[name].GetString()));

        private NameValueIndexer<string> _query;
        protected NameValueIndexer<string> Query => _query ?? (_query = new NameValueIndexer<string>(name => Request.Query[name].FirstOrDefault()));

        private NameValueIndexer<string> _header;
        protected NameValueIndexer<string> Header => _header ?? (_header = new NameValueIndexer<string>(name => Request.Headers[name].FirstOrDefault()));

        #endregion
    }
}
