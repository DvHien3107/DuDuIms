using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Pos.Model.Model.Comon.Payment;
using Promotion.Model.Model.Comon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Scoped.Payment
{
    public interface IAuthorizeNetService
    {
        //Payment
        AuthorizeNetResponse QuickCharge(decimal amount, creditCardType creditCard, customerAddressType billingAddress, List<lineItemType> lineItems);
        AuthorizeNetResponse ChargeCustomerProfile(string customerProfileId, string customerPaymentProfileId, decimal Amount);
        //Customer Profile
        AuthorizeNetResponse CreateCustomerProfile(string emailId, string posCustomerId, creditCardType creditCard);
        AuthorizeNetResponse CreateCustomerPaymentProfile(string customerProfileId, creditCardType creditCard);
        AuthorizeNetResponse GetCustomerProfile(string customerProfileId);
        AuthorizeNetResponse DeleteCustomerProfile(string customerProfileId);
        AuthorizeNetResponse DeleteCustomerPaymentProfile(string customerProfileId, string customerPaymentProfileId);

    }
    public class AuthorizeNetService : IAuthorizeNetService
    {
        private AuthorizeNet.Environment environment = AuthorizeNet.Environment.PRODUCTION;
        #region Payment
        public AuthorizeNetResponse QuickCharge(decimal amount, creditCardType creditCard, customerAddressType billingAddress, List<lineItemType> lineItems)
        {
            AuthorizeNetResponse responseData = new AuthorizeNetResponse();

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = environment;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Const.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Const.ApiTransactionKey,
            };

            //var creditCard = new creditCardType
            //{
            //    cardNumber = "4111111111111111",
            //    expirationDate = "1035",
            //    cardCode = "123"
            //};

            //var billingAddress = new customerAddressType
            //{
            //    firstName = "John",
            //    lastName = "Doe",
            //    address = "123 My St",
            //    city = "OurTown",
            //    zip = "98004"
            //};

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            // Add line Items
            //var lineItems = new lineItemType[2];
            //lineItems[0] = new lineItemType { itemId = "1", name = "t-shirt", quantity = 2, unitPrice = new Decimal(15.00) };
            //lineItems[1] = new lineItemType { itemId = "2", name = "snowboard", quantity = 1, unitPrice = new Decimal(450.00) };
            lineItemType[] arrItems = lineItems.ToArray();
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card

                amount = amount,
                payment = paymentType,
                billTo = billingAddress,
                lineItems = arrItems
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();
            // validate response
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse.messages != null)
                    {
                        responseData.status = 200;
                        responseData.transid = response.transactionResponse.transId;
                        responseData.code = response.transactionResponse.messages[0].code;
                        responseData.message = response.transactionResponse.messages[0].description;
                    }
                    else
                    {
                        responseData.status = 500;
                        responseData.message = "Failed Transaction.";
                        if (response.transactionResponse.errors != null)
                        {
                            responseData.code = response.transactionResponse.errors[0].errorCode;
                            responseData.message = response.transactionResponse.errors[0].errorText;
                        }
                    }
                }
                else
                {
                    responseData.status = 500;
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        responseData.code = response.transactionResponse.errors[0].errorCode;
                        responseData.message = response.transactionResponse.errors[0].errorText;
                    }
                    else
                    {
                        responseData.code = response.messages.message[0].code;
                        responseData.message = response.messages.message[0].text;
                    }
                }
            }
            else
            {
                responseData.status = 500;
                responseData.code = "000";
                responseData.message = "Null Response.";
            }

            return responseData;
        }
        public AuthorizeNetResponse ChargeCustomerProfile(string customerProfileId, string customerPaymentProfileId, decimal Amount)
        {
            Console.WriteLine("Charge Customer Profile");

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = environment;
            AuthorizeNetResponse responseData = new AuthorizeNetResponse();

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Const.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Const.ApiTransactionKey
            };

            //create a customer payment profile
            customerProfilePaymentType profileToCharge = new customerProfilePaymentType();
            profileToCharge.customerProfileId = customerProfileId;
            profileToCharge.paymentProfile = new paymentProfile { paymentProfileId = customerPaymentProfileId };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // refund type
                amount = Amount,
                profile = profileToCharge
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the collector that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            // validate response
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse.messages != null)
                    {
                        responseData.status = 200;
                        responseData.transid = response.transactionResponse.transId;
                        responseData.code = response.transactionResponse.messages[0].code;
                        responseData.message = response.transactionResponse.messages[0].description;
                        responseData.data = response;
                    }
                    else
                    {
                        responseData.status = 500;
                        responseData.message = "Failed Transaction.";
                        if (response.transactionResponse.errors != null)
                        {
                            responseData.code = response.transactionResponse.errors[0].errorCode;
                            responseData.message = response.transactionResponse.errors[0].errorText;
                        }
                    }
                }
                else
                {
                    responseData.status = 500;
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        responseData.code = response.transactionResponse.errors[0].errorCode;
                        responseData.message = response.transactionResponse.errors[0].errorText;
                    }
                    else
                    {
                        responseData.code = response.messages.message[0].code;
                        responseData.message = response.messages.message[0].text;
                    }
                }
            }
            else
            {
                responseData.status = 500;
                responseData.code = "000";
                responseData.message = "Null Response.";
            }

            return responseData;
        }
        #endregion

        #region CustomerProfile
        public AuthorizeNetResponse CreateCustomerProfile(string emailId, string posCustomerId, creditCardType creditCard)
        {
            AuthorizeNetResponse responseData = new AuthorizeNetResponse();

            // set whether to use the sandbox environment, or production enviornment
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = environment;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Const.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Const.ApiTransactionKey,
            };
            List<customerPaymentProfileType> paymentProfileList = new List<customerPaymentProfileType>();
            customerPaymentProfileType ccPaymentProfile = new customerPaymentProfileType();
            // standard api call to retrieve response
            if (creditCard != null)
            {
                paymentType cc = new paymentType { Item = creditCard };
                ccPaymentProfile.payment = cc;
            }
            

            //customerPaymentProfileType echeckPaymentProfile = new customerPaymentProfileType();

            paymentProfileList.Add(ccPaymentProfile);
            //paymentProfileList.Add(echeckPaymentProfile);


            customerProfileType customerProfile = new customerProfileType();
            customerProfile.merchantCustomerId = posCustomerId;
            customerProfile.email = emailId;
            customerProfile.paymentProfiles = paymentProfileList.ToArray();

            var request = new createCustomerProfileRequest { profile = customerProfile, validationMode = validationModeEnum.none };

            // instantiate the controller that will call the service
            var controller = new createCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            createCustomerProfileResponse response = controller.GetApiResponse();

            // validate response 
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.messages.message != null)
                    {
                        responseData.status = 200;
                        responseData.transid = response.customerProfileId;
                        responseData.code = response.customerPaymentProfileIdList[0];
                        responseData.message = "Success!";
                    }
                }
                else
                {
                    responseData.status = 500;
                    responseData.code = response.messages.message[0].code;
                    responseData.message = response.messages.message[0].text;

                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    responseData.status = 500;
                    responseData.code = response.messages.message[0].code;
                    responseData.message = response.messages.message[0].text;
                }
                else
                {
                    responseData.status = 500;
                    responseData.code = "000";
                    responseData.message = "Null Response.";
                }
            }

            return responseData;
        }
        public AuthorizeNetResponse CreateCustomerPaymentProfile(string customerProfileId, creditCardType creditCard)
        {
            AuthorizeNetResponse responseData = new AuthorizeNetResponse();
            // set whether to use the sandbox environment, or production enviornment
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = environment;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Const.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Const.ApiTransactionKey,
            };

            paymentType cc = new paymentType { Item = creditCard };

            customerPaymentProfileType echeckPaymentProfile = new customerPaymentProfileType();
            echeckPaymentProfile.payment = cc;

            var request = new createCustomerPaymentProfileRequest
            {
                customerProfileId = customerProfileId,
                paymentProfile = echeckPaymentProfile,
                validationMode = validationModeEnum.testMode
            };

            // instantiate the controller that will call the service
            var controller = new createCustomerPaymentProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            createCustomerPaymentProfileResponse response = controller.GetApiResponse();

            // validate response 
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.messages.message != null)
                    {
                        responseData.status = 200;
                        responseData.transid = response.customerPaymentProfileId;
                        responseData.message = "Success!";
                    }
                }
                else
                {
                    responseData.status = 500;
                    responseData.code = response.messages.message[0].code;
                    responseData.message = response.messages.message[0].text;
                    if (response.messages.message[0].code == "E00039")
                    {
                        responseData.message = "Duplicate Payment Profile ID: " + response.customerPaymentProfileId;
                    }
                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    responseData.status = 500; 
                    responseData.code = response.messages.message[0].code;
                    responseData.message = response.messages.message[0].text;
                }
                else
                {
                    responseData.status = 500;
                    responseData.code = "000";
                    responseData.message = "Null Response.";
                }
            }

            return responseData;

        }
        public AuthorizeNetResponse GetCustomerProfile(string customerProfileId)
        {
            AuthorizeNetResponse responseData = new AuthorizeNetResponse();
            Console.WriteLine("Get Customer Profile sample");

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = environment;
            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Const.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Const.ApiTransactionKey,
            };

            var request = new getCustomerProfileRequest();
            request.customerProfileId = customerProfileId;

            // instantiate the controller that will call the service
            var controller = new getCustomerProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                responseData.status = 200;
                responseData.data = response;
                responseData.message = response.messages.message[0].text;
            }
            else if (response != null)
            {
                responseData.status = 500;
                responseData.code = response.messages.message[0].code;
                responseData.message = response.messages.message[0].text;
            }

            return responseData;
        }
        public AuthorizeNetResponse DeleteCustomerProfile(string customerProfileId)
        {
            AuthorizeNetResponse responseData = new AuthorizeNetResponse();
            Console.WriteLine("DeleteCustomerProfile Sample");
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = environment;
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Const.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Const.ApiTransactionKey,
            };

            //please update the subscriptionId according to your sandbox credentials
            var request = new deleteCustomerProfileRequest
            {
                customerProfileId = customerProfileId
            };

            //Prepare Request
            var controller = new deleteCustomerProfileController(request);
            controller.Execute();

            //Send Request to EndPoint
            deleteCustomerProfileResponse response = controller.GetApiResponse();
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    responseData.status = 200;
                    responseData.message = response.messages.resultCode.ToString();
                }
            }
            else if (response != null && response.messages.message != null)
            {
                responseData.status = 500;
                responseData.message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
            }

            return responseData;
        }
        public AuthorizeNetResponse DeleteCustomerPaymentProfile(string customerProfileId, string customerPaymentProfileId)
        {
            AuthorizeNetResponse responseData = new AuthorizeNetResponse();
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = environment;
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = Const.ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = Const.ApiTransactionKey,
            };

            //please update the subscriptionId according to your sandbox credentials
            var request = new deleteCustomerPaymentProfileRequest
            {
                customerProfileId = customerProfileId,
                customerPaymentProfileId = customerPaymentProfileId
            };

            //Prepare Request
            var controller = new deleteCustomerPaymentProfileController(request);
            controller.Execute();

            //Send Request to EndPoint
            deleteCustomerPaymentProfileResponse response = controller.GetApiResponse();
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                if (response != null && response.messages.message != null)
                {
                    responseData.status = 200;
                    responseData.message = response.messages.resultCode.ToString();
                }
            }
            else if (response != null && response.messages.message != null)
            {
                responseData.status = 500;
                responseData.message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
            }

            return responseData;
        }
        #endregion

    }
}
