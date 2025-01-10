using Enrich.Payment.MxMerchant.Config.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Enrich.Payment.MxMerchant.Models
{
    public class CardInfo
    {
        public string number { get; set; }
        public string expiryMonth { get; set; }
        public string expiryYear { get; set; }
        public string avsZip { get; set; }
        public string avsStreet { get; set; }
        public string cvv { get; set; }
        public string name { get; set; }
        public long? id { get; set; }
        public string token { get; set; }
    }
    public class ResponseCardInfo
    {
        public bool isDefault { get; set; }
        public long id { get; set; }
        public DateTime created { get; set; }
        public string alias { get; set; }
        public string cardType { get; set; }
        public string last4 { get; set; }
        public string cardId { get; set; }
        public string token { get; set; }
        public string expiryMonth { get; set; }
        public string expiryYear { get; set; }
        public string name { get; set; }
        public string avsStreet { get; set; }
        public string avsZip { get; set; }
        public bool hasContract { get; set; }
    }
    public class MxMerchantCustomer
    {
        public long? merchantId { get; set; }
        public long? Id { get; set; }
        public string name { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string address1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }



    public class MxMerchantPayment
    {
        public string merchantId { get; set; }
        public string tenderType { get; set; } = "Card";
        public string entryClass { get; set; } = "WEB";
        public double amount { get; set; }
        public CardAccount cardAccount { get; set; }
        public Customer customer { get; set; }
        public BankAccount bankaccount { get; set; }

        public class CardAccount
        {
            public string token { get; set; }
        }
        public class Customer
        {
            public string id { get; set; }
        }
        public class BankAccount
        {
            public string type { get; set; }
            public string routingNumber { get; set; }
            public string accountNumber { get; set; }
            public string alias { get; set; }
            public string name { get; set; }
        }
    }
    public class MxMerchantPaymentResponse
    {
        public string id { get; set; }
        public string paymentToken { get; set; }
        public string status { get; set; }
        public string authMessage { get; set; }
        public string message()
        {
            if (this.authMessage.Contains(Constants.ErrorCode))
            {
                var messageObject = JsonConvert.DeserializeObject<MessageResponse>(this.authMessage);
                if(messageObject != null && string.IsNullOrEmpty(messageObject?.message))
                {
                    return messageObject.message;
                }
            }
            return this.authMessage;
        }
    }
    public class MxMerchantRecurring
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Account
        {
            public string token { get; set; }
        }

        public class Customer
        {
            public long id { get; set; }
        }

        public class Purchase
        {
            public string productName { get; set; }
            public double price { get; set; }
            public int quantity { get; set; }
            public double discountAmount { get; set; }
            public double taxAmount { get; set; }
            public double subTotalAmount { get; set; }
            public string description { get; set; }
        }

        public class Contract
        {
            public long? id { get; set; }
            public long? subid { get; set; }
            public Account account { get; set; }
            public Customer customer { get; set; }
            public List<Purchase> purchases { get; set; }
            public int frequency { get; set; }
            public string merchantId { get; set; }
            public double totalAmount { get; set; }
            public double discountAmount { get; set; }
            public double taxAmount { get; set; }
            public double subTotalAmount { get; set; }
            public int quantity { get; set; }
            public string dayNumber { get; set; } //If monthly interval, this sets the day of the month.
            public string weekDay { get; set; } //If weekly interval, this sets the day of the week. Values: Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday
            public string month { get; set; } //If yearly interval, this sets the month of the year (Also include dayNumber if you would like to bill on a different day than the first of the month).
            public string interval { get; set; }
            public string dayType { get; set; }
        }

        public class Subscription
        {
            public long customerId { get; set; }
            public long cardAccountId { get; set; }
            public bool allowPartialPayment { get; set; }
            public string status { get; set; }
            public string sendReceipt { get; set; }
            public bool eftAgreementRequested { get; set; }
            public string startDate { get; set; }
            public int occurrences { get; set; }
        }
        public Contract contract { get; set; }
        public Subscription subscription { get; set; }
    }
    public class MxMerchantRecurringReponse
    {
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
        public class Purchase
        {
            public long id { get; set; }
            public string productName { get; set; }
            public int quantity { get; set; }
            public int quantityReturned { get; set; }
            public string price { get; set; }
            public string discountAmount { get; set; }
            public string subTotalAmount { get; set; }
            public string taxAmount { get; set; }
            public int trackingNumber { get; set; }
            public List<object> taxes { get; set; }
            public List<object> discounts { get; set; }
        }

        public class Contract
        {
            public long id { get; set; }
            public int merchantId { get; set; }
            public string interval { get; set; }
            public int frequency { get; set; }
            public string weekDay { get; set; }
            public string month { get; set; }
            public int dayNumber { get; set; }
            public string dayType { get; set; }
            public List<Purchase> purchases { get; set; }
            public List<object> discounts { get; set; }
            public List<object> taxes { get; set; }
            public string totalAmount { get; set; }
            public string taxAmount { get; set; }
            public string subTotalAmount { get; set; }
            public string discountAmount { get; set; }
            public int subscriptionCount { get; set; }
        }

        public class Subscription
        {
            public long id { get; set; }
            public long contractId { get; set; }
            public DateTime startDate { get; set; }
            public int occurrences { get; set; }
            public string invoiceMethod { get; set; }
            public long cardAccountId { get; set; }
            public bool allowPartialPayment { get; set; }
            public string status { get; set; }
            public DateTime created { get; set; }
            public DateTime modified { get; set; }
            public long customerId { get; set; }
            public string sendReceipt { get; set; }
            public bool eftAgreementRequested { get; set; }
        }
        public Contract contract { get; set; }
        public Subscription subscription { get; set; }
    }
    public class MxMerchantRecurringPayment
    {
        public long id { get; set; }
        public string creatorName { get; set; }
        public CardAccount cardAccount { get; set; }
        //public long contractId { get; set; }
        //public Nullable<DateTime> startDate { get; set; }
        //public string invoiceMethod { get; set; }
        //public int cardAccountId { get; set; }
        //public bool allowPartialPayment { get; set; }
        public string status { get; set; }
        public string authMessage { get; set; }
        //public int remindBeforeDueDate { get; set; }
        //public int remindAfterDueDate { get; set; }
        public Nullable<DateTime> created { get; set; }
        //public Nullable<DateTime> modified { get; set; }
        //public int customerId { get; set; }
        //public int billingAddressId { get; set; }
        //public int shippingAddressId { get; set; }
        //public bool allowCreditCard { get; set; }
        //public bool allowACH { get; set; }
        //public string sendReceipt { get; set; }
        //public bool eftAgreementRequested { get; set; }
        //public string cardId { get; set; }

    }
    public class CardAccount
    {
        public string cardId { get; set; }
    }
    public class MessageResponse
    {
        public string errorCode { get; set; }
        public string message { get; set; }
        public string responseCode { get; set; }
    }
}