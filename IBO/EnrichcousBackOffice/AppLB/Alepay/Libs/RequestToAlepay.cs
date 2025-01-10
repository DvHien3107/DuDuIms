using EnrichcousBackOffice.AppLB.Alepay.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.AppLB.Alepay.Libs
{
    public class RequestToAlepay
    {
        private static string token = ConfigurationManager.AppSettings["TokenKey_Alepay"];
        private static string checksumKey = ConfigurationManager.AppSettings["ChecksumKey_Alepay"];
        public static string sendRequest(object obj, string url)
        {
            // mã hóa dữ liệu bằng RSA
            var encData = EncryptionDecryption.RSAEncrypt(obj);

            // tạo checksum
            var checksumData = EncryptionDecryption.MD5Encrypt(encData + checksumKey);

            ReqAlepay requestData = new ReqAlepay();
            requestData.token = token;
            requestData.data = encData;
            requestData.checksum = checksumData;

            // convert dữ liệu về json
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new TickDateTimeConverter() }
            };
            var dataJson = JsonConvert.SerializeObject(requestData, settings);

            // post dữ liệu sang api
            var result_json = Post_Alepay(dataJson, url);

            // dữ liệu trả về dạng json
            return result_json;
        }

        public static string Post_Alepay(string myParameters, string URI)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json;charset=UTF-8";
                try
                {
                    string HtmlResult = wc.UploadString(URI, "Post", myParameters);
                    return HtmlResult;
                }
                catch (WebException ex)
                {
                    var statusCode = (int)((HttpWebResponse)ex.Response).StatusCode;
                    var des = ((HttpWebResponse)ex.Response).StatusDescription;
                    var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    return resp;
                }
            }
        }
    }
}
