using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class AdobeLoadController : Controller
    {
        WebDataModel db = new WebDataModel();
        string clientID = System.Configuration.ConfigurationManager.AppSettings["Adobe_clientID"];
        const string ACCESS_TOKEN = "Access token";

        // GET: AdobeLoad
        public ActionResult Index()
        {
            //HDSD
            // url = /Adobeload?goback=/home/index

            Session.Remove(ACCESS_TOKEN);
            TempData["goback"] = Request["goback"].ToString();
            return Redirect("https://secure.na2.echosign.com/public/oauth?redirect_uri=https://localhost/AdobeLoad/GetAccessToken&response_type=code&client_id=" + clientID + "&scope=user_login:self+agreement_send:account+agreement_write:account+agreement_read:account");
        }
        public ActionResult GetAccessToken()
        {
            string code = Request["code"];
            string api_access_point = Request["api_access_point"];
            string web_access_point = Request["web_access_point"];
            string goback = TempData["goback"].ToString();


            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["Adobe_clientSecret"];
            string host = "https://localhost/AdobeLoad/GetAccessToken";
            string url = api_access_point + "/oauth/token?" +
                "code=" + code +
                "&client_id=" + clientID +
                "&client_secret=" + clientSecret +
                "&redirect_uri=" + host +
                "&grant_type=authorization_code";
            string response = API.AdobeRestAPIController.SendRequest(url, "POST");
            string access_token = JObject.Parse(response)["access_token"].ToString();
            string refresh_token = JObject.Parse(response)["refresh_token"].ToString();
            string token_type = JObject.Parse(response)["token_type"].ToString();
            //save refresh code

            var webconfig = db.SystemConfigurations.FirstOrDefault();
            webconfig.Adobe_refresh_token = refresh_token;
            db.Entry(webconfig).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            //luu thong tin access_token vao session
            var token_info = new AccessTokenDocuSignModel
            {
                access_token = access_token,
                expires_time = DateTime.Now.AddMinutes(50)
            };
            Session[ACCESS_TOKEN] = token_info;

            return Redirect(goback);
        }
    }
}