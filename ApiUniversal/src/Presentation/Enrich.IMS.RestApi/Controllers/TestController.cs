using Enrich.Core.Utils;
using Enrich.IMS.Dto.SendEmail;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Enrich.IMS.RestApi.Controllers
{
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEnrichSendEmail _enrichSendEmail;

        public TestController(IEnrichSendEmail enrichSendEmail)
        {
            _enrichSendEmail = enrichSendEmail;
        }

        [HttpGet("Test")]
        public IActionResult TestSendEmail()
        {
            var data = new SendEmailRequest();
            data.Body = "this is body";
            data.Subject = "this is subject";
            data.To = new List<EmailAddress>() { new EmailAddress { Email = "bau.phamngoc@enrichco.us", Name = "Pham Ngoc Bau" }};
            data.Service = "Recurring";
            data.StoreCode = "S0001";
            _enrichSendEmail.Send(data);
            return Ok();
        }

        [HttpGet("TestSendEmailBySendgridTemplateId")]
        public IActionResult TestSendEmailBySendgridTemplateId ()
        {
            var data = new SendEmailBySendGridTemplateId();
            data.TemplateId = "d-eb6591a522b24ea69daab78c193b0e9e";
            data.Subject = "this is subject";
            data.To = new List<EmailAddress>() { new EmailAddress { Email = "bau.phamngoc@enrichco.us", Name = "Pham Ngoc Bau" } };
            data.Service = "Recurring";
            data.StoreCode = "S0001";
            var sendgriddatatest = new {user="123" };
            data.Data = sendgriddatatest;
            _enrichSendEmail.SendBySendGridTemplateId(data);
            return Ok();
        }
    }
}
