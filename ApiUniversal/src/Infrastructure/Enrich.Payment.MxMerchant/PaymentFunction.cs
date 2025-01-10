using Enrich.Payment.MxMerchant.Api;
using Enrich.Core.Utils;
using Enrich.Payment.MxMerchant.Models;
using Enrich.Dto.Base;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.Payment.MxMerchant.Config.Enums;

namespace Enrich.Payment.MxMerchant
{
    public class PaymentFunction : IPaymentFunction
    {
        private readonly ISystemConfigurationRepository _repositorySystem;
        private readonly IMxMerchantFunction _mxMerchantFunction;

        public PaymentFunction(ISystemConfigurationRepository repositorySystem, IMxMerchantFunction mxMerchantFunction)
        {
            _repositorySystem = repositorySystem;
            _mxMerchantFunction = mxMerchantFunction;
        }

        public MxMerchantResponseDto MakePayment(MxMerchantDto dataRequest)
        {
            var webinfo = _repositorySystem.GetSystemConfiguration();
            var auth = new OauthInfo
            {
                AccessToken = webinfo.MxMerchant_AccessToken,
                AccessSecret = webinfo.MxMerchant_AccessSecret
            };
            var paymentInfo = new MxMerchantPayment
            {
                amount = (double)(dataRequest.Amount),
                cardAccount = new MxMerchantPayment.CardAccount { token = dataRequest.MxMerchantToken },
                customer = new MxMerchantPayment.Customer { id = dataRequest.MxMerchantId },
            };
            var respone = _mxMerchantFunction.MakePayment(paymentInfo, ref auth);
            if (auth.Updated) { SaveMxMerchantTokens(auth); }
            return new MxMerchantResponseDto
            {
                id = respone.id,
                authMessage = respone.message(),
                paymentToken = respone.paymentToken,
                status = respone.status
            };
        }
        public MxMerchantResponseDto MakePaymentACH(MxMerchantDto dataRequest)
        {
            var webinfo = _repositorySystem.GetSystemConfiguration();
            var auth = new OauthInfo
            {
                AccessToken = webinfo.MxMerchant_AccessToken,
                AccessSecret = webinfo.MxMerchant_AccessSecret
            };
            var paymentInfo = new MxMerchantPayment
            {
                amount = (double)(dataRequest.Amount),
                tenderType = TenderType.ACH.ToString(),
                bankaccount = new MxMerchantPayment.BankAccount
                {
                    accountNumber = dataRequest.AccountNumber,
                    routingNumber = dataRequest.RoutingNumber,
                    name = dataRequest.OwnerName, //tên chủ tài khoản
                    alias = dataRequest.BankName,
                    type = BankType.Checking.ToString() // Checking or Savings
                }
            };
            var respone = _mxMerchantFunction.MakePayment(paymentInfo, ref auth);
            if (auth.Updated) { SaveMxMerchantTokens(auth); }
            return new MxMerchantResponseDto
            {
                id = respone.id,
                authMessage = respone.message(),
                paymentToken = respone.paymentToken,
                status = respone.status
            };
        }
        public void SaveMxMerchantTokens(OauthInfo auth)
        {
            var webinfo = _repositorySystem.GetSystemConfiguration();
            webinfo.MxMerchant_AccessToken = auth.AccessToken;
            webinfo.MxMerchant_AccessSecret = auth.AccessSecret;
            _repositorySystem.Update(webinfo);
        }
    }
}