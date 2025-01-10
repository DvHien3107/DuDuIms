using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using EnrichcousBackOffice.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuveiClient;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.ViewModel;

namespace EnrichcousBackOffice.AppLB.NuveiLB
{
    /// <summary>
    /// Nuvei On-Boarding
    /// </summary>
    public static class SendXML
    {
        #region Nuvei Credit - Payment

        /*
        public static C_CustomerCredit Nuvei_CreditCardRegis(C_CustomerCredit cardInfo, string TERMINALID, string SECRET)
        {
            try
            {
                String gateway = "nuvei";       // Gateway that will process the transaction.
                String terminalId = TERMINALID;        // Terminal ID
                String secret = SECRET;            // Shared Secret as configured in the Terminal Setup in your Nuvei SelfCare  System

                String secureCardMerchantRef = cardInfo.MERCHANTREF;    // Unique Merchant Reference. Length is limited to 48 chars.
                String cardNumber = cardInfo.CARDNUMBER;        // The cardholders PAN (or SecureCard Card Reference);
                String cardType = cardInfo.CARDTYPE;        // See our Integrator Guide for a list of valid Card Type parameters
                String cardExpiry = cardInfo.CARDEXPIRY;        // Format: MMYY
                String cardHolderName = cardInfo.CARDHOLDERNAME;        // Cardholders name

                String dontCheckSecurity = string.IsNullOrEmpty(cardInfo.CVV) ? "" : "Y";    // (optional) "Y" if you do not want the CVV to be validated online.
                String cvv = cardInfo.CVV;            // (optional) 3 digit (4 for AMEX cards) security digit on the back of the card.
                String issueNo = cardInfo.ISSUENO;        // (optional) Issue number for Switch and Solo cards.

                IList<String> permittedTerminals = new List<String>(); // PERMITTED TERMINALS
                                                                       //permittedTerminals.Add ("1002");
                                                                       //permittedTerminals.Add ("1009");
                IList<CustomField> customFields = new List<CustomField>(); // CustomFields
                                                                           //customFields.Add ("name1", "value1"));
                                                                           //customFields.Add ("name2", "value2"));

                Boolean testAccount = true;

                XmlSecureCardRegRequest securereg = new XmlSecureCardRegRequest(secureCardMerchantRef, terminalId, cardNumber, cardExpiry, cardType, cardHolderName);

                if (!String.IsNullOrEmpty(dontCheckSecurity))
                {
                    securereg.SetDontCheckSecurity(dontCheckSecurity);
                }
                if (!String.IsNullOrEmpty(cvv))
                {
                    securereg.SetCvv(cvv);
                }
                if (!String.IsNullOrEmpty(issueNo))
                {
                    securereg.SetIssueNo(issueNo);
                }
                if (permittedTerminals != null && permittedTerminals.Count != 0)
                {
                    securereg.SetPermittedTerminals(permittedTerminals);
                }
                if (customFields != null && customFields.Count != 0)
                {
                    securereg.SetCustomFields(customFields);
                }
                XmlSecureCardRegResponse response = securereg.ProcessRequest(secret, testAccount, gateway);
                string xml = response.ToString();
                String expectedResponseHash = Response.GetResponseHash(terminalId + response.MerchantRef + response.CardRef + response.DateTimeHashString + secret);

                if (response.IsError == true)
                {
                    throw new Exception("ERROR : " + response.ErrorString);
                    //Handle Error Response
                }
                else if (response.Hash != expectedResponseHash)
                {
                    throw new Exception("ERROR : Response HASH parameter not as expected. If live possible man-in-the-middle attack.");
                    //Handle Invalid Hash scenario - inform merchant that transaction may have to be voided.
                }
                else
                {
                    var Card = new C_CustomerCredit
                    {
                        CustomerCode = cardInfo.CustomerCode,
                        PHONE = cardInfo.ISSUENO,
                        EMAIL = cardInfo.EMAIL,
                        NuveiResponse_CARDREFERENCE = response.CardRef,
                        NuveiResponse_DATETIME = response.DateTime,
                        MERCHANTREF = response.MerchantRef,
                        NuveiResponse_HASH = response.Hash
                    };

                    return Card;
                    //Handle Response
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        */

