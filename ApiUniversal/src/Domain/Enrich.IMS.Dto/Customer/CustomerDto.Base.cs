using System;

namespace Enrich.IMS.Dto.Customer
{
    public class CustomerBaseDto
    {
        public long Id { get; set; }

        public string CustomerCode { get; set; }

        public string StoreCode { get; set; }

        public string BusinessName { get; set; }

        public string PartnerCode { get; set; }

        public string PartnerName { get; set; }

        public int RemainingDays { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? EffectiveDate { get; set; }

    }
}