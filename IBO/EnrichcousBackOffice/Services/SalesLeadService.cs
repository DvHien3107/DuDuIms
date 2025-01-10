using Enrich.Core.Infrastructure;
using Enrich.Core.Ultils;
using Enrich.DataTransfer;
using Enrich.IServices.Utils.Mailing;
using Enrich.IServices.Utils.SMS;
using Enrich.IServices.Utils.Universal;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewModel;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;
using System.Web.Configuration;
using System.Xml;

namespace EnrichcousBackOffice.Services
{
    public partial class SalesLeadService
    {
        private P_Member cMem = Authority.GetCurrentMember();

        #region send SendMailVerify
        //send email verify account
        public async Task<string> SendMailVerify(string email, string name, string phone, string link)
        {
            WebDataModel db = new WebDataModel();
            var sl = db.C_SalesLead.Where(x => x.L_Email == email).FirstOrDefault();

            var emailData = new SendGridEmailTemplateData.VerifyRegister()
            {
                name = name,
                link = link,
                number = phone,
                trial = sl?.L_Version == "Trial" ? "Free Trial " : ""
            };
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/verify_demo_register");
            var _mailingService = EngineContext.Current.Resolve<IMailingService>();
            string result = await _mailingService.SendBySendGridWithTemplate(email, name, node["template_id"].InnerText, "", emailData, "");
            
            if (sl != null)
            {
                sl.L_IsSendMail = true;
                sl.L_CreateTrialAt = DateTime.UtcNow;
                db.SaveChanges();
            }
            return result;
        }

        //send email Subscribe for sales person
        public async Task<string> SendMailSubscribe(string name, string phone, string mail)
        {
            WebDataModel db = new WebDataModel();
            var emailData = new SendGridEmailTemplateData.MerchantSubscribe()
            {
                name = name,
                phone = phone,
                email = mail,
                link = ConfigurationManager.AppSettings["IMSUrl"] + "/SaleLead"
            };
            var email = db.SystemConfigurations.FirstOrDefault().SalesEmail;

            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/merchant_subscribe_us");
            var _mailingService = EngineContext.Current.Resolve<IMailingService>();
            return await _mailingService.SendBySendGridWithTemplate(email, "", node["template_id"].InnerText, "", emailData, "");
        }

        //send email complete setup for store
        public async Task<string> SendMailStoreActive(C_Customer cus)
        {
            WebDataModel db = new WebDataModel();
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/notify_installation_complete");
            var webinffo = UserContent.GetWebInfomation();

            //var partner = new MerchantService().GetPartner(cus.CustomerCode);
            var Config = db.SystemConfigurations.FirstOrDefault();
            var emailData = new SendGridEmailTemplateData.InstallCompleteTemplate()
            {
                salon_name = cus.BusinessName,
                login_user = cus.BusinessEmail ?? cus.SalonEmail ?? cus.Email,
                login_password = cus.Password ?? "<Your password>",
                checkin_link = AppConfig.Cfg.CheckinLink(""),
                email_support = webinffo.SupportEmail,
                phone_suport = webinffo.CompanySupportNumber,
            };

            emailData.pos_link = AppConfig.Cfg.SaloncenterLink("");

            var _mailingService = EngineContext.Current.Resolve<IMailingService>();
            return await _mailingService.SendBySendGridWithTemplate(cus.BusinessEmail, cus.ContactName, node["template_id"].InnerText, "", emailData, "");
        }
        #endregion

        public async Task<string> SendEmailAssigned(string SalesPersonName ,string  SalesLeadName, string Phone,string MailTo )
        {
            var emailData = new SendGridEmailTemplateData.NotificationAssigned()
            {
                sales_lead_name  = SalesLeadName,
                sales_person = SalesPersonName,
                phone = Phone,
                link = ConfigurationManager.AppSettings["IMSUrl"] + "/SaleLead"
            };
            ReadXML xml = new ReadXML(System.Web.Hosting.HostingEnvironment.MapPath("/App_Data/Config.xml"));
            XmlNode node = xml.GetNode("/root/sendgrid_template/notification_assigned");
            var _mailingService = EngineContext.Current.Resolve<IMailingService>();
            return await _mailingService.SendBySendGridWithTemplate(MailTo, "", node["template_id"].InnerText, "", emailData, "");
        }

