using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.AppLB.NuveiLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.ViewControler;
using Newtonsoft.Json.Linq;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class MerchantFormBoardingController : Controller
    {
        private string _Nuvei_BaseAuthorization = System.Configuration.ConfigurationManager.AppSettings.Get("Nuvei_BaseAuthorization"); //"Basic ZW5yaWNoY286TnV2ZWkxIQ==";
        private string _Nuvei_Applink = System.Configuration.ConfigurationManager.AppSettings.Get("Nuvei_Applink");
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private P_Member cMem = AppLB.Authority.GetCurrentMember();

        WebDataModel db = new WebDataModel();
        // GET: MerchantForms
        public ActionResult Index()
        {
            if (access.Any(k => k.Key.Equals("merchantforms_boarding")) != true || access["merchantforms_boarding"] != true)
            {
                return Redirect("/home");
            }
            //add to view history top button
            UserContent.TabHistory = "Merchant boarding" + "|" + Request.Url.AbsolutePath;

            var mc = (from c in db.C_Customer
                      join subscribe in db.C_MerchantSubscribe on c.CustomerCode equals subscribe.CustomerCode into ob
                      from x in ob
                      let MerChantActive = db.C_MerchantSubscribe.Where(x=>x.CustomerCode == c.CustomerCode).OrderBy(x=>x.Status== "active"?1:x.Status== "closed"?2:x.Status== "closed"?3:4).FirstOrDefault()
                      select new C_Customer_select_view
                      {
                          Id = c.Id,
                          SubscribeId = x.Id,
                          BusinessName = c.BusinessName,
                          BusinessPhone = c.BusinessPhone,
                          OwnerName = c.OwnerName,
                          CellPhone = c.CellPhone,
                          OwnerEmail = c.Email,
                          ApplicationId = x.ApplicationId,
                          MerChantId = MerChantActive != null ? MerChantActive.MerchantID : "",
                          ProccessorName = MerChantActive!=null ? MerChantActive.DbaName :"",
                          MerChantCode = c.CustomerCode,
                          Address = (string.IsNullOrEmpty(c.BusinessAddressCivicNumber) ? "---" : c.BusinessAddressCivicNumber) + ", " +
                                    (string.IsNullOrEmpty(c.BusinessAddressStreet) ? "---" : c.BusinessAddressStreet) + ", " +
                                    (string.IsNullOrEmpty(c.BusinessCity) ? "---" : c.BusinessCity) + ", " +
                                    (string.IsNullOrEmpty(c.BusinessState) ? "---" : c.BusinessState) + ", " +
                                    (string.IsNullOrEmpty(c.BusinessCountry) ? "---" : c.BusinessCountry),
                          MerchantStatus = MerChantActive != null ? MerChantActive.Status : "",
                          TicketId = x.TicketId
                      }).ToList();
            ViewBag.CustomerSelect = db.C_Customer.Where(c => c.Active == 1)
                        .Select(c => new Select_view { name = c.BusinessName + " - " + (c.SalonEmail ?? c.BusinessEmail ?? c.Email), value = c.CustomerCode, disabled = db.C_MerchantSubscribe.Any(s => c.CustomerCode == s.CustomerCode) }).ToList();
            return View(mc);
        }



        public ActionResult Filter(string info, string ticket)
        {
            var mc = (from c in db.C_Customer.AsEnumerable()
                      where CommonFunc.SearchName(c.BusinessName, info, true)
                      || CommonFunc.SearchName(c.OwnerName, info, true)
                      || CommonFunc.SearchName(c.LegalName, info, true)

                      || CommonFunc.SearchName(c.Email, info, true)
                      || CommonFunc.SearchName(c.BusinessEmail, info, true)
                      || CommonFunc.SearchName(c.SalonEmail, info, true)

                      || CommonFunc.SearchName(c.BusinessPhone, info, true)
                      || CommonFunc.SearchName(c.CellPhone, info, true)
                      || CommonFunc.SearchName(c.SalonPhone, info, true)

                      || CommonFunc.SearchName(c.BusinessAddressStreet + c.BusinessCity + c.BusinessState + c.BusinessZipCode + c.BusinessCountry, info, true)
                      || CommonFunc.SearchName(c.SalonAddress1 + c.SalonCity + c.SalonState + c.SalonZipcode, info, true)
                      || CommonFunc.SearchName(c.OwnerAddressStreet + c.City + c.State + c.Zipcode, info, true)
                      select c).ToList();

            var fmc = (from c in mc
                       join s in db.C_MerchantSubscribe.AsEnumerable()
                       on c.CustomerCode equals s.CustomerCode
                       where (string.IsNullOrEmpty(ticket) || CommonFunc.SearchName(s.TicketId.ToString(), ticket, true))
                       let MerChantActive = db.C_MerchantSubscribe.Where(x => x.CustomerCode == c.CustomerCode).OrderBy(x => x.Status == "active" ? 1 : x.Status == "closed" ? 2 : x.Status == "closed" ? 3 : 4).FirstOrDefault()
                       select new C_Customer_select_view
                       {
                           Id = c.Id,
                           SubscribeId = s.Id,
                           BusinessName = c.BusinessName,
                           BusinessPhone = c.BusinessPhone,
                           OwnerName = c.OwnerName,
                           CellPhone = c.CellPhone,
                           OwnerEmail = c.Email,
                           ApplicationId = s?.ApplicationId,
                           MerChantId = MerChantActive != null ? MerChantActive.MerchantID : "",
                           ProccessorName = MerChantActive != null ? MerChantActive.DbaName : "",
                           MerChantCode = c.CustomerCode,
                           Address = (string.IsNullOrEmpty(c.BusinessAddressCivicNumber) ? "---" : c.BusinessAddressCivicNumber) + ", " +
                                     (string.IsNullOrEmpty(c.BusinessAddressStreet) ? "---" : c.BusinessAddressStreet) + ", " +
                                     (string.IsNullOrEmpty(c.BusinessCity) ? "---" : c.BusinessCity) + ", " +
                                     (string.IsNullOrEmpty(c.BusinessState) ? "---" : c.BusinessState) + ", " +
                                     (string.IsNullOrEmpty(c.BusinessCountry) ? "---" : c.BusinessCountry),
                           CompletedStep = (!string.IsNullOrEmpty(s?.MerchantID)) ? 3 :
                                             (!string.IsNullOrEmpty(s?.Document_ApplicationAndAgreement + s?.Document_EquipmentForm + s?.Document_NonProfitEvidence + s?.Document_Other + s?.Document_ProcessingStatements + s?.Document_ProofOfBusiness + s?.Document_VoidCheck)
                                             ) ? 2 :
                                             (!string.IsNullOrEmpty(s?.ApplicationId)) ? 1 : 0,
                           MerchantStatus = MerChantActive != null ? MerChantActive.Status : "",
                           TicketId = s.TicketId
                       }).ToList();
            return PartialView("_merchantformboarding_load", fmc);
        }

        public ActionResult getMerchantSubcribe(long? Id)
        {
            var cus = db.C_Customer.Find(Id);
            if (cus != null)
            {
                ViewBag.CustomerCode = cus.CustomerCode;
                ViewBag.MerchantID = cus.Id;
                ViewBag.BusinessName = cus.BusinessName;
                ViewBag.CustomerCode = cus.CustomerCode;
                var merchantprocess = db.C_MerchantSubscribe.Where(m => m.CustomerCode == cus.CustomerCode && !string.IsNullOrEmpty(m.MerchantID)).OrderBy(m => m.Status).ToList().Select(x => new MerchantProcessingCustomizeModel
                {
                    Id = x.Id,
                    MerchantID = x.MerchantID,
                    ProcessorName = x.DbaName,
                    CardTypeAccept = x.CardTypeAccept?.Split(','),
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    Status = x.Status,
                    ListMerchantProcess = db.C_MerchantProcessing.Where(y => y.MerchantSubscribeId == x.Id)
                });
                return PartialView("_PartialSubcriber", merchantprocess);
            }
            return PartialView("_PartialSubcriber", new List<C_MerchantSubscribe>());
        }
        public JsonResult DeleteNuveiOnBoarding(long Id)
        {
            try
            {
                if (access.Any(k => k.Key.Equals("merchantforms_boarding")) != true || access["merchantforms_boarding"] != true)
                {
                    return Json(new object[] { false, "Access denied!" });
                }
                var cusSub = db.C_MerchantSubscribe.Find(Id);
                if (cusSub == null)
                {
                    return Json(new object[] { false, "Onboarding info not found!" });
                }
                if (cusSub.ResponseStatus == "Completed")
                {
                    return Json(new object[] { false, "Onboarding info completed can't delete!" });
                }
                db.C_MerchantSubscribe.Remove(cusSub);
                db.SaveChanges();
                return Json(new object[] { true, "Delete Onboarding info completed!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message, e.ToString() });
            }

        }
        public async System.Threading.Tasks.Task<JsonResult> Create_NuveiOnboarding(string CustomerCode, string description)
        {
            using (var db = new WebDataModel())
            {
                try
                {
                    var tic = new T_SupportTicket();
                    var Cus = db.C_Customer.Where(c => c.CustomerCode == CustomerCode).FirstOrDefault();
                    if (Cus == null)
                    {
                        throw new Exception("Merchant not found");
                    }
                    if (db.C_MerchantSubscribe.Any(t => t.CustomerCode == Cus.CustomerCode))
                    {
                        throw new Exception("This marchant has Priority Onboarding already!");
                    }
                    int countOfTicket = db.T_SupportTicket.Where(t => t.CreateAt.Value.Year == DateTime.Today.Year
                                                       && t.CreateAt.Value.Month == DateTime.Today.Month).Count();
                    tic.Id = long.Parse(DateTime.UtcNow.ToString("yyMM") + (countOfTicket + 1).ToString().PadLeft(4, '0') + DateTime.Now.ToString("ff"));
                    tic.Name = "Priority Onboarding: " + Cus.BusinessName;
                    tic.Description = description;
                    tic.CreateAt = DateTime.UtcNow;
                    tic.CreateByName = "System";
                    tic.Visible = true;
                    var groupOnboarding = db.P_Department.Where(d => d.Type == "ONBOARDING").ToList();
                    var group = groupOnboarding.Count() == 1 ? groupOnboarding.FirstOrDefault() : groupOnboarding.Where(g => g.ParentDepartmentId > 0).FirstOrDefault();
                    tic.GroupID = group?.Id;
                    tic.GroupName = group?.Name;


                    tic.TypeId = AppLB.UserContent.TICKET_TYPE.NuveiOnboarding.ToString();
                    tic.TypeName = "On Boarding";
                    var typeMapping = new T_TicketTypeMapping();
                    typeMapping.TypeId =long.Parse(tic.TypeId);
                    typeMapping.TypeName = tic.TypeName;
                    tic.T_TicketTypeMapping.Add(typeMapping);
                    tic.CustomerCode = Cus.CustomerCode;
                    tic.CustomerName = Cus.BusinessName;
                    tic.StatusId = AppLB.UserContent.DeploymentTicket_Status.Open.ToString();
                    tic.StatusName = "Open";
                    db.T_SupportTicket.Add(tic);
                    db.SaveChanges();

                    await TicketViewService.AutoAssignment(db, tic, "new", cMem.MemberNumber);


                    var merchantSubcribe = new C_MerchantSubscribe
                    {
                        Id = DateTime.UtcNow.Ticks,
                        CustomerCode = Cus.CustomerCode,
                        TicketId = tic.Id
                    };
                    db.C_MerchantSubscribe.Add(merchantSubcribe);
                    db.SaveChanges();
                    return Json(new object[] { true, "Create Priority Onboarding for " + Cus.BusinessName + " completed" });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("[NewTicketNuveiOnboarding]" + ex.Message);

                    return Json(new object[] { false, "Create Priority Onboarding fail!", ex.Message });
                }
            }
        }

        public JsonResult SubcribeStep1(string cus_code, string BankDDA, string BankTransit, string BankAccount, string StatementDescriptor, DateTime MerchantSignDate,
                string Agent, string Office, string PreviousProcessingIndicator, C_Customer addinfo)
        {
            try
            {

                if ((access.Any(k => k.Key.Equals("merchantforms_manage")) == false || access["merchantforms_manage"] != true))
                {
                    throw new Exception("Access denied");
                }
                var cus = db.C_Customer.Where(c => c.CustomerCode == cus_code).FirstOrDefault();
                var cussubcribe = db.C_MerchantSubscribe.Where(c => c.CustomerCode == cus.CustomerCode).FirstOrDefault();

                var enrich = db.SystemConfigurations.FirstOrDefault();
                //update customer info
                if (Request["update_cus"] == "1")
                {
                    cus.BusinessEmail = addinfo.BusinessEmail;
                    cus.DepositAccountNumber = BankAccount;
                    cus.OwnersOrOfficicers_PreviousProcessingIndicator = PreviousProcessingIndicator;
                    cus.BusinessDescription = addinfo.BusinessDescription;
                    cus.CurrentProcessorName = addinfo.CurrentProcessorName;
                    cus.BusinessAddressStreet = addinfo.BusinessAddressStreet;
                    cus.BusinessCity = addinfo.BusinessCity;
                    cus.BusinessState = addinfo.BusinessState;
                    cus.BusinessZipCode = addinfo.BusinessZipCode;
                    cus.BusinessCountry = addinfo.BusinessCountry;
                    cus.LegalName = addinfo.LegalName;
                    cus.FederalTaxId = addinfo.FederalTaxId;
                    cus.BusinessStartDate = addinfo.BusinessStartDate;
                    cus.Email = addinfo.Email;
                    cus.Birthday = addinfo.Birthday;
                    cus.SocialSecurity = string.IsNullOrEmpty(addinfo.SocialSecurity) ? cus.SocialSecurity : addinfo.SocialSecurity;
                    cus.BusinessPhone = addinfo.BusinessPhone;
                    if (Request["same_address"] == "1")
                    {
                        cus.OwnerAddressStreet = addinfo.BusinessAddressStreet;
                        cus.City = addinfo.BusinessCity;
                        cus.State = addinfo.BusinessState;
                        cus.Zipcode = addinfo.BusinessZipCode;
                        cus.Country = addinfo.BusinessCountry;
                    }
                    else
                    {
                        cus.OwnerAddressStreet = addinfo.OwnerAddressStreet;
                        cus.City = addinfo.City;
                        cus.State = addinfo.State;
                        cus.Zipcode = addinfo.Zipcode;
                        cus.Country = addinfo.Country;
                    }
                }
                //

                if (checkInfo(cus) == false)
                {
                    var html = CommonFunc.RenderRazorViewToString("_PartialAddInfo", cus, this);
                    return Json(new object[] { false, "Merchant infomation is missing!", html });
                }
                //if (cus.BusinessStartDate == null)
                //{
                //    throw new Exception("Bussiness Start up date is missing!");
                //}
                var nuveiPayload = new NuveiPayload();
                //int yearPresence = 0, monthPresence = 0;
                //yearPresence = DateTime.Now.Year - cus.BusinessStartDate.Value.Year;
                //monthPresence = DateTime.Now.Month - cus.BusinessStartDate.Value.Month;
                //if ((DateTime.Now.Day - cus.BusinessStartDate.Value.Day) < 0)
                //{
                //    monthPresence = monthPresence - 1;
                //}
                //if (monthPresence < 0)
                //{
                //    monthPresence = monthPresence + 12;
                //    yearPresence = yearPresence - 1;
                //}

                #region add payload info
                string b_addressnumber = "", b_addressstreet = "";
                for (int i = 0; i < cus.BusinessAddressStreet.Length; i++)
                {
                    if (!char.IsDigit(cus.OwnerAddressStreet[i]))
                    {
                        b_addressnumber = cus.BusinessAddressStreet.Substring(0, i);
                        b_addressstreet = cus.BusinessAddressStreet.Substring(i);
                        break;
                    };
                }

                var us_state_business = db.Ad_USAState.Where(delegate (Ad_USAState S) { return CommonFunc.compareName(S.name, cus.BusinessState); }).FirstOrDefault();
                if (us_state_business != null)
                {
                    cus.BusinessState = us_state_business.abbreviation;
                }
                var us_state_cus = db.Ad_USAState.Where(delegate (Ad_USAState S) { return CommonFunc.compareName(S.name, cus.State); }).FirstOrDefault();
                if (us_state_cus != null)
                {
                    cus.State = us_state_cus.abbreviation;
                }

                nuveiPayload.MerchantBusinessInformation = new MerchantBusinessInformation
                {
                    LegalName = cus.LegalName,
                    CorporateAddressCivicNum = b_addressnumber,
                    CorporateAddressStreet = b_addressstreet,
                    CorporateAddressPobox = "",//"12345",
                    CorporateAddressUnitDesignator = "Unit",
                    CorporateAddressUnit = "1",
                    CorporateCity = cus.BusinessCity,
                    CorporateState = cus.BusinessState,
                    CorporateZip = cus.BusinessZipCode,
                    CorporateTelephone = CommonFunc.CleanPhone(cus.BusinessPhone),
                    FederalTaxId = cus.FederalTaxId.Replace("-", ""),
                    BusinessEmail = cus.BusinessEmail,
                    BusinessPresence = "Less_than_1_year",// : (yearPresence > 300 ? "More_than_300_year" : yearPresence.ToString() + "_years"),
                    BusinessPresenceMonths = "1",
                    MailingAddress = "Corporate Address",
                    SupportLine = CommonFunc.CleanPhone(enrich.CompanySupportNumber),
                    StatementDescriptor = StatementDescriptor,
                    MailingAttention = cus.OwnerName,
                    PrefContactEmail = cus.Email,
                    PrefContactPhone = CommonFunc.CleanPhone(cus.CellPhone),
                    OwnershipType = "Corporation",
                    GoodsType = "",
                    BusinessDescription = cus.BusinessDescription,
                    CurrentProcessorName = cus.CurrentProcessorName,


                };
                nuveiPayload.DbaInformation = new DbaInformation
                {
                    DbaName = cus.BusinessName,
                    LocationAddressCivicNum = b_addressnumber,
                    LocationAddressStreet = b_addressstreet,
                    LocationAddressUnitDesignator = "Unit",
                    LocationAddressUnit = "1",
                    LocationCity = cus.BusinessCity,
                    LocationState = cus.BusinessState,
                    LocationZip = cus.BusinessZipCode,
                    LocationTelephone = CommonFunc.CleanPhone(cus.BusinessPhone),
                };
                string[] name = cus.OwnerName.Split(' ');
                string addressnumber = "", addressstreet = "";
                for (int i = 0; i < cus.OwnerAddressStreet.Length; i++)
                {
                    if (!char.IsDigit(cus.OwnerAddressStreet[i]))
                    {
                        addressnumber = cus.OwnerAddressStreet.Substring(0, i);
                        addressstreet = cus.OwnerAddressStreet.Substring(i);
                        break;
                    };
                }

                var country = db.Ad_Country.AsEnumerable().Where(c => c.Name.Replace(" ", "").ToLower() == cus.Country?.Replace(" ", "").ToLower()).FirstOrDefault();
                string countryCode = country == null ? "US" : country.CountryCode;

                nuveiPayload.OwnersOrOfficers = new OwnersOrOfficers
                {
                    OwnerCitizenship = countryCode,
                    OwnerCountry = countryCode,
                    BusinessStartupDate = cus.BusinessStartDate?.ToString("MM/dd/yyyy"),
                    PreviousProcessingIndicator = cus.OwnersOrOfficicers_PreviousProcessingIndicator,
                    OwnerList = new List<Owner>{
                    new Owner
                    {
                        Guarantor = true,
                        Title = "Owner",
                        Email = cus.Email,
                        PercentOwnership = 100,
                        FirstName = name[0],
                        LastName = name[name.Length-1],
                        Birthday=cus.Birthday?.ToString("MM/dd/yyyy"),
                        ResidenceAddressCivicNum = addressnumber,
                        ResidenceAddressStreet = addressstreet,
                        //ResidenceAddressUnitDesignator="Unit",
                        //ResidenceAddressUnit="1",
                        City=cus.City,
                        State=cus.State,
                        Zip=cus.Zipcode,
                        Telephone= CommonFunc.CleanPhone(cus.CellPhone),
                        SocialSecurity=cus.SocialSecurity
                    }
                }

                };
                nuveiPayload.ContractVersionEtf = new ContractVersionEtf { MerchantSignDate = MerchantSignDate.ToString("MM/dd/yyyy") };
                nuveiPayload.BankInformation = new BankInformation
                {
                    BankTransit = BankTransit,
                    BankDda = BankDDA,
                    BankAccount = BankAccount
                };
                nuveiPayload.Agent = Agent;
                nuveiPayload.Office = Office;
                nuveiPayload.User = "enrichco";//"enrichco";
                #endregion

                //cus.DepositAccountNumber = CommonFunc.getStringValid(BankDDA, BankAccount);
                cus.DepositRoutingNumber = BankTransit;
                cus.DepositAccountNumber = CommonFunc.getStringValid(BankDDA, BankAccount);//BankAccount;
                db.Entry(cus).State = System.Data.Entity.EntityState.Modified;

                string rs = SendXML.RequestAppID(_Nuvei_Applink, nuveiPayload, _Nuvei_BaseAuthorization, (cussubcribe?.ApplicationId ?? ""));

                //save
                if (cussubcribe != null)
                {
                    cussubcribe.ApplicationId = rs;
                    cussubcribe.StatementDescriptor = StatementDescriptor;
                    cussubcribe.Agent = Agent;
                    cussubcribe.Office = Office;
                    db.Entry(cussubcribe).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    cussubcribe = new C_MerchantSubscribe
                    {
                        Id = long.Parse(DateTime.UtcNow.ToString("yyMMddhhmmssfff")),
                        CustomerCode = cus.CustomerCode,
                        ApplicationId = rs,
                        StatementDescriptor = StatementDescriptor,
                        Agent = Agent,
                        Office = Office
                    };
                    db.C_MerchantSubscribe.Add(cussubcribe);
                }
                db.SaveChanges();


                return Json(new object[] { true, "Merchant appication updated!" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, "Error: " + e.Message });
            }

        }


        public ActionResult step1moreinfo(long id)
        {
            var cus = db.C_Customer.Find(id);
            return PartialView("_PartialAddInfo", cus);
        }

        public bool checkInfo(C_Customer cus)
        {
            if (
                string.IsNullOrEmpty(cus.BusinessEmail)
                || string.IsNullOrEmpty(cus.BusinessCity)
                || string.IsNullOrEmpty(cus.BusinessCountry)
                || string.IsNullOrEmpty(cus.LegalName)
                || string.IsNullOrEmpty(cus.BusinessAddressStreet)
                || string.IsNullOrEmpty(cus.BusinessState)
                || string.IsNullOrEmpty(cus.FederalTaxId)
                || string.IsNullOrEmpty(cus.BusinessZipCode)
                || cus.BusinessStartDate == null
                || string.IsNullOrEmpty(cus.Email)
                || string.IsNullOrEmpty(cus.Country)
                || string.IsNullOrEmpty(cus.OwnerAddressStreet)
                || cus.Birthday == null
                || string.IsNullOrEmpty(cus.SocialSecurity)
                || string.IsNullOrEmpty(cus.State)
                //|| string.IsNullOrEmpty(cus.OwnerAddressCivicNumber)
                || string.IsNullOrEmpty(cus.BusinessPhone)
                || string.IsNullOrEmpty(cus.Zipcode)
                || string.IsNullOrEmpty(cus.City))
                return false;
            else
                return true;
        }


        public JsonResult SubcribeStep2()
        {
            string[] doctype = { "ApplicationAndAgreement", "ProcessingStatements", "ProofOfBusiness", "VoidCheck", "EquipmentForm", "NonProfitEvidence", "Other" };
            List<List<string>> upload_status = new List<List<string>>();
            try
            {
                if ((access.Any(k => k.Key.Equals("merchantforms_manage")) == false || access["merchantforms_manage"] != true))

                {
                    throw new Exception("Access denied");
                }
                long Id = long.Parse(Request["CusID"]);
                var cus = db.C_Customer.Find(Id);
                var cussubcribe = db.C_MerchantSubscribe.Where(c => c.CustomerCode == cus.CustomerCode).FirstOrDefault();
                var ApplicationAndAgreement_select = Request["fileselect"];
                if (cussubcribe != null)
                {
                    if (Request.Files.Count == 0 && (ApplicationAndAgreement_select == "newfile" || string.IsNullOrEmpty(ApplicationAndAgreement_select)))
                    {
                        throw new Exception("Please choose a file!");
                    }
                    foreach (var DocumemtType in doctype)
                    {
                        try
                        {
                            string filesave = string.Empty;
                            if (DocumemtType == "ApplicationAndAgreement" && ApplicationAndAgreement_select != "newfile")
                            {
                                var mf = db.O_MerchantForm.Find(long.Parse(Request["fileselect"]));
                                string name = mf.PDF_URL.Insert(mf.PDF_URL.LastIndexOf('.'), "_signed");//signed url pdf
                                filesave = "/upload/documents/" + mf.MerchantCode + "_" + name.Split('/').Last();
                                System.IO.File.Copy(Server.MapPath(name), Server.MapPath(filesave), true);
                                string result = SendXML.HttpUploadFile(_Nuvei_Applink + "/applink/Application/US/" + cussubcribe.ApplicationId + "/Document?documentType=" + DocumemtType,
                                    Path.Combine(Server.MapPath("~/") + filesave), "file", "application/pdf", new NameValueCollection(), _Nuvei_BaseAuthorization);
                                upload_status.Add(new List<string> { DocumemtType, "1" });
                                switch (DocumemtType)
                                {
                                    case "ApplicationAndAgreement":
                                        cussubcribe.Document_ApplicationAndAgreement = filesave;
                                        break;
                                    case "ProcessingStatements":
                                        cussubcribe.Document_ProcessingStatements = filesave;
                                        break;
                                    case "ProofOfBusiness":
                                        cussubcribe.Document_ProofOfBusiness = filesave;
                                        break;
                                    case "VoidCheck":
                                        cussubcribe.Document_VoidCheck = filesave;
                                        break;
                                    case "EquipmentForm":
                                        cussubcribe.Document_EquipmentForm = filesave;
                                        break;
                                    case "NonProfitEvidence":
                                        cussubcribe.Document_NonProfitEvidence = filesave;
                                        break;
                                    case "Other":
                                        cussubcribe.Document_Other = filesave;
                                        break;
                                }
                            }
                            else
                            {

                                HttpPostedFileBase file = HttpContext.Request.Files[DocumemtType];
                                if (file != null)
                                {
                                    if (file.ContentLength > 10240000)
                                        throw new Exception("The maximum file upload size is 10MB!");

                                    filesave = "/upload/documents/" + file.FileName;
                                    file.SaveAs(Path.Combine(Server.MapPath("~/" + filesave)));
                                    NameValueCollection nvc = new NameValueCollection();
                                    string result = SendXML.HttpUploadFile(_Nuvei_Applink + "/applink/Application/US/" + cussubcribe.ApplicationId + "/Document?documentType=" + DocumemtType,
                                        Path.Combine(Server.MapPath("~/") + filesave), "file", file.ContentType, nvc, _Nuvei_BaseAuthorization);
                                    //save
                                    upload_status.Add(new List<string> { DocumemtType, "1" });
                                    switch (DocumemtType)
                                    {
                                        case "ApplicationAndAgreement":
                                            cussubcribe.Document_ApplicationAndAgreement = filesave;
                                            break;
                                        case "ProcessingStatements":
                                            cussubcribe.Document_ProcessingStatements = filesave;
                                            break;
                                        case "ProofOfBusiness":
                                            cussubcribe.Document_ProofOfBusiness = filesave;
                                            break;
                                        case "VoidCheck":
                                            cussubcribe.Document_VoidCheck = filesave;
                                            break;
                                        case "EquipmentForm":
                                            cussubcribe.Document_EquipmentForm = filesave;
                                            break;
                                        case "NonProfitEvidence":
                                            cussubcribe.Document_NonProfitEvidence = filesave;
                                            break;
                                        case "Other":
                                            cussubcribe.Document_Other = filesave;
                                            break;
                                    }
                                }
                            }
                            db.Entry(cussubcribe).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();

                        }
                        catch (Exception e)
                        {
                            upload_status.Add(new List<string> { DocumemtType, "0", e.Message });
                        }

                    }
                    return Json(new object[] { true, "Upload done!", upload_status, upload_status.Count(u => u[1] == "1"), upload_status.Count(u => u[1] == "0") });
                }
                return Json(new object[] { false, "Merchant have no Application ID" });
            }
            catch (Exception e)
            {
                return Json(new object[] { false, e.Message });
            }

        }

        public JsonResult SubcribeStep3()
        {
            try
            {
                if ((access.Any(k => k.Key.Equals("merchantforms_manage")) == false || access["merchantforms_manage"] != true))

                {
                    throw new Exception("Access denied");
                }
                long Id = long.Parse(Request["CusID"] ?? "0");
                var cus = db.C_Customer.Find(Id);
                if (cus == null)
                {
                    return Json(new object[] { false, "Merchant not found" });
                }
                var cusSub = db.C_MerchantSubscribe.Where(c => c.CustomerCode == cus.CustomerCode).FirstOrDefault();
                if (cusSub == null)
                {
                    return Json(new object[] { false, "Merchant have no Application ID" });
                }
                else
                {
                    string rs = SendXML.postJsonData(_Nuvei_Applink + "/applink/Application/US/" + cusSub.ApplicationId + "/Submit", "", _Nuvei_BaseAuthorization);
                    if (!string.IsNullOrWhiteSpace(rs))
                    {
                        string MerchantId = JObject.Parse(rs)["MerchantId"].ToString();
                        if (!string.IsNullOrWhiteSpace(MerchantId))
                        {
                            cusSub.MerchantID = MerchantId;
                            db.Entry(cusSub).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            return Json(new object[] { true, "Successful submission!" });
                        }
                        else
                        {
                            //string error = "";
                            //foreach (var item in JObject.Parse(rs)["Messages"].ToList())
                            //{
                            //    error = string.Join("\n- ",error, item["Message"]);
                            //}
                            return Json(new object[] { false, rs });
                        }

                    }
                    else
                    {
                        throw new Exception("Submission failure, Please try again later");
                    }

                }

            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex.Message });
            }
        }
        public JsonResult ClearSubscribe(string cus_code)
        {
            try
            {
                var cussubscribe = db.C_MerchantSubscribe.Where(c => c.CustomerCode == cus_code).FirstOrDefault();
                if (!string.IsNullOrEmpty(cussubscribe.ApplicationId))
                {
                    SendXML.postJsonData(_Nuvei_Applink + "/applink/Application/US/" + cussubscribe.ApplicationId, "", _Nuvei_BaseAuthorization, "DELETE");
                }
                cussubscribe.ApplicationId = "";
                cussubscribe.Agent = System.Configuration.ConfigurationManager.AppSettings.Get("Nuvei_Agent");
                cussubscribe.Office = System.Configuration.ConfigurationManager.AppSettings.Get("Nuvei_Offices");
                //file uploaded
                cussubscribe.Document_ApplicationAndAgreement = "";
                cussubscribe.Document_EquipmentForm = "";
                cussubscribe.Document_NonProfitEvidence = "";
                cussubscribe.Document_Other = "";
                cussubscribe.Document_ProcessingStatements = "";
                cussubscribe.Document_ProofOfBusiness = "";
                cussubscribe.Document_VoidCheck = "";
                cussubscribe.MerchantID = "";
                //NuveiNotification
                cussubscribe.GatewayMerchantId = "";
                cussubscribe.GatewayTerminalNumbers = "";
                cussubscribe.SharedSecret = "";
                cussubscribe.UId = "";
                cussubscribe.DbaName = "";
                cussubscribe.MessageType = "";
                cussubscribe.ResponseStatus = "";
                cussubscribe.ResponseCodeFromNuvei = "";

                db.Entry(cussubscribe).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Clear merchant Subscription is complete!" });
            }
            catch (Exception ex)
            {
                return Json(new object[] { false, ex?.Message });
            }

        }
        public JsonResult deleteSubscribeDoc(long Id, string doctype)
        {
            if ((access.Any(k => k.Key.Equals("merchantforms_manage")) == false || access["merchantforms_manage"] != true))

            {
                return Json(new object[] { false, "Access denied!" });
            }
            var code = db.C_Customer.Find(Id).CustomerCode;
            var cussubscribe = db.C_MerchantSubscribe.Where(c => c.CustomerCode == code).FirstOrDefault();
            if (cussubscribe != null)
            {
                switch (doctype)
                {
                    case "ApplicationAndAgreement":
                        cussubscribe.Document_ApplicationAndAgreement = "";
                        break;
                    case "ProcessingStatements":
                        cussubscribe.Document_ProcessingStatements = "";
                        break;
                    case "ProofOfBusiness":
                        cussubscribe.Document_ProofOfBusiness = "";
                        break;
                    case "VoidCheck":
                        cussubscribe.Document_VoidCheck = "";
                        break;
                    case "EquipmentForm":
                        cussubscribe.Document_EquipmentForm = "";
                        break;
                    case "NonProfitEvidence":
                        cussubscribe.Document_NonProfitEvidence = "";
                        break;
                    case "Other":
                        cussubscribe.Document_Other = "";
                        break;
                }
                db.Entry(cussubscribe).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Json(new object[] { true, "Delete document done!" });
            }
            else
            {
                return Json(new object[] { false, "This merchant has no Application ID!" });
            }
        }
    }


}