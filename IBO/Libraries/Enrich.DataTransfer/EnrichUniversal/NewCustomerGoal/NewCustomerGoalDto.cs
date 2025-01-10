using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.EnrichUniversal.NewCustomerGoal
{
    public class NewCustomerGoalDto
    {
        public string Id { get;set; }
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? Goal { get; set; }
        public int? Note { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
