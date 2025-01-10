using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using EnrichcousBackOffice.Models;
using iTextSharp.text.pdf;
using System.Web.Mvc;

namespace EnrichcousBackOffice.AppLB.FilePDF
{
    public class DejavooPDF
    {
        public static Dejavo_Datasheet GetDejavooInfo(string customerCode, string orderCode, long? terminalId)
        {
            WebDataModel db = new WebDataModel();
            var dejavoo = db.Dejavo_Datasheet.Where(x => x.OrdersCode == orderCode && x.TerminalID == terminalId.ToString()).FirstOrDefault();

            if (dejavoo == null)
            {
                var merchant = db.C_Customer.Where(c => c.CustomerCode == customerCode).FirstOrDefault();
                if (merchant != null)
                {
                    var new_dejavoo = new Dejavo_Datasheet();
                    new_dejavoo.M_Name = merchant.BusinessName;
                    new_dejavoo.M_Country = merchant.BusinessCountry;
                    new_dejavoo.M_Address = merchant.BusinessAddress;
                    new_dejavoo.M_City = merchant.BusinessCity;
                    new_dejavoo.M_State = merchant.BusinessState;
                    new_dejavoo.M_ZipCode = merchant.BusinessZipCode;
                    new_dejavoo.M_Telephone = merchant.BusinessPhone;
                    new_dejavoo.Software = "DEJAVOO Z11";
                    new_dejavoo.TerminalID = terminalId?.ToString();
                    new_dejavoo.TerminalNumber = db.O_Device.Find(terminalId)?.InvNumber;
                    new_dejavoo.OrdersCode = orderCode;

                    return new_dejavoo;
                }
            }

            return dejavoo;
        }

