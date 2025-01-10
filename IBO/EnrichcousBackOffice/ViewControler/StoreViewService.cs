using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Enrich.Core.Ultils;
using EnrichcousBackOffice.AppLB;
using EnrichcousBackOffice.Models;
using EnrichcousBackOffice.Models.CustomizeModel;
using EnrichcousBackOffice.Services;
using EnrichcousBackOffice.Utils.IEnums;
using Inner.Libs.Helpful;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static EnrichcousBackOffice.Models.CustomizeModel.StoreCustomizeModel;

namespace EnrichcousBackOffice.ViewControler
{
    public class StoreViewService
    {
        private WebDataModel db;

        public StoreViewService()
        {
            if (db == null)
            {
                this.db = new WebDataModel();
            }

        }

        #region services - current plan

        /// <summary>
        /// check current plan cua merchant
        /// if order -> completed: update store_service current plan
        /// </summary>
        /// <returns></returns>
        //public async Task<string> UpdateStoreService(List<Order_Subcription> serviceItem, string memberSubmit = "IMS System")
        //{

        //    try
        //    {
        //        string storecode = serviceItem.FirstOrDefault()?.StoreCode;
        //        if (string.IsNullOrEmpty(storecode))
        //        {
        //            //throw new Exception("Subscription is not found");
        //            return "OK";
        //        }
        //        var current_plan = db.Store_Services.Where(s => s.StoreCode == storecode).ToList();
        //        if (current_plan.Count() == 0)
        //        {
        //            //add new
        //            foreach (var item in serviceItem)
        //            {
        //                db.Store_Services.Add(new Store_Services
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    StoreCode = item.StoreCode,
        //                    StoreName = item.CustomerName,
        //                    CustomerCode = item.CustomerCode,
        //                    AutoRenew = item.AutoRenew,
        //                    EffectiveDate = item.StartDate,
        //                    RenewDate = item.EndDate,
        //                    Period = item.Period,
        //                    ProductCode = item.Product_Code,
        //                    Productname = item.ProductName,
        //                    Product_Code_POSSystem = item.Product_Code_POSSystem,
        //                    OrderCode = item.OrderCode,
        //                    Type = item.IsAddon == true ? "addon" : "license",
        //                    Active = -1,
        //                    LastUpdateAt = DateTime.Now,
        //                    LastUpdateBy = memberSubmit
        //                });
        //            }



        //        }
        //        else
        //        {
        //            foreach (var item in serviceItem)
        //            {
        //                db.Store_Services.Add(new Store_Services
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    StoreCode = item.StoreCode,
        //                    StoreName = item.CustomerName,
        //                    CustomerCode = item.CustomerCode,
        //                    AutoRenew = item.AutoRenew,
        //                    EffectiveDate = item.StartDate,
        //                    RenewDate = item.EndDate,
        //                    Period = item.Period,
        //                    ProductCode = item.Product_Code,
        //                    Productname = item.ProductName,
        //                    Product_Code_POSSystem = item.Product_Code_POSSystem,
        //                    OrderCode = item.OrderCode,
        //                    Type = item.IsAddon == true ? "addon" : "license",
        //                    Active = -1,//waiting
        //                    LastUpdateAt = DateTime.Now,
        //                    LastUpdateBy = memberSubmit
        //                });
        //            }
        //        }
        //        string customer_code = serviceItem.FirstOrDefault().CustomerCode;
        //        var cus = db.C_Customer.Where(c => c.CustomerCode == customer_code).FirstOrDefault();
        //        cus.StoreCode = storecode;
        //        cus.Active = 1/*true*/;//[1: Active], [0: Inactive], [-1: Not processing]
        //        cus.StoreStatus = false;
        //        db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
        //        db.SaveChanges();

