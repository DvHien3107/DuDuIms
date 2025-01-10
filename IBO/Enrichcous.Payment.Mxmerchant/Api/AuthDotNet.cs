using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Enrichcous.Payment.Mxmerchant.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Enrichcous.Payment.Mxmerchant.Api
{
    public static class AuthDotNet
    {
        //private readonly string ApiLoginID = "24qhtqhMW8hB";
        //private readonly string ApiTransactionKey = "56dJebZ5J64aGP8W";
        //private readonly AuthorizeNet.Environment Environment = AuthorizeNet.Environment.SANDBOX;

        #region Payment Authorize
        public static ADotNetResponse ChargeCustomerProfile(string customerProfileId, string customerPaymentProfileId, decimal Amount, AuthDotNetEnv Env )
        {
            Console.WriteLine("Charge Customer Profile");
            ADotNetResponse respon = new ADotNetResponse();
            respon.status = 404;
            respon.message = "nothing";

            string ApiLoginID = Env.ApiLoginID;
            string ApiTransactionKey = Env.ApiTransactionKey;
            AuthorizeNet.Environment Environment = AuthorizeNet.Environment.PRODUCTION;
            if (Env.IsSanbox)
            {
                Environment = AuthorizeNet.Environment.SANDBOX;
            }


            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = Environment;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ApiTransactionKey
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
                        respon.status = 200;
                        respon.message = "Successfully created transaction with Transaction ID: " + response.transactionResponse.transId;
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        respon.JsonResult = jsonString;
                        respon.ResponId = response.transactionResponse.transId;
                    }
                    else
                    {
                        respon.status = 500;
                        respon.message = "Failed Transaction.";
                        if (response.transactionResponse.errors != null)
                        {
                            respon.message = response.transactionResponse.errors[0].errorText;
                        }
                    }
                }
                else
                {
                    respon.status = 500;
                    respon.message = "Failed Transaction.";
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        respon.message = response.transactionResponse.errors[0].errorText;
                    }
                    else
                    {
                        respon.message = response.messages.message[0].text;
                    }
                }
            }
            else
            {
                respon.status = 500;
                respon.message = "Null Response.";
            }

            return respon;
        }
        #endregion

        #region Customer Profiles Authorize
        public static ADotNetResponse CreateCustomerProfile(string emailId, CustomerProfile posCustomerProfile, AuthDotNetEnv Env)
        {

            string ApiLoginID = Env.ApiLoginID;
            string ApiTransactionKey = Env.ApiTransactionKey;
            AuthorizeNet.Environment Environment = AuthorizeNet.Environment.PRODUCTION;
            if (Env.IsSanbox)
            {
                Environment = AuthorizeNet.Environment.SANDBOX;
            }


            ADotNetResponse respon = new ADotNetResponse();
            respon.status = 404;
            respon.message = "nothing";
            // set whether to use the sandbox environment, or production enviornment
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = Environment;
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ApiTransactionKey,
            };

            var billTo = new customerAddressType
            {
                firstName = posCustomerProfile.firstName,
                lastName = posCustomerProfile.lastName,
                address = posCustomerProfile.address,
                city = posCustomerProfile.city,
                state = posCustomerProfile.state,
                zip = posCustomerProfile.zip
            };
            List<customerAddressType> addressInfoList = new List<customerAddressType>();
            List<customerPaymentProfileType> paymentProfileList = new List<customerPaymentProfileType>();
            customerPaymentProfileType ccPaymentProfile = new customerPaymentProfileType();
            ccPaymentProfile.billTo = billTo;
            //paymentProfileList.Add(ccPaymentProfile);
            addressInfoList.Add(billTo);
            customerProfileType customerProfile = new customerProfileType();
            customerProfile.merchantCustomerId = posCustomerProfile.posCustomerId;
            customerProfile.email = emailId;
            customerProfile.paymentProfiles = paymentProfileList.ToArray();
            customerProfile.shipToList = addressInfoList.ToArray();


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
                        respon.status = 200;
                        respon.message = "Success!";
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        respon.JsonResult = jsonString;
                        respon.ResponId = response.customerProfileId;
                    }
                }
                else
                {
                    respon.status = 500;
                    respon.message = response.messages.message[0].text;
                    if (respon.message.IndexOf("A duplicate record with ID")!=-1)
                    {
                        respon.status = 200; 
                        respon.ResponId = respon.message.Replace("A duplicate record with ID ","").Replace(" already exists.", "");
                        respon.message = "Success!";
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        respon.JsonResult = jsonString;
                    }
                }
            }
            else
            {
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    respon.status = 500;
                    respon.message = response.messages.message[0].text;
                }
                else
                {
                    respon.status = 500;
                    respon.message = "Null Response.";
                }
            }

            return respon;
        }
        public static ADotNetResponse CreateCustomerPaymentProfile(string customerProfileId, AuthCardInfo newCard, AuthDotNetEnv Env)
        {

            string ApiLoginID = Env.ApiLoginID;
            string ApiTransactionKey = Env.ApiTransactionKey;
            AuthorizeNet.Environment Environment = AuthorizeNet.Environment.PRODUCTION;
            if (Env.IsSanbox)
            {
                Environment = AuthorizeNet.Environment.SANDBOX;
            }

            ADotNetResponse respon = new ADotNetResponse();
            respon.status = 404;
            respon.message = "nothing";
            // set whether to use the sandbox environment, or production enviornment
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = Environment;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ApiTransactionKey,
            };


            var creditCard = new creditCardType
            {
                cardNumber = newCard.cardNumber,
                expirationDate = newCard.expirationDate,
                cardCode = newCard.cardCode,
            };

            paymentType echeck = new paymentType { Item = creditCard };

            customerPaymentProfileType echeckPaymentProfile = new customerPaymentProfileType();
            echeckPaymentProfile.payment = echeck;

            var request = new createCustomerPaymentProfileRequest
            {
                customerProfileId = customerProfileId,
                paymentProfile = echeckPaymentProfile,
                validationMode = validationModeEnum.none
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
                        respon.status = 200;
                        respon.message = "Success!";
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        respon.JsonResult = jsonString;
                        respon.ResponId = response.customerPaymentProfileId;
                    }
                }
                else
                {
                    respon.status = 500;
                    respon.message = response.messages.message[0].text;
                    if (response.messages.message[0].code == "E00039")
                    {
                        respon.status = 200;
                        respon.message = "Success!";
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        respon.JsonResult = jsonString;
                        respon.ResponId = response.customerPaymentProfileId;
                        //respon.message = "Duplicate Payment Profile ID: " + response.customerPaymentProfileId;
                    }
                }
            }
            else
            {
                respon.status = 500;
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    respon.message = controller.GetErrorResponse().messages.message[0].text;
                }
                else
                {
                    respon.message = "Null Response.";
                }
            }

            return respon;

        }
        public static ADotNetResponse CreateCustomerProfileFromTransaction(string customerEmail, string transactionId, CustomerProfile posCustomerProfile, AuthDotNetEnv Env)
        {

            string ApiLoginID = Env.ApiLoginID;
            string ApiTransactionKey = Env.ApiTransactionKey;
            AuthorizeNet.Environment Environment = AuthorizeNet.Environment.PRODUCTION;
            if (Env.IsSanbox)
            {
                Environment = AuthorizeNet.Environment.SANDBOX;
            }

            ADotNetResponse respon = new ADotNetResponse();
            respon.status = 404;
            respon.message = "nothing";
            // set whether to use the sandbox environment, or production enviornment
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = Environment;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ApiTransactionKey,
            };
            var customerProfile = new customerProfileBaseType
            {
                merchantCustomerId = posCustomerProfile.posCustomerId,
                email = customerEmail,
                description = "This is a customer profile create from CreateCustomerProfileFromTransaction"
            };

            var request = new createCustomerProfileFromTransactionRequest
            {
                transId = transactionId,
                // You can either specify the customer information in form of customerProfileBaseType object
                customer = customerProfile
                //  OR   
                // You can just provide the customer Profile ID
                // customerProfileId = "123343"                
            };


            var controller = new createCustomerProfileFromTransactionController(request);
            controller.Execute();

            createCustomerProfileResponse response = controller.GetApiResponse();

            // validate response 
            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.messages.message != null)
                    {
                        respon.status = 200;
                        respon.message = "Success!";
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        respon.JsonResult = jsonString;
                    }
                }
                else
                {
                    respon.status = 500;
                    respon.message = response.messages.message[0].text;
                }
            }
            else
            {
                respon.status = 500;
                if (controller.GetErrorResponse().messages.message.Length > 0)
                {
                    respon.message = controller.GetErrorResponse().messages.message[0].text;
                }
                else
                {
                    respon.message = "Null Response.";
                }
            }

            return respon;

        }
        public static ADotNetResponse ChargeCreditCard(decimal amount, CustomerProfile posCustomerProfile, AuthCardInfo newCard, AuthDotNetEnv Env)
        {
            ADotNetResponse respon = new ADotNetResponse();
            respon.status = 404;
            respon.message = "nothing";
            string ApiLoginID = Env.ApiLoginID;
            string ApiTransactionKey = Env.ApiTransactionKey;
            AuthorizeNet.Environment Environment = AuthorizeNet.Environment.PRODUCTION;
            if (Env.IsSanbox)
            {
                Environment = AuthorizeNet.Environment.SANDBOX;
            }


            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = Environment;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ApiTransactionKey,
            };

            var creditCard = new creditCardType
            {
                cardNumber = newCard.cardNumber,
                expirationDate = newCard.expirationDate,
                cardCode = newCard.cardCode
            };

            var billingAddress = new customerAddressType
            {
                firstName = posCustomerProfile.firstName,
                lastName = posCustomerProfile.lastName,
                address = posCustomerProfile.address,
                city = posCustomerProfile.city,
                zip = posCustomerProfile.zip
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),    // charge the card

                amount = amount,
                payment = paymentType,
                billTo = billingAddress
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
                        respon.status = 200;
                        respon.message = "Successfully created transaction with Transaction ID: " + response.transactionResponse.transId;
                        var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                        respon.JsonResult = jsonString;
                    }
                    else
                    {
                        respon.status = 500;
                        respon.message = "Failed Transaction.";
                        if (response.transactionResponse.errors != null)
                        {
                            respon.message = response.transactionResponse.errors[0].errorText;
                        }
                    }
                }
                else
                {
                    respon.status = 500;
                    respon.message = "Failed Transaction.";
                    if (response.transactionResponse != null && response.transactionResponse.errors != null)
                    {
                        respon.message = response.transactionResponse.errors[0].errorText;
                    }
                    else
                    {
                        respon.message = response.messages.message[0].text;
                    }
                }
            }
            else
            {
                respon.status = 500;
                respon.message = "Null Response.";
            }

            return respon;
        }

        public static ADotNetResponse GetCustomerPaymentProfile(AuthDotNetEnv Env, string customerProfileId, string customerPaymentProfileId)
        {
            Console.WriteLine("Get Customer Payment Profile sample");
            string ApiLoginID = Env.ApiLoginID;
            string ApiTransactionKey = Env.ApiTransactionKey;
            AuthorizeNet.Environment Environment = AuthorizeNet.Environment.PRODUCTION;
            if (Env.IsSanbox)
            {
                Environment = AuthorizeNet.Environment.SANDBOX;
            }
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = Environment;
            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                name = ApiLoginID,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = ApiTransactionKey,
            };

            var request = new getCustomerPaymentProfileRequest();
            request.customerProfileId = customerProfileId;
            request.customerPaymentProfileId = customerPaymentProfileId;


            // instantiate the controller that will call the service
            var controller = new getCustomerPaymentProfileController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();
            ADotNetResponse respon = new ADotNetResponse();
            respon.status = 404;
            respon.message = "nothing";
            // Console.WriteLine("Customer Payment Profile Last 4: " + (response.paymentProfile.payment.Item as creditCardMaskedType).cardNumber);
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                //Console.WriteLine(response.messages.message[0].text);
                //Console.WriteLine("Customer Payment Profile Id: " + response.paymentProfile.customerPaymentProfileId);
                //if (response.paymentProfile.payment.Item is creditCardMaskedType)
                //{
                //    Console.WriteLine("Customer Payment Profile Last 4: " + (response.paymentProfile.payment.Item as creditCardMaskedType).cardNumber);
                //    Console.WriteLine("Customer Payment Profile Expiration Date: " + (response.paymentProfile.payment.Item as creditCardMaskedType).expirationDate);
                //}
                respon.status = 200;
                respon.message = "Customer Payment Profile Id: " + response.paymentProfile.customerPaymentProfileId;
                var jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                respon.JsonResult = jsonString;
                respon.Respon = response;
            }
            else if (response != null)
            {
                respon.status = 500;
                respon.message = "Error: " + response.messages.message[0].code + "  " + response.messages.message[0].text;
            }
            else
            {
                respon.status = 500;
                respon.message = "Null Response.";
            }

            return respon;
        }
        #endregion
    }
}
