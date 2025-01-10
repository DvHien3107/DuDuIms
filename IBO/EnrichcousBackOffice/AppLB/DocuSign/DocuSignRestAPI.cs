using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.AppLB.DocuSign
{
    public class DocuSignRestAPI
    {
        private static string basePath = System.Configuration.ConfigurationManager.AppSettings["DocuSign_RestAPI"];
        private static string accountId = System.Configuration.ConfigurationManager.AppSettings["DocuSign_Account_Id"];
        private static string INTEGRATOR_KEY = System.Configuration.ConfigurationManager.AppSettings["DocuSign_Integrator_Key"];

        /// <summary>
        /// Send Document
        /// </summary>
        /// <param name="accessToken">Access Token</param>
        /// <param name="path">File Path</param>
        /// <param name="signerName">Signer Name</param>
        /// <param name="signerEmail">Signer Email</param>
        /// <returns></returns>
        public static string SendDocument(string accessToken, string filePath, string signerName, string signerEmail, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                var config = new Configuration(basePath);
                config.AddDefaultHeader("Authorization", "Bearer " + accessToken);
                EnvelopesApi envelopesApi = new EnvelopesApi(new ApiClient(config));

                // Read a file from disk to use as a document.
                filePath = HttpContext.Current.Server.MapPath(filePath);
                byte[] fileBytes = File.ReadAllBytes(filePath);
                EnvelopeDefinition envDef = new EnvelopeDefinition();
                envDef.EmailSubject = "Please sign the document";

                // Add a document to the envelope  
                Document document = new Document
                {
                    DocumentBase64 = Convert.ToBase64String(fileBytes),
                    Name = Path.GetFileName(filePath),
                    DocumentId = "1"
                    //FileExtension = "pdf"
                };
                envDef.Documents = new List<Document> { document };

                // Add a recipient to sign the documeent
                Signer signer = new Signer
                {
                    Email = signerEmail,
                    Name = signerName,
                    RecipientId = "1",
                    RoutingOrder = "1"
                };

                // Add the recipient to the envelope object
                Recipients recipients = new Recipients
                {
                    Signers = new List<Signer> { signer }
                };
                envDef.Recipients = recipients;
                
                envDef.Status = "sent";//set envelope status to "sent" to immediately send the signature request

                // |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests) 
                EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);

                if (string.IsNullOrEmpty(envelopeSummary.EnvelopeId) == true)
                {
                    throw new Exception("Send failure.");
                }

                // print the JSON response  
                //var result = JsonConvert.SerializeObject(envelopeSummary);
                return envelopeSummary.EnvelopeId;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message, ex.InnerException?.Message);
                errMsg = ex.InnerException?.Message ?? ex.Message;
                return "";
            }
        }

        /// <summary>
        /// Check Status
        /// </summary>
        /// <param name="accessToken">Access Token</param>
        /// <param name="envelopeId">Envelope Id</param>
        /// <returns></returns>
        public static string CheckStatus(string accessToken, string envelopeId, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                var config = new Configuration(basePath);
                config.AddDefaultHeader("Authorization", "Bearer " + accessToken);
                EnvelopesApi envelopesApi = new EnvelopesApi(new ApiClient(config));

                //status: sent|delivered|completed
                //[sent] - After sending successfully
                //[delivered] - The recipient has viewed but not signed
                //[completed] - The recipient has viewed and signed
                var result = envelopesApi.GetEnvelope(accountId, envelopeId);
                return result.Status;
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message, ex.InnerException?.Message);
                errMsg = ex.InnerException?.Message ?? ex.Message;
                return "";
            }
        }
        
        /// <summary>
        /// Download Document
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="envelopeId"></param>
        /// <returns></returns>
        public static Stream DownloadDocument_PDF(string accessToken, string envelopeId, out string errMsg, string docId = "1")
        {
            errMsg = string.Empty;
            try
            {
                var config = new Configuration(basePath);
                config.AddDefaultHeader("Authorization", "Bearer " + accessToken);
                EnvelopesApi envelopesApi = new EnvelopesApi(new ApiClient(config));

                #region Check status
                //status: sent|delivered|completed
                //[sent] - After sending successfully
                //[delivered] - The recipient has viewed but not signed
                //[completed] - The recipient has viewed and signed
                var status = envelopesApi.GetEnvelope(accountId, envelopeId).Status;
                if (status == "completed")
                {
                    docId = "combined";
                }
                #endregion
                
                Stream docStream = envelopesApi.GetDocument(accountId, envelopeId, docId);
                return docStream;

                //Loc Inactive 20200225
                //using (var memoryStream = new MemoryStream())
                //{
                //    docStream.CopyTo(memoryStream);
                //    docStream.Close();
                //    FileResult fileResult = new FileContentResult(memoryStream.ToArray(), "application/pdf");
                //    fileResult.FileDownloadName = fileName.Replace(".pdf", "") + "_signed.pdf";
                //    return fileResult;
                //}
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message, ex.InnerException?.Message);
                errMsg = ex.InnerException?.Message ?? ex.Message;
                return null;
            }
        }

        /// <summary>
        /// Get Access Token
        /// </summary>
        /// <returns></returns>
        public static string GetToken(out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                var current_time = DateTime.Now;

                #region Kiem tra Session
                if (HttpContext.Current.Session["DocuSign_Token"] != null)
                {
                    var token_info = HttpContext.Current.Session["DocuSign_Token"] as AccessTokenDocuSignModel;
                    if (token_info.expires_time > current_time)
                    {
                        return token_info.access_token;
                    }
                }
                #endregion


                //Kiem tra database
                WebDataModel db = new WebDataModel();
                var webconfig = db.SystemConfigurations.FirstOrDefault();

                //Kiem tra access_token & expires_time
                var acc_token = webconfig.DocuSign_access_token;//acc_token = "string:access_token|string:expires_time"
                if (!string.IsNullOrEmpty(acc_token))
                {
                    if (DateTime.Parse(acc_token?.Split('|')[1]) > current_time)
                    {
                        
                        var token_info = new AccessTokenDocuSignModel
                        {
                            access_token = acc_token.Split('|')[0],
                            expires_time = DateTime.Parse(acc_token?.Split('|')[1])
                        };

                        HttpContext.Current.Session["DocuSign_Token"] = token_info;
                        return acc_token.Split('|')[0];
                    }
                    else
                    {
                        //Kiem tra refresh_token & expires_time
                        var ref_token = webconfig.DocuSign_refresh_token;//ref_token = "string:refresh_token|string:expires_time"
                        if (!string.IsNullOrEmpty(ref_token) && DateTime.Parse(ref_token?.Split('|')[1]) > current_time)
                        {
                            string refresh_token = ref_token.Split('|')[0];
                            string new_accessToken = RefreshToken(refresh_token, out errMsg);
                            if (!string.IsNullOrEmpty(new_accessToken))
                            {
                                return new_accessToken;
                            }
                            else
                            {
                                throw new Exception(errMsg);
                            }
                        }
                    }
                }

                //Giá trị trả về là -1 gồm các TH:
                //[Session chưa lưu AccessToken] + [Database chưa lưu AccessToken và RefeshToken hoặc AccessToken và RefeshToken hết hạn]
                return "-1";
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message, ex.InnerException?.Message);
                errMsg = ex.InnerException?.Message ?? ex.Message;
                return "";
            }
        }

        /// <summary>
        /// Refresh Token
        /// </summary>
        /// <param name="refresh_token"></param>
        /// <returns></returns>
        public static string RefreshToken(string refresh_token, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string url_oauth = System.Configuration.ConfigurationManager.AppSettings["DocuSign_Url_Oauth"];
                string authorization = System.Configuration.ConfigurationManager.AppSettings["DocuSign_Authorization"];

                string url = url_oauth + "/token?grant_type=refresh_token&refresh_token=" + refresh_token;
                string response = SendRequest(url, "POST", out errMsg, authorization);

                if (string.IsNullOrEmpty(response) == false)
                {
                    string access_token = JObject.Parse(response)["access_token"].ToString();
                    string token_type = JObject.Parse(response)["token_type"].ToString();
                    string expires_in = JObject.Parse(response)["expires_in"].ToString();
                    refresh_token = JObject.Parse(response)["refresh_token"].ToString();

                    //update access_token & refresh_token
                    var today = DateTime.Now;
                    var _expires_time = today.AddSeconds(double.Parse(expires_in));

                    WebDataModel db = new WebDataModel();
                    var webconfig = db.SystemConfigurations.FirstOrDefault();
                    webconfig.DocuSign_access_token = access_token + "|" + _expires_time.AddMinutes(-60).ToString();
                    webconfig.DocuSign_refresh_token = refresh_token + "|" + _expires_time.ToString();
                    db.Entry(webconfig).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    //luu thong tin access_token vao session
                    var token_info = new AccessTokenDocuSignModel
                    {
                        access_token = access_token,
                        expires_time = _expires_time
                    };

                    HttpContext.Current.Session["DocuSign_Token"] = token_info;
                    return access_token;
                }
                else
                {
                    throw new Exception(errMsg);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message, ex.InnerException?.Message);
                errMsg = ex.InnerException?.Message ?? ex.Message;
                return "";
            }
        }

        public static string SendRequest(string destinationUrl, string method, out string errMsg, string Authorization = "")
        {
            errMsg = string.Empty;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = method;
                request.ContentLength = 0;
                if (!string.IsNullOrEmpty(Authorization))
                {
                    request.Headers.Add("Authorization", "Basic " + Authorization);
                }
                
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                {
                    Stream responseStream = response.GetResponseStream();
                    string responseStr = new StreamReader(responseStream).ReadToEnd();
                    return responseStr;
                }
                else
                {
                    throw new Exception("Status error code: " + response.StatusCode);
                }
            }
            catch (WebException ex)
            {
                Debug.Fail(ex.Message, ex.InnerException?.Message);
                errMsg = ex.InnerException?.Message ?? ex.Message;
                return "";
            }
        }
    }
}