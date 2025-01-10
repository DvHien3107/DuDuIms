using Enrich.Payment.MxMerchant.Models;
using System.Collections.Specialized;
using System.Security.Cryptography;

namespace Enrich.Payment.MxMerchant.Api
{
    public interface IOAuthNew
    {
        public List<QueryParameter> SetupParams();
        public string GenerateSignatureBase(string[] endPointData);
        public string GenerateSignatureUsingHash(string signatureBase, HashAlgorithm hash);
        public string GenerateSignature(string[] endpointData);
        public void RecaptureParams();
        public string CreateHeaders(string[] endpointData, NameValueCollection tokens = null, IList<QueryParameter> queryParameters = null);
        public string GetTimeStamp();
        public string GenerateNonce();
        public NameValueCollection GetAccessToken();
    }
}
