using Microsoft.AspNetCore.Mvc;
using Pos.Application.Event;
using Pos.Application.Services.Scoped;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Table.IMS;
using System.Security;

namespace PosAPI.Controllers
{
    public class EmailController
    {
        private IEmailService _mailService;

        public EmailController(IEmailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPut]
        [Route("SendMail")]
        public rsData SendMail(string sendTo, string subject, string body)
        {
            var jsResult = new rsData();
            try
            {
                _mailService.SendMail(sendTo, subject, body);
                jsResult.Status = 200;
            }
            catch (Exception e)
            {
                jsResult.Status = 500;
                jsResult.Message = e.Message;
                jsResult.Obj_data = e;
            }
            //LogService.WriteLog(JsonConvert.SerializeObject(jsResult), "Log", "Response");
            return jsResult;
        }
    }
}
