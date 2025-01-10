using Enrich.Payment.MxMerchant.Config.Enums;
using Enrich.Payment.MxMerchant.Models;
using Enrich.Payment.MxMerchant.Utils;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Enrich.Payment.MxMerchant.Api
{
    public class OAuthUtilities : IOAuthUtilities
    {
        private readonly ConfigFactory _configFactory;
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new OrderedContractResolver() };

        public OAuthUtilities(ConfigFactory configFactory)
        {
            _configFactory = configFactory;
        }

        public string UrlEncode(string value)
        {
            string unreservedChars = Constants.Character.Unreserved;
            StringBuilder result = new StringBuilder();
            foreach (char symbol in value)
            {
                if (unreservedChars.IndexOf(symbol) != -1)
                {
                    result.Append(symbol);
                }
                else
                {
                    result.Append('%' + String.Format("{0:X2}", (int)symbol));
                }
            }
            return result.ToString();
        }
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash)
        {
            return ComputeHash(hash, signatureBase);
        }
        public byte[] KeyBuilder(NameValueCollection tokens)
        {
            string key = string.Format("{0}&", UrlEncode(_configFactory.ConsumerSecret));
            if (tokens != null)
            {
                key += UrlEncode(tokens["oauth_token_secret"]);
            }
            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            return keyBytes;
        }
        public string CreateHeader(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);

                if (i < parameters.Count - 1)
                {
                    sb.Append(",");
                }
            }
            return sb.ToString();
        }
        public string ComputeHash(HashAlgorithm hashAlgorithm, string data)
        {
            if (hashAlgorithm == null)
            {
                throw new ArgumentNullException("hashAlgorithm");
            }

            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentNullException("data");
            }

            byte[] dataBuffer = System.Text.Encoding.ASCII.GetBytes(data);
            byte[] hashBytes = hashAlgorithm.ComputeHash(dataBuffer);

            return Convert.ToBase64String(hashBytes);
        }
        public string NormalizeRequestParameters(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parameters.Count; i++)
            {
                var p = parameters[i];
                sb.AppendFormat("{0}={1}", p.Name, p.Value);
                if (i < parameters.Count - 1)
                {
                    sb.Append("&");
                }
            }
            return sb.ToString();
        }
        public string SendRequest(string[] endpointData, string headers, object JSON = null, List<QueryParameter> qp = null)
        {
            string Url = endpointData[0]; string method = endpointData[1];
            HttpWebRequest r;
            if (qp != null)
            {
                qp.Sort(new QueryParameterComparer());
                Url = Url + "?" + NormalizeRequestParameters(qp);
                r = (HttpWebRequest)WebRequest.Create(Url);
            }
            else
            {
                r = (HttpWebRequest)WebRequest.Create(Url);
            }

            string responseFromServer = "";
            r = (HttpWebRequest)WebRequest.Create(Url);
            r.Method = method;
            r.Headers.Add("Authorization", headers);
            if (JSON != null)
            {
                r.ContentType = "application/json";
                string actualJson = JsonConvert.SerializeObject(JSON, Formatting.None, settings);
                using (var streamWriter = new StreamWriter(r.GetRequestStream()))
                {
                    streamWriter.Write(actualJson);
                }
                var httpResponse = (HttpWebResponse)r.GetResponse();
                var reader = new StreamReader(httpResponse.GetResponseStream());
                responseFromServer = reader.ReadToEnd();
            }
            else
            {
                if (endpointData.GetValue(1)?.ToString() == "POST")
                {
                    r.ContentLength = 0;
                }
                using (WebResponse response = r.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    {
                        StreamReader reader = new StreamReader(stream);
                        responseFromServer = reader.ReadToEnd();
                    }
                }
            }
            return responseFromServer;
        }
    }
}
