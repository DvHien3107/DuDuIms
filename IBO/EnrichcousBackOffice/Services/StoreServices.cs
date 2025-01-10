using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewModel;
using EnrichcousBackOffice.Services;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using EnrichcousBackOffice.NextGen;
using EnrichcousBackOffice.Services.NextGen;
using EnrichcousBackOffice.Extensions;

namespace EnrichcousBackOffice.Services
{
    public class QLicense
    {
        public Store_Services service;
        public License_Product p;
        public License_Product_Item p_item;
        public License_Item li_item;
    }
    public class StoreServices
    {
        private readonly string _apiUrl = ConfigurationManager.AppSettings["ClickUpSyncNewUrl"];
        public static string F_DATE = "MM/dd/yyyy";
        public static string BASE_SERVICES = "Base Services";
        public WebDataModel DB;
        public P_Member auth = Authority.GetCurrentMember();
        #region Constructor
        public StoreServices(WebDataModel db)
        {
            if (db == null)
            {
                DB = new WebDataModel();
            }
            else
            {
                DB = db;
            }
        }
        #endregion

        #region StoreProfile



        public async Task SynDeliveryToClickUpAsync(string OrderCode)
        {
            //#if !DEBUG
            //string request = SyncClickUp.SyncClickUpRequest(OrderCode);
            //request = request.Replace("+", "plus");
            //using (var httpClient = new HttpClient())
            //{
            //    using (var response = await httpClient.PostAsync($"{_apiUrl}/v1/ClickUp/SyncDeploymentTicket?jsonString={request}", null))
            //    {
            //        string responseJson = string.Empty;
            //        if (response.StatusCode == HttpStatusCode.OK)
            //        {
            //            responseJson = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            //        }
            //        else
            //        {
            //            responseJson = await response.Content.ReadAsStringAsync();
            //            //response.ToString();
            //            LogModel.WriteLog("--SynDeliveryToClickUpAsync--"+responseJson);
            //            throw new Exception(responseJson);
            //        }
            //    }
            //}
            //#endif
        }
        /// <summary>
        /// Get store info
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public StoreProfile GetStore(string storeCode)
        {
            var customer = DB.C_Customer.FirstOrDefault(cus => cus.StoreCode == storeCode);
            if (customer == null)
                throw new Exception("Store not found.");
            var version = customer.Version ?? "";
            var cur_prod = DB.Store_Services.Where(s => s.StoreCode == storeCode && s.Active == 1 && s.Type == "license").FirstOrDefault()?.ProductCode;
            var product = DB.License_Product.Where(l => cur_prod != null && l.Code == cur_prod).FirstOrDefault();
            if (product != null)
            {
                version = string.IsNullOrEmpty(product.Code_POSSystem) ? "v2" : "v1";
            }

            StoreProfile model = new StoreProfile
            {
                StoreId = storeCode,
                Licenses = new List<Licenses>(),
                activeProducts = new List<ActiveProducts>()
            };
            string partnerLink = new MerchantService().GetPartner(customer.CustomerCode).PosApiUrl;
            string url = AppConfig.Cfg.StoreProfileUrl(partnerLink);
            HttpResponseMessage result;
            ApiPOSResponse response;
            try
            {
                result = ClientRestAPI.CallRestApi($"{url}{storeCode}", "", "", "get", null, SalonName: customer.BusinessName + " (#" + customer.StoreCode + ")");
                if (result == null || result.StatusCode == HttpStatusCode.InternalServerError)
                {
                    model = GetStoreIMS(customer);
                    return model;
                }
                if (version == IMSVersion.POS_VER2.Code<string>())
                    response = result.Content.ReadAsAsync<ApiStoreProfileResponseV2>().Result;
                else
                    response = result.Content.ReadAsAsync<ApiStoreProfileResponse>().Result;

                if (response.IsSuccess() == false)
                {
                    if (string.IsNullOrEmpty(response.Message))
                    {
                        throw new Exception();
                    }
                    throw new AppHandleException(response.Message);
                }
            }
            catch (Exception)
            {
                model = GetStoreIMS(customer);
                return model;
                //var msg = "Mango POS system not responding! Please wait a few minutes before trying again.";
                //if (e is AppHandleException)
                //{
                //    msg = $"Mango POS system: {e.Message} ";
                //}
                //throw new Exception(msg);
            }

            if (result.IsSuccessStatusCode && response.IsSuccess())
            {
                if (version == IMSVersion.POS_VER2.Code<string>())
                    model = ((ApiStoreProfileResponseV2)response).Data.FirstOrDefault();
                else
                    model = ((ApiStoreProfileResponse)response).Data.FirstOrDefault();
            }

            //model.StoreName = customer.BusinessName;
            //model.ContactName = customer.ContactName;
            //model.Email = customer.Email;
            //model.CellPhone = customer.CellPhone;
            //model.BusinessName = customer.BusinessName;
            //model.BusinessPhone = customer.BusinessPhone;
            //model.BusinessEmail = customer.BusinessEmail;
            //model.BusinessAddress = customer.AddressLine();

            return model;
        }

