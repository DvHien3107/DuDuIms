using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EnrichcousBackOffice.Models.CustomizeModel
{
    public class PaymentInfo
    {
        public string CardNumber { get; set; }
        public string TrackData { get; set; }
        public string Ksn { get; set; }
        public string EncryptedTrack { get; set; }
        public int? FormatId { get; set; }
        public string ApplePayload { get; set; }
        public string AndroidPayload { get; set; }
        public string CardType { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
        public string CardExpiry { get; set; }
        public string CardHolderName { get; set; }
        public string Cvv { get; set; }
        public string IssueNo { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Description { get; set; }
        public string CardCurrency { get; set; }
        public string Mpiref { get; set; }
        public string DeviceId { get; set; }
        public string AutoReady { get; set; }
        public string BillToFirstName { get; set; }
        public string BillTolastName { get; set; }
        public string Xid { get; set; }
        public string Cavv { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string IpAddress { get; set; }
        public string Signature { get; set; }
        public string RecurringTxnRef { get; set; }
        public string OrderId { get; set; }
        public double Amount { get; set; }
    }
}