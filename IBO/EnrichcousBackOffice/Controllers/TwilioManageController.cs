using DataTables.AspNet.Core;
using Enrich.DataTransfer.Twilio;
using Enrich.IServices;
using Enrich.IServices.Utils;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static iTextSharp.text.pdf.AcroFields;

namespace EnrichcousBackOffice.Controllers
{
    [Authorize]
    public class TwilioManageController : Controller
    {
        private ITwilioRestApiService _twilioRestApiService;
        private IPosSyncTwilioApiService _posSyncTwilioApiService;
        private Dictionary<string, bool> access = AppLB.Authority.GetAccessAuthority();
        private readonly ILogService _logService;
        private P_Member cMem = AppLB.Authority.GetCurrentMember();
        private readonly string OperationPosType = "Operation";
		private readonly string PromotionPosType = "Promotion";
		private readonly string POSDefaultOperationTwilioPhoneNumber = ConfigurationManager.AppSettings["POSDefaultOperationTwilioPhoneNumber"].ToString();
		//private readonly string POSDefaultPromitionTwilioPhoneNumber = ConfigurationManager.AppSettings["POSDefaultPromitionTwilioPhoneNumber"].ToString();
		private readonly string POSDefaultPromitionTwilioServiceId = ConfigurationManager.AppSettings["POSDefaultPromitionTwilioServiceId"].ToString();

		public TwilioManageController(ITwilioRestApiService twilioRestApiService, IPosSyncTwilioApiService posSyncTwilioApiService, ILogService logService)
        {
            _twilioRestApiService = twilioRestApiService;
            _posSyncTwilioApiService = posSyncTwilioApiService;
            _logService = logService;
        }

     
        // GET: TwilioManage
        public ActionResult Index()
        {
            if ((access.Any(k => k.Key.Equals("twilio_account_manage")) == false || access["twilio_account_manage"] != true))
            {
                return Redirect("/home/Forbidden");
            }
            return View();
        }

        /// <summary>
        /// Get twilio account
        /// </summary>
        /// <param name="dataTablesRequest"></param>
        /// <param name="SearchText"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetTwilioAccount(IDataTablesRequest dataTablesRequest, string SearchText)
        {
            var db = new WebDataModel();
            var query = from account in db.TwilioAccounts join cus in db.C_Customer on account.StoreCode equals cus.StoreCode select new { account,cus};
            if (!string.IsNullOrEmpty(SearchText))
            {
                SearchText = SearchText.Trim().ToLower();
                query = query.Where(x => x.account.StoreCode.ToLower().Contains(SearchText) || x.cus.BusinessName.Contains(SearchText));
            };

            var total = query.Count();
            var dataView = query.OrderByDescending(x => x.account.CreatedDate).Skip(dataTablesRequest.Start).Take(dataTablesRequest.Length).ToList().Select(x=>{

                var totalPhoneStore = db.TwilioA2PTollFreeVerification.Where(y => y.StoreCode == x.account.StoreCode).ToList();
				var totalPhoneApproved = totalPhoneStore.Count(y => y.VerificationStatus.ToLower() == "twilio_approved");
				var totalPhonePending = totalPhoneStore.Count(y => y.VerificationStatus.ToLower() != "twilio_rejected" && y.VerificationStatus.ToLower() != "twilio_approved");
				var totalPhoneReject = totalPhoneStore.Count(y => y.VerificationStatus.ToLower() == "twilio_rejected");
				var result = new
                {
                    Id = x.account.Id,
                    CustommerId = x.cus.Id,
                    StoreCode = x.account.StoreCode,
                    BusinessName = x.cus.BusinessName,
                    Name = x.account.Name,
                    Status = x.account.Status,
                    SId = x.account.SId,
					totalPhoneApproved = totalPhoneApproved,
					totalPhonePending = totalPhonePending,
					totalPhoneReject = totalPhoneReject,
					CreatedDate = x.account.CreatedDate,
                    CreatedBy = x.account.CreatedBy
                };
                return result;
            });
            return Json(new
            {
                recordsFiltered = total,
                recordsTotal = total,
                data = dataView,
            });
        }

