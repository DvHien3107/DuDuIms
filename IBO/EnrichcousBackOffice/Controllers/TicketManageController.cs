using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static iTextSharp.text.pdf.AcroFields;

namespace EnrichcousBackOffice.Controllers
{
    public class TicketManageController : Controller
    {
        // GET: TicketManageTicketManage?departmentid=
        public ActionResult Index(string departmentid)
        {
            var db = new WebDataModel();
            if (departmentid != "0")
            {
                var item = db.P_Department.FirstOrDefault(x => x.Id.ToString() == departmentid);
                ViewBag.Department = item.Name;
                ViewBag.DepartmentId = item.Id;
            }
            else
            {
                ViewBag.Department = "ALL DEPARTMENT";
                ViewBag.DepartmentId = 0;
            }

            ViewBag.Type = "All";
            return View();
        }

        public ActionResult Type(string typeid)
        {
            var db = new WebDataModel();
            var type = db.Database.SqlQuery<V_TicketType>($"select * from V_TicketType with (nolock) where Id = '{typeid}' ").FirstOrDefault();
            var item = db.P_Department.FirstOrDefault(x => x.Id.ToString() == type.BuildInCode);
            ViewBag.Department = item.Name;
            ViewBag.DepartmentId = item.Id;
            ViewBag.Type = type.TypeName;
            return View("Index");
        }
    }
}