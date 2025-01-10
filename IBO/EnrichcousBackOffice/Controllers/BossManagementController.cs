using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using Enrich.Core.Ultils;
using Enrich.IServices;
using Enrich.IServices.Utils.Mailing;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.AppConfig;
using Newtonsoft.Json;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class BossManagementController : Controller
    {
        private readonly WebDataModel db;
        private readonly Dictionary<string, bool> access;
        private readonly P_Member cMem;
        private readonly IMerchantService _merchantService;
        private readonly IMailingService _mailingService;

        public BossManagementController(IMerchantService merchantService, IMailingService mailingService)
        {
            _merchantService = merchantService;
            _mailingService = mailingService;
            db = new WebDataModel();
            access = AppLB.Authority.GetAccessAuthority();
            cMem = AppLB.Authority.GetCurrentMember();
        }

        // GET: BossManagement
        public ActionResult Index(string search)
        {
            ViewBag.p = access;
            if (access.Any(k => k.Key.Equals("boss_view")) == true && access["boss_view"] == true)
            {
                UserContent.TabHistory = "Boss management" + "|/BossManagement";

                var bossStores = db.C_BossStore.ToList();
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    bossStores = bossStores.Where(b => b.IsActive == true &&
                        ((b.Owner ?? string.Empty).ToLower().Contains(search) ||
                        (b.ContactPerson ?? string.Empty).ToLower().Contains(search) ||
                        (b.Phone ?? string.Empty).ToLower().Contains(search) ||
                        (b.Email ?? string.Empty).ToLower().Contains(search) ||
                        (b.Description ?? string.Empty).ToLower().Contains(search))
                    ).ToList();
                }
                TempData["search"] = search;
                return View(bossStores);
            }
            else
            {
                TempData["e"] = "You don't have permission";
                return View();
            }

        }

        public ActionResult ReloadIndex(string search)
        {
            if (access.Any(k => k.Key.Equals("boss_view")) == true && access["boss_view"] == true)
            {
                var bossStores = db.C_BossStore.Where(b => b.IsActive == true).ToList();
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.ToLower();
                    bossStores = bossStores.Where(b =>
                       ((b.Owner ?? string.Empty).ToLower().Contains(search) ||
                       (b.ContactPerson ?? string.Empty).ToLower().Contains(search) ||
                       (b.Phone ?? string.Empty).ToLower().Contains(search) ||
                       (b.Email ?? string.Empty).ToLower().Contains(search) ||
                       (b.Description ?? string.Empty).ToLower().Contains(search))
                    ).ToList();
                }
                TempData["search"] = search;
                return PartialView("_Partial_ListBoss", bossStores);
            }
            else
            {
                return PartialView("_Partial_ListBoss");
            }
        }

        // GET: BossManagement/Create
        public ActionResult Create(string key, string store)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("boss_addnew")) == true && access["boss_addnew"] == true)
                {
                    var bossManage = new C_BossStore { };
                    if (!string.IsNullOrEmpty(key))
                    {
                        bossManage = db.C_BossStore.Find(key);
                    }
                    var bossStore = db.C_BossStore.Where(c => !c.Id.Equals(key) && !string.IsNullOrEmpty(c.StoreCodes)).Select(b => b.StoreCodes).ToList();
                    string storesOfBoss = string.Empty;
                    bossStore.ForEach(b =>
                    {
                        storesOfBoss += "," + b;
                    });

                    var stores = (
                        from ss in db.Store_Services
                        join lp in db.License_Product on ss.ProductCode equals lp.Code
                        join lpi in db.License_Product_Item on lp.Id equals lpi.License_Product_Id
                        join li in db.License_Item on lpi.License_Item_Code equals li.Code
                        where li.Code.Equals("BossManage")
                                && lpi.Enable == true
                                && !storesOfBoss.Contains(ss.StoreCode)
                                && ss.Active == 1
                                && ss.RenewDate >= DateTime.UtcNow
                        select ss.CustomerCode).ToList();

                    //var customer = db.C_Customer.Where(c => stores.Any(s => s.Equals(c.CustomerCode))).ToList();
                    var customer = db.C_Customer.Where(c => c.Active == 1).ToList();
                    ViewBag.StoreAllow = stores;
                    var storeX = db.C_Customer.FirstOrDefault(c => c.StoreCode.Equals(store) && !string.IsNullOrEmpty(store));
                    ViewBag.StoreId = storeX?.Id;
                    TempData["ListStore"] = customer;
                    return PartialView("_Partial_AddBoss", bossManage);
                }
                else
                {
                    throw new Exception("You don't have permission");
                }
            }
            catch (Exception e)
            {
                var bossManage = new C_BossStore { };
                TempData["ListStore"] = new List<C_Customer>();
                return PartialView("_Partial_AddBoss", bossManage);
            }
        }

        // POST: BossManagement/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> Create([Bind(Include = "IdBossManage,Owner,ContactPerson,Phone,Email,Description")] C_BossStore c_BossStore)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("boss_addnew")) == true && access["boss_addnew"] == true)
                {
                    string url = AppConfig.Cfg.BossManage.Url.AddEdit;
                    var nBoss = new BossManageReq
                    {
                        idBossManage = c_BossStore.IdBossManage ?? "0",
                        owner = c_BossStore.Owner,
                        contactPerson = c_BossStore.ContactPerson,
                        phone = CommonFunc.CleanPhone(c_BossStore.Phone),
                        email = c_BossStore.Email,
                        comments = c_BossStore.Description
                    };
                    List<BossManageReq> reqBoss = new List<BossManageReq>();
                    reqBoss.Add(nBoss);
                    HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "post", reqBoss);
                    if (result.IsSuccessStatusCode)
                    {
                        string responseJson = result.Content.ReadAsStringAsync().Result;
                        responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                        BossManageRes responeData = JsonConvert.DeserializeObject<BossManageRes>(responseJson);
                        if (int.Parse(responeData.result) != 200)
                            throw new Exception(responeData.message);
                        else
                        {
                            var oldBoss = db.C_BossStore.FirstOrDefault(c => c.IdBossManage.Equals(responeData.idBossManage));
                            if (oldBoss == null)
                            {
                                var storecodes = Request["StoreCodes"] ?? string.Empty;
                                var newBoss = new C_BossStore
                                {
                                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                                    Owner = c_BossStore.Owner,
                                    ContactPerson = c_BossStore.ContactPerson,
                                    Phone = c_BossStore.Phone,
                                    Email = c_BossStore.Email,
                                    Password = responeData.password,
                                    IdBossManage = responeData.idBossManage,
                                    UrlConnect = responeData.urlConnect,
                                    StoreCodes = storecodes,
                                    Description = c_BossStore.Description,
                                    CreateAt = DateTime.UtcNow,
                                    CreateBy = cMem.FullName,
                                    IsActive = true
                                };
                                db.C_BossStore.Add(newBoss);

                                var msg = RequestAppendStoreToBoss(newBoss.Id, storecodes);
                                if (!string.IsNullOrEmpty(msg))
                                    throw new Exception(msg);
                                msg = await SendMailCreateBoss(newBoss);
                                if (!string.IsNullOrEmpty(msg))
                                    throw new Exception("Send mail fail. " + msg);
                                db.SaveChanges();
                                return Json(new object[] { true, "Create success.", newBoss });
                            }
                            else
                            {
                                var msg = RequestDeleteStoreFromBoss(oldBoss.Id);
                                if (!string.IsNullOrEmpty(msg))
                                    throw new Exception(msg);
                                var storecodes = Request["StoreCodes"] ?? string.Empty;
                                oldBoss.Owner = c_BossStore.Owner;
                                oldBoss.ContactPerson = c_BossStore.ContactPerson;
                                oldBoss.Phone = c_BossStore.Phone;
                                oldBoss.Password = responeData.password;
                                oldBoss.IdBossManage = responeData.idBossManage;
                                oldBoss.StoreCodes = storecodes;
                                oldBoss.Description = c_BossStore.Description;
                                oldBoss.UpdateAt = DateTime.UtcNow;
                                oldBoss.UpdateBy = cMem.FullName;
                                db.Entry(oldBoss).State = EntityState.Modified;

                                msg = RequestAppendStoreToBoss(oldBoss.Id, storecodes);
                                if (!string.IsNullOrEmpty(msg))
                                    throw new Exception(msg);

                                msg = await SendMailCreateBoss(oldBoss);
                                if (!string.IsNullOrEmpty(msg))
                                    throw new Exception("Send mail fail. " + msg);

                                db.SaveChanges();
                                return Json(new object[] { true, "Update success." });
                            }
                        }
                    }
                    else
                        throw new Exception("SIMPLY POS system not responding!");
                }
                else
                {
                    throw new Exception("You don't have permission");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Create boss fail. " + ex.Message });
            }
        }
        public ActionResult Details(string key)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("boss_view")) == true && access["boss_view"] == true)
                {
                    var bossManage = db.C_BossStore.Find(key);
                    if (bossManage == null)
                        throw new Exception("Boss not found");

                    var list_customer = db.C_Customer.Where(c => bossManage.StoreCodes.Contains(c.StoreCode)).ToList();

                    var viewModel = (from c in list_customer.OrderByDescending(x => x.CreateAt)
                                     join s in (from sub in db.Store_Services where sub.Active == 1 && sub.Type == "license" select sub) on c.CustomerCode equals s.CustomerCode into gj
                                     from s in gj.DefaultIfEmpty()
                                     select new Merchant_IndexView { Customer = c, Remaning = (s?.RenewDate != null) ? (int?)(s?.RenewDate.Value.Date - DateTime.UtcNow.Date).Value.Days : null, RenewDate = s?.RenewDate }).ToList();

                    ViewBag.Customers = viewModel;
                    return PartialView("_Partial_DetailBoss", bossManage);
                }
                else
                {
                    throw new Exception("You don't have permission");
                }
            }
            catch (Exception)
            {
                TempData["ListStore"] = new List<C_BossStore>();
                return PartialView("_Partial_DetailBoss");
            }
        }
        public async Task<JsonResult> ChangePassword(string key)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("boss_update")) == true && access["boss_update"] == true)
                {
                    var boss = db.C_BossStore.Find(key);
                    if (boss == null)
                        throw new Exception("Boss not found");
                    var newpass = Request["password"] ?? string.Empty;
                    var msg = RequestChangePassword(boss.Id, newpass);
                    if (!string.IsNullOrEmpty(msg))
                        throw new Exception(msg);

                    boss.Password = newpass;

                    msg = await SendMailChangePassword(boss);
                    if (!string.IsNullOrEmpty(msg))
                        throw new Exception("Send mail fail. " + msg);

                    db.Entry(boss).State = EntityState.Modified;
                    db.SaveChanges();

                    return Json(new object[] { true, "Update success." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("You don't have permission");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Change password fail. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AddToBoss(string store, string key)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("boss_update")) == true && access["boss_update"] == true)
                {
                    var boss = db.C_BossStore.FirstOrDefault(b => b.IdBossManage.Equals(key));
                    if (boss == null)
                        throw new Exception("Boss not found");
                    var storeX = db.C_Customer.FirstOrDefault(s => s.StoreCode.Equals(store));
                    if (storeX == null)
                        throw new Exception("Store not found");
                    var msg = RequestAppendStoreToBoss(boss.Id, storeX.StoreCode);
                    if (!string.IsNullOrEmpty(msg))
                        throw new Exception(msg);

                    boss.StoreCodes += ((string.IsNullOrEmpty(boss.StoreCodes) ? "" : ",") + storeX.StoreCode);
                    db.Entry(boss).State = EntityState.Modified;
                    db.SaveChanges();
                    var cus = db.C_Customer.FirstOrDefault(c => c.StoreCode == store);
                    _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Merchant join to their boss");
                    new MerchantService().WriteLogMerchant(cus, "Merchant join to their boss", "Merchant has been join to their boss <b>" + boss.Owner + "</b>");
                    return Json(new object[] { true, "Update success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("You don't have permission");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Update fail. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult StoreLeaveBoss(string store, string key)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("boss_update")) == true && access["boss_update"] == true)
                {
                    var boss = db.C_BossStore.FirstOrDefault(b => b.IdBossManage.Equals(key));
                    if (boss == null)
                        throw new Exception("Boss not found");
                    var storeX = db.C_Customer.FirstOrDefault(s => s.StoreCode.Equals(store));
                    if (storeX == null)
                        throw new Exception("Store not found");

                    var msg = RequestLeaveBoss(storeX.StoreCode);
                    if (!string.IsNullOrEmpty(msg))
                        throw new Exception(msg);
                    if (!string.IsNullOrEmpty(boss.StoreCodes))
                    {
                        var listStores = boss.StoreCodes.Split(',').ToList();
                        listStores.RemoveAt(listStores.IndexOf(storeX.StoreCode));
                        boss.StoreCodes = string.Join(",", listStores);
                        db.Entry(boss).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    var cus = db.C_Customer.FirstOrDefault(c => c.StoreCode == store);
                    _merchantService.SaveHistoryUpdate(cus.CustomerCode, "Merchant leave their boss");
                    new MerchantService().WriteLogMerchant(cus, "Merchant leave their boss", "Merchant has been leave their boss <b>" + boss.Owner + "</b>");
                    return Json(new object[] { true, "Update success" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    throw new Exception("You don't have permission");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Update fail. " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public string RequestAppendStoreToBoss(string id, string stores)
        {
            try
            {
                if (!string.IsNullOrEmpty(stores))
                {
                    var boss = db.C_BossStore.Find(id);
                    if (boss == null)
                        throw new Exception("Boss not found");
                    var reqStores = stores.Split(',').Select(s => new
                    {
                        idBossManage = boss.IdBossManage,
                        idims = s
                    }).ToList();

                    if (reqStores.Count > 0)
                    {
                        string url = AppConfig.Cfg.BossManage.Url.AddStore;
                        HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "post", reqStores);
                        if (result.IsSuccessStatusCode)
                        {
                            string responseJson = result.Content.ReadAsStringAsync().Result;
                            responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                            BossManageRes responeData = JsonConvert.DeserializeObject<BossManageRes>(responseJson);
                            if (int.Parse(responeData.result) != 200)
                                throw new Exception(responeData.message);
                        }
                        else
                            throw new Exception("SIMPLY POS system not responding!");
                    }
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public string RequestDeleteStoreFromBoss(string id)
        {
            try
            {
                var boss = db.C_BossStore.Find(id);
                if (boss == null)
                    throw new Exception("Boss not found");
                string url = AppConfig.Cfg.BossManage.Url.DelStore + boss.IdBossManage;
                HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "get", null);
                if (result.IsSuccessStatusCode)
                {
                    string responseJson = result.Content.ReadAsStringAsync().Result;
                    responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                    BossManageRes responeData = JsonConvert.DeserializeObject<BossManageRes>(responseJson);
                    if (int.Parse(responeData.result) != 200)
                        throw new Exception(responeData.message);
                }
                else
                    throw new Exception("SIMPLY POS system not responding!");
                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public string RequestChangePassword(string id, string newpass)
        {
            try
            {
                var reqStores = db.C_BossStore.Where(c => c.Id.Equals(id)).Select(b => new
                {
                    idBossManage = b.IdBossManage,
                    newPass = newpass
                }).ToList();

                if (reqStores.Count > 0)
                {
                    string url = AppConfig.Cfg.BossManage.Url.ChangePass;
                    HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "post", reqStores);
                    if (result.IsSuccessStatusCode)
                    {
                        string responseJson = result.Content.ReadAsStringAsync().Result;
                        responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                        BossManageRes responeData = JsonConvert.DeserializeObject<BossManageRes>(responseJson);
                        if (int.Parse(responeData.result) != 200)
                            throw new Exception(responeData.message);
                    }
                    else
                        throw new Exception("SIMPLY POS system not responding!");
                }
                else
                    throw new Exception("Boss not found");

                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string RequestLeaveBoss(string storeCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(storeCode))
                {
                    string url = AppConfig.Cfg.BossManage.Url.LeaveBoss + storeCode;
                    HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "get", null);
                    if (result.IsSuccessStatusCode)
                    {
                        string responseJson = result.Content.ReadAsStringAsync().Result;
                        responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                        BossManageRes responeData = JsonConvert.DeserializeObject<BossManageRes>(responseJson);
                        if (int.Parse(responeData.result) != 200)
                            throw new Exception(responeData.message);
                    }
                    else
                        throw new Exception("SIMPLY POS system not responding!");
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public JsonResult Delete(string key)
        {
            try
            {
                if (cMem == null)
                {
                    throw new Exception("You don't have permission!");
                }
                var del = db.C_BossStore.Find(key);
                if (del == null)
                    throw new Exception("Boss not found");

                string msg = RequestDeleteStoreFromBoss(del.Id);
                if (!string.IsNullOrEmpty(msg))
                    throw new Exception(msg);

                del.IsActive = false;
                del.StoreCodes = string.Empty;
                db.Entry(del).State = EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Remove " + del.Owner + " successfully" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Remove error: " + e.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public async Task<string> SendMailCreateBoss(C_BossStore boss)
        {
            try
            {
                var email = boss.Email;
                var name = boss.Owner;
                //if (string.IsNullOrEmpty(boss?.StoreCodes))
                //    throw new Exception("Create success, missing send email for Boss");

                var Stores = db.C_Customer.Where(c => boss.StoreCodes.Contains(c.StoreCode))
                    .Select(c => new
                    {
                        Store_Name = c.BusinessName,
                        Store_Address = c.BusinessAddressStreet + ", " + c.BusinessCity + ", " + c.BusinessState + ", " + c.BusinessZipCode + ", " + c.BusinessCountry
                    }).ToList();
                if (Stores.Count == 0)
                    throw new Exception(string.Empty);

                var dataMail = new
                {
                    ContactPerson = boss.ContactPerson,
                    Owner = boss.Owner,
                    Email = boss.Email,
                    Password = boss.Password,
                    UrlConnect = boss.UrlConnect,
                    List_Store = Stores
                };

                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/sendgrid_template/boss_create");
                string result = await _mailingService.SendBySendGridWithTemplate(email, name, node["template_id"].InnerText, "", dataMail, "");
                if (!string.IsNullOrEmpty(result))
                    throw new Exception(result);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<string> SendMailChangePassword(C_BossStore boss)
        {
            try
            {
                var email = boss.Email;
                var name = boss.Owner;
                var dataMail = new
                {
                    ContactPerson = boss.ContactPerson,
                    Owner = boss.Owner,
                    Email = boss.Email,
                    Password = boss.Password,
                    UrlConnect = boss.UrlConnect
                };
                ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
                XmlNode node = xml.GetNode("/root/sendgrid_template/boss_changepassword");
                string result = await _mailingService.SendBySendGridWithTemplate(email, name, node["template_id"].InnerText, "", dataMail, "");
                if (!string.IsNullOrEmpty(result))
                    throw new Exception(result);
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // POST: BossManagement/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            C_BossStore c_BossStore = db.C_BossStore.Find(id);
            db.C_BossStore.Remove(c_BossStore);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
