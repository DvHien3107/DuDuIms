using Enrich.Common;
using Enrich.Common.Enums;
using Enrich.Common.Helpers;
using Enrich.Core;
using Enrich.Core.Utils;
using Enrich.Dto;
using Enrich.Dto.Base;
using Enrich.Dto.Base.POSApi;
using Enrich.IMS.Entities;
using Enrich.IMS.Infrastructure.Data.Interface.Repositories;
using Enrich.IMS.Services.Interface.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Transactions;

namespace Enrich.IMS.Services.Implement.Services
{
    public class ActivatorService : IActivatorService
    {
        private readonly string _domainPos;
        private readonly string _domainNextGen;
        private readonly string _pathProfile;
        private readonly string _pathStoreChange;
        private readonly string _pathDefineFeature;
        private readonly string _pathAddFeature;
        private readonly string _pathRemoveFeature;
        private readonly string _pathGetFeature;
        private readonly string _pathOrderGiftcard;
        private readonly IEnrichLog _enrichLog;
        private readonly EnrichContext _context;
        private readonly ILicenseItemRepository _repositoryLicenseItem;
        private readonly ILicenseProductRepository _repositoryLicenseProduct;
        private readonly IStoreServiceRepository _repositoryStoreService;
        private readonly ICalendarEventRepository _repositoryCalendarEvent;
        private readonly ICustomerRepository _repositoryCustomer;
        private readonly IPartnerRepository _repositoryPartner;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStoreBaseServiceRepository _storeBaseServiceRepository;
        private readonly IEnrichSMSService _enrichSMS;

