using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pos.Application.Extensions.Helper;
using Pos.Application.Services.Scoped;
using Pos.Application.Services.Singleton;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Proc;
using Pos.Model.Model.Request;
using Pos.Model.Model.Table.POS;
using Pos.Model.Model.Table.RCP;
using Promotion.Model.Model.Comon;

namespace PosAPI.Controllers
{
    [AllowAnonymous]
    [Route("v1/[controller]")]
    public class RCPStoreController : Controller
    {
        private readonly IIMSRequestService _imsRequestService;
        private readonly IRCPService _rcpService;
        private readonly IPOSService _posService;
        private readonly IIMSService _imsService;
        private readonly ITwilioService _twilioService;
        public RCPStoreController(IIMSRequestService imsRequestService, IRCPService rcpService, IPOSService posService, IIMSService imsService, ITwilioService twilioService)
        {
            _imsRequestService = imsRequestService;
            _rcpService = rcpService;
            _posService = posService;
            _imsService = imsService;
            _twilioService = twilioService;
        }

        [HttpPost]
        [Route("SyncTollFree")]
        public async Task<rsData> SyncTollFree()
        {
            var jsResult = new rsData();
            try
            {
                var listTollFree = await _imsService.SyncTollFreeToPos();
                var CurrentTollFree = await _rcpService.getCurrentTollFree();
                List<GetTollFree> lstResult = new List<GetTollFree>();
                foreach (var item in listTollFree)
                {
                    var check = CurrentTollFree.FirstOrDefault(x => x.SId == item.SId && x.AuthToken == item.AuthToken);
                    if (check == null)
                    {
                        string messageservice = await _twilioService.getMessageService(item.SId, item.AuthToken, item.VerificationStatus);
                        await _rcpService.insertCurrentTollFree(item, messageservice);
                        lstResult.Add(item);
                    }
                }
                jsResult.Status = 200;
                jsResult.Obj_data = lstResult;
            }
            catch (Exception e)
            {
                jsResult.Status = 500;
                jsResult.Message = e.Message;
                jsResult.Obj_data = e;
            }
            try
            {
                await SyncPosLicense();
            }
            catch { }
            //LogService.WriteLog(JsonConvert.SerializeObject(jsResult), "Log", "Response");
            return jsResult;
        }

        [HttpPost]
        [Route("RegisTollFree")]
        public async Task<rsData> RegisTollFree()
        {
            var jsResult = new rsData();
            try
            {
                var CurrentTollFree = await _rcpService.getPendingTollFree();
                List<ImsTollFree> lstResult = new List<ImsTollFree>();
                foreach (var item in CurrentTollFree)
                {
                    string messageservice = await _twilioService.createMessageService(item.VerificationStatus + " Message Service", item.SId, item.AuthToken);
                    await _rcpService.updateTollFree(item.PhoneNumber, messageservice);
                    item.VerificationStatus = messageservice;
                    lstResult.Add(item);
                }
                jsResult.Status = 200;
                jsResult.Obj_data = lstResult;
            }
            catch (Exception e)
            {
                jsResult.Status = 500;
                jsResult.Message = e.Message;
                jsResult.Obj_data = e;
            }
            return jsResult;
        }

        [HttpPost]
        [Route("SyncTollPhone")]
        public async Task<rsData> SyncTollPhone()
        {
            var jsResult = new rsData();
            try
            {
                var CurrentTollFree = await _rcpService.getCurrentTollFree();
                CurrentTollFree = CurrentTollFree.Where(x => (x.PhoneMessageService == null || x.PhoneMessageService == "not found") && x.MessageService != "not found").ToList();
                List<ImsTollFree> lstResult = new List<ImsTollFree>();
                foreach (var item in CurrentTollFree)
                {
                    string phoneservice = await _twilioService.getPhoneMessageService(item.SId, item.AuthToken, item.MessageService, item.PhoneNumber);
                    await _rcpService.updatePhoneTollFree(item.PhoneNumber, phoneservice);
                    item.VerificationStatus = phoneservice;
                    lstResult.Add(item);
                }
                jsResult.Status = 200;
                jsResult.Obj_data = lstResult;
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
        [Route("RegisTollPhone")]
        public async Task<rsData> RegisTollPhone()
        {
            var jsResult = new rsData();
            try
            {
                var CurrentTollFree = await _rcpService.getCurrentTollFree();
                CurrentTollFree = CurrentTollFree.Where(x => x.PhoneMessageService == "not found").ToList();
                List<ImsTollFree> lstResult = new List<ImsTollFree>();
                foreach (var item in CurrentTollFree)
                {
                    string phonesid = await _imsService.getTollfreePhoneNumberSid(item.PhoneNumber);
                    string phoneservice = await _twilioService.createPhoneMessageService(item.MessageService, phonesid, item.SId, item.AuthToken);
                    await _rcpService.updatePhoneTollFree(item.PhoneNumber, phoneservice);
                    item.VerificationStatus = phoneservice;
                    lstResult.Add(item);
                }
                jsResult.Status = 200;
                jsResult.Obj_data = lstResult;
            }
            catch (Exception e)
            {
                jsResult.Status = 500;
                jsResult.Message = e.Message;
                jsResult.Obj_data = e;
            }
            return jsResult;
        }

        [HttpPost]
        [Route("SyncPosLicense")]
        public async Task<rsData> SyncPosLicense()
        {
            var jsResult = new rsData();
            try
            {
                var CurrentTollFree = await _imsService.getNextWeekLicense();
                foreach (var itm in CurrentTollFree)
                {
                    await _rcpService.updateExpDate(itm.StoreCode, itm.EndDate);
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


    }
}
