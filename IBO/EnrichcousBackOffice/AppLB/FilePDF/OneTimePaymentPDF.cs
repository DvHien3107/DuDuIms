using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using EnrichcousBackOffice.Controllers;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using iTextSharp.text.pdf;

namespace EnrichcousBackOffice.AppLB.FilePDF
{
    public class OneTimePaymentPDF
    {
        
        public static string CreateOneTimePayment(string merchantCode, string orderCode)
        {
            try
            {
                WebDataModel db = new WebDataModel();
                P_Member cMem = Authority.GetCurrentMember();
                string name = "One time payment (order #" + orderCode + ")";

                var merchant = db.C_Customer.Where(c => c.CustomerCode == merchantCode).FirstOrDefault();
                var order = db.O_Orders.Where(o => o.OrdersCode == orderCode && o.CustomerCode == merchantCode).FirstOrDefault();
                var MID = db.C_MerchantSubscribe.Where(m => m.CustomerCode == merchantCode).FirstOrDefault()?.MerchantID;

                if (merchant != null && order != null)
                {
                    #region check path
                    DirectoryInfo d = new DirectoryInfo(HttpContext.Current.Server.MapPath("~/upload/merchant/Merchant_" + merchantCode));
                    if (!d.Exists)
                    {
                        d.Create();
                    }
                    #endregion

                    string pdfTemplate = HttpContext.Current.Server.MapPath("/upload/merchant/template/Enrich_Finance_OneTime_Payment.pdf");
                    string fileName = "Enrich_Finance_OneTime_Payment_" + merchant.CustomerCode + "_" + orderCode + ".pdf";

                    //Neu pdf chua ky thi cho ghi de | Neu pdf da ky thi phai tao 1 pdf copy moi
                    var list_merchant_form = db.O_MerchantForm.Where(mf => mf.MerchantCode == merchantCode && mf.OrderId == order.Id && mf.TerminalId == null).ToList();
                    if (list_merchant_form != null && list_merchant_form.Count > 0)
                    {
                        if (list_merchant_form.Any(x => x.Signed == true) == true)//Tat ca pdf trong ds da dc ky
                        {
                            fileName = "Enrich_Finance_OneTime_Payment_" + merchant.CustomerCode + "_" + orderCode + "_copy" + list_merchant_form.Count + ".pdf";
                        }
                    }

                    string newFile = Path.Combine(HttpContext.Current.Server.MapPath("~/upload/merchant/Merchant_" + merchantCode + "/" + fileName));

                    #region Form Fields & Create PDF
                    PdfReader pdfReader = new PdfReader(pdfTemplate);
                    PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));
                    AcroFields pdfFormFields = pdfStamper.AcroFields;
                    string today = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                    //set form pdfFormFields
                    #region Page 1
                    pdfFormFields.SetField("Name of owner", CommonFunc.ConvertNonUnicodeURL(merchant.OwnerName, true));
                    pdfFormFields.SetField("Amount in number", order.GrandTotal?.ToString("#,##0.#0"));
                    pdfFormFields.SetField("Amount in words", " " + CommonFunc.SpellMoney.SpellOutAmount_USD((double)(order.GrandTotal ?? 0)));
                    pdfFormFields.SetField("DD", DateTime.Today.ToString("dd"));
                    pdfFormFields.SetField("MM", DateTime.Today.ToString("MM"));
                    pdfFormFields.SetField("YYYY", DateTime.Today.ToString("yyyy"));
                    pdfFormFields.SetField("INV #", " #" + order.OrdersCode);

                    // var bank_info = db.C_CustomerCredit.Where(b => b.CustomerCode == merchantCode).FirstOrDefault();
                    string city_state_zip = CommonFunc.ConvertNonUnicodeURL(merchant.BusinessCity + ", " + merchant.BusinessState + " " + merchant.BusinessZipCode, true);
                    pdfFormFields.SetField("Routing", merchant.DepositRoutingNumber ?? "");
                    pdfFormFields.SetField("Street Address", CommonFunc.ConvertNonUnicodeURL(merchant.BusinessAddressStreet, true));
                    pdfFormFields.SetField("Account", merchant.DepositAccountNumber);
                    pdfFormFields.SetField("City, State ZIP", city_state_zip);
                    pdfFormFields.SetField("Bank Name", CommonFunc.ConvertNonUnicodeURL(merchant.DepositBankName, true));
                    pdfFormFields.SetField("Cellular Number", merchant.BusinessPhone ?? merchant.OwnerMobile);
                    pdfFormFields.SetField("Email Address", merchant.BusinessEmail ?? merchant.Email);

                    pdfFormFields.SetField("DD MM YYYY HH:MM:SS", today);
                    pdfFormFields.SetField("MID", MID ?? "");
                    pdfFormFields.SetField("DBA", CommonFunc.ConvertNonUnicodeURL(merchant.BusinessName, true));
                    #endregion

                    #region Page 2
                    pdfFormFields.SetField("Name of owner 1", CommonFunc.ConvertNonUnicodeURL(merchant.OwnerName, true));
                    //pdfFormFields.SetField("Amount in number 1", order.Service_MonthlyFee?.ToString("#,##0.#0"));
                    //pdfFormFields.SetField("Amount in words 1", CommonFunc.SpellMoney.SpellOutAmount_USD((double)(order.Service_MonthlyFee ?? 0)));
                    pdfFormFields.SetField("DD 1", DateTime.Today.ToString("dd"));
                    pdfFormFields.SetField("MM 1", DateTime.Today.ToString("MM"));
                    pdfFormFields.SetField("YYYY 1", DateTime.Today.ToString("yyyy"));
                    // pdfFormFields.SetField("Service plan", db.O_Orders_Service.Where(x => x.OrdersCode == orderCode && x.Monthly == 1).FirstOrDefault()?.ServicePlan ?? "");

                    pdfFormFields.SetField("Rounting 1", merchant.DepositRoutingNumber ?? "");
                    pdfFormFields.SetField("Street Address 1", CommonFunc.ConvertNonUnicodeURL(merchant.BusinessAddressStreet, true));
                    pdfFormFields.SetField("Account 1", merchant.DepositAccountNumber);
                    pdfFormFields.SetField("City, State ZIP 1", city_state_zip);
                    pdfFormFields.SetField("Bank Name 1", CommonFunc.ConvertNonUnicodeURL(merchant.DepositBankName, true));
                    pdfFormFields.SetField("Cellular Number 1", merchant.BusinessPhone ?? merchant.OwnerMobile);
                    pdfFormFields.SetField("Email Address 1", merchant.BusinessEmail ?? merchant.Email);

                    pdfFormFields.SetField("DD MM YYYY HH:MM:SS 1", today);
                    pdfFormFields.SetField("MID 1", MID ?? "");
                    pdfFormFields.SetField("DBA 1", CommonFunc.ConvertNonUnicodeURL(merchant.BusinessName, true));
                    #endregion

                    #region Page 3
                    pdfFormFields.SetField("[Phone]", String.Format("{0:(###) ####-####}", double.Parse(db.SystemConfigurations.FirstOrDefault()?.CompanyHotline)));
                    pdfFormFields.SetField("DD MMM YYYY", DateTime.Today.ToString("dd MMM yyyy"));
                    pdfFormFields.SetField("Invoice #", orderCode ?? "");
                    pdfFormFields.SetField("Customer ID #", merchantCode ?? "");

                    pdfFormFields.SetField("[Owner Name]", CommonFunc.ConvertNonUnicodeURL(merchant.OwnerName ?? merchant.ContactName, true));
                    pdfFormFields.SetField("[Give option same as Billing, home, or other]", CommonFunc.ConvertNonUnicodeURL(order.ShippingAddress.Replace("|", ", "), true));
                    pdfFormFields.SetField("[Legal Business Name DBA as Salon Name]", CommonFunc.ConvertNonUnicodeURL(merchant.BusinessName, true));
                    pdfFormFields.SetField("[Address 1, Address 2]", CommonFunc.ConvertNonUnicodeURL(merchant.BusinessAddressStreet, true));
                    pdfFormFields.SetField("[City, State ZIP]", city_state_zip);

                    pdfFormFields.SetField("[SALES PERSON]", order.SalesMemberNumber != null ? CommonFunc.ConvertNonUnicodeURL(order.SalesName + " (#" + order.SalesMemberNumber + ")", true) : "");
                    pdfFormFields.SetField("[PO NUMBER]", "#" + order.OrdersCode ?? "");
                    pdfFormFields.SetField("[SHIP DATE]", order.ShippingDate != null ? order.ShippingDate?.ToString("dd/MM/yyyy") : "");
                    pdfFormFields.SetField("[SHIP VIA]", order.ShipVIA ?? "");
                    pdfFormFields.SetField("[TRACKING]", order.Status ?? "");

                    //Fields list device service
                    var list_device_service = new List<Device_Service_ModelCustomize>();

                    #region Get list order_service & order_device
                    //get list order_device
                    var list_product = db.Order_Products.Where(p => p.OrderCode == orderCode).ToList();
                    foreach (var item in list_product)
                    {
                        var dv = new Device_Service_ModelCustomize();
                        dv.Key = item.Id;
                        dv.Type = "Device";
                        dv.ProductCode = item.ProductCode;
                        dv.ProductName = item.ProductName;
                        dv.Quantity = item.Quantity ?? 0;
                        dv.Amount = item.TotalAmount ?? 0;
                        dv.Price = item.Price ?? 0;
                        dv.Feature = item.Feature;
                        dv.ModelCode = item.ModelCode;
                        if (item.BundleId > 0)
                        {
                            dv.BundleId = item.BundleId;
                            dv.BundleName = db.I_Bundle.Find(item.BundleId)?.Name;
                        }

                        var _pro = db.O_Product.Where(p => p.Code == dv.ProductCode).FirstOrDefault();
                        dv.Picture = _pro?.Picture;
                        list_device_service.Add(dv);
                    }

                    /*//get list order_service
                    var list_sv = db.Orders_Service.Where(s => s.OrdersCode == orderCode).ToList();
                    foreach (var item in list_sv)
                    {
                        var sv = new Device_Service_ModelCustomize();
                        sv.Key = item.Id;
                        sv.Type = "Service";
                        sv.ServiceName = item.ServiceName;
                        sv.ServiceCode = item.ServiceCode;
                        sv.Quantity = 1;
                        sv.SetupFee = item.SetupFee;
                        sv.MonthlyFee = item.MonthlyFee;
                        sv.Amount = item.TotalAmount ?? 0;
                        sv.ServicePlan = item.ServicePlan;
                        sv.StartDate = item.StartDate?.ToString("MM/dd/yyyy");
                        sv.Picture = "/Upload/Img/no_image.jpg";
                        list_device_service.Add(sv);
                    }*/


                    #endregion

                    for (int i = 0; i < list_device_service.Count; i++)
                    {
                        pdfFormFields.SetField("ITEM " + i, list_device_service[i].ModelCode);

                        if (list_device_service[i].Type == "Device")
                        {
                            string des = "";
                            if (string.IsNullOrEmpty(list_device_service[i].BundleName) == false)
                            {
                                des = " - Pakage: " + list_device_service[i].BundleName;
                            }
                            else
                            {
                                des = string.IsNullOrEmpty(list_device_service[i].Feature) == true ? "" : " (" + list_device_service[i].Feature + ")";
                            }


                            pdfFormFields.SetField("DES " + i, list_device_service[i].ProductName + des);
                            pdfFormFields.SetField("QTY " + i, list_device_service[i].Quantity.ToString());
                            pdfFormFields.SetField("UNIT PRICE " + i, list_device_service[i].Price.ToString("$#,##0.##"));
                            pdfFormFields.SetField("TOTAL " + i, (list_device_service[i].Price * list_device_service[i].Quantity).ToString("$#,##0.##"));
                        }
                        //else
                        //{
                        //    pdfFormFields.SetField("DES " + i, list_device_service[i].ServiceName + " (Setup fee: " + list_device_service[i].SetupFee?.ToString("$#,##0.##") + ")");
                        //    pdfFormFields.SetField("QTY " + i, list_device_service[i].ServicePlan);
                        //    pdfFormFields.SetField("UNIT PRICE " + i, list_device_service[i].MonthlyFee?.ToString("$#,##0.##") + "/month");
                        //    pdfFormFields.SetField("TOTAL " + i, (list_device_service[i].MonthlyFee + list_device_service[i].SetupFee)?.ToString("$#,##0.##"));
                        //}
                    }

                    pdfFormFields.SetField("COMMENT", order.Comment ?? "");
                    //pdfFormFields.SetField("SUBTOTAL", order.TotalAmount != null ? order.TotalAmount?.ToString("$#,##0.##") : "$0");
                    pdfFormFields.SetField("TAX RATE", order.TaxRate != null ? order.TaxRate?.ToString("#,##0") + "%" : "0%");
                    pdfFormFields.SetField("TAX", "$0");
                    pdfFormFields.SetField("SHIPPING", order.ShippingFee != null ? order.ShippingFee?.ToString("$#,##0.##") : "$0");
                    pdfFormFields.SetField("OTHER", order.OtherFee != null ? order.OtherFee?.ToString("$#,##0.##") : "$0");
                    pdfFormFields.SetField("TOTAL", order.GrandTotal != null ? order.GrandTotal?.ToString("$#,##0.##") : "$0");

                    pdfFormFields.SetField("DD MM YYYY HH:MM:SS 2", today);
                    pdfFormFields.SetField("MID 2", MID ?? "");
                    pdfFormFields.SetField("DBA 2", merchant.BusinessName ?? "");
                    #endregion

                    pdfStamper.FormFlattening = true; //true:read_only|false:edit
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
                            OrderId = order.Id
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
            catch (Exception)
            {
                return "";
            }
        }
    }
}