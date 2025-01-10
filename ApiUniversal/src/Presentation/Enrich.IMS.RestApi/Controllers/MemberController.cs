using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.Member;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{
    [EnrichRoute(EnrichRouteName.Member)]
    public class MemberController : EnrichAuthApiController<Member, MemberDto>
    {
        private IMemberService _service => _genericService as IMemberService;

        public MemberController(IMemberService service, EnrichContext context, IEnrichContainer container)
        : base(service, context, container)
        {
        }

        [HttpGet("quicksearch")]
        public async Task<ActionResult<IEnumerable<MemberQuickSearchItemDto>>> QuickSearchAsync()
        {
            var searchText = Query["text"];
            var response = await _service.QuickSearchAsync(searchText);
            return Ok(response);
        }

        [HttpGet("{email}")]
        public async Task<ActionResult<Member>> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return BadRequest();

            var response = await _service.GetByEmailAsync(email);

            return Ok(response);
        }
    }
}
