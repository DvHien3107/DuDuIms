using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class C_CustomerCard
    {
        public string Id { get; set; }

        public string CustomerCode { get; set; }

        public string StoreCode { get; set; }

        public string MerchantReference { get; set; }

        public string CardReference { get; set; }

        public string Currency { get; set; }

        public string CardType { get; set; }

        public string CardNumber { get; set; }

        public string CardHolderName { get; set; }

        public string CardExpiry { get; set; }

        public bool IsDefault { get; set; }

        public string MerchantCardReference { get; set; }

        public string TerminalId { get; set; }

        public bool Active { get; set; }

        public string CardAddressStreet { get; set; }

        public string CardCity { get; set; }

        public string CardCountry { get; set; }

        public string CardState { get; set; }

        public string CardZipCode { get; set; }

        public string CardPhone { get; set; }

        public string CardEmail { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public string CardFirstName { get; set; }

        public string CardLastName { get; set; }

        public long? MxMerchant_Id { get; set; }

        public string MxMerchant_CardId { get; set; }

        public string MxMerchant_Token { get; set; }

        public bool? IsRecurring { get; set; }

    }

}
