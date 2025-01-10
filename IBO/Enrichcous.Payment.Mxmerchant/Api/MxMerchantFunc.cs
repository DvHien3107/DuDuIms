using Enrichcous.Payment.Mxmerchant.Models;
using Enrichcous.Payment.Mxmerchant.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using static Enrichcous.Payment.Mxmerchant.Api.OAuth_Utilities;

namespace Enrichcous.Payment.Mxmerchant.Api
{
    public class MxMerchantFunc
    {
        //public card_info_response CreateCard(card_info card, ref OauthInfo oauth, long CustomerId)
        //{
        //    try
        //    {
        //        var qp = new List<QueryParameter>();
        //        qp.Add(new QueryParameter("id", CustomerId.ToString()));
        //        qp.Add(new QueryParameter("echo", "true"));
        //        string createdCard = SendRequest(ConfigFactory.Create_card, ref oauth, card, qp);
        //        return JsonConvert.DeserializeObject<card_info_response>(createdCard);
        //    }
        //    catch (Exception e) { throw e; }
        //}
        //public long CreateCustomer(MxMerchant_Customer customer, ref OauthInfo oauth)
        //{
        //    var qp = new List<QueryParameter>();
        //    qp.Add(new QueryParameter("echo", "true"));
        //    customer.merchantId = long.Parse(new ConfigFactory().merchantId);
        //    string result = SendRequest(ConfigFactory.Create_Customer, ref oauth, customer, qp);
        //    var MxMerchant_cusId = long.Parse(JObject.Parse(result)["id"].ToString() ?? "0");
        //    return MxMerchant_cusId;
        //}
        //public MxMerchant_payment_response MakePayment(MxMerchant_payment payment, ref OauthInfo oauth)
        //{
        //    var qp = new List<QueryParameter>();
        //    qp.Add(new QueryParameter("echo", "true"));
        //    payment.merchantId = new ConfigFactory().merchantId;
        //    string result = SendRequest(ConfigFactory.Create_payment, ref oauth, payment, qp);
        //    return JsonConvert.DeserializeObject<MxMerchant_payment_response>(result);
        //}
        //public MxMerchant_payment_response MakeFullRefund(MxMerchant_payment payment, ref OauthInfo oauth)
        //{
        //    var qp = new List<QueryParameter>();
        //    qp.Add(new QueryParameter("echo", "true"));
        //    string result = SendRequest(ConfigFactory.Full_Refund, ref oauth, payment.merchantId, qp);
        //    return JsonConvert.DeserializeObject<MxMerchant_payment_response>(result);
        //}

        //public MxMerchant_recurring_reponse MakeRecuring(MxMerchant_recurring recurring, ref OauthInfo oauth)
        //{
        //    var qp = new List<QueryParameter>();
        //    qp.Add(new QueryParameter("echo", "true"));
        //    recurring.contract.merchantId = new ConfigFactory().merchantId;
        //    string result = SendRequest(ConfigFactory.Create_Recurring, ref oauth, recurring, qp);
        //    return JsonConvert.DeserializeObject<MxMerchant_recurring_reponse>(result);
        //}
        //public MxMerchant_recurring_payment CheckRecurring(string contract_id, ref OauthInfo oauth)
        //{
        //    var qp = new List<QueryParameter>();
        //    qp.Add(new QueryParameter("id", contract_id));
        //    string result = SendRequest(ConfigFactory.Check_Recurring, ref oauth, null, qp);
        //    var ListPayment = JsonConvert.DeserializeObject<List<MxMerchant_recurring_payment>>(JsonConvert.SerializeObject(JObject.Parse(result)["records"]));
        //    var lastPayment = ListPayment.OrderByDescending(p => p.created).FirstOrDefault();
        //    return lastPayment;
        //}

