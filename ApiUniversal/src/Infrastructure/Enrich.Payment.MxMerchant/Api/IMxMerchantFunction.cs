using Enrich.Payment.MxMerchant.Models;

namespace Enrich.Payment.MxMerchant.Api
{
    public interface IMxMerchantFunction
    {
        /// <summary>
        /// Create new credit card
        /// </summary>
        /// <param name="card"></param>
        /// <param name="oauth"></param>
        /// <param name="CustomerId"></param>
        /// <returns></returns>
        public ResponseCardInfo CreateCard(CardInfo card, ref OauthInfo oauth, long CustomerId);
        /// <summary>
        /// Create customer
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="oauth"></param>
        /// <returns></returns>
        public long CreateCustomer(MxMerchantCustomer customer, ref OauthInfo oauth);
        /// <summary>
        /// Make new transaction payment
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="oauth"></param>
        /// <returns></returns>
        public MxMerchantPaymentResponse MakePayment(MxMerchantPayment payment, ref OauthInfo oauth);
        /// <summary>
        /// Make full refund transaction
        /// </summary>
        /// <param name="payment"></param>
        /// <param name="oauth"></param>
        /// <returns></returns>
        public MxMerchantPaymentResponse MakeFullRefund(MxMerchantPayment payment, ref OauthInfo oauth);
        /// <summary>
        /// Make recurring
        /// </summary>
        /// <param name="recurring"></param>
        /// <param name="oauth"></param>
        /// <returns></returns>
        public MxMerchantRecurringReponse MakeRecuring(MxMerchantRecurring recurring, ref OauthInfo oauth);
        /// <summary>
        /// Check recurring
        /// </summary>
        /// <param name="contract_id"></param>
        /// <param name="oauth"></param>
        /// <returns></returns>
        public MxMerchantRecurringPayment CheckRecurring(string contract_id, ref OauthInfo oauth);
        /// <summary>
        /// Send request to api MxMerchant 
        /// </summary>
        /// <param name="endpointData"></param>
        /// <param name="oauth"></param>
        /// <param name="JSON"></param>
        /// <param name="qp"></param>
        /// <returns></returns>
        public string SendRequest(string[] endpointData, ref OauthInfo oauth, object JSON, List<QueryParameter> qp);
    }
}