        //create new data for customer from sale lead
        public async Task<Tuple<bool, string>> CreateNewRegisterData(string Id, string phone, string createtrial = "New")
        {
            try
            {
                WebDataModel db = new WebDataModel();
                var c = new C_Customer();
               
                var sl = db.C_SalesLead.Where(x => x.Id == Id).FirstOrDefault();
                var lp = new License_Product();
                if ( !string.IsNullOrEmpty(sl.L_License_Code))
                {
                    lp = db.License_Product.Where(x=>x.Code == sl.L_License_Code&& x.isAddon==false).FirstOrDefault();
                    if (lp == null)
                    {
                        lp = db.License_Product.Where(l => (l.AllowDemo == true && createtrial.Equals("Trial")) || (l.AllowSlice == true && createtrial.Equals("Slice"))).FirstOrDefault();
                    }
                }
                else
                {
                    lp = db.License_Product.Where(l => (l.AllowDemo == true && createtrial.Equals("Trial")) || (l.AllowSlice == true && createtrial.Equals("Slice"))).FirstOrDefault();
                }
                // check data is exist in customer table
                if (!string.IsNullOrEmpty(sl.CustomerCode) && db.C_Customer.Where(x => x.CustomerCode == sl.CustomerCode).Count() > 0)
                {

                    c = db.C_Customer.Where(x => x.CustomerCode == sl.CustomerCode).FirstOrDefault();
                    if (string.IsNullOrEmpty(c.StoreCode))
                    {
                        c.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(c.CustomerCode, "[^.0-9]", "");
                    }
                    if (!createtrial.Equals("New"))
                    {
                        if (lp == null)
                        {
                            return Tuple.Create(false, "The " + createtrial + " registration program is currently paused !");
                        }
                        if (lp != null)
                        {
                            c.Version = IMSVersion.POS_VER2.Code<string>();
                            c.WordDetermine = sl.L_Version;
                        }
                    }

                }
                else
                {
                    if (!createtrial.Equals("New"))
                    {
                        if (lp == null)
                        {
                            return Tuple.Create(false, "The trial registration program is currently paused !");
                        }
                        if (lp != null)
                        {
                            c.Version = IMSVersion.POS_VER2.Code<string>();
                            c.WordDetermine = sl.L_Version;
                        }
                    }
                    c.Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff"));
                    c.CustomerCode = new MerchantService().MakeId();
                    c.StoreCode = WebConfigurationManager.AppSettings["StorePrefix"] + Regex.Replace(c.CustomerCode, "[^.0-9]", "");
                    c.OwnerName = c.ContactName = sl.L_ContactName;
                    c.OwnerMobile = c.CellPhone = sl.L_ContactPhone;
                    c.SalonPhone = c.BusinessPhone = sl.L_Phone;
                    c.MangoEmail = sl.L_Email;
                    c.CreateAt = DateTime.UtcNow;
                    if (cMem != null)
                    {
                        c.CreateBy = cMem.FullName;
                        c.UpdateBy = cMem.FullName;
                    }
                    else
                    {
                        c.CreateBy = "IMS System";
                        c.UpdateBy = "IMS System";
                    }
               
                    if (!string.IsNullOrEmpty(sl.L_Password))
                    {
                        c.Password = sl.L_Password;
                        c.MD5PassWord = SecurityLibrary.Md5Encrypt(sl.L_Password);
                    }
                    else
                    {
                        c.Password = SecurityLibrary.Md5Encrypt(DateTime.UtcNow.ToString("O")).Substring(0, 6);
                        c.MD5PassWord = SecurityLibrary.Md5Encrypt(c.Password);
                    }
                    c.BusinessName = sl.L_SalonName;
                    c.Email = sl.L_Email;
                    c.BusinessEmail = c.SalonEmail = sl.L_Email;
                    c.SalonAddress1 = c.BusinessAddressStreet = sl.L_Address?.Replace(",", " ");
                    c.SalonCity = c.BusinessCity = sl.L_City;
                    c.SalonState = c.BusinessState = sl.L_State;
                    c.SalonZipcode = c.BusinessZipCode =c.Zipcode = sl.L_Zipcode;
                    c.Country = c.BusinessCountry = "United States";
                    c.SalonTimeZone_Number = sl.SalonTimeZone_Number ?? TIMEZONE_NUMBER_BY_ID.Eastern.Text();
                    c.SalonTimeZone = sl.SalonTimeZone ?? TIMEZONE_NUMBER_BY_ID.Eastern.Code<string>();
                    c.Active = 1;
                    c.More_Info = sl.L_MoreInfo;
                    db.C_Customer.Add(c);
                    db.SaveChanges();
                    await EngineContext.Current.Resolve<IEnrichUniversalService>().InitialStoreDataAsync(c.StoreCode);


                }
                sl.CustomerCode = c.CustomerCode;
                sl.CustomerName = c.BusinessName;
                //update Salelead
                if (createtrial.Equals("Trial"))
                {
                    sl.L_IsVerify = true;
                    sl.SL_Status = LeadStatus.TrialAccount.Code<int>();
                    sl.SL_StatusName = LeadStatus.TrialAccount.Text();
                }
                else if (createtrial.Equals("Slice"))
                {
                    sl.L_IsVerify = true;
                    sl.SL_Status = LeadStatus.SliceAccount.Code<int>();
                    sl.SL_StatusName = LeadStatus.SliceAccount.Text();
                }
              
                sl.UpdateAt = DateTime.UtcNow;
                if (cMem != null)
                {
                    sl.UpdateBy = cMem.FullName;
                }
                else
                {
                    sl.UpdateBy = "IMS System";
                }
              

                db.SaveChanges();
                if (!createtrial.Equals("New"))
                {
                    return Tuple.Create(true, "Your account is ready to be activated, waiting for verify via email from customers !");
                }
                return Tuple.Create(true, "Customers is added to Sale Lead successfully");
            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex);
                return Tuple.Create(false, ex.Message);
            }
        }
     
