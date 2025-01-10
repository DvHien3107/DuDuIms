using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;

namespace EnrichcousBackOffice.Controllers
{
    public class DocuSignLoadController : Controller
    {
        // GET: DocuSignLoad
        private string url_oauth = System.Configuration.ConfigurationManager.AppSettings["DocuSign_Url_Oauth"];


        public ActionResult Index(string goback = "/home/index")
        {
            Session.Remove("DocuSign_Token");
            string INTEGRATOR_KEY = System.Configuration.ConfigurationManager.AppSettings["DocuSign_Integrator_Key"];

            TempData["goback"] = Request["goback"].ToString();
            return Redirect(url_oauth + "/auth?response_type=code&scope=signature%20extended&client_id=" + INTEGRATOR_KEY + "&state=enrichcous&redirect_uri=" + Request.Url.Scheme + "://" + Request.Url.Authority + "/DocuSignLoad/GetAccessToken");
        }


        public ActionResult GetAccessToken()
        {
            string errMsg = string.Empty;
            string goback = TempData["goback"].ToString();
            try
            {
                string code = Request["code"];
                string url = url_oauth + "/token?grant_type=authorization_code&code=" + code;
                string authorization = System.Configuration.ConfigurationManager.AppSettings["DocuSign_Authorization"];

                string response = AppLB.DocuSign.DocuSignRestAPI.SendRequest(url, "POST", out errMsg, authorization);
                if (string.IsNullOrEmpty(response))
                {
                    throw new Exception(errMsg);
                }

                string access_token = JObject.Parse(response)["access_token"].ToString();
                string refresh_token = JObject.Parse(response)["refresh_token"].ToString();
                string token_type = JObject.Parse(response)["token_type"].ToString();
                string expires_in = JObject.Parse(response)["expires_in"].ToString();

                //save access_token & refresh_token
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
                Session["DocuSign_Token"] = token_info;
            }
            catch (Exception ex)
            {
                TempData["e"] = ex.InnerException?.Message ?? ex.Message;
            }
            
            return Redirect(goback);
        }
    }
}