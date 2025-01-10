using Enrich.Dto.List;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enrich.IMS.RestApi.Library.Results
{
    public class PagingResult<TResult> : ObjectResult
    {
        private readonly PagingResponseDto<TResult> _result;

        public PagingResult(PagingResponseDto<TResult> result, bool onlyRecords = false)
            : base(onlyRecords ? (object)result.Records : result)
        {
            _result = result;
            StatusCode = StatusCodes.Status200OK;
        }

        public override void OnFormatting(ActionContext context)
        {
            base.OnFormatting(context);

            if (_result?.Pagination != null)
            {
                context.HttpContext.Response.Headers.Add("X-PAGINATION-TOTAL", new[] { _result.Pagination.TotalRecords.ToString() });
                context.HttpContext.Response.Headers.Add("X-PAGINATION-PAGE-COUNT", new[] { _result.Pagination.PageCount.ToString() });
                context.HttpContext.Response.Headers.Add("X-PAGINATION-PAGE-SIZE", new[] { _result.Pagination.PageSize.ToString() });
                context.HttpContext.Response.Headers.Add("X-PAGINATION-PAGE-INDEX", new[] { _result.Pagination.PageIndex.ToString() });
            }
        }
    }
}
