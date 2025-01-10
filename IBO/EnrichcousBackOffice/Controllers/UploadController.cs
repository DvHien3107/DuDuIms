using EnrichcousBackOffice.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class UploadController : Controller
    {

        /// <summary>
        /// uploads, tra ve duong dan tuong doi cua file
        /// </summary>
        /// <param name="strPath">folder luu tru tren server</param>
        /// <param name="InputFileName">input file name</param>
        /// <param name="prefix">prefix</param>
        /// <param name="fileName">Output duong dan tuong doi tap tin</param>
        public string UploadAttachFile(string strPath, string inputFileName, string prefix, string replaceName, out string fileName, bool date_tail = false)
        {
            fileName = "";
            try
            {
                #region check path
                DirectoryInfo d = new DirectoryInfo(Server.MapPath(strPath));
                if (!d.Exists)
                {
                    d.Create();
                }

                #endregion
                HttpPostedFileBase file = HttpContext.Request.Files[inputFileName];
                if (string.IsNullOrWhiteSpace(file?.FileName) == false)
                {
                    string pathFileName = string.Empty;
                    if (date_tail)
                    {

                        pathFileName = AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
                        var index = pathFileName.LastIndexOf('.'); index = index != -1 ? index : pathFileName.Length;
                        pathFileName = pathFileName.Insert(index, "_" + DateTime.Now.ToString("yyMMddhhmmssf"));
                    }
                    else
                    {
                        pathFileName = DateTime.Now.ToString("yyMMddhhmmssf") + "_" + AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
                    }

                    string strFileName;
                    if (string.IsNullOrWhiteSpace(replaceName))
                    {
                        if (!string.IsNullOrWhiteSpace(prefix))
                        {
                            strFileName = prefix + "_" + Path.GetFileName(pathFileName);
                        }
                        else
                        {
                            strFileName = Path.GetFileName(pathFileName);
                        }
                    }
                    else
                    {
                        strFileName = replaceName;
                    }

                    string fullPath = Path.Combine(Server.MapPath(strPath), strFileName);
                    file.SaveAs(fullPath);

                    fileName = Path.Combine(strPath, strFileName);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return fileName;
        }

        /// <summary>
        /// download file
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult DownloadFile(long id)
        {

            WebDataModel db = new WebDataModel();
            UploadMoreFile morefile = db.UploadMoreFiles.Find(id);

            string sPath = Path.Combine(Server.MapPath(morefile.FileName));
            return File(sPath, "application/octet-stream", morefile.FileName.Substring(morefile.FileName.LastIndexOf("/") + 1));

        }

        /// <summary>
        /// download file
        /// </summary>
        /// <param name="file">duong dan tuong doi tap tin</param>
        /// <returns></returns>
        public FileResult DownloadFile_(string file)
        {
            string sPath = Path.Combine(Server.MapPath(file));
            return File(sPath, "application/octet-stream", file.Substring(file.LastIndexOf("/") + 1));

        }

        /// <summary>
        /// delete file trong table uploadmorefile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public JsonResult MoreFileDeleteById(long id, string sPath)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                UploadMoreFile morefile = db.UploadMoreFiles.Find(id);
                sPath = Path.Combine(Server.MapPath(sPath), morefile.FileName);

                FileInfo f = new FileInfo(sPath);
                if (f.Exists)
                {
                    f.Delete();
                }

                db.Entry(morefile).State = EntityState.Deleted;
                db.SaveChanges();
                return Json(true);

            }
            catch (Exception)
            {
                return Json(false);
            }

        }

        public JsonResult MoreFileDelete(List<UploadMoreFile> files, string sPath = "")
        {
            try
            {
                var db = new WebDataModel();
                foreach (var morefile in files)
                {
                    sPath = Server.MapPath(Path.Combine(morefile.FileName));
                    FileInfo f = new FileInfo(sPath);
                    if (f.Exists && !db.UploadMoreFiles.Any(u => u.FileName == morefile.FileName && u.TableId != morefile.TableId))
                    {
                        f.Delete();
                    }
                }
                return Json(true);

            }
            catch (Exception)
            {
                return Json(false);
            }

        }

        /// <summary>
        /// delete file trong chuoi cac file cach nhau boi dau ;
        /// </summary>
        /// <param name="id"></param>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public JsonResult FileDelete(string id, string table_id, string table_name, string column_name)
        {
            try
            {
                WebDataModel db = new WebDataModel();

                string data = db.Database.SqlQuery<string>("Select " + column_name + " from " + table_name + " where Id = " + table_id).FirstOrDefaultAsync().Result;
                string filename = string.Empty;
                foreach (var item in data.Split(new char[] { ';' }))
                {
                    if (!string.IsNullOrWhiteSpace(item) && item.Contains(id))
                    {
                        filename = item + ";";
                    }
                }

                if (!string.IsNullOrWhiteSpace(filename))
                {
                    data = data.Replace(filename, "");
                    int noOfRowUpdated = db.Database.ExecuteSqlCommand("Update " + table_name + " set " + column_name + " = N'" + data + "' where Id = " + table_id);


                    string sPath = Path.Combine(filename);
                    FileInfo f = new FileInfo(sPath);
                    if (f.Exists)
                    {
                        f.Delete();
                    }



                }

                return Json(true);

            }
            catch (Exception)
            {
                return Json(false);
            }

        }

        #region upload multiple files

        /// <summary>
        /// upload [morefiles], save vao table UploadMoreFiles
        /// </summary>
        /// <param name="tableid"></param>
        /// <param name="tablename"></param>
        /// <param name="filestotal"></param>
        /// <param name="strPath"></param>
        protected List<string> UploadMoreFiles(long tableid, string tablename, int filestotal, string strPath = "/Upload/Other", string prefix = "", string img_name_element = null)
        {
            try
            {
                List<string> UploadIds = new List<string>();
                WebDataModel db = new WebDataModel();
                for (int i = 1; i <= filestotal; i++)
                {
                    string f = (img_name_element ?? "morefiles_") + i;
                    //Loc them 20200102
                    if (strPath == "/upload/img_snap")
                    {
                        f = "morefile_snap" + i;
                    }
                    //End

                    HttpPostedFileBase file = HttpContext.Request.Files[f];

                    if (file != null && file.FileName != "")
                    {
                        if (file.ContentLength > 20971520)
                        {
                            throw new Exception("File upload content length larger than allowed is 20 MB");
                        }
                        string fileName = AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
                        fileName = DateTime.Now.ToString("yyMMddhhmmssfff") + "_" + Path.GetFileName(fileName);
                        if (!string.IsNullOrWhiteSpace(prefix))
                        {
                            fileName = prefix + "_" + Path.GetFileName(fileName);

                        }


                        string fullPath = Path.Combine(Server.MapPath(strPath), fileName);
                        file.SaveAs(fullPath);


                        UploadMoreFile up = new UploadMoreFile
                        {
                            UploadId = DateTime.Now.Ticks + i,
                            FileName = Path.Combine(strPath, fileName),
                            TableId = tableid,
                            TableName = tablename
                        };
                        db.UploadMoreFiles.Add(up);
                        UploadIds.Add(up.UploadId.ToString());
                        db.SaveChanges();
                    }
                }
                return UploadIds;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// upload va tra ve duong dan cac file(;). khong save vao db.[morefiles]
        /// </summary>
        /// <param name="filestotal"></param>
        /// <param name="strPath">/upload/other</param>
        /// <param name="prefix"></param>
        /// <param name="returnFullPath">true: tra ve duong dan tuyet doi| false: tra ve duong dan tuong doi</param>
        protected string UploadMoreFiles(int filestotal, string strPath = "/Upload/Other", bool returnFullPath = false)
        {
            try
            {
                string _attachFiles = string.Empty;
                for (int i = 1; i <= filestotal; i++)
                {
                    string f = "morefiles_" + i;

                    HttpPostedFileBase file = HttpContext.Request.Files[f];

                    if (file != null && file.FileName != "")
                    {
                        string fileName = AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
                        fileName = DateTime.Now.ToString("yyMMddhhmmssfff") + "_" + Path.GetFileName(fileName);
                        string fullPath = Path.Combine(Server.MapPath(strPath), fileName);
                        file.SaveAs(fullPath);
                        if (returnFullPath)
                        {
                            _attachFiles += fullPath + ";";
                        }
                        else
                        {
                            _attachFiles += Path.Combine(strPath, fileName) + ";";
                        }

                    }
                }

                return _attachFiles;

            }
            catch (Exception)
            {
                throw;
            }


        }

        /// <summary>
        /// upload [morefiles], save vao table UploadMoreFiles
        /// </summary>
        /// <param name="tableid"></param>
        /// <param name="tablename"></param>
        /// <param name="filestotal"></param>
        /// <param name="strPath"></param>
        protected async Task<List<string>> UploadMultipleFilesAsync(long tableid, string tablename, int filestotal, string strPath = "/Upload/Other", string prefix = "", string img_name_element = null)
        {
            try
            {
                var uploadIds = new ConcurrentBag<string>();
                
                int start = 0;
                //get started because the form has many input files
                for (int i = 0; i < filestotal; i++)
                {
                    var key = Request.Files.GetKey(i);
                    if (key.Split('-')[0] == "uploadfiles")
                    {
                        start = i;
                        break;
                    }
                }
                var listTask = new List<Task>();
                var Id = DateTime.Now.Ticks;
                Random rnd = new Random();
                Parallel.For(0, (filestotal),
                        index =>
                        {
                            HttpPostedFileBase file = HttpContext.Request.Files[index];

                            if (file != null && file.FileName != "")
                            {
                                if (file.ContentLength <= 104857600)
                                {
                                    string fileName = AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
                                    fileName = DateTime.Now.ToString("yyMMddhhmmssfff") + rnd.Next(0, 9999) + "_" + Path.GetFileName(fileName);
                                    if (!string.IsNullOrWhiteSpace(prefix))
                                    {
                                        fileName = prefix + "_" + Path.GetFileName(fileName);
                                    }

                                    string fullPath = Path.Combine(Server.MapPath(strPath), fileName);
                                    file.SaveAs(fullPath);

                                    UploadMoreFile up = new UploadMoreFile
                                    {
                                        UploadId = Id + index,
                                        FileName = Path.Combine(strPath, fileName),
                                        TableId = tableid,
                                        TableName = tablename
                                    };
                                    using (var db = new WebDataModel())
                                    {
                                        db.UploadMoreFiles.Add(up);
                                        db.SaveChanges();
                                    }
                                    uploadIds.Add(up.UploadId.ToString());


                                }
                            }
                        });



             
                return uploadIds.ToList();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        /// <summary>
        /// upload va tra ve duong dan cac file(;). khong save vao db.[morefiles]
        /// </summary>
        /// <param name="filestotal"></param>
        /// <param name="strPath">/upload/other</param>
        /// <param name="prefix"></param>
        /// <param name="returnFullPath">true: tra ve duong dan tuyet doi| false: tra ve duong dan tuong doi</param>
        //protected string UploadMultipleFiles(int filestotal, string strPath = "/Upload/Other", bool returnFullPath = false)
        //{
        //    try
        //    {
        //        string _attachFiles = string.Empty;
        //        for (int i = 0; i < filestotal; i++)
        //        {
        //            HttpPostedFileBase file = HttpContext.Request.Files[i];
        //            if (file != null && file.FileName != "")
        //            {
        //                string fileName = AppLB.CommonFunc.ConvertNonUnicodeURL(Regex.Replace(file.FileName, "[ ,?#$&(){}~!]", ""));
        //                fileName = DateTime.Now.ToString("yyMMddhhmmssfff") + "_" + Path.GetFileName(fileName);
        //                string fullPath = Path.Combine(Server.MapPath(strPath), fileName);
        //                file.SaveAs(fullPath);
        //                if (returnFullPath)
        //                {
        //                    _attachFiles += fullPath + ";";
        //                }
        //                else
        //                {
        //                    _attachFiles += Path.Combine(strPath, fileName) + ";";
        //                }

        //            }
        //        }

        //        return _attachFiles;

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }


        //}



        #endregion

        public JsonResult DirectUpload()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                long TableId = long.Parse(Request["TableId"]);
                string TableName = Request["TableName"];
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    long id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff"));
                    HttpPostedFileBase file = Request.Files[i];
                    int fileSize = file.ContentLength;
                    string fileName = "/Upload/MerAttachment\\" + file.FileName;
                    string mimeType = file.ContentType;
                    System.IO.Stream fileContent = file.InputStream;
                    file.SaveAs(Path.Combine(Server.MapPath("~/") + fileName));
                    var save = new UploadMoreFile
                    {
                        UploadId = id++,
                        FileName = fileName,
                        TableId = TableId,
                        TableName = TableName
                    };
                    db.UploadMoreFiles.Add(save);
                    db.SaveChanges();
                    //To save file, use SaveAs method
                    //File will be saved in application root
                }
                return Json(new object[] { true, "Uploaded " + Request.Files.Count + " attachments" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });

            }

        }

        public JsonResult OriginUpload()
        {
            try
            {
                WebDataModel db = new WebDataModel();
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    long id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssff"));
                    HttpPostedFileBase file = Request.Files[i];
                    int fileSize = file.ContentLength;
                    string fileName = "/Upload/Other\\" + file.FileName;
                    string mimeType = file.ContentType;
                    System.IO.Stream fileContent = file.InputStream;
                    file.SaveAs(Path.Combine(Server.MapPath("~/") + fileName));
                }
                return Json(new object[] { true, "Uploaded " + Request.Files.Count + " attachments" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });

            }

        }
    }

}