        public StoreProfileV2 GetStoreIMS(C_Customer customer)
        {
            var db = new WebDataModel();
            try
            {
                if (customer == null) throw new Exception();
                var model = new StoreProfileV2
                {
                    StoreId = customer.StoreCode,
                    StoreName = customer.BusinessName,
                    ContactName = customer.ContactName,
                    LastUpdate = customer.CreateAt.Value.ToString("MM/dd/yyyy"),
                    UpdateBy = customer.UpdateBy,
                    Email = customer.MangoEmail ?? customer.SalonEmail ?? customer.Email,
                    Password = customer.MD5PassWord ?? "",
                    RequirePassChange = customer.RequirePassChange,
                    CellPhone = customer.CellPhone,
                    CreateBy = customer.CreateBy,
                    CreateAt = customer.CreateAt.Value.ToString("MM/dd/yyyy"),
                    Status = db.Store_Services.Any(s => (s.StoreCode == customer.StoreCode || s.StoreCode == customer.StoreCode) && s.Type == "license" && s.Active == 1) ? "Activated" : "DeActivated",
                    BusinessName = customer.BusinessName,
                    BusinessPhone = customer.BusinessPhone,
                    BusinessEmail = customer.BusinessEmail,
                    BusinessAddress = customer.BusinessAddress,
                    NewLicense = "0",
                    Licenses = LicensesCurrent(customer.StoreCode),
                };
                return model;
            }
            catch (Exception ex)
            {
                return new StoreProfileV2 { };
            }
        }

        /// <summary>
        /// Update store info
        /// </summary>
        /// <param name="data"></param>
        /// <exception cref="Exception"></exception>
        public static void PostStore(StoreProfile data)
        {
            var db = new WebDataModel();
            var cus = db.C_Customer.FirstOrDefault(c => c.StoreCode == data.StoreId);
            data.Licenses.ForEach(license =>
            {
                license.Count_current = null;
                license.PairCodes = null;
                if ("SALONCENTER".Equals(license.LicenseCode))
                {
                    string partnerLink = new MerchantService().GetPartner(cus.CustomerCode).ManageUrl;
                    license.Link = AppConfig.Cfg.SaloncenterLink(partnerLink);
                }
            });
            string jsonRequest = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            Console.WriteLine(jsonRequest);
            var postData = JsonConvert.DeserializeObject<Dictionary<string, Object>>(jsonRequest);
            //string url = AppConfig.Cfg.StoreChangeUrl();
            string url = DomainConfig.NextGenApi + "/v1/RCPStore/StoreChange";
            var result = ClientRestAPI.CallRestApi(url, "", "", "post", postData, SalonName: data.StoreName + "(#" + data.StoreId + ")");
            var posResponse = result.Content.ReadAsAsync<ApiPOSResponse>().Result;
            if (result.IsSuccessStatusCode == false || posResponse.IsSuccess() == false)
            {
                if (string.IsNullOrEmpty(posResponse.Message ?? ""))
                    throw new Exception("Simply POS connect failure, can not push data to SimplyPOS API");
                throw new Exception(posResponse.Message);
            }
        }

        #endregion

        #region License

        /// <summary>
        /// License Status
        /// </summary>
        /// <param name="license"></param>
        /// <returns></returns>
        public string GetLicenseStatus(Store_Services license)
        {
            switch (license.Active)
            {
                case -1: return "waiting";
                case -2:
                case 0: return "deactive";
                case 1:
                    if (license.RenewDate.HasValue && license.RenewDate < DateTime.Today) return "expired";
                    break;
            }

            return "active";
        }

