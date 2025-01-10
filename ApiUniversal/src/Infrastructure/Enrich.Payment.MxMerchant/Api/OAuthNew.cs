using Enrich.Payment.MxMerchant.Models;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Enrich.Payment.MxMerchant.Api
{
    public class OAuthNew : IOAuthNew
    {
        private readonly ConfigFactory _configFactory;
        private readonly IOAuthUtilities _oAuthUtilities;
        private string[] _endpointData;
        private IList<QueryParameter> _queryParameters;
        private NameValueCollection _tokens;
        private readonly string _startDate;
        private readonly string _endDate;
        private readonly string _datePostedFilter;
        private List<QueryParameter> _parameters;
        public OAuthNew(ConfigFactory configFactory, IOAuthUtilities oAuthUtilities)
        {
            _configFactory = configFactory;
            _oAuthUtilities = oAuthUtilities;
        }

        public List<QueryParameter> SetupParams()
        {
            List<QueryParameter> parameters = new List<QueryParameter>();
            if (_queryParameters != null && _queryParameters.Count > 0)
            {
                parameters.AddRange(_queryParameters);
            }
            if (_startDate != null)
            {
                if (_datePostedFilter != null)
                {
                    parameters.Add(new QueryParameter("datePostedFilter", "true"));
                }
                parameters.Add(new QueryParameter("endDate", _endDate));
                parameters.Add(new QueryParameter("startDate", _startDate));
            }
            parameters.Add(new QueryParameter("oauth_version", "1.0"));
            parameters.Add(new QueryParameter("oauth_nonce", GenerateNonce()));
            parameters.Add(new QueryParameter("oauth_timestamp", GetTimeStamp()));

            if (_tokens != null)
            {
                parameters.Add(new QueryParameter("oauth_token", _tokens["oauth_token"]));
            }

            parameters.Add(new QueryParameter("oauth_signature_method", "HMAC-SHA1"));
            parameters.Add(new QueryParameter("oauth_consumer_key", _configFactory.ConsumerKey));
            return parameters;
        }
        public string GenerateSignatureBase(string[] endPointData)
        {
            _parameters = SetupParams();
            if (endPointData[0].Contains("?"))
            {
                foreach (var p in endPointData[0].Split('?')[1].Split('&'))
                {
                    if (p.Contains("="))
                    {
                        var _p = p.Split('=');
                        _parameters.Add(new QueryParameter(_p[0], _p[1]));
                    }
                }
            };
            _parameters.Sort(new QueryParameterComparer());

            StringBuilder signatureBase = new StringBuilder();
            signatureBase.AppendFormat("{0}&", endPointData[1].ToUpper());
            signatureBase.AppendFormat("{0}&", _oAuthUtilities.UrlEncode(endPointData[0].Split('?')[0]));
            signatureBase.AppendFormat("{0}", _oAuthUtilities.UrlEncode(_oAuthUtilities.NormalizeRequestParameters(_parameters)));
            return signatureBase.ToString();
        }
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return _oAuthUtilities.ComputeHash(hash, signatureBase);
        }
        public string GenerateSignature(string[] endpointData)
        {
            string baseString = GenerateSignatureBase(endpointData);
            HMACSHA1 hmacsha1 = new HMACSHA1();
            hmacsha1.Key = _oAuthUtilities.KeyBuilder(_tokens);
            string signature = GenerateSignatureUsingHash(baseString, hmacsha1);
            return signature;
        }
        public void RecaptureParams()
        {
            string signature = GenerateSignature(_endpointData);
            _parameters.Add(new QueryParameter("oauth_signature", _oAuthUtilities.UrlEncode(signature)));
        }
        public string CreateHeaders(string[] endpointData, NameValueCollection tokens = null, IList<QueryParameter> queryParameters = null)
        {
            _endpointData = endpointData;
            _tokens = tokens;
            _queryParameters = queryParameters;

            RecaptureParams();
            _parameters.Sort(new QueryParameterComparer());
            StringBuilder headerString = new StringBuilder();
            headerString.AppendFormat("{0}", (_oAuthUtilities.CreateHeader(_parameters)));
            string h = "OAuth " + headerString;
            return h;
        }
        public string GetTimeStamp()
        {
            int stamp = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
            string sendTime = stamp.ToString();
            return sendTime;
        }
        public string GenerateNonce()
        {
            Random random = new Random();
            return random.Next(123400, 9999999).ToString();
        }
        public NameValueCollection GetAccessToken()
        {
            string headers = CreateHeaders(_configFactory.EndPointRequestToken);
            string requestTokenResponse = _oAuthUtilities.SendRequest(_configFactory.EndPointRequestToken, headers);
            NameValueCollection rt = HttpUtility.ParseQueryString(requestTokenResponse);
            headers = CreateHeaders(_configFactory.EndPointRequestToken, rt);
            string access_Token_Response = _oAuthUtilities.SendRequest(_configFactory.EndPointRequestToken, headers);
            NameValueCollection at = HttpUtility.ParseQueryString(requestTokenResponse);
            return at;
        }
    }
}