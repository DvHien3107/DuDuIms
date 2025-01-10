using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace EnrichcousBackOffice.API
{
    public class MangoPOSController : ApiController
    {
        //[HttpPost]
        //public HttpResponseMessage UpdateForIMS()
        //{
        //    //var token = Request.Headers.GetValues("AuthenticationKey").First();
        //    HttpResponseMessage message = new HttpResponseMessage();
        //    if (true/*token == System.Configuration.ConfigurationManager.AppSettings["AuthenticationKey"].ToString()*/)
        //    {
        //        try
        //        {
        //            string data = Request.Content.ReadAsStringAsync().Result;
        //            POS_info jsonData = JsonConvert.DeserializeObject<POS_info>(data);
        //            if (jsonData?.StoreId == null)
        //            {
        //                throw new Exception("Store_Id can't empty");
        //            }
        //            using (var db = new Models.WebDataModel())
        //            {
        //                var store = db.Store_Services.Where(s => s.StoreCode == jsonData.StoreId).OrderByDescending(s => s.Active).FirstOrDefault();
        //                if (store == null)
        //                {
        //                    throw new Exception("Store Id " + jsonData.StoreId + " is not exist!");
        //                }
        //                //var merchant = db.C_Customer.Where(c => c.CustomerCode == store.CustomerCode).FirstOrDefault();
        //                //update...
        //                //if (merchant != null)
        //                //{
        //                //    merchant.POS_Account = jsonData.POS_Account ?? merchant.POS_Account;
        //                //    merchant.POS_CheckIn_URL = jsonData.POS_CheckIn_URL ?? merchant.POS_CheckIn_URL;
        //                //    merchant.POS_Client_URL = jsonData.POS_Client_URL ?? merchant.POS_Client_URL;
        //                //    merchant.POS_Manager_URL = jsonData.POS_Manager_URL ?? merchant.POS_Manager_URL;
        //                //    merchant.POS_Pin = jsonData.POS_Pin ?? merchant.POS_Pin;
        //                //    merchant.POS_Tech_URL = jsonData.POS_Tech_URL ?? merchant.POS_Tech_URL;
        //                //    merchant.POS_Url = jsonData.POS_Url ?? merchant.POS_Url;
        //                //    merchant.StoreCode = store.StoreCode ?? merchant.StoreCode;
        //                //    merchant.StoreStatus = true;//store.Active ?? merchant.StoreStatus;
        //                //    merchant.POS_PairingCode = jsonData.POS_PairingCode;
        //                //    db.Entry(merchant).State = System.Data.Entity.EntityState.Modified;
        //                //    db.SaveChanges();
        //                //    //dbaSubcriber.
        //                //    //Task t = ViewControler.TicketViewController.AutoTicketScenario.UpdateTicketNuveiOnboarding(dbaSubcriber.CustomerCode, dbaSubcriber.ResponseStatus);
        //                //    //t.Wait();


        //                //}
        //            }




        //            //

        //            message = Request.CreateResponse(HttpStatusCode.OK);
        //            message.Content = new StringContent("{\"Result\" : 1, \"Message\" : \"Update successfully\"}");
        //        }
        //        catch (Exception e)
        //        {

        //            message = Request.CreateResponse(HttpStatusCode.InternalServerError);
        //            message.Content = new StringContent("{\"Result\" : -1, \"Message\" : \"" + e.Message + "\"}");
        //        }

        //    }
        //    //else
        //    //{
        //    //    message = Request.CreateResponse(HttpStatusCode.Unauthorized);
        //    //    message.Content = new StringContent("Authentication key is invalid");
        //    //}

        //    return message;
        //}


        private class POS_info
        {
            public string StoreId { get; set; }
            public string POS_Account { get; set; }
            public string POS_Url { get; set; }
            public string POS_Pin { get; set; }
            public string POS_CheckIn_URL { get; set; }
            public string POS_Tech_URL { get; set; }
            public string POS_Client_URL { get; set; }
            public string POS_Manager_URL { get; set; }
            public string POS_PairingCode { get; set; }
        }
    }
}
