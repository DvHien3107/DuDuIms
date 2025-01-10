using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.NewCustomerGoal
{
    public class NewCustomerGoalUpdateResponse
    {
        public int CreatedId { get; set; }

        public NewCustomerGoalDto NewCustomerGoal { get; set; }
    }
}