        public void CreateLog(string SalesLeadId, string SalesLeadName, string title, string description, string createBy = "IMS System", string MemberNumber = null, DateTime? CreateAt = null,bool UpdateLastSalesLead = true)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
            {
                WebDataModel db = new WebDataModel();
                var appointment = new Calendar_Event();
                appointment.Id = Guid.NewGuid().ToString();
                appointment.Name = title;
                appointment.Description = description;
                appointment.CreateAt = CreateAt ?? DateTime.UtcNow;
                appointment.CreateBy = createBy;
                appointment.LastUpdateBy = createBy;
                appointment.MemberNumber = MemberNumber;
                appointment.Type = Calendar_Event_Type.Log.Text();
                appointment.GMT = "+00:00";
                appointment.TimeZone = "(UTC) Coordinated Universal Time";
                appointment.StartEvent = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + appointment.GMT;
                appointment.Done = 1;
                appointment.SalesLeadId = SalesLeadId;
                appointment.SalesLeadName = SalesLeadName;
                if (UpdateLastSalesLead)
                {
                    var sl = db.C_SalesLead.Find(SalesLeadId);
                    sl.UpdateAt = DateTime.UtcNow;
                    sl.LastUpdateDesc = title;
                }
                db.Calendar_Event.Add(appointment);
                db.SaveChanges();
                scope.Complete();
            }
        }


        //send api active store 
        public bool CreateStoreFromPosApi(string licenseId,C_Customer c)
        {
            try
            {
                MerchantService merchantService = new MerchantService();
                //var data = new StoreProfile();
                //data = merchantService.BuildStoreProfile(licenseId);
                StoreProfileReq data = new MerchantService().GetStoreProfileReady(licenseId, true, "active");
                merchantService.DoRequest(data);
                merchantService.SyncStoreBaseService(licenseId, 1);
                new FeatureService().ActiveDefineFeatureByStore(data.storeId);
                return true;
            }
            catch (Exception ex)
            {
                WriteLogErrorService _writeLogErrorService = new WriteLogErrorService();
                _writeLogErrorService.InsertLogError(ex, c.BusinessName + " (#" + c.StoreCode + ")");
                return false;
            }
        }
    }
}