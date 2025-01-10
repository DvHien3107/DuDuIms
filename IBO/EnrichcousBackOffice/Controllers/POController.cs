using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DataTables.AspNet.Core;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
//using Microsoft.Office.Interop.Excel;
using SelectPdf;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class POController : Controller
    {

        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        WebDataModel db = new WebDataModel();
        // GET: PO
        public ActionResult Index(string sstatus, string sproduct, string scolor, string smodel)
        {
            if (!(access.Any(k => k.Key.Equals("inventory_purchase_order_request")) == true && access["inventory_purchase_order_request"] == true))
            {
                return Redirect("/home/Forbidden");
            }
            //var M = db.PO_Request.OrderByDescending(po => po.Code).ToList();

            TempData["sstatus"] = sstatus ?? string.Empty;
            TempData["sproduct"] = sproduct ?? string.Empty;
            TempData["scolor"] = scolor ?? string.Empty;
            TempData["smodel"] = smodel ?? string.Empty;

            ViewBag.product_model = db.O_Product_Model.Where(pm => pm.Active != false).ToList();
            ViewBag.product = db.O_Product.ToList();
            ViewBag.vendor = db.Vendors.ToList();
            ViewBag.color = db.O_Product_Model.GroupBy(m => m.Color).Select(m => m.Key).ToList();
            ViewBag.cMemId = cMem.Id;
            ViewBag.manager = access.Any(k => k.Key.Equals("inventory_purchase_order_request")) == true && access["inventory_purchase_order_request"] == true;
            return View();
        }
        public ActionResult LoadItemRequest(IDataTablesRequest dataTablesRequest, string sstatus, string sproduct, string scolor, string smodel)
        {

            var fs = db.PO_Request.Where(po =>
                (string.IsNullOrEmpty(smodel) || po.ModelName.Contains(smodel)) &&
                (string.IsNullOrEmpty(sstatus) || po.Status == sstatus) &&
                (string.IsNullOrEmpty(scolor) || po.Color == scolor) &&
                (string.IsNullOrEmpty(sproduct) || po.ProductCode == sproduct)
            ).AsEnumerable();

            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "RequestCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Code);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Code);
                    }
                    break;
                case "CreateDate":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.CreatedAt);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.CreatedAt);
                    }
                    break;
                case "Model":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.ModelName);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.ModelName);
                    }
                    break;
                case "Color":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Color);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Color);
                    }
                    break;
                case "RequestQty":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.RequestQty);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.RequestQty);
                    }
                    break;
                case "Status":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.Status);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.Status);
                    }
                    break;
                default:
                    fs = fs.OrderByDescending(s => s.CreatedAt);
                    break;
            }

            var totalRecord = fs.Count();
            fs = fs.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var dataView = fs.ToList();
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }

        public ActionResult RequestSave(string request_code)
        {
            if (!(access.Any(k => k.Key.Equals("inventory_purchase_order_request")) == true && access["inventory_purchase_order_request"] == true))
            {
                return Redirect("/home/Forbidden");
            }
            try
            {
                var model_code = Request["select_model"];
                //var product = Request["select_product"];
                //var color = Request["request_color"];
                int qty = int.Parse(Request["request_qty"] ?? "0");
                var note = Request["request_note"];
                //string picture = "";
                //if (!string.IsNullOrEmpty(model_code))
                //{
                //    var model = db.O_Product_Model.Where(pm => pm.ModelCode == model_code).FirstOrDefault();
                //    if (model == null)
                //        throw new Exception("Model not found!");
                //    product = model.ProductCode;
                //    color = model.Color;
                //    picture = model.Picture;
                //}
                //else
                //{
                //    picture = db.O_Product_Model.Where(pm => pm.ProductCode == product && pm.Color == color).FirstOrDefault()?.Picture ?? "";
                //}
                var model = db.O_Product_Model.Find(model_code);
                if (model == null)
                {
                    throw new Exception("Model not found");
                }
                if (model.Active == false)
                {
                    throw new Exception("Model not active");
                }
                if (!string.IsNullOrEmpty(request_code))
                {
                    var request = db.PO_Request.Find(request_code);
                    if (!(access.Any(k => k.Key.Equals("inventory_purchase_order_manager")) == true && access["inventory_purchase_order_manager"] == true)
                        && request.CreatedbyId != cMem.Id)
                    {
                        return Redirect("/home/Forbidden");
                    }

                    request.Color = model.Color;
                    request.ModelCode = model_code;
                    request.ModelName = model.ModelName;
                    request.ModelPicture = model.Picture;
                    request.UpdatedAt = DateTime.UtcNow;
                    request.UpdatedBy = cMem.FullName;
                    request.Note = note;
                    request.ProductCode = model.ProductCode;
                    request.ProductName = model.ProductName;
                    request.RequestQty = qty;
                    db.SaveChanges();
                    return Json(new object[] { true, "The request edited!" });
                }
                else
                {
                    var request = new PO_Request
                    {
                        Code = AppLB.CommonFunc.RenderCodeId(db.PO_Request.Max(pr => pr.Code), "R"),
                        Color = model.Color,
                        ModelCode = model_code,
                        ModelName = model.ModelName,
                        ModelPicture = model.Picture,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = cMem.FullName,
                        CreatedbyId = cMem.Id,
                        Note = note,
                        ProductCode = model.ProductCode,
                        ProductName = model.ProductName,
                        RequestQty = qty,
                        Status = "Requested"
                    };
                    db.PO_Request.Add(request);

                }
                db.SaveChanges();

                //--Add po detail--
                var vendors = db.Vendors.ToList();

                db.SaveChanges();
                return Json(new object[] { true, "New request created!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        public JsonResult DeleteRequest(string code)
        {
            try
            {
                var request = db.PO_Request.Find(code);
                if (request == null)
                {
                    throw new Exception("Request not found!");
                }
                if (!(access.Any(k => k.Key.Equals("inventory_purchase_order_manager")) == true && access["inventory_purchase_order_manager"] == true)
                        && request.CreatedbyId != cMem.Id)
                {
                    return Json(new object[] { false, "You don't have permission delete this request" });
                }
                db.PO_Request.Remove(request);
                db.SaveChanges();
                return Json(new object[] { true, "The request removed" });
            }
            catch (Exception)
            {

                throw;
            }
        }

        public ActionResult CreatePO(List<string> createpo)
        {
            createpo = createpo ?? new List<string>();
            var request = db.PO_Request.Where(r => createpo.Contains(r.Code)).ToList();
            ViewBag.vendors = db.Vendors.ToList();
            ViewBag.product = db.O_Product.ToList();
            ViewBag.product_model = db.O_Product_Model.Where(pm => pm.Active != false).ToList();
            return View(request);
        }

        public ActionResult CreatePOSubmit(List<string> codes)
        {
            var list_vendor = db.Vendors.ToList();
            string po_max_id = db.POes.Max(p => p.POCode);
            long request_id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff"));
            var new_POs = new List<PO>();
            var PO_removed_detail = new List<string>();
            foreach (var code in codes) //each PO_request code
            {
                List<string> vendors = Request["r_" + code]?.Split(',').ToList();
                var request = db.PO_Request.Find(code);
                request.Status = "Submited";
                int qty = 0;

                foreach (var vendorid in db.Vendors.Select(v => v.Id).ToList())// each vendor
                {
                    var ven_qty = int.Parse(Request["qty_" + code + "_" + vendorid] ?? "0");
                    PO PO = db.POes.Where(po => po.VendorId == vendorid && po.Status == "Ready for purchase").FirstOrDefault() ?? new_POs.Where(po => po.VendorId == vendorid).FirstOrDefault();

                    if (ven_qty > 0 && vendors.Contains(vendorid.ToString()))
                    {
                        if (PO == null)
                        {
                            PO = new PO
                            {
                                POCode = AppLB.CommonFunc.RenderCodeId(po_max_id, "PO"),
                                VendorId = vendorid,
                                VendorName = list_vendor.Where(v => v.Id == vendorid).FirstOrDefault().CompanyName,
                                Status = "Ready for purchase",
                                UpdatedBy = cMem.FullName,
                                UpdatedAt = DateTime.UtcNow
                            };
                            new_POs.Add(PO);
                            po_max_id = PO.POCode;
                        }
                        else
                        {
                            PO.UpdatedAt = DateTime.UtcNow;
                            PO.UpdatedBy = cMem.FullName;
                        }


                        var modelcode = db.PO_Request.Find(code).ModelCode;
                        var model = db.O_Product_Model.Find(modelcode);

                        var po_detail = new PO_Detail
                        {
                            Id = request_id++,
                            RequestCode = code,
                            POCode = PO.POCode,
                            Qty = ven_qty,
                            Price = decimal.Parse(Request["price_" + code + "_" + vendorid] ?? "0"),
                            Note = Request["note_" + code + "_" + vendorid],
                            ModelCode = model.ModelCode,
                            ModelName = model.ModelName,
                            ModelPicture = request.ModelPicture
                        };
                        db.PO_Detail.Add(po_detail);

                        qty += ven_qty;
                    }

                }
                request.Qty = qty;
            }

            db.POes.AddRange(new_POs);
            db.SaveChanges();
            return RedirectToAction("POManager");
        }


        public ActionResult RequestUpdate(string request_code)
        {
            var request = db.PO_Request.Find(request_code);
            var vendors = db.Vendors.ToList();

            long id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff"));
            //delete old detail
            var old_detail = db.PO_Detail.Where(po => po.RequestCode == request_code).AsQueryable();
            db.PO_Detail.RemoveRange(old_detail);

            //add new detail
            string po_max_id = db.POes.Max(p => p.POCode).ToString();
            foreach (var vendor in vendors)
            {
                var po = db.POes.Where(p => p.VendorId == vendor.Id && p.Status != "Created").FirstOrDefault();
                if (po == null)
                {
                    po = new PO
                    {
                        POCode = AppLB.CommonFunc.RenderCodeId(po_max_id, "PO"),
                        Total = 0,
                        VendorId = vendor.Id,
                        VendorName = vendor.CompanyName
                    };

                    db.POes.Add(po);
                    po_max_id = po.POCode;
                }

                int qty = int.Parse(Request["qty_" + vendor.Id] ?? "0");

                if (qty > 0)
                {
                    var po_detail = new PO_Detail
                    {
                        Id = id++,
                        POCode = po.POCode,
                        RequestCode = request_code,
                        ModelCode = request.ModelCode,
                        ModelName = request.ModelName,
                        Price = decimal.Parse(Request["price_" + vendor.Id] ?? "0"),
                        Note = Request["note_" + vendor.Id],
                        Qty = qty,
                        Purchased = (Request["purchased_" + vendor.Id] == "1")
                    };
                    db.PO_Detail.Add(po_detail);
                }
            }
            request.Status = Request["status"];
            db.Entry(request).State = System.Data.Entity.EntityState.Modified;

            db.SaveChanges();
            return RedirectToAction("pomanager");
        }

        public ActionResult POManager()
        {
            if (!(access.Any(k => k.Key.Equals("inventory_purchase_order_manager")) == true && access["inventory_purchase_order_manager"] == true))
            {
                return Redirect("/home/Forbidden");
            }

            //TempData["sstatus"] = sstatus ?? string.Empty;
            //TempData["svendor"] = svendor ?? 0;
            //TempData["stext"] = stext ?? string.Empty;
            //var M = (from pd in db.PO_Detail
            //         join re in db.PO_Request on pd.RequestCode equals re.Code
            //         group new { pd, re } by pd.POCode into pdg
            //         join po in db.POes on pdg.Key equals po.POCode
            //         orderby po.POCode descending
            //         select new PO_manager_view { PO = po, Detail = pdg.Select(g => new PO_Detail_view { Detail = g.pd, Detail_request = g.re }).ToList() }).ToList();

            ViewBag.product_model = db.O_Product_Model.Where(pm => pm.Active != false).ToList();
            ViewBag.product = db.O_Product.ToList();
            ViewBag.vendor = db.Vendors.ToList();
            ViewBag.color = db.O_Product_Model.GroupBy(m => m.Color).Select(m => m.Key).ToList();
            ViewBag.vendors = db.Vendors.ToList();
            return View();
        }

        public ActionResult LoadPOManager(IDataTablesRequest dataTablesRequest, string sstatus, long? svendor, string stext)
        {
            var fs = (from pd in db.PO_Detail
                     join re in db.PO_Request on pd.RequestCode equals re.Code
                     group new { pd, re } by pd.POCode into pdg
                     join po in db.POes on pdg.Key equals po.POCode
                     where (string.IsNullOrEmpty(sstatus) || po.Status == sstatus) && (svendor == null || po.VendorId == svendor) && (string.IsNullOrEmpty(stext) || pdg.Any(i => i.re.ModelName.Contains(stext) || i.re.ModelCode.Contains(stext) || i.re.ProductName.Contains(stext) || i.pd.POCode.Contains(stext) || po.SaleOrderNumber.Contains(stext)))
                     orderby po.POCode descending
                     select new PO_manager_view { PO = po, Detail = pdg.Select(g => new PO_Detail_view { Detail = g.pd, Detail_request = g.re }).ToList() }).AsEnumerable();

            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSort.Name)
            {
                case "POCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.PO.POCode);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.PO.POCode);
                    }
                    break;
                case "VendorName":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.PO.VendorName);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.PO.VendorName);
                    }
                    break;
                case "LastUpdate":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.PO.UpdatedAt);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.PO.UpdatedAt);
                    }
                    break;
                case "SONumber":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.PO.SaleOrderNumber);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.PO.SaleOrderNumber);
                    }
                    break;
                case "Status":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.PO.Status);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.PO.Status);
                    }
                    break;
                default:
                    fs = fs.OrderByDescending(s => s.PO.POCode);
                    break;
            }

            var totalRecord = fs.Count();
            fs = fs.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var dataView = fs.ToList();
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }

        public ActionResult LoadDetailPOManager(string POCode)
        {
            var POs = (from pd in db.PO_Detail
                       join re in db.PO_Request on pd.RequestCode equals re.Code
                       group new { pd, re } by pd.POCode into pdg
                       join po in db.POes on pdg.Key equals po.POCode
                       where po.POCode == POCode
                       orderby po.POCode descending
                       select new PO_manager_view { PO = po, Detail = pdg.Select(g => new PO_Detail_view { Detail = g.pd, Detail_request = g.re }).ToList() }).ToList();
            return PartialView("_Detail_POManager", POs.FirstOrDefault());
        }

        public ActionResult PO_Manager_search(string status, long? vendor, string text)
        {
            if (!(access.Any(k => k.Key.Equals("inventory_purchase_order_manager")) == true && access["inventory_purchase_order_manager"] == true))
            {
                return Redirect("/home/Forbidden");
            }
            var M = (from pd in db.PO_Detail
                     join re in db.PO_Request on pd.RequestCode equals re.Code
                     group new { pd, re } by pd.POCode into pdg
                     join po in db.POes on pdg.Key equals po.POCode
                     where (string.IsNullOrEmpty(status) || po.Status == status) && (vendor == null || po.VendorId == vendor) && (string.IsNullOrEmpty(text) || pdg.Any(i => i.re.ModelName.Contains(text) || i.re.ProductName.Contains(text)))
                     orderby po.POCode descending
                     select new PO_manager_view { PO = po, Detail = pdg.Select(g => new PO_Detail_view { Detail = g.pd, Detail_request = g.re }).ToList() }).ToList();
            ViewBag.vendors = db.Vendors.ToList();
            return PartialView("_PO_Manager_load", M);
        }
        #region Edit PO/ PO_detail
        public ActionResult SaveEditPO(string code, long VendorId)
        {
            try
            {
                var po = db.POes.Find(code);
                var vendor = db.Vendors.Find(VendorId);
                if (po == null)
                    throw new Exception("PO detail not found!");
                if (po.Status == "Purchased")
                {
                    throw new Exception("Can't edit purchased PO");
                }
                po.VendorId = vendor.Id;
                po.VendorName = vendor.CompanyName;
                db.Entry(po).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Save completed!", vendor.CompanyName });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        public ActionResult DeletePO(string code)
        {
            try
            {
                var po = db.POes.Find(code);
                if (po == null)
                    throw new Exception("PO not found!");
                if (po.Status == "Purchased")
                {
                    throw new Exception("Can't delete purchased PO");
                }
                db.POes.Remove(po);
                db.SaveChanges();
                return Json(new object[] { true, "delete completed!" });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        public ActionResult SaveEditPODetail(long id, int Qty, decimal Price, string Note)
        {
            try
            {
                var PO_d = db.PO_Detail.Find(id);
                if (PO_d == null)
                    throw new Exception("PO detail not found!");
                if (db.POes.Find(PO_d.POCode).Status == "Purchased")
                {
                    throw new Exception("Can't edit purchased PO");
                }
                PO_d.Qty = Qty;
                PO_d.Price = Price;
                PO_d.Note = Note;
                db.Entry(PO_d).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Save completed!" });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        public ActionResult DeletePODetail(long id)
        {
            try
            {
                var PO_d = db.PO_Detail.Find(id);
                if (PO_d == null)
                    throw new Exception("PO detail not found!");
                if (db.POes.Find(PO_d.POCode).Status == "Purchased")
                {
                    throw new Exception("Can't delete Purchased PO");
                }
                db.PO_Detail.Remove(PO_d);
                db.SaveChanges();
                return Json(new object[] { true, "delete completed!" });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        #endregion
        public JsonResult Update_status(string po_code, string new_status, string son)
        {
            try
            {
                var po = db.POes.Find(po_code);
                if (new_status != "Submited" && new_status != "Purchased" && new_status != "Ready for purchase" && new_status != "Cancel")
                {
                    throw new Exception("status not match");
                }
                if (new_status == "Purchased")
                {
                    po.SaleOrderNumber = son;
                    foreach (var d in db.PO_Detail.Where(pd => pd.POCode == po.POCode).ToList())
                    {
                        var rq = db.PO_Request.Find(d.RequestCode);
                        var model = db.O_Product_Model.Where(m => m.ModelCode == d.ModelCode ||
                        (m.ProductCode == rq.ProductCode && m.Color == rq.Color)).FirstOrDefault();
                        if (model != null)
                        {
                            model.Status = "Enroute";
                            db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                        }
                    }
                }
                else
                {
                    po.SaleOrderNumber = "";
                }
                po.Status = new_status;
                po.UpdatedAt = DateTime.UtcNow;
                po.UpdatedBy = cMem.FullName;
                db.SaveChanges();
                return Json(new object[] { true, "Status updated" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }

        public ActionResult Pdf_html(string code)
        {
            var po = db.POes.Find(code);

            var detail = (from pd in db.PO_Detail
                          where pd.POCode == po.POCode
                          join r in db.PO_Request on pd.RequestCode equals r.Code
                          select new PO_Detail_view { Detail = pd, Detail_request = r }).ToList();
            var purchase_detail = new PurchaseOrder_view { PO = po, Detail = detail };
            return View(purchase_detail);
        }
        public JsonResult ConvertToPdf(string code)
        {
            try
            {
                //Flag: ["Estimates"/"Invoices"]
                HtmlToPdf converter = new HtmlToPdf();

                // set converter options
                converter.Options.PdfPageSize = PdfPageSize.A4;
                converter.Options.PdfPageOrientation = PdfPageOrientation.Portrait;
                converter.Options.MarginLeft = 50;
                converter.Options.MarginRight = 50;
                converter.Options.MarginTop = 50;
                converter.Options.MarginBottom = 50;
                // create a new pdf document converting an html string
                var ReqURL = Request.Url.Authority;
                PdfDocument doc = converter.ConvertUrl("http://" + ReqURL + "/po/Pdf_html?code=" + code);

                // save pdf document
                string strFileName = "Purcharse" + code + ".pdf";
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

        //Check in inventory 
        public ActionResult CheckInInventory(DateTime? fdate, DateTime? tdate, string svendor, string sstatus)
        {
            if (!(access.Any(k => k.Key.Equals("inventory_check_in")) == true && access["inventory_check_in"] == true))
            {
                return Redirect("/home/Forbidden");
            }
            var POs = (from pd in db.PO_Detail
                       join re in db.PO_Request on pd.RequestCode equals re.Code
                       group new { pd, re, } by pd.POCode into pdg
                       join po in db.POes on pdg.Key equals po.POCode
                       orderby po.POCode descending
                       where po.Status == "Purchased"
                       select new PO_manager_view { PO = po, Detail = pdg.Select(g => new PO_Detail_view { Detail = g.pd, Detail_request = g.re }).ToList() }).ToList();

            TempData["FDate"] = fdate?.ToString("MM/dd/yyyy") ?? new DateTime(2021, 1, 1).ToString("MM/dd/yyyy");
            TempData["TDate"] = tdate?.ToString("MM/dd/yyyy") ?? DateTime.UtcNow.ToString("MM/dd/yyyy");
            TempData["SStatus"] = sstatus ?? "";
            TempData["SVendor"] = svendor ?? "";
            ViewBag.p = access;
            ViewBag.Vendor = db.Vendors.ToList();
            return View();
        }

        public ActionResult LoadCheckinInventory(IDataTablesRequest dataTablesRequest, DateTime? fdate, DateTime? tdate, string vendor, string status,string searchtext)
        {
            var fs = db.POes.Where(c => c.Status == "Purchased").AsEnumerable();
            var colSort = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();

            fs = fs.Where(c => c.UpdatedAt > fdate);
            fs = fs.Where(c => c.UpdatedAt < tdate?.AddDays(1));
            if (!string.IsNullOrEmpty(searchtext))
            {
                fs = fs.Where(c => c.SaleOrderNumber.Contains(searchtext) || c.POCode.Contains(searchtext));
            }
            if (!string.IsNullOrEmpty(vendor))
            {
                fs = fs.Where(c => c.VendorId.ToString() == vendor);
            }
            if (!string.IsNullOrEmpty(status))
            {
                fs = fs.Where(c => (c.AllCheckedIn ?? false).ToString().ToLower() == status.ToLower());
            }

            switch (colSort.Name)
            {
                case "POCode":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.POCode);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.POCode);
                    }
                    break;
                case "Vendor":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.VendorName);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.VendorName);
                    }
                    break;
                case "SaleOrderNumber":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.SaleOrderNumber);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.SaleOrderNumber);
                    }
                    break;
                case "LastUpdate":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.UpdatedAt);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.UpdatedAt);
                    }
                    break;
                case "CheckinAll":
                    if (colSort.Sort.Direction == SortDirection.Ascending)
                    {
                        fs = fs.OrderBy(s => s.AllCheckedIn);
                    }
                    else
                    {
                        fs = fs.OrderByDescending(s => s.AllCheckedIn);
                    }
                    break;
                default:
                    fs = fs.OrderByDescending(s => s.UpdatedAt);
                    break;
            }

            var totalRecord = fs.Count();
            fs = fs.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length);
            var dataView = fs.ToList();
            return Json(new
            {
                recordsFiltered = totalRecord,
                recordsTotal = totalRecord,
                draw = dataTablesRequest.Draw,
                data = dataView
            });
        }
        public ActionResult LoadDetailCheckinInventory(string POCode)
        {
            var POs = (from pd in db.PO_Detail
                       join re in db.PO_Request on pd.RequestCode equals re.Code
                       where pd.POCode == POCode
                       group new { pd, re, } by pd.POCode into pdg
                       join po in db.POes on pdg.Key equals po.POCode
                       orderby po.POCode descending
                       where po.Status == "Purchased"
                       select new PO_manager_view { PO = po, Detail = pdg.Select(g => new PO_Detail_view { Detail = g.pd, Detail_request = g.re }).ToList() }).ToList();
            return PartialView("_Detail_InventoryCheckin", POs.FirstOrDefault());
        }

        public ActionResult CheckInInventory_search(long? vendor, bool? status)
        {
            if (!(access.Any(k => k.Key.Equals("inventory_check_in")) == true && access["inventory_check_in"] == true))
            {
                return Redirect("/home/Forbidden");
            }
            var POs = (from pd in db.PO_Detail
                       join re in db.PO_Request on pd.RequestCode equals re.Code
                       group new { pd, re } by pd.POCode into pdg
                       join po in db.POes on pdg.Key equals po.POCode
                       orderby po.POCode descending
                       where po.Status == "Purchased" && (vendor == null || po.VendorId == vendor) && (status == null || (po.AllCheckedIn ?? false) == status)
                       select new PO_manager_view { PO = po, Detail = pdg.Select(g => new PO_Detail_view { Detail = g.pd, Detail_request = g.re }).ToList() }).ToList();
            ViewBag.checkedin = db.PO_Detail_Checkin.ToList();
            ViewBag.vendor = db.Vendors.ToList();
            return PartialView("_Checkin_load", POs);
        }
        public JsonResult CheckinModal(long id)
        {
            try
            {
                var pod = db.PO_Detail.Find(id);
                if (pod == null)
                {
                    throw new Exception("PO detail not found!");
                }
                var model = db.O_Product_Model.Find(pod.ModelCode);
                if (model == null)
                {
                    throw new Exception("Model not found!");
                }
                var invs = new List<string>();
                var inv_n_max = db.O_Device.Max(d => d.InvNumber) ?? "0000000000";
                for (int i = 1; i <= (pod.RemainQty ?? pod.Qty); i++)
                {
                    var inv = AppLB.CommonFunc.RenderCodeId(inv_n_max, "");
                    invs.Add(inv);
                    inv_n_max = inv;
                }
                return Json(new object[] { true, model, (pod.RemainQty ?? pod.Qty), invs });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }


        }
        public JsonResult get_invs(int? n)
        {
            if (n == null)
            {
                return Json(new object[] { false });
            }
            var invs = new List<string>();
            var inv_n_max = db.O_Device.Max(d => d.InvNumber) ?? "0";
            for (int i = 1; i <= n; i++)
            {
                var inv = AppLB.CommonFunc.RenderCodeId(inv_n_max, "");
                invs.Add(inv);
                inv_n_max = inv;
            }
            return Json(new object[] { true, invs });
        }
        public JsonResult CheckinSubmit(long? detail_id, string list_inv_string)
        {
            try
            {
                var list_inv = new List<string>();
                if (!string.IsNullOrWhiteSpace(list_inv_string))
                {
                    list_inv = list_inv_string.Split(',').ToList();
                }
                var device_new_id = long.Parse(DateTime.UtcNow.ToString("yyyyMMddhhmmssfff"));
                var date = DateTime.UtcNow;
                var checkin_id = long.Parse(DateTime.UtcNow.ToString("yyyyMMddhhmmssfff"));
                if (detail_id == null)
                {
                    throw new Exception("Please select item to check in");
                }
                var locations = db.Locations.ToList();
                var detail = db.PO_Detail.Find(detail_id);
                var po = db.POes.Find(detail.POCode);
                var request = db.PO_Request.Find(detail.RequestCode);
                if (detail.CheckedIn == true)
                {
                    throw new Exception(request.ProductName + " (" + request.Color + ") already checked in!");
                }
                var model = db.O_Product_Model.Find(detail.ModelCode);
                model.Status = "RTG";
                List<string> inv_numbers = new List<string>();
                if (list_inv.Count == 0)
                {
                    throw new Exception("Check in qty is required");
                }

                var location_id = Request["location"];
                var location = db.Locations.Find(location_id);
                foreach (var inv in list_inv)
                {
                    var new_device = new O_Device
                    {
                        Active = true,
                        CreateAt = date,
                        CreateBy = cMem.FullName,
                        UpdateAt = date,
                        UpdateBy = cMem.FullName,
                        DeviceId = device_new_id++,
                        Inventory = 1,
                        InvNumber = inv,
                        ProductCode = request.ProductCode,
                        ProductName = request.ProductName,
                        ModelCode = model.ModelCode,
                        ModelName = model.ModelName,
                        LocationId = location.Id,
                        LocationName = location.Name,
                        VendorId = po.VendorId,
                        VendorName = po.VendorName,
                    };
                    db.O_Device.Add(new_device);
                    inv_numbers.Add(new_device.InvNumber);
                }



                detail.RemainQty = (detail.RemainQty ?? detail.Qty) - list_inv.Count;
                if (detail.RemainQty == 0)
                {
                    detail.CheckedIn = true;
                    detail.CheckedInDate = DateTime.UtcNow;
                }
                else if (detail.RemainQty < 0)
                {
                    throw new Exception("Check in devices amount can't more than Remain Qty");
                }
                var checkin = db.PO_Detail_Checkin.Where(c => c.PO_Detail_id == detail.Id && c.LocationId == location_id).FirstOrDefault();
                if (checkin == null)
                {
                    checkin = new PO_Detail_Checkin
                    {
                        Id = checkin_id++,
                        LocationId = location_id,
                        LocationName = location.Name,
                        POCode = detail.POCode,
                        Qty = list_inv.Count,
                        PO_Detail_id = detail.Id,
                        CreatedAt = date,
                        CreatedBy = cMem.FullName,
                        InvNumbers = string.Join(",", inv_numbers),
                    };
                    db.PO_Detail_Checkin.Add(checkin);
                }
                else
                {
                    checkin.Qty += list_inv.Count;
                    checkin.InvNumbers += "," + string.Join(",", inv_numbers);
                    db.Entry(checkin).State = System.Data.Entity.EntityState.Modified;

                }


                db.SaveChanges();
                if (!db.PO_Detail.Any(d => d.POCode == po.POCode && d.CheckedIn != true))
                {
                    po.AllCheckedIn = true;
                    po.UpdatedAt = date;
                    po.UpdatedBy = cMem.FullName;
                    db.SaveChanges();
                }
                bool export = Request["action"].ToString() != "Submit";
                return Json(new object[] { true, "Checked In", detail.POCode, export });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });

            }

        }
        //create new model
        /// <summary>
        /// 
        /// </summary>
        /// <param name="detail_id">purchase order detail id</param>
        /// <param name="code">Model code</param>
        /// <param name="price"></param>
        /// <returns></returns>
        public JsonResult NewModel(long detail_id, string code, string name, decimal price)
        {
            using (var tran = db.Database.BeginTransaction())
            {
                try
                {
                    var detail = db.PO_Detail.Find(detail_id);
                    var request = db.PO_Request.Find(detail.RequestCode);
                    var po = db.POes.Find(detail.POCode);

                    HttpPostedFileBase file = HttpContext.Request.Files["model_picture"];
                    string filepath = "";
                    if (!string.IsNullOrEmpty(file?.FileName))
                    {
                        string fileName = AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
                        fileName = DateTime.UtcNow.ToString("yyMMddhhmmssfff") + "_" + fileName;
                        filepath = Path.Combine("/upload/Products", fileName);
                        file.SaveAs(Server.MapPath(filepath));
                    }

                    if (db.O_Product_Model.Any(m => m.ModelCode == code))
                    {
                        throw new Exception("Model code already exist");
                    }

                    var model = new O_Product_Model
                    {
                        Color = request.Color,
                        ModelCode = code,
                        ModelName = name,
                        Picture = filepath,
                        Price = price,
                        ProductCode = request.ProductCode,
                        ProductName = request.ProductName,
                        Status = "Enroute",
                        //VendorId = po.VendorId,
                        //VendorName = po.VendorName
                    };
                    db.O_Product_Model.Add(model);

                    request.ModelPicture = filepath;
                    db.Entry(request).State = System.Data.Entity.EntityState.Modified;

                    var details = (from d in db.PO_Detail
                                   join r in db.PO_Request on d.RequestCode equals r.Code
                                   where r.ProductCode == request.ProductCode && r.Color == model.Color
                                   join p in db.POes on d.POCode equals p.POCode
                                   select d).ToList();
                    foreach (var d in details)
                    {
                        d.ModelCode = code;
                        d.ModelName = model.ModelName;
                        d.ModelPicture = model.Picture;
                        db.Entry(d).State = System.Data.Entity.EntityState.Modified;
                    }
                    db.SaveChanges();
                    tran.Commit();
                    return Json(new object[] { true, "New model created", po.POCode });
                }
                catch (Exception e)
                {
                    tran.Dispose();
                    return Json(new object[] { false, e.Message });
                }
            }


        }

        //Export excel
        public async Task<FileStreamResult> ExportExcel(string po_code)
        {
            try
            {


                string webRootPath = "/upload/other/";
                string fileName = @"TempData.xlsx";
                var po = db.POes.Find(po_code);
                var memoryStream = new MemoryStream();
                // --- Below code would create excel file with dummy data----  
                using (var fs = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Create, FileAccess.Write))
                {

                    IWorkbook workbook = new XSSFWorkbook();

                    IFont font = workbook.CreateFont();
                    font.IsBold = true;
                    font.FontHeightInPoints = 12;

                    IFont font1 = workbook.CreateFont();
                    font1.IsBold = true;
                    font1.Underline = FontUnderlineType.Double;
                    font1.FontHeightInPoints = 13;

                    ICellStyle style = workbook.CreateCellStyle();
                    style.SetFont(font);
                    ICellStyle style1 = workbook.CreateCellStyle();
                    style1.SetFont(font1);

                    ISheet excelSheet = workbook.CreateSheet("data");
                    excelSheet.SetColumnWidth(1, 25 * 256);
                    excelSheet.SetColumnWidth(3, 15 * 256);

                    IRow row = excelSheet.CreateRow(0);
                    ICell cell = row.CreateCell(0);
                    cell.SetCellValue(new XSSFRichTextString("CHECK IN INVENTORY REPORT"));
                    cell.CellStyle = style;
                    excelSheet.AddMergedRegion(new CellRangeAddress(0, 0, 0, 5));


                    IRow row1 = excelSheet.CreateRow(3);
                    row1.CreateCell(0).SetCellValue("PO #");
                    row1.CreateCell(1).SetCellValue(po.POCode);

                    row1.CreateCell(3).SetCellValue("Agent");
                    row1.CreateCell(4).SetCellValue(po.UpdatedBy);


                    IRow row2 = excelSheet.CreateRow(4);
                    row2.CreateCell(0).SetCellValue("Vendor");
                    row2.CreateCell(1).SetCellValue(po.VendorName);

                    row2.CreateCell(3).SetCellValue("Date");
                    row2.CreateCell(4).SetCellValue(po.UpdatedAt.Value.ToString("MMM dd,yyyy hh:mm tt"));

                    IRow row3 = excelSheet.CreateRow(5);
                    row3.CreateCell(0).SetCellValue("SO #");
                    row3.CreateCell(1).SetCellValue(po.SaleOrderNumber);

                    row3.CreateCell(3).SetCellValue("Checked-In");
                    row3.CreateCell(4).SetCellValue(po.AllCheckedIn == true ? "Done" : "Not yet");

                    excelSheet.CreateFreezePane(0, 9, 0, 9);

                    var po_details = (
                                      from c in db.PO_Detail_Checkin
                                      where c.POCode == po.POCode
                                      join d in db.PO_Detail on c.PO_Detail_id equals d.Id
                                      join r in db.PO_Request on d.RequestCode equals r.Code
                                      select new { r.ProductName, r.ModelCode, r.ModelName, r.Color, c.InvNumbers, c.CreatedAt, c.LocationName, c.LocationId, r.ModelPicture } into s
                                      group s by new { s.LocationId, s.LocationName } into gs
                                      select gs).ToList();
                    int _row = 6;
                    for (int i = 0; i < po_details.Count; i++)
                    {
                        _row++;
                        IRow r = excelSheet.CreateRow(_row);
                        ICell location = r.CreateCell(0);
                        location.SetCellValue("Location:");
                        location.CellStyle = style;
                        r.CreateCell(1).SetCellValue(new XSSFRichTextString(po_details[i].Key.LocationName));

                        _row++;
                        IRow r_1 = excelSheet.CreateRow(_row);
                        ICell c_title1 = r_1.CreateCell(0); c_title1.SetCellValue("No."); c_title1.CellStyle = style1;
                        ICell c_title2 = r_1.CreateCell(1); c_title2.SetCellValue("Product"); c_title2.CellStyle = style1;
                        ICell c_title3 = r_1.CreateCell(2); c_title3.SetCellValue("Color"); c_title3.CellStyle = style1;
                        ICell c_title4 = r_1.CreateCell(3); c_title4.SetCellValue("Inv #"); c_title4.CellStyle = style1;
                        ICell c_title5 = r_1.CreateCell(4); c_title5.SetCellValue("Date"); c_title5.CellStyle = style1;


                        int start_row = _row;
                        foreach (var item in po_details[i].ToList())
                        {
                            foreach (var inv in (item.InvNumbers ?? "").Split(','))
                            {
                                _row++;
                                IRow r_next = excelSheet.CreateRow(_row);
                                r_next.CreateCell(0).SetCellValue(_row - start_row);
                                r_next.CreateCell(1).SetCellValue(item.ProductName + (!string.IsNullOrEmpty(item.ModelCode) ? "(" + item.ModelName + ")" : ""));
                                r_next.CreateCell(2).SetCellValue(item.Color);
                                r_next.CreateCell(3).SetCellValue(inv);
                                r_next.CreateCell(4).SetCellValue(item.CreatedAt.HasValue ? item.CreatedAt.Value.ToString("MMM dd,yyyy hh:mm tt") : "");

                            }
                        }



                    }


                    workbook.Write(fs);

                }

                using (var fileStream = new FileStream(Server.MapPath(Path.Combine(webRootPath, fileName)), FileMode.Open))
                {
                    await fileStream.CopyToAsync(memoryStream);
                }
                memoryStream.Position = 0;
                string _fileName = DateTime.UtcNow.ToString("yyyyMMdd") + "_" + po.POCode + "_" + AppLB.CommonFunc.ConvertNonUnicodeURL(po.VendorName).Replace("-", "_") + ".xlsx";
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", _fileName);


                #region interop excel - disabled

                //Application excel = new Application();
                //excel.Visible = false;
                //excel.DisplayAlerts = false;
                //var workbook = excel.Workbooks.Add(Type.Missing);
                //var worksheet = (Worksheet)workbook.ActiveSheet;
                //worksheet.Name = "Check in inventory";
                //worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 6]].Merge();
                //worksheet.Cells[1, 1] = "CHECK IN INVENTORY";
                //var po = db.POes.Find(po_code);

                //worksheet.Cells[2, 1] = "PO #";
                //worksheet.Cells[2, 2] = po.POCode;
                //worksheet.Cells[3, 1] = "Vendor";
                //worksheet.Cells[3, 2] = po.VendorName;
                //worksheet.Cells[4, 1] = "SO #";
                //worksheet.Cells[4, 2] = "'" + po.SaleOrderNumber;

                //worksheet.Cells[2, 4] = "Agent";
                //worksheet.Cells[2, 5] = po.UpdatedBy;
                //worksheet.Cells[3, 4] = "Date";
                //worksheet.Cells[3, 5] = po.UpdatedAt.Value.ToString("MMM dd,yyyy hh:mmtt");
                //worksheet.Cells[4, 4] = "Checked in";
                //worksheet.Cells[4, 5] = po.AllCheckedIn == true ? "Done" : "Not yet";

                //worksheet.Cells[1, 1].Font.Bold = true;
                //worksheet.Cells[2, 1].Font.Bold = true;
                //worksheet.Cells[3, 1].Font.Bold = true;
                //worksheet.Cells[4, 1].Font.Bold = true;

                //worksheet.Cells[2, 4].Font.Bold = true;
                //worksheet.Cells[3, 4].Font.Bold = true;
                //worksheet.Cells[4, 4].Font.Bold = true;



                //var po_details = (
                //                  from c in db.PO_Detail_Checkin
                //                  where c.POCode == po.POCode
                //                  join d in db.PO_Detail on c.PO_Detail_id equals d.Id
                //                  join r in db.PO_Request on d.RequestCode equals r.Code
                //                  select new { r.ProductName, r.ModelCode, r.Color, c.InvNumbers, c.CreatedAt, c.LocationName, c.LocationId, r.ModelPicture } into s
                //                  group s by new { s.LocationId, s.LocationName } into gs
                //                  select gs).ToList();
                //int row = 6;
                //worksheet.Rows[row].EntireColumn.rowheight = 20;
                //for (int i = 0; i < po_details.Count; i++)
                //{
                //    row++;
                //    worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 6]].Merge();
                //    worksheet.Cells[row, 1] = "Location: " + po_details[i].Key.LocationName;
                //    worksheet.Cells[row, 1].Font.Bold = true;
                //    worksheet.Rows[row].EntireColumn.rowheight = 20;
                //    row++;
                //    worksheet.Cells[row, 1] = "No.";
                //    worksheet.Cells[row, 2] = "Product";
                //    worksheet.Cells[row, 3] = "Color";
                //    worksheet.Cells[row, 4] = "Inv #";
                //    worksheet.Cells[row, 5] = "Check in date";
                //    worksheet.Cells[row, 6] = "Print";
                //    int start_row = row;
                //    foreach (var item in po_details[i].ToList())
                //        foreach (var inv in item.InvNumbers.Split(','))
                //        {
                //            row++;
                //            worksheet.Cells[row, 1] = row - start_row;
                //            worksheet.Cells[row, 2] = item.ProductName + (!string.IsNullOrEmpty(item.ModelCode) ? "(" + item.ModelCode + ")" : "");
                //            worksheet.Cells[row, 3] = item.Color;
                //            worksheet.Cells[row, 4] = inv;
                //            worksheet.Cells[row, 5] = item.CreatedAt.HasValue ? item.CreatedAt.Value.ToString("MMM dd,yyyy hh:mm tt") : "";
                //            worksheet.Cells[row, 4].Font.Bold = true;
                //            //Range oRange = (Range)worksheet.Cells[row, 6];
                //            //float Left = (float)((double)oRange.Left);
                //            //float Top = (float)((double)oRange.Top);
                //            //const float ImageSize = 40;
                //            //worksheet.Shapes.AddPicture(Server.MapPath(item.ModelPicture), Microsoft.Office.Core.MsoTriState.msoFalse, Microsoft.Office.Core.MsoTriState.msoCTrue, Left, Top, ImageSize, ImageSize);
                //            //    worksheet.Cells[row, 1].rowheight = 40;
                //        }

                //    var cellrange = worksheet.Range[worksheet.Cells[start_row, 1], worksheet.Cells[row, 6]];
                //    Borders b = cellrange.Borders;
                //    b.LineStyle = XlLineStyle.xlContinuous;
                //    b.Weight = 2d;
                //}

                //var celLrangE = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[row, 6]];
                //worksheet.Columns[1].columnwidth = 10;
                //worksheet.Columns[2].columnwidth = 40;
                //worksheet.Columns[3].columnwidth = 10;
                //worksheet.Columns[4].columnwidth = 15;
                //worksheet.Columns[5].columnwidth = 30;
                //worksheet.Columns[6].columnwidth = 30;

                ////celLrangE = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[2, ExportToExcel().Columns.Count]];
                //string savename = "/upload/" + DateTime.UtcNow.ToString("yyyyMMdd") + "_" + po.POCode + "_" + po.VendorName.Replace(" ", "_") + ".xlsx";
                //worksheet.SaveAs(Server.MapPath(savename));
                //excel.Quit();

                #endregion



                //return Json(new object[] { true, Path.Combine(webRootPath, fileName) });

            }
            catch (Exception e)
            {
                throw;
                //return Json(new object[] { false, e.Message });
            }
        }

        //load ajax
        public JsonResult GetModelInfo(string code)
        {
            try
            {
                var model = db.O_Product_Model.Where(pm => pm.ModelCode == code).FirstOrDefault();
                if (model == null)
                {
                    throw new Exception("Model not found!");
                }
                return Json(new object[] { true, model });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }
        public JsonResult LoadModelSelect(string product_code)
        {
            try
            {
                var models = db.O_Product_Model.Where(pm => string.IsNullOrEmpty(product_code) || pm.ProductCode == product_code).Select(pm => new { pm.ModelCode, pm.ModelName }).ToList();
                return Json(new object[] { true, models });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }
        }
        public JsonResult GetRequestInfo(string code)
        {
            try
            {
                var model = db.PO_Request.Where(pm => pm.Code == code).FirstOrDefault();
                if (model == null)
                {
                    throw new Exception("Model not found!");
                }
                return Json(new object[] { true, model });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }

        public JsonResult get_model_img(string product_code, string color)
        {
            var model = db.O_Product_Model.Where(m => m.ProductCode == product_code && m.Color == color).FirstOrDefault();
            string img = string.IsNullOrEmpty(model?.Picture) ? "/Upload/Img/no_image.jpg" : model.Picture;
            return Json(img);
        }
        public JsonResult LoadColorlist()
        {
            var db = new WebDataModel();
            return Json(db.O_Product_Model.GroupBy(m => m.Color).Select(m => m.Key).ToList());
        }
    }


}