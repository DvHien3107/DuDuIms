using Enrich.Common.Helpers;
using Enrich.Payment.MxMerchant.Config.Enums;
using Enrich.Payment.MxMerchant.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;

namespace Enrich.Payment.MxMerchant.Api
{
    public class MxMerchantFunction : IMxMerchantFunction
    {
        private readonly ConfigFactory _configFactory;
        private readonly IOAuthNew _oAuthNew;
        private readonly IOAuthUtilities _oAuthUtilities;

        public MxMerchantFunction(ConfigFactory configFactory, IOAuthUtilities oAuthUtilities, IOAuthNew oAuthNew)
        {
            _configFactory = configFactory;
            _oAuthUtilities = oAuthUtilities;
            _oAuthNew = oAuthNew;
        }
        public ResponseCardInfo CreateCard(CardInfo card, ref OauthInfo oauth, long CustomerId)
        {
            var qp = new List<QueryParameter>();
            qp.Add(new QueryParameter("id", CustomerId.ToString()));
            qp.Add(new QueryParameter("echo", "true"));
            string createdCard = SendRequest(_configFactory.CreateCard, ref oauth, card, qp);
            return JsonConvert.DeserializeObject<ResponseCardInfo>(createdCard) ?? new ResponseCardInfo();
        }
        public long CreateCustomer(MxMerchantCustomer customer, ref OauthInfo oauth)
        {
            var qp = new List<QueryParameter>();
            qp.Add(new QueryParameter("echo", "true"));
            customer.merchantId = long.Parse(_configFactory.MerchantId);
            string result = SendRequest(_configFactory.CreateCustomer, ref oauth, customer, qp);
            var MxMerchant_cusId = long.Parse(JObject.Parse(result)["id"]?.ToString() ?? "0");
            return MxMerchant_cusId;
        }
        public MxMerchantPaymentResponse MakePayment(MxMerchantPayment payment, ref OauthInfo oauth)
        {
            var qp = new List<QueryParameter>();
            qp.Add(new QueryParameter("echo", "true"));
            payment.merchantId = _configFactory.MerchantId;
            string result = SendRequest(_configFactory.CreatePayment, ref oauth, payment, qp);
            return JsonConvert.DeserializeObject<MxMerchantPaymentResponse>(result) ?? new MxMerchantPaymentResponse();
        }
        public MxMerchantPaymentResponse MakeFullRefund(MxMerchantPayment payment, ref OauthInfo oauth)
        {
            var qp = new List<QueryParameter>();
            qp.Add(new QueryParameter("echo", "true"));
            string result = SendRequest(_configFactory.FullRefund, ref oauth, payment.merchantId, qp);
            return JsonConvert.DeserializeObject<MxMerchantPaymentResponse>(result) ?? new MxMerchantPaymentResponse();
        }
        public MxMerchantRecurringReponse MakeRecuring(MxMerchantRecurring recurring, ref OauthInfo oauth)
        {
            var qp = new List<QueryParameter>();
            qp.Add(new QueryParameter("echo", "true"));
            recurring.contract.merchantId = _configFactory.MerchantId;
            string result = SendRequest(_configFactory.CreateRecurring, ref oauth, recurring, qp);
            return JsonConvert.DeserializeObject<MxMerchantRecurringReponse>(result) ?? new MxMerchantRecurringReponse();
        }
        public MxMerchantRecurringPayment CheckRecurring(string contract_id, ref OauthInfo oauth)
        {
            var qp = new List<QueryParameter>();
            qp.Add(new QueryParameter("id", contract_id));
            string result = SendRequest(_configFactory.CheckRecurring, ref oauth, null, qp);
            var ListPayment = JsonConvert.DeserializeObject<List<MxMerchantRecurringPayment>>(JsonConvert.SerializeObject(JObject.Parse(result)["records"]));
            var lastPayment = ListPayment?.OrderByDescending(p => p.created).FirstOrDefault() ?? new MxMerchantRecurringPayment();
            return lastPayment;
        }
        public string SendRequest(string[] endPointData, ref OauthInfo oauth, object JSON, List<QueryParameter> qp)
        {
            try
            {
                if (oauth == null) oauth = new OauthInfo();
                //get Authorization
                var tokens = new NameValueCollection();
                if (string.IsNullOrEmpty(oauth?.AccessToken))
                {
                    //request new tokens
                    tokens = _oAuthNew.GetAccessToken();
                    oauth.AccessToken = tokens["oauth_token"];
                    oauth.AccessSecret = tokens["oauth_token_secret"];
                    oauth.Updated = true;
                }
                else
                {
                    tokens.Add("oauth_token", oauth.AccessToken);
                    tokens.Add("oauth_token_secret", oauth.AccessSecret);
                }
                var authorization = _oAuthNew.CreateHeaders(endPointData, tokens, qp); // new NewOAuth(endpointData, tokens, qp).createHeaders();
                string result = _oAuthUtilities.SendRequest(endPointData, authorization, JSON, qp);
                return result;
            }
            catch (WebException we)
            {
                var re = (HttpWebResponse)we.Response;
                string JsonRespone = "";
                if (we.Status == WebExceptionStatus.ProtocolError)
                {
                    if (re?.StatusCode == HttpStatusCode.Unauthorized && !oauth.Updated)
                    {
                        oauth = new OauthInfo();
                        return SendRequest(endPointData, ref oauth, JSON, qp);
                    }
                    using (var stream = we.Response?.GetResponseStream())
                    using (var reader = new StreamReader(stream))
                    {
                        JsonRespone = reader.ReadToEnd();
                        ExResponse exRes = new ExResponse();
                        try
                        {
                            exRes = JsonConvert.DeserializeObject<ExResponse>(JsonRespone) ?? new ExResponse();
                        }
                        catch
                        {
                            throw new Exception(JsonRespone);
                        }
                        if (exRes?.errorCode == MxResponeCode.ValidationError.ToString())
                        {
                            throw new Exception(exRes.details.FirstOrDefault());
                        }
                        if (exRes?.errorCode == MxResponeCode.ContactCustomerSupport.ToString())
                        {
                            throw new Exception(exRes.message);
                        }
                        else
                        {
                            throw new Exception(JsonRespone);
                        }
                    }
                }
                else
                {
                    JsonRespone = JsonConvert.SerializeObject(we);
                }
                if (JSON is CardInfo)
                {
                    var card_info = (CardInfo)JSON;
                    if (!string.IsNullOrEmpty(card_info.number)) card_info.number = Regex.Replace(card_info.number, "[0-9]", "*");
                    if (!string.IsNullOrEmpty(card_info.cvv)) card_info.cvv = Regex.Replace(card_info.cvv, "[0-9]", "*");
                    if (!string.IsNullOrEmpty(card_info.token)) card_info.token = Regex.Replace(card_info.token, "[A-Z0-9]", "*");
                }
                var log = new IMSLog
                {
                    RequestUrl = endPointData[0] + (qp != null ? ("?" + _oAuthUtilities.NormalizeRequestParameters(qp)) : ""),
                    RequestMethod = endPointData[1],
                    JsonRequest = JsonConvert.SerializeObject(JSON),
                    JsonRespone = JsonRespone,
                    Success = false,
                    StatusCode = ConvertHelper.GetInt(re != null ? re?.StatusCode : we.Status),
                    Description = (we.Status == WebExceptionStatus.ProtocolError) ? Constants.ResponeError : Constants.RequestError,
                    CreateOn = DateTime.UtcNow,
                    CreateBy = Constants.SystemName,
                    Url = Constants.Url,
                };
                we.Data.Add("Log", JsonConvert.SerializeObject(log));
                throw we;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
