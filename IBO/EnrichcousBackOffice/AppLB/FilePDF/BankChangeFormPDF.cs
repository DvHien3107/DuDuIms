using System;
using EnrichcousBackOffice.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text.pdf;
using System.IO;

namespace EnrichcousBackOffice.AppLB.FilePDF
{
    public class BankChangeFormPDF
    {
        public static string CreateBankChangeForm(string merchantCode)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                P_Member cMem = Authority.GetCurrentMember();
                string name = "Bank change form";

                var merchant = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();
                if (merchant != null)
                {
                    #region check path
                    DirectoryInfo d = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/upload/merchant/Merchant_" + merchantCode));
                    if (!d.Exists)
                    {
                        d.Create();
                    }
                    #endregion
                    
                    string pdfTemplate = HttpContext.Current.Server.MapPath("/upload/merchant/template/Bank_Change_Form.pdf");
                    string fileName = "Bank_Change_Form_" + merchantCode + ".pdf";

                    //Neu pdf chua ky thi cho ghi de | Neu pdf da ky thi phai tao 1 pdf copy moi
                    var list_merchant_form = db.O_MerchantForm.Where(mf => mf.MerchantCode == merchantCode && mf.OrderId == null && mf.TerminalId == null).ToList();
                    if (list_merchant_form != null && list_merchant_form.Count > 0)
                    {
                        if (list_merchant_form.Any(x => x.Signed == true) == true)//Tat ca pdf trong ds da dc ky
                        {
                            fileName = "Bank_Change_Form_" + merchantCode + "_copy" + list_merchant_form.Count + ".pdf";
                        }
                    }

                    string newFile = Path.Combine(HttpContext.Current.Server.MapPath("~/upload/merchant/Merchant_" + merchantCode + "/" + fileName));

                    var MID = db.C_MerchantSubscribe.Where(m => m.CustomerCode == merchantCode).FirstOrDefault()?.MerchantID;

                    #region Form Fields & Create PDF
                    PdfReader pdfReader = new PdfReader(pdfTemplate);
                    PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
                    AcroFields pdfFormFields = pdfStamper.AcroFields;
                    //set form pdfFormFields
                    string address = CommonFunc.ConvertNonUnicodeURL(merchant.BusinessAddressStreet + ", " + merchant.BusinessCity + ", " + merchant.BusinessState + " " + merchant.BusinessZipCode + ", " + merchant.BusinessCountry, true);
                    pdfFormFields.SetField("Name of OwnerPrincipal", CommonFunc.ConvertNonUnicodeURL(merchant.OwnerName, true));
                    pdfFormFields.SetField("Company Name", CommonFunc.ConvertNonUnicodeURL(merchant.LegalName, true));
                    pdfFormFields.SetField("Address", address);
                    pdfFormFields.SetField("mm", DateTime.Today.ToString("MM"));
                    pdfFormFields.SetField("dd", DateTime.Today.ToString("dd"));
                    pdfFormFields.SetField("yyyy", DateTime.Today.ToString("yyyy"));
                    pdfFormFields.SetField("Merchant email required to confirm changes", merchant.BusinessEmail ?? merchant.Email);
                    pdfFormFields.SetField("Merchant ID", MID ?? "");

                    var bank_info = db.C_CustomerCard.FirstOrDefault(b => b.CustomerCode == merchantCode);
                    pdfFormFields.SetField("Account Holder Name", bank_info != null ? CommonFunc.ConvertNonUnicodeURL(bank_info.CardHolderName, true) : "");
                    pdfFormFields.SetField("RoutingABA Number", merchant.DepositRoutingNumber ?? "");
                    pdfFormFields.SetField("Account Number", bank_info?.CardNumber);
                    pdfFormFields.SetField("Bank Name", CommonFunc.ConvertNonUnicodeURL(merchant.DepositBankName, true));
                    pdfFormFields.SetField("Account Type", bank_info?.CardType ?? "");

                    pdfFormFields.SetField("Check Box1.0", "false");
                    pdfFormFields.SetField("Check Box1.1", "false");
                    
                    pdfStamper.FormFlattening = false; //true:read_only|false:edit
                    //close the pdf  
                    pdfStamper.Close();
                    #endregion


                    #region Save Database Merchant Form
                    string file_id = "";
                    string file_name = "";
                    string file_path = "";

                    var merchant_form = (from m in db.O_MerchantForm.AsEnumerable()
                                         where m.MerchantCode == merchantCode
                                         && m.PDF_URL?.Split('/')[4] == fileName
                                         select m).FirstOrDefault();

                    if (merchant_form != null)
                    {
                        merchant_form.UpdateBy = cMem.FullName;
                        merchant_form.UpdateAt = DateTime.UtcNow;
                        db.Entry(merchant_form).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        file_id = merchant_form.Id.ToString() ?? "";
                        file_name = merchant_form.TemplateName;
                        file_path = merchant_form.PDF_URL;
                    }
                    else
                    {
                        var new_merchant_form = new O_MerchantForm
                        {
                            Id = long.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfff")),
                            MerchantCode = merchantCode,
                            MerchantName = merchant.BusinessName,
                            TemplateName = name,
                            PDF_URL = "/upload/merchant/Merchant_" + merchantCode + "/" + fileName,
                            CreateBy = cMem.FullName,
                            CreateAt = DateTime.UtcNow,
                            Status = "uploaded",
                        };
                        db.O_MerchantForm.Add(new_merchant_form);
                        db.SaveChanges();
                        file_id = new_merchant_form.Id.ToString() ?? "";
                        file_name = new_merchant_form.TemplateName;
                        file_path = new_merchant_form.PDF_URL;
                    }
                    
                    #endregion

                    return file_id + "|" + file_name + "|" + file_path;
                }
                return "";
            }
            catch
            {
                return "";
            }
        }
    }
}