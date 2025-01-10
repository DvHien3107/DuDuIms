using Newtonsoft.Json;
using Pos.Application.Extensions.Helper;
using Pos.Application.Services.Scoped;
using Pos.Application.Services.Singleton;
using Pos.Model.Model.Comon;
using Pos.Model.Model.Request;
using Pos.Model.Model.Respons;
using Pos.Model.Model.Table.POS;
using Pos.Model.Model.Table.RCP;
using Promotion.Model.Model.Comon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Pos.Application.Event.POS
{
    public interface ILicenseEvent
    {
        Task<EventResponsive> StoreChange(StoreLicenses model);
    }
    public class LicenseEvent : ILicenseEvent
    {
        private readonly IRCPService _rcpService; 
        private readonly IPOSService _posService;
        private readonly ILicenseService _licenseService;

        public LicenseEvent(IRCPService rcpService, IPOSService posService, ILicenseService licenseService)
        {
            _rcpService = rcpService;
            _posService = posService;
            _licenseService = licenseService;
        }
        public async Task<EventResponsive> RunRecurringPlan()
        {
            EventResponsive result = new EventResponsive();
            result.status = 400;
            result.message = "EmptyContent";

            var getLstLicense = await _licenseService.getToDayRecurringPlan();
            foreach (var license in getLstLicense)
            {

            }


            return result;
        }

        public async Task<EventResponsive> StoreChange(StoreLicenses model)
        {
            EventResponsive result = new EventResponsive();
            result.status = 400;
            result.message = "EmptyContent";
            try
            {
                bool isCreate = false;
                string sql = "";
                int store_Id = 0;
                int RVCNo = 0;
                var POS = Const.POS_URL;
                var CheckIn = Const.CHECKIN_URL;
                if (!string.IsNullOrEmpty(model.storeId))
                {

                    Store storeNew = await _rcpService.getStoresByImsId(model.storeId);


                    if (storeNew == null)
                    {
                        isCreate = true;
                        storeNew = new Store();
                        storeNew.CreatedDate = DateTime.Now;
                    }

                    storeNew.StoreName = model.storeName;
                    storeNew.SubscriptionCode = model.SubscriptionCode;
                    storeNew.SubscriptionName = model.SubscriptionName;
                    storeNew.CompanyName = model.storeName;
                    storeNew.MerchantID = model.storeId;
                    storeNew.AddressLine = model.StoreAddress;
                    storeNew.City = model.City;
                    storeNew.State = model.State;
                    storeNew.ZipCode = model.ZipCode;
                    
                    storeNew.Phone = model.StorePhone;
                    storeNew.Email = model.StoreEmail;
                    storeNew.Password = model.Password;

                    storeNew.ContactPerson = model.ContactName;
                    storeNew.StatusContract = model.Status == "Activated" ? true : false;
                    storeNew.IsPlus = true;

                    if (isCreate)
                    {
                        storeNew.CurrentEmail = model.licenses.FirstOrDefault(x => x.licenseCode == "EMAIL")?.count_limit ?? 0;
                        storeNew.TotalEmail = model.licenses.FirstOrDefault(x => x.licenseCode == "EMAIL")?.count_limit ?? 0;
                        storeNew.CurrentSMS = model.licenses.FirstOrDefault(x => x.licenseCode == "SMS")?.count_limit ?? 0;
                        storeNew.TotalMessenger = model.licenses.FirstOrDefault(x => x.licenseCode == "SMS")?.count_limit ?? 0;
                        storeNew.MerchantID = model.storeId;
                    }
                    else
                    {
                        var LicenseEmail = model.licenses.FirstOrDefault(x => x.licenseCode == "EMAIL")?.count_limit ?? 0;
                        var LicenseSms = model.licenses.FirstOrDefault(x => x.licenseCode == "SMS")?.count_limit ?? 0;
                        storeNew.TotalEmail = storeNew.CurrentEmail + LicenseEmail;
                        storeNew.CurrentEmail = storeNew.CurrentEmail + LicenseEmail;
                        storeNew.TotalMessenger = storeNew.CurrentSMS + LicenseSms;
                        storeNew.CurrentSMS = storeNew.CurrentSMS + LicenseSms;

                    }
                    var ct = model.licenses.FirstOrDefault(x => x.licenseCode == "SALONCENTER");
                    if (ct != null)
                    {
                        storeNew.ExpiryDate = model.EndDate;
                    }
                    storeNew.LastChange = DateTime.Now;

                    if (isCreate)
                    {
                        store_Id = _rcpService.insertStore(storeNew);
                    }
                    else
                    {
                        store_Id = storeNew.StoreID;
                        _rcpService.updStore(storeNew);
                    }
                    result.status = 200;
                    result.message = "store change success";


                    var upd = await _posService.getDefRVCListsByImsId(model.storeId);
                    if (upd == null)
                    {
                        RDDefRVCList rDDefRVCList = new RDDefRVCList
                        {
                            RVCNo = store_Id,
                            RCPCode = store_Id,
                            RVCName = model.storeName,
                            Status = true,
                            IMSCode = model.storeId,
                            AutoNumApt = 1,
                            UTCDate = model.TimeZone,
                            UTCName = model.TimeZoneName,
                            MasterStore = 0
                        };
                        _posService.addDefRVCList(rDDefRVCList);
                        RVCNo = store_Id;

                        string CloneData = $@"exec P_Data_CloneDefault 0, {rDDefRVCList.RVCNo}";
                        await _posService.excuteQuery(CloneData);
                    }
                    else
                    {
                        upd.RVCName = model.storeName;
                        _posService.updDefRVCList(upd);
                        RVCNo = upd.RVCNo;
                    }
                    var ims = model.storeId;
                    //var licenses = _rCPContext.Licenses.Where(x => x.IMSID == ims).ToList();
                    try
                    {
                        await _rcpService.ExecuteAsync("Delete from License where IMSID =@ims", new
                        {
                            ims = ims
                        });
                        var licenseLSt = model.licenses.Where(x => x.licenseCode != "SALONCENTER" && x.licenseCode != "SMS" && x.licenseCode != "EMAIL").ToList();
                        foreach (var item in licenseLSt)
                        {
                            //MonitorInService 
                            string Update = $@" Delete RDFeaturesByRVC where IDKey ='{item.licenseCode}' and RVCNo={RVCNo}
                                   INSERT into RDFeaturesByRVC(IDKey ,RVCNo ,RequestNum) VALUES ('{item.licenseCode}',{RVCNo},{item.count_limit}) ";
                            await _posService.ExecuteAsync(Update);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.status = 500;
                        result.message = $@"Update License error {ex.Message}";
                        result.data = ex;
                    }
                }
                else
                {
                    result.status = 404;
                    result.message = "storeID not null";
                }
                if (result.status == 200)
                {
                    string Update = "";
                    try
                    {
                        var saloncenter = model.licenses.FirstOrDefault(x => x.licenseCode == "SALONCENTER");
                        Update = $@" Delete RDFeaturesByRVC where IDKey ='MangoStation' and RVCNo={RVCNo}
                                   INSERT into RDFeaturesByRVC(IDKey ,RVCNo ,RequestNum)VALUES ('MangoStation',{RVCNo},{saloncenter.count_limit})
                                   update RDPara set ParaStr = '{saloncenter.count_limit}', IsHide = 1 where ParaName in ('StationNum') and RVCNo = {RVCNo} ";
                        await _posService.ExecuteAsync(Update);

                        await _rcpService.ExecuteAsync($@"exec RDControlAPI {RVCNo}, '{model.storeId}', {store_Id}");
                    }
                    catch
                    {
                        result.status = 500;
                        result.message = $@"Update cript {Update} Fail";
                    }
                }
            }
            catch(Exception e)
            {
                result.status = 500;
                result.message = e.Message;
                result.data = e;
            }
            
            return result;
        }
    }
}
