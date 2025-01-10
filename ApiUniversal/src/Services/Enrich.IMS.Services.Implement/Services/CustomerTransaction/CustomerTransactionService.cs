using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Core.Container;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.IMS.Dto;
using Enrich.IMS.Dto.SendEmail;
using Enrich.IMS.Dto.Sms;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface;
using Enrich.IMS.Infrastructure.Data.Interface.Builders;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Mappers;
using Enrich.IMS.Services.Interface.Services;
using Enrich.Payment.MxMerchant.Models;
using Enrich.Services.Implement;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Services.Implement.Services
{
    public partial class CustomerTransactionService : GenericService<CustomerTransaction, CustomerTransactionDto>, ICustomerTransactionService
    {
        private ICustomerTransactionMapper _mapper => _mapperGeneric as ICustomerTransactionMapper;

        private readonly ICustomerTransactionRepository _repository;
        private readonly ISystemConfigurationRepository _repositorySystem;
        private readonly ICustomerRepository _repositoryCustomer;
        private readonly IPartnerRepository _repositoryPartner;
        private readonly IOrderRepository _repositoryOrder;
        private readonly IOrderSubscriptionRepository _repositoryOrderSubscription;
        private readonly IStoreServiceRepository _repositoryStoreService;
        private readonly IEnrichLog _enrichLog;
        private readonly IEnrichContainer _container;
        private readonly IPaymentFunction _paymentFunction;
        private readonly IEnrichSendEmail _enrichSendEmail;
        private readonly IEnrichSms _enrichSMS;
        private readonly ICustomerTransactionBuilder _builder;
        private readonly string _imsHost;

        public CustomerTransactionService(
            IConfiguration appConfig,
            IEnrichLog enrichLog,
            ICustomerTransactionRepository repository,
            ICustomerTransactionMapper mapper,
            ISystemConfigurationRepository repositorySystem,
            ICustomerRepository repositoryCustomer,
            IOrderRepository repositoryOrder,
            IEnrichContainer container,
            ICustomerTransactionBuilder builder,
            IStoreServiceRepository repositoryStoreService,
            IEnrichSendEmail enrichSendEmail,
            IPaymentFunction paymentFunction,
            IPartnerRepository repositoryPartner,
            IEnrichSms enrichSMS,
            IOrderSubscriptionRepository repositoryOrderSubscription)
            : base(repository, mapper)
        {
            _repository = repository;
            _enrichLog = enrichLog;
            _repositorySystem = repositorySystem;
            _repositoryCustomer = repositoryCustomer;
            _repositoryOrder = repositoryOrder;
            _container = container;
            _builder = builder;
            _repositoryStoreService = repositoryStoreService;
            _enrichSendEmail = enrichSendEmail;
            _paymentFunction = paymentFunction;
            _repositoryPartner = repositoryPartner;
            _enrichSMS = enrichSMS;
            _repositoryOrderSubscription = repositoryOrderSubscription;
            _imsHost = appConfig["IMSHost"];
        }

        public void Save_MxMerchantTokens(OauthInfo auth)
        {
            var webinfo = _repositorySystem.GetSystemConfiguration();
            webinfo.MxMerchant_AccessToken = auth.AccessToken;
            webinfo.MxMerchant_AccessSecret = auth.AccessSecret;
            _repositorySystem.Update(webinfo);
        }
        public async Task<CustomerTransaction> CreateWithFree(Order order)
        {
            _enrichLog.Info($"{EnumHelper.DisplayDescription(PaymentEnum.Message.Free)} order {order.OrdersCode} customer {order.CustomerCode}");
            //check exist approved transaction => return transaction success
            var _oldTransaction = await _repository.GetApproveTransactionAsync(order.OrdersCode);
            if (_oldTransaction != null) return _oldTransaction;
            try
            {
                var customer = await _repositoryCustomer.GetByCustomerCodeAsync(order.CustomerCode) ?? new Customer();
                //create new transaction
                var newTransaction = _builder.BuildForFree(order);
                await _repository.AddAsync(newTransaction);
                await ActionPaymentSuccess(order, null, null, null);
                return newTransaction;
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Error transaction free, order {order.OrdersCode} customer {order.CustomerCode}");
            }
            return new CustomerTransaction();
        }
        public async Task<CustomerTransaction> CreateWithCreditCard(Order order, CustomerCard card)
        {
            bool isPaid = false;
            var customer = await _repositoryCustomer.GetByCustomerCodeAsync(order.CustomerCode) ?? new Customer();
            try
            {
                _enrichLog.Info($"{EnumHelper.DisplayDescription(PaymentEnum.Message.CreditCard)} order {order.OrdersCode} customer {order.CustomerCode}");
                if (order == null) throw new Exception(ValidationMessages.NotFound.Order);
                if (card == null) throw new Exception(ValidationMessages.NotFound.Card);
                if (string.IsNullOrEmpty(card.MxMerchant_Token)) throw new Exception(ValidationMessages.NotYet.Card);
                if (card.MxMerchant_Id == null || card.MxMerchant_Id == 0) throw new Exception(ValidationMessages.NotFound.MxMerchant);

                //check exist approved transaction => return transaction success
                var _oldTransaction = await _repository.GetApproveTransactionAsync(order.OrdersCode);
                if (_oldTransaction != null) return _oldTransaction;
                if (string.IsNullOrEmpty(ConvertHelper.GetString(customer.MxMerchantId)))
                {
                    var partner = _repositoryPartner.GetByCode(customer.PartnerCode);
                    if (string.IsNullOrEmpty(ConvertHelper.GetString(partner?.MxMerchant_Id))) throw new Exception(ValidationMessages.NotYet.Card);
                    customer.MxMerchantId = partner.MxMerchant_Id;
                }
                var payment_info = new MxMerchantDto
                {
                    Amount = order.GrandTotal ?? 0,
                    MxMerchantToken = card.MxMerchant_Token,
                    MxMerchantId = customer.MxMerchantId.ToString(),
                };
                var respone = _paymentFunction.MakePayment(payment_info);
                _enrichLog.Info($"Order {order.OrdersCode} make payment with creditcard respone, customer {order.CustomerCode}", new { Response = JsonConvert.SerializeObject(respone) });
                //create transaction
                var newTransaction = _builder.BuildForCreditCard(order, card);
                newTransaction.PaymentStatus = respone.status;
                newTransaction.ResponseText = respone.authMessage;
                newTransaction.MxMerchant_token = respone.paymentToken;
                newTransaction.MxMerchant_authMessage = respone.authMessage;
                await _repository.AddAsync(newTransaction);

                //handle payment
                if (respone.status == PaymentEnum.Status.Approved.ToString())
                {
                    isPaid = true;
                    await ActionPaymentSuccess(order, customer, newTransaction, card);
                }
                else
                    await ActionPaymentFailed(order, customer, newTransaction);

                return newTransaction;
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Error transaction creditcard, order {order.OrdersCode} customer {order.CustomerCode}");
                var newTransaction = _builder.BuildForCreditCard(order, card);
                newTransaction.PaymentStatus = isPaid ? PaymentEnum.Status.Approved.ToString() : PaymentEnum.Status.Failed.ToString();
                newTransaction.ResponseText = e.Message;
                await _repository.AddAsync(newTransaction);

                if (!isPaid)
                    await ActionPaymentFailed(order, customer, newTransaction);
                else
                    await ActionPaymentSuccess(order, customer, newTransaction, null);

                return newTransaction;
            }
        }
        public async Task<CustomerTransaction> CreateWithACH(Order order)
        {
            bool isPaid = false;
            var customer = _repositoryCustomer.GetByCustomerCode(order.CustomerCode) ?? new Customer();
            try
            {
                _enrichLog.Info($"{EnumHelper.DisplayDescription(PaymentEnum.Message.ACH)} order {order.OrdersCode} customer {order.CustomerCode}");
                if (order == null) throw new Exception(ValidationMessages.NotFound.Order);
                //check exist approved transaction => return transaction success
                var _oldTransaction = await _repository.GetApproveTransactionAsync(order.OrdersCode);
                //if (_oldTransaction != null) return _oldTransaction;
                var payment_info = new MxMerchantDto
                {
                    Amount = order.GrandTotal ?? 0,
                    AccountNumber = customer.DepositAccountNumber,
                    RoutingNumber = customer.DepositRoutingNumber,
                    OwnerName = customer.OwnerName,
                    BankName = customer.DepositBankName
                };
                var respone = _paymentFunction.MakePaymentACH(payment_info);
                _enrichLog.Info($"Order {order.OrdersCode} make payment with ACH respone customer {order.CustomerCode}", new { Response = JsonConvert.SerializeObject(respone) });
                //create transaction
                var newTransaction = _builder.BuildForACH(order, customer);
                newTransaction.PaymentStatus = respone.status;
                newTransaction.ResponseText = respone.authMessage;
                newTransaction.MxMerchant_token = respone.paymentToken;
                newTransaction.MxMerchant_authMessage = respone.authMessage;
                await _repository.AddAsync(newTransaction);

                //handle payment
                if (respone.status == PaymentEnum.Status.Approved.ToString())
                {
                    isPaid = true;
                    await ActionPaymentSuccess(order, customer, newTransaction, null);
                }
                else
                    await ActionPaymentFailed(order, customer, newTransaction);

                return newTransaction;
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Error transaction ACH, order {order.OrdersCode} customer {order.CustomerCode}");
                var newTransaction = _builder.BuildForACH(order, customer);
                newTransaction.PaymentStatus = isPaid ? PaymentEnum.Status.Approved.ToString() : PaymentEnum.Status.Failed.ToString();
                newTransaction.ResponseText = e.Message;
                await _repository.AddAsync(newTransaction);

                if (!isPaid)
                    await ActionPaymentFailed(order, customer, newTransaction);
                else
                    await ActionPaymentSuccess(order, customer, newTransaction, null);
                return newTransaction;
            }
        }
        public async Task ActionPaymentSuccess(Order order, Customer customer, CustomerTransaction customerTransaction, CustomerCard customerCard)
        {
            try
            {
                await SendNotificationPaySuccess(order, customer, customerTransaction, customerCard);

                //check auto active subscription
                var infoSystem = _repositorySystem.GetSystemConfiguration();
                if (infoSystem.AutoActiveRecurringLicense == true)
                {
                    try
                    {
                        var storeServices = _repositoryStoreService.GetByOrderCode(order.OrdersCode);
                        var activator = _container.Resolve<IActivatorService>();
                        //actication subscription
                        foreach (var item in storeServices)
                        {
                            await activator.ActivatorAction(item.Id, SubscriptionEnum.Action.Active.ToString(), SubscriptionEnum.Action.Recurring.ToString());
                        }
                        //change status order = closed
                        await _repositoryOrder.ChangeStatusAsync(ConvertHelper.GetString(order.Id), OrderEnum.Status.Closed.ToString());
                    }
                    catch
                    {
                        //change status order = paid/wait
                        await _repositoryOrder.ChangeStatusAsync(ConvertHelper.GetString(order.Id), OrderEnum.Status.Paid_Wait.ToString());
                    }
                    return;
                }
                else
                {
                    //change status order = paid/wait
                    await _repositoryOrder.ChangeStatusAsync(ConvertHelper.GetString(order.Id), OrderEnum.Status.Paid_Wait.ToString());
                }
            }
            catch (Exception ex)
            {
                _enrichLog.Error(ex, $"Error action payment success, order {order.OrdersCode} customer {order.CustomerCode}");
            }
            return;
        }
        public async Task ActionPaymentFailed(Order order, Customer customer, CustomerTransaction customerTransaction)
        {
            try
            {
                await SendNotificationPayFailed(order, customer, customerTransaction);

                //check auto active subscription
                var infoSystem = _repositorySystem.GetSystemConfiguration();
                if (infoSystem.AutoActiveRecurringLicense == true)
                {
                    //change status order = payment later
                    order.DueDate = DateTime.UtcNow.AddDays(infoSystem.ExtensionDay ?? 3);
                    await _repositoryOrder.UpdateAsync(order);
                    await _repositoryOrder.ChangeStatusAsync(ConvertHelper.GetString(order.Id), OrderEnum.Status.PaymentLater.ToString());

                    var storeServices = _repositoryStoreService.GetByOrderCode(order.OrdersCode);
                    var activator = _container.Resolve<IActivatorService>();
                    //actication subscription
                    foreach (var item in storeServices)
                    {
                        await activator.ActivatorAction(item.Id, SubscriptionEnum.Action.Active.ToString(), SubscriptionEnum.Action.Recurring.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                _enrichLog.Error(ex, $"Error action payment error, order {order.OrdersCode} customer {order.CustomerCode}");
            }

            return;
        }

        private async Task SendNotificationPaySuccess(Order order, Customer customer, CustomerTransaction customerTransaction, CustomerCard customerCard)
        {
            try
            {
                await SendSMSReceiptPayment(customer, customerTransaction);

                if (customerCard == null) await SendEmailReceiptPayment(customer, customerTransaction);
                else await SendEmailReceiptPayment(customer, customerTransaction, customerCard);
            }
            catch (Exception ex)
            {
                _enrichLog.Error(ex, $"Error sent notification payment success, order {order.OrdersCode} customer {order.CustomerCode}");
            }
        }
        private async Task SendNotificationPayFailed(Order order, Customer customer, CustomerTransaction customerTransaction)
        {
            try
            {
                if (customer == null || customerTransaction == null) return;

                await SendEmailFailedPayment(customer, customerTransaction);
                await SendSMSFailedPayment(customer, customerTransaction);
            }
            catch (Exception ex)
            {
                _enrichLog.Error(ex, $"Error sent notification payment failed, order {order.OrdersCode} customer {order.CustomerCode}");
            }
        }

        private async Task SendEmailReceiptPayment(Customer customer, CustomerTransaction customerTransaction, CustomerCard card)
        {
            try
            {
                _enrichLog.Info($"Send receipt payment email to {customer.CustomerCode}, order {customerTransaction.OrdersCode}");
                //send email
                var mail = new SendEmailBySendGridTemplateId();
                mail.TemplateId = SendGridTemplate.PaidSuccessfully;
                mail.Subject = string.Empty;
                mail.To = new List<EmailAddress>() { new EmailAddress { Email = customer.Email ?? customer.SalonEmail, Name = customer.BusinessName } };
                mail.Service = "Recurring";
                mail.StoreCode = customer.StoreCode;
                mail.Data = new
                {
                    short_description = "A recurring payment was posted to the account (s) listed below",
                    salon_name = customer.BusinessName,
                    grand_total = (customerTransaction.Amount).ToString("#,##0.#0"),
                    grand_date_paid = customerTransaction.CreateAt.HasValue ? customerTransaction.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                    receipt = new List<receipt_data>{new receipt_data
                    {
                        salon = $"{customer.BusinessName} (#{customer.StoreCode})",
                        card = $"{card.CardType} - {card.CardNumber.Substring(4,6)}",
                        name = card.CardHolderName,
                        order_code = "#" + customerTransaction.OrdersCode,
                        date_paid = customerTransaction.CreateAt.HasValue ? customerTransaction.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                        total = customerTransaction.Amount.ToString("#,##0.#0"),
                    }}
                };
                await _enrichSendEmail.SendBySendGridTemplateId(mail);
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Send payment email error, order {customerTransaction.OrdersCode} customer {customerTransaction.CustomerCode}");
            }
        }
        private async Task SendEmailReceiptPayment(Customer customer, CustomerTransaction customerTransaction)
        {
            try
            {
                _enrichLog.Info($"Send receipt payment email to {customer.CustomerCode}, order {customerTransaction.OrdersCode}");
                var mail = new SendEmailBySendGridTemplateId();
                mail.TemplateId = SendGridTemplate.PaidSuccessfully;
                mail.Subject = string.Empty;
                mail.To = new List<EmailAddress>() { new EmailAddress { Email = customer.Email ?? customer.SalonEmail, Name = customer.BusinessName } };
                mail.Service = "Recurring";
                mail.StoreCode = customer.StoreCode;
                mail.Data = new
                {
                    short_description = "A recurring payment was posted to the account (s) listed below",
                    salon_name = customer.BusinessName,
                    grand_total = (customerTransaction.Amount).ToString("#,##0.#0"),
                    grand_date_paid = customerTransaction.CreateAt.HasValue ? customerTransaction.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                    receipt = new List<receipt_data>{new receipt_data
                            {
                                salon = $"{customer.BusinessName} (#{customer.StoreCode})",
                                card = $"ACH - {customer.DepositAccountNumber?.Substring((customer.DepositAccountNumber?.Length ?? 4) - 4)}",
                                order_code = "#" + customerTransaction.OrdersCode,
                                date_paid = customerTransaction.CreateAt.HasValue ? customerTransaction.CreateAt.Value.ToString("dd, MMM yyyy hh:mm tt (UTC)") : "",
                                total = customerTransaction.Amount.ToString("#,##0.#0"),
                            }}
                };
                await _enrichSendEmail.SendBySendGridTemplateId(mail);
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Send payment email error, order {customerTransaction.OrdersCode} customer {customerTransaction.CustomerCode}");
            }
        }
        private async Task SendEmailFailedPayment(Customer customer, CustomerTransaction customerTransaction)
        {
            try
            {
                _enrichLog.Info($"Send payment failed email to {customer.CustomerCode}, order {customerTransaction.OrdersCode}");
                var mail = new SendEmailBySendGridTemplateId();
                mail.TemplateId = SendGridTemplate.PaymentFailed;
                mail.Subject = string.Empty;
                mail.To = new List<EmailAddress>() { new EmailAddress { Email = customer.Email ?? customer.SalonEmail, Name = customer.BusinessName } };
                mail.Service = "Recurring";
                mail.StoreCode = customer.StoreCode;

                var order = _repositoryOrder.GetByOrderCode(customerTransaction.OrdersCode);
                var keyPair = Convert.ToBase64String(Encoding.UTF8.GetBytes(customer.MD5PassWord));
                var keyOrder = Convert.ToBase64String(Encoding.UTF8.GetBytes(order.OrdersCode));
                var orderSubscriptions = _repositoryOrderSubscription.GetByOrderCode(customerTransaction.OrdersCode);
                var servicesList = new List<object>();
                orderSubscriptions.ToList().ForEach(_order =>
                {
                    servicesList.Add(new
                    {
                        type = _order.Period ?? "One-time",
                        name = _order.ProductName,
                        date = (_order.StartDate?.ToString("MMM dd, yyyy") ?? "") + (_order.EndDate?.ToString(" - MMM dd, yyyy") ?? ""),
                        quanlity = _order.Quantity ?? 1,
                        unit = _order.Price?.ToString("#,##0.#0"),
                        discount = _order.DiscountPercent > 0 ? "(discount " + _order.DiscountPercent + "%)" : (_order.Discount > 0 ? "(discount $" + _order.Discount + ")" : ""),
                        amount = _order.Price?.ToString("#,##0.#0")
                    });
                });
                var hardware = new
                {
                    order_code = $"#{order.OrdersCode}",
                    order_date = order.CreatedAt?.ToString("MMM dd, yyyy"),
                    business_name = customer.BusinessName,
                    salon_code = $"#{customer.StoreCode}",
                    contact_name = string.IsNullOrEmpty(customer.OwnerName) ? customer.ContactName?.ToUpper() : customer.OwnerName?.ToUpper(),
                    contact_mobile = string.IsNullOrEmpty(customer.OwnerMobile) ? customer.BusinessPhone : customer.OwnerMobile,
                    licenses = servicesList,
                    subtotal = order.ServiceAmount?.ToString("#,##0.#0"),
                    discount = "0",
                    discount_rate = "0",
                    shippingFee = "0",
                    tax_rate = "0",
                    tax = "0",
                    total = order.GrandTotal?.ToString("#,##0.#0")
                };
                var finalServices = new List<object>();
                if (hardware != null) finalServices.Add(hardware);
                var mailData = new
                {
                    salon_name = customer.BusinessName,
                    payment_link = $"{_imsHost}/PaymentGate/Pay/?key={keyOrder}:{keyPair}",
                    cancel_link = $"{_imsHost}/PaymentGate/Pay/Cancel?key={keyOrder}:{keyPair}",
                    services = finalServices,
                    grand_total = customerTransaction.Amount.ToString("#,##0.#0"),
                    order_date = order.CreatedAt?.ToString("MMM dd, yyyy")
                };
                mail.Data = new
                {
                    salon_name = customer.BusinessName,
                    payment_link = $"{_imsHost}/PaymentGate/Pay/?key={keyOrder}:{keyPair}",
                    cancel_link = $"{_imsHost}/PaymentGate/Pay/Cancel?key={keyOrder}:{keyPair}",
                    services = finalServices,
                    grand_total = customerTransaction.Amount.ToString("#,##0.#0"),
                    order_date = order.CreatedAt?.ToString("MMM dd, yyyy")
                };
                await _enrichSendEmail.SendBySendGridTemplateId(mail);
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Send payment failed email error, order {customerTransaction.OrdersCode} customer {customerTransaction.CustomerCode}");
            }
        }
        private async Task SendSMSReceiptPayment(Customer customer, CustomerTransaction customerTransaction)
        {
            try
            {
                //send SMS
                var subcriptions = await _repositoryOrderSubscription.GetByOrderCodeAsync(customerTransaction.OrdersCode);
                if (!subcriptions.Any(c => c.SubscriptionType == SubscriptionEnum.Type.license.ToString() && string.IsNullOrEmpty(customer.PartnerCode))) return;
                _enrichLog.Info($"Send receipt payment SMS to {customer.CustomerCode}, order {customerTransaction.OrdersCode}");
                string smsMessage = $"Mango POS: We have applied your {"$" + string.Format("{0:#,0.00}", (customerTransaction.Amount).ToString("#,##0.#0"))} subscription payment. Thank you";
                var dataSMS = new SendSmsRequest
                {
                    Receiver = customer.OwnerMobile ?? customer.SalonPhone,
                    Text = smsMessage
                };
                await _enrichSMS.SendAsync(dataSMS);
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Send payment SMS error, order {customerTransaction.OrdersCode} customer {customerTransaction.CustomerCode}");
            }
        }
        private async Task SendSMSFailedPayment(Customer customer, CustomerTransaction customerTransaction)
        {
            try
            {
                //send SMS
                var infoSystem = _repositorySystem.GetSystemConfiguration();
                var subcriptions = await _repositoryOrderSubscription.GetByOrderCodeAsync(customerTransaction.OrdersCode);
                if (!subcriptions.Any(c => c.SubscriptionType == SubscriptionEnum.Type.license.ToString() && string.IsNullOrEmpty(customer.PartnerCode))) return;
                _enrichLog.Info($"Send payment failed SMS to {customer.CustomerCode}, order {customerTransaction.OrdersCode}");
                string smsMessage = $"Mango POS: We have applied your {"$" + string.Format("{0:#,0.00}", customerTransaction.Amount)} subscription payment. Thank you";
                var dataSMS = new SendSmsRequest
                {
                    Receiver = customer.OwnerMobile ?? customer.SalonPhone,
                    Text = smsMessage
                };
                await _enrichSMS.SendAsync(dataSMS);
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"Send payment SMS error, order {customerTransaction.OrdersCode} customer {customerTransaction.CustomerCode}");
            }
        }
    }
}