using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.IMS
{
    public class TwilioAccount
    {
        public int Id { get; set; }

        public string StoreCode { get; set; }

        public string Status { get; set; }

        public string Name { get; set; }

        public string SId { get; set; }

        public string OwnerAccountSid { get; set; }

        public string AuthToken { get; set; }

        public string CustomerProfileSId { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedDate { get; set; }

        public string UpdatedBy { get; set; }

    }

}
