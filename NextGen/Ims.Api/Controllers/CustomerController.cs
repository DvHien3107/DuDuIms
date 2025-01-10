using AuthorizeNet.Api.Contracts.V1;
using Enrich.IMS.Dto.Customer;
using Microsoft.AspNetCore.Mvc;
using Pos.Application.Event;
using Pos.Application.Services.Scoped.Payment;
using Pos.Model.Model.Comon.Payment;
using Pos.Model.Model.Table.IMS;


namespace PosAPI.Controllers
{
    [Route("v1/[controller]")]
    public class CustomerController : Controller
    {

        private ICustomerEvent _cusEvent;
        private IAuthorizeNetService _authService;
        
        public CustomerController(ICustomerEvent cusEvent, IAuthorizeNetService authService)
        {
            _cusEvent = cusEvent;
            _authService = authService;
        }

        [HttpPost("searchv2")]
        public async Task<ActionResult<CustomerSearchResponse>> SearchV2([FromBody]CustomerSearchRequest request)
        {
            var response = await _cusEvent.SearchAsyncv2(request);
            return response;
        }

        [HttpGet("getLstCreditCard")]
        public async Task<IActionResult> getLstCreditCard(string CustomerCode)
        {
            C_Customer customer = await _cusEvent.getCustomerByCode(CustomerCode);
            if (customer == null)
            {
                return Ok(new { status = 401, message = "Customer Not Found" });
            }
            if ((customer.MxMerchant_Id ?? "").Trim() == "")
            {
                return Ok(new { status = 401, message = "Customer Card Profile Not Found" });
            }
            else
            {
                AuthorizeNetResponse response = _authService.GetCustomerProfile(customer.MxMerchant_Id ?? "");
                return Ok(response);
            }
        }

        [HttpPost("addCreditcard")]
        public async Task<IActionResult> addCreditcard(string CustomerCode, string cardNumber, string expirationDate, string cardCode)
        {
            C_Customer customer = await _cusEvent.getCustomerByCode(CustomerCode);
            if(customer == null)
            {
                return Ok(new { status = 401, message = "Customer Not Found" });
            }
            creditCardType creditCard = new creditCardType();
            creditCard.cardNumber = cardNumber;
            creditCard.expirationDate = expirationDate;
            creditCard.cardCode = cardCode;
            AuthorizeNetResponse response = new AuthorizeNetResponse();
            if ((customer.MxMerchant_Id??"").Trim() == "")
            {
                response = _authService.CreateCustomerProfile(customer.Email, customer.CustomerCode, creditCard);
                if(response.status == 200)
                {
                   await _cusEvent.updateCustomerMerchantId(response.transid, customer.CustomerCode);
                }
            }
            else
            {
                response = _authService.CreateCustomerPaymentProfile(customer.MxMerchant_Id??"", creditCard);
            }
            return Ok(response);
        }
        //[HttpPost("report-active-account")]
        //public async Task<ActionResult<CustomerSearchResponse>> activeAccountReport()
        //{
        //    var response = await _cusEvent.SearchAsyncv2(request);
        //    return response;
        //}

    }
}
