using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace API.IMS.Models
{
    public class ResponseStore
    {
        public string status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
    }
    
    public class ResponseTransaction
    {
        public string status { get; set; }
        public string message { get; set; }
        public List<StoreTransaction> data { get; set; }
    }
    /// <summary>
    /// Data response api
    /// </summary>
    public class StoreProfile
    {
        public string storeId { get; set; }
        public string storeName { get; set; }
        public string contactName { get; set; }
        public string lastUpdate { get; set; }
        public string updateBy { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string cellPhone { get; set; }
        public string createBy { get; set; }
        public string createAt { get; set; }
        public string status { get; set; }
        public string businessName { get; set; }
        public string businessPhone { get; set; }
        public string businessEmail { get; set; }
        public string businessAddress { get; set; }
        public string licenseCode { get; set; }
        public string licenseName { get; set; }
        public List<BaseService> baseservices { get; set; }
        public List<Feature> features { get; set; }
    }

    #region store transaction
    public class StoreTransaction
    {
        public string OrderCode { get; set; }
        public decimal Total { get; set; }

        public string Status { get; set; }
        public string Date { get; set; }
        public string TransToken { get; set; }
        public string Url { get; set; }

        public List<ProductBaseDto> Subscription { get; set; }
        public List<ProductBaseDto> Hardware { get; set; }
        public CreditOnShow PaymentInfo { get; set; }
    }

    public class ProductBaseDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal? Amount { get; set; }

        //xxx

    }





    #endregion


    #region credit card infomation

    public class CreditOnShow
    {
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
        public string StreetAddress { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Zipcode { get; set; }
        public string Country { get; set; }
    }

    public class PaymentCredit : CreditOnShow
    {
        public string ID { get; set; }
        public string CardExpiry { get; set; }
        public bool Default { get; set; }
        public bool Active { get; set; }

    }

    #endregion

}