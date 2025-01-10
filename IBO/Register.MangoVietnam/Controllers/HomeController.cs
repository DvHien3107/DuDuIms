using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Register.MangoVietnam.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
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