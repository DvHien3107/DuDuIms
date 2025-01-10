using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Model.Model.Table.POS
{
    public class RDDefRVCList
    {
        
        public int RVCNo { get; set; }

        public string RVCName { get; set; }

        public bool? Status { get; set; }

        public decimal? RCPCode { get; set; }

        public string IMSCode { get; set; }

        public decimal? AutoNumApt { get; set; }

        public string UTCDate { get; set; }

        public string UTCName { get; set; }
        [Key]
        public string VerifyCode { get; set; }

        public int? CreditType { get; set; }

        public int? MasterStore { get; set; }

        public DateTime? LastChange { get; set; }

        public string Currency { get; set; }

    }

}
