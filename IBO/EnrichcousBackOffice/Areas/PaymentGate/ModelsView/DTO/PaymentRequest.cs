using Inner.Libs.Helpful;
using System.Collections.Generic;

namespace EnrichcousBackOffice.Areas.PaymentGate.ModelsView.DTO
{
    public class TransRequest
    {
        public string Id { get; set; }
        public long? MxMerchant_Id { get; set; }
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
        public string OrdersCode => Key?.Split(':')[0].FromBase64() ?? "";

        public string PaymentMethod { get; set; }
        public string PaymentNote { get; set; }

        public string MerchantCardRef()
        {
            var md5Card = CardNumber?.Trim().MD5Hash();
            var md5Store = StoreCode?.Trim().MD5Hash();
            var md5Mix = $"{md5Card}-{md5Store}";
            return md5Mix.MD5Hash();
        }
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