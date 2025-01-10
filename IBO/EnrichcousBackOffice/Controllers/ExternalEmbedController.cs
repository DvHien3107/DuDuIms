using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class ExternalEmbedController : Controller
    {
        WebDataModel db = new WebDataModel();
        private P_Member cMem = Authority.GetCurrentMember();
        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        // GET: ExternalEmbed
        public ActionResult Index()
        {
            if (!cMem.RoleCode.Contains("admin"))
            {
                return Redirect("/Home/forbidden");
            }
            var model = db.External_Embed.ToList();
            return View(model);
        }
        public ActionResult Open(string id)
        {
            var embed = db.External_Embed.Find(id);
            if (!access.Any(k => k.Key.Equals(embed.FunctionCode)) || access[embed.FunctionCode] != true || embed.Visible != true)
            {
                return Redirect("/Home/forbidden");
            }
            return View(embed);
        }
        public JsonResult new_embed(string Name)
        {
            if (!cMem.RoleCode.Contains("admin"))
            {
                return Json(new object[] { false, "Forbidden!" });
            }
            if (db.External_Embed.Any(e => e.Name.ToLower() == Name.ToLower()))
            {
                return Json(new object[] { false, "The name \"" + Name + "\" has been used by others!" });
            }
            try
            {
                var function_code = "other_" + Guid.NewGuid().ToString("N");
                var scp = Request.Unvalidated["EmbedScript"]??"";
                try
                {
                    var _scp = scp.Replace(" ", "");
                    _scp = Regex.Split(_scp, "src=\"", RegexOptions.IgnoreCase)[1];
                    _scp = _scp.Split('\"')[0];
                    _scp = "<iframe class=\"iframe_fullpage\" frameborder=0 src=\"" + _scp + "\"></iframe>";
                    scp = _scp;
                }
                catch { }
                
                var new_embed = new External_Embed
                {
                    Id = Guid.NewGuid().ToString("N"),
                    EmbedScript = scp,
                    Name = Name,
                    FunctionCode = function_code,
                    CreateAt = DateTime.Now,
                    CreateBy = cMem.FullName,
                    Visible = true,

                };
                var function = new A_FunctionInPage
                {
                    FunctionCode = function_code,
                    FunctionName = Name,
                    PageCode = "more_features",
                };
                db.External_Embed.Add(new_embed);
                db.A_FunctionInPage.Add(function);
                db.SaveChanges();
                return Json(new object[] { true, "Create new embed completed", new_embed });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Create new embed Fail!", e.Message });
            }

        }
        public JsonResult Rename_embed(string id, string name)
        {
            if (!cMem.RoleCode.Contains("admin"))
            {
                return Json(new object[] { false, "Forbidden!" });
            }
            if (db.External_Embed.Any(e => e.Id != id && e.Name.ToLower() == name.ToLower()))
            {
                return Json(new object[] { false, "The name \"" + name + "\" has been used by others!" });
            }
            try
            {
                var embed = db.External_Embed.Find(id);
                var function = db.A_FunctionInPage.Find(embed?.FunctionCode);
                embed.Name = name;
                function.FunctionName = name;
                db.Entry(embed).State = System.Data.Entity.EntityState.Modified;
                db.Entry(function).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Rename embed completed", embed });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Rename embed Fail!", e.Message });
            }
        }

        [ValidateInput(false)]
        public JsonResult save_embedScript(string id, string embed_script)
        {
            if (!cMem.RoleCode.Contains("admin"))
            {
                return Json(new object[] { false, "Forbidden!" });
            }
            try
            {
                var embed = db.External_Embed.Find(id);
                embed.EmbedScript = embed_script;
                db.Entry(embed).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Save embed script completed", embed_script });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Save embed script Fail!", e.Message });
            }
        }
        public JsonResult SetVissibleEmbed(string id, bool status)
        {
            if (!cMem.RoleCode.Contains("admin"))
            {
                return Json(new object[] { false, "Forbidden!" });
            }
            try
            {
                var embed = db.External_Embed.Find(id);
                if (embed == null)
                {
                    return Json(new object[] { false, "Embed not found!" });
                }
                embed.Visible = status;
                db.Entry(embed).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "\"" + embed.Name + "\" visible is " + (status ? "ON" : "OFF"), status });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Set visible status Fail!", e.Message });
            }

        }
        public JsonResult DeleteEmbed(string id)
        {
            if (!cMem.RoleCode.Contains("admin"))
            {
                return Json(new object[] { false, "Forbidden!" });
            }
            try
            {
                var embed = db.External_Embed.Find(id);
                if (embed == null)
                {
                    return Json(new object[] { false, "Embed not found!" });
                }
                db.External_Embed.Remove(embed);
                var function = db.A_FunctionInPage.Find(embed.FunctionCode);
                if (function != null)
                {
                    db.A_FunctionInPage.Remove(function);
                }
                var accs = db.A_GrandAccess.Where(a => a.FunctionCode == embed.FunctionCode).ToList();
                if (accs != null)
                {
                    db.A_GrandAccess.RemoveRange(accs);
                }
                db.SaveChanges();
                return Json(new object[] { true, "Delete \"" + embed.Name + "\" visible completed" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Delete Fail!", e.Message });
            }

        }
        public static List<External_Embed> GetEmbedList()
        {
            var _db = new WebDataModel();
            return _db.External_Embed.Where(e => e.Visible == true).ToList() ?? new List<External_Embed>();
        }
    }
}