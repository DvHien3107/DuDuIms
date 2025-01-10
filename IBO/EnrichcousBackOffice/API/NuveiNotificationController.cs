using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace EnrichcousBackOffice.API
{
    /// <summary>
    /// API ready to nuvei send a reponse notification approved
    /// </summary>
    public class NuveiNotificationController : ApiController
    {
       

        // POST api/<controller>
        //public  HttpResponseMessage Post()
        //{
        //    var token = Request.Headers.GetValues("AuthenticationKey").First();
        //    HttpResponseMessage message = new HttpResponseMessage();
        //    if (token == System.Configuration.ConfigurationManager.AppSettings["AuthenticationKey"].ToString())
        //    {
        //        try
        //        {
        //            string data = Request.Content.ReadAsStringAsync().Result;
        //            NVNotificationModel jsonData = JsonConvert.DeserializeObject<NVNotificationModel>(data);
        //            System.Diagnostics.Debug.WriteLine("MessageType:" + jsonData.MessageType);
        //            System.Diagnostics.Debug.WriteLine("UId:" + jsonData.UId);
        //            System.Diagnostics.Debug.WriteLine("GatewayTerminalNumbers:" + string.Join("|", jsonData.Message.GatewayTerminalNumbers));
        //            //save merchant data.
        //            using (var db = new Models.WebDataModel())
        //            {
        //                var dbaSubcriber = db.C_MerchantSubscribe.Where(m => m.MerchantID == jsonData.Message.MerchantId.Trim()).FirstOrDefault();
        //                //update...
        //                if (dbaSubcriber != null)
        //                {
        //                    dbaSubcriber.GatewayMerchantId = jsonData.Message.GatewayMerchantId;
        //                    dbaSubcriber.GatewayTerminalNumbers = string.Join("|", jsonData.Message.GatewayTerminalNumbers);
        //                    dbaSubcriber.SharedSecret = jsonData.Message.SharedSecret;
        //                    dbaSubcriber.UId = jsonData.UId;
        //                    dbaSubcriber.DbaName = jsonData.Message.DbaName;
        //                    dbaSubcriber.MessageType = jsonData.MessageType;
        //                    dbaSubcriber.ResponseStatus = jsonData.Status;
        //                    dbaSubcriber.ResponseCodeFromNuvei = data;
        //                    db.Entry(dbaSubcriber).State = System.Data.Entity.EntityState.Modified;
        //                    db.SaveChanges();
        //                    //dbaSubcriber.
        //                    _ = ViewControler.TicketViewController.AutoTicketScenario.UpdateTicketNuveiOnboarding(dbaSubcriber.CustomerCode, dbaSubcriber.ResponseStatus);
        //                }
        //            }




        //            //

        //            message = Request.CreateResponse(HttpStatusCode.OK);
        //            message.Content = new StringContent("{\"MessageType\":\"" + jsonData.MessageType + "\",\"Uid\":\"" + jsonData.UId + "\"}");
        //        }
        //        catch (Exception e) 
        //        {

        //            message = Request.CreateResponse(HttpStatusCode.BadRequest);
        //            message.Content = new StringContent(e.Message);
        //        }
              
        //    }
        //    else
        //    {
        //        message = Request.CreateResponse(HttpStatusCode.Unauthorized);
        //        message.Content = new StringContent("Authentication key is invalid");
        //    }
            
        //    return message;
        //}

      
        private class NVNotificationModel
        {
            public string MessageType { get; set; }
            public string UId { get; set; }
            public string CreatedDate { get; set; }
            /// <summary>
            /// Failed/Completed/Cancelled
            /// </summary>
            public string Status { get; set; }
            public string Responsecode { get; set; }
            public string ResponsePhrase { get; set; }
            public NVMessageModel Message { get; set; }
        }

        private class NVMessageModel
        {
            public string DbaName { get; set; }
            public string MerchantId { get; set; }
            public string GatewayMerchantId { get; set; }
            public string[] GatewayTerminalNumbers { get; set; }
            public string SharedSecret { get; set; }
        }

    }
}