        public ActivatorService(
            IConfiguration appConfig,
            EnrichContext context,
            IEnrichLog enrichLog,
            IStoreServiceRepository repositoryStoreService,
            ICustomerRepository repositoryCustomer,
            ICalendarEventRepository repositoryCalendarEvent,
            IPartnerRepository repositoryPartner,
            IHttpClientFactory httpClientFactory,
            ILicenseItemRepository repositoryLicenseItem,
            ILicenseProductRepository repositoryLicenseProduct,
            IStoreBaseServiceRepository storeBaseServiceRepository,
            IEnrichSMSService enrichSMS)
        {
            _context = context;
            _enrichLog = enrichLog;
            _repositoryCustomer = repositoryCustomer;
            _repositoryPartner = repositoryPartner;
            _repositoryCalendarEvent = repositoryCalendarEvent;
            _repositoryStoreService = repositoryStoreService;
            _repositoryLicenseItem = repositoryLicenseItem;
            _repositoryLicenseProduct = repositoryLicenseProduct;
            _httpClientFactory = httpClientFactory;
            _domainPos = appConfig["POSUrl:Domain"];
            _domainNextGen = appConfig["POSUrl:DomainNextGen"];
            _pathProfile = appConfig["POSUrl:Path:profile"];
            _pathStoreChange = appConfig["POSUrl:Path:change"];
            _pathDefineFeature = appConfig["POSUrl:Path:defineFeature"];
            _pathAddFeature = appConfig["POSUrl:Path:addFeature"];
            _pathRemoveFeature = appConfig["POSUrl:Path:removeFeature"];
            _pathGetFeature = appConfig["POSUrl:Path:getFeature"];
            _pathOrderGiftcard = appConfig["POSUrl:Path:orderGiftcard"];
            _storeBaseServiceRepository = storeBaseServiceRepository;
            _enrichSMS = enrichSMS;
        }
        /// <summary>
        /// Active cho recurring****
        /// </summary>
        /// <param name="storeServices"></param>
        /// <param name="stage"></param>
        /// <returns></returns>
        public async Task ActivatorAction(string storeServiceId, string action, string mode = "")
        {
            if (string.IsNullOrEmpty(storeServiceId)) throw new Exception(ValidationMessages.Invalid.StoreService);
            if (string.IsNullOrEmpty(action)) throw new Exception(ValidationMessages.Invalid.Action);
            var storeService = await _repositoryStoreService.FindByIdAsync(storeServiceId);
            if (storeService == null) throw new Exception(ValidationMessages.Invalid.StoreService);
            _enrichLog.Info($"Activation service {storeService.Productname} order {storeService.OrderCode} customer {storeService.CustomerCode}", new { StoreService = storeService });
            //action == Active
            if (action == SubscriptionEnum.Action.Active.ToString())
            {
                //subscription type == license
                if (storeService.Type == SubscriptionEnum.Type.license.ToString())
                {
                    var currentLicense = await _repositoryStoreService.GetLicenseActivatedAsync(storeService.CustomerCode, storeService.ProductCode);
                    //Renew license
                    if (currentLicense != null && currentLicense.LastRenewOrderCode == storeService.OrderCode)
                    {
                        //Remove current license
                        currentLicense.AutoRenew = storeService.AutoRenew;
                        currentLicense.Active = (int)SubscriptionEnum.Status.Removed;
                        currentLicense.LastUpdateAt = DateTime.UtcNow;
                        currentLicense.LastUpdateBy = _context.UserFullName;
                        currentLicense.LastRenewOrderCode = storeService.OrderCode;
                        //Effective date = old effective date
                        storeService.EffectiveDate = currentLicense.EffectiveDate;
                        await _repositoryStoreService.UpdateAsync(storeService);
                    }
                    //Active new license
                    else if (currentLicense != null && currentLicense.LastRenewOrderCode != storeService.OrderCode)
                    {
                        //deactive current license
                        currentLicense.Active = (int)SubscriptionEnum.Status.Deactive;
                        currentLicense.LastUpdateAt = DateTime.UtcNow;
                        currentLicense.LastUpdateBy = _context.UserFullName;
                        try //Add merchant log
                        {
                            string titleLog = $"Deactived subscription";
                            string descriptionLog = $"Deactived subscription <b>" + currentLicense.Productname + "</b>";
                            await _repositoryCalendarEvent.CreateAsync(currentLicense, titleLog, descriptionLog);
                        }
                        catch (Exception ex)
                        {
                            _enrichLog.Error(ex, $"[{storeService.CustomerCode}] Error add merchant log Deactived subscription");
                        }
                    }
                    await _repositoryStoreService.UpdateAsync(currentLicense);
                }
                //subscription type == addon
                else if(storeService.Type == SubscriptionEnum.Type.addon.ToString() && storeService.Period == SubscriptionEnum.Period.MONTHLY.ToString())
                {
                    var currentAddon = await _repositoryStoreService.GetByLastOrderCodeAsync(storeService.OrderCode, storeService.ProductCode);
                    if (currentAddon != null)
                    {
                        //Remove current license
                        currentAddon.Active = (int)SubscriptionEnum.Status.Removed;
                        await _repositoryStoreService.UpdateAsync(currentAddon);
                    }
                }
                try //Add merchant log
                {
                    string titleLog = $"Activation subscription";
                    string descriptionLog = $"Activation subscription <b>" + storeService.Productname + "</b>";
                    if (mode == SubscriptionEnum.Action.Recurring.ToString())
                    {
                        titleLog = $"Recurring subscription";
                        descriptionLog = $"Recurring subscription <b>" + storeService.Productname + "</b>";
                    }
                    await _repositoryCalendarEvent.CreateAsync(storeService, titleLog, descriptionLog);
                }
                catch (Exception ex)
                {
                    _enrichLog.Error(ex, $"[{storeService.CustomerCode}] Error add merchant log Activation subscription");
                }
            }

            try
            {
                if (storeService.Type == SubscriptionEnum.Type.license.ToString() || storeService.Type == SubscriptionEnum.Type.addon.ToString())
                {
                    StoreProfileRequest storeProfile = await GetStoreProfileReady(storeService, SubscriptionEnum.Action.Active.ToString());
                    await RequestBaseServiceAction(storeProfile);
                    await SyncStoreBaseServiceRemainingAsync(storeService.Id);
                    //await RequestFeatureAction(storeService.CustomerCode);
                }
                else if (storeService.Type == SubscriptionEnum.Type.giftcard.ToString())
                {
                    await RequestGiftcardAction(storeService.Id);
                }

                //save change active store service after call api success
                storeService.Active = (int)SubscriptionEnum.Status.Active;
                storeService.LastUpdateAt = DateTime.UtcNow;
                storeService.LastUpdateBy = _context.UserFullName;
                await _repositoryStoreService.UpdateAsync(storeService);
            }
            catch (Exception e)
            {
                _enrichLog.Error(e, $"{storeService.CustomerCode} Activation on POS error");
            }
        }
        private async Task<StoreProfileRequest> GetStoreProfileReady(StoreService storeService, string action)
        {
            if (storeService == null) throw new Exception(ValidationMessages.Invalid.StoreService);
            var customer = await _repositoryCustomer.GetByCustomerCodeAsync(storeService.CustomerCode);
            if (customer == null) throw new Exception(ValidationMessages.NotFound.Customer);

            var data = new StoreProfileRequest()
            {
                StoreId = customer.StoreCode,
                StoreName = customer.BusinessName,
                StoreAddress = customer.GetSalonAddress(),
                City = customer.GetSalonCity(),
                State = customer.GetSalonState(),
                ZipCode = customer.GetSalonZipCode(),
                StoreEmail = customer.MangoEmail ?? customer.SalonEmail ?? customer.Email,
                StorePhone = customer.BusinessPhone,
                ContactName = customer.OwnerName ?? customer.ContactName,
                LastUpdate = DateTime.UtcNow.ToString(Constants.Format.Date_EN),
                UpdateBy = storeService?.LastUpdateBy ?? _context.UserFullName ?? Constants.SystemName,
                Password = customer.MD5PassWord ?? "",
                RequirePassChange = customer.RequirePassChange ?? "off",
                Status = EnumHelper.DisplayName(CustomerEnum.StatusSearch.Active),
                NewLicense = storeService?.Type == SubscriptionEnum.Type.license.ToString() && action == SubscriptionEnum.Action.Active.ToString() ? "1" : "0",
                SubscriptionCode = storeService.ProductCode,
                SubscriptionName = storeService.Productname,
                TimeZone = customer.SalonTimeZoneNumber ?? EnumHelper.DisplayName(TIMEZONE.Eastern),
                TimeZoneName = customer.SalonTimeZone ?? TIMEZONE.Eastern.ToString(),
                CompanyPartner = customer.PartnerCode ?? string.Empty,
                Licenses = await GetBaseServiceForStoreChange(storeService, customer, action)
            };

            return data;
        }
        private async Task<List<LicensesRequest>> GetBaseServiceForStoreChange(StoreService storeService, Customer customer, string action)
        {
            if (storeService == null) throw new Exception(ValidationMessages.Invalid.StoreService);
            if (customer == null) throw new Exception(ValidationMessages.NotFound.Customer);
            var licenses = new List<LicensesRequest>();
            if (!string.IsNullOrEmpty(storeService.Id))
            {
                if (action == SubscriptionEnum.Action.Active.ToString()) //active
                {
                    if (storeService.Type == SubscriptionEnum.Type.addon.ToString()) //active addon
                    {
                        var baseServices = await _repositoryStoreService.GetBaseServiceAsync(customer.CustomerCode, storeService.Id);
                        licenses = baseServices.Select(l => new LicensesRequest
                        {
                            Rollover = l.Count_limit == 0 ? -1 : (l.Count_limit == -1 ? 0 : 1),
                            LicenseCode = l.LicenseCode,
                            LicenseType = l.LicenseType,
                            Count_limit = (l.Count_limit == -1 ? -1 : l.Count_limit * (storeService.Quantity ?? 1)).ToString(),
                            Start_period = l.Start_period,
                            End_period = l.End_period
                        }).ToList();
                    }
                    else // active license
                    {
                        var baseServices = await _repositoryStoreService.GetBaseServiceAsync(customer.CustomerCode, storeService.Id, true);
                        licenses = baseServices.Select(l => new LicensesRequest
                        {
                            Rollover = 0,
                            LicenseCode = l.LicenseCode,
                            LicenseType = l.LicenseType,
                            Count_limit = l.Count_limit.ToString(),
                            Start_period = l.Start_period,
                            End_period = l.End_period
                        }).ToList();
                        var addon = await _repositoryStoreService.GetBaseServiceAsync(customer.CustomerCode, storeService.Id);
                        if (addon != null)
                        {
                            var addonSMS = addon.Where(c => c.LicenseCode == "SMS").Select(l => new { Count_limit = l.Count_limit.ToString() }).FirstOrDefault();
                            if (addonSMS != null)
                            {
                                licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Rollover = 1;
                                licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit = addonSMS.Count_limit;
                            }
                        }
                    }
                    var temp = await _repositoryStoreService.GetBaseServiceAsync(customer.CustomerCode);
                    if (temp != null && licenses != null)
                    {
                        var smsTemp = temp.Where(c => c.LicenseCode == "SMS").FirstOrDefault() ?? new BaseService();
                        if (smsTemp.Count_limit == -1)
                        {
                            licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Rollover = 0;
                            licenses.FirstOrDefault(c => c.LicenseCode == "SMS").Count_limit = "-1";
                        }
                    }
                }
            }
            var licenseActive = await _repositoryStoreService.GetLicenseActivatedAsync(customer.CustomerCode);
            _repositoryLicenseItem.GetAll().Where(li => li.GroupName == EnumHelper.DisplayName(SubscriptionEnum.GroupName.BaseService)).ToList().ForEach(item =>
            {
                if (licenses.Any(license => license.LicenseCode == item.Code) == false)
                {
                    licenses.Add(new LicensesRequest
                    {
                        Rollover = -1,
                        LicenseCode = item.Code,
                        LicenseType = item.Type,
                        Count_limit = "0",
                        Start_period = licenseActive?.EffectiveDate.Value.ToString(Constants.Format.Date_EN),
                        End_period = licenseActive?.RenewDate.Value.ToString(Constants.Format.Date_EN),
                    });
                }
            });
            return licenses;
        }
       
