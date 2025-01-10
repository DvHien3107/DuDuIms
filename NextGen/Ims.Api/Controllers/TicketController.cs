using Enrich.Common;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.Singleton;
using static Enrich.IMS.Dto.SqlColumns;
using System.Linq;
using Pos.Model.Model.Request;
using Microsoft.AspNetCore.Authorization;
using Pos.Application.Services.Scoped.IMS;

namespace PosAPI.Controllers
{
    [AllowAnonymous]
    [Route("/v1/api/[controller]")]
    public class TicketController : Controller
    {
        private readonly ITicketService _ticketService;

        public TicketController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet("LoadTicket")]
        public async Task<IActionResult> LoadTicket(DateTime start, DateTime end, string DepartmentID = "", string TypeId = "")
        {
            try
            {
                var lstTicket = await _ticketService.LoadTicket(DepartmentID, TypeId, start, end);
                return Ok(new { status = 200, reponsData = lstTicket });
            }
            catch (Exception ex)
            {
                return Ok(new { status = 500, message = ex.Message, trace = ex.StackTrace });
            }

        }
    }
}