        //        if (!db.Store_Services.Any(s => s.StoreCode == storecode && s.Type == "license"))
        //        {
        //            #region push store to Simply Pos
        //            var data = PushStoreToMangoPos(serviceItem.FirstOrDefault().OrderCode, db, true);
        //            string baseurl = ConfigurationManager.AppSettings["mango_update_store"];
        //            var result = ClientRestAPI.CallRestApi(baseurl, "", "", "post", data);
        //            if (result.IsSuccessStatusCode)
        //            {
        //                var dataObjects = result.Content.ReadAsAsync<Dictionary<string, object>>().Result;
        //                if (dataObjects["Status"].ToString() != "200")
        //                {
        //                    string jsondata = JsonConvert.SerializeObject(data);
        //                    await SendEmail.SendBySendGrid("activepos@mangoforsalon.com", "team", "ERR:IMS CAN NOT CONNECT TO MANGO API [Mango#" + storecode + "]", "url:<br/>" + baseurl + "<br/>Return from MangoAPI:<br/> " + dataObjects["Status"].ToString() + "-" + dataObjects["Message"] + "<br/> Data: <br/>" + jsondata, "", "", false);
        //                    //throw new Exception("Mango POS connect failure, can not push data to MangoPOS API");
        //                    //send email err.
        //                }
        //            }
        //            else
        //            {
        //                string jsondata = JsonConvert.SerializeObject(data);
        //                await SendEmail.SendBySendGrid("activepos@mangoforsalon.com", "team", "ERR:IMS CAN NOT CONNECT TO MANGO API [Mango#" + storecode + "]", "url: <br/>" + baseurl + "<br/>Return:<br/>" + result.StatusCode.ToString() + "<br/>Data:<br/>" + jsondata, "", "", false);
        //                //throw new Exception("Mango POS connect failure, can not push data to MangoPOS API");

        //            }
        //            #endregion
        //        }
        //        return "OK";

        //    }
        //    catch (Exception e)
        //    {
        //        return string.IsNullOrWhiteSpace(e.InnerException?.Message) == true ? e.Message : e.InnerException.Message;
        //    }

        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order_code"></param>
        /// <param name="storecode"></param>
        /// <param name="memberSubmit"></param>
        /// <returns></returns>
        /// 

