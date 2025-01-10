using Enrich.Dto.Base;
using Enrich.Dto.Requests;
using Enrich.IMS.Dto.SalesLead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enrich.IMS.Dto.NewCustomerGoal
{
    public class NewCustomerGoalUpdateRequest : BaseSaveByJsonPatchRequest<NewCustomerGoalDto>
    {
        /// <summary>
        /// use to verify a NewCustomerGoal (unverify to verify)
        /// </summary>
        public bool IsVerify { get; set; } = false;
        public NewCustomerGoalUpdateOption UpdateOption { get; set; }

        public NewCustomerGoalUpdateRequest(NewCustomerGoalDto dto, NewCustomerGoalDto oldDto = null) : this()
        {
            NewDto = dto;
            OldDto = oldDto;
        }

        public NewCustomerGoalUpdateRequest()
        {
        }
    }
}
