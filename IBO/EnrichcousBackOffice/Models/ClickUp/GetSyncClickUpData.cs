using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnrichcousBackOffice.Models.ClickUp
{
    public class GetSyncClickUpData
    {
        public string CustomerName { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerPhone { get; set; }
        public string BusinessPhone { get; set; }
        public string ActivatedLicense { get; set; }
        public string MangoLoginEmail { get; set; }
        public string GoLiveDate { get; set; }
        public string SalonNote { get; set; }
        public string OwnerName { get; set; }
    }
}
