using Enrich.Payment.MxMerchant.Models;
using System.Collections.Specialized;
using System.Security.Cryptography;

namespace Enrich.Payment.MxMerchant.Api
{
    public interface IOAuthUtilities
    {
        public string UrlEncode(string value);
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash);
        public byte[] KeyBuilder(NameValueCollection tokens);
        public string CreateHeader(IList<QueryParameter> parameters);
        public string ComputeHash(HashAlgorithm hashAlgorithm, string data);
        public string NormalizeRequestParameters(IList<QueryParameter> parameters);
        public string SendRequest(string[] endpointData, string headers, object JSON = null, List<QueryParameter> qp = null);
    }
}