        private async Task<bool> RequestBaseServiceAction(StoreProfileRequest data)
        {
            var jsonData = JsonConvert.SerializeObject(data, Formatting.Indented);
            string partnerUrl = _repositoryPartner.GetByCode(data.CompanyPartner)?.PosApiUrl;
            //string url = (partnerUrl != null ? partnerUrl : _domainNextGen) + _pathStoreChange;
            string url = (partnerUrl != null ? partnerUrl : _domainNextGen) + "/v1/RCPStore/StoreChange";
            _enrichLog.Info($"[POS][Request] {data.StoreId} {url}", new { Request = data });
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
                {


                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var result = JsonConvert.DeserializeObject<ApiPOSResponse>(payloadString);
                        _enrichLog.Info($"[POS][Response] {data.StoreId} {url}", new { Response = result });

                        await _enrichLog.WriteLog(jsonData, payloadString, "RequestBaseServiceAction", url, "POST", "200", data.StoreName);

                        if (result.IsSuccess() != true)
                        {
                            if (result == null) throw new Exception("POS connect failure, can not push data to API");
                            throw new Exception(result.Message);
                        }
                        return true;
                    }
                    else
                    {
                        var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var payload = JsonConvert.DeserializeObject<Response>(payloadString) ?? new Response();
                        await _enrichLog.WriteLog(jsonData, payloadString, "RequestBaseServiceAction", url, "POST", "500", data.StoreName);
                        if (payload.Code == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException(payload?.Message);
                        throw new HttpResponseException(payload.Code, payload?.Message);
                    }

                }
            }
        }

        