        /// <summary>
        /// Add Store License line from order code
        /// </summary>
        /// <param name="ordersCode"></param>
        /// <returns></returns>
        public async Task AddStoreLicense(string ordersCode)
        {
            if (DB.Store_Services.Any(st => st.OrderCode == ordersCode))
                DB.Store_Services.RemoveRange(DB.Store_Services.Where(s => s.OrderCode == ordersCode));
            var serviceItem = DB.Order_Subcription.Where(o => o.OrderCode == ordersCode && o.SubscriptionType != "setupfee" && o.SubscriptionType != "interactionfee").ToList();
            foreach (var item in serviceItem)
            {
                DB.Store_Services.Add(new Store_Services
                {
                    Id = Guid.NewGuid().ToString(),
                    StoreCode = item.StoreCode,
                    StoreName = item.CustomerName,
                    CustomerCode = item.CustomerCode,
                    StoreApply = item.PriceType,
                    AutoRenew = item.AutoRenew ?? false,
                    EffectiveDate = item.StartDate,
                    RenewDate = /*CommonFunc.EndMonth(*/item.EndDate/*)*/,
                    Period = item.Period,
                    ProductCode = item.Product_Code,
                    Productname = item.ProductName,
                    OrderCode = item.OrderCode,
                    Product_Code_POSSystem = item.Product_Code_POSSystem,
                    Type = item.SubscriptionType,
                    Active = -1,
                    LastUpdateAt = DateTime.UtcNow,
                    LastUpdateBy = auth?.FullName ?? "IMS System",
                    ApplyDiscountAsRecurring = item.ApplyDiscountAsRecurring,
                    PeriodRecurring = item.PeriodRecurring,
                    Quantity = item.SubscriptionQuantity,
                });
            }

            await DB.SaveChangesAsync();
        }

        /// <summary>
        /// LicensesCurrent
        /// </summary>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public List<Licenses> LicensesCurrent(string storeCode, string deactive_store = null)
        {
            return GetLicense(storeCode, query => ((IEnumerable<QLicense>)query).Where(st => (deactive_store != null && st.service.Id == deactive_store) || (st.service.Active == 1 && st.service.StoreCode == storeCode)));

        }

        /// <summary>
        /// LicensesPlan
        /// </summary>
        /// <param name="licenseId"></param>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public List<Licenses> LicensesPlan(string storeCode, string licenseId)
        {
            return GetLicense(DB.Store_Services.Find(licenseId)?.StoreCode ?? storeCode, query => ((IEnumerable<QLicense>)query).Where(st =>
                 st.service.Id == licenseId || (st.service.Active == 1 && st.service.Type == "addon")
            ));

        }

        /// <summary>
        /// LicensesAddOn
        /// </summary>
        /// <param name="licenseId"></param>
        /// <param name="storeCode"></param>
        /// <returns></returns>
        public List<Licenses> LicensesAddOn(string storeCode, string licenseId)
        {
            var storeService = DB.Store_Services.AsNoTracking().FirstOrDefault(x=>x.Id==licenseId);
            return GetLicense(storeService?.StoreCode?? storeCode, query => ((IEnumerable<QLicense>)query).Where(st => st.service.Id == licenseId), storeService?.Quantity ?? 1);
        }

