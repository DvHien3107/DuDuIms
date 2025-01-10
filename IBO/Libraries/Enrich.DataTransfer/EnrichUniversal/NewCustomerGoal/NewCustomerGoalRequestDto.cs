using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.DataTransfer.EnrichUniversal.NewCustomerGoal
{
    public class NewCustomerGoalRequestDto : BaseSearchWithFilterRequest<NewCustomerGoalSearchConditionDto, NewCustomerGoalFilterConditionDto>
    {
        public NewCustomerGoalRequestDto()
        {
            Condition = new NewCustomerGoalSearchConditionDto();
        }
    }
}
