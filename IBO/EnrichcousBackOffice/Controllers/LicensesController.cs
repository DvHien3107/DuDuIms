using Enrich.Core.Infrastructure;
using Enrich.Core.UnitOfWork.Data;
using Enrich.DataTransfer;
using Enrich.IServices;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Models.Proc;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewControler;
using Inner.Libs.Helpful;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class LicensesController : Controller
    {
        private readonly IMerchantService _merchantService;
        private readonly ILogService _logService;
        private readonly EnrichContext _enrichContext;
        public LicensesController(IMerchantService merchantService, ILogService logService, EnrichContext enrichContext)
        {
            _merchantService = merchantService;
            _logService = logService;
            _enrichContext = enrichContext;
        }

        // GET: Licenses
        WebDataModel db = new WebDataModel();
        private string pathLog = "/App_Data/Logs/";
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = Authority.GetCurrentMember();
        static HttpClient client = new HttpClient();
        public ActionResult LicenseItems()
        {
            _logService.Info($"[Licenses][LicenseItems] start ");
            try
            {
                var Groups = (
       from Group in db.License_Item_Group
       join Items in from Item in db.License_Item
                     where Item.Enable == true
                     group Item by Item.GroupID
       on Group.ID equals Items.Key into ps
       from p in ps.DefaultIfEmpty()
       select new Child_LicensesItemGroup
       {
           Group = Group,
           Items = p.ToList()
       }).ToList();

                var GGroups = Groups.Where(g => g.Group.ParentID == null).ToList().OrderBy(g => g.Group.Name);
                var hierGroups = (
                                  from c in GGroups
                                  join gs in from g in Groups
                                             where g.Group.ParentID != null
                                             group g by g.Group.ParentID
                                  on c.Group.ID equals gs.Key into ps
                                  from p in ps.DefaultIfEmpty()
                                  select new LicensesItemGroup
                                  {
                                      Group = c.Group,
                                      Items = c.Items,
                                      ChildGroups = p != null ? p.ToList() : new List<Child_LicensesItemGroup>()
                                  }).ToList();

                _logService.Info($"[Licenses][LicenseItems] complete");
                return View(hierGroups);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][LicenseItems] error ");
                throw;
            }

        }
        public JsonResult UpdateMangoPosFeature()
        {
            _logService.Info($"[Licenses][UpdateMangoPosFeature] start Update Feature SIMPLY POS");
            try
            {
                new FeatureService().GetDefineFeature();
                List<License_Item> features = db.License_Item.Where(l => l.GroupID == 1000000 && l.Enable == true).ToList();
                var featureView = features.Select(f => new
                {
                    ID = f.ID.ToString(),
                    GroupID = f.GroupID,
                    GroupName = f.GroupName,
                    Name = f.Name,
                    Type = f.Type,
                    Enable = f.Enable,
                    Description = f.Description,
                    BuiltIn = f.BuiltIn,
                    Code = f.Code
                });
                _logService.Info($"[Licenses][UpdateSimplyPosFeature] completed get Feature SIMPLY POS", new { featureView = Newtonsoft.Json.JsonConvert.SerializeObject(featureView) });
                return Json(new object[] { true, "Update Feature SIMPLY POS Completed", featureView }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][UpdateSimplyPosFeature] error Update Feature SIMPLY POS");
                return Json(new object[] { false, ex.Message, null }, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult LicenseProductAddon_Partial(bool isAddon = false, string tabSelected = "license")
        {
            _logService.Info($"[Licenses][LicenseProductAddon_Partial] start");
            try
            {
                var mTypeLicense = LicenseType.LICENSE.Text();
                var mTypeAddon = LicenseType.ADD_ON.Text();
                var mGiftCard = LicenseType.GiftCard.Text();
                var mTypeOther = LicenseType.VirtualHardware_Other.Text();
                string TypeSelected = "";
                switch (tabSelected)
                {

                    case "license":
                        TypeSelected = mTypeLicense;
                        break;
                    case "addon":
                        TypeSelected = mTypeAddon;
                        break;
                    case "giftcard":
                        TypeSelected = mGiftCard;
                        break;
                    default:
                        TypeSelected = mTypeOther;
                        break;

                }

                var Groups = (
                    from Group in db.License_Item_Group
                    join Items in from Item in db.License_Item where Item.Enable == true group Item by Item.GroupID
                    on Group.ID equals Items.Key into ps
                    from p in ps.DefaultIfEmpty()
                    select new Child_LicensesItemGroup
                    {
                        Group = Group,
                        Items = p.ToList()
                    }).ToList();

                var GGroups = Groups.Where(g => g.Group.ParentID == null && g.Group.Options != null).ToList();
                var hierGroups = (
                                  from c in GGroups
                                  join gs in from g in Groups
                                             where g.Group.ParentID != null
                                             group g by g.Group.ParentID
                                  on c.Group.ID equals gs.Key into ps
                                  from p in ps.DefaultIfEmpty()
                                  select new LicensesItemGroup
                                  {
                                      Group = c.Group,
                                      Items = c.Items,
                                      ChildGroups = p != null ? p.ToList() : new List<Child_LicensesItemGroup>()
                                  }).ToList();

                var Products = (from P in db.License_Product
                                where P.Type == TypeSelected && P.IsDelete != true
                                join Gi in from item in db.License_Product_Item group item by item.License_Product_Id
                                           on P.Id equals Gi.Key into ps
                                from p in ps.DefaultIfEmpty()
                                select new LicenseProductView
                                {
                                    Product = P,
                                    Items = p.ToList()
                                }).OrderBy(x => x.Product.Type == mTypeLicense ? 1 : x.Product.Type == mTypeAddon ? 2 : x.Product.Type == mTypeOther ? 3 : 4).ThenByDescending(x => x.Product.Available).ThenByDescending(x => x.Product.Level.HasValue).ThenBy(x => x.Product.Level).ThenBy(x => x.Product.Price).ToList();
                ViewBag.Products = Products;

                Products.ForEach(p =>
                {
                    p.NumberUpdated = CheckHistoryStoreActivedProduct(p.Product.Id);
                    var resArray = p.NumberUpdated.Split('/');
                    p.UpdateStore = !(resArray[0].Equals(resArray[1]));
                });
                _logService.Info($"[Licenses][LicenseProductAddon_Partial] completed");
                return PartialView("_Partial_AddonTable", hierGroups);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][LicenseProductAddon_Partial] error");
                throw;
            }

        }
        #region Ajax
        public JsonResult GetLicensesItem(long id)
        {
            _logService.Info($"[Licenses][GetLicensesItem] start id:{id}");
            try
            {
                var item = db.License_Item.Find(id);
                if (item == null)
                {
                    _logService.Info($"[Licenses][GetLicensesItem] licenses item not found!");
                    return Json(new object[] { false, "Licenses Item not found!" });
                }
                _logService.Info($"[Licenses][GetLicensesItem] completed", new { item = Newtonsoft.Json.JsonConvert.SerializeObject(item) });
                return Json(new object[] { true, item.ID.ToString(), item.GroupID.ToString(), item });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][GetLicensesItem] error");
                throw;
            }

        }
        public JsonResult GetLicenseProduct(string id)
        {
            _logService.Info($"[Licenses][GetLicenseProduct] start id:{id}");
            try
            {
                var item = db.License_Product.Find(id);
                item.Trial_Months = item.Trial_Months ?? 0;
                item.DeploymentTiketAuto = item.DeploymentTiketAuto ?? false;
                if (item == null)
                {
                    _logService.Info($"[Licenses][GetLicenseProduct] Licenses Product not found");
                    return Json(new object[] { false, "Licenses Product not found!" });
                }
                return Json(new object[] { true, item.Id.ToString(), item });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][GetLicenseProduct] error");
                throw;
            }

        }
        public JsonResult EnableItem(long id, bool is_enable)
        {
            _logService.Info($"[Licenses][EnableItem] start id:{id} - is_enable:{is_enable} ");
            try
            {
                var Item = db.License_Item.Find(id);
                if (Item == null)
                {
                    _logService.Warning($"[Licenses][EnableItem] Item not found!");
                    throw new Exception("Item not found!");
                }
                if (Item.BuiltIn == true)
                {
                    _logService.Warning($"[Licenses][EnableItem] Can't edit BuiltIn License Items!");
                    throw new Exception("Can't edit BuiltIn License Items!");
                }
                Item.Enable = is_enable;
                db.SaveChanges();
                _logService.Info($"[Licenses][EnableItem] complete item update completed");
                return Json(new object[] { true, "Item update completed!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][EnableItem] error");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult SaveItem(long? ID, License_Item item)
        {
            _logService.Info($"[Licenses][SaveItem] start ID:{ID}", new { item = Newtonsoft.Json.JsonConvert.SerializeObject(item) });
            try
            {
                if (ID == null)
                {
                    if (db.License_Item.Any(g => g.Code == item.Code))
                    {
                        _logService.Warning($"[Licenses][SaveItem] Code licenses item already exists ", new { item = Newtonsoft.Json.JsonConvert.SerializeObject(item) });
                        throw new Exception("Code licenses item already exists!");
                    }
                    item.ID = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + new Random().Next(1, 9999).ToString().PadLeft(4, '0'));
                    item.GroupName = db.License_Item_Group.Find(item.GroupID).Name;
                    item.Enable = true;

                    db.License_Item.Add(item);
                    db.SaveChanges();
                    _logService.Info($"[Licenses][SaveItem] complete new item added");
                    return Json(new object[] { 1, "New item added!", item.ID.ToString(), item });
                }
                else
                {
                    if (db.License_Item.Any(g => g.Code == item.Code && g.ID != item.ID))
                    {
                        _logService.Warning($"[Licenses][SaveItem] Code licenses item already exists!", new { item = Newtonsoft.Json.JsonConvert.SerializeObject(item) });
                        throw new Exception("Code licenses item already exists!");
                    }

                    var editing_Item = db.License_Item.Find(item.ID);
                    if (editing_Item == null)
                    {
                        _logService.Warning($"[Licenses][SaveItem] Item not found!", new { item = Newtonsoft.Json.JsonConvert.SerializeObject(item) });
                        throw new Exception("Item not found!");
                    }
                    if (editing_Item.BuiltIn != true)
                    {
                        editing_Item.Code = item.Code;
                        editing_Item.Type = item.Type;
                        //throw new Exception("Can't edit BuiltIn License Items!");
                    }
                    editing_Item.Name = item.Name;
                    editing_Item.Description = item.Description;
                    db.Entry(editing_Item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    _logService.Info($"[Licenses][SaveItem] compeled item update");
                    return Json(new object[] { 2, "Item update compeled!", editing_Item });
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][SaveItem] error ID:{ID}");
                return Json(new object[] { 0, ex.Message });
            }
        }
        public JsonResult EditGroup(long id, string newname)
        {
            _logService.Info($"[Licenses][EditGroup] start id:{id} - newname: {newname}");
            try
            {
                if (db.License_Item_Group.Any(g => g.Name == newname && g.ID != id))
                {
                    _logService.Warning($"[Licenses][EditGroup] This name already exists id:{id} - newname: {newname}");
                    throw new Exception("This name already exists!");
                }
                var group = db.License_Item_Group.Find(id);
                if (group == null)
                {
                    _logService.Warning($"[Licenses][EditGroup] Group not found! id:{id} - newname: {newname}");
                    throw new Exception("Group not found!");
                }
                group.Name = newname;
                db.Entry(group).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Group name changed!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][EditGroup] error id:{id} - newname: {newname}");
                return Json(new object[] { false, ex.Message });
            }

        }
        public JsonResult EditProduct(string id, string newname)
        {
            _logService.Info($"[Licenses][EditProduct] start id:{id} - newname: {newname}");
            try
            {
                if (db.License_Product.Any(g => g.Name == newname && g.Id != id))
                {
                    _logService.Warning($"[Licenses][EditProduct] This name already exists");
                    throw new Exception("This name already exists!");
                }
                var product = db.License_Product.Find(id);
                if (product == null)
                {
                    _logService.Warning($"[Licenses][EditProduct] License product not found");
                    throw new Exception("License product not found!");
                }
                product.Name = newname;
                db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                _logService.Info($"[Licenses][EditProduct] completed");
                return Json(new object[] { true, "License product name changed!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][EditProduct] start id:{id} - newname: {newname}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult NewGroup(string newname, long? parentID)
        {
            _logService.Info($"[Licenses][NewGroup] start newname:{newname} - parentID: {parentID}");
            try
            {
                if (db.License_Item_Group.Any(g => g.Name == newname))
                {
                    _logService.Warning($"[Licenses][NewGroup] This name already exists");
                    throw new Exception("This name already exists!");
                }
                License_Item_Group group = new License_Item_Group();
                group.ID = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + new Random().Next(1, 9999).ToString().PadLeft(4, '0'));
                group.Name = newname;
                group.Options = "Product,Addon";
                if (parentID != null)
                {
                    group.ParentID = parentID;
                    group.ParentName = db.License_Item_Group.Find(parentID).Name;
                }
                db.License_Item_Group.Add(group);
                db.SaveChanges();
                _logService.Info($"[Licenses][NewGroup] complete");
                return Json(new object[] { true, "New group added!", group.ID.ToString(), group.Name });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][NewGroup] error");
                return Json(new object[] { false, ex.Message });
            }

        }
        public JsonResult NewProduct(string newname, bool isAddon)
        {
            _logService.Info($"[Licenses][NewProduct] start newname:{newname} - isAddon: {isAddon}");
            try
            {
                if (db.License_Product.Any(g => g.Name == newname))
                {
                    _logService.Warning($"[Licenses][NewProduct] This name already exists");
                    throw new Exception("This name already exists!");
                }
                License_Product product = new License_Product();
                product.Id = Guid.NewGuid().ToString("N");
                product.Name = newname;
                product.isAddon = isAddon;
                db.License_Product.Add(product);
                db.SaveChanges();
                _logService.Info($"[Licenses][NewProduct] complete");
                return Json(new object[] { true, "New item added!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][NewProduct] error");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult DeleteGroup(long id)
        {
            _logService.Info($"[Licenses][DeleteGroup] start id:{id}");
            try
            {
                var Group = db.License_Item_Group.Find(id);
                if (Group == null)
                {
                    _logService.Warning($"[Licenses][DeleteGroup] group not found");
                    throw new Exception("Group not found!");
                }
                db.License_Item_Group.Remove(Group);
                var ChildGroups = db.License_Item_Group.Where(g => g.ParentID == id).AsQueryable();
                if (ChildGroups.Count() > 0)
                {
                    db.License_Item_Group.RemoveRange(ChildGroups);
                }
                var Items = db.License_Item.Where(i => i.GroupID == id || ChildGroups.Any(cg => cg.ID == i.GroupID)).AsQueryable();

                if (Items.Count() > 0)
                {
                    db.License_Item.RemoveRange(Items);
                    var product_license_items = db.License_Product_Item.Where(li => Items.Any(i => i.Code == li.License_Item_Code)).AsQueryable();
                    if (product_license_items.Count() > 0)
                    {
                        db.License_Product_Item.RemoveRange(product_license_items);
                    }
                }
                db.SaveChanges();
                _logService.Warning($"[Licenses][DeleteGroup] complete");
                return Json(new object[] { true, "Group deleted!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][DeleteGroup] error");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult DeleteItem(long id)
        {
            _logService.Info($"[Licenses][DeleteItem] start id:{id}");
            try
            {
                var Item = db.License_Item.Find(id);
                if (Item == null)
                {
                    _logService.Warning($"[Licenses][DeleteItem] item not found!");
                    throw new Exception("Item not found!");
                }
                if (Item.BuiltIn == true)
                {
                    _logService.Warning($"[Licenses][DeleteItem] Can't delete BuiltIn License Items");
                    throw new Exception("Can't delete BuiltIn License Items!");
                }
                db.License_Item.Remove(Item);
                var product_license_items = db.License_Product_Item.Where(li => Item.Code == li.License_Item_Code).AsQueryable();
                if (product_license_items.Count() > 0)
                {
                    db.License_Product_Item.RemoveRange(product_license_items);
                }
                db.SaveChanges();
                //UpdateProductPrice();
                _logService.Info($"[Licenses][DeleteItem] complete");
                return Json(new object[] { true, "Item deleted!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][DeleteItem] error id:{id}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult CheckDeleteProduct(string id)
        {
            _logService.Info($"[Licenses][CheckDeleteProduct] start id:{id}");
            try
            {
                var Product = db.License_Product.Find(id);
                var list_active = (from s in db.Store_Services
                                   where s.ProductCode == Product.Code && s.Active == 1
                                   join c in db.C_Customer on s.CustomerCode equals c.CustomerCode
                                   select new { s, c }).AsEnumerable();
                var select = (from item in list_active
                              select new
                              {
                                  Code = item.s.CustomerCode,
                                  Store = item.s.StoreCode,
                                  Name = item.s.StoreName,
                                  Address = !string.IsNullOrEmpty(item.c.SalonAddress1 + item.c.SalonCity + item.c.SalonState)
                                        ? string.Join(", ", new object[] { item.c.SalonAddress1, item.c.SalonCity, item.c.SalonState, item.c.SalonZipcode, item.c.BusinessCountry })
                                        : string.Join(", ", new object[] { item.c.BusinessAddress, item.c.BusinessCity, item.c.BusinessState, item.c.BusinessZipCode, item.c.BusinessCountry })
                              }).ToList();
                _logService.Info($"[Licenses][CheckDeleteProduct] complete");
                return Json(new object[] { true, select.Count > 0 });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][CheckDeleteProduct] error id:{id}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult DeleteProduct(string id)
        {
            _logService.Info($"[Licenses][DeleteProduct] start id:{id}");
            try
            {
                var Product = db.License_Product.Find(id);
                if (Product == null)
                {
                    _logService.Warning($"[Licenses][DeleteProduct] Product not found id:{id}");
                    throw new Exception("Product not found!");
                }
                //if (db.Store_Services.Any(s => s.ProductCode == Product.Code && s.Active == 1))
                //{
                //    throw new Exception("This License has been activated for some salons. You cannot delete!");
                //}
                //db.License_Product.Remove(Product);
                //var product_license_items = db.License_Product_Item.Where(i => i.License_Product_Id == Product.Id).AsQueryable();
                Product.IsDelete = true;
                db.SaveChanges();
                _logService.Info($"[Licenses][DeleteProduct] complete");
                return Json(new object[] { true, "Product deleted!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][DeleteProduct] error id:{id}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult ChangeItemType(long ItemID, string type)
        {
            _logService.Info($"[Licenses][ChangeItemType] start ItemID:{ItemID} - type {type}");
            try
            {
                var Item = db.License_Item.Find(ItemID);
                if (Item == null)
                {
                    _logService.Warning($"[Licenses][ChangeItemType] Item not found!");
                    throw new Exception("Item not found!");
                }
                if (!Enum.IsDefined(typeof(UserContent.LICENSES_ITEM_TYPES), type))
                {
                    _logService.Warning($"[Licenses][ChangeItemType] Item type do not match!");
                    throw new Exception("Item type do not match!");
                }
                if (Item.BuiltIn == true)
                {
                    _logService.Warning($"[Licenses][ChangeItemType] Can't change type BuiltIn License Items!");
                    throw new Exception("Can't change type BuiltIn License Items!");
                }

                //Item.PaymentType = type;
                db.Entry(Item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                //UpdateProductPrice();
                _logService.Info($"[Licenses][ChangeItemType] completed ItemID:{ItemID} - type {type}");
                return Json(new object[] { true, "Item type change complete!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][ChangeItemType] error ItemID:{ItemID} - type {type}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult ChangeItemLicenseType(long ItemID, string l_type)
        {
            _logService.Info($"[Licenses][ChangeItemLicenseType] start ItemID:{ItemID} - l_type {l_type}");
            try
            {
                var Item = db.License_Item.Find(ItemID);
                if (Item == null)
                {
                    _logService.Warning($"[Licenses][ChangeItemLicenseType] Item not found!");
                    throw new Exception("Item not found!");
                }
                if (l_type != "COUNT" && l_type != "FEATURE")
                {
                    _logService.Warning($"[Licenses][ChangeItemLicenseType] Item type do not match!");
                    throw new Exception("Item type do not match!");
                }
                Item.Type = l_type;
                if (l_type == "FEATURE")
                {
                    db.License_Product_Item.Where(l => l.License_Item_Code == Item.Code).ToList().ForEach(l =>
                    {
                        l.Value = (l.Value != 0) ? 1 : l.Value;
                        db.Entry(l).State = System.Data.Entity.EntityState.Modified;
                    });
                }

                db.Entry(Item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                _logService.Info($"[Licenses][ChangeItemLicenseType] completed");
                return Json(new object[] { true, "Item type change complete!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][ChangeItemLicenseType] error ItemID:{ItemID} - l_type {l_type}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult ChangeOptions(long GroupID, bool isProduct, bool isAddon)
        {
            _logService.Info($"[Licenses][ChangeOptions] start GroupID:{GroupID} - isProduct {isProduct}  - isAddon {isAddon}");
            try
            {
                var group = db.License_Item_Group.Find(GroupID);
                if (group == null)
                {
                    _logService.Warning($"[Licenses][ChangeOptions] Group not found!");
                    throw new Exception("Group not found!");
                }
                List<string> options = new List<string>();
                if (isProduct)
                {
                    options.Add("Product");
                }
                if (isAddon)
                {
                    options.Add("Addon");
                }
                group.Options = String.Join(",", options);
                db.Entry(group).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                _logService.Info($"[Licenses][ChangeOptions] completed");
                return Json(new object[] { true, "Group options change complete!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][ChangeOptions] error GroupID:{GroupID} - isProduct {isProduct}  - isAddon {isAddon}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public ActionResult SelectItemModal_Partial(string ProductId, bool update)
        {
            _logService.Info($"[Licenses][SelectItemModal_Partial] start ProductId:{ProductId} - update {update}");
            try
            {
                var Groups = new List<Child_LicensesItemGroup>();
                if (update)
                {
                    ViewBag.lp_items = db.License_Product_Item.Where(i => i.License_Product_Id == ProductId).ToList();
                }
                var licent_product = db.License_Product.Find(ProductId);
                Groups = (from Group in db.License_Item_Group
                          where Group.ID != 1000000 || string.IsNullOrEmpty(licent_product.Code_POSSystem)
                          join Items in from Item in db.License_Item
                                        where update == db.License_Product_Item.Any(pl => pl.License_Item_Code == Item.Code && pl.License_Product_Id == ProductId)
                                        && Item.Enable == true
                                        group Item by Item.GroupID
                          on Group.ID equals Items.Key into ps
                          from p in ps.DefaultIfEmpty()
                          select new Child_LicensesItemGroup
                          {
                              Group = Group,
                              Items = p.ToList()
                          }).ToList();
                ViewBag.hasItem = Groups.Sum(g => g.Items.Count) > 0;
                var GGroups = Groups.Where(g => g.Group.ParentID == null).ToList();
                var hierGroups = (
                                      from c in GGroups
                                      join gs in from g in Groups
                                                 where g.Group.ParentID != null
                                                 group g by g.Group.ParentID
                                      on c.Group.ID equals gs.Key into ps
                                      from p in ps.DefaultIfEmpty()
                                      select new LicensesItemGroup
                                      {
                                          Group = c.Group,
                                          Items = c.Items,
                                          ChildGroups = p != null ? p.ToList() : new List<Child_LicensesItemGroup>()
                                      }).ToList();
                ViewBag.isAddon = db.License_Product.Find(ProductId)?.isAddon;
                _logService.Info($"[Licenses][SelectItemModal_Partial] completed");
                return PartialView("_Partial_ProductSelectItem", hierGroups);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][SelectItemModal_Partial] error ProductId:{ProductId} - update {update}");
                throw;
            }

        }
        public JsonResult Load_LicenseProductItem(string productId, long itemId)
        {
            _logService.Info($"[Licenses][Load_LicenseProductItem] start ProductId:{productId} - itemId {itemId}");
            try
            {
                var l_item = db.License_Item.Find(itemId);
                var item = db.License_Product_Item.Where(i => i.License_Item_Code == l_item.Code && i.License_Product_Id == productId).FirstOrDefault();
                var product = db.License_Product.Find(productId);
                //vinh_need-fix
                _logService.Info($"[Licenses][Load_LicenseProductItem] completed");
                return Json(new object[] { /*l_item.PaymentType*/"", item ?? new License_Product_Item(), product.Name, l_item.Name, product.SubscriptionDuration, l_item.Type });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][Load_LicenseProductItem] error ProductId:{productId} - itemId {itemId}");
                throw;
            }

        }
        public JsonResult Save_LicenseProductItem(string ProductID, List<string> enabled_item, bool update = false)
        {
            _logService.Info($"[Licenses][Save_LicenseProductItem] start ProductId:{ProductID} - enabled_item {enabled_item}");
            try
            {
                var Product = db.License_Product.Where(x => x.Id == ProductID).FirstOrDefault();
                if (Product != null)
                {
                    if (enabled_item.Any(x => x.Contains("MangoSlice")))
                    {
                        Product.AllowSlice = true;
                    }
                    else
                    {
                        Product.AllowSlice = false;
                    }
                }
                if (update)
                {
                    var disable_lpItem = db.License_Product_Item.Where(i => i.License_Product_Id == ProductID && i.Enable == true && !enabled_item.Contains(i.Id.ToString())).AsQueryable();
                    var BS = db.License_Item.Where(c => "Base Services".Equals(c.GroupName) && c.Enable == true).Select(c => c.Code).ToList();
                    foreach (var item in disable_lpItem)
                    {
                        item.Enable = false;
                        db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                    }
                    foreach (var item_code in enabled_item)
                    {
                        int value = 0;
                        int.TryParse(Request["select_" + item_code], out value);
                        if ((BS.IndexOf(item_code) < 0 || item_code.Equals("SALONCENTER") || item_code.Equals("CHECKIN")) && value == 0)
                            value = 1;
                        var item = db.License_Product_Item.Where(i => i.License_Product_Id == ProductID && i.License_Item_Code == item_code).FirstOrDefault();
                        if (item != null)
                        {
                            item.Value = value;
                            item.Enable = true;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                    }

                    var storeActived = db.Store_Services.Where(c => c.RenewDate == null || c.RenewDate > DateTime.UtcNow && c.Active == 1)
                        .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new LicenseStoreActive
                        {
                            Id = lp.Id,
                            StoreName = ss.StoreName,
                            StoreCode = ss.StoreCode
                        }).Where(c => c.Id == ProductID).ToList().Count;

                    db.SaveChanges();
                    _logService.Info($"[Licenses][Save_LicenseProductItem] completed");
                    return Json(new object[] { true, "Update product licenses completed!", storeActived == 0 ? "0" : ProductID.ToString() });
                }
                else
                {
                    if (enabled_item == null)
                        return Json(new object[] { true, "Add product licenses completed" });
                    var r = new Random();
                    var BS = db.License_Item.Where(c => "Base Services".Equals(c.GroupName) && c.Enable == true).Select(c => c.Code).ToList();
                    foreach (var item_code in enabled_item)
                    {
                        int value = 0;
                        int.TryParse(Request["select_" + item_code], out value);
                        if ((BS.IndexOf(item_code) < 0 || item_code.Equals("SALONCENTER") || item_code.Equals("CHECKIN")) && value == 0)
                            value = 1;
                        License_Product_Item new_item = new License_Product_Item
                        {
                            Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + r.Next(1, 9999).ToString().PadLeft(4, '0')),
                            License_Product_Id = ProductID,
                            License_Item_Code = item_code,
                            Value = value,
                            Enable = true
                        };
                        db.License_Product_Item.Add(new_item);
                    }

                    db.SaveChanges();
                    var storeActived = db.Store_Services.Where(c => c.RenewDate == null || c.RenewDate > DateTime.UtcNow && c.Active == 1)
                        .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new LicenseStoreActive
                        {
                            Id = lp.Id,
                            StoreName = ss.StoreName,
                            StoreCode = ss.StoreCode
                        }).Where(c => c.Id == ProductID).ToList().Count;

                    _logService.Info($"[Licenses][Save_LicenseProductItem] Add product licenses completed");
                    return Json(new object[] { true, "Add product licenses completed", storeActived == 0 ? "0" : ProductID.ToString() });
                }
            }

            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][Save_LicenseProductItem] error");
                return Json(new object[] { false, ex.Message });
            }
        }
        public string CheckHistoryStoreActivedProduct(string productId)
        {
            _logService.Info($"[Licenses][CheckHistoryStoreActivedProduct] start productId:{productId}");
            try
            {
                var storeActived = db.Store_Services.Where(c => c.RenewDate == null || c.RenewDate > DateTime.UtcNow && c.Active == 1)
                  .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new LicenseStoreActive
                  {
                      Id = lp.Id,
                      StoreName = ss.StoreName,
                      StoreCode = ss.StoreCode,
                      LicenseId = ss.Id
                  }).Where(c => c.Id == productId).Select(c => new { c.LicenseId }).ToList();

                string path = Server.MapPath(pathLog + productId.ToString() + "-log.txt");
                if (!System.IO.File.Exists(path))
                {
                    return "0/0";
                }
                else
                {
                    var text = System.IO.File.ReadAllText(path);
                    var arrText = text.Replace("\n", "").Replace("\r", "").Split('|').ToList();
                    arrText.Remove("");
                    if (arrText.Count == 0)
                        return "0/" + storeActived.Count.ToString();

                    var fileStore = arrText.Select(a => new
                    {
                        LicenseId = a.Split(':')[0],
                        Status = a.Split(':')[1]
                    }).ToList();

                    bool storeFail = false;
                    int countSuccess = 0;
                    storeActived.ForEach(r =>
                    {
                        var fillter = fileStore.Where(f => f.LicenseId.Equals(r.LicenseId)).ToList();
                        if (fillter.Count == 0)
                            storeFail = true;
                        else if (fillter.Where(f => f.Status.Equals("Success")).Count() == 0)
                            storeFail = true;
                        else
                            countSuccess++;
                    });
                    if (!storeFail)
                        System.IO.File.Delete(path);
                    return countSuccess.ToString() + "/" + storeActived.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][CheckHistoryStoreActivedProduct] error productId:{productId}");
                throw;
            }

        }
        public async Task WriteHistoryStoreActivedProduct(string productId, string licenseId = null, string status = null)
        {
            _logService.Info($"[Licenses][WriteHistoryStoreActivedProduct] start productId:{productId}-licenseId:{licenseId}-status:{status}");
            try
            {
                string path = Server.MapPath(pathLog + productId.ToString() + "-log.txt");
                if (!System.IO.File.Exists(path))
                {
                    FileStream fs = System.IO.File.Create(path);
                    fs.Close();
                }
                string content = licenseId + ':' + status + ':' + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") + '|';
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                {
                    await file.WriteLineAsync(content);
                    file.Close();
                }
                _logService.Info($"[Licenses][WriteHistoryStoreActivedProduct] completed");
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][WriteHistoryStoreActivedProduct] error productId:{productId}-licenseId:{licenseId}-status:{status}");
                throw new Exception(ex.Message);
            }
        }
        public void CreateHistoryStoreActivedProduct(string productId)
        {
            _logService.Info($"[Licenses][CreateHistoryStoreActivedProduct] start productId:{productId}");
            try
            {
                string path = Server.MapPath(pathLog + productId.ToString() + "-log.txt");
                if (!System.IO.File.Exists(path))
                {
                    FileStream fs = System.IO.File.Create(path);
                    fs.Close();
                }
                _logService.Info($"[Licenses][CreateHistoryStoreActivedProduct] completed");
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][CreateHistoryStoreActivedProduct] error productId:{productId}");
                throw new Exception(ex.Message);
            }
        }
        public void RemoveHistoryStoreActivedProduct(string productId)
        {
            _logService.Info($"[Licenses][RemoveHistoryStoreActivedProduct] start productId:{productId}");
            try
            {
                string path = Server.MapPath(pathLog + productId.ToString() + "-log.txt");
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                _logService.Info($"[Licenses][RemoveHistoryStoreActivedProduct] complete");
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][RemoveHistoryStoreActivedProduct] error productId:{productId}");
                throw;
            }

        }
        public ActionResult GetStoresActivedProduct_Partial(string productId, bool reUpdate)
        {
            _logService.Info($"[Licenses][GetStoresActivedProduct_Partial] start productId:{productId} - reUpdate {reUpdate}");

            try
            {
                var storeProducts = new List<LicenseStoreActive>();
                if (reUpdate)
                {
                    var storeActived = db.Store_Services.Where(c => c.RenewDate == null || c.RenewDate > DateTime.UtcNow && c.Active == 1)
                        .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new LicenseStoreActive
                        {
                            Id = lp.Id,
                            StoreName = ss.StoreName,
                            StoreCode = ss.StoreCode,
                            LicenseId = ss.Id
                        }).Where(c => c.Id.Equals(productId)).ToList();
                    if (storeActived.Count == 0)
                        return PartialView("_Partial_UpdateLicenseStore", storeProducts);

                    string path = Server.MapPath(pathLog + productId.ToString() + "-log.txt");
                    if (!System.IO.File.Exists(path))
                        return PartialView("_Partial_UpdateLicenseStore", storeProducts);

                    var text = System.IO.File.ReadAllText(path);
                    var arrText = text.Replace("\n", "").Replace("\r", "").Split('|').ToList();
                    arrText.Remove("");
                    //if (arrText.Count == 0)
                    //{
                    //    System.IO.File.Delete(path);
                    //    return PartialView("_Partial_UpdateLicenseStore", storeProducts);
                    //}

                    var fileStore = arrText.Select(a => new
                    {
                        LicenseId = a.Split(':')[0],
                        Status = a.Split(':')[1]
                    }).ToList();

                    storeActived.ForEach(r =>
                    {
                        var fillter = fileStore.Where(f => f.LicenseId.Equals(r.LicenseId)).ToList();
                        if (fillter.Count == 0)
                            storeProducts.Add(r);
                        else if (fillter.Where(f => f.Status.Equals("Success")).Count() == 0)
                            storeProducts.Add(fillter.Select(f => new LicenseStoreActive
                            {
                                Id = r.Id,
                                StoreName = r.StoreName,
                                StoreCode = r.StoreCode,
                                LicenseId = f.LicenseId
                            }).FirstOrDefault());
                    });
                }
                else
                {
                    storeProducts = db.Store_Services.Where(c => c.RenewDate == null || c.RenewDate > DateTime.UtcNow && c.Active == 1)
                        .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new LicenseStoreActive
                        {
                            Id = lp.Id,
                            StoreName = ss.StoreName,
                            StoreCode = ss.StoreCode,
                            LicenseId = ss.Id
                        }).Where(c => c.Id.Equals(productId)).ToList();
                }

                _logService.Info($"[Licenses][GetStoresActivedProduct_Partial] completed");
                return PartialView("_Partial_UpdateLicenseStore", storeProducts);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][GetStoresActivedProduct_Partial] error productId:{productId} - reUpdate {reUpdate}");
                throw;
            }

        }
        public JsonResult GetStoresActivedProduct(string productId, bool reUpdate)
        {
            _logService.Info($"[Licenses][GetStoresActivedProduct] start productId:{productId} - reUpdate {reUpdate}");

            try
            {
                var storeProducts = new List<LicenseStoreActive>();
                if (reUpdate)
                {
                    var storeActived = db.Store_Services.Where(c => c.RenewDate == null || c.RenewDate > DateTime.UtcNow && c.Active == 1)
                        .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new LicenseStoreActive
                        {
                            Id = lp.Id,
                            StoreName = ss.StoreName,
                            StoreCode = ss.StoreCode,
                            LicenseId = ss.Id
                        }).Where(c => c.Id.Equals(productId)).ToList();
                    if (storeActived.Count == 0)
                        return Json(new object[] { storeProducts });

                    string path = Server.MapPath(pathLog + productId.ToString() + "-log.txt");
                    if (!System.IO.File.Exists(path))
                        return Json(new object[] { storeProducts });

                    var text = System.IO.File.ReadAllText(path);
                    var arrText = text.Replace("\n", "").Replace("\r", "").Split('|').ToList();
                    arrText.Remove("");
                    if (arrText.Count == 0)
                    {
                        System.IO.File.Delete(path);
                        return Json(new object[] { storeProducts });
                    }

                    var fileStore = arrText.Select(a => new
                    {
                        LicenseId = a.Split(':')[0],
                        Status = a.Split(':')[1]
                    }).ToList();

                    storeActived.ForEach(r =>
                    {
                        var fillter = fileStore.Where(f => f.LicenseId.Equals(r.LicenseId)).ToList();
                        if (fillter.Count == 0)
                            storeProducts.Add(r);
                        else if (fillter.Where(f => f.Status.Equals("Success")).Count() == 0)
                            storeProducts.Add(fillter.Select(f => new LicenseStoreActive
                            {
                                Id = r.Id,
                                StoreName = r.StoreName,
                                StoreCode = r.StoreCode,
                                LicenseId = f.LicenseId
                            }).FirstOrDefault());
                    });
                }
                else
                {
                    storeProducts = db.Store_Services.Where(c => c.RenewDate == null || c.RenewDate > DateTime.UtcNow && c.Active == 1)
                        .Join(db.License_Product, ss => ss.ProductCode, lp => lp.Code, (ss, lp) => new LicenseStoreActive
                        {
                            Id = lp.Id,
                            StoreName = ss.StoreName,
                            StoreCode = ss.StoreCode,
                            LicenseId = ss.Id
                        }).Where(c => c.Id.Equals(productId)).ToList();
                }
                _logService.Info($"[Licenses][GetStoresActivedProduct] completed");
                return Json(new object[] { storeProducts });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][GetStoresActivedProduct] error productId:{productId} - reUpdate {reUpdate}");
                throw;
            }

        }
        public async Task<JsonResult> Update_StoreActivedProduct(string productId, string licenseId)
        {
            _logService.Info($"[Licenses][Update_StoreActivedProduct] start productId:{productId} - licenseId {licenseId}");
            try
            {
                var merchantController = EngineContext.Current.Resolve<MerchantManController>();
                var sameactived = merchantController.StoreActivation(licenseId/*, ims_ver*/, true, "same-active");

                await WriteHistoryStoreActivedProduct(productId, licenseId, "Success");
                _logService.Info($"[Licenses][Update_StoreActivedProduct] completed");
                return new Func.JsonStatusResult(new { status = "Success", message = "" }, HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                await WriteHistoryStoreActivedProduct(productId, licenseId, "Fail");
                _logService.Error($"[Licenses][Update_StoreActivedProduct] error productId:{productId} - licenseId {licenseId}");
                return new Func.JsonStatusResult(new { status = "Fail", message = string.IsNullOrEmpty(e.InnerException?.Message) ? e.Message : e.InnerException.Message }, HttpStatusCode.ExpectationFailed);
            }
        }
        public JsonResult Save_LicenseProduct(License_Product prd, bool? isProduct)
        {
            _logService.Info($"[Licenses][Save_LicenseProduct] start isProduct {isProduct}", new { LicenseProduct = Newtonsoft.Json.JsonConvert.SerializeObject(prd) });
            try
            {
                if (prd.Available == null)
                {
                    prd.Available = false;
                }
                if (!string.IsNullOrEmpty(prd.Id))
                {
                    var edit_prd = db.License_Product.Find(prd.Id);
                    if (edit_prd == null)
                    {
                        throw new Exception("License product not found !");
                    }
                    if (db.License_Product.Any(p => p.Code == prd.Code && prd.Id != p.Id && p.IsDelete != true))
                    {
                        throw new Exception("License product already exist !");
                    }
                    if (isProduct == true)
                    {
                        edit_prd.Code_POSSystem = Request["Code_POSSystem"];
                    }
                    else
                    {
                        edit_prd.Code_POSSystem = string.Empty;
                    }
                    if (edit_prd.Name != prd.Name)
                    {
                        var ListStoreServiceByLicenseCode = db.Store_Services.Where(s => s.ProductCode == edit_prd.Code);
                        foreach (var s in ListStoreServiceByLicenseCode)
                        {
                            s.Productname = prd.Name;
                        }
                    }
                    edit_prd.Name = prd.Name;
                    edit_prd.Price = prd.Price;
                    edit_prd.PartnerPrice = prd.PartnerPrice;
                    edit_prd.MembershipPrice = prd.MembershipPrice;
                    edit_prd.Type = prd.Type;
                    edit_prd.SubscriptionDuration = prd.SubscriptionDuration;
                    edit_prd.SubscriptionEndWarningDays = prd.SubscriptionEndWarningDays;
                    edit_prd.Trial_Months = prd.Trial_Months ?? 0;
                    edit_prd.Promotion_Apply_Months = prd.Promotion_Apply_Months ?? 0;
                    edit_prd.Promotion_Time_To_Available = prd.Promotion_Time_To_Available ?? 0;
                    edit_prd.Promotion_Price = prd.Promotion_Price ?? 0;
                    edit_prd.AllowDemo = prd.Trial_Months > 0 ? true : false;
                    edit_prd.Available = prd.Available;
                    edit_prd.Level = prd.Level;
                    edit_prd.Active = prd.Active == null ? false : prd.Active;
                    edit_prd.ActivationFee = prd.ActivationFee;
                    edit_prd.InteractionFee = prd.InteractionFee;
                    edit_prd.DeploymentTiketAuto = prd.DeploymentTiketAuto ?? false;
                    edit_prd.PreparingDays = prd.PreparingDays;
                    edit_prd.PeriodRecurring = prd.PeriodRecurring;
                    edit_prd.SiteId = edit_prd.Type == "license" ? prd.SiteId : null;
                    edit_prd.isAddon = prd.Type != "license";
                    edit_prd.FlagDeactivateExpires = Request["bool_FlagDeactivateExpires"] == "true" ? 1 : 0;
                    var GiftCardType = LicenseType.GiftCard.Text();
                    if (edit_prd.Type == GiftCardType)
                    {
                        edit_prd.GiftCardQuantity = prd.GiftCardQuantity ?? 1;
                    }
                    if (edit_prd.SubscriptionDuration == "MONTHLY")
                    {
                        edit_prd.NumberOfPeriod = prd.NumberOfPeriod;
                    }
                    else
                    {
                        edit_prd.NumberOfPeriod = 1;
                    }
                    if (edit_prd.Code != prd.Code)
                    {
                        foreach (var item in db.Order_Subcription.Where(o => o.Product_Code == edit_prd.Code))
                        {
                            item.Product_Code = prd.Code;
                            item.ProductName = prd.Name;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                        foreach (var item in db.Store_Services.Where(s => s.ProductCode == edit_prd.Code))
                        {
                            item.ProductCode = prd.Code;
                            item.Productname = prd.Name;
                            db.Entry(item).State = System.Data.Entity.EntityState.Modified;
                        }
                        edit_prd.Code = prd.Code;
                    }


                    db.Entry(edit_prd).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    prd.Code = System.Text.RegularExpressions.Regex.Replace(AppLB.CommonFunc.ConvertNonUnicodeURL(prd.Name, true), @"\W", "");
                    if (db.License_Product.Any(p => p.Code == prd.Code && p.IsDelete != true))
                    {
                        throw new Exception("License product already exist!");
                    }
                    prd.Id = Guid.NewGuid().ToString("N");
                    var ProductType = LicenseType.LICENSE.Text();
                    var GiftCardType = LicenseType.GiftCard.Text();
                    if (prd.Type == ProductType)
                    {
                        prd.isAddon = false;
                        prd.Code_POSSystem = Request["Code_POSSystem"];
                    }
                    else
                    {
                        prd.isAddon = true;
                        prd.Code_POSSystem = string.Empty;
                    }

                    if (prd.Type == GiftCardType)
                    {
                        prd.GiftCardQuantity = prd.GiftCardQuantity ?? 1;
                    }
                    prd.Trial_Months = prd.Trial_Months ?? 0;
                    prd.Promotion_Apply_Months = prd.Promotion_Apply_Months ?? 0;
                    prd.Promotion_Time_To_Available = prd.Promotion_Time_To_Available ?? 0;
                    prd.Promotion_Price = prd.Promotion_Price ?? 0;
                    prd.ActivationFee = prd.ActivationFee ?? 0;
                    prd.InteractionFee = prd.InteractionFee ?? 0;
                    prd.PreparingDays = prd.PreparingDays ?? 0;
                    prd.DeploymentTiketAuto = prd.DeploymentTiketAuto ?? false;
                    prd.AllowDemo = prd.Trial_Months > 0 ? true : false;
                    prd.Active = prd.Active ?? false;
                    prd.FlagDeactivateExpires = Request["bool_FlagDeactivateExpires"] == "true" ? 1 : 0;
                    prd.SiteId = prd.Type == "license" ? prd.SiteId : null; 
                    if (prd.SubscriptionDuration == "MONTHLY")
                    {
                        prd.NumberOfPeriod = prd.NumberOfPeriod;
                    }
                    else
                    {
                        prd.NumberOfPeriod = 1;
                    }
                    ////slice / trial
                    //var oldSlice = db.License_Product.FirstOrDefault(l => l.AllowSlice == true);
                    //if (oldSlice != null && prd.AllowSlice == true && oldSlice.Code != prd.Code)
                    //{
                    //    oldSlice.AllowSlice = false;
                    //    db.Entry(oldSlice).State = System.Data.Entity.EntityState.Modified;
                    //}

                    //var oldTrial = db.License_Product.FirstOrDefault(l => l.AllowDemo == true);
                    //if (oldTrial != null && prd.AllowDemo == true && oldTrial.Code != prd.Code)
                    //{
                    //    oldTrial.AllowDemo = false;
                    //    db.Entry(oldTrial).State = System.Data.Entity.EntityState.Modified;
                    //}

                    db.License_Product.Add(prd);
                    db.SaveChanges();
                }
                string message = string.Empty;
                switch (prd.Type)
                {
                    case "license":
                        message = "Product's license has been changed";
                        break;
                    case "addon":
                        message = "Addon has been changed";
                        break;
                    case "giftcard":
                        message = "Gift Card has been changed";
                        break;
                    case "other":
                        message = "Other sevirces has been changed";
                        break;
                    default:
                        message = "Product's license has been changed";
                        break;
                }
                _logService.Info($"[Licenses][Save_LicenseProduct] completed");
                return Json(new object[] { true, message });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][Save_LicenseProduct] error isProduct {isProduct}");
                return Json(new object[] { false, ex.Message });
            }
        }
        public decimal Total_PrdLic_Price = 0;
        public class sumPriceInput
        {
            public long id { get; set; }
            public List<long> items { get; set; }
        }
        #endregion
        #region License
        public ActionResult LicenseProduct()
        {
            _logService.Info($"[Licenses][LicenseProduct] start");
            return View();
        }
        public ActionResult ChangeTab(string TabName)
        {
            _logService.Info($"[Licenses][ChangeTab] start TabName {TabName}");

            try
            {
                var mTypeLicense = LicenseType.LICENSE.Text();
                var mTypeAddon = LicenseType.ADD_ON.Text();
                var mGiftCard = LicenseType.GiftCard.Text();
                var mTypeOther = LicenseType.VirtualHardware_Other.Text();
                string TypeSelected = "";
                switch (TabName)
                {

                    case "license":
                        TypeSelected = mTypeLicense;
                        break;
                    case "addon":
                        TypeSelected = mTypeAddon;
                        break;
                    case "giftcard":
                        TypeSelected = mGiftCard;
                        break;
                    default:
                        TypeSelected = mTypeOther;
                        break;

                }

                var Groups = (
                    from Group in db.License_Item_Group
                    join Items in from Item in db.License_Item where Item.Enable == true group Item by Item.GroupID
                    on Group.ID equals Items.Key into ps
                    from p in ps.DefaultIfEmpty()
                    select new Child_LicensesItemGroup
                    {
                        Group = Group,
                        Items = p.ToList()
                    }).ToList();

                var GGroups = Groups.Where(g => g.Group.ParentID == null && g.Group.Options != null).ToList();
                var hierGroups = (
                                  from c in GGroups
                                  join gs in from g in Groups
                                             where g.Group.ParentID != null
                                             group g by g.Group.ParentID
                                  on c.Group.ID equals gs.Key into ps
                                  from p in ps.DefaultIfEmpty()
                                  select new LicensesItemGroup
                                  {
                                      Group = c.Group,
                                      Items = c.Items,
                                      ChildGroups = p != null ? p.ToList() : new List<Child_LicensesItemGroup>()
                                  }).ToList();

                var Products = (from P in db.License_Product
                                where P.Type == TypeSelected && P.IsDelete != true
                                join Gi in from item in db.License_Product_Item group item by item.License_Product_Id
                                           on P.Id equals Gi.Key into ps
                                from p in ps.DefaultIfEmpty()
                                select new LicenseProductView
                                {
                                    Product = P,
                                    Items = p.ToList()
                                }).OrderBy(x => x.Product.Type == mTypeLicense ? 1 : x.Product.Type == mTypeAddon ? 2 : x.Product.Type == mTypeOther ? 3 : 4)
                                .ThenByDescending(x => x.Product.Available).ThenByDescending(x => x.Product.Level.HasValue)
                                .ThenBy(x => x.Product.Level).ThenBy(x => x.Product.Price).ToList();
                ViewBag.Products = Products;
                Products.ForEach(p =>
                {
                    p.NumberUpdated = CheckHistoryStoreActivedProduct(p.Product.Id);
                    var resArray = p.NumberUpdated.Split('/');
                    p.UpdateStore = !(resArray[0].Equals(resArray[1]));
                });
                var Partners = db.C_Partner.Where(c => c.SiteId > 0).ToList();
                Partners.Add(new C_Partner { SiteId = 1, Name = "Simply Pos" });
                ViewBag.Partners = Partners.OrderBy(c => c.SiteId).ToList();

                _logService.Info($"[Licenses][ChangeTab] completed");
                return PartialView("_Partial_LicenseProductTabContent", hierGroups);
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][ChangeTab] error TabName {TabName}");
                throw;
            }

        }
        [HttpPost]
        public ActionResult EnableAvaible(string productId)
        {
            _logService.Info($"[Licenses][ChangeTab] start productId {productId}");
            try
            {
                var product = db.License_Product.Where(x => x.Id == productId).FirstOrDefault();
                if (product != null)
                {
                    product.Available = !(product.Available ?? true);
                    db.SaveChanges();
                    return Json(new { status = true, message = "License Update Success !" });
                }
                _logService.Info($"[Licenses][ChangeTab] completed");
                return Json(new { status = false, message = "License Not Found !" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][EnableAvaible] error productId {productId}");
                throw;
            }

        }
        [HttpPost]
        public ActionResult EnableStatus(string productId)
        {
            _logService.Info($"[Licenses][EnableStatus] start productId {productId}");
            try
            {
                var product = db.License_Product.Where(x => x.Id == productId).FirstOrDefault();
                if (product != null)
                {
                    product.Active = !(product.Active ?? true);
                    db.SaveChanges();
                    return Json(new { status = true, message = "License Update Success !" });
                }
                _logService.Info($"[Licenses][EnableStatus] completed");
                return Json(new { status = false, message = "License Not Found !" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][EnableStatus] error productId {productId}");
                throw;
            }

        }
        [HttpPost]
        public ActionResult ReActive(string productId)
        {
            _logService.Info($"[Licenses][ReActive] start productId {productId}");
            try
            {
                var product = db.License_Product.Where(x => x.Id == productId).FirstOrDefault();
                if (product != null)
                {
                    product.Active = true;
                    db.SaveChanges();
                    return Json(new { status = true, message = "License Update Success !" });
                }
                _logService.Info($"[Licenses][ReActive] completed");
                return Json(new { status = false, message = "License Not Found!" });
            }
            catch (Exception ex)
            {
                _logService.Error(ex, $"[Licenses][ReActive] error productId {productId}");
                throw;
            }

        }

        [HttpGet]
        public JsonResult solveRecurring()
        {
            List<RecurringPlanning> lstRcurring = db.Database.SqlQuery<RecurringPlanning>("exec P_GetRecurringList").ToList();
            DateTime Today = DateTime.Now;
            foreach(var item in lstRcurring)
            {
                int dateDistance = ((item.RenewDate ?? DateTime.Now) - Today).Days;
                if(dateDistance < -3)
                {
                    string sql = $"update Store_Services set AutoRenew=0, Active=-1 where CustomerCode='{item.CustomerCode}' and ProductCode='{item.SubscriptionCode}' and Active =1";
                    db.Database.ExecuteSqlCommand(sql);
                } else {

                }
            }
            //(EndDate.Date - StartDate.Date).Days
            return Json(new { status = true }, JsonRequestBehavior.AllowGet);
        }


        //public async Task<ActionResult> SaveOrder(O_Orders orderModel, string action, TotalMoneyOrder total_money_order, List<Device_Service_ModelCustomize> lstService, bool send_payment_email = false)
        //{
        //    _logService.Info($"[Invoice][SaveOrder] start save order action = {action} | send_payment_email = {send_payment_email}", new { orderModel = Newtonsoft.Json.JsonConvert.SerializeObject(orderModel) });
        //    WebDataModel db = new WebDataModel();
        //    using (var Trans = db.Database.BeginTransaction())
        //    {
        //        try
        //        {
        //            if (orderModel.Id > 0)//Update
        //            {
        //                Random Rand = new Random();
        //                var order = db.O_Orders.FirstOrDefault(o => o.Id == orderModel.Id);
        //                if (order != null)
        //                {
        //                    var cus = db.C_Customer.FirstOrDefault(c => c.CustomerCode == order.CustomerCode);
        //                    var _partner = db.C_Partner.FirstOrDefault(c => c.Code == orderModel.PartnerCode);
        //                    if (_partner != null && cus.PartnerCode != orderModel.PartnerCode)
        //                    {
        //                        cus.PartnerCode = _partner.Code;
        //                        cus.PartnerName = _partner.Name;
        //                        db.Entry(cus).State = EntityState.Modified;
        //                    }
        //                    #region update assign ticket new salon
        //                    if (!string.IsNullOrEmpty(orderModel.SalesMemberNumber) && orderModel.SalesMemberNumber != order.SalesMemberNumber)
        //                    {
        //                        var member = db.P_Member.Where(x => x.MemberNumber == orderModel.SalesMemberNumber).FirstOrDefault();
        //                        var tickets = db.T_SupportTicket.Where(x => x.OrderCode == order.OrdersCode);
        //                        List<string> ListDeparmentTicketByMember = new List<string>();
        //                        var ChirldDepartment = db.P_Department.Where(x => !string.IsNullOrEmpty(x.GroupMemberNumber)).ToList();
        //                        foreach (var dep in ChirldDepartment)
        //                        {
        //                            var listMember = dep.GroupMemberNumber.Split(',');
        //                            if (listMember.Contains(member.MemberNumber))
        //                            {
        //                                ListDeparmentTicketByMember.Add(dep.Id.ToString());
        //                            }
        //                        }
        //                        foreach (var t in tickets)
        //                        {
        //                            if (ListDeparmentTicketByMember.Any(x => x.Contains(t.GroupID.ToString())))
        //                            {
        //                                if (string.IsNullOrEmpty(t.AssignedToMemberNumber) || !(t.AssignedToMemberNumber.Contains(member.MemberNumber)))
        //                                {
        //                                    if (!string.IsNullOrEmpty(t.AssignedToMemberNumber))
        //                                    {
        //                                        t.AssignedToMemberNumber = t.AssignedToMemberNumber.Substring(t.AssignedToMemberNumber.Length - 1) == "," ? (t.AssignedToMemberNumber + member.MemberNumber + ",") : (t.AssignedToMemberNumber + "," + member.MemberNumber + ",");
        //                                    }
        //                                    else
        //                                    {
        //                                        t.AssignedToMemberNumber = member.MemberNumber + ",";
        //                                    }
        //                                    if (!string.IsNullOrEmpty(t.AssignedToMemberName))
        //                                    {
        //                                        t.AssignedToMemberName = t.AssignedToMemberName.Substring(t.AssignedToMemberName.Length - 1) == "," ? (t.AssignedToMemberName + member.FullName + ",") : (t.AssignedToMemberName + "," + member.FullName + ",");
        //                                    }
        //                                    else
        //                                    {
        //                                        t.AssignedToMemberName = member.FullName + ",";
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                    #endregion
        //                    #region UPDATE ORDER
        //                    var sale_mn = Request["SalesMemberNumber"];

        //                    order.CustomerCode = orderModel.CustomerCode;
        //                    order.PartnerCode = orderModel.PartnerCode;
        //                    order.CustomerName = cus?.BusinessName;
        //                    order.Comment = Request.Unvalidated["desc"];
        //                    order.UpdatedAt = DateTime.UtcNow;
        //                    order.UpdatedHistory += "|" + DateTime.UtcNow.ToString("dd MMM,yyyy hh:mm tt") + " - By: " + cMem.FullName;
        //                    order.DiscountAmount = total_money_order.DiscountAmount;
        //                    order.DiscountPercent = float.Parse(total_money_order.DiscountPercent.ToString());
        //                    order.ShippingFee = total_money_order.ShippingFee;
        //                    order.TaxRate = total_money_order.TaxRate;
        //                    order.TotalHardware_Amount = total_money_order.SubTotal;
        //                    order.GrandTotal = total_money_order.GrandTotal;
        //                    order.ShippingAddress = Request["sh_street"] + "|" + Request["sh_city"] + "|" + Request["sh_state"] + "|" + Request["sh_zip"] + "|" + Request["sh_country"];
        //                    order.InvoiceDate = orderModel.InvoiceDate;
        //                    order.CreateDeployTicket = bool.Parse(Request["CreateDeployTicket"] ?? "false");
        //                    //order.DueDate = orderModel.DueDate;
        //                    order.Renewal = orderModel.Renewal;
        //                    if (order.InvoiceNumber == null)
        //                    {
        //                        order.InvoiceNumber = long.Parse(order.OrdersCode);
        //                    }

        //                    #endregion
        //                    //cap nhat lai order_product
        //                    #region UPDATE DEVICES
        //                    var list_product_db = db.Order_Products.Where(d => d.OrderCode == order.OrdersCode).ToList();
        //                    var list_device_ss = lstService.Where(d => d.Type == "device" || d.Type == "package").ToList();
        //                    var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
        //                    List<string> ModelCodes = new List<string>();
        //                    if (order?.InvoiceNumber > 0)
        //                    {
        //                        //TH: tao luon invoice ma van chua co tic
        //                        //ket, khong can tao ticket sales nua,=> tao luon ims onboarding va finance ticket.
        //                        string linkViewInvoiceFull = ConfigurationManager.AppSettings["IMSUrl"] + "/order/ImportInvoiceToPDF?_code=" + order.OrdersCode + "&flag=Invoices";
        //                        string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + order.Id;


        //                    }
        //                    foreach (var item in list_device_ss)
        //                    {
        //                        //tao order_product

        //                        if (item.BundleId > 0)
        //                        {
        //                            var mbs = db.I_Bundle_Device.Where(m => m.Bundle_Id == item.BundleId).ToList();
        //                            var package = new Order_Products();
        //                            package.Id = id++;
        //                            package.OrderCode = order.OrdersCode;
        //                            package.Price = item.PriceApply; //item.price ;
        //                            package.CreateBy = cMem.FullName;
        //                            package.CreateAt = DateTime.UtcNow;
        //                            package.BundleId = item.BundleId;
        //                            package.BundleQTY = item.Quantity;
        //                            package.Discount = item.Discount;
        //                            package.DiscountPercent = item.DiscountPercent;
        //                            package.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
        //                            db.Order_Products.Add(package);
        //                            foreach (var mb in mbs)
        //                            {
        //                                var model = db.O_Product_Model.Find(mb.ModelCode);
        //                                var new_device = new Order_Products();
        //                                new_device.Id = id++;
        //                                new_device.OrderCode = order.OrdersCode;
        //                                new_device.ProductCode = model.ProductCode;
        //                                new_device.ProductName = model.ProductName;
        //                                new_device.Price = mb.Price ?? 0;
        //                                new_device.Quantity = mb.Quantity * item.Quantity;
        //                                new_device.CreateBy = cMem.FullName;
        //                                new_device.CreateAt = DateTime.UtcNow;
        //                                new_device.Feature = model.Color;
        //                                new_device.BundleId = item.BundleId;
        //                                new_device.ModelCode = model.ModelCode;
        //                                new_device.ModelName = model.ModelName;
        //                                new_device.BundleQTY = item.Quantity;
        //                                var oldOrderProduct = list_product_db.FirstOrDefault(x => x.ModelCode == mb.ModelCode && x.ProductCode == model.ProductCode);
        //                                if (oldOrderProduct != null)
        //                                {
        //                                    new_device.SerNumbers = oldOrderProduct.SerNumbers;
        //                                    new_device.InvNumbers = oldOrderProduct.InvNumbers;
        //                                }
        //                                ModelCodes.Add(mb.ModelCode);
        //                                db.Order_Products.Add(new_device);
        //                            }
        //                        }
        //                        else
        //                        {
        //                            var new_device = new Order_Products();
        //                            new_device.Id = id++;
        //                            new_device.OrderCode = order.OrdersCode;
        //                            new_device.ProductCode = item.ProductCode;
        //                            new_device.ProductName = item.ProductName;
        //                            new_device.Price = item.PriceApply; //item.Price;
        //                            new_device.Quantity = item.Quantity;
        //                            new_device.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
        //                            new_device.CreateBy = cMem.FullName;
        //                            new_device.CreateAt = DateTime.UtcNow;
        //                            new_device.Feature = item.Feature;
        //                            new_device.ModelCode = item.ModelCode;
        //                            new_device.ModelName = item.ModelName;
        //                            new_device.BundleId = null;
        //                            new_device.BundleQTY = null;
        //                            new_device.Discount = item.Discount;
        //                            new_device.DiscountPercent = item.DiscountPercent;
        //                            var oldOrderProduct = list_product_db.FirstOrDefault(x => x.ModelCode == item.ModelCode && x.ProductCode == item.ProductCode);
        //                            if (oldOrderProduct != null)
        //                            {
        //                                new_device.SerNumbers = oldOrderProduct.SerNumbers;
        //                                new_device.InvNumbers = oldOrderProduct.InvNumbers;
        //                            }
        //                            ModelCodes.Add(item.ModelCode);
        //                            db.Order_Products.Add(new_device);
        //                        }
        //                    }
        //                    foreach (var item in list_product_db)
        //                    {
        //                        //xoa order_product
        //                        db.Order_Products.Remove(item);
        //                    }
        //                    #endregion
        //                    if (list_device_ss.Count > 0 && string.IsNullOrEmpty(order.BundelStatus))
        //                    {
        //                        order.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
        //                    }
        //                    //Cap nhat lai order_license product & addon
        //                    #region UPDATE PRODUCT & ADDON
        //                    var oldproducts = db.Order_Subcription.Where(s => s.OrderCode == order.OrdersCode).ToList();
        //                    foreach (var item in oldproducts)
        //                    {
        //                        db.Order_Subcription.Remove(item);
        //                    }
        //                    if (string.IsNullOrWhiteSpace(cus.StoreCode) == true)
        //                    {   //new
        //                        cus.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(cus.CustomerCode, "[^.0-9]", "");
        //                        db.Entry(cus).State = EntityState.Modified;
        //                    }
        //                    var list_subsc_ss = (Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>)
        //                        .Where(d => (d.Type == "license" || d.Type == "addon" || d.Type == "giftcard" || d.Type == "other" || d.Type == "setupfee" || d.Type == "interactionfee")).ToList();
        //                    foreach (var item in list_subsc_ss)
        //                    {
        //                        var product = db.License_Product.Where(p => p.Id == item.SubscriptionId).FirstOrDefault();
        //                        var RecurringPrice = decimal.Parse(string.IsNullOrEmpty(Request["RecurringPrice_" + item.SubscriptionId]) ? "-1" : Request["RecurringPrice_" + item.SubscriptionId]);
        //                        var Renewal = Request["Renewal_" + item.SubscriptionId]?.ToString() == "true";
        //                        var ApplyDate = Request["ApplyDate_" + item.SubscriptionId]?.ToString() == "true";
        //                        var applyRecurring = Request["ApplyDiscountAsRecurring_" + item.SubscriptionId]?.ToString() == "true";
        //                        var NumberOfItem = db.License_Product_Item.Where(lp => lp.License_Product_Id == product.Id).Count();
        //                        var sp = new Order_Subcription
        //                        {
        //                            Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + Rand.Next(1, 9999).ToString().PadLeft(4, '0')),
        //                            Actived = true,
        //                            AutoRenew = item.SubscriptionDuration == "MONTHLY" ? Renewal : false,
        //                            Price = item.PriceApply,
        //                            Amount = item.Amount,
        //                            PriceType = item.PriceType,
        //                            Promotion_Apply_Months = (item.PriceType == Store_Apply_Status.Promotional.ToString()) ? item.Promotion_Apply_Months : 0,
        //                            Period = item.SubscriptionDuration,
        //                            OrderCode = order.OrdersCode,
        //                            NumberOfItem = item.Type == "setupfee" || item.Type == "interactionfee" ? 0 : NumberOfItem,
        //                            ProductId = product.Id,
        //                            ProductName = product.Name,
        //                            Product_Code = product.Code,
        //                            Product_Code_POSSystem = product.Code_POSSystem,
        //                            PurcharsedDay = DateTime.UtcNow,
        //                            IsAddon = product.isAddon == true || item.Type == "setupfee" || item.Type == "interactionfee",
        //                            SubscriptionType = item.Type,
        //                            CustomerCode = order.CustomerCode,
        //                            CustomerName = order.CustomerName,
        //                            StoreCode = cus.StoreCode,
        //                            Quantity = item.Quantity,
        //                            Discount = item.Discount,
        //                            DiscountPercent = item.DiscountPercent,
        //                            ApplyDiscountAsRecurring = item.ApplyDiscountAsRecurring,
        //                            PeriodRecurring = item.PeriodRecurring,
        //                            SubscriptionQuantity = item.SubscriptionQuantity < 1 ? 1 : item.SubscriptionQuantity,
        //                            ApplyPaidDate = ApplyDate
        //                        };
        //                        if (RecurringPrice >= 0) sp.RecurringPrice = RecurringPrice;

        //                        //update date
        //                        string start_date = Request["Effective_start_date_" + item.SubscriptionId];
        //                        //if (product.isAddon != true)
        //                        //{
        //                        sp.StartDate = string.IsNullOrEmpty(start_date) ? order.InvoiceDate : DateTime.Parse(start_date);
        //                        if (sp.Period == "MONTHLY")
        //                        {
        //                            string end_date = Request["Expiry_date_" + item.SubscriptionId];
        //                            sp.EndDate = string.IsNullOrEmpty(end_date) ? sp.StartDate.Value.AddMonths(item.Quantity) : DateTime.Parse(end_date);
        //                            // if type equals trial, get month default of license
        //                            //if (sp.PriceType == Store_Apply_Status.Trial.ToString())
        //                            //{
        //                            //    sp.EndDate = sp.StartDate.Value.AddMonths(product.Trial_Months.Value);
        //                            //}
        //                            //else
        //                            //{
        //                            //    sp.EndDate = sp.StartDate.Value.AddMonths(item.Quantity);
        //                            //}
        //                        }
        //                        else if (sp.Period == "QUATERLY")
        //                        {
        //                            sp.EndDate = sp.StartDate.Value.AddMonths(3 * item.Quantity);
        //                        }
        //                        else if (sp.Period == "ANNUAL")
        //                        {
        //                            sp.EndDate = sp.StartDate.Value.AddYears(item.Quantity);
        //                        }
        //                        if (item.Type == "setupfee")
        //                        {
        //                            sp.EndDate = item.ExpiryDate;
        //                        }
        //                        //}
        //                        //else
        //                        //{
        //                        //    sp.StartDate = order.InvoiceDate;
        //                        //}
        //                        db.Order_Subcription.Add(sp);

        //                        if (sp.SubscriptionType != "setupfee" && sp.SubscriptionType != "interactionfee")
        //                        {
        //                            var sservice = db.Store_Services.FirstOrDefault(c => c.OrderCode == order.OrdersCode && c.ProductCode == sp.Product_Code);
        //                            if (sservice != null)
        //                            {
        //                                sservice.Quantity = sp.SubscriptionQuantity;
        //                                sservice.EffectiveDate = sp.StartDate;
        //                                sservice.AutoRenew = sp.AutoRenew;
        //                                sservice.ApplyDiscountAsRecurring = sp.ApplyDiscountAsRecurring;
        //                                sservice.StoreApply = sp.PriceType;
        //                                sservice.RenewDate = sp.EndDate;
        //                                sservice.LastUpdateAt = DateTime.UtcNow;
        //                                db.Entry(sservice).State = System.Data.Entity.EntityState.Modified;
        //                            }
        //                        }
        //                        //MODIFY & CREATE STORT_PRODUCT_LICENSE
        //                    }
        //                    #endregion

        //                    await db.SaveChangesAsync();
        //                    Trans.Commit();
        //                    Trans.Dispose();

        //                    if (order?.InvoiceNumber > 0)
        //                    {
        //                        string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + order.Id;
        //                        var checkTerminal = (from ordPrd in db.Order_Products
        //                                             join prd in db.O_Product on ordPrd.ProductCode equals prd.Code
        //                                             join prdmodel in db.O_Product_Model on ordPrd.ModelCode equals prdmodel.ModelCode
        //                                             where ordPrd.OrderCode == order.OrdersCode && (prd.ProductLineCode == "terminal" || prdmodel.MerchantOnboarding == true)
        //                                             select ordPrd).Any();
        //                        var activeCreateOnboardingTicket = db.SystemConfigurations.FirstOrDefault().ActiveOnboardingTicket;
        //                        if (activeCreateOnboardingTicket == true && checkTerminal && !db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Any(t => t.CustomerCode == order.CustomerCode && t.OrderCode == order.OrdersCode && t.T_TicketTypeMapping.Any(x => x.TypeId == (long)UserContent.TICKET_TYPE.NuveiOnboarding)))
        //                        {
        //                            await TicketViewService.AutoTicketScenario.NewTicketNuveiOnboarding(order.InvoiceNumber.ToString(), "<iframe  width='600' height='900' src='" + linkViewInvoice + "'></iframe>");
        //                        }
        //                    }
        //                    OrderViewService.WriteLogSalesLead(order, true, cMem);
        //                    if (order.Status != InvoiceStatus.Canceled.ToString())// || Request["Status"] == "Payment cleared"
        //                    {
        //                        string result = await new StoreViewService().CloseStoreService(order.OrdersCode, cus.StoreCode, cMem.FullName, true);
        //                    }
        //                    TempData["s"] = "Update success !";
        //                    string back = order.InvoiceNumber > 0 ? "/order" : "/order/estimates";

        //                    //auto closed if 0$
        //                    if (order.GrandTotal == 0 && order.Status == InvoiceStatus.Open.ToString())
        //                    {
        //                        var deploymentTypeId = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
        //                        //var checkexistTicketDeployment = db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Where(x => x.OrderCode == order.OrdersCode && x.T_TicketTypeMapping.Any(y => y.TypeId == deploymentTypeId)).FirstOrDefault();
        //                        ////check exist ticket deployment 
        //                        //if (checkexistTicketDeployment == null)
        //                        //{
        //                        //    TempData["ShowCreateTicketDelivery"] = "true";
        //                        //}
        //                        if (list_device_ss.Count > 0)
        //                        {
        //                            await ChangeInvoiceSatus(order.OrdersCode, InvoiceStatus.Paid_Wait.ToString());
        //                        }
        //                        else
        //                        {
        //                            await ChangeInvoiceSatus(order.OrdersCode, InvoiceStatus.Closed.ToString());
        //                        }

        //                    }
        //                    await this.SendQuestionAreForm(ModelCodes, order.CustomerCode);
        //                    if (send_payment_email && string.IsNullOrEmpty(order.PartnerCode))
        //                    {
        //                        _ = ResendPaymentEmail(order.OrdersCode);
        //                    }

        //                    _logService.Info($"[Invoice][SaveOrder] completed save order edit #{order.OrdersCode}");
        //                    return RedirectToAction("EstimatesDetail", new { id = order.Id, url_back = back });


        //                }
        //                else
        //                {
        //                    throw new Exception("Does not exist.");
        //                }


        //            }
        //            else//Create
        //            {
        //                Random Rand = new Random();
        //                var list_device_service = Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>;
        //                var total_money_order = Session[TOTAL_MONEY_ORDER] as TotalMoneyOrder;
        //                var sale_mn = Request["SalesMemberNumber"];
        //                var ship_address = Request["sh_street"] + "|" + Request["sh_city"] + "|" + Request["sh_state"] + "|" + Request["sh_zip"] + "|" + Request["sh_country"];

        //                #region CREATE ORDER
        //                var new_order = OrderViewService.NewOrder(db, list_device_service, total_money_order, cMem, sale_mn, orderModel.CustomerCode, Request.Unvalidated["desc"], ship_address, orderModel.InvoiceDate, orderModel.DueDate, orderModel.Renewal);
        //                new_order.Status = InvoiceStatus.Open.ToString();
        //                new_order.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
        //                new_order.InvoiceNumber = long.Parse(new_order.OrdersCode);
        //                new_order.PartnerCode = orderModel.PartnerCode;
        //                new_order.CreateDeployTicket = bool.Parse(Request["CreateDeployTicket"] ?? "false");
        //                //  new_order.CreateDeployTicket = orderModel.CreateDeployTicket;
        //                db.O_Orders.Add(new_order);
        //                var cus = db.C_Customer.Where(s => s.CustomerCode == new_order.CustomerCode).FirstOrDefault();

        //                var _partner = db.C_Partner.FirstOrDefault(c => c.Code == orderModel.PartnerCode);
        //                if (_partner != null)
        //                {
        //                    cus.PartnerCode = _partner.Code;
        //                    cus.PartnerName = _partner.Name;
        //                    db.Entry(cus).State = EntityState.Modified;
        //                }
        //                #endregion
        //                //add new  order_product
        //                #region UPDATE DEVICES
        //                var list_device_ss = (Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>).Where(d => d.Type == "device" || d.Type == "package").ToList();
        //                var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
        //                List<string> ModelCodes = new List<string>();
        //                foreach (var item in list_device_ss)
        //                {
        //                    //tao order_product
        //                    if (item.BundleId > 0)
        //                    {
        //                        var mbs = db.I_Bundle_Device.Where(m => m.Bundle_Id == item.BundleId).ToList();
        //                        var package = new Order_Products();
        //                        package.Id = id++;
        //                        package.OrderCode = new_order.OrdersCode;
        //                        package.Price = item.PriceApply;
        //                        package.CreateBy = cMem.FullName;
        //                        package.CreateAt = DateTime.UtcNow;
        //                        package.BundleId = item.BundleId;
        //                        package.BundleQTY = item.Quantity;
        //                        package.Discount = item.Discount;
        //                        package.DiscountPercent = item.DiscountPercent;
        //                        package.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
        //                        db.Order_Products.Add(package);
        //                        foreach (var mb in mbs)
        //                        {
        //                            var model = db.O_Product_Model.Find(mb.ModelCode);
        //                            var new_device = new Order_Products();
        //                            new_device.Id = id++;
        //                            new_device.OrderCode = new_order.OrdersCode;
        //                            new_device.ProductCode = model.ProductCode;
        //                            new_device.ProductName = model.ProductName;
        //                            new_device.Price = mb.Price ?? 0;
        //                            new_device.Quantity = mb.Quantity * item.Quantity;
        //                            new_device.CreateBy = cMem.FullName;
        //                            new_device.CreateAt = DateTime.UtcNow;
        //                            new_device.Feature = model.Color;
        //                            new_device.BundleId = item.BundleId;
        //                            new_device.ModelCode = model.ModelCode;
        //                            new_device.ModelName = model.ModelName;
        //                            new_device.BundleQTY = item.Quantity;
        //                            ModelCodes.Add(model.ModelCode);
        //                            db.Order_Products.Add(new_device);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        var new_device = new Order_Products();
        //                        new_device.Id = id++;
        //                        new_device.OrderCode = new_order.OrdersCode;
        //                        new_device.ProductCode = item.ProductCode;
        //                        new_device.ProductName = item.ProductName;
        //                        new_device.Price = item.PriceApply;
        //                        new_device.Quantity = item.Quantity;
        //                        new_device.TotalAmount = item.Amount; //(item.Price * item.Quantity) - item.Discount;
        //                        new_device.CreateBy = cMem.FullName;
        //                        new_device.CreateAt = DateTime.UtcNow;
        //                        new_device.Feature = item.Feature;
        //                        new_device.ModelCode = item.ModelCode;
        //                        new_device.ModelName = item.ModelName;
        //                        new_device.BundleId = null;
        //                        new_device.BundleQTY = null;
        //                        new_device.Discount = item.Discount;
        //                        new_device.DiscountPercent = item.DiscountPercent;
        //                        ModelCodes.Add(item.ModelCode);
        //                        db.Order_Products.Add(new_device);
        //                    }
        //                }
        //                #endregion

        //                //add new order_license product & addon
        //                #region UPDATE PRODUCT & ADDON
        //                if (string.IsNullOrEmpty(cus.StoreCode) == true)
        //                {
        //                    //new
        //                    cus.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(cus.CustomerCode, "[^.0-9]", "");
        //                    db.Entry(cus).State = EntityState.Modified;
        //                }
        //                var list_subsc_ss = (Session[LIST_PRODUCT_SERVICE] as List<Device_Service_ModelCustomize>)
        //                    .Where(d => (d.Type == "license" || d.Type == "addon" || d.Type == "giftcard" || d.Type == "other" || d.Type == "setupfee" || d.Type == "interactionfee")).ToList();
        //                foreach (var product in list_subsc_ss)
        //                {
        //                    var Renewal = Request["Renewal_" + product.SubscriptionId]?.ToString() == "true";
        //                    var ApplyDate = Request["ApplyDate_" + product.SubscriptionId]?.ToString() == "true";
        //                    var applyRecurring = Request["ApplyDiscountAsRecurring_" + product.SubscriptionId]?.ToString() == "true";
        //                    var product_info = db.License_Product.Where(p => p.Id == product.SubscriptionId).FirstOrDefault();
        //                    var NumberOfItem = db.License_Product_Item.Where(lp => lp.License_Product_Id == product.SubscriptionId).Count();
        //                    var sp = new Order_Subcription
        //                    {
        //                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff") + Rand.Next(1, 9999).ToString().PadLeft(4, '0')),
        //                        Actived = true,
        //                        AutoRenew = product.SubscriptionDuration == "MONTHLY" ? Renewal : false,
        //                        //Price = product_info.Price,
        //                        PriceType = product.PriceType,
        //                        Promotion_Apply_Months = (product.PriceType == Store_Apply_Status.Promotional.ToString()) ? product.Promotion_Apply_Months : 0,
        //                        Price = product.PriceApply,
        //                        RecurringPrice = product.PriceApply,
        //                        Amount = product.Amount,
        //                        Period = product.SubscriptionDuration,
        //                        OrderCode = new_order.OrdersCode,
        //                        NumberOfItem = product.Type == "setupfee" || product.Type == "interactionfee" ? 0 : NumberOfItem,
        //                        ProductId = product_info.Id,
        //                        ProductName = product_info.Name,
        //                        PreparingDate = product.Type == "license" ? (product.PreparingDays > 0 ? product.PreparingDays : 0) : 0,
        //                        Product_Code = product_info.Code,
        //                        Product_Code_POSSystem = product_info.Code_POSSystem,
        //                        PurcharsedDay = DateTime.UtcNow,
        //                        IsAddon = product_info.isAddon == true || product.Type == "setupfee" || product.Type == "interactionfee",
        //                        CustomerCode = new_order.CustomerCode,
        //                        CustomerName = new_order.CustomerName,
        //                        StoreCode = cus.StoreCode,
        //                        SubscriptionType = product.Type,
        //                        Quantity = product.Quantity,
        //                        Discount = product.Discount,
        //                        DiscountPercent = product.DiscountPercent,
        //                        ApplyDiscountAsRecurring = product.SubscriptionDuration == "MONTHLY" ? applyRecurring : false,
        //                        PeriodRecurring = product.PeriodRecurring,
        //                        SubscriptionQuantity = product.SubscriptionQuantity < 1 ? 1 : product.SubscriptionQuantity,
        //                        ApplyPaidDate = ApplyDate
        //                    };
        //                    //update date
        //                    string start_date = Request["Effective_start_date_" + product_info.Id];
        //                    sp.StartDate = string.IsNullOrEmpty(start_date) ? new_order.InvoiceDate : DateTime.Parse(start_date);
        //                    if (sp.Period == "MONTHLY")
        //                    {
        //                        string end_date = Request["Expiry_date_" + product_info.Id];
        //                        sp.EndDate = string.IsNullOrEmpty(end_date) ? sp.StartDate.Value.AddMonths(product.Quantity) : DateTime.Parse(end_date);
        //                        // if type equals trial, get month default of license
        //                        //if (sp.PriceType == Store_Apply_Status.Trial.ToString())
        //                        //{
        //                        //    sp.EndDate = sp.StartDate.Value.AddMonths(product_info.Trial_Months.Value);
        //                        //}
        //                        //else
        //                        //{
        //                        //    sp.EndDate = sp.StartDate.Value.AddMonths(product.Quantity);
        //                        //}
        //                    }
        //                    else if (sp.Period == "QUATERLY")
        //                    {
        //                        sp.EndDate = sp.StartDate.Value.AddMonths(product.Quantity * 3);
        //                    }
        //                    else if (sp.Period == "ANNUAL")
        //                    {
        //                        sp.EndDate = sp.StartDate.Value.AddYears(product.Quantity);
        //                    }
        //                    if (product.Type == "setupfee")
        //                    {
        //                        sp.EndDate = product.ExpiryDate;
        //                    }
        //                    db.Order_Subcription.Add(sp);
        //                }

        //                #endregion
        //                if (list_device_ss.Count > 0)
        //                {
        //                    new_order.BundelStatus = AppLB.UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString();
        //                }
        //                db.SaveChanges();
        //                Trans.Commit();
        //                Trans.Dispose();
        //                var ReqURL = Request.Url.Authority;
        //                var des = "<iframe  width='600' height='900' src='" + Request.Url.Scheme + "://" + ReqURL + "/order/ImportInvoiceToPDF?_code=" + new_order.OrdersCode + "&flag=Estimates'></iframe>";
        //                await this.SendQuestionAreForm(ModelCodes, new_order.CustomerCode);
        //                string create_invoice_rs = "";

        //                if (action == "Create Invoice")
        //                {
        //                    OrderViewService.WriteLogSalesLead(new_order, false, cMem, true);
        //                    var rs = await ChangeInvoiceSatus(new_order.OrdersCode, InvoiceStatus.Open.ToString(), false);
        //                    if (!(bool)((object[])rs.Data)[0])
        //                    {
        //                        create_invoice_rs = ((object[])rs.Data)[1].ToString();
        //                    }
        //                    string linkViewInvoice = ConfigurationManager.AppSettings["IMSUrl"] + "/order/TicketViewInvoice?id=" + new_order.Id;
        //                    var checkTerminal = (from ordPrd in db.Order_Products
        //                                         join prd in db.O_Product on ordPrd.ProductCode equals prd.Code
        //                                         join prdmodel in db.O_Product_Model on ordPrd.ModelCode equals prdmodel.ModelCode
        //                                         where ordPrd.OrderCode == new_order.OrdersCode && (prd.ProductLineCode == "terminal" || prdmodel.MerchantOnboarding == true)
        //                                         select ordPrd).Any();
        //                    var activeCreateOnboardingTicket = db.SystemConfigurations.FirstOrDefault().ActiveOnboardingTicket;
        //                    if (activeCreateOnboardingTicket == true && checkTerminal && !db.T_SupportTicket.Include(x => x.T_TicketTypeMapping).Any(t => t.CustomerCode == new_order.CustomerCode && t.OrderCode == new_order.OrdersCode && t.T_TicketTypeMapping.Any(y => y.TypeId == (long)UserContent.TICKET_TYPE.NuveiOnboarding)))
        //                    {
        //                        await TicketViewService.AutoTicketScenario.NewTicketNuveiOnboarding(new_order.InvoiceNumber.ToString(), "<iframe  width='600' height='900' src='" + linkViewInvoice + "'></iframe>");
        //                    }
        //                }
        //                else if (new_order.InvoiceNumber > 0)
        //                {
        //                    await OrderViewService.CheckMerchantWordDetermine(new_order.CustomerCode);
        //                }
        //                else
        //                {
        //                    OrderViewService.WriteLogSalesLead(new_order, false, cMem);
        //                }

        //                if (create_invoice_rs == "")
        //                {
        //                    TempData["s"] = (action == "Create Invoice") ? "Create Invoice successful" : "Create Estimete successful.";
        //                }
        //                else
        //                {
        //                    TempData["e"] = "Close estimete fail: " + create_invoice_rs;
        //                }
        //                string back = (action == "Create Invoice" || new_order.InvoiceNumber > 0) ? "/order" : "/order/estimates";


        //                //auto closed if 0$
        //                if (new_order.GrandTotal == 0 && new_order.Status == InvoiceStatus.Open.ToString())
        //                {
        //                    var deploymentTypeId = (long)AppLB.UserContent.TICKET_TYPE.Deployment;

        //                    if (list_device_ss.Count > 0)
        //                    {
        //                        await ChangeInvoiceSatus(new_order.OrdersCode, InvoiceStatus.Paid_Wait.ToString());
        //                    }
        //                    else
        //                    {
        //                        await ChangeInvoiceSatus(new_order.OrdersCode, InvoiceStatus.Closed.ToString());
        //                    }

        //                }
        //                if (send_payment_email && string.IsNullOrEmpty(new_order.PartnerCode))
        //                {
        //                    _ = ResendPaymentEmail(new_order.OrdersCode);
        //                }
        //                _logService.Info($"[Invoice][SaveOrder] completed save order create #{new_order.OrdersCode}");
        //                return RedirectToAction("EstimatesDetail", new { id = new_order.Id, url_back = back });
        //                //return RedirectToAction("Estimates");
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            Trans.Dispose();
        //            var url_back = Request["hd_url_back"] ?? "index";
        //            TempData["e"] = ex.Message;
        //            _logService.Error(ex, $"[Invoice][SaveOrder] error Save order");
        //            return Redirect(url_back);
        //        }
        //    }
        //}

        #endregion
    }
}