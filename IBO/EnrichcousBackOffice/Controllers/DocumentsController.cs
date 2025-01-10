using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    [MyAuthorize]
    public class DocumentsController : UploadController
    {
        // GET: Documents

        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        const string CategoryViewFunctionCodePrefix = "document_category_view_";
        const string CategoryEditFunctionCodePrefix = "document_category_update_";

        public ActionResult Index()
        {
            WebDataModel db = new WebDataModel();
            var categories = db.Document_Category.ToList();
            return View(categories);
        }
        #region category
        public ActionResult AddOrUpdateDocumentCategory(int? CategoryId)
        {
            WebDataModel db = new WebDataModel();
            var category = new Document_Category();
            if (CategoryId != null)
            {
                category = db.Document_Category.Find(CategoryId);
                if (category == null)
                {
                    return Content("document file not found");
                }
            }
            ViewBag.ListCategoryAvailable = db.Document_Category.Where(x => x.ParentId == null).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
            return PartialView("_DocumentCategoryCreateOrUpdate", category);
        }
        [HttpPost]
        public ActionResult AddOrUpdateCategorySubmit(Document_Category model)
        {
            WebDataModel db = new WebDataModel();
            try
            {
                if (model.Id > 0)
                {
                    var category = db.Document_Category.Find(model.Id);
                    if (category == null)
                    {
                        return Json(new { status = false, message = "category not found" });
                    }
                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.DisplayOrder = model.DisplayOrder;
                    category.UpdateAt = DateTime.UtcNow;
                    category.UpdateBy = cMem.FullName;
                    if (category.ParentId == null)
                    {
                        var AccessView = db.A_FunctionInPage.Where(x => x.FunctionCode == CategoryViewFunctionCodePrefix + category.Id).FirstOrDefault();
                        if (AccessView != null)
                        {
                            AccessView.FunctionName =  model.Name + ". View access";
                        }
                        var AccessUpdate = db.A_FunctionInPage.Where(x => x.FunctionCode == CategoryEditFunctionCodePrefix + category.Id).FirstOrDefault();
                        if (AccessUpdate != null)
                        {
                            AccessUpdate.FunctionName =  model.Name + ". Update access";
                        }
                    }
                   
                    db.SaveChanges();
                    return Json(new { status = true, category = category, message = "update category success" });
                }
                else
                {
                    model.CreateAt = DateTime.UtcNow;
                    model.CreateBy = cMem.FullName;
                 
                    db.Document_Category.Add(model);
                    db.SaveChanges();
                    if (model.ParentId == null)
                    {
                        var lastDisplayOrder = db.A_FunctionInPage.Where(x => x.PageCode == "document").OrderByDescending(x => x.Order).FirstOrDefault()?.Order??0;
                        var newAccessView = new A_FunctionInPage();
                        newAccessView.FunctionCode = CategoryViewFunctionCodePrefix + model.Id;
                        newAccessView.FunctionName =model.Name + ". View access";
                        newAccessView.PageCode = "document";
                        newAccessView.Order = ++lastDisplayOrder;
                        var newAccessUpdate = new A_FunctionInPage();
                        newAccessUpdate.FunctionCode = CategoryEditFunctionCodePrefix + model.Id;
                        newAccessUpdate.FunctionName =model.Name + ". Update access";
                        newAccessUpdate.PageCode = "document";
                        newAccessUpdate.Order = ++lastDisplayOrder;

                        db.A_FunctionInPage.Add(newAccessView);
                        db.A_FunctionInPage.Add(newAccessUpdate);
                        db.SaveChanges();
                    }
                   
                
                    return Json(new { status = true, category = model, message = "add category success" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult DeleteCategory(int CategoryId)
        {
            WebDataModel db = new WebDataModel();
            try
            {
              
                    var category = db.Document_Category.Find(CategoryId);
                    if (category == null)
                    {
                        return Json(new { status = false, message = "category not found" });
                    }
                    if (category.ParentId == null)
                    {
                        var accessview = db.A_FunctionInPage.Find(CategoryViewFunctionCodePrefix + CategoryId.ToString());
                        if (accessview != null)
                        {
                           db.A_FunctionInPage.Remove(accessview);
                        var grandAccess = db.A_GrandAccess.Where(x => x.FunctionCode == accessview.FunctionCode).ToList();
                        if (grandAccess.Count > 0)
                        {
                            db.A_GrandAccess.RemoveRange(grandAccess);
                        }
                        }

                    var accessupdate = db.A_FunctionInPage.Find(CategoryEditFunctionCodePrefix + CategoryId.ToString());
                    if (accessview != null)
                    {
                        db.A_FunctionInPage.Remove(accessupdate);
                        var grandAccess = db.A_GrandAccess.Where(x => x.FunctionCode == accessupdate.FunctionCode).ToList();
                        if (grandAccess.Count > 0)
                        {
                            db.A_GrandAccess.RemoveRange(grandAccess);
                        }
                    }
                     }
                   db.Document_Category.Remove(category);
                    db.SaveChanges();
                    return Json(new { status = true, message = "delete category success" });
                
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult ReloadMenu(int? CategorySelectedId)
        {
            WebDataModel db = new WebDataModel();
            var allCategory = db.Document_Category.ToList();
            ViewBag.CategorySelectedId = CategorySelectedId;
            return PartialView("_DocumentCategoryMenu", allCategory);

        }
        #endregion

        #region document file
        public ActionResult GetListFile(IDataTablesRequest dataTablesRequest, DocumentFileSearchModel searchModel)
        {
            WebDataModel db = new WebDataModel();
            var categoryIds = new List<int>();
            categoryIds.Add(searchModel.CategoryId);
            var subcategory = db.Document_Category.Where(x => x.ParentId == searchModel.CategoryId).Select(x=>x.Id).ToList();
            if (subcategory.Count > 0)
            {
                categoryIds.AddRange(subcategory);
            }
            var query = from documentfiles in db.Document_File where categoryIds.Any(x=>x== documentfiles.CategoryId) select documentfiles;
            var colSearch = dataTablesRequest.Columns.Where(c => c.Sort != null).FirstOrDefault();
            switch (colSearch?.Name)
            {
  
                case "Name":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.Name);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.Name);
                    }
                    break;
                case "UpdateBy":
                    if (colSearch.Sort.Direction == SortDirection.Ascending)
                    {
                        query = query.OrderBy(x => x.UpdateAt);
                    }
                    else
                    {
                        query = query.OrderByDescending(x => x.UpdateAt);
                    }
                    break;

                default:
                    query = query.OrderByDescending(x => x.CreateAt);
                    break;
            };
            int totalRecord = query.Count();
            var data = query.Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x =>
            {
                var model = new DocumentFileModel();
                model.Id = x.Id;
                model.Name = x.Name;
                model.Type = x.Type;
                model.Description = x.Description;
                string Breadcrumb = "";
                var category = db.Document_Category.Where(y => y.Id == x.CategoryId).FirstOrDefault();
                if (category != null)
                {
                    if (category.ParentId == null)
                    {
                        Breadcrumb = category.Name;
                    }
                    else
                    {
                        var parentCategory = db.Document_Category.Where(z => z.Id == category.ParentId).FirstOrDefault();
                        if (parentCategory != null)
                        {
                            Breadcrumb = parentCategory.Name+"> "+ category.Name;
                        }
                        else
                        {
                            Breadcrumb = category.Name;
                        }
                    }
                }
               

           
                model.Breadcrumb = Breadcrumb;
                model.CreateAt = string.Format("{0:r}", x.CreateAt);
                model.CreateBy = x.CreateBy;
                model.UpdateAt = string.Format("{0:r}", x.UpdateAt);
                model.UpdateBy = x.UpdateBy;

                return model;
            });
            return Json(new
            {
                data = data,
                recordsTotal = totalRecord,
                recordsFiltered = totalRecord
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetContent(int CategoryId)
        {
            WebDataModel db = new WebDataModel();
            var category = db.Document_Category.Find(CategoryId);
            if (category == null)
            {
                return Content("category not found");
            }
            var model = new DocumentCategoryModel();
            model.Id = category.Id;
            model.Name = category.Name;
            model.Description = category.Description;
            model.UpdateAt = category.UpdateAt;
            model.UpdateBy = category.UpdateBy;
            model.CreateAt = category.CreateAt;
            model.CreateBy = category.CreateBy;
            //var files = new List<DocumentFileModel>();
            //if (category.IsRoot)
            //{

            //}
            //else
            //{
            //    var files = db.Document_File.Where(x => x.CategoryId == category.Id).ToList();
            //    foreach(var file in files)
            //    {

            //    }
            //}
            //model.DocumentFiles = files;
          //  ViewBag.access = access;
            return PartialView("_DocumentFileContent",model);
        }

        
        public ActionResult AddOrUpdateDocumentFiles(int? DocumentFileId,int? CategoryId)
        {
            WebDataModel db = new WebDataModel();
            var documentFiles = new Document_File();
            if (DocumentFileId !=null)
            {
                 documentFiles = db.Document_File.Find(DocumentFileId);
                if (documentFiles == null)
                {
                    return Content("document file not found");
                }
                if(documentFiles.Type== "Attachment")
                {
                    ViewBag.attachments = db.UploadMoreFiles.Where(x => x.TableId == DocumentFileId && x.TableName == "Document_File").ToList();
                }
            }
            else
            {
                documentFiles.CategoryId = CategoryId.Value;
                documentFiles.Type = "Attachment";
            }
           
            return PartialView("_DocumentFileCreateOrUpdate", documentFiles);
        }

        [HttpPost]
        public async Task<ActionResult> AddOrUpdateDocumentFilesSubmit(Document_File model)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                
                if (model.Id > 0)
                {
                    var documentFiles = db.Document_File.Find(model.Id);
                    if (documentFiles == null)
                    {
                        return Json(new { status = false, message = "document file not found" });
                    }
                    documentFiles.Name = model.Name;
                    documentFiles.Description = model.Description;
                    documentFiles.Type = model.Type;
                    if (model.Type == "Attachment")
                    {
                      
                        
                            int filesTotal = Request.Files.Count;
                            var UploadIds = await UploadMultipleFilesAsync(documentFiles.Id, "Document_File", filesTotal);
                            documentFiles.Link = null;
           
                            //string Path = "/upload/documents/documentfiles";
                            //string absolutePath = Server.MapPath(Path);
                            //bool exists = System.IO.Directory.Exists(absolutePath);
                            //if (!exists)
                            //    System.IO.Directory.CreateDirectory(absolutePath);
                            //var DocumentFileFiles = Request.Files["AttachmentFile"];
                            //string FileName = System.IO.Path.GetFileNameWithoutExtension(DocumentFileFiles.FileName) + DateTime.UtcNow.ToString("hhmmffff") + System.IO.Path.GetExtension(DocumentFileFiles.FileName);
                            //if (DocumentFileFiles.ContentLength > 0)
                            //{
                            //    DocumentFileFiles.SaveAs(absolutePath + "/" + FileName);
                            //    documentFiles.Link = Path + "/" + FileName;
                            //    documentFiles.FileName = DocumentFileFiles.FileName;
                            //    documentFiles.FileExtension = System.IO.Path.GetExtension(DocumentFileFiles.FileName);
                            //}


                    }
                    else
                    {
                        documentFiles.UploadIds = null;
                        documentFiles.Link = model.Link;
                    }
                    documentFiles.UpdateAt = DateTime.UtcNow;
                    documentFiles.UpdateBy = cMem.FullName;
                    db.SaveChanges();
                    return Json(new { status = true, message = "update document file success" });
                }
                else
                {
                   
                    model.CreateAt = DateTime.UtcNow;
                    model.UpdateBy = cMem.FullName;
                    db.Document_File.Add(model);
                    db.SaveChanges();
                    if (model.Type == "Attachment")
                    {
                        int filesTotal = Request.Files.Count;
                        var UploadIds = await UploadMultipleFilesAsync (model.Id, "Document_File", filesTotal);
                        model.Link = null;

                    }
                    else
                    {
                        model.UploadIds = null;
                        model.Link = model.Link;
                    }
                    db.SaveChanges();
                    return Json(new { status = true, message = "add document file success" });
                }
               
            }
           catch(Exception ex)
            {
                return Json(new { status = false,message=ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DetailDocumentFile(int DocumentFileId)
        {
            WebDataModel db = new WebDataModel();
            var documentfile = db.Document_File.Find(DocumentFileId);
            if (documentfile == null)
            {
                return Content("document file not found");
            }
            if (documentfile.Type=="Attachment")
            {
                ViewBag.Attachments = db.UploadMoreFiles.Where(x => x.TableId == DocumentFileId && x.TableName == "Document_File").ToList();
            }
            return PartialView("_DocumentFileDetail", documentfile);
        }

        [HttpPost]
        public ActionResult DeleteDocumentFile(int DocumentFileId)
        {
            WebDataModel db = new WebDataModel();
            var documentfile = db.Document_File.Find(DocumentFileId);
            if (documentfile == null)
            {
                return Json(new {status=false,message="document file not found" });
            }
            db.Document_File.Remove(documentfile);
            db.SaveChanges();
            return Json(new { status = true, message = "delete file success" });
        }
        [HttpPost]
        public ActionResult DownLoadDocumentFile(int DocumentFileId)
        {
            WebDataModel db = new WebDataModel();
            var documentfile = db.Document_File.Find(DocumentFileId);
            if (documentfile == null)
            {
                return Json(new { status = false, message = "document file not found" });
            }
            var fullpath = Server.MapPath(documentfile.Link);
            if (!System.IO.File.Exists(fullpath))
            {
                return Json(new { status = false, message = "file not found" });
            }
              return Json(new { status = true, message = "success",path= documentfile.Link });
        }
        //public ActionResult DownloadFile(string path)
        //{
        //    var filename = System.IO.Path.GetFileName(path);
        //    byte[] bytes = System.IO.File.ReadAllBytes(path);

        //    //Send the File to Download.
        //    return File(bytes, "application/octet-stream", filename);
          
        //}
        #endregion
    }
}