        public static void Nuvei_payment(PaymentInfo pay)
        {
            String gateway = "nuvei";        // Gateway that will process the transaction.
            String terminalId = "33001";                        // Terminal ID
            String currency = "USD";                          // EUR/GBP/USD etc.

            // These values are specific to the cardholder.
            String cardNumber = pay.CardNumber;        // The cardholders PAN (or SecureCard Card Reference);
            String trackData = pay.TrackData; // track Data
            String encryptedTrack = pay.EncryptedTrack; //encrypted Track for DukptCardDetails
            String ksn = pay.Ksn; //ksn for DukptCardDetails
            int? formatId = pay.FormatId; // formatId for DukptCardDetails
            String applePayload = pay.ApplePayload; // Apple Payload
            String androidPayload = pay.AndroidPayload; // Android Payload
            String cardType = pay.CardType;        // See our Integrator Guide for a list of valid Card Type parameters
            String email = pay.Email;            // (optional) Cardholders e-mail address for sending of a receipt
            String mobileNumber = pay.MobileNumber;            // (optional) Cardholders mobile phone number for sending of a receipt. Digits only, Include international prefix.
            String cardExpiry = pay.CardExpiry;        // Format: MMYY
            String cardHolderName = pay.CardHolderName;        // Cardholders name
            String cvv = pay.Cvv;            // (optional) 3 digit (4 for AMEX cards) security digit on the back of the card.
            String issueNo = pay.IssueNo;            // (optional) Issue number for Switch and Solo cards.
            String orderId = pay.OrderId;
            Double amount = pay.Amount;

            // These fields are for AVS (Address Verification Check). This is only appropriate in the UK and the US.
            String address1 = pay.Address1;        // (optional) This is the first line of the cardholders address.
            String address2 = pay.Address2;        // (optional) This is the second line of the cardholders address.
            String postcode = pay.Postcode;        // (optional) This is the cardholders post code.
            String country = pay.Country;        // (optional) This is the cardholders country name.
            String phone = pay.Phone;        // (optional) This is the cardholders home phone number.

            // These values are specific to the transaction.

            Boolean isMailOrder = false;    // If true the transaction will be processed as a Mail Order transaction. This is only for use with Mail Order enabled Terminal IDs.

            String description = pay.Description;        // (optional) Transaction description

            // eDCC fields. Populate these if you have retreived a rate for the transaction, offered it to the cardholder and they have accepted that rate.
            String cardCurrency = pay.CardCurrency;        // (optional) This is the three character ISO currency code returned in the rate request.
            Double? cardAmount = null;        // (optional) This is the foreign currency transaction amount returned in the rate request.
            Double? conversionRate = null;      // (optional) This is the currency conversion rate returned in the rate request.

            // 3D Secure reference. Only include if you have verified 3D Secure throuugh the Nuvei MPI and received an MPIREF back.
            String mpiref = pay.Mpiref;            // This should be blank unless instructed otherwise by Nuvei.
            String deviceId = pay.DeviceId;            // This should be blank unless instructed otherwise by Nuvei.

            String autoReady = pay.AutoReady;        // (optional) Y or N. Automatically set the transaction to a status of Ready in the batch. If not present the terminal default will be used.
            Boolean multicur = false;        // This should be false unless instructed otherwise by Nuvei.

            String billToFirstName = pay.BillToFirstName; // BillTo FirstName
            String billTolastName = pay.BillTolastName; // BillTo LastName
            String xid = pay.Xid; //XID
            String cavv = pay.Cavv; // CAVV
            String city = pay.City; // CITY
            String region = pay.Region; // REGION
            String ipAddress = pay.IpAddress; // IPADDRESS
            String signature = pay.Signature; // SIGNATURE
            IList<CustomField> customFields = new List<CustomField>(); // CustomFields
            //customFields.Add ("name1", "value1"));
            //customFields.Add ("name2", "value2"));
            String recurringTxnRef = pay.RecurringTxnRef; // RECURRING TXN REF

            String secret = "SandboxSecret001";            // Shared Secret as configured in the Terminal Setup in your Nuvei SelfCare  System

            Boolean testAccount = true;

            XmlAuthRequest request = new XmlAuthRequest(terminalId, orderId, currency, amount, cardType);

            if (!String.IsNullOrEmpty(trackData))
            {
                request.SetTrackData(trackData);
            }
            else if (!String.IsNullOrEmpty(encryptedTrack) && !String.IsNullOrEmpty(ksn) && formatId.HasValue)
            {
                request.SetDukptCardDetails(encryptedTrack, ksn, formatId.Value);
            }
            else if (!String.IsNullOrEmpty(applePayload))
            {
                request.SetApplePayload(applePayload);
            }
            else if (!String.IsNullOrEmpty(androidPayload))
            {
                request.SetAndroidPayload(androidPayload);
            }
            else
            {
                request.SetCardNumber(cardNumber);
            }
            if (!String.IsNullOrEmpty(cardExpiry) && !String.IsNullOrEmpty(cardHolderName))
            {
                request.SetNonSecureCardCardInfo(cardExpiry, cardHolderName);
            }
            if (!String.IsNullOrEmpty(cvv))
            {
                request.SetCvv(cvv);
            }
            if (!String.IsNullOrEmpty(cardCurrency) && cardAmount.HasValue && conversionRate.HasValue)
            {
                request.SetForeignCurrencyInformation(cardCurrency, cardAmount.Value, conversionRate.Value);
            }
            if (!String.IsNullOrEmpty(email))
            {
                request.SetEmail(email);
            }
            if (!String.IsNullOrEmpty(mobileNumber))
            {
                request.SetMobileNumber(mobileNumber);
            }
            if (!String.IsNullOrEmpty(description))
            {
                request.SetDescription(description);
            }

            if (!String.IsNullOrEmpty(issueNo))
            {
                request.SetIssueNo(issueNo);
            }
            if (!String.IsNullOrEmpty(address1) && !String.IsNullOrEmpty(postcode))
            {
                request.SetAvs(address1, address2, postcode);
            }
            if (!String.IsNullOrEmpty(country))
            {
                request.SetCountry(country);
            }
            if (!String.IsNullOrEmpty(phone))
            {
                request.SetPhone(phone);
            }

            if (!String.IsNullOrEmpty(deviceId))
            {
                request.SetDeviceId(deviceId);
            }
            if (!String.IsNullOrEmpty(mpiref))
            {
                request.SetMpiRef(mpiref);
            }
            if (!String.IsNullOrEmpty(billToFirstName))
            {
                request.SetBillToFirstName(billToFirstName);
            }
            if (!String.IsNullOrEmpty(billTolastName))
            {
                request.SetBillTolastName(billTolastName);
            }
            if (!String.IsNullOrEmpty(xid))
            {
                request.SetXid(xid);
            }
            if (!String.IsNullOrEmpty(cavv))
            {
                request.SetCavv(cavv);
            }
            if (!String.IsNullOrEmpty(city))
            {
                request.SetCity(city);
            }
            if (!String.IsNullOrEmpty(region))
            {
                request.SetRegion(region);
            }
            if (!String.IsNullOrEmpty(ipAddress))
            {
                request.SetIpAddress(ipAddress);
            }
            if (!String.IsNullOrEmpty(signature))
            {
                request.SetSignature(signature);
            }
            if (customFields != null && customFields.Count != 0)
            {
                request.SetCustomFields(customFields);
            }
            if (!String.IsNullOrEmpty(recurringTxnRef))
            {
                request.SetRecurringTxnRef(recurringTxnRef);
            }

            if (isMailOrder)
            {
                request.SetMotoTrans();
            }
            if (multicur)
            {
                request.SetMultiCur();
            }
            if (!String.IsNullOrEmpty(autoReady))
            {
                request.SetAutoReady(autoReady);
            }

            XmlAuthResponse response = request.ProcessRequest(secret, testAccount, gateway);

            String expectedResponseHash = Response.GetResponseHash(terminalId + response.UniqueRef + ((multicur) ? currency : "") + amount.ToString(CultureInfo.InvariantCulture) + response.DateTimeHashString + response.ResponseCode + response.ResponseText + secret);

            if (response.IsError == true)
            {
                throw new Exception("ERROR : " + response.ErrorString);
                //Handle Error Response
            }
            else if (response.Hash != expectedResponseHash)
            {
                throw new Exception("ERROR : Response HASH parameter not as expected. If live possible man-in-the-middle attack.");
                //Handle Invalid Hash scenario - inform merchant that transaction may have to be voided.
            }
            else
            {
                Console.Out.WriteLine("RESPONSECODE : " + response.ResponseCode);
                if (response.ResponseCode.Equals("A"))
                {
                    //Handle success response
                }
                else
                {
                    //Handle declined response
                }
                Console.Out.WriteLine("RESPONSETEXT : " + response.ResponseText);
                Console.Out.WriteLine("APPROVALCODE : " + response.ApprovalCode);
                Console.Out.WriteLine("DATETIME : " + response.DateTimeHashString);
                Console.Out.WriteLine("AVSRESPONSE : " + response.AvsResponse);
                Console.Out.WriteLine("CVVRESPONSE : " + response.CvvResponse);
                Console.Out.WriteLine("UNIQUEREF : " + response.UniqueRef);
                Console.Out.WriteLine("HASH : " + response.Hash);
                //Handle Response
            }
        }