        public async Task<string> CloseStoreService(string order_code, string storecode, string memberSubmit = "IMS System", bool newLicense = true, Order_Subcription order_sub = null)
        {
            try
            {
                if (string.IsNullOrEmpty(storecode))
                {
                    //throw new Exception("Subscription is not found");
                    return "OK";
                }

                var serviceItem = db.Order_Subcription.Where(o => o.OrderCode == order_code && !string.IsNullOrEmpty(order_code) && o.SubscriptionType != "setupfee" && o.SubscriptionType != "interactionfee").ToList();
                if (order_sub != null && string.IsNullOrEmpty(order_code))
                {
                    //storeinhouse la 1 dang nhu nay - hong co order_code(khong co invoice)
                    order_sub.OrderCode = string.Empty;
                    serviceItem.Add(order_sub);
                }
                if (serviceItem.Count == 0)
                {
                    return "OK";
                }

                //add new
                foreach (var item in serviceItem)
                {
                    var storeService = db.Store_Services.Where(s => s.OrderCode == order_code && s.ProductCode == item.Product_Code).FirstOrDefault();
                    var checkExistLicenseItems = db.License_Product_Item.Count(x => x.License_Product_Id == item.ProductId) > 0;
                    if (item.Period != "MONTHLY")
                    {
                        for (int i = 0; i < item.Quantity; i++)
                        {
                            if (storeService == null)
                            {
                                db.Store_Services.Add(new Store_Services
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    StoreCode = item.StoreCode,
                                    StoreName = item.CustomerName,
                                    CustomerCode = item.CustomerCode,
                                    AutoRenew = item.AutoRenew ?? false,
                                    StoreApply = item.PriceType,
                                    EffectiveDate = item.StartDate,
                                    RenewDate = item.EndDate,
                                    Period = item.Period,
                                    ProductCode = item.Product_Code,
                                    Productname = item.ProductName,
                                    Product_Code_POSSystem = item.Product_Code_POSSystem,
                                    OrderCode = item.OrderCode,
                                    Type = item.SubscriptionType,
                                    Active = -1,
                                    LastUpdateAt = DateTime.UtcNow,
                                    LastUpdateBy = memberSubmit,
                                    ApplyDiscountAsRecurring = item.ApplyDiscountAsRecurring,
                                    PeriodRecurring = item.PeriodRecurring,
                                    Quantity = item.SubscriptionQuantity,
                                });
                            }
                        }
                    }
                    else
                    {
                        if (storeService == null)
                        {
                            db.Store_Services.Add(new Store_Services
                            {
                                Id = Guid.NewGuid().ToString(),
                                StoreCode = item.StoreCode,
                                StoreName = item.CustomerName,
                                CustomerCode = item.CustomerCode,
                                AutoRenew = item.AutoRenew ?? false,
                                StoreApply = item.PriceType ?? Store_Apply_Status.Real.Text(),
                                EffectiveDate = item.StartDate,
                                RenewDate = item.EndDate,
                                Period = item.Period,
                                ProductCode = item.Product_Code,
                                Productname = item.ProductName,
                                Product_Code_POSSystem = item.Product_Code_POSSystem,
                                OrderCode = item.OrderCode,
                                Type = item.SubscriptionType,
                                Active = -1,
                                LastUpdateAt = DateTime.UtcNow,
                                LastUpdateBy = memberSubmit,
                                ApplyDiscountAsRecurring = item.ApplyDiscountAsRecurring,
                                PeriodRecurring = item.PeriodRecurring,
                                Quantity = item.SubscriptionQuantity,
                            });
                        }
                        //update
                        else
                        {
                            storeService.AutoRenew = item.AutoRenew ?? false;
                            storeService.StoreApply = item.PriceType;
                            storeService.RenewDate = item.EndDate;
                            storeService.ApplyDiscountAsRecurring = item.ApplyDiscountAsRecurring ?? false;
                        }
                    }
                }

                var products_code = serviceItem.Select(s => s.Product_Code);
                db.Store_Services.Where(s => s.OrderCode == order_code && !products_code.Contains(s.ProductCode) && s.Active == -1).ForEach(s =>
                {
                    db.Store_Services.Remove(s);
                });

                string customer_code = serviceItem.FirstOrDefault().CustomerCode;
                var cus = db.C_Customer.Where(c => c.CustomerCode == customer_code).FirstOrDefault();
                cus.StoreCode = storecode;
                cus.Active = 1;
                cus.StoreStatus = false;
                db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return "OK";

            }
            catch (Exception e)
            {
                return string.IsNullOrWhiteSpace(e.InnerException?.Message) == true ? e.Message : e.InnerException.Message;
            }

        }

        //public async Task<string> ActiveService(string store_service_id, string memberSubmit = "IMS System")
        //{
        //    try
        //    {
        //        var service = db.Store_Services.Find(store_service_id);
        //        var customer = db.C_Customer.Where(c => c.CustomerCode == service.CustomerCode).FirstOrDefault();
        //        service.Active = 1;
        //        db.Entry(service).State = System.Data.Entity.EntityState.Modified;


