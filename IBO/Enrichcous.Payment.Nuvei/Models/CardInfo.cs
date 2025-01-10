using Enrichcous.Payment.Nuvei.Config.Enums;
using Inner.Libs.Helpful;
using System;

namespace Enrichcous.Payment.Nuvei.Models
{
    public class CardInfo
    {
        private string _currency = null;
        public string CardReference { get; set; }
        public string Currency
        {
            get => _currency ?? ECurrency.USD.Text();
            set => _currency = value ?? ECurrency.USD.Text();
        } 
        public string TerminalId { get; set; }
        public string CardType { get; set; }
        public string MerchantCardReference { get; set; }
        public string MerchantReference { get; set; }
        public string CardHolderName { get; set; }
        public string CardFirstName { get; set; }
        public string CardLastName { get; set; }
        
        public string CardNumber { get; set; }
        public string CardExpiry { get; set; }
        public string CardCSC { get; set; }
        
        public string CardAddressStreet { get; set; }
        public string CardCity { get; set; }
        public string CardState { get; set; }
        public string CardCountry { get; set; } = "United States";
        public string CardZipCode { get; set; }
        public string CardEmail { get; set; }
        public string CardPhone { get; set; }
    }
}