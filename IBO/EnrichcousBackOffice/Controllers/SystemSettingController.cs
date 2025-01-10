using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class SystemSettingController : UploadController
    {
        // GET: SystemSetting
        public ActionResult Index()
        {
            WebDataModel db = new WebDataModel();
            var company_info = db.SystemConfigurations.FirstOrDefault();
            return View(company_info);
        }

        /// <summary>
        /// Save company info
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Save(SystemConfiguration sysconfig)
        {
            try
            {
                if (AppLB.Authority.GetCurrentMember().RoleCode.Contains("admin") == false)
                {
                    return Redirect("/home/forbidden");
                }


                WebDataModel db = new WebDataModel();
                var company_info = db.SystemConfigurations.Find(sysconfig.Id);
                string picture = string.Empty;
                string deleteFileFlag = Request["hdPicDelete_pic0"];
                UploadAttachFile("/upload/img", "pic0", "", "company_logo.png", out picture);
                
                if (deleteFileFlag == "1" || string.IsNullOrWhiteSpace(picture) == false)
                {
                    company_info.Logo = picture;
                }

                picture = string.Empty;
                deleteFileFlag = Request["hdPicDelete_pic1"];
                UploadAttachFile("/upload/img", "pic1", "", "product_logo.png", out picture);
                if (deleteFileFlag == "1" || string.IsNullOrWhiteSpace(picture) == false)
                {
                    company_info.ProductLogo = picture;
                }
                

                string CompanyAddress = string.Join("|", Request["StreetAddress"]??"", Request["City"]??"", Request["State"]??"", Request["Zipcode"]??"", Request["Country"]??"");
                string shipper_address = string.Join("|", Request["Shipper_StreetAddress"] ?? "", Request["Shipper_City"] ?? "", Request["Shipper_State"] ?? "", Request["Shipper_Zipcode"] ?? "", Request["Shipper_Country"] ?? "");
                company_info.CompanyName = sysconfig.CompanyName;
                company_info.CompanyAddress = CompanyAddress;
                company_info.CompanyHotline = sysconfig.CompanyHotline;
                company_info.CompanySupportNumber = sysconfig.CompanySupportNumber;
                company_info.CompanyFax = sysconfig.CompanyFax;
                company_info.NotificationEmail = sysconfig.NotificationEmail;
                company_info.AdminEmail = sysconfig.AdminEmail;
                company_info.SupportEmail = sysconfig.SupportEmail;
                company_info.SalesEmail = sysconfig.SalesEmail;
                company_info.HREmail = sysconfig.HREmail;
                bool timezoneIsChange = company_info.SystemTimezoneId != sysconfig.SystemTimezoneId;
                company_info.SystemTimezoneId = sysconfig.SystemTimezoneId;
                company_info.BillingNotification = sysconfig.BillingNotification;
                company_info.EnableSendRecurringEmailToBilling = sysconfig.EnableSendRecurringEmailToBilling??false;
                company_info.ProductName = sysconfig.ProductName;
                company_info.UPS_ShipAttentionName = sysconfig.UPS_ShipAttentionName;
                company_info.UPS_AccessLicenseNumber = sysconfig.UPS_AccessLicenseNumber;
                company_info.UPS_AccountName = sysconfig.UPS_AccountName;
                company_info.UPS_AccountNumber = sysconfig.UPS_AccountNumber;
                company_info.UPS_AccountPassword = sysconfig.UPS_AccountPassword;
                company_info.UPS_ShipperAccountNumber = sysconfig.UPS_ShipperAccountNumber;
                company_info.UPS_ShipperAddress = shipper_address;
                company_info.UPS_ShipperCellPhone = sysconfig.UPS_ShipperCellPhone;
                company_info.AutoActiveRecurringLicense = sysconfig.AutoActiveRecurringLicense==true;
                company_info.ActiveOnboardingTicket = sysconfig.ActiveOnboardingTicket == true;
                // set active onboarding type
                var type_Onboarding = db.T_TicketType.Where(x => x.BuildInCode == "Onboarding_Ticket").FirstOrDefault();
                if (type_Onboarding != null)
                {
                    type_Onboarding.Active = company_info.ActiveOnboardingTicket;
                }
                company_info.NotificationBeforeExpiration = sysconfig.NotificationBeforeExpiration;
                company_info.RecurringBeforeDueDate = sysconfig.RecurringBeforeDueDate;
                company_info.MerchantPasswordDefault = sysconfig.MerchantPasswordDefault;
                company_info.ExtensionDay = sysconfig.ExtensionDay;
              
                db.Entry(company_info).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                if (timezoneIsChange)
                {
                    TimezoneHelper.ReloadTimezone();
                }
                TempData["success"] = "Save success!";
            }
            catch (Exception ex)
            {
                TempData["error"] = "Error: " + ex.Message;
            }

            return RedirectToAction("index");
        }
    }
}