        #endregion

        /// <summary>
        /// Create Nuvei application Id
        /// </summary>
        /// <param name="NuveiApplink"></param>
        /// <param name="nuveiPayload"></param>
        /// <param name="Authorization"></param>
        /// <param name="appid">update if already exists appid</param>
        /// <returns></returns>
        public static string RequestAppID(string NuveiApplink, NuveiPayload nuveiPayload, string Authorization, string appid = "")
        {
            //var json = new JavaScriptSerializer().Serialize(nuveiPayload);
            string json = JsonConvert.SerializeObject(nuveiPayload);
            //System.Diagnostics.Trace.WriteLine("");
            //System.Diagnostics.Trace.WriteLine(json);
            //System.Diagnostics.Trace.WriteLine("");

            //?api_key=test:Testing123$
            //test api: test:Testing123$
            // live api:enrichco:Nuvei1!

            //Feradal Tax ID khong thay đổi khi update
            if (string.IsNullOrWhiteSpace(appid))
            {
                var respone = postJsonData(NuveiApplink + "/applink/Application/US", json, Authorization);
                try
                {
                    string AppID = JObject.Parse(respone)["ApplicationId"].ToString();
                    return AppID;
                }
                catch (Exception)
                {
                    throw new Exception(respone);
                }

            }
            else
            {
                //update application
                var respone = postJsonData(NuveiApplink + "/applink/Application/US/" + appid, json, Authorization, "PUT");
                if (string.IsNullOrWhiteSpace(respone))
                {
                    return appid;
                }

                throw new Exception(respone);

            }

        }

