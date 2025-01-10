using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Request
{
    public class TransRequest
    {
        //MxMerchant_Id
        //PaymentProfile_Id
        //CardNumber
        //CardExpiry
        //CardCSC
        //OrdersCode
        public string Id { get; set; }
        public string PaymentProfile_Id { get; set; }
        public string MxMerchant_Id { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string CardCSC { get; set; }
        public string CardAddressStreet { get; set; }
        public string CardCity { get; set; }
        public string CardState { get; set; }
        public string CardCountry { get; set; } = "United States";
        public string CardZipCode { get; set; }
        public string CardPhone { get; set; }
        public string CardEmail { get; set; }

        public string CardId { get; set; }
        public string CustomerCode { get; set; }
        public string StoreCode { get; set; }
        public string Key { get; set; }
        public string OrdersCode{ get; set; }

        public string PaymentMethod { get; set; }
        public string PaymentNote { get; set; }

        //public string MerchantCardRef()
        //{
        //    var md5Card = CardNumber?.Trim().MD5Hash();
        //    var md5Store = StoreCode?.Trim().MD5Hash();
        //    var md5Mix = $"{md5Card}-{md5Store}";
        //    return md5Mix.MD5Hash();
        //}
        public string CardHolderName { get; set; }
    }

    public class ReceiptPayment_MailData
    {
        public string salon_name;
        public List<receipt_data> receipt;
        public string short_description { get; set; }
        // public string recurring_text { get; set; }
        public string grand_date_paid { get; internal set; }
        public object grand_total { get; internal set; }
    }
    public class receipt_data
    {
        public string salon;
        public string card;
        public string name;
        public string order_code;
        public string date_paid;
        public string total;
    }
}
