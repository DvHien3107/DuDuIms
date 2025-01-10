using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.AppLB;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Excel;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class ProductManageController : UploadController
    {
        WebDataModel db = new WebDataModel();
        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        // GET: ProductManage
        public ActionResult Product(string tab = "available")
        {
            if ((access.Any(k => k.Key.Equals("products_accessdevice")) == false || access["products_accessdevice"] != true))
            {
                return Redirect("/home/Forbidden");
            }
            var product = (from p in db.O_Product
                           join l in db.O_Product_Line on p.ProductLineCode equals l.Code
                           join ms in from m in db.O_Product_Model 
                                      let o = db.Order_Products.Where(y=>y.ModelCode == m.ModelCode).Count()>0
                                      select new O_Product_Model_CustomizeModel
                                      {
                                          o_Product_Model = m,
                                          IsAlreadyExistInvoice = o
                                      } into pa
                                      group pa by pa.o_Product_Model.ProductCode
                                      on p.Code equals ms.Key
                                     //let existModelInInvoice = db.Order_Products.Where(x=>x.ModelCode== ms.Key)
                                      into ps
                           from ms in ps.DefaultIfEmpty()
                           let existInvoice = db.Order_Products.Where(x=>x.ProductCode == p.Code).Count()>0
                           select new ProductManage_view { Product = p, Line = l, Models = ms.OrderByDescending(o=>o.o_Product_Model.ModelCode).ToList(),IsAlreadyExistInvoice = existInvoice }).ToList();
            if(tab.Trim().ToLower()== "available")
            {
                product = (from p in product where p.Product.IsDeleted != true select new ProductManage_view { Product = p.Product, Line = p.Line, Models = p.Models.Where(x=>x.o_Product_Model.IsDeleted!=true && x.o_Product_Model.Active ==true).ToList(),IsAlreadyExistInvoice = p.IsAlreadyExistInvoice }).ToList();
            }
            else
            {
                product = (from p in product where p.Product.IsDeleted == true || p.Models.Where(x=>x.o_Product_Model.IsDeleted==true||x.o_Product_Model.Active==false).Count()>0 select new ProductManage_view { Product = p.Product, Line = p.Line, Models = p.Models.Where(x => x.o_Product_Model.IsDeleted == true || x.o_Product_Model.Active != true).ToList(), IsAlreadyExistInvoice = p.IsAlreadyExistInvoice }).ToList();
            }
            ViewBag.Tab = tab;
            ViewBag.ProductLines = db.O_Product_Line.OrderByDescending(p=>p.Code).ToList();
            ViewBag.Vendors = db.Vendors.Where(v => v.Active == true).Select(v => new VendorCustomize { VendorId = v.Id, VendorName = v.CompanyName }).ToList();
            return View(product);
        }
        public JsonResult SaveProduct(string code, string name, string line, string des)
        {
            try
            {
                if ((access.Any(k => k.Key.Equals("products_accessdevice")) == false || access["products_accessdevice"] != true))
                {
                    throw new Exception("Access denied!");
                }
                var line_name = db.O_Product_Line.Find(line).Name;
                if (!string.IsNullOrEmpty(code))
                {
                    var product = db.O_Product.Find(code);
                    if (product == null)
                    {
                        throw new Exception("Product not found");
                    }
                    product.Name = name;
                    product.ProductLineCode = line;
                    product.Description = des;
                    db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, "Product update completed!", product, line_name });
                }
                else
                {
                    code = "P" + DateTime.UtcNow.ToString("yyMMddHHmmssff");//Regex.Replace(CommonFunc.ConvertNonUnicodeURL(name, false).Replace("-","_"),@"\W", "_").ToLower();
                    if (db.O_Product.Any(p => p.Name == name))
                    {
                        throw new Exception("Product name already exist!");
                    }
                    var new_pro = new O_Product
                    {
                        Code = code,
                        Name = name,
                        ProductLineCode = line,
                        Description = des
                    };
                    db.O_Product.Add(new_pro);
                    db.SaveChanges();
                    ViewBag.Linename = line_name;
                    return Json(new object[] { true, "Create new product completed!", CommonFunc.RenderRazorViewToString("_new_Product", new_pro, this), new_pro.Code });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }

        /// <summary>
        /// Save product model
        /// </summary>
        /// <param name="code">Model code</param>
        /// <param name="model_name">model name</param>
        /// <param name="product">product code</param>
        /// <param name="vendor"></param>
        /// <param name="color"></param>
        /// <param name="price"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public JsonResult SaveModel(string code, string model_name, string product, string color,bool? device_required,bool? merchant_onboarding, decimal? price,decimal? saleprice, string status, bool active = true)
        {
            //HttpPostedFileBase file = HttpContext.Request.Files[0];
            using (var tran = db.Database.BeginTransaction())
            {
                string image = "";
                try
                {
                    if ((access.Any(k => k.Key.Equals("products_accessdevice")) == false || access["products_accessdevice"] != true))
                    {
                        throw new Exception("Access denied!");
                    }
                    if (price == null)
                    {
                        throw new Exception("Price is required");
                    }
                    var Product = db.O_Product.Find(product);
                    //var Vendor = db.Vendors.Find(vendor);
                    if (!string.IsNullOrEmpty(code))
                    {
                        var model = db.O_Product_Model.AsEnumerable().Where(m => AppLB.CommonFunc.ConvertNonUnicodeURL(m.ModelCode, true) == AppLB.CommonFunc.ConvertNonUnicodeURL(code, true)).FirstOrDefault();

                        if (model == null)
                        {
                            throw new Exception("Model not found");
                        }
                        bool productChanged = model.ProductCode != Product.Code;
                        string ProductCode = model.ProductCode;
                        model.ProductCode = Product.Code;
                        model.ProductName = Product.Name;
                        //model.VendorId = Vendor.Id;
                        //model.VendorName = Vendor.CompanyName;
                        model.ModelName = model_name;
                        model.Color = color;
                        model.Price = price;
                        model.SalePrice = saleprice;
                        model.DeviceRequired = device_required ?? false;
                        model.MerchantOnboarding = merchant_onboarding ?? false;
                        model.Active = active;
                        //model.Status = status;
                        db.Entry(model).State = System.Data.Entity.EntityState.Modified;

                        if (Request["image_modified"] == "1")
                        {
                            UploadAttachFile("/upload/product/", "image", "Model" + code, "", out image);
                            model.Picture = image;
                        }


                        //update model cho PO detail
                        var details = (from d in db.PO_Detail
                                       join r in db.PO_Request on d.RequestCode equals r.Code
                                       where r.ProductCode == ProductCode && r.Color == color
                                       join p in db.POes on d.POCode equals p.POCode
                                       //where p.VendorId == model.VendorId
                                       select d).ToList();
                        foreach (var d in details)
                        {
                            d.ModelCode = code;
                            d.ModelPicture = model.Picture;
                            db.Entry(d).State = System.Data.Entity.EntityState.Modified;
                        }


                        db.SaveChanges();
                        tran.Commit();
                        return Json(new object[] { true, "Model update completed!", model, productChanged, ProductCode });
                    }
                    else
                    {
                        var new_code = "M" + DateTime.UtcNow.ToString("yyMMddHHmmssff");//Regex.Replace(CommonFunc.ConvertNonUnicodeURL(model_name, true), @"\W", "").ToLower();

                        if (db.O_Product_Model.Any(m => m.ModelName.ToLower().Replace(" ","") == model_name.ToLower().Replace(" ","")))
                        {
                            throw new Exception("Model name already exist!");
                        }
                        var new_model = new O_Product_Model
                        {
                            ModelCode = new_code,
                            Color = color,
                            ModelName = model_name,
                            Picture = "",
                            Price = price,
                            SalePrice = saleprice,
                            ProductCode = Product.Code,
                            ProductName = Product.Name,
                            DeviceRequired = device_required ?? false,
                            MerchantOnboarding = merchant_onboarding ?? false,
                        //VendorId = Vendor.Id,
                        //VendorName = Vendor.CompanyName,
                            Active = active,
                            Status = "Enroute"//status
                        };
                        if (Request["image_modified"] == "1")
                        {
                            UploadAttachFile("/upload/product/", "image", "Model_" + Regex.Replace(CommonFunc.ConvertNonUnicodeURL(model_name, true), @"\W", "").ToLower(), "", out image);
                            new_model.Picture = image;
                        }
                        db.O_Product_Model.Add(new_model);

                        //update model cho PO detail
                        var details = (from d in db.PO_Detail
                                       join r in db.PO_Request on d.RequestCode equals r.Code
                                       where r.ProductCode == new_model.ProductCode && r.Color == new_model.Color
                                       join p in db.POes on d.POCode equals p.POCode
                                       //where p.VendorId == new_model.VendorId
                                       select d).ToList();
                        foreach (var d in details)
                        {
                            d.ModelName = model_name;
                            d.ModelPicture = new_model.Picture;
                            db.Entry(d).State = System.Data.Entity.EntityState.Modified;
                        }
                        db.SaveChanges();
                        tran.Commit();
                        return Json(new object[] { true, "Create new model completed!", CommonFunc.RenderRazorViewToString("_new_Model", new_model, this) });
                    }
                }
                catch (Exception e)
                {
                    tran.Dispose();
                    return Json(new object[] { false, e.Message });
                }

            }

        }

        /// <summary>
        /// New product line
        /// </summary>
        /// <param name="line">Product line name</param>
        /// <returns></returns>
        public JsonResult NewLine(string line)
        {

            try
            {
                if ((access.Any(k => k.Key.Equals("products_accessdevice")) == false || access["products_accessdevice"] != true))
                {
                    throw new Exception("Access denied!");
                }
                string line_code = CommonFunc.ConvertNonUnicodeURL(line, false).Replace("-", "_").ToLower();
                if (db.O_Product_Line.Any(l => l.Code == line_code))
                {
                    throw new Exception("Product line already exist!");
                }
                var new_line = new O_Product_Line { Code = line_code, Name = line };
                db.O_Product_Line.Add(new_line);
                db.SaveChanges();
                return Json(new object[] { true, "New product line created!", new_line });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }

        public JsonResult getProductInfo(string Code)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var Product = db.O_Product.Find(Code);
                return Json(new object[] { true, Product });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult getModelInfo(string Code)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var model = db.O_Product_Model.AsEnumerable().Where(m=> AppLB.CommonFunc.ConvertNonUnicodeURL(m.ModelCode, true) == AppLB.CommonFunc.ConvertNonUnicodeURL(Code,true)).FirstOrDefault();
                return Json(new object[] { true, model });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public ActionResult DeleteProduct(string ProductCode)
        {
            try
            {
                var product = db.O_Product.Where(x => x.Code == ProductCode).FirstOrDefault();
                if (product != null)
                {
                    product.IsDeleted = true;
                }
                var listproductmodel = db.O_Product_Model.Where(x => x.ProductCode == ProductCode).ToList();
                if (listproductmodel.Count() > 0)
                {
                    foreach(var listproduct in listproductmodel)
                    {
                        listproduct.IsDeleted = true;
                    }
                }
                var listDevice = db.O_Device.Where(d => (d.Inventory != 0 || d.Junkyard == true) && d.ProductCode == ProductCode);
                if (listDevice.Count() > 0)
                {
                    db.O_Device.RemoveRange(listDevice);
                }
                db.SaveChanges();
                return Json(new { status = true, message = "Delete success" });
            }
            catch(Exception ex)
            {
                return Json(new { status =false,message =ex.Message});
            }
           
        }
        public ActionResult DeleteModel(string ProductModelCode)
        {
            try
            {
                var productmodel = db.O_Product_Model.Where(x => x.ModelCode == ProductModelCode).FirstOrDefault();
                if (productmodel != null)
                {
                    productmodel.IsDeleted = true;
                }
                var listDevice = db.O_Device.Where(d => (d.Inventory != 0 || d.Junkyard ==true) && d.ModelCode == ProductModelCode);
                if (listDevice.Count() > 0)
                {
                    db.O_Device.RemoveRange(listDevice);
                }
                db.SaveChanges();
                return Json(new { status = true, message = "Delete success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        #region Vendor
        public ActionResult GetVendorModal()
        {
            var vendors = db.Vendors.ToList();
            return PartialView("_vendor_manage", vendors);
        }
        public JsonResult getVendor(long id)
        {
            var db = new WebDataModel();
            var vendors = db.Vendors.Find(id);
            if (vendors != null)
                return Json(new object[] { true, vendors });
            else
                return Json(new object[] { false, "Vendor not found!" });
        }
        public JsonResult SaveVendor(Vendor vendor)
        {

            try
            {
                //if ((access.Any(k => k.Key.Equals("inventory_manage")) == false || access["inventory_manage"] != true))
                //{
                //    throw new Exception("Forbiden, not permission!");
                //}
                var db = new WebDataModel();
                if (vendor.Id == 0)
                {
                    if (db.Vendors.Any(v => v.CompanyName == vendor.CompanyName))
                    {
                        throw new Exception("Vendor already exist!");
                    }
                    string cur_id = db.Locations.Max(l => l.Id);
                    vendor.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                    db.Vendors.Add(vendor);
                    db.SaveChanges();
                    return Json(new object[] { true, "New Vendor created", vendor, CommonFunc.RenderRazorViewToString("_new_Vendor", vendor, this) });
                }
                else
                {
                    var n_vendor = db.Vendors.Find(vendor.Id);
                    if (n_vendor == null)
                    {
                        throw new Exception("Vendor not found!");
                    }
                    var listmodel = new List<O_Product_Model>();
                    if (n_vendor.Active == true && vendor.Active != true)
                    {
                        listmodel = db.O_Product_Model.ToList();
                        foreach (var model in listmodel)
                        {
                            if (model.Active != false)
                            {
                                model.Active = false;
                                db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                            }
                        }
                    }
                    n_vendor.Active = vendor.Active ?? false;
                    n_vendor.Address = vendor.Address;
                    n_vendor.City = vendor.City;
                    n_vendor.CompanyName = vendor.CompanyName;
                    n_vendor.ContactEmail = vendor.ContactEmail;
                    n_vendor.ContactFirstName = vendor.ContactFirstName;
                    n_vendor.ContactLastName = vendor.ContactLastName;
                    n_vendor.ContactPhone = vendor.ContactPhone;
                    n_vendor.Country = vendor.Country;
                    n_vendor.Description = vendor.Description;
                    n_vendor.Email = vendor.Email;
                    n_vendor.Fax = vendor.Fax;
                    n_vendor.Phone = vendor.Phone;
                    n_vendor.State = vendor.State;
                    n_vendor.VendorType = vendor.VendorType;
                    n_vendor.Website = vendor.Website;
                    n_vendor.Zipcode = vendor.Zipcode;
                    db.Entry(n_vendor).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, "Updated Success", n_vendor, CommonFunc.RenderRazorViewToString("_new_Vendor", n_vendor, this), listmodel.Select(m => m.ModelCode).ToList() });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }

        #endregion
    }


}