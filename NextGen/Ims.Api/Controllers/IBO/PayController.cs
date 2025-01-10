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
using Pos.Application.DBContext;

namespace PosAPI.Controllers.IBO
{
    [Route("/v1/api/[controller]")]
    public class PayController : ImsBaseController
    {
        private IOrderEvent _orderEvent;
        private IOrderService _orderService;
        private ICustomerService _customerService;
        private IAuthorizeNetService _authorizeNetService;
        private ITransactionService _transactionService;
        private IStoreService _storeService;

        public PayController(IOrderEvent orderEvent, IStoreService storeService, IOrderService orderService, ICustomerService customerService, IAuthorizeNetService authorizeNetService, ITransactionService transactionService)
        {
            _orderEvent = orderEvent;
            _storeService = storeService;
            _orderService = orderService;
            _customerService = customerService;
            _authorizeNetService = authorizeNetService;
            _transactionService = transactionService;
        }

        [AllowAnonymous]
        [HttpPost("Collect")]
        public async Task<IActionResult> Collect(TransRequest info)
        {
            //var pc = Session[PC_PAY]?.ToString();
            bool msgSuccess = false;
            try
            {
                C_CustomerCard cardData = new C_CustomerCard();
                bool paylater = false;

                O_Orders order = await _orderService.getOrderByCode(info.OrdersCode);
                if (order == null)
                {
                    return Ok(new { status = 400, message = $"Invoice #{info.OrdersCode} not found." });
                }
                //C_CustomerTransaction old_trans = await _orderService.getCusTransByCode(info.OrdersCode);
                if (order?.Status == "Closed")
                {
                    //TempData["s"] = "Invoice has been closed.";
                    //return RedirectToAction("Index", new { key = info?.Key });
                    return Ok(new { status = 400, message = "Invoice has been closed." });
                }
                C_Customer cus = await _customerService.getCustomerInfoByCode(order.CustomerCode);
                if(cus == null)
                {
                    return Ok(new { status = 400, message = $"Customer [{order.CustomerCode}], had been voided." });
                }

                info.MxMerchant_Id = cus.MxMerchant_Id; 
                //info.PaymentProfile_Id = cus.DepositAccountNumber; 
                if ((info.PaymentProfile_Id ?? "").Trim() != "" && (info.MxMerchant_Id ?? "").Trim() != "")
                {
                    //AuthorizeNetResponse result = _authorizeNetService.ChargeCustomerProfile(info.MxMerchant_Id, info.PaymentProfile_Id, order.GrandTotal ?? 0);

                    //await _orderService.updateOrderStauts(OrderStatus.Closed, info.OrdersCode, result.transid, result.message);
                    //return Ok(new { result });
                }
                else
                {
                    creditCardType creditCard = new creditCardType();
                    creditCard.cardNumber = info.CardNumber;
                    creditCard.expirationDate = info.CardExpiry;
                    creditCard.cardCode = info.CardCSC;

                    if ((info.MxMerchant_Id ?? "").Trim() == "")
                    {
                        AuthorizeNetResponse responseData = _authorizeNetService.CreateCustomerProfile(cus.Email, cus.CustomerCode, creditCard);
                        if (responseData.status != 200)
                        {
                            return Ok(new { responseData });
                        }
                        info.MxMerchant_Id = responseData.transid;
                        info.PaymentProfile_Id = responseData.code;
                        await _customerService.updateCustomerMerchantId(responseData.transid, cus.CustomerCode);
                        await _customerService.updateCustomerPaymentProfileId(responseData.code, cus.CustomerCode);
                    }
                    if ((info.PaymentProfile_Id ?? "").Trim() == "")
                    {
                        AuthorizeNetResponse responseData = _authorizeNetService.CreateCustomerPaymentProfile(info.MxMerchant_Id, creditCard);
                        if (responseData.status != 200)
                        {
                            return Ok(new { responseData });
                        }
                        info.PaymentProfile_Id = responseData.transid;
                        await _customerService.updateCustomerPaymentProfileId(responseData.transid, cus.CustomerCode);
                    }
                }

                DateTime now = DateTime.UtcNow;
                AuthorizeNetResponse result = _authorizeNetService.ChargeCustomerProfile(info.MxMerchant_Id, info.PaymentProfile_Id, order.GrandTotal ?? 0);
                C_CustomerTransaction insertTransaction = new C_CustomerTransaction();
                Guid myuuid = Guid.NewGuid();
                insertTransaction.Id = myuuid.ToString().Replace("-", "");
                insertTransaction.Card = info.PaymentProfile_Id;
                insertTransaction.PaymentStatus = result.status.ToString();
                insertTransaction.ResponseText = JsonConvert.SerializeObject(result.data);
                insertTransaction.CustomerCode = order.CustomerCode;
                insertTransaction.StoreCode = cus.StoreCode;
                insertTransaction.OrdersCode = order.OrdersCode;
                insertTransaction.Amount = order.GrandTotal ?? 0;
                insertTransaction.CreateAt = now;
                insertTransaction.UpdateAt = now;
                try
                {
                    createTransactionResponse resultData = (createTransactionResponse)result.data;
                    insertTransaction.BankName = resultData.transactionResponse.accountType;
                    if (resultData.transactionResponse.testRequest == "1")
                    {
                        insertTransaction.BankName += "[TEST]";
                    }
                    insertTransaction.CardNumber = resultData.transactionResponse.accountNumber;
                    insertTransaction.MxMerchant_id = info.MxMerchant_Id;
                    insertTransaction.MxMerchant_token = info.PaymentProfile_Id;
                    insertTransaction.MxMerchant_authMessage = result.message;
                    insertTransaction.PaymentMethod = "Pay.Collect";
                    insertTransaction.PaymentNote = info.PaymentNote;
                }
                catch { }
                //insertTransaction.BankName = resultData;
                await _transactionService.insertCustomerTrans(insertTransaction);
                if (result.status == 200)
                {
                    await _orderService.updateOrderStauts(OrderStatus.Paid_Wait, info.OrdersCode, result.transid, result.message);
                    await _orderEvent.CloseOrder(info.OrdersCode);
                    await _storeService.UpdateStoreService(cus.CustomerCode);
                }
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return Ok(new { status = 500, message = ex.Message, stack = ex.StackTrace });
            }
        }

        [AllowAnonymous]
        [HttpGet("GetListCardByCustomerId")]
        public async Task<IActionResult> GetListCardByCustomerId(string CustomerCode)
        {
            //var pc = Session[PC_PAY]?.ToString();
            try
            {
                C_Customer cus = await _customerService.getCustomerInfoByCode(CustomerCode);
                if((cus.MxMerchant_Id??"") == "")
                {
                    return Ok(new { status = 401, message = "Customer Does not have profile" });
                }
                var result = _authorizeNetService.GetCustomerProfile(cus.MxMerchant_Id);
                return Ok(new { result });
            }
            catch (Exception ex)
            {
                return Ok(new { status = 500, message = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
