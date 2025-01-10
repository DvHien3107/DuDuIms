using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Event;
using Pos.Model.Model.Auth;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Respons;
using Pos.Model.Model.Table;

namespace PosAPI.Controllers
{
    [AllowAnonymous]
    [Route("/v1/[controller]")]
    public class RecurringController : Controller
    {
        private IOrderEvent _orderEvent;
        public RecurringController(IOrderEvent orderEvent) {
            _orderEvent = orderEvent;
        }

        [HttpGet("Recurring1Hours")]
        public async Task<IActionResult> Recurring1Hours()
        {
            await _orderEvent.ClosePendingOrder();
            return Ok(new { status = 200 });
        }

        [HttpGet("Recurring3Hours")]
        public async Task<IActionResult> Recurring3Hours()
        {

            return Ok(new { status = 200 });
        }

        [HttpGet("Recurring6Hours")]
        public async Task<IActionResult> Recurring6Hours()
        {
           
            return Ok(new { status = 200 });
        }

        [HttpGet("Recurring12Hours")]
        public async Task<IActionResult> Recurring12Hours()
        {

            return Ok(new { status = 200 });
        }
    }
}