        /// <summary>
        /// Post Json Data to nuvei api.
        /// </summary>
        /// <param name="destinationUrl"></param>
        /// <param name="requestJson"></param>
        /// <param name="Authorization"></param>
        /// <returns></returns>
        public static string postJsonData(string destinationUrl, string requestJson, string Authorization = null, string method = "POST")
        {
            HttpWebResponse response;
            bool response_nOK = false;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(destinationUrl);
                byte[] bytes;
                bytes = System.Text.Encoding.UTF8.GetBytes(requestJson);
                request.ContentType = "application/json";
                request.ContentLength = bytes.Length;
                request.Method = method;
                if (!string.IsNullOrEmpty(Authorization))
                {
                    request.Headers.Add("Authorization", Authorization);
                }
                //request.Headers.Add("api_key", "enrichco:Nuvei1!");
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    response_nOK = true;
                }
            }
            catch (WebException ex)
            {
                response = ((HttpWebResponse)ex.Response);
            }

            string responseStr = SaveLog(destinationUrl, method, requestJson, response);
            if (response_nOK)
            {
                return null;
            }
            return responseStr;
        }

        private static string SaveLog(string destinationUrl, string method, string requestJson, HttpWebResponse response)
        {
            Stream responseStream = response.GetResponseStream();
            string responseStr = new StreamReader(responseStream).ReadToEnd();
            using (WebDataModel db = new WebDataModel())
            {
                var log = new IMSLog
                {
                    Id = Guid.NewGuid().ToString(),
                    CreateBy = Authority.GetCurrentMember().FullName ?? "IMS System",
                    CreateOn = DateTime.UtcNow,
                    Url = HttpContext.Current.Request.RawUrl,
                    RequestUrl = destinationUrl,
                    RequestMethod = method.ToUpper(),
                    StatusCode = (int)response.StatusCode,
                    Success = (int)response.StatusCode < 300,
                    JsonRequest = requestJson,
                    JsonRespone = responseStr,
                    //Description = apiDeter
                };
                db.IMSLogs.Add(log);
                db.SaveChanges();
            }
            return responseStr;
        }

        public static string HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc, string Authorization = null)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";
            request.KeepAlive = true;
            request.Credentials = System.Net.CredentialCache.DefaultCredentials;
            if (!string.IsNullOrEmpty(Authorization))
            {
                request.Headers.Add("Authorization", Authorization);
            }
            Stream rs = request.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            HttpWebResponse wresp = null;
            try
            {
                wresp = (HttpWebResponse)request.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                SaveLog(url, request.Method, JsonConvert.SerializeObject(new object[] { file }), wresp);
                return reader2.ReadToEnd();
            }
            catch (WebException ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                SaveLog(url, request.Method, JsonConvert.SerializeObject(new object[] { file }), (HttpWebResponse)ex.Response);
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd().Replace("\"", "");
                throw new Exception(resp);
            }
            catch (Exception ex)
            {
                throw ex;
                // Something more serious happened
                // like for example you don't have network access
                // we cannot talk about a server exception here as
                // the server probably was never reached
            }
        }
    }


}