        private async Task<List<LicenseProductItem>> GetDefineFeatureByStore(string customerCode)
        {
            var customer = await _repositoryCustomer.GetByCustomerCodeAsync(customerCode);
            string partnerUrl = _repositoryPartner.GetByCode(customer.PartnerCode)?.PosApiUrl;
            string url = (partnerUrl != null ? partnerUrl : _domainPos) + _pathGetFeature + customer.StoreCode;
            _enrichLog.Info($"[POS][Request] {customer.StoreCode} {url}");
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        payloadString = payloadString.Remove(payloadString.Length - 1, 1).Remove(0, 1);
                        var result = JsonConvert.DeserializeObject<ApiPOSResponse>(payloadString);
                        _enrichLog.Info($"[POS][Response] {customer.StoreCode} {url}", new { Response = result });
                        if (result.IsSuccess() != true)
                        {
                            if (result == null) throw new Exception("POS connect failure, can not push data to API");
                            throw new Exception(result.Message);
                        }
                        return result.DataResult.Select(c => new LicenseProductItem
                        {
                            License_Item_Code = c.IdKey,
                            Value = c.NumRequest
                        }).ToList();
                    }
                    else
                    {
                        var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var payload = JsonConvert.DeserializeObject<Response>(payloadString);
                        if (payload.Code == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException(payload.Message);
                        throw new HttpResponseException(payload.Code, payload.Message);
                    }
                }
            }
        }
        private async Task<bool> AddDefineFeature(List<LicenseProductItem> features, string customerCode)
        {
            if (features.Count > 0)
            {
                var customer = _repositoryCustomer.GetByCustomerCode(customerCode);
                var licenseItems = _repositoryLicenseItem.GetAll().Where(c => c.Enable == true);
                var featureStore = features.Join(licenseItems, f => f.License_Item_Code, l => l.Code, (f, l) => new
                {
                    l.Type,
                    f.Enable,
                    f.License_Item_Code,
                    f.License_Product_Id,
                    f.Value
                }).Select(c => new
                {
                    idKey = c.License_Item_Code,
                    requestNum = c.Type.ToLower().Contains("feature") ? 0 : c.Value,
                    idims = customer.StoreCode
                }).ToList();
                string partnerUrl = _repositoryPartner.GetByCode(customer.PartnerCode)?.PosApiUrl;
                string url = (partnerUrl != null ? partnerUrl : _domainPos) + _pathAddFeature;
                var jsonData = JsonConvert.SerializeObject(featureStore, Formatting.Indented);
                _enrichLog.Info($"[POS][Request] {customer.StoreCode} {url}", new { Request = featureStore });
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            payloadString = payloadString.Remove(payloadString.Length - 1, 1).Remove(0, 1);
                            var result = JsonConvert.DeserializeObject<ApiPOSResponse>(payloadString);
                            _enrichLog.Info($"[POS][Response] {customer.StoreCode} {url}", new { Response = result });
                            if (result.IsSuccess() != true)
                            {
                                if (result == null) throw new Exception("POS connect failure, can not push data to API");
                                throw new Exception(result.Message);
                            }
                            return true;
                        }
                        else
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var payload = JsonConvert.DeserializeObject<Response>(payloadString);
                            if (payload.Code == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException(payload.Message);
                            throw new HttpResponseException(payload.Code, payload.Message);
                        }
                    }
                }
            }
            return true;
        }
        private async Task<bool> RemoveDefineFeature(List<LicenseProductItem> features, string customerCode)
        {
            if (features.Count > 0)
            {
                var customer = _repositoryCustomer.GetByCustomerCode(customerCode);
                var featureStore = features.Select(f => new
                {
                    idKey = f.License_Item_Code,
                    idims = customer.StoreCode
                }).ToList();
                string partnerUrl = _repositoryPartner.GetByCode(customer.PartnerCode)?.PosApiUrl;
                string url = (partnerUrl != null ? partnerUrl : _domainPos) + _pathRemoveFeature;
                _enrichLog.Info($"[POS][Request] {customer.StoreCode} {url}", new { Request = featureStore });
                var jsonData = JsonConvert.SerializeObject(featureStore, Formatting.Indented);
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    var content = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");
                    using (var response = await httpClient.PostAsync(url, content).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            payloadString = payloadString.Remove(payloadString.Length - 1, 1).Remove(0, 1);
                            var result = JsonConvert.DeserializeObject<ApiPOSResponse>(payloadString);
                            _enrichLog.Info($"[POS][Response] {customer.StoreCode} {url}", new { Response = result });
                            if (result.IsSuccess() != true)
                            {
                                if (result == null) throw new Exception("POS connect failure, can not push data to API");
                                throw new Exception(result.Message);
                            }
                            return true;
                        }
                        else
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var payload = JsonConvert.DeserializeObject<Response>(payloadString);
                            if (payload.Code == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException(payload.Message);
                            throw new HttpResponseException(payload.Code, payload.Message);
                        }
                    }
                }
            }
            return true;
        }
        private async Task<bool> RequestFeatureAction(string customerCode)
        {
            //Get all feature of Store in Mango POS
            List<LicenseProductItem> oldFeaturesInPos = await GetDefineFeatureByStore(customerCode);
            //Get all feature in IMS to POS
            var featureOnStore = await _repositoryStoreService.GetFeatureAsync(customerCode);
            var allFeatures = featureOnStore.Select(c => new MangoFeature
            {
                IdKey = c.License_Item_Code,
                NumRequest = (c.Value ?? 1) * ((c.Quantity > 1 && c.SupscriptionType == "addon") ? (c.Quantity ?? 1) : 1),
                RequestNum = c.FeatureType == "COUNT",
            }).GroupBy(g => g.IdKey).Select(s => new MangoFeature
            {
                IdKey = s.First().IdKey,
                RequestNum = s.First().RequestNum,
                NumRequest = s.First().RequestNum == true ? s.Sum(it => it.NumRequest) : s.First().NumRequest
            }).ToList();

            var featureDel = oldFeaturesInPos.Where(o => !allFeatures.Any(f => f.IdKey.Equals(o.License_Item_Code)) && o.Value <= 1).ToList();
            var featureAdd = allFeatures.Where(f => !oldFeaturesInPos.Any(o => o.License_Item_Code.Equals(f.IdKey)) || (f.NumRequest > 1 && f.RequestNum == true)).Select(c => new LicenseProductItem
            {
                License_Item_Code = c.IdKey,
                Value = c.NumRequest
            }).ToList();

            await RemoveDefineFeature(featureDel, customerCode);
            await AddDefineFeature(featureAdd, customerCode);
            return true;
        }
        private async Task<bool> RequestGiftcardAction(string storeServiceId)
        {
            var storeService = await _repositoryStoreService.FindByIdAsync(storeServiceId);
            if (storeService == null) throw new Exception(ValidationMessages.Invalid.StoreService);

            var licenseProduct = await _repositoryLicenseProduct.GetBySubscriptionCodeAsync(storeService.ProductCode);
            if (licenseProduct != null && licenseProduct.GiftCardQuantity > 0)
            {
                var customer = _repositoryCustomer.GetByCustomerCode(storeService.CustomerCode);
                string partnerUrl = _repositoryPartner.GetByCode(customer.PartnerCode)?.PosApiUrl;
                string url = (partnerUrl != null ? partnerUrl : _domainPos) + _pathOrderGiftcard + "IDIMS=" + storeService.StoreCode + "&Qty=" + licenseProduct.GiftCardQuantity;
                _enrichLog.Info($"[POS][Request] {customer.StoreCode} {url}");
                using (var httpClient = _httpClientFactory.CreateClient())
                {
                    using (var response = await httpClient.GetAsync(url).ConfigureAwait(false))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            payloadString = payloadString.Remove(payloadString.Length - 1, 1).Remove(0, 1);
                            var result = JsonConvert.DeserializeObject<ApiPOSResponse>(payloadString);
                            _enrichLog.Info($"[POS][Response] {customer.StoreCode} {url}", new { Response = result });
                            if (result.IsSuccess() != true)
                            {
                                if (result == null) throw new Exception("POS connect failure, can not push data to API");
                                throw new Exception(result.Message);
                            }
                            var newGiftcard = new CustomerGiftcard
                            {
                                Id = Guid.NewGuid().ToString(),
                                OrderCode = storeService.OrderCode,
                                CustomerCode = storeService.CustomerCode,
                                QRCode = string.Join(",", result.Data.Select(c => c.QRCode).ToList()),
                                Printed = string.Join(",", result.Data.Select(c => c.Printed).ToList()),
                                CreateAt = DateTime.UtcNow,
                                CreateBy = _context.UserFullName,
                                StoreServiceId = storeService.Id
                            };
                            await _repositoryLicenseProduct.AddAsync(newGiftcard);
                            return true;
                        }
                        else
                        {
                            var payloadString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            var payload = JsonConvert.DeserializeObject<Response>(payloadString);
                            if (payload.Code == HttpStatusCode.Unauthorized) throw new UnauthorizedAccessException(payload.Message);
                            throw new HttpResponseException(payload.Code, payload.Message);
                        }
                    }
                }
            }
            else return false;
        }

        /// <summary>
        /// Sync base servive remaining when active/deactive license to POS
        /// </summary>
        /// <param name="storeProfile"></param>
        /// <returns></returns>
        private async Task SyncStoreBaseServiceRemainingAsync(string storeId, int operation = 1)
        {
            var storeService = await _repositoryStoreService.FindByIdAsync(storeId);
            if (storeService == null) return;

            var baseServices = await _repositoryStoreService.GetBaseServiceByStoreServiceIdAsync(storeService.Id);
            if (baseServices == null) return;

            foreach (var baseService in baseServices)
            {
                var remaining = baseService.Count_limit == -1 ? Constants.UnlimitedValue : baseService.Count_limit;
                try
                {
                    var baseServiceDB = await _storeBaseServiceRepository.GetBaseServiceByStoreCode(baseService.LicenseCode, storeService.StoreCode);
                    if (baseServiceDB == null)
                    {
                        baseServiceDB = new StoreBaseService
                        {
                            StoreCode = storeService.StoreCode,
                            KeyName = baseService.LicenseCode,
                            RemainingValue = remaining,
                            MaximumValue = remaining,
                            CreateAt = TimeHelper.GetUTCNow(),
                            CreateBy = _context.UserFullName
                        };
                        await _storeBaseServiceRepository.AddAsync(baseServiceDB);
                    }
                    else
                    {
                        baseServiceDB.MaximumValue += operation * remaining;
                        baseServiceDB.RemainingValue += operation * remaining;
                        baseServiceDB.MaximumValue = baseServiceDB.MaximumValue < 0 ? 0 : baseServiceDB.MaximumValue;
                        baseServiceDB.RemainingValue = baseServiceDB.RemainingValue < 0 ? 0 : baseServiceDB.RemainingValue;
                        baseServiceDB.UpdateAt = TimeHelper.GetUTCNow();
                        baseServiceDB.UpdateBy = _context.UserFullName;

                        await _storeBaseServiceRepository.UpdateAsync(baseServiceDB);
                        remaining = baseServiceDB.RemainingValue;
                    }
                }
                catch (Exception ex)
                {
                    _enrichLog.Error(ex, $"Sync base service remaining error {JsonConvert.SerializeObject(storeId)}");
                    continue;
                }

                try
                {
                    if (baseService.LicenseCode == BaseServiceEnum.Code.SMS.ToString())
                    {
                        var history = new SMSHistoryRemaining
                        {
                            StoreCode = storeService.StoreCode,
                            ObjectId = 0,
                            RemainingSMS = remaining,
                            Caller = Constants.ServiceName.IMSTeam,
                            Type = operation == 1 ? 200 : 300,
                            CreatedDate = DateTime.UtcNow
                        };
                        await _enrichSMS.InsertHistorySMSRemainingAsync(history);
                    }
                }
                catch (Exception ex)
                {
                    _enrichLog.Error(ex, $"Insert history remaining SMS error {JsonConvert.SerializeObject(storeId)}");
                }
            }
        }

    }
}
