using Pos.Application.Event;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Event.POS;
using Enrich.IMS.Dto.Customer;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Request;
using Microsoft.AspNetCore.Authorization;

namespace PosAPI.Controllers
{
    [Route("v1/[controller]")]
    [AllowAnonymous]
    public class LicenseController
    {
        private ILicenseEvent _licenseEvent;
        private IOrderEvent _orderEvent;

        public LicenseController(ILicenseEvent licenseEvent, IOrderEvent orderEvent)
        {
            _licenseEvent = licenseEvent;
            _orderEvent = orderEvent;
        }

        [HttpPost("ClosePendingOrder")]
        public async Task<rsData> ClosePendingOrder()
        {
            var jsResult = new rsData();
            await _orderEvent.ClosePendingOrder();
            jsResult.Status = 200;
            jsResult.Message = "OK";
            return jsResult;
        }

        [HttpGet("CloseOrder")]
        public async Task<rsData> CloseOrderByCode(string OrderCode)
        {
            var jsResult = new rsData();
            try
            {
                var data = await _orderEvent.CloseOrderByCode(OrderCode);
                jsResult.Status = data.status;
                jsResult.Message = data.message;
                jsResult.Obj_data = data.data;
            }
            catch(Exception e)
            {
                jsResult.Status = 500;
                jsResult.Message = e.Message;
                jsResult.Obj_data = e;
            }
            return jsResult;
        }

        [HttpGet("StoreChange")]
        public async Task<rsData> StoreChange(StoreLicenses modelLicense)
        {
            var jsResult = new rsData();
            try
            {
                var data = await _orderEvent.StoreChane(modelLicense);
                jsResult.Status = data.status;
                jsResult.Message = data.message;
                jsResult.Obj_data = data.data;
            }
            catch (Exception e)
            {
                jsResult.Status = 500;
                jsResult.Message = e.Message;
                jsResult.Obj_data = e;
            }
            return jsResult;
        }
    }
}
