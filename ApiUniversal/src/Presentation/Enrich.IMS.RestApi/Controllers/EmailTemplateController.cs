using Enrich.Core.Container;
using Enrich.Dto;
using Enrich.IMS.Dto.EmailTemplate;
using Enrich.IMS.Entities;
using Enrich.IMS.RestApi.Library;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Enrich.IMS.RestApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class EmailTemplateController : EnrichAuthApiController<EmailTemplate, EmailTemplateDto>
    {
        private IEmailTemplateService _service => _service as IEmailTemplateService;
        public EmailTemplateController(IEmailTemplateService service, EnrichContext context, IEnrichContainer container)
           : base(service, context, container) {

        }

        //public override async Task<ActionResult> CreateAsync([FromBody] EmailTemplateDto dto)
        //{
        //    var response = await _service.AddAsync(dto);
        //    return StatusCode(StatusCodes.Status201Created, response);
        //}
    }
}
