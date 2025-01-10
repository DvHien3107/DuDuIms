using Enrichcous.Payment.Mxmerchant.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Enrichcous.Payment.Mxmerchant.Api
{
    public class OAuth_Utilities
    {
        public string UrlEncode(string value)
        {
            string unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
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

        public byte[] keyBuilder(NameValueCollection tokens)
        {
            ConfigFactory ConfigFactory = new ConfigFactory();
            SHA1 sha1 = new SHA1CryptoServiceProvider();

            string key = string.Format("{0}&", UrlEncode(ConfigFactory.consumerSecret));
            if (tokens != null)
            {
                key += UrlEncode(tokens["oauth_token_secret"]);
            }

            byte[] keyBytes = Encoding.ASCII.GetBytes(key);
            return keyBytes;
        }

        public string createHeader(IList<QueryParameter> parameters)
        {
            StringBuilder sb = new StringBuilder();
            OAuth_Utilities.QueryParameter p = null;
            for (int i = 0; i < parameters.Count; i++)
            {
                p = parameters[i];
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

        public class QueryParameter
        {
            private string name = null;
            private string value = null;

            public QueryParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get { return name; }
            }

            public string Value
            {
                get { return value; }
            }
        }

        public class QueryParameterComparer : IComparer<QueryParameter>
        {

            #region IComparer<QueryParameter> Members

            public int Compare(QueryParameter x, QueryParameter y)
            {
                if (x.Name == y.Name)
                {
                    return string.Compare(x.Value, y.Value);
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            }

            #endregion
        }

        public string sendRequest(string[] endpointData, string headers, object JSON = null, List<QueryParameter> qp = null)
        {
            string Url = endpointData[0]; string method = endpointData[1];
            HttpWebRequest r;
            if (qp != null)
            {
                qp.Sort(new OAuth_Utilities.QueryParameterComparer());

                Url = Url + "?" + newOAuth.RequestNormalizer.NormalizeRequestParameters(qp);
                Console.WriteLine(Url);
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
                NameValueCollection H = httpResponse.Headers;
                //Stream dataStream = r.GetRequestStream();
                // dataStream = httpResponse.GetResponseStream();
                var reader = new StreamReader(httpResponse.GetResponseStream());
                responseFromServer = reader.ReadToEnd();
        }
            else
            {
                if (endpointData.GetValue(1).ToString() == "POST")
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
                //Stream dataStream = r.GetRequestStream();
                //WebResponse response = r.GetResponse();

                //dataStream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(dataStream);
                //responseFromServer = reader.ReadToEnd();
            }
            return responseFromServer;
        }


        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ContractResolver = new OrderedContractResolver()
        };
        public class OrderedContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(System.Type type, MemberSerialization memberSerialization)
            {
                return base.CreateProperties(type, memberSerialization).OrderBy(p => p.PropertyName).ToList();
            }
        }
    }
}
