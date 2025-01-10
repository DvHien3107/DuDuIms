using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Antlr.Runtime;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Utils;
using EnrichcousBackOffice.Utils.AppConfig;
using EnrichcousBackOffice.Utils.IEnums;
using EnrichcousBackOffice.ViewModel;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using NPOI.HSSF.Record;
using WebGrease.Css.Extensions;

namespace EnrichcousBackOffice.Services
{
    public class FeatureService : IServicesBase
    {
        public FeatureService() : base()
        {
        }
        public FeatureService(bool trans = false) : base(trans)
        {
        }
        public FeatureService(WebDataModel db) : base(db)
        {
        }
        public void GetDefineFeature()
        {
            try
            {
                string url = AppConfig.Cfg.GetDefineFeatureUrl();
                HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "get", null);
                if (result.IsSuccessStatusCode)
                {
                    string responseJson = result.Content.ReadAsStringAsync().Result;
                    responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                    ResponeApiMangoPosFeature responeData = JsonConvert.DeserializeObject<ResponeApiMangoPosFeature>(responseJson);
                    if (responeData.Result.Equals("200") && responeData.Message.Equals("Success"))
                    {
                        var rand = new Random();
                        responeData.DataResult.ForEach(r =>
                        {
                            var oldFeature = DB.License_Item.Where(l => l.Code.Equals(r.IdKey)).FirstOrDefault();
                            if (oldFeature == null)
                            {
                                DB.License_Item.Add(new License_Item
                                {
                                    ID = long.Parse(DateTime.Now.ToString("yyMMddhhmmssff") + rand.Next(1, 9999).ToString().PadLeft(4, '0')),
                                    Name = r.FeatureNames,
                                    GroupID = 1000000,
                                    GroupName = DB.License_Item_Group.Find(1000000).Name,
                                    Code = r.IdKey,
                                    Enable = true,
                                    Type = r.RequestNum == true ? "COUNT" : "FEATURE",
                                    Description = r.Category + " - " + r.Description,
                                    BuiltIn = true
                                });
                            }
                            else
                            {
                                //oldFeature.Name = r.FeatureNames;
                                oldFeature.GroupID = 1000000;
                                oldFeature.GroupName = DB.License_Item_Group.Find(1000000).Name;
                                oldFeature.Code = r.IdKey;
                                oldFeature.Enable = true;
                                oldFeature.Type = r.RequestNum == true ? "COUNT" : "FEATURE";
                                //oldFeature.Description = r.Category + " - " + r.Description;
                                oldFeature.BuiltIn = true;
                                DB.Entry(oldFeature).State = EntityState.Modified;
                            }
                            DB.SaveChanges();
                        });

                        var oldFeatures = DB.License_Item.Where(c => c.GroupID == 1000000).ToList();
                        oldFeatures.ForEach(o =>
                        {
                            var fea = responeData.DataResult.Where(d => d.IdKey.Equals(o.Code)).FirstOrDefault();
                            if (fea == null)
                            {
                                o.Enable = false;
                                DB.License_Product_Item.Where(c => c.License_Item_Code.Trim().Equals(o.Code.Trim())).ToList().ForEach(f => f.Enable = false);
                                DB.SaveChanges();
                            }
                        });
                    }
                }
                else
                {
                    throw new Exception("Mango POS system not responding!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<License_Product_Item> GetDefineFeatureByStore(string StoreCode)
        {
            try
            {
                var cus = DB.C_Customer.Where(x => x.StoreCode == StoreCode).FirstOrDefault();
                string partnerLink = new MerchantService().GetPartner(cus.CustomerCode).PosApiUrl;
                string url = AppConfig.Cfg.GetDefineFeatureByStoreUrl(partnerLink);
                string SalonNameLog = "";
                if (cus != null)
                {
                    SalonNameLog = cus.BusinessName + " (#" + cus.StoreCode + ")";
                }
                HttpResponseMessage result = ClientRestAPI.CallRestApi(url + StoreCode, "", "", "get", null, SalonName: SalonNameLog);
                if (result?.IsSuccessStatusCode == true)
                {
                    string responseJson = result.Content.ReadAsStringAsync().Result;
                    responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                    ResponeApiMangoPosFeature responeData = JsonConvert.DeserializeObject<ResponeApiMangoPosFeature>(responseJson);
                    if (responeData.DataResult != null)
                        return responeData.DataResult.Select(c => new License_Product_Item
                        {
                            License_Item_Code = c.IdKey,
                            Value = c.NumRequest
                        }).ToList();
                    return new List<License_Product_Item> { };
                }
                else
                {
                    throw new Exception("Mango POS system not responding!");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void AddDefineFeature(List<License_Product_Item> features, string storeCode)
        {
            try
            {
                if (features.Count > 0)
                {
                    // get list define feature Store actived
                    //List<License_Product_Item> oldFeatures = GetDefineFeatureByStore(storeCode);
                    //List<string> oldFeatureKey = new List<string>();
                    //if (oldFeatures.Count > 0)
                    //{
                    //    var id = features.Select(f => f.License_Product_Id).ToList().Max();
                    //    oldFeatureKey = oldFeatures.Select(c => c.License_Item_Code).ToList();

                    //    features.ForEach(f =>
                    //    {
                    //        if (oldFeatureKey.Contains(f.License_Item_Code))
                    //            f.Value += oldFeatures[oldFeatureKey.IndexOf(f.License_Item_Code)].Value;
                    //    });
                    //}
                    var xfeatures = features.Join(DB.License_Item, f => f.License_Item_Code, l => l.Code, (f, l) => new
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
                        idims = storeCode
                    }).ToList();
                    //remove feature exist have Type != COUNT
                    //xfeatures.RemoveAll(c => oldFeatureKey.Contains(c.idKey) == true && c.requestNum == 0);

                    var cus = DB.C_Customer.Where(x => x.StoreCode == storeCode).FirstOrDefault();
                    string partnerLink = new MerchantService().GetPartner(cus.CustomerCode).PosApiUrl;
                    string url = AppConfig.Cfg.AddDefineFeatureUrl(partnerLink);
                    string SalonNameLog = "";
                    if (cus != null)
                    {
                        SalonNameLog = cus.BusinessName + " (#" + cus.StoreCode + ")";
                    }
                    HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "post", xfeatures, SalonName: SalonNameLog);
                    if (result.IsSuccessStatusCode)
                    {
                        string responseJson = result.Content.ReadAsStringAsync().Result;
                        responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                        ResponeApiMangoPosFeature responeData = JsonConvert.DeserializeObject<ResponeApiMangoPosFeature>(responseJson);
                        if (int.Parse(responeData.Result) != 200)
                            throw new Exception(responeData.Message);
                    }
                    else
                    {
                        throw new Exception("Mango POS system not responding!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void RemoveDefineFeature(List<License_Product_Item> features, string storeCode)
        {
            try
            {
                if (features.Count > 0)
                {
                    var xfeatures = features.Select(f => new
                    {
                        idKey = f.License_Item_Code,
                        idims = storeCode
                    }).ToList();
                    var c = DB.C_Customer.Where(x => x.StoreCode == storeCode).FirstOrDefault();
                    string partnerLink = new MerchantService().GetPartner(c.CustomerCode).PosApiUrl;
                    string url = AppConfig.Cfg.RemoveDefineFeatureUrl(partnerLink);
                    string SalonNameLog = "";
                    if (c != null)
                    {
                        SalonNameLog = c.BusinessName + " (#" + c.StoreCode + ")";
                    }
                    HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "post", xfeatures, SalonName: SalonNameLog);
                    if (result.IsSuccessStatusCode)
                    {
                        string responseJson = result.Content.ReadAsStringAsync().Result;
                        responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                        ResponeApiMangoPosFeature responeData = JsonConvert.DeserializeObject<ResponeApiMangoPosFeature>(responseJson);
                        if (int.Parse(responeData.Result) != 200)
                            throw new Exception(responeData.Message);
                    }
                    else
                    {
                        throw new Exception("Mango POS system not responding!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void EditDefineFeature(List<License_Product_Item> features, string storeCode)
        {
            try
            {
                if (features.Count > 0)
                {
                    var xfeatures = features.Select(c => new
                    {
                        idKey = c.License_Item_Code,
                        requestNum = c.Value,
                        idims = storeCode
                    }).ToList();
                    var cus = DB.C_Customer.Where(x => x.StoreCode == storeCode).FirstOrDefault();
                    string partnerLink = new MerchantService().GetPartner(cus.CustomerCode).PosApiUrl;
                    string url = AppConfig.Cfg.AddDefineFeatureUrl(partnerLink);
                    string SalonNameLog = "";
                    if (cus != null)
                    {
                        SalonNameLog = cus.BusinessName + " (#" + cus.StoreCode + ")";
                    }
                    HttpResponseMessage result = ClientRestAPI.CallRestApi(url, "", "", "post", xfeatures, SalonName: SalonNameLog);
                    if (result.IsSuccessStatusCode)
                    {
                        string responseJson = result.Content.ReadAsStringAsync().Result;
                        responseJson = responseJson.Remove(responseJson.Length - 1, 1).Remove(0, 1);
                        ResponeApiMangoPosFeature responeData = JsonConvert.DeserializeObject<ResponeApiMangoPosFeature>(responseJson);
                        if (int.Parse(responeData.Result) != 200)
                            throw new Exception(responeData.Message);
                    }
                    else
                    {
                        throw new Exception("Mango POS system not responding!");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public List<Licenses> GetListDefineFeatureByStore(string storeCode, string licenseId = null, bool isActive = false)
        {
            if (isActive && !string.IsNullOrEmpty(licenseId))
            {
                var defineFeatures = (
                    from ss in DB.Store_Services
                    join lp in DB.License_Product on ss.ProductCode equals lp.Code
                    join lpi in DB.License_Product_Item on lp.Id equals lpi.License_Product_Id
                    join li in DB.License_Item on lpi.License_Item_Code equals li.Code
                    where ss.Id == licenseId && li.GroupID == 1000000 && lpi.Enable == true
                    select new Licenses
                    {
                        LicenseCode = lpi.License_Item_Code,
                        LicenseType = li.Type,
                        Count_limit = lpi.Value.ToString(),
                        Subscription_warning_msg = li.Description,
                        Status = "new"
                    }).ToList();

                var storeFeatures = GetDefineFeatureByStore(storeCode);
                var features = (
                    from f in storeFeatures
                    join l in DB.License_Item on f.License_Item_Code.Trim() equals l.Code.Trim()
                    select new Licenses
                    {
                        LicenseCode = l.Code,
                        LicenseType = l.Type,
                        Count_limit = f.Value.ToString(),
                        Subscription_warning_msg = l.Description
                    }).ToList();

                var resultFeatures = features.Concat(defineFeatures).ToList();
                var temp = features.Concat(defineFeatures).ToList();
                temp.ForEach(f =>
                {
                    var duplicate = resultFeatures.Where(c => c.LicenseCode.Trim().Contains(f.LicenseCode.Trim())).ToList();
                    if (duplicate.Count > 1)
                    {
                        f.Count_limit += ("," + duplicate[1].Count_limit);
                        resultFeatures.Remove(duplicate[1]);
                    }
                    else if (f.LicenseType.ToLower().Contains("count") && f.Status != null)
                    {
                        f.Count_limit = "0," + f.Count_limit;
                    }
                    else
                        f.Count_limit += ",0";
                });
                return resultFeatures;
            }
            else
            {
                List<License_Product_Item> features = GetDefineFeatureByStore(storeCode);
                if (features != null)
                {
                    List<Licenses> fomatFeatures = features.Join(DB.License_Item, f => f.License_Item_Code.TrimEnd(), l => l.Code.TrimEnd(), (f, l) => new
                    {
                        f.License_Item_Code,
                        l.Type,
                        l.Name,
                        f.Value,
                        l.Description
                    }).Where(f => string.IsNullOrEmpty(f.License_Item_Code) == false)
                    .Select(s => new Licenses
                    {
                        LicenseCode = s.Name,
                        LicenseType = s.Type,
                        Count_limit = s.Value.ToString() + ",0",
                        Subscription_warning_msg = s.Description
                    }).ToList();
                    if (fomatFeatures.Count > 0)
                        return fomatFeatures;
                }
            }

            return new List<Licenses>() { };
        }
        public void ActiveDefineFeatureByStore(string storeCode)
        {
            //Get all feature of Store in Mango POS
            List<License_Product_Item> oldFeaturesInPos = GetDefineFeatureByStore(storeCode);
            //Get all feature in IMS to POS
            var allFeatures = (
                        from ss in DB.Store_Services
                        join lp in DB.License_Product on ss.ProductCode equals lp.Code
                        join lpi in DB.License_Product_Item on lp.Id equals lpi.License_Product_Id
                        join li in DB.License_Item on lpi.License_Item_Code equals li.Code
                        where ss.StoreCode == storeCode && ss.Active == 1 && li.GroupID == 1000000 && lpi.Enable == true
                        select new MangoPosFeature
                        {
                            IdKey = lpi.License_Item_Code,
                            NumRequest = (lpi.Value ?? 1) * ((ss.Quantity > 1 && ss.Type == "addon") ? (ss.Quantity ?? 1) : 1),
                            RequestNum = li.Type == "COUNT"
                        }).ToList();
            allFeatures = allFeatures.GroupBy(g => g.IdKey).Select(s => new MangoPosFeature
            {
                IdKey = s.First().IdKey,
                RequestNum = s.First().RequestNum,
                NumRequest = s.First().RequestNum == true ? s.Sum(it => it.NumRequest) : s.First().NumRequest
            }).ToList();

            //Get feature for delete
            var featureDel = oldFeaturesInPos.Where(o => !allFeatures.Any(f => f.IdKey.Equals(o.License_Item_Code)) && o.Value <= 1).ToList();
            var featureAdd = allFeatures.Where(f => !oldFeaturesInPos.Any(o => o.License_Item_Code.Equals(f.IdKey)) || (f.NumRequest > 1 && f.RequestNum == true)).ToList();

            RemoveDefineFeature(featureDel, storeCode);
            AddDefineFeature(featureAdd
                    .Select(c => new License_Product_Item
                    {
                        License_Item_Code = c.IdKey,
                        Value = c.NumRequest
                    }).ToList(), storeCode);
        }

        public class MangoPosFeature
        {
            public string IdKey { get; set; }
            public string FeatureNames { get; set; }
            public string Category { get; set; }
            public string Description { get; set; }
            public bool RequestNum { get; set; }
            public int NumRequest { get; set; }
        }
        public class GiftcardData
        {
            public string ControlNumber { get; set; }
            public string QRCode { get; set; }
            public string Printed { get; set; }
        }
        public class ResponeApiMangoPosFeature
        {
            public string Result { get; set; }
            public string Message { get; set; }
            public string Total { get; set; }
            public List<MangoPosFeature> DataResult { get; set; }
            public List<GiftcardData> Data { get; set; }
        }
    }
}