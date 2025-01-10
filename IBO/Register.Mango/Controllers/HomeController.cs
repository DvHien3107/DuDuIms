using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Register.Mango.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            var indexPage = ConfigurationManager.AppSettings["default_page_url"];
            if (string.IsNullOrWhiteSpace(indexPage) || indexPage.Contains("/home") || indexPage == "/")
            {
                return View();
            }
            return Redirect(indexPage);
        }
         [HttpGet]
        public PartialViewResult RenderPageOne()
        {
            return PartialView("_PageOne");
        }
        [HttpGet]
        public PartialViewResult RenderPageTwo()
        {
            return PartialView("_PageTwo");
        }
    }
}