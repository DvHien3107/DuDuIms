using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Services.Scoped;
using Pos.Application.Services.Singleton;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Table.IMS;
using SlackAPI.WebSocketMessages;

namespace PosAPI.Controllers
{
    [AllowAnonymous]
    [Route("v1/[controller]")]
    public class TwilioController : Controller
    {
        private readonly IRCPService _rcpService;
        private readonly IPOSService _posService;

        public TwilioController(IRCPService rcpService, IPOSService posService)
        {
            _rcpService = rcpService;
            _posService = posService;
        }

        [HttpGet]
        [Route("getLstTollFreeToSync")]
        public async Task<rsData> getLstTollFreeToSync()
        {
            var jsResult = new rsData();
            try
            {
                var CurrentTollFree = await _posService.getLstTollFreeToSync();
                jsResult.Obj_data = CurrentTollFree;
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
        [HttpPost]
        [Route("AddTwillioResponse")]
        public async Task<rsData> AddTwillioResponse([FromBody]RDTwillioResponse item)
        {
            var jsResult = new rsData();
            try
            {
                _rcpService.insertRDTwillioResponse(item);
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

        [HttpPost]
        [Route("AddTwillioResponseArray")]
        public async Task<rsData> AddTwillioResponseArray([FromBody]RDTwillioResponse[] item)
        {
            var jsResult = new rsData();
            try
            {
                foreach (var item2 in item)
                {
                    _rcpService.insertRDTwillioResponse(item2);
                }
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
        //[HttpGet]
        //[Route("getLstTollFreeToSyncNew")]
        //public async Task<rsData> getLstTollFreeToSyncNew([FromBody] RDTwillioResponse defaultitem)
        //{
        //    var jsResult = new rsData();
        //    try
        //    {
        //        var CurrentTollFree = await _posService.getLstTollFreeToSync();
        //        foreach (var item in CurrentTollFree)
        //        {
        //            RDTwillioResponse newRow = new RDTwillioResponse();

        //        }
        //        jsResult.Obj_data = CurrentTollFree;
        //        jsResult.Status = 200;
        //    }
        //    catch (Exception e)
        //    {
        //        jsResult.Status = 500;
        //        jsResult.Message = e.Message;
        //        jsResult.Obj_data = e;
        //    }
        //    //LogService.WriteLog(JsonConvert.SerializeObject(jsResult), "Log", "Response");
        //    return jsResult;
        //}

    }
}
