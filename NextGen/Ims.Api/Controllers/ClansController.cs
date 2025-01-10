using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.Singleton;
using Pos.Model.Model.Orther;

namespace PosAPI.Controllers
{
    [Route("v1/[controller]")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public class ClansController : Controller
    {
        private readonly IClanService _clanService;

        public ClansController(IClanService clanService)
        {
            _clanService = clanService ?? throw new ArgumentNullException(nameof(clanService));
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IEnumerable<Clan>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadAllAsync()
        {
            var allClans = await _clanService.ReadAllAsync();
            return Ok(allClans);
        }
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(typeof(IEnumerable<Clan>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ReadNull()
        {
            return Ok(null);
        }
    }
}
