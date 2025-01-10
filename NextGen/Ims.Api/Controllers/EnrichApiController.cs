using Enrich.Dto.Base;
using Enrich.Dto.List;
using Enrich.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Enrich.Common.Helpers;

namespace PosAPI.Controllers
{
    [ApiController]
    public class EnrichApiController : ControllerBase
    {
        /// <summary>
        /// Must be pass in constructor
        /// </summary>
       
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
        //protected PagingResult<TResult> Paging<TResult>(PagingResponseDto<TResult> result, bool onlyRecords = false)
        //    => new PagingResult<TResult>(result, onlyRecords);

        //////OK-AsyncEnumerable
        ////protected OkObjectResult OkAsAsyncEnumerable<T>(IEnumerable<T> sources, CancellationToken cancellationToken = default(CancellationToken)) => Ok(new AsyncEnumerable<T>(async yield =>
        ////{
        ////    var count = await sources.YieldAsync(yield, cancellationToken).ConfigureAwait(false);
        ////}));

        ////Exception
        //protected ExceptionResult Exception(int businessCode, string message, object extendData = null)
        //=> Exception(StatusCodes.Status500InternalServerError, businessCode, message, extendData);

        //protected ExceptionResult ExceptionBadRequest(int businessCode, string message, object extendData = null)
        //=> Exception(StatusCodes.Status400BadRequest, businessCode, message, extendData);

        //protected ExceptionResult ExceptionNotFound(int businessCode, string message, object extendData = null)
        //=> Exception(StatusCodes.Status404NotFound, businessCode, message, extendData);

        //protected ExceptionResult Exception(int httpStatusCode, int businessCode, string message, object extendData = null)
        //    => new ExceptionResult(httpStatusCode, businessCode, message, extendData);

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
