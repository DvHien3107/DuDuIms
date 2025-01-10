using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pos.Application.Event.POS;
using Pos.Application.Event;
using Pos.Application.Services.Singleton;
using Pos.Model.Model.Request;
using Pos.Model.Model.Table.IMS;
using Pos.Application.Services.Scoped;
using Pos.Application.Services.Scoped.Payment;
using AuthorizeNet.Api.Contracts.V1;
using Pos.Model.Model.Comon.Payment;
using Pos.Application.Services.Scoped.IMS;
using Pos.Model.Enum.IMS;
using Microsoft.AspNetCore.Authorization;
using static Enrich.IMS.Dto.SqlColumns;
using Enrich.IMS.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Pos.Application.Extensions.Helper;
using Pos.Application.DBContext;

namespace PosAPI.Controllers.IBO
{
    [AllowAnonymous]
    [Route("/v1/api/[controller]")]
    public class OrderController : ImsBaseController
    {
        private IOrderEvent _orderEvent;
        private IOrderService _orderService;
        private ICustomerService _customerService;
        private IAuthorizeNetService _authorizeNetService;
        private ITransactionService _transactionService;
        private IStoreService _storeService;
        private IMemberService _memberService;

        public OrderController(IMemberService memberService, IOrderEvent orderEvent, IStoreService storeService, IOrderService orderService, ICustomerService customerService, IAuthorizeNetService authorizeNetService, ITransactionService transactionService)
        {
            _memberService = memberService;
            _orderEvent = orderEvent;
            _storeService = storeService;
            _orderService = orderService;
            _customerService = customerService;
            _authorizeNetService = authorizeNetService;
            _transactionService = transactionService;
        }

        [HttpPost("LoadOrder")]
        public async Task<IActionResult> LoadOrder(string OrderCode)
        {
            var respon = await _orderService.reloadOrder(OrderCode);
            return Ok(new { status = respon.Status, message = respon.Message });
        }

        [HttpPost("UpdateRecurringCard")]
        public async Task<IActionResult> UpdateRecurringCard(string cardAccountId, string SubscriptionId, string StoreServiceId, bool AutoRenew)
        {
            try
            {
                await _storeService.UpdateRecurringCard(cardAccountId, SubscriptionId, StoreServiceId, AutoRenew);
                return Ok(new { status = 200, message = "Update Success" });
            }
            catch (Exception ex)
            {
                return Ok(new { status = 500, message = ex.Message, trace = ex.StackTrace });
            }

        }
    }
}