        //        if(service.Type== "license")
        //        {
        //            var current_sups = db.Store_Services.Where(s => s.CustomerCode == service.CustomerCode && s.Type == "license" && s.Active == 1).FirstOrDefault();
        //            if (current_sups != null)
        //            {
        //                current_sups.Active = 0;
        //                db.Entry(current_sups).State = System.Data.Entity.EntityState.Modified;
        //            }
        //        }
        //        db.SaveChanges();
        //        var data = PushStoreToMangoPos(customer, db);
        //        string baseurl = ConfigurationManager.AppSettings["mango_update_store"];
        //        var result = ClientRestAPI.CallRestApi(baseurl, "", "", "post", data);
        //        if (result.IsSuccessStatusCode)
        //        {
        //            var dataObjects = result.Content.ReadAsAsync<Dictionary<string, object>>().Result;
        //            if (dataObjects["Status"].ToString() != "200")
        //            {
        //                string jsondata = JsonConvert.SerializeObject(data);
        //                await SendEmail.SendBySendGrid("activepos@mangoforsalon.com", "team", "ERR:IMS CAN NOT CONNECT TO MANGO API [Mango#" + customer.StoreCode + "]", "url:<br/>" + baseurl + "<br/>Return from MangoAPI:<br/> " + dataObjects["Status"].ToString() + "-" + dataObjects["Message"] + "<br/> Data: <br/>" + jsondata, "", "", false);
        //                //throw new Exception("Mango POS connect failure, can not push data to MangoPOS API");
        //                //send email err.
        //            }
        //        }
        //        else
        //        {
        //            string jsondata = JsonConvert.SerializeObject(data);
        //            await SendEmail.SendBySendGrid("activepos@mangoforsalon.com", "team", "ERR:IMS CAN NOT CONNECT TO MANGO API [Mango#" + customer.StoreCode + "]", "url: <br/>" + baseurl + "<br/>Return:<br/>" + result.StatusCode.ToString() + "<br/>Data:<br/>" + jsondata, "", "", false);
        //            //throw new Exception("Mango POS connect failure, can not push data to MangoPOS API");

        //        }
        //        return "OK";
        //    }
        //    catch (Exception e)
        //    {
        //        return string.IsNullOrWhiteSpace(e.InnerException?.Message) == true ? e.Message : e.InnerException.Message;
        //    }

        //}

