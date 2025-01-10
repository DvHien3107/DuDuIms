using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.AppLB;
//using Microsoft.Office.Interop.Excel;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.ViewControler;
using EnrichcousBackOffice.AppLB.UPS;
using SelectPdf;
using System.Xml;
using System.Threading.Tasks;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.Util;
using Microsoft.Office.Interop.Excel;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.Services;
using DataTables.AspNet.Core;
using EnrichcousBackOffice.ViewModel;
using System.Data.Entity;
using Enrich.Core.Infrastructure;
using Enrich.DataTransfer;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class DeviceController : UploadController
    {
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        // GET: Device

        public ActionResult Index()
        {
            if ((access.Any(k => k.Key.Equals("inventory_view")) == false || access["inventory_view"] != true))
            {
                return Redirect("/home/Forbidden");
            }
            WebDataModel db = new WebDataModel();
            var inventory = db.O_Device.ToList();
            ViewBag.Warehouse = db.O_Device.Where(m => !(m.Junkyard ?? false) && m.Inventory == 1).OrderBy(m => m.InvNumber).ToList();
            ViewBag.product = db.O_Product.Where(p => p.IsDeleted != true).ToList();
            return View(inventory);
        }
        public ActionResult ChangeTab(string tabname, string SearchText, string search_location)
        {
            WebDataModel db = new WebDataModel();
            ViewBag.product = db.O_Product.ToList();
            var inventory = new List<Device_Product_view>();

            inventory = (from d in db.O_Device.AsEnumerable()
                         where (CommonFunc.SearchName(d.ProductName, SearchText, true)
                                || CommonFunc.SearchName(d.VendorName, SearchText, true)
                                || CommonFunc.SearchName(d.ModelName, SearchText, true)
                                || CommonFunc.SearchName(d.SerialNumber, SearchText, true)
                                || CommonFunc.SearchName(d.InvNumber, SearchText, true)
                                )
                            && (string.IsNullOrEmpty(search_location) || d.LocationId == search_location)
                            && ((tabname == "All") ||
                            (tabname == "Junkyard" && d.Junkyard == true) ||
                            (tabname == "Assigned" && d.Junkyard != true && d.Inventory == 0) ||
                            (tabname == "Warehouse" && d.Junkyard != true && d.Inventory == 1)
                            )
                         join m in db.O_Product_Model on d.ModelCode equals m.ModelCode
                         where m.IsDeleted != true
                         group new { d, m } by d.ProductCode into dg
                         join p in db.O_Product on dg.Key equals p.Code
                         select new Device_Product_view
                         {
                             Product = p,
                             item = dg.Select(g =>
                                new Device_view
                                {
                                    Device = g.d,
                                    Model = g.m,
                                    Color = g.m.Color,
                                    VendorName = "",
                                    Picture = g.m.Picture
                                }).ToList()
                         }).ToList();
            ViewBag.canExportExcel = (tabname == "All");
            ViewBag.tabname = tabname;
            if (tabname == "Junkyard")
            {
                return PartialView("_PartialListJunkYard", inventory);
            }
            return PartialView("_PartialListDevice", inventory);
        }
        #region Create - Edit - Save - Delete

        /// <summary>
        /// Create and Edit device
        /// </summary>
        /// <param name="DeviceId"></param>
        /// <returns></returns>
        public ActionResult Save(long? DeviceId)
        {
            try
            {
                WebDataModel db = new WebDataModel();

                var Device = new O_Device();

                if (DeviceId > 0) //Edit
                {
                    //Check access edit
                    if (access.Any(k => k.Key.Equals("products_updatedevice")) == false || access["products_updatedevice"] != true)
                    {
                        ViewBag.showSaveBtn = false;
                        //return Redirect("/home/forbidden");
                    }
                    //.End

                    Device = db.O_Device.Find(DeviceId);

                    if (Device == null)
                    {
                        throw new Exception("Device does not exist.");
                    }

                    ViewBag.MoreFiles = db.UploadMoreFiles.Where(f => f.TableId == DeviceId && f.TableName.Equals("O_Device")).ToList();
                    ViewBag.Title = "Edit product";
                }
                else //Create
                {
                    //Check access addnew
                    if (access.Any(k => k.Key.Equals("products_addnewdevice")) == false || access["products_addnewdevice"] != true)
                    {
                        ViewBag.showSaveBtn = false;
                        return Redirect("/home/forbidden");
                    }
                    //.End

                    ViewBag.Title = "New product";
                }

                ViewBag.DeviceType = db.O_Product.ToList();
                ViewBag.Vendor = db.Vendors.ToList();
                ViewBag.ListDeviceAddOn = db.O_Device.Where(d => d.ProductCode == "DEVICES_ADD_ON").ToList();

                ViewBag.Countries = db.Ad_Country.ToList();

                return View(Device);
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error: " + ex.Message;
                return RedirectToAction("index");
            }
        }
        [HttpPost]
        public ActionResult DeleteDevice(string[] seletedIds)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                foreach (var Id in seletedIds)
                {
                    long mDeviceId = long.Parse(Id);
                    var device = db.O_Device.Where(d => d.DeviceId == mDeviceId).FirstOrDefault();
                    if (device != null)
                    {
                        db.O_Device.Remove(device);
                    }
                }
                db.SaveChanges();
                return Json(new { status = true, message = "delete success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        //public JsonResult LoadListFeature()
        //{
        //    var db = new WebDataModel();
        //    var listFeature = db.Device_Features.Select(f => new { text = f.Feature, value = f.Feature }).ToList();
        //    return Json(listFeature);
        //}

        ///// <summary>
        ///// Save device
        ///// </summary>
        ///// <param name="_device"></param>
        ///// <returns></returns>
        //public ActionResult SaveSubmit(O_Device _device)
        //{
        //    try
        //    {
        //        //Check access edit
        //        if (_device.DeviceId > 0)
        //        {
        //            if (access.Any(k => k.Key.Equals("products_updatedevice")) == false || access["products_updatedevice"] != true)
        //            {
        //                return Redirect("/home/forbidden");
        //            }
        //        }
        //        else
        //        {
        //            if (access.Any(k => k.Key.Equals("products_addnewdevice")) == false || access["products_addnewdevice"] != true)
        //            {
        //                return Redirect("/home/forbidden");
        //            }
        //        }
        //        //.End

        //        WebDataModel db = new WebDataModel();
        //        var list_AttachedDevices_Id = Request["attached_devices"];
        //        var list_AttachedDevices_Name = "";

        //        if (list_AttachedDevices_Id != null)
        //        {
        //            var Id_Split = list_AttachedDevices_Id.Split(',');
        //            for (int i = 0; i < Id_Split.Length; i++)
        //            {
        //                var AttachedDevices_Id = long.Parse(Id_Split[i]);
        //                var AttachedDevices_Name = db.O_Device.Where(d => d.DeviceId == AttachedDevices_Id).FirstOrDefault()?.InvNumber;
        //                if (i == (Id_Split.Length - 1))
        //                {
        //                    list_AttachedDevices_Name = list_AttachedDevices_Name + AttachedDevices_Name;
        //                }
        //                else
        //                {
        //                    list_AttachedDevices_Name = list_AttachedDevices_Name + AttachedDevices_Name + ",";
        //                }
        //            }
        //        }


        //        #region Device Type

        //        var typeCode = Request["Product"];
        //        if (string.IsNullOrEmpty(typeCode) == true)
        //        {
        //            throw new Exception("Please choose device type..");
        //        }
        //        var typeName = db.O_Product.Where(t => t.Code.Equals(typeCode)).FirstOrDefault()?.Name;

        //        #endregion

        //        var vendor_id = Request["Vendor"];
        //        var description = Request.Unvalidated["_Description"];
        //        var technical_description = Request.Unvalidated["_TechnicalDescription"];
        //        var manual = Request.Unvalidated["_Manual"];

        //        #region Picture

        //        int filescount = int.Parse((Request["filescount"] ?? "0").ToString());
        //        UploadAttachFile("/upload/products/devices", "pic0", "device_" + _device.DeviceId, "", out string picture);
        //        string deleteFile = Request["hdPicDelete_pic0"];

        //        #endregion


        //        if (_device.DeviceId > 0) //Edit
        //        {
        //            #region Edit Device

        //            var device = db.O_Device.Find(_device.DeviceId);
        //            if (device == null)
        //            {
        //                throw new Exception("Device does not exist.");
        //            }
        //            device.InvNumber = _device.InvNumber;
        //            device.ProductCode = typeCode;
        //            device.ProductName = typeName;
        //            device.Inventory = _device.Inventory ?? 0;
        //            device.Description = description;
        //            device.TechnicalDescription = technical_description;
        //            device.Active = _device.Active;
        //            device.UpdateBy = db.P_Member.Where(m => m.PersonalEmail.Equals(User.Identity.Name)).FirstOrDefault()?.FullName;
        //            device.UpdateAt = DateTime.Now;
        //            device.AddonDevicesId = list_AttachedDevices_Id;
        //            device.AddonDevicesName = list_AttachedDevices_Name;

        //            if (string.IsNullOrEmpty(vendor_id) == false)
        //            {
        //            }
        //            db.Entry(device).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //            TempData["success"] = "Edit device success!";
        //            //UploadMoreFiles(device.DeviceId, "O_Device", filescount, "/upload/products/devices");

        //            #endregion
        //        }

        //        if (Request["submit"].ToString() == "save_return")
        //            return RedirectToAction("index");
        //        else
        //            return RedirectToAction("save");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = "Error: " + ex.Message;
        //        return RedirectToAction("save", new { DeviceId = _device.DeviceId });
        //    }
        //}

        ///// <summary>
        ///// Delete device
        ///// </summary>
        ///// <param name="id">Device id</param>
        ///// <returns></returns>
        //public ActionResult DeleteDevice(long? id)
        //{
        //    try
        //    {
        //        //Check access
        //        if (access.Any(k => k.Key.Equals("products_deletedevice")) == false || access["products_deletedevice"] != true)
        //        {
        //            return Redirect("/home/forbidden");
        //        }
        //        //.End

        //        WebDataModel db = new WebDataModel();

        //        var device = db.O_Device.Find(id);
        //        if (device != null)
        //        {
        //            if (device.Active == false)
        //            {
        //                db.O_Device.Remove(device);
        //            }
        //            else
        //            {
        //                device.Active = false;
        //                db.Entry(device).State = System.Data.Entity.EntityState.Modified;
        //            }

        //            db.SaveChanges();
        //        }

        //        TempData["success"] = "Delete device success!";
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = "Error: " + ex.Message;
        //    }

        //    return RedirectToAction("index");
        //}

        [ValidateInput(false)]
        public JsonResult SendToJunk(long? Id, string desc, int status)

        {
            if (access.Any(k => k.Key.Equals("inventory_manage")) == false || access["inventory_manage"] != true)
            {
                return Json(new object[] { false, "You are not authorized to perform this action!" });
            }
            ////coding.....
            WebDataModel db = new WebDataModel();
            try
            {
                if (Id > 0 && status == 1) //Send device to junktard
                {
                    var Device = db.O_Device.Find(Id);
                    Device.Junkyard = true;
                    Device.JunkyardDescription = desc;
                    db.Entry(Device).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, "Sent product #" + Device.InvNumber + " to Junkyard!" });
                }
                else if (Id > 0 && status == 0) //Return device to Warehouse
                {
                    var Device = db.O_Device.Find(Id);
                    Device.Junkyard = false;
                    Device.JunkyardDescription = desc;
                    db.Entry(Device).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    var m = db.O_Product_Model.Find(Device.ModelCode);
                    var view_device = new { Device.DeviceId, Device.InvNumber, Device.SerialNumber, Device.ProductName, m.Color, Device.JunkyardDescription };

                    return Json(new object[] { true, "Returned product #" + Device.InvNumber + " to Warehouse!", view_device });
                }
                else
                {
                    throw new Exception("device does not exist!");
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e });

            }
        }

        public JsonResult ChangeLocation(long? device_id, string location_id)
        {

            try
            {
                if ((access.Any(k => k.Key.Equals("inventory_manage")) == false || access["inventory_manage"] != true))
                {
                    throw new Exception("Access denied");
                }
                var db = new WebDataModel();
                var device = db.O_Device.Find(device_id);
                if (device.Inventory == 0)
                {
                    throw new Exception("Can't change location of assigned device");
                }
                var location = db.Locations.Find(location_id);
                if (device == null)
                {
                    throw new Exception("Device not found!");
                }
                if (location == null)
                {
                    throw new Exception("Location not found!");
                }
                device.LocationId = location.Id;
                device.LocationName = location.Name;
                db.SaveChanges();
                return Json(new object[] { true, "Device location changed!", location.Name });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        #endregion

        #region Serial Number
        public JsonResult SaveSerialNumber(long? device_id, string serial_number)
        {
            try
            {
                if ((access.Any(k => k.Key.Equals("inventory_manage")) == false || access["inventory_manage"] != true))
                {
                    throw new Exception("Access denied");
                }
                var db = new WebDataModel();
                var device = db.O_Device.Find(device_id);
                if (device == null)
                {
                    throw new Exception("Device not found!");
                }
                var oldSerialNumber = device.SerialNumber;
                device.SerialNumber = serial_number;

                //Change serialNumber of Items assigned
                var orderPro = db.Order_Products.Where(c => c.InvNumbers.Contains(device.InvNumber)).FirstOrDefault();
                if (orderPro != null)
                {
                    //throw new Exception("Do not change the serial number of the assigned Device on an order");
                    if (string.IsNullOrEmpty(orderPro.SerNumbers))
                    {
                        orderPro.SerNumbers = serial_number;
                    }
                    else
                    {
                        var InventoryArr = orderPro.SerNumbers.Split(',').ToList();
                        InventoryArr = InventoryArr.Where(c => c != oldSerialNumber).ToList();
                        if (!string.IsNullOrEmpty(serial_number))
                            InventoryArr.Add(serial_number);
                        orderPro.SerNumbers = string.Join(",", InventoryArr);
                    }
                }

                db.SaveChanges();
                return Json(new object[] { true, "Serial Number save completed!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        #endregion

        #region product productline

        /// <summary>
        /// Get info type
        /// </summary>
        /// <param name="TypeCode">Product code</param>
        /// <returns></returns>
        public JsonResult GetTypeInfo(string TypeCode)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var type = db.O_Product.Where(t => t.Code.Equals(TypeCode)).FirstOrDefault();

                if (type == null)
                {
                    throw new Exception("Device type dose not exist.");
                }

                return Json(new object[] { true, type }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// Save product
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SaveProduct01()
        {

            try
            {

                string OldCode = Request["OldCode"];
                string ProductName = Request["product_name"];
                decimal ProductPrice = decimal.Parse(Request["product_price"]);
                string ProductLine = Request["ProductLine"];
                UploadAttachFile("/Upload/Products", "pic1", null, null, out string filename);
                HttpPostedFileBase file = HttpContext.Request.Files["pic1"];
                WebDataModel db = new WebDataModel();
                if (string.IsNullOrEmpty(ProductName))
                {
                    throw new Exception("Product name cannot be empty!");
                }
                if (db.O_Product.Any(t => t.Name.ToLower().Equals(ProductName.ToLower()) && t.Name.ToLower() != ProductName.ToLower()))
                {
                    throw new Exception("Type name is already exist.");
                }
                if (string.IsNullOrEmpty(OldCode) == true) //Add new
                {

                    var product = new O_Product()
                    {
                        Code = CommonFunc.ConvertNonUnicodeURL(ProductName).Replace("-", "_").ToLower(),
                        Name = ProductName,
                        ProductLineCode = ProductLine,
                        Picture = filename
                    };

                    db.O_Product.Add(product);
                    db.SaveChanges();
                    return Json(new { status = true, text = "Add new product done!", code = product.Code, name = product.Name, OldCode });
                }
                else //Edit
                {
                    var oldproduct = db.O_Product.Find(OldCode);
                    if (oldproduct == null)
                    {
                        throw new Exception("Product does not exist.");
                    }

                    var product = new O_Product()
                    {
                        Code = CommonFunc.ConvertNonUnicodeURL(ProductName).Replace("-", "_").ToLower(),
                        Name = ProductName,
                        ProductLineCode = ProductLine,
                        Picture = filename
                    };


                    string deleteFile = Request["hdPicDelete_pic1"];
                    if (string.IsNullOrWhiteSpace(oldproduct.Picture) == false && deleteFile == "0")
                    {
                        product.Picture = oldproduct.Picture;
                    }
                    if (string.IsNullOrWhiteSpace(oldproduct.Picture) == false && deleteFile == "1")
                    {
                        //xoa file
                        try
                        {
                            FileInfo f = new FileInfo(Server.MapPath(oldproduct.Picture));
                            if (f.Exists)
                            {
                                f.Delete();
                            }
                        }
                        catch (Exception)
                        {
                        }
                        product.Picture = null;
                    }
                    if (string.IsNullOrWhiteSpace(filename) == false)
                    {
                        product.Picture = filename;
                    }

                    //update product for device
                    var list_Device = db.O_Device.Where(m => m.ProductCode == oldproduct.Code).ToList();
                    foreach (var device in list_Device)
                    {
                        device.ProductCode = product.Code;
                        device.ProductName = product.Name;
                        db.Entry(device).State = System.Data.Entity.EntityState.Modified;
                    }

                    db.O_Product.Remove(oldproduct);
                    db.O_Product.Add(product);
                    db.SaveChanges();
                    return Json(new { status = true, text = "Edit product done!", code = product.Code, name = product.Name, OldCode });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, text = "Error: " + ex.Message });
            }
        }


        /// <summary>
        /// Delete product
        /// </summary>
        /// <param name="TypeCode"> product code</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeleteType(string TypeCode)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var type = db.O_Product.Where(t => t.Code.Equals(TypeCode)).FirstOrDefault();
                if (type != null)
                {
                    db.O_Product.Remove(type);
                    db.SaveChanges();
                }

                var list_Type = db.O_Product.ToList();

                return Json(new object[] { true, list_Type });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        #endregion

        #region Vendor

        /// <summary>
        /// Get info vendor
        /// </summary>
        /// <param name="VendorId"></param>
        /// <returns></returns>
        public JsonResult GetVendorInfo(long? VendorId)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var vendor = db.Vendors.Find(VendorId);

                if (vendor == null)
                {
                    throw new Exception("Vendor dose not exist.");
                }

                return Json(new object[] { true, vendor }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        /// <summary>
        /// Save vendor
        /// </summary>
        /// <param name="VendorId"></param>
        /// <param name="CompanyName"></param>
        /// <param name="Address"></param>
        /// <param name="Country"></param>
        /// <param name="State"></param>
        /// <param name="City"></param>
        /// <param name="ZipCode"></param>
        /// <param name="Website"></param>
        /// <param name="Email"></param>
        /// <param name="Fax"></param>
        /// <param name="Phone"></param>
        /// <param name="FirstName"></param>
        /// <param name="LastName"></param>
        /// <param name="ContactPhone"></param>
        /// <param name="ContactEmail"></param>
        /// <returns></returns>
        public JsonResult SaveVendor(long? VendorId, string CompanyName, string Address, string Country,
            string State, string City, string ZipCode, string Website, string Email, string Fax, string Phone,
            string FirstName, string LastName, string ContactPhone, string ContactEmail)
        {
            try
            {
                WebDataModel db = new WebDataModel();

                if (VendorId > 0) //Edit
                {
                    var vendor = db.Vendors.Find(VendorId);
                    if (vendor == null)
                    {
                        throw new Exception("Vendor dose not exist.");
                    }

                    if (db.Vendors.Any(v => v.CompanyName.Equals(CompanyName)) == true && vendor.CompanyName != CompanyName)
                    {
                        throw new Exception("Company name is already exist.");
                    }

                    vendor.CompanyName = CompanyName;
                    vendor.Address = Address;
                    vendor.Country = Country;
                    vendor.State = State;
                    vendor.City = City;
                    vendor.Zipcode = ZipCode;
                    vendor.Website = Website;
                    vendor.Email = Email;
                    vendor.Fax = Fax;
                    vendor.Phone = Phone;
                    vendor.ContactFirstName = FirstName;
                    vendor.ContactLastName = LastName;
                    vendor.ContactPhone = ContactPhone;
                    vendor.ContactEmail = ContactEmail;

                    db.Entry(vendor).State = System.Data.Entity.EntityState.Modified;
                }
                else //Add new
                {
                    if (db.Vendors.Any(v => v.CompanyName.Equals(CompanyName)) == true)
                    {
                        throw new Exception("Company name is already exist.");
                    }

                    VendorId = db.Vendors.ToList().Count() > 0 ? (db.Vendors.Max(v => v.Id) + 1) : 1;
                    var vendor = new Vendor()
                    {
                        Id = VendorId ?? 0,
                        CompanyName = CompanyName,
                        Address = Address,
                        Country = Country,
                        State = State,
                        City = City,
                        Zipcode = ZipCode,
                        Website = Website,
                        Email = Email,
                        Fax = Fax,
                        Phone = Phone,
                        ContactFirstName = FirstName,
                        ContactLastName = LastName,
                        ContactPhone = ContactPhone,
                        ContactEmail = ContactEmail,
                        Active = true,
                        VendorType = "Vendor device"
                    };

                    db.Vendors.Add(vendor);
                }

                db.SaveChanges();
                var list_Vendor = db.Vendors.ToList();

                return Json(new object[] { true, list_Vendor, VendorId });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }


        #endregion

        #region Dashboard

        public ActionResult Dashboard(string Page = "all")
        {
            if ((access.Any(k => k.Key.Equals("inventory_dashboard")) == false || access["inventory_dashboard"] != true))
            {
                return Redirect("/home/forbidden");
            }
            UserContent.TabHistory = "Deployment|/Device/Dashboard";
            var db = new WebDataModel();
            ViewBag.p = access;
            ViewBag.history = db.O_Orders.Where(o => db.Order_Products.Any(op => op.OrderCode == o.OrdersCode) && o.InvoiceNumber > 0).OrderByDescending(o => o.UpdatedAt).Take(10).ToList();
            ViewBag.canViewMore = db.O_Orders.Where(o => db.Order_Products.Any(op => op.OrderCode == o.OrdersCode) && o.InvoiceNumber > 0).Count() > 10;
            ViewBag.packaging_field = db.I_ProcessSetting.Where(p => p.FieldType == "Packaging").ToList();
            TempData["Page"] = Page;
            //ViewBag.testing_field = db.I_ProcessSetting.Where(p => p.FieldType == "Testing").ToList();
            //ViewBag.shipping_field = db.I_ProcessSetting.Where(p => p.FieldType == "Shipping").ToList();
            //var m = db.O_Orders.Where(o => db.Order_Products.Any(op => op.OrderCode == o.OrdersCode) && o.InvoiceNumber > 0 && o.IsDelete != true)
            //    .Select(o => new OrderPackage_view
            //    {
            //        Id = o.Id,
            //        OrdersCode = o.OrdersCode,
            //        Status = o.Status,
            //        GrandTotal = o.GrandTotal,
            //        CustomerName = o.CustomerName,
            //        BundelStatus = o.BundelStatus,
            //        UpdatedBy = o.UpdatedHistory,
            //        Packages = (from op in db.Order_Products
            //                    where op.OrderCode == o.OrdersCode
            //                    group op by op.BundleId into g
            //                    join b in db.I_Bundle on g.Key equals b.Id
            //                    select new package { Code = b.BundleCode, Name = b.Name }
            //                    ).ToList(),
            //        TicketCode = (from st in db.T_SupportTicket
            //                      where st.OrderCode == o.OrdersCode && st.TypeId == 4
            //                      select st.Id
            //                    ).FirstOrDefault()
            //    }).OrderByDescending(ord => ord.OrdersCode).ToList();
            //var e = m.Where(s => s?.Packages.Count > 0).ToList();
            return View();
        }

        public ActionResult LoadDeployment(IDataTablesRequest dataTablesRequest, string Status)
        {
            var db = new WebDataModel();
            var dataView = db.O_Orders.Where(o => db.Order_Products.Any(op => op.OrderCode == o.OrdersCode) && o.InvoiceNumber > 0 && o.IsDelete != true)
                .Select(o => new OrderPackage_view
                {
                    Id = o.Id,
                    OrdersCode = o.OrdersCode,
                    Status = o.Status,
                    GrandTotal = o.GrandTotal,
                    CustomerName = o.CustomerName,
                    BundelStatus = o.BundelStatus,
                    UpdatedBy = o.UpdatedHistory,
                    Packages = (from op in db.Order_Products
                                where op.OrderCode == o.OrdersCode
                                group op by op.BundleId into g
                                join b in db.I_Bundle on g.Key equals b.Id
                                select new package { Code = b.BundleCode, Name = b.Name }
                                ).ToList(),
                    TicketCode = (from st in db.T_SupportTicket.Include(x=>x.T_TicketTypeMapping)
                                  where st.OrderCode == o.OrdersCode && st.T_TicketTypeMapping.Any(y=>y.TypeId == 4)
                                  select st.Id
                                ).FirstOrDefault()
                }).OrderByDescending(ord => ord.OrdersCode).AsEnumerable();
            if (Status.ToLower() == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString().ToLower())
                dataView = dataView.Where(c => c.BundelStatus.ToLower() == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString().ToLower());
            else
                dataView = dataView.Where(c => Status.ToLower() == "all" || c.BundelStatus.ToLower() != UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString().ToLower());

            if (!string.IsNullOrEmpty(dataTablesRequest.Search.Value))
                dataView = dataView.Where(c => CommonFunc.SearchName(c.OrdersCode, dataTablesRequest.Search.Value) ||
                                                CommonFunc.SearchName(c.Status, dataTablesRequest.Search.Value) ||
                                                CommonFunc.SearchName(c.GrandTotal.ToString(), dataTablesRequest.Search.Value) ||
                                                CommonFunc.SearchName(c.CustomerName, dataTablesRequest.Search.Value) ||
                                                CommonFunc.SearchName(c.TicketCode.ToString(), dataTablesRequest.Search.Value) ||
                                                CommonFunc.SearchName(c.BundelStatus, dataTablesRequest.Search.Value));

            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "OrderCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.OrdersCode);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.OrdersCode);
                    }
                    break;
                case "Ticket":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.TicketCode);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.TicketCode);
                    }
                    break;
                case "MerchantName":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.CustomerName);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.CustomerName);
                    }
                    break;
                case "Total":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.GrandTotal);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.GrandTotal);
                    }
                    break;
                case "LastUpdate":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => getDateFromHistory(s.UpdatedBy));
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => getDateFromHistory(s.UpdatedBy));
                    }
                    break;
                case "Station":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.Packages.Min(p => p.Code));
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.Packages.Max(p => p.Code));
                    }
                    break;
                case "Status":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        dataView = dataView.OrderBy(s => s.BundelStatus);
                    }
                    else
                    {
                        dataView = dataView.OrderByDescending(s => s.BundelStatus);
                    }
                    break;
                default:
                    dataView = dataView.OrderByDescending(s => s.OrdersCode);
                    break;
            }

            var totalRecords = dataView.Count();
            dataView = dataView.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            return Json(new
            {
                recordsFiltered = totalRecords,
                recordsTotal = totalRecords,
                draw = dataTablesRequest.Draw,
                data = dataView.ToList()
            });
        }

        public DateTime getDateFromHistory(string history)
        {
            try
            {
                if (string.IsNullOrEmpty(history))
                    throw new Exception();
                var lastUpdate = history.Split('|').Last().Replace("-", "<br/>");
                var dateText = Convert.ToDateTime(lastUpdate.Substring(0, lastUpdate.IndexOf("<br/>")));
                return dateText;
            }
            catch (Exception)
            {
                return new DateTime(1990, 1, 1);
            }
        }

        public ActionResult DashboardLoad()
        {
            var db = new WebDataModel();
            var m = db.O_Orders.Where(o => db.Order_Products.Any(op => op.OrderCode == o.OrdersCode) && o.InvoiceNumber > 0 && o.IsDelete != true)
                .Select(o => new OrderPackage_view
                {
                    Id = o.Id,
                    OrdersCode = o.OrdersCode,
                    Status = o.Status,
                    GrandTotal = o.GrandTotal,
                    CustomerName = o.CustomerName,
                    BundelStatus = o.BundelStatus,
                    UpdatedBy = o.UpdatedHistory,
                    Packages = (from op in db.Order_Products
                                where op.OrderCode == o.OrdersCode
                                group op by op.BundleId into g
                                join b in db.I_Bundle on g.Key equals b.Id
                                select new package { Code = b.BundleCode, Name = b.Name }
                                ).ToList(),
                    TicketCode = (from st in db.T_SupportTicket.Include(x=>x.T_TicketTypeMapping)
                                  where st.OrderCode == o.OrdersCode && st.T_TicketTypeMapping.Any(x=>x.TypeId == 4)
                                  select st.Id
                                ).FirstOrDefault()
                }).OrderByDescending(ord => ord.OrdersCode).ToList();

            return PartialView("_Dashboard_load", m);
        }

        /// <summary>
        /// for packaging feature
        /// </summary>
        /// <param name="id">OrderId</param>
        /// <returns></returns>
        public JsonResult getOrderProgress(long id)
        {
            try
            {
                var db = new WebDataModel();
                var order = db.O_Orders.Find(id);
                if (order.BundelStatus != UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString())
                {
                    var order_product = (from op in db.Order_Products
                                         where op.OrderCode == order.OrdersCode
                                         group op by op.BundleId into g
                                         select new OrderProgress_view
                                         {
                                             BundleId = g.Key,
                                             Contains = (from ig in g
                                                         join pm in db.O_Product_Model on ig.ModelCode equals pm.ModelCode
                                                         select new OrderProgressDetail_view
                                                         {
                                                             OrderProductId = ig.Id,
                                                             OrderId = ig.Id,
                                                             ProductName = ig.ProductName,
                                                             ModelCode = ig.ModelCode,
                                                             ModelName = ig.ModelName,
                                                             Quantity = ig.Quantity,
                                                             Price = ig.Price,
                                                             //VendorName = pm.VendorName,
                                                             Color = pm.Color,
                                                             Picture = pm.Picture,
                                                             List_invs = db.O_Device.Where(d => d.ModelCode == ig.ModelCode && (d.Inventory == 1 || ig.InvNumbers.Contains(d.InvNumber))).Select(d => d.InvNumber).ToList(),
                                                             Invs_selected = ig.InvNumbers,
                                                             List_sers = db.O_Device.Where(d => d.ModelCode == ig.ModelCode && !string.IsNullOrEmpty(d.SerialNumber) && (d.Inventory == 1 || ig.InvNumbers.Contains(d.InvNumber))).Select(d => d.SerialNumber).ToList(),
                                                             Sers_selected = ig.SerNumbers,
                                                             DeviceRequired = pm.DeviceRequired,
                                                             List_cuss = ig.CusNumbers
                                                         }).ToList()
                                         }).ToList();
                    var order_product_html = RenderRazorViewToString("_PartialOrder_SelectDeviceList", order_product);
                    string argent = CommonFunc.GetLastRowString(order.UpdatedHistory ?? "", '|');
                    var progress_fill = //db.I_ProgressFill.Where(p => p.OrderCode == order.OrdersCode).ToList();
                    (from f in db.I_ProgressFill
                     where f.OrderCode == order.OrdersCode
                     join p in db.I_ProcessSetting on f.FieldFillId equals p.Id
                     select new { f.FieldFillId, f.Content, p.IsCheck }).ToList();
                    var order_carrier = db.Order_Carrier.Where(c => c.OrderCode == order.OrdersCode).FirstOrDefault();
                    //bool read_only = order.BundelStatus == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString();
                    return Json(new object[] { true, order, order_product_html, argent, progress_fill, order_carrier, order_carrier?.DateShipped.HasValue == true ? order_carrier.DateShipped.Value.ToString("MM/dd/yyyy") : "" });
                }
                else
                {
                    throw new Exception("Deployment ticket for this order not completed!");
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }


        #region UPS package register

        /// <summary>
        /// for packaging feature
        /// </summary>
        /// <param name="id">order id</param>
        /// <returns></returns>
        public JsonResult GetUPSPackage(long id)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    var orderTk = (from tk in db.Order_UPSTracking
                                   join ord in db.O_Orders on tk.OrderCode equals ord.OrdersCode
                                   where ord.Id == id && tk.Active == 1
                                   select tk).FirstOrDefault();
                    if (orderTk == null)
                    {
                        throw new Exception("Order package is notfound");
                    }
                    var upsPackage = db.Order_UPSPackage.Where(p => p.OrderUPSTrackingId == orderTk.TrackingNumber).ToList();
                    return Json(new object[] { orderTk, upsPackage });
                }
                catch (Exception)
                {
                    return Json(new object[] { null, null });
                }
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ordid">Order id</param>
        /// <param name="scode">UPS service code</param>
        /// <param name="packageWeight">package weight, cach nhau = dau |</param>
        /// <param name="desc">Shipment description</param>
        /// <param name="ship_address"></param>
        /// <returns></returns>
        public JsonResult UPS_PackageSubmit(string ordid, string packageWeight, string scode, string desc, string ship_address)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    var ord = db.O_Orders.Find(long.Parse(ordid));
                    ord.ShippingAddress = ship_address;

                    Shipment shipment = new Shipment();
                    string note = /*"Order#" + ord.InvoiceNumber.ToString() + "\n" +*/ desc;
                    var service_code = scode.Split(new char[] { '-' })[0];
                    UPSResultMessage result = shipment.Ship(ord, service_code, packageWeight, note);
                    if (result.Result == 1)
                    {
                        var ordTracking = new Order_UPSTracking
                        {
                            Id = Guid.NewGuid().ToString(),
                            OrderCode = ord.OrdersCode,
                            TrackingNumber = result.TrackingNumber,
                            Status = result.status,
                            ShippmentDescription = note,
                            UPS_Service_Code = scode,
                            Active = 1
                        };
                        db.Order_UPSTracking.Add(ordTracking);
                        int i = 0;
                        List<string> pckid = new List<string>();
                        foreach (var item in packageWeight.Split(new char[] { '|' }))
                        {
                            if (string.IsNullOrWhiteSpace(item))
                            {
                                i++;
                                continue;
                            }
                            var item_data = item.Split(new char[] { '-' });
                            var ordPackage = new Order_UPSPackage
                            {
                                Id = Guid.NewGuid().ToString(),
                                OrderUPSTrackingId = result.TrackingNumber,
                                PackageWeight = item_data[0],
                                PackageType = item_data[1] + "- " + item_data[2],
                                ImageLabelBase64 = (result.Data as List<string>).ElementAt(i)
                            };
                            db.Order_UPSPackage.Add(ordPackage);
                            pckid.Add(ordPackage.Id);
                            i++;
                        }

                        db.Entry(ord).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        var ticket = db.T_SupportTicket.Include(x=>x.T_TicketTypeMapping).Where(t => t.OrderCode == ord.OrdersCode && t.T_TicketTypeMapping.Any(x=>x.TypeId == (long)UserContent.TICKET_TYPE.Deployment)).FirstOrDefault();
                        string tic_content = " - TRACKING NUMBER:<b>" + result.TrackingNumber + "</b><br/>" +
                            @" - Package Quanity:" + (packageWeight.Split(new char[] { '|' }).Length - 1) + "<br/>" +
                            @" - Package Weight:(LBS): <b>" + packageWeight.Remove(packageWeight.Length - 1) + "</b>";
                        TicketViewService.InsertFeedback(db, ticket.Id, cMem.FullName + " just submited to UPS", tic_content, "");

                        return Json(new UPSResultMessage { Result = 1, Data = pckid, TrackingNumber = result.TrackingNumber });
                    }
                    else
                    {
                        throw new Exception(result.Msg);
                    }


                }
                catch (Exception e)
                {
                    return Json(new UPSResultMessage { Result = -1, Msg = e.Message });
                }

            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tn">Tracking number</param>
        /// <param name="update">true: Update tracking record in database is 'inactive' to ready for create new package</param>
        /// <returns></returns>
        public JsonResult UPS_Cancel(string tn, bool update = false)
        {
            var db = new WebDataModel();
            var ord_tracking = db.Order_UPSTracking.Where(t => t.TrackingNumber == tn).FirstOrDefault();
            if (update)
            {
                //cancel tu UPS failure, can update database inactive package nay de san sang submit package moi.
                //var trackingNumber = db.Order_UPSTracking.Where(t => t.TrackingNumber == tn && t.Active == 1).FirstOrDefault();
                ord_tracking.Active = -1;
                db.Entry(ord_tracking).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();


                //cancel tu UPS khong thanh cong
                var ticket = db.T_SupportTicket.Include(x=>x.T_TicketTypeMapping).Where(t => t.OrderCode == ord_tracking.TrackingNumber && t.T_TicketTypeMapping.Any(x=>x.TypeId == (long)UserContent.TICKET_TYPE.Deployment)).FirstOrDefault();
                if (ticket != null)
                {
                    string tic_content = " - TRACKING NUMBER: <b>#" + tn + "</b><br/>" +
                        @" - STATUS: <b>CANCEL FAILURE</b><br/>" +
                        @"Note: - UPS does not allow to cancel the shipment now.<br/>YOU CAN TO DO IT DIRECTLY ON THE UPS SITE.";
                    TicketViewService.InsertFeedback(db, ticket.Id, "Tracking number <b>#" + tn + "</b> has been disabled", tic_content, "", (long)UserContent.DeploymentTicket_Status.Open);

                }

                //update bundel status trong order
                var ord = db.O_Orders.Where(o => o.OrdersCode == ord_tracking.OrderCode).FirstOrDefault();
                if (ord.BundelStatus == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString())
                {
                    ord.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Ready.ToString();
                    db.Entry(ord).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                return Json(true);
            }
            else
            {
                Shipment voidShipment = new Shipment();
                var result = voidShipment.Void(tn);
                if (result.Result == 1)
                {
                    foreach (var item in db.Order_UPSPackage.Where(p => p.OrderUPSTrackingId == tn))
                    {
                        db.Order_UPSPackage.Remove(item);
                    }
                    db.Order_UPSTracking.Remove(ord_tracking);
                }
                db.SaveChanges();

                //cancel tu UPS thanh cong
                var ticket = db.T_SupportTicket.Include(x=>x.T_TicketTypeMapping).Where(t => t.OrderCode == ord_tracking.TrackingNumber && t.T_TicketTypeMapping.Any(x=>x.TypeId == (long)UserContent.TICKET_TYPE.Deployment)).FirstOrDefault();
                if (ticket != null)
                {
                    string tic_content = " - TRACKING NUMBER: <b>#" + tn + "</b><br/>" +
                        @" - STATUS: <b>CANCELED</b><br/>";
                    TicketViewService.InsertFeedback(db, ticket.Id, "Tracking number <b>#" + result.TrackingNumber + "</b> has been canceled", tic_content, "", (long)UserContent.DeploymentTicket_Status.Open);

                }

                //update bundel status trong order
                var ord = db.O_Orders.Where(o => o.OrdersCode == ord_tracking.OrderCode).FirstOrDefault();
                if (ord.BundelStatus == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString())
                {
                    ord.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Ready.ToString();
                    db.Entry(ord).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                return Json(result);
            }

        }

        public ActionResult UpsLabelPrinter(string id)
        {
            var package = new WebDataModel().Order_UPSPackage.Find(id);
            return View(package);
        }
        #endregion
        public JsonResult LoadHistory(int n)
        {
            var db = new WebDataModel();
            var history = db.O_Orders.Where(o => db.Order_Products.Any(op => op.OrderCode == o.OrdersCode) && o.InvoiceNumber > 0).OrderByDescending(o => o.UpdatedAt).Skip(n).Take(10).ToList();
            var canViewMore = db.O_Orders.Where(o => db.Order_Products.Any(op => op.OrderCode == o.OrdersCode) && o.InvoiceNumber > 0).Count() > (10 + n);

            return Json(new object[] { true, RenderRazorViewToString("_partial_bundle_history", history), canViewMore });
        }
        #endregion
        #region Package management
        public ActionResult Package()
        {
            if ((access.Any(k => k.Key.Equals("inventory_create_package")) == false || access["inventory_create_package"] != true))
            {
                return Redirect("/home/forbidden");
            }
            var db = new WebDataModel();
            ViewBag.p = access;
            //ViewBag.orders = db.O_Orders.Where(o => db.Order_Products.Any(od => od.OrderCode == o.OrdersCode)).OrderBy(o => o.CustomerName).ToList();
            //var m = db.I_Bundle.OrderByDescending(b => b.UpdateAt).ToList();
            ViewBag.history = db.I_Bundle.OrderByDescending(b => b.UpdateAt ?? b.CreateAt).Take(10).ToList();
            ViewBag.canViewMore = db.I_Bundle.Count() > 10;

            ViewBag.list_line = db.O_Product_Line.ToList();

            var bundles = db.I_Bundle.Select(b => new I_Bundle_view
            {
                Id = b.Id,
                BundleCode = b.BundleCode,
                Info = b.Info,
                Name = b.Name,
                Price = b.Price,
                list_model = (from bd in db.I_Bundle_Device
                              join m in db.O_Product_Model on bd.ModelCode equals m.ModelCode
                              where bd.Bundle_Id == b.Id
                              select new I_Bundle_Device_view
                              {
                                  ModelCode = bd.ModelCode,
                                  ModelName = bd.ModelName,
                                  Quantity = bd.Quantity,
                                  ProductName = m.ProductName,
                                  Color = m.Color,
                                  //Vendor = m.VendorName
                              }).ToList(),
                UpdateAt = b.UpdateAt,
                UpdateBy = b.UpdateBy
            }).OrderByDescending(b => b.Id).ToList();
            return View(bundles);
        }
        [AllowAnonymous]
        public ActionResult PDF_package(string FullName)
        {
            var db = new WebDataModel();
            var bundles = (from bd in db.I_Bundle_Device
                           join m in db.O_Product_Model on bd.ModelCode equals m.ModelCode
                           group new { bd, m } by bd.Bundle_Id into Gr
                           join b in db.I_Bundle on Gr.Key equals b.Id
                           select new Bundle_view { Bundle = b, Detail = Gr.Select(g => new Bundle_Detail_view { BundleDevice = g.bd, Model = g.m }).ToList() }).ToList();
            ViewBag.NameCreated = FullName;
            return View(bundles);
        }


        public JsonResult ConvertToPdf()
        {
            try
            {
                HtmlToPdf converter = new HtmlToPdf();

                // set converter options
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                //converter.Options.MarginLeft = 50;
                //converter.Options.MarginRight = 50;
                converter.Options.MarginTop = 50;
                converter.Options.MarginBottom = 50;
                // create a new pdf document converting an html string
                var ReqURL = Request.Url.Authority;
                PdfDocument doc = converter.ConvertUrl("http://" + ReqURL + "/device/PDF_package?FullName=" + cMem.FullName);

                // save pdf document
                string strFileName = DateTime.Now.ToString("yyyyMMdd_hhmmss") + "_Package.pdf";
                string strPath = Path.Combine(Server.MapPath("~/upload/PDF/"), strFileName);
                doc.Save(strPath);
                var _url_return = "/upload/PDF/" + strFileName;

                // close pdf document
                doc.Close();

                return Json(new object[] { true, _url_return });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }


        //Load danh sach bundle khi chuyen sang tab bundle hardware
        public ActionResult LoadBundleTable()
        {
            WebDataModel db = new WebDataModel();
            var bundles = db.I_Bundle.Select(b => new I_Bundle_view
            {
                Id = b.Id,
                BundleCode = b.BundleCode,
                Info = b.Info,
                Name = b.Name,
                Price = b.Price,
                list_model = (from bd in db.I_Bundle_Device
                              join m in db.O_Product_Model on bd.ModelCode equals m.ModelCode
                              where bd.Bundle_Id == b.Id
                              select new I_Bundle_Device_view
                              {
                                  ModelCode = bd.ModelCode,
                                  ModelName = bd.ModelName,
                                  Quantity = bd.Quantity,
                                  ProductName = m.ProductName,
                                  Color = m.Color,
                                  //Vendor = m.VendorName
                              }).ToList(),
                UpdateAt = b.UpdateAt,
                UpdateBy = b.UpdateBy
            }).OrderByDescending(b => b.Id).ToList();


            return PartialView("_Partial_DashboardBundle", bundles);
        }
        //Load bundle va show modal edit bundle
        public JsonResult loadBundle(long? id)
        {
            try
            {
                var db = new WebDataModel();
                var bundle = db.I_Bundle.Find(id);
                if (id != null)
                {
                    if ((access.Any(k => k.Key.Equals("inventory_edit_package")) == false || access["inventory_edit_package"] != true))
                    {
                        throw new Exception("Access denied");
                    }
                    if (bundle == null)
                    {
                        throw new Exception("Bundle not found!");
                    }
                    var bundle_model = (from bm in db.I_Bundle_Device
                                        where bm.Bundle_Id == id
                                        join m in db.O_Product_Model on bm.ModelCode equals m.ModelCode
                                        select new O_Product_Model_Selected_view
                                        {
                                            ProductCode = m.ProductCode,
                                            ModelCode = m.ModelCode,
                                            ModelName = m.ModelName,
                                            ProductName = m.ProductName,
                                            //VendorName = m.VendorName,
                                            Color = m.Color,
                                            Quantity = bm.Quantity,
                                            Price = bm.Price,
                                            Picture = m.Picture
                                        }).ToList();
                    Session["selected_model"] = bundle_model;
                    var List_model_html = RenderRazorViewToString("_Partial_ListProduct_SelectedModel", bundle_model);
                    return Json(new object[] { true, bundle, List_model_html, bundle_model.Select(b => b.ModelCode).ToList() });
                }
                else
                {
                    if ((access.Any(k => k.Key.Equals("inventory_create_package")) == false || access["inventory_create_package"] != true))
                    {
                        throw new Exception("Access denied");
                    }
                    Session["selected_model"] = new List<O_Product_Model_Selected_view>();
                    return Json(new object[] { true, new I_Bundle(), "" });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Fail: " + e.Message });
            }

        }
        //Tim kiem Product_model de chon vao bundle
        public JsonResult SearchProduct_bundle(string line, string text)
        {
            var db = new WebDataModel();
            var products = db.O_Product.Where(p => (string.IsNullOrEmpty(line) || p.ProductLineCode == line) && p.Name.Contains(text)).ToList();
            var list_pr_code = products.Select(p => p.Code).ToList();
            ViewBag.list_model = db.O_Product_Model.GroupBy(m => m.ProductCode).Where(g => list_pr_code.Contains(g.Key)).ToList();
            var view = RenderRazorViewToString("_Partial_ListProduct", products);

            var list_select = db.O_Product_Model.Select(pm => new { pm.ProductCode, pm.Color, pm.Picture }).ToList();
            return Json(new object[] { true, view, list_select });
        }
        //Chọn Product_model vào bundle
        public JsonResult SelectModelBundle(string model_code)
        {
            try
            {
                var db = new WebDataModel();
                List<O_Product_Model_Selected_view> selected_model = (List<O_Product_Model_Selected_view>)(Session["selected_model"] ?? new List<O_Product_Model_Selected_view>());
                var model = db.O_Product_Model.Find(model_code);
                if (model == null)
                {
                    throw new Exception("Model not found!");
                }
                var model_view = new O_Product_Model_Selected_view
                {
                    ProductCode = model.ProductCode,
                    ModelCode = model.ModelCode,
                    ModelName = model.ModelName,
                    ProductName = model.ProductName,
                    //VendorName = model.VendorName,
                    Picture = model.Picture,
                    Color = model.Color,
                    Price = model.Price,
                    Quantity = 1
                };
                if (!selected_model.Any(s => s.ModelCode == model.ModelCode))
                {
                    selected_model.Add(model_view);
                    Session["selected_model"] = selected_model;
                    ViewBag.ProductPicture = db.O_Product.Where(p => p.Code == model.ProductCode).Select(p => new ProductPicture { Code = p.Code, Picture = p.Picture }).ToList();
                    var view = RenderRazorViewToString("_Partial_ListProduct_SelectedModel", new List<O_Product_Model_Selected_view> { model_view });
                    return Json(new object[] { true, view });
                }
                else
                {
                    throw new Exception("Model already seleted!");
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        //Bỏ chọn product_model khỏi bundle
        public JsonResult unSelectModelBundle(string model_code)
        {
            try
            {
                List<O_Product_Model_Selected_view> selected_model = (List<O_Product_Model_Selected_view>)(Session["selected_model"] ?? new List<O_Product_Model_Selected_view>());
                var remove_model = selected_model.Where(s => s.ModelCode == model_code).FirstOrDefault();
                if (remove_model == null)
                    throw new Exception("Select model not found");
                selected_model.Remove(remove_model);
                Session["selected_model"] = selected_model;

                return Json(new object[] { true });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }


        }
        //Save bundle
        public JsonResult SaveBundle(long? bundle_id, string bundle_name, string bundle_info)
        {
            try
            {
                var db = new WebDataModel();
                List<O_Product_Model_Selected_view> selected_model = (List<O_Product_Model_Selected_view>)Session["selected_model"];
                if (selected_model.Count <= 0)
                    throw new Exception("Package must have at least one model!");
                long id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                var bundle = new I_Bundle();
                if (bundle_id > 0)
                {
                    if ((access.Any(k => k.Key.Equals("inventory_edit_package")) == false || access["inventory_edit_package"] != true))
                    {
                        throw new Exception("Access denied");
                    }
                    bundle = db.I_Bundle.Find(bundle_id);
                    if (bundle == null)
                    {
                        throw new Exception("Package not found!");
                    }
                    bundle.Name = bundle_name;
                    bundle.Info = bundle_info;

                    //bundle.Price = total;
                    bundle.UpdateAt = DateTime.UtcNow;
                    bundle.UpdateBy = cMem.FullName;
                    var old_bundle_model = db.I_Bundle_Device.Where(bd => bd.Bundle_Id == bundle.Id).ToList();
                    db.I_Bundle_Device.RemoveRange(old_bundle_model);
                    db.Entry(bundle).State = System.Data.Entity.EntityState.Modified;

                }
                else
                {
                    if ((access.Any(k => k.Key.Equals("inventory_create_package")) == false || access["inventory_create_package"] != true))
                    {
                        throw new Exception("Access denied");
                    }
                    bundle.Id = id;
                    bundle.BundleCode = (int.Parse(db.I_Bundle.Select(b => b.BundleCode).Max() ?? "0") + 1).ToString().PadLeft(5, '0');
                    bundle.Name = bundle_name;
                    bundle.Info = bundle_info;

                    bundle.CreateAt = bundle.UpdateAt = DateTime.UtcNow;
                    bundle.CreateBy = bundle.UpdateBy = cMem.FullName;
                    db.I_Bundle.Add(bundle);
                }
                decimal total_price = 0;
                foreach (var m in selected_model)
                {
                    var qty = int.Parse(Request["model_qty_" + m.ModelCode]);
                    var price = decimal.Parse(Request["price_" + m.ModelCode]);

                    if (qty <= 0)
                        throw new Exception("Model " + m.ModelCode + ": quantity (" + qty + ") must at least one!");
                    total_price += price * qty;
                    var new_b_model = new I_Bundle_Device
                    {
                        Bundle_Id = bundle.Id,
                        Id = id++,
                        ModelCode = m.ModelCode,
                        ModelName = m.ModelName,
                        Price = price,
                        Quantity = qty
                    };
                    db.I_Bundle_Device.Add(new_b_model);
                }
                db.SaveChanges();
                db.I_Bundle.Find(bundle.Id).Price = total_price;
                db.SaveChanges();
                return Json(new object[] { true });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        //delete bundle
        public JsonResult delete_bundle(long id)
        {
            if ((access.Any(k => k.Key.Equals("inventory_delete_package")) == false || access["inventory_delete_package"] != true))
            {
                throw new Exception("Access denied");
            }
            try
            {
                var db = new WebDataModel();
                var b_model = db.I_Bundle_Device.Where(d => d.Bundle_Id == id).ToList();
                db.I_Bundle_Device.RemoveRange(b_model);
                var bundle = db.I_Bundle.Find(id);
                db.I_Bundle.Remove(bundle);
                db.SaveChanges();
                return Json(new object[] { true, "Package #" + bundle.BundleCode + " deleted!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }


        public async Task<JsonResult> ShippingSave(Order_Carrier order_carrier_data)
        {
            try
            {

                if ((access.Any(k => k.Key.Equals("deployment_dashboard")) != true || access["deployment_dashboard"] != true))
                {
                    throw new Exception("You don't have permission!");
                }

                var db = new WebDataModel();

                long IdSet = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                long OrderId;
                long.TryParse(Request["update_order_id"], out OrderId);
                var Order = db.O_Orders.Find(OrderId);
                if (Order == null)
                {
                    throw new Exception("Order not found!");
                }
                bool completed_check = Request["completed_save"] == "true";

                if (Order.BundelStatus == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString() && (access.Any(k => k.Key.Equals("deployment_director")) != true || access["deployment_director"] != true))
                {
                    throw new Exception("You don't have permission!");
                }

                //Save device package
                if (Order.BundelStatus != UserContent.DEPLOYMENT_PACKAGE_STATUS.Preparation.ToString())
                {
                    Exception e = null;
                    foreach (var p in db.Order_Products.Where(op => op.OrderCode == Order.OrdersCode).AsQueryable())
                    {
                        var cusnumbers = Request["cus_numbers_" + p.Id];
                        var invnumbers = Request["inv_numbers_" + p.Id];
                        var typeOption = Request["option_type_" + p.Id];
                        if (typeOption.Equals("ser"))
                        {
                            var sernumbers = Request["ser_numbers_" + p.Id];
                            List<string> dataTemp = new List<string>();
                            foreach (var ser in sernumbers.Split(','))
                            {
                                var dev = db.O_Device.FirstOrDefault(c => c.SerialNumber.Equals(ser));
                                dataTemp.Add(dev.InvNumber);
                            }
                            invnumbers = string.Join(",", dataTemp);
                        }
                        else if (typeOption.Equals("cus"))
                        {
                            invnumbers = string.Empty;
                        }

                        var ProductModel = db.O_Product_Model.Where(x => x.ModelCode == p.ModelCode).FirstOrDefault();
                        if (ProductModel != null)
                        {
                            if (ProductModel.DeviceRequired == true)
                            {
                                if (completed_check && (string.IsNullOrEmpty(invnumbers) || (invnumbers ?? "").Split(',').Count() != p.Quantity) && (string.IsNullOrEmpty(cusnumbers) && typeOption.Equals("cus")))
                                {
                                    completed_check = false;
                                    e = new Exception("Must select enought " + p.Quantity + " " + p.ProductName + " to set completed.");
                                }
                            }
                        }
                        List<string> old_invs = p.InvNumbers?.Split(',').ToList() ?? new List<string>();
                        List<string> old_cuss = p.CusNumbers?.Split(',').ToList() ?? new List<string>();
                        List<string> new_invs = invnumbers?.Split(',').ToList() ?? new List<string>();
                        using (var service = new DeploymentService())
                        {
                            old_invs.Where(o => !new_invs.Contains(o)).ToList().ForEach(o =>
                            {
                                service.unAssignedDevice(o);
                            });
                            if (!typeOption.Equals("cus"))
                            {
                                new_invs.Where(n => !old_invs.Contains(n)).ToList().ForEach(n =>
                                {
                                    service.AssignedDevice(n, Order.OrdersCode);
                                });
                            }
                            else
                            {
                                service.unAssignedCustomDevice(Order.OrdersCode, p.ModelCode);
                                service.AssignedCustomDevice(Order.OrdersCode, cusnumbers, p.ModelCode);
                            }
                        }

                    }
                    //save field
                    //var field_list = db.I_ProcessSetting.ToList();
                    //var old_fill = db.I_ProgressFill.Where(p => db.I_ProcessSetting.Any(f => f.FieldType == Order.BundelStatus && f.Id == p.FieldFillId)).ToList();
                    //db.I_ProgressFill.RemoveRange(old_fill);
                    //foreach (var f in field_list)
                    //{
                    //    string content = Request["fill_" + f.Id];
                    //    if (completed_check && f.Requirement == true && string.IsNullOrEmpty(content))
                    //    {
                    //        completed_check = false;
                    //        e = new Exception("Field " + f.FieldName + " is empty. You can't set completed!");
                    //    }
                    //    var newfield = new I_ProgressFill
                    //    {
                    //        Id = IdSet++,
                    //        OrderCode = Order.OrdersCode,
                    //        FieldFillId = f.Id,
                    //        Content = content
                    //    };
                    //    db.I_ProgressFill.Add(newfield);
                    //}

                    var order_carrier = db.Order_Carrier.Where(c => c.OrderCode == Order.OrdersCode).FirstOrDefault();
                    if (order_carrier == null)
                    {
                        order_carrier_data.Id = Guid.NewGuid().ToString();
                        order_carrier_data.OrderCode = Order.OrdersCode;
                        db.Order_Carrier.Add(order_carrier_data);
                        order_carrier = order_carrier_data;
                    }
                    else
                    {
                        order_carrier.CarrierName = order_carrier_data.CarrierName;
                        order_carrier.DateShipped = order_carrier_data.DateShipped;
                        order_carrier.TrackingNumber = order_carrier_data.TrackingNumber;
                        order_carrier.CarrierNote = order_carrier_data.CarrierNote;
                        db.Entry(order_carrier).State = System.Data.Entity.EntityState.Modified;
                    }
                    if (completed_check)
                    {
                        //var ups_completed = db.Order_UPSTracking.Any(u => u.OrderCode == Order.OrdersCode && u.Status == "Success" && u.Active == 1);
                        if (/*ups_completed || */(!string.IsNullOrWhiteSpace(order_carrier.CarrierName) && order_carrier.DateShipped != null && !string.IsNullOrWhiteSpace(order_carrier.TrackingNumber)))
                        {
                            Order.Status = InvoiceStatus.Closed.ToString();
                            Order.BundelStatus = UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString();

                        }
                        else
                        {
                            completed_check = false;
                            e = new Exception("Carrier does not have enough information to completed!");
                        }
                    }

                    Order.Note_Delivery = Request["Add_note_packaging"];
                    Order.ShippingAddress = Request["sh_street"] + "|" + Request["sh_city"] + "|" + Request["sh_state"] + "|" + Request["sh_zip"] + "|" + Request["sh_country"];
                    Order.UpdatedHistory = "|" + DateTime.UtcNow.ToString("dd/MMM/yyyy hh:mm tt") + " - By: " + cMem.FullName;
                    Order.UpdatedAt = DateTime.UtcNow;
                    Order.UpdatedBy = cMem.FullName;
                    var deployment = (long)AppLB.UserContent.TICKET_TYPE.Deployment;
                    var ticket = db.T_SupportTicket.Include(x=>x.T_TicketTypeMapping).Where(t => t.OrderCode == Order.OrdersCode && t.T_TicketTypeMapping.Any(y=>y.TypeId == deployment)).FirstOrDefault();
                    string Feedback = "";
                    if (Order.BundelStatus == UserContent.DEPLOYMENT_PACKAGE_STATUS.Complete.ToString())
                    {
                        Feedback = "Delivery status: <span class='text-success'>Completed</span>";
                    }
                    else
                    {
                        Feedback = "Delivery status: <span class='text-orange'>Pending</span>";
                    }
                    var fb = new T_TicketFeedback
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                        TicketId = ticket.Id,
                        Feedback = Feedback,
                        CreateByNumber = cMem.MemberNumber,
                        CreateByName = cMem.FullName,
                        CreateAt = DateTime.UtcNow,
                        FeedbackTitle = "The delivery device has been updated",
                        DateCode = DateTime.UtcNow.ToString("yyyyMMdd"),
                        GlobalStatus = "private"
                    };
                    ticket.UpdateAt = DateTime.UtcNow;
                    ticket.UpdateBy = cMem.FullName;
                    ticket.UpdateTicketHistory += DateTime.UtcNow.ToString("dd MMM yyyy hh:mm tt") + " - by " + cMem.FullName + "|";
                    db.T_TicketFeedback.Add(fb);

                    db.Entry(Order).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    if (e != null)
                    {
                        throw e;
                    }
                    //await TicketViewController.AutoTicketScenario.UpdateTicketFromSatellite(Order.OrdersCode, AppLB.UserContent.TICKET_TYPE.Deployment, cMem.FullName);

                    //complete
                    if (completed_check)
                    {
                        var cus = db.C_Customer.Where(c => c.CustomerCode == Order.CustomerCode).FirstOrDefault();
                        var OrderHardWareList = new List<object>();
                        List<object> hardwareList = new List<object>();
                        List<object> packageList = new List<object>();
                        object hardware = null;
                        var _listOrderProduct = db.Order_Products.Where(p => p.OrderCode == Order.OrdersCode).ToList();
                        var listOrderProduct = _listOrderProduct.Where(l => l.BundleId != null).GroupBy(l => l.BundleId).Select(g => new Order_Products
                        {
                            BundleId = g.Key,
                            Quantity = g.ToList()[0].BundleQTY ?? 0,
                            Price = g.Sum(l => l.Price * l.Quantity / l.BundleQTY),
                            TotalAmount = g.Sum(l => l.Price * l.Quantity),
                            ProductCode = g.Max(l => l.ProductCode),
                            ProductName = g.Max(l => l.ProductName),
                            ModelName = g.Max(l => l.ModelName),
                            Feature = g.Max(l => l.Feature),
                            DiscountPercent = g.Max(l => l.DiscountPercent),
                            Discount = g.Max(l => l.Discount),
                        }).ToList();
                        listOrderProduct.AddRange(_listOrderProduct.Where((l => l.BundleId == null)).ToList());
                        listOrderProduct.ForEach(_order =>
                        {
                            if (_order.BundleId > 0)
                            {
                                var Bundle_Device = db.I_Bundle_Device.Where(d => d.Bundle_Id == _order.BundleId).AsQueryable();
                                var models = db.O_Product_Model.Where(m => Bundle_Device.Select(d => d.ModelCode).Contains(m.ModelCode)).ToList();
                                var packageHardware = new List<object>();
                                models.ForEach(item =>
                                {
                                    packageHardware.Add(new
                                    {
                                        type = item.ModelName,
                                        color = item.Color,
                                        name = item.ProductName,
                                        quanlity = Bundle_Device.FirstOrDefault(d => d.ModelCode == item.ModelCode)?.Quantity * _order.Quantity,
                                        unit = DisplayDefault(_order.Price),
                                        amount = DisplayDefault(_order.TotalAmount)
                                    });
                                });
                                packageList.Add(new
                                {
                                    name = db.I_Bundle.Find(_order.BundleId)?.Name,
                                    count = models.Count + 1,
                                    quanlity = _order.Quantity ?? 1,
                                    unit = DisplayDefault(_order.Price),
                                    discount = _order.DiscountPercent > 0 ? "(discount " + _order.DiscountPercent + "%)" : (_order.Discount > 0 ? "(discount $" + _order.Discount + ")" : ""),
                                    amount = DisplayDefault(_order.Price * (_order.Quantity ?? 1) - _order.Discount),
                                    hardwareList = packageHardware
                                });
                            }
                            else
                            {
                                hardwareList.Add(new
                                {
                                    type = _order.ModelName,
                                    color = _order.Feature,
                                    name = _order.ProductName,
                                    quanlity = _order.Quantity ?? 1,
                                    unit = Display(_order.Price),
                                    discount = _order.DiscountPercent > 0 ? "(discount " + _order.DiscountPercent + "%)" : (_order.Discount > 0 ? "(discount $" + _order.Discount + ")" : ""),
                                    amount = DisplayDefault(_order.Price * (_order.Quantity ?? 1) - _order.Discount),
                                });
                            }
                        });
                        var orderDetail = db.Order_Subcription.Where(s => s.OrderCode == Order.OrdersCode && s.Actived == true)
                            .Select(s => new VmOrderService
                            {
                                Product_Code = s.Product_Code,
                                Period = s.Period,
                                ProductName = s.ProductName,
                                StartDate = s.StartDate,
                                EndDate = s.EndDate,
                                Price = s.Price,
                                Quantity = s.Quantity ?? 1,
                                Discount = s.Discount,
                                DiscountPercent = s.DiscountPercent,
                                Amount = s.Period == "MONTHLY" ? (s.Price ?? 0) - (s.Discount ?? 0) : ((s.Price ?? 0) * (s.Quantity ?? 1)) - (s.Discount ?? 0),
                            }).ToList();

                        var servicesList = new List<object>();
                        orderDetail.ForEach(_order =>
                        {
                            servicesList.Add(new
                            {
                                type = _order.Period ?? "One-time",
                                name = _order.ProductName,
                                date = (_order.StartDate?.ToString("dd MMM yyyy") ?? "") + (_order.EndDate?.ToString(" - dd MMM yyyy") ?? ""),
                                quanlity = _order.Quantity ?? 1,
                                unit = DisplayDefault(_order.Price),
                                discount = _order.DiscountPercent > 0 ? "(discount " + _order.DiscountPercent + "%)" : (_order.Discount > 0 ? "(discount $" + _order.Discount + ")" : ""),
                                amount = DisplayDefault(_order.Amount)
                            });
                        });
                        var discount = Order.DiscountAmount ?? 0;
                        if (Order.DiscountPercent > 0)
                        {
                            discount = Order.TotalHardware_Amount * Convert.ToDecimal(Order.DiscountPercent / 100) ?? 0;
                        }

                        var amount = Order.TotalHardware_Amount - discount;
                        var taxAmount = amount * Convert.ToDecimal(Order.TaxRate / 100);
                        hardware = new
                        {
                            order_code = $"#{Order.OrdersCode}",
                            order_date = Order.CreatedAt?.ToString("dd MMM yyyy"),

                            business_name = cus.BusinessName,
                            salon_code = $"#{cus.StoreCode}",
                            business_address = $"{cus.BusinessAddressStreet ?? ""}<br/>{cus.BusinessCity ?? ""},{cus.BusinessState ?? ""}, {cus.BusinessZipCode ?? ""} {cus.BusinessCountry ?? ""}",
                            contact_name = string.IsNullOrEmpty(cus.OwnerName) ? cus.ContactName?.ToUpper() : cus.OwnerName?.ToUpper(),
                            contact_mobile = string.IsNullOrEmpty(cus.OwnerMobile) ? cus.BusinessPhone : cus.OwnerMobile,
                            hardware = hardwareList,
                            package = packageList,
                            licenses = servicesList,
                            subtotal = DisplayDefault(Order.TotalHardware_Amount),
                            discount = Display(discount),
                            discount_rate = Display((decimal?)Order.DiscountPercent),
                            shippingFee = Display(Order.ShippingFee),
                            tax_rate = Display(Order.TaxRate),
                            tax = Display(taxAmount),
                            total = DisplayDefault(amount + taxAmount + Order.ShippingFee)
                        };
                        var finalServices = new List<object>();
                        if (hardware != null) finalServices.Add(hardware);
                        var emailData = new SendGridEmailTemplateData.EmailDeplomentTemplate()
                        {
                            SalonName = cus.BusinessName,
                            OrderNumber = Order.OrdersCode,
                            DateShipped = order_carrier_data.DateShipped.Value.ToString("MM/dd/yyyy"),
                            CarrierName = order_carrier_data.CarrierName,
                            TrackingNumber = order_carrier_data.TrackingNumber,
                            Note = Order.Note_Delivery,
                            CreatedOn = Order.CreatedAt.Value.ToString("MM/dd/yyyy"),
                            Phone = cus.BusinessPhone,
                            Address = Order.ShippingAddress,
                            LinkPayment = new InvoiceService().GetPaymentLink(Order.OrdersCode, cus.MD5PassWord),
                            services = finalServices
                        };
                        //send sms && email
                        var orderViewService = EngineContext.Current.Resolve<OrderViewService>();
                        await orderViewService.Send_SMS_Email_After_Deployment_Complete(cus, cMem.PersonalEmail, emailData);
                    }

                    return Json(new object[] { true, completed_check ? "Bundle delivery info completed!" : "Bundle delivery info saved!" });
                }
                else
                {
                    throw new Exception("This Package is Not ready!");
                }

            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Fail: " + e.Message });
            }

        }

        public JsonResult unassignedDevice(string Invs)
        {
            bool result = false;
            using (var service = new DeploymentService())
            {
                result = service.unAssignedDevice(Invs);
            }
            return Json(result);
        }


        #endregion

        #region INVENTORY PROCESS SETTING
        public ActionResult GetProcessSetting()
        {
            WebDataModel db = new WebDataModel();
            //Check access
            if (access.Any(k => k.Key.Equals("inventory_process_setting")) == false || access["inventory_process_setting"] != true)
            {
                ViewBag.err = "Access denied";
            }
            //.End

            var _process = db.I_ProcessSetting.OrderBy(p => p.FieldType).ToList();

            return PartialView("_PartialProcessSetting", _process.Count > 0 ? _process : new List<I_ProcessSetting>());
        }

        /// <summary>
        /// Add new process
        /// </summary>
        /// <param name="_Type">FieldType</param>
        /// <param name="_Name">FieldName</param>
        /// <param name="_Requirement">Requirement</param>
        /// <returns></returns>
        public JsonResult AddProcess(string _Type, string _Name, bool _Requirement, bool _ischeck)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                //Check access
                if (access.Any(k => k.Key.Equals("inventory_process_setting")) == false || access["inventory_process_setting"] != true)
                {
                    throw new Exception("Access denied");
                }
                //.End

                if (db.I_ProcessSetting.Any(p => p.FieldType == _Type && p.FieldName == _Name) == true)
                {
                    throw new Exception("This process already exists");
                }
                else
                {
                    var _new_process = new I_ProcessSetting()
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                        FieldName = _Name,
                        Requirement = _Requirement,
                        FieldType = _Type,
                        IsCheck = _ischeck
                    };
                    db.I_ProcessSetting.Add(_new_process);
                    db.SaveChanges();

                    return Json(new object[] { true, _new_process });
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Update process
        /// </summary>
        /// <param name="_Id">Process Id</param>
        /// <param name="_Type">FieldType</param>
        /// <param name="_Name">FieldName</param>
        /// <param name="_Requirement">Requirement</param>
        /// <returns></returns>
        public JsonResult SaveProcess(long? _Id, string _Type, string _Name, bool _Requirement, bool _ischeck)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                //Check access
                if (access.Any(k => k.Key.Equals("inventory_process_setting")) == false || access["inventory_process_setting"] != true)
                {
                    throw new Exception("Access denied");
                }
                //.End

                var _process = db.I_ProcessSetting.Find(_Id);
                if (_process != null)
                {
                    if (db.I_ProcessSetting.Any(p => p.FieldName == _Name) && _Name != _process.FieldName)
                    {
                        throw new Exception("Field Name already exists");
                    }
                    else
                    {
                        _process.FieldName = _Name;
                        _process.Requirement = _Requirement;
                        _process.FieldType = _Type;
                        _process.IsCheck = _ischeck;
                        db.Entry(_process).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        return Json(new object[] { true, _process });
                    }
                }
                else
                {
                    throw new Exception("This process does not exist");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Delete process
        /// </summary>
        /// <param name="_Id">Process Id</param>
        /// <returns></returns>
        public JsonResult DeleteProcess(long? _Id)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                //Check access
                if (access.Any(k => k.Key.Equals("inventory_process_setting")) == false || access["inventory_process_setting"] != true)
                {
                    throw new Exception("Access denied");
                }
                //.End

                var _process = db.I_ProcessSetting.Find(_Id);
                if (_process != null)
                {
                    db.I_ProcessSetting.Remove(_process);
                    db.SaveChanges();
                    return Json(new object[] { true, "Delete success" });
                }
                else
                {
                    throw new Exception("This process does not exist");
                }
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }

        /// <summary>
        /// Search process for [FieldType] + [FieldName]
        /// </summary>
        /// <param name="Type">FieldType</param>
        /// <param name="SearchName">FieldName</param>
        /// <returns></returns>
        public JsonResult SearchProcess(string Type, string SearchName)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                //Check access
                if (access.Any(k => k.Key.Equals("inventory_process_setting")) == false || access["inventory_process_setting"] != true)
                {
                    throw new Exception("Access denied");
                }
                //.End

                SearchName = CommonFunc.ConvertNonUnicodeURL(SearchName)?.ToLower();

                var _list_process = (from p in db.I_ProcessSetting
                                     where (Type == "" || p.FieldType == Type)
                                     && (SearchName == "" || p.FieldName.ToLower().Contains(SearchName))
                                     orderby p.FieldType
                                     select p).ToList();

                return Json(new object[] { true, _list_process });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, "Error: " + ex.Message });
            }
        }
        #endregion

        #region Import Excel - Disabled

        //public ActionResult LoadExcel()
        //{
        //    try
        //    {
        //        WebDataModel db = new WebDataModel();
        //        HttpPostedFileBase file = Request.Files[0];
        //        string filesave = DateTime.UtcNow.Ticks.ToString() + file.FileName;
        //        file.SaveAs(Path.Combine(Server.MapPath("/upload/documents/"), filesave));
        //        Application xlApp = new Application();
        //        Workbook xlWorkbook = xlApp.Workbooks.Open(Path.Combine(Server.MapPath("/upload/documents/"), filesave));
        //        _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
        //        Range xlRange = xlWorksheet.UsedRange;

        //        int lastUsedRow = xlWorksheet.Cells.Find("*", System.Reflection.Missing.Value,
        //                           System.Reflection.Missing.Value, System.Reflection.Missing.Value,
        //                           XlSearchOrder.xlByRows, XlSearchDirection.xlPrevious,
        //                           false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

        //        // Find the last real column
        //        int lastUsedColumn = xlWorksheet.Cells.Find("*", System.Reflection.Missing.Value,
        //                                       System.Reflection.Missing.Value, System.Reflection.Missing.Value,
        //                                       XlSearchOrder.xlByColumns, XlSearchDirection.xlPrevious,
        //                                       false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;

        //        List<List<string>> List_devices = new List<List<string>>();
        //        for (int i = 2; i <= lastUsedRow; i++)
        //        {
        //            List<string> device = new List<string>();
        //            for (int j = 1; j <= lastUsedColumn; j++)
        //            {
        //                device.Add(Convert.ToString(xlRange.Cells[i, j].Value2));
        //            }
        //            List_devices.Add(device);
        //        }
        //        xlWorkbook.Close();

        //        //check
        //        List<string> Results = new List<string>();
        //        foreach (var info in List_devices)
        //        {

        //            string result = "";
        //            //line
        //            result += (db.O_Product_Line.Any(delegate (O_Product_Line p)
        //            { return CommonFunc.compareName(p.Name, info[0]); }) ? "" : "Product line  <b>" + info[0] + " </b> does not exist. ");

        //            //name
        //            result += (db.O_Product.Any(delegate (O_Product p)
        //            { return CommonFunc.compareName(p.Name, info[1]); }) ? "" : "Product name  <b>" + info[1] + " </b> does not exist. ");

        //            //SerialNumber
        //            if (!long.TryParse(info[2], out long SerialNumber))
        //                result += ("Serial Number  <b>" + info[2] + " </b> is malformed. ");
        //            else if (db.O_Device.Any(delegate (O_Device o) { return o.InvNumber == info[2]; }))
        //                result += ("Serial Number  <b>" + info[2] + " </b> already exist. ");

        //            //Vendor name
        //            if (!string.IsNullOrEmpty(info[3]))
        //                result += (db.Vendors.Any(delegate (Vendor p)
        //                {
        //                    return CommonFunc.compareName(p.CompanyName, info[3]);
        //                })
        //                ? "" : "Vendor <b>" + info[3] + "</b> does not exist. ");
        //            Results.Add(result);
        //        }
        //        for (int i = 0; i < List_devices.Count() - 1; i++)
        //        {
        //            for (int j = i + 1; j < List_devices.Count(); j++)
        //                if (List_devices[i][2] == List_devices[j][2])
        //                {
        //                    Results[i] += "Serial number is identical to #" + (j + 1) + ". ";
        //                    Results[j] += "Serial number is identical to #" + (i + 1) + ". ";
        //                }

        //        }


        //        //Not ok
        //        foreach (var result in Results)
        //        {
        //            if (result != "")
        //            {
        //                ViewBag.Results = Results;
        //                return View(List_devices);
        //            }

        //        }

        //        //All ok => Add new
        //        var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
        //        foreach (var info in List_devices)
        //        {
        //            var product = db.O_Product.Where(delegate (O_Product p)
        //            { return CommonFunc.compareName(p.Name, info[1]); }).FirstOrDefault();
        //            var vendor = db.Vendors.Where(delegate (Vendor p)
        //            { return CommonFunc.compareName(p.CompanyName, info[3]); }).FirstOrDefault() ?? new Vendor();
        //            var l_features = info[5].Split(',').Select(x => x = x.Trim()).OrderBy(x => x).ToList();
        //            var features = string.Join(",", l_features);
        //            var device = new O_Device()
        //            {
        //                DeviceId = id++,
        //                InvNumber = info[2],
        //                ProductCode = product.Code,
        //                ProductName = product.Name,
        //                Active = true,
        //                CreateBy = db.P_Member.Where(m => m.PersonalEmail.Equals(User.Identity.Name)).FirstOrDefault()?.FullName,
        //                CreateAt = DateTime.Now,

        //            };
        //            db.O_Device.Add(device);
        //            var New_feature = new List<Device_Features>();

        //            if (l_features.Count > 1)
        //            {
        //                foreach (var feature in l_features)
        //                {
        //                    if (!db.Device_Features.Any(f => f.Feature == feature))
        //                    {
        //                        New_feature.Add(new Device_Features { Feature = feature });
        //                    }
        //                }
        //            }
        //            if (!db.Device_Features.Any(f => f.Feature == features))
        //            {
        //                New_feature.Add(new Device_Features { Feature = features });
        //            }
        //            db.Device_Features.AddRange(New_feature);
        //        }

        //        //add history import
        //        //var history = new ImportExcelHistory
        //        //{
        //        //    Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
        //        //    Name = db.P_Member.Where(m => m.PersonalEmail.Equals(User.Identity.Name)).FirstOrDefault()?.FullName,
        //        //    ImportAt = DateTime.Now,
        //        //    ImportFile = "/upload/documents/" + filesave
        //        //};
        //        //db.ImportExcelHistories.Add(history);
        //        db.SaveChanges();
        //        TempData["success"] = List_devices.Count + " Devices added!";
        //        return RedirectToAction("");
        //    }
        //    catch (Exception ex)
        //    {
        //        TempData["error"] = "Import failure! " + ex.Message;
        //        return RedirectToAction("");
        //    }
        //}

        #endregion

        #region Import Serial Number
        public JsonResult ImportSerialNumber()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                HttpPostedFileBase file = Request.Files["excelfile"];
                if (file == null)
                {
                    throw new Exception("File is not found");
                }
                string filesave = DateTime.UtcNow.Ticks.ToString() + file.FileName;
                file.SaveAs(Path.Combine(Server.MapPath("/upload/documents/"), filesave));
                Application xlApp = new Application();
                Workbook xlWorkbook = xlApp.Workbooks.Open(Path.Combine(Server.MapPath("/upload/documents/"), filesave));
                _Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Range xlRange = xlWorksheet.UsedRange;

                int lastUsedRow = xlWorksheet.Cells.Find("*", System.Reflection.Missing.Value,
                                   System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                   XlSearchOrder.xlByRows, XlSearchDirection.xlPrevious,
                                   false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

                // Find the last real column
                int lastUsedColumn = xlWorksheet.Cells.Find("*", System.Reflection.Missing.Value,
                                               System.Reflection.Missing.Value, System.Reflection.Missing.Value,
                                               XlSearchOrder.xlByColumns, XlSearchDirection.xlPrevious,
                                               false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Column;

                List<ExcelSerialNumber> exceldata = new List<ExcelSerialNumber>();
                for (int i = 2; i <= lastUsedRow; i++)
                {
                    var item = new ExcelSerialNumber
                    {
                        Inv = CommonFunc.ConvertNonUnicodeURL(Convert.ToString(xlRange.Cells[i, 1].Value2)).Replace("-", ""),
                        Serial = CommonFunc.ConvertNonUnicodeURL(Convert.ToString(xlRange.Cells[i, 2].Value2)).Replace("-", ""),
                        Comment = Convert.ToString(xlRange.Cells[i, 3].Value2)
                    };
                    exceldata.Add(item);
                }
                xlWorkbook.Close();

                //check
                List<string> Results = new List<string>();
                foreach (var info in exceldata)
                {
                    var device = db.O_Device.Where(d => d.InvNumber == info.Inv).FirstOrDefault();
                    if (device != null && !string.IsNullOrEmpty(info.Serial))
                    {
                        device.SerialNumber = info.Serial;
                        device.Description = info.Comment;
                        db.Entry(device).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                db.SaveChanges();
                return Json(new object[] { true, "Import serial number completed!" });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        #endregion

        #region location
        public JsonResult getLocation(string code)
        {
            if ((access.Any(k => k.Key.Equals("inventory_manage")) == false || access["inventory_manage"] != true))
            {
                return Json(new object[] { false, "Forbiden, not permission!" });
            }
            var db = new WebDataModel();
            var location = db.Locations.Find(code);
            if (location != null)
                return Json(new object[] { true, location });
            else
                return Json(new object[] { false, "Location not found!" });
        }
        public JsonResult SaveLocation(string Name, string Code, string Street, string City, string State, string Zipcode, string Country)
        {

            try
            {
                if ((access.Any(k => k.Key.Equals("inventory_manage")) == false || access["inventory_manage"] != true))
                {
                    throw new Exception("Forbiden, not permission!");
                }
                var db = new WebDataModel();
                if (string.IsNullOrEmpty(Code))
                {
                    if (db.Locations.Any(l => l.Name.ToLower() == Name.ToLower()))
                    {
                        throw new Exception("Location name already exist!");
                    }
                    string cur_id = db.Locations.Max(l => l.Id);
                    var location = new Location
                    {
                        Id = CommonFunc.RenderCodeId(cur_id, "L"),
                        Name = Name,
                        Address = Street + "|" + City + "|" + State + "|" + Zipcode + "|" + Country
                    };
                    db.Locations.Add(location);
                    db.SaveChanges();
                    return Json(new object[] { true, "New location created", location });
                }
                else
                {
                    var location = db.Locations.Find(Code);
                    if (location == null)
                    {
                        throw new Exception("Location not found!");
                    }
                    if (db.Locations.Any(l => l.Name.ToLower() == Name.ToLower() && l.Id != Code))
                    {
                        throw new Exception("Location name already exist!");
                    }
                    location.Name = Name;
                    location.Address = Street + "|" + City + "|" + State + "|" + Zipcode + "|" + Country;
                    db.Entry(location).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Json(new object[] { true, "Location " + Code + " Updated", location });
                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        #endregion

        #region ajax json
        public JsonResult getDevice(long id)
        {
            try
            {
                var db = new WebDataModel();
                var device = db.O_Device.Find(id);
                if (device == null)
                {
                    throw new Exception("Device not found");
                }
                var product = db.O_Product.Find(device.ProductCode);
                var line = db.O_Product_Line.Find(product.ProductLineCode);
                var m = db.O_Product_Model.Find(device.ModelCode);
                var assigned = db.Order_Products.Where(c => c.InvNumbers.IndexOf(device.InvNumber) >= 0).Join(db.O_Orders, p => p.OrderCode, o => o.OrdersCode, (p, o) => new
                {
                    p.OrderCode,
                    p.CreateAt,
                    o.CustomerName
                }).FirstOrDefault();
                var a = db.Order_Products.Where(c => c.InvNumbers.IndexOf(device.InvNumber) >= 0).ToList();
                string linkTicket = string.Empty;
                string TicketNumber = string.Empty;
                if (assigned != null)
                {
                    var ticket = db.T_SupportTicket.FirstOrDefault(s => s.OrderCode.Equals(assigned.OrderCode) && s.GroupName.Equals("Deployment"));
                    if (ticket != null)
                    {
                        linkTicket = Request.Url.GetLeftPart(UriPartial.Authority) + "/ticket/detail/" + ticket.Id.ToString();
                        TicketNumber = ticket.Id.ToString();
                    }
                }


                string location;
                if (device.Junkyard == true)
                {
                    location = "Junkyard";
                }
                else if (device.Inventory == 0)
                {
                    location = "Assigned";
                }
                else
                {
                    location = "Warehouse";
                }

                return Json(new object[] { true, device, m, location, line.Name, device.DeviceId.ToString(), assigned, linkTicket, TicketNumber });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }



        public JsonResult GetProductType()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var ListProType = db.O_Product.ToList();
                return Json(new object[] { true, ListProType });
            }
            catch (Exception)
            {
                return Json(new object[] { false });
            }
        }
        [HttpPost]
        public JsonResult getProductInfo(string Code)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var Product = db.O_Product.Find(Code);
                return Json(new object[] { true, Product });
            }
            catch (Exception)
            {
                return Json(new object[] { false });
            }
        }
        public JsonResult GetProductLine()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var ListProductLine = db.O_Product_Line.ToList();

                return Json(new object[] { true, ListProductLine });
            }
            catch (Exception)
            {
                return Json(new object[] { false });
            }
        }
        public JsonResult SaveProductLine(string ocode, string name)
        {
            WebDataModel db = new WebDataModel();
            try
            {
                string code = CommonFunc.ConvertNonUnicodeURL(name).Replace("-", "_").ToLower();
                if (db.O_Product_Line.Any(p => p.Code == code && p.Code != ocode))
                {
                    throw new Exception("Cannot use this name!");
                }
                if (string.IsNullOrEmpty(ocode))
                {
                    //Add new 
                    O_Product_Line np = new O_Product_Line
                    {
                        Code = code,
                        Name = name
                    };
                    db.O_Product_Line.Add(np);
                    db.SaveChanges();
                    return Json(new object[] { true, "Add new product line successfully!", np.Name, np.Code });
                }
                else
                {
                    //edit
                    var oldpl = db.O_Product_Line.Find(ocode);
                    if (oldpl == null)
                    {
                        throw new Exception("Product line not exist!");
                    }
                    else
                    {

                        O_Product_Line np = new O_Product_Line
                        {
                            Code = code,
                            Name = name
                        };

                        var list_Product = db.O_Product.Where(m => m.ProductLineCode == oldpl.Code).ToList();
                        foreach (var product in list_Product)
                        {
                            product.ProductLineCode = np.Code;
                            db.Entry(product).State = System.Data.Entity.EntityState.Modified;
                        }
                        db.O_Product_Line.Remove(oldpl);
                        db.O_Product_Line.Add(np);
                        db.SaveChanges();
                        return Json(new object[] { true, "Edit product line successfully!", np.Name, np.Code });
                    }

                }
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });
            }

        }
        public JsonResult GetDescProduct(long? Id)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var re = db.O_Device.Find(Id).JunkyardDescription;
                return Json(new object[] { true, re });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e });
            }
        }
        public JsonResult GetSalonAddress(long? order_id, string cus)
        {
            var db = new WebDataModel();
            var order = db.O_Orders.Find(order_id);
            var salon = new C_Customer();
            if (order != null)
            {
                salon = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();
            }
            else
            {
                salon = db.C_Customer.Where(c => c.CustomerCode == cus).FirstOrDefault();
            }
            if (salon != null)
            {
                return Json(new object[] { true, salon.BusinessAddressStreet, salon.BusinessCity, salon.BusinessState, salon.BusinessZipCode, salon.BusinessCountry });
            }
            else
            {
                return Json(new object[] { false, "Salon address not found!" });
            }
        }
        [HttpPost]
        public JsonResult UpdateCommentDevice(long DeviceId, string Comment)
        {
            try
            {
                var db = new WebDataModel();
                var device = db.O_Device.FirstOrDefault(x => x.DeviceId == DeviceId);
                device.Comment = Comment.Trim();
                db.SaveChanges();
                return Json(new { status = true, message = "update comment success" });
            }
            catch (Exception e)
            {
                return Json(new { status = false, message = e.Message });
            }

        }
        //public JsonResult checkvalue_importdevices(string list_value)
        //{
        //    list_value = list_value.Replace("\n", "").Replace(" ", "");
        //    var list_device = list_value.Split('*');

        //    //list_data[row][col]
        //    WebDataModel db = new WebDataModel();
        //    List<string> Results = new List<string>();
        //    foreach (var device in list_device)
        //    {
        //        string result = "";
        //        var info = device.Split('|');

        //        //line
        //        result += (db.O_Product_Line.Any(delegate (O_Product_Line p)
        //        { return CommonFunc.compareName(p.Name, info[0]); }) ? "" : "\nProduct line " + info[0] + " không tồn tại. ");

        //        //name
        //        result += (db.O_Product.Any(delegate (O_Product p)
        //          { return CommonFunc.compareName(p.Name, info[1]); }) ? "" : "\nProduct name " + info[1] + " không tồn tại. ");

        //        //SerialNumber
        //        if (!long.TryParse(info[2], out long SerialNumber))
        //            result += ("\nSerial Number " + info[2] + " không đúng định dạng. ");
        //        else if (db.O_Device.Any(delegate (O_Device o) { return o.SerialNumber == info[2]; }))
        //            result += ("\nSerial Number " + info[2] + " đã tồn tại. ");

        //        //Vendor name
        //        result += (db.Vendors.Any(delegate (Vendor p)
        //         {
        //             return CommonFunc.compareName(p.CompanyName, info[3]);
        //         })
        //        ? "" : "\nKhông tìm thấy Vendor " + info[3] + ". ");
        //        Results.Add(result);
        //    }
        //    for (int i = 0; i < list_device.Count() - 1; i++)
        //    {
        //        for (int j = i + 1; j < list_device.Count(); j++)
        //            if (list_device[i].Split('|')[2] == list_device[j].Split('|')[2])
        //            {
        //                Results[i] += "\nSerial number trùng với #" + (j + 1) + ". ";
        //                Results[j] += "\nSerial number trùng với #" + (i + 1) + ". ";
        //            }

        //    }
        //    foreach (var result in Results)
        //    {
        //        if (result != "")
        //        {
        //            return Json(new object[] { false, Results });
        //        }

        //    }

        //    //Add new
        //    var id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssff"));
        //    foreach (var d in list_device)
        //    {
        //        var info = d.Split('|');
        //        var product = db.O_Product.Where(delegate (O_Product p)
        //        { return CommonFunc.compareName(p.Name, info[1]); }).FirstOrDefault();
        //        var device = new O_Device()
        //        {
        //            DeviceId = id++,
        //            SerialNumber = info[2],
        //            ProductCode = product.Code,
        //            ProductName = product.Name,
        //            A_InvNumber = info[4],
        //            Active = true,
        //            CreateBy = db.P_Member.Where(m => m.PersonalEmail.Equals(User.Identity.Name)).FirstOrDefault()?.FullName,
        //            CreateAt = DateTime.Now,
        //        };
        //        db.O_Device.Add(device);
        //    }
        //    db.SaveChanges();
        //    return Json(new object[] { true, "Đã thêm mới " + list_device.Length + " thiết bị!" });
        //}
        #endregion

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public async Task<FileStreamResult> ExportExcel(string s, string l)
        {
            var db = new WebDataModel();
            var list = (from d in db.O_Device.AsEnumerable()
                        where (CommonFunc.SearchName(d.ProductName, s, true)
                                || CommonFunc.SearchName(d.VendorName, s, true)
                                || CommonFunc.SearchName(d.ModelCode, s, true)
                                || CommonFunc.SearchName(d.SerialNumber, s, true)
                                || CommonFunc.SearchName(d.InvNumber, s, true)
                        )
                           && (string.IsNullOrEmpty(l) || d.LocationId == l)
                           && (d.Inventory == 1)
                        join m in db.O_Product_Model on d.ModelCode equals m.ModelCode

                        join p in db.O_Product on d.ProductCode equals p.Code
                        select new
                        {
                            Product = p,
                            Model = m,
                            Device = d
                        }).ToList();
            try
            {
                string location = !string.IsNullOrEmpty(s) ? db.Locations.Find(l)?.Name : "All locations";

                var webinfo = db.SystemConfigurations.FirstOrDefault();
                string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });

                string webRootPath = "/upload/other/";
                string fileName = @"TempData.xlsx";
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {

                    IWorkbook workbook = new XSSFWorkbook();

                    NPOI.SS.UserModel.IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;

                    NPOI.SS.UserModel.IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Underline = FontUnderlineType.Double;
                    font1.FontHeightInPoints = 13;

                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);

                    ISheet excelSheet = workbook.CreateSheet("Inventory Report");
                    excelSheet.SetColumnWidth(1, 15 * 256);
                    excelSheet.SetColumnWidth(2, 15 * 256);
                    excelSheet.SetColumnWidth(3, 20 * 256);
                    excelSheet.SetColumnWidth(4, 10 * 256);
                    excelSheet.SetColumnWidth(5, 20 * 256);
                    excelSheet.SetColumnWidth(6, 15 * 256);
                    excelSheet.SetColumnWidth(7, 20 * 256);
                    excelSheet.SetColumnWidth(8, 20 * 256);

                    IRow row1 = excelSheet.CreateRow(0);
                    row1.CreateCell(0).SetCellValue(webinfo?.CompanyName);


                    IRow row2 = excelSheet.CreateRow(1);
                    row2.CreateCell(0).SetCellValue(address.Length > 0 ? address[0] : "---");


                    IRow row3 = excelSheet.CreateRow(2);
                    row3.CreateCell(0).SetCellValue(address.Length > 0 ? address[1] + "," + address[2] + address[3] : "---,--- #####");

                    IRow row4 = excelSheet.CreateRow(3);
                    row4.CreateCell(0).SetCellValue("www.enrichcous.com");




                    ICell cell = row1.CreateCell(5);
                    cell.SetCellValue(new XSSFRichTextString("INVENTORY REPORT"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 5, 6));
                    cell.CellStyle = style;

                    row2.CreateCell(5).SetCellValue("Date: " + DateTime.Now.ToString("MM dd,yyyy hh:mm tt"));
                    excelSheet.AddMergedRegion(new CellRangeAddress(1, 1, 5, 6));
                    row3.CreateCell(5).SetCellValue("Location: " + location);
                    excelSheet.AddMergedRegion(new CellRangeAddress(2, 2, 5, 6));
                    row4.CreateCell(5).SetCellValue("Filter: " + s);
                    excelSheet.AddMergedRegion(new CellRangeAddress(3, 3, 5, 6));
                    //excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));

                    excelSheet.CreateFreezePane(0, 7, 0, 7);

                    int row_num = 6;
                    IRow row_next = excelSheet.CreateRow(row_num);
                    ICell c0 = row_next.CreateCell(0); c0.SetCellValue("#"); c0.CellStyle = style1;
                    ICell c1 = row_next.CreateCell(1); c1.SetCellValue("Inv #"); c1.CellStyle = style1;
                    ICell c2 = row_next.CreateCell(2); c2.SetCellValue("Model #"); c2.CellStyle = style1;
                    ICell c3 = row_next.CreateCell(3); c3.SetCellValue("Product Name"); c3.CellStyle = style1;
                    ICell c4 = row_next.CreateCell(4); c4.SetCellValue("Color"); c4.CellStyle = style1;
                    ICell c5 = row_next.CreateCell(5); c5.SetCellValue("Vendor"); c5.CellStyle = style1;
                    ICell c6 = row_next.CreateCell(6); c6.SetCellValue("Junkyard"); c6.CellStyle = style1;
                    ICell c7 = row_next.CreateCell(7); c7.SetCellValue("Junkyard Desc"); c7.CellStyle = style1;
                    ICell c8 = row_next.CreateCell(8); c8.SetCellValue("Location"); c8.CellStyle = style1;

                    int start_row = row_num;
                    foreach (var device in list)
                    {
                        row_num++;
                        IRow row_next_1 = excelSheet.CreateRow(row_num);
                        row_next_1.CreateCell(0).SetCellValue(row_num - start_row);
                        row_next_1.CreateCell(1).SetCellValue(device.Device.InvNumber);
                        row_next_1.CreateCell(2).SetCellValue(device.Device.ModelName);
                        row_next_1.CreateCell(3).SetCellValue(device.Device.ProductName);
                        row_next_1.CreateCell(4).SetCellValue(device.Model.Color);
                        //row_next_1.CreateCell(5).SetCellValue(device.Model.VendorName);
                        row_next_1.CreateCell(6).SetCellValue(device.Device.Junkyard == true ? "In Junkyard" : "");
                        row_next_1.CreateCell(7).SetCellValue(device.Device.Junkyard == true ? CommonFunc.HtmlToPlainText(device.Device.JunkyardDescription).Replace("\n", "") : "");
                        row_next_1.CreateCell(8).SetCellValue(device.Device.LocationName);
                    }

                    workbook.Write(fs);
                }

                using (var fileStream = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;
                string _fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_InventoryReport_" + AppLB.CommonFunc.ConvertNonUnicodeURL(location).Replace("-", "_") + "_" + AppLB.CommonFunc.ConvertNonUnicodeURL(s).Replace("-", "_") + ".xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);

                #region interop excel  - disabled

                //Application excel = new Application();
                //excel.Visible = false;
                //excel.DisplayAlerts = false;
                //var workbook = excel.Workbooks.Add(Type.Missing);
                //var worksheet = (Worksheet)workbook.ActiveSheet;
                //worksheet.Name = "Inventory Report";

                //var webinfo = db.SystemConfigurations.FirstOrDefault();
                //string[] address = webinfo?.CompanyAddress?.Split(new char[] { '|' });

                ////left
                //worksheet.Cells[1, 1] = webinfo?.CompanyName;
                //worksheet.Cells[2, 1] = address.Length > 0 ? address[0] : "855 Metropolitan Pkwy";
                //worksheet.Cells[3, 1] = address.Length > 0 ? address[1] + "," + address[2] + address[3] : "Atlanta, GA 30310";
                //worksheet.Cells[4, 1] = "www.enrichcous.com";
                //worksheet.Cells[5, 1] = webinfo?.CompanyHotline;

                //worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 3]].Merge();
                //worksheet.Range[worksheet.Cells[2, 1], worksheet.Cells[2, 3]].Merge();
                //worksheet.Range[worksheet.Cells[3, 1], worksheet.Cells[3, 3]].Merge();
                //worksheet.Range[worksheet.Cells[4, 1], worksheet.Cells[4, 3]].Merge();
                //worksheet.Range[worksheet.Cells[5, 1], worksheet.Cells[5, 3]].Merge();
                //worksheet.Cells[1, 1].Font.Bold = true;
                ////rigth
                //worksheet.Cells[1, 7] = "INVENTORY REPORT";
                //worksheet.Cells[2, 7] = "Date";
                //worksheet.Cells[2, 8] = DateTime.Now.ToString("MM dd,yyyy hh:mm tt");
                //worksheet.Cells[3, 7] = "Location";
                //worksheet.Cells[3, 8] = location;
                //worksheet.Range[worksheet.Cells[1, 7], worksheet.Cells[1, 8]].Merge();
                //worksheet.Cells[1, 7].Font.Bold = true;
                //worksheet.Cells[2, 7].Font.Bold = true;
                //worksheet.Cells[3, 7].Font.Bold = true;
                //worksheet.get_Range("G1", "H3").Cells.HorizontalAlignment = XlHAlign.xlHAlignRight;

                //int row = 7;
                //if (!string.IsNullOrWhiteSpace(SearchText))
                //{
                //    worksheet.Cells[4, 1] = "Search";
                //    worksheet.Cells[4, 2] = SearchText;
                //    worksheet.Cells[4, 1].Font.Bold = true;
                //    row++;
                //}
                ////head table
                //worksheet.Cells[row, 1] = "Inv #";
                //worksheet.Cells[row, 2] = "Model #";
                //worksheet.Cells[row, 3] = "Product";
                //worksheet.Cells[row, 4] = "Color";
                //worksheet.Cells[row, 5] = "Vendor";
                //worksheet.Cells[row, 6] = "Junkyard";
                //worksheet.Cells[row, 7] = "Junkyard description";
                //worksheet.Cells[row, 8] = "Location";
                //var cellrange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 8]];
                //cellrange.Font.Bold = true;

                //worksheet.Rows[row].EntireColumn.rowheight = 20;
                //int start_row = row;
                //foreach (var device in list)
                //{
                //    row++;
                //    worksheet.Cells[row, 1] = device.Device.InvNumber;
                //    worksheet.Cells[row, 2] = device.Device.ModelCode;
                //    worksheet.Cells[row, 3] = device.Device.ProductName;
                //    worksheet.Cells[row, 4] = device.Model.Color;
                //    worksheet.Cells[row, 5] = device.Model.VendorName;
                //    worksheet.Cells[row, 6] = device.Device.Junkyard == true ? "in Junkyard" : "";
                //    worksheet.Cells[row, 7] = device.Device.Junkyard == true ? CommonFunc.HtmlToPlainText(device.Device.JunkyardDescription).Replace("\n", "") : "";
                //    worksheet.Cells[row, 8] = device.Device.LocationName;
                //}
                //cellrange = worksheet.Range[worksheet.Cells[start_row, 1], worksheet.Cells[row, 8]];
                //Borders b = cellrange.Borders;
                //b.LineStyle = XlLineStyle.xlContinuous;
                //b.Weight = 2d;
                //cellrange.EntireColumn.AutoFit();

                //cellrange = worksheet.Range[worksheet.Cells[start_row, 1], worksheet.Cells[row, 1]];
                //cellrange.Font.Bold = true;
                ////celLrangE = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, ExportToExcel().Columns.Count]];
                //string savename = "/upload/" + DateTime.UtcNow.ToString("yyyyMMdd") + "_InventoryDashboard_" + location.Replace(" ", "") + "_" + SearchText.Replace(" ", "") + ".xlsx";
                //worksheet.SaveAs(Server.MapPath(savename));
                //workbook.Close();
                //excel.Quit();
                //return Json(new object[] { true, "Export completed", savename });
                #endregion



            }
            catch (Exception)
            {
                throw;
                //return Json(new object[] { false, e.Message });
            }
        }
        public string Display(decimal? num)
        {
            if (num == null || num == 0)
            {
                return null;
            }
            return num?.ToString("#,##0.#0") ?? null;
        }
        public string DisplayDefault(decimal? num)
        {
            if (num == null || num == 0)
            {
                return "0";
            }
            return num?.ToString("#,##0.#0") ?? "0";
        }

    }



    internal class ExcelSerialNumber
    {
        public string Inv { get; set; }
        public string Serial { get; set; }
        public string Comment { get; set; }
    }

    internal class Product_count
    {
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
    }
}