using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Areas.Page.Controllers
{

    [Authorize]
    public class FormController : Controller
    {

        private P_Member Cmem = AppLB.Authority.GetCurrentMember();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="mc">merchant code</param>
        /// <param name="mn">member numnber</param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Index(string mc, string mn, string t = "form")
        {
            var db = new WebDataModel();
            List<Form_CustomizeItems> customizeForms = new List<Form_CustomizeItems>();
            var form = db.Form_CustomizeRequest.Where(f => f.MemberNumber == mn && f.CustomerCode == mc && f.Status == "completed").FirstOrDefault();
            if (form != null)
            {
                customizeForms = db.Form_CustomizeItems.Where(f => f.Code == form.Code).OrderBy(f => f.Index).ToList();
                if (t != "view")
                {
                    Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION_TITLE + Cmem.MemberNumber] = form.Title;
                    Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + Cmem.MemberNumber] = customizeForms;
                }
            }
            ViewBag.merchant = db.C_Customer.Where(c => c.CustomerCode.Equals(mc)).FirstOrDefault();
            ViewBag.form_title = (form?.Title)??"TITLE";
            return View(customizeForms.OrderBy(f => f.Index));
        }
        


        public JsonResult AddUpdateQuestion(string id, string question, string type = "text")
        {
            try
            {
                List<Form_CustomizeItems> customizeForms = new List<Form_CustomizeItems>();
                if (Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + Cmem.MemberNumber] != null)
                {
                    customizeForms = Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + Cmem.MemberNumber] as List<Form_CustomizeItems>;
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    //add new
                    customizeForms.Add(new Form_CustomizeItems { Id = Guid.NewGuid().ToString(), Question = question, Type = type, Index = (customizeForms.Count + 1) });
                }
                else
                {
                    //update
                    var updQuestion = customizeForms.Where(q => q.Id == id).FirstOrDefault();
                    updQuestion.Question = question;
                }
                Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + Cmem.MemberNumber] = customizeForms;
                return Json(new object[] { true, "", customizeForms.OrderBy(f => f.Index) });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message, null });
            }


        }

        [HttpPost]
        public JsonResult RemoveQuestion(string id)
        {
            try
            {
                List<Form_CustomizeItems> customizeForms = new List<Form_CustomizeItems>();
                if (Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + Cmem.MemberNumber] != null)
                {
                    customizeForms = Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + Cmem.MemberNumber] as List<Form_CustomizeItems>;
                }


                var rmQuestion = customizeForms.Where(q => q.Id == id).FirstOrDefault();
                foreach (var item in customizeForms.OrderBy(q => q.Index).Skip(rmQuestion.Index??1))
                {
                    item.Index = item.Index - 1;
                }

                customizeForms.Remove(rmQuestion);
                Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + Cmem.MemberNumber] = customizeForms;

                return Json(new object[] { true, "", customizeForms.OrderBy(f => f.Index) });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message, null });
            }

        }

        public JsonResult UpdateTitle(string title)
        {

            Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION_TITLE + Cmem.MemberNumber] = title;
            return Json(new object[] { true });

        }


    }

    public class FormCustomizeHandle
    {

        /// <summary>
        /// luu vao database
        /// </summary>
        /// <param name="mc">merchant code/customer code</param>
        /// <param name="mn">member number</param>
        /// <param name="errMsg"></param>
        /// <returns></returns>
        public static Form_CustomizeRequest UpdateCustomizeForm(string mc, string mn, out string errMsg)
        {
            errMsg = string.Empty;
            

            List<Form_CustomizeItems> customizeForms = new List<Form_CustomizeItems>();
            if (HttpContext.Current.Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + mn] != null)
            {
                customizeForms = HttpContext.Current.Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION + mn] as List<Form_CustomizeItems>;
            }

            if (customizeForms == null || customizeForms.Count == 0)
            {
                errMsg = "Form is not found";
                return null;
            }

            var db = new WebDataModel();
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var thefirstItem = customizeForms.FirstOrDefault();
                    var customize = db.Form_CustomizeRequest.Where(f => f.Code == thefirstItem.Code).FirstOrDefault();
                    if (customize == null)
                    {
                        customize = new Form_CustomizeRequest
                        {
                            CustomerCode = mc,
                            MemberNumber = mn,
                            Status = "completed",
                            Title = HttpContext.Current.Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION_TITLE + mn]?.ToString(),
                            Code = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + mc + "_" + mn
                        };
                        db.Form_CustomizeRequest.Add(customize);
                    }
                    else
                    {
                        customize.Title = HttpContext.Current.Session[AppLB.UserContent.CUSTOMIZE_FORM_SESSION_TITLE + mn]?.ToString();
                        foreach (var item in db.Form_CustomizeItems.Where(f=>f.Code == customize.Code))
                        {
                            db.Form_CustomizeItems.Remove(item);
                        }
                    }
                   
                   
                    foreach (var item in customizeForms)
                    { 
                        var customizeItems = new Form_CustomizeItems
                        {
                            Code = customize.Code,
                            Id = item.Id,
                            Index = item.Index,
                            Question = item.Question,
                            Type = item.Type
                        };
                        db.Form_CustomizeItems.Add(customizeItems);
                       
                    }
                    db.SaveChanges();
                    tran.Commit();

                    HttpContext.Current.Session.Remove(AppLB.UserContent.CUSTOMIZE_FORM_SESSION_TITLE + mn);
                    HttpContext.Current.Session.Remove(AppLB.UserContent.CUSTOMIZE_FORM_SESSION + mn);
                    return customize;
                }
                catch (Exception e)
                {
                    tran.Dispose();
                    errMsg = (e.InnerException?.Message) ?? e.Message;
                    return null;
                }
            }


        }
    }
}