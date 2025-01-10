using DataTables.AspNet.Core;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Utils.AppConfig;
using Hangfire;
using Newtonsoft.Json;
using Notification.SMS.IMS.Service;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace EnrichcousBackOffice.Controllers
{
    public class OtherReportController : Controller
    {

        private Dictionary<string, bool> access = Authority.GetAccessAuthority();
        // GET: Report
        public ActionResult SMSReport()
        {
            if (access.Any(k => k.Key.Equals("report_sms")) == false || access["report_sms"] != true)
            {
                return Redirect("/home/forbidden");
            }
            return View();
        }
        // GET: Export
        public ActionResult SMSExport()
        {
            if (access.Any(k => k.Key.Equals("report_sms")) == false || access["report_sms"] != true)
            {
                return Redirect("/home/forbidden");
            }
            return View();
        }
        // POST: Report
        public void ProcessSMSExportFile(SMSReportSearchModel filter, SMSExportFile smsExportFiles)
        {
            var _smsService = new NotificationService();
            if (filter.DateSentAfter.HasValue)
            {
                filter.DateSentAfter = filter.DateSentAfter.Value.Date + new TimeSpan(0, 0, 0);
                filter.DateSentAfter = filter.DateSentAfter.Value.IMSToUTCDateTime();
            }
            if (filter.DateSentBefore.HasValue)
            {
                filter.DateSentBefore = filter.DateSentBefore.Value.Date + new TimeSpan(23, 59, 59);
                filter.DateSentBefore = filter.DateSentBefore.Value.IMSToUTCDateTime();
            }
            var listMessage = _smsService.GetAllMessage(FromDate: filter.DateSentAfter, ToDate: filter.DateSentBefore, FromPhone: filter.FromPhone, Body: filter.Body, NumSegments: filter.NumSegments);

            var smsData = listMessage.Select(x => new
            {

                x.Body,
                x.NumSegments,
                DateSent = String.Format("{0:r}", x.DateSent),
                DateCreated = String.Format("{0:r}", x.DateCreated),
                From = x.From.ToString(),
                x.To,
                Status = x.Status.ToString(),
                x.Price,
                x.PriceUnit,
                x.ErrorMessage
            });

            var memoryStream = new MemoryStream();
            // --- Below code would create excel file with dummy data----
            var path = HostingEnvironment.MapPath(smsExportFiles.Path);
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {

                IWorkbook workbook = new XSSFWorkbook();
                //name style
                IFont font = workbook.CreateFont();
                font.IsBold = true;
                font.FontHeightInPoints = 14;
                ICellStyle style = workbook.CreateCellStyle();
                style.SetFont(font);

                IDataFormat dataFormatCustom = workbook.CreateDataFormat();
                ISheet excelSheet = workbook.CreateSheet("Sheet 1");
                //set column width
                excelSheet.SetColumnWidth(1, 15 * 256);
                excelSheet.SetColumnWidth(2, 15 * 256);
                excelSheet.SetColumnWidth(3, 20 * 256);
                excelSheet.SetColumnWidth(4, 10 * 256);
                excelSheet.SetColumnWidth(5, 20 * 256);
                excelSheet.SetColumnWidth(6, 15 * 256);
                excelSheet.SetColumnWidth(7, 20 * 256);
                excelSheet.SetColumnWidth(8, 20 * 256);
                //reprot info

                IFont fontTitle = workbook.CreateFont();
                fontTitle.IsBold = true;
                fontTitle.FontHeightInPoints = 17;

                excelSheet.CreateFreezePane(0, 10, 0, 10);

                //Search info
                IRow s_row = excelSheet.CreateRow(8);

                //header table
                //header style
                IFont font1 = workbook.CreateFont();
                font1.IsBold = true;
                font1.Color = HSSFColor.White.Index;
                font1.FontHeightInPoints = 13;
                ICellStyle style1 = workbook.CreateCellStyle();
                style1.SetFont(font1);
                style1.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Green.Index;
                style1.FillPattern = FillPattern.SolidForeground;
                IRow header = excelSheet.CreateRow(0);
                string[] head_titles = { "Body", "#Segments", "Date Sent", "From", "To", "Status", "Price" };
                for (int i = 0; i < head_titles.Length; i++)
                {
                    ICell c = header.CreateCell(i); c.SetCellValue(head_titles[i]);
                    c.CellStyle = style1;

                }

                int row_num = 1;


                foreach (var message in smsData)
                {
                    IRow row_next_1 = excelSheet.CreateRow(row_num);
                    row_next_1.CreateCell(0).SetCellValue(message.Body);
                    row_next_1.CreateCell(1).SetCellValue(message.NumSegments);
                    row_next_1.CreateCell(2).SetCellValue(message.DateSent);
                    row_next_1.CreateCell(3).SetCellValue(message.From);
                    row_next_1.CreateCell(4).SetCellValue(message.To);
                    row_next_1.CreateCell(5).SetCellValue(message.Status);
                    row_next_1.CreateCell(6).SetCellValue(message.Price);
                    row_num++;
                }

                workbook.Write(fs);
            }
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                fileStream.CopyTo(memoryStream);
                fileStream.Close();
            }
            memoryStream.Position = 0;
            var db = new WebDataModel();
            var fileExport = db.SMSExportFiles.Find(smsExportFiles.Id);
            fileExport.DoneDate = DateTime.UtcNow;
            db.SaveChanges();
          
            using(var hubContext = new SmsexportfileHub())
            {
                hubContext.completeExportFiles();
            }
             
        }

        [HttpPost]
        public ActionResult ExportFile(SMSReportSearchModel filter)
        {
            try
            {


                P_Member cMem = Authority.GetCurrentMember();
                var db = new WebDataModel();
                var reportFile = new SMSExportFile();
                reportFile.CreateAt = DateTime.UtcNow;
                reportFile.CreateBy = cMem.FullName;
                reportFile.MemberNumber = cMem.MemberNumber;
                reportFile.Name = "SMS Report: " + filter.DateSentAfter.Value.ToString("MMM dd, yyyy") + " - " + filter.DateSentBefore.Value.ToString("MMM dd, yyyy");
                DirectoryInfo d = new DirectoryInfo(Server.MapPath("/upload/smsreport/"));
                if (!d.Exists)
                {
                    d.Create();
                }
                reportFile.Path = "/upload/smsreport/" + DateTime.UtcNow.ToString("yyyyMMddhhmmssfff") + ".xlsx";
                db.SMSExportFiles.Add(reportFile);
                db.SaveChanges();
                reportFile.HangfireJobId = BackgroundJob.Enqueue<OtherReportController>(x => x.ProcessSMSExportFile(filter, reportFile));
                db.SaveChanges();

                return Json(new { status = true, message = "Create job exprort excel success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }

        }
        [HttpPost]
        public async Task<ActionResult> LoadDataSMSReport(SMSReportSearchModel filter, string Url)
        {

            int Page = 1;
            var _smsService = new NotificationService();
            if (filter.DateSentAfter.HasValue)
            {
                filter.DateSentAfter = filter.DateSentAfter.Value.Date + new TimeSpan(0, 0, 0);
                filter.DateSentAfter = filter.DateSentAfter.Value.IMSToUTCDateTime();
            }
            if (filter.DateSentBefore.HasValue)
            {
                filter.DateSentBefore = filter.DateSentBefore.Value.Date + new TimeSpan(23, 59, 59);
                filter.DateSentBefore = filter.DateSentBefore.Value.IMSToUTCDateTime();
            }
            if (string.IsNullOrEmpty(Url))
            {
               
                var listMessage = await _smsService.GetMessage(FromDate: filter.DateSentAfter, ToDate: filter.DateSentBefore, FromPhone: filter.FromPhone, ToPhone: filter.ToPhone, Body: filter.Body, NumSegments: filter.NumSegments);
                var data = listMessage.Records.Select(x => new
                {
                    x.Body,
                    x.NumSegments,
                    DateSent = x.DateSent.HasValue? x.DateSent.Value.ToUniversalTime().UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt"):"",
                    DateCreated = x.DateCreated.HasValue ? x.DateCreated.Value.ToUniversalTime().UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt") : "",
                    From = x.From.ToString(),
                    x.To,
                    Status = x.Status.ToString(),
                    x.Price,
                    x.PriceUnit,
                    x.ErrorMessage
                });
                string prevPage = listMessage.GetPreviousPageUrl(Twilio.Rest.Domain.Api);
                string nextPage = listMessage.GetNextPageUrl(Twilio.Rest.Domain.Api);
                string firstPage = listMessage.GetFirstPageUrl(Twilio.Rest.Domain.Api);
                return Json(new
                {
                    recordsFiltered = data.Count(),
                    recordsTotal = data.Count(),
                    data = data,
                    prevPage,
                    hasNextPage = listMessage.HasNextPage(),
                    nextPage,
                    firstPage,
                });
            }
            else
            {

                var listMessage = await _smsService.GetMessage(Url);
                Uri myUri = new Uri(Url);
                Page = int.Parse(HttpUtility.ParseQueryString(myUri.Query).Get("Page")) + 1;
                var data = listMessage.Records.Select(x => new
                {
                    x.Body,
                    x.NumSegments,
                    DateSent = x.DateSent.HasValue ? x.DateSent.Value.ToUniversalTime().UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt") : "",
                    DateCreated = x.DateCreated.HasValue ? x.DateCreated.Value.ToUniversalTime().UtcToIMSDateTime().ToString("MMM dd, yyyy hh:mm tt") : "",
                    From = x.From.ToString(),
                    x.To,
                    Status = x.Status.ToString(),
                    x.Price,
                    x.PriceUnit,
                    x.ErrorMessage
                });
                string prevPage = listMessage.GetPreviousPageUrl(Twilio.Rest.Domain.Api);
                string nextPage = listMessage.GetNextPageUrl(Twilio.Rest.Domain.Api);
                string firstPage = listMessage.GetFirstPageUrl(Twilio.Rest.Domain.Api);
                return Json(new
                {
                    recordsFiltered = data.Count(),
                    recordsTotal = data.Count(),
                    data = data,
                    prevPage,
                    nextPage,
                    hasNextPage = listMessage.HasNextPage(),
                    firstPage,
                    page = Page,
                });
            }
        }
        [HttpPost]
        public ActionResult LoadExportFiles(IDataTablesRequest dataTablesRequest)
        {

            var db = new WebDataModel();
            var query = db.SMSExportFiles;
            var recordsFiltered = query.Count();
            var files = query.OrderByDescending(x => x.CreateAt).Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList();

            var data = files.Select(x => new
            {
                x.Id,
                x.DoneDate,
                x.MemberNumber,
                CreateAt = String.Format("{0:r}", x.CreateAt),
                Done = x.DoneDate != null ? true : false,
                x.CreateBy,
                x.Path,
                x.Name
            });

            return Json(new
            {
                recordsFiltered = recordsFiltered,
                recordsTotal = recordsFiltered,
                data = data,

            });
        }
        [HttpPost]
        public ActionResult DeleteSmsReportFile(int? Id)
        {
            try
            {
                var db = new WebDataModel();
                var file = db.SMSExportFiles.Find(Id);
                if (file != null)
                {
                    db.SMSExportFiles.Remove(file);
                    db.SaveChanges();
                }
                return Json(new { status = true, message = "delete success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        public ActionResult SMSUsedReport(string storeCode, DateTime? fromDate, DateTime? toDate)
        {
            if (access.Any(k => k.Key.Equals("report_sms_used")) == false || access["report_sms_used"] == false)
            {
                return RedirectToAction("Forbidden", "home");
            }
            ViewBag.fromDate = fromDate.HasValue ? fromDate.Value.ToString("MM/dd/yyyy") : DateTime.UtcNow.AddMonths(-1).ToString("MM/dd/yyyy");
            ViewBag.toDate = toDate.HasValue ? toDate.Value.ToString("MM/dd/yyyy") : DateTime.UtcNow.ToString("MM/dd/yyyy");
            ViewBag.storeCode = storeCode;
            return View();
        }

        [HttpPost]
        public ActionResult LoadDataSMSUsedReport(IDataTablesRequest dataTablesRequest, string storeCode, DateTime? fromDate, DateTime? toDate)
        {
            var db = new WebDataModel();
            string url = string.Empty;
            string message = string.Empty;
            int recordsTotal = 0;
            if (string.IsNullOrEmpty(storeCode))
            {
                url = AppConfig.Cfg.ReportSMSusedlist() +
                    $"fromDate={fromDate.Value.ToString("MM/dd/yyyy")}&toDate={toDate.Value.ToString("MM/dd/yyyy")}&page={dataTablesRequest.Start / dataTablesRequest.Length}&limit={dataTablesRequest.Length}";
            }
            else
            {
                url = AppConfig.Cfg.ReportSMSusedstore() +
                    $"storeId={storeCode}&fromDate={fromDate.Value.ToString("MM/dd/yyyy")}&toDate={toDate.Value.ToString("MM/dd/yyyy")}&page={dataTablesRequest.Start / dataTablesRequest.Length}&limit={dataTablesRequest.Length}";
            }
            HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "get", null);
            if (result?.IsSuccessStatusCode == true)
            {
                string responseJson = result.Content.ReadAsStringAsync().Result;
                ResponeApiSMSUsedReport responeData = JsonConvert.DeserializeObject<ResponeApiSMSUsedReport>(responseJson);
                if (responeData.status.Equals(200) && responeData.message.Equals("success"))
                {
                    recordsTotal = responeData.data.total;
                    responeData.data.list.ForEach(o => { o.storeId = string.IsNullOrEmpty(o.storeId) ? storeCode : o.storeId; });
                    var data = (from s in responeData.data.list
                            join c in db.C_Customer
                            on s.storeId equals c.StoreCode into cus
                            from c in cus.DefaultIfEmpty()
                            select new { s, c?.BusinessName }).ToList();

                    return Json(new
                    {
                        recordsFiltered = recordsTotal,
                        recordsTotal = recordsTotal,
                        data = data
                    });
                }
            }
            else
            {
                message = "SIMPLY POS system not responding!";
            }
           
            return Json(new
            {
                recordsFiltered = recordsTotal,
                recordsTotal = recordsTotal,
                message = message,
                data = new List<string> { }
            });
        }
    }
}