        /// <summary>
        /// check service da order or renew nhung chua thanh toan
        /// if order -> submited: show storeservice voi status waitting
        /// </summary>
        /// <returns></returns>
        public List<StoreCustomizeModel.StoreServiceWaitingModel> GetServiceWaiting(string storeCode, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                string open_status = InvoiceStatus.Open.ToString();
                var service_waiting = (from s in db.Order_Subcription
                                       join o in db.O_Orders on s.OrderCode equals o.OrdersCode
                                       where o.Status == open_status
                                       select new StoreCustomizeModel.StoreServiceWaitingModel
                                       {
                                           Id = s.Id.ToString(),
                                           AutoRenew = s.AutoRenew,
                                           CustomerCode = s.CustomerCode,
                                           EffectiveDate = null,
                                           OrderCode = o.OrdersCode,
                                           Period = s.Period,
                                           ProductCode = s.Product_Code,
                                           Productname = s.ProductName,
                                           Product_Code_POSSystem = s.Product_Code_POSSystem,
                                           RenewDate = null,
                                           Status = "Waiting",
                                           StoreCode = s.StoreCode,
                                           StoreName = s.CustomerName,
                                           Type = ""

                                       }).OrderByDescending(s => s.Id).ToList();
                return service_waiting;


            }
            catch (Exception e)
            {
                errMsg = string.IsNullOrWhiteSpace(e.InnerException?.Message) == true ? e.Message : e.InnerException.Message;
                return new List<StoreCustomizeModel.StoreServiceWaitingModel>();
            }

        }

        /// <summary>
        /// get current plan
        /// </summary>
        /// <returns></returns>
        public List<Store_Services> GetStoreService(string storecode, out string errMsg)
        {
            errMsg = string.Empty;
            try
            {
                var service_current_plan = db.Store_Services.Where(s => s.StoreCode == storecode).OrderByDescending(s => s.Id).ToList();
                return service_current_plan;

            }
            catch (Exception e)
            {
                errMsg = string.IsNullOrWhiteSpace(e.InnerException?.Message) == true ? e.Message : e.InnerException.Message;
                return new List<Store_Services>();
            }
        }



        #endregion


        #region DAY DU LIEU STORE CHO MANGO POS


        //public Store_DataModel PushStoreToMangoPos(C_Customer cus, WebDataModel db)
        //{
        //    if (db == null)
        //    {
        //        db = new WebDataModel();
        //    }
        //    try
        //    {
        //        if (string.IsNullOrEmpty(cus.Password))
        //        {
        //            var newPass = SecurityLibrary.Md5Encrypt(DateTime.Now.ToString("O")).Substring(0, 6);
        //            cus.MD5PassWord = SecurityLibrary.Md5Encrypt(newPass);
        //            cus.Password = newPass;
        //            db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
        //            db.SaveChanges();
        //        }
        //        var store = db.Store_Services.Where(s => s.StoreCode == cus.StoreCode).OrderByDescending(s => s.LastUpdateAt).FirstOrDefault();
        //        var storeData = new Store_DataModel
        //        {
        //            storeId = cus.StoreCode,
        //            storeName = cus.BusinessName,
        //            ContactName = cus.ContactName ?? cus.OwnerName,
        //            Email = string.IsNullOrEmpty(cus.SalonEmail) ? cus.Email : cus.SalonEmail,
        //            CellPhone = cus.CellPhone ?? cus.OwnerMobile,
        //            BusinessName = cus.BusinessName,
        //            BusinessPhone = cus.BusinessPhone,
        //            BusinessEmail = string.IsNullOrWhiteSpace(cus.SalonEmail) ? cus.BusinessEmail : cus.SalonEmail,
        //            BusinessAddress = StoreServices.AddressLine(cus),
        //            Password = cus.MD5PassWord,
        //            CreateBy = cus.CreateBy,
        //            CreateAt = cus.CreateAt?.ToString("MM/dd/yyyy"),
        //            Status = cus.StoreStatus == true ? "Activated" : "DeActivated",
        //            updateBy = store.LastUpdateBy,
        //            lastUpdate = store.LastUpdateAt?.ToString("dd/MM/yyyy hh:mm tt"),
        //        };


        //        var list_order_subscription = db.Store_Services.Where(s => s.OrderCode == cus.StoreCode/* && s.Active == true*/).OrderBy(s => s.LastUpdateAt).ToList();
        //        var list_active_products = new List<ActiveProducts>();
        //        var list_licenses = new List<StoreCustomizeModel.Licenses>();

        //        foreach (var order_subscription in list_order_subscription)
        //        {
        //            if (order_subscription.Type.ToLower() == "license" && order_subscription.Active == 1)
        //            {
        //                var active_products = new ActiveProducts
        //                {
        //                    code = string.IsNullOrWhiteSpace(order_subscription.Product_Code_POSSystem) == true ? "MANGOONE" : order_subscription.Product_Code_POSSystem,
        //                    name = order_subscription.Productname,
        //                };
        //                list_active_products.Add(active_products);
        //            }

        //            //storeData.lastUpdate = order_subscription.LastUpdate?.ToString("MM/dd/yyyy HH:mm:ss");
        //            var license_product = db.License_Product.Where(l => l.Code == order_subscription.ProductCode).FirstOrDefault();

        //            var list_order_subscription_license = db.License_Product_Item.Where(s => s.License_Product_Id == license_product.Id).ToList();
        //            foreach (var license_product_item in list_order_subscription_license)
        //            {
        //                //if (license_product_item != null && license_product_item.License_Item_Code?.ToLower() != "setup" && license_product_item.License_Item_Code?.ToLower() != "monthly")
        //                //{
        //                var licenses_exist = list_licenses.Where(x => x.licenseCode == license_product_item.License_Item_Code).FirstOrDefault();
        //                //if (licenses_exist != null)
        //                //{
        //                //    licenses_exist.count_warning_value += (license_product_item.CountWarning ?? 0);
        //                //    licenses_exist.count_limit += (license_product_item.CountWarning ?? 0);
        //                //}
        //                if (licenses_exist != null &&
        //                    (!string.IsNullOrEmpty(licenses_exist?.start_period) && order_subscription.EffectiveDate > DateTime.Parse(licenses_exist?.start_period)))
        //                {
        //                    list_licenses.Remove(licenses_exist);
        //                    var licenses = new StoreCustomizeModel.Licenses
        //                    {
        //                        licenseCode = license_product_item.License_Item_Code,
        //                        licenseType = db.License_Item.Where(i => i.Code == license_product_item.License_Item_Code).FirstOrDefault()?.Type,
        //                        subscription_warning_date = (order_subscription.RenewDate.HasValue && license_product.SubscriptionEndWarningDays != null) ?
        //                        order_subscription.RenewDate.Value.AddDays(-(double)license_product.SubscriptionEndWarningDays).ToString("MM/dd/yyyy") : "",
        //                        subscription_warning_msg = "",
        //                        count_warning_value = (license_product_item.CountWarning ?? 0).ToString(),
        //                        count_limit = (license_product_item.Value ?? 0) + licenses_exist.count_limit,
        //                        start_period = order_subscription.EffectiveDate?.ToString("MM/dd/yyyy"),
        //                        end_period = order_subscription.RenewDate?.ToString("MM/dd/yyyy"),
        //                        status = order_subscription.Active != 1 ? "deactive" : (order_subscription.RenewDate.HasValue == true ? (order_subscription.RenewDate > DateTime.Today ? "active" : "expires") : "active")
        //                    };

        //                    list_licenses.Add(licenses);
        //                }
        //                else if (licenses_exist == null)
        //                {
        //                    var licenses = new StoreCustomizeModel.Licenses
        //                    {
        //                        licenseCode = license_product_item.License_Item_Code,
        //                        licenseType = db.License_Item.Where(i => i.Code == license_product_item.License_Item_Code).FirstOrDefault()?.Type,
        //                        subscription_warning_date = (order_subscription.RenewDate.HasValue && license_product.SubscriptionEndWarningDays != null) ?
        //                        order_subscription.RenewDate.Value.AddDays(-(double)license_product.SubscriptionEndWarningDays).ToString("MM/dd/yyyy") : "",
        //                        subscription_warning_msg = "",
        //                        count_warning_value = (license_product_item.CountWarning ?? 0).ToString(),
        //                        count_limit = (license_product_item.Value ?? 0).ToString(),
        //                        start_period = order_subscription.EffectiveDate?.ToString("MM/dd/yyyy"),
        //                        end_period = order_subscription.RenewDate?.ToString("MM/dd/yyyy"),
        //                        status = order_subscription.Active != 1 ? "deactive" :
        //                        (order_subscription.RenewDate.HasValue == true ? (order_subscription.RenewDate > DateTime.Today ? "active" : "expires") : "active")
        //                    };

        //                    list_licenses.Add(licenses);
        //                }
        //                //}
        //            }
        //        }


        //        storeData.activeProducts = list_active_products;
        //        storeData.licenses = list_licenses;
        //        return storeData;
        //    }
        //    catch (Exception)
        //    {
        //        return new Store_DataModel();

        //    }



        //}

        /// <summary>
        /// PushStore with current order only, include waiting, deactive license
        /// </summary>
        /// <param name="orderCode"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public Store_DataModel PushStoreToMangoPos(string orderCode, WebDataModel db, bool newInvoice = false)
        {
            if (db == null)
            {
                db = new WebDataModel();
            }
            try
            {
                var order = db.O_Orders.Where(o => o.OrdersCode == orderCode).FirstOrDefault();
                var cus = db.C_Customer.Where(c => c.CustomerCode == order.CustomerCode).FirstOrDefault();
                var list_order_subscription = db.Store_Services.Where(s => s.OrderCode == order.OrdersCode/* && s.Active == true*/).OrderBy(s => s.LastUpdateAt).ToList();
                var list_active_products = new List<ActiveProducts>();
                var list_licenses = new List<StoreCustomizeModel.Licenses>();
                if (!list_order_subscription.Any(s => s.Type == LicenseType.LICENSE.Text()))
                {
                    throw new Exception("Not found subscription in order");
                }

                if (string.IsNullOrEmpty(cus.Password) || string.IsNullOrEmpty(cus.MD5PassWord))
                {
                    var newPass = SecurityLibrary.Md5Encrypt(DateTime.Now.ToString("O")).Substring(0, 6);
                    cus.MD5PassWord = SecurityLibrary.Md5Encrypt(newPass);
                    cus.Password = newPass;
                    db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                var store = db.Store_Services.Where(s => s.StoreCode == cus.StoreCode).FirstOrDefault();

                string salonaddress = cus.SalonAddress1 + ", " + cus.SalonCity + ", " + cus.SalonState + ", " + cus.SalonZipcode;
                if (string.IsNullOrWhiteSpace(salonaddress.Replace(",", "").Replace(" ", "")))
                {
                    salonaddress = cus.BusinessAddressStreet + ", " + cus.BusinessCity + ", " + cus.BusinessState + ", " + cus.BusinessZipCode + " " + cus.BusinessCountry;
                }
                else
                {
                    salonaddress += " United States";
                }

                var storeData = new Store_DataModel
                {
                    storeId = cus.StoreCode,
                    storeName = cus.BusinessName,
                    NewLicense = newInvoice ? "1" : "0",
                    ContactName = cus.ContactName ?? cus.OwnerName,
                    Email = string.IsNullOrEmpty(cus.SalonEmail) ? cus.Email : cus.SalonEmail,
                    CellPhone = cus.CellPhone ?? cus.OwnerMobile,
                    BusinessName = cus.BusinessName,
                    BusinessPhone = cus.BusinessPhone,
                    BusinessEmail = string.IsNullOrWhiteSpace(cus.SalonEmail) ? cus.BusinessEmail : cus.SalonEmail,
                    BusinessAddress = salonaddress,
                    Password = cus.MD5PassWord,
                    CreateBy = cus.CreateBy,
                    CreateAt = cus.CreateAt?.ToString("MM/dd/yyyy"),
                    Status = cus.StoreStatus == true ? "Activated" : "DeActivated",
                    updateBy = store?.LastUpdateBy,
                    lastUpdate = store?.LastUpdateAt.HasValue == true ? store?.LastUpdateAt.Value.ToString("MM/dd/yyyy") : "",
                };


                //define storelicense

                foreach (var order_subscription in list_order_subscription)
                {
                    if (order_subscription.Type.ToLower() == "license" && (order_subscription.Active == 1 || newInvoice))
                    {
                        var active_products = new ActiveProducts
                        {
                            code = string.IsNullOrWhiteSpace(order_subscription.Product_Code_POSSystem) == true ? "MANGOONE" : order_subscription.Product_Code_POSSystem,
                            name = order_subscription.Productname,
                        };
                        list_active_products.Add(active_products);
                    }

                    var license_product = db.License_Product.Where(l => l.Code == order_subscription.ProductCode).FirstOrDefault();

                    var list_order_subscription_license = db.License_Product_Item.Where(s => s.License_Product_Id == license_product.Id && s.Enable == true).ToList();
                    foreach (var license_product_item in list_order_subscription_license)
                    {
                        var current_license = list_licenses.FirstOrDefault(l => l.licenseCode == license_product_item.License_Item_Code);
                        if (current_license != null)
                        {
                            current_license.count_limit = (int.Parse(current_license.count_limit ?? "0") + (license_product_item.Value ?? 0)).ToString();
                        }
                        else
                        {
                            var licenses = new StoreCustomizeModel.Licenses
                            {
                                licenseCode = license_product_item.License_Item_Code,
                                licenseType = db.License_Item.Where(i => i.Code == license_product_item.License_Item_Code).FirstOrDefault()?.Type,
                                subscription_warning_date = (order_subscription.RenewDate.HasValue && license_product.SubscriptionEndWarningDays != null) ?
                            order_subscription.RenewDate.Value.AddDays(-(double)license_product.SubscriptionEndWarningDays).ToString("MM/dd/yyyy") : "",
                                subscription_warning_msg = "",
                                count_warning_value = (license_product_item.CountWarning ?? 0).ToString(),
                                count_limit = (license_product_item.Value ?? 0).ToString(),
                                start_period = order_subscription.EffectiveDate?.ToString("MM/dd/yyyy"),
                                end_period = order_subscription.RenewDate?.ToString("MM/dd/yyyy"),
                                status = order_subscription.Active != -1 ? "waiting" : (order_subscription.Active == 0 ? "deactive" :
                            (order_subscription.RenewDate.HasValue == true ? (order_subscription.RenewDate > DateTime.Today ? "active" : "expires") : "active"))
                            };
                            list_licenses.Add(licenses);
                        }
                    }
                }

                storeData.activeProducts = list_active_products;
                storeData.licenses = list_licenses;

                return storeData;
            }
            catch (Exception)
            {
                return new Store_DataModel();

            }



        }

        #endregion

        public Store_DataModel UpdateCusInfoToMangoPos(C_Customer cus, WebDataModel db)
        {
            if (db == null)
            {
                db = new WebDataModel();
            }
            try
            {
                if (string.IsNullOrEmpty(cus.Password) || string.IsNullOrEmpty(cus.MD5PassWord))
                {
                    var newPass = SecurityLibrary.Md5Encrypt(DateTime.UtcNow.ToString("O")).Substring(0, 6);
                    cus.MD5PassWord = SecurityLibrary.Md5Encrypt(newPass);
                    cus.Password = newPass;
                    db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                }
                var store = db.Store_Services.Where(s => s.StoreCode == cus.StoreCode).OrderByDescending(s => s.LastUpdateAt).FirstOrDefault();
                var storeData = new Store_DataModel
                {
                    storeId = cus.StoreCode,
                    storeName = cus.BusinessName,
                    ContactName = cus.ContactName ?? cus.OwnerName,
                    Email = !string.IsNullOrEmpty(cus.SalonEmail) ? cus.SalonEmail : (!string.IsNullOrEmpty(cus.BusinessEmail) ? cus.BusinessEmail : cus.Email),
                    CellPhone = cus.CellPhone ?? cus.OwnerMobile,
                    BusinessName = cus.BusinessName,
                    BusinessPhone = cus.BusinessPhone,
                    BusinessEmail = string.IsNullOrWhiteSpace(cus.SalonEmail) ? cus.BusinessEmail : cus.SalonEmail,
                    BusinessAddress = cus.AddressLine(),
                    Password = cus.MD5PassWord,
                    CreateBy = cus.CreateBy,
                    CreateAt = cus.CreateAt?.ToString("MM/dd/yyyy"),
                    Status = (cus.Active == 1 && cus.StoreStatus == true) ? "Activated" : "DeActivated",
                    updateBy = store.LastUpdateBy,
                    lastUpdate = store.LastUpdateAt?.ToString("dd/MM/yyyy hh:mm tt"),
                };
                //Add active product
                storeData.activeProducts = new MerchantService().MainActiveSubscription(cus.StoreCode).Select(active => new ActiveProducts()
                {
                    code = active.ProductCode,
                    name = active.Productname,
                }).ToList();
                var active_products = db.Store_Services.Where(s => s.StoreCode == cus.StoreCode && s.Type == "license" && s.Active == 1).ToList();

                //update store status khi customer khong active 
                if (cus.Active != 1)
                {
                    cus.StoreStatus = false;
                    db.Entry(cus).State = System.Data.Entity.EntityState.Modified;
                    foreach (var prd in active_products)
                    {
                        prd.Active = 0;
                        prd.LastUpdateAt = DateTime.UtcNow;
                        db.Entry(prd).State = System.Data.Entity.EntityState.Modified;
                    }
                }
                db.SaveChanges();
                var licenseList = new StoreServices(db).LicensesCurrent(cus.StoreCode);
                storeData.licenses = JsonConvert.DeserializeObject<List<Licenses>>(JsonConvert.SerializeObject(licenseList));
                return storeData;
            }
            catch (Exception e)
            {
                return new Store_DataModel();
            }
        }
    }

}