        public static string CreateDejavooPDF(Dejavo_Datasheet dejavoModel)
        {
            WebDataModel db = new WebDataModel();
            using (var TranS = db.Database.BeginTransaction())
            {
                try
                {
                    //File name dejavoo: [OrderCode_TerminalSeriNumber.pdf]
                    P_Member cMem = Authority.GetCurrentMember();
                    
                    string merchantCode = db.O_Orders.Where(o => o.OrdersCode == dejavoModel.OrdersCode).FirstOrDefault()?.CustomerCode;
                    string fileName = "DEJAVOO_Z11_DATA_SHEET_" + merchantCode + "_" + dejavoModel.OrdersCode + "_" + dejavoModel.TerminalNumber + ".pdf";
                    string name = "DEJAVOO Z11 DATA SHEET  (order #" + dejavoModel.OrdersCode + " - Inv #" + dejavoModel.TerminalNumber + ")";

                    #region check path
                    DirectoryInfo d = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/upload/merchant/Merchant_" + merchantCode));
                    if (!d.Exists)
                    {
                        d.Create();
                    }
                    #endregion

                    string pdfTemplate = HttpContext.Current.Server.MapPath("/upload/merchant/template/DEJAVOO_Z11_DATA_SHEET.pdf");
                    string newFile = Path.Combine(HttpContext.Current.Server.MapPath("~/upload/merchant/Merchant_" + merchantCode + "/" + fileName));

                    #region Form Fields & Create PDF
                    PdfReader pdfReader = new PdfReader(pdfTemplate);
                    PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
                    AcroFields pdfFormFields = pdfStamper.AcroFields;
                    //set form pdfFormFields
                    string city_state_zipcode = (dejavoModel.M_City != null ? dejavoModel.M_City + ", " : "") + (dejavoModel.M_State != null ? dejavoModel.M_State + ", " : "") + (dejavoModel.M_ZipCode != null ? dejavoModel.M_ZipCode : "");
                    pdfFormFields.SetField("Name", dejavoModel.M_Name);
                    pdfFormFields.SetField("Address", dejavoModel.M_Address);
                    pdfFormFields.SetField("City State Zip", city_state_zipcode);
                    pdfFormFields.SetField("Telephone", dejavoModel.M_Telephone);

                    pdfFormFields.SetField("Software", dejavoModel.Software);
                    pdfFormFields.SetField("Acquirer Bin", dejavoModel.AcquirerBin);
                    pdfFormFields.SetField("POS Merchant Number", dejavoModel.POS_MerchantNumber);
                    pdfFormFields.SetField("Terminal ID", dejavoModel.TerminalID);
                    pdfFormFields.SetField("Store Number", dejavoModel.StoreNumber);
                    pdfFormFields.SetField("Terminal Number", dejavoModel.TerminalNumber);
                    pdfFormFields.SetField("Merchant Location Number", dejavoModel.M_LocationNumber);
                    pdfFormFields.SetField("Merchant Category Code", dejavoModel.M_CategoryCode);
                    pdfFormFields.SetField("Agent Number", dejavoModel.AgentNumber);
                    pdfFormFields.SetField("Chain Number", dejavoModel.ChainNumber);
                    pdfFormFields.SetField("Merchant City Code", dejavoModel.M_ZipCode);
                    pdfFormFields.SetField("Merchant Name", dejavoModel.M_Name);
                    pdfFormFields.SetField("Merchant Location", dejavoModel.M_City);
                    pdfFormFields.SetField("Merchant State", dejavoModel.M_State);
                    pdfFormFields.SetField("Time Zone Difference", dejavoModel.TimeZoneDifference);

                    pdfFormFields.SetField("Acquirer Bin", dejavoModel.AcquirerBin);
                    pdfFormFields.SetField("ABA", dejavoModel.ABA);
                    pdfFormFields.SetField("FIID", dejavoModel.FIID);
                    pdfFormFields.SetField("Sharing Group", dejavoModel.SharingGroup);
                    pdfFormFields.SetField("Reimbursement Attribute", dejavoModel.RemibursementAttribute);

                    pdfFormFields.SetField("Authorization", dejavoModel.Authorization);
                    pdfFormFields.SetField("Settlement", dejavoModel.Settlement);
                    pdfFormFields.SetField("Voice Authorization", dejavoModel.Voice_Authorization);
                    pdfFormFields.SetField("URL", dejavoModel.URL);
                    pdfFormFields.SetField("PORT", dejavoModel.PORT);


                    pdfStamper.FormFlattening = true; //true:read_only|false:edit
                    //close the pdf  
                    pdfStamper.Close();
                    #endregion


                    #region Save Database Dejavoo
                    if (dejavoModel.Id > 0) //Update dejavoo
                    {
                        dejavoModel.UpdateBy = "By: " + cMem.FullName + "_At: " + DateTime.UtcNow.ToShortDateString() + "|";
                        db.Entry(dejavoModel).State = System.Data.Entity.EntityState.Modified;
                    }
                    else //Create dejavoo
                    {
                        var new_dejavo = new Dejavo_Datasheet();
                        new_dejavo = dejavoModel;
                        new_dejavo.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff"));
                        new_dejavo.CreateBy = cMem.FullName;
                        new_dejavo.PDF_Url = "/upload/merchant/Merchant_" + merchantCode + "/" + fileName;
                        new_dejavo.CreateAt = DateTime.UtcNow;
                        db.Dejavo_Datasheet.Add(new_dejavo);
                    }
                    #endregion


                    #region Save Database Merchant Form
                    string file_id = "";
                    string file_name = "";
                    string file_path = "";
                    string terminal_id = "";

                    var merchant_form = (from m in db.O_MerchantForm.AsEnumerable()
                                         where m.MerchantCode == merchantCode
                                         && m.PDF_URL?.Split('/')[4] == fileName
                                         select m).FirstOrDefault();
                    
                    if (merchant_form != null)
                    {
                        merchant_form.UpdateBy = cMem.FullName;
                        merchant_form.UpdateAt = DateTime.UtcNow;
                        merchant_form.TemplateName = "DEJAVOO Z11 DATA SHEET";
                        merchant_form.PDF_URL = "/upload/merchant/Merchant_" + merchantCode + "/" + fileName;
                        db.Entry(merchant_form).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        file_id = merchant_form.Id.ToString() ?? "";
                        file_name = merchant_form.TemplateName;
                        file_path = merchant_form.PDF_URL;
                        terminal_id = merchant_form.TerminalId?.ToString();
                    }
                    else
                    {
                        var new_merchant_form = new O_MerchantForm
                        {
                            Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                            MerchantCode = merchantCode,
                            MerchantName = dejavoModel.M_Name,
                            TemplateName = "DEJAVOO Z11 DATA SHEET",
                            PDF_URL = "/upload/merchant/Merchant_" + merchantCode + "/" + fileName,
                            CreateBy = cMem.FullName,
                            CreateAt = DateTime.UtcNow,
                            OrderId = db.O_Orders.Where(o => o.OrdersCode == dejavoModel.OrdersCode).FirstOrDefault()?.Id,
                            TerminalId = long.Parse(dejavoModel.TerminalID),
                            Status = "uploaded",
                        };
                        db.O_MerchantForm.Add(new_merchant_form);
                        db.SaveChanges();
                        file_id = new_merchant_form.Id.ToString() ?? "";
                        file_name = new_merchant_form.TemplateName;
                        file_path = new_merchant_form.PDF_URL;
                        terminal_id = new_merchant_form.TerminalId?.ToString();
                    }
                    
                    #endregion

                    db.SaveChanges();
                    TranS.Commit();
                    TranS.Dispose();
                    return file_name + "|" + merchantCode + "|" + dejavoModel.OrdersCode;
                }
                catch (Exception)
                {
                    TranS.Dispose();
                    return "";
                }
            }
        }
    }
}