        /// <summary>
        /// Search merchant
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult SearchMerchant(string search)
        {
            var db = new WebDataModel();
            var ClosedStatusOrder = InvoiceStatus.Closed.ToString();
            var PaidStatusOrder = InvoiceStatus.Paid_Wait.ToString();

            string STORE_IN_HOUSE = MerchantType.STORE_IN_HOUSE.Code<string>();
            var merchants = (from c in db.C_Customer
                            let order = db.O_Orders.Any(o => c.CustomerCode == o.CustomerCode)
                            where c.Active != 0 && (order || c.Type == STORE_IN_HOUSE) && (string.IsNullOrEmpty(search) || c.BusinessName.Contains(search) || c.StoreCode.Contains(search) || c.BusinessPhone.Contains(search))
                            orderby c.StoreCode
                            select c).Skip(0).Take(10).Distinct().Select(c => new { c.Id, c.StoreCode, c.BusinessName}).ToList();
         
            return Json(merchants, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Create account twilio
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateAccountTwilio()
        {
            var db = new WebDataModel();
            return PartialView("_CreateTwlioAccount");
        }

        /// <summary>
        /// Submit create sub account
        /// </summary>
        /// <param name="storeCode"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateSubAccount(string storeCode,string Name)
        {
            try
            {
                var db = new WebDataModel();
                var account = db.TwilioAccounts.FirstOrDefault(x => x.StoreCode == storeCode);
                if (account != null)
                    return Json(new { status = false, message = "This merchant is already exist twilio account" });


                //create subaccount
                var twilioAccount = await _twilioRestApiService.CreateSubAccount(Name);
                var entity = new TwilioAccount
                {
                    StoreCode = storeCode,
                    SId = twilioAccount.Sid,
                    Name = twilioAccount.FriendlyName,
                    AuthToken = twilioAccount.AuthToken,
                    OwnerAccountSid = twilioAccount.OwnerAccountSid,
                    Status = twilioAccount.Status.ToString(),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = cMem.FullName
                };
                db.TwilioAccounts.Add(entity);
                db.SaveChanges();

                //create messaging service 
                var service = await _twilioRestApiService.CreateServiceAsync("Promotion Message Service",entity.SId, entity.AuthToken);
			    var serviceEntity = new TwilioMessageService
			    {
				    StoreCode = storeCode,
				    MessageServiceId = service.Sid,
				    MessageServiceName = service.FriendlyName,
				    Type = "Promotion",
				    TwilioAccountSid = entity.SId,
				    CreatedDate = DateTime.UtcNow,
				    CreatedBy = cMem.FullName
			    };
			    db.TwilioMessageServices.Add(serviceEntity);
				db.SaveChanges();

				return Json(new { status = true, message = "Create twilio account success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> LoadPhoneNumber(int Id)
        {
            var db = new WebDataModel();
            var account = db.TwilioAccounts.Find(Id);
            if (account == null)
                return Json(new { status = false, message = "This merchant is already exist twilio account" });
            var query = from phone in db.TwilioA2PTollFreeVerification where phone.StoreCode == account.StoreCode select phone;
          
            var total = query.Count();
            var dataView = query.OrderByDescending(x => x.Id).ToList();
            return Json(new
            {
                recordsFiltered = total,
                recordsTotal = total,
                data = dataView,
            });
        }

        /// <summary>
        /// Load detail account
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> DetailAccount(int Id)
        {
            var db = new WebDataModel();
            var account = db.TwilioAccounts.FirstOrDefault(x => x.Id == Id);
            if (account == null)
                throw new Exception("Twilio account not found");

            var twilioA2PTollFreeVerification = db.TwilioA2PTollFreeVerification.Where(x => x.TwilioAccountSId == account.SId).ToList();
            ViewBag.TwilioA2PTollFreeVerifications = twilioA2PTollFreeVerification;
            return PartialView("_DetailTwilioSubAccount", account);
        }

        /// <summary>
        /// Load available phone number
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetListPhoneNumberAvailable(int Id)
        {
            var db = new WebDataModel();
            var account = db.TwilioAccounts.Find(Id);
            var tollFree = await _twilioRestApiService.AvailablePhoneNumberTollFree(account.SId, account.AuthToken);
            var data = new List<string>();
            foreach (var record in tollFree)
            {
                data.Add(record.PhoneNumber.ToString());
            }
            return Json(data);
        }

        [HttpPost]
        public ActionResult RequestTollFreeVerification(int Id)
        {
            var db = new WebDataModel();
            var account = db.TwilioAccounts.FirstOrDefault(x => x.Id == Id);
            if (account == null)
                throw new Exception("Twilio account not found");

            //var checkExistVerification = db.TwilioA2PTollFreeVerification.Any(x => x.TwilioAccountSId == account.SId);
            //if(checkExistVerification)
            //    throw new Exception("Error: Limit 1 phone number for 1 account");

            var customer = db.C_Customer.FirstOrDefault(x => x.StoreCode == account.StoreCode);
            var customerProfile = db.TwilioCustomerProfiles.Where(x => x.StoreCode == account.StoreCode && x.TwilioAccountSId == account.SId).FirstOrDefault();
            ViewBag.Customer = customer;
            ViewBag.Account = account;
            return PartialView("_BuyPhoneNumber", customerProfile);
        }

        [HttpPost]
        public ActionResult DetailTollFreeVerification(int Id)
        {
            var db = new WebDataModel();
            var verification = db.TwilioA2PTollFreeVerification.FirstOrDefault(x => x.Id == Id);
            if (verification == null)
                throw new Exception("Twilio verification not found");
            var customerProfile = db.TwilioCustomerProfiles.Where(x => x.StoreCode == verification.StoreCode && x.TwilioAccountSId == verification.TwilioAccountSId).FirstOrDefault();
            ViewBag.Profile = customerProfile;
            return PartialView("_DetailTollFreeVerification", verification);
        }

        private async Task CreateTollFreeHook(TwilioAccount twilioAccount)
        {
            var sink = await _twilioRestApiService.CreateSinkAsync($"Twilio Toll-Free Hook Account {twilioAccount.SId}", twilioAccount.SId, twilioAccount.AuthToken);
            var subscription = await _twilioRestApiService.CreateSubscriptionAsync(sink.Sid, twilioAccount.SId, twilioAccount.AuthToken);
        }

        [HttpPost]
        public async Task<ActionResult> RequestTollFreeVerificationSubmit()
        {
            try
            {
                var accountSId = Request["SId"].ToString();
                var db = new WebDataModel();
                var account = db.TwilioAccounts.FirstOrDefault(x => x.SId == accountSId);
                var customerProfileSId = Request["CustomerProfileSId"]?.ToString();
                string phoneNumber = Request["TollfreePhoneNumberSid"].ToString();
                var incommingPhoneNumber = await _twilioRestApiService.IncomingPhoneNumber(phoneNumber, account.SId, account.AuthToken);
                bool PosUsingForOperation = bool.Parse(Request["PosUsingForOperation"]?.ToString() ?? "false");
                bool PosUsingForPosPromotion = bool.Parse(Request["PosUsingForPosPromotion"]?.ToString() ?? "false");
                var tollFreeVerificationInfo = new TwilioA2PTollFreeVerification
                {
                    StoreCode = Request["StoreCode"].ToString(),
                    ProductionMessageSample = Request["ProductionMessageSample"].ToString(),
                    UseCaseSummary = Request["UseCaseSummary"].ToString(),
                    TollfreePhoneNumberSid = incommingPhoneNumber.Sid,
                    PhoneNumber = phoneNumber,
                    MessageVolume = Request["MessageVolume"].ToString(),
                    UseCaseCategories = Request["UseCaseCategories"].ToString(),
                    TwilioAccountSId = accountSId,
                    OptInType = Request["OptInType"].ToString(),
                    OptInImageUrls = string.Join(",", Request["OptInImageUrls"].ToString().Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)),
                    AdditionalInformation = Request["AdditionalInformation"]?.ToString(),
                    PosUsingForOperation = PosUsingForOperation,
                    PosUsingForPosPromotion = PosUsingForPosPromotion,
                    CreatedOn = DateTime.UtcNow,
                    Active =true,
                    CreatedBy = cMem.FullName
                };

                
                if (string.IsNullOrEmpty(customerProfileSId))
                {

                    var profile = new TwilioCustomerProfile()
                    {
                        TwilioAccountSId = Request["SId"].ToString(),
                        BusinessName = Request["BusinessName"].ToString(),
                        BusinessStreetAddress = Request["BusinessStreetAddress"].ToString(),
                        BusinessCity = Request["BusinessCity"].ToString(),
                        BusinessStateProvinceRegion = Request["BusinessStateProvinceRegion"].ToString(),
                        BusinessCountry = Request["BusinessCountry"].ToString(),
                        BusinessPostalCode = Request["BusinessPostalCode"].ToString(),
                        BusinessContactFirstName = Request["BusinessContactFirstName"].ToString(),
                        BusinessContactLastName = Request["BusinessContactLastName"].ToString(),
                        BusinessWebsite = Request["BusinessWebsite"].ToString(),
                        NotificationEmail = Request["NotificationEmail"].ToString(),
                        StoreCode = Request["StoreCode"].ToString(),
                        BusinessContactPhone = Request["BusinessContactPhone"].ToString(),
                        BusinessContactEmail = Request["BusinessContactEmail"].ToString(),
                        CreatedOn = DateTime.UtcNow,
                        CreatedBy = cMem.FullName
                    };
                    var request = new TollfreeVerificationRequest
                    {
                        BusinessName = profile.BusinessName,
                        BusinessWebsite = profile.BusinessWebsite,
                        BusinessStreetAddress = profile.BusinessStreetAddress,
                        BusinessCity = profile.BusinessCity,
                        BusinessStateProvinceRegion = profile.BusinessStateProvinceRegion,
                        BusinessPostalCode = profile.BusinessPostalCode,
                        BusinessCountry = profile.BusinessCountry,

                        BusinessContactFirstName = profile.BusinessContactFirstName,
                        BusinessContactLastName = profile.BusinessContactLastName,
                        BusinessContactEmail = profile.BusinessContactEmail,
                        BusinessContactPhone = profile.BusinessContactPhone,
                        NotificationEmail = profile.NotificationEmail,

                        ProductionMessageSample = tollFreeVerificationInfo.ProductionMessageSample,
                        UseCaseSummary = tollFreeVerificationInfo.UseCaseSummary,
                        MessageVolume = tollFreeVerificationInfo.MessageVolume,
                        TollfreePhoneNumberSid = tollFreeVerificationInfo.TollfreePhoneNumberSid,
                        UseCaseCategories = tollFreeVerificationInfo.UseCaseCategories.Split(',').ToList(),
                        OptInImageUrls = tollFreeVerificationInfo.OptInImageUrls.Split(',').ToList(),
                        OptInType = tollFreeVerificationInfo.OptInType
                    };
                    var res = await _twilioRestApiService.RequestTollfreeVerification(request, account.SId, account.AuthToken);
                    profile.CustomerProfileSId = res.customer_profile_sid;
                    tollFreeVerificationInfo.SId = res.sid;
                    tollFreeVerificationInfo.VerificationStatus = res.status;
                    db.TwilioCustomerProfiles.Add(profile);
                    db.TwilioA2PTollFreeVerification.Add(tollFreeVerificationInfo);
                    db.SaveChanges();

                    //create hook
                    await CreateTollFreeHook(account);

                }
                else
                {
                    var profile = db.TwilioCustomerProfiles.FirstOrDefault(x => x.CustomerProfileSId == customerProfileSId);
                    if(profile == null)
                    {
                        return Json(new { status = false, message = "Twilio customer profile not found" });
                    }
                    var request = new TollfreeVerificationWithCustomerProfileSIdRequest
                    {
                        CustomerProfileSid = profile.CustomerProfileSId,
                        BusinessName = profile.BusinessName,
                        BusinessWebsite = profile.BusinessWebsite,
                        NotificationEmail = profile.NotificationEmail,
                        ProductionMessageSample = tollFreeVerificationInfo.ProductionMessageSample,
                        UseCaseSummary = tollFreeVerificationInfo.UseCaseSummary,
                        MessageVolume = tollFreeVerificationInfo.MessageVolume,
                        TollfreePhoneNumberSid = tollFreeVerificationInfo.TollfreePhoneNumberSid,
                        UseCaseCategories = tollFreeVerificationInfo.UseCaseCategories.Split(',').ToList(),
                        OptInImageUrls = tollFreeVerificationInfo.OptInImageUrls.Split(',').ToList(),
                        OptInType = tollFreeVerificationInfo.OptInType
                    };
                    var res = await _twilioRestApiService.RequestTollfreeVerificationWithCustomerProfile(request, account.SId, account.AuthToken);
                    tollFreeVerificationInfo.CustomerProfileSId = res.customer_profile_sid;
                    tollFreeVerificationInfo.SId = res.sid;
                    tollFreeVerificationInfo.VerificationStatus = res.status;
                    db.TwilioA2PTollFreeVerification.Add(tollFreeVerificationInfo);
                    db.SaveChanges();
                }
                return Json(new {status = true, message = "Buy Toll-Free Phone Number Success" });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = ex.Message, data = ex.ToString() });
            }
        }

        [HttpPost]
        public ActionResult UpdateTollFree(int Id)
        {
            try
            {
                var db = new WebDataModel();
                bool PosUsingForOperation = bool.Parse(Request["PosUsingForOperation"]?.ToString() ?? "false");
                bool PosUsingForPosPromotion = bool.Parse(Request["PosUsingForPosPromotion"]?.ToString() ?? "false");
                var verificationTollFree = db.TwilioA2PTollFreeVerification.Find(Id);
                verificationTollFree.PosUsingForPosPromotion = PosUsingForPosPromotion;
                verificationTollFree.PosUsingForOperation = PosUsingForOperation;
                db.SaveChanges();
                 return Json(new { status = true, message = "Update success" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult SuspendAccount(int Id)
        {
            try
            {
                var db = new WebDataModel();
                var account = db.TwilioAccounts.FirstOrDefault(x => x.Id == Id);
                _twilioRestApiService.SuspendedSubAccountAsync(account.SId);
                account.Status = "Suspended";
                db.SaveChanges();
                return Json(new { status = true, message = "Account Suspended" });
            }
            catch(Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public ActionResult ClosingAccount(int Id)
        {
            try
            {
                var db = new WebDataModel();
                var account = db.TwilioAccounts.FirstOrDefault(x => x.Id == Id);
                _twilioRestApiService.ClosedSubAccountAsync(account.SId);
                account.Status = "Closed";
                db.SaveChanges();
                return Json(new { status = true, message = "Account Closed" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult> SyncPhoneNumberToPos(int TollFreeVerificationId)
        {
			var db = new WebDataModel();
			var verification = db.TwilioA2PTollFreeVerification.AsNoTracking().FirstOrDefault(x=>x.Id == TollFreeVerificationId);
			try
            {
                _logService.Info($"[SyncPhoneNumberToPos] Start sync phone number Id: {TollFreeVerificationId}");
				await SyncTollFreeToPos(verification.StoreCode,TollFreeVerificationId, !(verification.SyncToPosStatus ?? false));
				_logService.Info($"[SyncPhoneNumberToPos] End sync phone number Id: {TollFreeVerificationId}");
                return Json(new { status = true, message = "Sync to pos success" });
            }
            catch (Exception ex)
            {
				return Json(new { status = false, message = ex.Message });
            }
        }

        #region Hook
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> HookStatus(List<TwilioTollFreeVerificationWebhookRequest> requests)
        {
            try
            {
                _logService.Info($"[TwilioManager] Start hook request: {JsonConvert.SerializeObject(requests)}");
                if (requests == null || requests.Count == 0)
                {
                    return Content("");
                }
                var db = new WebDataModel();
                foreach (var item in requests)
                {
                    var verification = db.TwilioA2PTollFreeVerification.FirstOrDefault(x => x.SId == item.data.tollfreeverificationsid);
                    _logService.Info($"[TwilioManager] Start update hook request: {JsonConvert.SerializeObject(verification)}");
                    if (verification != null)
                    {
                        var accountSid = db.TwilioAccounts.FirstOrDefault(x => x.StoreCode == verification.StoreCode);
                        if (accountSid != null)
                        {
                            accountSid.CustomerProfileSId = item.data.customerprofilesid;
                        }
                        var profile = db.TwilioCustomerProfiles.FirstOrDefault(x => x.StoreCode == verification.StoreCode);
                        if (profile != null && string.IsNullOrEmpty(profile.CustomerProfileSId))
                        {
                            profile.CustomerProfileSId = item.data.customerprofilesid;
                        }
                        verification.VerificationStatus = item.data.verificationstatus;
                        if (verification.VerificationStatus.ToLower() == "twilio_approved")
						{
							
							try
							{
								verification.SyncToPosStatus = true;
								db.SaveChanges();
								await SyncTollFreeToPos(verification.StoreCode, verification.Id, true);
							}
							catch (Exception ex)
							{
								verification.SyncToPosStatus = false;
								db.SaveChanges();
							}
						}
                        verification.LastHookJson = JsonConvert.SerializeObject(item);
                        db.SaveChanges();
						_logService.Info($"[TwilioManager] End update hook request: {JsonConvert.SerializeObject(verification)}");
                      
                        verification.LastHookJson = JsonConvert.SerializeObject(item);
						verification.TwilioUpdateDate = DateTime.UtcNow;
						db.SaveChanges();
                        _logService.Info($"[TwilioManager] End update hook request: {JsonConvert.SerializeObject(verification)}");
                    }
                }
                _logService.Info($"[TwilioManager] End hook request: {JsonConvert.SerializeObject(requests)}");
                return Content("");
            }
             catch (Exception ex)
            {
                _logService.Error($"[TwilioManager] hook error request: {JsonConvert.SerializeObject(requests)}");
                return Content("");
            }
        }

        #endregion

        #region Tools
        public async Task<ActionResult> SyncAllTwilioStatus()
        {
            var db = new WebDataModel();
            var tollfreeVerifications = db.TwilioA2PTollFreeVerification.ToList();
            foreach(var tollfree in tollfreeVerifications)
            {
                try
                {
                    _logService.Info($"[SyncAllTwilioStatus] Start sync toll-free id: {tollfree.SId}");
                    var account = db.TwilioAccounts.Where(x=>x.SId == tollfree.TwilioAccountSId).First();
                    var res = await _twilioRestApiService.RetrieveTollFreeVerification(tollfree.SId, account.SId, account.AuthToken);
                    tollfree.VerificationStatus = res.status;
                    tollfree.TwilioUpdateDate = res.date_updated;
					db.SaveChanges();
                    _logService.Info($"[SyncAllTwilioStatus] End sync toll-free id: {tollfree.SId}");
                }
                catch(Exception ex)
                {
                    _logService.Error($"[SyncAllTwilioStatus] End sync toll-free id: {tollfree.SId}",ex);
                }
            }
            return Content("");
        }


		public async Task<ActionResult> SyncAllTollFreeToPos()
		{
			var db = new WebDataModel();
			var stores = db.TwilioA2PTollFreeVerification.Where(x=>x.SyncToPosStatus == true ).GroupBy(x=>x.StoreCode).Select(x=>x.First()).ToList();
			foreach (var item in stores)
			{
				try
				{
					_logService.Info($"[SyncAllTwilioStatus] Start sync toll-free to POS id: {item.StoreCode}");
                    await SyncTollFreeToPosByStoreCode(item.StoreCode);
					_logService.Info($"[SyncAllTwilioStatus] End sync toll-free to POS id: {item.StoreCode}");
				}
				catch (Exception ex)
				{
					_logService.Error($"[SyncAllTwilioStatus] End sync toll-free to POS id: {item.StoreCode}", ex);
				}
			}
			return Content("");
		}
		#endregion

		#region Test

		private async Task SyncTollFreeToPos(string storeCode,int tollFreeId, bool status)
		{
            try
            {
				_logService.Info($"[TwilioManager] [{storeCode}] Start sync toll-free to POS");
				var db = new WebDataModel();
				var accountSid = db.TwilioAccounts.FirstOrDefault(x => x.StoreCode == storeCode);

				//check status account twilio
				if (accountSid == null || accountSid.Status.ToLower() != "active") return;


                var currentTollFreeNumber = db.TwilioA2PTollFreeVerification.FirstOrDefault(x => x.Id == tollFreeId);
				currentTollFreeNumber.SyncToPosStatus = status;
                db.SaveChanges();
                try
                {
					//check message service if not exist create new
					var messagingService = db.TwilioMessageServices.FirstOrDefault(x => x.StoreCode == storeCode);
					if (messagingService == null)
					{
						_logService.Info($"[TwilioManager] [{storeCode}] start create message service");
						var service = await _twilioRestApiService.CreateServiceAsync("Promotion Message Service", accountSid.SId, accountSid.AuthToken);
						messagingService = new TwilioMessageService
						{
							StoreCode = storeCode,
							MessageServiceId = service.Sid,
							MessageServiceName = service.FriendlyName,
							Type = "Promotion",
							TwilioAccountSid = accountSid.SId,
							CreatedDate = DateTime.UtcNow,
							CreatedBy = cMem.FullName
						};
						db.TwilioMessageServices.Add(messagingService);
						db.SaveChanges();
						_logService.Info($"[TwilioManager] [{storeCode}] end create message service success");
					}

					//get toll-free active on POS
					var verification = db.TwilioA2PTollFreeVerification.Where(x => x.StoreCode == storeCode && x.SyncToPosStatus == true).ToList();

					var operationTollFree = verification.FirstOrDefault(x => x.PosUsingForOperation == true);
					if (operationTollFree != null)
					{
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = operationTollFree.PhoneNumber,
							Type = OperationPosType
						});
					}
					else
					{
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = POSDefaultOperationTwilioPhoneNumber,
							Type = OperationPosType
						});
					}

					var promotionTollFree = verification.Where(x => x.PosUsingForPosPromotion == true).ToList();
					if (promotionTollFree.Count > 0)
					{
						var readMessagingService = await _twilioRestApiService.ReadPhoneServiceAsync(messagingService.MessageServiceId, accountSid.SId, accountSid.AuthToken);
						foreach (var item in promotionTollFree)
						{
							try
							{
								_logService.Info($"[TwilioManager] [{storeCode}] start toll-free item {item.Id}");
								//process promotion
								if (item.PosUsingForPosPromotion == true)
								{
									var checkPhoneExistInMessaging = readMessagingService.Any(x => x.PhoneNumber.ToString() == item.PhoneNumber);
									if (checkPhoneExistInMessaging == false)
									{
										//add phone in message
										await _twilioRestApiService.AddPhoneServiceAsync(item.TollfreePhoneNumberSid, messagingService.MessageServiceId, accountSid.SId, accountSid.AuthToken);
									}
								}
								_logService.Info($"[TwilioManager] [{storeCode}] end toll-free item {item.Id}");
							}
							catch (Exception ex)
							{
								_logService.Error($"[TwilioManager] [{storeCode}] sync toll-free item error  {item.Id}", ex);
								throw ex;
							}
						}

						var itemNeedDelete = readMessagingService.Where(x => promotionTollFree.Any(y => y.PhoneNumber != x.PhoneNumber.ToString())).ToList();
						if (itemNeedDelete.Count > 0)
							foreach (var item in itemNeedDelete)
							{
								await _twilioRestApiService.DeletePhoneServiceAsync(item.Sid, messagingService.MessageServiceId, accountSid.SId, accountSid.AuthToken);
							}
						//sync to pos
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = messagingService.MessageServiceId,
							Type = OperationPosType
						});
					}
					else
					{
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = POSDefaultPromitionTwilioServiceId,
							Type = PromotionPosType
						});

					}
					_logService.Info($"[TwilioManager] [{storeCode}] end sync toll-free to POS");
				}
                catch (Exception ex)
                {
					currentTollFreeNumber.SyncToPosStatus = !status;
					db.SaveChanges();
					_logService.Error($"[TwilioManager] [{storeCode}] sync toll-free to POS error", ex);
                    throw ex;
				}
			
			}
		    catch(Exception ex)
            {
				_logService.Error($"[TwilioManager] [{storeCode}] sync toll-free to POS error", ex);
				throw ex;
			}
		}


		private async Task SyncTollFreeToPosByStoreCode(string storeCode)
		{
			try
			{
				_logService.Info($"[TwilioManager] [{storeCode}] Start sync toll-free to POS");
				var db = new WebDataModel();
				var accountSid = db.TwilioAccounts.FirstOrDefault(x => x.StoreCode == storeCode);

				//check status account twilio
				if (accountSid == null || accountSid.Status.ToLower() != "active") return;
					//check message service if not exist create new
					var messagingService = db.TwilioMessageServices.FirstOrDefault(x => x.StoreCode == storeCode);
					if (messagingService == null)
					{
						_logService.Info($"[TwilioManager] [{storeCode}] start create message service");
						var service = await _twilioRestApiService.CreateServiceAsync("Promotion Message Service", accountSid.SId, accountSid.AuthToken);
						messagingService = new TwilioMessageService
						{
							StoreCode = storeCode,
							MessageServiceId = service.Sid,
							MessageServiceName = service.FriendlyName,
							Type = "Promotion",
							TwilioAccountSid = accountSid.SId,
							CreatedDate = DateTime.UtcNow,
							CreatedBy = cMem.FullName
						};
						db.TwilioMessageServices.Add(messagingService);
						db.SaveChanges();
						_logService.Info($"[TwilioManager] [{storeCode}] end create message service success");
					}

					//get toll-free active on POS
					var verification = db.TwilioA2PTollFreeVerification.Where(x => x.StoreCode == storeCode && x.SyncToPosStatus == true).ToList();

					var operationTollFree = verification.FirstOrDefault(x => x.PosUsingForOperation == true);
					if (operationTollFree != null)
					{
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = operationTollFree.PhoneNumber,
							Type = OperationPosType
						});
					}
					else
					{
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = POSDefaultOperationTwilioPhoneNumber,
							Type = OperationPosType
						});
					}

					var promotionTollFree = verification.Where(x => x.PosUsingForPosPromotion == true).ToList();
					if (promotionTollFree.Count > 0)
					{
						var readMessagingService = await _twilioRestApiService.ReadPhoneServiceAsync(messagingService.MessageServiceId, accountSid.SId, accountSid.AuthToken);
						foreach (var item in promotionTollFree)
						{
							try
							{
								_logService.Info($"[TwilioManager] [{storeCode}] start toll-free item {item.Id}");
								//process promotion
								if (item.PosUsingForPosPromotion == true)
								{
									var checkPhoneExistInMessaging = readMessagingService.Any(x => x.PhoneNumber.ToString() == item.PhoneNumber);
									if (checkPhoneExistInMessaging == false)
									{
										//add phone in message
										await _twilioRestApiService.AddPhoneServiceAsync(item.TollfreePhoneNumberSid, messagingService.MessageServiceId, accountSid.SId, accountSid.AuthToken);
									}
								}
								_logService.Info($"[TwilioManager] [{storeCode}] end toll-free item {item.Id}");
							}
							catch (Exception ex)
							{
								_logService.Error($"[TwilioManager] [{storeCode}] sync toll-free item error  {item.Id}", ex);
								throw ex;
							}
						}

						var itemNeedDelete = readMessagingService.Where(x => promotionTollFree.Any(y => y.PhoneNumber != x.PhoneNumber.ToString())).ToList();
						if (itemNeedDelete.Count > 0)
							foreach (var item in itemNeedDelete)
							{
								await _twilioRestApiService.DeletePhoneServiceAsync(item.Sid, messagingService.MessageServiceId, accountSid.SId, accountSid.AuthToken);
							}
						//sync to pos
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = messagingService.MessageServiceId,
							Type = OperationPosType
						});
					}
					else
					{
						await _posSyncTwilioApiService.PosUpdateTwilioPhoneNumerAsync(new PosUpdateTwilioPhoneNumberRequest
						{
							StoreId = accountSid.StoreCode,
							AccountSId = accountSid.SId,
							AuthToken = accountSid.AuthToken,
							Phone = POSDefaultPromitionTwilioServiceId,
							Type = PromotionPosType
						});

					}
					_logService.Info($"[TwilioManager] [{storeCode}] end sync toll-free to POS");
			}
			catch (Exception ex)
			{
				_logService.Error($"[TwilioManager] [{storeCode}] sync toll-free to POS error", ex);
				throw ex;
			}
		}
		#endregion
	}
}