        //public string SendRequest(string[] endpointData, ref OauthInfo oauth, object JSON = null, List<QueryParameter> qp = null)
        //{
        //    try
        //    {
        //        //get Authorization
        //        var tokens = new NameValueCollection();
        //        if (string.IsNullOrEmpty(oauth?.AccessToken))
        //        {
        //            //request new tokens
        //            tokens = new RetrieveTokens().getAccessToken();
        //            oauth.AccessToken = tokens["oauth_token"];
        //            oauth.AccessSecret = tokens["oauth_token_secret"];
        //            oauth.Updated = true;
        //        }
        //        else
        //        {
        //            tokens.Add("oauth_token", oauth.AccessToken);
        //            tokens.Add("oauth_token_secret", oauth.AccessSecret);
        //        }
        //        var Authorization = new newOAuth(endpointData, tokens, qp).createHeaders();
        //        string result = new OAuth_Utilities().sendRequest(endpointData, Authorization, JSON, qp);
        //        return result;
        //    }
        //    catch (WebException we)
        //    {
        //        var re = (HttpWebResponse)we.Response;
        //        string JsonRespone = "";
        //        if (we.Status == WebExceptionStatus.ProtocolError)
        //        {

        //            if (re?.StatusCode == HttpStatusCode.Unauthorized && !oauth.Updated)
        //            {
        //                oauth = new OauthInfo();
        //                return SendRequest(endpointData, ref oauth, JSON, qp);
        //            }
        //            using (var stream = we.Response?.GetResponseStream())
        //            using (var reader = new StreamReader(stream))
        //            {
        //                JsonRespone = reader.ReadToEnd();
        //                ExResponse exRes = null;
        //                try
        //                {
        //                    exRes = JsonConvert.DeserializeObject<ExResponse>(JsonRespone);
        //                }
        //                catch
        //                {
        //                    throw new Exception(JsonRespone);
        //                }
        //                if (exRes.errorCode == "ValidationError")
        //                {
        //                    throw new Exception(exRes.details.FirstOrDefault());
        //                }
        //                else
        //                {
        //                    throw new Exception(JsonRespone);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            JsonRespone = JsonConvert.SerializeObject(we);
        //        }
        //        if (JSON is card_info)
        //        {
        //            var card_info = (card_info)JSON;
        //            if (!string.IsNullOrEmpty(card_info.number)) card_info.number = Regex.Replace(card_info.number, "[0-9]", "*");
        //            if (!string.IsNullOrEmpty(card_info.cvv)) card_info.cvv = Regex.Replace(card_info.cvv, "[0-9]", "*");
        //            if (!string.IsNullOrEmpty(card_info.token)) card_info.token = Regex.Replace(card_info.token, "[A-Z0-9]", "*");
        //        }
        //        var log = new IMSLog
        //        {
        //            RequestUrl = endpointData[0] + (qp != null ? ("?" + newOAuth.RequestNormalizer.NormalizeRequestParameters(qp)) : ""),
        //            RequestMethod = endpointData[1],
        //            JsonRequest = JsonConvert.SerializeObject(JSON),
        //            JsonRespone = JsonRespone,
        //            Success = false,
        //            StatusCode = re != null ? (int)re?.StatusCode : (int)we.Status,

        //            Description = (we.Status == WebExceptionStatus.ProtocolError) ? "MxMerchant Api Respone error" : "MxMerchant Api Request fail",
        //            CreateOn = DateTime.UtcNow,
        //            CreateBy = "Payment System",
        //            Url = "Enrichcous.Payment.Mxmerchant.Api/PaymentFunc/SendRequest",
        //        };
        //        we.Data.Add("Log", JsonConvert.SerializeObject(log));
        //        throw we;

        //    }
        //    catch (Exception e)
        //    {
        //        throw e;
        //    }

        //}
        //public partial class IMSLog
        //{
        //    public string Url { get; set; }
        //    public string RequestUrl { get; set; }
        //    public string RequestMethod { get; set; }
        //    public Nullable<int> StatusCode { get; set; }
        //    public Nullable<bool> Success { get; set; }
        //    public string JsonRequest { get; set; }
        //    public string JsonRespone { get; set; }
        //    public string Description { get; set; }
        //    public DateTime CreateOn { get; internal set; }
        //    public string CreateBy { get; internal set; }
        //}
        //public class ExResponse
        //{
        //    public string errorCode { get; set; }
        //    public string message { get; set; }
        //    public List<string> details { get; set; }
        //    public string responseCode { get; set; }
        //}
    }
}
