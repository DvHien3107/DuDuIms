using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Mvc;

namespace EnrichcousBackOffice.API
{
    public class AdobeRestAPIController
    {
        #region Upload-send-check-download Document

        public static string UploadDocument(string accessToken, string filePath, string fileName)
        {
            //accessToken = "Bearer 3AAABLblqZhApbrEJb9Yg4YLHKmum1WzGNk9-m-7thZTShomi-fi9fEvnLm0EgdEkJz07u8Oj9aBh7r5T0w-2Bt5gVlQXCT2o";
            //filePath = "~/upload/merchant/template/";
            //fileName = "DejavooTemplate.pdf";
            //-----

            string transientDocumentId = "0";
            var nvc = new NameValueCollection
            {
                    {"File", System.Web.HttpContext.Current.Server.MapPath(filePath)},
                    {"Content-Type", "multipart/form-data"},
                    {"Mime-Type", "application/pdf"},
                    {"File-Name", fileName}
            };

            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] boundarybytesF = Encoding.ASCII.GetBytes("--" + boundary + "\r\n");  // the first time it itereates, you need to make sure it doesn't put too many new paragraphs down or it completely messes up poor webbrick.  

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create("https://api.na2.echosign.com/api/rest/v6/transientDocuments");
            wr.Method = "POST";
            wr.Headers["Authorization"] = string.Format("Bearer " + accessToken);
            wr.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_8_2) AppleWebKit/537.17 (KHTML, like Gecko) Chrome/24.0.1312.52 Safari/537.17";
            wr.KeepAlive = true;
            wr.Credentials = CredentialCache.DefaultCredentials;
            wr.AllowWriteStreamBuffering = true;
            wr.ContentType = "multipart/form-data; boundary=" + boundary;

            Stream rs = wr.GetRequestStream();
            bool firstLoop = true;
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                if (firstLoop)
                {
                    rs.Write(boundarybytesF, 0, boundarybytesF.Length);
                    firstLoop = false;
                }
                else
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                }
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);
            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, "File", new FileInfo(System.Web.HttpContext.Current.Server.MapPath(Path.Combine(filePath, fileName))).Name, "application/octet-stream");

            byte[] headerbytes = Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);
            FileStream fileStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(Path.Combine(filePath, fileName)), FileMode.Open, FileAccess.Read);

            byte[] buffer = new byte[fileStream.Length];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            fileStream.Close();
            byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            try
            {
                var httpResponse = (HttpWebResponse)wr.GetResponse();
                using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = sr.ReadToEnd();
                    var jsonSerialization = new JavaScriptSerializer();
                    var dictObj = jsonSerialization.Deserialize<Dictionary<string, dynamic>>(result);

                    if (dictObj.Count > 0)
                    {
                        transientDocumentId = Convert.ToString(dictObj["transientDocumentId"]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return transientDocumentId;
        }

        public static string SendDocument(string accessToken, string transientDocumentId, string fileName, string email)
        {
            string url = "https://api.na2.echosign.com/api/rest/v6/agreements";

            string requestJson = @"
            {
                ""fileInfos"": [{
                    ""transientDocumentId"": """ + transientDocumentId + @"""
                }],
                ""name"": """ + fileName + @""",
                ""participantSetsInfo"": [{
                    ""memberInfos"": [{
                        ""email"": """ + email + @"""
                    }],
                    ""order"": 1,
                    ""role"": ""SIGNER""
                }],
                ""signatureType"": ""ESIGN"",
                ""state"": ""IN_PROCESS""
            }";

            string response = SendRequest(url, "POST", "Bearer " + accessToken, requestJson);//tra ve agreement id

            return JObject.Parse(response)["id"].ToString();
        }

        public bool CheckStatus(string auth, string agreementId)
        {
            try
            {
                string Url = "https://api.na2.echosign.com:443/api/rest/v6/agreements/" + agreementId;
                string response = SendRequest(Url, "GET", auth);
                string status = JObject.Parse(response)["status"].ToString();
                if (status == "SIGNED")
                    return true;
                if (status == "OUT_FOR_SIGNATURE")
                    return false;
                throw new Exception("check failure");
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string DownloadAgreement(string auth, string agreementId, string pathfile)
        {
            try
            {
                string Url = "https://api.na2.echosign.com:443/api/rest/v6/agreements/" + agreementId + "/combinedDocument";
                Stream response = getRequestFile(Url, auth);
                using (Stream output = System.IO.File.OpenWrite(pathfile))
                    response.CopyTo(output);
                response.Close();
                return "";
            }
            catch (WebException ex)
            {
                throw ex;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public Stream getRequestFile(string destinationUrl, string Authorization = null)
        {
            try
            {
                WebRequest wrGETURL;
                wrGETURL = WebRequest.Create(destinationUrl);
                wrGETURL.Headers.Add("Authorization", Authorization);
                //using (Stream output = File.OpenWrite("file.pdf"))
                Stream input = wrGETURL.GetResponse().GetResponseStream();
                return input;
            }
            catch (WebException ex)
            {
                throw ex;
            }

        }

        #endregion
        public static string getToken()
        {
            string RefreshToken = AppLB.UserContent.GetWebInfomation()?.Adobe_refresh_token;
            if (string.IsNullOrEmpty(RefreshToken) == true)
            {
                //TH chua co refesh_token
                return null;
            }

            string clientID = System.Configuration.ConfigurationManager.AppSettings["Adobe_clientID"];
            string clientScrect = System.Configuration.ConfigurationManager.AppSettings["Adobe_clientSecret"];

            string newAccessToken = API.AdobeRestAPIController.RefreshToken(RefreshToken, clientID, clientScrect);
            return newAccessToken;
        }

        public static string RefreshToken(string refresh_token, string client_id, string client_secret)
        {
            string grant_type = "refresh_token";
            string postdata = "?refresh_token=" + refresh_token + "&client_id=" + client_id + "&client_secret=" + client_secret + "&grant_type=" + grant_type;
            string response = SendRequest("https://api.na2.echosign.com/oauth/refresh" + postdata, "POST");

            if (string.IsNullOrEmpty(response) == false)
            {
                //TH refesh_token khong dung hoac het thoi gian ton tai
                if (string.IsNullOrEmpty(JObject.Parse(response)["code"]?.ToString()) == false)
                {
                    string msg_error = JObject.Parse(response)["message"].ToString();
                    return null;
                }


                return JObject.Parse(response)["access_token"].ToString();
            }
            return response;
        }

        public static string SendRequest(string destinationUrl, string method, string Authorization = null, string requestJson = "")
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = method;
                if (!string.IsNullOrEmpty(Authorization))
                {
                    request.Headers.Add("Authorization", Authorization);
                }

                if (!string.IsNullOrEmpty(requestJson))
                {
                    byte[] bytes;
                    bytes = System.Text.Encoding.ASCII.GetBytes(requestJson);
                    request.ContentType = "application/json";
                    request.ContentLength = bytes.Length;

                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                
                HttpWebResponse response;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return responseStr;
                }
                return null;
            }
            catch (WebException ex)
            {
                throw ex;
            }
        }
    }
}