using Enrich.DataTransfer;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class EmailTemplateController : Controller
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();

        public ActionResult Index()
        {
            WebDataModel db = new WebDataModel();
            return View();
        }
        // GET: EmailTemplate
        public ActionResult Save(long? Id)
        {
            WebDataModel db = new WebDataModel();
            T_EmailTemplate et = new T_EmailTemplate();
            if (Id > 0)
            {
                ViewBag.title = "Edit email template #" + Id.ToString();
                et = db.T_EmailTemplate.Find(Id);
            }
            else
            {
                ViewBag.title = "Add new email template";
            }
            var emailgroups = db.T_EmailTemplateGroup.ToList();
            ViewBag.Groups = emailgroups;
            return View(et);
        }
        [HttpPost]
        public ActionResult Save(T_EmailTemplate et)
        {
            if (access.Any(k => k.Key.Equals("ticket_email_template")) == false || access["ticket_email_template"] != true)
            {
                return RedirectToAction("Index");
            }
            WebDataModel db = new WebDataModel();
            if (et.Id == 0)
            {
                et.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff"));
                et.TemplateGroupName = db.T_EmailTemplateGroup.Find(et.TemplateGroupId)?.Name ?? "";
                et.Content = Request.Unvalidated["_Content"];
                db.T_EmailTemplate.Add(et);
                TempData["s"] = "Add new email template completed";
            }
            else
            {

                var EmailTemplate = db.T_EmailTemplate.Find(et.Id);
                EmailTemplate.Name = et.Name;
                EmailTemplate.TemplateGroupId = et.TemplateGroupId;
                EmailTemplate.TemplateGroupName = db.T_EmailTemplateGroup.Find(et.TemplateGroupId)?.Name ?? "";
                EmailTemplate.Content = Request.Unvalidated["_Content"];
                db.Entry(EmailTemplate).State = System.Data.Entity.EntityState.Modified;
                TempData["s"] = "Edit email template completed";
            }
            db.SaveChanges();
            if (Request["submit"] == "close")
                return RedirectToAction("index");
            return RedirectToAction("Save");
        }
        public JsonResult savegroup(string gn, long? id)
        {
            if (access.Any(k => k.Key.Equals("ticket_email_template")) == false || access["ticket_email_template"] != true)
            {
                return Json(new object[] { false, "You don't have permission to add group!"});
            }
            if (id == null)
            {
                try
                {
                    WebDataModel db = new WebDataModel();
                    long ID = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff"));
                    var grouplist = new T_EmailTemplateGroup();
                    grouplist.Id = ID;
                    grouplist.Name = gn;
                    db.T_EmailTemplateGroup.Add(grouplist);
                    db.SaveChanges();
                    return Json(new object[] { true, "Add group completed!", grouplist });
                }
                catch (Exception ex)
                {
                    return Json(new object[] { false, "Error" + ex.Message });
                }
            }
            else
            {
                try
                {
                    WebDataModel db = new WebDataModel();
                    var grouplist = db.T_EmailTemplateGroup.Find(id);
                    grouplist.Name = gn;
                    db.Entry(grouplist).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, "Edit group completed!", grouplist });
                }
                catch (Exception ex)
                {
                    return Json(new object[] { false, "Error" + ex.Message });
                }
            }


        }
        public ActionResult Search(long group, string name)
        {
            WebDataModel db = new WebDataModel();
            var listEmailTemplate = db.T_EmailTemplate.Where(e =>
           (e.TemplateGroupId == group || group == 0) && (e.Name.Contains(name)))
           .Select(e => new Enrich.DataTransfer.T_EmailTemplate_customize { Id = e.Id, Name = e.Name, TemplateGroupName = e.TemplateGroupName }).ToList();
            return PartialView("_EmailTemplateListPartial", listEmailTemplate);
        }

        public ActionResult SearchSelect(long group, string name)
        {
            WebDataModel db = new WebDataModel();
            var listEmailTemplate = db.T_EmailTemplate.Where(e =>
           (e.TemplateGroupId == group || group == 0) && (e.Name.Contains(name)))
           .Select(e => new Enrich.DataTransfer.T_EmailTemplate_customize { Id = e.Id, Name = e.Name }).ToList();
            return PartialView("_EmailTemplateSelectPartial_ListET", listEmailTemplate);
        }
        public JsonResult DelEmailtemplate(long Id)
        {
            if (access.Any(k => k.Key.Equals("ticket_email_template")) == false || access["ticket_email_template"] != true)
            {
                return Json(new object[] { false, "You don't have permission to delete!" });

            }
            WebDataModel db = new WebDataModel();
            var et = db.T_EmailTemplate.Find(Id);
            if (et != null)
            {
                db.T_EmailTemplate.Remove(et);
                db.SaveChanges();
                return Json(new object[] { true, "Email template deleted!" });
            }
            else
            {
                return Json(new object[] { false, "selected email template not found!" });
            }
        }
        public JsonResult GetEmailContent(long Id)
        {
            WebDataModel db = new WebDataModel();
            string content = db.T_EmailTemplate.Find(Id).Content;
            return Json(content);
        }
        public JsonResult GetEmailContentReplaced(long Id, EmailTemplateContent etContent)
        {
            WebDataModel db = new WebDataModel();
            string content = db.T_EmailTemplate.Find(Id).Content;
            var codes = typeof(EmailTemplateContent).GetProperties();
            foreach(var code in codes)
            {
                content = content.Replace("{" + code.Name + "}", typeof(EmailTemplateContent).GetProperty(code.Name).GetValue(etContent, null)?.ToString());
            }
            //content = content.Replace("{BUSSINESS_ADDRESS}", etContent.BUSSINESS_ADDRESS);
            //content = content.Replace("{BUSSINESS_NAME}", etContent.BUSSINESS_NAME);
            //content = content.Replace("{BUSSINESS_PHONE}", etContent.BUSSINESS_PHONE);
            //content = content.Replace("{MERCHANT_COMMENT}", etContent.MERCHANT_COMMENT);
            //content = content.Replace("{MERCHANT_EMAIL}", etContent.MERCHANT_EMAIL);
            //content = content.Replace("{MERCHANT_NAME}", etContent.MERCHANT_NAME);
            //content = content.Replace("{MERCHANT_PHONE}", etContent.MERCHANT_PHONE);




            return Json(content);
        }
    }


}