        /// <summary>
        /// GetLicense
        /// </summary>
        /// <param name="storeCode"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private List<Licenses> GetLicense(string storeCode, Func<IEnumerable, IEnumerable<QLicense>> filter,int quantity = 1)
        {
            var timeNow = DateTime.UtcNow.Date;
            var listData =
                DB.Store_Services.AsNoTracking().Where(st => st.StoreCode == storeCode && st.EffectiveDate <= timeNow)
              .Join(DB.License_Product.AsNoTracking(), service => service.ProductCode, p => p.Code,
                  (service, p) => new { service, p })
              .Join(DB.License_Product_Item.AsNoTracking(), p => p.p.Id, p_item => p_item.License_Product_Id,
                  (p, p_item) => new { p.service, p.p, p_item })
              .Join(DB.License_Item.AsNoTracking(), p => p.p_item.License_Item_Code, li_item => li_item.Code,
                  (p, li_item) => new QLicense { service = p.service, p = p.p, p_item = p.p_item, li_item = li_item })
              .Where(rs => rs.li_item.GroupName == BASE_SERVICES && rs.li_item.Enable == true && rs.p_item.Enable == true);
            var filters = filter(listData).GroupBy(st => st.p_item.License_Item_Code).ToList();

            var productItemsLq = filters.Select(rs =>  
            {
                var feature = new LicenseProductItem();
                if (rs.Key == "SMS")
                {
                    feature.Code = rs.Key;
                    feature.Type = rs.Max(item => item.li_item.Type);
                    var itemValue = rs.Where(x => x.p_item.Value == -1).Count();
                    feature.CountLimit = itemValue > 0 ? -1 : (rs.Where(item => item.p_item.Value > 0).Sum(item => item.p_item.Value) ?? 0) * quantity;
                    feature.CountWarning = rs.Max(item => item.p_item.CountWarning) ?? 0;
                    feature.StartDate = rs.Max(item => item.service.EffectiveDate);
                    feature.EndDate = rs.Max(item => item.service.RenewDate);
                    feature.WarningDate = rs.Max(item =>
                    {
                        if (item.service.RenewDate.HasValue && item.p.SubscriptionEndWarningDays != null)
                            return item.service.RenewDate?.AddDays(-(double)item.p.SubscriptionEndWarningDays);
                        return null;
                    });
                    feature.Status = GetLicenseStatus(DB.Store_Services.Find(rs.Max(item => item.service.Id)));
                }
                else
                {
                    feature.Code = rs.Key;
                    feature.Type = rs.Max(item => item.li_item.Type);
                    feature.CountLimit = rs.Where(x => x.p_item.Value == -1).Count() > 0 ? -1 : rs.Where(item => item.p_item.Value > 0).Sum(item => item.p_item.Value) ?? 0;
                    feature.CountWarning = rs.Max(item => item.p_item.CountWarning) ?? 0;
                    feature.StartDate = rs.Max(item => item.service.EffectiveDate);
                    feature.EndDate = rs.Max(item => item.service.RenewDate);
                    feature.WarningDate = rs.Max(item =>
                    {
                        if (item.service.RenewDate.HasValue && item.p.SubscriptionEndWarningDays != null)
                            return item.service.RenewDate?.AddDays(-(double)item.p.SubscriptionEndWarningDays);
                        return null;
                    });
                    feature.Status = GetLicenseStatus(DB.Store_Services.Find(rs.Max(item => item.service.Id)));
                }
                return feature;
            });
        
            var productItems = productItemsLq.ToList();
            return LicensesFillDefault(productItems);


        }

        public string GetLicenseStatusMerchant(Store_Services license)
        {
            switch (license.Active)
            {
                case -1: return "waiting";
                case -2:
                case 0: return "deactive";
                case 1:
                    if (license.RenewDate.HasValue && license.RenewDate < DateTime.Today) return "expired";
                    break;
            }

            return "active";
        }

        /// <summary>
        /// Licenses Fill Default
        /// </summary>
        /// <param name="productItems"></param>
        /// <returns></returns>
        private List<Licenses> LicensesFillDefault(List<LicenseProductItem> productItems)
        {

            DB.License_Item.AsNoTracking().Where(li => li.GroupName == BASE_SERVICES).ToList()
            .ForEach(item =>
            {
                if (productItems.Any(prod => prod.Code == item.Code) == false)
                {
                    productItems.Add(new LicenseProductItem { Code = item.Code, Type = item.Type, CountLimit = 0, CountWarning = 0, Status = "deactive" });
                }
            });
            List<Licenses> licenses = new List<Licenses>();
            productItems.ForEach(item =>
            {
                licenses.Add(new Licenses
                {
                    LicenseCode = item.Code,
                    LicenseType = item.Type,
                    Count_warning_value = item.CountWarning.ToString(),
                    Count_limit = item.CountLimit.ToString(),
                    Start_period = item.StartDate?.ToString(F_DATE) ?? "",
                    End_period = item.EndDate?.ToString(F_DATE) ?? "",
                    Subscription_warning_date = item.WarningDate?.ToString(F_DATE) ?? "",
                    Subscription_warning_msg = "",
                    Status = item.Status,
                    Link = ""
                });
            });
            return licenses;
        }

        #endregion
    }

    public class LicenseProductItem
    {
        public string Code { get; set; }
        public string Type { get; set; }
        public int CountLimit { get; set; }
        public int CountWarning { get; set; }
        public DateTime? WarningDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
    }
}