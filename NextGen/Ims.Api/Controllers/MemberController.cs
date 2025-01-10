using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.Scoped.IMS;
using Pos.Model.Model.Table.IMS;

namespace PosAPI.Controllers
{
    [Route("v1/api/[controller]")]
    public class MemberController : Controller
    {
        private IMemberService _memService;

        public MemberController(IMemberService memService) {
            _memService = memService;
        }

        [HttpGet("loadDepartment")]
        public async Task<IActionResult> loadDepartment(string MemberNumber)
        {
            try
            {
                var respon = await _memService.loadDepartment(MemberNumber);
                return Ok(new { status = 200, data = respon });
            }
            catch (Exception ex) {
                return Ok(new { status = 500, mess = ex.Message, trace = ex.StackTrace });
            }
            
        }
    }
}
